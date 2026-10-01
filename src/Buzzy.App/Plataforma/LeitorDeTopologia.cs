using System.Runtime.InteropServices;
using Buzzy.Core;

namespace Buzzy.App.Plataforma;

/// <summary>Um monitor como o Windows o enumera, antes de ganhar a chave: o nome GDI liga-o à configuração de vídeo.</summary>
internal readonly record struct MonitorEnumerado(string NomeGdi, RetanguloPx Tela, RetanguloPx AreaUtil, int Dpi, bool Principal);

/// <summary>
/// Uma leitura coerente da topologia, com as chaves estáveis (DEC-030) e o que só o adaptador conhece: o nome GDI de
/// cada chave, para o log, e de onde cada chave veio. Classe, e não registro, de propósito: o texto de um registro
/// imprimiria tudo, e o log só leva campos escolhidos.
/// </summary>
internal sealed class LeituraDaTopologia
{
    private readonly Dictionary<string, string> _nomeGdiPorChave;

    internal LeituraDaTopologia(Topologia topologia, IReadOnlyList<ChaveAtribuida> chaves, string? erroDaConsulta, int caminhosSemNome)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        ArgumentNullException.ThrowIfNull(chaves);
        Topologia = topologia;
        Chaves = chaves;
        ErroDaConsulta = erroDaConsulta;
        CaminhosSemNome = caminhosSemNome;
        _nomeGdiPorChave = chaves.ToDictionary(c => c.Chave, c => c.NomeGdi, StringComparer.Ordinal);
    }

    internal Topologia Topologia { get; }

    /// <summary>A chave de cada monitor, com o nome GDI e a origem, na ordem de <see cref="Topologia.Monitores"/>.</summary>
    internal IReadOnlyList<ChaveAtribuida> Chaves { get; }

    /// <summary>Nulo se a consulta da configuração de vídeo deu certo; senão, o motivo (função e código).</summary>
    internal string? ErroDaConsulta { get; }

    /// <summary>Alvos ativos cujo nome ou caminho não pôde ser lido numa consulta que deu certo.</summary>
    internal int CaminhosSemNome { get; }

    internal int ChavesDoCache => Chaves.Count(c => c.Origem == OrigemDaChave.Cache);

    internal int ChavesDeReserva => Chaves.Count(c => c.Origem == OrigemDaChave.Reserva);

    /// <summary>O nome GDI (<c>\\.\DISPLAYn</c>) do monitor da chave nesta leitura, ou <c>-</c> se a chave não é dela.</summary>
    internal string NomeGdi(string chave) => _nomeGdiPorChave.TryGetValue(chave, out string? nome) ? nome : "-";
}

/// <summary>
/// Lê a topologia real dos monitores e a converte para o modelo puro do núcleo, em pixels
/// físicos do desktop virtual (DEC-008). Só lê geometria, área útil e DPI dos monitores e, para
/// a chave estável de cada um, a configuração de vídeo (<see cref="ConfiguracaoDeVideo"/>, DEC-030).
/// </summary>
internal static class LeitorDeTopologia
{
    private static readonly object Trava = new();

    /// <summary>
    /// Nome GDI → chave lida do caminho e tela, da última leitura coerente com a consulta boa. Segura a chave de um
    /// monitor quando a consulta falha (sessão remota ou sem acesso ao console), enquanto o nome GDI e a tela forem os
    /// mesmos. Só vive nesta execução.
    /// </summary>
    private static IReadOnlyDictionary<string, ChaveConhecida> _cache = new Dictionary<string, ChaveConhecida>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Topologia atual, ou nulo se o Windows devolver algo incoerente (por exemplo, no meio de
    /// uma troca de modo de vídeo, sem monitor principal). Quem chama mantém a anterior.
    /// </summary>
    internal static Topologia? Ler(out string? erro) => LerDetalhado(out erro)?.Topologia;

    /// <summary>
    /// A leitura completa: a topologia com as chaves estáveis, o nome GDI de cada chave e o resultado da consulta da
    /// configuração de vídeo. Nula, com o motivo em <paramref name="erro"/>, quando a leitura é incoerente: a
    /// enumeração falhou, um monitor não pôde ser lido por inteiro (informação ou DPI) ou a topologia não fecha. Uma
    /// falha só da consulta da configuração de vídeo nunca torna a leitura nula: as chaves vêm do cache ou da reserva.
    /// </summary>
    internal static LeituraDaTopologia? LerDetalhado(out string? erro)
    {
        ConsultaDeVideo consulta = ConfiguracaoDeVideo.Consultar();
        List<MonitorEnumerado>? enumerados = Enumerar(out erro);
        if (enumerados is null) return null;
        lock (Trava)
        {
            LeituraDaTopologia? leitura = Montar(enumerados, consulta, _cache, out IReadOnlyDictionary<string, ChaveConhecida> cacheDepois, out erro);
            _cache = cacheDepois;
            return leitura;
        }
    }

    /// <summary>
    /// Converte o que o Windows informou de um monitor. Uma falha ao ler a informação ou o DPI, ou um monitor sem nome
    /// GDI, devolve nulo com o motivo, e a leitura inteira passa a ser incoerente: a topologia nunca sai sem um monitor
    /// nem com uma escala inventada (crítica da Fase 5, L3). Quem lê tenta de novo, e a anterior continua valendo.
    /// </summary>
    internal static MonitorEnumerado? Descrever(bool infoLida, int erroDaInfo, Win32.MONITORINFOEX info, int resultadoDoDpi, uint dpi, out string? falha)
    {
        if (!infoLida)
        {
            falha = $"GetMonitorInfo falhou (erro {erroDaInfo})";
            return null;
        }
        if (resultadoDoDpi != 0 || dpi == 0 || dpi > int.MaxValue)
        {
            falha = $"GetDpiForMonitor falhou (0x{resultadoDoDpi:X8}, dpi {dpi})";
            return null;
        }
        if (string.IsNullOrWhiteSpace(info.szDevice))
        {
            falha = "monitor sem nome GDI";
            return null;
        }
        falha = null;
        return new MonitorEnumerado(info.szDevice, Retangulo(info.rcMonitor), Retangulo(info.rcWork), (int)dpi, (info.dwFlags & Win32.MONITORINFOF_PRIMARY) != 0);
    }

    /// <summary>
    /// Monta a leitura com as chaves (<see cref="ChavesDeMonitor.Atribuir"/>), sem tocar no Windows. O cache só é
    /// trocado, em <paramref name="cacheDepois"/>, por uma leitura coerente com a consulta boa, e passa a ter só as
    /// chaves lidas do caminho; senão, volta o mesmo <paramref name="cache"/>. Nome GDI repetido torna a leitura
    /// incoerente, como antes, quando o nome era a chave.
    /// </summary>
    internal static LeituraDaTopologia? Montar(
        IReadOnlyList<MonitorEnumerado> enumerados,
        ConsultaDeVideo consulta,
        IReadOnlyDictionary<string, ChaveConhecida> cache,
        out IReadOnlyDictionary<string, ChaveConhecida> cacheDepois,
        out string? erro)
    {
        ArgumentNullException.ThrowIfNull(enumerados);
        ArgumentNullException.ThrowIfNull(consulta);
        ArgumentNullException.ThrowIfNull(cache);
        cacheDepois = cache;

        if (enumerados.Select(m => m.NomeGdi).Distinct(StringComparer.OrdinalIgnoreCase).Count() != enumerados.Count)
        {
            erro = "Nome GDI de monitor repetido.";
            return null;
        }

        (string NomeGdi, RetanguloPx Tela)[] nomesETelas = [.. enumerados.Select(m => (m.NomeGdi, m.Tela))];
        IReadOnlyDictionary<string, string>? mapa = consulta.Erro is null ? ChavesDeMonitor.Mapear(consulta.Alvos) : null;
        IReadOnlyList<ChaveAtribuida> chaves = ChavesDeMonitor.Atribuir(nomesETelas, mapa, cache);

        Topologia topologia;
        try
        {
            topologia = new Topologia(enumerados.Select((m, i) => new MonitorDoDesktop(chaves[i].Chave, m.Tela, m.AreaUtil, m.Dpi, m.Principal)));
        }
        catch (ArgumentException e)
        {
            erro = e.Message;
            return null;
        }

        if (consulta.Erro is null) cacheDepois = ChavesDeMonitor.NovoCache(nomesETelas, chaves);
        erro = null;
        return new LeituraDaTopologia(topologia, chaves, consulta.Erro, consulta.CaminhosSemNome);
    }

    internal static RetanguloPx Retangulo(Win32.RECT r) => new(r.Left, r.Top, r.Right, r.Bottom);

    /// <summary>Os monitores na ordem do Windows; nulo, com o motivo, se a enumeração ou um monitor falhou.</summary>
    private static List<MonitorEnumerado>? Enumerar(out string? erro)
    {
        var monitores = new List<MonitorEnumerado>();
        string? falha = null;

        bool Visitar(nint hMonitor, nint hdc, nint lprc, nint dado)
        {
            if (falha is not null) return true;
            var mi = new Win32.MONITORINFOEX { cbSize = Marshal.SizeOf<Win32.MONITORINFOEX>() };
            bool infoLida = Win32.GetMonitorInfo(hMonitor, ref mi);
            int erroDaInfo = infoLida ? 0 : Marshal.GetLastWin32Error();
            uint dpi = 0;
            int resultadoDoDpi = infoLida ? Win32.GetDpiForMonitor(hMonitor, Win32.MDT_EFFECTIVE_DPI, out dpi, out _) : 0;
            if (Descrever(infoLida, erroDaInfo, mi, resultadoDoDpi, dpi, out falha) is { } monitor) monitores.Add(monitor);
            return true;
        }

        Win32.MonitorEnumProc callback = Visitar;
        bool ok = Win32.EnumDisplayMonitors(0, 0, callback, 0);
        GC.KeepAlive(callback);

        if (!ok)
        {
            erro = "EnumDisplayMonitors falhou";
            return null;
        }
        erro = falha;
        return falha is null ? monitores : null;
    }
}

using System.Globalization;
using System.Text;

namespace Buzzy.Core.Personagem;

/// <summary>
/// Gravação e reprodução de sequências de eventos (TODO.md, Fase 2): um evento por linha, em
/// texto na cultura invariante, e a reprodução que devolve, para cada linha, as transições, os
/// efeitos e o retrato resultante. Os testes comparam essa saída com arquivos de referência.
///
/// Formato de entrada: linhas que começam com <c>&gt;</c> trazem um evento, por exemplo
/// <c>&gt; Press x=1632 y=1000</c>; linhas vazias e começadas por <c>#</c> são comentários.
/// Topologias são citadas pelo nome. <c>Tick vezes=N</c> aplica N passos de uma vez, e
/// <c>AutonomyTimer</c> sem geração usa a geração agendada no momento.
///
/// Formato de saída: a linha do evento, depois <c>~</c> para cada transição, <c>!</c> para cada
/// efeito, <c>x</c> para evento descartado e <c>=</c> com o retrato.
/// </summary>
public static class Gravacao
{
    private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;

    /// <summary>Linha de um evento, sem o prefixo <c>&gt;</c>.</summary>
    public static string Escrever(Evento evento, Func<Topologia, string> nomeDaTopologia)
    {
        ArgumentNullException.ThrowIfNull(evento);
        ArgumentNullException.ThrowIfNull(nomeDaTopologia);
        return evento switch
        {
            Press e => $"Press {Ponto(e.Cursor)}",
            DragMove e => $"DragMove {Ponto(e.Cursor)}",
            DragEnd e => $"DragEnd {Ponto(e.Cursor)}",
            ContextMenu e => $"ContextMenu {Ponto(e.Cursor)}",
            EnergySelected e => $"EnergySelected nivel={e.Nivel}",
            Loaded e => $"Loaded topologia={nomeDaTopologia(e.Topologia)} energia={e.Preferencias.Energia} telaCheia={SimNao(e.Preferencias.ModoTelaCheia)}"
                + (e.PosicaoSalva is { } p ? $" posicao={DescreverPosicao(p)}" : ""),
            TopologyChanged e => $"TopologyChanged topologia={nomeDaTopologia(e.Topologia)}",
            FullscreenTargetsChanged e => $"FullscreenTargetsChanged ocupados={e.Ocupados}",
            SettingsChanged e => $"SettingsChanged energia={e.Preferencias.Energia} telaCheia={SimNao(e.Preferencias.ModoTelaCheia)}",
            MovementSignal e => $"MovementSignal sinal={e.Sinal}",
            AutonomyTimer e => string.Create(Invariante, $"AutonomyTimer geracao={e.Geracao}"),
            ExpressionChange e => $"ExpressionChange expressao={e.Expressao}",
            _ => evento.GetType().Name,
        };
    }

    /// <summary>
    /// Eventos de uma linha de entrada (sem o prefixo). Devolve mais de um só para
    /// <c>Tick vezes=N</c>. <paramref name="atual"/> resolve <c>AutonomyTimer</c> sem geração.
    /// </summary>
    public static IReadOnlyList<Evento> Ler(string linha, Func<string, Topologia> topologiaPorNome, EstadoDoNucleo atual)
    {
        ArgumentNullException.ThrowIfNull(linha);
        ArgumentNullException.ThrowIfNull(topologiaPorNome);
        ArgumentNullException.ThrowIfNull(atual);

        string[] partes = linha.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) throw new FormatException("Linha de evento vazia.");
        string nome = partes[0];
        var campos = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string parte in partes.Skip(1))
        {
            int igual = parte.IndexOf('=', StringComparison.Ordinal);
            if (igual <= 0) throw new FormatException($"Campo sem '=' em \"{linha}\": {parte}");
            if (!campos.TryAdd(parte[..igual], parte[(igual + 1)..]))
                throw new FormatException($"Campo repetido em \"{linha}\": {parte[..igual]}");
        }

        string Campo(string chave) => campos.TryGetValue(chave, out string? v) ? v : throw new FormatException($"Falta o campo {chave} em \"{linha}\".");
        PontoPx Cursor() => new(Inteiro(Campo("x")), Inteiro(Campo("y")));

        Evento Unico() => nome switch
        {
            "Press" => new Press(Cursor()),
            "Click" => new Click(),
            "DoubleClick" => new DoubleClick(),
            "DragStart" => new DragStart(),
            "DragMove" => new DragMove(Cursor()),
            "DragEnd" => new DragEnd(Cursor()),
            "DragCancel" => new DragCancel(),
            "ContextMenu" => new ContextMenu(Cursor()),
            "EnergyPanelOpen" => new EnergyPanelOpen(),
            "EnergySelected" => new EnergySelected(Enum.Parse<NivelDeEnergia>(Campo("nivel"))),
            "EnergyPanelClose" => new EnergyPanelClose(),
            "CmdHide" => new CmdHide(),
            "CmdShow" => new CmdShow(),
            "CmdPauseAutonomy" => new CmdPauseAutonomy(),
            "CmdResumeAutonomy" => new CmdResumeAutonomy(),
            "CmdOpenSettings" => new CmdOpenSettings(),
            "CmdResetPosition" => new CmdResetPosition(),
            "CmdExit" => new CmdExit(),
            "Loaded" => new Loaded(
                topologiaPorNome(Campo("topologia")),
                campos.TryGetValue("posicao", out string? p) ? LerPosicao(p) : null,
                LerPreferencias(campos)),
            "TopologyChanged" => new TopologyChanged(topologiaPorNome(Campo("topologia"))),
            "SessionLocked" => new SessionLocked(),
            "SessionUnlocked" => new SessionUnlocked(),
            "Suspending" => new Suspending(),
            "Resumed" => new Resumed(),
            "SessionEnding" => new SessionEnding(),
            "FullscreenTargetsChanged" => new FullscreenTargetsChanged(new MonitoresOcupados(
                Campo("ocupados").Split(',', StringSplitOptions.RemoveEmptyEntries))),
            "SettingsChanged" => new SettingsChanged(LerPreferencias(campos)),
            "Tick" => new Tick(),
            "MovementSignal" => new MovementSignal(Enum.Parse<SinalDeMovimento>(Campo("sinal"))),
            "AutonomyTimer" => new AutonomyTimer(campos.TryGetValue("geracao", out string? g) ? long.Parse(g, NumberStyles.Integer, Invariante) : atual.Geracao),
            "ExpressionChange" => new ExpressionChange(Enum.Parse<Expressao>(Campo("expressao"))),
            _ => throw new FormatException($"Evento desconhecido: {nome}"),
        };

        if (nome == "Tick" && campos.TryGetValue("vezes", out string? vezes))
        {
            int n = Inteiro(vezes);
            if (n < 1) throw new FormatException($"Tick vezes={n}: precisa ser positivo.");
            return [.. Enumerable.Range(0, n).Select(_ => (Evento)new Tick())];
        }
        return [Unico()];
    }

    /// <summary>Linha canônica de um efeito, sem o prefixo <c>!</c>.</summary>
    public static string DescreverEfeito(Efeito efeito)
    {
        ArgumentNullException.ThrowIfNull(efeito);
        return efeito switch
        {
            MoverJanela e => $"MoverJanela monitor={e.Destino.Monitor.Chave} ancora={Par(e.Destino.Ancora)} retangulo={Retangulo(e.Destino.Retangulo)}",
            AgendarDecisao e => string.Create(Invariante, $"AgendarDecisao atrasoMs={(long)e.Atraso.TotalMilliseconds} geracao={e.Geracao}"),
            AbrirMenu e => $"AbrirMenu ponto={Par(e.Ponto)}",
            GravarPosicao e => $"GravarPosicao posicao={DescreverPosicao(e.Posicao)}",
            GravarPreferencias e => $"GravarPreferencias energia={e.Preferencias.Energia} telaCheia={SimNao(e.Preferencias.ModoTelaCheia)}",
            _ => efeito.GetType().Name,
        };
    }

    /// <summary>
    /// Reproduz as linhas de entrada num núcleo novo e devolve a saída canônica: cada linha de
    /// evento seguida das transições, efeitos, descartes e do retrato.
    /// </summary>
    public static IReadOnlyList<string> Reproduzir(
        ConfiguracaoDoNucleo configuracao, ulong semente, IEnumerable<string> linhas, Func<string, Topologia> topologiaPorNome)
    {
        ArgumentNullException.ThrowIfNull(configuracao);
        ArgumentNullException.ThrowIfNull(linhas);
        ArgumentNullException.ThrowIfNull(topologiaPorNome);

        var nucleo = new Nucleo(configuracao, semente);
        var saida = new List<string>();
        foreach (string bruta in linhas)
        {
            string linha = bruta.Trim();
            if (!linha.StartsWith('>')) continue;
            string evento = linha[1..].Trim();
            saida.Add("> " + evento);

            foreach (Evento e in Ler(evento, topologiaPorNome, nucleo.Estado))
            {
                if (!nucleo.Enfileirar(e)) saida.Add($"x {Escrever(e, _ => "?")} descartado (autônomo com o usuário no controle)");
            }
            IReadOnlyList<Efeito> efeitos = nucleo.Processar((_, r) =>
            {
                foreach (Transicao t in r.Transicoes) saida.Add("~ " + t);
            });
            foreach (Efeito ef in efeitos) saida.Add("! " + DescreverEfeito(ef));
            saida.Add("= " + nucleo.Retrato.Descrever());
        }
        return saida;
    }

    /// <summary>Posição relativa como <c>chave;fracaoX;fracaoY;ancoraX;ancoraY</c>.</summary>
    public static string DescreverPosicao(PosicaoDoPersonagem p)
    {
        ArgumentNullException.ThrowIfNull(p);
        return string.Create(Invariante, $"{p.ChaveMonitor};{p.FracaoX:0.######};{p.FracaoY:0.######};{p.AncoraAbsoluta.X};{p.AncoraAbsoluta.Y}");
    }

    public static PosicaoDoPersonagem LerPosicao(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        string[] p = texto.Split(';');
        if (p.Length != 5) throw new FormatException($"Posição inválida: {texto}");
        return new PosicaoDoPersonagem(
            p[0],
            double.Parse(p[1], NumberStyles.Float, Invariante),
            double.Parse(p[2], NumberStyles.Float, Invariante),
            new PontoPx(Inteiro(p[3]), Inteiro(p[4])));
    }

    private static Preferencias LerPreferencias(Dictionary<string, string> campos) => new(
        campos.TryGetValue("energia", out string? e) ? Enum.Parse<NivelDeEnergia>(e) : Preferencias.Padrao.Energia,
        campos.TryGetValue("telaCheia", out string? t) ? t switch
        {
            "sim" => true,
            "nao" => false,
            _ => throw new FormatException($"telaCheia={t}: use sim ou nao."),
        } : Preferencias.Padrao.ModoTelaCheia);

    private static int Inteiro(string texto) => int.Parse(texto, NumberStyles.AllowLeadingSign, Invariante);

    private static string Ponto(PontoPx p) => string.Create(Invariante, $"x={p.X} y={p.Y}");

    private static string Par(PontoPx p) => string.Create(Invariante, $"({p.X},{p.Y})");

    private static string Retangulo(RetanguloPx r) => string.Create(Invariante, $"({r.Esquerda},{r.Topo})-({r.Direita},{r.Base})");

    private static string SimNao(bool valor) => valor ? "sim" : "nao";

    /// <summary>Junta linhas com \n, para comparar e gravar arquivos de referência.</summary>
    public static string Juntar(IEnumerable<string> linhas)
    {
        ArgumentNullException.ThrowIfNull(linhas);
        var sb = new StringBuilder();
        foreach (string l in linhas) sb.Append(l).Append('\n');
        return sb.ToString();
    }
}

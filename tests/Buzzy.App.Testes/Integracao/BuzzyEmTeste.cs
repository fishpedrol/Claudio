using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Buzzy.Core;

namespace Buzzy.App.Testes.Integracao;

/// <summary>Uma linha <c>BUZZY|CHAVE|campo=valor</c> do log de diagnóstico.</summary>
internal sealed record EventoDoLog(string Chave, IReadOnlyDictionary<string, string> Campos, string Linha)
{
    internal string this[string campo] => Campos.TryGetValue(campo, out string? v) ? v : "";

    internal static EventoDoLog? Ler(string linha)
    {
        int i = linha.IndexOf("BUZZY|", StringComparison.Ordinal);
        if (i < 0) return null;
        string[] partes = linha[(i + 6)..].Split('|');
        var campos = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string parte in partes.Skip(1))
        {
            int igual = parte.IndexOf('=', StringComparison.Ordinal);
            if (igual > 0) campos[parte[..igual]] = parte[(igual + 1)..];
        }
        return new EventoDoLog(partes[0], campos, linha);
    }

    private static readonly System.Text.RegularExpressions.Regex ReRetangulo =
        new(@"\((-?\d+),(-?\d+)\)-\((-?\d+),(-?\d+)\)", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex RePonto =
        new(@"(-?\d+),(-?\d+)", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Lê o formato de <see cref="RetanguloPx.ToString"/>: <c>(e,t)-(d,b)</c>, com sinais.</summary>
    internal static RetanguloPx Retangulo(string texto)
    {
        var m = ReRetangulo.Match(texto);
        if (!m.Success) throw new FormatException($"Retângulo ilegível: {texto}");
        int N(int g) => int.Parse(m.Groups[g].Value, CultureInfo.InvariantCulture);
        return new RetanguloPx(N(1), N(2), N(3), N(4));
    }

    /// <summary>Lê <c>x,y</c> ou <c>(x,y)</c>, com sinais.</summary>
    internal static PontoPx Ponto(string texto)
    {
        var m = RePonto.Match(texto);
        if (!m.Success) throw new FormatException($"Ponto ilegível: {texto}");
        return new PontoPx(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));
    }
}

/// <summary>Resultado de abrir uma segunda instância do Buzzy.</summary>
/// <param name="SaiuSozinha">Se o processo saiu dentro do tempo, sem o teste encerrá-lo.</param>
/// <param name="Codigo">Código de saída, quando saiu sozinha.</param>
/// <param name="Encerramento">O que o teste precisou fazer porque ela não saiu no tempo; nulo se nada.</param>
internal sealed record SegundaInstancia(bool SaiuSozinha, int? Codigo, string? Encerramento);

/// <summary>
/// Um Buzzy.exe iniciado pelo teste com <c>--diagnostico</c>. Garante que não havia outro
/// Buzzy aberto (e nunca encerra um que não abriu), confere que as janelas registradas no log
/// são deste processo antes de mandar qualquer mensagem a elas e lê só o trecho do log escrito
/// depois de iniciar. Se a partida falhar em qualquer ponto, fecha o que abriu.
/// </summary>
internal sealed class BuzzyEmTeste : IDisposable
{
    private readonly long _inicioDoLog;

    internal static string ArquivoDeLog { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Buzzy", "diagnostico.log");

    private BuzzyEmTeste(Process processo, long inicioDoLog)
    {
        Processo = processo;
        _inicioDoLog = inicioDoLog;
    }

    internal Process Processo { get; }

    /// <summary>Hora de criação do processo, para separar os filhos dele de PIDs reaproveitados.</summary>
    internal DateTime Inicio { get; private set; }

    /// <summary>Janela do personagem, já conferida como deste processo (0 até lá).</summary>
    internal nint Janela { get; private set; }

    /// <summary>Janela de serviço, já conferida como deste processo (0 até lá).</summary>
    internal nint Servico { get; private set; }

    internal static void ExigirNenhumBuzzyAberto()
    {
        Process[] abertos = Process.GetProcessesByName("Buzzy");
        try
        {
            if (abertos.Length > 0)
                throw new InvalidOperationException($"Já há Buzzy aberto (pids {string.Join(", ", abertos.Select(p => p.Id))}); o teste não encerra processos que não abriu.");
        }
        finally
        {
            foreach (Process p in abertos) p.Dispose();
        }
    }

    /// <summary>
    /// O Buzzy recusa rodar elevado: mostra uma caixa de aviso na tela e sai com código 5. Um
    /// teste elevado passaria a elevação ao Buzzy que abre; por isso ele nem chega a abrir.
    /// </summary>
    internal static void ExigirTesteSemElevacao()
    {
        if (Environment.IsPrivilegedProcess)
            throw new InvalidOperationException("O teste está rodando como administrador: o Buzzy aberto por ele herdaria a elevação, recusaria rodar e mostraria uma caixa de aviso. Rode os testes sem elevação.");
    }

    internal static long TamanhoDoLog() => File.Exists(ArquivoDeLog) ? new FileInfo(ArquivoDeLog).Length : 0;

    /// <summary>
    /// Inicia um Buzzy.exe com <c>--diagnostico</c>, sem conferências (a segunda instância usa isto).
    /// Com <paramref name="pausado"/>, o movimento autônomo começa pausado: o personagem fica no
    /// lugar inicial, como os testes de gesto e de janela esperam.
    /// </summary>
    internal static Process IniciarProcesso(bool pausado = true)
    {
        var psi = new ProcessStartInfo(Caminhos.ExeDoBuzzy()) { UseShellExecute = false };
        psi.ArgumentList.Add("--diagnostico");
        if (pausado) psi.ArgumentList.Add("--pausado");
        return Process.Start(psi) ?? throw new InvalidOperationException("Buzzy.exe não iniciou.");
    }

    internal static BuzzyEmTeste Iniciar(bool pausado = true)
    {
        ExigirTesteSemElevacao();
        string exe = Caminhos.ExeDoBuzzy();
        if (!File.Exists(exe))
            throw new InvalidOperationException($"Buzzy.exe não compilado em {exe} (configuração {Caminhos.Configuracao}, a mesma deste executável de testes).");

        // O trecho do log é marcado ANTES de iniciar: as primeiras linhas do Buzzy podem ser
        // gravadas antes de o Process.Start voltar.
        long inicioDoLog = TamanhoDoLog();
        ExigirNenhumBuzzyAberto(); // repetida imediatamente antes de iniciar
        var b = new BuzzyEmTeste(IniciarProcesso(pausado), inicioDoLog);
        try
        {
            b.Inicio = b.Processo.StartTime;
            EventoDoLog janela = b.Esperar(e => e.Chave == "JANELA", 15000, "janela do personagem");
            b.Janela = b.JanelaDesteProcesso(janela["hwnd"], "JANELA");
            EventoDoLog servico = b.Esperar(e => e.Chave == "SERVICO", 5000, "janela de serviço");
            b.Servico = b.JanelaDesteProcesso(servico["hwnd"], "SERVICO");
            b.Esperar(e => e.Chave == "BANDEJA" && e.Campos.ContainsKey("adicionado"), 5000, "ícone da bandeja");
            return b;
        }
        catch
        {
            b.Dispose(); // WM_CLOSE (se a janela já foi conferida), espera e, se preciso, encerra só este processo
            throw;
        }
    }

    /// <summary>
    /// Abre uma segunda instância, que deve só pedir para a primeira aparecer e sair (código 0;
    /// 6 se não conseguir falar com a primeira). Se não sair no tempo, é encerrada — foi o teste
    /// que a abriu — e o fato volta em <see cref="SegundaInstancia.Encerramento"/>.
    /// </summary>
    internal static SegundaInstancia AbrirSegundaInstancia(int limiteMs = 8000)
    {
        Process segunda = IniciarProcesso();
        bool saiu = false;
        int? codigo = null;
        string? encerramento = null;
        try
        {
            saiu = segunda.WaitForExit(limiteMs);
            if (saiu) codigo = segunda.ExitCode;
        }
        finally
        {
            if (!segunda.HasExited)
            {
                encerramento = EncerrarAForca(segunda, "segunda instância");
                Console.WriteLine("         " + encerramento);
            }
            segunda.Dispose();
        }
        return new SegundaInstancia(saiu, codigo, encerramento);
    }

    /// <summary>Significado dos códigos de saída do Buzzy.exe (CodigosDeSaida.cs), para as mensagens de falha.</summary>
    internal static string DescreverCodigo(int? codigo) => codigo switch
    {
        null => "não saiu",
        CodigosDeSaida.Normal => "saída normal; logo na partida, quer dizer que outra instância já estava aberta",
        CodigosDeSaida.TopologiaIlegivel => "topologia dos monitores ilegível na partida",
        CodigosDeSaida.Elevado => "recusou rodar elevado",
        CodigosDeSaida.InstanciaUnicaIndisponivel => "instância única indisponível, ou a primeira instância não respondeu",
        _ => "código não documentado",
    };

    /// <summary>Eventos do log escritos desde o início deste Buzzy (inclui os de outras instâncias do mesmo período).</summary>
    internal List<EventoDoLog> Eventos() => EventosDesde(_inicioDoLog);

    internal static List<EventoDoLog> EventosDesde(long inicio)
    {
        if (!File.Exists(ArquivoDeLog)) return [];
        using var fs = new FileStream(ArquivoDeLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        if (inicio > fs.Length) inicio = 0; // o log foi rotacionado
        fs.Seek(inicio, SeekOrigin.Begin);
        using var sr = new StreamReader(fs, new UTF8Encoding(false));
        var eventos = new List<EventoDoLog>();
        string? linha;
        while ((linha = sr.ReadLine()) is not null)
        {
            if (EventoDoLog.Ler(linha) is { } e) eventos.Add(e);
        }
        return eventos;
    }

    internal EventoDoLog Esperar(Func<EventoDoLog, bool> condicao, int limiteMs, string oQue)
    {
        var fim = DateTime.UtcNow.AddMilliseconds(limiteMs);
        while (true)
        {
            EventoDoLog? achado = Eventos().LastOrDefault(condicao);
            if (achado is not null) return achado;
            if (Processo.HasExited)
                throw new InvalidOperationException($"O Buzzy encerrou (código {Processo.ExitCode}: {DescreverCodigo(Processo.ExitCode)}) antes de registrar: {oQue}.");
            if (DateTime.UtcNow > fim)
                throw new TimeoutException($"Tempo esgotado esperando no log: {oQue}.");
            Thread.Sleep(100);
        }
    }

    internal RetanguloPx RetanguloDaJanela()
    {
        NativoTeste.GetWindowRect(Janela, out NativoTeste.RECT r);
        return new RetanguloPx(r.Left, r.Top, r.Right, r.Bottom);
    }

    /// <summary>
    /// Posta uma mensagem de mouse à janela do personagem deste Buzzy, com o ponto de tela
    /// convertido para coordenadas de cliente pela posição ATUAL da janela (sem borda: o cliente é
    /// a janela inteira). Não é input: nada passa pela fila de input do Windows nem por outro
    /// aplicativo; o resultado é rotulado como mensagem postada, não como gesto.
    /// </summary>
    internal void PostarMouse(int mensagem, nint wParam, PontoPx tela)
    {
        RetanguloPx r = RetanguloDaJanela();
        int x = tela.X - r.Esquerda, y = tela.Y - r.Topo;
        nint lParam = (nint)(((y & 0xFFFF) << 16) | (x & 0xFFFF));
        if (!NativoTeste.PostMessage(Janela, mensagem, wParam, lParam))
            throw new InvalidOperationException($"PostMessage 0x{mensagem:X4} à janela do Buzzy falhou.");
    }

    /// <summary>Espera a janela do personagem chegar ao retângulo dado.</summary>
    internal void EsperarRetangulo(RetanguloPx esperado, int limiteMs, string oQue)
    {
        var fim = DateTime.UtcNow.AddMilliseconds(limiteMs);
        RetanguloPx atual;
        while ((atual = RetanguloDaJanela()) != esperado)
        {
            if (Processo.HasExited) throw new InvalidOperationException($"O Buzzy encerrou antes de: {oQue}.");
            if (DateTime.UtcNow > fim) throw new TimeoutException($"Tempo esgotado esperando {oQue}: janela em {atual}, esperado {esperado}.");
            Thread.Sleep(20);
        }
    }

    /// <summary>Fecha pelo WM_CLOSE da janela do personagem e devolve o código de saída.</summary>
    internal int FecharPorWmClose(int limiteMs = 5000)
    {
        NativoTeste.PostMessage(Janela, NativoTeste.WM_CLOSE, 0, 0);
        if (!Processo.WaitForExit(limiteMs))
            throw new TimeoutException("O Buzzy não encerrou depois de WM_CLOSE.");
        return Processo.ExitCode;
    }

    /// <summary>
    /// Fecha o Buzzy que ESTE teste abriu: WM_CLOSE à janela do personagem, só se ela já foi
    /// conferida como deste processo; espera; se preciso, encerra só este processo. Não lança,
    /// para não esconder a falha que levou até aqui.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (!Processo.HasExited)
            {
                if (Janela != 0) NativoTeste.PostMessage(Janela, NativoTeste.WM_CLOSE, 0, 0);
                if (!Processo.WaitForExit(Janela != 0 ? 5000 : 1000))
                    Console.WriteLine("         limpeza: " + EncerrarAForca(Processo, "Buzzy aberto pelo teste"));
            }
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception)
        {
            Console.WriteLine($"         limpeza: não deu para conferir ou encerrar o Buzzy aberto pelo teste: {e.Message}");
        }
        finally
        {
            Processo.Dispose();
        }
    }

    /// <summary>Encerra à força um processo que o próprio teste abriu, sem tocar em mais nenhum (nem nos filhos).</summary>
    internal static string EncerrarAForca(Process processo, string nome)
    {
        int pid = processo.Id;
        try
        {
            processo.Kill(entireProcessTree: false);
            processo.WaitForExit(5000);
            return $"o processo {pid} ({nome}) não encerrou no tempo e foi encerrado à força pelo teste que o abriu";
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception or NotSupportedException)
        {
            return processo.HasExited
                ? $"o processo {pid} ({nome}) encerrou sozinho durante a limpeza"
                : $"o processo {pid} ({nome}) não pôde ser encerrado: {e.Message}";
        }
    }

    /// <summary>Lê o HWND registrado no log e confere que a janela é deste processo; sem isso, nenhuma mensagem vai para ela.</summary>
    private nint JanelaDesteProcesso(string texto, string chave)
    {
        if (!long.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out long valor) || valor == 0)
            throw new InvalidOperationException($"HWND de {chave} ilegível no log: '{texto}'.");
        var hwnd = (nint)valor;
        uint pid = NativoTeste.PidDe(hwnd);
        if (pid != (uint)Processo.Id)
            throw new InvalidOperationException($"O HWND de {chave} ({hwnd}) registrado no log não pertence ao Buzzy aberto pelo teste (pid {Processo.Id}); nenhuma mensagem foi enviada a ele.");
        return hwnd;
    }
}

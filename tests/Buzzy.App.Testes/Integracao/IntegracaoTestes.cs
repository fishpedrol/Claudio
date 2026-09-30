using System.Diagnostics;
using System.Globalization;
using Buzzy.App.Plataforma;
using Buzzy.Core;
using Buzzy.Testes;

namespace Buzzy.App.Testes.Integracao;

/// <summary>
/// Testes de integração da Fase 1 (TODO.md: "teste de fumaça que inicia o app, encontra a
/// janela, confere os estilos e encerra"). Iniciam o Buzzy.exe de verdade, então só rodam
/// com --integracao, depois de avisar o usuário. Nenhum injeta input: mensagens são
/// enviadas só às janelas do Buzzy aberto pelo próprio teste.
/// </summary>
[Integracao]
internal sealed class IntegracaoTestes
{
    [Teste]
    public void Fumaca_IniciaNoChaoDaAreaUtilDoPrincipalComOsEstilosCertosEEncerraLimpo()
    {
        // As coordenadas lidas aqui só batem com as do log do Buzzy (pixels físicos) se este
        // processo também estiver em Per-Monitor V2 (app.manifest dos testes). Conferido antes
        // de abrir o Buzzy e de comparar qualquer coordenada.
        Afirmar.Verdadeiro(NativoTeste.ThreadEmPerMonitorV2(), "a thread do teste está em Per-Monitor V2 (coordenadas físicas, como o log do Buzzy)");

        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar();

        long ex = NativoTeste.EstiloEstendido(b.Janela);
        Afirmar.Verdadeiro((ex & NativoTeste.WS_EX_LAYERED) != 0, "WS_EX_LAYERED: transparência por pixel do WPF");
        Afirmar.Verdadeiro((ex & NativoTeste.WS_EX_NOACTIVATE) != 0, "WS_EX_NOACTIVATE: não rouba foco");
        Afirmar.Verdadeiro((ex & NativoTeste.WS_EX_TOOLWINDOW) != 0, "WS_EX_TOOLWINDOW: fora da barra de tarefas e do Alt+Tab");
        Afirmar.Verdadeiro((ex & NativoTeste.WS_EX_TOPMOST) != 0, "WS_EX_TOPMOST: sempre no topo por padrão");
        Afirmar.Falso((ex & NativoTeste.WS_EX_APPWINDOW) != 0, "sem WS_EX_APPWINDOW");
        Afirmar.Falso((ex & NativoTeste.WS_EX_TRANSPARENT) != 0, "sem WS_EX_TRANSPARENT: modo fantasma fora do MVP (Q-21)");
        Afirmar.Verdadeiro(NativoTeste.IsWindowVisible(b.Janela), "janela visível");

        EventoDoLog carregamento = b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Settling", 5000, "núcleo carregado");
        Afirmar.Igual("Booting", carregamento["de"], "o núcleo parte de Booting");
        Afirmar.Igual("início", carregamento["motivo"], "a carga mantém o motivo de inicialização no log");
        b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["de"] == "Settling" && e["para"] == "Idle", 5000, "posição inicial acomodada pelo núcleo");

        Topologia topologia = Afirmar.NaoNulo(LeitorDeTopologia.Ler(out string? erro), erro);
        MonitorDoDesktop principal = topologia.Principal;
        RetanguloPx janela = b.RetanguloDaJanela();
        TamanhoPx esperado = new TamanhoDip(128, 128).ParaPixels(principal.Dpi);
        Afirmar.Igual(esperado, janela.Tamanho, "tamanho da janela = tamanho do sprite no DPI do principal");
        Afirmar.Verdadeiro(principal.AreaUtil.Contem(janela), $"janela {janela} dentro da área útil {principal.AreaUtil} do principal");
        Afirmar.Igual(principal.AreaUtil.Base, janela.Base, "pés no chão da área útil");

        EventoDoLog posicao = b.Esperar(e => e.Chave == "POSICAO", 5000, "posição");
        Afirmar.Igual(janela, EventoDoLog.Retangulo(posicao["retangulo"]), "retângulo registrado = retângulo real");
        Afirmar.Igual(principal.Chave, posicao["monitor"]);

        EventoDoLog quadro = b.Esperar(e => e.Chave == "PRIMEIRO_QUADRO", 5000, "primeiro quadro (M6)");
        double ms = double.Parse(quadro["ms"], CultureInfo.InvariantCulture);
        Afirmar.Verdadeiro(ms > 0 && ms < 30000, $"M6 plausível: {ms} ms");
        Console.WriteLine($"         M6 tempo até o primeiro quadro: {ms} ms");

        EventoDoLog bandeja = b.Esperar(e => e.Chave == "BANDEJA" && e.Campos.ContainsKey("adicionado"), 1000, "bandeja");
        Afirmar.Igual("True", bandeja["adicionado"], "ícone adicionado à bandeja");
        Afirmar.Igual("True", bandeja["versao4"], "notificações na versão 4");

        // Só contam processos criados depois do início do Buzzy: o pai registrado pelo Windows
        // pode ser um PID antigo reaproveitado.
        Afirmar.Igual(0, NativoTeste.Filhos(b.Processo.Id, b.Inicio).Count, "o Buzzy não cria processos filhos");

        int codigo = b.FecharPorWmClose();
        Afirmar.Igual(0, codigo, "código de saída");
        List<EventoDoLog> ev = b.Eventos();
        Afirmar.Verdadeiro(ev.Any(e => e.Chave == "BANDEJA" && e["removido"] == "True"), "ícone removido ao sair");
        Afirmar.Verdadeiro(ev.Any(e => e.Chave == "FIM" && e["codigo"] == "0"), "FIM registrado");
        Afirmar.Igual(0, NativoTeste.Filhos(b.Processo.Id, b.Inicio).Count, "nenhum processo filho do Buzzy sobra depois do encerramento");
    }

    [Teste]
    public void InstanciaUnica_SegundaAberturaRevelaOBuzzyEscondidoESai()
    {
        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar();

        // Minimizar sem ativar nada: o Buzzy deve se esconder (Q-03: minimizar esconde).
        NativoTeste.ShowWindow(b.Janela, NativoTeste.SW_SHOWMINNOACTIVE);
        b.Esperar(e => e.Chave == "VISIVEL" && e["visivel"] == "nao", 3000, "escondido ao ser minimizado");
        Afirmar.Falso(NativoTeste.IsWindowVisible(b.Janela), "escondido depois de minimizado");

        // A segunda instância é encerrada pelo próprio teste se não sair no tempo (try/finally
        // dentro de AbrirSegundaInstancia), e o fato aparece na mensagem de falha.
        SegundaInstancia segunda = BuzzyEmTeste.AbrirSegundaInstancia();
        Afirmar.Verdadeiro(segunda.SaiuSozinha, $"a segunda instância sai sozinha ({segunda.Encerramento ?? "saiu"})");
        Afirmar.Igual<int?>(CodigosDeSaida.Normal, segunda.Codigo, $"código da segunda instância ({BuzzyEmTeste.DescreverCodigo(segunda.Codigo)})");

        b.Esperar(e => e.Chave == "INSTANCIA" && e["papel"] == "segunda" && e["pedidoEntregue"] == "True", 3000, "pedido da segunda instância");
        b.Esperar(e => e.Chave == "VISIVEL" && e["visivel"] == "sim" && e["motivo"] == "segunda instância", 3000, "reaparece pela segunda instância");
        Afirmar.Verdadeiro(NativoTeste.IsWindowVisible(b.Janela), "visível de novo");
        Afirmar.Igual(1, Process.GetProcessesByName("Buzzy").Length, "continua uma instância só");

        Afirmar.Igual(0, b.FecharPorWmClose());
    }

    [Teste]
    public void Topologia_MensagensDeMudancaSaoAgrupadasNumaReleituraEOSpriteContinuaNaAreaUtil()
    {
        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar();
        RetanguloPx antes = b.RetanguloDaJanela();
        long marca = BuzzyEmTeste.TamanhoDoLog();

        // As mesmas mensagens que o Windows envia ao trocar resolução e mover a barra de tarefas.
        // A topologia real não mudou, então a releitura precisa concluir "mudou=nao".
        // WM_DISPLAYCHANGE pode ser postada; WM_SETTINGCHANGE leva ponteiro e só vai por envio
        // síncrono, como o próprio Windows faz na difusão.
        Afirmar.Verdadeiro(NativoTeste.PostMessage(b.Servico, NativoTeste.WM_DISPLAYCHANGE, 32, (nint)(1920 | (1080 << 16))), "WM_DISPLAYCHANGE postada");
        nint enviada = NativoTeste.SendMessageTimeout(b.Servico, NativoTeste.WM_SETTINGCHANGE, NativoTeste.SPI_SETWORKAREA, 0, NativoTeste.SMTO_ABORTIFHUNG, 2000, out _);
        Afirmar.Verdadeiro(enviada != 0, "WM_SETTINGCHANGE(SPI_SETWORKAREA) entregue");

        b.Esperar(e => e.Chave == "TOPOLOGIA" && e["motivo"].Contains("WM_DISPLAYCHANGE", StringComparison.Ordinal), 3000, "releitura da topologia");
        Thread.Sleep(700);
        List<EventoDoLog> releituras = [.. BuzzyEmTeste.EventosDesde(marca).Where(e => e.Chave == "TOPOLOGIA")];
        Afirmar.Igual(1, releituras.Count, "as duas mensagens viram uma releitura só");
        string[] motivos = releituras[0]["motivo"].Split(',');
        Afirmar.Verdadeiro(motivos.Contains("WM_DISPLAYCHANGE") && motivos.Contains("SPI_SETWORKAREA"), $"a releitura junta as duas mensagens (motivo={releituras[0]["motivo"]})");
        Afirmar.Igual("nao", releituras[0]["mudou"], "topologia real igual");

        RetanguloPx depois = b.RetanguloDaJanela();
        Afirmar.Igual(antes, depois, "sprite no mesmo lugar");
        Topologia t = Afirmar.NaoNulo(LeitorDeTopologia.Ler(out string? erro), erro);
        Afirmar.Verdadeiro(t.Principal.AreaUtil.Contem(depois), "sprite dentro da área útil");

        Afirmar.Igual(0, b.FecharPorWmClose());
    }

    [Teste]
    public void Bandeja_IconeERecriadoQuandoABarraDeTarefasReinicia()
    {
        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar();
        long marca = BuzzyEmTeste.TamanhoDoLog();
        int taskbarCreated = NativoTeste.RegisterWindowMessage("TaskbarCreated");
        Afirmar.Verdadeiro(taskbarCreated != 0, "mensagem TaskbarCreated registrada");

        NativoTeste.PostMessage(b.Servico, taskbarCreated, 0, 0);

        var fim = DateTime.UtcNow.AddSeconds(3);
        List<EventoDoLog> depois = [];
        while (DateTime.UtcNow < fim)
        {
            depois = BuzzyEmTeste.EventosDesde(marca);
            if (depois.Any(e => e.Chave == "BANDEJA" && e.Campos.ContainsKey("adicionado"))) break;
            Thread.Sleep(100);
        }
        Afirmar.Verdadeiro(depois.Any(e => e.Chave == "BANDEJA" && e["barraDeTarefasRecriada"] == "sim"), "reagiu a TaskbarCreated");
        Afirmar.Verdadeiro(depois.Any(e => e.Chave == "BANDEJA" && e["adicionado"] == "True"), "ícone adicionado de novo");

        Afirmar.Igual(0, b.FecharPorWmClose());
    }
}

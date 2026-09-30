using System.Globalization;
using Buzzy.Core;
using Buzzy.Testes;

namespace Buzzy.App.Testes.Integracao;

/// <summary>
/// Testes de integração da raiz de composição (Aplicacao.cs) pedidos pela auditoria da Fase 2:
/// o menu não prende eventos na fila, o relógio de passo fixo só corre quando o núcleo pede e a
/// janela volta ao lugar do núcleo quando alguém a move por fora. Mensagens só são postadas às
/// janelas do Buzzy aberto pelo próprio teste; nada injeta input. Só rodam com --integracao.
/// </summary>
[Integracao]
internal sealed class ComposicaoTestes
{
    private static EventoDoLog EsperarDesde(BuzzyEmTeste b, long marca, Func<EventoDoLog, bool> condicao, int limiteMs, string oQue)
    {
        var fim = DateTime.UtcNow.AddMilliseconds(limiteMs);
        while (true)
        {
            EventoDoLog? achado = BuzzyEmTeste.EventosDesde(marca).FirstOrDefault(condicao);
            if (achado is not null) return achado;
            if (b.Processo.HasExited) throw new InvalidOperationException($"O Buzzy encerrou antes de registrar: {oQue}.");
            if (DateTime.UtcNow > fim) throw new TimeoutException($"Tempo esgotado esperando no log: {oQue}.");
            Thread.Sleep(50);
        }
    }

    private static PontoPx PontoOpaco(BuzzyEmTeste b)
        => EventoDoLog.Ponto(b.Esperar(e => e.Chave == "POSICAO", 5000, "posição")["pontoOpaco"]);

    [Teste]
    public void MenuAberto_WmCloseEncerraNaHoraSemFicarPresoNaFila()
    {
        // Regressão: antes, o laço modal do menu rodava dentro do processamento do núcleo e o
        // CMD_EXIT do WM_CLOSE ficava na fila até o menu fechar.
        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar();
        b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Idle", 5000, "núcleo carregado");
        PontoPx opaco = PontoOpaco(b);
        long marca = BuzzyEmTeste.TamanhoDoLog();

        b.PostarMouse(NativoTeste.WM_RBUTTONDOWN, 0, opaco);
        b.PostarMouse(NativoTeste.WM_RBUTTONUP, 0, opaco);
        EsperarDesde(b, marca, e => e.Chave == "MENU" && e["exibindo"] == "sim", 3000, "menu exibido");

        int codigo = b.FecharPorWmClose(3000);
        Afirmar.Igual(0, codigo, "encerrou com o menu aberto");
        List<EventoDoLog> ev = BuzzyEmTeste.EventosDesde(marca);
        Afirmar.Verdadeiro(ev.Any(e => e.Chave == "NUCLEO" && e["evento"] == "CmdExit" && e["para"] == "Exiting"), "CMD_EXIT aplicado pelo núcleo");
        Afirmar.Verdadeiro(ev.Any(e => e.Chave == "ENCERRANDO"), "ENCERRANDO registrado");
    }

    [Teste]
    public void Relogio_ParadoEmRepouso_LigaNaReacaoEDesligaNoFim()
    {
        // Com a autonomia ligada: a agenda precisa voltar depois da reação.
        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar(pausado: false);
        b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Idle", 5000, "núcleo carregado");
        EventoDoLog agenda = b.Esperar(e => e.Chave == "AGENDA" && e.Campos.ContainsKey("atrasoMs"), 3000, "agenda autônoma depois da carga");
        long atraso = long.Parse(agenda["atrasoMs"], CultureInfo.InvariantCulture);
        Afirmar.Verdadeiro(atraso >= 3000, $"a primeira decisão só vem depois do intervalo de acomodação ({atraso} ms)");

        // Critério 3 da Fase 2, no app: em repouso (IDLE sem gesto), o relógio de passo fixo não liga.
        Thread.Sleep(700);
        Afirmar.Falso(b.Eventos().Any(e => e.Chave == "RELOGIO" && e["ligado"] == "sim"), "sem relógio em repouso depois da carga");

        long marca = BuzzyEmTeste.TamanhoDoLog();
        PontoPx opaco = PontoOpaco(b);
        b.PostarMouse(NativoTeste.WM_LBUTTONDOWN, NativoTeste.MK_LBUTTON, opaco);
        b.PostarMouse(NativoTeste.WM_LBUTTONUP, 0, opaco);
        EsperarDesde(b, marca, e => e.Chave == "RELOGIO" && e["ligado"] == "sim" && e["evento"] == "Click", 3000, "relógio ligado pela reação ao clique");
        EsperarDesde(b, marca, e => e.Chave == "RELOGIO" && e["ligado"] == "nao", 3000, "relógio desligado no fim da reação");
        EsperarDesde(b, marca, e => e.Chave == "AGENDA" && e["cancelada"] == "sim", 3000, "agenda cancelada durante o gesto");
        EventoDoLog nova = EsperarDesde(b, marca, e => e.Chave == "AGENDA" && e.Campos.ContainsKey("atrasoMs"), 3000, "agenda de volta depois da reação");
        Afirmar.Verdadeiro(long.Parse(nova["atrasoMs"], CultureInfo.InvariantCulture) >= 3000, "a autonomia volta só depois do intervalo de acomodação");

        Afirmar.Igual(0, b.FecharPorWmClose());
    }

    [Teste]
    public void JanelaMovidaPorFora_VoltaAoLugarDoNucleoNaReleituraDaTopologia()
    {
        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar();
        b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Idle", 5000, "núcleo carregado");
        RetanguloPx lugar = b.RetanguloDaJanela();
        long marca = BuzzyEmTeste.TamanhoDoLog();

        // Outro agente (aqui, o teste) move a janela sem passar pelo núcleo.
        Afirmar.Verdadeiro(NativoTeste.SetWindowPos(b.Janela, 0, lugar.Esquerda - 300, lugar.Topo - 200, 0, 0,
            NativoTeste.SWP_NOSIZE | NativoTeste.SWP_NOZORDER | NativoTeste.SWP_NOACTIVATE), "janela movida por fora");
        Afirmar.Diferente(lugar, b.RetanguloDaJanela(), "a janela saiu do lugar");

        // Uma releitura de topologia (sem mudança real) reafirma o lugar do núcleo.
        Afirmar.Verdadeiro(NativoTeste.PostMessage(b.Servico, NativoTeste.WM_DISPLAYCHANGE, 32, (nint)(1920 | (1080 << 16))), "WM_DISPLAYCHANGE postada");
        EsperarDesde(b, marca, e => e.Chave == "POSICAO" && e["reaplicada"] == "sim", 3000, "lugar reaplicado");
        b.EsperarRetangulo(lugar, 3000, "de volta ao lugar do núcleo");

        Afirmar.Igual(0, b.FecharPorWmClose());
    }
}

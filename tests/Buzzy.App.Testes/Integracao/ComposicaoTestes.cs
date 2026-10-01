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
        long marca = BuzzyEmTeste.MarcaDoLog();

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

        long marca = BuzzyEmTeste.MarcaDoLog();
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
        long marca = BuzzyEmTeste.MarcaDoLog();

        // Outro agente (aqui, o teste) move a janela sem passar pelo núcleo.
        Afirmar.Verdadeiro(b.MoverPorFora(b.Janela, 0, lugar.Esquerda - 300, lugar.Topo - 200, NativoTeste.SWP_NOZORDER, "mover a janela"), "janela movida por fora");
        Afirmar.Diferente(lugar, b.RetanguloDaJanela(), "a janela saiu do lugar");

        // Uma releitura de topologia (sem mudança real) reafirma o lugar do núcleo.
        Afirmar.Verdadeiro(b.Postar(b.Servico, NativoTeste.WM_DISPLAYCHANGE, 32, (nint)(1920 | (1080 << 16)), "WM_DISPLAYCHANGE"), "WM_DISPLAYCHANGE postada");
        EsperarDesde(b, marca, e => e.Chave == "POSICAO" && e["reaplicada"] == "sim", 3000, "lugar reaplicado");
        b.EsperarRetangulo(lugar, 3000, "de volta ao lugar do núcleo");

        Afirmar.Igual(0, b.FecharPorWmClose());
    }

    [Teste]
    public void ReafirmacaoTardia_JanelaMovidaDepoisDaReleitura_VoltaAoLugar_UmaVezSo()
    {
        // Fase 5, passo P9 (D14 do desenho dos monitores): com "Lembrar locais das janelas", o Windows pode devolver a janela
        // ao monitor reconectado DEPOIS da releitura. A conferência tardia, 1,5 s depois de cada releitura publicada,
        // reafirma o lugar do núcleo, uma vez: nada periódico.
        using BuzzyEmTeste b = BuzzyEmTeste.Iniciar();
        b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Idle", 5000, "núcleo carregado");
        RetanguloPx lugar = b.RetanguloDaJanela();
        long marca = BuzzyEmTeste.MarcaDoLog();

        Afirmar.Verdadeiro(b.Postar(b.Servico, NativoTeste.WM_DISPLAYCHANGE, 32, (nint)(1920 | (1080 << 16)), "WM_DISPLAYCHANGE"), "WM_DISPLAYCHANGE postada");
        EventoDoLog releitura = EsperarDesde(b, marca, e => e.Chave == "TOPOLOGIA" && e["motivo"] == "WM_DISPLAYCHANGE", 3000, "releitura da topologia");

        // Depois da releitura, outro agente (aqui, o teste) move a janela sem passar pelo núcleo.
        long marcaMovida = BuzzyEmTeste.MarcaDoLog();
        RetanguloPx fora = lugar.Deslocado(-300, -200);
        Afirmar.Verdadeiro(b.MoverPorFora(b.Janela, 0, fora.Esquerda, fora.Topo, NativoTeste.SWP_NOZORDER, "mover a janela"), "janela movida por fora depois da releitura");
        Afirmar.Igual(fora, b.RetanguloDaJanela(), "a janela saiu do lugar");

        EventoDoLog reaplicada = EsperarDesde(b, marcaMovida, e => e.Chave == "POSICAO" && e["reaplicada"] == "sim", 4000, "lugar reaplicado pela conferência tardia");
        Afirmar.Igual("reafirmação tardia", reaplicada["motivo"], "o motivo da reaplicação");
        double ms = (reaplicada.Instante - releitura.Instante).TotalMilliseconds;
        Afirmar.Verdadeiro(ms >= 1400 && ms < 3000, $"1,5 s depois da releitura: {ms:0} ms");
        Afirmar.Igual(fora, EventoDoLog.Retangulo(reaplicada["real"]), "o log traz onde a janela estava");
        b.EsperarRetangulo(lugar, 2000, "de volta ao lugar do núcleo");

        // Uma vez por releitura: movida de novo, sem mensagem nova, a janela fica onde foi posta.
        long marcaDeNovo = BuzzyEmTeste.MarcaDoLog();
        Afirmar.Verdadeiro(b.MoverPorFora(b.Janela, 0, fora.Esquerda, fora.Topo, NativoTeste.SWP_NOZORDER, "mover a janela"), "janela movida por fora de novo");
        Thread.Sleep(2500);
        Afirmar.Igual(fora, b.RetanguloDaJanela(), "nenhuma conferência sem releitura: nada periódico");
        Afirmar.Falso(BuzzyEmTeste.EventosDesde(marcaDeNovo).Any(e => e.Chave is "POSICAO" or "TOPOLOGIA"), "nem reaplicação nem releitura sem mensagem");

        Afirmar.Igual(0, b.FecharPorWmClose());
    }
}

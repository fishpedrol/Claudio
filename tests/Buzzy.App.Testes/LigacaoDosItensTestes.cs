using Buzzy.App.Apresentacao;
using Buzzy.App.Composicao;
using Buzzy.Core;
using Buzzy.Core.Entrada;
using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.App.Testes;

/// <summary>
/// A ligação da raiz com os itens do tamagotchi (DEC-028; revisão de correção do app, achados 1, 2 e 7), sem a raiz e sem
/// janela de verdade: o que um evento de ponteiro numa janela de item faz com a captura e a ordem Z
/// (<see cref="LigacaoDosItens.ReceberPonteiro"/>) e o que cada efeito novo do núcleo faz com o árbitro dos itens, as
/// janelas e o temporizador da onda (<see cref="LigacaoDosItens.Executar"/>). As janelas são falsas e o temporizador é o
/// de verdade, sem disparar (nenhum teste aqui roda o laço de mensagens).
/// </summary>
internal sealed class LigacaoDosItensTestes
{
    private const nint HwndDoPersonagem = 77;

    private static readonly MonitorDoDesktop M96 = new("m1", new RetanguloPx(0, 0, 1920, 1080), new RetanguloPx(0, 0, 1920, 1040), 96, true);

    private static readonly MetricasDeGesto Metricas = new(4, 4, 4, 4, 500);

    private sealed class Cenario
    {
        internal Cenario()
        {
            Itens = new GerenteDosItens(id =>
            {
                var janela = new JanelaDoItemFalsa(id);
                Criadas.Add(janela);
                return janela;
            }, () => HwndDoPersonagem);
            Onda = new TemporizadorDaOnda(Disparos.Add);
        }

        internal GestosDosItens Gestos { get; } = new();

        internal GerenteDosItens Itens { get; }

        internal TemporizadorDaOnda Onda { get; }

        internal List<long> Disparos { get; } = [];

        internal List<JanelaDoItemFalsa> Criadas { get; } = [];

        internal JanelaDoItemFalsa Janela(int id) => Criadas.Single(j => j.Id == id);

        internal void Mostrar(int id, Item item, int x)
            => LigacaoDosItens.Executar(new MostrarItem(id, item, Lugar(x)), Gestos, Itens, Onda);

        internal GestoDoItem Ponteiro(int id, EventoDePonteiro evento) => LigacaoDosItens.ReceberPonteiro(Gestos, Itens, id, evento);

        internal void Executar(Efeito efeito) => LigacaoDosItens.Executar(efeito, Gestos, Itens, Onda);

        internal void Limpar()
        {
            foreach (JanelaDoItemFalsa j in Criadas) j.Chamadas.Clear();
        }
    }

    private static Posicionamento Lugar(int x)
    {
        TamanhoPx tamanho = SpriteDoItem.TamanhoLogico.ParaPixels(M96.Dpi);
        var ancora = new PontoPx(x, 1040);
        return new Posicionamento(M96, ancora, tamanho, Posicionador.RetanguloDoSprite(ancora, tamanho));
    }

    private static PonteiroPressionado Esquerdo(int x, int y, long ms = 0) => new(new PontoPx(x, y), BotaoDoPonteiro.Esquerdo, ms, Metricas);

    private static PonteiroSolto SoltarEsquerdo(int x, int y, long ms = 0) => new(new PontoPx(x, y), BotaoDoPonteiro.Esquerdo, ms);

    private static PonteiroMovido Mover(int x, int y) => new(new PontoPx(x, y), true, 0);

    [Teste]
    public void GestoNumItem_CapturaENoTopo_EnoFimSoltaEVoltaParaBaixoDoPersonagem()
    {
        // Achado 1 da revisão: sem o fim do gesto, a janela do item solto ficaria com a captura do mouse e acima dele.
        var c = new Cenario();
        c.Mostrar(1, Item.Banana, 500);
        c.Limpar();

        GestoDoItem press = c.Ponteiro(1, Esquerdo(500, 1010));
        Afirmar.Sequencia<Evento>([new ItemPress(1, new PontoPx(500, 1010))], press.Eventos);
        Afirmar.Sequencia(["capturar", "frente"], c.Janela(1).Chamadas, "botão pressionado: a janela do item captura e vai para o topo (L17)");
        c.Limpar();

        c.Ponteiro(1, Mover(520, 900));
        Afirmar.Sequencia([], c.Janela(1).Chamadas, "no arraste, a captura e a ordem Z ficam como estão");

        GestoDoItem fim = c.Ponteiro(1, SoltarEsquerdo(540, 800));
        Afirmar.Sequencia<Evento>([new ItemDragEnd(1, new PontoPx(540, 800))], fim.Eventos);
        Afirmar.Sequencia(["soltar", $"abaixo de {HwndDoPersonagem}"], c.Janela(1).Chamadas, "fim do gesto: solta o mouse e volta para logo abaixo do personagem");
        Afirmar.Falso(c.Janela(1).Capturando, "nenhuma janela de item fica com a captura");
        Afirmar.Nulo(c.Gestos.ItemEmGesto);

        // Um clique (sem arraste) também termina o gesto.
        c.Limpar();
        c.Ponteiro(1, Esquerdo(500, 1010, ms: 2000));
        c.Ponteiro(1, SoltarEsquerdo(500, 1010, ms: 2050));
        Afirmar.Sequencia(["capturar", "frente", "soltar", $"abaixo de {HwndDoPersonagem}"], c.Janela(1).Chamadas, "clique: pega e larga");
    }

    [Teste]
    public void PressionarOutroItemNoMeioDoGesto_OAnteriorSoltaEVoltaParaBaixo_ONovoCaptura()
    {
        var c = new Cenario();
        c.Mostrar(1, Item.Banana, 300);
        c.Mostrar(2, Item.Agua, 700);
        c.Ponteiro(1, Esquerdo(300, 1010));
        c.Ponteiro(1, Mover(330, 950));
        c.Limpar();

        GestoDoItem outro = c.Ponteiro(2, Esquerdo(700, 1010));
        Afirmar.Sequencia<Evento>([new ItemRelease(1), new ItemPress(2, new PontoPx(700, 1010))], outro.Eventos);
        Afirmar.Sequencia(["soltar", $"abaixo de {HwndDoPersonagem}"], c.Janela(1).Chamadas, "o item largado solta o mouse e volta para baixo");
        Afirmar.Sequencia(["capturar", "frente"], c.Janela(2).Chamadas, "o novo captura e vai para o topo");
    }

    [Teste]
    public void LiberarCapturaDoItem_EsqueceOGesto_SoltaAJanela_EOSoltarTardioNaoViraNada()
    {
        // Achado 2 da revisão: o Windows minimiza o personagem (ou a sessão bloqueia) com o item na mão; o núcleo encerra o
        // gesto e manda LIBERAR_CAPTURA_DO_ITEM. O soltar que chega depois não pode virar ITEM_DRAG_END nem ITEM_RELEASE.
        var c = new Cenario();
        c.Mostrar(3, Item.Cerveja, 400);
        c.Ponteiro(3, Esquerdo(400, 1010));
        c.Ponteiro(3, Mover(380, 950));
        c.Limpar();

        c.Executar(new LiberarCapturaDoItem(3));
        Afirmar.Nulo(c.Gestos.ItemEmGesto, "o árbitro dos itens esqueceu o gesto");
        Afirmar.Sequencia(["soltar", $"abaixo de {HwndDoPersonagem}"], c.Janela(3).Chamadas, "a janela solta o mouse e volta para baixo");
        c.Limpar();

        GestoDoItem tardio = c.Ponteiro(3, SoltarEsquerdo(360, 900));
        Afirmar.Sequencia<Evento>([], tardio.Eventos, "o soltar tardio não vira evento");
        Afirmar.Sequencia([], c.Janela(3).Chamadas, "nem mexe na janela");

        // LIBERAR_CAPTURA_DO_ITEM de outro item não esquece o gesto em curso.
        c.Ponteiro(3, Esquerdo(400, 1010, ms: 5000));
        c.Executar(new LiberarCapturaDoItem(99));
        Afirmar.Igual((int?)3, c.Gestos.ItemEmGesto, "o gesto do item 3 continua");
    }

    [Teste]
    public void RemoverItemNoMeioDoProprioGesto_EsqueceOGestoEFechaAJanela()
    {
        var c = new Cenario();
        c.Mostrar(4, Item.Cafe, 400);
        c.Mostrar(5, Item.Bala, 800);
        c.Ponteiro(4, Esquerdo(400, 1010));
        c.Limpar();

        c.Executar(new RemoverItem(5, MotivoDaRemocao.Substituido));
        Afirmar.Igual((int?)4, c.Gestos.ItemEmGesto, "remover outro item não mexe no gesto");
        c.Executar(new RemoverItem(4, MotivoDaRemocao.Recolhido));
        Afirmar.Nulo(c.Gestos.ItemEmGesto, "o item do gesto saiu: o árbitro esquece");
        Afirmar.Sequencia(["soltar", "fechar"], c.Janela(4).Chamadas);
        Afirmar.Sequencia<Evento>([], c.Ponteiro(4, SoltarEsquerdo(400, 1010)).Eventos, "o soltar que chega depois não vira nada");
    }

    [Teste]
    public void AgendarECancelarOnda_OTemporizadorAcompanha()
    {
        // Achado 7 da revisão (B3): CANCELAR_ONDA cancela o disparo pendente, que senão chegaria ao núcleo com a geração velha.
        var c = new Cenario();
        c.Executar(new AgendarOnda(TimeSpan.FromSeconds(30), 6));
        Afirmar.Igual((true, (long?)6, true), (c.Onda.Pendente, c.Onda.GeracaoPendente, c.Onda.Ligado), "AGENDAR_ONDA: um disparo pendente, com a geração");
        c.Executar(new AgendarOnda(TimeSpan.FromSeconds(30), 7));
        Afirmar.Igual((long?)7, c.Onda.GeracaoPendente, "o agendamento novo substitui o pendente");
        c.Executar(new CancelarOnda());
        Afirmar.Falso(c.Onda.Pendente || c.Onda.Ligado, "CANCELAR_ONDA: nada pendente e o DispatcherTimer desligado");
        Afirmar.Sequencia([], c.Disparos, "nada disparou");
        c.Executar(new CancelarOnda());
        Afirmar.Falso(c.Onda.Pendente, "cancelar sem nada pendente não lança");
    }

    [Teste]
    public void EfeitosDasJanelasVaoAoGerente_EOutroEfeitoLanca()
    {
        var c = new Cenario();
        c.Mostrar(6, Item.Vodka, 600);
        Afirmar.Igual((1, 1), (c.Itens.Quantas, c.Itens.Visiveis), "MOSTRAR_ITEM: a janela criada e à vista");
        c.Executar(new MoverItem(6, Lugar(650)));
        Afirmar.Igual($"retangulo {Lugar(650).Retangulo}", c.Janela(6).Chamadas[^1], "MOVER_ITEM: o lugar novo");
        c.Executar(new EsconderItem(6));
        Afirmar.Igual((1, 0), (c.Itens.Quantas, c.Itens.Visiveis), "ESCONDER_ITEM");
        Afirmar.Lanca<ArgumentOutOfRangeException>(() => c.Executar(new MoverJanela(Lugar(1))), "um efeito que não é do tamagotchi");

        // Sem as janelas e o temporizador (antes de criados, ou depois de encerrados), nada lança.
        var g = new GestosDosItens();
        LigacaoDosItens.Executar(new CancelarOnda(), g, null, null);
        LigacaoDosItens.Executar(new MostrarItem(1, Item.Bala, Lugar(10)), g, null, null);
        Afirmar.Nulo(g.ItemEmGesto);
    }
}

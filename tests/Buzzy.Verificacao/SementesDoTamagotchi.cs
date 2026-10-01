using System.Globalization;
using Buzzy.Core;
using Buzzy.Core.Personagem;
using Buzzy.Core.Testes.Movimento;

namespace Buzzy.Verificacao;

/// <summary>O que a simulação do núcleo previu para uma semente, para as esperas do cenário e para o relatório.</summary>
/// <param name="Semente">A semente do Buzzy (<c>--semente</c>).</param>
/// <param name="PrimeiraDecisao">O atraso da primeira decisão da agenda depois da carga.</param>
/// <param name="Resumo">O que a simulação viu, em uma linha.</param>
internal sealed record Previsao(ulong Semente, TimeSpan PrimeiraDecisao, string Resumo)
{
    /// <summary>A velocidade de andar da fase da onda na caminhada prevista, em % da física (100 sem onda).</summary>
    internal int VelocidadePercentual { get; init; } = 100;

    /// <summary>A onda da frente na caminhada prevista, como na linha do retrato (Tipo/Fase/Nível), ou "-".</summary>
    internal string Onda { get; init; } = "-";
}

/// <summary>
/// Sementes da verificação do tamagotchi adulto (DEC-028; crítica, seção 6) para os cenários que dependem da agenda
/// autônoma, escolhidas por simulação do núcleo com a configuração do aplicativo (a chave ligada desde o passo T9) e o
/// simulador de tempo dos testes do núcleo, compilado junto (o mesmo arquivo): a carga, o menu, a invocação e a queda do
/// item, o arraste até ele, o uso e os temporizadores da agenda e da onda, na ordem em que o Buzzy vai recebê-los. A
/// raiz recebe os eventos do usuário em outros instantes (o menu e as esperas do injetor), mas a ordem relativa aos
/// temporizadores é a mesma: a primeira decisão vem bem depois do ITEM_PRESS, que a cancela, e nenhuma fase da onda
/// vira perto da decisão observada. Só aritmética do núcleo: nada aqui abre janela nem injeta input.
/// </summary>
internal static class SementesDoTamagotchi
{
    private static readonly TamanhoDip Sprite = new(128, 128);

    /// <summary>Até onde procurar: as primeiras sementes já servem; o limite só evita uma busca sem fim.</summary>
    private const ulong UltimaSemente = 5000;

    /// <summary>A configuração do núcleo que Aplicacao.Iniciar usa (fonte única no núcleo), com a chave ligada.</summary>
    internal static ConfiguracaoDoNucleo Configuracao => ConfiguracaoDoNucleo.DoAplicativo(Sprite);

    /// <summary>
    /// V8: com a agenda ligada e uma banana já no chão, a primeira decisão (entre 7 e 15 s depois da carga, para dar tempo
    /// de invocar a banana e de ela cair antes) é andar, por pelo menos 2,5 s e sem passar por cima da banana: o
    /// ITEM_PRESS na banana chega com ele andando, e o ponto opaco dela continua livre para o clique.
    /// </summary>
    internal static Previsao? SegurarItemAndando(Topologia topologia)
    {
        for (ulong semente = 1; semente <= UltimaSemente; semente++)
        {
            if (PrimeiraDecisao(topologia, semente) is not { } primeira || primeira < TimeSpan.FromSeconds(7) || primeira > TimeSpan.FromSeconds(15)) continue;
            var sim = new SimuladorDeTempo(Configuracao, semente, topologia);
            sim.Avancar(TimeSpan.FromSeconds(2.5));
            if (Invocar(sim, Item.Banana) is not { } id || !Assentar(sim) || sim.Estado.Itens.PorId(id) is not { } banana) continue;
            sim.Avancar(primeira + TimeSpan.FromSeconds(1) - TimeSpan.FromMilliseconds(sim.AgoraMs), s => s.Estado == Estado.Walking);
            if (sim.Estado.Estado != Estado.Walking) continue;
            // A banana nasce a 8 DIP dele: a janela dele não pode chegar a cobri-la enquanto anda.
            RetanguloPx lugarDaBanana = banana.Lugar.Retangulo;
            bool cobre = false;
            sim.Avancar(TimeSpan.FromSeconds(2.5), s =>
            {
                cobre |= s.Lugar is { } l && l.Retangulo.Intersecta(lugarDaBanana);
                return s.Estado != Estado.Walking;
            });
            if (cobre || sim.Estado.Estado != Estado.Walking) continue;
            return new Previsao(semente, primeira, Invariante($"semente {semente}: primeira decisão em {primeira.TotalSeconds:0.0} s → WALKING, longe da banana"));
        }
        return null;
    }

    /// <summary>
    /// V11: com a agenda ligada, o item (cocaína ou baseado) invocado e solto nele antes da primeira decisão (12 s ou mais
    /// depois da carga), o uso termina e, em até 45 s depois de soltar, ele anda pelo menos 1,5 s na mesma fase da onda,
    /// sem a onda virar a menos de 1,5 s do começo da caminhada. A previsão traz a onda e a velocidade dessa fase.
    /// </summary>
    internal static Previsao? AndarComOnda(Topologia topologia, Item item)
    {
        ConfiguracaoDoNucleo cfg = Configuracao;
        for (ulong semente = 1; semente <= UltimaSemente; semente++)
        {
            if (PrimeiraDecisao(topologia, semente) is not { } primeira || primeira < TimeSpan.FromSeconds(12)) continue;
            var sim = new SimuladorDeTempo(cfg, semente, topologia);
            sim.Avancar(TimeSpan.FromSeconds(2.5));
            if (Invocar(sim, item) is not { } id || !Assentar(sim)) continue;
            sim.Avancar(TimeSpan.FromSeconds(1.5));
            if (!UsarAgora(sim, id)) continue;
            var ondas = new List<double>();
            sim.AoResultado = (_, e, _) => { if (e is ItemEffectTimer) ondas.Add(sim.AgoraMs); };
            sim.Avancar(TimeSpan.FromSeconds(45), s => s.Estado == Estado.Walking);
            if (sim.Estado.Estado != Estado.Walking || sim.Estado.Onda is not { } onda) continue;
            double inicio = sim.AgoraMs;
            int velocidade = Maquina.PerfilDaFase(sim.Estado, cfg)?.Velocidade ?? 100;
            bool mudou = false;
            sim.Avancar(TimeSpan.FromSeconds(1.5), s =>
            {
                mudou |= !Equals(s.Onda, onda);
                return s.Estado != Estado.Walking;
            });
            if (mudou || sim.Estado.Estado != Estado.Walking || ondas.Any(t => Math.Abs(t - inicio) < 1500)) continue;
            return new Previsao(semente, primeira, Invariante($"semente {semente}: anda {inicio / 1000:0.0} s depois da carga com a onda {Descrever(onda)} a {velocidade}%"))
            {
                VelocidadePercentual = velocidade,
                Onda = Descrever(onda),
            };
        }
        return null;
    }

    /// <summary>
    /// V12: com a agenda ligada, a vodka solta nele e, durante o uso dela, a cerveja segurada sobre ele e solta logo que o
    /// uso acaba (a onda de bebedeira chega ao nível 3); a primeira decisão (14 s ou mais depois da carga) não atrapalha,
    /// porque cada ITEM_PRESS a cancela. Depois, em até 45 s, ele anda no pico do nível 3, por pelo menos 4 s, e a
    /// simulação vê ao menos dois recuos de um pixel inteiro (o cambaleio a 120%), sempre no chão.
    /// </summary>
    internal static Previsao? CambalearNoNivel3(Topologia topologia)
    {
        ConfiguracaoDoNucleo cfg = Configuracao;
        for (ulong semente = 1; semente <= UltimaSemente; semente++)
        {
            if (PrimeiraDecisao(topologia, semente) is not { } primeira || primeira < TimeSpan.FromSeconds(14)) continue;
            var sim = new SimuladorDeTempo(cfg, semente, topologia);
            sim.Avancar(TimeSpan.FromSeconds(2.5));
            if (Invocar(sim, Item.Vodka) is not { } vodka || !Assentar(sim)) continue;
            sim.Avancar(TimeSpan.FromSeconds(1.5));
            if (Invocar(sim, Item.Cerveja) is not { } cerveja || !Assentar(sim)) continue;
            sim.Avancar(TimeSpan.FromSeconds(1.5));
            if (!UsarAgora(sim, vodka)) continue;
            sim.Avancar(TimeSpan.FromSeconds(0.5));
            if (sim.Estado.Itens.PorId(cerveja) is not { } itemCerveja || sim.Estado.Lugar is null) continue;
            (PontoPx pressao, PontoPx soltura) = GestoAteEle(sim.Estado, itemCerveja);
            Arrastar(sim, cerveja, pressao, soltura, soltar: false);
            sim.Avancar(TimeSpan.FromSeconds(5), s => s.Estado != Estado.Using);
            if (sim.Estado.Estado != Estado.Idle) continue;
            sim.Aplicar(new ItemDragEnd(cerveja, soltura));
            if (sim.Estado.Estado != Estado.Using || sim.Estado.Onda is not { Tipo: Onda.Bebado, Nivel: 3 }) continue;
            var ondas = new List<double>();
            sim.AoResultado = (_, e, _) => { if (e is ItemEffectTimer) ondas.Add(sim.AgoraMs); };
            sim.Avancar(TimeSpan.FromSeconds(45), s => s.Estado == Estado.Walking);
            if (sim.Estado.Estado != Estado.Walking || sim.Estado.Onda is not { Tipo: Onda.Bebado, Fase: FaseDaOnda.Pico, Nivel: 3 } onda
                || sim.Estado.Lugar is not { } lugar) continue;
            double inicio = sim.AgoraMs;
            int sentido = sim.Estado.Direcao == Direcao.Direita ? 1 : -1, chao = lugar.Ancora.Y, recuos = 0;
            int? anterior = null;
            bool sempreNoChao = true;
            sim.Avancar(TimeSpan.FromSeconds(4), s =>
            {
                if (s.Estado != Estado.Walking || s.Lugar is not { } l) return true;
                if (anterior is { } a && (l.Ancora.X - a) * sentido < 0) recuos++;
                anterior = l.Ancora.X;
                sempreNoChao &= l.Ancora.Y == chao;
                return false;
            });
            if (sim.Estado.Estado != Estado.Walking || !sempreNoChao || recuos < 2 || ondas.Any(t => Math.Abs(t - inicio) < 1500)) continue;
            return new Previsao(semente, primeira, Invariante($"semente {semente}: anda {inicio / 1000:0.0} s depois da carga com a onda {Descrever(onda)}; {recuos} recuos de 1 px em 4 s, sempre no chão"))
            {
                VelocidadePercentual = Maquina.PerfilDaFase(sim.Estado, cfg)?.Velocidade ?? 100,
                Onda = Descrever(onda),
            };
        }
        return null;
    }

    /// <summary>
    /// "Pausar com ele na parede sem estar preso" (correção do núcleo, achado 4; DEC-022 e DEC-024): a primeira decisão
    /// (entre 3 e 14 s) o leva a andar até uma lateral e escalá-la sem foguete; em até 4 s de subida, os pés ficam 200 px
    /// acima do chão; um clique nele vira reação, e a acomodação o deixa agarrado à parede SEM estar preso; pausado, ele
    /// desce até o chão.
    /// </summary>
    internal static Previsao? AgarrarNaParedeSemEstarPreso(Topologia topologia)
    {
        for (ulong semente = 1; semente <= UltimaSemente; semente++)
        {
            if (PrimeiraDecisao(topologia, semente) is not { } primeira || primeira < TimeSpan.FromSeconds(3) || primeira > TimeSpan.FromSeconds(14)) continue;
            var sim = new SimuladorDeTempo(Configuracao, semente, topologia);
            sim.Avancar(primeira + TimeSpan.FromSeconds(1), s => s.Estado == Estado.Walking);
            if (sim.Estado.Estado != Estado.Walking || !sim.Estado.Movimento.QuerEscalar) continue;
            sim.Avancar(TimeSpan.FromSeconds(25), s => s.Estado != Estado.Walking);
            if (sim.Estado.Estado != Estado.Climbing || sim.Estado.Movimento.Foguete || sim.Estado.Lugar is not { } lugar) continue;
            int chao = lugar.Monitor.AreaUtil.Base;
            double subiu = sim.AgoraMs;
            sim.Avancar(TimeSpan.FromSeconds(4), s => s.Estado != Estado.Climbing || s.Lugar!.Ancora.Y <= chao - 200);
            if (sim.Estado.Estado != Estado.Climbing || sim.Estado.Movimento.Foguete || sim.Estado.Lugar is not { } alto || alto.Ancora.Y > chao - 200) continue;
            double tempoDeSubida = sim.AgoraMs - subiu;
            sim.Aplicar(new Press(alto.Retangulo.Centro));
            sim.Aplicar(new Click());
            if (sim.Estado.Estado != Estado.Reacting) continue;
            sim.Avancar(TimeSpan.FromSeconds(2), s => s.Estado == Estado.Climbing);
            if (sim.Estado.Estado != Estado.Climbing || !sim.Estado.Movimento.Agarrado || sim.Estado.PresoPeloUsuario) continue;
            sim.Aplicar(new CmdPauseAutonomy());
            sim.Avancar(TimeSpan.FromSeconds(25), s => s.Estado != Estado.Climbing);
            if (sim.Estado.Estado != Estado.Idle || sim.Estado.Lugar?.Ancora.Y != chao) continue;
            return new Previsao(semente, primeira, Invariante($"semente {semente}: escala a lateral sem foguete; 200 px acima do chão em {tempoDeSubida / 1000:0.0} s de subida; depois do clique, agarrado sem estar preso; pausado, desce"));
        }
        return null;
    }

    // ------------------------------------------------------------------ apoio

    /// <summary>O atraso da primeira decisão da agenda depois da carga (a carga é a do aplicativo, sem posição salva).</summary>
    private static TimeSpan? PrimeiraDecisao(Topologia topologia, ulong semente)
    {
        var nucleo = new Nucleo(Configuracao, semente);
        nucleo.Enfileirar(new Loaded(topologia, null, Preferencias.Padrao));
        return nucleo.Processar().OfType<AgendarDecisao>().LastOrDefault()?.Atraso;
    }

    /// <summary>O botão direito nele pede o menu (CONTEXT_MENU) e o item é invocado; devolve o Id dele, ou nulo.</summary>
    private static int? Invocar(SimuladorDeTempo sim, Item item)
    {
        if (sim.Estado.Lugar is not { } lugar) return null;
        int id = sim.Estado.ProximoIdDeItem;
        sim.Aplicar(new ContextMenu(lugar.Retangulo.Centro));
        sim.Aplicar(new CmdSummonItem(item));
        return sim.Estado.Itens.PorId(id) is null ? null : id;
    }

    /// <summary>Os passos do relógio até nenhum item cair, no máximo 3 s; verdadeiro se todos pararam no chão.</summary>
    private static bool Assentar(SimuladorDeTempo sim)
    {
        sim.Avancar(TimeSpan.FromSeconds(3), s => !s.Itens.AlgumCaindo);
        return !sim.Estado.Itens.AlgumCaindo;
    }

    /// <summary>
    /// O gesto que a verificação faz: o botão pressionado no centro do item e solto no centro do personagem, de modo que o
    /// centro do item fique no centro dele (a verificação pressiona no ponto opaco e solta deslocada do mesmo tanto).
    /// </summary>
    private static (PontoPx Pressao, PontoPx Soltura) GestoAteEle(EstadoDoNucleo s, ItemNoMundo item)
        => (item.Lugar.Retangulo.Centro, s.Lugar!.Retangulo.Centro);

    private static void Arrastar(SimuladorDeTempo sim, int id, PontoPx pressao, PontoPx soltura, bool soltar = true)
    {
        sim.Aplicar(new ItemPress(id, pressao));
        sim.Aplicar(new ItemDragStart(id));
        sim.Aplicar(new ItemDragMove(id, new PontoPx((pressao.X + soltura.X) / 2, (pressao.Y + soltura.Y) / 2)));
        sim.Aplicar(new ItemDragMove(id, soltura));
        if (soltar) sim.Aplicar(new ItemDragEnd(id, soltura));
    }

    /// <summary>Arrasta o item até ele e solta: verdadeiro se ele passou a usá-lo.</summary>
    private static bool UsarAgora(SimuladorDeTempo sim, int id)
    {
        if (sim.Estado.Itens.PorId(id) is not { } item || sim.Estado.Lugar is null) return false;
        (PontoPx pressao, PontoPx soltura) = GestoAteEle(sim.Estado, item);
        Arrastar(sim, id, pressao, soltura);
        return sim.Estado.Estado == Estado.Using;
    }

    /// <summary>A onda como na linha do retrato: Tipo/Fase/Nível.</summary>
    internal static string Descrever(EstadoDaOnda onda) => Invariante($"{onda.Tipo}/{onda.Fase}/{onda.Nivel}");

    private static string Invariante(FormattableString texto) => texto.ToString(CultureInfo.InvariantCulture);
}

using Buzzy.Core.Personagem;
using Buzzy.Visual.Pixel;

namespace Buzzy.App.Apresentacao;

/// <summary>Esticar e achatar de desenho animado nas poses provisórias (toon force, DEC-023).</summary>
internal enum Deformacao
{
    Nenhuma,

    /// <summary>Mais largo e mais baixo: o impacto no chão.</summary>
    Achatado,

    /// <summary>Mais estreito e mais alto: a velocidade, no foguete e na queda rápida.</summary>
    Esticado,
}

/// <summary>Giro de 90° da pose (DEC-025): o esconderijo numa lateral é o de baixo, girado.</summary>
internal enum Giro
{
    Nenhum,

    /// <summary>A borda de baixo do quadro vai para a esquerda: esconderijo na lateral esquerda.</summary>
    Horario,

    /// <summary>A borda de baixo do quadro vai para a direita: esconderijo na lateral direita.</summary>
    AntiHorario,
}

/// <summary>
/// Quadro do sprite pedido à pixel art: pose, espelhamento, expressão, deformação e giro, e, no tamagotchi (DEC-028), o
/// item na mão e a sobreposição da onda. Todos os campos entram na chave do cache de quadros (crítica, C28).
/// </summary>
/// <param name="Pose">Nome da pose em <c>PosesPixel</c> (docs/IDENTIDADE_VISUAL.md, seção 7), achada por <see cref="PosesPixel.PorNome"/>.</param>
/// <param name="Espelhado">As poses de perfil olham para a direita; a esquerda é o espelho.</param>
/// <param name="Expressao">Expressão do rosto, ou nulo para a expressão própria da pose.</param>
/// <param name="Deformacao">Esticar e achatar de desenho animado (toon force).</param>
/// <param name="Giro">Giro de 90° (esconderijo numa lateral).</param>
internal readonly record struct QuadroDoSprite(string Pose, bool Espelhado, string? Expressao, Deformacao Deformacao = Deformacao.Nenhuma, Giro Giro = Giro.Nenhum)
{
    /// <summary>
    /// O item na mão (chave de <see cref="ItensPixel"/>: o nome do valor de <c>Item</c> em minúsculas), só nas poses de
    /// uso; nulo sem item. Nas outras poses, a pixel art não o desenha.
    /// </summary>
    public string? Item { get; init; }

    /// <summary>A sobreposição da onda (crítica, L12), por cima do boneco; o modificador de pose dela vale onde a pose é modificável.</summary>
    public EfeitoVisual Efeito { get; init; }

    /// <summary>
    /// A fase da sobreposição, de 0 a <see cref="EfeitosPixel.Fases"/> − 1: com o relógio ligado, troca a cada 12 passos;
    /// com ele parado, e sem sobreposição, é sempre 0 (DEC-011).
    /// </summary>
    public int Fase { get; init; }
}

/// <summary>O que a pose precisa do movimento em curso (Fase 4, DEC-022 a DEC-024).</summary>
/// <param name="VelocidadeVerticalDip">Velocidade vertical em DIP/s, positiva para baixo.</param>
/// <param name="Quiques">Quiques de borracha já dados nesta queda.</param>
/// <param name="Foguete">Se a escalada é um foguete de borracha.</param>
/// <param name="Agarrado">Parado, agarrado à parede ou ao cipó.</param>
/// <param name="Esconderijo">Em que borda está escondido (DEC-025).</param>
internal readonly record struct Dinamica(double VelocidadeVerticalDip, int Quiques, bool Foguete, bool Agarrado = false,
    LadoDoEsconderijo Esconderijo = LadoDoEsconderijo.Nenhum);

/// <summary>
/// Poses provisórias por estado (TODO.md, Fase 4) e, no tamagotchi (DEC-028), as de uso, as dos gestos da onda e a
/// sobreposição da onda: a apresentação escolhe o quadro pelo retrato do núcleo, sem mudar estado nem posição
/// (ARCHITECTURE.md 2.10). As animações completas, com manifesto, tempos e expressões em camadas, são da Fase 6.
/// </summary>
internal static class PoseDoPersonagem
{
    /// <summary>Passos do relógio lógico por quadro do ciclo de caminhada (60 passos/s → 7,5 quadros/s).</summary>
    private const int PassosPorQuadroAndando = 8;

    /// <summary>Passos por quadro do ciclo de escalada.</summary>
    private const int PassosPorQuadroEscalando = 12;

    /// <summary>Passos em que o pulo ainda mostra o impulso antes do voo.</summary>
    private const int PassosDoImpulso = 6;

    /// <summary>Passos em que o impacto (pouso ou quique) aparece achatado.</summary>
    internal const int PassosDoAchatamento = 5;

    /// <summary>A partir desta velocidade vertical, em DIP/s, o corpo aparece esticado.</summary>
    internal const double VelocidadeDoEsticamento = 700;

    /// <summary>Passos por quadro do balanço no cipó (DEC-024).</summary>
    private const int PassosPorQuadroNoCipo = 10;

    /// <summary>O balanço no cipó vai e volta: esquerda, meio, direita, meio.</summary>
    private static readonly int[] BalancoDoCipo = [1, 2, 3, 2];

    /// <summary>Quem está parado, agarrado à parede: o primeiro quadro da escalada, virado para ela.</summary>
    private const string PoseAgarradoNaParede = "escalando-1";

    /// <summary>Quem está parado, agarrado ao cipó: o quadro do meio do balanço.</summary>
    private const string PoseAgarradoNoCipo = "cipo-2";

    /// <summary>Escondido atrás de uma borda (DEC-025): só a cabeça e as mãos.</summary>
    private const string PoseEscondido = "escondido";

    /// <summary>
    /// Passos por fase da sobreposição da onda com o relógio ligado: 5 trocas por segundo a 60 passos por segundo
    /// (desenho da arte, 4.8).
    /// </summary>
    internal const int PassosPorFaseDaSobreposicao = 12;

    /// <summary>Passos por quadro da tremedeira (L11): o corpo vai e volta 1 pixel, 7,5 vezes por segundo.</summary>
    internal const int PassosPorQuadroDaTremedeira = 4;

    /// <summary>A cara da pose da tremedeira, que o quadro do parado repete: na alternância, só o corpo treme.</summary>
    private static readonly string? CaraDaTremedeira = PosesPixel.PorNome(NomeDoGesto(Gesto.Tremedeira))?.Expressao;

    /// <summary>
    /// O quadro do sprite para o retrato, sem mudar estado nem posição (ARCHITECTURE.md 2.10): a pose pelo estado, pela
    /// dinâmica e, no tamagotchi (DEC-028), pelo uso e pelo gesto da onda; e, por cima de qualquer pose, a sobreposição da
    /// onda da frente (crítica, L12), na fase do relógio. As caras de efeito e a emoção dominante chegam pela expressão do
    /// retrato, nas poses que mostram a cara dele (crítica, F9).
    /// </summary>
    internal static QuadroDoSprite Escolher(Retrato r, long passosNoEstado, Dinamica dinamica = default)
    {
        ArgumentNullException.ThrowIfNull(r);
        QuadroDoSprite quadro = Pose(r, passosNoEstado, dinamica);
        EfeitoVisual efeito = r.Onda is { } onda ? SobreposicaoDaOnda(onda.Tipo) : EfeitoVisual.Nenhum;
        // Sem sobreposição, a fase fica em 0: o cache não guarda o mesmo desenho uma vez por fase.
        return efeito == EfeitoVisual.Nenhum ? quadro : quadro with { Efeito = efeito, Fase = FaseDaSobreposicao(r.RelogioAtivo, passosNoEstado) };
    }

    /// <summary>
    /// A sobreposição de cada onda (crítica, L12): Bebado, bolhas; Chapado, fumaça; Eletrico, brilhos; Tonto,
    /// estrelinhas; Euforico, corações; Viajando, cores. Satisfeito, Alegre, Relaxado e Ligado só mudam a cara e o jeito.
    /// </summary>
    internal static EfeitoVisual SobreposicaoDaOnda(Onda onda) => onda switch
    {
        Onda.Bebado => EfeitoVisual.Bolhas,
        Onda.Chapado => EfeitoVisual.Fumaca,
        Onda.Eletrico => EfeitoVisual.Brilhos,
        Onda.Tonto => EfeitoVisual.Estrelinhas,
        Onda.Euforico => EfeitoVisual.Coracoes,
        Onda.Viajando => EfeitoVisual.Cores,
        _ => EfeitoVisual.Nenhum,
    };

    /// <summary>
    /// A fase da sobreposição: com o relógio ligado, (passos no estado / 12) % 3; com ele parado, sempre a 0, a fase
    /// parada (DEC-011): sem relógio, nada no sprite muda sozinho.
    /// </summary>
    internal static int FaseDaSobreposicao(bool relogioLigado, long passosNoEstado)
        => relogioLigado ? (int)(Math.Max(0, passosNoEstado) / PassosPorFaseDaSobreposicao % EfeitosPixel.Fases) : 0;

    private static QuadroDoSprite Pose(Retrato r, long passosNoEstado, Dinamica dinamica)
    {
        bool esquerda = r.Direcao == Direcao.Esquerda;
        string expressao = NomeDaExpressao(r.Expressao);
        Deformacao rapido = Math.Abs(dinamica.VelocidadeVerticalDip) >= VelocidadeDoEsticamento ? Deformacao.Esticado : Deformacao.Nenhuma;
        if (r.Estado == Estado.Using && r.Uso is { } uso) return DeUso(uso, r.PassoDoUso, esquerda, expressao, dinamica);
        // Escondido (DEC-025): no esconderijo, e também na reação e no pressionar de quem continua
        // escondido, só a cabeça e as mãos aparecem; o corpo nunca surge de relance.
        if (dinamica.Esconderijo != LadoDoEsconderijo.Nenhum && r.Estado is Estado.Peeking or Estado.Reacting or Estado.Pressed)
        {
            string? cara = r.Estado == Estado.Pressed ? "surpreso" : r.Expressao == Expressao.Neutro ? null : expressao;
            return new(PoseEscondido, false, cara, Deformacao.Nenhuma, GiroDoEsconderijo(dinamica.Esconderijo));
        }
        return r.Estado switch
        {
            Estado.Walking => new($"andando-{1 + (int)(passosNoEstado / PassosPorQuadroAndando % 4)}", esquerda, expressao),
            // Toon force: o foguete de borracha sobe esticado, com o braço para cima.
            Estado.Climbing when dinamica.Foguete => new("impulso", esquerda, null, Deformacao.Esticado),
            // Na parede, o macaquinho olha para ela: parede da direita sem espelho, da esquerda espelhada.
            Estado.Climbing when dinamica.Agarrado => new(PoseAgarradoNaParede, esquerda, null),
            Estado.Climbing => new($"escalando-{1 + (int)(passosNoEstado / PassosPorQuadroEscalando % 2)}", esquerda, null),
            // Na borda de cima, pendurado num cipó (DEC-024): parado, o quadro do meio; andando, balança.
            // Com a cara neutra, fica a da pose: rindo no cipó.
            Estado.Hanging when dinamica.Agarrado => new(PoseAgarradoNoCipo, esquerda, r.Expressao == Expressao.Neutro ? null : expressao),
            Estado.Hanging => new($"cipo-{BalancoDoCipo[(int)(passosNoEstado / PassosPorQuadroNoCipo % BalancoDoCipo.Length)]}", esquerda,
                r.Expressao == Expressao.Neutro ? null : expressao),
            // Toon force: o quique começa achatado no chão e sobe esticado.
            Estado.Jumping when dinamica.Quiques > 0 && passosNoEstado < PassosDoAchatamento => new("pousando", false, null, Deformacao.Achatado),
            Estado.Jumping when dinamica.Quiques > 0 && rapido == Deformacao.Esticado => new("impulso", esquerda, null, Deformacao.Esticado),
            Estado.Jumping when dinamica.Quiques > 0 => new("no-ar", esquerda, null),
            Estado.Jumping => new(passosNoEstado < PassosDoImpulso ? "impulso" : "no-ar", esquerda, null, passosNoEstado < PassosDoImpulso ? Deformacao.Nenhuma : rapido),
            Estado.Falling => new("caindo", false, null, rapido),
            Estado.Landing => new("pousando", false, null, passosNoEstado < PassosDoAchatamento ? Deformacao.Achatado : Deformacao.Nenhuma),
            Estado.Resting => new(r.Expressao is Expressao.Dormindo ? "dormindo" : "sentado", false, null),
            Estado.Pressed or Estado.Dragging => new("segurado", false, null),
            Estado.Reacting => new("reagindo", false, null),
            Estado.Idle => r.Gesto switch
            {
                Gesto.Espiar => new("espiando", false, null),
                Gesto.OlharAoRedor => new("olhando", false, null),
                Gesto.Cocar => new("cocando", false, null),
                Gesto.Espreguicar => new("espreguicando", false, null),
                Gesto.Brincar => new("brincando", false, null),
                // Gestos da onda (DEC-028): as poses provisórias da arte (crítica, L11), com a cara delas, como os outros
                // gestos. A tremedeira é o parado deslocado 1 pixel: os dois quadros se alternam, com a mesma cara.
                Gesto.Tremedeira when passosNoEstado / PassosPorQuadroDaTremedeira % 2 != 0 => new("parado", false, CaraDaTremedeira),
                Gesto.Soluco or Gesto.Danca or Gesto.Gargalhada or Gesto.Espirro or Gesto.Tosse or Gesto.Tremedeira => new(NomeDoGesto(r.Gesto), false, null),
                _ => new("parado", false, expressao),
            },
            _ => new("parado", false, expressao),
        };
    }

    /// <summary>
    /// USING (DEC-028). No chão, o quadro da animação do verbo no passo do uso, com o item na mão e a cara da própria
    /// pose (crítica, C10): de frente, sem espelho, como as outras poses de frente. Na parede, no cipó e no esconderijo,
    /// que ainda não têm pose de uso (C25), a pose de quem está agarrado ou espia ali, com a cara do item durante o uso
    /// (a do retrato: o núcleo a fixa do começo ao fim do uso) e sem objeto na mão.
    /// </summary>
    private static QuadroDoSprite DeUso(Uso uso, int passoDoUso, bool esquerda, string expressao, Dinamica dinamica) => uso.Apoio switch
    {
        ApoioDoUso.Parede => new(PoseAgarradoNaParede, esquerda, expressao),
        ApoioDoUso.Cipo => new(PoseAgarradoNoCipo, esquerda, expressao),
        ApoioDoUso.Esconderijo => new(PoseEscondido, false, expressao, Deformacao.Nenhuma, GiroDoEsconderijo(dinamica.Esconderijo)),
        _ => new(UsosPixel.Quadro(VerboDaArte(uso.Verbo), passoDoUso).Nome, false, null) { Item = NomeDoItem(uso.Item) },
    };

    /// <summary>O esconderijo numa lateral é o de baixo, girado: a borda de baixo do quadro vai para a borda da tela (DEC-025).</summary>
    private static Giro GiroDoEsconderijo(LadoDoEsconderijo lado) => lado switch
    {
        LadoDoEsconderijo.Esquerda => Giro.Horario,
        LadoDoEsconderijo.Direita => Giro.AntiHorario,
        _ => Giro.Nenhum,
    };

    /// <summary>Nome da expressão na pixel art: o mesmo do núcleo, em minúsculas (IDENTIDADE_VISUAL.md, seção 6).</summary>
    internal static string NomeDaExpressao(Expressao e) => e.ToString().ToLowerInvariant();

    /// <summary>
    /// Chave do item na pixel art (<see cref="ItensPixel"/>): o nome do valor de <see cref="Item"/> em minúsculas
    /// (crítica, C7), como nas expressões: <c>lancaperfume</c>, <c>md</c>.
    /// </summary>
    internal static string NomeDoItem(Item item) => item.ToString().ToLowerInvariant();

    /// <summary>Nome da pose de um gesto da onda em <see cref="PosesPixel.DosGestos"/>: o nome do gesto em minúsculas (crítica, L11).</summary>
    internal static string NomeDoGesto(Gesto gesto) => gesto.ToString().ToLowerInvariant();

    /// <summary>
    /// O verbo da arte (<see cref="Verbo"/>) para o verbo de uso do núcleo: os mesmos nomes, na mesma ordem
    /// (o teste do contrato entre o núcleo e a arte amarra os dois enums).
    /// </summary>
    internal static Verbo VerboDaArte(VerboDeUso verbo) => verbo switch
    {
        VerboDeUso.Comer => Verbo.Comer,
        VerboDeUso.Beber => Verbo.Beber,
        VerboDeUso.Fumar => Verbo.Fumar,
        VerboDeUso.Cheirar => Verbo.Cheirar,
        VerboDeUso.Engolir => Verbo.Engolir,
        VerboDeUso.Inalar => Verbo.Inalar,
        _ => throw new ArgumentOutOfRangeException(nameof(verbo), verbo, "Verbo de uso sem animação na arte."),
    };
}

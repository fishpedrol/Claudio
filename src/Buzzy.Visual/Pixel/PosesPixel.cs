namespace Buzzy.Visual.Pixel;

/// <summary>
/// Poses-chave da pixel art, uma por estado ou gesto de ARCHITECTURE.md 2.6 (as mesmas da
/// identidade anterior). Ângulos dos membros em graus na tela: 0 para baixo, 90 para a direita,
/// −90 para a esquerda, 180 para cima.
/// </summary>
public static class PosesPixel
{
    public static readonly IReadOnlyList<PosePixel> Todas =
    [
        new PosePixel
        {
            Nome = "parado",
            Estado = "IDLE",
            BracoA = new(-24, -8),
            BracoB = new(24, 8),
        },
        new PosePixel
        {
            Nome = "reagindo",
            Estado = "REACTING",
            QuadrilY = 49,
            BracoA = new(-128, -158),
            BracoB = new(128, 158),
            PernaA = new(-18, 12),
            PernaB = new(14, -4),
            Cauda = Cauda.Alta,
            Expressao = "rindo",
        },
        new PosePixel
        {
            Nome = "caindo",
            Estado = "FALLING",
            QuadrilY = 49,
            BracoA = new(-145, -170),
            BracoB = new(145, 170),
            PernaA = new(-22, -8),
            PernaB = new(22, 8),
            Cauda = Cauda.Alta,
            Expressao = "assustado",
        },
        new PosePixel
        {
            Nome = "pousando",
            Estado = "LANDING",
            QuadrilY = 51.5,
            BracoA = new(-70, -50),
            BracoB = new(70, 50),
            PernaA = new(-42, 22),
            PernaB = new(42, -22),
            Cauda = Cauda.Caida,
            Expressao = "surpreso",
        },
        new PosePixel
        {
            Nome = "pendurado",
            Estado = "HANGING",
            QuadrilY = 40,
            CabecaDescida = 11,
            BracoA = new(-150, -165),
            BracoB = new(150, 165),
            MaoA = Mao.Fechada,
            MaoB = Mao.Fechada,
            PernaA = new(-8, 0),
            PernaB = new(18, 34),
            Cauda = Cauda.Caida,
            Expressao = "feliz",
        },
        new PosePixel
        {
            Nome = "sentado",
            Estado = "RESTING",
            QuadrilY = 56,
            BracoA = new(-20, 22),
            BracoB = new(20, -22),
            PernaA = new(-58, 44),
            PernaB = new(58, -44),
            Expressao = "neutro",
        },
        new PosePixel
        {
            Nome = "dormindo",
            Estado = "RESTING",
            QuadrilY = 56,
            Tronco = 4,
            Cabeca = 16,
            CabecaDescida = 2,
            BracoA = new(-24, 62),
            BracoB = new(24, -62),
            PernaA = new(-58, 44),
            PernaB = new(58, -44),
            Cauda = Cauda.Caida,
            Expressao = "dormindo",
        },
        new PosePixel
        {
            Nome = "segurado",
            Estado = "PRESSED/DRAGGING",
            QuadrilY = 49,
            BracoA = new(-12, -2),
            BracoB = new(12, 2),
            PernaA = new(-8, -2),
            PernaB = new(8, 2),
            Cauda = Cauda.Caida,
            Expressao = "surpreso",
        },
        new PosePixel
        {
            Nome = "olhando",
            Estado = "gesto: olhar ao redor",
            Cabeca = -10,
            BracoA = new(-24, -8),
            BracoB = new(160, -95),
            Expressao = "curioso",
        },
        new PosePixel
        {
            Nome = "cocando",
            Estado = "gesto: coçar-se",
            Cabeca = 8,
            BracoA = new(-24, -8),
            BracoB = new(168, -120),
            Expressao = "pensativo",
        },
        new PosePixel
        {
            Nome = "espreguicando",
            Estado = "gesto: espreguiçar-se",
            QuadrilY = 48.5,
            BracoA = new(-146, -160),
            BracoB = new(146, 160),
            Cauda = Cauda.Alta,
            Expressao = "bocejando",
        },
        new PosePixel
        {
            Nome = "espiando",
            Estado = "gesto: espiar",
            QuadrilY = 78,
            Borda = 58,
            BracoA = new(-80, 150),
            BracoB = new(80, -150),
            MaoA = Mao.Fechada,
            MaoB = Mao.Fechada,
            Cauda = Cauda.Alta,
            Expressao = "curioso",
        },
        new PosePixel
        {
            Nome = "brincando",
            Estado = "gesto: brincar",
            QuadrilY = 48,
            Cabeca = -8,
            BracoA = new(-58, -150),
            BracoB = new(62, 24),
            PernaA = new(-4, 0),
            PernaB = new(44, -24),
            Cauda = Cauda.Alta,
            Expressao = "travesso",
        },
        Andando("andando-1", perto: new(28, 8), longe: new(-28, -12), bracoPerto: new(-26, -14), bracoLonge: new(24, 34), y: 48.5),
        Andando("andando-2", perto: new(4, 0), longe: new(-14, -62), bracoPerto: new(-6, 0), bracoLonge: new(6, 12), y: 47.5),
        Andando("andando-3", perto: new(-28, -12), longe: new(28, 8), bracoPerto: new(24, 34), bracoLonge: new(-26, -14), y: 48.5),
        Andando("andando-4", perto: new(-14, -62), longe: new(4, 0), bracoPerto: new(6, 12), bracoLonge: new(-6, 0), y: 47.5),
        new PosePixel
        {
            Nome = "impulso",
            Estado = "JUMPING (antecipação)",
            Vista = Vista.Perfil,
            QuadrilX = 30,
            QuadrilY = 52.5,
            Tronco = 26,
            BracoA = new(-58, -30),
            BracoB = new(-66, -40),
            PernaA = new(58, -18),
            PernaB = new(66, -14),
            Cauda = Cauda.PerfilAlta,
            Expressao = "determinado",
        },
        new PosePixel
        {
            Nome = "no-ar",
            Estado = "JUMPING (extensão)",
            Vista = Vista.Perfil,
            QuadrilX = 29,
            QuadrilY = 49,
            Tronco = 12,
            BracoA = new(128, 112),
            BracoB = new(138, 120),
            PernaA = new(-28, -44),
            PernaB = new(-14, -30),
            Cauda = Cauda.PerfilAlta,
            Expressao = "empolgado",
        },
        new PosePixel
        {
            Nome = "escalando-1",
            Estado = "CLIMBING",
            Vista = Vista.Perfil,
            QuadrilX = 42,
            QuadrilY = 47,
            Tronco = 4,
            BracoA = new(128, 150),
            BracoB = new(158, 176),
            MaoA = Mao.Fechada,
            MaoB = Mao.Fechada,
            PernaA = new(64, 8),
            PernaB = new(112, 22),
            Cauda = Cauda.PerfilCaida,
            Expressao = "determinado",
        },
        new PosePixel
        {
            Nome = "escalando-2",
            Estado = "CLIMBING",
            Vista = Vista.Perfil,
            QuadrilX = 42,
            QuadrilY = 46,
            Tronco = 4,
            BracoA = new(158, 176),
            BracoB = new(128, 150),
            MaoA = Mao.Fechada,
            MaoB = Mao.Fechada,
            PernaA = new(112, 22),
            PernaB = new(64, 8),
            Cauda = Cauda.PerfilCaida,
            Expressao = "determinado",
        },
    ];

    private static PosePixel Andando(string nome, Membro perto, Membro longe, Membro bracoPerto, Membro bracoLonge, double y) => new()
    {
        Nome = nome,
        Estado = "WALKING",
        Vista = Vista.Perfil,
        QuadrilX = 30,
        QuadrilY = y,
        Tronco = 5,
        BracoA = bracoLonge,
        BracoB = bracoPerto,
        PernaA = longe,
        PernaB = perto,
        Cauda = Cauda.Perfil,
    };
}

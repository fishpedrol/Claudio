namespace Buzzy.Core.Personagem;

/// <summary>
/// Parâmetros fixos do núcleo numa execução. As capacidades que chegam em fases posteriores
/// (queda física, painel de energia) ficam desligadas até lá, sem mudar a tabela de transições.
/// </summary>
public sealed record ConfiguracaoDoNucleo
{
    /// <summary>Tamanho lógico do sprite em DIPs (ARCHITECTURE.md 2.4).</summary>
    public TamanhoDip Tamanho { get; init; } = new(128, 128);

    /// <summary>Passos do relógio lógico por segundo (passo fixo proposto de 1/60 s, ARCHITECTURE.md 2.9).</summary>
    public int PassosPorSegundo { get; init; } = 60;

    /// <summary>Duração da reação a um clique, em passos.</summary>
    public int PassosDaReacao { get; init; } = 36;

    /// <summary>Duração do pouso, em passos.</summary>
    public int PassosDoPouso { get; init; } = 12;

    /// <summary>
    /// Intervalo de acomodação (ARCHITECTURE.md 2.6 e 2.7): tempo mínimo entre o fim de uma
    /// interação do usuário e a próxima decisão autônoma.
    /// </summary>
    public TimeSpan IntervaloDeAcomodacao { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Sem apoio depois de acomodar: verdadeiro vai para <see cref="Estado.Falling"/> (Fase 4);
    /// falso prende a âncora no chão da área útil, como a Fase 3 pede antes da queda animada.
    /// </summary>
    public bool QuedaFisica { get; init; }

    /// <summary>
    /// Se o passo do relógio move o personagem pelas superfícies (Fase 4, DEC-022): andar, escalar,
    /// pendurar-se, pular e cair. Desligado, os estados de movimento só mudam pelos sinais de
    /// movimento, como na Fase 2.
    /// </summary>
    public bool Movimento { get; init; }

    /// <summary>Velocidades e gravidade do movimento (iguais em todos os níveis de energia).</summary>
    public ParametrosDeMovimento Fisica { get; init; } = new();

    /// <summary>Se o painel de energia existe (Fase 8). Antes, o clique duplo só produz reação.</summary>
    public bool PainelDeEnergiaDisponivel { get; init; }

    /// <summary>
    /// Se o clique duplo alterna o esconderijo (DEC-025, pedido do usuário): esconde o personagem
    /// atrás da borda mais próxima, só com a cabeça e as mãos para fora, e o tira de lá no clique
    /// duplo seguinte. Ligado, o painel de energia não abre pelo clique duplo; abre pelo menu.
    /// </summary>
    public bool EsconderijoNoCliqueDuplo { get; init; }

    /// <summary>Se a janela de configurações existe (Fase 8).</summary>
    public bool ConfiguracoesDisponiveis { get; init; }

    /// <summary>Ações que a agenda pode escolher. O app liga cada uma quando a fase dela chega.</summary>
    public AcoesAutonomas Acoes { get; init; } = AcoesAutonomas.Todas;

    /// <summary>Perfis por nível de energia.</summary>
    public Func<NivelDeEnergia, PerfilDeEnergia> Perfil { get; init; } = PerfilDeEnergia.Padrao;

    /// <summary>
    /// A configuração que o aplicativo usa hoje. É a fonte única: o app e as simulações dos testes
    /// que escolhem sementes para ele partem daqui, para nunca divergirem.
    /// </summary>
    public static ConfiguracaoDoNucleo DoAplicativo(TamanhoDip tamanho) => new()
    {
        Tamanho = tamanho,
        // Fase 4 (DEC-022 a DEC-025): física, queda animada, todas as ações, esconderijo no clique duplo.
        Acoes = AcoesAutonomas.Todas,
        QuedaFisica = true,
        Movimento = true,
        EsconderijoNoCliqueDuplo = true,
    };
}

/// <summary>
/// Pesos e tempos da agenda autônoma para um nível de energia (DEC-014, ARCHITECTURE.md 2.11).
/// Muda frequência e duração das ações, nunca a física nem as regras de apoio e segurança
/// (invariante 12). Os valores são iniciais e serão calibrados nas Fases 4 e 7.
/// </summary>
public sealed record PerfilDeEnergia(
    NivelDeEnergia Nivel,
    TimeSpan DecisaoMinima,
    TimeSpan DecisaoMaxima,
    TimeSpan DescansoMinimo,
    TimeSpan DescansoMaximo,
    int PassosDoGestoMinimo,
    int PassosDoGestoMaximo,
    int PesoAndar,
    int PesoEscalar,
    int PesoPular,
    int PesoDescansar,
    int PesoGesto,
    int PesoTrocarExpressao)
{
    /// <summary>Distância de uma caminhada autônoma, em DIP (Fase 4).</summary>
    public int DistanciaAndandoMinima { get; init; } = 150;

    public int DistanciaAndandoMaxima { get; init; } = 500;

    /// <summary>Distância horizontal de um pulo, em DIP (Fase 4).</summary>
    public int DistanciaDoPuloMinima { get; init; } = 80;

    public int DistanciaDoPuloMaxima { get; init; } = 200;

    /// <summary>Altura do arco de um pulo, em DIP (Fase 4).</summary>
    public int AlturaDoPuloMinima { get; init; } = 50;

    public int AlturaDoPuloMaxima { get; init; } = 100;

    /// <summary>Tempo na parede até a agenda decidir saltar ou soltar (Fase 4).</summary>
    public TimeSpan TempoNaParedeMinimo { get; init; } = TimeSpan.FromSeconds(15);

    public TimeSpan TempoNaParedeMaximo { get; init; } = TimeSpan.FromSeconds(35);

    /// <summary>Tempo pendurado até a próxima decisão: "por pouco tempo" (Fase 4).</summary>
    public TimeSpan TempoPenduradoMinimo { get; init; } = TimeSpan.FromSeconds(3);

    public TimeSpan TempoPenduradoMaximo { get; init; } = TimeSpan.FromSeconds(8);

    /// <summary>
    /// Em quantas subidas, de cada 100, ele dispara parede acima num foguete de borracha (toon
    /// force, DEC-023). É frequência de uma ação; a velocidade do foguete é a mesma em todo nível.
    /// </summary>
    public int ChanceDoFoguete { get; init; } = 30;

    public static readonly PerfilDeEnergia Baixa = new(
        NivelDeEnergia.Baixa,
        TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(45),
        TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(180),
        60, 120,
        PesoAndar: 2, PesoEscalar: 1, PesoPular: 0, PesoDescansar: 6, PesoGesto: 3, PesoTrocarExpressao: 3)
    {
        DistanciaAndandoMinima = 80,
        DistanciaAndandoMaxima = 250,
        DistanciaDoPuloMinima = 60,
        DistanciaDoPuloMaxima = 120,
        AlturaDoPuloMinima = 30,
        AlturaDoPuloMaxima = 60,
        TempoNaParedeMinimo = TimeSpan.FromSeconds(10),
        TempoNaParedeMaximo = TimeSpan.FromSeconds(20),
        TempoPenduradoMinimo = TimeSpan.FromSeconds(2),
        TempoPenduradoMaximo = TimeSpan.FromSeconds(5),
        ChanceDoFoguete = 10,
    };

    public static readonly PerfilDeEnergia Media = new(
        NivelDeEnergia.Media,
        TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90),
        60, 150,
        PesoAndar: 4, PesoEscalar: 2, PesoPular: 1, PesoDescansar: 3, PesoGesto: 3, PesoTrocarExpressao: 2);

    public static readonly PerfilDeEnergia Alta = new(
        NivelDeEnergia.Alta,
        TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(40),
        90, 180,
        PesoAndar: 5, PesoEscalar: 3, PesoPular: 3, PesoDescansar: 1, PesoGesto: 4, PesoTrocarExpressao: 2)
    {
        DistanciaAndandoMinima = 250,
        DistanciaAndandoMaxima = 900,
        DistanciaDoPuloMinima = 120,
        DistanciaDoPuloMaxima = 320,
        AlturaDoPuloMinima = 70,
        AlturaDoPuloMaxima = 150,
        TempoNaParedeMinimo = TimeSpan.FromSeconds(20),
        TempoNaParedeMaximo = TimeSpan.FromSeconds(45),
        TempoPenduradoMinimo = TimeSpan.FromSeconds(5),
        TempoPenduradoMaximo = TimeSpan.FromSeconds(12),
        ChanceDoFoguete = 50,
    };

    public static PerfilDeEnergia Padrao(NivelDeEnergia nivel) => nivel switch
    {
        NivelDeEnergia.Baixa => Baixa,
        NivelDeEnergia.Media => Media,
        NivelDeEnergia.Alta => Alta,
        _ => throw new ArgumentOutOfRangeException(nameof(nivel), nivel, "Nível de energia desconhecido."),
    };
}

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

    /// <summary>Se o painel de energia existe (Fase 8). Antes, o clique duplo só produz reação.</summary>
    public bool PainelDeEnergiaDisponivel { get; init; }

    /// <summary>Se a janela de configurações existe (Fase 8).</summary>
    public bool ConfiguracoesDisponiveis { get; init; }

    /// <summary>Ações que a agenda pode escolher. O app liga cada uma quando a fase dela chega.</summary>
    public AcoesAutonomas Acoes { get; init; } = AcoesAutonomas.Todas;

    /// <summary>Perfis por nível de energia.</summary>
    public Func<NivelDeEnergia, PerfilDeEnergia> Perfil { get; init; } = PerfilDeEnergia.Padrao;
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
    public static readonly PerfilDeEnergia Baixa = new(
        NivelDeEnergia.Baixa,
        TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(45),
        TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(180),
        60, 120,
        PesoAndar: 2, PesoEscalar: 1, PesoPular: 0, PesoDescansar: 6, PesoGesto: 3, PesoTrocarExpressao: 3);

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
        PesoAndar: 5, PesoEscalar: 3, PesoPular: 3, PesoDescansar: 1, PesoGesto: 4, PesoTrocarExpressao: 2);

    public static PerfilDeEnergia Padrao(NivelDeEnergia nivel) => nivel switch
    {
        NivelDeEnergia.Baixa => Baixa,
        NivelDeEnergia.Media => Media,
        NivelDeEnergia.Alta => Alta,
        _ => throw new ArgumentOutOfRangeException(nameof(nivel), nivel, "Nível de energia desconhecido."),
    };
}

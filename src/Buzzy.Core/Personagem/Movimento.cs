namespace Buzzy.Core.Personagem;

/// <summary>
/// Parâmetros físicos do movimento (ARCHITECTURE.md 2.9), em DIPs e segundos, convertidos pela
/// escala do monitor da âncora. São os mesmos em todos os níveis de energia (invariante 12): a
/// energia muda a frequência e o tamanho das ações (<see cref="PerfilDeEnergia"/>), nunca as
/// velocidades nem a gravidade.
/// </summary>
public sealed record ParametrosDeMovimento
{
    /// <summary>Caminhada no chão, em DIP/s.</summary>
    public double VelocidadeAndando { get; init; } = 90;

    /// <summary>Subida e descida nas paredes laterais, em DIP/s.</summary>
    public double VelocidadeEscalando { get; init; } = 110;

    /// <summary>Deslocamento pendurado na borda superior, em DIP/s.</summary>
    public double VelocidadePendurado { get; init; } = 80;

    /// <summary>Gravidade, em DIP/s².</summary>
    public double Gravidade { get; init; } = 2200;

    /// <summary>Velocidade máxima de queda, em DIP/s.</summary>
    public double VelocidadeMaximaDeQueda { get; init; } = 1500;

    /// <summary>Velocidade horizontal ao saltar da parede ou do teto, em DIP/s.</summary>
    public double ImpulsoDaParede { get; init; } = 220;

    /// <summary>Altura do salto para longe da parede, em DIP.</summary>
    public double AlturaDoPuloDaParede { get; init; } = 40;

    /// <summary>Menor espaço livre, em DIP, para uma caminhada ou um pulo naquela direção; menos que isso, vira.</summary>
    public double EspacoMinimo { get; init; } = 24;

    // Toon force (DEC-023): física de desenho animado, de borracha como o Luffy, igual em todos os
    // níveis de energia. Zero em RestituicaoDoQuique ou em VelocidadeDoFoguete desliga o efeito.

    /// <summary>Quique de borracha: fração da velocidade de impacto devolvida para cima.</summary>
    public double RestituicaoDoQuique { get; init; } = 0.5;

    /// <summary>Menor velocidade de impacto que faz quicar, em DIP/s; abaixo dela, pousa.</summary>
    public double ImpactoMinimoDoQuique { get; init; } = 600;

    /// <summary>Quiques seguidos, no máximo, antes de pousar.</summary>
    public int QuiquesMaximos { get; init; } = 2;

    /// <summary>Fração da velocidade horizontal que sobra a cada quique.</summary>
    public double AtritoDoQuique { get; init; } = 0.7;

    /// <summary>Foguete de borracha: subida disparada parede acima até a borda superior, em DIP/s.</summary>
    public double VelocidadeDoFoguete { get; init; } = 1000;
}

/// <summary>
/// Estado físico do movimento em curso, parte do estado do núcleo. A posição fina da âncora fica
/// em pixels físicos (a janela usa a arredondada, em <see cref="EstadoDoNucleo.Lugar"/>); as
/// velocidades, em pixels físicos por segundo.
/// </summary>
/// <param name="X">Âncora fina, horizontal.</param>
/// <param name="Y">Âncora fina, vertical.</param>
/// <param name="VX">Velocidade horizontal no ar.</param>
/// <param name="VY">Velocidade vertical no ar (positiva para baixo).</param>
/// <param name="Restante">Caminhada: pixels que faltam para o fim do percurso.</param>
/// <param name="SentidoVertical">Escalada: −1 sobe, +1 desce.</param>
/// <param name="QuerEscalar">Caminhada: anda até a parede para escalar.</param>
public sealed record EstadoDoMovimento(double X, double Y, double VX, double VY, double Restante, int SentidoVertical, bool QuerEscalar)
{
    public static readonly EstadoDoMovimento Nenhum = new(0, 0, 0, 0, 0, -1, false);

    /// <summary>Quiques de borracha já dados nesta queda (toon force, DEC-023).</summary>
    public int Quiques { get; init; }

    /// <summary>Escalada: sobe disparado, num foguete de borracha (toon force, DEC-023).</summary>
    public bool Foguete { get; init; }
}

/// <summary>
/// Superfícies do monitor em que o personagem está (ARCHITECTURE.md 2.5), já como limites da
/// âncora (centro da base do sprite), com o sprite inteiro na área útil. Com a toon force
/// (DEC-023), as duas laterais são escaláveis, mesmo a que encosta em outro monitor: a borda da
/// tela vira parede para o macaquinho.
/// </summary>
/// <param name="Esquerda">Menor X da âncora: o sprite encosta na lateral esquerda da área útil.</param>
/// <param name="Direita">Maior X da âncora: o sprite encosta na lateral direita.</param>
/// <param name="Chao">Y da âncora com os pés no chão (a borda de baixo da área útil).</param>
/// <param name="Teto">Y da âncora com o topo do sprite na borda de cima (pendurado).</param>
/// <param name="PassagemEsquerda">Se outro monitor encosta na lateral esquerda (passagem, Fase 5); senão, é parede.</param>
/// <param name="PassagemDireita">Se outro monitor encosta na lateral direita.</param>
public readonly record struct Superficies(int Esquerda, int Direita, int Chao, int Teto, bool PassagemEsquerda, bool PassagemDireita)
{
    /// <summary>
    /// Superfícies do monitor para um sprite do tamanho dado. Uma lateral é passagem quando a tela
    /// de outro monitor encosta nela, na altura da área útil; senão, é parede. Janelas de outros
    /// aplicativos nunca são superfície (Q-05).
    /// </summary>
    public static Superficies Do(Topologia topologia, MonitorDoDesktop monitor, TamanhoPx tamanho)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        ArgumentNullException.ThrowIfNull(monitor);
        RetanguloPx area = monitor.AreaUtil;
        int metade = tamanho.Largura / 2;
        int esquerda = area.Esquerda + metade;
        int direita = Math.Max(esquerda, area.Direita - (tamanho.Largura - metade));
        int chao = area.Base;
        int teto = Math.Min(chao, area.Topo + tamanho.Altura);
        return new Superficies(esquerda, direita, chao, teto, Encostado(topologia, monitor, -1), Encostado(topologia, monitor, +1));
    }

    /// <summary>
    /// Se a âncora está encostada numa lateral da área útil, parede ou passagem (as duas são
    /// escaláveis com a toon force); devolve o lado (−1 esquerda, +1 direita).
    /// </summary>
    public bool NaLateral(double x, out int lado)
    {
        if (x >= Direita - 0.5) { lado = +1; return true; }
        if (x <= Esquerda + 0.5) { lado = -1; return true; }
        lado = 0;
        return false;
    }

    private static bool Encostado(Topologia topologia, MonitorDoDesktop monitor, int lado)
    {
        foreach (MonitorDoDesktop outro in topologia.Monitores)
        {
            if (ReferenceEquals(outro, monitor) || outro.Chave == monitor.Chave) continue;
            bool encosta = lado < 0 ? outro.Tela.Direita == monitor.Tela.Esquerda : outro.Tela.Esquerda == monitor.Tela.Direita;
            bool naAltura = outro.Tela.Topo < monitor.AreaUtil.Base && outro.Tela.Base > monitor.AreaUtil.Topo;
            if (encosta && naAltura) return true;
        }
        return false;
    }
}

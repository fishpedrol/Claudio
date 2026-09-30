using Buzzy.Core.Personagem;

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

/// <summary>Quadro do sprite pedido à pixel art: pose, espelhamento, expressão, deformação e giro.</summary>
/// <param name="Pose">Nome da pose em <c>PosesPixel</c> (docs/IDENTIDADE_VISUAL.md, seção 7).</param>
/// <param name="Espelhado">As poses de perfil olham para a direita; a esquerda é o espelho.</param>
/// <param name="Expressao">Expressão do rosto, ou nulo para a expressão própria da pose.</param>
/// <param name="Deformacao">Esticar e achatar de desenho animado (toon force).</param>
/// <param name="Giro">Giro de 90° (esconderijo numa lateral).</param>
internal readonly record struct QuadroDoSprite(string Pose, bool Espelhado, string? Expressao, Deformacao Deformacao = Deformacao.Nenhuma, Giro Giro = Giro.Nenhum);

/// <summary>O que a pose precisa do movimento em curso (Fase 4, DEC-022 a DEC-024).</summary>
/// <param name="VelocidadeVerticalDip">Velocidade vertical em DIP/s, positiva para baixo.</param>
/// <param name="Quiques">Quiques de borracha já dados nesta queda.</param>
/// <param name="Foguete">Se a escalada é um foguete de borracha.</param>
/// <param name="Agarrado">Parado, agarrado à parede ou ao cipó.</param>
/// <param name="Esconderijo">Em que borda está escondido (DEC-025).</param>
internal readonly record struct Dinamica(double VelocidadeVerticalDip, int Quiques, bool Foguete, bool Agarrado = false,
    LadoDoEsconderijo Esconderijo = LadoDoEsconderijo.Nenhum);

/// <summary>
/// Poses provisórias por estado (TODO.md, Fase 4): a apresentação escolhe o quadro pelo retrato
/// do núcleo, sem mudar estado nem posição (ARCHITECTURE.md 2.10). As animações completas, com
/// manifesto, tempos e expressões em camadas, são da Fase 6.
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

    internal static QuadroDoSprite Escolher(Retrato r, long passosNoEstado, Dinamica dinamica = default)
    {
        ArgumentNullException.ThrowIfNull(r);
        bool esquerda = r.Direcao == Direcao.Esquerda;
        string? expressao = NomeDaExpressao(r.Expressao);
        Deformacao rapido = Math.Abs(dinamica.VelocidadeVerticalDip) >= VelocidadeDoEsticamento ? Deformacao.Esticado : Deformacao.Nenhuma;
        // Escondido (DEC-025): no esconderijo, e também na reação e no pressionar de quem continua
        // escondido, só a cabeça e as mãos aparecem; o corpo nunca surge de relance.
        if (dinamica.Esconderijo != LadoDoEsconderijo.Nenhum && r.Estado is Estado.Peeking or Estado.Reacting or Estado.Pressed)
        {
            Giro giro = dinamica.Esconderijo switch
            {
                LadoDoEsconderijo.Esquerda => Giro.Horario,
                LadoDoEsconderijo.Direita => Giro.AntiHorario,
                _ => Giro.Nenhum,
            };
            string? cara = r.Estado == Estado.Pressed ? "surpreso" : r.Expressao == Expressao.Neutro ? null : expressao;
            return new("escondido", false, cara, Deformacao.Nenhuma, giro);
        }
        return r.Estado switch
        {
            Estado.Walking => new($"andando-{1 + (int)(passosNoEstado / PassosPorQuadroAndando % 4)}", esquerda, expressao),
            // Toon force: o foguete de borracha sobe esticado, com o braço para cima.
            Estado.Climbing when dinamica.Foguete => new("impulso", esquerda, null, Deformacao.Esticado),
            // Na parede, o macaquinho olha para ela: parede da direita sem espelho, da esquerda espelhada.
            Estado.Climbing when dinamica.Agarrado => new("escalando-1", esquerda, null),
            Estado.Climbing => new($"escalando-{1 + (int)(passosNoEstado / PassosPorQuadroEscalando % 2)}", esquerda, null),
            // Na borda de cima, pendurado num cipó (DEC-024): parado, o quadro do meio; andando, balança.
            // Com a cara neutra, fica a da pose: rindo no cipó.
            Estado.Hanging when dinamica.Agarrado => new("cipo-2", esquerda, r.Expressao == Expressao.Neutro ? null : expressao),
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
                _ => new("parado", false, expressao),
            },
            _ => new("parado", false, expressao),
        };
    }

    /// <summary>Nome da expressão na pixel art: o mesmo do núcleo, em minúsculas (IDENTIDADE_VISUAL.md, seção 6).</summary>
    internal static string NomeDaExpressao(Expressao e) => e.ToString().ToLowerInvariant();
}

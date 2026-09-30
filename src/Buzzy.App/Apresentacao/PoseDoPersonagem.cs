using Buzzy.Core.Personagem;

namespace Buzzy.App.Apresentacao;

/// <summary>Quadro do sprite pedido à pixel art: pose, espelhamento e expressão.</summary>
/// <param name="Pose">Nome da pose em <c>PosesPixel</c> (docs/IDENTIDADE_VISUAL.md, seção 7).</param>
/// <param name="Espelhado">As poses de perfil olham para a direita; a esquerda é o espelho.</param>
/// <param name="Expressao">Expressão do rosto, ou nulo para a expressão própria da pose.</param>
internal readonly record struct QuadroDoSprite(string Pose, bool Espelhado, string? Expressao);

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

    internal static QuadroDoSprite Escolher(Retrato r, long passosNoEstado)
    {
        ArgumentNullException.ThrowIfNull(r);
        bool esquerda = r.Direcao == Direcao.Esquerda;
        string? expressao = NomeDaExpressao(r.Expressao);
        return r.Estado switch
        {
            Estado.Walking => new($"andando-{1 + (int)(passosNoEstado / PassosPorQuadroAndando % 4)}", esquerda, expressao),
            // Na parede, o macaquinho olha para ela: parede da direita sem espelho, da esquerda espelhada.
            Estado.Climbing => new($"escalando-{1 + (int)(passosNoEstado / PassosPorQuadroEscalando % 2)}", esquerda, null),
            Estado.Hanging => new("pendurado", false, null),
            Estado.Jumping => new(passosNoEstado < PassosDoImpulso ? "impulso" : "no-ar", esquerda, null),
            Estado.Falling => new("caindo", false, null),
            Estado.Landing => new("pousando", false, null),
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

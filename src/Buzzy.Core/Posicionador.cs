namespace Buzzy.Core;

/// <summary>
/// Onde o personagem está, de um jeito que sobrevive a mudanças de topologia
/// (ARCHITECTURE.md 2.8): a chave do monitor, a posição relativa da âncora dentro da área
/// útil desse monitor (frações de 0 a 1) e a última âncora absoluta, usada como reserva
/// quando o monitor some.
/// </summary>
public sealed record PosicaoDoPersonagem(string ChaveMonitor, double FracaoX, double FracaoY, PontoPx AncoraAbsoluta);

/// <summary>Resultado de posicionar o sprite: monitor, âncora, tamanho físico e retângulo da janela.</summary>
public sealed record Posicionamento(MonitorDoDesktop Monitor, PontoPx Ancora, TamanhoPx Tamanho, RetanguloPx Retangulo);

/// <summary>
/// Posiciona o sprite estático da Fase 1. A âncora é o ponto entre os pés: o centro da
/// borda inferior do sprite. O sprite sempre termina inteiro dentro da área útil de um
/// monitor presente, quando cabe nela.
/// </summary>
public static class Posicionador
{
    /// <summary>
    /// Posição horizontal inicial da âncora, como fração da largura da área útil do monitor
    /// principal: perto do canto inferior direito, onde o Buzzy atrapalha menos. Provisória
    /// até a Fase 5, quando a posição escolhida pelo usuário passa a ser persistida.
    /// </summary>
    public const double FracaoInicialX = 0.85;

    /// <summary>
    /// Retângulo do sprite com a âncora dada. A âncora fica na coluna
    /// <c>Esquerda + Largura / 2</c> e na borda inferior exclusiva (<c>Base</c>): com a âncora
    /// no chão da área útil, a última linha do sprite é a última linha da área útil.
    /// </summary>
    public static RetanguloPx RetanguloDoSprite(PontoPx ancora, TamanhoPx tamanho)
    {
        int esquerda = ancora.X - tamanho.Largura / 2;
        return new RetanguloPx(esquerda, ancora.Y - tamanho.Altura, esquerda + tamanho.Largura, ancora.Y);
    }

    /// <summary>
    /// Âncora mais próxima de <paramref name="ancora"/> com o sprite inteiro dentro da área
    /// útil. Se o sprite for mais largo que a área, fica centralizado nela; se for mais alto,
    /// fica com os pés no chão da área útil e a cabeça passa do topo.
    /// </summary>
    public static PontoPx PrenderNaAreaUtil(PontoPx ancora, TamanhoPx tamanho, RetanguloPx areaUtil)
    {
        if (areaUtil.Vazio) throw new ArgumentException("Área útil vazia.", nameof(areaUtil));
        if (tamanho.Largura <= 0 || tamanho.Altura <= 0) throw new ArgumentException($"Tamanho inválido: {tamanho}.", nameof(tamanho));

        int aEsquerda = tamanho.Largura / 2;
        int aDireita = tamanho.Largura - aEsquerda;

        int x = tamanho.Largura <= areaUtil.Largura
            ? Math.Clamp(ancora.X, areaUtil.Esquerda + aEsquerda, areaUtil.Direita - aDireita)
            : areaUtil.Esquerda + (areaUtil.Largura - tamanho.Largura) / 2 + aEsquerda;

        int y = tamanho.Altura <= areaUtil.Altura
            ? Math.Clamp(ancora.Y, areaUtil.Topo + tamanho.Altura, areaUtil.Base)
            : areaUtil.Base;

        return new PontoPx(x, y);
    }

    /// <summary>Posição inicial: no chão da área útil do monitor principal, em <see cref="FracaoInicialX"/>.</summary>
    public static Posicionamento Inicial(Topologia topologia, TamanhoDip tamanho)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        return NoMonitor(topologia.Principal, FracaoInicialX, 1.0, tamanho);
    }

    /// <summary>
    /// Posiciona a âncora na fração dada da área útil do monitor e prende o sprite nela.
    /// Frações fora de [0, 1] são presas ao intervalo; NaN vira 0,5.
    /// </summary>
    public static Posicionamento NoMonitor(MonitorDoDesktop monitor, double fracaoX, double fracaoY, TamanhoDip tamanho)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        RetanguloPx area = monitor.AreaUtil;
        TamanhoPx fisico = tamanho.ParaPixels(monitor.Dpi);

        var desejada = new PontoPx(
            area.Esquerda + (int)Math.Round(Fracao(fracaoX) * area.Largura, MidpointRounding.AwayFromZero),
            area.Topo + (int)Math.Round(Fracao(fracaoY) * area.Altura, MidpointRounding.AwayFromZero));

        PontoPx ancora = PrenderNaAreaUtil(desejada, fisico, area);
        return new Posicionamento(monitor, ancora, fisico, RetanguloDoSprite(ancora, fisico));
    }

    /// <summary>Descreve um posicionamento como posição relativa à área útil do seu monitor.</summary>
    public static PosicaoDoPersonagem Descrever(Posicionamento p)
    {
        ArgumentNullException.ThrowIfNull(p);
        RetanguloPx area = p.Monitor.AreaUtil;
        return new PosicaoDoPersonagem(
            p.Monitor.Chave,
            (p.Ancora.X - area.Esquerda) / (double)area.Largura,
            (p.Ancora.Y - area.Topo) / (double)area.Altura,
            p.Ancora);
    }

    /// <summary>
    /// Reacomoda o sprite depois de uma mudança de topologia (ARCHITECTURE.md 2.8).
    /// Se o monitor da posição ainda existe, mantém a posição relativa na área útil atual
    /// dele, o que cobre troca de resolução, escala e barra de tarefas. Se não existe, usa o
    /// monitor mais próximo da última âncora absoluta, com a mesma posição relativa, e a
    /// posição passa a ser desse monitor: se o original voltar, o personagem não pula de volta.
    /// </summary>
    public static (Posicionamento Resultado, PosicaoDoPersonagem NovaPosicao) Reacomodar(
        Topologia nova, PosicaoDoPersonagem atual, TamanhoDip tamanho)
    {
        ArgumentNullException.ThrowIfNull(nova);
        ArgumentNullException.ThrowIfNull(atual);

        MonitorDoDesktop? mesmo = nova.PorChave(atual.ChaveMonitor);
        if (mesmo is not null)
        {
            Posicionamento r = NoMonitor(mesmo, atual.FracaoX, atual.FracaoY, tamanho);
            return (r, atual with { AncoraAbsoluta = r.Ancora });
        }

        MonitorDoDesktop proximo = nova.MonitorMaisProximo(atual.AncoraAbsoluta);
        Posicionamento r2 = NoMonitor(proximo, atual.FracaoX, atual.FracaoY, tamanho);
        return (r2, Descrever(r2));
    }

    private static double Fracao(double f) => double.IsNaN(f) ? 0.5 : Math.Clamp(f, 0.0, 1.0);
}

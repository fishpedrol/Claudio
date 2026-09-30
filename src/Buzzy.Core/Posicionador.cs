namespace Buzzy.Core;

/// <summary>
/// Onde o personagem está, de um jeito que sobrevive a mudanças de topologia
/// (ARCHITECTURE.md 2.8): a chave do monitor, a posição relativa da âncora dentro da área
/// útil desse monitor (frações de 0 a 1) e a última âncora absoluta, usada como reserva
/// quando o monitor some.
/// </summary>
public sealed record PosicaoDoPersonagem(string ChaveMonitor, double FracaoX, double FracaoY, PontoPx AncoraAbsoluta)
{
    /// <summary>
    /// Tela do monitor da chave na última vez em que a posição foi descrita nele (o "retângulo desse
    /// monitor na época" de ARCHITECTURE.md 2.8); nula quando é desconhecida. Na partida, acha o
    /// monitor pelo retângulo quando a chave não existe mais (<see cref="Posicionador.Restaurar"/>).
    /// Fica fora do construtor posicional: uma posição construída sem ela tem a tela desconhecida.
    /// </summary>
    public RetanguloPx? TelaDoMonitor { get; init; }
}

/// <summary>Resultado de posicionar o sprite: monitor, âncora, tamanho físico e retângulo da janela.</summary>
public sealed record Posicionamento(MonitorDoDesktop Monitor, PontoPx Ancora, TamanhoPx Tamanho, RetanguloPx Retangulo);

/// <summary>Qual passo de <see cref="Posicionador.Restaurar"/> achou o monitor da posição salva.</summary>
public enum OrigemDaRestauracao
{
    /// <summary>O monitor da chave salva existe.</summary>
    PelaChave,

    /// <summary>A chave não existe, mas há um monitor com a tela salva (<see cref="PosicaoDoPersonagem.TelaDoMonitor"/>).</summary>
    PeloRetangulo,

    /// <summary>Nem a chave nem a tela: o monitor principal.</summary>
    NoPrincipal,
}

/// <summary>
/// Posiciona o sprite estático da Fase 1. A âncora é o ponto entre os pés: o centro da
/// borda inferior do sprite. O sprite sempre termina inteiro dentro da área útil de um
/// monitor presente, quando cabe nela.
/// </summary>
public static class Posicionador
{
    /// <summary>
    /// Posição horizontal inicial da âncora, como fração da largura da área útil do monitor
    /// principal: perto do canto inferior direito, onde o Buzzy atrapalha menos. Vale na primeira
    /// execução e sempre que não há posição salva; com ela, vale <see cref="Restaurar"/>.
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

    /// <summary>
    /// Pixel dos pés: o logo acima da âncora. A âncora fica na borda inferior exclusiva do sprite,
    /// que numa pilha de monitores (ou com a barra oculta) já é o primeiro pixel do monitor de baixo;
    /// o monitor do personagem é o que contém o pixel dos pés.
    /// </summary>
    public static PontoPx PixelDosPes(PontoPx ancora) => new(ancora.X, ancora.Y - 1);

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
            area.Esquerda + (int)Math.Round(SanearFracao(fracaoX) * area.Largura, MidpointRounding.AwayFromZero),
            area.Topo + (int)Math.Round(SanearFracao(fracaoY) * area.Altura, MidpointRounding.AwayFromZero));

        PontoPx ancora = PrenderNaAreaUtil(desejada, fisico, area);
        return new Posicionamento(monitor, ancora, fisico, RetanguloDoSprite(ancora, fisico));
    }

    /// <summary>Descreve um posicionamento como posição relativa à área útil do seu monitor, com a tela dele.</summary>
    public static PosicaoDoPersonagem Descrever(Posicionamento p)
    {
        ArgumentNullException.ThrowIfNull(p);
        RetanguloPx area = p.Monitor.AreaUtil;
        return new PosicaoDoPersonagem(
            p.Monitor.Chave,
            (p.Ancora.X - area.Esquerda) / (double)area.Largura,
            (p.Ancora.Y - area.Topo) / (double)area.Altura,
            p.Ancora)
        {
            TelaDoMonitor = p.Monitor.Tela,
        };
    }

    /// <summary>
    /// Reacomoda o sprite depois de uma mudança de topologia (ARCHITECTURE.md 2.8).
    /// Se o monitor da posição ainda existe, mantém a posição relativa na área útil atual
    /// dele, o que cobre troca de resolução, escala e barra de tarefas; a posição passa a guardar
    /// a tela atual dele. Se não existe, usa o monitor mais próximo do pixel dos pés da última
    /// âncora absoluta (<see cref="PixelDosPes"/>), com a mesma posição relativa, e a posição passa
    /// a ser desse monitor: se o original voltar, o personagem não pula de volta.
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
            return (r, atual with { AncoraAbsoluta = r.Ancora, TelaDoMonitor = mesmo.Tela });
        }

        MonitorDoDesktop proximo = nova.MonitorMaisProximo(PixelDosPes(atual.AncoraAbsoluta));
        Posicionamento r2 = NoMonitor(proximo, atual.FracaoX, atual.FracaoY, tamanho);
        return (r2, Descrever(r2));
    }

    /// <summary>
    /// Restaura na partida a posição salva (ARCHITECTURE.md 2.8), nesta ordem: o monitor da chave; se
    /// ela não existe, o primeiro monitor com a tela da época (<see cref="PosicaoDoPersonagem.TelaDoMonitor"/>);
    /// senão, o principal. Nos três passos vale a posição relativa salva, com as frações saneadas
    /// (NaN vira 0,5; fora de [0, 1], presas), e a posição passa a ser do monitor escolhido, com a tela
    /// atual dele. A âncora absoluta salva não é usada: noutra sessão, com outro principal, ela está
    /// noutro referencial. Restaurar de novo o resultado, na mesma topologia, não move o personagem.
    /// Durante a execução vale <see cref="Reacomodar"/>.
    /// </summary>
    public static (Posicionamento Resultado, PosicaoDoPersonagem NovaPosicao, OrigemDaRestauracao Origem) Restaurar(
        Topologia topologia, PosicaoDoPersonagem salva, TamanhoDip tamanho)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        ArgumentNullException.ThrowIfNull(salva);

        (MonitorDoDesktop destino, OrigemDaRestauracao origem) =
            topologia.PorChave(salva.ChaveMonitor) is { } daChave ? (daChave, OrigemDaRestauracao.PelaChave)
            : MonitorComATela(topologia, salva.TelaDoMonitor) is { } daTela ? (daTela, OrigemDaRestauracao.PeloRetangulo)
            : (topologia.Principal, OrigemDaRestauracao.NoPrincipal);

        double fx = SanearFracao(salva.FracaoX), fy = SanearFracao(salva.FracaoY);
        Posicionamento r = NoMonitor(destino, fx, fy, tamanho);
        var nova = new PosicaoDoPersonagem(destino.Chave, fx, fy, r.Ancora) { TelaDoMonitor = destino.Tela };
        return (r, nova, origem);
    }

    /// <summary>O primeiro monitor, na ordem da topologia, com exatamente essa tela; nulo se a tela é desconhecida ou não há nenhum.</summary>
    private static MonitorDoDesktop? MonitorComATela(Topologia topologia, RetanguloPx? tela)
        => tela is { } t ? topologia.Monitores.FirstOrDefault(m => m.Tela == t) : null;

    /// <summary>
    /// Fração de posição saneada: NaN vira 0,5; o resto, inclusive ±∞, é preso em [0, 1]. A mesma regra vale
    /// para a posição e para o settings.json (Persistencia.EsquemaDeConfiguracoes).
    /// </summary>
    internal static double SanearFracao(double fracao) => double.IsNaN(fracao) ? 0.5 : Math.Clamp(fracao, 0.0, 1.0);
}

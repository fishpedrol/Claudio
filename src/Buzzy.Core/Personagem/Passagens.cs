namespace Buzzy.Core.Personagem;

/// <summary>
/// Uma porta entre dois monitores (Fase 5, passo P13; DEC-032): o trecho [<paramref name="Topo"/>,
/// <paramref name="Base"/>) da lateral x = <paramref name="Borda"/> de um monitor, encostado na área útil do vizinho de
/// chave <paramref name="ChaveVizinho"/>. <paramref name="Lado"/> é o lado do monitor de origem: −1 esquerda, +1 direita.
/// </summary>
public readonly record struct Porta(string ChaveVizinho, int Lado, int Borda, int Topo, int Base);

/// <summary>Como ele atravessa: andando, pela porta plana (passo P13).</summary>
public enum TipoDeTravessia
{
    Andando,

    /// <summary>O salto de degrau (P13b): um arco balístico fechado, do chão da origem ao do vizinho.</summary>
    Salto,
}

/// <summary>
/// Uma travessia em curso (Fase 5, passo P13; DEC-032), parte do estado do movimento: do monitor de chave
/// <paramref name="ChaveOrigem"/> para o de <paramref name="ChaveDestino"/>, pela lateral <paramref name="Lado"/> (−1 esquerda,
/// +1 direita) na borda x = <paramref name="Borda"/>, em pixels físicos. A âncora troca de monitor quando, arredondada, passa da
/// borda (<see cref="Passagens.PassouDaBorda"/>). No salto (P13b), o arco é fechado: a partida (<paramref name="X0"/>,
/// <paramref name="Y0"/>), as velocidades (<paramref name="VX"/>, <paramref name="VY0"/>) e a gravidade <paramref name="G"/> em
/// pixels por segundo da escala da origem, congelada no voo, e a duração em passos; o último passo é exatamente o pouso
/// (<paramref name="XDestino"/>, <paramref name="YDestino"/>), e <paramref name="Passo"/> é o passo atual.
/// </summary>
public sealed record Travessia(
    TipoDeTravessia Tipo, string ChaveOrigem, string ChaveDestino, int Lado, int Borda,
    double XDestino = 0, double YDestino = 0, double X0 = 0, double Y0 = 0, double VX = 0, double VY0 = 0, double G = 0,
    int PassosTotais = 0, int Passo = 0);

/// <summary>
/// A geometria das passagens entre monitores (Fase 5, passo P13; DEC-032; desenho da travessia, D1, D2 e 4.1), pura e sem
/// estado. Uma porta nasce da adjacência exata das ÁREAS ÚTEIS, e não das telas: com uma barra de tarefas vertical entre dois
/// monitores, as áreas não se tocam, e a lateral é parede. O resto da lateral é parede, em trechos. Com a toon force
/// (DEC-023), toda lateral continua escalável; a porta só acrescenta a possibilidade de atravessar.
/// </summary>
public static class Passagens
{
    /// <summary>
    /// As portas da lateral <paramref name="lado"/> (−1 ou +1) do monitor, ordenadas por <see cref="Porta.Topo"/> e, no
    /// empate, pela chave do vizinho, em comparação ordinal: cada vizinho cuja área útil começa exatamente na borda da área
    /// útil dele, na faixa vertical em que as duas se sobrepõem. Monitores que só se tocam pela quina, ou separados por um
    /// vão, não têm porta.
    /// </summary>
    public static IReadOnlyList<Porta> Portas(Topologia topologia, MonitorDoDesktop monitor, int lado)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        ArgumentNullException.ThrowIfNull(monitor);
        if (lado is not (-1 or 1)) throw new ArgumentOutOfRangeException(nameof(lado), lado, "O lado é −1 (esquerda) ou +1 (direita).");
        RetanguloPx a = monitor.AreaUtil;
        int borda = lado > 0 ? a.Direita : a.Esquerda;
        var portas = new List<Porta>();
        foreach (MonitorDoDesktop outro in topologia.Monitores)
        {
            if (outro.Chave == monitor.Chave) continue;
            RetanguloPx n = outro.AreaUtil;
            if ((lado > 0 ? n.Esquerda : n.Direita) != borda) continue;
            int topo = Math.Max(a.Topo, n.Topo), baixo = Math.Min(a.Base, n.Base);
            if (topo < baixo) portas.Add(new Porta(outro.Chave, lado, borda, topo, baixo));
        }
        portas.Sort((p, q) => p.Topo != q.Topo ? p.Topo.CompareTo(q.Topo) : string.CompareOrdinal(p.ChaveVizinho, q.ChaveVizinho));
        return portas;
    }

    /// <summary>Os trechos [Topo, Base) da lateral que são parede: a altura da área útil menos as portas, de cima para baixo.</summary>
    public static IReadOnlyList<(int Topo, int Base)> TrechosDeParede(Topologia topologia, MonitorDoDesktop monitor, int lado)
    {
        var trechos = new List<(int Topo, int Base)>();
        int y = monitor.AreaUtil.Topo;
        foreach (Porta p in Portas(topologia, monitor, lado))
        {
            if (p.Topo > y) trechos.Add((y, p.Topo));
            y = Math.Max(y, p.Base);
        }
        if (y < monitor.AreaUtil.Base) trechos.Add((y, monitor.AreaUtil.Base));
        return trechos;
    }

    /// <summary>
    /// A porta da travessia plana pela lateral <paramref name="lado"/> (4.1), ou nula: o vizinho tem o mesmo chão (a mesma
    /// base da área útil), a porta vai até o chão com a altura do sprite maior dos dois (o sprite troca de tamanho na borda,
    /// pelo DPI de cada monitor) e o sprite cabe na área útil do vizinho. Só pode haver uma: a que contém o chão.
    /// </summary>
    public static Porta? PortaPlana(Topologia topologia, MonitorDoDesktop monitor, int lado, TamanhoDip sprite)
    {
        int chao = monitor.AreaUtil.Base;
        foreach (Porta p in Portas(topologia, monitor, lado))
        {
            if (p.Base != chao || topologia.PorChave(p.ChaveVizinho) is not { } vizinho || vizinho.AreaUtil.Base != chao) continue;
            TamanhoPx aqui = sprite.ParaPixels(monitor.Dpi), la = sprite.ParaPixels(vizinho.Dpi);
            if (p.Topo > chao - Math.Max(aqui.Altura, la.Altura)) continue;
            if (la.Largura > vizinho.AreaUtil.Largura || la.Altura > vizinho.AreaUtil.Altura) continue;
            return p;
        }
        return null;
    }

    /// <summary>
    /// O salto de degrau pela lateral <paramref name="lado"/> (P13b; D2, D3 e D6), ou nulo. Um vizinho com uma porta e o chão
    /// a uma distância Δ ≠ 0, medida em DIP pela menor escala dos dois (a mesma nos dois sentidos), de até
    /// <see cref="ParametrosDeMovimento.DescidaMaxima"/> para baixo e <see cref="ParametrosDeMovimento.SubidaMaxima"/> para
    /// cima, e o sprite cabendo nele. Entre vários vizinhos do mesmo lado, o de menor |Δ|; no empate, o de menor Δ (subir
    /// primeiro) e, depois, a chave (D12). O arco parte da âncora encostada na lateral, no chão da origem, com a gravidade na
    /// escala da origem: para cada tempo de voo de <see cref="ParametrosDeMovimento.TemposDoSalto"/>, em ordem, e para cada
    /// pouso no chão do vizinho, da lateral de entrada para dentro (0, ¼, ½, 1, 1½ e 2 larguras do sprite), e para cada
    /// recuo da partida antes da lateral (0, ¼, ½, 1 e 2 larguras, até <paramref name="recuoMaximo"/> px), vence o primeiro
    /// em que o sprite, com o tamanho do monitor da âncora, fica na união das áreas úteis em todos os passos. O recuo é o que
    /// permite subir: o sprite precisa ganhar altura antes de a frente dele passar da borda. Sem nenhum, o degrau não serve.
    /// Determinístico, sem gerador pseudoaleatório.
    /// </summary>
    public static Travessia? SaltoDeDegrau(Topologia topologia, MonitorDoDesktop monitor, int lado, TamanhoDip sprite, ParametrosDeMovimento fisica, int passosPorSegundo, int recuoMaximo = 0)
    {
        ArgumentNullException.ThrowIfNull(fisica);
        var vizinhos = new List<(Porta Porta, MonitorDoDesktop Vizinho, int Delta)>();
        foreach (Porta p in Portas(topologia, monitor, lado))
        {
            if (topologia.PorChave(p.ChaveVizinho) is not { } vizinho) continue;
            int delta = vizinho.AreaUtil.Base - monitor.AreaUtil.Base;
            if (delta == 0) continue;
            double dip = delta / (Math.Min(monitor.Dpi, vizinho.Dpi) / 96.0);
            if (dip < -fisica.SubidaMaxima || dip > fisica.DescidaMaxima) continue;
            vizinhos.Add((p, vizinho, delta));
        }
        vizinhos.Sort((a, b) => Math.Abs(a.Delta) != Math.Abs(b.Delta) ? Math.Abs(a.Delta).CompareTo(Math.Abs(b.Delta))
            : a.Delta != b.Delta ? a.Delta.CompareTo(b.Delta) : string.CompareOrdinal(a.Vizinho.Chave, b.Vizinho.Chave));

        foreach ((Porta porta, MonitorDoDesktop vizinho, _) in vizinhos)
        {
            TamanhoPx aqui = sprite.ParaPixels(monitor.Dpi), la = sprite.ParaPixels(vizinho.Dpi);
            if (la.Largura > vizinho.AreaUtil.Largura || la.Altura > vizinho.AreaUtil.Altura) continue;
            Superficies daOrigem = Superficies.Do(topologia, monitor, aqui), doDestino = Superficies.Do(topologia, vizinho, la);
            double limite = lado > 0 ? daOrigem.Direita : daOrigem.Esquerda, y0 = daOrigem.Chao, y1 = doDestino.Chao;
            double g = fisica.Gravidade * monitor.Dpi / 96.0;
            int[] pousos = [0, la.Largura / 4, la.Largura / 2, la.Largura, la.Largura * 3 / 2, la.Largura * 2];
            int[] recuos = [.. new[] { 0, aqui.Largura / 4, aqui.Largura / 2, aqui.Largura, aqui.Largura * 2 }.Where(r => r <= recuoMaximo && r <= daOrigem.Direita - daOrigem.Esquerda)];
            foreach (double tempo in fisica.TemposDoSalto)
            foreach (int recuo in recuos)
            {
                double x0 = limite - lado * recuo;
                int passos = (int)Math.Ceiling(tempo * passosPorSegundo);
                foreach (int dentro in pousos)
                {
                    double x1 = lado > 0 ? doDestino.Esquerda + dentro : doDestino.Direita - dentro;
                    if (x1 < doDestino.Esquerda || x1 > doDestino.Direita) continue;
                    var salto = new Travessia(TipoDeTravessia.Salto, monitor.Chave, vizinho.Chave, lado, porta.Borda,
                        x1, y1, x0, y0, (x1 - x0) / tempo, (y1 - y0 - 0.5 * g * tempo * tempo) / tempo, g, passos);
                    if (ArcoNaUniao(topologia, salto, monitor, vizinho, sprite, passosPorSegundo)) return salto;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// O transbordo pela lateral <paramref name="lado"/> (P13c; D5 e C14 da crítica), ou nulo: escalando a lateral do monitor
    /// mais baixo com os pés na altura <paramref name="alturaDoChao"/>, que é a do chão de um vizinho mais alto (o topo do
    /// trecho de parede abaixo da porta), um arco curto o leva ao chão do vizinho. A partida é a âncora encostada na lateral,
    /// nessa altura, com o sprite inteiro na área útil da origem; o pouso, no chão do vizinho, da lateral de entrada para
    /// dentro; o arco, pelo mesmo solucionador do salto de degrau, sem recuo. Sem limite de altura: é o caminho da subida
    /// além do alcance do salto.
    /// </summary>
    public static Travessia? Transbordo(Topologia topologia, MonitorDoDesktop monitor, int lado, int alturaDoChao, TamanhoDip sprite, ParametrosDeMovimento fisica, int passosPorSegundo)
    {
        ArgumentNullException.ThrowIfNull(fisica);
        if (alturaDoChao >= monitor.AreaUtil.Base) return null;
        TamanhoPx aqui = sprite.ParaPixels(monitor.Dpi);
        if (alturaDoChao - aqui.Altura < monitor.AreaUtil.Topo) return null;
        foreach (Porta porta in Portas(topologia, monitor, lado))
        {
            if (topologia.PorChave(porta.ChaveVizinho) is not { } vizinho || vizinho.AreaUtil.Base != alturaDoChao) continue;
            TamanhoPx la = sprite.ParaPixels(vizinho.Dpi);
            if (la.Largura > vizinho.AreaUtil.Largura || la.Altura > vizinho.AreaUtil.Altura || porta.Topo > alturaDoChao - Math.Max(aqui.Altura, la.Altura)) continue;
            Superficies daOrigem = Superficies.Do(topologia, monitor, aqui), doDestino = Superficies.Do(topologia, vizinho, la);
            double x0 = lado > 0 ? daOrigem.Direita : daOrigem.Esquerda;
            if (Arco(topologia, monitor, vizinho, porta, lado, x0, alturaDoChao, doDestino, sprite, fisica, passosPorSegundo) is { } arco) return arco;
        }
        return null;
    }

    /// <summary>
    /// O primeiro arco válido de (<paramref name="x0"/>, <paramref name="y0"/>) ao chão do destino: para cada tempo de voo e
    /// cada pouso, da lateral de entrada para dentro, o sprite na união das áreas úteis em todos os passos.
    /// </summary>
    private static Travessia? Arco(Topologia topologia, MonitorDoDesktop origem, MonitorDoDesktop destino, Porta porta, int lado, double x0, double y0,
        Superficies doDestino, TamanhoDip sprite, ParametrosDeMovimento fisica, int passosPorSegundo)
    {
        int largura = sprite.ParaPixels(destino.Dpi).Largura;
        int[] pousos = [0, largura / 4, largura / 2, largura, largura * 3 / 2, largura * 2];
        double g = fisica.Gravidade * origem.Dpi / 96.0, y1 = doDestino.Chao;
        foreach (double tempo in fisica.TemposDoSalto)
        {
            int passos = (int)Math.Ceiling(tempo * passosPorSegundo);
            foreach (int dentro in pousos)
            {
                double x1 = lado > 0 ? doDestino.Esquerda + dentro : doDestino.Direita - dentro;
                if (x1 < doDestino.Esquerda || x1 > doDestino.Direita) continue;
                var salto = new Travessia(TipoDeTravessia.Salto, origem.Chave, destino.Chave, lado, porta.Borda,
                    x1, y1, x0, y0, (x1 - x0) / tempo, (y1 - y0 - 0.5 * g * tempo * tempo) / tempo, g, passos);
                if (ArcoNaUniao(topologia, salto, origem, destino, sprite, passosPorSegundo)) return salto;
            }
        }
        return null;
    }

    /// <summary>
    /// A âncora fina do salto no passo <paramref name="passo"/> (1 até <see cref="Travessia.PassosTotais"/>), analítica: no
    /// último, exatamente o pouso.
    /// </summary>
    public static (double X, double Y) PosicaoNoSalto(Travessia salto, int passo, int passosPorSegundo)
    {
        ArgumentNullException.ThrowIfNull(salto);
        if (passo >= salto.PassosTotais) return (salto.XDestino, salto.YDestino);
        double t = (double)passo / passosPorSegundo;
        return (salto.X0 + salto.VX * t, salto.Y0 + salto.VY0 * t + 0.5 * salto.G * t * t);
    }

    /// <summary>Se, em todos os passos do salto, o sprite, com o tamanho do monitor da âncora, fica na união das áreas úteis.</summary>
    private static bool ArcoNaUniao(Topologia topologia, Travessia salto, MonitorDoDesktop origem, MonitorDoDesktop destino, TamanhoDip sprite, int passosPorSegundo)
    {
        for (int k = 1; k <= salto.PassosTotais; k++)
        {
            (double x, double y) = PosicaoNoSalto(salto, k, passosPorSegundo);
            var ancora = new PontoPx((int)Math.Round(x, MidpointRounding.AwayFromZero), (int)Math.Round(y, MidpointRounding.AwayFromZero));
            MonitorDoDesktop m = PassouDaBorda(salto, ancora) ? destino : origem;
            if (!NaUniaoDasAreasUteis(topologia, Posicionador.RetanguloDoSprite(ancora, sprite.ParaPixels(m.Dpi)))) return false;
        }
        return true;
    }

    /// <summary>
    /// Se a âncora arredondada já passou da borda da travessia (D8): do lado do destino, ela está no monitor de destino. Com a
    /// borda em x = 0 e o destino à esquerda, a âncora −1 já está nele; a 0, ainda na origem.
    /// </summary>
    public static bool PassouDaBorda(Travessia travessia, PontoPx ancora)
    {
        ArgumentNullException.ThrowIfNull(travessia);
        return travessia.Lado > 0 ? ancora.X >= travessia.Borda : ancora.X < travessia.Borda;
    }

    /// <summary>
    /// Se o retângulo está inteiro na união das áreas úteis (4.2): a soma das interseções com cada área útil é a área dele.
    /// É exato porque as áreas úteis dos monitores não se sobrepõem. Um retângulo vazio está na união.
    /// </summary>
    public static bool NaUniaoDasAreasUteis(Topologia topologia, RetanguloPx r)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        if (r.Largura <= 0 || r.Altura <= 0) return true;
        long soma = 0;
        foreach (MonitorDoDesktop m in topologia.Monitores)
        {
            RetanguloPx u = m.AreaUtil;
            long largura = Math.Min(r.Direita, u.Direita) - (long)Math.Max(r.Esquerda, u.Esquerda);
            long altura = Math.Min(r.Base, u.Base) - (long)Math.Max(r.Topo, u.Topo);
            if (largura > 0 && altura > 0) soma += largura * altura;
        }
        return soma == (long)r.Largura * r.Altura;
    }
}

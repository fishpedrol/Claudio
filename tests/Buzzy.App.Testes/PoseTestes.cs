using System.Windows.Media.Imaging;
using Buzzy.App.Apresentacao;
using Buzzy.Core;
using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.App.Testes;

/// <summary>
/// Poses provisórias por estado (Fase 4) e o esticar e achatar de desenho animado da toon force
/// (DEC-023): a pose escolhida pelo retrato e pela dinâmica, e a deformação da pixel art, que
/// mantém os pés na mesma linha, o alfa só 0 ou 255 e os cantos transparentes.
/// </summary>
internal sealed class PoseTestes
{
    private static Retrato R(Estado estado, Direcao direcao = Direcao.Direita, Expressao expressao = Expressao.Neutro)
        => new(estado, MotivoDoOcultamento.Nenhum, new PontoPx(0, 0), "m", new TamanhoPx(128, 128), direcao, expressao, Gesto.Nenhum, false, false, NivelDeEnergia.Media, true, Sinal.Nenhum);

    private static int[] Pixels(BitmapSource bmp)
    {
        int[] p = new int[bmp.PixelWidth * bmp.PixelHeight];
        bmp.CopyPixels(p, bmp.PixelWidth * 4, 0);
        return p;
    }

    private static byte Alfa(int pixel) => (byte)((uint)pixel >> 24);

    /// <summary>Retângulo dos pixels opacos: (esquerda, topo, direita exclusiva, base exclusiva).</summary>
    private static (int E, int T, int D, int B) Limites(BitmapSource bmp)
    {
        int[] p = Pixels(bmp);
        int w = bmp.PixelWidth, h = bmp.PixelHeight;
        int e = int.MaxValue, t = int.MaxValue, d = int.MinValue, b = int.MinValue;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (Alfa(p[y * w + x]) != 255) continue;
                e = Math.Min(e, x);
                t = Math.Min(t, y);
                d = Math.Max(d, x + 1);
                b = Math.Max(b, y + 1);
            }
        }
        return (e, t, d, b);
    }

    [Teste]
    public void Foguete_SobeEsticadoOlhandoParaAParede()
    {
        foreach (Direcao direcao in new[] { Direcao.Direita, Direcao.Esquerda })
        {
            QuadroDoSprite q = PoseDoPersonagem.Escolher(R(Estado.Climbing, direcao), 3, new Dinamica(-1000, 0, Foguete: true));
            Afirmar.Igual(Deformacao.Esticado, q.Deformacao, $"foguete {direcao}");
            Afirmar.Igual(direcao == Direcao.Esquerda, q.Espelhado, $"foguete {direcao}: olha para a parede");
            QuadroDoSprite normal = PoseDoPersonagem.Escolher(R(Estado.Climbing, direcao), 3, new Dinamica(-110, 0, Foguete: false));
            Afirmar.Igual(Deformacao.Nenhuma, normal.Deformacao, $"escalada normal {direcao}");
            Afirmar.Verdadeiro(normal.Pose.StartsWith("escalando-", StringComparison.Ordinal), $"escalada normal {direcao}: {normal.Pose}");
        }
    }

    [Teste]
    public void Quique_ComecaAchatadoNoChaoESobeEsticado()
    {
        QuadroDoSprite impacto = PoseDoPersonagem.Escolher(R(Estado.Jumping), 0, new Dinamica(-750, 1, false));
        Afirmar.Igual(Deformacao.Achatado, impacto.Deformacao, "o quique começa achatado");
        QuadroDoSprite subindo = PoseDoPersonagem.Escolher(R(Estado.Jumping), PoseDoPersonagem.PassosDoAchatamento, new Dinamica(-720, 1, false));
        Afirmar.Igual(("impulso", Deformacao.Esticado), (subindo.Pose, subindo.Deformacao), "depois sobe esticado");
        QuadroDoSprite noTopo = PoseDoPersonagem.Escolher(R(Estado.Jumping), 20, new Dinamica(-50, 1, false));
        Afirmar.Igual(Deformacao.Nenhuma, noTopo.Deformacao, "devagar, no alto do quique, o corpo volta ao normal");
    }

    [Teste]
    public void PousoAchataEQuedaRapidaEstica()
    {
        Afirmar.Igual(Deformacao.Achatado, PoseDoPersonagem.Escolher(R(Estado.Landing), 0).Deformacao, "o pouso começa achatado");
        Afirmar.Igual(Deformacao.Nenhuma, PoseDoPersonagem.Escolher(R(Estado.Landing), PoseDoPersonagem.PassosDoAchatamento).Deformacao, "e desamassa");
        Afirmar.Igual(Deformacao.Esticado, PoseDoPersonagem.Escolher(R(Estado.Falling), 30, new Dinamica(1200, 0, false)).Deformacao, "queda rápida estica");
        Afirmar.Igual(Deformacao.Nenhuma, PoseDoPersonagem.Escolher(R(Estado.Falling), 2, new Dinamica(100, 0, false)).Deformacao, "começo da queda, devagar");
        Afirmar.Igual(Deformacao.Nenhuma, PoseDoPersonagem.Escolher(R(Estado.Jumping), 2, new Dinamica(-900, 0, false)).Deformacao, "o impulso de um pulo comum não deforma");
    }

    [Teste]
    public void TodaPoseEscolhidaExisteNaPixelArtComQualquerDeformacao()
    {
        var dinamicas = new[] { default(Dinamica), new Dinamica(1200, 0, false), new Dinamica(-800, 1, false), new Dinamica(-1000, 0, true) };
        foreach (Estado estado in Enum.GetValues<Estado>())
        {
            foreach (Dinamica dinamica in dinamicas)
            {
                foreach (long passos in new long[] { 0, 3, 7, 40 })
                {
                    QuadroDoSprite q = PoseDoPersonagem.Escolher(R(estado, Direcao.Esquerda), passos, dinamica);
                    BitmapSource bmp = SpriteProvisorio.Renderizar(q, 96);
                    Afirmar.Igual(128, bmp.PixelWidth, $"{estado} {dinamica} {passos}: mesmo tamanho de janela");
                }
            }
        }
    }

    [Teste]
    public void AchatarEEsticarMantemOsPesOAlfaBinarioEOsCantosTransparentes()
    {
        foreach (string pose in new[] { "pousando", "no-ar", "caindo", "parado", "impulso" })
        {
            foreach (int dpi in new[] { 96, 144, 192 })
            {
                (int eN, int tN, int dN, int bN) = Limites(SpriteProvisorio.Renderizar(new QuadroDoSprite(pose, false, null), dpi));
                foreach (Deformacao d in new[] { Deformacao.Achatado, Deformacao.Esticado })
                {
                    BitmapSource bmp = SpriteProvisorio.Renderizar(new QuadroDoSprite(pose, false, null, d), dpi);
                    int[] p = Pixels(bmp);
                    string onde = $"{pose} {d} em {dpi} DPI";
                    Afirmar.Igual(0, p.Count(x => Alfa(x) is not 0 and not 255), $"{onde}: alfa só 0 ou 255");
                    (int e, int t, int dd, int b) = Limites(bmp);
                    Afirmar.Igual(bN, b, $"{onde}: os pés continuam na mesma linha");
                    Afirmar.Verdadeiro(e > 0 && t > 0 && dd < bmp.PixelWidth, $"{onde}: o desenho não encosta nas bordas de cima e dos lados ({e},{t},{dd},{b})");
                    int w = bmp.PixelWidth, h = bmp.PixelHeight, bloco = Math.Max(3, w / 20);
                    foreach ((int x0, int y0) in new[] { (0, 0), (w - bloco, 0), (0, h - bloco), (w - bloco, h - bloco) })
                        for (int y = y0; y < y0 + bloco; y++)
                            for (int x = x0; x < x0 + bloco; x++)
                                Afirmar.Igual((byte)0, Alfa(p[y * w + x]), $"{onde}: canto ({x},{y}) transparente");
                    if (d == Deformacao.Achatado)
                        Afirmar.Verdadeiro(dd - e > dN - eN && b - t < bN - tN, $"{onde}: mais largo e mais baixo ({dd - e}x{b - t} contra {dN - eN}x{bN - tN})");
                    else // Uma pose que já ocupa a altura do quadro só afina: a margem de cima vale mais.
                        Afirmar.Verdadeiro(dd - e < dN - eN && b - t >= bN - tN, $"{onde}: mais estreito e não mais baixo ({dd - e}x{b - t} contra {dN - eN}x{bN - tN})");
                }
            }
        }
    }
}

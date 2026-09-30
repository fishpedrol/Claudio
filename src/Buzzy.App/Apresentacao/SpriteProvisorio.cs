using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Buzzy.Core;

namespace Buzzy.App.Apresentacao;

/// <summary>
/// Sprite PROVISÓRIO e estático da Fase 1: formas geométricas simples, desenhadas em código,
/// só para testar a janela (TODO.md, Fase 1; ARCHITECTURE.md 2.10). Não é arte, não é a
/// proposta visual do Buzzy e não imita nenhum personagem existente. Será substituído pelo
/// manifesto de assets da Fase 6, depois de uma proposta visual original aprovada.
///
/// Regra de P1 (ARCHITECTURE.md 2.13.7, item 7): só alfa exatamente 0 deixa o clique passar.
/// Por isso o desenho é rasterizado sem suavização de borda e depois todo pixel é forçado a
/// alfa 0 ou 255: não sobra halo de alfa baixo que capture clique fora da silhueta.
/// </summary>
internal static class SpriteProvisorio
{
    /// <summary>Tamanho lógico do personagem. A escala escolhida pelo usuário entra na Fase 8.</summary>
    internal static readonly TamanhoDip TamanhoLogico = new(128, 128);

    private static readonly Color CorCorpo = Color.FromRgb(0x6E, 0x56, 0x46);
    private static readonly Color CorClara = Color.FromRgb(0xE3, 0xCD, 0xB0);
    private static readonly Color CorContorno = Color.FromRgb(0x2E, 0x23, 0x1C);
    private static readonly Color CorOlho = Color.FromRgb(0xFF, 0xFF, 0xFF);
    private static readonly Color CorPupila = Color.FromRgb(0x1B, 0x1B, 0x1B);

    /// <summary>
    /// Renderiza o sprite no DPI do monitor, pixel a pixel (tamanho físico =
    /// <see cref="TamanhoLogico"/> no DPI dado), com alfa só 0 ou 255. Congelado.
    /// </summary>
    internal static BitmapSource Renderizar(int dpi)
    {
        TamanhoPx tamanho = TamanhoLogico.ParaPixels(dpi);
        int[] pixels = Rasterizar(Desenhar(suavizar: false), tamanho.Largura, tamanho.Altura, dpi);
        Limiarizar(pixels);

        var bmp = new WriteableBitmap(tamanho.Largura, tamanho.Altura, dpi, dpi, PixelFormats.Pbgra32, null);
        bmp.WritePixels(new Int32Rect(0, 0, tamanho.Largura, tamanho.Altura), pixels, tamanho.Largura * 4, 0);
        bmp.Freeze();
        return bmp;
    }

    /// <summary>PNG do mesmo desenho, quadrado, para o ícone da bandeja (aqui a suavização é bem-vinda).</summary>
    internal static byte[] IconePng(int ladoPx)
    {
        if (ladoPx <= 0) throw new ArgumentOutOfRangeException(nameof(ladoPx));
        double dpi = 96.0 * ladoPx / TamanhoLogico.Largura;
        var alvo = new RenderTargetBitmap(ladoPx, ladoPx, dpi, dpi, PixelFormats.Pbgra32);
        alvo.Render(Desenhar(suavizar: true));

        var codificador = new PngBitmapEncoder();
        codificador.Frames.Add(BitmapFrame.Create(alvo));
        using var memoria = new MemoryStream();
        codificador.Save(memoria);
        return memoria.ToArray();
    }

    /// <summary>
    /// Pontos de teste em coordenadas locais do bitmap: um pixel opaco (no meio da barriga) e
    /// um transparente (no canto superior esquerdo). Conferidos contra os pixels reais.
    /// </summary>
    internal static (PontoPx Opaco, PontoPx Transparente) PontosDeTeste(BitmapSource bmp)
    {
        int largura = bmp.PixelWidth, altura = bmp.PixelHeight;
        int[] pixels = new int[largura * altura];
        bmp.CopyPixels(pixels, largura * 4, 0);

        byte Alfa(int x, int y) => (byte)((uint)pixels[y * largura + x] >> 24);

        var opaco = new PontoPx(largura / 2, (int)(altura * 100.0 / 128));
        var transparente = new PontoPx(Math.Max(1, largura / 32), Math.Max(1, altura / 32));

        if (Alfa(opaco.X, opaco.Y) != 255)
            throw new InvalidOperationException($"O ponto opaco de teste {opaco} não está opaco no sprite.");
        if (Alfa(transparente.X, transparente.Y) != 0)
            throw new InvalidOperationException($"O ponto transparente de teste {transparente} não está transparente no sprite.");
        return (opaco, transparente);
    }

    /// <summary>
    /// Força cada pixel Pbgra32 a alfa 0 (tudo zero) ou 255 (cor desfeita da pré-multiplicação).
    /// </summary>
    internal static void Limiarizar(int[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            uint p = (uint)pixels[i];
            uint a = p >> 24;
            if (a == 255) continue;
            if (a < 128)
            {
                pixels[i] = 0;
                continue;
            }
            uint r = Math.Min(255u, ((p >> 16) & 0xFF) * 255 / a);
            uint g = Math.Min(255u, ((p >> 8) & 0xFF) * 255 / a);
            uint b = Math.Min(255u, (p & 0xFF) * 255 / a);
            pixels[i] = unchecked((int)(0xFF000000u | (r << 16) | (g << 8) | b));
        }
    }

    private static int[] Rasterizar(Visual visual, int largura, int altura, int dpi)
    {
        var alvo = new RenderTargetBitmap(largura, altura, dpi, dpi, PixelFormats.Pbgra32);
        alvo.Render(visual);
        int[] pixels = new int[largura * altura];
        alvo.CopyPixels(pixels, largura * 4, 0);
        return pixels;
    }

    /// <summary>
    /// Figura em DIPs, num quadro de 128 × 128, com os pés tocando a borda de baixo: a âncora
    /// (centro da base) fica entre os pés. Um primata genérico de formas simples.
    /// </summary>
    private static DrawingVisual Desenhar(bool suavizar)
    {
        var visual = new DrawingVisual();
        RenderOptions.SetEdgeMode(visual, suavizar ? EdgeMode.Unspecified : EdgeMode.Aliased);

        var corpo = new SolidColorBrush(CorCorpo);
        var clara = new SolidColorBrush(CorClara);
        var olho = new SolidColorBrush(CorOlho);
        var pupila = new SolidColorBrush(CorPupila);
        var contorno = new Pen(new SolidColorBrush(CorContorno), 2);
        var traco = new Pen(new SolidColorBrush(CorContorno), 2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

        using DrawingContext dc = visual.RenderOpen();

        // Cauda: curva grossa atrás do corpo, com contorno feito por um traço mais largo embaixo.
        var cauda = Geometry.Parse("M 84,104 C 110,108 122,86 112,70 C 104,58 92,66 98,74");
        dc.DrawGeometry(null, new Pen(new SolidColorBrush(CorContorno), 11) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, cauda);
        dc.DrawGeometry(null, new Pen(corpo, 7) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, cauda);

        // Pés, braços e corpo.
        dc.DrawEllipse(corpo, contorno, new Point(52, 120.5), 11, 6);
        dc.DrawEllipse(corpo, contorno, new Point(76, 120.5), 11, 6);
        dc.DrawEllipse(corpo, contorno, new Point(38, 96), 8, 18);
        dc.DrawEllipse(corpo, contorno, new Point(90, 96), 8, 18);
        dc.DrawEllipse(corpo, contorno, new Point(64, 96), 27, 26);
        dc.DrawEllipse(clara, null, new Point(64, 100), 15, 16);

        // Orelhas e cabeça.
        dc.DrawEllipse(corpo, contorno, new Point(33, 40), 12, 12);
        dc.DrawEllipse(corpo, contorno, new Point(95, 40), 12, 12);
        dc.DrawEllipse(clara, null, new Point(33, 40), 6, 6);
        dc.DrawEllipse(clara, null, new Point(95, 40), 6, 6);
        dc.DrawEllipse(corpo, contorno, new Point(64, 46), 32, 32);

        // Rosto: focinho claro, olhos, nariz e boca.
        dc.DrawEllipse(clara, null, new Point(64, 55), 22, 18);
        dc.DrawEllipse(olho, contorno, new Point(54, 44), 7, 7);
        dc.DrawEllipse(olho, contorno, new Point(74, 44), 7, 7);
        dc.DrawEllipse(pupila, null, new Point(55, 45), 3.5, 3.5);
        dc.DrawEllipse(pupila, null, new Point(75, 45), 3.5, 3.5);
        dc.DrawEllipse(pupila, null, new Point(64, 58), 3, 2);
        dc.DrawGeometry(null, traco, Geometry.Parse("M 56,64 Q 64,70 72,64"));

        return visual;
    }
}

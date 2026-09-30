using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Buzzy.Core;
using Buzzy.Visual.Pixel;

namespace Buzzy.App.Apresentacao;

/// <summary>
/// Sprite estático do Buzzy até a animação da Fase 6: o quadro "parado" da pixel art
/// (docs/IDENTIDADE_VISUAL.md, DEC-018 e DEC-019), com o chapéu de palha. O quadro tem 64 × 64
/// pixels de arte e é ampliado por vizinho mais próximo até o tamanho físico do DPI do monitor
/// (2×, 3× e 4× exatos em 100%, 150% e 200%).
///
/// Regra de P1 (ARCHITECTURE.md 2.13.7, item 7): só alfa exatamente 0 deixa o clique passar. A pixel
/// art já tem só alfa 0 ou 255, e a ampliação sem suavização mantém isso.
/// </summary>
internal static class SpriteProvisorio
{
    /// <summary>Tamanho lógico do personagem. A escala escolhida pelo usuário entra na Fase 8.</summary>
    internal static readonly TamanhoDip TamanhoLogico = new(128, 128);

    private static readonly Lazy<Tela> Parado = new(() => BonecoPixel.Desenhar(PosesPixel.Todas.First(p => p.Nome == "parado")));

    /// <summary>Quadros já renderizados por pose, espelho, expressão e DPI (só na thread da interface).</summary>
    private static readonly Dictionary<(QuadroDoSprite Quadro, int Dpi), BitmapSource> Cache = [];

    /// <summary>
    /// Renderiza o sprite no DPI do monitor (tamanho físico = <see cref="TamanhoLogico"/> no DPI
    /// dado), com alfa só 0 ou 255. Congelado.
    /// </summary>
    internal static BitmapSource Renderizar(int dpi)
    {
        TamanhoPx tamanho = TamanhoLogico.ParaPixels(dpi);
        return Bitmap(Parado.Value, tamanho.Largura, tamanho.Altura, dpi);
    }

    /// <summary>
    /// O quadro pedido (pose provisória da Fase 4) no DPI do monitor, renderizado uma vez e guardado.
    /// Mesmo tamanho lógico em todas as poses: a janela e a âncora (centro da base) não mudam.
    /// </summary>
    internal static BitmapSource Renderizar(QuadroDoSprite quadro, int dpi)
    {
        if (Cache.TryGetValue((quadro, dpi), out BitmapSource? pronto)) return pronto;
        PosePixel pose = PosesPixel.Todas.FirstOrDefault(p => p.Nome == quadro.Pose)
            ?? throw new ArgumentException($"Pose desconhecida: {quadro.Pose}.", nameof(quadro));
        Tela tela = BonecoPixel.Desenhar(pose, quadro.Expressao);
        if (quadro.Espelhado) tela = tela.Espelhada();
        TamanhoPx tamanho = TamanhoLogico.ParaPixels(dpi);
        BitmapSource bmp = Bitmap(tela, tamanho.Largura, tamanho.Altura, dpi);
        Cache[(quadro, dpi)] = bmp;
        return bmp;
    }

    /// <summary>PNG do ícone da bandeja (cabeça desenhada em 16 × 16), ampliado sem suavização.</summary>
    internal static byte[] IconePng(int ladoPx)
    {
        if (ladoPx <= 0) throw new ArgumentOutOfRangeException(nameof(ladoPx));
        BitmapSource icone = Bitmap(Icone.Desenhar(), ladoPx, ladoPx, 96);

        var codificador = new PngBitmapEncoder();
        codificador.Frames.Add(BitmapFrame.Create(icone));
        using var memoria = new MemoryStream();
        codificador.Save(memoria);
        return memoria.ToArray();
    }

    /// <summary>
    /// Pontos de teste em coordenadas locais do bitmap: um pixel opaco (na barriga) e um
    /// transparente (perto do canto superior esquerdo). Conferidos contra os pixels reais.
    /// </summary>
    internal static (PontoPx Opaco, PontoPx Transparente) PontosDeTeste(BitmapSource bmp)
    {
        ArgumentNullException.ThrowIfNull(bmp);
        int largura = bmp.PixelWidth, altura = bmp.PixelHeight;
        int[] pixels = new int[largura * altura];
        bmp.CopyPixels(pixels, largura * 4, 0);

        byte Alfa(int x, int y) => (byte)((uint)pixels[y * largura + x] >> 24);

        var opaco = new PontoPx(largura / 2, (int)(altura * 0.62));
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
        ArgumentNullException.ThrowIfNull(pixels);
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

    /// <summary>Amplia a tela por vizinho mais próximo até largura × altura e gera um bitmap congelado.</summary>
    private static BitmapSource Bitmap(Tela tela, int largura, int altura, int dpi)
    {
        uint[] origem = tela.ParaArgb();
        int[] pixels = new int[largura * altura];
        for (int y = 0; y < altura; y++)
        {
            int sy = y * tela.Altura / altura;
            for (int x = 0; x < largura; x++)
            {
                int sx = x * tela.Largura / largura;
                pixels[y * largura + x] = unchecked((int)origem[sy * tela.Largura + sx]);
            }
        }
        // Alfa só 0 ou 255: em Pbgra32, a cor pré-multiplicada é a própria cor.
        Limiarizar(pixels);

        var bmp = new WriteableBitmap(largura, altura, dpi, dpi, PixelFormats.Pbgra32, null);
        bmp.WritePixels(new Int32Rect(0, 0, largura, altura), pixels, largura * 4, 0);
        bmp.Freeze();
        return bmp;
    }
}

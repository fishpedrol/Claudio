using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Buzzy.Visual.Pixel;

namespace Buzzy.Identidade;

/// <summary>
/// Prévias da identidade em pixel art (assets/identidade/pixel/previa/): folha de modelo ampliada
/// sem suavização, grade de poses, grade de expressões e tamanho real (2× = 128 DIP a 100%).
/// </summary>
internal static class PreviaPixel
{
    private static readonly uint Fundo = 0xFFF1EEE8;
    private static readonly uint FundoEscuro = 0xFF23252B;

    internal static int Gerar(string raiz, IReadOnlyList<PosePixel> poses)
    {
        string pasta = Path.Combine(raiz, "assets", "identidade", "pixel", "previa");
        Directory.CreateDirectory(pasta);

        int problemas = 0;
        foreach (PosePixel pose in poses)
        {
            foreach (string expressao in Rostos.Expressoes.Keys)
            {
                // O cipó (DEC-024) é o único desenho que encosta numa borda, a de cima, onde se prende
                // na tela: a conferência vale para o corpo, sem ele.
                Tela t = BonecoPixel.Desenhar(pose with { Cipo = null }, expressao);
                if (t.Limites() is not { } l) continue;
                bool encosta = l.Esquerda <= 0 || l.Topo <= 0 || l.Direita >= BonecoPixel.Lado || l.Base > BonecoPixel.Lado;
                if (encosta && expressao == pose.Expressao)
                {
                    Console.WriteLine($"  NA BORDA DO QUADRO: pose '{pose.Nome}' ocupa ({l.Esquerda},{l.Topo})-({l.Direita},{l.Base})");
                    problemas++;
                }
            }
        }

        PosePixel parado = poses.First(p => p.Nome == "parado");
        Salvar(Ampliada(BonecoPixel.Desenhar(parado), 8, Fundo), Path.Combine(pasta, "parado-8x.png"));
        Salvar(Ampliada(Icone.Desenhar(), 16, Fundo), Path.Combine(pasta, "icone-16x.png"));
        Salvar(Ampliada(BonecoPixel.Desenhar(poses.First(p => p.Nome == "andando-1")), 8, Fundo), Path.Combine(pasta, "andando-8x.png"));
        Salvar(Grade([.. poses.Select(p => (p.Nome, BonecoPixel.Desenhar(p)))], 4, 6, Fundo), Path.Combine(pasta, "poses-claro.png"));
        Salvar(Grade([.. poses.Select(p => (p.Nome, BonecoPixel.Desenhar(p)))], 4, 6, FundoEscuro), Path.Combine(pasta, "poses-escuro.png"));
        Salvar(Grade([.. Rostos.Expressoes.Keys.Select(x => (x, Recortar(BonecoPixel.Desenhar(parado, x), 12, 0, 40, 32)))], 8, 7, Fundo), Path.Combine(pasta, "expressoes.png"));
        Salvar(Grade([.. poses.Select(p => (p.Nome, BonecoPixel.Desenhar(p)))], 2, 10, Fundo), Path.Combine(pasta, "tamanho-real.png"));

        // A folha nativa (64 × 64 por quadro) é o arquivo que a Fase 6 usa; a ampliação é só prévia.
        string folha = Path.Combine(raiz, "assets", "identidade", "pixel", "buzzy-poses.png");
        Salvar(Grade([.. poses.Select(p => (p.Nome, BonecoPixel.Desenhar(p)))], 1, poses.Count, 0x00000000, rotulos: false), folha);
        return problemas;
    }

    private static Tela Recortar(Tela t, int x, int y, int w, int h)
    {
        var r = new Tela(w, h);
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
                r[i, j] = t[x + i, y + j];
        return r;
    }

    private static BitmapSource Ampliada(Tela t, int escala, uint fundo)
    {
        int w = t.Largura * escala, h = t.Altura * escala;
        var px = new uint[w * h];
        uint[] origem = t.ParaArgb();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                uint c = origem[y / escala * t.Largura + x / escala];
                px[y * w + x] = (c >> 24) == 0 ? fundo : c;
            }
        return BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, px, w * 4);
    }

    /// <summary>Grade de telas ampliadas, com rótulo embaixo de cada uma.</summary>
    private static BitmapSource Grade(IReadOnlyList<(string Nome, Tela Tela)> itens, int escala, int colunas, uint fundo, bool rotulos = true)
    {
        int cw = itens.Max(i => i.Tela.Largura) * escala, ch = itens.Max(i => i.Tela.Altura) * escala;
        int margem = rotulos ? 10 : 0, rotulo = rotulos ? 18 : 0;
        int linhas = (itens.Count + colunas - 1) / colunas;
        int w = colunas * (cw + margem) + margem, h = linhas * (ch + margem + rotulo) + margem;
        var visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            dc.DrawRectangle(new SolidColorBrush(ParaCor(fundo)), null, new Rect(0, 0, w, h));
            for (int i = 0; i < itens.Count; i++)
            {
                int x = margem + i % colunas * (cw + margem), y = margem + i / colunas * (ch + margem + rotulo);
                BitmapSource img = Ampliada(itens[i].Tela, escala, fundo);
                dc.DrawImage(img, new Rect(x, y, img.PixelWidth, img.PixelHeight));
                if (rotulos)
                {
                    var texto = new FormattedText(itens[i].Nome, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"), 12, new SolidColorBrush((fundo & 0xFFFFFF) < 0x808080 ? Colors.Gainsboro : Color.FromRgb(0x33, 0x33, 0x40)), 1.0);
                    dc.DrawText(texto, new Point(x, y + ch + 2));
                }
            }
        }
        var alvo = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
        RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.NearestNeighbor);
        alvo.Render(visual);
        return alvo;
    }

    private static Color ParaCor(uint argb) => Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    private static void Salvar(BitmapSource bmp, string caminho)
    {
        var png = new PngBitmapEncoder();
        png.Frames.Add(BitmapFrame.Create(bmp));
        using FileStream f = File.Create(caminho);
        png.Save(f);
        Console.WriteLine($"  {caminho}");
    }
}

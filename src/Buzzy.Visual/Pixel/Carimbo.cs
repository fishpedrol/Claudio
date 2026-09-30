namespace Buzzy.Visual.Pixel;

/// <summary>
/// Pequena imagem desenhada pixel a pixel em texto: cada caractere é uma cor da legenda e
/// <c>.</c> deixa o pixel de baixo como está. Usada para os detalhes que a geometria não resolve
/// bem em poucos pixels: olhos, sobrancelhas, nariz, bocas, dedos.
/// </summary>
public sealed class Carimbo
{
    /// <summary>Legenda padrão dos carimbos.</summary>
    public static readonly IReadOnlyDictionary<char, Cor> Legenda = new Dictionary<char, Cor>
    {
        ['K'] = Cor.Contorno,
        ['e'] = Cor.PeloEscuro,
        ['p'] = Cor.Pelo,
        ['c'] = Cor.Creme,
        ['s'] = Cor.CremeSombra,
        ['C'] = Cor.CremeClaro,
        ['o'] = Cor.Pessego,
        ['O'] = Cor.PessegoEscuro,
        ['i'] = Cor.Iris,
        ['I'] = Cor.IrisClara,
        ['u'] = Cor.Pupila,
        ['W'] = Cor.Branco,
        ['b'] = Cor.Boca,
        ['l'] = Cor.Lingua,
        ['r'] = Cor.Bochecha,
        ['v'] = Cor.Sobrancelha,
        ['h'] = Cor.Palha,
        ['H'] = Cor.PalhaClara,
        ['j'] = Cor.PalhaEscura,
        ['f'] = Cor.Faixa,
        ['F'] = Cor.FaixaEscura,
    };

    private readonly Cor?[] _pixels;

    public Carimbo(params string[] linhas)
    {
        ArgumentNullException.ThrowIfNull(linhas);
        if (linhas.Length == 0) throw new ArgumentException("Carimbo vazio.", nameof(linhas));
        Largura = linhas.Max(l => l.Length);
        Altura = linhas.Length;
        _pixels = new Cor?[Largura * Altura];
        for (int y = 0; y < Altura; y++)
        {
            for (int x = 0; x < linhas[y].Length; x++)
            {
                char c = linhas[y][x];
                if (c == '.' || c == ' ') continue;
                _pixels[y * Largura + x] = Legenda.TryGetValue(c, out Cor cor)
                    ? cor
                    : throw new ArgumentException($"Caractere '{c}' fora da legenda na linha {y}.", nameof(linhas));
            }
        }
    }

    public int Largura { get; }

    public int Altura { get; }

    public Cor? this[int x, int y] => x >= 0 && y >= 0 && x < Largura && y < Altura ? _pixels[y * Largura + x] : null;
}

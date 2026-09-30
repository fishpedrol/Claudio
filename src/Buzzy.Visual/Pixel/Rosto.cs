namespace Buzzy.Visual.Pixel;

/// <summary>Um rosto: olhos, sobrancelhas, boca, topete e bochechas, para as vistas de frente e de perfil.</summary>
public sealed record Rosto(
    string OlhoE,
    string OlhoD,
    string Sobrancelhas,
    string Boca,
    Topete Topete,
    string OlhoPerfil,
    string BocaPerfil,
    bool Corado = false);

/// <summary>Estado do tufo de pelo do alto da cabeça, que reage à emoção.</summary>
public enum Topete
{
    Normal,
    Ericado,
    Caido,
}

/// <summary>
/// Carimbos do rosto na escala nativa (1 pixel = 2 DIP a 100%) e as 14 expressões de
/// docs/IDENTIDADE_VISUAL.md. Os olhos grandes, castanhos e com dois brilhos seguem as pranchas.
/// </summary>
public static class Rostos
{
    // ------------------------------------------------------------------ olhos de frente (7 × 8)
    // Olho da esquerda da tela; o da direita usa o mesmo desenho (o brilho fica do mesmo lado nos
    // dois, como a luz das pranchas).
    public static readonly IReadOnlyDictionary<string, Carimbo> Olhos = new Dictionary<string, Carimbo>
    {
        ["aberto"] = new(
            ".KKKKK.",
            "KKWWWKK",
            "KWiiiiK",
            "KWWuiiK",
            "KiuuuiK",
            "KiiuuIK",
            "KWiiIWK",
            ".KKKKK."),
        ["lado"] = new(
            ".KKKKK.",
            "KKWWWKK",
            "KWWiiiK",
            "KWiWuiK",
            "KWiuuuK",
            "KWIuuiK",
            "KWWiiIK",
            ".KKKKK."),
        ["cima"] = new(
            ".KKKKK.",
            "KKiiiKK",
            "KiWuuiK",
            "KiuuuiK",
            "KIiiiIK",
            "KWWWWWK",
            "KWWWWWK",
            ".KKKKK."),
        ["arregalado"] = new(
            ".KKKKK.",
            "KWWWWWK",
            "KWWiiWK",
            "KWiWuiK",
            "KWiuuiK",
            "KWWiiWK",
            "KWWWWWK",
            ".KKKKK."),
        ["estrela"] = new(
            ".KKKKK.",
            "KKWWWKK",
            "KWiWiiK",
            "KWWWWiK",
            "KiiWiuK",
            "KiiuuIK",
            "KWiiIWK",
            ".KKKKK."),
        ["feliz"] = new(
            ".......",
            ".......",
            "..KKK..",
            ".K...K.",
            "K.....K",
            ".......",
            ".......",
            "......."),
        ["fechado"] = new(
            ".......",
            ".......",
            ".......",
            ".......",
            "K.....K",
            ".KKKKK.",
            ".......",
            "......."),
        ["sonolento"] = new(
            ".......",
            ".......",
            ".......",
            ".KKKKK.",
            "KpppppK",
            "KiuuuiK",
            "KWiiIWK",
            ".KKKKK."),
        ["entediado"] = new(
            ".......",
            ".......",
            ".......",
            ".KKKKK.",
            "KpppppK",
            "KWiuuiK",
            "KWWiiWK",
            ".KKKKK."),
        ["piscada"] = new(
            ".......",
            ".......",
            ".......",
            "K.....K",
            ".KKKKK.",
            ".......",
            ".......",
            "......."),
    };

    // ------------------------------------------------------------------ olhos de perfil (5 × 8)
    public static readonly IReadOnlyDictionary<string, Carimbo> OlhosPerfil = new Dictionary<string, Carimbo>
    {
        ["aberto"] = new(
            ".KKK.",
            "KKWWK",
            "KWiiK",
            "KWuiK",
            "KiuuK",
            "KiuIK",
            "KWiWK",
            ".KKK."),
        ["feliz"] = new(
            ".....",
            ".....",
            ".KKK.",
            "K...K",
            ".....",
            ".....",
            ".....",
            "....."),
        ["arregalado"] = new(
            ".KKK.",
            "KWWWK",
            "KWiWK",
            "KWuiK",
            "KWuiK",
            "KWiWK",
            "KWWWK",
            ".KKK."),
        ["concentrado"] = new(
            ".....",
            ".KKKK",
            "KKWWK",
            "KWuiK",
            "KiuuK",
            "KiuIK",
            "KWiWK",
            ".KKK."),
    };

    // ------------------------------------------------------------------ sobrancelhas (pares, 17 × 3)
    // Desenhadas sobre a máscara do rosto, acima dos olhos, como um só carimbo.
    public static readonly IReadOnlyDictionary<string, Carimbo> Sobrancelhas = new Dictionary<string, Carimbo>
    {
        ["neutras"] = new(
            "..vvv.....vvv..",
            ".v...........v.",
            "..............."),
        ["erguidas"] = new(
            ".vvvv.....vvvv.",
            "v.............v",
            "..............."),
        ["uma-erguida"] = new(
            "..........vvv..",
            "..vvv....v...v.",
            ".v............."),
        ["preocupadas"] = new(
            "....v.....v....",
            "..vv.......vv..",
            ".v...........v."),
        ["bravas"] = new(
            ".v...........v.",
            "..vv.......vv..",
            "....v.....v...."),
        ["caidas"] = new(
            "...............",
            "..vvv.....vvv..",
            ".v...........v."),
        ["retas"] = new(
            "...............",
            ".vvvv.....vvvv.",
            "..............."),
        ["nenhuma"] = new("..............."),
    };

    // ------------------------------------------------------------------ bocas de frente (9 × 4)
    public static readonly IReadOnlyDictionary<string, Carimbo> Bocas = new Dictionary<string, Carimbo>
    {
        ["sorriso"] = new(
            ".K.....K.",
            "..KKKKK..",
            ".........",
            "........."),
        ["firme"] = new(
            ".........",
            "..KKKKK..",
            ".........",
            "........."),
        ["aberta"] = new(
            ".KKKKKKK.",
            ".KbbbbbK.",
            "..KblbK..",
            "...KKK..."),
        ["risada"] = new(
            "KKKKKKKKK",
            "KWbbbbbWK",
            ".KbblbbK.",
            "..KKKKK.."),
        ["canto"] = new(
            ".........",
            "......K..",
            "..KKKK...",
            "........."),
        ["o"] = new(
            "...KKK...",
            "..KbbbK..",
            "..KbbbK..",
            "...KKK..."),
        ["ondulada"] = new(
            ".........",
            "..K.K.K..",
            ".K.K.K.K.",
            "........."),
        ["reta"] = new(
            ".........",
            "..KKKKK..",
            ".........",
            "........."),
        ["reta-pequena"] = new(
            ".........",
            "...KKK...",
            ".........",
            "........."),
        ["lingua"] = new(
            ".K.....K.",
            "..KKKKK..",
            "....KlK..",
            ".....K..."),
        ["bocejo"] = new(
            "..KKKKK..",
            ".KbbbbbK.",
            ".KbblbbK.",
            "..KKKKK.."),
    };

    // ------------------------------------------------------------------ bocas de perfil (5 × 4)
    public static readonly IReadOnlyDictionary<string, Carimbo> BocasPerfil = new Dictionary<string, Carimbo>
    {
        ["sorriso"] = new(
            "....K",
            "KKKK.",
            ".....",
            "....."),
        ["aberta"] = new(
            ".KKKK",
            "KbbbK",
            ".KlK.",
            "..K.."),
        ["o"] = new(
            "..KK.",
            ".KbbK",
            "..KK.",
            "....."),
        ["reta"] = new(
            ".....",
            ".KKKK",
            ".....",
            "....."),
    };

    /// <summary>As 14 expressões (docs/IDENTIDADE_VISUAL.md, seção 6).</summary>
    public static readonly IReadOnlyDictionary<string, Rosto> Expressoes = new Dictionary<string, Rosto>
    {
        ["neutro"] = new("aberto", "aberto", "neutras", "sorriso", Topete.Normal, "aberto", "sorriso"),
        ["feliz"] = new("feliz", "feliz", "neutras", "aberta", Topete.Normal, "feliz", "aberta", Corado: true),
        ["rindo"] = new("feliz", "feliz", "erguidas", "risada", Topete.Ericado, "feliz", "aberta", Corado: true),
        ["curioso"] = new("lado", "lado", "uma-erguida", "canto", Topete.Normal, "aberto", "reta"),
        ["surpreso"] = new("arregalado", "arregalado", "erguidas", "o", Topete.Ericado, "arregalado", "o"),
        ["assustado"] = new("arregalado", "arregalado", "preocupadas", "ondulada", Topete.Ericado, "arregalado", "o"),
        ["sonolento"] = new("sonolento", "sonolento", "caidas", "reta", Topete.Caido, "aberto", "reta"),
        ["bocejando"] = new("fechado", "fechado", "caidas", "bocejo", Topete.Caido, "feliz", "o"),
        ["dormindo"] = new("fechado", "fechado", "nenhuma", "reta-pequena", Topete.Caido, "feliz", "reta"),
        ["travesso"] = new("aberto", "piscada", "uma-erguida", "lingua", Topete.Normal, "aberto", "sorriso"),
        ["entediado"] = new("entediado", "entediado", "retas", "reta", Topete.Caido, "aberto", "reta"),
        ["pensativo"] = new("cima", "cima", "uma-erguida", "canto", Topete.Normal, "aberto", "reta"),
        ["empolgado"] = new("estrela", "estrela", "erguidas", "aberta", Topete.Ericado, "aberto", "aberta", Corado: true),
        ["determinado"] = new("aberto", "aberto", "bravas", "firme", Topete.Normal, "concentrado", "reta"),
    };
}

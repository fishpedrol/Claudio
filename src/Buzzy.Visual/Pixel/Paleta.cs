namespace Buzzy.Visual.Pixel;

/// <summary>
/// Cores da pixel art do Buzzy. Os tons-base vêm das pranchas de referência
/// (assets/references/): azul-marinho do pelo #283A5F, creme #FDD5A6, pêssego #F6996D e castanho
/// #9A5236, lidos da paleta da prancha, e o chapéu de palha com a faixa vermelha #B83A37 (DEC-019).
/// </summary>
public enum Cor : byte
{
    Nada = 0,
    Contorno,
    PeloEscuro,
    Pelo,
    PeloClaro,
    Creme,
    CremeSombra,
    CremeClaro,
    Pessego,
    PessegoEscuro,
    Iris,
    IrisClara,
    Pupila,
    Branco,
    Boca,
    Lingua,
    Bochecha,
    Sobrancelha,
    Palha,
    PalhaClara,
    PalhaEscura,
    Faixa,
    FaixaEscura,
}

public static class Paleta
{
    /// <summary>Cor em ARGB (0xAARRGGBB). <see cref="Cor.Nada"/> é totalmente transparente.</summary>
    public static uint Argb(Cor cor) => cor switch
    {
        Cor.Nada => 0x00000000,
        Cor.Contorno => 0xFF121830,
        Cor.PeloEscuro => 0xFF1B2748,
        Cor.Pelo => 0xFF283A5F,
        Cor.PeloClaro => 0xFF3B5486,
        Cor.Creme => 0xFFFDD5A6,
        Cor.CremeSombra => 0xFFE6B083,
        Cor.CremeClaro => 0xFFFFE9CC,
        Cor.Pessego => 0xFFF6996D,
        Cor.PessegoEscuro => 0xFFD2704A,
        Cor.Iris => 0xFF6A3419,
        Cor.IrisClara => 0xFF9A5236,
        Cor.Pupila => 0xFF140A06,
        Cor.Branco => 0xFFFFFFFF,
        Cor.Boca => 0xFF5A1D22,
        Cor.Lingua => 0xFFE8727A,
        Cor.Bochecha => 0xFFF4A987,
        Cor.Sobrancelha => 0xFF2E2230,
        Cor.Palha => 0xFFEFB262,
        Cor.PalhaClara => 0xFFFCCB7E,
        Cor.PalhaEscura => 0xFFC27F45,
        Cor.Faixa => 0xFFB83A37,
        Cor.FaixaEscura => 0xFF862A2B,
        _ => throw new ArgumentOutOfRangeException(nameof(cor), cor, "Cor fora da paleta."),
    };

    /// <summary>Nome da cor e o valor em hexadecimal, para a folha de modelo.</summary>
    public static string Hex(Cor cor) => $"#{Argb(cor) & 0xFFFFFF:X6}";
}

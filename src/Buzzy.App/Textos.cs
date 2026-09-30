using System.Globalization;
using System.Resources;

namespace Buzzy.App;

/// <summary>Acesso aos textos de <c>Textos.resx</c> (Q-12: textos fora do código).</summary>
internal static class Textos
{
    private static readonly ResourceManager Recursos = new("Buzzy.App.Textos", typeof(Textos).Assembly);

    internal static string MenuEsconder => Obter(nameof(MenuEsconder));
    internal static string MenuMostrar => Obter(nameof(MenuMostrar));
    internal static string MenuPausar => Obter(nameof(MenuPausar));
    internal static string MenuRetomar => Obter(nameof(MenuRetomar));
    internal static string MenuSair => Obter(nameof(MenuSair));
    internal static string DicaDaBandeja => Obter(nameof(DicaDaBandeja));
    internal static string AvisoElevado => Obter(nameof(AvisoElevado));

    /// <summary>Todas as chaves usadas pelo aplicativo, para o teste que confere se nenhuma falta.</summary>
    internal static IReadOnlyList<string> Chaves { get; } =
        [nameof(MenuEsconder), nameof(MenuMostrar), nameof(MenuPausar), nameof(MenuRetomar), nameof(MenuSair), nameof(DicaDaBandeja), nameof(AvisoElevado)];

    internal static string Obter(string chave)
        => Recursos.GetString(chave, CultureInfo.InvariantCulture)
           ?? throw new InvalidOperationException($"Texto ausente em Textos.resx: {chave}");
}

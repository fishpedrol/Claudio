using System.Globalization;
using System.Resources;
using Buzzy.Core.Personagem;

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

    /// <summary>O submenu da emoção dominante (DEC-027).</summary>
    internal static string MenuEmocaoDominante => Obter(nameof(MenuEmocaoDominante));

    /// <summary>A opção "Automática" da emoção dominante: o humor varia como antes da escolha.</summary>
    internal static string MenuEmocaoAutomatica => Obter(nameof(MenuEmocaoAutomatica));

    /// <summary>
    /// O nome de uma das 14 caras de humor (<see cref="Expressoes.DeHumor"/>) no submenu da emoção dominante, com a
    /// tecla de acesso. As caras de efeito do tamagotchi não são emoção dominante: pedir uma delas é erro.
    /// </summary>
    internal static string Emocao(Expressao emocao)
        => Expressoes.EhDeHumor(emocao)
            ? Obter(ChaveDaEmocao(emocao))
            : throw new ArgumentOutOfRangeException(nameof(emocao), emocao, "A emoção dominante é uma das 14 caras de humor.");

    /// <summary>O submenu dos itens do tamagotchi (DEC-028); só existe com a chave dele ligada.</summary>
    internal static string MenuItens => Obter(nameof(MenuItens));

    /// <summary>"Recolher itens": todos os itens saem da tela (CMD_CLEAR_ITEMS).</summary>
    internal static string MenuRecolherItens => Obter(nameof(MenuRecolherItens));

    /// <summary>
    /// O nome de um item do tamagotchi (DEC-028) no submenu "Itens", com a tecla de acesso. Só o nome: o menu não descreve
    /// nada. Um valor fora do enum é erro.
    /// </summary>
    internal static string Item(Item item)
        => Enum.IsDefined(item)
            ? Obter(ChaveDoItem(item))
            : throw new ArgumentOutOfRangeException(nameof(item), item, "Item fora do enum.");

    /// <summary>Todas as chaves usadas pelo aplicativo, para o teste que confere se nenhuma falta.</summary>
    internal static IReadOnlyList<string> Chaves { get; } =
    [
        nameof(MenuEsconder), nameof(MenuMostrar), nameof(MenuPausar), nameof(MenuRetomar), nameof(MenuSair), nameof(DicaDaBandeja), nameof(AvisoElevado),
        nameof(MenuEmocaoDominante), nameof(MenuEmocaoAutomatica), .. Expressoes.DeHumor.Select(ChaveDaEmocao),
        nameof(MenuItens), nameof(MenuRecolherItens), .. TabelaDoTamagotchi.Itens.Select(ChaveDoItem),
    ];

    internal static string Obter(string chave)
        => Recursos.GetString(chave, CultureInfo.InvariantCulture)
           ?? throw new InvalidOperationException($"Texto ausente em Textos.resx: {chave}");

    /// <summary>A chave do nome de uma cara de humor no .resx: <c>Emocao</c> seguido do nome do valor (<c>EmocaoFeliz</c>).</summary>
    private static string ChaveDaEmocao(Expressao emocao) => "Emocao" + emocao;

    /// <summary>A chave do nome de um item no .resx: <c>Item</c> seguido do nome do valor (<c>ItemBanana</c>).</summary>
    private static string ChaveDoItem(Item item) => "Item" + item;
}

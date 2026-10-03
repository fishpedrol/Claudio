namespace Buzzy.PortaoApis;

/// <summary>
/// Uso de uma função da lista proibida pelo próprio Buzzy, com decisão aprovada e restrito a um lugar só.
/// </summary>
/// <param name="Modulo">Módulo normalizado (<see cref="ListaProibida.NormalizarModulo"/>).</param>
/// <param name="Funcao">Nome exato da função, sem as variantes A e W que a lista proibida aceita.</param>
/// <param name="Assembly">O assembly que pode declarar o P/Invoke, sem extensão.</param>
/// <param name="Tipo">O tipo que declara o P/Invoke: ele ou um tipo aninhado nele (<c>Tipo+Nativo</c>).</param>
/// <param name="Arquivo">O único arquivo de fonte que pode citar a função, relativo à raiz do repositório.</param>
/// <param name="Motivo">A decisão e os limites; aparece no relatório.</param>
internal sealed record UsoRestrito(string Modulo, string Funcao, string Assembly, string Tipo, string Arquivo, string Motivo);

/// <summary>Um P/Invoke que coincide com a lista proibida e está na lista de usos restritos.</summary>
/// <param name="Arquivo">O binário.</param>
/// <param name="Api">Como aparece nos metadados: <c>user32.dll!SetWinEventHook</c>.</param>
/// <param name="Onde">O método que declara o P/Invoke.</param>
internal sealed record UsoRestritoVisto(string Arquivo, string Api, string Onde, Categoria Categoria, UsoRestrito Uso);

/// <summary>
/// Os usos restritos (SECURITY.md 3.1 e 8, item 1): funções que continuam na lista proibida, para o resto do produto, e que
/// um único tipo do Buzzy pode usar, com decisão aprovada. Hoje, só o observador de tela cheia (DEC-013 e DEC-034): ele
/// assina, fora do processo, a troca da janela em primeiro plano e a mudança de geometria dela, e lê só o retângulo dela.
///
/// Regras de uso:
/// - nos binários, vale só para o P/Invoke do módulo e da função exatos, declarado no tipo indicado, ou num tipo aninhado
///   nele, do assembly indicado; declarado em outro tipo ou outro assembly, o P/Invoke reprova;
/// - na fonte, vale só no arquivo indicado (o caminho completo termina nele, sem diferenciar maiúsculas nem o tipo da
///   barra); citada em outro arquivo, a função reprova, como sempre;
/// - cada P/Invoke permitido aparece no relatório como "uso restrito";
/// - outra função, outro tipo ou outro arquivo exigem decisão nova e uma entrada aqui, com o motivo.
/// </summary>
internal static class UsosRestritos
{
    private const string Observador = "Buzzy.App.Plataforma.ObservadorDeTelaCheia";
    private const string ArquivoDoObservador = @"src\Buzzy.App\Plataforma\ObservadorDeTelaCheia.cs";

    public static readonly IReadOnlyList<UsoRestrito> Entradas =
    [
        new("user32", "SetWinEventHook", "Buzzy", Observador, ArquivoDoObservador,
            "DEC-013 e DEC-034: o observador de tela cheia assina, fora do processo e sem o próprio processo, a troca da janela em primeiro plano (o sistema todo) e a mudança de geometria só na thread dela; nenhum evento de input"),
        new("user32", "GetForegroundWindow", "Buzzy", Observador, ArquivoDoObservador,
            "DEC-013 e DEC-034: a janela em primeiro plano, para ler só o retângulo dela e saber quais monitores ela cobre; o identificador não é gravado nem vai ao log"),
        new("user32", "GetWindowThreadProcessId", "Buzzy", Observador, ArquivoDoObservador,
            "DEC-034: só a thread da janela em primeiro plano, para restringir a assinatura de geometria a ela; o processo nunca é pedido (o ponteiro dele vai nulo)"),
    ];

    /// <summary>O uso restrito deste P/Invoke, ou nulo: módulo e função exatos, no assembly e no tipo indicados.</summary>
    public static UsoRestrito? NoBinario(string assembly, string modulo, string funcao, string tipoQueDeclara)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(modulo);
        ArgumentNullException.ThrowIfNull(funcao);
        ArgumentNullException.ThrowIfNull(tipoQueDeclara);
        string moduloNormalizado = ListaProibida.NormalizarModulo(modulo);
        return Entradas.FirstOrDefault(u =>
            string.Equals(u.Modulo, moduloNormalizado, StringComparison.Ordinal)
            && string.Equals(u.Funcao, funcao, StringComparison.Ordinal)
            && string.Equals(u.Assembly, assembly, StringComparison.Ordinal)
            && (string.Equals(u.Tipo, tipoQueDeclara, StringComparison.Ordinal)
                || tipoQueDeclara.StartsWith(u.Tipo + "+", StringComparison.Ordinal)));
    }

    /// <summary>O uso restrito que permite citar a função desta regra neste arquivo de fonte, ou nulo.</summary>
    public static UsoRestrito? NaFonte(string arquivo, Regra regra)
    {
        ArgumentNullException.ThrowIfNull(arquivo);
        ArgumentNullException.ThrowIfNull(regra);
        if (regra.Tipo != TipoDeRegra.FuncaoNativa) return null;
        string caminho = arquivo.Replace('/', '\\');
        return Entradas.FirstOrDefault(u =>
            string.Equals(u.Funcao, regra.Alvo, StringComparison.OrdinalIgnoreCase)
            && caminho.EndsWith(@"\" + u.Arquivo, StringComparison.OrdinalIgnoreCase));
    }
}

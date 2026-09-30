using Buzzy.Testes;

namespace Buzzy.App.Testes;

internal static class Programa
{
    /// <summary>
    /// Executor dos testes; com <c>--gravar-sem-parar PASTA</c>, o gravador que o teste de queda inicia e
    /// encerra à força (<see cref="GravadorSemParar"/>).
    /// </summary>
    [STAThread]
    internal static int Main(string[] args) => args is [GravadorSemParar.Opcao, var pasta]
        ? GravadorSemParar.Executar(pasta)
        : Executor.Executar(typeof(Programa).Assembly, args);
}

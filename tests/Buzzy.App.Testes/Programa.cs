using Buzzy.Testes;

namespace Buzzy.App.Testes;

internal static class Programa
{
    [STAThread]
    internal static int Main(string[] args) => Executor.Executar(typeof(Programa).Assembly, args);
}

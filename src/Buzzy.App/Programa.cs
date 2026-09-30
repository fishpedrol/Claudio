using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Buzzy.App.Composicao;
using Buzzy.App.Plataforma;

namespace Buzzy.App;

/// <summary>
/// Ponto de entrada do Buzzy.
///
/// Uso: <c>Buzzy.exe [--diagnostico]</c>. <c>--diagnostico</c> liga o log em
/// <c>%LOCALAPPDATA%\Buzzy\diagnostico.log</c>; sem ele, o Buzzy não grava nada.
/// </summary>
internal static class Programa
{
    [STAThread]
    internal static int Main(string[] argumentos)
    {
        if (argumentos.Contains("--diagnostico", StringComparer.Ordinal))
            Diagnostico.Ligar();

        Diagnostico.Evento("INICIO",
            ("pid", Environment.ProcessId),
            ("versao", Assembly.GetExecutingAssembly().GetName().Version),
            ("runtime", RuntimeInformation.FrameworkDescription),
            ("so", RuntimeInformation.OSDescription));

        // SECURITY.md 8, item 5: o Buzzy não usa privilégio de administrador. Iniciado elevado
        // (por um terminal de administrador ou "Executar como administrador"), ele avisa e
        // sai antes de criar janelas ou objetos nomeados.
        if (Environment.IsPrivilegedProcess)
        {
            Diagnostico.Evento("ELEVADO", ("recusado", "sim"));
            MessageBox.Show(Textos.AvisoElevado, Textos.DicaDaBandeja, MessageBoxButton.OK, MessageBoxImage.Information);
            Diagnostico.Evento("FIM", ("codigo", CodigosDeSaida.Elevado), ("pid", Environment.ProcessId));
            return CodigosDeSaida.Elevado;
        }

        InstanciaUnica instancia;
        try
        {
            instancia = InstanciaUnica.Obter();
        }
        catch (Exception e) when (e is UnauthorizedAccessException or WaitHandleCannotBeOpenedException or IOException)
        {
            Diagnostico.Evento("INSTANCIA", ("erro", e.GetType().Name), ("mensagem", e.Message));
            Diagnostico.Evento("FIM", ("codigo", CodigosDeSaida.InstanciaUnicaIndisponivel), ("pid", Environment.ProcessId));
            return CodigosDeSaida.InstanciaUnicaIndisponivel;
        }

        using (instancia)
        {
            if (!instancia.EhPrimeira)
            {
                bool entregue = instancia.PedirParaAPrimeiraAparecer(out string? erro);
                Diagnostico.Evento("INSTANCIA", ("papel", "segunda"), ("pid", Environment.ProcessId), ("pedidoEntregue", entregue), ("erro", erro ?? ""));
                int codigoSegunda = entregue ? CodigosDeSaida.Normal : CodigosDeSaida.InstanciaUnicaIndisponivel;
                Diagnostico.Evento("FIM", ("codigo", codigoSegunda), ("pid", Environment.ProcessId));
                return codigoSegunda;
            }

            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            var aplicacao = new Aplicacao(app, instancia);
            app.Startup += (_, _) => aplicacao.Iniciar();
            int codigo = app.Run();

            Diagnostico.Evento("FIM", ("codigo", codigo), ("pid", Environment.ProcessId));
            return codigo;
        }
    }
}

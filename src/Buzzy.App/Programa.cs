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
/// Uso: <c>Buzzy.exe [--diagnostico] [--pausado] [--semente N] [--perfil-de-teste NOME]</c>.
/// <list type="bullet">
/// <item><c>--diagnostico</c> liga o log em <c>%LOCALAPPDATA%\Buzzy\diagnostico.log</c>; sem ele, o
/// Buzzy não grava nada.</item>
/// <item><c>--pausado</c> começa com o movimento autônomo pausado (o mesmo que "Pausar movimento" no
/// menu); as verificações de tela usam para ter o personagem parado no lugar inicial.</item>
/// <item><c>--semente N</c> fixa a semente da agenda autônoma, para reproduzir um comportamento.</item>
/// <item><c>--perfil-de-teste NOME</c> isola os dados do Buzzy em <c>%LOCALAPPDATA%\Buzzy\testes\NOME</c>
/// (Fase 5): os testes e as ferramentas que abrem o Buzzy usam, para nunca tocar nas configurações reais
/// do usuário. NOME tem de 1 a 32 caracteres entre a–z, 0–9 e hífen (sem começar por hífen) e não pode
/// ser um nome reservado do Windows; sem nome ou com um inválido, a persistência fica desligada nesta
/// execução.</item>
/// </list>
/// </summary>
internal static class Programa
{
    [STAThread]
    internal static int Main(string[] argumentos)
    {
        if (argumentos.Contains("--diagnostico", StringComparer.Ordinal))
            Diagnostico.Ligar();
        OpcoesDaAplicacao opcoes = LerOpcoes(argumentos);

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
            var aplicacao = new Aplicacao(app, instancia, opcoes);
            app.Startup += (_, _) => aplicacao.Iniciar();
            int codigo = app.Run();

            Diagnostico.Evento("FIM", ("codigo", codigo), ("pid", Environment.ProcessId));
            return codigo;
        }
    }

    /// <summary>Opções da linha de comando; um valor ilegível é ignorado e registrado no diagnóstico.</summary>
    internal static OpcoesDaAplicacao LerOpcoes(string[] argumentos)
    {
        bool pausado = argumentos.Contains("--pausado", StringComparer.Ordinal);
        ulong? semente = null;
        int i = Array.IndexOf(argumentos, "--semente");
        if (i >= 0)
        {
            if (i + 1 < argumentos.Length && ulong.TryParse(argumentos[i + 1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out ulong valor))
                semente = valor;
            else
                Diagnostico.Evento("ARGUMENTO", ("ignorado", "--semente"), ("motivo", "falta um número inteiro sem sinal"));
        }

        // Perfil de teste: um nome inválido nunca cai na pasta real do usuário, desliga a persistência.
        // O nome recusado não vai para o log: pode ser um caminho.
        string? perfil = null;
        bool persistenciaDesligada = false;
        int p = Array.IndexOf(argumentos, "--perfil-de-teste");
        if (p >= 0)
        {
            if (p + 1 < argumentos.Length && PastaDeDados.NomeDePerfilValido(argumentos[p + 1]))
            {
                perfil = argumentos[p + 1];
            }
            else
            {
                persistenciaDesligada = true;
                Diagnostico.Evento("ARGUMENTO", ("ignorado", "--perfil-de-teste"),
                    ("motivo", p + 1 < argumentos.Length ? "nome inválido; persistência desligada" : "falta o nome; persistência desligada"));
            }
        }
        return new OpcoesDaAplicacao(pausado, semente, perfil, persistenciaDesligada);
    }
}

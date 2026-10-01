using System.Text.RegularExpressions;
using Buzzy.Testes;

namespace Buzzy.PortaoApis.Testes;

/// <summary>A tabela única de regras: conteúdo mínimo exigido, integridade e normalização de nomes.</summary>
public sealed class TestesDaListaProibida
{
    // Lista mínima de SECURITY.md 3.2 pedida para a Fase 1, com a categoria de cada item.
    private static readonly (string Nome, Categoria Categoria)[] FuncoesMinimas =
    [
        ("SetWindowsHookEx", Categoria.InputGlobal), ("RegisterRawInputDevices", Categoria.InputGlobal),
        ("GetAsyncKeyState", Categoria.InputGlobal), ("GetKeyboardState", Categoria.InputGlobal),
        ("GetKeyState", Categoria.InputGlobal), ("RegisterHotKey", Categoria.InputGlobal),
        ("SendInput", Categoria.InjetarInput), ("mouse_event", Categoria.InjetarInput), ("keybd_event", Categoria.InjetarInput),
        ("BitBlt", Categoria.CapturaDeTela), ("StretchBlt", Categoria.CapturaDeTela), ("PrintWindow", Categoria.CapturaDeTela),
        ("CreateDC", Categoria.CapturaDeTela),
        ("CreateProcess", Categoria.Processos), ("CreateProcessAsUser", Categoria.Processos), ("ShellExecute", Categoria.Processos),
        ("ShellExecuteEx", Categoria.Processos), ("WinExec", Categoria.Processos),
        ("URLDownloadToFile", Categoria.Rede),
        ("EnumWindows", Categoria.LerOutrosAplicativos), ("EnumChildWindows", Categoria.LerOutrosAplicativos),
        ("EnumThreadWindows", Categoria.LerOutrosAplicativos), ("FindWindow", Categoria.LerOutrosAplicativos),
        ("FindWindowEx", Categoria.LerOutrosAplicativos), ("GetWindowText", Categoria.LerOutrosAplicativos),
        ("GetWindowTextLength", Categoria.LerOutrosAplicativos), ("GetClassName", Categoria.LerOutrosAplicativos),
        ("OpenClipboard", Categoria.LerOutrosAplicativos), ("GetClipboardData", Categoria.LerOutrosAplicativos),
        ("OpenProcess", Categoria.LerOutrosAplicativos), ("ReadProcessMemory", Categoria.LerOutrosAplicativos),
        ("EnumProcesses", Categoria.LerOutrosAplicativos), ("CreateToolhelp32Snapshot", Categoria.LerOutrosAplicativos),
        ("QueryFullProcessImageName", Categoria.LerOutrosAplicativos), ("GetModuleFileNameEx", Categoria.LerOutrosAplicativos),
        ("GetForegroundWindow", Categoria.LerOutrosAplicativos), ("WindowFromPoint", Categoria.LerOutrosAplicativos),
        ("SetWinEventHook", Categoria.LerOutrosAplicativos),
        ("RegSetValueEx", Categoria.PersistenciaEscondida), ("RegCreateKeyEx", Categoria.PersistenciaEscondida),
        ("CreateService", Categoria.PersistenciaEscondida), ("OpenSCManager", Categoria.PersistenciaEscondida),
        ("LoadLibrary", Categoria.CodigoDinamico), ("LoadLibraryEx", Categoria.CodigoDinamico), ("GetProcAddress", Categoria.CodigoDinamico),
    ];

    private static readonly string[] ModulosMinimos = ["ws2_32", "wsock32", "winhttp", "wininet"];

    private static readonly (string Tipo, Categoria Categoria)[] TiposMinimos =
    [
        ("System.Diagnostics.ProcessStartInfo", Categoria.Processos),
        ("System.Net.WebClient", Categoria.Rede), ("System.Net.WebRequest", Categoria.Rede), ("System.Net.HttpWebRequest", Categoria.Rede),
        ("System.Windows.Clipboard", Categoria.LerOutrosAplicativos), ("System.Windows.Forms.Clipboard", Categoria.LerOutrosAplicativos),
        ("System.Windows.Automation.AutomationElement", Categoria.LerOutrosAplicativos),
        ("Microsoft.Win32.Registry", Categoria.PersistenciaEscondida), ("Microsoft.Win32.RegistryKey", Categoria.PersistenciaEscondida),
        ("System.Runtime.InteropServices.NativeLibrary", Categoria.CodigoDinamico),
    ];

    private static readonly (string Namespace, Categoria Categoria)[] NamespacesMinimos =
    [
        ("Windows.Graphics.Capture", Categoria.CapturaDeTela),
        ("System.Net.Http", Categoria.Rede), ("System.Net.Sockets", Categoria.Rede), ("System.Net.WebSockets", Categoria.Rede),
        ("System.Reflection.Emit", Categoria.CodigoDinamico),
    ];

    private static readonly (string Tipo, string Membro, Categoria Categoria)[] MembrosMinimos =
    [
        ("System.Drawing.Graphics", "CopyFromScreen", Categoria.CapturaDeTela),
        ("System.Diagnostics.Process", "Start", Categoria.Processos),
        ("System.Diagnostics.Process", "GetProcesses", Categoria.LerOutrosAplicativos),
        ("System.Diagnostics.Process", "GetProcessesByName", Categoria.LerOutrosAplicativos),
        ("System.Diagnostics.Process", "GetProcessById", Categoria.LerOutrosAplicativos),
        ("System.Reflection.Assembly", "Load", Categoria.CodigoDinamico),
        ("System.Reflection.Assembly", "LoadFrom", Categoria.CodigoDinamico),
        ("System.Reflection.Assembly", "LoadFile", Categoria.CodigoDinamico),
        ("System.Reflection.Assembly", "UnsafeLoadFrom", Categoria.CodigoDinamico),
        ("System.Runtime.Loader.AssemblyLoadContext", "LoadFromAssemblyPath", Categoria.CodigoDinamico),
        ("System.Runtime.Loader.AssemblyLoadContext", "LoadFromStream", Categoria.CodigoDinamico),
        ("System.Runtime.InteropServices.Marshal", "GetDelegateForFunctionPointer", Categoria.CodigoDinamico),
    ];

    [Teste]
    public void ContemTodasAsFuncoesNativasDaListaMinima()
    {
        foreach ((string nome, Categoria categoria) in FuncoesMinimas)
        {
            Regra regra = Afirmar.NaoNulo(ListaProibida.ProcurarNativa("qualquer.dll", nome), nome);
            Afirmar.Igual(TipoDeRegra.FuncaoNativa, regra.Tipo, nome);
            Afirmar.Igual(categoria, regra.Categoria, nome);
        }
    }

    [Teste]
    public void ContemOsModulosDeRedeEOsItensGerenciadosDaListaMinima()
    {
        foreach (string modulo in ModulosMinimos)
            Afirmar.Igual(Categoria.Rede, Afirmar.NaoNulo(ListaProibida.ProcurarNativa(modulo, "FuncaoQualquer"), modulo).Categoria, modulo);

        foreach ((string tipo, Categoria categoria) in TiposMinimos)
        {
            int ponto = tipo.LastIndexOf('.');
            Regra regra = Afirmar.NaoNulo(ListaProibida.ProcurarTipo(tipo[..ponto], tipo[(ponto + 1)..]), tipo);
            Afirmar.Igual(categoria, regra.Categoria, tipo);
        }

        foreach ((string nomeDoNamespace, Categoria categoria) in NamespacesMinimos)
        {
            Afirmar.Igual(categoria, Afirmar.NaoNulo(ListaProibida.ProcurarTipo(nomeDoNamespace, "TipoQualquer"), nomeDoNamespace).Categoria);
            Afirmar.Igual(categoria, Afirmar.NaoNulo(ListaProibida.ProcurarTipo(nomeDoNamespace + ".Sub", "TipoQualquer"), nomeDoNamespace + ".Sub").Categoria);
        }

        foreach ((string tipo, string membro, Categoria categoria) in MembrosMinimos)
            Afirmar.Igual(categoria, Afirmar.NaoNulo(ListaProibida.ProcurarMembro(tipo, membro), $"{tipo}.{membro}").Categoria);

        Afirmar.Igual(Categoria.CapturaDeTela, Afirmar.NaoNulo(ListaProibida.ProcurarMetodoCom("DuplicateOutput")).Categoria);
    }

    [Teste]
    public void CadaRegraTemMotivoEAsOitoCategoriasEstaoCobertas()
    {
        foreach (Regra regra in ListaProibida.Regras)
        {
            Afirmar.Verdadeiro(regra.Motivo.Length > 10, $"regra {regra.Descricao} sem motivo");
            Afirmar.Diferente(Categoria.Manifesto, regra.Categoria, regra.Descricao);
            Afirmar.Igual(regra.Tipo == TipoDeRegra.MembroGerenciado, regra.Membro is not null, regra.Descricao);
        }

        Categoria[] categorias = [.. Enum.GetValues<Categoria>().Where(c => c != Categoria.Manifesto)];
        foreach (Categoria categoria in categorias)
            Afirmar.Verdadeiro(ListaProibida.Regras.Any(r => r.Categoria == categoria), $"nenhuma regra para {categoria.Nome()}");
        Afirmar.Igual(9, categorias.Length);
    }

    [Teste]
    public void ConfiguracaoDeVideo_LerEhPermitido_MudarEhProibido()
    {
        // Revisão de segurança do bloco P6-P9, achado 6: o P6 trouxe a família DisplayConfig para a chave estável do monitor,
        // só de leitura. "Nenhuma configuração global alterada" deixa de depender só de revisão: as funções que mudam o vídeo
        // do sistema todo reprovam o build, em qualquer grafia.
        foreach (string funcao in new[] { "SetDisplayConfig", "DisplayConfigSetDeviceInfo", "ChangeDisplaySettings", "ChangeDisplaySettingsA", "ChangeDisplaySettingsW",
            "ChangeDisplaySettingsEx", "ChangeDisplaySettingsExA", "ChangeDisplaySettingsExW", "setdisplayconfig" })
        {
            Regra regra = Afirmar.NaoNulo(ListaProibida.ProcurarNativa("user32.dll", funcao), funcao);
            Afirmar.Igual(Categoria.ConfiguracaoGlobal, regra.Categoria, funcao);
        }
        foreach (string funcao in new[] { "GetDisplayConfigBufferSizes", "QueryDisplayConfig", "DisplayConfigGetDeviceInfo" })
            Afirmar.Nulo(ListaProibida.ProcurarNativa("user32.dll", funcao), funcao);
        Afirmar.Igual("Alterar configuração global", Categoria.ConfiguracaoGlobal.Nome());
    }

    [Teste]
    public void PadroesDeFonteSaoPalavrasOuSequenciasComPonto()
    {
        var formato = new Regex(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$", RegexOptions.CultureInvariant);
        var categoriaPorPadrao = new Dictionary<string, Categoria>(StringComparer.OrdinalIgnoreCase);
        foreach (Regra regra in ListaProibida.Regras)
        {
            foreach (string padrao in regra.PadroesNaFonte)
            {
                Afirmar.Verdadeiro(formato.IsMatch(padrao), $"padrão inválido \"{padrao}\" em {regra.Descricao}");
                // Um padrão repetido em duas regras só é aceitável na mesma categoria (Clipboard).
                if (categoriaPorPadrao.TryGetValue(padrao, out Categoria anterior))
                    Afirmar.Igual(anterior, regra.Categoria, $"padrão \"{padrao}\" em categorias diferentes");
                categoriaPorPadrao[padrao] = regra.Categoria;
            }
        }
    }

    [Teste]
    public void FuncaoNativaIgnoraMaiusculasESufixosAeW()
    {
        Afirmar.Igual("SendInput", ListaProibida.ProcurarNativa("user32.dll", "SendInput")?.Alvo);
        Afirmar.Igual("SendInput", ListaProibida.ProcurarNativa("user32.dll", "SENDINPUT")?.Alvo);
        Afirmar.Igual("SendInput", ListaProibida.ProcurarNativa("user32.dll", "sendinputw")?.Alvo);
        Afirmar.Igual("CreateDC", ListaProibida.ProcurarNativa("gdi32.dll", "CreateDCW")?.Alvo);
        Afirmar.Igual("ShellExecuteEx", ListaProibida.ProcurarNativa("shell32.dll", "ShellExecuteExW")?.Alvo);
        Afirmar.Igual("ShellExecute", ListaProibida.ProcurarNativa("shell32.dll", "ShellExecuteA")?.Alvo);
        Afirmar.Igual("GetWindowTextLength", ListaProibida.ProcurarNativa("user32.dll", "GetWindowTextLengthW")?.Alvo);
        Afirmar.Igual("PrintWindow", ListaProibida.ProcurarNativa("user32.dll", "PrintWindow")?.Alvo);
        Afirmar.Igual("URLDownloadToFile", ListaProibida.ProcurarNativa("urlmon.dll", "URLDownloadToFileW")?.Alvo);

        // Só as variantes: cortar letras finais daria falso positivo.
        Afirmar.Nulo(ListaProibida.ProcurarNativa("user32.dll", "PrintWindo"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("user32.dll", "SendInputX"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("user32.dll", "SendInputWW"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("user32.dll", "SendInpu"));
    }

    [Teste]
    public void ModuloIgnoraMaiusculasCaminhoEExtensao()
    {
        string[] grafias = ["ws2_32", "WS2_32.DLL", "ws2_32.dll", @"C:\Windows\System32\ws2_32.dll", "ws2_32.", "C:/Windows/System32/Ws2_32.Dll"];
        foreach (string modulo in grafias)
            Afirmar.Igual("ws2_32", ListaProibida.ProcurarNativa(modulo, "connect")?.Alvo, modulo);

        Afirmar.Igual("ws2_32", ListaProibida.NormalizarModulo(@"C:\Windows\System32\WS2_32.DLL"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("ws2_32x.dll", "connect"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("urlmon.dll", "CoInternetParseUrl"));
    }

    [Teste]
    public void ExcecoesDocumentadasNaoSaoProibidas()
    {
        // Funções do adaptador de plataforma.
        string[] doAdaptador =
        [
            "SetWindowPos", "GetWindowLongPtrW", "SetWindowLongPtrW", "MonitorFromPoint", "MonitorFromWindow",
            "GetMonitorInfoW", "EnumDisplayMonitors", "SetForegroundWindow", "GetWindowRect", "SetCapture", "ReleaseCapture",
            "GetSystemMetrics", "GetDpiForWindow", "CreateWindowExW", "DefWindowProcW", "RegisterWindowMessageW",
        ];
        foreach (string funcao in doAdaptador)
            Afirmar.Nulo(ListaProibida.ProcurarNativa("user32.dll", funcao), funcao);
        // Configuração de vídeo, só leitura, para a chave estável do monitor (DEC-030; ARCHITECTURE.md 2.13.3).
        foreach (string funcao in new[] { "GetDisplayConfigBufferSizes", "QueryDisplayConfig", "DisplayConfigGetDeviceInfo" })
            Afirmar.Nulo(ListaProibida.ProcurarNativa("user32.dll", funcao), funcao);
        Afirmar.Nulo(ListaProibida.ProcurarNativa("shell32.dll", "Shell_NotifyIconW"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("shcore.dll", "GetDpiForMonitor"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("kernel32.dll", "GetModuleFileNameW"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("kernel32.dll", "GetCurrentProcess"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("advapi32.dll", "RegOpenKeyExW"));
        Afirmar.Nulo(ListaProibida.ProcurarNativa("advapi32.dll", "RegGetValueW"));

        // Process: o tipo e a leitura do próprio processo.
        Afirmar.Nulo(ListaProibida.ProcurarTipo("System.Diagnostics", "Process"));
        string[] doProprioProcesso = ["GetCurrentProcess", "get_Id", "get_StartTime", "get_WorkingSet64", "Kill", "Dispose"];
        foreach (string membro in doProprioProcesso)
            Afirmar.Nulo(ListaProibida.ProcurarMembro("System.Diagnostics.Process", membro), membro);

        // Acessibilidade do WPF e eventos de sistema.
        Afirmar.Nulo(ListaProibida.ProcurarTipo("System.Windows.Automation", "AutomationProperties"));
        Afirmar.Nulo(ListaProibida.ProcurarTipo("System.Windows.Automation.Peers", "UIElementAutomationPeer"));
        Afirmar.Nulo(ListaProibida.ProcurarTipo("Microsoft.Win32", "SystemEvents"));
        Afirmar.Nulo(ListaProibida.ProcurarTipo("System.Reflection", "Assembly"));
        Afirmar.Nulo(ListaProibida.ProcurarMembro("System.Reflection.Assembly", "GetExecutingAssembly"));
        Afirmar.Nulo(ListaProibida.ProcurarMembro("System.Runtime.InteropServices.Marshal", "SizeOf"));

        // Namespace parecido não conta: System.Net.HttpListener não é System.Net.Http.
        Afirmar.Nulo(ListaProibida.ProcurarTipo("System.Net.HttpExtras", "Qualquer"));
        Afirmar.Nulo(ListaProibida.ProcurarTipo("System.Net", "IPAddress"));
    }
}

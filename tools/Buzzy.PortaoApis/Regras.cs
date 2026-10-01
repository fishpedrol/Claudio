namespace Buzzy.PortaoApis;

/// <summary>
/// Capacidades proibidas, na ordem da tabela de SECURITY.md 3.2. <see cref="Manifesto"/> não
/// vem de 3.2: cobre SECURITY.md 8, item 5 (sem elevação) e ARCHITECTURE.md 2.4 (Per-Monitor V2).
/// </summary>
internal enum Categoria
{
    InputGlobal,
    InjetarInput,
    CapturaDeTela,
    Processos,
    Rede,
    LerOutrosAplicativos,
    PersistenciaEscondida,
    CodigoDinamico,
    Manifesto,

    /// <summary>
    /// Alterar a configuração global do Windows (AGENTS.md; regra dura do projeto): só o usuário muda vídeo, energia e
    /// sessão. Fica no fim do enum para não renumerar as outras (revisão de segurança do bloco P6-P9, achado 6).
    /// </summary>
    ConfiguracaoGlobal,
}

internal static class Categorias
{
    /// <summary>Nome da linha correspondente em SECURITY.md 3.2.</summary>
    public static string Nome(this Categoria categoria) => categoria switch
    {
        Categoria.InputGlobal => "Input global",
        Categoria.InjetarInput => "Injetar input",
        Categoria.CapturaDeTela => "Captura de tela",
        Categoria.Processos => "Processos",
        Categoria.Rede => "Rede",
        Categoria.LerOutrosAplicativos => "Ler outros aplicativos",
        Categoria.PersistenciaEscondida => "Persistência escondida",
        Categoria.CodigoDinamico => "Código dinâmico",
        Categoria.Manifesto => "Manifesto",
        Categoria.ConfiguracaoGlobal => "Alterar configuração global",
        _ => throw new ArgumentOutOfRangeException(nameof(categoria), categoria, null),
    };
}

/// <summary>O que uma regra da lista proibida reconhece nos binários.</summary>
internal enum TipoDeRegra
{
    /// <summary>
    /// Função nativa pelo nome, num P/Invoke ou na tabela de importação de um PE. Aceita o nome
    /// exato e os sufixos A e W, sem diferenciar maiúsculas, em qualquer módulo.
    /// </summary>
    FuncaoNativa,

    /// <summary>Qualquer função de um módulo nativo, sem diferenciar maiúsculas nem a extensão .dll.</summary>
    ModuloNativo,

    /// <summary>Referência a um tipo gerenciado pelo nome completo.</summary>
    TipoGerenciado,

    /// <summary>Referência a qualquer tipo de um namespace ou dos namespaces abaixo dele.</summary>
    NamespaceGerenciado,

    /// <summary>Referência a um membro de um tipo gerenciado, em qualquer sobrecarga.</summary>
    MembroGerenciado,

    /// <summary>
    /// Método de interface COM pelo nome: declarado numa interface do próprio assembly (como
    /// fica uma interface [ComImport] escrita em C#) ou referenciado em outro assembly.
    /// </summary>
    MetodoCom,
}

/// <summary>Uma entrada da lista proibida.</summary>
/// <param name="Tipo">O que a regra reconhece nos binários.</param>
/// <param name="Alvo">
/// Nome da função, do módulo sem extensão, do tipo completo, do namespace ou do método COM.
/// Em <see cref="TipoDeRegra.MembroGerenciado"/>, o tipo que declara o membro.
/// </param>
/// <param name="Membro">Só em <see cref="TipoDeRegra.MembroGerenciado"/>: o nome do membro.</param>
/// <param name="Categoria">Linha de SECURITY.md 3.2.</param>
/// <param name="Motivo">Por que a API dá a capacidade proibida; aparece no relatório.</param>
/// <param name="PadroesNaFonte">
/// Palavras inteiras, ou sequências de palavras separadas por ponto, procuradas no código-fonte
/// sem diferenciar maiúsculas. Lista vazia: a regra só vale para os binários, e o comentário da
/// regra explica por quê.
/// </param>
internal sealed record Regra(
    TipoDeRegra Tipo,
    string Alvo,
    string? Membro,
    Categoria Categoria,
    string Motivo,
    IReadOnlyList<string> PadroesNaFonte)
{
    /// <summary>Nome legível: <c>SendInput</c>, <c>ws2_32</c>, <c>System.Diagnostics.Process.Start</c>.</summary>
    public string Descricao => Membro is null ? Alvo : $"{Alvo}.{Membro}";
}

/// <summary>
/// A lista proibida: tabela única com todas as regras do portão, agrupadas pelas categorias de
/// SECURITY.md 3.2. Cada regra traz o motivo. Entradas marcadas "(além da lista mínima)" foram
/// acrescentadas por darem a mesma capacidade por outro nome; nenhuma é usada pelo produto.
///
/// Exceções deliberadas, que NÃO estão aqui e portanto são permitidas:
/// - System.Diagnostics.Process como tipo, Process.GetCurrentProcess e os membros de leitura do
///   próprio processo (Id, StartTime, WorkingSet64 e afins): o Buzzy pode medir a si mesmo.
///   Só Start, GetProcesses, GetProcessesByName e GetProcessById são proibidos.
/// - O namespace System.Windows.Automation como um todo: o WPF usa AutomationProperties e
///   System.Windows.Automation.Peers para a acessibilidade das próprias janelas. Só o lado
///   cliente, que lê outros processos (AutomationElement, Automation), é proibido.
/// - Microsoft.Win32 como namespace (SystemEvents avisa mudanças de tela e sessão). Só Registry
///   e RegistryKey são proibidos.
/// - Leitura nativa do registro (RegOpenKeyEx, RegQueryValueEx, RegGetValue): SECURITY.md 3.2
///   proíbe criar persistência, não ler. O acesso gerenciado ao registro é proibido inteiro,
///   porque Registry e RegistryKey servem para as duas coisas.
/// - Funções de janela e monitor que o adaptador de plataforma usa sobre as próprias janelas e a
///   topologia (SetWindowPos, GetWindowLongPtr, MonitorFromPoint, GetMonitorInfo,
///   EnumDisplayMonitors, Shell_NotifyIcon, SetForegroundWindow para o menu da bandeja e afins),
///   conforme SECURITY.md 3.1.
///
/// O Buzzy.exe (apphost) tem uma lista de permissões própria, em <see cref="PermissoesDoApphost"/>.
/// </summary>
internal static class ListaProibida
{
    public static readonly IReadOnlyList<Regra> Regras =
    [
        // ---- Input global ---------------------------------------------------------------
        Funcao("SetWindowsHookEx", Categoria.InputGlobal, "instala hook de teclado ou mouse que observa o sistema todo"),
        Funcao("SetWindowsHook", Categoria.InputGlobal, "forma antiga de SetWindowsHookEx, ainda exportada pelo user32 (além da lista mínima)"),
        Funcao("RegisterRawInputDevices", Categoria.InputGlobal, "registra input bruto; com RIDEV_INPUTSINK recebe teclado e mouse fora do foco"),
        Funcao("GetAsyncKeyState", Categoria.InputGlobal, "lê o estado de qualquer tecla do sistema, mesmo sem foco"),
        Funcao("GetKeyboardState", Categoria.InputGlobal, "lê o estado do teclado; serve para observar teclas fora do foco"),
        Funcao("GetKeyState", Categoria.InputGlobal, "lê o estado de teclas; o Buzzy só usa o input entregue às próprias janelas"),
        Funcao("RegisterHotKey", Categoria.InputGlobal, "atalho global de teclado; exige decisão aprovada"),

        // ---- Injetar input --------------------------------------------------------------
        // SendInput é permitido só em ferramentas de teste fora do executável (spikes/ e tools/
        // de teste), nunca no produto.
        Funcao("SendInput", Categoria.InjetarInput, "injeta teclado e mouse sintéticos"),
        Funcao("mouse_event", Categoria.InjetarInput, "injeta mouse sintético (antecessora de SendInput)"),
        Funcao("keybd_event", Categoria.InjetarInput, "injeta teclado sintético (antecessora de SendInput)"),
        Funcao("SetCursorPos", Categoria.InjetarInput, "move o cursor do usuário (além da lista mínima)"),
        Funcao("SetPhysicalCursorPos", Categoria.InjetarInput, "move o cursor do usuário em pixels físicos (além da lista mínima)"),
        Funcao("BlockInput", Categoria.InjetarInput, "bloqueia teclado e mouse do sistema todo (além da lista mínima)"),
        Funcao("InjectTouchInput", Categoria.InjetarInput, "injeta toque sintético (além da lista mínima)"),
        Funcao("InjectSyntheticPointerInput", Categoria.InjetarInput, "injeta ponteiro sintético (além da lista mínima)"),

        // ---- Captura de tela ------------------------------------------------------------
        Funcao("BitBlt", Categoria.CapturaDeTela, "copia pixels de um DC; sobre o desktop ou outra janela, captura a tela"),
        Funcao("StretchBlt", Categoria.CapturaDeTela, "copia pixels de um DC com escala; sobre o desktop ou outra janela, captura a tela"),
        Funcao("PrintWindow", Categoria.CapturaDeTela, "fotografa o conteúdo de uma janela"),
        Funcao("CreateDC", Categoria.CapturaDeTela, "cria DC do monitor ou do desktop, de onde se leem os pixels da tela"),
        Membro("System.Drawing.Graphics", "CopyFromScreen", Categoria.CapturaDeTela, "copia pixels da tela para uma imagem",
            fonte: ["CopyFromScreen"]),
        Namespace("Windows.Graphics.Capture", Categoria.CapturaDeTela, "Windows Graphics Capture: captura janelas e monitores",
            fonte: ["Windows.Graphics.Capture", "GraphicsCaptureItem", "GraphicsCaptureSession", "GraphicsCapturePicker",
                    "Direct3D11CaptureFramePool", "IGraphicsCaptureItemInterop"]),
        MetodoCom("DuplicateOutput", Categoria.CapturaDeTela, "Desktop Duplication (IDXGIOutput1::DuplicateOutput): copia a imagem do monitor"),
        MetodoCom("DuplicateOutput1", Categoria.CapturaDeTela, "Desktop Duplication (IDXGIOutput5::DuplicateOutput1) (além da lista mínima)"),
        MetodoCom("CreateForWindow", Categoria.CapturaDeTela, "IGraphicsCaptureItemInterop: cria item de captura de uma janela (além da lista mínima)"),
        MetodoCom("CreateForMonitor", Categoria.CapturaDeTela, "IGraphicsCaptureItemInterop: cria item de captura de um monitor (além da lista mínima)"),

        // ---- Processos ------------------------------------------------------------------
        Funcao("CreateProcess", Categoria.Processos, "inicia outro processo"),
        Funcao("CreateProcessAsUser", Categoria.Processos, "inicia outro processo com outro token"),
        Funcao("CreateProcessWithLogon", Categoria.Processos, "inicia outro processo com credenciais (além da lista mínima)"),
        Funcao("CreateProcessWithToken", Categoria.Processos, "inicia outro processo com outro token (além da lista mínima)"),
        Funcao("ShellExecute", Categoria.Processos, "abre programa, documento ou URL pelo shell"),
        Funcao("ShellExecuteEx", Categoria.Processos, "abre programa, documento ou URL pelo shell"),
        Funcao("WinExec", Categoria.Processos, "inicia outro processo (API antiga)"),
        // Qualificado na fonte: "Start" sozinho casaria com DispatcherTimer.Start e afins.
        Membro("System.Diagnostics.Process", "Start", Categoria.Processos, "inicia outro processo",
            fonte: ["Process.Start"]),
        Tipo("System.Diagnostics.ProcessStartInfo", Categoria.Processos, "descreve um processo a iniciar"),

        // ---- Rede -----------------------------------------------------------------------
        // O MVP não tem rede (SECURITY.md 1 e 6). Módulos inteiros: qualquer função deles.
        Modulo("ws2_32", Categoria.Rede, "Winsock: sockets"),
        Modulo("wsock32", Categoria.Rede, "Winsock antigo: sockets"),
        Modulo("mswsock", Categoria.Rede, "extensões do Winsock (além da lista mínima)"),
        Modulo("winhttp", Categoria.Rede, "WinHTTP: cliente HTTP"),
        Modulo("wininet", Categoria.Rede, "WinINet: cliente HTTP e FTP"),
        Modulo("httpapi", Categoria.Rede, "HTTP Server API: servidor HTTP (além da lista mínima)"),
        Modulo("websocket", Categoria.Rede, "WebSocket Protocol Component API (além da lista mínima)"),
        Funcao("URLDownloadToFile", Categoria.Rede, "baixa um arquivo da rede (urlmon)"),
        Funcao("URLDownloadToCacheFile", Categoria.Rede, "baixa um arquivo da rede (urlmon) (além da lista mínima)"),
        Funcao("URLOpenStream", Categoria.Rede, "lê conteúdo da rede (urlmon) (além da lista mínima)"),
        Funcao("URLOpenBlockingStream", Categoria.Rede, "lê conteúdo da rede (urlmon) (além da lista mínima)"),
        Funcao("URLOpenPullStream", Categoria.Rede, "lê conteúdo da rede (urlmon) (além da lista mínima)"),
        // A lista de nomes simples também pega o uso sem o namespace escrito, pelo global using
        // implícito do SDK (que fica em obj/ e não é lido).
        Namespace("System.Net.Http", Categoria.Rede, "cliente HTTP (HttpClient e afins)",
            fonte: ["System.Net.Http", "HttpClient", "HttpClientHandler", "SocketsHttpHandler", "HttpRequestMessage", "HttpResponseMessage"]),
        Namespace("System.Net.Sockets", Categoria.Rede, "sockets TCP e UDP",
            fonte: ["System.Net.Sockets", "Socket", "TcpClient", "TcpListener", "UdpClient"]),
        Namespace("System.Net.WebSockets", Categoria.Rede, "conexões WebSocket",
            fonte: ["System.Net.WebSockets", "ClientWebSocket", "WebSocket"]),
        Namespace("System.Net.NetworkInformation", Categoria.Rede, "ping e leitura dos adaptadores de rede e seus endereços (SECURITY.md 6) (além da lista mínima)",
            fonte: ["System.Net.NetworkInformation", "NetworkInterface"]),
        Namespace("System.Net.Mail", Categoria.Rede, "envio de e-mail por SMTP (além da lista mínima)",
            fonte: ["System.Net.Mail", "SmtpClient"]),
        Namespace("System.Net.Quic", Categoria.Rede, "conexões QUIC (além da lista mínima)",
            fonte: ["System.Net.Quic", "QuicConnection", "QuicListener"]),
        Tipo("System.Net.WebClient", Categoria.Rede, "cliente HTTP e FTP"),
        Tipo("System.Net.WebRequest", Categoria.Rede, "requisição de rede"),
        Tipo("System.Net.HttpWebRequest", Categoria.Rede, "requisição HTTP"),
        Tipo("System.Net.FtpWebRequest", Categoria.Rede, "requisição FTP (além da lista mínima)"),
        Tipo("System.Net.HttpListener", Categoria.Rede, "servidor HTTP (além da lista mínima)"),
        Tipo("System.Net.Dns", Categoria.Rede, "consulta DNS pela rede (além da lista mínima)"),

        // ---- Ler outros aplicativos -----------------------------------------------------
        Funcao("EnumWindows", Categoria.LerOutrosAplicativos, "enumera as janelas de todos os aplicativos"),
        Funcao("EnumChildWindows", Categoria.LerOutrosAplicativos, "enumera janelas filhas, inclusive de outros aplicativos"),
        Funcao("EnumThreadWindows", Categoria.LerOutrosAplicativos, "enumera as janelas de uma thread, inclusive de outros aplicativos"),
        Funcao("EnumDesktopWindows", Categoria.LerOutrosAplicativos, "enumera as janelas de uma área de trabalho (além da lista mínima)"),
        Funcao("FindWindow", Categoria.LerOutrosAplicativos, "procura janelas de outros aplicativos por classe ou título"),
        Funcao("FindWindowEx", Categoria.LerOutrosAplicativos, "procura janelas de outros aplicativos por classe ou título"),
        Funcao("GetWindowText", Categoria.LerOutrosAplicativos, "lê o título de uma janela"),
        Funcao("GetWindowTextLength", Categoria.LerOutrosAplicativos, "mede o título de uma janela"),
        Funcao("InternalGetWindowText", Categoria.LerOutrosAplicativos, "lê o título de uma janela (além da lista mínima)"),
        Funcao("GetClassName", Categoria.LerOutrosAplicativos, "lê a classe de uma janela"),
        Funcao("OpenClipboard", Categoria.LerOutrosAplicativos, "abre a área de transferência"),
        Funcao("GetClipboardData", Categoria.LerOutrosAplicativos, "lê a área de transferência"),
        Funcao("OleGetClipboard", Categoria.LerOutrosAplicativos, "lê a área de transferência por OLE (além da lista mínima)"),
        Funcao("AddClipboardFormatListener", Categoria.LerOutrosAplicativos, "observa mudanças na área de transferência (além da lista mínima)"),
        Funcao("SetClipboardViewer", Categoria.LerOutrosAplicativos, "observa mudanças na área de transferência (além da lista mínima)"),
        Funcao("OpenProcess", Categoria.LerOutrosAplicativos, "abre outro processo"),
        Funcao("ReadProcessMemory", Categoria.LerOutrosAplicativos, "lê a memória de outro processo"),
        Funcao("EnumProcesses", Categoria.LerOutrosAplicativos, "lista os processos do sistema"),
        Funcao("K32EnumProcesses", Categoria.LerOutrosAplicativos, "EnumProcesses exportada pelo kernel32 (além da lista mínima)"),
        Funcao("CreateToolhelp32Snapshot", Categoria.LerOutrosAplicativos, "tira retrato dos processos, threads e módulos do sistema"),
        Funcao("Process32First", Categoria.LerOutrosAplicativos, "percorre o retrato de processos (além da lista mínima)"),
        Funcao("Process32Next", Categoria.LerOutrosAplicativos, "percorre o retrato de processos (além da lista mínima)"),
        Funcao("QueryFullProcessImageName", Categoria.LerOutrosAplicativos, "lê o caminho do executável de um processo"),
        Funcao("GetModuleFileNameEx", Categoria.LerOutrosAplicativos, "lê o caminho de um módulo de outro processo"),
        Funcao("K32GetModuleFileNameEx", Categoria.LerOutrosAplicativos, "GetModuleFileNameEx exportada pelo kernel32 (além da lista mínima)"),
        Funcao("GetForegroundWindow", Categoria.LerOutrosAplicativos, "identifica a janela de outro aplicativo em primeiro plano; só entra com DEC-013 na Fase 8"),
        Funcao("WindowFromPoint", Categoria.LerOutrosAplicativos, "identifica a janela de outro aplicativo sob um ponto"),
        // Nesta fase o observador de DEC-013 ainda não existe; na Fase 8 ele terá regra própria,
        // restrita aos eventos e filtros de DEC-013 (SECURITY.md 8, item 1).
        Funcao("SetWinEventHook", Categoria.LerOutrosAplicativos, "observa eventos de janelas de outros aplicativos; proibido nesta fase, só entra com DEC-013 na Fase 8 e com regra própria"),
        Funcao("AccessibleObjectFromWindow", Categoria.LerOutrosAplicativos, "lê a interface de outro aplicativo por MSAA (além da lista mínima)"),
        Funcao("AccessibleObjectFromPoint", Categoria.LerOutrosAplicativos, "lê a interface de outro aplicativo por MSAA (além da lista mínima)"),
        Tipo("System.Windows.Clipboard", Categoria.LerOutrosAplicativos, "lê a área de transferência (WPF)", fonte: ["Clipboard"]),
        Tipo("System.Windows.Forms.Clipboard", Categoria.LerOutrosAplicativos, "lê a área de transferência (Windows Forms)", fonte: ["Clipboard"]),
        Tipo("System.Windows.Automation.AutomationElement", Categoria.LerOutrosAplicativos, "UI Automation do lado cliente: lê a interface de outros processos"),
        // Só nos binários: na fonte, "Automation" é também parte do namespace
        // System.Windows.Automation, que o WPF usa para a própria acessibilidade.
        Tipo("System.Windows.Automation.Automation", Categoria.LerOutrosAplicativos, "UI Automation do lado cliente: observa foco e eventos de outros processos (além da lista mínima)",
            fonte: []),
        Membro("System.Diagnostics.Process", "GetProcesses", Categoria.LerOutrosAplicativos, "lista os processos do sistema",
            fonte: ["GetProcesses"]),
        Membro("System.Diagnostics.Process", "GetProcessesByName", Categoria.LerOutrosAplicativos, "procura processos pelo nome",
            fonte: ["GetProcessesByName"]),
        Membro("System.Diagnostics.Process", "GetProcessById", Categoria.LerOutrosAplicativos, "abre outro processo pelo identificador",
            fonte: ["GetProcessById"]),

        // ---- Persistência escondida -----------------------------------------------------
        // A chave Run de Q-04 (iniciar com o Windows, Fase 8, opcional e ligada pelo usuário)
        // precisará de regra própria quando for implementada.
        Funcao("RegSetValueEx", Categoria.PersistenciaEscondida, "grava valor no registro, como a chave Run"),
        Funcao("RegSetValue", Categoria.PersistenciaEscondida, "grava valor no registro (API antiga) (além da lista mínima)"),
        Funcao("RegSetKeyValue", Categoria.PersistenciaEscondida, "grava valor no registro (além da lista mínima)"),
        Funcao("RegCreateKeyEx", Categoria.PersistenciaEscondida, "cria chave no registro"),
        Funcao("RegCreateKey", Categoria.PersistenciaEscondida, "cria chave no registro (API antiga) (além da lista mínima)"),
        Funcao("CreateService", Categoria.PersistenciaEscondida, "instala um serviço do Windows"),
        Funcao("OpenSCManager", Categoria.PersistenciaEscondida, "abre o gerenciador de serviços para instalar ou alterar serviços"),
        Tipo("Microsoft.Win32.Registry", Categoria.PersistenciaEscondida, "acesso ao registro, inclusive a chave Run"),
        Tipo("Microsoft.Win32.RegistryKey", Categoria.PersistenciaEscondida, "acesso ao registro, inclusive a chave Run"),

        // ---- Código dinâmico ------------------------------------------------------------
        Funcao("LoadLibrary", Categoria.CodigoDinamico, "carrega DLL por caminho em tempo de execução"),
        Funcao("LoadLibraryEx", Categoria.CodigoDinamico, "carrega DLL por caminho em tempo de execução"),
        Funcao("LoadPackagedLibrary", Categoria.CodigoDinamico, "carrega DLL em tempo de execução (além da lista mínima)"),
        Funcao("LdrLoadDll", Categoria.CodigoDinamico, "carrega DLL pelo ntdll, por baixo de LoadLibrary (além da lista mínima)"),
        Funcao("GetProcAddress", Categoria.CodigoDinamico, "obtém função por nome em tempo de execução, escondendo a chamada do portão"),
        Funcao("LdrGetProcedureAddress", Categoria.CodigoDinamico, "obtém função pelo ntdll, por baixo de GetProcAddress (além da lista mínima)"),
        // Qualificados na fonte: "Load", "LoadFrom" e "LoadFile" sozinhos são nomes comuns.
        Membro("System.Reflection.Assembly", "Load", Categoria.CodigoDinamico, "carrega assembly em tempo de execução",
            fonte: ["Assembly.Load"]),
        Membro("System.Reflection.Assembly", "LoadFrom", Categoria.CodigoDinamico, "carrega assembly de um caminho",
            fonte: ["Assembly.LoadFrom"]),
        Membro("System.Reflection.Assembly", "LoadFile", Categoria.CodigoDinamico, "carrega assembly de um caminho",
            fonte: ["Assembly.LoadFile"]),
        Membro("System.Reflection.Assembly", "UnsafeLoadFrom", Categoria.CodigoDinamico, "carrega assembly de um caminho",
            fonte: ["UnsafeLoadFrom"]),
        // Só nos binários: o uso comum é AppDomain.CurrentDomain.Load, sem nome distintivo.
        Membro("System.AppDomain", "Load", Categoria.CodigoDinamico, "carrega assembly em tempo de execução, como Assembly.Load (além da lista mínima)",
            fonte: []),
        Membro("System.Runtime.Loader.AssemblyLoadContext", "LoadFromAssemblyPath", Categoria.CodigoDinamico, "carrega assembly de um caminho",
            fonte: ["LoadFromAssemblyPath"]),
        Membro("System.Runtime.Loader.AssemblyLoadContext", "LoadFromStream", Categoria.CodigoDinamico, "carrega assembly de bytes arbitrários",
            fonte: ["LoadFromStream"]),
        Membro("System.Runtime.Loader.AssemblyLoadContext", "LoadFromNativeImagePath", Categoria.CodigoDinamico, "carrega assembly de um caminho (além da lista mínima)",
            fonte: ["LoadFromNativeImagePath"]),
        Membro("System.Runtime.Loader.AssemblyLoadContext", "LoadUnmanagedDllFromPath", Categoria.CodigoDinamico, "carrega DLL nativa de um caminho (além da lista mínima)",
            fonte: ["LoadUnmanagedDllFromPath"]),
        Tipo("System.Runtime.InteropServices.NativeLibrary", Categoria.CodigoDinamico, "carrega DLL nativa e obtém funções em tempo de execução"),
        Membro("System.Runtime.InteropServices.Marshal", "GetDelegateForFunctionPointer", Categoria.CodigoDinamico, "chama ponteiro de função obtido em tempo de execução",
            fonte: ["GetDelegateForFunctionPointer"]),
        Namespace("System.Reflection.Emit", Categoria.CodigoDinamico, "gera código em tempo de execução",
            fonte: ["System.Reflection.Emit", "DynamicMethod", "ILGenerator", "AssemblyBuilder", "PersistedAssemblyBuilder"]),

        // ---- Alterar configuração global --------------------------------------------------
        // A chave estável do monitor (DEC-030) só LÊ a configuração de vídeo (GetDisplayConfigBufferSizes, QueryDisplayConfig e
        // DisplayConfigGetDeviceInfo, permitidas); estas a mudam para o sistema todo (revisão de segurança do bloco P6-P9).
        Funcao("SetDisplayConfig", Categoria.ConfiguracaoGlobal, "muda a topologia, a resolução, a orientação ou o modo de vídeo do sistema todo"),
        Funcao("DisplayConfigSetDeviceInfo", Categoria.ConfiguracaoGlobal, "muda propriedades de um alvo ou de uma fonte de vídeo (escala, HDR) para o sistema todo"),
        Funcao("ChangeDisplaySettings", Categoria.ConfiguracaoGlobal, "muda o modo de vídeo do monitor principal para o sistema todo"),
        Funcao("ChangeDisplaySettingsEx", Categoria.ConfiguracaoGlobal, "muda o modo de vídeo ou a posição de um monitor para o sistema todo"),
    ];

    private static readonly Dictionary<string, Regra> FuncoesPorVariante = IndexarFuncoes();
    private static readonly Dictionary<string, Regra> ModulosPorNome = Indexar(TipoDeRegra.ModuloNativo, r => r.Alvo.ToLowerInvariant());
    private static readonly Dictionary<string, Regra> TiposPorNome = Indexar(TipoDeRegra.TipoGerenciado, r => r.Alvo);
    private static readonly Dictionary<string, Regra> MembrosPorNome = Indexar(TipoDeRegra.MembroGerenciado, r => $"{r.Alvo}::{r.Membro}");
    private static readonly Dictionary<string, Regra> MetodosComPorNome = Indexar(TipoDeRegra.MetodoCom, r => r.Alvo);
    private static readonly Regra[] Namespaces = [.. Regras.Where(r => r.Tipo == TipoDeRegra.NamespaceGerenciado)];

    /// <summary>
    /// Regra que proíbe uma função nativa: primeiro pelo nome da função (ignorando maiúsculas e
    /// os sufixos A e W), depois pelo módulo (ignorando maiúsculas, caminho e extensão .dll).
    /// </summary>
    public static Regra? ProcurarNativa(string modulo, string funcao)
    {
        ArgumentNullException.ThrowIfNull(modulo);
        ArgumentNullException.ThrowIfNull(funcao);
        if (FuncoesPorVariante.TryGetValue(funcao.Trim().ToLowerInvariant(), out Regra? porFuncao)) return porFuncao;
        return ModulosPorNome.GetValueOrDefault(NormalizarModulo(modulo));
    }

    /// <summary>Regra que proíbe um tipo gerenciado de nível superior, pelo nome ou pelo namespace.</summary>
    public static Regra? ProcurarTipo(string nomeDoNamespace, string nome)
    {
        ArgumentNullException.ThrowIfNull(nomeDoNamespace);
        ArgumentNullException.ThrowIfNull(nome);
        if (TiposPorNome.TryGetValue(Juntar(nomeDoNamespace, nome), out Regra? porTipo)) return porTipo;
        foreach (Regra r in Namespaces)
        {
            if (string.Equals(nomeDoNamespace, r.Alvo, StringComparison.Ordinal)
                || nomeDoNamespace.StartsWith(r.Alvo + ".", StringComparison.Ordinal))
                return r;
        }
        return null;
    }

    /// <summary>Regra que proíbe um membro, dado o nome completo do tipo que o declara.</summary>
    public static Regra? ProcurarMembro(string tipo, string membro)
        => MembrosPorNome.GetValueOrDefault($"{tipo}::{membro}");

    /// <summary>Regra que proíbe um método de interface COM pelo nome.</summary>
    public static Regra? ProcurarMetodoCom(string metodo) => MetodosComPorNome.GetValueOrDefault(metodo);

    /// <summary>
    /// Nome de módulo comparável: sem caminho, sem ponto final, sem extensão .dll, em minúsculas.
    /// "C:\Windows\System32\WS2_32.DLL", "ws2_32.dll" e "ws2_32" viram "ws2_32".
    /// </summary>
    public static string NormalizarModulo(string modulo)
    {
        ArgumentNullException.ThrowIfNull(modulo);
        string nome = modulo.Trim();
        int barra = nome.LastIndexOfAny(['\\', '/']);
        if (barra >= 0) nome = nome[(barra + 1)..];
        nome = nome.TrimEnd('.');
        if (nome.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) nome = nome[..^4];
        return nome.ToLowerInvariant();
    }

    public static string Juntar(string nomeDoNamespace, string nome) => nomeDoNamespace.Length == 0 ? nome : $"{nomeDoNamespace}.{nome}";

    // O nome exato e os sufixos A e W de cada função. Comparar variantes, em vez de cortar o
    // último caractere, evita falso positivo: "PrintWindow" termina em W e não é variante de nada.
    private static Dictionary<string, Regra> IndexarFuncoes()
    {
        var indice = new Dictionary<string, Regra>(StringComparer.Ordinal);
        foreach (Regra r in Regras.Where(r => r.Tipo == TipoDeRegra.FuncaoNativa))
        {
            string nome = r.Alvo.ToLowerInvariant();
            string[] variantes = [nome, nome + "a", nome + "w"];
            foreach (string variante in variantes)
            {
                if (!indice.TryAdd(variante, r) && !ReferenceEquals(indice[variante], r))
                    throw new InvalidOperationException($"Lista proibida: a variante \"{variante}\" aparece em duas regras.");
            }
        }
        return indice;
    }

    private static Dictionary<string, Regra> Indexar(TipoDeRegra tipo, Func<Regra, string> chave)
    {
        var indice = new Dictionary<string, Regra>(StringComparer.Ordinal);
        foreach (Regra r in Regras.Where(r => r.Tipo == tipo))
        {
            if (!indice.TryAdd(chave(r), r))
                throw new InvalidOperationException($"Lista proibida: \"{chave(r)}\" aparece em duas regras.");
        }
        return indice;
    }

    private static Regra Funcao(string nome, Categoria categoria, string motivo)
        => new(TipoDeRegra.FuncaoNativa, nome, null, categoria, motivo, [nome, nome + "A", nome + "W"]);

    private static Regra Modulo(string nome, Categoria categoria, string motivo)
        => new(TipoDeRegra.ModuloNativo, nome, null, categoria, motivo, [nome]);

    private static Regra Tipo(string nomeCompleto, Categoria categoria, string motivo, IReadOnlyList<string>? fonte = null)
        => new(TipoDeRegra.TipoGerenciado, nomeCompleto, null, categoria, motivo, fonte ?? [nomeCompleto[(nomeCompleto.LastIndexOf('.') + 1)..]]);

    private static Regra Namespace(string nome, Categoria categoria, string motivo, IReadOnlyList<string> fonte)
        => new(TipoDeRegra.NamespaceGerenciado, nome, null, categoria, motivo, fonte);

    private static Regra Membro(string tipo, string membro, Categoria categoria, string motivo, IReadOnlyList<string> fonte)
        => new(TipoDeRegra.MembroGerenciado, tipo, membro, categoria, motivo, fonte);

    private static Regra MetodoCom(string nome, Categoria categoria, string motivo)
        => new(TipoDeRegra.MetodoCom, nome, null, categoria, motivo, [nome]);
}

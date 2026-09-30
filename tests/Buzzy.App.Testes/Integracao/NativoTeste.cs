using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Buzzy.App.Testes.Integracao;

/// <summary>
/// Chamadas ao Windows usadas SÓ pelos testes, fora do produto. Agem sobre as janelas do
/// Buzzy aberto pelo próprio teste, conferem a qual processo uma janela pertence, consultam o
/// contexto de DPI do próprio processo de testes e listam processos filhos do Buzzy.
/// </summary>
internal static class NativoTeste
{
    internal const int GWL_EXSTYLE = -20;
    internal const long WS_EX_TOPMOST = 0x00000008L;
    internal const long WS_EX_TRANSPARENT = 0x00000020L;
    internal const long WS_EX_TOOLWINDOW = 0x00000080L;
    internal const long WS_EX_APPWINDOW = 0x00040000L;
    internal const long WS_EX_LAYERED = 0x00080000L;
    internal const long WS_EX_NOACTIVATE = 0x08000000L;

    internal const int WM_CLOSE = 0x0010;
    internal const int WM_SETTINGCHANGE = 0x001A;
    internal const int WM_DISPLAYCHANGE = 0x007E;
    internal const int SPI_SETWORKAREA = 0x002F;
    internal const int SW_SHOWMINNOACTIVE = 7;

    /// <summary>DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2.</summary>
    internal const nint ContextoPerMonitorV2 = -4;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PROCESSENTRY32W
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public nint th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    internal static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWindow(nint hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetWindowRect(nint hWnd, out RECT r);

    internal const uint SMTO_ABORTIFHUNG = 0x0002;

    /// <summary>
    /// Mensagens do sistema que levam ponteiro, como WM_SETTINGCHANGE, não podem ser postadas
    /// a outro processo (PostMessage falha com ERROR_MESSAGE_SYNC_ONLY, 1159): o Windows as
    /// entrega por envio síncrono, e o teste faz o mesmo.
    /// </summary>
    [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW", SetLastError = true)]
    internal static extern nint SendMessageTimeout(nint hWnd, int msg, nint wParam, nint lParam, uint flags, uint timeoutMs, out nint resultado);

    [DllImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool PostMessage(nint hWnd, int msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW", CharSet = CharSet.Unicode)]
    internal static extern int RegisterWindowMessage(string lpString);

    [DllImport("user32.dll")]
    internal static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll")]
    internal static extern nint GetThreadDpiAwarenessContext();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AreDpiAwarenessContextsEqual(nint contextoA, nint contextoB);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll", EntryPoint = "Process32FirstW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32First(nint hSnapshot, ref PROCESSENTRY32W lppe);

    [DllImport("kernel32.dll", EntryPoint = "Process32NextW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32Next(nint hSnapshot, ref PROCESSENTRY32W lppe);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint hObject);

    internal static long EstiloEstendido(nint hwnd) => (long)GetWindowLongPtr(hwnd, GWL_EXSTYLE);

    /// <summary>PID do processo dono da janela; 0 se a janela não existe.</summary>
    internal static uint PidDe(nint hwnd)
    {
        GetWindowThreadProcessId(hwnd, out uint pid);
        return pid;
    }

    /// <summary>
    /// Se a thread atual está em Per-Monitor V2, como o Buzzy. Fora dele, o Windows entrega a
    /// este processo coordenadas virtualizadas (escaladas) nos monitores com escala diferente
    /// de 100%, e elas não batem com o log do Buzzy, que está em pixels físicos. Só consulta o
    /// contexto de DPI: não abre janela.
    /// </summary>
    internal static bool ThreadEmPerMonitorV2()
        => AreDpiAwarenessContextsEqual(GetThreadDpiAwarenessContext(), ContextoPerMonitorV2);

    /// <summary>
    /// PIDs dos processos cujo pai registrado é <paramref name="pid"/> e que foram criados a
    /// partir de <paramref name="inicioDoPai"/>. O Windows guarda só o número do pai, e esse
    /// número pode ser de um processo antigo, já encerrado, que o Buzzy reaproveitou; a hora de
    /// criação separa os filhos verdadeiros. Um processo cuja hora de criação não dá para ler
    /// entra na lista, por segurança.
    /// </summary>
    internal static List<int> Filhos(int pid, DateTime inicioDoPai)
    {
        const uint TH32CS_SNAPPROCESS = 0x00000002;
        var candidatos = new List<int>();
        nint instantaneo = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (instantaneo == -1) throw new InvalidOperationException("CreateToolhelp32Snapshot falhou.");
        try
        {
            var e = new PROCESSENTRY32W { dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32W>() };
            for (bool ok = Process32First(instantaneo, ref e); ok; ok = Process32Next(instantaneo, ref e))
            {
                if (e.th32ParentProcessID == (uint)pid && e.th32ProcessID != (uint)pid) candidatos.Add((int)e.th32ProcessID);
            }
        }
        finally
        {
            CloseHandle(instantaneo);
        }
        return [.. candidatos.Where(c => CriadoAPartirDe(c, inicioDoPai))];
    }

    private static bool CriadoAPartirDe(int pid, DateTime referencia)
    {
        try
        {
            using Process processo = Process.GetProcessById(pid);
            return processo.StartTime >= referencia;
        }
        catch (ArgumentException)
        {
            return false; // já encerrou entre o instantâneo e agora: não é um filho vivo
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception or NotSupportedException)
        {
            return true; // hora de criação ilegível: conta, por segurança
        }
    }
}

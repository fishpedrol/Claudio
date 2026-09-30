using System.Runtime.InteropServices;
using Buzzy.Core;

namespace Buzzy.App.Plataforma;

/// <summary>
/// Lê a topologia real dos monitores e a converte para o modelo puro do núcleo, em pixels
/// físicos do desktop virtual (DEC-008). Só lê geometria, área útil e DPI dos monitores.
/// </summary>
internal static class LeitorDeTopologia
{
    /// <summary>
    /// Topologia atual, ou nulo se o Windows devolver algo incoerente (por exemplo, no meio de
    /// uma troca de modo de vídeo, sem monitor principal). Quem chama mantém a anterior.
    /// </summary>
    internal static Topologia? Ler(out string? erro)
    {
        var monitores = new List<MonitorDoDesktop>();
        string? falha = null;

        bool Visitar(nint hMonitor, nint hdc, nint lprc, nint dado)
        {
            var mi = new Win32.MONITORINFOEX { cbSize = Marshal.SizeOf<Win32.MONITORINFOEX>() };
            if (!Win32.GetMonitorInfo(hMonitor, ref mi))
            {
                falha = $"GetMonitorInfo falhou (erro {Marshal.GetLastWin32Error()})";
                return true;
            }

            int dpi = Win32.GetDpiForMonitor(hMonitor, Win32.MDT_EFFECTIVE_DPI, out uint dx, out _) == 0 && dx > 0
                ? (int)dx
                : 96;

            monitores.Add(new MonitorDoDesktop(
                mi.szDevice,
                Retangulo(mi.rcMonitor),
                Retangulo(mi.rcWork),
                dpi,
                (mi.dwFlags & Win32.MONITORINFOF_PRIMARY) != 0));
            return true;
        }

        Win32.MonitorEnumProc callback = Visitar;
        bool ok = Win32.EnumDisplayMonitors(0, 0, callback, 0);
        GC.KeepAlive(callback);

        if (!ok)
        {
            erro = "EnumDisplayMonitors falhou";
            return null;
        }

        try
        {
            erro = falha;
            return new Topologia(monitores);
        }
        catch (ArgumentException e)
        {
            erro = e.Message;
            return null;
        }
    }

    internal static RetanguloPx Retangulo(Win32.RECT r) => new(r.Left, r.Top, r.Right, r.Bottom);
}

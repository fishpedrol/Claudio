using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Buzzy.App.Plataforma;
using Buzzy.Core;

namespace Buzzy.App.Apresentacao;

/// <summary>
/// Janela do personagem (ARCHITECTURE.md 2.13.1): sem borda, do tamanho do sprite, com
/// transparência por pixel (janela layered do WPF, confirmada em P1), sempre no topo, fora da
/// barra de tarefas e do Alt+Tab (Q-03) e que NÃO ativa ao ser clicada (DEC-009, P3).
///
/// Na Fase 1 ela não se move sozinha nem é arrastada: só mostra o sprite, reage ao botão
/// direito abrindo o menu e avisa quando o DPI muda ou quando o Windows tenta minimizá-la.
/// </summary>
internal sealed class JanelaPersonagem : Window
{
    private readonly Image _imagem = new()
    {
        Stretch = Stretch.None,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        SnapsToDevicePixels = true,
    };

    internal JanelaPersonagem()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        ShowActivated = false;
        Focusable = false;
        Title = "Buzzy";
        WindowStartupLocation = WindowStartupLocation.Manual;
        SizeToContent = SizeToContent.Manual;
        Width = SpriteProvisorio.TamanhoLogico.Largura;
        Height = SpriteProvisorio.TamanhoLogico.Altura;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        RenderOptions.SetBitmapScalingMode(_imagem, BitmapScalingMode.NearestNeighbor);
        Content = _imagem;

        StateChanged += AoMudarEstado;
        DpiChanged += (_, e) =>
        {
            // O WPF também avisa quando só reavaliou o DPI, sem mudança (observado logo depois
            // de mostrar a janela); isso não é mudança de topologia.
            int antes = (int)Math.Round(e.OldDpi.PixelsPerInchX);
            int depois = (int)Math.Round(e.NewDpi.PixelsPerInchX);
            if (antes != depois) DpiMudou?.Invoke(depois);
        };
    }

    internal nint Hwnd { get; private set; }

    /// <summary>Botão direito solto sobre o personagem, com o ponto em coordenadas de tela.</summary>
    internal event Action<PontoPx>? MenuSolicitado;

    /// <summary>O Windows mudou o DPI da janela (troca de escala ou de monitor).</summary>
    internal event Action<int>? DpiMudou;

    /// <summary>O Windows tentou minimizar a janela (Q-03: minimizar esconde).</summary>
    internal event Action? Minimizada;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        Hwnd = new WindowInteropHelper(this).Handle;

        // Não ativar ao ser clicada e ficar fora da barra de tarefas e do Alt+Tab.
        nint estilo = Win32.GetWindowLongPtr(Hwnd, Win32.GWL_EXSTYLE);
        Win32.SetWindowLongPtr(Hwnd, Win32.GWL_EXSTYLE, (nint)((long)estilo | Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW));

        HwndSource.FromHwnd(Hwnd)?.AddHook(Gancho);
    }

    internal void DefinirSprite(BitmapSource sprite) => _imagem.Source = sprite;

    internal BitmapSource? Sprite => _imagem.Source as BitmapSource;

    /// <summary>Posiciona e dimensiona a janela em pixels físicos, sem ativar nem mudar a ordem Z.</summary>
    internal void AplicarRetangulo(RetanguloPx r)
        => Win32.SetWindowPos(Hwnd, 0, r.Esquerda, r.Topo, r.Largura, r.Altura, Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);

    /// <summary>
    /// Recoloca a janela no topo do grupo "sempre no topo", uma vez, sem ativar. Só é chamado
    /// por ação explícita do usuário (mostrar), nunca por timer (SECURITY.md 2).
    /// </summary>
    internal void ReafirmarTopo()
        => Win32.SetWindowPos(Hwnd, Win32.HWND_TOPMOST, 0, 0, 0, 0, Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE);

    private nint Gancho(nint hwnd, int msg, nint wParam, nint lParam, ref bool tratado)
    {
        switch (msg)
        {
            case Win32.WM_MOUSEACTIVATE:
                // O clique chega ao Buzzy, mas o aplicativo em uso continua com o foco.
                tratado = true;
                return Win32.MA_NOACTIVATE;

            case Win32.WM_GETDPISCALEDSIZE:
            {
                // A janela vai mudar de DPI (outro monitor ou outra escala). Sem esta resposta,
                // o Windows escalaria linearmente o tamanho pedido no SetWindowPos, que já é o
                // do DPI novo, e a janela ficaria com a escala aplicada duas vezes até a
                // próxima acomodação. O tamanho certo é o do sprite no DPI novo.
                int dpiNovo = (int)(long)wParam;
                if (dpiNovo <= 0) break;
                TamanhoPx tamanho = SpriteProvisorio.TamanhoLogico.ParaPixels(dpiNovo);
                System.Runtime.InteropServices.Marshal.WriteInt32(lParam, 0, tamanho.Largura);
                System.Runtime.InteropServices.Marshal.WriteInt32(lParam, 4, tamanho.Altura);
                tratado = true;
                return 1;
            }

            case Win32.WM_LBUTTONDOWN:
                Diagnostico.Evento("CLIQUE", ("botao", "esquerdo"), ("cliente", $"{Win32.XComSinal(lParam)},{Win32.YComSinal(lParam)}"));
                break;

            case Win32.WM_RBUTTONUP:
            {
                var p = new Win32.POINT { X = Win32.XComSinal(lParam), Y = Win32.YComSinal(lParam) };
                Diagnostico.Evento("CLIQUE", ("botao", "direito"), ("cliente", $"{p.X},{p.Y}"));
                Win32.ClientToScreen(hwnd, ref p);
                MenuSolicitado?.Invoke(new PontoPx(p.X, p.Y));
                tratado = true;
                return 0;
            }

            case Win32.WM_CONTEXTMENU:
                // Já tratado no WM_RBUTTONUP; a janela nunca tem foco de teclado.
                tratado = true;
                return 0;
        }
        return 0;
    }

    private void AoMudarEstado(object? remetente, EventArgs e)
    {
        if (WindowState != WindowState.Minimized) return;
        WindowState = WindowState.Normal;
        Minimizada?.Invoke();
    }
}

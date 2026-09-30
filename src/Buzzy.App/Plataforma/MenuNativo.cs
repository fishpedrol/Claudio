using System.Windows.Interop;
using Buzzy.Core;

namespace Buzzy.App.Plataforma;

/// <summary>Comandos do menu.</summary>
internal enum ComandoDoMenu
{
    Nenhum = 0,
    AlternarVisibilidade = 1,
    Sair = 2,

    /// <summary>Pausar ou retomar o movimento autônomo (CMD_PAUSE_AUTONOMY / CMD_RESUME_AUTONOMY).</summary>
    AlternarMovimento = 3,
}

/// <summary>
/// O menu do Buzzy, o mesmo para o botão direito no personagem e para o ícone da bandeja
/// (Q-03). É o menu nativo do Windows: tem teclas de acesso, navegação por teclado e
/// acessibilidade prontas, e escala por monitor no modo Per-Monitor V2.
///
/// Um menu só fecha direito ao clicar fora quando o dono está em primeiro plano. O dono é
/// uma janela oculta TEMPORÁRIA, criada para cada abertura: ela recebe o primeiro plano só
/// porque o usuário acabou de pedir o menu, e é destruída quando o menu fecha, para que o
/// Windows devolva a ativação à janela seguinte na ordem Z — em geral, o aplicativo que o
/// usuário estava usando. O Buzzy não lê qual é essa janela.
/// </summary>
internal static class MenuNativo
{
    internal static ComandoDoMenu Mostrar(PontoPx ponto, bool buzzyVisivel, bool movimentoPausado, bool abrirParaCima)
    {
        using var dono = new HwndSource(new HwndSourceParameters("Buzzy.Menu")
        {
            WindowStyle = Win32.WS_POPUP,
            ExtendedWindowStyle = (int)Win32.WS_EX_TOOLWINDOW,
            PositionX = ponto.X,
            PositionY = ponto.Y,
            Width = 0,
            Height = 0,
        });

        nint menu = Win32.CreatePopupMenu();
        if (menu == 0) return ComandoDoMenu.Nenhum;
        try
        {
            Win32.AppendMenu(menu, Win32.MF_STRING, (nint)ComandoDoMenu.AlternarVisibilidade, buzzyVisivel ? Textos.MenuEsconder : Textos.MenuMostrar);
            Win32.AppendMenu(menu, Win32.MF_STRING, (nint)ComandoDoMenu.AlternarMovimento, movimentoPausado ? Textos.MenuRetomar : Textos.MenuPausar);
            Win32.AppendMenu(menu, Win32.MF_SEPARATOR, 0, null);
            Win32.AppendMenu(menu, Win32.MF_STRING, (nint)ComandoDoMenu.Sair, Textos.MenuSair);

            bool primeiroPlano = Win32.SetForegroundWindow(dono.Handle);
            // Para diagnóstico e para a verificação da Fase 1: o dono do menu é janela do
            // próprio Buzzy; sem primeiro plano, o menu não recebe teclado nem fecha ao clicar fora.
            Diagnostico.Evento("MENU", ("exibindo", "sim"), ("dono", dono.Handle), ("donoEmPrimeiroPlano", primeiroPlano));
            uint opcoes = Win32.TPM_RETURNCMD | Win32.TPM_NONOTIFY | Win32.TPM_RIGHTBUTTON | Win32.TPM_LEFTALIGN
                | (abrirParaCima ? Win32.TPM_BOTTOMALIGN : Win32.TPM_TOPALIGN);
            int escolhido = Win32.TrackPopupMenuEx(menu, opcoes, ponto.X, ponto.Y, dono.Handle, 0);

            // Recomendação da documentação do Shell_NotifyIcon: uma mensagem qualquer ao dono
            // depois do menu, para o próximo clique fora dele funcionar.
            Win32.PostMessage(dono.Handle, Win32.WM_NULL, 0, 0);

            ComandoDoMenu comando = Enum.IsDefined((ComandoDoMenu)escolhido) ? (ComandoDoMenu)escolhido : ComandoDoMenu.Nenhum;
            Diagnostico.Evento("MENU", ("fechado", comando), ("donoEmPrimeiroPlano", primeiroPlano));
            return comando;
        }
        finally
        {
            Win32.DestroyMenu(menu);
        }
    }
}

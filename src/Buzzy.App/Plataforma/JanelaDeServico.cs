using System.Windows.Interop;
using Buzzy.Core;

namespace Buzzy.App.Plataforma;

/// <summary>O que o usuário fez no ícone da bandeja.</summary>
internal enum AcaoNaBandeja
{
    /// <summary>Clique com o botão esquerdo ou Enter/espaço no ícone: mostrar o Buzzy.</summary>
    Selecionar,

    /// <summary>Botão direito do mouse no ícone: abrir o menu.</summary>
    Menu,

    /// <summary>Shift+F10 ou tecla de menu no ícone selecionado pelo teclado: abrir o menu.</summary>
    MenuPeloTeclado,
}

/// <summary>
/// Janela de nível superior OCULTA, nunca mostrada, que recebe o que o Buzzy precisa mesmo
/// com o personagem escondido: as notificações do ícone da bandeja, a mensagem de recriação
/// da barra de tarefas e as mensagens de mudança de vídeo e de área útil (ARCHITECTURE.md
/// 2.4). Não pode ser uma janela só de mensagens, porque essas não recebem difusões como
/// "TaskbarCreated" e WM_SETTINGCHANGE.
/// </summary>
internal sealed class JanelaDeServico : IDisposable
{
    private readonly HwndSource _fonte;
    private readonly int _mensagemBarraCriada;

    // Na versão 4, o botão direito do mouse chega como WM_RBUTTONUP seguido de WM_CONTEXTMENU;
    // a tecla de menu manda só WM_CONTEXTMENU. É assim que o Buzzy sabe se o menu foi aberto
    // pelo teclado, sem ler o estado do teclado.
    private bool _botaoDireitoAcabouDeSoltar;

    internal JanelaDeServico()
    {
        _mensagemBarraCriada = Win32.RegisterWindowMessage("TaskbarCreated");
        _fonte = new HwndSource(new HwndSourceParameters("Buzzy.Servico")
        {
            WindowStyle = Win32.WS_POPUP,
            ExtendedWindowStyle = (int)Win32.WS_EX_TOOLWINDOW,
            PositionX = 0,
            PositionY = 0,
            Width = 0,
            Height = 0,
        });
        _fonte.AddHook(Gancho);
    }

    internal nint Hwnd => _fonte.Handle;

    /// <summary>
    /// Se as notificações da bandeja chegam na versão 4. Sem ela (a Shell recusou
    /// NIM_SETVERSION), vale o formato antigo, que não traz a posição da âncora.
    /// </summary>
    internal bool NotificacoesVersao4 { get; set; } = true;

    /// <summary>Ação no ícone da bandeja; a âncora vem nula no formato antigo.</summary>
    internal event Action<AcaoNaBandeja, PontoPx?>? BandejaAcionada;

    internal event Action? BarraDeTarefasRecriada;

    internal event Action<string>? TopologiaPodeTerMudado;

    private nint Gancho(nint hwnd, int msg, nint wParam, nint lParam, ref bool tratado)
    {
        if (msg == Bandeja.MensagemDeRetorno)
        {
            TratarBandeja(wParam, lParam);
            tratado = true;
            return 0;
        }

        if (msg == _mensagemBarraCriada && _mensagemBarraCriada != 0)
        {
            BarraDeTarefasRecriada?.Invoke();
            return 0;
        }

        switch (msg)
        {
            case Win32.WM_DISPLAYCHANGE:
                TopologiaPodeTerMudado?.Invoke("WM_DISPLAYCHANGE");
                break;
            case Win32.WM_SETTINGCHANGE when (int)wParam == Win32.SPI_SETWORKAREA:
                TopologiaPodeTerMudado?.Invoke("SPI_SETWORKAREA");
                break;
        }
        return 0;
    }

    private void TratarBandeja(nint wParam, nint lParam)
    {
        if (NotificacoesVersao4)
        {
            // Versão 4: LOWORD(lParam) = evento, HIWORD(lParam) = id do ícone,
            // wParam = coordenadas da âncora, com sinal.
            int evento = Win32.LoWord(lParam);
            var ancora = new PontoPx(Win32.XComSinal(wParam), Win32.YComSinal(wParam));
            switch (evento)
            {
                case Win32.WM_RBUTTONUP:
                    _botaoDireitoAcabouDeSoltar = true;
                    break;
                case Win32.NIN_SELECT or Win32.NIN_KEYSELECT:
                    _botaoDireitoAcabouDeSoltar = false;
                    BandejaAcionada?.Invoke(AcaoNaBandeja.Selecionar, ancora);
                    break;
                case Win32.WM_CONTEXTMENU:
                    AcaoNaBandeja acao = _botaoDireitoAcabouDeSoltar ? AcaoNaBandeja.Menu : AcaoNaBandeja.MenuPeloTeclado;
                    _botaoDireitoAcabouDeSoltar = false;
                    BandejaAcionada?.Invoke(acao, ancora);
                    break;
            }
            return;
        }

        // Formato antigo: lParam = mensagem de mouse, sem coordenadas.
        switch ((int)lParam)
        {
            case Win32.WM_LBUTTONUP:
                BandejaAcionada?.Invoke(AcaoNaBandeja.Selecionar, null);
                break;
            case Win32.WM_RBUTTONUP:
                BandejaAcionada?.Invoke(AcaoNaBandeja.Menu, null);
                break;
        }
    }

    public void Dispose()
    {
        _fonte.RemoveHook(Gancho);
        _fonte.Dispose();
    }
}

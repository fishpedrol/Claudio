using Buzzy.Core;
using Buzzy.Core.Personagem;

namespace Buzzy.App.Composicao;

/// <summary>
/// O que o shell diz sobre notificações (<c>SHQueryUserNotificationState</c>, valores de <c>QUERY_USER_NOTIFICATION_STATE</c>):
/// o sinal auxiliar da tela cheia (ARCHITECTURE.md 2.13, Q-09), nunca a única fonte.
/// </summary>
internal enum EstadoDoShell
{
    /// <summary>Protetor de tela, sessão bloqueada ou outra sessão ativa.</summary>
    NaoPresente = 1,

    /// <summary>Um aplicativo em tela cheia, ou as configurações de apresentação ligadas.</summary>
    Ocupado = 2,

    /// <summary>Um aplicativo Direct3D em tela cheia exclusiva.</summary>
    D3dEmTelaCheia = 3,

    /// <summary>As configurações de apresentação do Windows ligadas.</summary>
    Apresentacao = 4,

    /// <summary>Nada disso: notificações livres.</summary>
    AceitaNotificacoes = 5,

    /// <summary>A primeira hora depois de instalar ou atualizar o Windows.</summary>
    HoraQuieta = 6,

    /// <summary>Um aplicativo da Microsoft Store em execução (em tela cheia, no Windows 8).</summary>
    App = 7,
}

/// <summary>
/// O que o observador lê da janela em primeiro plano na hora da avaliação, descartado logo depois dela (DEC-013): nada de
/// identidade, título, conteúdo ou processo.
/// </summary>
/// <param name="Janela">O retângulo dela, em pixels físicos; nulo sem janela em primeiro plano ou se o Windows não informar.</param>
/// <param name="Shell">O sinal do shell; nulo se a consulta falhar.</param>
/// <param name="DoBuzzy">A janela em primeiro plano é do próprio Buzzy (o menu): a avaliação mantém o que estava.</param>
internal readonly record struct LeituraDoPrimeiroPlano(RetanguloPx? Janela, EstadoDoShell? Shell, bool DoBuzzy);

/// <summary>
/// Uma mudança dos monitores ocupados, como vai ao núcleo (<see cref="FullscreenTargetsChanged"/>) e ao log
/// <c>TELA_CHEIA</c>.
/// </summary>
/// <param name="Ocupados">Os monitores que a janela em primeiro plano cobre inteiros, com o shell confirmando a tela cheia.</param>
/// <param name="Motivos">Os sinais agrupados nesta avaliação, na ordem em que chegaram.</param>
/// <param name="Shell">O sinal do shell lido nesta avaliação.</param>
/// <param name="Candidatos">Quantos monitores a janela cobre inteiros, antes do sinal do shell (diagnóstico do P7).</param>
internal readonly record struct MudancaDaTelaCheia(MonitoresOcupados Ocupados, string Motivos, EstadoDoShell? Shell, int Candidatos);

/// <summary>
/// Quando a raiz reavalia a tela cheia (DEC-013 e DEC-034; Q-09), com a leitura, a topologia e o agendador injetados, para
/// ser testada sem janela:
/// <list type="bullet">
/// <item>cada sinal (troca de primeiro plano, geometria da janela em primeiro plano, topologia publicada) pede a avaliação
/// (<see cref="Sinalizar"/>), que sai num disparo único, <see cref="Espera"/> depois do primeiro sinal da rajada, sem ser
/// adiada pelos seguintes: uma janela arrastada sem parar é reavaliada no máximo uma vez por espera;</item>
/// <item>a avaliação lê a janela em primeiro plano (<see cref="Avaliar"/>): ocupados são os monitores da topologia publicada
/// cuja tela inteira está dentro dela, só com o shell dizendo que há uma tela cheia (<see cref="ShellEmTelaCheia"/>). Sem o
/// sinal do shell, nada está ocupado: a área de trabalho, que cobre todos os monitores, e uma janela maximizada com a barra
/// escondida não contam. Com a janela do próprio Buzzy em primeiro plano (o menu), nada muda;</item>
/// <item>só uma mudança vai adiante (<see cref="MudancaDaTelaCheia"/>), uma vez: o mesmo conjunto de novo não é publicado.</item>
/// </list>
/// Nada é periódico: só disparos únicos depois de um sinal; em repouso, nenhum. <see cref="Parar"/>, no encerramento,
/// cancela o pendente, e nada mais é agendado. Só na thread da interface.
/// </summary>
internal sealed class AgendaDaTelaCheia
{
    /// <summary>Espera entre o primeiro sinal de uma rajada e a avaliação: junta a troca de primeiro plano e o redimensionamento que costuma vir com ela.</summary>
    internal static readonly TimeSpan Espera = TimeSpan.FromMilliseconds(250);

    /// <summary>Quantos motivos uma avaliação leva, no máximo; os seguintes só são contados (",+k" no fim).</summary>
    internal const int MaximoDeMotivos = 8;

    private readonly Func<LeituraDoPrimeiroPlano> _ler;
    private readonly Func<Topologia?> _topologia;
    private readonly Func<TimeSpan, Action, Action> _agendarUmaVez;
    private readonly Action<MudancaDaTelaCheia> _publicar;
    private readonly List<string> _motivos = [];
    private int _motivosAlemDoTeto;
    private Action? _cancelar;
    private bool _parada;

    /// <param name="ler">Lê a janela em primeiro plano agora (<see cref="Plataforma.ObservadorDeTelaCheia.Ler"/>).</param>
    /// <param name="topologia">A topologia publicada ao núcleo, a mesma que ele usa.</param>
    /// <param name="agendarUmaVez">Agenda um disparo único e devolve o que o cancela.</param>
    /// <param name="publicar">Recebe cada mudança dos monitores ocupados.</param>
    internal AgendaDaTelaCheia(Func<LeituraDoPrimeiroPlano> ler, Func<Topologia?> topologia, Func<TimeSpan, Action, Action> agendarUmaVez, Action<MudancaDaTelaCheia> publicar)
    {
        _ler = ler ?? throw new ArgumentNullException(nameof(ler));
        _topologia = topologia ?? throw new ArgumentNullException(nameof(topologia));
        _agendarUmaVez = agendarUmaVez ?? throw new ArgumentNullException(nameof(agendarUmaVez));
        _publicar = publicar ?? throw new ArgumentNullException(nameof(publicar));
    }

    /// <summary>Os monitores ocupados publicados por último; nenhum antes da primeira mudança.</summary>
    internal MonitoresOcupados Publicados { get; private set; } = MonitoresOcupados.Nenhum;

    /// <summary>Se há uma avaliação agendada.</summary>
    internal bool AvaliacaoPendente => _cancelar is not null;

    /// <summary>Um sinal que pode ter mudado a tela cheia: a avaliação sai <see cref="Espera"/> depois do primeiro da rajada.</summary>
    internal void Sinalizar(string motivo)
    {
        if (_parada) return;
        GuardarMotivo(motivo);
        if (_cancelar is not null) return;
        _cancelar = _agendarUmaVez(Espera, () =>
        {
            _cancelar = null;
            Avaliar();
        });
    }

    /// <summary>A avaliação agora, sem espera (a partida): cancela a pendente e leva os motivos dela.</summary>
    internal void AvaliarAgora(string motivo)
    {
        if (_parada) return;
        GuardarMotivo(motivo);
        _cancelar?.Invoke();
        _cancelar = null;
        Avaliar();
    }

    /// <summary>O encerramento: cancela a avaliação pendente; nada mais é agendado nem publicado.</summary>
    internal void Parar()
    {
        _parada = true;
        _cancelar?.Invoke();
        _cancelar = null;
    }

    /// <summary>
    /// Os monitores da topologia cuja tela inteira está dentro do retângulo da janela, em ordem de chave. Função pura: um
    /// retângulo vazio não cobre nada.
    /// </summary>
    internal static IReadOnlyList<string> Cobertos(Topologia topologia, RetanguloPx janela)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        if (janela.Largura <= 0 || janela.Altura <= 0) return [];
        return [.. topologia.Monitores.Where(m => janela.Contem(m.Tela)).Select(m => m.Chave).Order(StringComparer.Ordinal)];
    }

    /// <summary>
    /// Se o shell diz que há uma tela cheia: um aplicativo em tela cheia, um Direct3D exclusivo ou um da Store. As
    /// configurações de apresentação sozinhas, a sessão ausente e a falha da consulta não contam.
    /// </summary>
    internal static bool ShellEmTelaCheia(EstadoDoShell? estado)
        => estado is EstadoDoShell.Ocupado or EstadoDoShell.D3dEmTelaCheia or EstadoDoShell.App;

    private void Avaliar()
    {
        if (_parada) return;
        string motivos = TirarMotivos();
        LeituraDoPrimeiroPlano leitura = _ler();
        if (leitura.DoBuzzy || _topologia() is not { } topologia) return;

        IReadOnlyList<string> cobertos = leitura.Janela is { } janela ? Cobertos(topologia, janela) : [];
        var ocupados = ShellEmTelaCheia(leitura.Shell) ? new MonitoresOcupados(cobertos) : MonitoresOcupados.Nenhum;
        if (ocupados.Equals(Publicados)) return;
        Publicados = ocupados;
        _publicar(new MudancaDaTelaCheia(ocupados, motivos, leitura.Shell, cobertos.Count));
    }

    private void GuardarMotivo(string motivo)
    {
        if (_motivos.Count < MaximoDeMotivos) _motivos.Add(motivo);
        else _motivosAlemDoTeto++;
    }

    private string TirarMotivos()
    {
        string texto = string.Join(",", _motivos) + (_motivosAlemDoTeto > 0 ? $",+{_motivosAlemDoTeto}" : "");
        _motivos.Clear();
        _motivosAlemDoTeto = 0;
        return texto;
    }
}

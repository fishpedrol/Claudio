using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Buzzy.App.Apresentacao;
using Buzzy.App.Plataforma;
using Buzzy.Core;
using Buzzy.Core.Personagem;

namespace Buzzy.App.Composicao;

/// <summary>
/// Raiz de composição das Fases 1 e 2 (DEC-007): liga o núcleo puro do personagem e do
/// posicionamento ao adaptador de plataforma (janelas, bandeja, menu e mensagens do Windows).
///
/// Ocioso por eventos (DEC-011): nada roda em timer periódico. Os únicos timers são de uma
/// vez só: o agrupamento das mensagens de topologia (300 ms depois de uma mensagem, com até
/// três novas tentativas se a leitura vier incoerente) e novas tentativas de pôr o ícone na
/// bandeja quando a Shell recusa.
/// </summary>
internal sealed class Aplicacao
{
    /// <summary>
    /// Espera depois da última mensagem de topologia antes de reler (ARCHITECTURE.md 2.4).
    /// Valor provisório: o intervalo definitivo será calibrado em P5, antes da Fase 5.
    /// </summary>
    private static readonly TimeSpan Agrupamento = TimeSpan.FromMilliseconds(300);

    /// <summary>Esperas das novas tentativas quando a leitura da topologia vem incoerente.</summary>
    private static readonly TimeSpan[] EsperasDeReleitura = [TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)];

    private const int TentativasDaBandeja = 3;

    private readonly Application _app;
    private readonly InstanciaUnica _instancia;
    private readonly DispatcherTimer _agrupador;
    private readonly DispatcherTimer _repetirBandeja;
    private readonly DispatcherTimer _relogio;
    private readonly List<string> _motivosPendentes = [];
    private readonly Dictionary<Evento, string> _motivosDoNucleo = new(ReferenceEqualityComparer.Instance);

    private JanelaDeServico? _servico;
    private JanelaPersonagem? _personagem;
    private Bandeja? _bandeja;
    private Nucleo? _nucleo;
    private Topologia _topologia = null!;
    private Posicionamento _posicionamento = null!;
    private DispatcherTimer? _decisaoAutonoma;
    private EventHandler? _aoDispararDecisao;
    private TimeSpan _tempoAcumulado;
    private long _ultimaMarcacaoRelogio;
    private long _passosDoRelogio;
    private int _dpiDoSprite;
    private int _dpiDoIcone;
    private int _releiturasFalhas;
    private int _tentativasDaBandeja;
    private bool _visivel;
    private bool _primeiroQuadroRegistrado;
    private bool _encerrando;
    private bool _processandoNucleo;

    internal Aplicacao(Application app, InstanciaUnica instancia)
    {
        _app = app;
        _instancia = instancia;
        _agrupador = new DispatcherTimer(DispatcherPriority.Normal) { Interval = Agrupamento };
        _agrupador.Tick += AoAgrupar;
        _repetirBandeja = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromSeconds(2) };
        _repetirBandeja.Tick += (_, _) =>
        {
            _repetirBandeja.Stop();
            AdicionarIconeNaBandeja();
        };
        _relogio = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromSeconds(1d / 60),
        };
        _relogio.Tick += AoTiqueDoRelogio;
    }

    internal void Iniciar()
    {
        // Limitação aceita do System.Windows.Application: o WPF encerra na pergunta de fim de
        // sessão (WM_QUERYENDSESSION) se ninguém cancelar; um desligamento cancelado depois por
        // outro aplicativo deixa o Buzzy fechado (DECISIONS.md).
        _app.SessionEnding += (_, _) => Enviar(new SessionEnding(), "fim de sessão");
        _app.DispatcherUnhandledException += (_, e) =>
        {
            Diagnostico.Evento("ERRO", ("tipo", e.Exception.GetType().Name), ("mensagem", e.Exception.Message));
            _bandeja?.Remover();
        };

        Topologia? topologia = LerTopologiaNaPartida();
        if (topologia is null)
        {
            _app.Shutdown(CodigosDeSaida.TopologiaIlegivel);
            return;
        }
        _topologia = topologia;
        Diagnostico.Evento("TOPOLOGIA", ("motivo", "início"), ("impressao", topologia.ImpressaoDigital));

        _servico = new JanelaDeServico();
        _servico.BandejaAcionada += AoAcionarBandeja;
        _servico.BarraDeTarefasRecriada += AoRecriarBarra;
        _servico.TopologiaPodeTerMudado += AoPossivelMudancaDeTopologia;
        Diagnostico.Evento("SERVICO", ("hwnd", _servico.Hwnd));

        _personagem = new JanelaPersonagem();
        _personagem.MenuSolicitado += p => Adiar(() => Enviar(new ContextMenu(p), "personagem"));
        _personagem.DpiMudou += dpi => AoPossivelMudancaDeTopologia($"WM_DPICHANGED {dpi}");
        _personagem.Minimizada += () => Adiar(() => Enviar(new CmdHide(), "minimizado pelo Windows"));
        _personagem.Closing += (_, e) =>
        {
            if (_encerrando) return;
            e.Cancel = true;
            Adiar(() => Enviar(new CmdExit(), "fechamento da janela"));
        };
        _personagem.ContentRendered += AoPrimeiroQuadro;

        // Cria a janela sem mostrá-la. Loaded escolhe o primeiro posicionamento e os efeitos
        // do núcleo a colocam em pixels físicos antes de torná-la visível.
        new WindowInteropHelper(_personagem).EnsureHandle();
        Diagnostico.Evento("JANELA", ("hwnd", _personagem.Hwnd));

        ulong semente = unchecked((ulong)Environment.TickCount64);
        _nucleo = new Nucleo(new ConfiguracaoDoNucleo
        {
            Tamanho = SpriteProvisorio.TamanhoLogico,
            Acoes = AcoesAutonomas.Descansar | AcoesAutonomas.TrocarExpressao,
        }, semente);
        Diagnostico.Evento("NUCLEO", ("semente", semente));
        Enviar(new Loaded(topologia, PosicaoSalva: null, Preferencias.Padrao), "início");

        _dpiDoIcone = topologia.Principal.Dpi;
        _bandeja = new Bandeja(_servico.Hwnd, CriarIcone(_dpiDoIcone));
        AdicionarIconeNaBandeja();

        _instancia.EscutarPedidos(() => Adiar(() =>
        {
            Diagnostico.Evento("INSTANCIA", ("papel", "primeira"), ("pedido", "mostrar"));
            MostrarPorComando("segunda instância");
        }));
    }

    // ------------------------------------------------------------------ eventos

    private void AoPrimeiroQuadro(object? remetente, EventArgs e)
    {
        if (_primeiroQuadroRegistrado) return;
        _primeiroQuadroRegistrado = true;
        using Process atual = Process.GetCurrentProcess();
        double ms = (DateTime.Now - atual.StartTime).TotalMilliseconds;
        Diagnostico.Evento("PRIMEIRO_QUADRO", ("ms", Math.Round(ms)));
    }

    private void AoAcionarBandeja(AcaoNaBandeja acao, PontoPx? ancora)
    {
        PontoPx ponto = ancora ?? PontoDoIcone();
        Diagnostico.Evento("BANDEJA", ("acao", acao), ("ancora", ponto));
        if (acao == AcaoNaBandeja.Selecionar)
            Adiar(() => MostrarPorComando("bandeja"));
        else
            Adiar(() => ExibirMenuDoDesktop(ponto, "bandeja", peloTeclado: acao == AcaoNaBandeja.MenuPeloTeclado));
    }

    private void AoRecriarBarra()
    {
        // A barra de tarefas reinicia ou o DPI do monitor principal muda (a Shell difunde a
        // mesma mensagem): o ícone é refeito no tamanho do DPI atual.
        Diagnostico.Evento("BANDEJA", ("barraDeTarefasRecriada", "sim"));
        if (_bandeja is null) return;
        Topologia? atual = LeitorDeTopologia.Ler(out _);
        if (atual is not null) _topologia = atual;
        _dpiDoIcone = _topologia.Principal.Dpi;
        _bandeja.TrocarIcone(CriarIcone(_dpiDoIcone), aplicar: false);
        _tentativasDaBandeja = 0;
        if (!_bandeja.Recriar()) AgendarNovaTentativaDaBandeja();
        _servico!.NotificacoesVersao4 = _bandeja.Versao4;
    }

    private void AoPossivelMudancaDeTopologia(string motivo)
    {
        if (_encerrando) return;
        _motivosPendentes.Add(motivo);
        _releiturasFalhas = 0;
        _agrupador.Stop();
        _agrupador.Interval = Agrupamento;
        _agrupador.Start();
    }

    private void AoAgrupar(object? remetente, EventArgs e)
    {
        _agrupador.Stop();
        if (_encerrando) return;
        string motivos = string.Join(",", _motivosPendentes);

        Topologia? nova = LeitorDeTopologia.Ler(out string? erro);
        if (nova is null)
        {
            // Leitura incoerente (troca de modo em andamento): mantém a anterior e tenta de
            // novo algumas vezes, com esperas crescentes. Não vira atividade periódica.
            bool vaiRepetir = _releiturasFalhas < EsperasDeReleitura.Length;
            Diagnostico.Evento("TOPOLOGIA", ("motivo", motivos), ("erro", erro), ("mantida", "anterior"), ("novaTentativa", vaiRepetir));
            if (vaiRepetir)
            {
                _agrupador.Interval = EsperasDeReleitura[_releiturasFalhas++];
                _agrupador.Start();
            }
            else
            {
                _motivosPendentes.Clear();
            }
            return;
        }

        _motivosPendentes.Clear();
        bool mudou = !nova.MesmaConfiguracao(_topologia);
        _topologia = nova;
        Enviar(new TopologyChanged(nova), motivos);

        if (nova.Principal.Dpi != _dpiDoIcone && _bandeja is not null)
        {
            _dpiDoIcone = nova.Principal.Dpi;
            _bandeja.TrocarIcone(CriarIcone(_dpiDoIcone), aplicar: true);
        }

        Diagnostico.Evento("TOPOLOGIA", ("motivo", motivos), ("mudou", mudou ? "sim" : "nao"), ("visivel", _visivel), ("impressao", nova.ImpressaoDigital));
    }

    // ------------------------------------------------------------------ ações

    private static Topologia? LerTopologiaNaPartida()
    {
        string? erro = null;
        for (int tentativa = 1; tentativa <= 5; tentativa++)
        {
            Topologia? t = LeitorDeTopologia.Ler(out erro);
            if (t is not null) return t;
            Thread.Sleep(200);
        }
        Diagnostico.Evento("ERRO", ("etapa", "topologia inicial"), ("mensagem", erro));
        return null;
    }

    private void AplicarNaJanela(Posicionamento p)
    {
        if (_personagem is null) return;
        if (p.Monitor.Dpi != _dpiDoSprite || _personagem.Sprite is null)
        {
            _personagem.DefinirSprite(SpriteProvisorio.Renderizar(p.Monitor.Dpi));
            _dpiDoSprite = p.Monitor.Dpi;
        }
        _personagem.AplicarRetangulo(p.Retangulo);
        RegistrarPosicao(p);
    }

    /// <summary>
    /// Só com <c>--diagnostico</c>: registra a posição e dois pontos de teste do sprite
    /// (um opaco e um transparente, em coordenadas de tela), usados pela verificação da Fase 1.
    /// </summary>
    private void RegistrarPosicao(Posicionamento p)
    {
        if (!Diagnostico.Ligado || _personagem?.Sprite is not BitmapSource sprite) return;
        string opaco = "indisponível", transparente = "indisponível";
        try
        {
            (PontoPx o, PontoPx t) = SpriteProvisorio.PontosDeTeste(sprite);
            opaco = $"{p.Retangulo.Esquerda + o.X},{p.Retangulo.Topo + o.Y}";
            transparente = $"{p.Retangulo.Esquerda + t.X},{p.Retangulo.Topo + t.Y}";
        }
        catch (InvalidOperationException e)
        {
            Diagnostico.Evento("ERRO", ("etapa", "pontos de teste do sprite"), ("mensagem", e.Message));
        }
        Diagnostico.Evento("POSICAO",
            ("monitor", p.Monitor.Chave),
            ("retangulo", p.Retangulo),
            ("ancora", p.Ancora),
            ("dpi", p.Monitor.Dpi),
            ("areaUtil", p.Monitor.AreaUtil),
            ("pontoOpaco", opaco),
            ("pontoTransparente", transparente));
    }

    private void MostrarPorComando(string motivo)
    {
        if (_encerrando || _personagem is null) return;
        bool jaEstavaVisivel = _visivel;

        // O núcleo usa a topologia que conhece. Atualize-a antes de CMD_SHOW para que uma
        // mudança ocorrida enquanto estava escondido seja validada antes de a janela reaparecer.
        Topologia? atual = LeitorDeTopologia.Ler(out _);
        if (atual is not null)
        {
            _topologia = atual;
            Enviar(new TopologyChanged(atual), $"revalidar antes de mostrar: {motivo}");
        }

        Enviar(new CmdShow(), motivo);
        if (_encerrando || !_visivel) return;

        _personagem.ReafirmarTopo();
        if (jaEstavaVisivel) RegistrarVisibilidade(true, motivo);
    }

    private void ExibirMenuDoDesktop(PontoPx ponto, string origem, bool peloTeclado)
    {
        if (_encerrando) return;
        // A decisão usa o estado do momento em que o menu abriu, que é o texto que o
        // usuário leu no item. O laço modal do menu continua despachando operações.
        bool visivelAoAbrir = _visivel;
        Diagnostico.Evento("MENU", ("aberto", origem), ("ponto", ponto), ("peloTeclado", peloTeclado ? "sim" : "nao"));
        ComandoDoMenu comando = MenuNativo.Mostrar(ponto, visivelAoAbrir, abrirParaCima: origem == "bandeja");
        if (_encerrando) return;

        switch (comando)
        {
            case ComandoDoMenu.AlternarVisibilidade when visivelAoAbrir:
                Enviar(new CmdHide(), "menu");
                break;
            case ComandoDoMenu.AlternarVisibilidade:
                MostrarPorComando("menu");
                break;
            case ComandoDoMenu.Sair:
                Enviar(new CmdExit(), "menu");
                break;
            case ComandoDoMenu.Nenhum when peloTeclado:
                // Quem abriu o menu da bandeja pelo teclado e o cancelou volta para a área de
                // notificação (documentação do NIM_SETFOCUS). Depois de um cancelamento com o
                // mouse, não: o clique fora pode ter ativado outro aplicativo.
                _bandeja?.DevolverFoco();
                break;
        }
    }

    private void Enviar(Evento evento, string motivo)
    {
        if (_encerrando) return;
        if (_nucleo is null) throw new InvalidOperationException("O núcleo ainda não foi criado.");

        if (!_nucleo.Enfileirar(evento))
        {
            Diagnostico.Evento("NUCLEO", ("evento", evento.GetType().Name), ("descartado", "autônomo sob controle do usuário"));
            return;
        }

        _motivosDoNucleo[evento] = motivo;
        ProcessarFilaDoNucleo();
    }

    private void ProcessarFilaDoNucleo()
    {
        if (_processandoNucleo || _nucleo is null) return;
        _processandoNucleo = true;
        try
        {
            while (_nucleo.Pendentes > 0 && !_encerrando)
            {
                Evento[] eventosDoLote = [.. _motivosDoNucleo.Keys];
                var efeitos = new List<(Evento Evento, Efeito Efeito, string Motivo)>();
                long descartadosAntes = _nucleo.Descartados;
                try
                {
                    _nucleo.Processar((eventoAplicado, resultado) =>
                    {
                        string motivoEvento = _motivosDoNucleo.GetValueOrDefault(eventoAplicado, "sem motivo");
                        foreach (Transicao transicao in resultado.Transicoes)
                        {
                            Diagnostico.Evento("NUCLEO",
                                ("evento", eventoAplicado.GetType().Name),
                                ("motivo", motivoEvento),
                                ("de", transicao.De),
                                ("para", transicao.Para),
                                ("regra", transicao.Regra));
                        }
                        foreach (Efeito efeito in resultado.Efeitos)
                            efeitos.Add((eventoAplicado, efeito, motivoEvento));
                    });
                }
                finally
                {
                    foreach (Evento eventoDoLote in eventosDoLote)
                        _motivosDoNucleo.Remove(eventoDoLote);
                }

                long descartados = _nucleo.Descartados - descartadosAntes;
                if (descartados > 0)
                    Diagnostico.Evento("NUCLEO", ("autonomosDescartados", descartados));

                foreach ((Evento evento, Efeito efeito, string motivoEvento) in efeitos)
                {
                    if (_encerrando) break;
                    ExecutarEfeito(evento, efeito, motivoEvento);
                }
            }
        }
        finally
        {
            _processandoNucleo = false;
        }
    }

    private void ExecutarEfeito(Evento evento, Efeito efeito, string motivo)
    {
        switch (efeito)
        {
            case MoverJanela mover:
                _posicionamento = mover.Destino;
                AplicarNaJanela(mover.Destino);
                break;

            case MostrarJanela:
                if (_personagem is null || _visivel) break;
                _personagem.Show();
                _visivel = true;
                // O WPF pode reaplicar a posição inicial ao mostrar a janela. Confirma o
                // retângulo físico depois de Show(), como fazia a composição da Fase 1.
                AplicarNaJanela(_posicionamento);
                RegistrarVisibilidade(true, motivo);
                break;

            case EsconderJanela:
                if (_personagem is null || !_visivel) break;
                _personagem.Hide();
                _visivel = false;
                RegistrarVisibilidade(false, motivo);
                break;

            case LigarRelogio:
                IniciarRelogio();
                break;

            case DesligarRelogio:
                _relogio.Stop();
                _tempoAcumulado = TimeSpan.Zero;
                break;

            case AgendarDecisao agendar:
                AgendarTemporizadorDeDecisao(agendar);
                break;

            case CancelarDecisao:
                CancelarDecisaoAutonoma();
                break;

            case LiberarCaptura:
                // A captura e os gestos reais de mouse entram na Fase 3; a Fase 2 não mantém captura.
                break;

            case AbrirMenu pedidoMenu:
                ExibirMenuDoDesktop(pedidoMenu.Ponto, "personagem", peloTeclado: false);
                break;

            case Encerrar:
                EncerrarAplicacao(motivo);
                break;

            case GravarPosicao:
            case GravarPreferencias:
            case AbrirPainelDeEnergia:
            case FecharPainelDeEnergia:
            case AbrirConfiguracoes:
                // Estes recursos são ativados nas fases de persistência e configurações.
                Diagnostico.Evento("NUCLEO", ("efeitoPendente", efeito.GetType().Name), ("evento", evento.GetType().Name));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(efeito), efeito, "Efeito do núcleo sem adaptador.");
        }
    }

    private void RegistrarVisibilidade(bool visivel, string motivo)
        => Diagnostico.Evento("VISIVEL", ("visivel", visivel ? "sim" : "nao"), ("motivo", motivo));

    private void IniciarRelogio()
    {
        if (_relogio.IsEnabled) return;
        _tempoAcumulado = TimeSpan.Zero;
        _ultimaMarcacaoRelogio = Stopwatch.GetTimestamp();
        _relogio.Start();
    }

    private void AoTiqueDoRelogio(object? remetente, EventArgs e)
    {
        if (_nucleo is null || !_relogio.IsEnabled) return;
        long agora = Stopwatch.GetTimestamp();
        _tempoAcumulado += Stopwatch.GetElapsedTime(_ultimaMarcacaoRelogio, agora);
        _ultimaMarcacaoRelogio = agora;

        TimeSpan passo = TimeSpan.FromSeconds(1d / _nucleo.Configuracao.PassosPorSegundo);
        while (_tempoAcumulado >= passo && _relogio.IsEnabled)
        {
            _tempoAcumulado -= passo;
            Enviar(new Tick(), $"relógio passo {++_passosDoRelogio}");
        }
    }

    private void AgendarTemporizadorDeDecisao(AgendarDecisao agendamento)
    {
        CancelarDecisaoAutonoma();
        var temporizador = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = agendamento.Atraso > TimeSpan.Zero ? agendamento.Atraso : TimeSpan.FromMilliseconds(1),
        };
        EventHandler aoDisparar = null!;
        aoDisparar = (_, _) =>
        {
            temporizador.Stop();
            temporizador.Tick -= aoDisparar;
            if (ReferenceEquals(_decisaoAutonoma, temporizador))
            {
                _decisaoAutonoma = null;
                _aoDispararDecisao = null;
            }
            Enviar(new AutonomyTimer(agendamento.Geracao), $"agenda autônoma geração {agendamento.Geracao}");
        };
        _decisaoAutonoma = temporizador;
        _aoDispararDecisao = aoDisparar;
        temporizador.Tick += aoDisparar;
        temporizador.Start();
    }

    private void CancelarDecisaoAutonoma()
    {
        DispatcherTimer? temporizador = _decisaoAutonoma;
        EventHandler? aoDisparar = _aoDispararDecisao;
        _decisaoAutonoma = null;
        _aoDispararDecisao = null;
        if (temporizador is null) return;
        temporizador.Stop();
        if (aoDisparar is not null) temporizador.Tick -= aoDisparar;
    }

    private void EncerrarAplicacao(string motivo)
    {
        if (_encerrando) return;
        _encerrando = true;
        Diagnostico.Evento("ENCERRANDO", ("motivo", motivo));

        // Um menu aberto nesta thread (por exemplo, fim de sessão com o menu na tela) é
        // fechado antes de destruir as janelas.
        Win32.EndMenu();
        _agrupador.Stop();
        _repetirBandeja.Stop();
        _relogio.Stop();
        CancelarDecisaoAutonoma();
        _bandeja?.Dispose();
        _servico?.Dispose();
        _personagem?.Close();
        _app.Shutdown(CodigosDeSaida.Normal);
    }

    // ------------------------------------------------------------------ apoio

    /// <summary>Executa depois de a mensagem atual terminar, na thread da interface.</summary>
    private void Adiar(Action acao) => _app.Dispatcher.BeginInvoke(acao);

    private void AdicionarIconeNaBandeja()
    {
        if (_bandeja is null || _encerrando) return;
        if (_bandeja.Adicionar())
        {
            _servico!.NotificacoesVersao4 = _bandeja.Versao4;
            return;
        }
        AgendarNovaTentativaDaBandeja();
    }

    private void AgendarNovaTentativaDaBandeja()
    {
        if (++_tentativasDaBandeja > TentativasDaBandeja) return;
        _repetirBandeja.Stop();
        _repetirBandeja.Start();
    }

    /// <summary>Âncora de reserva para o menu quando a notificação não traz coordenadas.</summary>
    private PontoPx PontoDoIcone()
    {
        if (_bandeja?.Retangulo() is { } r) return r.Centro;
        RetanguloPx area = _topologia.Principal.AreaUtil;
        return new PontoPx(area.Direita - 1, area.Base - 1);
    }

    private static nint CriarIcone(int dpiPrincipal)
    {
        int lado = Win32.GetSystemMetricsForDpi(Win32.SM_CXSMICON, (uint)dpiPrincipal);
        if (lado <= 0) lado = 16;
        byte[] png = SpriteProvisorio.IconePng(lado);
        nint icone = Win32.CreateIconFromResourceEx(png, png.Length, true, 0x00030000, lado, lado, 0);
        Diagnostico.Evento("ICONE", ("lado", lado), ("dpi", dpiPrincipal), ("criado", icone != 0));
        return icone;
    }
}

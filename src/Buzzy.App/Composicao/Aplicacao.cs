using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Buzzy.App.Apresentacao;
using Buzzy.App.Plataforma;
using Buzzy.Core;
using Buzzy.Core.Entrada;
using Buzzy.Core.Personagem;

namespace Buzzy.App.Composicao;

/// <summary>Opções da linha de comando que a raiz de composição usa.</summary>
/// <param name="MovimentoPausado">
/// <c>--pausado</c>: começa com o movimento autônomo pausado, como o item "Pausar movimento" do
/// menu. Usado pelas verificações de tela, que precisam do personagem parado no lugar inicial.
/// </param>
/// <param name="Semente">
/// <c>--semente N</c>: semente fixa da agenda autônoma, para reproduzir uma sequência de
/// comportamento (diagnóstico e testes). Sem ela, a semente vem do relógio do sistema.
/// </param>
internal sealed record OpcoesDaAplicacao(bool MovimentoPausado, ulong? Semente)
{
    internal static readonly OpcoesDaAplicacao Padrao = new(false, null);
}

/// <summary>
/// Raiz de composição das Fases 1 a 4 (DEC-007): liga o núcleo puro do personagem, do
/// posicionamento e da arbitragem de input ao adaptador de plataforma (janelas, captura do
/// mouse, bandeja, menu e mensagens do Windows) e escolhe o quadro do sprite pelo retrato.
///
/// Ocioso por eventos (DEC-011): em repouso, nada roda em timer periódico. Os timers são de uma
/// vez só — o agrupamento das mensagens de topologia (300 ms depois de uma mensagem, com até
/// três novas tentativas se a leitura vier incoerente), novas tentativas de pôr o ícone na
/// bandeja e a próxima decisão da agenda autônoma. O relógio de passo fixo só corre enquanto o
/// núcleo pede (reação, pouso, gesto curto e movimento) e anda junto com os quadros do
/// compositor do WPF (<see cref="CompositionTarget.Rendering"/>): a janela se move no máximo uma
/// vez por quadro, com a posição mais recente. O arraste não usa relógio: cada movimento do
/// mouse vira posição da janela no mesmo tratamento da mensagem.
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

    /// <summary>Maior atraso que o relógio de passo fixo recupera de uma vez (15 passos a 60 Hz).</summary>
    private static readonly TimeSpan AtrasoMaximoDoRelogio = TimeSpan.FromMilliseconds(250);

    private readonly Application _app;
    private readonly InstanciaUnica _instancia;
    private readonly OpcoesDaAplicacao _opcoes;
    private readonly DispatcherTimer _agrupador;
    private readonly DispatcherTimer _repetirBandeja;
    private readonly List<string> _motivosPendentes = [];
    private readonly Dictionary<Evento, string> _motivosDoNucleo = new(ReferenceEqualityComparer.Instance);
    private readonly ArbitroDeGestos _arbitro = new();

    /// <summary>M5 do arraste em curso (DEC-011), só com <c>--diagnostico</c>: ms de cada movimento até a janela no lugar.</summary>
    private readonly List<double> _latenciasDoArraste = [];

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

    /// <summary>A janela foi movida durante o arraste sem registrar a posição (diagnóstico).</summary>
    private bool _posicaoSemRegistro;

    /// <summary>Se o relógio de passo fixo está ligado (inscrito nos quadros do compositor).</summary>
    private bool _relogioLigado;

    /// <summary>Quadro do sprite na janela e o estado em que ele começou a ser contado.</summary>
    private QuadroDoSprite? _quadroAtual;
    private Estado _estadoDoQuadro = Estado.Booting;
    private int _quiquesDoQuadro;
    private long _passoDeEntradaNoEstado;

    internal Aplicacao(Application app, InstanciaUnica instancia, OpcoesDaAplicacao? opcoes = null)
    {
        _app = app;
        _instancia = instancia;
        _opcoes = opcoes ?? OpcoesDaAplicacao.Padrao;
        _agrupador = new DispatcherTimer(DispatcherPriority.Normal) { Interval = Agrupamento };
        _agrupador.Tick += AoAgrupar;
        _repetirBandeja = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromSeconds(2) };
        _repetirBandeja.Tick += (_, _) =>
        {
            _repetirBandeja.Stop();
            AdicionarIconeNaBandeja();
        };
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
        _personagem.Ponteiro += AoPonteiro;
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

        ulong semente = _opcoes.Semente ?? unchecked((ulong)Environment.TickCount64);
        // Fase 4 (DEC-022): física do movimento, queda animada e todas as ações autônomas.
        _nucleo = new Nucleo(new ConfiguracaoDoNucleo
        {
            Tamanho = SpriteProvisorio.TamanhoLogico,
            Acoes = AcoesAutonomas.Todas,
            QuedaFisica = true,
            Movimento = true,
        }, semente);
        Diagnostico.Evento("NUCLEO", ("semente", semente), ("pausado", _opcoes.MovimentoPausado ? "sim" : "nao"));
        Enviar(new Loaded(topologia, PosicaoSalva: null, Preferencias.Padrao), "início");
        if (_opcoes.MovimentoPausado) Enviar(new CmdPauseAutonomy(), "linha de comando --pausado");

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

    /// <summary>
    /// Ponteiro sobre o personagem ou capturado num gesto começado nele (ARCHITECTURE.md 2.7): a
    /// arbitragem decide o gesto e a captura, e o núcleo aplica cada gesto no mesmo tratamento da
    /// mensagem — um movimento de arraste vira posição da janela antes de a mensagem acabar.
    /// </summary>
    private void AoPonteiro(EventoDePonteiro evento)
    {
        if (_encerrando || _nucleo is null || _personagem is null) return;
        long recebido = Stopwatch.GetTimestamp();
        Arbitragem arbitragem = _arbitro.Receber(evento);

        // A captura acompanha o gesto: pega no botão esquerdo pressionado e solta no fim dele.
        if (arbitragem.Capturar) _personagem.Capturar();
        else _personagem.SoltarCaptura();

        bool moveu = false;
        foreach (Evento gesto in arbitragem.Gestos)
        {
            if (gesto is DragStart) _latenciasDoArraste.Clear();
            Enviar(gesto, "ponteiro");
            moveu |= gesto is DragMove;
            if (gesto is DragEnd or DragCancel) RegistrarFimDoArraste(gesto);
        }

        if (moveu && Diagnostico.Ligado)
            _latenciasDoArraste.Add(Stopwatch.GetElapsedTime(recebido).TotalMilliseconds);
    }

    /// <summary>
    /// Só com <c>--diagnostico</c>: resumo do arraste e M5 (DEC-011), sem registrar cada movimento.
    /// A posição validada ao soltar já foi registrada no fim do processamento do gesto.
    /// </summary>
    private void RegistrarFimDoArraste(Evento fim)
    {
        if (!Diagnostico.Ligado) return;
        double[] ms = [.. _latenciasDoArraste.Order()];
        _latenciasDoArraste.Clear();
        if (ms.Length == 0)
        {
            Diagnostico.Evento("ARRASTE", ("fim", fim.GetType().Name), ("movimentos", 0));
            return;
        }
        double p95 = ms[Math.Min(ms.Length - 1, (int)Math.Ceiling(ms.Length * 0.95) - 1)];
        Diagnostico.Evento("ARRASTE",
            ("fim", fim.GetType().Name),
            ("movimentos", ms.Length),
            ("m5MediaMs", Math.Round(ms.Average(), 3)),
            ("m5P95Ms", Math.Round(p95, 3)),
            ("m5MaxMs", Math.Round(ms[^1], 3)));
    }

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
        ReafirmarLugarDaJanela(motivos);

        if (nova.Principal.Dpi != _dpiDoIcone && _bandeja is not null)
        {
            _dpiDoIcone = nova.Principal.Dpi;
            _bandeja.TrocarIcone(CriarIcone(_dpiDoIcone), aplicar: true);
        }

        Diagnostico.Evento("TOPOLOGIA", ("motivo", motivos), ("mudou", mudou ? "sim" : "nao"), ("visivel", _visivel), ("impressao", nova.ImpressaoDigital));
    }

    // ------------------------------------------------------------------ ações

    /// <summary>
    /// Depois de reler a topologia: se a janela não está onde o núcleo a pôs (o Windows a
    /// reposicionou ao trocar monitor ou DPI, ou outro agente a moveu) ou o sprite ficou em outro
    /// DPI, reaplica o lugar do núcleo, que é quem decide a posição. Nunca no meio de um gesto do
    /// usuário (ARCHITECTURE.md 2.8: com o botão pressionado, a validação é ao soltar).
    /// </summary>
    private void ReafirmarLugarDaJanela(string motivo)
    {
        if (_personagem is null || _nucleo is null || !_visivel || _encerrando) return;
        if (_nucleo.Estado.Estado is Estado.Pressed or Estado.Dragging) return;
        if (_nucleo.Estado.Lugar is not { } lugar) return;
        RetanguloPx? real = _personagem.RetanguloReal();
        if (real == lugar.Retangulo && lugar.Monitor.Dpi == _dpiDoSprite) return;
        Diagnostico.Evento("POSICAO", ("reaplicada", "sim"), ("motivo", motivo), ("real", real), ("nucleo", lugar.Retangulo));
        _posicionamento = lugar;
        AplicarNaJanela(lugar);
    }

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
            QuadroDoSprite quadro = _quadroAtual ?? new QuadroDoSprite("parado", false, null);
            _personagem.DefinirSprite(SpriteProvisorio.Renderizar(quadro, p.Monitor.Dpi));
            _quadroAtual = quadro;
            _dpiDoSprite = p.Monitor.Dpi;
        }
        _personagem.AplicarRetangulo(p.Retangulo);
        // Durante o arraste e o movimento, registrar cada passo pesaria no próprio movimento: a
        // posição é registrada quando o personagem para (fim de ProcessarFilaDoNucleo).
        if (EmMovimentoOuArraste()) _posicaoSemRegistro = true;
        else RegistrarPosicao(p);
    }

    private bool EmMovimentoOuArraste()
        => _nucleo?.Estado.Estado is { } e && (e == Estado.Dragging || e.EmMovimento());

    /// <summary>
    /// Só com <c>--diagnostico</c>: registra a posição e dois pontos de teste do sprite em pé (um
    /// opaco e um transparente, em coordenadas de tela), usados pelas verificações de tela para
    /// clicar no personagem parado. Os pontos são sempre os do quadro "parado" no DPI do monitor.
    /// </summary>
    private void RegistrarPosicao(Posicionamento p)
    {
        _posicaoSemRegistro = false;
        if (!Diagnostico.Ligado || _personagem is null) return;
        string opaco = "indisponível", transparente = "indisponível";
        try
        {
            (PontoPx o, PontoPx t) = SpriteProvisorio.PontosDeTeste(SpriteProvisorio.Renderizar(p.Monitor.Dpi));
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
        bool pausadoAoAbrir = _nucleo?.Estado.AutonomiaPausada ?? false;
        Diagnostico.Evento("MENU", ("aberto", origem), ("ponto", ponto), ("peloTeclado", peloTeclado ? "sim" : "nao"));
        ComandoDoMenu comando = MenuNativo.Mostrar(ponto, visivelAoAbrir, pausadoAoAbrir, abrirParaCima: origem == "bandeja");
        if (_encerrando) return;

        switch (comando)
        {
            case ComandoDoMenu.AlternarVisibilidade when visivelAoAbrir:
                Enviar(new CmdHide(), "menu");
                break;
            case ComandoDoMenu.AlternarVisibilidade:
                MostrarPorComando("menu");
                break;
            case ComandoDoMenu.AlternarMovimento when pausadoAoAbrir:
                Enviar(new CmdResumeAutonomy(), "menu");
                break;
            case ComandoDoMenu.AlternarMovimento:
                Enviar(new CmdPauseAutonomy(), "menu");
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
        if (!Enfileirar(evento, motivo)) return;
        ProcessarFilaDoNucleo();
    }

    /// <summary>Põe o evento na fila do núcleo sem processar (lote de passos do relógio).</summary>
    private bool Enfileirar(Evento evento, string motivo)
    {
        if (_encerrando) return false;
        if (_nucleo is null) throw new InvalidOperationException("O núcleo ainda não foi criado.");

        if (!_nucleo.Enfileirar(evento))
        {
            Diagnostico.Evento("NUCLEO", ("evento", evento.GetType().Name), ("descartado", "autônomo sob controle do usuário"));
            return false;
        }

        _motivosDoNucleo[evento] = motivo;
        return true;
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

                for (int i = 0; i < efeitos.Count; i++)
                {
                    if (_encerrando) break;
                    (Evento evento, Efeito efeito, string motivoEvento) = efeitos[i];
                    // Num lote (vários passos do relógio no mesmo quadro), só a última posição
                    // antes do próximo mostrar/esconder vai para a janela: uma movimentação por quadro.
                    if (efeito is MoverJanela && MovimentacaoPosterior(efeitos, i)) continue;
                    ExecutarEfeito(evento, efeito, motivoEvento);
                }
            }
        }
        finally
        {
            _processandoNucleo = false;
        }
        AtualizarSprite();
        if (_posicaoSemRegistro && _visivel && !_encerrando && !EmMovimentoOuArraste()) RegistrarPosicao(_posicionamento);
    }

    /// <summary>Se há outra <see cref="MoverJanela"/> depois de <paramref name="i"/>, sem mostrar/esconder no meio.</summary>
    private static bool MovimentacaoPosterior(List<(Evento Evento, Efeito Efeito, string Motivo)> efeitos, int i)
    {
        for (int j = i + 1; j < efeitos.Count; j++)
        {
            switch (efeitos[j].Efeito)
            {
                case MoverJanela:
                    return true;
                case MostrarJanela or EsconderJanela:
                    return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Poses provisórias da Fase 4: escolhe o quadro pelo retrato do núcleo e só troca o sprite
    /// quando o quadro ou o DPI mudam (ARCHITECTURE.md 2.10: redesenho só quando o quadro muda).
    /// </summary>
    private void AtualizarSprite()
    {
        if (_personagem is null || _nucleo is null || _encerrando || _posicionamento is null) return;
        Retrato retrato = _nucleo.Retrato;
        if (!retrato.Estado.Visivel()) return;
        EstadoDoMovimento movimento = _nucleo.Estado.Movimento;
        // A contagem da pose recomeça a cada troca de estado e a cada quique de borracha, que
        // continua em JUMPING (toon force, DEC-023).
        if (retrato.Estado != _estadoDoQuadro || movimento.Quiques != _quiquesDoQuadro)
        {
            _estadoDoQuadro = retrato.Estado;
            _quiquesDoQuadro = movimento.Quiques;
            _passoDeEntradaNoEstado = _nucleo.Estado.Passos;
        }
        int dpi = _posicionamento.Monitor.Dpi;
        var dinamica = new Dinamica(movimento.VY * 96.0 / dpi, movimento.Quiques, movimento.Foguete);
        QuadroDoSprite quadro = PoseDoPersonagem.Escolher(retrato, _nucleo.Estado.Passos - _passoDeEntradaNoEstado, dinamica);
        if (quadro == _quadroAtual && dpi == _dpiDoSprite && _personagem.Sprite is not null) return;
        int quadrosAntes = SpriteProvisorio.QuadrosEmCache;
        _personagem.DefinirSprite(SpriteProvisorio.Renderizar(quadro, dpi));
        if (SpriteProvisorio.QuadrosEmCache != quadrosAntes)
            Diagnostico.Evento("SPRITE", ("quadrosEmCache", SpriteProvisorio.QuadrosEmCache), ("pose", quadro.Pose), ("expressao", quadro.Expressao ?? "-"), ("deformacao", quadro.Deformacao), ("dpi", dpi));
        _quadroAtual = quadro;
        _dpiDoSprite = dpi;
    }

    /// <summary>
    /// Memória do processo no log de diagnóstico, só quando o movimento para (nunca por timer):
    /// o heap gerenciado, o comprometido pelo GC, o conjunto de trabalho e os quadros em cache.
    /// </summary>
    private static void RegistrarMemoria(string quando)
    {
        if (!Diagnostico.Ligado) return;
        GCMemoryInfo gc = GC.GetGCMemoryInfo();
        const double MB = 1024 * 1024;
        Diagnostico.Evento("MEMORIA",
            ("quando", quando),
            ("heapMB", Math.Round(gc.HeapSizeBytes / MB, 1)),
            ("comprometidaGcMB", Math.Round(gc.TotalCommittedBytes / MB, 1)),
            ("conjuntoMB", Math.Round(Environment.WorkingSet / MB, 1)),
            ("quadrosEmCache", SpriteProvisorio.QuadrosEmCache),
            ("gc0", GC.CollectionCount(0)), ("gc1", GC.CollectionCount(1)), ("gc2", GC.CollectionCount(2)));
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
                if (!_relogioLigado) Diagnostico.Evento("RELOGIO", ("ligado", "sim"), ("evento", evento.GetType().Name));
                IniciarRelogio();
                break;

            case DesligarRelogio:
                if (_relogioLigado)
                {
                    Diagnostico.Evento("RELOGIO", ("ligado", "nao"), ("evento", evento.GetType().Name));
                    RegistrarMemoria("relógio desligado");
                }
                PararRelogio();
                break;

            case AgendarDecisao agendar:
                Diagnostico.Evento("AGENDA", ("atrasoMs", (long)agendar.Atraso.TotalMilliseconds), ("geracao", agendar.Geracao));
                AgendarTemporizadorDeDecisao(agendar);
                break;

            case CancelarDecisao:
                Diagnostico.Evento("AGENDA", ("cancelada", "sim"));
                CancelarDecisaoAutonoma();
                break;

            case LiberarCaptura:
                // O núcleo encerrou o gesto por conta própria (esconder ou sair no meio dele): a
                // arbitragem esquece o gesto e a janela solta o mouse, sem gerar DRAG_CANCEL.
                _arbitro.Reiniciar();
                _latenciasDoArraste.Clear();
                _personagem?.SoltarCaptura();
                break;

            case AbrirMenu pedidoMenu:
                // O laço modal do menu roda depois do processamento, não dentro dele: eventos que
                // chegam com o menu aberto (relógio, agenda, bandeja) são aplicados na hora.
                Adiar(() => ExibirMenuDoDesktop(pedidoMenu.Ponto, "personagem", peloTeclado: false));
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

    /// <summary>
    /// Liga o relógio de passo fixo: inscreve-se nos quadros do compositor do WPF, que só existem
    /// enquanto alguém está inscrito. Sem nada se mexendo, não há inscrição nem quadros (DEC-011).
    /// </summary>
    private void IniciarRelogio()
    {
        if (_relogioLigado) return;
        _relogioLigado = true;
        _tempoAcumulado = TimeSpan.Zero;
        _ultimaMarcacaoRelogio = Stopwatch.GetTimestamp();
        CompositionTarget.Rendering += AoQuadroDoCompositor;
    }

    private void PararRelogio()
    {
        if (!_relogioLigado) return;
        _relogioLigado = false;
        CompositionTarget.Rendering -= AoQuadroDoCompositor;
        _tempoAcumulado = TimeSpan.Zero;
    }

    /// <summary>
    /// Um quadro do compositor: aplica os passos fixos acumulados desde o anterior num lote só, e a
    /// janela vai para a posição do último passo.
    /// </summary>
    private void AoQuadroDoCompositor(object? remetente, EventArgs e)
    {
        if (_nucleo is null || !_relogioLigado || _encerrando) return;
        long agora = Stopwatch.GetTimestamp();
        _tempoAcumulado += Stopwatch.GetElapsedTime(_ultimaMarcacaoRelogio, agora);
        _ultimaMarcacaoRelogio = agora;

        TimeSpan passo = TimeSpan.FromSeconds(1d / _nucleo.Configuracao.PassosPorSegundo);
        // Depois de uma parada da thread da interface (chamada lenta ao Windows, depurador), os
        // passos atrasados não saem todos de uma vez: o relógio lógico perde o excesso, em vez de
        // aplicar uma rajada de movimentos. O passo continua fixo (ARCHITECTURE.md 2.9).
        if (_tempoAcumulado > AtrasoMaximoDoRelogio)
        {
            long descartados = (long)((_tempoAcumulado - AtrasoMaximoDoRelogio) / passo);
            _tempoAcumulado = AtrasoMaximoDoRelogio;
            Diagnostico.Evento("RELOGIO", ("atraso", "limitado"), ("passosDescartados", descartados));
        }
        bool algum = false;
        while (_tempoAcumulado >= passo)
        {
            _tempoAcumulado -= passo;
            algum |= Enfileirar(new Tick(), $"relógio passo {++_passosDoRelogio}");
        }
        if (algum) ProcessarFilaDoNucleo();
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
            Diagnostico.Evento("AGENDA", ("disparo", agendamento.Geracao));
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
        _personagem?.SoltarCaptura();
        _agrupador.Stop();
        _repetirBandeja.Stop();
        PararRelogio();
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

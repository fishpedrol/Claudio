using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Buzzy.App.Apresentacao;
using Buzzy.App.Plataforma;
using Buzzy.Core;

namespace Buzzy.App.Composicao;

/// <summary>
/// Raiz de composição da Fase 1 (DEC-007): liga o núcleo puro (topologia e posicionamento)
/// ao adaptador de plataforma (janelas, bandeja, menu, mensagens do Windows). Não contém
/// regra de posicionamento: ela está em <see cref="Posicionador"/>.
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
    private readonly List<string> _motivosPendentes = [];

    private JanelaDeServico? _servico;
    private JanelaPersonagem? _personagem;
    private Bandeja? _bandeja;
    private Topologia _topologia = null!;
    private PosicaoDoPersonagem _posicao = null!;
    private Posicionamento _posicionamento = null!;
    private int _dpiDoSprite;
    private int _dpiDoIcone;
    private int _releiturasFalhas;
    private int _tentativasDaBandeja;
    private bool _visivel;
    private bool _primeiroQuadroRegistrado;
    private bool _encerrando;

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
    }

    internal void Iniciar()
    {
        // Limitação aceita do System.Windows.Application: o WPF encerra na pergunta de fim de
        // sessão (WM_QUERYENDSESSION) se ninguém cancelar; um desligamento cancelado depois por
        // outro aplicativo deixa o Buzzy fechado (DECISIONS.md).
        _app.SessionEnding += (_, _) => Encerrar("fim de sessão");
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

        _posicionamento = Posicionador.Inicial(topologia, SpriteProvisorio.TamanhoLogico);
        _posicao = Posicionador.Descrever(_posicionamento);

        _servico = new JanelaDeServico();
        _servico.BandejaAcionada += AoAcionarBandeja;
        _servico.BarraDeTarefasRecriada += AoRecriarBarra;
        _servico.TopologiaPodeTerMudado += AoPossivelMudancaDeTopologia;
        Diagnostico.Evento("SERVICO", ("hwnd", _servico.Hwnd));

        _personagem = new JanelaPersonagem();
        _personagem.MenuSolicitado += p => Adiar(() => AbrirMenu(p, "personagem", peloTeclado: false));
        _personagem.DpiMudou += dpi => AoPossivelMudancaDeTopologia($"WM_DPICHANGED {dpi}");
        _personagem.Minimizada += () => Esconder("minimizado pelo Windows");
        _personagem.Closing += (_, e) =>
        {
            if (_encerrando) return;
            e.Cancel = true;
            Adiar(() => Encerrar("fechamento da janela"));
        };
        _personagem.ContentRendered += AoPrimeiroQuadro;

        // Cria a janela sem mostrar, posiciona em pixels físicos e só então mostra: o Buzzy
        // não aparece primeiro num canto qualquer.
        new WindowInteropHelper(_personagem).EnsureHandle();
        AplicarNaJanela(_posicionamento);
        _personagem.Show();
        _visivel = true;
        AplicarNaJanela(_posicionamento);
        Diagnostico.Evento("JANELA", ("hwnd", _personagem.Hwnd));
        Diagnostico.Evento("VISIVEL", ("visivel", "sim"), ("motivo", "início"));

        _dpiDoIcone = topologia.Principal.Dpi;
        _bandeja = new Bandeja(_servico.Hwnd, CriarIcone(_dpiDoIcone));
        AdicionarIconeNaBandeja();

        _instancia.EscutarPedidos(() => Adiar(() =>
        {
            Diagnostico.Evento("INSTANCIA", ("papel", "primeira"), ("pedido", "mostrar"));
            Mostrar("segunda instância");
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
            Adiar(() => Mostrar("bandeja"));
        else
            Adiar(() => AbrirMenu(ponto, "bandeja", peloTeclado: acao == AcaoNaBandeja.MenuPeloTeclado));
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

        // Escondido, a mudança só atualiza a topologia em memória (ARCHITECTURE.md 2.6):
        // a posição é revalidada ao reaparecer, e um monitor que some e volta enquanto o Buzzy
        // está escondido não faz a posição lembrada mudar de monitor.
        if (_visivel) Reacomodar();

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

    private void Reacomodar()
    {
        (Posicionamento resultado, PosicaoDoPersonagem posicao) = Posicionador.Reacomodar(_topologia, _posicao, SpriteProvisorio.TamanhoLogico);
        _posicionamento = resultado;
        _posicao = posicao;
        AplicarNaJanela(resultado);
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

    private void Mostrar(string motivo)
    {
        if (_encerrando || _personagem is null) return;
        if (!_visivel)
        {
            // Escondido, a topologia só foi guardada em memória: revalida antes de reaparecer.
            Topologia? atual = LeitorDeTopologia.Ler(out _);
            if (atual is not null) _topologia = atual;
            (Posicionamento resultado, PosicaoDoPersonagem posicao) = Posicionador.Reacomodar(_topologia, _posicao, SpriteProvisorio.TamanhoLogico);
            _posicionamento = resultado;
            _posicao = posicao;
            AplicarNaJanela(resultado);
            _personagem.Show();
            _visivel = true;
            AplicarNaJanela(resultado);
        }
        _personagem.ReafirmarTopo();
        Diagnostico.Evento("VISIVEL", ("visivel", "sim"), ("motivo", motivo));
    }

    private void Esconder(string motivo)
    {
        if (_encerrando || _personagem is null || !_visivel) return;
        _personagem.Hide();
        _visivel = false;
        Diagnostico.Evento("VISIVEL", ("visivel", "nao"), ("motivo", motivo));
    }

    private void AbrirMenu(PontoPx ponto, string origem, bool peloTeclado)
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
                Esconder("menu");
                break;
            case ComandoDoMenu.AlternarVisibilidade:
                Mostrar("menu");
                break;
            case ComandoDoMenu.Sair:
                Encerrar("menu");
                break;
            case ComandoDoMenu.Nenhum when peloTeclado:
                // Quem abriu o menu da bandeja pelo teclado e o cancelou volta para a área de
                // notificação (documentação do NIM_SETFOCUS). Depois de um cancelamento com o
                // mouse, não: o clique fora pode ter ativado outro aplicativo.
                _bandeja?.DevolverFoco();
                break;
        }
    }

    private void Encerrar(string motivo)
    {
        if (_encerrando) return;
        _encerrando = true;
        Diagnostico.Evento("ENCERRANDO", ("motivo", motivo));

        // Um menu aberto nesta thread (por exemplo, fim de sessão com o menu na tela) é
        // fechado antes de destruir as janelas.
        Win32.EndMenu();
        _agrupador.Stop();
        _repetirBandeja.Stop();
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

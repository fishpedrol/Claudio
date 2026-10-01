using Buzzy.Core.Personagem;

namespace Buzzy.Core.Testes.Movimento;

/// <summary>
/// Faz o papel da raiz de composição num relógio virtual, sem janela: executa os efeitos de
/// tempo do núcleo (relógio de passo fixo, agenda autônoma e temporizador da onda do tamagotchi)
/// e entrega <see cref="Tick"/>, <see cref="AutonomyTimer"/> e <see cref="ItemEffectTimer"/>
/// quando venceriam. Cada passo do relógio virtual dura um passo fixo do núcleo (1/60 s). Usado
/// pelos testes da Fase 4 e do tamagotchi para simular minutos de comportamento autônomo de
/// forma determinística.
/// </summary>
internal sealed class SimuladorDeTempo
{
    private readonly Nucleo _nucleo;
    private readonly double _passoMs;
    private double _agoraMs;
    private bool _relogio;
    private (double VenceEmMs, long Geracao)? _decisao;
    private (double VenceEmMs, long Geracao)? _onda;

    /// <param name="posicaoSalva">A posição salva da carga, como a partida a lê do settings.json (Fase 5); nula, a inicial.</param>
    public SimuladorDeTempo(ConfiguracaoDoNucleo config, ulong semente, Topologia topologia, Preferencias? preferencias = null, PosicaoDoPersonagem? posicaoSalva = null)
        : this(config, semente, new Loaded(topologia, posicaoSalva, preferencias ?? Preferencias.Padrao))
    {
    }

    /// <summary>Um simulador que começa com esta carga (com a borda do esconderijo e a marca de preso gravadas, por exemplo).</summary>
    public SimuladorDeTempo(ConfiguracaoDoNucleo config, ulong semente, Loaded carga)
    {
        ArgumentNullException.ThrowIfNull(carga);
        _nucleo = new Nucleo(config, semente);
        _passoMs = 1000.0 / config.PassosPorSegundo;
        Aplicar(carga);
    }

    private SimuladorDeTempo(Nucleo nucleo, double passoMs, double agoraMs, bool relogio, (double, long)? decisao, (double, long)? onda)
    {
        _nucleo = nucleo;
        _passoMs = passoMs;
        _agoraMs = agoraMs;
        _relogio = relogio;
        _decisao = decisao;
        _onda = onda;
    }

    /// <summary>
    /// Um simulador que continua deste com o núcleo recriado pelo construtor <c>Nucleo(config, estado)</c>, a partir do
    /// estado atual mudado por <paramref name="mudar"/>: é assim que os testes do tamagotchi semeiam uma onda antes de
    /// existir quem a comece (DEC-028). O relógio virtual, o relógio de passo fixo e os temporizadores pendentes
    /// continuam os deste; os callbacks não. Este simulador não muda.
    /// </summary>
    public SimuladorDeTempo Semeado(Func<EstadoDoNucleo, EstadoDoNucleo> mudar)
    {
        ArgumentNullException.ThrowIfNull(mudar);
        return new SimuladorDeTempo(new Nucleo(_nucleo.Configuracao, mudar(_nucleo.Estado)), _passoMs, _agoraMs, _relogio, _decisao, _onda);
    }

    public Nucleo Nucleo => _nucleo;

    public EstadoDoNucleo Estado => _nucleo.Estado;

    public double AgoraMs => _agoraMs;

    /// <summary>Se o relógio de passo fixo está ligado, segundo os efeitos do núcleo.</summary>
    public bool RelogioLigado => _relogio;

    /// <summary>Transições aplicadas desde a criação, na ordem.</summary>
    public List<Transicao> Transicoes { get; } = [];

    /// <summary>Chamado depois de cada evento aplicado, com o estado antes e depois.</summary>
    public Action<EstadoDoNucleo, Evento, EstadoDoNucleo>? AoAplicar { get; set; }

    /// <summary>
    /// Chamado para cada evento que a máquina aplicou, com o estado logo antes dele e o resultado (efeitos e
    /// transições): para conferir os efeitos, como o GravarPosicao.
    /// </summary>
    public Action<EstadoDoNucleo, Evento, Resultado>? AoResultado { get; set; }

    /// <summary>Aplica um evento externo (gesto, comando) e executa os efeitos de tempo.</summary>
    public void Aplicar(Evento evento)
    {
        EstadoDoNucleo antes = _nucleo.Estado;
        if (!_nucleo.Enfileirar(evento)) return;
        EstadoDoNucleo anterior = antes;
        IReadOnlyList<Efeito> efeitos = _nucleo.Processar((aplicado, r) =>
        {
            Transicoes.AddRange(r.Transicoes);
            AoResultado?.Invoke(anterior, aplicado, r);
            anterior = r.Estado;
        });
        foreach (Efeito e in efeitos)
        {
            switch (e)
            {
                case LigarRelogio: _relogio = true; break;
                case DesligarRelogio: _relogio = false; break;
                case AgendarDecisao a: _decisao = (_agoraMs + a.Atraso.TotalMilliseconds, a.Geracao); break;
                case CancelarDecisao: _decisao = null; break;
                case AgendarOnda a: _onda = (_agoraMs + a.Atraso.TotalMilliseconds, a.Geracao); break;
                case CancelarOnda: _onda = null; break;
            }
        }
        AoAplicar?.Invoke(antes, evento, _nucleo.Estado);
    }

    /// <summary>
    /// Avança o relógio virtual: a cada passo, entrega o temporizador vencido (a onda ou a agenda,
    /// o que venceu primeiro; num empate, a onda, que tem a prioridade do relógio, maior que a da
    /// agenda) e, com o relógio ligado, um <see cref="Tick"/>. Parado, pula direto para o próximo
    /// temporizador. Para antes do fim se <paramref name="parar"/> devolver verdadeiro.
    /// </summary>
    public void Avancar(TimeSpan duracao, Func<EstadoDoNucleo, bool>? parar = null)
    {
        double fim = _agoraMs + duracao.TotalMilliseconds;
        while (_agoraMs < fim)
        {
            if (parar?.Invoke(_nucleo.Estado) == true) return;
            bool decisaoVencida = _decisao is { } d && d.VenceEmMs <= _agoraMs;
            if (_onda is { } o && o.VenceEmMs <= _agoraMs && (!decisaoVencida || o.VenceEmMs <= _decisao!.Value.VenceEmMs))
            {
                _onda = null;
                Aplicar(new ItemEffectTimer(o.Geracao));
                continue;
            }
            if (decisaoVencida)
            {
                long geracao = _decisao!.Value.Geracao;
                _decisao = null;
                Aplicar(new AutonomyTimer(geracao));
                continue;
            }
            if (_relogio)
            {
                Aplicar(new Tick());
                _agoraMs += _passoMs;
                continue;
            }
            double proxima = Math.Min(_decisao?.VenceEmMs ?? fim, _onda?.VenceEmMs ?? fim);
            _agoraMs = Math.Min(Math.Max(proxima, _agoraMs), fim);
        }
    }

    /// <summary>Passos do relógio, um a um, enquanto ele estiver ligado (ou até <paramref name="maximo"/>).</summary>
    public int Passos(int maximo)
    {
        int n = 0;
        while (n < maximo && _relogio)
        {
            Aplicar(new Tick());
            _agoraMs += _passoMs;
            n++;
        }
        return n;
    }
}

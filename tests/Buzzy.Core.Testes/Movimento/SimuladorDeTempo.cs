using Buzzy.Core.Personagem;

namespace Buzzy.Core.Testes.Movimento;

/// <summary>
/// Faz o papel da raiz de composição num relógio virtual, sem janela: executa os efeitos de
/// tempo do núcleo (relógio de passo fixo e agenda autônoma) e entrega <see cref="Tick"/> e
/// <see cref="AutonomyTimer"/> quando venceriam. Cada passo do relógio virtual dura um passo
/// fixo do núcleo (1/60 s). Usado pelos testes da Fase 4 para simular minutos de comportamento
/// autônomo de forma determinística.
/// </summary>
internal sealed class SimuladorDeTempo
{
    private readonly Nucleo _nucleo;
    private readonly double _passoMs;
    private double _agoraMs;
    private bool _relogio;
    private (double VenceEmMs, long Geracao)? _decisao;

    public SimuladorDeTempo(ConfiguracaoDoNucleo config, ulong semente, Topologia topologia, Preferencias? preferencias = null)
    {
        _nucleo = new Nucleo(config, semente);
        _passoMs = 1000.0 / config.PassosPorSegundo;
        Aplicar(new Loaded(topologia, null, preferencias ?? Preferencias.Padrao));
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

    /// <summary>Aplica um evento externo (gesto, comando) e executa os efeitos de tempo.</summary>
    public void Aplicar(Evento evento)
    {
        EstadoDoNucleo antes = _nucleo.Estado;
        if (!_nucleo.Enfileirar(evento)) return;
        IReadOnlyList<Efeito> efeitos = _nucleo.Processar((_, r) => Transicoes.AddRange(r.Transicoes));
        foreach (Efeito e in efeitos)
        {
            switch (e)
            {
                case LigarRelogio: _relogio = true; break;
                case DesligarRelogio: _relogio = false; break;
                case AgendarDecisao a: _decisao = (_agoraMs + a.Atraso.TotalMilliseconds, a.Geracao); break;
                case CancelarDecisao: _decisao = null; break;
            }
        }
        AoAplicar?.Invoke(antes, evento, _nucleo.Estado);
    }

    /// <summary>
    /// Avança o relógio virtual: a cada passo, entrega o <see cref="AutonomyTimer"/> vencido e,
    /// com o relógio ligado, um <see cref="Tick"/>. Parado, pula direto para a próxima decisão.
    /// Para antes do fim se <paramref name="parar"/> devolver verdadeiro.
    /// </summary>
    public void Avancar(TimeSpan duracao, Func<EstadoDoNucleo, bool>? parar = null)
    {
        double fim = _agoraMs + duracao.TotalMilliseconds;
        while (_agoraMs < fim)
        {
            if (parar?.Invoke(_nucleo.Estado) == true) return;
            if (_decisao is { } d && d.VenceEmMs <= _agoraMs)
            {
                _decisao = null;
                Aplicar(new AutonomyTimer(d.Geracao));
                continue;
            }
            if (_relogio)
            {
                Aplicar(new Tick());
                _agoraMs += _passoMs;
                continue;
            }
            _agoraMs = _decisao is { } proxima ? Math.Min(Math.Max(proxima.VenceEmMs, _agoraMs), fim) : fim;
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

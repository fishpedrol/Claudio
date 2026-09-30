using System.Globalization;

namespace Buzzy.Core.Personagem;

/// <summary>
/// Tudo o que o núcleo sabe entre dois eventos, imutável. O gerador pseudoaleatório faz parte
/// do estado, então aplicar a mesma sequência de eventos ao mesmo estado inicial dá sempre o
/// mesmo resultado (invariante 7).
/// </summary>
public sealed record EstadoDoNucleo
{
    public Estado Estado { get; init; } = Estado.Booting;

    /// <summary>Por que está escondido; <see cref="MotivoDoOcultamento.Nenhum"/> fora de <see cref="Estado.Hidden"/>.</summary>
    public MotivoDoOcultamento Motivo { get; init; }

    /// <summary>Topologia em cache (ARCHITECTURE.md 2.13.7, item 3). Nula antes de carregar.</summary>
    public Topologia? Topologia { get; init; }

    /// <summary>Onde a janela está (ou vai aparecer). Nulo antes de carregar.</summary>
    public Posicionamento? Lugar { get; init; }

    /// <summary>A mesma posição, relativa à área útil do monitor; sobrevive a mudanças de topologia.</summary>
    public PosicaoDoPersonagem? Posicao { get; init; }

    /// <summary>Deslocamento da pegada: cursor menos âncora no <c>PRESS</c> (invariante 2).</summary>
    public PontoPx Pegada { get; init; }

    public Direcao Direcao { get; init; }

    public Expressao Expressao { get; init; }

    public Gesto Gesto { get; init; }

    /// <summary>Passos que faltam para o gesto curto terminar.</summary>
    public int PassosDoGesto { get; init; }

    /// <summary>Passos que faltam em <see cref="Estado.Reacting"/> ou <see cref="Estado.Landing"/>.</summary>
    public int PassosRestantes { get; init; }

    public bool AutonomiaPausada { get; init; }

    public bool PainelAberto { get; init; }

    public Preferencias Preferencias { get; init; } = Preferencias.Padrao;

    public Aleatorio Aleatorio { get; init; }

    /// <summary>Geração do último agendamento da agenda autônoma.</summary>
    public long Geracao { get; init; }

    /// <summary>Se há um <see cref="AutonomyTimer"/> da geração atual pendente.</summary>
    public bool DecisaoAgendada { get; init; }

    /// <summary>Se o relógio de passo fixo está ligado.</summary>
    public bool RelogioAtivo { get; init; }

    /// <summary>Monitores cobertos por tela cheia, em cache (DEC-013).</summary>
    public MonitoresOcupados Ocupados { get; init; } = MonitoresOcupados.Nenhum;

    /// <summary>Posição anterior à mudança automática por tela cheia, só em memória.</summary>
    public PosicaoDoPersonagem? RetornoDaTelaCheia { get; init; }

    /// <summary>Depois de <c>CMD_SHOW</c> em <c>HIDDEN(POR_TELA_CHEIA)</c>: o modo não age até a próxima mudança.</summary>
    public bool TelaCheiaAdiada { get; init; }

    /// <summary>Relógio lógico: passos fixos já aplicados.</summary>
    public long Passos { get; init; }

    /// <summary>Acontecimento pontual do último evento, para a apresentação.</summary>
    public Sinal Sinal { get; init; }

    /// <summary>Estado inicial, em <see cref="Estado.Booting"/>, com a semente dada.</summary>
    public static EstadoDoNucleo Inicial(ulong semente) => new() { Aleatorio = new Aleatorio(semente) };

    /// <summary>O que a apresentação e os testes enxergam (ARCHITECTURE.md 2.10).</summary>
    public Retrato Retrato() => new(
        Estado,
        Motivo,
        Lugar?.Ancora ?? default,
        Lugar?.Monitor.Chave ?? "",
        Lugar?.Tamanho ?? default,
        Direcao,
        Expressao,
        Gesto,
        AutonomiaPausada,
        PainelAberto,
        Preferencias.Energia,
        RelogioAtivo,
        Sinal);
}

/// <summary>
/// Retrato do estado (ARCHITECTURE.md 2.2 e 2.10): estado de comportamento, posição, direção,
/// expressão, gesto, dimensões ortogonais e sinal pontual. É o que a apresentação desenha e o
/// que as reproduções gravadas comparam.
/// </summary>
public sealed record Retrato(
    Estado Estado,
    MotivoDoOcultamento Motivo,
    PontoPx Ancora,
    string ChaveMonitor,
    TamanhoPx Tamanho,
    Direcao Direcao,
    Expressao Expressao,
    Gesto Gesto,
    bool AutonomiaPausada,
    bool PainelAberto,
    NivelDeEnergia Energia,
    bool RelogioAtivo,
    Sinal Sinal)
{
    /// <summary>Linha canônica, na cultura invariante, usada nas reproduções gravadas.</summary>
    public string Descrever()
    {
        string estado = Estado == Estado.Hidden ? $"Hidden({Motivo})" : Estado.ToString();
        return string.Create(CultureInfo.InvariantCulture,
            $"{estado} ancora=({Ancora.X},{Ancora.Y}) monitor={ChaveMonitor} tamanho={Tamanho.Largura}x{Tamanho.Altura} direcao={Direcao} expressao={Expressao} gesto={Gesto} pausada={SimNao(AutonomiaPausada)} painel={SimNao(PainelAberto)} energia={Energia} relogio={SimNao(RelogioAtivo)} sinal={Sinal}");
    }

    private static string SimNao(bool valor) => valor ? "sim" : "nao";

    public override string ToString() => Descrever();
}

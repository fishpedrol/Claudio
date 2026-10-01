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

    /// <summary>
    /// Se a carga (<see cref="Loaded"/>) já aconteceu. Só a primeira carga vale; pedidos anteriores
    /// a ela (mostrar, esconder, sessão, topologia) ficam guardados até ela chegar.
    /// </summary>
    public bool Carregado { get; init; }

    /// <summary>Por que está escondido; <see cref="MotivoDoOcultamento.Nenhum"/> fora de <see cref="Estado.Hidden"/>.</summary>
    public MotivoDoOcultamento Motivo { get; init; }

    /// <summary>Topologia em cache (ARCHITECTURE.md 2.13.7, item 3). Nula antes de carregar.</summary>
    public Topologia? Topologia { get; init; }

    /// <summary>Onde a janela está (ou vai aparecer). Nulo antes de carregar.</summary>
    public Posicionamento? Lugar { get; init; }

    /// <summary>
    /// Posição fina, velocidade e plano do movimento em curso (Fase 4, DEC-022). Vale nos estados
    /// de movimento; ao entrar num deles, parte da âncora de <see cref="Lugar"/>.
    /// </summary>
    public EstadoDoMovimento Movimento { get; init; } = EstadoDoMovimento.Nenhum;

    /// <summary>
    /// A mesma posição, relativa à área útil do monitor; sobrevive a mudanças de topologia. Acompanha toda mudança, em qualquer
    /// estado, sem mover a janela (DEC-030, <see cref="Posicionador.Rebasear"/>): escondido, ele continua ligado à chave do monitor.
    /// </summary>
    public PosicaoDoPersonagem? Posicao { get; init; }

    /// <summary>Deslocamento da pegada: cursor menos âncora no <c>PRESS</c> (invariante 2).</summary>
    public PontoPx Pegada { get; init; }

    public Direcao Direcao { get; init; }

    public Expressao Expressao { get; init; }

    public Gesto Gesto { get; init; }

    /// <summary>Passos que faltam para o gesto curto terminar.</summary>
    public int PassosDoGesto { get; init; }

    /// <summary>Passos que faltam em <see cref="Estado.Reacting"/>, <see cref="Estado.Landing"/> ou <see cref="Estado.Using"/>.</summary>
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

    /// <summary>
    /// Posição anterior à mudança automática por tela cheia, só em memória. "O modo não age de
    /// novo até a próxima mudança" sai da própria regra "uma vez por mudança" de
    /// <see cref="FullscreenTargetsChanged"/>, sem campo à parte. Acompanha as mudanças de topologia como a posição.
    /// </summary>
    public PosicaoDoPersonagem? RetornoDaTelaCheia { get; init; }

    /// <summary>
    /// Se os monitores ocupados pela tela cheia mudaram durante o gesto em curso (PRESSED ou
    /// DRAGGING): ao soltar, o ponto do usuário vale e o retorno temporário é descartado.
    /// </summary>
    public bool TelaCheiaMudouNoGesto { get; init; }

    /// <summary>
    /// Colocado pelo usuário numa lateral ou no cipó da borda de cima (DEC-024): lá fica até o
    /// usuário tirá-lo. A agenda só o faz passear pela mesma superfície; nunca salta, se solta nem
    /// desce ao chão por conta própria. Um clique não o tira; um arraste para outro lugar, sim.
    /// </summary>
    public bool PresoPeloUsuario { get; init; }

    /// <summary>
    /// Em que borda o personagem está escondido (DEC-025), ou <see cref="LadoDoEsconderijo.Nenhum"/>.
    /// Sobrevive a PRESSED e REACTING (o primeiro clique do clique duplo), a HIDDEN e às
    /// revalidações: toda acomodação de quem está escondido o devolve ao esconderijo.
    /// </summary>
    public LadoDoEsconderijo Esconderijo { get; init; }

    /// <summary>
    /// A onda de desenho animado do último item usado (DEC-028), ou a paranoia, na frente: tipo, fase e nível. Nula sem
    /// onda. Só em memória, nunca gravada (SECURITY.md 5); com o tamagotchi desligado, não vale.
    /// </summary>
    public EstadoDaOnda? Onda { get; init; }

    /// <summary>
    /// A onda de fundo (DEC-028; desenho do núcleo, 4.5): a que estava na frente quando chegou uma de precedência maior
    /// ou igual. Fica congelada, sem temporizador nem efeito no comportamento, e volta à frente, com a fase recomeçada,
    /// quando a da frente acaba. Só cabem duas: uma terceira descarta a de fundo anterior.
    /// </summary>
    public EstadoDaOnda? OndaDeFundo { get; init; }

    /// <summary>
    /// A carga da paranoia (pedido do usuário de 2026-10-01; DEC-028), de 0 para cima: quantos itens de substância ele usou
    /// no episódio. Cada um soma 1, depois da combinação; da 4ª em diante, a paranoia (a onda <c>Paranoico</c>) começa ou
    /// sobe. Volta a 0 no fim de todo evento em que nem a onda da frente nem a de fundo é de substância (a paranoia conta
    /// como substância). Só em memória, como a onda.
    /// </summary>
    public int Carga { get; init; }

    /// <summary>Geração do último agendamento do temporizador da onda.</summary>
    public long GeracaoDaOnda { get; init; }

    /// <summary>Se há um <see cref="ItemEffectTimer"/> da geração atual pendente.</summary>
    public bool OndaAgendada { get; init; }

    /// <summary>O uso em curso, em <see cref="Estado.Using"/>; nulo fora dele.</summary>
    public Uso? Uso { get; init; }

    /// <summary>
    /// Os itens na tela (DEC-028): no máximo <see cref="ConfiguracaoDoNucleo.MaximoDeItens"/>, só em memória, nunca
    /// gravados (SECURITY.md 5). Somem ao sair do aplicativo.
    /// </summary>
    public ItensNoMundo Itens { get; init; } = ItensNoMundo.Nenhum;

    /// <summary>O Id do próximo item invocado: cada Id é usado uma vez só.</summary>
    public int ProximoIdDeItem { get; init; } = 1;

    /// <summary>
    /// Se o usuário segura um item (DEC-028): o personagem fica atento, parado onde está, sem decisão autônoma, até o
    /// item sair da mão. Derivado dos itens; não é guardado.
    /// </summary>
    public bool Atento => Itens.NaMao is not null;

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
        Sinal)
    {
        EmocaoDominante = Preferencias.EmocaoDominante,
        Onda = Onda,
        OndaDeFundo = OndaDeFundo,
        Carga = Carga,
        Uso = Uso,
        PassoDoUso = Uso is { } uso ? uso.Passos - PassosRestantes : 0,
        Itens = Itens,
    };
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
    /// <summary>A emoção dominante escolhida (DEC-027), para a marca no menu; nula, "Automática".</summary>
    public Expressao? EmocaoDominante { get; init; }

    /// <summary>A onda do item em curso (DEC-028): tipo, fase e nível, para as sobreposições da apresentação; nula sem onda.</summary>
    public EstadoDaOnda? Onda { get; init; }

    /// <summary>A onda de fundo, congelada atrás da da frente (DEC-028); nula sem ela.</summary>
    public EstadoDaOnda? OndaDeFundo { get; init; }

    /// <summary>A carga da paranoia (pedido do usuário de 2026-10-01): quantos itens de substância ele usou no episódio; 0 sem episódio.</summary>
    public int Carga { get; init; }

    /// <summary>O uso em curso, em USING (DEC-028): o item, o verbo, a duração e o apoio, para a pose de uso; nulo fora dele.</summary>
    public Uso? Uso { get; init; }

    /// <summary>O passo do uso em curso, de 0 à duração menos 1, para o quadro da animação; 0 fora de USING.</summary>
    public int PassoDoUso { get; init; }

    /// <summary>Os itens na tela (DEC-028), inclusive o da mão do usuário.</summary>
    public ItensNoMundo Itens { get; init; } = ItensNoMundo.Nenhum;

    /// <summary>
    /// Linha canônica, na cultura invariante, usada nas reproduções gravadas. A onda de um item (<c>onda=Tipo/Fase/Nível</c>),
    /// a de fundo (<c>fundo=</c>), a carga da paranoia (<c>carga=</c>, só acima de 0), o uso
    /// (<c>uso=Item/Verbo/PassodeDuração/Apoio</c>), a emoção dominante (<c>emocao=</c>) e os itens
    /// (<c>itens=[Id:Item:Situação:(x,y);…]</c>) só aparecem quando há: sem eles, a linha é a de antes (referências
    /// gravadas 01 a 05).
    /// </summary>
    public string Descrever()
    {
        string estado = Estado == Estado.Hidden ? $"Hidden({Motivo})" : Estado.ToString();
        string onda = Onda is { } o ? $" onda={o.Tipo}/{o.Fase}/{o.Nivel}" : "";
        string fundo = OndaDeFundo is { } f ? $" fundo={f.Tipo}/{f.Fase}/{f.Nivel}" : "";
        string carga = Carga > 0 ? string.Create(CultureInfo.InvariantCulture, $" carga={Carga}") : "";
        string uso = Uso is { } u ? string.Create(CultureInfo.InvariantCulture, $" uso={u.Item}/{u.Verbo}/{PassoDoUso}de{u.Passos}/{u.Apoio}") : "";
        string emocao = EmocaoDominante is { } e ? $" emocao={e}" : "";
        string itens = Itens.Quantidade > 0 ? $" itens=[{Itens}]" : "";
        return string.Create(CultureInfo.InvariantCulture,
            $"{estado} ancora=({Ancora.X},{Ancora.Y}) monitor={ChaveMonitor} tamanho={Tamanho.Largura}x{Tamanho.Altura} direcao={Direcao} expressao={Expressao} gesto={Gesto} pausada={SimNao(AutonomiaPausada)} painel={SimNao(PainelAberto)} energia={Energia} relogio={SimNao(RelogioAtivo)} sinal={Sinal}{onda}{fundo}{carga}{uso}{emocao}{itens}");
    }

    private static string SimNao(bool valor) => valor ? "sim" : "nao";

    public override string ToString() => Descrever();
}

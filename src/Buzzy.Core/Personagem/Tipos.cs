namespace Buzzy.Core.Personagem;

/// <summary>
/// Estado de comportamento (ARCHITECTURE.md 2.6). Os nomes são os da especificação, para que a
/// tabela de transições e os testes possam ser conferidos linha a linha.
/// </summary>
public enum Estado
{
    /// <summary>Carrega configurações e topologia e restaura a posição.</summary>
    Booting,

    /// <summary>Parado sobre uma superfície.</summary>
    Idle,

    /// <summary>Andando sobre o chão.</summary>
    Walking,

    /// <summary>Subindo, descendo ou parado numa parede.</summary>
    Climbing,

    /// <summary>Sustentado pela borda superior.</summary>
    Hanging,

    /// <summary>Em trajetória balística iniciada por decisão autônoma.</summary>
    Jumping,

    /// <summary>Sem apoio, sob gravidade.</summary>
    Falling,

    /// <summary>Transição curta depois de tocar o chão.</summary>
    Landing,

    /// <summary>Descansando ou dormindo, sem animação contínua; o relógio fica parado.</summary>
    Resting,

    /// <summary>Botão pressionado sobre o personagem; ainda não se sabe se é clique ou arraste.</summary>
    Pressed,

    /// <summary>Personagem preso ao cursor.</summary>
    Dragging,

    /// <summary>Validação logo depois de soltar, de mostrar ou de uma mudança de topologia.</summary>
    Settling,

    /// <summary>Reação curta a um clique.</summary>
    Reacting,

    /// <summary>Escondido, sem relógio e sem desenho; o motivo fica em <see cref="MotivoDoOcultamento"/>.</summary>
    Hidden,

    /// <summary>Grava o estado e encerra.</summary>
    Exiting,

    /// <summary>
    /// Escondido atrás da borda de baixo (a barra de tarefas) ou de uma lateral, só com a cabeça e
    /// as mãos para fora (DEC-025). Entra e sai pelo clique duplo; sem relógio; a agenda só troca a cara.
    /// </summary>
    Peeking,
}

/// <summary>Em que borda o personagem está escondido (DEC-025).</summary>
public enum LadoDoEsconderijo
{
    Nenhum,

    /// <summary>Atrás da borda de baixo da área útil: a barra de tarefas, com a barra embaixo.</summary>
    Baixo,

    /// <summary>Atrás da lateral esquerda da área útil.</summary>
    Esquerda,

    /// <summary>Atrás da lateral direita da área útil.</summary>
    Direita,
}

/// <summary>Grupo do estado na tabela de ARCHITECTURE.md 2.6.</summary>
public enum GrupoDoEstado
{
    Sistema,
    Autonomo,
    Fisico,
    Usuario,
}

/// <summary>Por que o personagem está escondido. Decide quais eventos podem mostrá-lo de novo.</summary>
public enum MotivoDoOcultamento
{
    Nenhum,
    PorUsuario,
    PorSessao,
    PorSuspensao,
    PorTelaCheia,
}

/// <summary>Nível de energia (DEC-014): muda frequência e duração das ações autônomas, nunca a física.</summary>
public enum NivelDeEnergia
{
    Baixa,
    Media,
    Alta,
}

/// <summary>
/// Expressão do rosto, dimensão independente do estado (invariante 6). Uma por expressão de
/// docs/IDENTIDADE_VISUAL.md, seção 6; o manifesto da Fase 6 precisa cobrir todas.
/// </summary>
public enum Expressao
{
    Neutro,
    Feliz,
    Rindo,
    Curioso,
    Surpreso,
    Assustado,
    Sonolento,
    Bocejando,
    Dormindo,
    Travesso,
    Entediado,
    Pensativo,
    Empolgado,
    Determinado,
}

/// <summary>
/// Gesto curto (ARCHITECTURE.md 2.6): ação visual de duração limitada na superfície atual,
/// que não muda estado, posição nem superfície (invariante 15).
/// </summary>
public enum Gesto
{
    Nenhum,
    Espiar,
    OlharAoRedor,
    Cocar,
    Espreguicar,
    Brincar,
}

/// <summary>Para onde o personagem está virado. As poses de perfil são desenhadas para a direita.</summary>
public enum Direcao
{
    Direita,
    Esquerda,
}

/// <summary>Acontecimento pontual que a apresentação pode mostrar (ARCHITECTURE.md 2.10).</summary>
public enum Sinal
{
    Nenhum,
    FoiClicado,
    FoiClicadoDuasVezes,
    Pousou,
    Acordou,
}

/// <summary>
/// Sinais que o módulo de movimento emite quando o personagem encontra uma superfície
/// (ARCHITECTURE.md 2.5 e 2.9). Na Fase 2 ainda não há movimento: os testes injetam esses
/// sinais para exercitar a tabela de transições; a partir da Fase 4 eles saem do passo físico.
/// </summary>
public enum SinalDeMovimento
{
    /// <summary>Andando, chegou a uma parede.</summary>
    Parede,

    /// <summary>Andando, chegou a uma passagem para o monitor vizinho.</summary>
    Passagem,

    /// <summary>Andando, o chão acabou sem parede.</summary>
    FimDoChao,

    /// <summary>Escalando, chegou ao topo da área útil.</summary>
    TopoDaParede,

    /// <summary>Escalando para baixo, a parede acabou no chão.</summary>
    FimDaParede,

    /// <summary>Escalando, alcançou uma borda superior onde pode se pendurar.</summary>
    BordaSuperior,

    /// <summary>Pendurado, chegou a uma passagem compatível ou ao fim da borda.</summary>
    FimDaBorda,

    /// <summary>Pulando ou caindo, tocou o chão.</summary>
    ContatoComOChao,
}

/// <summary>Ações que a agenda autônoma pode escolher em <see cref="Estado.Idle"/>.</summary>
[Flags]
public enum AcoesAutonomas
{
    Nenhuma = 0,
    Andar = 1,
    Escalar = 2,
    Pular = 4,
    Descansar = 8,
    Gesto = 16,
    TrocarExpressao = 32,
    Todas = Andar | Escalar | Pular | Descansar | Gesto | TrocarExpressao,
}

/// <summary>Consultas sobre os estados.</summary>
public static class Estados
{
    public static GrupoDoEstado Grupo(this Estado estado) => estado switch
    {
        Estado.Booting or Estado.Hidden or Estado.Exiting => GrupoDoEstado.Sistema,
        Estado.Idle or Estado.Walking or Estado.Climbing or Estado.Hanging or Estado.Jumping or Estado.Resting or Estado.Peeking => GrupoDoEstado.Autonomo,
        Estado.Falling or Estado.Landing => GrupoDoEstado.Fisico,
        Estado.Pressed or Estado.Dragging or Estado.Settling or Estado.Reacting => GrupoDoEstado.Usuario,
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, "Estado desconhecido."),
    };

    /// <summary>Se a janela do personagem aparece na tela neste estado.</summary>
    public static bool Visivel(this Estado estado) => estado is not (Estado.Booting or Estado.Hidden or Estado.Exiting);

    /// <summary>Estados do grupo usuário: eventos autônomos que chegam neles são descartados (ARCHITECTURE.md 2.3).</summary>
    public static bool ControladoPeloUsuario(this Estado estado) => estado.Grupo() == GrupoDoEstado.Usuario;

    /// <summary>Estados em que o personagem se move pelo passo físico e o relógio precisa correr.</summary>
    public static bool EmMovimento(this Estado estado)
        => estado is Estado.Walking or Estado.Climbing or Estado.Hanging or Estado.Jumping or Estado.Falling or Estado.Landing;

    /// <summary>Estados que aceitam <c>PRESS</c>: autônomos, físicos e <see cref="Estado.Reacting"/> (DEC-004).</summary>
    public static bool AceitaPressionar(this Estado estado)
        => estado.Grupo() is GrupoDoEstado.Autonomo or GrupoDoEstado.Fisico || estado == Estado.Reacting;
}

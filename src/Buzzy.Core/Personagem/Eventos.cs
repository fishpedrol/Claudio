namespace Buzzy.Core.Personagem;

/// <summary>
/// De onde vem um evento, da maior para a menor prioridade (ARCHITECTURE.md 2.3): ação direta
/// do usuário sobre o personagem; menu, bandeja e painel de energia; sistema; relógio e
/// movimento; comportamento autônomo; troca de expressão.
/// </summary>
public enum Origem
{
    Expressao = 0,
    Autonomo = 1,
    Relogio = 2,
    Sistema = 3,
    ComandoDoUsuario = 4,
    AcaoDireta = 5,
}

/// <summary>
/// Evento normalizado que o núcleo consome (ARCHITECTURE.md 2.6, tabela de eventos). Os nomes
/// são os da especificação. Coordenadas em pixels físicos do desktop virtual (DEC-008).
/// </summary>
public abstract record Evento
{
    public abstract Origem Origem { get; }
}

// Gestos derivados pela arbitragem de input (ARCHITECTURE.md 2.7). Na Fase 2 os testes os
// entregam prontos; o reconhecedor de gestos é da Fase 3.

/// <summary><c>PRESS</c>: botão esquerdo pressionado sobre pixel opaco, com o cursor em <paramref name="Cursor"/>.</summary>
public sealed record Press(PontoPx Cursor) : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

/// <summary><c>CLICK</c>: soltou dentro do retângulo de arraste.</summary>
public sealed record Click : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

/// <summary><c>DOUBLE_CLICK</c>: segundo clique dentro do tempo e do retângulo do sistema.</summary>
public sealed record DoubleClick : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

/// <summary><c>DRAG_START</c>: o cursor saiu do retângulo de arraste.</summary>
public sealed record DragStart : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

/// <summary><c>DRAG_MOVE</c>: posição mais recente do cursor durante o arraste.</summary>
public sealed record DragMove(PontoPx Cursor) : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

/// <summary><c>DRAG_END</c>: soltou o botão depois de arrastar.</summary>
public sealed record DragEnd(PontoPx Cursor) : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

/// <summary><c>DRAG_CANCEL</c>: a captura foi perdida (Alt+Tab, UAC, outra captura).</summary>
public sealed record DragCancel : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

/// <summary><c>CONTEXT_MENU</c>: botão direito solto sobre o personagem.</summary>
public sealed record ContextMenu(PontoPx Cursor) : Evento
{
    public override Origem Origem => Origem.AcaoDireta;
}

// Painel de energia (Fase 8).

/// <summary><c>ENERGY_PANEL_OPEN</c>: pedido explícito de abrir o painel (menu "Energia").</summary>
public sealed record EnergyPanelOpen : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>ENERGY_SELECTED</c>: o usuário escolheu um nível no painel aberto.</summary>
public sealed record EnergySelected(NivelDeEnergia Nivel) : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>ENERGY_PANEL_CLOSE</c>: o painel fechou (Esc, botão de fechar ou perda de foco).</summary>
public sealed record EnergyPanelClose : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

// Bandeja e menu.

/// <summary><c>CMD_HIDE</c>.</summary>
public sealed record CmdHide : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>CMD_SHOW</c>.</summary>
public sealed record CmdShow : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>CMD_PAUSE_AUTONOMY</c>.</summary>
public sealed record CmdPauseAutonomy : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>CMD_RESUME_AUTONOMY</c>.</summary>
public sealed record CmdResumeAutonomy : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>CMD_OPEN_SETTINGS</c> (Fase 8).</summary>
public sealed record CmdOpenSettings : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>CMD_RESET_POSITION</c>: volta à posição inicial.</summary>
public sealed record CmdResetPosition : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

/// <summary><c>CMD_EXIT</c>.</summary>
public sealed record CmdExit : Evento
{
    public override Origem Origem => Origem.ComandoDoUsuario;
}

// Sistema.

/// <summary>
/// Configurações e topologia carregadas (primeira linha da tabela de transições). A posição
/// salva é nula até a persistência da Fase 5.
/// </summary>
public sealed record Loaded(Topologia Topologia, PosicaoDoPersonagem? PosicaoSalva, Preferencias Preferencias) : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary><c>TOPOLOGY_CHANGED</c>: nova leitura dos monitores, já agrupada pelo adaptador.</summary>
public sealed record TopologyChanged(Topologia Topologia) : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary><c>SESSION_LOCKED</c>.</summary>
public sealed record SessionLocked : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary><c>SESSION_UNLOCKED</c>.</summary>
public sealed record SessionUnlocked : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary><c>SUSPENDING</c>.</summary>
public sealed record Suspending : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary><c>RESUMED</c>.</summary>
public sealed record Resumed : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary><c>SESSION_ENDING</c>.</summary>
public sealed record SessionEnding : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary>
/// <c>FULLSCREEN_TARGETS_CHANGED</c> (DEC-013): só as chaves dos monitores cobertos pela janela
/// ativa em tela cheia, sem identidade nem conteúdo de outra janela (invariante 13).
/// </summary>
public sealed record FullscreenTargetsChanged(MonitoresOcupados Ocupados) : Evento
{
    public override Origem Origem => Origem.Sistema;
}

/// <summary><c>SETTINGS_CHANGED</c>.</summary>
public sealed record SettingsChanged(Preferencias Preferencias) : Evento
{
    public override Origem Origem => Origem.Sistema;
}

// Relógio e movimento.

/// <summary><c>TICK</c>: um passo fixo do relógio lógico (1/60 s por padrão).</summary>
public sealed record Tick : Evento
{
    public override Origem Origem => Origem.Relogio;
}

/// <summary>
/// Sinal do módulo de movimento (parede, passagem, contato com o chão). A partir da Fase 4 sai
/// do próprio passo físico; na Fase 2 só os testes o injetam.
/// </summary>
public sealed record MovementSignal(SinalDeMovimento Sinal) : Evento
{
    public override Origem Origem => Origem.Relogio;
}

/// <summary>
/// <c>AUTONOMY_TIMER</c>: disparo do temporizador único da agenda autônoma. A geração evita que
/// um disparo antigo, que chegou depois de cancelado, seja tomado pelo atual.
/// </summary>
public sealed record AutonomyTimer(long Geracao) : Evento
{
    public override Origem Origem => Origem.Autonomo;
}

/// <summary>
/// Troca de expressão pedida pela personalidade. Vale em qualquer estado e nunca muda estado de
/// comportamento nem posição (invariante 6).
/// </summary>
public sealed record ExpressionChange(Expressao Expressao) : Evento
{
    public override Origem Origem => Origem.Expressao;
}

/// <summary>Conjunto imutável, ordenado e sem repetição de chaves de monitor, com igualdade por valor.</summary>
public sealed class MonitoresOcupados : IEquatable<MonitoresOcupados>
{
    public static readonly MonitoresOcupados Nenhum = new([]);

    private readonly string[] _chaves;

    public MonitoresOcupados(IEnumerable<string> chaves)
    {
        ArgumentNullException.ThrowIfNull(chaves);
        _chaves = [.. chaves.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        if (_chaves.Any(string.IsNullOrEmpty))
            throw new ArgumentException("Chave de monitor vazia.", nameof(chaves));
    }

    public IReadOnlyList<string> Chaves => _chaves;

    public bool Vazio => _chaves.Length == 0;

    public bool Contem(string chave) => Array.BinarySearch(_chaves, chave, StringComparer.Ordinal) >= 0;

    public bool Equals(MonitoresOcupados? outro) => outro is not null && _chaves.AsSpan().SequenceEqual(outro._chaves);

    public override bool Equals(object? obj) => Equals(obj as MonitoresOcupados);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (string chave in _chaves) hash.Add(chave, StringComparer.Ordinal);
        return hash.ToHashCode();
    }

    public override string ToString() => string.Join(",", _chaves);
}

/// <summary>Preferências que o núcleo usa. A Fase 8 acrescenta as demais e a persistência.</summary>
/// <param name="Energia">Nível de energia; padrão Média (DEC-014).</param>
/// <param name="ModoTelaCheia">Modo automático de tela cheia (Q-09); padrão ligado.</param>
public sealed record Preferencias(NivelDeEnergia Energia, bool ModoTelaCheia)
{
    public static readonly Preferencias Padrao = new(NivelDeEnergia.Media, true);
}

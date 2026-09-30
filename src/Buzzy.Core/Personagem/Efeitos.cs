namespace Buzzy.Core.Personagem;

/// <summary>
/// Pedido do núcleo à raiz de composição (ARCHITECTURE.md 2.3, passo 4). O núcleo nunca move
/// janela, grava arquivo nem agenda timer diretamente; ele devolve efeitos, na ordem em que
/// devem ser executados.
/// </summary>
public abstract record Efeito;

/// <summary>Leva a janela ao posicionamento dado (pixels físicos).</summary>
public sealed record MoverJanela(Posicionamento Destino) : Efeito;

/// <summary>Mostra a janela do personagem sem ativá-la.</summary>
public sealed record MostrarJanela : Efeito;

/// <summary>Esconde a janela do personagem.</summary>
public sealed record EsconderJanela : Efeito;

/// <summary>Liga o relógio de passo fixo: a raiz passa a entregar <see cref="Tick"/>.</summary>
public sealed record LigarRelogio : Efeito;

/// <summary>Desliga o relógio de passo fixo.</summary>
public sealed record DesligarRelogio : Efeito;

/// <summary>
/// Agenda o temporizador único da agenda autônoma. Um agendamento novo substitui o anterior;
/// quando disparar, a raiz entrega <see cref="AutonomyTimer"/> com a mesma geração.
/// </summary>
public sealed record AgendarDecisao(TimeSpan Atraso, long Geracao) : Efeito;

/// <summary>Cancela o temporizador da agenda autônoma.</summary>
public sealed record CancelarDecisao : Efeito;

/// <summary>Solta a captura do mouse de um gesto em curso (esconder ou sair no meio do arraste).</summary>
public sealed record LiberarCaptura : Efeito;

/// <summary>Abre o menu do Buzzy no ponto dado.</summary>
public sealed record AbrirMenu(PontoPx Ponto) : Efeito;

/// <summary>Abre o painel compacto de energia (Fase 8).</summary>
public sealed record AbrirPainelDeEnergia : Efeito;

/// <summary>Fecha o painel de energia.</summary>
public sealed record FecharPainelDeEnergia : Efeito;

/// <summary>Abre a janela de configurações (Fase 8).</summary>
public sealed record AbrirConfiguracoes : Efeito;

/// <summary>
/// Grava a posição escolhida pelo usuário no settings.json (Fase 5), com a tela do monitor da época
/// (<see cref="PosicaoDoPersonagem.TelaDoMonitor"/>), que a partida seguinte usa na restauração. Só sai de
/// evento do usuário ou do sistema, nunca do relógio nem da agenda (invariante 18). Se a raiz grava com
/// atraso ou na hora, diz <see cref="Persistencia.PoliticaDeGravacao"/>.
/// </summary>
public sealed record GravarPosicao(PosicaoDoPersonagem Posicao) : Efeito;

/// <summary>
/// Grava as preferências no settings.json (Fase 5; quem as muda é o painel da Fase 8), pela mesma política
/// de <see cref="GravarPosicao"/>.
/// </summary>
public sealed record GravarPreferencias(Preferencias Preferencias) : Efeito;

/// <summary>Encerra o aplicativo.</summary>
public sealed record Encerrar : Efeito;

/// <summary>Uma troca de estado de comportamento, com a regra da tabela que a produziu.</summary>
public sealed record Transicao(Estado De, Estado Para, string Regra)
{
    public override string ToString() => $"{De} -> {Para} ({Regra})";
}

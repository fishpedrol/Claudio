> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de o desenho dos eventos do sistema no app (sessão, energia, fim de sessão, minimização e barra de tarefas), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a Fase 5. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-029 a DEC-031) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

# Fase 5: desenho dos eventos do sistema no app

Esta área cobre sessão (bloqueio, desbloqueio e conexão), energia (suspensão e retomada), fim de sessão com gravação da posição, a minimização da janela do personagem pelo Windows quando um monitor é desconectado e a barra de tarefas movida ou oculta.

O app, os testes e os documentos foram lidos na árvore principal. O núcleo com física (`Maquina.cs`, `Movimento.cs`, `Configuracao.cs` e `EstadoDoNucleo.cs`) foi lido da cópia `f4`. Também consultei a documentação da Microsoft (WTSRegisterSessionNotification, WM_WTSSESSION_CHANGE, WM_POWERBROADCAST, PBT_*, Desktop Activity Moderator e RegisterSuspendResumeNotification) e o `Window.cs` do dotnet/wpf. Não editei, compilei nem executei nada.

## 0. O que existe hoje (conferido no código)

- **O núcleo já tem todas as regras.** Estas funções estão no `Maquina.cs` de `f4`:
  - `SessionLocked` e `Suspending` chamam `Esconder(motivo)`. A precedência de `Precedencia()` é usuário 4, sessão 3, suspensão 2 e tela cheia 1. Além de esconder, chamam `LiberarGestoDoUsuario`, `FixarArrasteInterrompido` e `GravarPosicaoDoUsuario`.
  - `SessionUnlocked` e `Resumed` chamam `Reaparecer(motivoQueSeDesfaz)`. Ela só age em `Hidden` com o mesmo motivo e revalida com `Posicionador.Reacomodar` **contra a topologia em cache**.
  - `SessionEnding` chama `Sair`, que emite `GravarPosicao` imediatamente antes de `Encerrar`. Isso já é testado em `Qualquer_CmdExitOuSessionEnding_GravaAPosicaoLogoAntesDeEncerrar`.
  - Em `Hidden`, `MudarTopologia` só atualiza o cache.
  - Um `Resumed` ou `SessionUnlocked` repetido depois de revelar não tem efeito.
- **O app não envia nenhum evento de sessão ou energia.** A busca em `src/Buzzy.App` encontra só `_app.SessionEnding += … Enviar(new SessionEnding(), "fim de sessão")`, em `Aplicacao.Iniciar`. O `Win32.cs` não declara WTS nem `WM_POWERBROADCAST`.
- **Barra de tarefas.** `JanelaDeServico.Gancho` trata `WM_DISPLAYCHANGE`, `WM_SETTINGCHANGE` com `SPI_SETWORKAREA`, a bandeja e `TaskbarCreated`. `LeitorDeTopologia` lê `rcWork`. Por isso, mover a barra ou ligar a ocultação automática já vira `TopologyChanged` pelo agrupador de 300 ms (`AoPossivelMudancaDeTopologia` → `AoAgrupar`).
- **Lacuna.** `AoRecriarBarra` relê a topologia mas só troca o `_topologia` do app. O núcleo não recebe `TopologyChanged`.
- **Minimização.** `JanelaPersonagem.AoMudarEstado` faz `WindowState = Normal` na hora e dispara `Minimizada`, e `Aplicacao` adia um `CmdHide("minimizado pelo Windows")`. Toda minimização vira `HIDDEN(POR_USUARIO)`.
  - O Windows 11 vem com "Minimizar janelas quando um monitor for desconectado" ligado (valor `MonitorRemovalRecalcBehavior`). Com isso, desplugar o monitor do Buzzy o mandaria para a bandeja. Isso viola S8 ("o personagem nunca some") e ARCHITECTURE 2.8.
- **Uma janela de ferramenta sempre no topo pode ser minimizada?** Tecnicamente sim: `ShowWindow(SW_MINIMIZE/SW_SHOWMINNOACTIVE)` não depende de `WS_MINIMIZEBOX`, e `IntegracaoTestes.InstanciaUnica_SegundaAberturaRevelaOBuzzyEscondidoESai` já faz isso. Ainda não se sabe se o recurso do Windows 11 alcança uma janela sem botão na barra e sempre no topo; isso fica para P5 [HW].
- **Comportamento do WPF** (`Window.OnWindowStateChanged` e `nCmdForShow` do dotnet/wpf):
  - com `ShowActivated = false`, pôr `WindowState = Normal` numa janela minimizada chama `ShowWindow(SW_SHOWNOACTIVATE)`, que restaura sem ativar;
  - `Show()` com `WindowState == Minimized` usa `SW_SHOWMINNOACTIVE`, ou seja, mostra minimizada;
  - `WmShowWindow` só sincroniza a visibilidade em casos de janela dona. Um `ShowWindow` vindo de fora não atualiza a visibilidade do WPF.
- **Mostrar pela bandeja.** `MostrarPorComando` já relê a topologia de forma síncrona e manda `TopologyChanged` antes de `CmdShow`.

## 1. Decisões

**D1 — Todas as mensagens do sistema entram pela `JanelaDeServico`.**
- **Decisão:** `WM_WTSSESSION_CHANGE`, `WM_POWERBROADCAST`, `WM_QUERYENDSESSION` e `WM_ENDSESSION` passam a ser tratadas no `Gancho` da janela de serviço, na thread da interface.
- **Motivos:**
  - ela já é a janela de nível superior oculta que recebe difusões (uma janela só de mensagens não as recebe);
  - o HWND está no log (`SERVICO`) e os testes o conferem (`BuzzyEmTeste.Servico`), então tudo pode ser testado com mensagens postadas;
  - roda na mesma thread do núcleo.
- **Descartado:**
  - `Microsoft.Win32.SystemEvents`: o portão permite o namespace, mas ele cria janela e thread próprias e usa eventos estáticos. Além disso, só traduz `PBT_APMRESUMESUSPEND` como retomada, perdendo a retomada automática, e não pode ser testado postando mensagens ao HWND conhecido.
  - Tratar na janela do personagem: ela é escondida, minimizada e movida por outros motivos.

**D2 — Registro para avisos de sessão.**
- **Decisão:**
  - `WTSRegisterSessionNotification(hwndServico, NOTIFY_FOR_THIS_SESSION)` em `Aplicacao.Iniciar`;
  - se falhar, até 4 novas tentativas por temporizador de disparo único, em 2, 5, 15 e 30 s;
  - `WTSUnRegisterSessionNotification` em `JanelaDeServico.Dispose`, antes de destruir a janela.
- **Motivos:** a documentação avisa que a chamada retorna `RPC_S_INVALID_BINDING` (1702) se o app iniciar antes dos serviços de Área de Trabalho Remota. Isso importa quando "iniciar com o Windows" (Q-04) chegar. A documentação também exige desregistrar antes de destruir a janela. Sem o registro, o app continua funcionando (só não se esconde no bloqueio) e registra o erro no log.
- **Descartado:**
  - esperar `Global\TermSrvReadyEvent`: é um objeto global de outro serviço e não ganha nada sobre tentativas limitadas;
  - `NOTIFY_FOR_ALL_SESSIONS`, `WTSEnumerateSessions*` e `WTSQuerySessionInformation`: observariam outras sessões. Passam a ser regras do portão (seção 3.6).

**D3 — Destino de cada código WTS.**
- **Decisão** (tabela na seção 4.2):
  - `LOCK` e `UNLOCK` vão para o núcleo;
  - `CONSOLE_CONNECT` e `REMOTE_CONNECT` significam "a topologia pode ter mudado": pedem releitura agrupada e contam como sinal forte;
  - os demais só vão para o log;
  - o `lParam` (identificador da sessão) não é lido nem registrado.
- **Por que não esconder em `*_DISCONNECT`:**
  - ninguém vê a área de trabalho de uma sessão desconectada;
  - na troca rápida de usuário, o Windows bloqueia a sessão antes (`LOCK` seguido de `CONSOLE_DISCONNECT`);
  - um motivo novo de ocultamento exigiria mudar a precedência de DEC-020 sem ganho para o usuário.

**D4 — Energia: difusão do Windows e inscrição explícita.**
- **Decisão:** tratar `WM_POWERBROADCAST` e também chamar `RegisterSuspendResumeNotification(hwndServico, DEVICE_NOTIFY_WINDOW_HANDLE)`, desfeito com `UnregisterSuspendResumeNotification` no `Dispose`. O árbitro descarta repetições.
- **Motivo:** com Modern Standby, o Desktop Activity Moderator só entrega o aviso aos processos inscritos e congela os outros alguns segundos depois. Sem a inscrição, o Buzzy seria congelado sem esconder nem gravar. Em S3 a difusão já chega e a inscrição pode duplicá-la.
- **Descartado:** `PowerRegisterSuspendResumeNotification` (callback em outra thread) e `RegisterPowerSettingNotification` (fica para depois, ver D8).

**D5 — Qual aviso de retomada vira `RESUMED`, e quando.**
- **Decisão:**
  - `PBT_APMRESUMEAUTOMATIC` (0x12), `PBT_APMRESUMESUSPEND` (0x7) e `PBT_APMRESUMECRITICAL` (0x6) são o mesmo sinal, "retomada";
  - o primeiro deles depois de um `PBT_APMSUSPEND` abre a retomada;
  - `RESUMED` sai **uma vez**, imediatamente depois da primeira releitura agrupada da topologia;
  - essa releitura não acontece antes de `EsperaAposRetomada` = 1500 ms, nem antes de 300 ms da última mensagem de topologia;
  - se nada disso acontecer, `RESUMED` sai no teto de 8 s;
  - os avisos seguintes só estendem a espera.
- **Motivos:**
  - pela documentação, AUTOMATIC chega em toda retomada, SUSPEND só com usuário presente (nunca num despertar remoto) e CRITICAL quando o aviso de suspensão não foi entregue;
  - monitores DisplayPort costumam sumir e voltar nos primeiros segundos. Revelar antes faria `Reacomodar` trocar o personagem de monitor, e pela regra 5 de 2.8 ele não volta sozinho.
- **Descartado:**
  - usar só SUSPEND: pode nunca chegar, e o personagem ficaria preso escondido;
  - um `RESUMED` por aviso: o núcleo aguenta, mas o log, a gravação e a contagem ficam ambíguos;
  - revelar na hora.

**D6 — O adaptador coordena desbloqueio e retomada.**
- **Decisão:**
  - desbloqueio durante uma retomada em curso é adiado e entregue logo depois do `RESUMED`;
  - desbloqueio com suspensão pendente e nenhuma retomada vista (aviso perdido) entrega `RESUMED` e em seguida `SESSION_UNLOCKED`.
- **Motivo:** o núcleo guarda um só motivo de ocultamento. Um personagem em `HIDDEN(POR_SUSPENSAO)` não é revelado por `SESSION_UNLOCKED`, e a regra da tabela está correta. Além disso, o Windows Hello desbloqueia cerca de 1 s depois de acordar, antes de os monitores estabilizarem.

**D7 — Revelar sempre contra a topologia do momento.**
- **Decisão:**
  - `SESSION_UNLOCKED`, e também `RESUMED` quando sai pelo teto ou por aviso perdido, vêm precedidos de releitura síncrona e `TOPOLOGY_CHANGED`, como `MostrarPorComando` já faz;
  - o `RESUMED` normal sai logo depois da releitura agrupada que acabou de enviar `TOPOLOGY_CHANGED`.
- **Motivo:** `Reaparecer` usa a topologia em cache. Com o personagem escondido, o agrupador pode ainda não ter terminado, por exemplo quando o monitor foi desplugado com a tela bloqueada.

**D8 — O aviso de tela desligada fica para a Fase 6.**
- **Decisão:** não usar `RegisterPowerSettingNotification(GUID_CONSOLE_DISPLAY_STATE)` na Fase 5. A tabela 2.13.3 separa "suspensão e retomada" (Fase 5) de "tela desligada" (Fase 6).
- **Motivos:**
  - nenhum critério da Fase 5 depende disso (S12 é suspender, retomar e bloquear);
  - tela apagada não muda visibilidade nem posição, só o custo de desenhar, que é assunto do laço de animação que a Fase 6 refaz;
  - exigiria uma dimensão nova no núcleo, "tela apagada": sem relógio nem agenda, mas sem esconder;
  - o `lParam` é um ponteiro para `POWERBROADCAST_SETTING` e não pode ser testado com mensagem postada;
  - o caso comum, tela que apaga depois do bloqueio, já deixa o personagem em `HIDDEN(POR_SESSAO)` sem relógio.

**D9 — Minimização pelo Windows: classificar pela coincidência com mudança de monitores e desfazer sem ativar.**
- **Decisão:**
  - toda minimização passa pelo árbitro;
  - ela é **do sistema** se:
    - a topologia lida na hora diverge da conhecida; ou
    - houve um sinal forte de topologia até `J` = 2000 ms antes; ou
    - chega um sinal forte até o fim desse prazo;
  - minimização do sistema é desfeita com `ShowWindow(SW_SHOWNOACTIVATE)`, e o núcleo reacomoda o personagem pela releitura;
  - sem coincidência, a minimização é do usuário (ver D10);
  - enquanto a decisão está pendente, a janela continua minimizada, fora da tela, e o app não a move;
  - para não disputar com o Windows: com 3 restaurações do sistema em 10 s, a próxima minimização é tratada como do usuário e o personagem vai para a bandeja.
- **Motivos:** sem hook e sem ler outros aplicativos, o único indício é a coincidência com a mudança de monitores, que o próprio recurso do Windows provoca. `SW_SHOWNOACTIVATE` é o comando que o próprio WPF usa quando `ShowActivated = false`.
- **Descartado:**
  - toda minimização vira `CMD_HIDE`, como hoje: S8 falha com o padrão do Windows 11;
  - toda minimização é desfeita: contraria Q-03, que é decisão do usuário;
  - ler `MonitorRemovalRecalcBehavior` no registro: não diz a causa desta minimização;
  - `SetWinEventHook` ou outro hook para saber quem minimizou: proibido;
  - bloquear a minimização: `SC_MINIMIZE` não intercepta `ShowWindow`, e `WM_WINDOWPOSCHANGING` não cancela a mudança de estado;
  - `WindowState = Normal` do WPF: não restaura quando o WPF acha que a janela está escondida.

**D10 — Q-03 continua valendo para a minimização feita pelo usuário.**
- **Decisão:**
  - sem coincidência e com o personagem visível, `CMD_HIDE` ao fim do prazo, sem restaurar antes. A janela minimizada é escondida, e a próxima exibição desfaz a minimização;
  - com o personagem já escondido por qualquer motivo, **nunca** `CMD_HIDE`. Pela precedência, ele trocaria `POR_SESSAO` ou `POR_SUSPENSAO` por `POR_USUARIO`. Nesse caso o app só garante que a janela não fique visível minimizada.
- **Motivo:** preserva a decisão do usuário e evita um quadro piscando.

**D11 — Barra de tarefas.**
- **Decisão:**
  - mover a barra, mudar o tamanho ou ligar a ocultação automática já chega por `SPI_SETWORKAREA` e é relido;
  - com ocultação automática, `rcWork` é o retângulo do monitor inteiro, e o chão é a base da tela;
  - a barra aparecer e sumir não muda `rcWork`, não gera mensagem e não acorda o Buzzy;
  - muda só `AoRecriarBarra` (`TaskbarCreated`), que passa a pedir a releitura agrupada ao núcleo.
- **Motivo:** quando o Explorer reinicia, a área útil pode mudar, e hoje o núcleo não fica sabendo.

**D12 — Fim de sessão.**
- **Decisão:**
  - manter o `Application` do WPF (DEC-016, item 10);
  - a janela de serviço também trata:
    - `WM_QUERYENDSESSION`: responde TRUE e envia `SESSION_ENDING`, que grava a posição e pede `Encerrar`;
    - `WM_ENDSESSION(TRUE)`: o mesmo, como rede de segurança;
    - `WM_ENDSESSION(FALSE)`: só registra no log;
  - `Application.SessionEnding` continua ligado. As duas vias convergem: `EXITING` ignora o segundo evento e `_encerrando` barra o resto.
- **Motivos:** o WPF já encerra na pergunta, então gravar na pergunta é o único momento garantido. Tratar também na janela de serviço torna o resultado independente da ordem em que o Windows pergunta às janelas e torna o caminho testável.
- **Descartado:**
  - trocar `Application.Run` por `Dispatcher.Run` e sair só em `WM_ENDSESSION(TRUE)`: o Buzzy sobreviveria a um desligamento cancelado, mas isso reescreve o ciclo de vida das Fases 1 a 4. Reavaliar na Fase 9;
  - cancelar `SessionEnding`: o WPF responderia 0 e bloquearia o desligamento.

**D13 — Gravação urgente e tratadores síncronos.**
- **Decisão:** um `GravarPosicao` produzido por `SUSPENDING`, `SESSION_LOCKED`, `SESSION_ENDING` ou `CMD_EXIT` grava de forma síncrona, antes de a mensagem voltar ao Windows. Os demais seguem o atraso da persistência (2.12). Os tratadores rodam direto no `Gancho`, sem `Adiar`.
- **Motivos:**
  - o Windows dá cerca de 2 s depois de `PBT_APMSUSPEND`, e sob o DAM esse prazo é global;
  - dá cerca de 5 s depois de `WM_QUERYENDSESSION` antes de avisar que o app bloqueia o desligamento;
  - o bloqueio costuma preceder suspensão ou troca de usuário;
  - a urgência é deduzida do evento de origem (o `evento` que `ExecutarEfeito` já recebe), sem mexer em `Efeitos.cs` nem nas referências gravadas.

**D14 — Árbitro puro no núcleo.**
- **Decisão:**
  - criar `Buzzy.Core.Sistema.ArbitroDoSistema`, no molde do `ArbitroDeGestos` (DEC-021);
  - ele recebe sinais já traduzidos com um instante `Environment.TickCount64` e devolve decisões;
  - o adaptador traduz os códigos do Windows e executa as decisões.
- **Motivos:**
  - descartar repetições, esperar depois da retomada, ordenar desbloqueio e retomada e classificar a minimização são regras de tempo, que precisam de testes [AUTO] sem janela;
  - `TickCount64` continua contando durante a suspensão, então dois sinais separados por uma suspensão nunca se correlacionam.

**D15 — O núcleo do personagem não muda.**
- **Decisão:** nenhum evento, efeito ou transição novos. A tabela 2.6 ganha só uma nota sobre a entrega e os invariantes 18 a 22, descritos na seção 2.13.6, que é nova.
- **Motivo:** as regras já existem e estão testadas. Não mexer em `Maquina.cs`, `EstadoDoNucleo.cs`, `Eventos.cs` e `Efeitos.cs` reduz conflitos com as outras áreas da Fase 5.

**D16 — Evidência.**
- **Decisão:**
  - [AUTO] no árbitro e no núcleo, com as topologias de exemplo;
  - testes de integração por mensagens postadas às janelas do Buzzy aberto pelo teste, rotuladas SIMULADAS;
  - bloqueio, suspensão, desconexão, escala, orientação e barra reais ficam como [MANUAL] ou [HW], feitos pelo usuário.
- **Motivo:** AGENTS.md proíbe o agente de alterar configurações globais do Windows. `LockWorkStation` ou `SetSuspendState` dentro de um teste interromperiam o usuário.

## 2. Tipos e arquivos novos

### 2.1 `src/Buzzy.Core/Sistema/ArbitroDoSistema.cs` (núcleo puro, `net10.0`, namespace `Buzzy.Core.Sistema`)

```csharp
public enum MudancaDeSessao { Bloqueio, Desbloqueio }
public enum MudancaDeEnergia { Suspensao, Retomada }          // Retomada = PBT 0x12, 0x7 ou 0x6
public enum PrazoDoSistema { TetoDaRetomada, ConfirmacaoDaMinimizacao }
public enum OrigemDaMinimizacao { Sistema, PersonagemEscondido }

/// Sinal já normalizado pelo adaptador; Ms = Environment.TickCount64 no recebimento.
public abstract record SinalDoSistema(long Ms);
public sealed record SinalDeSessao(MudancaDeSessao Mudanca, long Ms) : SinalDoSistema(Ms);
public sealed record SinalDeEnergia(MudancaDeEnergia Mudanca, long Ms) : SinalDoSistema(Ms);
/// Mensagem que costuma acompanhar mudança de monitores (lista em 4.6: "sinais fortes").
public sealed record SinalDeTopologia(string Motivo, long Ms) : SinalDoSistema(Ms);
/// Fim de uma releitura AGRUPADA (AoAgrupar): leu (Mudou = impressão digital mudou) ou desistiu.
public sealed record TopologiaRelida(bool Mudou, bool Desistiu, long Ms) : SinalDoSistema(Ms);
public sealed record JanelaMinimizada(bool PersonagemVisivel, bool TopologiaDivergente, long Ms) : SinalDoSistema(Ms);
/// A janela deixou de estar minimizada (restaurada por fora, ou por um MostrarJanela do núcleo).
public sealed record JanelaRestaurada(long Ms) : SinalDoSistema(Ms);
/// O usuário pediu para mostrar (bandeja, menu, segunda instância).
public sealed record PedidoDeMostrar(long Ms) : SinalDoSistema(Ms);
public sealed record PrazoVencido(PrazoDoSistema Prazo, long Geracao, bool PersonagemVisivel, long Ms) : SinalDoSistema(Ms);

public abstract record DecisaoDoSistema;
public sealed record EnviarAoNucleo(Evento Evento, bool RelerTopologiaAntes) : DecisaoDoSistema;
public sealed record PedirReleitura(string Motivo, TimeSpan NaoAntesDe) : DecisaoDoSistema;
public sealed record AgendarPrazo(PrazoDoSistema Prazo, TimeSpan Atraso, long Geracao) : DecisaoDoSistema;
public sealed record CancelarPrazo(PrazoDoSistema Prazo) : DecisaoDoSistema;
public sealed record RestaurarJanela(OrigemDaMinimizacao Origem) : DecisaoDoSistema;
public sealed record Ignorado(string Motivo) : DecisaoDoSistema;   // só para log e testes

public sealed record ParametrosDoSistema
{
    public TimeSpan JanelaDeCorrelacao { get; init; } = TimeSpan.FromMilliseconds(2000);  // J
    public TimeSpan EsperaAposRetomada { get; init; } = TimeSpan.FromMilliseconds(1500);  // E
    public TimeSpan TetoDaRetomada { get; init; } = TimeSpan.FromSeconds(8);              // T
    public int MaximoDeRestauracoes { get; init; } = 3;
    public TimeSpan JanelaDeRestauracoes { get; init; } = TimeSpan.FromSeconds(10);
    public static readonly ParametrosDoSistema Padrao = new();
}

/// Árbitro dos eventos do sistema (ARCHITECTURE 2.13.6). Não lê relógio nem Windows; o
/// tempo entra nos sinais. Não é thread-safe: a raiz de composição o usa só na thread da interface.
public sealed class ArbitroDoSistema
{
    public ArbitroDoSistema(ParametrosDoSistema? parametros = null);
    public ParametrosDoSistema Parametros { get; }
    public bool SuspensaoPendente { get; }
    public bool RetomadaEmCurso { get; }
    public bool DesbloqueioAdiado { get; }
    public bool MinimizacaoPendente { get; }
    public IReadOnlyList<DecisaoDoSistema> Receber(SinalDoSistema sinal);   // regras R1–R11 (4.3)
}
```

O `Buzzy.Core.csproj` não muda: a pasta entra sozinha.

### 2.2 Testes novos

| Arquivo | Conteúdo |
|---|---|
| `tests/Buzzy.Core.Testes/Sistema/ArbitroDoSistemaTestes.cs` | Unitários das regras R1 a R11 (tabela 5.1). |
| `tests/Buzzy.Core.Testes/Sistema/SimuladorDoSistema.cs` | Relógio virtual, no molde de `SimuladorDeTempo` de `f4`. Entrega `PrazoVencido` e `TopologiaRelida` quando o tempo passa do instante agendado. A releitura cai em `max(300 ms, NaoAntesDe)`. |
| `tests/Buzzy.Core.Testes/Sistema/ArbitroDoSistemaPropriedadesTestes.cs` | Sequências aleatórias com semente fixa; verifica os invariantes 18 a 22 e o determinismo. |
| `tests/Buzzy.Core.Testes/Sistema/EventosDoSistemaNasTopologiasTestes.cs` | Cenários S1 a S12 no núcleo, com a configuração de `f4` e as topologias de exemplo. |
| `tests/Buzzy.App.Testes/Integracao/SistemaTestes.cs` | Integração por mensagens postadas às janelas do Buzzy aberto pelo teste (tabela 5.3). |
| `tests/Buzzy.Verificacao/VerificacaoFase5.cs` (opcional) | Modo `--fase 5`: com o receptor em primeiro plano, minimização correlacionada, bloqueio e desbloqueio simulados. Confere que o primeiro plano continua no receptor. Rótulo SIMULADO. |

Helper novo em `TopologiasDeExemplo`: `SemMonitorComoOWindows(Topologia t, string chave)`. Remove o monitor e, se ele era o principal, promove o monitor restante mais próximo da antiga origem e translada todos para que o novo principal fique em (0,0).

## 3. Mudanças em arquivos existentes

### 3.1 `src/Buzzy.App/Plataforma/Win32.cs`

Nova seção "Sessão, energia e fim de sessão (Fase 5)":

```csharp
internal const int WM_QUERYENDSESSION = 0x0011, WM_ENDSESSION = 0x0016,
                   WM_POWERBROADCAST = 0x0218, WM_WTSSESSION_CHANGE = 0x02B1;
internal const uint ENDSESSION_CLOSEAPP = 0x00000001, ENDSESSION_CRITICAL = 0x40000000, ENDSESSION_LOGOFF = 0x80000000;
internal const int PBT_APMSUSPEND = 0x4, PBT_APMRESUMECRITICAL = 0x6, PBT_APMRESUMESUSPEND = 0x7,
                   PBT_APMPOWERSTATUSCHANGE = 0xA, PBT_APMRESUMEAUTOMATIC = 0x12, PBT_POWERSETTINGCHANGE = 0x8013;
internal const int WTS_CONSOLE_CONNECT = 0x1, WTS_CONSOLE_DISCONNECT = 0x2, WTS_REMOTE_CONNECT = 0x3,
                   WTS_REMOTE_DISCONNECT = 0x4, WTS_SESSION_LOGON = 0x5, WTS_SESSION_LOGOFF = 0x6,
                   WTS_SESSION_LOCK = 0x7, WTS_SESSION_UNLOCK = 0x8, WTS_SESSION_REMOTE_CONTROL = 0x9,
                   WTS_SESSION_CREATE = 0xA, WTS_SESSION_TERMINATE = 0xB, WTS_SESSION_DESKTOP_READY = 0xF;
internal const uint NOTIFY_FOR_THIS_SESSION = 0, DEVICE_NOTIFY_WINDOW_HANDLE = 0;
internal const int RPC_S_INVALID_BINDING = 1702;
internal const int SW_HIDE = 0, SW_SHOWNOACTIVATE = 4;

[DllImport("wtsapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
internal static extern bool WTSRegisterSessionNotification(nint hWnd, uint dwFlags);
[DllImport("wtsapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
internal static extern bool WTSUnRegisterSessionNotification(nint hWnd);
[DllImport("user32.dll", SetLastError = true)]
internal static extern nint RegisterSuspendResumeNotification(nint hRecipient, uint flags);
[DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
internal static extern bool UnregisterSuspendResumeNotification(nint handle);
/// Só sobre a janela do próprio Buzzy.
[DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
internal static extern bool IsIconic(nint hWnd);
/// Só sobre a janela do próprio Buzzy, e só com SW_SHOWNOACTIVATE ou SW_HIDE (nunca ativa).
[DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
internal static extern bool ShowWindow(nint hWnd, int nCmdShow);
```

### 3.2 `src/Buzzy.App/Plataforma/JanelaDeServico.cs`

- **Eventos novos**, disparados de forma síncrona dentro do `Gancho`:
  - `event Action<MudancaDeSessao, string>? SessaoMudou`;
  - `event Action<MudancaDeEnergia, string>? EnergiaMudou`;
  - `event Action<string>? FimDeSessaoPedido`.
  - A `string` é o nome do código, só para o log.
- **`bool RegistrarSessao(out int erro)`:** chama `WTSRegisterSessionNotification(Hwnd, NOTIFY_FOR_THIS_SESSION)`. É idempotente: se já registrada, devolve true.
- **`bool RegistrarEnergia(out int erro)`:** guarda o `HPOWERNOTIFY` retornado por `RegisterSuspendResumeNotification(Hwnd, DEVICE_NOTIFY_WINDOW_HANDLE)`.
- **Traduções estáticas puras**, testáveis sem janela:
  - `static MudancaDeSessao? TraduzirSessao(int codigo)`: 7 vira `Bloqueio`, 8 vira `Desbloqueio`, o resto vira `null`;
  - `static bool SessaoMudaTopologia(int codigo)`: verdadeira para 1 e 3;
  - `static MudancaDeEnergia? TraduzirEnergia(int pbt)`: 4 vira `Suspensao`; 0x12, 7 e 6 viram `Retomada`; o resto vira `null`;
  - `static string NomeWts(int)`, `static string NomePbt(int)`, `static string Flags(nint lParam)` (esta devolve, por exemplo, "CLOSEAPP|LOGOFF").
- **Casos novos no `Gancho`:**

| Mensagem | Ação | Retorno |
|---|---|---|
| `WM_WTSSESSION_CHANGE` | Registra `SESSAO\|codigo=<nome>`, sem o id da sessão. Se `TraduzirSessao` der um valor, dispara `SessaoMudou`. Se não, e `SessaoMudaTopologia` for verdadeira, dispara `TopologiaPodeTerMudado(<nome>)`. | `tratado = true`, 0 |
| `WM_POWERBROADCAST` | Registra `ENERGIA\|evento=<nome>`, exceto `PBT_APMPOWERSTATUSCHANGE`, que pode ser frequente. Se `TraduzirEnergia` der um valor, dispara `EnergiaMudou`. | `tratado = true`, **1 (TRUE)** |
| `WM_QUERYENDSESSION` | Registra `FIM_DE_SESSAO\|etapa=consulta\|flags=…` e dispara `FimDeSessaoPedido("WM_QUERYENDSESSION")`. | `tratado = true`, **1** |
| `WM_ENDSESSION` | Registra `FIM_DE_SESSAO\|etapa=confirmacao\|encerra=sim/nao`. Se `wParam != 0`, dispara `FimDeSessaoPedido("WM_ENDSESSION")`. | `tratado = true`, 0 |

- **Reentrância:**
  - campo `_profundidade`, incrementado dentro de `try/finally` em volta do `Gancho`;
  - campo `_descartada`: no começo do `Gancho`, `if (_descartada) return 0;`.
- **`Dispose()`:**
  - se `_descartada`, sai; senão marca `_descartada = true`;
  - desfaz `WTSUnRegisterSessionNotification` e `UnregisterSuspendResumeNotification`, se estiverem registrados;
  - destrói a janela com `_fonte.RemoveHook(Gancho); _fonte.Dispose();`, mas, **se `_profundidade > 0`**, adia isso com `_fonte.Dispatcher.BeginInvoke(DispatcherPriority.Send, …)`;
  - motivo: `WM_QUERYENDSESSION` leva a `EncerrarAplicacao`, que chama `_servico.Dispose()` de dentro do próprio gancho.

### 3.3 `src/Buzzy.App/Apresentacao/JanelaPersonagem.cs`

- `event Action<long>? Minimizada` passa a levar o instante (`Environment.TickCount64`) e a disparar **sem** mexer em `WindowState`.
- Novo `event Action<long>? RestauradaPorFora`.
- `bool EstaMinimizada => Hwnd != 0 && Win32.IsIconic(Hwnd);`
- `bool RestaurarSemAtivar()`:
  - se `!EstaMinimizada`, devolve false;
  - `_restaurandoPorNos = true`;
  - `Win32.ShowWindow(Hwnd, SW_SHOWNOACTIVATE)`;
  - no `finally`, `_restaurandoPorNos = false`;
  - devolve true.
  - O `WM_SIZE(SIZE_RESTORED)` gerado de forma síncrona faz o WPF pôr `WindowState = Normal`.
- `void EsconderNoWin32() => Win32.ShowWindow(Hwnd, SW_HIDE);`. É usado quando o núcleo e o WPF já consideram a janela escondida.
- `AoMudarEstado`:
  - `Minimized` dispara `Minimizada(ms)`;
  - `Normal` vindo de `Minimized` com `!_restaurandoPorNos` dispara `RestauradaPorFora(ms)`;
  - a linha `WindowState = WindowState.Normal` sai.

### 3.4 `src/Buzzy.App/Composicao/Aplicacao.cs`

**Campos novos:**
- `ArbitroDoSistema _sistema = new()`;
- `Dictionary<PrazoDoSistema, (DispatcherTimer Timer, long Geracao)> _prazos`;
- `DispatcherTimer _repetirSessao` e `int _tentativasDeSessao`, com `EsperasDoRegistroDeSessao = [2 s, 5 s, 15 s, 30 s]`;
- `long _releituraNaoAntesDe` (valor de `TickCount64`).

**`Iniciar`:** depois de ligar `TopologiaPodeTerMudado`:
- liga `_servico.SessaoMudou += (m, n) => ReceberDoSistema(new SinalDeSessao(m, Agora()), n);`;
- liga `_servico.EnergiaMudou += (m, n) => ReceberDoSistema(new SinalDeEnergia(m, Agora()), n);`;
- liga `_servico.FimDeSessaoPedido += motivo => Enviar(new SessionEnding(), motivo);`;
- chama `RegistrarSessao()` (seção 4.8) e `_servico.RegistrarEnergia(...)`, que registra `ENERGIA|registrada=sim|nao|erro=…`;
- troca a linha de `_personagem.Minimizada` por `_personagem.Minimizada += ms => Adiar(() => AoMinimizar(ms));`;
- liga `_personagem.RestauradaPorFora += ms => ReceberDoSistema(new JanelaRestaurada(ms), "restaurada por fora");`;
- mantém `_app.SessionEnding`;
- tudo isso vem antes de `Enviar(new Loaded(...))`.

**`ReceberDoSistema(SinalDoSistema s, string origem)`:**
- se `_encerrando`, sai;
- para cada decisão em `_sistema.Receber(s)`, registra `SISTEMA|sinal=<tipo>|origem=<origem>|decisao=<Descrever(d)>` e chama `ExecutarDecisao(d, origem)`;
- para se `_encerrando` ficar verdadeiro no meio.
- `Descrever` produz `EnviarAoNucleo:Resumed`, `PedirReleitura:retomada:1500`, `AgendarPrazo:TetoDaRetomada:8000`, `CancelarPrazo:…`, `RestaurarJanela:Sistema` e `Ignorado:<motivo>`.

**`ExecutarDecisao`:**
- `EnviarAoNucleo`: se `RelerTopologiaAntes`, chama `RelerTopologiaAgora("antes de <evento>")`; depois `Enviar(e.Evento, "sistema: " + origem)`;
- `PedirReleitura`: `AgendarReleitura(motivo, naoAntesDe)`;
- `AgendarPrazo` e `CancelarPrazo`: temporizador de disparo único com `DispatcherPriority.Normal`. No `Tick`: `Stop()` e depois `ReceberDoSistema(new PrazoVencido(prazo, geracao, _visivel, Agora()), "prazo")`;
- `RestaurarJanela`: `RestaurarJanelaDoPersonagem()` (seção 4.6).

**`AoPossivelMudancaDeTopologia(motivo)`:**
- antes do que já faz, chama `ReceberDoSistema(new SinalDeTopologia(motivo, Agora()), motivo)`;
- troca o reinício do agrupador por `ReiniciarAgrupador()` (seção 4.4).

**Novos:** `AgendarReleitura`, `ReiniciarAgrupador` e `RelerTopologiaAgora(motivo)`.
- `RelerTopologiaAgora` é extraído de `MostrarPorComando`: `Ler` síncrono; se não for nulo, atualiza `_topologia`, faz `Enviar(TopologyChanged)`, `AtualizarIconeSeDpiMudou` (extraído de `AoAgrupar`) e registra `TOPOLOGIA|sincrona=sim`.
- Ela **não** avisa o árbitro.

**`AoAgrupar`:**
- no sucesso, depois de `TopologyChanged`, `ReafirmarLugarDaJanela` e do ícone, chama `ReceberDoSistema(new TopologiaRelida(mudou, false, Agora()), motivos)`;
- quando desiste (`!vaiRepetir`), chama `ReceberDoSistema(new TopologiaRelida(false, true, Agora()), motivos)`.

**`AoRecriarBarra`:** troca a leitura silenciosa (`_topologia = atual`) por `AoPossivelMudancaDeTopologia("TaskbarCreated")`. A troca de ícone continua.

**`MostrarPorComando`:**
- no começo, `ReceberDoSistema(new PedidoDeMostrar(Agora()), motivo)`;
- o `Ler` + `TopologyChanged` que já existe passa a ser `RelerTopologiaAgora(...)`;
- depois do `CmdShow`: `if (_visivel && _personagem.EstaMinimizada) RestaurarJanelaDoPersonagem();`.

**`AoMinimizar(long ms)`:**
- sai se `_encerrando` ou se a janela não está mais minimizada;
- calcula `divergente` com `TopologiaDivergenteAgora(out porque)` (seção 4.6);
- registra `MINIMIZADA|visivel=…|divergente=…`;
- chama `ReceberDoSistema(new JanelaMinimizada(_visivel, divergente, ms), "janela minimizada")`.

**`ExecutarEfeito`:**
- **`MostrarJanela`:** `Show()`, depois `if (_personagem.RestaurarSemAtivar()) ReceberDoSistema(new JanelaRestaurada(Agora()), "mostrar")`, e depois o que já existe;
- **`GravarPosicao`:**
  - `bool urgente = evento is Suspending or SessionLocked or SessionEnding or CmdExit;`;
  - repassa para a persistência (da área de persistência) como `Pedir(g.Posicao, urgente)`;
  - até ela existir, só registra `("urgente", …)` no log.

**Proteções:**
- `AplicarNaJanela`: `if (_personagem.EstaMinimizada) return;`. O `_posicionamento` já foi atualizado por quem chamou, e o lugar é aplicado ao restaurar.
- `ReafirmarLugarDaJanela`: sai se a janela estiver minimizada.

**`EncerrarAplicacao`:** para `_repetirSessao` e os temporizadores de prazo. O `_servico.Dispose()` já desfaz os registros.

**`Programa.cs`:** não muda.

### 3.5 Núcleo e ARCHITECTURE 2.6

- `Maquina.cs`, `EstadoDoNucleo.cs`, `Configuracao.cs`, `Eventos.cs` e `Efeitos.cs` **não mudam** nesta área. A tabela de transições não ganha linhas.
- **Notas na tabela 2.6:**
  - na tabela de eventos, linha "Sistema": "Contrato de entrega dos eventos do sistema pelo adaptador: seção 2.13.6";
  - nas linhas `HIDDEN(POR_SESSAO)` e `HIDDEN(POR_SUSPENSAO)`: "o adaptador entrega `SESSION_UNLOCKED` e `RESUMED` precedidos da topologia atual (2.13.6)".
- **Invariantes novos 18 a 22**, verificados no árbitro:
  - **18.** No máximo um `RESUMED` por `SUSPENDING` entregue, e nenhum `RESUMED` sem um `SUSPENDING` pendente. Repetições de suspensão ou retomada não chegam ao núcleo.
  - **19.** Numa retomada em curso, `RESUMED` só sai depois de uma releitura agrupada iniciada no mínimo `EsperaAposRetomada` depois do primeiro aviso, ou no teto. `SESSION_UNLOCKED` recebido nesse intervalo só sai logo depois desse `RESUMED`.
  - **20.** `SESSION_UNLOCKED` e `RESUMED` chegam precedidos de `TOPOLOGY_CHANGED` com uma leitura daquele momento, síncrona ou da releitura que concluiu a retomada. A exceção é quando a leitura falha.
  - **21.** Cada minimização termina em exatamente uma resolução: restaurada pelo sistema, escondida pelo usuário (`CMD_HIDE`) ou mantida escondida. A decisão fica pendente no máximo por `J`. `CMD_HIDE` só sai com o personagem visível e sem nenhum sinal forte entre `J` antes da minimização e o fim do prazo, ou pela trava de 3 restaurações em 10 s.
  - **22.** O adaptador de sistema nunca ativa janela: nenhum `SW_RESTORE`, `SW_SHOWNORMAL`, `SetForegroundWindow` ou `SetActiveWindow` nesses caminhos. Também nunca emite `CMD_SHOW`, então nunca revela o que o usuário escondeu (invariante 10).

### 3.6 Portão de APIs (`tools/Buzzy.PortaoApis/Regras.cs`)

- **Novas regras em `Categoria.LerOutrosAplicativos`:**
  - `WTSEnumerateSessions` e `WTSEnumerateSessionsEx`: "lista as sessões do sistema";
  - `WTSEnumerateProcesses` e `WTSEnumerateProcessesEx`: "lista os processos de todas as sessões";
  - `WTSQuerySessionInformation`: "lê usuário e estado de qualquer sessão";
  - todas marcadas "(além da lista mínima)".
- **Exceções permitidas, citadas no comentário de `ListaProibida`:** `WTSRegisterSessionNotification`, `WTSUnRegisterSessionNotification`, `RegisterSuspendResumeNotification`, `UnregisterSuspendResumeNotification`, `IsIconic` e `ShowWindow`.
- `NOTIFY_FOR_ALL_SESSIONS` é uma constante, não uma API, e o portão não a pega. Ela fica proibida em SECURITY 3.1 e deve ser conferida em revisão.

### 3.7 Testes existentes

- **`IntegracaoTestes.InstanciaUnica_SegundaAberturaRevelaOBuzzyEscondidoESai`:**
  - a espera por `VISIVEL nao` sobe de 3000 para 5000 ms, porque agora o esconder sai depois de `J`;
  - depois de revelar, afirma `IsIconic == false`.
- **`IntegracaoTestes.Bandeja_IconeERecriadoQuandoABarraDeTarefasReinicia`:** afirma também `TOPOLOGIA` com `motivo` contendo `TaskbarCreated`.
- **`NativoTeste.cs`:** constantes WTS, PBT, `WM_QUERYENDSESSION`, `WM_ENDSESSION`, `WM_POWERBROADCAST`, `WM_WTSSESSION_CHANGE` e `ENDSESSION_CLOSEAPP`, mais `IsIconic` e `GetForegroundWindow`, esta só nos testes. O portão não varre `tests/`.
- **`BuzzyEmTeste.cs`:** `EventoDoLog` ganha `TimeSpan Instante`, lido do prefixo `[hh:mm:ss.fff]`, para medir durações.
- **`PlataformaTestes.cs`:** dois testes novos (tabela 5.2).
- **`tests/Buzzy.PortaoApis.Testes/TestesDaListaProibida.cs`:** acrescenta as cinco funções WTS a `FuncoesMinimas` e as seis permitidas a `ExcecoesDocumentadasNaoSaoProibidas`.

### 3.8 Documentação (na ordem de AGENTS.md, ao fechar)

- **ARCHITECTURE 1:** o que foi implementado.
- **ARCHITECTURE 2.8, "Durante a execução":** novo item: "O Windows 11 pode minimizar a janela ao desconectar o monitor: o adaptador desfaz sem ativar e o núcleo reacomoda (2.13.6)".
- **ARCHITECTURE 2.12:** gravação urgente (D13).
- **ARCHITECTURE 2.13.3:**
  - linha "Fim de sessão": "o WPF encerra na pergunta; a posição é gravada de forma síncrona ao tratá-la, na janela de serviço ou em `SessionEnding`, o que vier primeiro";
  - linha "Bloqueio": WTS com novas tentativas;
  - a linha "Suspensão e tela desligada" se divide em "Suspensão e retomada" (Fase 5: difusão, inscrição e descarte) e "Tela desligada" (Fase 6);
  - linha nova "Minimização pelo sistema".
- **ARCHITECTURE 2.13.6 (nova), "Sessão, energia e minimização pelo sistema":** regras R1 a R11, parâmetros e invariantes 18 a 22.
- **DECISIONS:** "DEC-0xx — Eventos do sistema no app (Fase 5)", resumindo D1 a D16. Registrar que D10 interpreta Q-03 (a minimização do *usuário* esconde) sem mudá-la.
- **SECURITY:**
  - 3.1: APIs e restrições, com o texto de 3.6;
  - 6: o identificador da sessão nunca é lido nem registrado;
  - 8, item 1: novas regras do portão.
- **TODO, Fase 5:** subtarefas desta área e a calibração de `J`, `E`, `T` e do agrupamento em P5.

## 4. Regras exatas

### 4.1 Parâmetros (valores iniciais, a calibrar em P5)

| Nome | Valor | Onde fica |
|---|---|---|
| Agrupamento `A` | 300 ms (já existe) | `Aplicacao.Agrupamento` |
| Janela de correlação `J` | 2000 ms | `ParametrosDoSistema` |
| Espera após retomada `E` | 1500 ms | `ParametrosDoSistema` |
| Teto da retomada `T` | 8000 ms | `ParametrosDoSistema` |
| Trava de restaurações | 3 em 10 s | `ParametrosDoSistema` |
| Tentativas de registro de sessão | 2, 5, 15, 30 s (4, de disparo único) | `Aplicacao` |

Todos os temporizadores são de disparo único e só existem durante uma transição. Em repouso não há nenhum (DEC-011).

### 4.2 Tradução no adaptador

| Mensagem e código | Vira | Observação |
|---|---|---|
| `WTS_SESSION_LOCK` (0x7) | `SinalDeSessao(Bloqueio)` | |
| `WTS_SESSION_UNLOCK` (0x8) | `SinalDeSessao(Desbloqueio)` | |
| `WTS_CONSOLE_CONNECT` (0x1), `WTS_REMOTE_CONNECT` (0x3) | `TopologiaPodeTerMudado(nome)` | releitura agrupada e sinal forte |
| 0x2, 0x4, 0x5, 0x6, 0x9, 0xA, 0xB, 0xF | só log | |
| `PBT_APMSUSPEND` (0x4) | `SinalDeEnergia(Suspensao)` | |
| `PBT_APMRESUMEAUTOMATIC` (0x12), `PBT_APMRESUMESUSPEND` (0x7), `PBT_APMRESUMECRITICAL` (0x6) | `SinalDeEnergia(Retomada)` | |
| `PBT_APMPOWERSTATUSCHANGE` (0xA), `PBT_POWERSETTINGCHANGE` (0x8013) | nada | não registrado nesta fase |
| `WM_QUERYENDSESSION` | `Enviar(SessionEnding)`, responde 1 | síncrono |
| `WM_ENDSESSION` com `wParam` ≠ 0 | `Enviar(SessionEnding)` | síncrono e idempotente |
| `WM_ENDSESSION` com `wParam` = 0 | só log | tarde demais (DEC-016, item 10) |

### 4.3 Árbitro: estado e regras

**Estado inicial:**
- `suspensa`, `retomando`, `desbloqueioAdiado` e `minimizada` começam `false`;
- `ultimoSinalForte = null`;
- `geracao[Teto] = geracao[Confirmacao] = 0`;
- `restauracoes`: fila vazia de instantes.

```
R1  SinalDeSessao(Bloqueio):
      desbloqueioAdiado = false
      → EnviarAoNucleo(SessionLocked, false)

R2  SinalDeSessao(Desbloqueio):
      se retomando: desbloqueioAdiado = true → Ignorado("desbloqueio adiado até o fim da retomada")
      senão se suspensa: suspensa = false → EnviarAoNucleo(Resumed, true), EnviarAoNucleo(SessionUnlocked, false)
      senão → EnviarAoNucleo(SessionUnlocked, true)

R3  SinalDeEnergia(Suspensao):
      se suspensa e retomando: retomando = false → CancelarPrazo(Teto), Ignorado("nova suspensão durante a retomada")
      senão se suspensa → Ignorado("suspensão repetida")
      senão: suspensa = true → EnviarAoNucleo(Suspending, false)

R4  SinalDeEnergia(Retomada) em t:
      ultimoSinalForte = t;  [R9 se minimizada]
      se suspensa e não retomando: retomando = true; g = ++geracao[Teto]
            → PedirReleitura("retomada", E), AgendarPrazo(Teto, T, g)
      senão se retomando → PedirReleitura("retomada repetida", E)     // estende; o teto não reinicia
      senão → PedirReleitura("retomada sem suspensão", E)

R5  SinalDeTopologia em t:  ultimoSinalForte = t;  [R9 se minimizada]

R6  TopologiaRelida(mudou, desistiu) em t:
      se mudou: ultimoSinalForte = t; [R9 se minimizada]
      se retomando: ConcluirRetomada(relerAntes: desistiu)

R7  JanelaMinimizada(visivel, divergente) em t:
      se minimizada → Ignorado("minimização repetida")
      senão se não visivel → RestaurarJanela(PersonagemEscondido)
      senão se |{r em restauracoes : t − r < 10 000}| ≥ 3 → EnviarAoNucleo(CmdHide, false)   // trava contra disputa com o Windows
      senão se divergente ou (ultimoSinalForte ≠ null e t − ultimoSinalForte ≤ J) → RestauracaoDoSistema(t)
      senão: minimizada = true; g = ++geracao[Confirmacao] → AgendarPrazo(Confirmacao, J, g)

R8  PrazoVencido(Confirmacao, g, visivel):
      se não minimizada ou g ≠ geracao[Confirmacao] → Ignorado("prazo antigo")
      minimizada = false
      se visivel → EnviarAoNucleo(CmdHide, false)            // Q-03, sem restaurar antes
      senão → RestaurarJanela(PersonagemEscondido)

R9  (interna, com minimizada) sinal forte ou releitura com mudança em t:
      minimizada = false → CancelarPrazo(Confirmacao) + RestauracaoDoSistema(t)

R10 PrazoVencido(Teto, g):
      se não retomando ou g ≠ geracao[Teto] → Ignorado("prazo antigo")
      ConcluirRetomada(relerAntes: true)

R11 PedidoDeMostrar: se minimizada: minimizada = false → CancelarPrazo(Confirmacao), RestaurarJanela(Sistema)
    JanelaRestaurada: se minimizada: minimizada = false → CancelarPrazo(Confirmacao)

RestauracaoDoSistema(t): restauracoes.Enfileirar(t) → RestaurarJanela(Sistema), PedirReleitura("minimizada pelo sistema", 0)
ConcluirRetomada(relerAntes): retomando = false; suspensa = false
      → CancelarPrazo(Teto), EnviarAoNucleo(Resumed, relerAntes)
      se desbloqueioAdiado: desbloqueioAdiado = false → EnviarAoNucleo(SessionUnlocked, false)
```

Sinais **fortes**: `WM_DISPLAYCHANGE`, `SPI_SETWORKAREA`, `WM_DPICHANGED` da janela do personagem, `TaskbarCreated`, `WTS_CONSOLE_CONNECT`, `WTS_REMOTE_CONNECT`, qualquer retomada, e `TopologiaRelida` com `Mudou`.

Não contam como fortes: bloqueio, desbloqueio, bandeja e `WM_SETTINGCHANGE` com outro `wParam`.

### 4.4 Agrupador com "não antes de"

```
AgendarReleitura(motivo, naoAntesDe):
    _motivosPendentes.Add(motivo); _releiturasFalhas = 0
    _releituraNaoAntesDe = max(_releituraNaoAntesDe, agora + naoAntesDe)
    ReiniciarAgrupador()
ReiniciarAgrupador():
    _agrupador.Stop(); _agrupador.Interval = max(A, _releituraNaoAntesDe − agora); _agrupador.Start()
```

Aqui `agora` é `Environment.TickCount64`. As esperas de nova tentativa (500 ms, 1 s e 2 s) não mudam.

Como o agrupador é um temporizador só, reiniciado na própria thread, a primeira `TopologiaRelida` depois de uma retomada acontece necessariamente depois de `E`. Por isso R6 não compara tempos.

### 4.5 Revelar com a topologia do momento

`EnviarAoNucleo(e, relerAntes: true)` executa:
1. `RelerTopologiaAgora`: `Ler()`; se não for nulo, atualiza `_topologia`, faz `Enviar(TopologyChanged)` (em `Hidden`, só atualiza o cache) e atualiza o ícone se o DPI do principal mudou. Se falhar, segue com o cache e registra o erro.
2. `Enviar(e)`.

A releitura síncrona nunca é repassada ao árbitro como `TopologiaRelida`.

### 4.6 Minimização

**Divergência no momento da minimização**, calculada em `TopologiaDivergenteAgora`:

```
L = LeitorDeTopologia.Ler()
divergente = L == null
          ∨ ¬L.MesmaConfiguracao(_topologia)
          ∨ (núcleo.Lugar ≠ null ∧ L.PorChave(núcleo.Lugar.Monitor.Chave) == null)
```

**Classificação**, com `tm` = instante da minimização:
- é do sistema se `divergente`, ou se há um sinal forte `s` com `tm − s ≤ J`, ou com `tm < s < tm + J` (estritamente antes do prazo);
- é do usuário se não for do sistema e o personagem estiver visível no prazo.

**Execução de `RestaurarJanela`** (`RestaurarJanelaDoPersonagem`):

```
se !_visivel: _personagem.EsconderNoWin32(); fim     // não aparece nem por um quadro; a próxima exibição desfaz a minimização
_personagem.RestaurarSemAtivar()                     // IsIconic ⇒ ShowWindow(SW_SHOWNOACTIVATE)
AplicarNaJanela(_posicionamento)                     // lugar atual do núcleo; a releitura agrupada reacomoda
```

**Caminho do usuário:** `CmdHide` gera `EsconderJanela`, e `Hide()` do WPF esconde a janela ainda minimizada (SW_HIDE), sem piscar. Na próxima `MostrarJanela`, `Show()` usa `SW_SHOWMINNOACTIVE`, porque o `WindowState` ficou `Minimized`, e em seguida `RestaurarSemAtivar()` a restaura.

### 4.7 Gravação urgente e fim de sessão

- `urgente = evento is Suspending or SessionLocked or SessionEnding or CmdExit`.
- Urgente significa gravação atômica síncrona dentro do mesmo tratamento de mensagem. Se houver uma gravação atrasada pendente, ela é substituída pela mais recente. Conteúdo igual ao último gravado não é regravado.

Sequência na pergunta de fim de sessão:
1. O Windows envia `WM_QUERYENDSESSION` à janela de serviço, ou o WPF dispara `SessionEnding`.
2. `Enviar(SessionEnding)` leva o núcleo a `EXITING` e produz `[GravarPosicao, Encerrar]`.
3. `GravarPosicao` grava de forma urgente e síncrona.
4. `Encerrar` executa `EncerrarAplicacao`: o menu fecha, a captura é solta, os temporizadores param, a bandeja é removida, `_servico.Dispose()` desfaz os registros e adia a destruição da janela, o personagem fecha e `_app.Shutdown` é assíncrono.
5. O `Gancho` devolve 1.

A segunda via encontra `_encerrando` e não faz nada.

### 4.8 Registro, novas tentativas e desregistro

`RegistrarSessao()`:
- `_servico.RegistrarSessao(out erro)`;
- em caso de falha, registra `SESSAO|registrada=nao|erro=<n>|novaTentativaMs=<ms>` e agenda `EsperasDoRegistroDeSessao[_tentativasDeSessao++]`, enquanto houver tentativas;
- em caso de sucesso, registra `SESSAO|registrada=sim`.

`RegisterSuspendResumeNotification` não tem novas tentativas: se falhar, a difusão comum continua cobrindo S3.

No `Dispose`: `WTSUnRegisterSessionNotification` e `UnregisterSuspendResumeNotification`, antes de destruir a janela. Registra `SESSAO|removida=sim` e `ENERGIA|removida=sim`.

### 4.9 Barra de tarefas

- O chão é `AreaUtil.Base`, lido por monitor.
- Barra embaixo a 96 DPI: `rcWork = (0,0)-(1920,1032)`, com chão em 1032.
- Ocultação automática: `rcWork = rcMonitor = (0,0)-(1920,1080)`, com chão em 1080. A âncora vai para `(x,1080)` pela posição relativa (`FracaoY = 1`).
- A barra deslizar para dentro e para fora não gera mensagem. Ela cobre os pés temporariamente, o que é aceito (ver seção 7).

### 4.10 Exemplos com coordenadas negativas e linhas do tempo

**S2/S8 no hardware do usuário.**
- O secundário `DISPLAY2` fica em `(-1920,0)-(0,1080)`, com área útil até 1032. A âncora está em `(-400,1032)`: `FracaoX = (−400 + 1920)/1920 = 0,791667` e `FracaoY = 1`.
- Ao desplugar, sobra só `DISPLAY1` em `(0,0)-(1920,1080)`. `Reacomodar` escolhe o monitor mais próximo de `(-400,1032)`, que é `DISPLAY1`:
  - x = 0 + round(0,791667 × 1920) = 1520;
  - y = 1032;
  - `PrenderNaAreaUtil` mantém x dentro de [64, 1856] e y dentro de [128, 1032].
- Resultado: âncora em **(1520,1032)**, retângulo **(1456,904)-(1584,1032)**.
- O mesmo vale se o monitor for desplugado com a tela bloqueada: no desbloqueio, a releitura síncrona e `TOPOLOGY_CHANGED` precedem `SESSION_UNLOCKED`, e o resultado é igual.
- Com a âncora na parede esquerda, `(-1856,1032)`: `FracaoX = 64/1920`, x vira 64, e o retângulo é `(0,904)-(128,1032)`.

**S3.** Em `EmpilhadoSecundarioAcima`, a âncora está em `(1000,-48)`: `FracaoX = 1320/2560 = 0,515625` e `FracaoY = 1`. Removido o secundário, a âncora vai para **(990,1032)**.

**Linha do tempo do S8, com a minimização do Windows 11 ligada:**
- t=0: o Windows aplica a nova topologia.
- t≈50: `WM_DISPLAYCHANGE` gera `SinalDeTopologia` e liga o agrupador (300 ms).
- t≈60: `WM_SIZE(SIZE_MINIMIZED)` dispara `Minimizada`.
- Em seguida, `AoMinimizar` encontra `divergente = true` (falta `DISPLAY2`). R7 decide `RestaurarJanela(Sistema)` e `PedirReleitura`. A janela é restaurada fora da tela, sem ativar.
- t≈350: `AoAgrupar` envia `TopologyChanged`. O núcleo vai de `Idle` a `Settling` e depois a `Idle` em (1520,1032), e o app aplica `MoverJanela`.
- Se a ordem for inversa (minimização antes da mensagem), o resultado é o mesmo: pela divergência ou por R9.

**Linha do tempo do S12 sem senha ao despertar:**
- t=0: `PBT_APMSUSPEND` gera `Suspending`. O núcleo vai de `Idle` a `Hidden(PorSuspensao)`, com `EsconderJanela`, `CancelarDecisao` e `GravarPosicao` urgente e síncrona. O tratador devolve TRUE.
- t=R: `PBT_APMRESUMEAUTOMATIC` pede releitura com espera `E` e agenda o teto `T`.
- t=R+30: `RESUMESUSPEND` pede releitura repetida; a releitura fica marcada para não antes de R+1530.
- t=R+400: `WM_DISPLAYCHANGE` reinicia o agrupador com intervalo `max(300, 1130)`.
- t=R+1530: `TopologyChanged`, `TopologiaRelida`, `RESUMED`. O núcleo vai de `Hidden` a `Settling` e depois a `Idle`, e a janela aparece.

**Com senha ao despertar e Windows Hello:**
- `LOCK` chega antes da suspensão e deixa o núcleo em `Hidden(PorSessao)`.
- `Suspending` não tem efeito pela precedência.
- `UNLOCK` em R+900 fica adiado.
- Em R+1530: `RESUMED`, que não tem efeito, e em seguida `SESSION_UNLOCKED`, que revela.

## 5. Testes [AUTO] propostos

### 5.1 `ArbitroDoSistemaTestes` (núcleo, tempos sintéticos)

| Teste | Cenário | Afirma |
|---|---|---|
| `Bloqueio_EnviaSessionLockedNaHora` | Bloqueio | `[EnviarAoNucleo(SessionLocked,false)]` |
| `Desbloqueio_SemSuspensao_EnviaUnlockedComReleitura` | Desbloqueio | `SessionUnlocked` com `RelerTopologiaAntes = true` |
| `Suspensao_RepetidaIgnorada` | Suspensão duas vezes (difusão + inscrição) | um só `Suspending`, depois `Ignorado` |
| `RetomadaAutomatica_PedeReleituraComEsperaEAgendaTeto` | Suspensão, retomada | `PedirReleitura(_,1500)` e `AgendarPrazo(Teto,8000,g)`; nenhum `Resumed` |
| `Retomada_ConcluiNaPrimeiraReleitura_UmResumedSo` | Suspensão, retomada, retomada, `TopologiaRelida`, `TopologiaRelida` | exatamente um `Resumed`, e `CancelarPrazo(Teto)` |
| `RetomadaPeloUsuarioSozinha_TambemAbre` | só 0x7 depois da suspensão | abre a retomada |
| `RetomadaSemSuspensao_SoRele` | retomada sozinha | nenhum `Resumed`; `PedirReleitura("retomada sem suspensão")` |
| `Retomada_TetoEnviaResumedComReleitura` | prazo do teto vence | `Resumed` com `relerAntes = true` |
| `Retomada_ReleituraDesistiu_ConcluiComReleitura` | `TopologiaRelida(desistiu)` | `Resumed(true)` |
| `DesbloqueioDuranteRetomada_SaiDepoisDoResumed` | Suspensão, retomada, desbloqueio, releitura | a ordem é `Resumed` e depois `SessionUnlocked` |
| `DesbloqueioComSuspensaoSemRetomada_ResumedAntes` | Suspensão, desbloqueio | `Resumed(true)`, depois `SessionUnlocked(false)` |
| `BloqueioCancelaDesbloqueioAdiado` | Suspensão, retomada, desbloqueio, bloqueio, releitura | `Resumed` sem `SessionUnlocked` |
| `NovaSuspensaoDuranteRetomada_CancelaTeto` | Suspensão, retomada, suspensão | `CancelarPrazo(Teto)`; nenhum `Suspending` novo |
| `Minimizacao_SinalAteJAntes_Sistema` | sinal em t, minimização em t+2000 / t+2001 | `RestaurarJanela(Sistema)` / prazo agendado |
| `Minimizacao_Divergente_SistemaEPedeReleitura` | minimização com `divergente` | `RestaurarJanela(Sistema)`, `PedirReleitura(_,0)` |
| `Minimizacao_SemSinal_ViraCmdHideNoPrazo` (Q-03) | minimização, prazo com personagem visível | `EnviarAoNucleo(CmdHide)` sem `RestaurarJanela` |
| `Minimizacao_SinalAntesDoPrazo_Sistema` | minimização, sinal em t+500 | `CancelarPrazo`, `RestaurarJanela(Sistema)`; o prazo seguinte é `Ignorado` |
| `Minimizacao_ReleituraComMudanca_Sistema` | minimização, `TopologiaRelida(mudou)` | sistema |
| `Minimizacao_PersonagemEscondido_NuncaCmdHide` | minimização com `visivel = false` | `RestaurarJanela(PersonagemEscondido)` |
| `Minimizacao_EscondidoAntesDoPrazo_SoReesconde` | minimização, prazo com `visivel = false` | nenhum `CmdHide` |
| `Minimizacao_PedidoDeMostrar_Restaura` | minimização pendente, pedido de mostrar | `RestaurarJanela(Sistema)`; o prazo seguinte é ignorado |
| `Minimizacao_RestauradaPorFora_Cancela` | pendente, `JanelaRestaurada` | só `CancelarPrazo` |
| `Minimizacao_TresRestauracoesEmDezSegundos_Bandeja` | 4 minimizações correlacionadas em 10 s | a quarta produz `CmdHide` |
| `Retomada_ContaComoSinalForte` | retomada em t, minimização em t+1000 | sistema |

### 5.2 App sem janelas (`PlataformaTestes.cs`)

- **`ConstantesDeSessaoEEnergiaTemOsValoresDoWindows`:** 0x02B1, 0x0218, 0x0011, 0x0016, os PBT 4, 6, 7, 0x12, os WTS 1, 3, 7, 8, os flags de 0 e `SW` 0 e 4.
- **`TraducaoDasMensagensDeSessaoEEnergia`:** tabela de 4.2 aplicada a `TraduzirSessao`, `SessaoMudaTopologia` e `TraduzirEnergia`.

### 5.3 Integração (`SistemaTestes.cs`, [Integracao], mensagens SIMULADAS só às janelas do Buzzy aberto pelo teste)

| Teste | Cenário | Afirma |
|---|---|---|
| `Registro_SessaoEEnergia_NaPartidaENoFim` | inicia e fecha com WM_CLOSE | `SESSAO\|registrada=sim`, `ENERGIA\|registrada=sim`, `…\|removida=sim` |
| `Bloqueio_EscondeGravaEDesbloqueioMostraNoMesmoLugar` (S12) | posta 0x2B1/7 e depois 0x2B1/8 | `NUCLEO SessionLocked→Hidden`, `urgente=sim`, `IsWindowVisible` falso; depois `TOPOLOGIA sincrona=sim` antes de `SessionUnlocked Hidden→Settling→Idle`, mesmo retângulo, `IsIconic` falso, `GetForegroundWindow() != Janela` |
| `Desbloqueio_NaoMostraOQueOUsuarioEscondeu` (inv. 10) | minimiza sem sinal (espera J+1 s), posta 7, 8, `PBT` 4 e 0x12 | continua escondido; `MINIMIZADA` resolve como usuário |
| `Suspensao_EscondeERetomadaMostraDepoisDaEspera` (S12) | `WM_POWERBROADCAST` 4, depois 0x12 | escondido; `VISIVEL sim` entre 1400 e 3000 ms depois de `ENERGIA …RESUMEAUTOMATIC` (pelo `Instante`); um `Resumed` |
| `Retomada_AutomaticaEPeloUsuario_UmResumed` | 4, 0x12, 7 | um único `NUCLEO evento=Resumed`; `SISTEMA …PedirReleitura:retomada repetida` |
| `Suspensao_Repetida_UmSuspending` | 4, 4 | um `Suspending` no log do núcleo |
| `Retomada_SemSuspensao_NaoMudaVisibilidade` | só 0x12 | nenhum `VISIVEL`; `TOPOLOGIA motivo` contém "retomada sem suspensão" |
| `Desbloqueio_ComSuspensaoPendente_Mostra` | 4, depois WTS 8 | `Resumed`, depois `SessionUnlocked`; visível |
| `BloqueioDuranteSuspensao_SoDesbloqueioMostra` | 4, 7, 0x12, espera 3 s, depois 8 | escondido até o 8; depois visível |
| `ConexaoDoConsole_Rele` | WTS 1 | `TOPOLOGIA` com motivo `WTS_CONSOLE_CONNECT`, `mudou=nao`; retângulo igual |
| `Minimizacao_JuntoComMudancaDeVideo_RestauraSemAtivar` (S8, simulado) | posta `WM_DISPLAYCHANGE`, depois `ShowWindow(SW_SHOWMINNOACTIVE)` | em até 2 s: `IsIconic` falso, visível, mesmo retângulo, `RestaurarJanela:Sistema`, nenhum `VISIVEL nao`, primeiro plano ≠ Buzzy |
| `Minimizacao_SeguidaDeMudancaDeVideo_Restaura` | minimiza; 500 ms depois, `WM_DISPLAYCHANGE` | restaurada, sem esconder |
| `Minimizacao_Isolada_EscondeComoQ03` | minimiza | `VISIVEL nao` entre J−200 e J+1500 ms; `EnviarAoNucleo:CmdHide`; depois, a segunda instância revela e `IsIconic` fica falso |
| `FimDeSessao_ConsultaRespondeSimGravaEEncerra` | `SendMessageTimeout(Servico, WM_QUERYENDSESSION, 0, ENDSESSION_CLOSEAPP)` | resultado 1; `SessionEnding→Exiting`; `GravarPosicao urgente=sim`; `FIM codigo=0` em até 5 s; `BANDEJA removido=True` |
| `FimDeSessao_EndSessionFalse_Continua` | posta `WM_ENDSESSION(0)` | vivo e visível depois de 1 s |
| `FimDeSessao_EndSessionTrue_Encerra` | posta `WM_ENDSESSION(1)` | sai com código 0 |

Se `PostMessage(WM_POWERBROADCAST)` a outro processo falhar com `ERROR_MESSAGE_SYNC_ONLY` (1159), usar `SendMessageTimeout`, como o teste atual faz com `WM_SETTINGCHANGE`.

### 5.4 Núcleo nas topologias (`EventosDoSistemaNasTopologiasTestes`)

Configuração de `f4`: `QuedaFisica = true`, `Movimento = true`, `Acoes = Nenhuma`.

| Teste | Afirma |
|---|---|
| `S1aS7_BloqueioRemovendoMonitor_DesbloqueioReacomodaNoMaisProximo` | Para cada topologia de `TopologiasDeExemplo.Todas` com 2 ou mais monitores e cada monitor não principal M: `Loaded` em M (0,5; 1), bloqueio, `TopologyChanged(SemMonitor)`, desbloqueio, ticks até `Idle`. A âncora fica na área útil de `MonitorMaisProximo(âncora anterior)`, `Tamanho == 128 DIP × DPI/96` e o estado final não é `Hidden`. Depois o mesmo com suspensão e retomada. |
| `S2_SecundarioAEsquerda_Numerico` | (-400,1032) vai para (1520,1032) e o retângulo é (1456,904)-(1584,1032) |
| `S3_SecundarioAcima_Numerico` | (1000,-48) vai para (990,1032) |
| `RemocaoDoPrincipal_ComoOWindows` | com `SemMonitorComoOWindows`: âncora dentro de alguma área útil e tamanho coerente com o DPI. O monitor escolhido não é afirmado (ver seção 7). |
| `S5_EscalaMudaEscondido_TamanhoAparenteIgual` | `EscalasMistas`: personagem em `DISPLAY2` (96), bloqueio, DPI vira 144, desbloqueio: `Tamanho` 192×192, pés no chão |
| `S6_RetratoViraPaisagemEscondido` | `Retrato`: bloqueio, orientação muda, desbloqueio: âncora dentro da área útil |
| `S10_ResolucaoMudaVisivel_MantemFracao` | `TopologyChanged` com o dobro da resolução: `FracaoX` mantida (±1 px) |
| `S11_BarraOcultaEBordas` | Visível e também atrás de bloqueio: `BarraOculta` dá âncora com y = 1080; `BarraNoTopo` dá y = 1080 e topo ≥ 48+128; `BarraAEsquerda` dá x ≥ 126; `BarraADireita` dá x ≤ 1794. |
| `S12_SuspensaoNoMeioDaQueda_RetomaCaindo` | `Falling`, suspensão: `GravarPosicao` com `FracaoY < 1`; retomada: `Settling`, `Falling` e ticks até `Landing` e depois `Idle` no chão |

### 5.5 Propriedades (`ArbitroDoSistemaPropriedadesTestes`)

- **`SequenciasAleatorias_MantemInvariantes18a22`:**
  - semente `20261001`, 3000 sequências de 80 sinais;
  - tempos crescentes, com saltos de 0 a 5000 ms que incluem exatamente `J`, `J±1` e `E`;
  - usa o `SimuladorDoSistema`, com 5% de releituras que desistem;
  - confere, a cada passo: invariantes 18 a 22, que cada minimização tenha uma única resolução, e que `CmdHide` só saia com o personagem visível.
- **`MesmaSequencia_MesmasDecisoes`:** determinismo.
- **`ComONucleo_UsuarioSempreVence`:**
  - repassa as decisões para uma `Maquina` real (`Cenario`), intercalando `CmdHide` e `CmdShow` aleatórios;
  - depois de um `CmdHide`, nenhuma decisão do sistema revela o personagem antes de um `CmdShow` (invariante 10 no contexto do app);
  - nunca sai `CmdHide` do árbitro com o personagem escondido.

### 5.6 Mapa dos cenários S1–S12 nesta área

| Cenários | Cobertura nesta área |
|---|---|
| S1–S7 | 5.4, em todas as topologias de exemplo |
| S8 | 5.4 (remoção, com o personagem visível ou escondido) e 5.3 (minimização simulada); o real é [HW] |
| S9 | é da área de restauração e persistência, fora daqui |
| S10 | 5.4 |
| S11 | 5.4 e `TaskbarCreated` em 5.3 |
| S12 | 5.1, 5.3 e 5.4 |

## 6. Verificações [MANUAL] e [HW] pendentes

| Item | Por que não é [AUTO] | Como o usuário faz | Evidência |
|---|---|---|---|
| P5, calibração de `J`, `E`, `T` e `A` | exige desplugar monitor, dormir e reconectar de verdade | Buzzy com `--diagnostico`. Desplugar o monitor esquerdo, ou usar Win+P "Somente tela do PC" e depois "Estender", com "Minimizar janelas quando um monitor for desconectado" e "Lembrar locais de janelas" ligados e desligados. Suspender e retomar. | Log com `SESSAO`, `ENERGIA`, `MINIMIZADA`, `SISTEMA`, `TOPOLOGIA` e `VISIVEL`, com os ms entre `WM_DISPLAYCHANGE` e a minimização. Mostra também **se o Windows minimiza mesmo a janela** de ferramenta sempre no topo. |
| S8 [MANUAL][HW] | desconexão física | O Buzzy fica no monitor em x negativo; desplugar com a opção de minimizar ligada e depois desligada; reconectar. | Aparece no monitor que restou, cai até o chão, nunca some nem fica minimizado, o foco não muda. Ao reconectar, não volta sozinho, mesmo com "Lembrar locais de janelas" (a janela volta ao lugar do núcleo). |
| S10 [MANUAL] | mudar resolução, escala ou orientação é configuração global, que o agente não altera | Mudar a resolução, a escala de **um** monitor para 125% ou 150% (isso dá escala mista neste hardware) e a orientação para retrato; reverter depois. | Posição relativa mantida, pés no chão, tamanho aparente igual, um `TOPOLOGIA mudou=sim` por troca. |
| S11 [MANUAL] | configuração global da barra | Ligar e desligar a ocultação automática; reiniciar o Windows Explorer pelo Gerenciador de Tarefas. A barra em outra borda não é suportada pelo Windows 11: registrar como N/A, já que o [AUTO] cobre `BarraNoTopo` e as laterais. | Chão na base da tela ou no topo da barra; ícone recriado e `TOPOLOGIA motivo=TaskbarCreated`. Observar se os pés cobrem a faixa que revela a barra oculta. |
| S12 [MANUAL] | bloqueio e sono reais interrompem o usuário | Win+L e desbloquear; suspender e acordar com e sem "exigir entrada ao despertar"; hibernar, se estiver disponível; esconder pela bandeja antes de bloquear e dormir; dormir com o personagem andando ou caindo; trocar de usuário, se houver outra conta; desligar e religar com o Buzzy aberto; um desligamento cancelado por outro aplicativo com trabalho não salvo. | Um `Suspending` e um `Resumed` por ciclo; `settings.json` gravado na suspensão e no desligamento; o personagem escondido pelo usuário continua escondido. No desligamento cancelado, o Buzzy fecha: é uma limitação conhecida (DEC-016). |
| Modern Standby [HW] | provavelmente não existe nesta máquina de mesa | `powercfg /a` (só leitura) mostra S3 ou S0ix | Se for S0ix, conferir que a inscrição entrega os avisos; senão fica pendente. |
| RDP e controle remoto [HW] | precisa de outra máquina | conectar por RDP à sessão | Pendente. |
| Foco com receptor (`--fase 5`, SIMULADO) | abre janelas e ativa o receptor | `Buzzy.Verificacao --injetar-input-na-tela --fase 5`, depois de avisar o usuário | O primeiro plano continua no receptor. |

## 7. Riscos, dúvidas e pontos de contato

- **Heurística de minimização.** Uma minimização do usuário até 2 s depois de uma mudança de vídeo seria desfeita em vez de esconder o personagem. Uma minimização do sistema sem sinal dentro de `J` iria para a bandeja, que é o comportamento atual. P5 calibra `J`. Ainda não se sabe se o recurso do Windows 11 minimiza esta janela (UNCERTAIN).
- **Fim de sessão.** O WPF encerra na pergunta: se outro app cancelar o desligamento, o Buzzy já fechou (DEC-016, item 10, mantido).
- **Avisos de energia duplicados** pela difusão e pela inscrição em S3 são descartados pelo árbitro, mas isso não foi verificado em hardware.
- **Monitores lentos para acordar.** Se o monitor DisplayPort passar de `T` = 8 s para voltar, `Reacomodar` muda o personagem de monitor para sempre (regra 5 de 2.8).
  - **Sugestão à área de restauração:** um período de graça depois da retomada que preserve a chave original.
- **Troca ou remoção do principal.** Quando o principal muda ou é removido, as coordenadas se deslocam e a `AncoraAbsoluta` antiga, usada por `Reacomodar` para achar o monitor mais próximo, fica noutro sistema de coordenadas. Isso pode escolher um monitor que não era o vizinho físico. Cabe à área de topologia, que também muda `Posicionador`. O teste 5.4 não afirma qual monitor é escolhido.
- **Persistência:**
  - precisa expor `Pedir(posicao, urgente)` e gravar o que estiver pendente em `EncerrarAplicacao`;
  - os testes de fim de sessão gravariam o `settings.json` real do usuário. É preciso uma opção de pasta de dados para testes, ou fazer cópia e restaurar.
- **Conflito de mescla.** A Fase 4 ainda não foi mesclada: implementar depois de trazer `f4`, cujo `Aplicacao.Iniciar` diverge da árvore principal.
- **Arquivos que outras áreas também mudam:**
  - `Win32.cs`: usar uma seção própria para reduzir o conflito;
  - `Aplicacao.cs`, pontos combinados:
    - `AoPossivelMudancaDeTopologia` continua a única entrada para mensagens de topologia do Windows;
    - `AoAgrupar` avisa o árbitro nas duas saídas;
    - `ExecutarEfeito(GravarPosicao)` e `EncerrarAplicacao` são compartilhados com a persistência;
  - `JanelaDeServico.cs`: a área de topologia pode querer `WM_DPICHANGED` na janela de serviço, para mudança de escala do principal sem o personagem nele;
  - `JanelaPersonagem.cs`: a área de P6 mexe em DPI;
  - `IntegracaoTestes` e `NativoTeste`.
  - Esta área **não** toca em `Maquina.cs`, `EstadoDoNucleo.cs`, `Configuracao.cs`, `Eventos.cs` nem `Efeitos.cs`. Se a persistência preferir urgência explícita, com `GravarPosicao(..., Urgente)`, ela substitui a dedução pelo evento, mas as referências gravadas de 01 a 05 mudam.
- **Número da decisão.** O próximo livre é DEC-023. Combinar com as outras áreas da Fase 5.
- **Ordem improvável de mensagens.** Um `PBT_APMSUSPEND` entregue dentro de um laço de mensagens aninhado, durante o processamento do núcleo, gravaria depois de o tratador voltar. É raro e fica aceito.
- **Bloqueio perto da retomada.** Se `RESUMED` for entregue antes de um `LOCK` atrasado, o personagem aparece por instantes na área de trabalho que está atrás da tela de bloqueio. O usuário não vê.
- **Personagem na parede ao bloquear.** Se estava escalando, ao desbloquear ele cai até o chão, porque `Validar` não encontra apoio. É o comportamento da tabela, só registro como observação.
- **Registro de sessão que falha sempre.** O personagem continua visível durante o bloqueio, com a autonomia rodando. É inofensivo e fica no log.
- **Barra oculta.** Com ocultação automática, os pés opacos cobrem parte da faixa de 2 px que revela a barra. Observar em S11; um ajuste, se preciso, fica para a Fase 11.
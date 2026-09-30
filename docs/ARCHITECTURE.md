# ARCHITECTURE.md — Arquitetura do Buzzy

> Esta página separa fatos implementados de desenho planejado. Nenhuma proposta aparece como arquitetura existente. Cada seção planejada carrega `STATUS: PLANNED`; escolhas que aguardam decisão do usuário carregam `STATUS: UNCERTAIN` e apontam para [DECISIONS.md](DECISIONS.md).
>
> Última atualização: 2026-09-30

## 1. Arquitetura implementada

Código das Fases 1 a 4 em `src/`, organizado pelas três camadas de DEC-007 (detalhes em DEC-016, DEC-020, DEC-021 e DEC-022). O estado de verificação de cada critério está em TODO.md; nada aqui é VERIFIED por estar escrito. O código em `spikes/` continua descartável e não conta como módulo do Buzzy.

| Projeto ou pasta | Camada | O que existe |
|---|---|---|
| `src/Buzzy.Core` | núcleo puro (`net10.0`, sem WPF nem Windows) | Geometria em pixels físicos e DIPs; `Topologia` (monitores, principal, impressão digital, monitor que contém um ponto, monitor mais próximo, prender na área útil); `Posicionador` (âncora no centro da base, posição inicial, reacomodação pela posição relativa quando a topologia muda). `Personagem/` (Fase 2): máquina de estados da seção 2.6 como função pura de (estado, evento) para (estado, efeitos), fila com prioridade, agenda autônoma com semente, perfis de energia, retrato e gravação/reprodução de sequências. `Personagem/Movimento.cs` (Fase 4): superfícies do monitor da âncora (seção 2.5) e parâmetros da física de passo fixo (seção 2.9), aplicada pela máquina de estados. `Entrada/` (Fase 3): árbitro de gestos da seção 2.7. |
| `src/Buzzy.App/Plataforma` | adaptador de plataforma | Único arquivo com importações do Windows (`Win32.cs`); leitura da topologia; janela de serviço oculta que recebe as mensagens de topologia, bandeja e `TaskbarCreated`; bandeja v4; menu nativo; instância única; log de diagnóstico opcional. |
| `src/Buzzy.App/Apresentacao` | apresentação e adaptador do ponteiro | Janela WPF do personagem (sem borda, `AllowsTransparency`, não ativa, janela de ferramenta, sempre no topo, do tamanho do sprite; responde `MA_NOACTIVATE` e `WM_GETDPISCALEDSIZE`) e o sprite provisório gerado em código. Converte as mensagens de mouse entregues a ela em eventos de ponteiro em pixels físicos e segura a captura do mouse só durante um gesto começado no personagem (Fase 3). Escolhe a pose provisória da pixel art pelo retrato (`PoseDoPersonagem`, Fase 4) e a renderiza uma vez por pose, espelho, expressão e DPI. |
| `src/Buzzy.App/Composicao` | raiz de composição | `Aplicacao` liga janela, serviço, bandeja, menu, topologia, arbitragem e núcleo, e executa os efeitos do núcleo. Em repouso, só há timers de disparo único: agrupamento de topologia, novas tentativas da bandeja e a próxima decisão da agenda autônoma. O relógio de passo fixo só corre quando o núcleo pede (reação, pouso, gesto curto e movimento). Enquanto corre, segue os quadros do compositor do WPF (`CompositionTarget.Rendering`): aplica num lote os passos acumulados e move a janela uma vez por quadro. O arraste não usa relógio. |
| `src/Buzzy.Visual` | apresentação, a partir da Fase 6 | Gerador da identidade em pixel art (`Pixel/`, DEC-018) e o renderizador vetorial da direção anterior, arquivada (DEC-017); ainda não é usado pelo app. |

Fluxo implementado:

- **Topologia:** o Windows avisa a janela de serviço; a raiz agrupa as mensagens e lê a topologia; o núcleo revalida a posição. Se a janela tiver saído do lugar do núcleo, a raiz reaplica esse lugar.
- **Mouse:** a janela do personagem converte o mouse em eventos de ponteiro; a arbitragem produz gestos e diz se a captura continua; o núcleo aplica os gestos e devolve os efeitos.
- **Efeitos:** a raiz executa os efeitos do núcleo (mover, mostrar, esconder, relógio, agenda, menu adiado para fora do processamento, soltar a captura), no mesmo tratamento da mensagem.

Travessia entre monitores (Fase 5), persistência (Fases 5 e 8), animação (Fase 6), painel de energia e tela cheia (Fase 8) ainda não existem. O núcleo já tem as regras da tabela para eles; o app só liga cada capacidade quando a fase dela chega. Na Fase 4, o app liga todas as ações autônomas, a queda física e o movimento.

## 2. Arquitetura planejada

STATUS: PLANNED. P1 e P2 foram aceitos nos limites documentados; P3 aguarda validação controlada. WPF/C#/.NET 10 está em DEC-006. As seções 2.1 a 2.12 descrevem o desenho do produto e a seção 2.13 seu encaixe em WPF. DEC-007 a DEC-014 definem o desenho planejado; DEC-015 registra a autorização de execução. P3 é gate antes da Fase 1; P7 é gate antes da Fase 8. P4 foi aposentado porque o produto não terá chat nem campo de texto.

### 2.1 Princípios

- **Um processo, módulos separados por responsabilidade.** O Buzzy é um único executável. Os módulos são fronteiras de código, não processos, serviços ou plugins. A única exceção aceitável são processos auxiliares que a própria stack impõe, como o motor de um WebView.
- **Núcleo puro e determinístico.** Estado, eventos, arbitragem de input, movimento, nível de energia e personalidade não verbal são código sem acesso ao Windows. Com a mesma semente, nível de energia e sequência de eventos, o núcleo produz a mesma sequência de estados. Isso permite testar quase todo o comportamento sem janela, sem monitor e sem hardware.
- **Um único adaptador toca o Windows.** Janela, input bruto, monitores, DPI, bandeja, ciclo de vida e arquivos passam pelo adaptador de plataforma. O resto do código não chama APIs do sistema.
- **Comportamento separado de aparência.** O núcleo emite estados e sinais lógicos. A apresentação decide quais quadros desenhar. Trocar a arte não altera o núcleo.
- **Ocioso por eventos.** Quando nada se move nem anima, não há timer periódico. O loop de simulação só roda enquanto há movimento, animação ou arraste. A detecção de tela cheia usa eventos WinEvent delimitados em DEC-013; não usa polling periódico global.

### 2.2 Componentes

| Componente | Responsabilidade | Depende de | Nunca faz |
|---|---|---|---|
| Adaptador de plataforma | Cria e move as janelas, recebe input bruto, captura o mouse durante o arraste, enumera monitores, lê DPI, mantém o ícone da bandeja, trata sessão e energia, resolve a pasta de dados e emite eventos limitados da janela em primeiro plano para Q-09. Converte tudo para o sistema de coordenadas canônico. | Windows e a stack escolhida | Decidir comportamento; ler input fora das próprias janelas; ler título, texto, pixels ou identidade de outros apps |
| Mundo do desktop | Modelo puro da topologia: monitores, áreas úteis, escala, orientação, monitor principal. Deriva superfícies (chão, paredes, passagens). Responde "em que monitor está este ponto" e "qual o ponto válido mais próximo". | Nada | Chamar APIs do sistema |
| Núcleo do personagem | Máquina de estados, fila de eventos, relógio lógico, agenda de comportamento autônomo com semente. Produz um retrato do estado e uma lista de efeitos. | Mundo do desktop, Movimento, Arbitragem de input, Personalidade | Desenhar, gravar arquivo, mover janela diretamente |
| Arbitragem de input | Converte eventos de ponteiro em gestos (pressionar, clicar, iniciar arraste, arrastar, soltar, cancelar). Decide se o input pertence ao personagem, painel de energia ou menu. | Nada | Ler teclado global; interpretar teclas como comandos de movimento |
| Movimento | Cinemática com passo fixo: caminhar, escalar, pendurar-se, saltar, cair, pousar. Colisão contra as superfícies do mundo do desktop. | Mundo do desktop | Iniciar ação por conta própria; a decisão é do núcleo |
| Apresentação | Recebe o retrato do estado e escolhe animação, quadro e expressão pelo manifesto de assets. Desenha a janela transparente e gera a máscara de clique. | Manifesto de assets, adaptador (superfície de desenho) | Alterar estado ou posição do personagem |
| Personalidade | Pesos locais para a frequência de ações autônomas, duração das pausas e preferência por expressões e gestos curiosos. É consultada pelo núcleo determinístico, nunca decide sozinha. | Perfil de comportamento local | Ler conteúdo de aplicativos; gerar ou armazenar conversa |
| Configurações e persistência | Esquema tipado com versão, valores padrão, validação, migração e gravação atômica em arquivo local. | Adaptador (caminho da pasta de dados) | Guardar texto digitado, segredos ou dados de outros aplicativos |
| Raiz de composição | Liga os módulos, executa os efeitos pedidos pelo núcleo e controla o loop. | Todos | Conter regra de comportamento |

### 2.3 Fluxo entre componentes

```
Windows --> Adaptador de plataforma --> evento normalizado --> fila do núcleo
                ^                                                  |
                |                                                  v
        efeitos de janela        Arbitragem de input --> Núcleo do personagem <-- Mundo do desktop
        (mover, mostrar,                                    |   |
         esconder, foco)                                    |   +--> Movimento
                ^                                           v
                |                           retrato do estado + efeitos
        Raiz de composição <----------------------------------+
                |
                +--> Apresentação (desenha quadro, atualiza máscara de clique)
                +--> Configurações e persistência (grava posição e preferências)
                +--> Painel de energia (seleciona Baixa, Média ou Alta)
```

1. O adaptador recebe uma mensagem do Windows e a converte em um evento normalizado, já em coordenadas canônicas.
2. A arbitragem transforma eventos de ponteiro em gestos e marca a prioridade de cada evento.
3. O núcleo aplica o evento ao estado atual pela tabela de transições e devolve o novo retrato e os efeitos.
4. A raiz de composição executa os efeitos: move a janela, pede um quadro à apresentação, agenda gravação de configurações ou atualiza o painel de energia.
5. Enquanto houver movimento ou animação, o relógio lógico gera eventos de passo fixo. Quando o personagem para e a animação termina, o relógio para.

Prioridade de eventos, da maior para a menor, conforme [PRODUCT_SPEC.md](PRODUCT_SPEC.md): ação direta do usuário sobre o personagem (pressionar, arrastar); menu, bandeja, painel de energia e outras ações explícitas do usuário, incluindo ocultar e pausar; eventos do sistema, incluindo o modo de tela cheia; comportamento autônomo. Um evento de prioridade maior interrompe atividade de prioridade menor. Eventos autônomos que chegam durante um estado controlado pelo usuário são descartados, não enfileirados.

### 2.4 Coordenadas e desktop virtual

STATUS: PLANNED. Proposta registrada em DEC-008.

- O processo declara consciência de DPI **Per-Monitor V2** desde a Fase 1. Assim o Windows não virtualiza coordenadas nem estica a janela.
- **Sistema canônico:** pixels físicos do desktop virtual. A origem (0,0) é o canto superior esquerdo do monitor principal. Monitores à esquerda ou acima do principal têm coordenadas negativas.
- **Monitor:** chave estável, retângulo do monitor, retângulo da área útil (sem a barra de tarefas), escala (DPI dividido por 96), orientação e se é o principal.
- **Chave estável do monitor:** caminho do dispositivo obtido de `QueryDisplayConfig` e `DisplayConfigGetDeviceInfo`. O nome GDI (`\\.\DISPLAYn`) pode mudar entre sessões e serve apenas como alternativa. STATUS: UNCERTAIN até o protótipo P5 confirmar a estabilidade da chave.
- **Topologia:** a lista de monitores tem uma impressão digital, formada pelas chaves, retângulos e escalas. Uma impressão diferente significa mudança de configuração.
- **Mudanças:** `WM_DISPLAYCHANGE`, `WM_DPICHANGED` e `WM_SETTINGCHANGE` com `SPI_SETWORKAREA` disparam uma nova leitura. O adaptador agrupa rajadas de mensagens antes de publicar uma única `TOPOLOGY_CHANGED`. O intervalo de agrupamento é um valor a calibrar no protótipo P5.
- **Tamanho do personagem:** o corpo tem tamanho lógico em DIPs. O tamanho físico é o tamanho lógico vezes a escala do monitor onde está a âncora (ponto entre os pés) vezes a escala escolhida pelo usuário. Ao cruzar para um monitor de outra escala, o tamanho físico muda no instante em que a âncora cruza a borda.
- **Nenhuma suposição de layout.** Nada no código presume monitores lado a lado, alinhados pelo topo, com a mesma resolução ou com o principal à esquerda.

### 2.5 Superfícies

STATUS: PLANNED. O usuário aceitou Q-05 em [DECISIONS.md](DECISIONS.md): no MVP, as superfícies vêm apenas das áreas úteis dos monitores; janelas de outros aplicativos ficam fora.

Proposta para o MVP: as superfícies são derivadas apenas das áreas úteis dos monitores. Janelas de outros aplicativos não são superfícies. O personagem circula pelas bordas alcançáveis, não pelo conteúdo aberto no desktop.

- **Chão:** a borda inferior da área útil de cada monitor. Com a barra de tarefas embaixo, o chão é o topo da barra.
- **Paredes:** trechos de borda lateral sem monitor vizinho encostado; podem ser escalados para cima e para baixo.
- **Passagens entre monitores:** trechos vizinhos com sobreposição e suporte geométrico compatível. O personagem atravessa andando quando a altura coincide; sobe ou desce por uma transição segura quando a diferença é alcançável. Um vão sem superfície válida não vira um salto automático ilimitado.
- **Bordas superiores:** podem servir como uma borda onde o personagem se apoia pelas mãos, fica pendurado por pouco tempo e se desloca até uma passagem alcançável. Soltar-se inicia `FALLING`.
- O chão de um monitor continua sólido mesmo com outro monitor logo abaixo. A travessia para baixo só ocorre por um caminho apoiado ou por salto dentro do alcance; nunca por queda espontânea causada apenas por um monitor estar abaixo.
- As superfícies são recalculadas a cada `TOPOLOGY_CHANGED`.

**Implementado na Fase 4 (DEC-022), num monitor.** `Superficies.Do` calcula, para o monitor da âncora e o tamanho do sprite nele:

- chão: a base da área útil;
- teto: o topo da área útil mais a altura do sprite, para a borda superior;
- limites laterais da âncora: o sprite inteiro fica dentro da área útil;
- se cada lateral é parede ou passagem: é passagem quando outro monitor encosta nela com sobreposição vertical.

As superfícies são recalculadas a cada passo, a partir da topologia em cache. Uma passagem ainda não é atravessada, porque a travessia é da Fase 5.

**Toon force (DEC-023):** a pedido do usuário, toda lateral da área útil é escalável, inclusive a passagem: a borda da tela vira parede para o macaquinho. Ao chegar numa passagem, ele para, vira ou sobe por ela, como numa parede.

### 2.6 Máquina de estados

STATUS: PLANNED. Estado de comportamento e expressão são dimensões independentes. Uma troca de expressão nunca muda o estado de comportamento.

**Estados de comportamento**

| Estado | Grupo | Significado |
|---|---|---|
| `BOOTING` | sistema | Carrega configurações e topologia e restaura a posição. |
| `IDLE` | autônomo | Parado sobre uma superfície. |
| `WALKING` | autônomo | Andando sobre o chão. |
| `CLIMBING` | autônomo | Subindo, descendo ou parado em uma parede. |
| `HANGING` | autônomo | Sustentado pela borda superior, deslocando-se ou descansando por pouco tempo. |
| `JUMPING` | autônomo | Em trajetória balística iniciada por decisão autônoma. |
| `FALLING` | físico | Sem apoio, sob gravidade. |
| `LANDING` | físico | Transição curta depois de tocar o chão. |
| `RESTING` | autônomo | Descansando ou dormindo, sem animação contínua. O relógio fica parado. |
| `PRESSED` | usuário | Botão pressionado sobre o personagem; ainda não se sabe se é clique ou arraste. |
| `DRAGGING` | usuário | Personagem preso ao cursor. |
| `SETTLING` | usuário | Validação logo depois de soltar ou depois de uma mudança de topologia. |
| `REACTING` | usuário | Reação curta a um clique. |
| `HIDDEN` | sistema | Escondido. Sem relógio, sem desenho. Guarda **por que** foi escondido: `POR_USUARIO`, `POR_SESSAO`, `POR_SUSPENSAO` ou `POR_TELA_CHEIA`. |
| `EXITING` | sistema | Grava estado e encerra. |
| `PEEKING` | autônomo | Escondido atrás da borda de baixo (a barra de tarefas) ou de uma lateral, só com a cabeça e as mãos para fora (DEC-025). Entra e sai pelo clique duplo. Sem relógio; a agenda só troca a cara. |

**Dimensões da Fase 4 ligadas às escolhas do usuário:**

| Dimensão | Valores | Efeito |
|---|---|---|
| Preso pelo usuário | sim ou não | O usuário o soltou na lateral ou no cipó: lá fica até o usuário tirá-lo. A agenda só o faz passear pela mesma superfície (DEC-024). |
| Esconderijo | nenhum, baixo, esquerda ou direita | A borda atrás da qual ele está escondido. Sobrevive ao primeiro clique do clique duplo, a `HIDDEN` e às revalidações (DEC-025). |

**Dimensões ortogonais.** As informações abaixo acompanham o personagem sem fazer parte do estado de comportamento:

| Dimensão | Valores | Efeito |
|---|---|---|
| Expressão | feliz, curioso, sonolento e demais | Nenhum sobre comportamento ou posição |
| Gesto curto | nenhum, espiar, olhar ao redor, coçar-se, espreguiçar-se, brincar e demais definidos no manifesto | Ação visual de macaquinho com duração limitada, executada na superfície atual (`IDLE`, `CLIMBING` parado ou `HANGING`); não muda estado de comportamento, posição nem superfície; qualquer `PRESS`, `CMD_*` ou evento do sistema a encerra na hora |
| Autonomia pausada | sim ou não | Definida por `CMD_PAUSE_AUTONOMY`/`CMD_RESUME_AUTONOMY`. Enquanto sim, nenhum `AUTONOMY_TIMER` é agendado; queda ou pouso em curso terminam; arraste, clique, painel, ocultação e modo de tela cheia continuam funcionando |
| Painel de energia | aberto ou fechado | Enquanto aberto, pausa a autonomia; o movimento físico em curso pode terminar. Arrastar o mascote fecha o painel. Ele contém somente o seletor Baixa/Média/Alta, sem conversa ou campo de texto. |
| Motivo do ocultamento | `POR_USUARIO`, `POR_SESSAO`, `POR_SUSPENSAO`, `POR_TELA_CHEIA` | Decide quais eventos podem tirar o personagem de `HIDDEN`; tela cheia não desfaz uma ocultação feita pelo usuário. Precedência (DEC-020): `POR_USUARIO` > `POR_SESSAO` > `POR_SUSPENSAO` > `POR_TELA_CHEIA`; em `HIDDEN`, um motivo só substitui outro de precedência menor |
| Nível de energia | `BAIXA`, `MEDIA`, `ALTA` | Altera frequência e duração das ações autônomas e a frequência de expressões; padrão `MEDIA` |
| Retorno temporário do modo de tela cheia | posição e chave do monitor apenas em memória, ou vazio | Restaura a posição prévia sem substituir a posição persistida escolhida pelo usuário. É descartado quando o usuário arrasta o personagem ou o mostra manualmente durante o modo: a escolha manual passa a valer. A posição gravada ao esconder ou sair é sempre o retorno, se houver, e nunca a posição temporária (DEC-020) |

**Eventos**

| Origem | Eventos |
|---|---|
| Ponteiro, somente sobre as janelas do Buzzy ou com captura ativa | `POINTER_DOWN(p, botão)`, `POINTER_MOVE(p)`, `POINTER_UP(p, botão)`, `CAPTURE_LOST` |
| Gestos derivados pela arbitragem | `PRESS`, `CLICK`, `DOUBLE_CLICK`, `DRAG_START`, `DRAG_MOVE(p)`, `DRAG_END(p)`, `DRAG_CANCEL`, `CONTEXT_MENU` |
| Painel de energia | `ENERGY_PANEL_OPEN`, `ENERGY_SELECTED(nivel)`, `ENERGY_PANEL_CLOSE`; `ENERGY_SELECTED` aceita somente `BAIXA`, `MEDIA` ou `ALTA` |
| Sistema | `TOPOLOGY_CHANGED(topologia)`, `SESSION_LOCKED`, `SESSION_UNLOCKED`, `SUSPENDING`, `RESUMED`, `SESSION_ENDING` |
| Adaptador de janela ativa | `FULLSCREEN_TARGETS_CHANGED(monitoresOcupados)`; payload contém somente chaves de monitores, sem HWND, processo, título ou texto |
| Bandeja e menu | `CMD_HIDE`, `CMD_SHOW`, `CMD_PAUSE_AUTONOMY`, `CMD_RESUME_AUTONOMY`, `CMD_OPEN_SETTINGS`, `CMD_RESET_POSITION`, `CMD_EXIT` |
| Relógio | `TICK(dt)` com passo fixo, `AUTONOMY_TIMER` |
| Configurações | `SETTINGS_CHANGED(config)` |

**Transições principais**

| De | Evento | Para | Regra |
|---|---|---|---|
| `BOOTING` | configurações e topologia carregadas | `SETTLING` | Posição restaurada pela seção 2.8. Só a primeira carga vale; pedidos anteriores a ela (mostrar, esconder, sessão, topologia) ficam guardados e o personagem só aparece com a carga. Se o monitor restaurado estiver ocupado pela tela cheia (monitores em cache), aplica-se a linha de `FULLSCREEN_TARGETS_CHANGED` (DEC-020). |
| qualquer autônomo ou físico | `PRESS` sobre pixel opaco | `PRESSED` | Movimento autônomo congela no quadro atual. Vale também no meio de um pulo ou queda. |
| `PRESSED` | `DRAG_START` | `DRAGGING` | Plano autônomo descartado. |
| `PRESSED` | `CLICK` | `REACTING` | Reação curta. Depois, `SETTLING` decide o próximo estado. Se o monitor do personagem mudou ou sumiu enquanto o botão estava pressionado, a posição é validada já no `CLICK` (DEC-020). |
| `DRAGGING` | `DRAG_MOVE(p)` | `DRAGGING` | Posição = cursor menos o deslocamento da pegada. |
| `DRAGGING` | `DRAG_END` ou `DRAG_CANCEL` | `SETTLING` | Validação da seção 2.7. |
| `SETTLING` | com apoio | `IDLE` | Autonomia retomada depois de um intervalo de acomodação, exceto se o painel de energia estiver aberto. |
| `SETTLING` | com esconderijo marcado | `PEEKING` | Volta ao esconderijo na mesma borda, perto do lugar validado (DEC-025). |
| `SETTLING` | sem apoio, a mais de 32 DIP do chão, com o topo do sprite a até 96 DIP da borda de cima | `HANGING` agarrado ao cipó | Parado e sem relógio. Se foi o usuário que o soltou ali (`DRAG_END`, `DRAG_CANCEL`), ou se ele já estava preso, fica preso pelo usuário (DEC-024). |
| `SETTLING` | sem apoio, a mais de 32 DIP do chão, com a âncora a até 64 DIP de uma lateral | `CLIMBING` agarrado à parede | Idem, olhando para a parede. Com as duas bordas perto, vale a mais próxima em proporção ao alcance. |
| `SETTLING` | sem apoio | `FALLING` | Cai até o chão do monitor. |
| `PRESSED` (segundo clique), `IDLE`, `REACTING` | `DOUBLE_CLICK` ou menu "Energia" | `SETTLING` se vier de `PRESSED`; senão permanece | A partir da Fase 8, abre o painel compacto somente com o seletor de energia; pausa a autonomia enquanto estiver aberto. Antes da Fase 8, o segundo clique só produz reação não verbal. |
| qualquer estado visível com painel aberto | `ENERGY_SELECTED(nivel)` | permanece | Atualiza a mesma preferência persistida; o novo nível afeta as próximas decisões autônomas. |
| qualquer estado visível com painel aberto | `ENERGY_PANEL_CLOSE` | permanece | Fecha o painel e retoma a agenda após intervalo de acomodação. |
| `FALLING`, `LANDING` | contato com o chão | `LANDING`, depois `IDLE` | O painel não altera a física; se estiver aberto, a autonomia continua pausada. |
| `FALLING`, `JUMPING` | contato com o chão a 600 DIP/s ou mais, com a autonomia livre e menos de dois quiques seguidos | `JUMPING` (quique de borracha) | Toon force (DEC-023): volta a subir com metade da velocidade, rindo. Depois do segundo quique, ou com a autonomia pausada ou o painel aberto, vale a linha de contato com o chão. |
| `IDLE` | `AUTONOMY_TIMER` | `WALKING`, `CLIMBING`, `JUMPING`, `RESTING` ou permanece com um gesto curto | Escolha ponderada pela personalidade e pelo nível de energia, com semente. Só acontece com o painel de energia fechado e a autonomia não pausada. |
| `WALKING` | parede, passagem ou fim do chão | `IDLE`, `CLIMBING`, `FALLING` ou `WALKING` | Conforme a superfície (seção 2.5). |
| `CLIMBING` | topo da área útil, fim da parede ou `AUTONOMY_TIMER` | `IDLE`, `WALKING`, `JUMPING` ou `FALLING` | Ao chegar ao topo, para, anda pela borda, salta ou se solta. Soltar-se leva a `FALLING`. |
| `CLIMBING` | alcança borda superior apoiável | `HANGING` | Agarra a borda; a apresentação escolhe a pose correspondente. |
| `HANGING` | deslocamento autônomo, `AUTONOMY_TIMER` ou passagem compatível | `HANGING`, `CLIMBING`, `JUMPING` ou `FALLING` | Move-se apenas ao longo de uma borda alcançável; soltar-se leva a `FALLING`. |
| `RESTING` | `AUTONOMY_TIMER` | `IDLE` | Acorda e volta a decidir. O relógio só é religado neste momento. |
| `CLIMBING`, `HANGING` presos pelo usuário | `AUTONOMY_TIMER` | permanece | Fica e troca de cara, ou passeia de 40 a 220 DIP pela mesma superfície. Na parede, nunca chega ao chão nem passa para o cipó; no cipó, dá meia-volta nas quinas. Nunca salta, se solta nem desce ao chão (DEC-024). |
| `CLIMBING`, `HANGING` agarrados, sem estarem presos | `AUTONOMY_TIMER` | `CLIMBING`, `HANGING`, `JUMPING` ou `FALLING` | Volta a escalar para cima ou para baixo, segue pela borda, salta ou se solta. Isso acontece, por exemplo, depois da reação a um clique no meio de uma escalada (DEC-024). |
| `PRESSED`, `IDLE`, `REACTING`, `PEEKING` | `DOUBLE_CLICK` com o esconderijo ligado | `SETTLING`, depois `PEEKING` ou o estado da acomodação | Fora do esconderijo, esconde-se atrás da lateral mais próxima (se está no alto e junto dela) ou da borda de baixo. Escondido, sai: de pé no chão, ou grudado na parede e preso pelo usuário. O painel de energia não abre pelo clique duplo (DEC-025). |
| `PEEKING` | `AUTONOMY_TIMER` | permanece | Só troca a cara, espiando. |
| `PEEKING` | `PRESS`, `CLICK` | `PRESSED`, `REACTING`, depois `PEEKING` | Um clique faz a cabeça reagir e o deixa escondido; um arraste (`DRAG_START`) o tira do esconderijo. |
| qualquer estado visível, exceto `PRESSED`, `DRAGGING` e `EXITING` | `FULLSCREEN_TARGETS_CHANGED(monitoresOcupados)` | `SETTLING` no monitor livre, `HIDDEN(POR_TELA_CHEIA)` ou estado atual | Uma vez por mudança e só se a âncora estiver num monitor ocupado: transfere instantaneamente para um monitor livre sem ativar a janela; se todos estiverem ocupados, fecha o painel e oculta. Guarda a posição anterior só em memória, se ainda não houver uma guardada. |
| `PRESSED`, `DRAGGING` | `FULLSCREEN_TARGETS_CHANGED` | sem troca de estado | Só atualiza os monitores ocupados em cache. O gesto do usuário nunca é interrompido; ao soltar, `SETTLING` respeita o ponto escolhido pelo usuário, mesmo que seja um monitor ocupado, e descarta o retorno temporário. Um arraste (`DRAG_END` ou `DRAG_CANCEL` em `DRAGGING`) sempre descarta o retorno; um clique, clique duplo ou cancelamento em `PRESSED` só o descarta se a tela cheia mudou durante o gesto (DEC-020). |
| `HIDDEN(POR_TELA_CHEIA)` | `FULLSCREEN_TARGETS_CHANGED` com monitor livre | `SETTLING` | Reaparece no monitor livre; mantém o retorno temporário. |
| visível ou `HIDDEN(POR_TELA_CHEIA)` com retorno temporário guardado | `FULLSCREEN_TARGETS_CHANGED(vazio)` | `SETTLING` | Restaura a posição anterior validada pela seção 2.8 e limpa o retorno temporário. Se o retorno foi descartado por ação manual, o personagem fica onde o usuário o deixou. |
| `HIDDEN(POR_TELA_CHEIA)` | `CMD_SHOW` | `SETTLING` | O usuário pediu: aparece na posição anterior validada, descarta o retorno temporário e o modo não o oculta de novo até a próxima mudança de tela cheia. |
| `HIDDEN(POR_TELA_CHEIA)` | `CMD_HIDE` | `HIDDEN(POR_USUARIO)` | A ocultação passa a ser do usuário; o fim da tela cheia não o faz reaparecer. O retorno temporário vira a posição do personagem e é descartado (vale também para `SESSION_LOCKED` e `SUSPENDING`, que substituem `POR_TELA_CHEIA` pela precedência; DEC-020). |
| `HIDDEN` por outro motivo, com retorno temporário guardado | `FULLSCREEN_TARGETS_CHANGED(vazio)` | sem troca de estado | O episódio de tela cheia acabou com o personagem escondido pelo usuário, pela sessão ou pela suspensão: ele não reaparece, a posição de antes da tela cheia volta a valer e o retorno é descartado (DEC-020). |
| `HIDDEN` por outro motivo, durante um episódio de tela cheia | `CMD_SHOW` | `SETTLING` | Escolha manual: reaparece onde estava e o retorno temporário é descartado; o fim da tela cheia não o move mais (invariante 14; DEC-020). |
| qualquer, com retorno temporário guardado | `SETTINGS_CHANGED` que desliga o modo de tela cheia | `SETTLING`, sem troca de estado ou fim do gesto | Desligar o modo desfaz o efeito temporário (Q-09):<br>• visível ou `HIDDEN(POR_TELA_CHEIA)`: o personagem volta à posição anterior validada;<br>• escondido por outro motivo: não reaparece, mas a posição anterior volta a valer;<br>• `PRESSED` e `DRAGGING`: o gesto não é interrompido. Um arraste escolhe a posição; um clique, clique duplo ou cancelamento em `PRESSED` leva de volta à posição anterior; esconder no meio do gesto grava e guarda a posição anterior.<br>Com o modo desligado, o retorno não sobra fora de um gesto (DEC-020). |
| qualquer estado visível | `CMD_PAUSE_AUTONOMY`, `CMD_RESUME_AUTONOMY` | permanece | Liga ou desliga a dimensão "autonomia pausada"; retomar agenda a próxima decisão após o intervalo de acomodação. |
| `RESTING`, `CLIMBING` | `PRESS`, `DOUBLE_CLICK`, `CMD_*` | conforme a linha correspondente | Nenhum estado autônomo bloqueia interação do usuário (DEC-004). |
| `JUMPING`, `FALLING` | contato com o chão | `LANDING`, depois `IDLE` | Vale com o painel aberto ou fechado; o painel não altera a física. |
| estados autônomos, físicos e `REACTING` | `TOPOLOGY_CHANGED` | `SETTLING` | Revalida a posição. |
| `PRESSED`, `DRAGGING` | `TOPOLOGY_CHANGED` | sem troca de estado | Só atualiza a topologia em cache; a validação acontece ao soltar. |
| `BOOTING`, `HIDDEN`, `EXITING` | `TOPOLOGY_CHANGED` | sem troca de estado | Só atualiza a topologia em cache. `HIDDEN` valida ao reaparecer; `BOOTING` valida ao terminar de carregar. |
| qualquer, exceto `EXITING` | `CMD_HIDE` | `HIDDEN(POR_USUARIO)` | Fecha o painel, encerra captura e arraste, grava a posição escolhida pelo usuário (o retorno temporário, se houver; DEC-020). |
| qualquer, exceto `EXITING` | `SESSION_LOCKED` | `HIDDEN(POR_SESSAO)` | Idem. Em `HIDDEN`, segue a precedência dos motivos: o motivo do usuário é preservado, e o da sessão substitui a suspensão e a tela cheia. |
| qualquer, exceto `EXITING` | `SUSPENDING` | `HIDDEN(POR_SUSPENSAO)` | Idem, com a mesma precedência: preserva o usuário e a sessão bloqueada (com a sessão bloqueada, `RESUMED` não mostra o personagem) e substitui a tela cheia. |
| `HIDDEN(POR_USUARIO)` | `CMD_SHOW` | `SETTLING` | Só o usuário desfaz o que o usuário pediu. |
| `HIDDEN(POR_SESSAO)` | `SESSION_UNLOCKED` ou `CMD_SHOW` | `SETTLING` | Revalida a posição contra a topologia atual. No `SESSION_UNLOCKED`, se o monitor em que ele reaparece estiver ocupado pela tela cheia (monitores em cache), aplica-se a linha de `FULLSCREEN_TARGETS_CHANGED`; o `CMD_SHOW` é escolha do usuário e não a reaplica (DEC-020). |
| `HIDDEN(POR_SUSPENSAO)` | `RESUMED` ou `CMD_SHOW` | `SETTLING` | Idem, com `RESUMED` no lugar de `SESSION_UNLOCKED`. |
| `HIDDEN(POR_USUARIO)` | `SESSION_UNLOCKED`, `RESUMED` | `HIDDEN(POR_USUARIO)` | O personagem **não** reaparece: um evento do sistema não desfaz uma ação direta do usuário. |
| qualquer | `CMD_EXIT`, `SESSION_ENDING` | `EXITING` | Grava configurações e a posição escolhida pelo usuário (o retorno temporário, se houver) e encerra. |

Toda decisão autônoma agendada respeita o intervalo de acomodação, em qualquer estado que decide (DEC-020).

**Invariantes, verificáveis por teste automático**

1. Em `PRESSED`, `DRAGGING` e `SETTLING`, nenhum evento autônomo muda estado ou posição.
2. Em `DRAGGING`, a posição do personagem é sempre o cursor menos o deslocamento da pegada. O personagem não anda, não pula, não foge e não começa escalada.
3. O painel de energia não recebe nem interpreta texto; teclas locais só alteram seu controle quando ele está focado.
4. Nenhum comportamento autônomo começa enquanto o painel de energia está aberto.
5. Depois de `SETTLING`, a âncora está dentro da área útil de algum monitor presente.
6. A expressão pode mudar em qualquer estado sem alterar estado de comportamento ou posição.
7. Com a mesma semente e a mesma sequência de eventos, a sequência de retratos é idêntica.
8. Abrir ou fechar o painel de energia nunca muda a posição do personagem.
9. Iniciar um arraste fecha o painel de energia; ao soltar, nenhuma preferência de energia é alterada implicitamente.
10. `SESSION_UNLOCKED` e `RESUMED` nunca fazem o personagem reaparecer quando ele foi escondido pelo usuário.
11. Todo estado tem pelo menos uma transição de entrada e uma de saída, com duas exceções por construção: `BOOTING`, que só tem saída, e `EXITING`, que só tem entrada.
12. Um nível de energia mais alto pode aumentar frequência/duração de ações, mas nunca muda colisões, limites de superfície, segurança ou prioridade da ação direta.
13. O adaptador nunca envia identidade ou conteúdo de outra janela ao núcleo; o modo de tela cheia recebe apenas os monitores cobertos pela janela ativa.
14. Em `PRESSED` e `DRAGGING`, `FULLSCREEN_TARGETS_CHANGED` não muda estado nem posição. Depois de um arraste ou de `CMD_SHOW` manual durante o modo de tela cheia, o fim da tela cheia não move o personagem.
15. Um gesto curto nunca muda estado de comportamento, posição ou superfície, e termina ao chegar qualquer evento de prioridade maior.
16. A posição gravada (`GravarPosicao`) nunca é a posição temporária do modo de tela cheia (DEC-020).
17. Todo `AgendarDecisao` tem atraso maior ou igual ao intervalo de acomodação (DEC-020).

### 2.7 Arbitragem de input, clique, arraste e painel de energia

STATUS: PLANNED. Desenho aceito sob delegação do usuário em DEC-009; P3 validou a janela do personagem antes da Fase 1. P4 foi retirado porque o produto não terá chat, conversa nem entrada de texto. A arbitragem e o ciclo de arraste foram implementados na Fase 3 (`src/Buzzy.Core/Entrada/ArbitroDeGestos.cs`, adaptador em `JanelaPersonagem.cs`; detalhes de implementação em DEC-021); o estado de verificação de cada critério está em TODO.md.

**Quem recebe o input**

- O personagem só recebe cliques nos pixels visíveis. Pixels transparentes deixam o clique passar para a janela de baixo. Como fazer isso depende da stack (seção 2.13).
- A janela do personagem **não ativa**: clicar ou arrastar o Buzzy não tira o foco do aplicativo que o usuário está usando. P3 permanece gate técnico antes da Fase 1.
- O painel compacto de energia fica em uma janela própria, pequena e ativável, ancorada ao lado do personagem. Ele contém somente o seletor Baixa/Média/Alta e entra na Fase 8, junto com as configurações. Só o controle recebe teclado quando o painel está em foco.
- O Buzzy não instala hooks globais, não registra atalhos globais e não lê teclado fora das próprias janelas.

**Clique ou arraste**

1. `POINTER_DOWN` com o botão esquerdo sobre um pixel opaco gera `PRESS`. O adaptador captura o mouse para continuar recebendo movimento fora da janela. O núcleo entra em `PRESSED` e congela o movimento autônomo.
2. Se o cursor sai do retângulo de arraste do sistema, centrado no ponto de pressão, a arbitragem emite `DRAG_START`. O retângulo vem de `SM_CXDRAG` e `SM_CYDRAG`, lidos para o DPI do monitor. É a mesma regra que o Windows usa para detectar arraste.
3. Se o botão é solto dentro do retângulo, a arbitragem emite `CLICK`, não importa quanto tempo o botão ficou pressionado.
4. Um segundo `CLICK` dentro do tempo de clique duplo do sistema (`GetDoubleClickTime`) e dentro do retângulo de clique duplo (`SM_CXDOUBLECLK`, `SM_CYDOUBLECLK`) gera `DOUBLE_CLICK`. A reação ao primeiro clique acontece na hora, sem esperar o tempo do clique duplo.
5. Botão direito solto sobre o personagem gera `CONTEXT_MENU`.
6. `CAPTURE_LOST` durante o arraste, por Alt+Tab, janela de UAC ou outra captura, gera `DRAG_CANCEL`. O personagem fica onde estava e passa pela validação normal. Ele não volta ao ponto de origem.

Regras concretas implementadas na Fase 3 (DEC-021):

- O limiar vale para cada lado do ponto de pressão: o gesto vira arraste quando o cursor anda mais que `SM_CXDRAG` na horizontal ou mais que `SM_CYDRAG` na vertical. As métricas vêm do DPI atual da janela do personagem, que é o do monitor em que ele foi pressionado.
- O clique duplo segue a regra do sistema. O segundo botão pressionado precisa chegar antes do tempo de clique duplo, contado desde o primeiro pressionar. A distância precisa ser menor que a metade inteira do retângulo de clique duplo, nas métricas do primeiro clique. Um terceiro clique rápido começa uma sequência nova, e um arraste interrompe a sequência.
- Soltar fora do retângulo sem nenhum movimento intermediário (gesto muito rápido) gera `DRAG_START` e `DRAG_END` no ponto solto.
- Nada fica preso ao cursor. Três situações encerram o gesto com `DRAG_CANCEL`: um movimento sem o botão esquerdo logicamente pressionado (`MK_LBUTTON`), um novo botão pressionado sem o soltar anterior e `WM_CANCELMODE`. Com o ClickLock ligado, o Windows mantém o botão logicamente pressionado, e o gesto segue até o clique de liberação.
- O botão direito só abre o menu fora de um gesto do botão esquerdo. O laço modal do menu roda depois do processamento do núcleo, nunca dentro dele.

**Ciclo de arraste**

1. `PRESS` leva a `PRESSED` e o movimento autônomo congela.
2. `DRAG_START` leva a `DRAGGING`: o plano autônomo é descartado, pulo, queda e escalada são interrompidos e a agenda autônoma é suspensa.
3. A cada `DRAG_MOVE`, a janela vai para cursor menos o deslocamento da pegada. O próprio Windows agrupa os movimentos pendentes na fila, sempre com a posição mais recente; cada movimento vira posição da janela no mesmo tratamento da mensagem (latência M5 registrada pelo app em modo de diagnóstico). Não há física nem suavização durante o arraste.
4. `DRAG_END` leva a `SETTLING`. A validação calcula a âncora, escolhe o monitor que contém a âncora ou o mais próximo e prende a âncora na área útil desse monitor.
5. O personagem identifica a superfície sob os pés. Com apoio, vai para `IDLE`. Sem apoio, vai para `FALLING` até o chão do monitor. Soltar o personagem junto a uma parede não inicia escalada.
6. Depois de um intervalo de acomodação, a agenda autônoma volta a funcionar.

**Foco do painel de energia**

- O painel contém apenas o controle de energia; não há campo de texto nem evento de envio de conteúdo.
- O MVP não tem atalhos de teclado para controlar o personagem (Q-06). Nenhuma tecla move o personagem, com ou sem foco. A navegação por teclado e leitor de tela aplica-se ao controle e às configurações, conforme Q-20.
- Na Fase 8, o painel abre por clique duplo ou pelo menu e recebe foco como consequência dessa ação explícita do usuário.
- O painel fecha com Esc, pelo botão de fechar ou ao perder foco, de acordo com a implementação validada na Fase 8.

### 2.8 Monitores: restauração, conexão e desconexão

STATUS: PLANNED. Proposta registrada em DEC-008.

**Posição gravada:** chave do monitor, retângulo desse monitor na época, posição relativa da âncora dentro da área útil (frações de 0 a 1) e posição absoluta de reserva.

**Restauração ao iniciar**

1. Se o monitor da chave gravada existe, a posição relativa é aplicada à área útil atual dele. Isso cobre troca de resolução e de escala.
2. Se não existe, mas há um monitor com o mesmo retângulo, ele é usado.
3. Caso contrário, a posição relativa é aplicada ao monitor principal.
4. Em todos os casos, a âncora é presa à área útil e o personagem passa por `SETTLING`.
5. Se o monitor original voltar depois, o personagem não pula de volta sozinho. A posição só muda por ação do usuário ou por movimento autônomo.

**Durante a execução**

- O monitor onde o personagem está é desconectado: o personagem vai para o monitor mais próximo da última âncora, na mesma posição relativa, e cai até o chão.
- Troca de resolução, escala ou orientação: a posição relativa é mantida e o tamanho físico é recalculado.
- A barra de tarefas muda de lugar, de tamanho ou se esconde sozinha: as superfícies são recalculadas e o personagem se acomoda.
- Mudança durante o arraste: a topologia é atualizada, mas a validação só acontece ao soltar. O próprio Windows reposiciona o cursor se o monitor sumir.

**O que entra antes da Fase 5**

| Fundamento | Fase |
|---|---|
| Per-Monitor V2, coordenadas canônicas, enumeração de monitores, área útil, escala | Fase 1 |
| Monitor que contém um ponto, monitor mais próximo, prender o ponto na área útil | Fase 1 |
| Reler a topologia e prender a posição quando ela muda | Fase 1 |
| Arraste entre monitores e validação ao soltar | Fase 3 |
| Chave estável, restauração com alternativas, passagens entre monitores, troca de escala durante o movimento, cenários de conexão | Fase 5 |

### 2.9 Movimento

STATUS: PLANNED. O modelo físico detalhado segue as superfícies escolhidas em Q-05; detalhes de colisão e movimento ainda são especificados e validados nas fases correspondentes.

- **Corpo lógico:** retângulo em DIPs com âncora entre os pés. É independente do tamanho da imagem.
- **Passo fixo:** simulação com passo fixo e integração semi-implícita, sem depender da taxa de quadros. O valor inicial proposto é 1/60 s, a confirmar na Fase 4, quando o movimento existir e puder ser medido.
- **Caminhar:** velocidade constante sobre o chão. Diante de parede, vira ou escala. Diante de passagem, atravessa ou cai.
- **Escalar:** só em paredes, até o topo da área útil. De lá, desce, pula, se solta ou se pendura na borda superior.
- **Pendurar-se:** em `HANGING`, desloca-se pelas mãos ao longo de uma borda superior alcançável por tempo limitado; ao terminar, volta a escalar, salta dentro do alcance ou se solta e cai.
- **Saltar:** trajetória balística calculada para atingir um alvo. O alvo e a velocidade inicial são determinísticos, dada a semente.
- **Cair:** gravidade com velocidade máxima até tocar o chão.
- Velocidades e gravidade são definidas em DIPs por segundo e convertidas pela escala do monitor atual.

**Implementado na Fase 4 (DEC-022), num monitor:**

- **Passo fixo:** confirmado em 1/60 s (`ConfiguracaoDoNucleo.PassosPorSegundo`).
- **Estado:** o núcleo guarda a posição fina da âncora e a velocidade em pixels físicos (`EstadoDoMovimento`). A janela recebe a posição arredondada, e a posição relativa acompanha o movimento para sobreviver a uma mudança de topologia.
- **Velocidades e gravidade** (`ParametrosDeMovimento`), as mesmas em todos os níveis de energia:
  - caminhada 90 DIP/s, escalada 110 DIP/s, pendurado 80 DIP/s;
  - gravidade 2200 DIP/s², queda máxima 1500 DIP/s;
  - impulso ao saltar da parede: 220 DIP/s, com 40 DIP de altura.
- **Pulo:** a velocidade inicial vem da altura do arco (`vy0 = −√(2gH)`), e a velocidade horizontal cobre a distância sorteada no tempo de voo. No ar, a área útil limita as laterais e o topo.
- **Pausa ou painel aberto:** o movimento em curso termina num lugar estável. A caminhada para, a escalada desce, quem está pendurado se solta, e pulo, queda e pouso terminam.
- **Energia:** distâncias, alturas, tempos na parede e tempos pendurado são faixas dos perfis de energia (seção 2.11).
- **Toon force (DEC-023):**
  - **Quique de borracha:** um impacto de pelo menos 600 DIP/s quica. Volta a subir com metade da velocidade vertical, fica com 70% da horizontal e dá no máximo dois quiques; a contagem fica em `EstadoDoMovimento.Quiques`.
  - **Foguete de borracha:** parte das subidas a partir do chão (`EstadoDoMovimento.Foguete`) sobe a 1000 DIP/s até a borda superior. A chance é do perfil de energia (`ChanceDoFoguete`), e a velocidade é a mesma para todos.
  - **Com a autonomia pausada ou o painel aberto:** nem quique nem foguete.

### 2.10 Apresentação, assets e expressões

STATUS: PLANNED.

- O núcleo expõe um retrato do estado: estado de comportamento, direção, fase do movimento, expressão e sinais pontuais, como "pousou" ou "foi clicado".
- Um **manifesto de assets** em arquivo de dados liga cada estado a um clipe de animação e cada expressão a uma camada ou variante. O manifesto também define a âncora da imagem, o tamanho lógico e a taxa de quadros de cada clipe.
- **Trocar asset** significa trocar o manifesto e as imagens. O núcleo, o movimento, a arbitragem, o mundo do desktop e a segurança não mudam. Um teste automático roda a mesma suíte do núcleo com dois manifestos diferentes.
- Até a Fase 6, o app mostra quadros estáticos da pixel art, com o chapéu de palha (DEC-018 e DEC-019). Na Fase 4, `PoseDoPersonagem` escolhe uma pose provisória por estado: ciclo de caminhada e de escalada, pendurado, impulso, no ar, caindo, pousando, sentado ou dormindo, segurado, reagindo e os gestos, espelhada para a esquerda.
- **Toon force (DEC-023):** a pose também pode vir achatada (impacto) ou esticada (velocidade), pela dinâmica do movimento (`Dinamica`: velocidade vertical, quiques e foguete). A deformação é da própria pixel art (`Tela.Deformada`, vizinho mais próximo, pés na mesma linha), então a janela, a âncora e a regra do alfa não mudam.
- **Cipó (DEC-024):** na borda de cima, o personagem aparece pendurado num cipó (`cipo-1` a `cipo-3`). Balança em ciclo quando anda pela borda e fica no quadro do meio quando está agarrado. Agarrado à parede, a pose da escalada fica parada.
- **Esconderijo (DEC-025):** em `PEEKING`, e também em `PRESSED` e `REACTING` de quem continua escondido, aparece a pose `escondido`, só com o chapéu, a cabeça e as mãos na borda. Nas laterais, ela é girada 90° (`Tela.Girada`, sem perda).
- A apresentação só redesenha quando o quadro muda ou quando a posição exige. Um clipe de 10 quadros por segundo gera 10 redesenhos por segundo, não 60.
- A máscara de clique sai do canal alfa do quadro atual.

### 2.11 Personalidade ajustável e comportamento não verbal

STATUS: PLANNED.

**Personalidade.** O MVP expressa curiosidade, humor e energia por movimento, expressões faciais e gestos. Não há frases, diálogo, chat, texto digitado ou respostas. A intensidade é um valor local de três opções e influencia parâmetros determinísticos do comportamento:

- peso de cada comportamento autônomo, usado pela escolha da transição de `IDLE`;
- faixa de tempo entre decisões autônomas;
- tendência de expressão em cada estado;
- pesos relativos para espiar, explorar bordas, brincar, reagir a cliques e descansar.

O usuário informa que o conceito visual e o temperamento do Buzzy se inspiram em Luffy. A semelhança com o Luffy é intencional (DEC-019): o que ela significa para o produto fica em [PRODUCT_SPEC.md](PRODUCT_SPEC.md), seção Visão; aqui ela só orienta pesos e tempos de comportamento (impulsivo, otimista, curioso). A intensidade usa três níveis persistidos em configurações: `BAIXA`, `MEDIA` (padrão) e `ALTA` (Low/Mid/High na ideia do usuário). Baixa produz pausas maiores e menos ações; Média, um ritmo brincalhão equilibrado; Alta aumenta a frequência e a duração das brincadeiras e reações não verbais. O nível pode mudar pesos, intervalos e duração de ações, mas não muda a máquina física nem as regras de segurança. Pausa e comando direto do usuário sempre prevalecem.

Com a mesma semente, nível de energia e sequência de eventos, o núcleo determinístico produz as mesmas escolhas de comportamento. O MVP tem uma personalidade original; não prevê perfis ou conteúdo de diálogo. O seletor aparece no painel aberto por dois cliques e nas configurações, usando a mesma preferência persistida na Fase 8.

### 2.12 Configurações, persistência e tempo

STATUS: PLANNED. Decisões registradas em DEC-010 e DEC-011.

- **Arquivo:** um JSON com `schemaVersion` na pasta local do usuário. Sem pacote MSIX, a pasta é `%LOCALAPPDATA%\Buzzy`. Com MSIX, é a pasta local do pacote.
- **Conteúdo proposto:** última posição escolhida pelo usuário (seção 2.8), escala do personagem, sempre no topo, iniciar com o Windows, energia `BAIXA`/`MEDIA`/`ALTA` (padrão `MEDIA`), permitir atravessar monitores, modo de tela cheia ligado por padrão e idioma. Opacidade fica fora do MVP. As decisões de produto correspondentes estão em DEC-013/014 e Q-03 a Q-07, Q-09, Q-12 e Q-23; os campos entram nas fases previstas no TODO.md.
- **Leitura:** campo desconhecido é ignorado, valor fora da faixa é preso ao limite e arquivo ilegível é trocado pelos valores padrão. Uma cópia do arquivo ilegível é guardada para diagnóstico, no máximo uma.
- **Gravação:** escreve em um arquivo temporário e substitui o original de forma atômica, mantendo o último arquivo bom como `.bak`. A gravação acontece com atraso depois de soltar o personagem e sempre ao sair.
- **Tempo:** o relógio lógico só gera `TICK` enquanto há movimento, animação ou arraste. Em `IDLE` sem animação, em `RESTING` e em `HIDDEN`, não há timer periódico. A agenda autônoma usa um único timer até a próxima decisão. O modo de tela cheia é notificado por eventos limitados do Windows, sem polling global periódico.

### 2.13 Encaixe na stack recomendada

STATUS: PLANNED. WPF com C# e .NET 10 foi escolhida em DEC-006. P1 e P2 foram aceitos nos limites documentados; P3 ainda verifica arraste sem roubo de foco antes da Fase 1. Duas linhas da tabela da seção 2.13.3 dependem de outros protótipos: a identidade estável do monitor depende de P5, e a consulta de aplicativo em tela cheia depende de P7. Esta seção descreve WPF como janela e interface e limita chamadas Win32 diretas ao adaptador de plataforma.

#### 2.13.1 Janelas

| Janela | Tipo | Por quê |
|---|---|---|
| Personagem | `Window` WPF sem borda, do tamanho do sprite, com `AllowsTransparency`; a imagem tem alfa real e a janela não ativa | WPF usa o caminho layered para transparência por pixel. P1 confirma o click-through exato no Windows alvo |
| Painel de energia | Janela WPF própria, compacta e ativável, ancorada ao lado do personagem; contém somente três opções | O personagem não ativa; o painel recebe foco após ação explícita de dois cliques ou menu |
| Configurações | Janela WPF comum, aberta pelo menu | Usa controles e navegação de teclado do framework |
| Menu de contexto e bandeja | Menu nativo do Windows (`TrackPopupMenuEx`) com janela dona temporária; ícone de bandeja pelo adaptador, com `Shell_NotifyIcon` versão 4 (DEC-016) | Fecha ao clicar fora e devolve o foco mesmo aberto pelo personagem, que não ativa; teclado e leitor de tela prontos; sem dependência de terceiros |

A janela do personagem tem o tamanho do sprite, nunca o tamanho da tela. A documentação recomenda que a janela layered seja a menor possível, porque cada atualização copia o bitmap inteiro para a memória do sistema, e há relatos de atraso de mouse no sistema todo com overlay de tela cheia.

#### 2.13.2 Onde cada componente da seção 2.2 mora

| Componente | Realização na stack recomendada |
|---|---|
| Núcleo do personagem, mundo do desktop, arbitragem de input, movimento, personalidade não verbal e esquema de configurações | Biblioteca C# pura sem referência a WPF nem a APIs Windows. Recebe geometria e tempo como entrada e pode ser testada sem abrir janelas |
| Adaptador de plataforma | Único módulo que traduz eventos WPF e chama APIs Windows quando necessário: captura e foco do mouse, topologia, DPI, bandeja, sessão, energia e caminho dos dados |
| Apresentação | Janela e composição visual WPF, com imagem transparente dimensionada ao sprite; o desenho não fica ativo quando o estado não muda |
| Configurações e persistência | Serialização JSON versionada; gravação atômica num adaptador de armazenamento local |
| Raiz de composição | Inicialização WPF, ligação entre janelas, adaptador e núcleo; agenda trabalho apenas enquanto necessário |

#### 2.13.3 Correspondência com as APIs do Windows

Cada linha liga uma decisão das seções anteriores ao mecanismo que a realiza. WPF cuida das janelas e controles; chamadas diretas ao Windows ficam no adaptador e usam APIs documentadas pela Microsoft. Nenhuma exige elevação.

| Assunto | Seção | API |
|---|---|---|
| Transparência por pixel e clique que atravessa | 2.7 | `Window.AllowsTransparency` em janela sem borda WPF; framework cria a janela layered. P1 confirma que os pixels alfa 0 deixam o clique passar a outro processo |
| Modo fantasma, click-through total | Q-21 | Acrescentar o estilo transparente à janela layered. **Não faz parte do MVP**, conforme decisão do usuário; um personagem que não recebe clique poderia ficar inacessível |
| Não roubar foco | 2.7 | Responder "não ativar" à mensagem de ativação por mouse, com o estilo que evita ativação |
| Arraste | 2.7 | `SetCapture` ao pressionar, movimento do mouse, `ReleaseCapture` ao soltar, e a mensagem de mudança de captura como ponto único de término. Nunca o atalho que entrega a janela ao laço de mover do sistema, porque ele congela a física |
| Clique ou arraste | 2.7 | Retângulo de arraste do sistema, lido para o DPI do monitor, e o tempo de clique duplo do sistema |
| Coordenadas com sinal | 2.4 | Extrair as coordenadas das mensagens com as macros que preservam o sinal, nunca com as que tratam o valor como sem sinal |
| DPI por monitor | 2.4 | Declarar Per-Monitor V2 no manifesto, não por chamada de função, e tratar a mensagem de mudança de DPI aplicando o retângulo sugerido |
| Topologia | 2.4 e 2.8 | Enumerar monitores e ler informação de cada um, incluindo a área útil; mensagens de mudança de vídeo, de DPI e de mudança da área útil, com agrupamento de rajadas |
| Monitor que contém um ponto | 2.4, 2.7 e 2.8 | Consulta por ponto retornando nulo fora de qualquer monitor, para detectar o vão entre monitores, e a variante que retorna o mais próximo para prender a posição |
| Identidade estável do monitor | 2.4 | Consulta de configuração de vídeo e leitura do nome do dispositivo de destino, guardando o caminho do dispositivo. Nunca o identificador de execução, o identificador do adaptador nem o nome de vídeo, que não são estáveis |
| Bandeja | Q-03 | Notificação de ícone na versão 4, identificada por janela e número, com recriação quando a barra de tarefas reinicia |
| Sem botão na barra de tarefas | Q-03 | Estilo de janela de ferramenta |
| Fim de sessão | 2.6 | Responder sim de imediato à pergunta de encerramento e gravar na mensagem de encerramento, com gravação incremental antes |
| Bloqueio e desbloqueio de sessão | 2.6 | Registro de notificação de sessão da estação de trabalho, que entrega o bloqueio e o desbloqueio. Junto com a notificação de energia da linha seguinte, é um dos dois itens desta lista que exigem registro explícito; o resto chega nas mensagens comuns da janela |
| Suspensão e tela desligada | 2.6 | Notificação de suspensão e retomada, e notificação de estado da tela da sessão para parar de desenhar com o monitor desligado |
| Repouso | 2.12 | A fila de mensagens bloqueia quando não há nada a fazer. Animação com timer que o sistema pode agrupar, nunca elevando a resolução global do timer |
| Pasta de dados | 2.12 | Consulta de pasta conhecida, sem montar o caminho com texto |
| Mudança da janela em primeiro plano | DEC-013 | Eventos WinEvent de primeiro plano e mudança de geometria, fora do processo observado; filtrar a janela de nível superior ativa e emitir só os monitores que ela cobre. **Risco a medir em P7 (hipótese):** o evento de mudança de geometria assinado para todo o sistema também dispara por movimentos de cursor e de janelas de outros apps, o que pode acordar o Buzzy continuamente enquanto o usuário joga ou digita. Se P7 confirmar, restringir a assinatura de geometria à janela ativa (reassinando a cada troca de primeiro plano) ou revisar o desenho |
| Janela em tela cheia e monitor ocupado | Q-09 | `GetForegroundWindow`, `GetWindowRect`, `MonitorFromWindow`/`GetMonitorInfo`; comparar o retângulo ativo com os limites do monitor. `SHQueryUserNotificationState` pode ser sinal auxiliar, nunca a única fonte |

#### 2.13.4 Apresentação e repouso

WPF apresenta o sprite numa janela layered. O projeto não pressupõe que o framework seja barato em repouso: a aplicação deve suspender `CompositionTarget.Rendering`, animações e timers periódicos quando nada muda, e reativá-los apenas durante movimento, animação ou arraste. P2 mede CPU, memória, GPU e acordadas em repouso e em duas taxas de animação. Otimizações de desenho só são escolhidas com dados, na Fase 6.

**Implementado na Fase 4 (DEC-022):**

- **Inscrição:** a raiz só se inscreve em `CompositionTarget.Rendering` enquanto o núcleo pede o relógio (`LigarRelogio`) e cancela a inscrição no `DesligarRelogio`. Em repouso, inclusive em `RESTING`, não há inscrição.
- **Lote por quadro:** a cada quadro, o tempo decorrido vira passos fixos, que entram num lote na fila do núcleo. Só o último `MoverJanela` do lote é aplicado, e o sprite é trocado só quando a pose muda.
- **Atraso:** o acumulador recupera no máximo 250 ms; o excesso é descartado e registrado no log.
- **Log de diagnóstico:** a posição só é registrada quando o personagem para, fora do movimento e do arraste.

#### 2.13.5 Limites que os protótipos precisam esclarecer

- P1 confirma o clique por pixel em janela WPF e define regra de alfa para os assets.
- P2 confirma que o desenho e os timers param de acordar o processo em repouso e fornece a base para metas de desempenho.
- P3 valida o arraste sem ativar a janela e sem roubar foco. Use o receptor controlado do harness; input sintético é rotulado como tal. Se a única forma de arrastar alterar o foco, P3 falha; não adotar essa mudança sem decisão registrada.
- A conversão entre DIPs de WPF e pixels físicos do desktop fica no adaptador; P6 verifica a transição real entre monitores com escalas diferentes.
- O menu e a bandeja usam a API da Shell e o menu nativo do Windows (DEC-016). Não adicionar pacote de terceiros sem justificar, fixar versão e revisar a dependência.

#### 2.13.7 Regras que valem para qualquer stack aprovada

1. Nenhum overlay do tamanho da tela; a janela tem o tamanho do sprite.
2. Nenhum hook global de teclado ou mouse, nenhuma leitura de input em segundo plano e nenhuma captura de tela. A única exceção é o observador WinEvent de DEC-013, fora do processo, limitado às mudanças de primeiro plano/geometria; ele não lê conteúdo e entrega ao núcleo somente monitores ocupados.
3. Topologia em cache, atualizada por evento com agrupamento de rajadas, nunca por consulta periódica.
4. Física em unidades independentes de dispositivo, convertidas pela escala do monitor da âncora.
5. Estado de repouso explícito, sem timer de intervalo curto.
6. Persistência incremental e atômica na pasta local do usuário.
7. O asset não tem área grande de alfa baixo, porque só alfa exatamente 0 é transparente ao clique. Sombra, se houver, fica em janela separada sem interação.

### 2.14 Limites de complexidade

O MVP usa um processo, arquivos locais e módulos de código. Backend, banco de dados, microserviços, sistema de plugins, comunicação entre processos própria e interfaces genéricas de provedores ficam fora. IA no produto também fica fora conforme DEC-003; não há arquitetura de extensão para ela.

### 2.15 Sem IA no aplicativo

STATUS: PLANNED. O aplicativo é um mascote local, determinístico e sem IA. Não haverá conversa, chat, campo de texto, respostas escritas, voz nem reconhecimento de fala. Não integrar nem planejar LLM, RAG, APIs de IA, modelos locais, geração de conteúdo ou memória. Só reabrir o limite de IA por solicitação explícita do usuário (DEC-003).

## 3. Decisões ainda pendentes

As escolhas do usuário e as pendências ainda abertas estão numeradas (Q-01 em diante) em [DECISIONS.md](DECISIONS.md).

## 4. Histórico de mudanças arquiteturais

| Data | Mudança | Decisão relacionada |
|---|---|---|
| 2026-09-25 | Não havia arquitetura de software; foi criada a documentação inicial. | DEC-001 |
| 2026-09-26 | Registradas fronteiras candidatas e requisitos arquiteturais planejados, sem escolher stack. | DEC-002 a DEC-005 |
| 2026-09-26 | Proposta de arquitetura da Fase 0: componentes, fluxo, coordenadas, máquina de estados, arbitragem de input, multi-monitor, movimento, apresentação, conversa, persistência e tempo. Tudo STATUS: PLANNED. | DEC-006 a DEC-012 (propostas) |
| 2026-09-26 | WPF/.NET 10 escolhido; seção 2.13 atualizada para janelas, adaptador de APIs Windows, repouso e protótipos P1–P3. STATUS: PLANNED; ainda não executado. | DEC-006 |
| 2026-09-26 | Correções na máquina de estados depois de auditoria: caixa de texto virou dimensão ortogonal ao estado; mudança de topologia deixou de tirar o personagem de `HIDDEN`; `HIDDEN` passou a guardar o motivo do ocultamento; `CLIMBING` e `RESTING` ganharam transição de saída. Quatro invariantes novos. | DEC-004 |
| 2026-09-27 | Usuário aceitou DEC-007, DEC-008, DEC-010 e DEC-012 como desenho planejado e as metas Q-08. As decisões não indicam implementação. P1/P2 aceitos como gates limitados; P3 permanece em fechamento. | DEC-007 a DEC-012 |
| 2026-09-28 | Sob delegação explícita do usuário, Codex aceitou DEC-009 com validação em etapas (P3 antes da Fase 1; P4 antes da Fase 7) e DEC-011 com ociosidade por eventos. P3 e P4 continuam sendo verificações futuras, não gates já passados. | DEC-009, DEC-011 |
| 2026-09-28 | Usuário esclareceu o Buzzy como mascote sem IA, com circulação e escalada nas superfícies dos monitores; Codex definiu o modo automático por eventos de janela ativa (P7) e a energia Baixa/Média/Alta, padrão Média. Tudo planejado; P7 continua pendente. | DEC-013, DEC-014 |
| 2026-09-28 | O usuário esclareceu que quer um mascote de verdade, sem chat, texto ou respostas. A personalidade fica inteiramente em movimento, expressões e gestos. Dois cliques abrem somente o seletor de energia, implementado com as configurações na Fase 8; P4 de foco/IME foi aposentado. | DEC-003, DEC-009, DEC-014, Q-07, Q-23 |
| 2026-09-28 | Revisão documental de coerência (Claude): prioridade alinhada ao PRODUCT_SPEC; dimensões "gesto curto" e "autonomia pausada"; `DOUBLE_CLICK` a partir de `PRESSED`; modo de tela cheia não interrompe `PRESSED`/`DRAGGING`, e arraste ou `CMD_SHOW` manual descartam o retorno temporário; `CMD_SHOW`/`CMD_HIDE` em `HIDDEN(POR_TELA_CHEIA)`; `HANGING` no movimento; risco de frequência do evento de geometria levado a P7; invariantes 14 e 15. Tudo PLANNED. | DEC-004, DEC-013, DEC-014 |
| 2026-09-29 | Fase 1 implementada nas três camadas de DEC-007 (seção 1); menu de contexto nativo com dono temporário no lugar do menu WPF; renderizador vetorial da identidade em `src/Buzzy.Visual` para a Fase 6. Verificação dos critérios em TODO.md. | DEC-016, DEC-017 |
| 2026-09-29 | A pedido do usuário, a identidade passou a ser pixel art fiel às pranchas: quadro de 64 × 64 pixels mostrado em 128 DIP (ampliação inteira 2×/3×/4× em 100/150/200%), gerado por `src/Buzzy.Visual/Pixel/`. A direção vetorial foi arquivada. | DEC-018 |
| 2026-09-30 | Fase 2 ligada e auditada: a máquina de estados do núcleo passou a comandar a janela pela raiz de composição. A auditoria independente esclareceu a tabela: precedência dos motivos de ocultamento; retorno temporário da tela cheia (nunca gravado, descartado por escolha manual, sem sobrar para o episódio seguinte); só a primeira carga vale; `CLICK` valida se o monitor mudou; intervalo de acomodação em todo agendamento. Invariantes 16 e 17. Na raiz: menu adiado para fora do processamento, teto do relógio e lugar da janela reafirmado na releitura da topologia. | DEC-020 |
| 2026-09-30 | Fase 3: árbitro de gestos no núcleo puro (`Entrada/`) e captura do mouse no adaptador da janela do personagem, com as regras concretas da seção 2.7. | DEC-021 |
| 2026-09-30 | Linha nova da tabela 2.6: desligar o modo de tela cheia desfaz o efeito temporário, inclusive no fim de um gesto que não escolheu posição. | DEC-020 |
| 2026-09-30 | Fase 4: física de passo fixo no núcleo (andar, escalar, pendurar-se, pular, cair), superfícies do monitor da âncora, relógio pelos quadros do compositor e poses provisórias por estado (seções 2.5, 2.9 e 2.13.4). | DEC-022 |
| 2026-09-30 | Toon force, a pedido do usuário: toda lateral é escalável, quique e foguete de borracha, esticar e achatar nas poses (seções 2.5, 2.6, 2.9 e 2.10). | DEC-023 |
| 2026-09-30 | A pedido do usuário: cipó na borda de cima; agarrar onde é solto e ficar preso até o usuário tirar; estado `PEEKING` (esconderijo) pelo clique duplo, com o painel de energia passando para o menu (seções 2.6 e 2.10). | DEC-024, DEC-025 |

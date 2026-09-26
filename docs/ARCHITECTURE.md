# ARCHITECTURE.md — Arquitetura do Buzzy

> Esta página separa fatos implementados de desenho planejado. Nenhuma proposta aparece como arquitetura existente. Cada seção planejada carrega `STATUS: PLANNED`; escolhas que aguardam decisão do usuário carregam `STATUS: UNCERTAIN` e apontam para [DECISIONS.md](DECISIONS.md).
>
> Última atualização: 2026-09-26

## 1. Arquitetura implementada

Nenhuma. A inspeção dos arquivos em 2026-09-26 encontrou apenas documentação, um `.gitignore` e o repositório Git local. Não há código, dependência, build ou teste.

## 2. Arquitetura planejada

STATUS: PLANNED. Proposta da Fase 0, pendente de revisão do Codex e de decisão do usuário. A stack recomendada está em DEC-006 ([DECISIONS.md](DECISIONS.md)) e ainda é STATUS: UNCERTAIN. As seções 2.1 a 2.12 valem para qualquer stack; a seção 2.13 mostra como elas se encaixam na stack recomendada.

### 2.1 Princípios

- **Um processo, módulos separados por responsabilidade.** O Buzzy é um único executável. Os módulos são fronteiras de código, não processos, serviços ou plugins. A única exceção aceitável são processos auxiliares que a própria stack impõe, como o motor de um WebView.
- **Núcleo puro e determinístico.** Estado, eventos, arbitragem de input, movimento e conversa são código sem acesso ao Windows. Com a mesma semente e a mesma sequência de eventos, o núcleo produz a mesma sequência de estados. Isso permite testar quase todo o comportamento sem janela, sem monitor e sem hardware.
- **Um único adaptador toca o Windows.** Janela, input bruto, monitores, DPI, bandeja, ciclo de vida e arquivos passam pelo adaptador de plataforma. O resto do código não chama APIs do sistema.
- **Comportamento separado de aparência.** O núcleo emite estados e sinais lógicos. A apresentação decide quais quadros desenhar. Trocar a arte não altera o núcleo.
- **Ocioso por eventos.** Quando nada se move nem anima, não há timer periódico. O loop de simulação só roda enquanto há movimento, animação ou arraste.

### 2.2 Componentes

| Componente | Responsabilidade | Depende de | Nunca faz |
|---|---|---|---|
| Adaptador de plataforma | Cria e move as janelas, recebe input bruto, captura o mouse durante o arraste, enumera monitores, lê DPI, mantém o ícone da bandeja, trata sessão e energia, resolve a pasta de dados. Converte tudo para o sistema de coordenadas canônico. | Windows e a stack escolhida | Decidir comportamento; ler input fora das próprias janelas |
| Mundo do desktop | Modelo puro da topologia: monitores, áreas úteis, escala, orientação, monitor principal. Deriva superfícies (chão, paredes, passagens). Responde "em que monitor está este ponto" e "qual o ponto válido mais próximo". | Nada | Chamar APIs do sistema |
| Núcleo do personagem | Máquina de estados, fila de eventos, relógio lógico, agenda de comportamento autônomo com semente. Produz um retrato do estado e uma lista de efeitos. | Mundo do desktop, Movimento, Arbitragem de input, Conversa | Desenhar, gravar arquivo, mover janela diretamente |
| Arbitragem de input | Converte eventos de ponteiro em gestos (pressionar, clicar, iniciar arraste, arrastar, soltar, cancelar). Decide quem é o dono do input: personagem, caixa de texto ou menu. | Nada | Ler teclado global; interpretar tecla digitada como comando |
| Movimento | Cinemática com passo fixo: caminhar, escalar, saltar, cair, pousar. Colisão contra as superfícies do mundo do desktop. | Mundo do desktop | Iniciar ação por conta própria; a decisão é do núcleo |
| Apresentação | Recebe o retrato do estado e escolhe animação, quadro e expressão pelo manifesto de assets. Desenha a janela transparente e gera a máscara de clique. | Manifesto de assets, adaptador (superfície de desenho) | Alterar estado ou posição do personagem |
| Conversa local | Normaliza o texto digitado e escolhe uma resposta em uma tabela local de intenções, com semente. | Arquivo de conteúdo local, somente leitura | Rede, IA, execução de texto como código, gravação do que foi digitado |
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
                +--> Janela da conversa (mostra resposta, abre ou fecha a caixa)
```

1. O adaptador recebe uma mensagem do Windows e a converte em um evento normalizado, já em coordenadas canônicas.
2. A arbitragem transforma eventos de ponteiro em gestos e marca a prioridade de cada evento.
3. O núcleo aplica o evento ao estado atual pela tabela de transições e devolve o novo retrato e os efeitos.
4. A raiz de composição executa os efeitos: move a janela, pede um quadro à apresentação, agenda gravação de configurações ou atualiza a caixa de texto.
5. Enquanto houver movimento ou animação, o relógio lógico gera eventos de passo fixo. Quando o personagem para e a animação termina, o relógio para.

Prioridade de eventos, da maior para a menor, conforme [PRODUCT_SPEC.md](PRODUCT_SPEC.md): ação direta do usuário, input local, eventos do sistema, comportamento autônomo. Um evento de prioridade maior interrompe atividade de prioridade menor. Eventos autônomos que chegam durante um estado controlado pelo usuário são descartados, não enfileirados.

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

STATUS: PLANNED. O escopo de superfícies do MVP é a escolha em aberto Q-05 em [DECISIONS.md](DECISIONS.md).

Proposta para o MVP: as superfícies são derivadas apenas das áreas úteis dos monitores. Janelas de outros aplicativos não são superfícies.

- **Chão:** a borda inferior da área útil de cada monitor. Com a barra de tarefas embaixo, o chão é o topo da barra.
- **Parede:** trecho de uma borda lateral da área útil sem monitor vizinho encostado.
- **Passagem:** trecho de borda lateral encostado em outro monitor com sobreposição vertical. Se o chão vizinho está na mesma altura, dentro de uma tolerância, o personagem atravessa andando. Se está mais baixo, ele cai até o chão vizinho. Se está mais alto, o trecho funciona como parede.
- **Topo:** a borda superior da área útil limita a escalada. Contato apenas em diagonal, pelo canto, não cria passagem.
- O chão de um monitor continua sólido mesmo com outro monitor logo abaixo. O personagem só desce para o monitor de baixo quando o usuário o arrasta.
- As superfícies são recalculadas a cada `TOPOLOGY_CHANGED`.

### 2.6 Máquina de estados

STATUS: PLANNED. Estado de comportamento e expressão são dimensões independentes. Uma troca de expressão nunca muda o estado de comportamento.

**Estados de comportamento**

| Estado | Grupo | Significado |
|---|---|---|
| `BOOTING` | sistema | Carrega configurações e topologia e restaura a posição. |
| `IDLE` | autônomo | Parado sobre uma superfície. |
| `WALKING` | autônomo | Andando sobre o chão. |
| `CLIMBING` | autônomo | Subindo, descendo ou parado em uma parede. |
| `JUMPING` | autônomo | Em trajetória balística iniciada por decisão autônoma. |
| `FALLING` | físico | Sem apoio, sob gravidade. |
| `LANDING` | físico | Transição curta depois de tocar o chão. |
| `RESTING` | autônomo | Descansando ou dormindo, sem animação contínua. O relógio fica parado. |
| `PRESSED` | usuário | Botão pressionado sobre o personagem; ainda não se sabe se é clique ou arraste. |
| `DRAGGING` | usuário | Personagem preso ao cursor. |
| `SETTLING` | usuário | Validação logo depois de soltar ou depois de uma mudança de topologia. |
| `REACTING` | usuário | Reação curta a um clique. |
| `CONVERSING` | usuário | Caixa de texto aberta; autonomia suspensa. |
| `HIDDEN` | sistema | Escondido pela bandeja, sessão bloqueada ou suspensão. Sem relógio, sem desenho. |
| `EXITING` | sistema | Grava estado e encerra. |

**Eventos**

| Origem | Eventos |
|---|---|
| Ponteiro, somente sobre as janelas do Buzzy ou com captura ativa | `POINTER_DOWN(p, botão)`, `POINTER_MOVE(p)`, `POINTER_UP(p, botão)`, `CAPTURE_LOST` |
| Gestos derivados pela arbitragem | `PRESS`, `CLICK`, `DOUBLE_CLICK`, `DRAG_START`, `DRAG_MOVE(p)`, `DRAG_END(p)`, `DRAG_CANCEL`, `CONTEXT_MENU` |
| Caixa de texto | `TEXT_OPEN`, `TEXT_FOCUS_GAINED`, `TEXT_FOCUS_LOST`, `TEXT_SUBMIT(texto)`, `TEXT_CLOSE` |
| Sistema | `TOPOLOGY_CHANGED(topologia)`, `SESSION_LOCKED`, `SESSION_UNLOCKED`, `SUSPENDING`, `RESUMED`, `SESSION_ENDING` |
| Bandeja e menu | `CMD_HIDE`, `CMD_SHOW`, `CMD_PAUSE_AUTONOMY`, `CMD_RESUME_AUTONOMY`, `CMD_OPEN_SETTINGS`, `CMD_RESET_POSITION`, `CMD_EXIT` |
| Relógio | `TICK(dt)` com passo fixo, `AUTONOMY_TIMER` |
| Configurações | `SETTINGS_CHANGED(config)` |

**Transições principais**

| De | Evento | Para | Regra |
|---|---|---|---|
| `BOOTING` | configurações e topologia carregadas | `SETTLING` | Posição restaurada pela seção 2.8. |
| qualquer autônomo ou físico | `PRESS` sobre pixel opaco | `PRESSED` | Movimento autônomo congela no quadro atual. Vale também no meio de um pulo ou queda. |
| `PRESSED` | `DRAG_START` | `DRAGGING` | Plano autônomo descartado. |
| `PRESSED` | `CLICK` | `REACTING` | Reação curta. Depois, `SETTLING` decide o próximo estado. |
| `DRAGGING` | `DRAG_MOVE(p)` | `DRAGGING` | Posição = cursor menos o deslocamento da pegada. |
| `DRAGGING` | `DRAG_END` ou `DRAG_CANCEL` | `SETTLING` | Validação da seção 2.7. |
| `SETTLING` | com apoio | `IDLE` | Autonomia retomada depois de um intervalo de acomodação. |
| `SETTLING` | sem apoio | `FALLING` | Cai até o chão do monitor. |
| `IDLE`, `REACTING` | `DOUBLE_CLICK` ou menu "Conversar" | `CONVERSING` | Abre a caixa de texto e dá foco a ela. |
| `CONVERSING` | `TEXT_SUBMIT` | `CONVERSING` | Mostra a resposta local; a caixa continua aberta. |
| `CONVERSING` | `TEXT_CLOSE` | `IDLE` | Autonomia retomada. |
| `CONVERSING` | `PRESS` e depois `DRAG_START` | `DRAGGING` | A caixa acompanha o personagem e continua aberta; ao soltar, volta a `CONVERSING`. |
| `IDLE` | `AUTONOMY_TIMER` | `WALKING`, `CLIMBING`, `JUMPING` ou `RESTING` | Escolha ponderada pela personalidade, com semente. |
| `WALKING` | parede, passagem ou fim do chão | `IDLE`, `CLIMBING`, `FALLING` ou `WALKING` | Conforme a superfície (seção 2.5). |
| `JUMPING`, `FALLING` | contato com o chão | `LANDING`, depois `IDLE` | — |
| qualquer, exceto `DRAGGING` e `PRESSED` | `TOPOLOGY_CHANGED` | `SETTLING` | Em `DRAGGING` e `PRESSED`, só atualiza a topologia; a validação acontece ao soltar. |
| qualquer | `CMD_HIDE`, `SESSION_LOCKED`, `SUSPENDING` | `HIDDEN` | Encerra captura e arraste, grava a posição. |
| `HIDDEN` | `CMD_SHOW`, `SESSION_UNLOCKED`, `RESUMED` | `SETTLING` | Revalida a posição contra a topologia atual. |
| qualquer | `CMD_EXIT`, `SESSION_ENDING` | `EXITING` | Grava configurações e encerra. |

**Invariantes, verificáveis por teste automático**

1. Em `PRESSED`, `DRAGGING` e `SETTLING`, nenhum evento autônomo muda estado ou posição.
2. Em `DRAGGING`, a posição do personagem é sempre o cursor menos o deslocamento da pegada. O personagem não anda, não pula, não foge e não começa escalada.
3. Em `CONVERSING`, eventos de teclado nunca mudam estado de comportamento nem posição.
4. Nenhum estado autônomo começa enquanto a caixa de texto está aberta.
5. Depois de `SETTLING`, a âncora está dentro da área útil de algum monitor presente.
6. A expressão pode mudar em qualquer estado sem alterar estado de comportamento ou posição.
7. Com a mesma semente e a mesma sequência de eventos, a sequência de retratos é idêntica.

### 2.7 Arbitragem de input, clique, arraste e foco

STATUS: PLANNED. Proposta registrada em DEC-009.

**Quem recebe o input**

- O personagem só recebe cliques nos pixels visíveis. Pixels transparentes deixam o clique passar para a janela de baixo. Como fazer isso depende da stack (seção 2.13).
- A janela do personagem **não ativa**: clicar ou arrastar o Buzzy não tira o foco do aplicativo que o usuário está usando. STATUS: UNCERTAIN até o protótipo P3.
- A caixa de texto fica em uma janela própria, pequena e ativável, ancorada ao lado do personagem. Só ela recebe teclado.
- O Buzzy não instala hooks globais, não registra atalhos globais e não lê teclado fora das próprias janelas.

**Clique ou arraste**

1. `POINTER_DOWN` com o botão esquerdo sobre um pixel opaco gera `PRESS`. O adaptador captura o mouse para continuar recebendo movimento fora da janela. O núcleo entra em `PRESSED` e congela o movimento autônomo.
2. Se o cursor sai do retângulo de arraste do sistema, centrado no ponto de pressão, a arbitragem emite `DRAG_START`. O retângulo vem de `SM_CXDRAG` e `SM_CYDRAG`, lidos para o DPI do monitor. É a mesma regra que o Windows usa para detectar arraste.
3. Se o botão é solto dentro do retângulo, a arbitragem emite `CLICK`, não importa quanto tempo o botão ficou pressionado.
4. Um segundo `CLICK` dentro do tempo de clique duplo do sistema (`GetDoubleClickTime`) e dentro do retângulo de clique duplo (`SM_CXDOUBLECLK`, `SM_CYDOUBLECLK`) gera `DOUBLE_CLICK`. A reação ao primeiro clique acontece na hora, sem esperar o tempo do clique duplo.
5. Botão direito solto sobre o personagem gera `CONTEXT_MENU`.
6. `CAPTURE_LOST` durante o arraste, por Alt+Tab, janela de UAC ou outra captura, gera `DRAG_CANCEL`. O personagem fica onde estava e passa pela validação normal. Ele não volta ao ponto de origem.

**Ciclo de arraste**

1. `PRESS` leva a `PRESSED` e o movimento autônomo congela.
2. `DRAG_START` leva a `DRAGGING`: o plano autônomo é descartado, pulo, queda e escalada são interrompidos e a agenda autônoma é suspensa.
3. A cada `DRAG_MOVE`, a janela vai para cursor menos o deslocamento da pegada. Movimentos são agrupados por quadro, sempre com a posição mais recente. Não há física nem suavização durante o arraste.
4. `DRAG_END` leva a `SETTLING`. A validação calcula a âncora, escolhe o monitor que contém a âncora ou o mais próximo e prende a âncora na área útil desse monitor.
5. O personagem identifica a superfície sob os pés. Com apoio, vai para `IDLE`. Sem apoio, vai para `FALLING` até o chão do monitor. Soltar o personagem junto a uma parede não inicia escalada.
6. Depois de um intervalo de acomodação, a agenda autônoma volta a funcionar.

**Foco da caixa de texto**

- Com a caixa de texto focada, todo teclado vai para o campo de texto. O núcleo recebe apenas `TEXT_SUBMIT` e `TEXT_CLOSE`, nunca teclas soltas.
- O MVP proposto não tem comandos de movimento por teclado (Q-06). Então nenhuma tecla move o personagem, com ou sem foco. Se Q-06 aprovar atalhos, eles valem só com a janela do personagem focada e a caixa de texto fechada.
- A caixa abre por clique duplo ou pelo menu. Ela pede foco no momento da abertura, que é consequência direta de um clique do usuário.
- A caixa fecha com Esc, pelo botão de fechar ou depois de um tempo sem foco e sem digitação. O tempo é uma configuração a definir na Fase 7.
- A caixa aceita IME e acentos, porque é um campo de texto padrão da stack.

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

STATUS: PLANNED. O modelo físico detalhado depende da escolha em aberto Q-05.

- **Corpo lógico:** retângulo em DIPs com âncora entre os pés. É independente do tamanho da imagem.
- **Passo fixo:** simulação com passo fixo e integração semi-implícita, sem depender da taxa de quadros. O valor inicial proposto é 1/60 s, a calibrar no protótipo P4.
- **Caminhar:** velocidade constante sobre o chão. Diante de parede, vira ou escala. Diante de passagem, atravessa ou cai.
- **Escalar:** só em paredes, até o topo da área útil. De lá, desce, pula ou se solta.
- **Saltar:** trajetória balística calculada para atingir um alvo. O alvo e a velocidade inicial são determinísticos, dada a semente.
- **Cair:** gravidade com velocidade máxima até tocar o chão.
- Velocidades e gravidade são definidas em DIPs por segundo e convertidas pela escala do monitor atual.

### 2.10 Apresentação, assets e expressões

STATUS: PLANNED.

- O núcleo expõe um retrato do estado: estado de comportamento, direção, fase do movimento, expressão e sinais pontuais, como "pousou" ou "foi clicado".
- Um **manifesto de assets** em arquivo de dados liga cada estado a um clipe de animação e cada expressão a uma camada ou variante. O manifesto também define a âncora da imagem, o tamanho lógico e a taxa de quadros de cada clipe.
- **Trocar asset** significa trocar o manifesto e as imagens. O núcleo, o movimento, a arbitragem, o mundo do desktop e a segurança não mudam. Um teste automático roda a mesma suíte do núcleo com dois manifestos diferentes.
- O asset provisório é original e simples, como formas geométricas ou um esboço próprio. Ele não imita nenhum mascote existente.
- A apresentação só redesenha quando o quadro muda ou quando a posição exige. Um clipe de 10 quadros por segundo gera 10 redesenhos por segundo, não 60.
- A máscara de clique sai do canal alfa do quadro atual.

### 2.11 Conversa local

STATUS: PLANNED.

- A caixa de texto envia o texto para uma tabela local de intenções: palavras-chave e respostas por personalidade, em um arquivo de conteúdo que acompanha o aplicativo e é somente leitura.
- A escolha da resposta é determinística, dada a semente, com respostas de reserva quando nada combina.
- O texto tem limite de tamanho, é tratado só como dado, nunca é interpretado nem executado, e não é gravado em disco nem em log.
- Não há rede, API, LLM nem memória de conversa.

### 2.12 Configurações, persistência e tempo

STATUS: PLANNED. Propostas registradas em DEC-010 e DEC-011.

- **Arquivo:** um JSON com `schemaVersion` na pasta local do usuário. Sem pacote MSIX, a pasta é `%LOCALAPPDATA%\Buzzy`. Com MSIX, é a pasta local do pacote.
- **Conteúdo proposto:** posição (seção 2.8), escala do personagem, opacidade, sempre no topo, iniciar com o Windows, nível de autonomia, permitir atravessar monitores e idioma. Os campos que dependem das escolhas Q-03 a Q-07 só entram depois delas.
- **Leitura:** campo desconhecido é ignorado, valor fora da faixa é preso ao limite e arquivo ilegível é trocado pelos valores padrão. Uma cópia do arquivo ilegível é guardada para diagnóstico, no máximo uma.
- **Gravação:** escreve em um arquivo temporário e substitui o original de forma atômica, mantendo o último arquivo bom como `.bak`. A gravação acontece com atraso depois de soltar o personagem e sempre ao sair.
- **Tempo:** o relógio lógico só gera `TICK` enquanto há movimento, animação ou arraste. Em `IDLE` sem animação, em `RESTING` e em `HIDDEN`, não há timer periódico. A agenda autônoma usa um único timer até a próxima decisão.

### 2.13 Encaixe na stack recomendada

STATUS: UNCERTAIN. Depende da aprovação de DEC-006.

(Seção preenchida após a comparação de tecnologias.)

### 2.14 Limites de complexidade

O MVP usa um processo, arquivos locais e módulos de código. Backend, banco de dados, microserviços, sistema de plugins, comunicação entre processos própria e interface genérica de provedores de IA ficam fora até uma necessidade concreta do MVP aparecer e ser registrada em DECISIONS.md.

### 2.15 IA futura

STATUS: PLANNED. O MVP funciona sem LLM, RAG, API, memória de IA ou rede. Uma integração futura, se aprovada, será um adaptador opcional fora do núcleo determinístico. Desligar a integração ou perder acesso a ela não pode interromper o personagem. Nenhuma interface genérica de provedor é construída agora.

## 3. Decisões ainda pendentes

As escolhas em aberto estão numeradas (Q-01 em diante) na seção "Escolhas em aberto" de [DECISIONS.md](DECISIONS.md).

## 4. Histórico de mudanças arquiteturais

| Data | Mudança | Decisão relacionada |
|---|---|---|
| 2026-09-25 | Não havia arquitetura de software; foi criada a documentação inicial. | DEC-001 |
| 2026-09-26 | Registradas fronteiras candidatas e requisitos arquiteturais planejados, sem escolher stack. | DEC-002 a DEC-005 |
| 2026-09-26 | Proposta de arquitetura da Fase 0: componentes, fluxo, coordenadas, máquina de estados, arbitragem de input, multi-monitor, movimento, apresentação, conversa, persistência e tempo. Tudo STATUS: PLANNED. | DEC-006 a DEC-012 (propostas) |

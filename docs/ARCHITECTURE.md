# ARCHITECTURE.md — Arquitetura do Buzzy

> Desenho do Buzzy e o que dele já está no código. As seções numeradas são citadas pelo código: não renumere. O estado de verificação de cada critério está em [TODO.md](TODO.md), e as decisões, em [DECISIONS.md](DECISIONS.md); nada aqui é VERIFIED por estar escrito. As notas datadas de implementação e o histórico de mudanças até 2026-10-02 estão em [arquivo/arquitetura-historico.md](arquivo/arquitetura-historico.md); os desenhos originais da Fase 5 e do tamagotchi, que o código também cita, estão em [arquivo/desenho-fase5/](arquivo/desenho-fase5/) e [arquivo/desenho-tamagotchi/](arquivo/desenho-tamagotchi/).
>
> Última atualização: 2026-10-02.

## 1. Arquitetura implementada

Código das Fases 1 a 4, dos passos P1–P11 da Fase 5 e da emoção dominante e do tamagotchi (núcleo, arte e app) em `src/`, nas três camadas da DEC-007 (detalhes em DEC-016, DEC-020 a DEC-022 e DEC-027 a DEC-031). O código em `spikes/` é descartável e não é módulo do Buzzy.

| Projeto ou pasta | Camada | O que existe |
|---|---|---|
| `src/Buzzy.Core` | núcleo puro (`net10.0`, sem WPF nem Windows) | Geometria em pixels físicos e DIPs; `Topologia` (monitores, principal, impressão digital, monitor de um ponto, mais próximo, prender na área útil); `Posicionador` (âncora no centro da base, posição inicial, reacomodação, restauração em cascata e as posições que acompanham a topologia: `SoTranslacao`, `MonitorCorrespondente`, `Rebasear`, `AcompanharPonto`; seção 2.8). `Persistencia/`: esquema do `settings.json` e política de gravação (seção 2.12). `Personagem/`: a máquina de estados da seção 2.6 como função pura de (estado, evento) para (estado, efeitos), fila com prioridade, agenda autônoma com semente, perfis de energia, retrato e gravação/reprodução; a emoção dominante (DEC-027) em `Maquina.cs`, sem chave; `Movimento.cs`, as superfícies (seção 2.5) e a física de passo fixo (seção 2.9); o tamagotchi (DEC-028) em `Tamagotchi.cs` (tipos e a carga da paranoia), `TabelaDoTamagotchi.cs` (tabelas e classes), `Maquina.Onda.cs` (onda, alívio e paranoia) e `Maquina.Itens.cs` (itens, `USING` e o baseado por conta própria), atrás da chave `ConfiguracaoDoNucleo.Tamagotchi` (seção 2.16). `Entrada/`: o árbitro de gestos da seção 2.7. |
| `src/Buzzy.App/Plataforma` | adaptador de plataforma | O único arquivo com importações do Windows (`Win32.cs`); leitura da topologia com a chave estável de cada monitor (`LeitorDeTopologia`, `ConfiguracaoDeVideo`, `ChavesDeMonitor`; seção 2.4); janela de serviço oculta que recebe as mensagens de topologia, bandeja, `TaskbarCreated`, sessão WTS e energia; bandeja v4; menu nativo, com a lista de entradas separada da montagem e os submenus com ícones em bitmap (`MenuNativo`, `BitmapsDoMenu`; seção 2.16); instância única; log de diagnóstico opcional; pasta de dados, com os perfis de teste, e arquivo de configurações com gravação atômica (seção 2.12). |
| `src/Buzzy.App/Apresentacao` | apresentação e adaptador do ponteiro | Janela WPF do personagem (sem borda, `AllowsTransparency`, não ativa, janela de ferramenta, sempre no topo, do tamanho do sprite; responde `MA_NOACTIVATE` e `WM_GETDPISCALEDSIZE`), que converte o mouse em eventos de ponteiro em pixels físicos e segura a captura só durante um gesto começado nela. `PoseDoPersonagem` escolhe o quadro da pixel art pelo retrato (seção 2.10), renderizado uma vez por quadro e DPI num cache de 16 MiB (`CacheDeQuadros`). Tamagotchi: uma janela por item (`JanelaDoItem`), com a mesma receita, e o sprite do item (`SpriteDoItem`). |
| `src/Buzzy.App/Composicao` | raiz de composição | `Aplicacao` liga janela, serviço, bandeja, menu, topologia, arbitragem e núcleo e executa os efeitos do núcleo. `ArbitroDeEventosDoSistema` faz o desbloqueio e a retomada aguardarem a topologia publicada (DEC-031). O tamagotchi fica em `Aplicacao.Itens.cs` e `GerenteDosItens` (janelas dos itens, um árbitro de gestos só delas e o temporizador da onda; seção 2.3). A gravação fica em `AgendaDeGravacao`, e a releitura da topologia, em `AgendaDaReleitura`. Em repouso só há timers de disparo único: a releitura (com as novas tentativas e a conferência tardia), as novas tentativas da bandeja, a próxima decisão da agenda autônoma, a gravação com atraso e as novas tentativas dela e, com uma onda em curso, o próximo disparo dela. O relógio de passo fixo só corre quando o núcleo pede (reação, pouso, gesto curto, movimento, uso de um item e item visível caindo) e segue os quadros do compositor (`CompositionTarget.Rendering`), aplicando num lote os passos acumulados e movendo a janela uma vez por quadro. O arraste não usa relógio. |
| `src/Buzzy.Visual` | apresentação | Gerador da identidade em pixel art (`Pixel/`, DEC-018), com a arte da emoção dominante e do tamagotchi: itens, caras, poses de uso, sobreposições e ícones do menu (seção 2.10). O renderizador vetorial da DEC-017 está arquivado e sem uso. |

Fluxo implementado:

- **Configurações:** na partida, antes de tudo, a raiz lê o `settings.json` uma vez e entrega ao núcleo, na carga, a posição salva, a postura e as preferências; os efeitos de gravação do núcleo viram pedidos à agenda de gravação (seção 2.12).
- **Topologia:** o Windows avisa a janela de serviço, ou a do personagem no `WM_DPICHANGED`; a raiz agrupa as mensagens, com teto, e lê a topologia com as chaves estáveis; o núcleo acompanha ou revalida a posição (seção 2.8). Se a janela do personagem ou a de um item saiu do lugar do núcleo, a raiz reaplica esse lugar, na releitura e na conferência tardia, sem mexer na ordem Z.
- **Mouse:** a janela do personagem converte o mouse em eventos de ponteiro; a arbitragem produz gestos e diz se a captura continua; o núcleo aplica os gestos e devolve os efeitos.
- **Efeitos:** a raiz executa os efeitos do núcleo (mover, mostrar, esconder, relógio, agenda, menu adiado para fora do processamento, soltar a captura) no mesmo tratamento da mensagem.
- **Itens do tamagotchi:** cada janela de item converte o mouse em eventos de ponteiro; o árbitro dos itens produz os gestos, que viram eventos do item; o núcleo decide e devolve os efeitos das janelas dos itens e da onda (seções 2.3 e 2.7).

O núcleo já tem as regras da tabela para as capacidades das fases seguintes (escala mista, animação, painel de energia, tela cheia); o app liga cada uma quando a fase dela chega. O app liga todas as ações autônomas, a queda física, o movimento e a travessia entre monitores, com a ação de ir ao outro monitor (DEC-032; seção 2.5) e, com a chave do tamagotchi, a ação do baseado por conta própria; essas duas ficam fora de `AcoesAutonomas.Todas` (seção 2.16). O menu tem a chave "Conteúdo adulto" (DEC-033).

## 2. Arquitetura planejada

STATUS: PLANNED. As seções 2.1 a 2.12 e 2.16 descrevem o desenho do produto, e a 2.13, o encaixe no WPF (DEC-006). DEC-007 a DEC-014 definem o desenho; DEC-015, a autorização de execução. Os protótipos P1–P3 passaram; o P7 é gate antes da Fase 8. P4 foi aposentado, porque o produto não tem chat nem campo de texto.

### 2.1 Princípios

- **Um processo, módulos separados por responsabilidade.** O Buzzy é um único executável; os módulos são fronteiras de código, não processos, serviços ou plugins.
- **Núcleo puro e determinístico.** Estado, eventos, arbitragem de input, movimento, energia e personalidade não verbal são código sem acesso ao Windows. Com a mesma semente, energia e sequência de eventos, o núcleo produz a mesma sequência de estados, e quase todo o comportamento se testa sem janela, monitor ou hardware.
- **Um único adaptador toca o Windows.** Janela, input bruto, monitores, DPI, bandeja, ciclo de vida e arquivos passam pelo adaptador de plataforma; o resto do código não chama APIs do sistema.
- **Comportamento separado de aparência.** O núcleo emite estados e sinais lógicos; a apresentação decide os quadros. Trocar a arte não altera o núcleo.
- **Ocioso por eventos.** Sem movimento nem animação, não há timer periódico; o loop de simulação só roda com movimento, animação ou arraste. A detecção de tela cheia usa eventos WinEvent delimitados na DEC-013, sem polling periódico global.

### 2.2 Componentes

| Componente | Responsabilidade | Depende de | Nunca faz |
|---|---|---|---|
| Adaptador de plataforma | Cria e move as janelas, recebe input bruto, captura o mouse durante o arraste, enumera monitores, lê DPI, mantém o ícone da bandeja, trata sessão e energia, resolve a pasta de dados e emite eventos limitados da janela em primeiro plano para Q-09. Converte tudo para o sistema de coordenadas canônico. | Windows e a stack | Decidir comportamento; ler input fora das próprias janelas; ler título, texto, pixels ou identidade de outros apps |
| Mundo do desktop | Modelo puro da topologia: monitores, áreas úteis, escala, orientação, principal. Deriva superfícies (chão, paredes, passagens). Responde "em que monitor está este ponto" e "qual o ponto válido mais próximo". | Nada | Chamar APIs do sistema |
| Núcleo do personagem | Máquina de estados, fila de eventos, relógio lógico, agenda autônoma com semente. Produz um retrato do estado e uma lista de efeitos. | Mundo do desktop, Movimento, Arbitragem, Personalidade | Desenhar, gravar arquivo, mover janela diretamente |
| Arbitragem de input | Converte eventos de ponteiro em gestos (pressionar, clicar, iniciar arraste, arrastar, soltar, cancelar) e decide se o input é do personagem, do painel de energia ou do menu. | Nada | Ler teclado global; interpretar teclas como comandos de movimento |
| Movimento | Cinemática com passo fixo: caminhar, escalar, pendurar-se, saltar, cair, pousar; colisão contra as superfícies do mundo do desktop. | Mundo do desktop | Iniciar ação por conta própria; a decisão é do núcleo |
| Apresentação | Recebe o retrato do estado e escolhe animação, quadro e expressão pelo manifesto de assets; desenha a janela transparente e gera a máscara de clique. | Manifesto de assets, adaptador | Alterar estado ou posição do personagem |
| Personalidade | Pesos locais para a frequência das ações autônomas, a duração das pausas e a preferência por expressões e gestos curiosos, consultados pelo núcleo determinístico. | Perfil de comportamento local | Ler conteúdo de aplicativos; gerar ou armazenar conversa |
| Configurações e persistência | Esquema tipado com versão, padrões, validação, migração e gravação atômica em arquivo local. | Adaptador (pasta de dados) | Guardar texto digitado, segredos ou dados de outros aplicativos |
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

1. O adaptador recebe uma mensagem do Windows e a converte num evento normalizado, já em coordenadas canônicas.
2. A arbitragem transforma eventos de ponteiro em gestos e marca a prioridade de cada evento.
3. O núcleo aplica o evento ao estado pela tabela de transições e devolve o novo retrato e os efeitos.
4. A raiz de composição executa os efeitos: move a janela, pede um quadro à apresentação, agenda a gravação das configurações ou atualiza o painel de energia.
5. Enquanto houver movimento ou animação, o relógio lógico gera eventos de passo fixo; quando o personagem para e a animação termina, o relógio para.

Prioridade dos eventos, da maior para a menor ([PRODUCT_SPEC.md](PRODUCT_SPEC.md)): ação direta do usuário sobre o personagem (pressionar, arrastar); menu, bandeja, painel de energia e outras ações explícitas, incluindo ocultar e pausar; eventos do sistema, incluindo o modo de tela cheia; comportamento autônomo. Um evento de prioridade maior interrompe atividade de prioridade menor. Eventos autônomos que chegam durante um estado controlado pelo usuário são descartados, não enfileirados.

**Efeitos do tamagotchi (DEC-028), que só saem com a chave ligada.** O temporizador da onda tem dois: `AgendarOnda`, um disparo único de 1 s ou mais, que volta como `ITEM_EFFECT_TIMER` com a mesma geração, e `CancelarOnda`. As janelas dos itens têm cinco: `MostrarItem`, `MoverItem`, `EsconderItem`, `RemoverItem` (com o motivo: usado, recolhido ou substituído) e `LiberarCapturaDoItem`. Num evento, os efeitos saem nesta ordem:

1. os de antes: soltar capturas e fechar o painel;
2. a janela do personagem;
3. as janelas dos itens: primeiro os removidos e depois, item a item, na ordem do Id, esconder o que deixou de aparecer, mostrar o que passou a aparecer ou mover o que mudou de lugar;
4. o relógio;
5. a agenda;
6. a onda;
7. os de depois: gravações, menu, painel, configurações e encerrar.

**Na raiz (`GerenteDosItens` e `LigacaoDosItens`, em `Aplicacao.Itens.cs`):**

- `MostrarItem` cria a janela do item, já com o sprite no DPI do monitor, ou mostra de novo a escondida; `MoverItem` a leva ao lugar e só redesenha o sprite se o DPI mudou; `EsconderItem` esconde; `RemoverItem` fecha;
- `LiberarCapturaDoItem`: o árbitro dos itens esquece o gesto sem gerar `ITEM_RELEASE`, e a janela solta o mouse e volta para baixo do personagem; um soltar que chegue depois não vira nada;
- um `MoverItem` só é pulado quando há outro do mesmo Id adiante no lote, antes de um mostrar, esconder ou remover desse Id; nenhum outro efeito é pulado;
- mover, esconder, remover ou soltar a captura de um Id que a raiz não conhece é ignorado, e os três primeiros vão ao log com `desconhecido=sim`;
- `AgendarOnda` arma o temporizador da onda, um `DispatcherTimer` de disparo único que se desliga antes de avisar e entrega `ITEM_EFFECT_TIMER` com a geração agendada; `CancelarOnda` o desarma;
- no fim de cada processamento, só com `--diagnostico`, cada item que acabou de parar no chão ganha uma linha de pouso no log (seção 2.13.4);
- saindo (`EXITING`), o núcleo não emite efeito de janela de item: ao encerrar, a raiz para o temporizador da onda, esquece o gesto sobre um item e fecha todas as janelas dos itens.

Um efeito que a raiz não conhece lança exceção.

### 2.4 Coordenadas e desktop virtual

Decisão em DEC-008; chave, leitura e releitura implementadas nos passos P6 e P9 da Fase 5 (DEC-030), sessão e energia no P10 e P11 (DEC-031).

- O processo declara **Per-Monitor V2** desde a Fase 1: o Windows não virtualiza coordenadas nem estica a janela.
- **Sistema canônico:** pixels físicos do desktop virtual, com a origem (0,0) no canto superior esquerdo do monitor principal; monitores à esquerda ou acima dele têm coordenadas negativas.
- **Monitor:** chave estável, retângulo do monitor, retângulo da área útil (sem a barra de tarefas), escala (DPI / 96), orientação e se é o principal.
- **Chave estável do monitor** (`ChavesDeMonitor`, `ConfiguracaoDeVideo`), pelo caminho do dispositivo de `QueryDisplayConfig` e `DisplayConfigGetDeviceInfo`; o nome GDI (`\\.\DISPLAYn`), que pode mudar entre sessões, é só a reserva. A estabilidade continua a conferir no protótipo P5.
  - a chave é `mon:` seguido de 16 dígitos hexadecimais, os 8 primeiros bytes do SHA-256 do caminho, em maiúsculas: tamanho fixo, só ASCII, sem espaço, `;`, `,`, `|` nem `=`. O caminho só serve para calcular o resumo e nunca sai do adaptador;
  - a consulta lê só os caminhos ativos, com o nome GDI da fonte e o caminho do dispositivo do alvo; um alvo marcado como indisponível, de um monitor que acabou de sair, fica de fora. Num clone (uma fonte com vários alvos), vale o menor caminho em comparação ordinal;
  - quando a consulta falha ou não traz o caminho de um monitor, vale a chave da última consulta boa para o mesmo nome GDI com uma tela do mesmo tamanho, transladada ou não (a troca de principal e o rearranjo movem as telas sem trocar os monitores); sem ela, a reserva `gdi:` seguida do nome GDI. O DPI não conta;
  - primeiro são distribuídas as chaves lidas agora, depois as do cache e as reservas; nenhuma chave se repete. O cache só vive na execução, guarda também as chaves que continuaram por ele e só é trocado por uma leitura coerente, inteira e com a consulta boa;
  - o núcleo trata a chave como opaca e só a compara por igualdade.
- **Topologia:** a lista de monitores tem uma impressão digital (chaves, retângulos e escalas); uma impressão diferente é mudança de configuração. **Leitura incoerente:** uma falha ao ler a informação ou o DPI de um monitor, um DPI zero e um nome GDI ausente ou repetido tornam a leitura inteira incoerente; a anterior continua valendo, e a leitura é tentada de novo. Só como último recurso, depois das 5 leituras da partida e na última tentativa de uma rajada, vale a **leitura parcial**: o monitor que falha fica de fora, e o cache não muda.
- **Mudanças:** `WM_DISPLAYCHANGE`, `WM_DPICHANGED`, `WM_SETTINGCHANGE` com `SPI_SETWORKAREA` e a `TaskbarCreated` pedem uma nova leitura, e o adaptador agrupa as rajadas antes de publicar um único `TOPOLOGY_CHANGED` (`AgendaDaReleitura`): cada mensagem vai ao log só com o tipo; a releitura sai 300 ms depois da última mensagem, com teto de 1 s desde a primeira; uma leitura incoerente é tentada de novo em 500 ms, 1 s e 2 s; cada releitura publicada arma uma conferência tardia do lugar das janelas, 1,5 s depois. Os três tempos são provisórios até o protótipo P5. Regras na seção 2.8.
- **Sessão e energia (DEC-031):** a janela de serviço registra o próprio HWND para as notificações WTS da sessão atual. `WTS_SESSION_LOCK` e `PBT_APMSUSPEND` chegam ao núcleo na hora; `WTS_SESSION_UNLOCK` e `PBT_APMRESUMEAUTOMATIC` só depois de uma leitura coerente e publicada da topologia, e a retomada espera no mínimo 1,5 s antes de pedir a releitura (provisório até o P5). `PBT_APMRESUMESUSPEND` é consumido sem emitir uma segunda retomada. O log `MENSAGEM` vem antes do sinal ao árbitro e do pedido à agenda; bloqueio e suspensão cancelam o evento oposto ainda pendente. Depois de a `SUSPENDING` entrar no núcleo, a raiz descarrega a gravação pendente (seção 2.12).
- **Releitura imediata do mostrar:** mostrar por comando (bandeja, menu ou segunda instância) relê a topologia na hora, numa função própria da raiz, antes do `CMD_SHOW`, e registra a linha `TOPOLOGIA` com `imediata=sim`. Ela não passa pela agenda: não arma a conferência tardia, não reafirma o lugar, que o `CMD_SHOW` aplica, e não libera o desbloqueio nem a retomada retidos. Incoerente, a topologia anterior continua valendo, sem nova tentativa.
- **Minimização (passo P12; DEC-031, itens 7 e 8):** a janela do personagem minimizada volta ao normal na hora. Com uma releitura pendente, ou com a topologia de agora diferente da publicada ou incoerente, a minimização foi do sistema ("Minimizar janelas quando um monitor for desconectado"): o personagem não se esconde, e, sem releitura pendente, ela pede uma. Senão, foi do usuário, e minimizar esconde (Q-03). Já escondido, nunca vira `CMD_HIDE`; a raiz só garante a janela fora da vista. Linha `MINIMIZADO`.
- **Tamanho do personagem:** tamanho lógico em DIPs; o físico é o lógico vezes a escala do monitor da âncora (ponto entre os pés) vezes a escala escolhida pelo usuário. Ao cruzar para um monitor de outra escala, o tamanho físico muda no instante em que a âncora cruza a borda.
- **Nenhuma suposição de layout:** nada presume monitores lado a lado, alinhados pelo topo, com a mesma resolução ou com o principal à esquerda.

### 2.5 Superfícies

Q-05: no MVP, as superfícies vêm só das áreas úteis dos monitores; janelas de outros aplicativos não são superfícies, e o personagem circula pelas bordas alcançáveis, não pelo conteúdo aberto no desktop.

- **Chão:** a borda inferior da área útil de cada monitor; com a barra de tarefas embaixo, o topo da barra.
- **Paredes:** trechos de borda lateral sem monitor vizinho encostado, escaláveis para cima e para baixo (com a toon force, toda lateral é escalável; abaixo).
- **Passagens entre monitores:** trechos vizinhos com sobreposição e suporte geométrico compatível. O personagem atravessa andando quando a altura coincide e sobe ou desce por uma transição segura quando a diferença é alcançável; um vão sem superfície válida não vira salto automático ilimitado.
- **Bordas superiores:** o personagem se apoia pelas mãos, fica pendurado por pouco tempo e se desloca até uma passagem alcançável; soltar-se inicia `FALLING`.
- O chão de um monitor continua sólido mesmo com outro logo abaixo: a travessia para baixo só ocorre por caminho apoiado ou por salto dentro do alcance, nunca por queda espontânea.
- As superfícies são recalculadas a cada `TOPOLOGY_CHANGED`.

**Implementado na Fase 4 (DEC-022), num monitor.** `Superficies.Do` calcula, para o monitor da âncora e o tamanho do sprite nele: o chão (a base da área útil); o teto (o topo da área útil mais a altura do sprite); os limites laterais da âncora, com o sprite inteiro na área útil — num monitor mais estreito que o sprite, os dois limites ficam no meio, onde a validação o põe (`PrenderNaAreaUtil`); e se cada lateral é parede ou passagem (passagem quando outro monitor encosta nela com sobreposição vertical). As superfícies são recalculadas a cada passo, a partir da topologia em cache. **Toon force (DEC-023):** toda lateral é escalável, inclusive a passagem: ao chegar numa passagem, ele para, vira ou sobe por ela, como numa parede. A travessia é da Fase 5.

**Implementado no passo P13 da Fase 5 (DEC-032): a travessia.** As portas nascem da adjacência exata das áreas úteis (`Passagens.Portas`), na faixa vertical em que elas se sobrepõem; o resto da lateral é parede, e, com a toon force, toda lateral continua escalável. Há três travessias, todas com o sprite inteiro na união das áreas úteis (invariante 21):
- **plana** (o mesmo chão): andando, atômica — nada a para no meio, e a pausa e o item na mão esperam o fim; a âncora troca de monitor quando, arredondada, passa da borda, e o tamanho do sprite passa a ser o do novo DPI;
- **salto de degrau** (o chão do vizinho até 480 DIP abaixo ou 120 DIP acima, pela menor escala dos dois): arco balístico fechado, com a gravidade na escala da origem, escolhido por um solucionador determinístico entre tempos de voo (0,3 a 1,4 s), pousos (da lateral de entrada para dentro) e, quando planejado, uma partida até 2 larguras antes da lateral (seção 2.9);
- **transbordo** (o vizinho mais alto que o alcance): escalando a lateral do monitor mais baixo, quando os pés cruzam a altura do chão do vizinho, um arco curto até ele.

Na porta (ou, no transbordo, na altura do chão do vizinho), a agenda sorteia entre atravessar (`PesoAtravessar`) e o caminho de sempre da parede; a ação `IrAoOutroMonitor` atravessa sem o sorteio. Monitores empilhados não têm travessia vertical. A capacidade é `ConfiguracaoDoNucleo.Travessia`, e a escolha do usuário, `Preferencias.AtravessarMonitores` (ligada por padrão); desligada, a lateral é só parede.

### 2.6 Máquina de estados

Estado de comportamento e expressão são dimensões independentes: uma troca de expressão nunca muda o estado de comportamento.

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
| `HIDDEN` | sistema | Escondido, sem relógio nem desenho. Guarda **por que** foi escondido: `POR_USUARIO`, `POR_SESSAO`, `POR_SUSPENSAO` ou `POR_TELA_CHEIA`. |
| `EXITING` | sistema | Grava o estado e encerra. |
| `PEEKING` | autônomo | Escondido atrás da borda de baixo (a barra de tarefas) ou de uma lateral, só com a cabeça e as mãos para fora (DEC-025). Entra e sai pelo clique duplo. Sem relógio; a agenda só troca a cara. |
| `USING` | usuário | Usa o item que o usuário soltou sobre ele — come, bebe, fuma, cheira, engole ou inala, de desenho animado — por um número fixo de passos, no apoio em que estava (chão, parede, cipó ou esconderijo). Também é o estado do baseado que ele fuma por conta própria, que entra pela agenda, só no chão e sem item no mundo. O relógio corre, nada autônomo chega e um `PRESS` o segura na hora. Só existe com a chave do tamagotchi ligada (DEC-028). |

**Dimensões ligadas às escolhas do usuário (Fase 4):**

| Dimensão | Valores | Efeito |
|---|---|---|
| Preso pelo usuário | sim ou não | O usuário o soltou na lateral ou no cipó: lá fica até o usuário tirá-lo; a agenda só o faz passear pela mesma superfície (DEC-024). Gravado com a posição (DEC-029). |
| Esconderijo | nenhum, baixo, esquerda ou direita | A borda atrás da qual ele está escondido. Sobrevive ao primeiro clique do clique duplo, a `HIDDEN` e às revalidações (DEC-025). Gravado com a posição (DEC-029). |

**Dimensões da emoção dominante e do tamagotchi (DEC-027, DEC-028):**

| Dimensão | Valores | Efeito |
|---|---|---|
| Emoção dominante | automática ou uma das 14 caras de humor | Cara de base e a mais sorteada nas trocas de cara; nenhum efeito sobre estado, posição, ações ou física. Preferência gravada no `settings.json` (seção 2.12). |
| Onda do item | nenhuma; ou a da frente (tipo, fase e nível de 1 a 3) e até uma de fundo, congelada | Pesos, intervalos, gestos e caras; por exceção ao invariante 12, as velocidades de andar, escalar e pendurar e o cambaleio. Avança só pelos disparos únicos do próprio temporizador. A onda `Paranoico` vem do sorteio da paranoia e nunca fica no fundo. Só em memória (seção 2.16). |
| Carga da paranoia | nenhuma; ou o episódio: quantas substâncias, se alguma era droga sintética, os itens de substância distintos e se já sorteou (`CargaDaParanoia`) | Decide o sorteio único da paranoia no episódio (seção 2.16). Volta toda a zero no fim de todo evento sem onda de substância na frente nem no fundo. O retrato só mostra o número de substâncias. Só em memória. |
| Gerador da paranoia | um gerador próprio (`AleatorioDaParanoia`), semeado com semente × 41 + 13 | Só o sorteio da paranoia o usa, um passo por episódio de mistura com droga sintética; ela nunca usa o gerador principal. Não volta ao começo com o episódio. Só em memória. |
| Itens no mundo | até 6, cada um caindo, no chão, segurado ou arrastado | Só em memória. Com um item na mão do usuário, o personagem fica atento: parado onde está, sem decisão autônoma, até o item sair da mão. |

**Dimensões ortogonais**, que acompanham o personagem sem fazer parte do estado de comportamento:

| Dimensão | Valores | Efeito |
|---|---|---|
| Expressão | as 14 caras de humor e as 8 caras de efeito, que só a onda mostra (DEC-028) | Nenhum sobre comportamento ou posição |
| Gesto curto | nenhum, espiar, olhar ao redor, coçar-se, espreguiçar-se, brincar e os demais do manifesto; e os oito da onda (soluço, dança, gargalhada, espirro, tosse, tremedeira, olhar pro teto e agachar), que só ela sorteia, também só em `IDLE`. O olhar pro teto também vem, sem sorteio, no fim do uso que começou a paranoia | Ação visual de duração limitada na superfície atual (`IDLE`, `CLIMBING` parado ou `HANGING`); não muda estado, posição nem superfície; qualquer `PRESS`, `CMD_*` ou evento do sistema a encerra na hora |
| Autonomia pausada | sim ou não | Por `CMD_PAUSE_AUTONOMY`/`CMD_RESUME_AUTONOMY`. Enquanto sim, nenhum `AUTONOMY_TIMER` é agendado; queda ou pouso em curso terminam; arraste, clique, painel, ocultação e modo de tela cheia continuam. Com ela, ou com o painel aberto, quem está agarrado sem estar preso desce ou se solta (regra da calma, DEC-022, item 4), e o disparo da onda só troca a cara |
| Painel de energia | aberto ou fechado | Aberto, pausa a autonomia; o movimento físico em curso pode terminar. Arrastar o mascote fecha o painel. Só o seletor Baixa/Média/Alta, sem conversa nem campo de texto |
| Motivo do ocultamento | `POR_USUARIO`, `POR_SESSAO`, `POR_SUSPENSAO`, `POR_TELA_CHEIA` | Decide quais eventos tiram o personagem de `HIDDEN`; a tela cheia não desfaz uma ocultação do usuário. Precedência (DEC-020): `POR_USUARIO` > `POR_SESSAO` > `POR_SUSPENSAO` > `POR_TELA_CHEIA`; em `HIDDEN`, um motivo só substitui outro de precedência menor |
| Nível de energia | `BAIXA`, `MEDIA`, `ALTA` | Frequência e duração das ações autônomas e frequência das expressões; padrão `MEDIA` |
| Retorno temporário do modo de tela cheia | posição e chave do monitor, só em memória, ou vazio | Restaura a posição prévia sem substituir a persistida. É descartado quando o usuário arrasta o personagem ou o mostra manualmente durante o modo. A posição gravada ao esconder ou sair é sempre o retorno, se houver, nunca a temporária (DEC-020). Numa mudança de topologia, acompanha os monitores como a posição (seção 2.8) |

**Eventos**

| Origem | Eventos |
|---|---|
| Ponteiro, só sobre as janelas do Buzzy ou com captura ativa | `POINTER_DOWN(p, botão)`, `POINTER_MOVE(p)`, `POINTER_UP(p, botão)`, `CAPTURE_LOST` |
| Gestos derivados pela arbitragem | `PRESS`, `CLICK`, `DOUBLE_CLICK`, `DRAG_START`, `DRAG_MOVE(p)`, `DRAG_END(p)`, `DRAG_CANCEL`, `CONTEXT_MENU` |
| Painel de energia | `ENERGY_PANEL_OPEN`, `ENERGY_SELECTED(nivel)`, `ENERGY_PANEL_CLOSE`; `ENERGY_SELECTED` aceita só `BAIXA`, `MEDIA` ou `ALTA` |
| Sistema | `TOPOLOGY_CHANGED(topologia)`, `SESSION_LOCKED`, `SESSION_UNLOCKED`, `SUSPENDING`, `RESUMED`, `SESSION_ENDING` |
| Adaptador de janela ativa | `FULLSCREEN_TARGETS_CHANGED(monitoresOcupados)`; só chaves de monitores, sem HWND, processo, título ou texto |
| Bandeja e menu | `CMD_HIDE`, `CMD_SHOW`, `CMD_PAUSE_AUTONOMY`, `CMD_RESUME_AUTONOMY`, `CMD_OPEN_SETTINGS`, `CMD_RESET_POSITION`, `CMD_EXIT`; `CMD_SET_DOMINANT_EMOTION(emoção ou automática)` (DEC-027); `CMD_SUMMON_ITEM(item)` e `CMD_CLEAR_ITEMS` (DEC-028); `CMD_SET_ADULT_CONTENT(ligado)` (DEC-033) |
| Ponteiro sobre a janela de um item, por um árbitro de gestos próprio dela (DEC-028) | `ITEM_PRESS(id, p)`, `ITEM_DRAG_START(id)`, `ITEM_DRAG_MOVE(id, p)`, `ITEM_DRAG_END(id, p)` e `ITEM_RELEASE(id)`, este para clique, clique duplo ou gesto cancelado (captura perdida, movimento sem o botão ou outro botão pressionado sem soltar o anterior); o botão direito no item é o `CONTEXT_MENU` de sempre (seção 2.7) |
| Relógio | `TICK(dt)` com passo fixo, `AUTONOMY_TIMER`; `ITEM_EFFECT_TIMER(geração)`, o disparo da onda de um item, com a prioridade do relógio: não é descartado com o usuário no controle e não encerra um gesto (DEC-028) |
| Configurações | `SETTINGS_CHANGED(config)` |

**Transições principais**

| De | Evento | Para | Regra |
|---|---|---|---|
| `BOOTING` | configurações e topologia carregadas | `SETTLING` | Posição restaurada pela seção 2.8: com posição salva, `Posicionador.Restaurar` escolhe o monitor pela chave, pela tela do monitor da época ou o principal, e a regra termina em "posição salva restaurada pela chave", "pelo retângulo do monitor" ou "no monitor principal"; sem posição salva, vale a posição inicial e o texto não muda. O mesmo sufixo vale para "HIDDEN: pedido de mostrar anterior à carga" (DEC-030). Com posição salva, a carga traz a postura gravada (DEC-029): a acomodação o devolve escondido na mesma borda, se o esconderijo pelo clique duplo está ligado e a borda está na lista, ou agarrado e ainda preso; longe da parede e do cipó, a marca de preso se apaga. Só a primeira carga vale; pedidos anteriores a ela (mostrar, esconder, sessão, topologia) ficam guardados, e o personagem só aparece com a carga. Se o monitor restaurado estiver ocupado pela tela cheia (monitores em cache), aplica-se a linha de `FULLSCREEN_TARGETS_CHANGED` (DEC-020). |
| qualquer autônomo ou físico | `PRESS` sobre pixel opaco | `PRESSED` | O movimento autônomo congela no quadro atual, também no meio de um pulo ou queda. |
| `PRESSED` | `DRAG_START` | `DRAGGING` | Plano autônomo descartado. |
| `PRESSED` | `CLICK` | `REACTING` | Reação curta; depois, `SETTLING` decide o próximo estado. Se o monitor do personagem mudou ou sumiu com o botão pressionado, a posição é validada já no `CLICK`, a partir da posição que acompanhou a topologia (DEC-020, DEC-030); a mesma regra vale para toda saída de `PRESSED` (linha `PRESSED`, `DRAGGING` / `TOPOLOGY_CHANGED`). |
| `DRAGGING` | `DRAG_MOVE(p)` | `DRAGGING` | Posição = cursor menos o deslocamento da pegada. |
| `DRAGGING` | `DRAG_END` ou `DRAG_CANCEL` | `SETTLING` | Validação da seção 2.7. |
| `SETTLING` | com apoio | `IDLE` | Autonomia retomada depois de um intervalo de acomodação, salvo com o painel de energia aberto. |
| `SETTLING` | com esconderijo marcado | `PEEKING` | Volta ao esconderijo na mesma borda, perto do lugar validado (DEC-025). |
| `SETTLING` | sem apoio, a mais de 32 DIP do chão, com o topo do sprite a até 96 DIP da borda de cima | `HANGING` agarrado ao cipó | Parado e sem relógio. Se foi o usuário que o soltou ali (`DRAG_END`, `DRAG_CANCEL`), ou se ele já estava preso, fica preso pelo usuário (DEC-024). |
| `SETTLING` | sem apoio, a mais de 32 DIP do chão, com a âncora a até 64 DIP de uma lateral | `CLIMBING` agarrado à parede | Idem, olhando para a parede. Com as duas bordas perto, vale a mais próxima em proporção ao alcance. No fim do uso de um item na parede ou no cipó, vale o apoio do uso, mesmo a menos de 32 DIP do chão (DEC-028). |
| `SETTLING` | sem apoio | `FALLING` | Cai até o chão do monitor. |
| `PRESSED` (segundo clique), `IDLE`, `REACTING` | `DOUBLE_CLICK` (só com `EsconderijoNoCliqueDuplo` desligado, a regra antiga guardada para os testes) ou o menu "Energia" | `SETTLING` se vier de `PRESSED`; senão permanece | Na Fase 8, abre o painel compacto só com o seletor de energia e pausa a autonomia enquanto ele está aberto; antes dela, só reação não verbal. No app, o clique duplo esconde (linha do esconderijo abaixo) e o painel abre pelo menu (DEC-025). |
| qualquer estado visível com painel aberto | `ENERGY_SELECTED(nivel)` | permanece | Atualiza a mesma preferência persistida; o novo nível afeta as próximas decisões autônomas. |
| qualquer estado visível com painel aberto | `ENERGY_PANEL_CLOSE` | permanece | Fecha o painel e retoma a agenda depois do intervalo de acomodação. |
| `FALLING`, `LANDING` | contato com o chão | `LANDING`, depois `IDLE` | O painel não altera a física; se estiver aberto, a autonomia continua pausada. |
| `FALLING`, `JUMPING` | contato com o chão a 600 DIP/s ou mais, com a autonomia livre e menos de dois quiques seguidos | `JUMPING` (quique de borracha) | Toon force (DEC-023): volta a subir com metade da velocidade, rindo. Depois do segundo quique, ou pausado, ou com o painel aberto, vale a linha de contato com o chão. |
| `IDLE` | `AUTONOMY_TIMER` | `WALKING`, `CLIMBING`, `JUMPING`, `RESTING`, `USING` (o baseado por conta própria) ou permanece com um gesto curto | Escolha ponderada pela personalidade e pela energia, com semente, só com o painel fechado e a autonomia livre. Com a chave do tamagotchi ligada e a ação `FumarBaseado` na configuração, como no app, a última opção é fumar um baseado por conta própria, com o peso `PesoFumarBaseado`, só no chão, fora do esconderijo, sem item na mão do usuário e sem `Chapado` nem `Paranoico` na frente (senão, peso zero). O uso começa como o do baseado solto (linha `ITEM_DRAG_END`), no chão e sem item no mundo, com a regra "IDLE + AUTONOMY_TIMER: Fumar Baseado por conta própria" (seção 2.16). |
| `WALKING` | parede, passagem ou fim do chão | `IDLE`, `CLIMBING`, `FALLING` ou `WALKING` | Conforme a superfície (seção 2.5). |
| `WALKING` | porta plana, com a travessia ligada e sem calma (sorteada, ou a ação de ir ao outro monitor) | `WALKING` | Atravessa andando, de forma atômica, até o sprite estar inteiro no destino; depois, com calma, item na mão ou sem percurso, `IDLE` (DEC-032). |
| `WALKING` | degrau ao alcance, na porta ou na partida planejada | `JUMPING` | Salto de travessia; o contato com o chão do vizinho leva a `LANDING`. |
| `CLIMBING` | subindo, os pés cruzam a altura do chão de um vizinho mais alto | `JUMPING` | Transbordo para o chão do vizinho (sorteado, ou sem sorteio indo ao outro monitor). |
| `CLIMBING` | topo da área útil, fim da parede ou `AUTONOMY_TIMER` | `IDLE`, `WALKING`, `JUMPING` ou `FALLING` | No topo, para, anda pela borda, salta ou se solta; soltar-se leva a `FALLING`. |
| `CLIMBING` | alcança borda superior apoiável | `HANGING` | Agarra a borda; a apresentação escolhe a pose. |
| `HANGING` | deslocamento autônomo, `AUTONOMY_TIMER` ou passagem compatível | `HANGING`, `CLIMBING`, `JUMPING` ou `FALLING` | Só se move ao longo de uma borda alcançável; soltar-se leva a `FALLING`. |
| `RESTING` | `AUTONOMY_TIMER` | `IDLE` | Acorda e volta a decidir; o relógio só é religado aqui. |
| `CLIMBING`, `HANGING` presos pelo usuário | `AUTONOMY_TIMER` | permanece | Fica e troca de cara, ou passeia de 40 a 220 DIP pela mesma superfície: na parede, nunca chega ao chão nem passa para o cipó; no cipó, dá meia-volta nas quinas. Nunca salta, se solta nem desce ao chão (DEC-024). |
| `CLIMBING`, `HANGING` agarrados, sem estarem presos | `AUTONOMY_TIMER` | `CLIMBING`, `HANGING`, `JUMPING` ou `FALLING` | Volta a escalar, segue pela borda, salta ou se solta — por exemplo, depois da reação a um clique no meio de uma escalada (DEC-024). |
| `PRESSED`, `IDLE`, `REACTING`, `PEEKING` | `DOUBLE_CLICK` com o esconderijo ligado | `SETTLING`, depois `PEEKING` ou o estado da acomodação | Fora do esconderijo, esconde-se atrás da lateral mais próxima (se está no alto e junto dela) ou da borda de baixo. Escondido, sai: de pé no chão, ou grudado na parede e preso pelo usuário. O painel de energia não abre pelo clique duplo (DEC-025). |
| `PEEKING` | `AUTONOMY_TIMER` | permanece | Só troca a cara, espiando. |
| `PEEKING` | `PRESS`, `CLICK` | `PRESSED`, `REACTING`, depois `PEEKING` | Um clique faz a cabeça reagir e o deixa escondido; um arraste (`DRAG_START`) o tira do esconderijo. |
| qualquer estado visível, exceto `PRESSED`, `DRAGGING` e `EXITING` | `FULLSCREEN_TARGETS_CHANGED(monitoresOcupados)` | `SETTLING` no monitor livre, `HIDDEN(POR_TELA_CHEIA)` ou estado atual | Uma vez por mudança e só se a âncora estiver num monitor ocupado: transfere na hora para um monitor livre sem ativar a janela; com todos ocupados, fecha o painel e oculta. Guarda a posição anterior só em memória, se ainda não houver uma guardada. |
| `PRESSED`, `DRAGGING` | `FULLSCREEN_TARGETS_CHANGED` | sem troca de estado | Só atualiza os monitores ocupados em cache. O gesto nunca é interrompido; ao soltar, `SETTLING` respeita o ponto escolhido, mesmo num monitor ocupado, e descarta o retorno temporário. Um arraste (`DRAG_END` ou `DRAG_CANCEL` em `DRAGGING`) sempre descarta o retorno; um clique, clique duplo ou cancelamento em `PRESSED` só o descarta se a tela cheia mudou durante o gesto (DEC-020). |
| `HIDDEN(POR_TELA_CHEIA)` | `FULLSCREEN_TARGETS_CHANGED` com monitor livre | `SETTLING` | Reaparece no monitor livre; mantém o retorno temporário. |
| visível ou `HIDDEN(POR_TELA_CHEIA)` com retorno temporário guardado | `FULLSCREEN_TARGETS_CHANGED(vazio)` | `SETTLING` | Restaura a posição anterior validada pela seção 2.8 e limpa o retorno. Se o retorno foi descartado por ação manual, ele fica onde o usuário o deixou. |
| `HIDDEN(POR_TELA_CHEIA)` | `CMD_SHOW` | `SETTLING` | O usuário pediu: aparece na posição anterior validada, descarta o retorno, e o modo não o oculta de novo até a próxima mudança de tela cheia. |
| `HIDDEN(POR_TELA_CHEIA)` | `CMD_HIDE` | `HIDDEN(POR_USUARIO)` | A ocultação passa a ser do usuário; o fim da tela cheia não o faz reaparecer. O retorno vira a posição do personagem e é descartado (também com `SESSION_LOCKED` e `SUSPENDING`, que substituem `POR_TELA_CHEIA` pela precedência; DEC-020). |
| `HIDDEN` por outro motivo, com retorno temporário guardado | `FULLSCREEN_TARGETS_CHANGED(vazio)` | sem troca de estado | O episódio acabou com ele escondido pelo usuário, pela sessão ou pela suspensão: não reaparece, a posição de antes da tela cheia volta a valer e o retorno é descartado (DEC-020). |
| `HIDDEN` por outro motivo, durante um episódio de tela cheia | `CMD_SHOW` | `SETTLING` | Escolha manual: reaparece onde estava e o retorno é descartado; o fim da tela cheia não o move mais (invariante 14; DEC-020). |
| qualquer, com retorno temporário guardado | `SETTINGS_CHANGED` que desliga o modo de tela cheia | `SETTLING`, sem troca de estado ou fim do gesto | Desligar o modo desfaz o efeito temporário (Q-09):<br>• visível ou `HIDDEN(POR_TELA_CHEIA)`: volta à posição anterior validada;<br>• escondido por outro motivo: não reaparece, mas a posição anterior volta a valer;<br>• `PRESSED` e `DRAGGING`: o gesto não é interrompido; um arraste escolhe a posição; um clique, clique duplo ou cancelamento em `PRESSED` leva de volta à posição anterior; esconder no meio do gesto grava e guarda a posição anterior.<br>Com o modo desligado, o retorno não sobra fora de um gesto (DEC-020). |
| qualquer estado visível | `CMD_PAUSE_AUTONOMY`, `CMD_RESUME_AUTONOMY` | permanece | Liga ou desliga a autonomia pausada; retomar agenda a próxima decisão depois do intervalo de acomodação. |
| `RESTING`, `CLIMBING` | `PRESS`, `DOUBLE_CLICK`, `CMD_*` | conforme a linha correspondente | Nenhum estado autônomo bloqueia a interação do usuário (DEC-004). |
| `JUMPING`, `FALLING` | contato com o chão | `LANDING`, depois `IDLE` | Com o painel aberto ou fechado; o painel não altera a física. |
| estados autônomos, físicos, `REACTING` e `USING` | `TOPOLOGY_CHANGED` sem mudar a geometria do monitor do personagem: só outros monitores mudaram, ou o dele foi só transladado no desktop virtual (tela e área útil deslocadas pelo mesmo (dx, dy), com o mesmo DPI) ou só trocou de chave | permanece, com uma transição para o mesmo estado | O que está em curso continua: a caminhada, a escalada, o pulo, a reação, o uso, o esconderijo e o preso. A âncora, a janela e, nos estados de movimento, a posição fina andam pela translação; a posição passa a descrever o lugar novo; o relógio e a agenda não mudam. A regra diz que o monitor "não mudou", "mudou de chave" ou "foi transladado (dx,dy)", sem a chave (DEC-030; invariante 19). |
| estados autônomos, físicos, `REACTING` e `USING` | `TOPOLOGY_CHANGED` com a geometria do monitor do personagem mudada (resolução, escala, orientação ou área útil) | `SETTLING` | Revalida na mesma posição relativa da área útil atual dele, com a regra "TOPOLOGY_CHANGED". Em `USING`, o uso acaba, e a onda continua (DEC-028). |
| estados autônomos, físicos, `REACTING` e `USING` | `TOPOLOGY_CHANGED` sem o monitor do personagem | `SETTLING` | Vai ao sobrevivente mais próximo do pixel dos pés medido nas coordenadas antigas, na mesma posição relativa (seção 2.8), com a regra "TOPOLOGY_CHANGED: o monitor do personagem foi desconectado". Em `USING`, o uso acaba, e a onda continua. |
| `PRESSED`, `DRAGGING` | `TOPOLOGY_CHANGED` | sem troca de estado | Nada é validado no gesto; a posição guardada e o retorno da tela cheia acompanham a topologia (seção 2.8). Em `PRESSED`, toda saída (`CLICK`, `DOUBLE_CLICK`, `DRAG_CANCEL` e `DRAG_START`) parte de onde ele estaria parado: se o monitor dele mudou ou sumiu, do lugar que a posição acompanhada descreve, já validado. Em `DRAGGING`, o lugar do arraste anda com o monitor em que está, e a janela vai junto, como o cursor; a validação acontece ao soltar (DEC-030). |
| `BOOTING`, `HIDDEN`, `EXITING` | `TOPOLOGY_CHANGED` | sem troca de estado | Em `BOOTING`, só atualiza a topologia em cache; a validação vem ao terminar de carregar. Em `HIDDEN`, a posição guardada e o retorno também acompanham a topologia, sem mover a janela (seção 2.8), e a validação vem ao reaparecer. Em `EXITING`, nada. |
| qualquer, exceto `EXITING` | `CMD_HIDE` | `HIDDEN(POR_USUARIO)` | Fecha o painel, encerra captura e arraste e grava a posição escolhida pelo usuário (o retorno temporário, se houver; DEC-020). |
| qualquer, exceto `EXITING` | `SESSION_LOCKED` | `HIDDEN(POR_SESSAO)` | Idem. Em `HIDDEN`, segue a precedência: o motivo do usuário é preservado, e o da sessão substitui a suspensão e a tela cheia. |
| qualquer, exceto `EXITING` | `SUSPENDING` | `HIDDEN(POR_SUSPENSAO)` | Idem, com a mesma precedência: preserva o usuário e a sessão bloqueada (com a sessão bloqueada, `RESUMED` não mostra o personagem) e substitui a tela cheia. |
| `HIDDEN(POR_USUARIO)` | `CMD_SHOW` | `SETTLING` | Só o usuário desfaz o que o usuário pediu. |
| `HIDDEN(POR_SESSAO)` | `SESSION_UNLOCKED` ou `CMD_SHOW` | `SETTLING` | Revalida a posição contra a topologia atual. No `SESSION_UNLOCKED`, se o monitor em que ele reaparece estiver ocupado pela tela cheia (em cache), aplica-se a linha de `FULLSCREEN_TARGETS_CHANGED`; o `CMD_SHOW` é escolha do usuário e não a reaplica (DEC-020). |
| `HIDDEN(POR_SUSPENSAO)` | `RESUMED` ou `CMD_SHOW` | `SETTLING` | Idem, com `RESUMED` no lugar de `SESSION_UNLOCKED`. |
| `HIDDEN(POR_USUARIO)` | `SESSION_UNLOCKED`, `RESUMED` | `HIDDEN(POR_USUARIO)` | O personagem **não** reaparece: evento do sistema não desfaz ação direta do usuário. |
| qualquer | `CMD_EXIT`, `SESSION_ENDING` | `EXITING` | Grava as configurações e a posição escolhida pelo usuário (o retorno temporário, se houver) e encerra. |
| `CLIMBING`, `HANGING` agarrados sem estarem presos, com a autonomia pausada ou o painel aberto | fim de qualquer evento | permanece; os passos seguintes o fazem descer ou se soltar | Regra da calma (DEC-022, item 4): deixa de estar agarrado. O preso fica; com um item na mão do usuário, o atento o segura até o item sair da mão. |
| qualquer carregado, exceto `EXITING` | `CMD_SET_ADULT_CONTENT(ligado)` | permanece, com uma transição para o mesmo estado que registra a escolha | Grava a preferência. Desligar tira do mundo os itens adultos (o da mão solta a captura antes), acaba as ondas de substância, zera a carga do episódio e termina o uso de um item adulto (`SETTLING`); ligar só grava. Igual à atual ou antes da carga, é ignorado (DEC-033). |
| qualquer carregado, exceto `EXITING` | `CMD_SET_DOMINANT_EMOTION(e)` | permanece, com uma transição para o mesmo estado que só registra a escolha | Grava a preferência. Sem onda e fora de `RESTING`, `REACTING` e `USING`, a cara muda na hora. Fora das 14 caras de humor ou igual à atual, é ignorado (DEC-027). |
| qualquer visível e carregado | `CMD_SUMMON_ITEM(item)` | permanece | O item nasce ao lado dele, acima do chão, e cai (seção 2.16). Com 6 itens, o mais antigo fora da mão sai. Parado e sem onda, ele fica empolgado. Escondido, antes da carga ou com um item fora do enum, nada acontece. |
| qualquer | `CMD_CLEAR_ITEMS` | permanece | Todos os itens saem; o da mão do usuário solta a captura antes. |
| qualquer visível, sobre um item visível | `ITEM_PRESS(id, p)` | `WALKING` e `RESTING` vão a `IDLE`; os outros permanecem | Atento: andando, para; descansando, acorda; na parede e no cipó, fica agarrado, e o foguete apaga; pulo e queda seguem até o chão. Nenhuma decisão autônoma até o item sair da mão. Outro item que estivesse na mão é largado antes, com a captura solta. |
| qualquer | `ITEM_DRAG_MOVE(id, p)` do item arrastado | permanece | A âncora do item é o cursor menos a pegada, sem prender, como no invariante 2. |
| `IDLE`, `WALKING`, `CLIMBING`, `HANGING`, `RESTING`, `REACTING`, `LANDING`, `PEEKING` | `ITEM_DRAG_END(id, p)` com o item sobre o personagem | `USING` | O item sai (`RemoverItem`, usado), o uso começa no apoio em que ele está, e as ondas mudam na hora (seção 2.16): primeiro a combinação, em que um item de alívio acalma a onda da frente um passo, sem começar a dele; depois a paranoia, em que um item de substância entra na carga do episódio e o uso que fecha a mistura com droga sintética faz o sorteio único. A regra termina com o que mudou: `; alivia X -> Y`, `; a paranoia começa: Paranoico/Subida/1` ou `; a paranoia sobe: X -> Y`; o sorteio que não sai não deixa texto. Descansando, acorda antes; o plano, a reação ou o pouso são cortados. |
| `JUMPING`, `FALLING`, `USING`, `PRESSED`, `DRAGGING` e os de transição ou sistema (`SETTLING`, `HIDDEN`, `BOOTING`, `EXITING`) | `ITEM_DRAG_END(id, p)` com o item sobre o personagem | permanece | Recusado: o item cai de onde foi solto. `PRESSED` e `DRAGGING` são inalcançáveis com um ponteiro só. |
| qualquer | `ITEM_DRAG_END` fora do personagem, `ITEM_RELEASE`, ou `ITEM_DRAG_END` de um item só segurado | permanece | O item cai de onde foi solto, ou fica, se já está no chão; nunca é usado. O atento acaba, e a agenda volta depois do intervalo de acomodação. |
| `USING` | fim dos passos do uso | `SETTLING`, depois `IDLE`, `CLIMBING` agarrado, `HANGING` agarrado ou `PEEKING` | Volta ao mesmo apoio, com a cara de base: preso continua preso, escondido continua escondido. Se o uso começou a paranoia e ele volta a `IDLE` sem gesto, com ela ainda na frente, olha pro teto na hora (`OlharProTeto`, 90 passos, sem sorteio, também pausado), com a regra "IDLE: a paranoia começou, gesto OlharProTeto". |
| `USING` | `PRESS` | `PRESSED` | No mesmo evento: o uso acaba, e a onda continua. |
| `USING` | `TOPOLOGY_CHANGED` que revalida, tela cheia, `CMD_HIDE`, sessão, suspensão, `CMD_EXIT` | conforme a linha de cada evento | O uso acaba, e a onda continua; só `EXITING` cancela a onda. Um `TOPOLOGY_CHANGED` que não muda a geometria do monitor dele não interrompe o uso (DEC-030). |
| qualquer | `ITEM_EFFECT_TIMER(g)` da geração agendada | permanece | A onda avança uma fase ou um nível; quando acaba, a de fundo volta. A cara da fase entra na hora, salvo em `RESTING`, `REACTING` e `USING`. A agenda não é reagendada: pausado, o disparo só troca a cara. Um disparo de outra geração é ignorado. |

Toda decisão autônoma agendada respeita o intervalo de acomodação, em qualquer estado que decide (DEC-020).

Com a chave do tamagotchi desligada, os eventos dos itens e da onda são descartados antes de qualquer outra regra, inclusive antes de encerrar um gesto, e o baseado por conta própria tem peso zero (DEC-028). A linha da emoção dominante não depende da chave. Com a chave ligada, no fim de todo evento, a carga da paranoia volta toda a zero se nem a onda da frente nem a de fundo é de substância: o episódio acabou, e o seguinte sorteia de novo (seção 2.16).

**Invariantes, verificáveis por teste automático**

1. Em `PRESSED`, `DRAGGING`, `SETTLING` e `USING`, nenhum evento autônomo muda estado ou posição.
2. Em `DRAGGING`, a posição do personagem é sempre o cursor menos o deslocamento da pegada. O personagem não anda, não pula, não foge e não começa escalada.
3. O painel de energia não recebe nem interpreta texto; teclas locais só alteram o controle dele quando está focado.
4. Nenhum comportamento autônomo começa com o painel de energia aberto. Começar é entrar no estado: a transição para o mesmo estado, como a que registra a escolha da emoção dominante, não conta (DEC-027).
5. Depois de `SETTLING`, a âncora está dentro da área útil de algum monitor presente.
6. A expressão pode mudar em qualquer estado sem alterar estado de comportamento ou posição.
7. Com a mesma semente e a mesma sequência de eventos, a sequência de retratos é idêntica.
8. Abrir ou fechar o painel de energia nunca muda a posição do personagem.
9. Iniciar um arraste fecha o painel de energia; ao soltar, nenhuma preferência de energia é alterada implicitamente.
10. `SESSION_UNLOCKED` e `RESUMED` nunca fazem o personagem reaparecer quando ele foi escondido pelo usuário.
11. Todo estado tem pelo menos uma transição de entrada e uma de saída, com duas exceções por construção: `BOOTING`, só com saída, e `EXITING`, só com entrada. `USING` entra pelo soltar de um item ou pela agenda (o baseado por conta própria) e sai pelo fim do uso ou por uma interrupção; só aparece com a chave do tamagotchi ligada (DEC-028).
12. Um nível de energia mais alto pode aumentar a frequência e a duração das ações, mas nunca muda colisões, limites de superfície, segurança ou prioridade da ação direta. A onda de um item é a exceção documentada para as velocidades (invariante 26, DEC-028); o nível de energia continua sem mudar a física.
13. O adaptador nunca envia identidade ou conteúdo de outra janela ao núcleo; o modo de tela cheia recebe só os monitores cobertos pela janela ativa.
14. Em `PRESSED` e `DRAGGING`, `FULLSCREEN_TARGETS_CHANGED` não muda estado nem posição. Depois de um arraste ou de um `CMD_SHOW` manual durante o modo de tela cheia, o fim da tela cheia não move o personagem.
15. Um gesto curto nunca muda estado de comportamento, posição ou superfície e termina ao chegar qualquer evento de prioridade maior. Vale também para os oito gestos da onda (DEC-028), inclusive o olhar pro teto do começo da paranoia.
16. A posição gravada (`GravarPosicao`) nunca é a posição temporária do modo de tela cheia (DEC-020).
17. Todo `AgendarDecisao` tem atraso maior ou igual ao intervalo de acomodação (DEC-020).
18. Todo `GravarPosicao` (DEC-029):
    - sai só de `DRAG_END`, `DRAG_CANCEL`, `CMD_RESET_POSITION`, `CMD_HIDE`, `SESSION_LOCKED`, `SUSPENDING`, `CMD_EXIT` ou `SESSION_ENDING`; nunca do relógio, do movimento, da agenda autônoma, da troca de expressão, da carga, da topologia, da tela cheia nem das preferências, e por isso não há gravação periódica (DEC-011);
    - traz uma posição gravável: chave não vazia, frações em [0, 1] e a tela do monitor da época conhecida e não vazia, com a postura do estado depois do evento (a borda do esconderijo e a marca de preso);
    - volta igual do `settings.json`, com a postura: escrita e lida pelo esquema (seção 2.12), é válida, sem aviso e sem precisar de normalização.

    É conferido nas sequências aleatórias sem a física e também com a configuração do aplicativo (física, agarrar e esconderijo), e, nesta, também na partida seguinte.

Os invariantes 19 e 20 são da topologia em execução (passo P8 da Fase 5; DEC-030):

19. Nos estados que revalidam (autônomos, físicos, `REACTING` e `USING`), um `TOPOLOGY_CHANGED` que não muda a geometria do monitor do personagem (só outros monitores mudaram, ou o dele só foi transladado ou só trocou de chave, com a mesma tela) não muda o estado: há uma única transição, para o mesmo estado; a âncora, o retângulo e, nos estados de movimento, a posição fina andam exatamente pela translação; a posição descreve o lugar novo; o esconderijo, o preso, o uso, a direção e a cara continuam; a agenda e o relógio não mudam, salvo pelo fim de um gesto curto (invariante 15) e pelos itens.
20. Depois de um `TOPOLOGY_CHANGED`, visível e fora de `PRESSED` e `DRAGGING`, o monitor do personagem é um da topologia nova e, quando o sprite cabe nele, a âncora está na área útil dele.

O invariante 21 é da travessia (passo P13; DEC-032):

21. À vista, fora de `PRESSED`, `DRAGGING`, `REACTING` e do esconderijo, o sprite fica inteiro na área útil do monitor da âncora, quando cabe nela; no meio de uma travessia (andando, ou no voo do salto de degrau ou do transbordo), inteiro na união das áreas úteis. Uma mudança de topologia no meio da travessia a desfaz e vai a `SETTLING`, fora do invariante 19.

Os invariantes 22 a 30 são da emoção dominante e do tamagotchi (DEC-027, DEC-028 e DEC-033; regras exatas na seção 2.16):

22. Sem itens, sem onda, com a emoção automática e sem o baseado por conta própria (a ação `FumarBaseado` fora das ações), retratos, transições e efeitos são idênticos aos de antes; as referências gravadas 01–05 continuam idênticas byte a byte. Com a configuração do aplicativo, que liga a ação, a agenda muda de propósito, porque ele fuma; as equivalências usam as ações de sempre (`Todas`).
23. Itens só nascem por `CMD_SUMMON_ITEM`, com o próximo Id, que nunca se repete; um uso vem do `ITEM_DRAG_END` do usuário sobre o personagem, num estado que aceita, ou da ação autônoma `FumarBaseado` (só o baseado, só em `IDLE` no chão, nunca cria item no mundo). Um item só sai usado, recolhido ou substituído pelo sétimo. Nada autônomo, do relógio ou do sistema invoca ou usa um item do mundo, e só se entra em `USING` por esses dois caminhos.
24. Em `USING`, `PRESS` leva a `PRESSED` no mesmo evento, e nenhum evento autônomo chega à máquina. O uso dura exatamente os passos do verbo e sai por `SETTLING` para o mesmo apoio, mantendo o preso e o esconderijo. Interrompido, só o uso acaba; a onda continua.
25. Com onda, fora de `EXITING`, há exatamente um `ITEM_EFFECT_TIMER` pendente, de 1 s ou mais; sem onda, nenhum. O nível fica entre 1 e 3 (na queda, 1), e há no máximo uma onda de fundo, de outro tipo. Sem item novo, a onda da frente acaba em no máximo 2 + nível disparos.
26. A onda só muda pesos, intervalos, gestos, caras, os tempos na parede e pendurado, as velocidades de andar, escalar e pendurar (de 50% a 200%) e o cambaleio, sempre no chão e entre as laterais. Gravidade, queda máxima, quique, foguete, colisões, limites, apoio e prioridade do usuário ficam intactos.
27. A emoção dominante é nula ou uma das 14 caras de humor. Com a mesma semente e os mesmos eventos, ligá-la muda só as expressões.
28. Há no máximo 6 itens. Fora da mão do usuário, todo item tem o sprite inteiro na área útil do monitor dele e, parado, os pés no chão. A janela de um item aparece se e somente se ele é visível: na mão do usuário, sempre; fora dela, só com o personagem visível e fora de um monitor ocupado pela tela cheia. Nenhum item invisível fica caindo.
29. O relógio corre se e somente se o personagem se move sem estar agarrado, está em `REACTING` ou `USING`, faz um gesto em `IDLE` ou há um item visível caindo.
30. Com o conteúdo adulto desligado (DEC-033), não há item adulto no mundo, onda de substância na frente ou no fundo nem uso de um item adulto.

Como são conferidos: os invariantes 19 e 20, a cada `TOPOLOGY_CHANGED` nas sequências aleatórias, contra regras escritas no teste, à parte do núcleo (`InvariantesTestes`), e com a física do aplicativo (`MudancaDeTopologiaTestes`); os 22 a 29, a cada evento nas sequências aleatórias com a chave ligada (`InvariantesTestes`), e o 22 e o 27 também com a física e a configuração do aplicativo (`ChaveLigadaTestes`). O mesmo teste confere ainda as listas fechadas da energia e da emoção, a ausência de relógio fora dos estados que o pedem, as propriedades do alívio, da paranoia e do baseado por conta própria contra uma transcrição das tabelas escrita à parte do núcleo, com casos exigidos em cada semente (as regras e os casos estão no próprio teste).

### 2.7 Arbitragem de input, clique, arraste e painel de energia

Desenho da DEC-009, validado pelo P3 e implementado na Fase 3 (`src/Buzzy.Core/Entrada/ArbitroDeGestos.cs`, adaptador em `JanelaPersonagem.cs`; DEC-021).

**Quem recebe o input**

- O personagem só recebe cliques nos pixels visíveis; pixels transparentes deixam o clique passar à janela de baixo (seção 2.13).
- A janela do personagem **não ativa**: clicar ou arrastar o Buzzy não tira o foco do aplicativo em uso.
- O painel compacto de energia fica numa janela própria, pequena e ativável, ancorada ao lado do personagem, só com o seletor Baixa/Média/Alta; entra na Fase 8, com as configurações. Só o controle recebe teclado quando o painel está em foco.
- O Buzzy não instala hooks globais, não registra atalhos globais e não lê teclado fora das próprias janelas.

**Clique ou arraste**

1. `POINTER_DOWN` com o botão esquerdo sobre um pixel opaco gera `PRESS`. O adaptador captura o mouse para continuar recebendo o movimento fora da janela. O núcleo entra em `PRESSED` e congela o movimento autônomo.
2. Se o cursor sai do retângulo de arraste do sistema, centrado no ponto de pressão, a arbitragem emite `DRAG_START`. O retângulo vem de `SM_CXDRAG` e `SM_CYDRAG`, lidos para o DPI do monitor — a mesma regra que o Windows usa.
3. Se o botão é solto dentro do retângulo, a arbitragem emite `CLICK`, não importa quanto tempo o botão ficou pressionado.
4. Um segundo `CLICK` dentro do tempo de clique duplo do sistema (`GetDoubleClickTime`) e dentro do retângulo de clique duplo (`SM_CXDOUBLECLK`, `SM_CYDOUBLECLK`) gera `DOUBLE_CLICK`. A reação ao primeiro clique acontece na hora, sem esperar o tempo do clique duplo.
5. Botão direito solto sobre o personagem gera `CONTEXT_MENU`.
6. `CAPTURE_LOST` durante o arraste, por Alt+Tab, janela de UAC ou outra captura, gera `DRAG_CANCEL`. O personagem fica onde estava e passa pela validação normal; não volta ao ponto de origem.

Regras concretas da Fase 3 (DEC-021):

- O limiar vale para cada lado do ponto de pressão: vira arraste quando o cursor anda mais que `SM_CXDRAG` na horizontal ou mais que `SM_CYDRAG` na vertical, nas métricas do DPI atual da janela do personagem (o do monitor em que foi pressionado).
- O clique duplo segue a regra do sistema: o segundo pressionar chega antes do tempo de clique duplo, contado desde o primeiro pressionar, a uma distância menor que a metade inteira do retângulo de clique duplo, nas métricas do primeiro clique. Um terceiro clique rápido começa uma sequência nova, e um arraste interrompe a sequência.
- Soltar fora do retângulo sem movimento intermediário (gesto muito rápido) gera `DRAG_START` e `DRAG_END` no ponto solto.
- Nada fica preso ao cursor: um movimento sem o botão esquerdo logicamente pressionado (`MK_LBUTTON`), um novo botão pressionado sem o soltar anterior e `WM_CANCELMODE` encerram o gesto com `DRAG_CANCEL`. Com o ClickLock ligado, o Windows mantém o botão logicamente pressionado, e o gesto segue até o clique de liberação.
- O botão direito só abre o menu fora de um gesto do botão esquerdo. O laço modal do menu roda depois do processamento do núcleo, nunca dentro dele.

**Ciclo de arraste**

1. `PRESS` leva a `PRESSED`, e o movimento autônomo congela.
2. `DRAG_START` leva a `DRAGGING`: o plano autônomo é descartado, pulo, queda e escalada são interrompidos e a agenda autônoma é suspensa.
3. A cada `DRAG_MOVE`, a janela vai para o cursor menos o deslocamento da pegada. O Windows agrupa os movimentos pendentes na fila, sempre com a posição mais recente; cada movimento vira posição da janela no mesmo tratamento da mensagem (latência M5 registrada pelo app em modo de diagnóstico). Não há física nem suavização durante o arraste.
4. `DRAG_END` leva a `SETTLING`. A validação calcula a âncora, escolhe o monitor que contém a âncora ou o mais próximo e prende a âncora na área útil desse monitor.
5. O personagem identifica a superfície sob os pés. Com apoio, vai para `IDLE`; sem apoio, para `FALLING` até o chão do monitor (ou agarra o cipó ou a parede, DEC-024). Soltar o personagem junto a uma parede não inicia escalada.
6. Depois de um intervalo de acomodação, a agenda autônoma volta a funcionar.

**Janelas dos itens do tamagotchi** (DEC-028; `JanelaDoItem` e `Aplicacao.Itens.cs`)

- **Quem recebe o input:** cada item tem uma janela própria, com a receita da janela do personagem (seção 2.13.1). Só os pixels opacos do item recebem clique; a janela nunca é ativada nem tira o foco. `WM_MOUSEMOVE` só é tratado durante um gesto começado no item.
- **Árbitro próprio:** uma segunda instância do árbitro de gestos, só para os itens, com as mesmas regras do personagem (limiar do sistema, clique duplo, `MK_LBUTTON`, ClickLock e captura perdida). Cada gesto vira um evento do item em que o botão esquerdo foi pressionado, mesmo que a mensagem chegue por outra janela:

  | Gesto do árbitro | Evento do item |
  |---|---|
  | `PRESS` | `ITEM_PRESS` |
  | `DRAG_START`, `DRAG_MOVE`, `DRAG_END` | `ITEM_DRAG_START`, `ITEM_DRAG_MOVE`, `ITEM_DRAG_END` |
  | `CLICK`, `DOUBLE_CLICK`, `DRAG_CANCEL` | `ITEM_RELEASE`: o item fica no chão, ou cai de onde está, e nunca é usado |
  | `CONTEXT_MENU` | o mesmo, sem tradução: abre o menu do personagem |

  Um botão pressionado noutro item, sem o soltar do anterior, larga o anterior antes de pegar o novo.
- **Botão direito:** solto num item, fora de um gesto do botão esquerdo, abre o mesmo menu do personagem e da bandeja (Q-03). Tirar um item da tela é pelo "Recolher itens".
- **Captura:** a janela do item captura o mouse só no gesto e a solta no fim, sem contar como captura perdida. Quando o núcleo encerra o gesto por conta própria (esconder, inclusive pela minimização da janela do personagem, bloquear a sessão, suspender, sair, recolher ou pegar outro item), ele emite `LiberarCapturaDoItem` (seção 2.3).
- **Entrega:** quem decide se o item foi solto sobre o personagem é o núcleo, pelo retângulo do item já preso na área útil contra o do personagem encolhido (seção 2.16); a raiz não informa a transparência. Com `--diagnostico`, a raiz refaz o teste só para o log.
- **Ordem Z, sempre por evento:** fora de um gesto, a janela do item fica logo abaixo da do personagem; no gesto sobre o item, vai ao topo do grupo "sempre no topo", para o item não sumir atrás do personagem justamente quando vai ser solto sobre ele, e volta para baixo no fim; quando o personagem reaparece no topo, a ordem é reafirmada. Nunca por timer (SECURITY.md 2). Depois de uma releitura da topologia e na conferência tardia dela (seção 2.8), a raiz só devolve ao lugar do núcleo a janela à vista, fora de um gesto, que saiu dele, sem mexer na ordem Z (`SWP_NOZORDER`; DEC-030); a janela do item na mão fica onde o cursor a pôs.
- **Um ponteiro só:** um gesto num item e outro no personagem não coexistem, porque a captura do mouse é única.

**Foco do painel de energia**

- O painel contém só o controle de energia; não há campo de texto nem envio de conteúdo.
- Sem atalhos de teclado para controlar o personagem (Q-06): nenhuma tecla move o personagem, com ou sem foco. Teclado e leitor de tela valem para o controle e as configurações (Q-20).
- Na Fase 8, o painel abre pelo menu (DEC-025) e recebe foco como consequência dessa ação explícita.
- O painel fecha com Esc, pelo botão de fechar ou ao perder o foco, como a Fase 8 validar.

### 2.8 Monitores: restauração, conexão e desconexão

Decisão em DEC-008; restauração e topologia em execução implementadas nos passos P1, P2, P8 e P9 da Fase 5 (DEC-030), com a posição salva entregue ao núcleo desde o passo P7 (DEC-029). Os cenários reais continuam [MANUAL][HW] (TODO.md, Fase 5).

**Posição gravada:** chave do monitor, retângulo desse monitor na época, posição relativa da âncora na área útil (frações de 0 a 1) e posição absoluta de reserva.

- O retângulo é `PosicaoDoPersonagem.TelaDoMonitor`: a tela do monitor da chave na última vez em que a posição foi descrita nele; nulo quer dizer desconhecido. É preenchido por `Descrever`, pelos dois casos de `Reacomodar`, pela validação da máquina e por `Restaurar`, sempre com a tela real de um monitor presente, e nunca é deslocado por cálculo, nem quando a posição acompanha a translação da topologia (`Rebasear`): o arquivo não pode guardar uma tela que nunca existiu.
- A âncora absoluta vale para a execução; na partida, a cascata abaixo a recalcula, porque noutra sessão, com outro principal, ela fica noutro referencial.
- A igualdade de `PosicaoDoPersonagem` inclui a tela: compare as posições campo a campo ou obtenha as duas pelas mesmas funções do `Posicionador`.

**Restauração ao iniciar** (`Posicionador.Restaurar`)

1. Se o monitor da chave gravada existe, a posição relativa é aplicada à área útil atual dele. Isso cobre troca de resolução e de escala.
2. Se não existe, mas há um monitor com o mesmo retângulo, ele é usado (o primeiro da topologia com a tela igual; com monitores clonados ou sobrepostos, a escolha é determinística, mas arbitrária).
3. Caso contrário, a posição relativa é aplicada ao monitor principal.
4. Em todos os casos, a âncora é presa à área útil, e o personagem passa por `SETTLING`.
5. Se o monitor original voltar depois, o personagem não pula de volta sozinho: a posição só muda por ação do usuário ou por movimento autônomo.

Nos três primeiros passos, as frações são saneadas (NaN vira 0,5; o resto, inclusive ±∞, é preso em [0, 1]), e a âncora absoluta gravada não é usada; a posição nova fica com a chave e a tela do monitor escolhido, e restaurar de novo o resultado, na mesma topologia, não o move. Em `SETTLING`, uma posição gravada no ar segue a acomodação de sempre (perto do teto agarra o cipó, junto de uma lateral gruda na parede e, senão, cai; DEC-024), e a carga traz a postura gravada (seção 2.12; linha de `BOOTING` da seção 2.6). Com a chave sumida, o passo 2 pode achar o sobrevivente que passou a ocupar a tela antiga, enquanto com o app aberto ele iria ao sobrevivente mais próximo: é a semântica da cascata.

**Durante a execução** (passo P8, no núcleo, e P9, na raiz; DEC-030):

- **Posições guardadas:** em qualquer estado fora de `EXITING`, a posição do personagem e o retorno da tela cheia acompanham a topologia sem mover a janela (`Posicionador.Rebasear`). Com o monitor correspondente, vale a mesma fração na área útil atual dele, com a chave e a tela dele. Sem ele, a chave, as frações e a tela ficam, e só a âncora anda, junto com o sobrevivente mais próximo do pixel dos pés medido nas coordenadas antigas (no empate, o principal; depois, a ordem da topologia nova). Se o monitor voltar antes de a posição ser usada, ela vale nele.
- **Monitor correspondente** (`MonitorCorrespondente`): o da mesma chave; sem ele, o primeiro com a mesma tela cuja chave não existia antes, como quando a chave passa de `gdi:` para `mon:` ou muda com a porta ou o driver. A condição da chave nova impede que o sobrevivente que o Windows põe na origem, quando o principal é desconectado, conte como o mesmo monitor.
- **O monitor do personagem não muda de geometria** (`SoTranslacao`: tela e área útil deslocadas pelo mesmo (dx, dy), com o mesmo DPI; a marca de principal não conta), como numa troca de principal, num rearranjo ou quando só outros monitores mudam: nos estados que revalidam, o estado continua, e a janela anda junto (seção 2.6, invariante 19).
- **O monitor do personagem é desconectado:** ele vai para o sobrevivente mais próximo do pixel dos pés da última âncora, `(x, y − 1)`, medido nas coordenadas antigas, na mesma posição relativa, e passa por `SETTLING` (quando o principal sai, o Windows move a origem, e o mais próximo nas coordenadas novas seria outro). O pixel dos pés é a convenção do monitor da âncora: com a barra oculta, a âncora no chão fica na base da tela, que já é o primeiro pixel do monitor de baixo.
- **Troca de resolução, escala ou orientação:** a posição relativa é mantida, o tamanho físico é recalculado e o personagem passa por `SETTLING`.
- **A barra de tarefas muda de lugar, de tamanho ou se esconde sozinha:** a área útil muda, as superfícies são recalculadas e o personagem se acomoda.
- **Mudança durante um gesto:** nada é validado com o botão pressionado. No arraste, o lugar anda com o monitor em que está, como a janela e o cursor, que o Windows leva com o monitor físico quando a origem muda; a validação vem ao soltar. Pressionado sem arrastar, toda saída parte de onde ele estaria parado. O próprio Windows reposiciona o cursor se o monitor sumir.
- **Itens do tamagotchi:** a mesma regra, e o item na mão anda com o monitor em que está (seção 2.16).
- **Releitura e conferência tardia** (`AgendaDaReleitura`): a releitura sai 300 ms depois da última mensagem, com teto de 1 s desde a primeira, e a rajada acaba quando ela sai. Uma leitura incoerente mantém a topologia anterior e é tentada de novo em 500 ms, 1 s e 2 s, a última como leitura parcial (seção 2.4); depois, a agenda desiste até a próxima mensagem. Cada releitura leva no máximo 32 motivos ao log. Depois de cada releitura publicada, a raiz devolve ao lugar do núcleo a janela do personagem e as dos itens à vista que saíram dele, fora de um gesto e sem mexer na ordem Z, e repete essa conferência 1,5 s depois, uma vez por rajada, nunca periódica (com "Lembrar locais das janelas" ligado, o Windows pode devolver uma janela ao monitor reconectado depois da releitura). A barra de tarefas recriada (`TaskbarCreated`) passa pela mesma releitura. Os três tempos são provisórios até o protótipo P5.

**Fundamentos por fase**

| Fundamento | Fase |
|---|---|
| Per-Monitor V2, coordenadas canônicas, enumeração de monitores, área útil, escala | 1 |
| Monitor que contém um ponto, monitor mais próximo, prender o ponto na área útil | 1 |
| Reler a topologia e prender a posição quando ela muda | 1 |
| Arraste entre monitores e validação ao soltar | 3 |
| Chave estável, restauração com alternativas, passagens entre monitores, troca de escala durante o movimento, cenários de conexão | 5 |

### 2.9 Movimento

O modelo segue as superfícies de Q-05.

- **Corpo lógico:** retângulo em DIPs com âncora entre os pés, independente do tamanho da imagem.
- **Passo fixo:** simulação com passo fixo e integração semi-implícita, sem depender da taxa de quadros: 1/60 s (`ConfiguracaoDoNucleo.PassosPorSegundo`).
- **Caminhar:** velocidade constante sobre o chão; diante de parede, vira ou escala; diante de passagem, atravessa ou cai.
- **Salto de travessia (passo P13; DEC-032):** a posição em cada passo é analítica — `x0 + vx·t` e `y0 + vy0·t + ½·g·t²`, com a gravidade na escala da origem, congelada no voo — e o último passo é exatamente o pouso. O tempo de voo e o pouso são os primeiros, numa lista fixa, em que todos os passos mantêm o sprite na união das áreas úteis; não usa o gerador pseudoaleatório. Os pulos e quedas autônomos continuam presos ao monitor da âncora.
- **Escalar:** só em paredes, até o topo da área útil; de lá, desce, pula, se solta ou se pendura na borda superior.
- **Pendurar-se:** em `HANGING`, desloca-se pelas mãos ao longo de uma borda superior alcançável, por tempo limitado; ao terminar, volta a escalar, salta dentro do alcance ou se solta e cai.
- **Saltar:** trajetória balística calculada para atingir um alvo; alvo e velocidade inicial são determinísticos, dada a semente.
- **Cair:** gravidade com velocidade máxima até tocar o chão.
- Velocidades e gravidade são definidas em DIPs por segundo e convertidas pela escala do monitor atual.

**Implementado na Fase 4 (DEC-022), num monitor:**

- **Estado:** o núcleo guarda a posição fina da âncora e a velocidade em pixels físicos (`EstadoDoMovimento`); a janela recebe a posição arredondada, e a posição relativa acompanha o movimento para sobreviver a uma mudança de topologia.
- **Velocidades e gravidade** (`ParametrosDeMovimento`), iguais em todos os níveis de energia: caminhada 90 DIP/s, escalada 110 DIP/s, pendurado 80 DIP/s; gravidade 2200 DIP/s², queda máxima 1500 DIP/s; impulso ao saltar da parede de 220 DIP/s, com 40 DIP de altura.
- **Pulo:** a velocidade inicial vem da altura do arco (`vy0 = −√(2gH)`), e a horizontal cobre a distância sorteada no tempo de voo; no ar, a área útil limita as laterais e o topo.
- **Pausa ou painel aberto:** o movimento em curso termina num lugar estável (a caminhada para, a escalada desce, quem está pendurado se solta, pulo, queda e pouso terminam).
- **Energia:** distâncias, alturas, tempos na parede e tempos pendurado são faixas dos perfis de energia (seção 2.11; tabela na DEC-022).
- **Toon force (DEC-023):** um impacto de pelo menos 600 DIP/s quica, voltando a subir com metade da velocidade vertical e 70% da horizontal, no máximo duas vezes (`EstadoDoMovimento.Quiques`); parte das subidas a partir do chão é um foguete a 1000 DIP/s até a borda superior (`EstadoDoMovimento.Foguete`), com a chance do perfil de energia (`ChanceDoFoguete`) e a mesma velocidade para todos; com a autonomia pausada ou o painel aberto, nem quique nem foguete.
- **Regra da calma (DEC-022, item 4):** com a autonomia pausada ou o painel aberto, quem está agarrado à parede ou ao cipó sem estar preso deixa de estar agarrado, no fim de qualquer evento, e os passos seguintes o fazem descer pela parede ou se soltar do cipó. O preso pelo usuário fica, e o atento a um item na mão do usuário também. Não depende da chave do tamagotchi.

**Com o tamagotchi (DEC-028), só com a chave ligada** (tabelas na seção 2.16):

- **Velocidades da onda:** passam pela física efetiva (`Maquina.FisicaEfetiva`) em cinco lugares — andar; escalar e o passeio do preso na parede; pendurado e o passeio do preso no cipó —, de 50% a 200% das de sempre; o resto da física não muda.
- **Cambaleio:** nas ondas do bêbado e do tonto, o passo de cada quadro da caminhada é multiplicado por uma onda triangular de 48 passos (`Maquina.FatorDoCambaleio`); ele fica no chão, preso entre as laterais, e o recuo devolve distância ao percurso.
- **Itens:** caem com a gravidade e a queda máxima do personagem, presos entre as laterais do monitor deles, e quicam uma vez, de leve. Um item que deixa de aparecer vai direto ao chão.

### 2.10 Apresentação, assets e expressões

- O núcleo expõe um retrato do estado: estado de comportamento, direção, fase do movimento, expressão e sinais pontuais, como "pousou" ou "foi clicado".
- Um **manifesto de assets** em arquivo de dados liga cada estado a um clipe de animação e cada expressão a uma camada ou variante, com a âncora da imagem, o tamanho lógico e a taxa de quadros de cada clipe (Fase 6).
- **Trocar asset** é trocar o manifesto e as imagens; núcleo, movimento, arbitragem, mundo do desktop e segurança não mudam. Um teste automático roda a mesma suíte do núcleo com dois manifestos diferentes.
- Até a Fase 6, o app mostra quadros estáticos da pixel art, com o chapéu de palha (DEC-018, DEC-019): `PoseDoPersonagem` escolhe uma pose provisória por estado (ciclo de caminhada e de escalada, pendurado, impulso, no ar, caindo, pousando, sentado ou dormindo, segurado, reagindo e os gestos), espelhada para a esquerda.
- **Toon force (DEC-023):** a pose pode vir achatada (impacto) ou esticada (velocidade), pela dinâmica do movimento (`Dinamica`: velocidade vertical, quiques e foguete). A deformação é da própria pixel art (`Tela.Deformada`, vizinho mais próximo, pés na mesma linha); a janela, a âncora e a regra do alfa não mudam.
- **Cipó (DEC-024):** na borda de cima, ele aparece pendurado num cipó (`cipo-1` a `cipo-3`), balançando em ciclo quando anda pela borda e no quadro do meio quando está agarrado. Agarrado à parede, a pose da escalada fica parada.
- **Esconderijo (DEC-025):** em `PEEKING`, e em `PRESSED` e `REACTING` de quem continua escondido, aparece a pose `escondido`, só com o chapéu, a cabeça e as mãos na borda; nas laterais, girada 90° (`Tela.Girada`, sem perda).
- **Retrato da emoção dominante e do tamagotchi (DEC-027, DEC-028):** o retrato tem `EmocaoDominante` (para a marca no menu), `Onda` e `OndaDeFundo` (para as sobreposições), `Uso` e `PassoDoUso` (para o quadro de uso), `Itens`, inclusive o da mão, e `Carga`, o número de substâncias do episódio da paranoia. A linha canônica das reproduções ganha os trechos `onda=Tipo/Fase/Nível`, `fundo=Tipo/Fase/Nível`, `carga=N` (só acima de 0, logo depois de `fundo=`), `uso=Item/Verbo/PassodeDuração/Apoio`, `emocao=Nome` e `itens=[Id:Item:Situação:(x,y);…]`, só quando há valor; sem eles, a linha é a de antes. O resto da carga e o gerador da paranoia ficam fora do retrato e da linha.
- **Arte da emoção dominante e do tamagotchi em `src/Buzzy.Visual/Pixel/`** (passos A1–A4; regras de desenho em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md)):
  - **Chaves:** os nomes dos enums do núcleo em minúsculas, para os itens (`ItensPixel`, 24 × 24 pixels de arte), as 8 caras de efeito (`Rostos.DeEfeito`) e as poses dos 8 gestos da onda (`PosesPixel.DosGestos`). Cada lado confere as mesmas listas nos próprios testes, e `NucleoEArteTestes` compara os enums do núcleo com as listas da arte: itens, verbos, passos do uso, caras e gestos.
  - **`UsosPixel`:** a sequência de quadros de cada verbo, no chão e de frente (`Sequencia`, `Passos`, `Quadro` pelo passo do uso), com a soma igual à duração do uso no núcleo.
  - **`PosesPixel.PorNome`** acha qualquer pose: as de estado e gesto (`Todas`), as dos gestos da onda (`DosGestos`) e as de uso (`UsosPixel.Poses`); as duas últimas ficam fora de `Todas`, e a folha nativa não muda.
  - **`BonecoPixel.Desenhar(pose, expressao, item, efeito, fase)`:** a pose diz como segura o item (`Segura`: nada, na mão B ou, na lança-perfume, o frasco na mão A e o lenço na B). Com um item, o braço que o leva ao rosto se dobra por cinemática inversa de dois ossos (`Alcancar`, `AjustadaAoItem`) até a ponta do item cair na boca ou no nariz. Nas poses de uso, a cara é a da pose (expressão nula). `Pontos` dá as mãos, a cabeça, a boca e o nariz, e `ItensColocados`, onde cada item ficou. Da paranoia: a mão `Mao.Apontando` e a gota `Rosto.Gota`, desenhada por cima dos braços da frente.
  - **`EfeitosPixel`:** 9 sobreposições em 3 fases (a 0 é a parada, sem relógio), que nunca cobrem `AreaDoRosto(pose, expressao)` — os traços do rosto e, na cara paranoica, a gota de suor — nem, no cipó, o cipó e a mão que o segura (no suor, a gota que cairia ali salta para o espelho da posição, do outro lado da cabeça). `Modificar` muda a pose pela onda só quando `Modificavel` é verdadeiro: no chão e fora das poses de uso. Tabelas em IDENTIDADE_VISUAL.md, seção 7b.
  - **`IconesDoMenu`:** o rosto (o recorte de 40 × 32 de `expressoes.png`, pela mesma `Tela.Recortada` da prévia), o item (o desenho do chão), `Fator` pelo DPI, `Ampliar` por vizinho mais próximo, em BGRA com alfa só 0 ou 255, e `DeBaixoParaCima`, que inverte as linhas para um DIB de altura positiva.
- **Na apresentação do tamagotchi** (`PoseDoPersonagem`, `SpriteProvisorio` e `CacheDeQuadros`):
  - **Quadro:** `QuadroDoSprite` leva o item na mão (a chave da arte, só nas poses de uso), a sobreposição da onda e a fase dela; o quadro inteiro, com o DPI, é a chave do cache.
  - **Uso no chão:** o quadro da sequência do verbo no passo do uso (`UsosPixel.Quadro`), com o item na mão e a cara da pose, de frente e sem espelho.
  - **Uso na parede, no cipó e no esconderijo,** que ainda não têm pose de uso: a pose de quem está ali (`escalando-1` virado para a parede, `cipo-2`, `escondido`, girada nas laterais), sem o objeto, com a cara do retrato, que o núcleo fixa na cara do item durante o uso; o apoio do uso decide a pose. No esconderijo, a boca fica fora do quadro, e só os olhos mostram a cara.
  - **Caras:** as de efeito e a emoção dominante chegam pela expressão do retrato nas poses que mostram a cara dele (andando, pendurado no cipó, parado sem gesto, escondido e nos apoios do uso); nas outras, vale a cara da pose.
  - **Gestos da onda:** as poses de `PosesPixel.DosGestos`, pelo nome do gesto, com a cara da pose. A tremedeira alterna com o `parado` a cada 4 passos, com a mesma cara: só o corpo treme. `olharproteto` e `agachar` têm desenho próprio e a cara paranoica, também no olhar pro teto do fim do uso que começou a paranoia.
  - **Sobreposição:** só a da onda da frente (IDENTIDADE_VISUAL.md, seção 7b), por cima de qualquer pose, inclusive dos quadros de uso; a de fundo não desenha nada. A onda `Paranoico` mostra o suor, com o tremidinho de 1 pixel onde a pose é modificável. A fase é (passos no estado / 12) % 3 com o relógio ligado e 0 com ele parado; sem sobreposição, fica 0, para o cache não guardar o mesmo desenho uma vez por fase.
  - **Cache:** LRU por bytes de pixels, com orçamento de 16 MiB: ler um quadro o renova, os mais antigos saem para caber um novo, e um quadro maior que o orçamento não é guardado. A 100%, cada quadro tem 64 KiB, e cabem 256; a 300%, só 28.
  - **Chave ausente:** a arte lança exceção para um nome que não conhece, sem quadro de reserva; a proteção são testes: `PoseTestes` desenha todo quadro que a escolha pode pedir, com todos os valores dos enums (cerca de 3 mil quadros), e `NucleoEArteTestes` compara os enums com as listas da arte.
- **Janela de um item (`SpriteDoItem`):** o desenho do chão do item, ampliado pelo DPI sem suavização, com alfa só 0 ou 255, num cache próprio de 4 MiB por item e DPI; os limites opacos e os pontos de teste da janela saem do mesmo desenho.
- A apresentação só redesenha quando o quadro muda ou quando a posição exige: um clipe de 10 quadros por segundo gera 10 redesenhos por segundo, não 60.
- A máscara de clique sai do canal alfa do quadro atual.

### 2.11 Personalidade ajustável e comportamento não verbal

O MVP expressa curiosidade, humor e energia por movimento, expressões faciais e gestos, sem frases, diálogo, chat, texto digitado ou respostas. A energia é um valor local de três opções que influencia parâmetros determinísticos:

- o peso de cada comportamento autônomo, usado na escolha da transição de `IDLE`;
- a faixa de tempo entre decisões autônomas;
- a tendência de expressão em cada estado;
- os pesos relativos para espiar, explorar bordas, brincar, reagir a cliques e descansar.

A semelhança com o Luffy é intencional (DEC-019; o que ela significa para o produto está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md), Visão); aqui ela só orienta pesos e tempos (impulsivo, otimista, curioso). Os níveis `BAIXA`, `MEDIA` (padrão) e `ALTA` são persistidos: Baixa com pausas maiores e menos ações, Média num ritmo brincalhão equilibrado, Alta com mais frequência e duração das brincadeiras e reações. O nível muda pesos, intervalos e duração das ações, nunca a física nem as regras de segurança; pausa e comando direto do usuário sempre prevalecem. Com a mesma semente, energia e sequência de eventos, o núcleo produz as mesmas escolhas. O seletor aparece no painel aberto pelo menu (DEC-025) e nas configurações, com a mesma preferência, na Fase 8.

No tamagotchi, o sorteio de `IDLE` tem uma última opção, fumar um baseado por conta própria, com o peso `PerfilDeEnergia.PesoFumarBaseado`: 1 nos três níveis, calibrado para cerca de um baseado a cada 4 minutos de tempo elegível na Média. A energia muda a frequência pelos intervalos e pelos outros pesos. Condições na seção 2.16; calibração na DEC-028, item 41.

### 2.12 Configurações, persistência e tempo

Decisões em DEC-010, DEC-011, DEC-027 e DEC-029; implementado no bloco A e no passo P7 da Fase 5 e no passo T1 da "Interação".

- **Arquivo:** um JSON com `schemaVersion` na pasta local do usuário: `%LOCALAPPDATA%\Buzzy` sem pacote MSIX; com MSIX, a pasta local do pacote.
- **Conteúdo planejado:** a última posição escolhida pelo usuário (seção 2.8), escala do personagem, sempre no topo, iniciar com o Windows, energia (padrão `MEDIA`), atravessar monitores, modo de tela cheia ligado por padrão e idioma; sem opacidade no MVP. Os campos entram nas fases previstas em TODO.md (decisões em DEC-013, DEC-014, Q-03 a Q-07, Q-09, Q-12 e Q-23).
- **Leitura:** campo desconhecido é ignorado, valor fora da faixa é preso ao limite, e arquivo ilegível é trocado pelo `.bak`, se for válido, ou pelos padrões (DEC-029); uma cópia do ilegível é guardada para diagnóstico, no máximo uma.
- **Gravação:** num arquivo temporário, depois substituição atômica, com o último arquivo bom como `.bak`; com atraso depois de soltar o personagem e sempre ao sair.
- **Tempo:** o relógio lógico só gera `TICK` com movimento, animação ou arraste. Em `IDLE` sem animação, em `RESTING` e em `HIDDEN`, nenhum timer periódico; a agenda autônoma usa um único timer até a próxima decisão. O modo de tela cheia é notificado por eventos limitados do Windows, sem polling global.

*Esquema, no núcleo (`Buzzy.Core.Persistencia`, sem E/S); a v2 trouxe a emoção dominante (DEC-027), a v3, a postura (passo P7), e a v4, o conteúdo adulto (DEC-033):*
- **Formato:** JSON em UTF-8 sem BOM, indentação de 2 espaços, fim de linha `\n`, campos em ordem fixa e números no formato mais curto, sem depender da cultura. Campos:
  - `schemaVersion`, hoje 4;
  - `posicao`, com `chaveMonitor`, `telaDoMonitor` (`esquerda`, `topo`, `direita`, `base`), `fracaoX`, `fracaoY`, `ancoraAbsoluta` (`x`, `y`) e, sempre escritos, `esconderijo` (`"nenhum"`, `"baixo"`, `"esquerda"` ou `"direita"`) e `presoPeloUsuario` (booleano); a postura só existe junto com uma posição;
  - `preferencias`, com `energia` (`"baixa"`, `"media"` ou `"alta"`), `modoTelaCheia`, `atravessarMonitores`, `emocaoDominante`, sempre escrito: `"automatica"` ou o nome de uma das 14 caras de humor em minúsculas ASCII, como `"feliz"`, e `conteudoAdulto` (booleano).

  Amostras de referência: `settings-v1.json` a `settings-v4.json`, em `tests/Buzzy.Core.Testes/Persistencia/Amostras/`. Arquivos v1 a v3 são lidos sem migração e sem aviso: sem a emoção, vale a automática; sem a postura, nenhuma borda e solto; sem o conteúdo adulto, ligado.
- **Leitura (`EsquemaDeConfiguracoes.Ler`), que nunca lança.** Só é ilegível o arquivo com mais de 64 KiB contando o BOM, UTF-8 inválido, JSON inválido (comentários e vírgula final são aceitos), mais de 8 níveis contando a raiz, raiz que não é objeto ou `schemaVersion` ausente, não inteiro ou menor que 1; um escape de surrogate solto num texto lido também torna o arquivo inteiro ilegível.
- **Campo a campo,** com um aviso por caso:
  - campo desconhecido é ignorado, e um repetido vale na primeira ocorrência;
  - valor fora da faixa é preso: frações em [0, 1], coordenadas de −32768 a 32767;
  - tipo errado vale o padrão do campo;
  - a posição é tudo ou nada: exige a chave (1 a 1024 caracteres, UTF-16 válido, sem caractere de controle) e as duas frações finitas; tela inválida vira desconhecida, e âncora inválida vira (0, 0);
  - a energia só vale pelos três nomes, sem diferenciar maiúsculas;
  - a emoção dominante só vale pelos 15 nomes (`automatica` e as 14 caras), sem diferenciar maiúsculas e nunca por `Enum.Parse`; `null` ou ausente vale a automática, sem aviso; outro valor, a automática, com aviso;
  - a borda do esconderijo só vale pelos quatro nomes, sem diferenciar maiúsculas; `null` ou ausente vale nenhuma, sem aviso; outro valor, nenhuma, com aviso. `presoPeloUsuario` ausente vale falso, sem aviso; outro valor não booleano, falso, com aviso;
  - os avisos nunca levam valores nem nomes vindos do arquivo.
- **Versão futura** (`schemaVersion` maior que a atual): lê os campos que conhece e bloqueia a gravação nessa execução. Toda ampliação do esquema incrementa `schemaVersion`: um build da v2 vê um arquivo v3 como futuro e não grava por cima.
- **Escrita (`Escrever`):** normaliza antes e nunca lança por causa do conteúdo: frações saneadas e sem `-0`, coordenadas presas, tela vazia omitida, energia fora dos três níveis gravada como `media`, emoção fora das 14 gravada como `automatica`, posição com chave inválida omitida e, sem posição, a postura em nenhuma borda e solto; uma borda fora do enum vira `nenhum`. Ler o que foi escrito devolve as configurações normalizadas.

*Arquivos, no adaptador (`Plataforma/ArquivoDeConfiguracoes.cs`):*
- **Quatro nomes, na mesma pasta, e nenhum outro:** `settings.json` (o principal); `settings.json.bak` (a reserva: o principal anterior, válido quando foi substituído); `settings.json.tmp` (o temporário de uma gravação, nunca lido); `settings.corrupt.json` (o último principal ilegível substituído, uma cópia só, nunca lida).
- **Leitura sem efeito colateral:** o principal; se faltar ou não servir, a reserva; senão, os padrões. Não cria, não altera e não impede outro processo de usar nenhum arquivo. Um arquivo com mais de 64 KiB é ilegível sem ser lido; um arquivo preso tem 3 tentativas, com duas pausas de 100 ms; principal inacessível depois delas faz valer a reserva ou os padrões e bloqueia a gravação nessa execução, para não sobrescrever o que não se conseguiu ler.
- **Gravação atômica:** apaga um temporário que tenha sobrado e cria outro do zero (`FileMode.CreateNew`, para nunca gravar através de um link), sem buffer e com `Flush(true)`. Depois confere o principal de novo e troca de uma vez:

  | Principal na hora | Ação |
  |---|---|
  | ausente | `File.Move` do temporário |
  | válido | `File.Replace`, guardando o anterior como `.bak` |
  | ilegível | `File.Replace`, guardando-o como `settings.corrupt.json`; a reserva fica como estava |
  | de versão futura | bloqueia a gravação |
  | inacessível | a tentativa falha |

  Uma queda em qualquer ponto deixa o principal anterior, o novo ou só a reserva, nunca um principal presente e ilegível. Uma falha de E/S é tentada de novo, dentro das tentativas pedidas; o erro leva só o tipo e o código da exceção, nunca o caminho. `File.Replace` exige NTFS local.

*Quando gravar (`PoliticaDeGravacao`, no núcleo):* com atraso de 2 s depois do último pedido, reiniciado a cada pedido; na hora quando o pedido vem de `SUSPENDING`, `SESSION_ENDING`, `CMD_EXIT` ou `SESSION_LOCKED`; depois de uma falha, novas tentativas únicas em 2, 10 e 60 s e, depois delas, só no próximo pedido; na hora, até 3 tentativas com 50 ms entre elas. Nada é periódico: só eventos do usuário ou do sistema pedem gravação (para a posição, os do invariante 18, seção 2.6).

*Agenda de gravação, na raiz (`Composicao/AgendaDeGravacao.cs`):*
- **Partida:** antes de tudo, a raiz cria o arquivo da execução pela regra da pasta, uma instância só, e o lê uma vez. Qualquer exceção da leitura, também a que não é de E/S, desliga a persistência nessa execução, sem derrubar a partida; o log leva só o tipo e o código. A carga (`Loaded`) leva a posição salva, com a tela do monitor da época, a postura e as preferências; sem arquivo, as padrão.
- **Pedidos:** cada `GravarPosicao` troca a posição e a postura do conteúdo desejado, e cada `GravarPreferencias`, as preferências; a raiz nunca lê o estado do núcleo para gravar. Com o mesmo conteúdo do disco, nada é gravado nem agendado. Sem nenhum pedido, nada fica pendente: a partida não grava sozinha. Vindo da reserva ou dos padrões, o primeiro pedido grava e recria o principal.
- **Gesto do usuário:** o disparo com atraso que chega com o botão pressionado, num arraste ou com um item na mão não rearma a espera (o que seria periódico com o ClickLock); o fim do gesto grava.
- **Falhas:** uma falha de E/S, também na gravação na hora, arma as novas tentativas, salvo depois de parar; uma exceção que não é de E/S desliga a gravação nessa execução, sem derrubar o Buzzy.
- **Encerramento:** descarrega o pendente e para a agenda antes de desmontar o resto; o erro não tratado descarrega o que der, uma vez só e sem lançar.
- **Suspensão (DEC-031):** depois de entregar a `SUSPENDING` ao núcleo, o tratador descarrega o pendente, com as tentativas do caminho imediato, e cancela o disparo com atraso. Com o personagem à vista, o próprio pedido da `SUSPENDING` já grava na hora; escondido pelo usuário ou pela sessão, o núcleo não pede gravação, e só a descarga leva ao disco um pedido de menos de 2 s antes. O bloqueio da sessão grava na hora pelo pedido do núcleo.
- **Custo:** a E/S é síncrona, na thread da interface; o arquivo tem cerca de 500 bytes, a gravação levou de 7,7 a 8,9 ms na integração, e o tempo vai para o log (seção 2.13.4).

*Pasta e perfis de teste (`Plataforma/PastaDeDados.cs`):* uma regra só escolhe a pasta da execução, com falha fechada — persistência desligada: nenhuma; com `--perfil-de-teste NOME`: `%LOCALAPPDATA%\Buzzy\testes\NOME`, e nenhuma se o nome for inválido, nunca a pasta real; sem perfil: `%LOCALAPPDATA%\Buzzy`; sem a pasta local do usuário: nenhuma. Sem pasta, nada é lido nem gravado como configuração. O arquivo só é criado por essa regra (`ArquivoDeConfiguracoes.DaExecucao`), e a pasta só na primeira gravação. As regras do nome estão em SECURITY.md 3.1.

*Fora do arquivo:* a posição temporária da tela cheia, os monitores ocupados, o cache das chaves dos monitores (seção 2.4) e, do tamagotchi, os itens, o uso, a onda, a carga do episódio da paranoia e o gerador dela, só em memória (DEC-028). A ocultação não vai: escondido pela bandeja ou pela sessão, ele aparece ao reabrir, na borda do esconderijo se estava nela. Com retorno de tela cheia guardado, a posição gravada é o retorno, mas a postura é a do estado atual.

### 2.13 Encaixe na stack recomendada

WPF com C# e .NET 10 (DEC-006). O WPF cuida das janelas e da interface; as chamadas Win32 diretas ficam no adaptador de plataforma. A identidade estável do monitor ainda depende do protótipo P5, e a consulta de aplicativo em tela cheia, do P7.

#### 2.13.1 Janelas

| Janela | Tipo | Por quê |
|---|---|---|
| Personagem | `Window` WPF sem borda, do tamanho do sprite, com `AllowsTransparency`; a imagem tem alfa real, e a janela não ativa | O WPF usa o caminho layered para transparência por pixel; o P1 confirmou o click-through no Windows alvo |
| Painel de energia | Janela WPF própria, compacta e ativável, ancorada ao lado do personagem, só com as três opções | O personagem não ativa; o painel recebe foco depois da ação explícita no menu (DEC-025) |
| Configurações | Janela WPF comum, aberta pelo menu | Controles e navegação de teclado do framework |
| Menu de contexto e bandeja | Menu nativo (`TrackPopupMenuEx`) com janela dona temporária, com os submenus da emoção dominante e dos itens e ícones em bitmap (seção 2.16); ícone da bandeja pelo adaptador, com `Shell_NotifyIcon` versão 4 (DEC-016) | Fecha ao clicar fora e devolve o foco mesmo aberto pelo personagem ou por um item, que não ativam; teclado e leitor de tela prontos; sem dependência de terceiros |
| Item do tamagotchi (DEC-028) | `Window` WPF sem borda, uma por item, de 48 × 48 DIP, com `AllowsTransparency`, sempre no topo, janela de ferramenta e não ativa (`WS_EX_NOACTIVATE`, `MA_NOACTIVATE`, `ShowActivated` falso); `WM_GETDPISCALEDSIZE` devolve o tamanho do item no DPI novo. Fica logo abaixo do personagem na ordem Z e no topo só durante o gesto sobre o item (seção 2.7) | A mesma receita da janela do personagem, validada por P1 e P3. Só a raiz a fecha. Minimizada pelo Windows, volta ao normal na hora, sem esconder o personagem; a correção de DPI e a da minimização da Fase 5 (passos P12 e P14) valem também para ela |

A janela do personagem tem o tamanho do sprite, nunca o da tela, e a de um item, o do item: a documentação recomenda janelas layered pequenas, porque cada atualização copia o bitmap inteiro, e há relatos de atraso de mouse no sistema todo com overlay de tela cheia.

#### 2.13.2 Onde cada componente da seção 2.2 mora

| Componente | Realização |
|---|---|
| Núcleo do personagem, mundo do desktop, arbitragem de input, movimento, personalidade e esquema de configurações | Biblioteca C# pura, sem referência a WPF nem a APIs Windows; recebe geometria e tempo como entrada e é testada sem janelas |
| Adaptador de plataforma | Único módulo que traduz eventos WPF e chama APIs Windows: captura e foco do mouse, topologia, DPI, bandeja, sessão, energia e caminho dos dados |
| Apresentação | Janela e composição visual WPF, com imagem transparente do tamanho do sprite; o desenho não fica ativo quando o estado não muda |
| Configurações e persistência | O esquema no núcleo (`Buzzy.Core.Persistencia`), com `JsonDocument` e `Utf8JsonWriter`, sem `JsonSerializer` nem reflexão; a pasta e o arquivo em `Buzzy.App/Plataforma`; a agenda de gravação na raiz (seção 2.12) |
| Raiz de composição | Inicialização WPF e ligação entre janelas, adaptador e núcleo; agenda trabalho só enquanto necessário |

#### 2.13.3 Correspondência com as APIs do Windows

Cada linha liga uma decisão das seções anteriores ao mecanismo que a realiza, com APIs documentadas pela Microsoft; nenhuma exige elevação.

| Assunto | Seção | API |
|---|---|---|
| Transparência por pixel e clique que atravessa | 2.7 | `Window.AllowsTransparency` em janela WPF sem borda; o framework cria a janela layered. P1: os pixels alfa 0 deixam o clique passar a outro processo |
| Modo fantasma, click-through total | Q-21 | Estilo transparente na janela layered. **Fora do MVP**, por decisão do usuário: o personagem poderia ficar inacessível |
| Não roubar foco | 2.7 | Responder "não ativar" à ativação por mouse, com o estilo que evita ativação, na janela do personagem e nas dos itens |
| Arraste | 2.7 | `SetCapture` ao pressionar, movimento do mouse, `ReleaseCapture` ao soltar e a mensagem de mudança de captura como ponto único de término. Nunca o laço de mover do sistema, que congela a física |
| Clique ou arraste | 2.7 | Retângulo de arraste do sistema, lido para o DPI do monitor, e o tempo de clique duplo do sistema |
| Coordenadas com sinal | 2.4 | Extrair as coordenadas das mensagens com as macros que preservam o sinal |
| DPI por monitor | 2.4 | Per-Monitor V2 declarado no manifesto, não por chamada de função; tratar a mudança de DPI aplicando o retângulo sugerido |
| Topologia | 2.4 e 2.8 | Enumerar monitores e ler a informação de cada um, com a área útil; mensagens de mudança de vídeo, de DPI e da área útil, com agrupamento de rajadas (300 ms, teto de 1 s), a `TaskbarCreated` pela mesma releitura e uma conferência tardia de disparo único; uma falha ao ler a informação ou o DPI de um monitor torna a leitura incoerente, e só a leitura parcial, o último recurso, deixa esse monitor de fora |
| Monitor que contém um ponto | 2.4, 2.7 e 2.8 | Consulta por ponto que retorna nulo fora de qualquer monitor, para detectar o vão, e a variante que retorna o mais próximo, para prender a posição |
| Identidade estável do monitor | 2.4 | Configuração de vídeo, só leitura: `GetDisplayConfigBufferSizes` e `QueryDisplayConfig` com os caminhos ativos, e `DisplayConfigGetDeviceInfo` para o nome GDI da fonte e o caminho do dispositivo do alvo. O caminho vira um resumo na hora e nunca é gravado nem registrado (DEC-030); o nome amigável e os códigos do EDID nunca são lidos; nunca o identificador de execução, o do adaptador nem o nome de vídeo, que não são estáveis. As três APIs ficam numa seção própria de `Win32.cs` e não estão na lista proibida; as que mudam a configuração de vídeo estão (SECURITY.md 3.2) |
| Bandeja | Q-03 | Ícone de notificação versão 4, identificado por janela e número, recriado quando a barra de tarefas reinicia |
| Menu com submenus e ícones | 2.16 | `InsertMenuItemW` com posição explícita e `MENUITEMINFO` (80 bytes em x64); cada submenu é anexado logo depois de criado, para o `DestroyMenu` do principal destruí-lo junto, e sem `MNS_CHECKORBMP`, para a marca de rádio e o ícone ficarem lado a lado. O ícone é um DIB de 32 bits (`CreateDIBSection` sem DC, `BI_RGB`, `BITMAPINFOHEADER` de 40 bytes, altura positiva, linhas invertidas, preenchido por `Marshal.Copy`, sem `BitBlt`, `StretchBlt` nem `CreateDC`), apagado com `DeleteObject` depois do `DestroyMenu`, que não apaga o bitmap de um item. As três APIs ficam em `Win32.cs` e não estão na lista proibida (SECURITY.md 3.2) |
| Ordem Z das janelas dos itens | 2.7 | `SetWindowPos` sem ativar: logo abaixo da janela do personagem ou, durante o gesto, `HWND_TOPMOST`; na releitura e na conferência tardia, só o lugar, com `SWP_NOZORDER`. Sem P/Invoke novo |
| Sem botão na barra de tarefas | Q-03 | Estilo de janela de ferramenta |
| Fim de sessão | 2.6 | O WPF só expõe `SessionEnding`, na pergunta de encerramento; a gravação acontece nele, na hora (DEC-016, item 10) |
| Bloqueio e desbloqueio de sessão | 2.6 | Registro de notificação de sessão da estação de trabalho (`WTSRegisterSessionNotification`); com a notificação de energia, um dos dois itens que exigem registro explícito; o resto chega nas mensagens comuns da janela |
| Suspensão e tela desligada | 2.6 | Notificação de suspensão e retomada (`WM_POWERBROADCAST`) e notificação de estado da tela da sessão, para parar de desenhar com o monitor desligado |
| Repouso | 2.12 | A fila de mensagens bloqueia quando não há nada a fazer; animação com timer que o sistema pode agrupar, nunca elevando a resolução global do timer |
| Pasta de dados | 2.12 | `Environment.GetFolderPath(LocalApplicationData, DoNotVerify)`, sem montar o caminho com texto; um resultado vazio ou relativo deixa o Buzzy sem pasta, sem configurações e sem log |
| Gravação atômica | 2.12 | `File.Replace` e `File.Move` da biblioteca base (`ReplaceFileW`, `MoveFileExW`) e `FileStream.Flush(true)`; sem P/Invoke novo nem regra do portão afetada; exige NTFS local |
| Mudança da janela em primeiro plano | DEC-013 | Eventos WinEvent de primeiro plano e de mudança de geometria, fora do processo observado; filtrar a janela de nível superior ativa e emitir só os monitores que ela cobre. **Risco a medir no P7:** o evento de geometria assinado para todo o sistema também dispara com o cursor e outras janelas, o que pode acordar o Buzzy continuamente; se o P7 confirmar, restringir a assinatura à janela ativa (reassinando a cada troca de primeiro plano) ou revisar o desenho |
| Janela em tela cheia e monitor ocupado | Q-09 | `GetForegroundWindow`, `GetWindowRect`, `MonitorFromWindow`/`GetMonitorInfo`, comparando o retângulo ativo com os limites do monitor; `SHQueryUserNotificationState` pode ser sinal auxiliar, nunca a única fonte |

#### 2.13.4 Apresentação e repouso

O WPF apresenta o sprite numa janela layered. O app suspende `CompositionTarget.Rendering`, animações e timers periódicos quando nada muda e os reativa só durante movimento, animação ou arraste; otimizações de desenho são escolhidas com dados, na Fase 6.

- **Inscrição:** a raiz só se inscreve em `CompositionTarget.Rendering` enquanto o núcleo pede o relógio (`LigarRelogio`) e cancela no `DesligarRelogio`; em repouso, inclusive em `RESTING`, não há inscrição.
- **Lote por quadro:** a cada quadro, o tempo decorrido vira passos fixos, que entram num lote na fila do núcleo; só o último `MoverJanela` do lote é aplicado, e o sprite só é trocado quando o quadro muda (pose, cara, item, sobreposição, fase, deformação, giro ou DPI). Sem relógio, a sobreposição fica na fase 0, e nada no sprite muda sozinho.
- **Atraso:** o acumulador recupera no máximo 250 ms; o excesso é descartado e registrado no log.
- **Disparos únicos:** a gravação com atraso e as novas tentativas dela, a releitura da topologia e as novas tentativas dela e a conferência tardia usam o mesmo disparo único (`DisparoUnico`): o temporizador para antes de chamar a ação, e o encerramento cancela o que estiver armado. A releitura roda na prioridade normal do `Dispatcher`, e a gravação, na de fundo. O temporizador da onda é um `DispatcherTimer` de disparo único, ligado só entre um agendamento e o disparo dele.
- **Log de diagnóstico**, só com `--diagnostico`, até 1 MB com uma rotação e sem dado pessoal (SECURITY.md 6). A posição só é registrada quando o personagem para, fora do movimento e do arraste. É contrato dos testes de integração e da verificação de tela:
  - `MENU|exibindo`, com a emoção marcada (`emocaoMarcada=`, o nome ou `Automatica`) e o número de ícones; `MENU|fechado=`, com o nome do comando de `ComandoDoMenu`, o `argumento=` da emoção ou do item escolhido, `icones`, `bitmapsCriados`, `bitmapsApagados`, `lado` (o rosto), `ladoItem` e `dpi`, gravado depois de apagar os bitmaps; falhas do Windows como `MENU|icone=falhou`, `item`, `submenu` ou `menu`, só com o código;
  - `ITEM`: `mostrado` (com o HWND, o retângulo, o DPI e os pontos de teste opaco e transparente), `movido=Id|parado=sim` (uma linha por pouso no chão, no fim do processamento, mesmo quando o passo do pouso não move a janela, e depois do `RELOGIO|ligado=nao` do mesmo passo; os testes de integração a exigem), `escondido`, `removido` com o motivo, `solto` (o fim do gesto, com `sobre`, `usado` e a latência M5 do arraste do item), `clique`, `capturaPerdida`, `capturaLiberada`, `menuPedido`, `minimizado`, `fechadas` no encerramento e `desconhecido=sim` para um Id que a raiz não conhece; `ITEM|reaplicado=Id`, uma linha por janela de item devolvida ao lugar, com `motivo`, `real` e `nucleo`; `monitor` é a chave opaca;
  - `ONDA`: `agendada` (com o atraso e a geração), `disparada` e `cancelada`;
  - `PARANOIA`: uma linha por sorteio da paranoia, saindo ou não, com `item`, `chance` (`1 em 8`), `saiu` (`sim` ou `nao`), `substancias` e `distintas` (os itens de substância distintos do episódio, na ordem do enum), por exemplo `PARANOIA|item=Bala|chance=1 em 8|saiu=sim|substancias=2|distintas=Vodka,Bala`. A raiz compara o gerador da paranoia antes e depois de cada evento do lote (`LigacaoDosItens.SorteioDaParanoia`) e escreve a linha quando ele andou; a subida de nível com a paranoia na frente não gera linha;
  - o baseado por conta própria não tem linha própria nem linha `ITEM`: a linha `NUCLEO` da transição traz a regra (`NUCLEO|evento=AutonomyTimer|…|de=Idle|para=Using|regra=IDLE + AUTONOMY_TIMER: Fumar Baseado por conta própria`), `ONDA|agendada` traz a subida do chapado, de 10.000 ms, `SPRITE` traz os quadros `fumando-*` com `item=baseado` e `efeito=Fumaca`, e `PARANOIA` traz `item=Baseado` quando o baseado dele faz o sorteio do episódio;
  - `SPRITE`: uma linha por quadro desenhado fora do cache, com `item`, `efeito`, `fase`, `bytesEmCache` e `descartados`; `MEMORIA`, com `bytesEmCache` e `janelasDeItens`;
  - `TOPOLOGIA`, na partida (`motivo=início`), a cada releitura publicada e na releitura imediata do mostrar (`imediata=sim`): `motivo` (as mensagens agrupadas, na ordem em que chegaram, no máximo 32, com `,+k` quando outras passam do teto), `mudou` e `visivel` (nas releituras), `impressao` e os campos das chaves: `chaves=chave=\\.\DISPLAYn;…`, na ordem do Windows; `consulta`, `ok` ou a falha, só com a função e o código; as contagens `cache`, `reserva` e `semNome`; e, numa leitura parcial, `ignorados` e `falhaDoIgnorado`. Uma leitura incoerente sai como `TOPOLOGIA|motivo|erro|mantida=anterior|novaTentativa`;
  - `MENSAGEM|tipo=`: uma linha por mensagem que pode ter mudado a topologia ou por evento de sessão e energia, só com o tipo (`WM_DISPLAYCHANGE`, `SPI_SETWORKAREA`, `WM_DPICHANGED` com o DPI novo, `TaskbarCreated`), crua, para calibrar o agrupamento no protótipo P5;
  - `POSICAO`: `monitor` é a chave opaca, e `gdi`, o nome GDI dessa chave na última leitura. `POSICAO|reaplicada=sim` leva o `motivo` (as mensagens da releitura ou `reafirmação tardia`), o retângulo `real` e o do `nucleo`;
  - `CONFIG`: na partida, `lido` (`principal`, `reserva`, `padroes` ou `desligado`, este com `motivo=opcao`, `pasta` ou `erro` e, no erro, o tipo e o código), os estados do `principal` e da `reserva`, `versao` (`atual`, `anterior`, `futura` ou `-`, nunca o número lido), `avisos`, `tentativas`, `gravacao` (`bloqueada` ou `liberada`) e `pasta` (`perfil` ou `padrao`); a cada pedido, `pedido` (`posicao` ou `preferencias`), `evento`, `imediata` e `gravacao` (`ligada`, `bloqueada` ou `desligada`); a cada gravação, `gravado` (`sim`, `nao` ou `sem mudanca`), `motivo`, `bytes`, `ms`, `principalAntes`, `copiaDeDiagnostico`, `tentativas`, `erro` (tipo e código) e `novaTentativaMs`; e `adiado=gesto` quando o disparo chega no meio de um gesto. Nunca a pasta, a chave, a tela nem valores do arquivo.
- **Leitura do log nos testes:** o log rotaciona ao passar de 1 MB (DEC-016, item 7). Os testes de integração e a verificação de tela leem pelo mesmo código (`LeituraDoLog`): a marca guarda o deslocamento e uma assinatura dos 64 bytes antes dele, e a leitura reconhece a rotação pelo conteúdo, lendo o resto de `diagnostico.1.log` antes do arquivo novo. Supõe que só o Buzzy rotaciona ou apaga o log durante uma execução. A medição de desempenho mantém a leitura própria, que avança a cada leitura.

#### 2.13.5 Limites que os protótipos precisam esclarecer

- P1 confirmou o clique por pixel em janela WPF e a regra de alfa dos assets (seção 2.13.7, item 7); P2, que o desenho e os timers param de acordar o processo em repouso, base das metas de desempenho; P3, o arraste sem ativar a janela nem roubar foco (se a única forma de arrastar alterasse o foco, o P3 falharia).
- A conversão entre DIPs do WPF e pixels físicos fica no adaptador; o P6 verifica a transição real entre monitores de escalas diferentes.
- O menu e a bandeja usam a API da Shell e o menu nativo (DEC-016); nenhum pacote de terceiros sem justificar, fixar versão e revisar a dependência.

#### 2.13.7 Regras que valem para qualquer stack aprovada

1. Nenhum overlay do tamanho da tela; a janela tem o tamanho do sprite.
2. Nenhum hook global de teclado ou mouse, nenhuma leitura de input em segundo plano e nenhuma captura de tela. A única exceção é o observador WinEvent da DEC-013, fora do processo, limitado às mudanças de primeiro plano e de geometria; ele não lê conteúdo e entrega ao núcleo só os monitores ocupados.
3. Topologia em cache, atualizada por evento com agrupamento de rajadas, nunca por consulta periódica.
4. Física em unidades independentes de dispositivo, convertidas pela escala do monitor da âncora.
5. Estado de repouso explícito, sem timer de intervalo curto.
6. Persistência incremental e atômica na pasta local do usuário.
7. O asset não tem área grande de alfa baixo, porque só alfa exatamente 0 é transparente ao clique. Sombra, se houver, fica em janela separada, sem interação.

### 2.14 Limites de complexidade

O MVP usa um processo, arquivos locais e módulos de código. Backend, banco de dados, microsserviços, sistema de plugins, comunicação entre processos própria e interfaces genéricas de provedores ficam fora; IA no produto também (DEC-003), sem arquitetura de extensão para ela.

### 2.15 Sem IA no aplicativo

O aplicativo é um mascote local, determinístico e sem IA: nada de conversa, chat, campo de texto, respostas escritas, voz, reconhecimento de fala, LLM, RAG, APIs de IA, modelos locais, geração de conteúdo ou memória. Só se reabre por pedido explícito do usuário (DEC-003).

### 2.16 Emoção dominante e tamagotchi adulto: tabelas e regras

Decisões em DEC-027 e DEC-028; o tamagotchi fica atrás da chave `ConfiguracaoDoNucleo.Tamagotchi`, ligada no aplicativo, e a emoção dominante não tem chave. Estados, eventos, transições e invariantes estão na seção 2.6; a execução no app, nas seções 2.3, 2.7, 2.10 e 2.13. O estado de verificação está em TODO.md, seção "Interação".

Os números são de jogo, escolhidos para o comportamento se ler na tela, sem relação com nada real; as classes dos itens e das ondas também são regra de jogo, pedida pelo usuário, e não dizem nada sobre o mundo real. As tabelas ficam em `TabelaDoTamagotchi`, e os testes podem trocá-las (`ConfiguracaoDoNucleo.TabelaDeItens` e `TabelaDeOndas`), como a chance da paranoia (`ChanceDaParanoia`).

**Itens**, na ordem do menu. Os passos do uso são do relógio, a 60 por segundo, e são os do verbo. A cara durante o uso vale no retrato; no chão, a arte mostra a cara de cada quadro de uso. A intensidade é quantos níveis o item soma à onda.

| Item | Verbo | Passos do uso | Cara durante o uso | Onda | Intensidade | Classe |
|---|---|---|---|---|---|---|
| Banana | comer | 150 | Feliz | Satisfeito | 1 | alívio |
| Agua | beber | 120 | Feliz | nenhuma: alivia a que houver | 0 | alívio |
| Vodka | beber | 120 | Determinado | Bebado | 2 | substância |
| Cerveja | beber | 120 | Feliz | Bebado | 1 | substância |
| Baseado | fumar | 210 | Pensativo | Chapado | 2 | substância |
| Cigarro | fumar | 210 | Pensativo | Relaxado | 1 | substância |
| Cocaina | cheirar | 120 | Surpreso | Eletrico | 2 | sintética |
| Md | engolir | 90 | Travesso | Euforico | 2 | sintética |
| LancaPerfume | inalar | 120 | Surpreso | Tonto | 2 | sintética |
| Cafe | beber | 120 | Determinado | Ligado | 1 | alívio |
| Energetico | beber | 120 | Empolgado | Ligado | 2 | alívio |
| Cogumelo | comer | 150 | Curioso | Viajando | 2 | substância |
| Bala | engolir | 90 | Feliz | Euforico | 1 | sintética |

*Classe do item* (`DadosDoItem.Alivio` e `Sintetica`): o alívio é a comida e a bebida sem álcool; a droga sintética ("como bala, md, coca e lança", nas palavras do usuário) também é substância; o resto é substância, inclusive o cogumelo, mesmo comido. A bala começa o Euforico no nível 1 (o MD, no 2); a onda Alegre ficou sem item e continua no enum e nas tabelas, no mesmo lugar.

**Ondas:**

| Onda | Precedência | Subida | Cada nível do pico | Queda base | Cara na subida | Cara no pico | Cara na queda | Classe |
|---|---|---|---|---|---|---|---|---|
| Satisfeito | 1 | 3 s | 60 s | — | Feliz | Feliz | — | leve |
| Alegre | 1 | 2 s | 40 s | 20 s | Empolgado | Empolgado | Entediado | leve |
| Relaxado | 1 | 3 s | 60 s | — | Pensativo | Pensativo | — | substância |
| Ligado | 2 | 5 s | 75 s | 45 s | Surpreso | Determinado | Sonolento | leve |
| Bebado | 3 | 8 s | 100 s | 90 s | Feliz | Bebado | Enjoado | substância |
| Chapado | 3 | 10 s | 110 s | 90 s | Pensativo | Chapado | Sonolento | substância |
| Eletrico | 3 | 3 s | 75 s | 90 s | Surpreso | Eletrico | Entediado | substância |
| Euforico | 3 | 15 s | 110 s | 120 s | Feliz | Apaixonado | Entediado | substância |
| Tonto | 3 | 1 s | 15 s | 10 s | Surpreso | Tonto | Sonolento | substância |
| Viajando | 3 | 20 s | 140 s | 60 s | Curioso | Viajando | Pensativo | substância |
| Paranoico | 4 | 1 s | 40 s | 15 s | Assustado | Paranoico | Sonolento | substância |

*Classe da onda* (`DadosDaOnda.DeSubstancia`): a de substância é a que a banana, o café e o energético acalmam e a que mantém aberto o episódio da paranoia; a leve não mantém o episódio, e a comida e a bebida combinam com ela como antes. A água acalma as duas. A `Paranoico` só vem do sorteio da paranoia, e só ela tem a precedência 4.

*Tempos:* a onda nova começa na subida, no nível da intensidade do item; da subida, vai ao pico; no pico, cada disparo baixa um nível; no nível 1, vai para a queda, também no nível 1, ou acaba, se a onda não tem queda; a queda acaba no disparo seguinte e dura a base × 100%, 125% ou 150%, pelo pior nível atingido no episódio (1, 2 ou 3); nenhuma fase dura menos de 1 s. Sem item novo, a onda gera no máximo 2 + nível disparos; o episódio mais longo é o do Viajando no nível 3: 20 + 3 × 140 + 90 = 530 s.

**Perfis do pico** (percentuais sobre o perfil de energia, 100 = igual; "a/b/c" = níveis 1, 2 e 3; "perfil" = a chance do foguete do perfil de energia). A subida usa o perfil do nível 1, só com a cara da subida.

| Onda | Intervalo | Descanso | Andar | Escalar | Pular | Descansar | Gesto | Troca de cara | Velocidade | Cambaleio | Foguete | Altura do pulo |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Satisfeito | 100 | 100 | 100 | 100 | 100 | 150 | 150 | 150 | 100 | 0 | perfil | 100 |
| Alegre | 60/50/40 | 60 | 130 | 130 | 200 | 30 | 150 | 100 | 120/125/130 | 0 | perfil | 120 |
| Relaxado | 130 | 120 | 70 | 50 | 30 | 150 | 120 | 100 | 90 | 0 | perfil | 100 |
| Ligado | 70/55/40 | 60/45/30 | 150 | 150 | 150/200/250 | 30/15/5 | 120 | 100 | 115/130/145 | 0 | 40/50/60 | 110/125/140 |
| Bebado | 100 | 120 | 130 | 40 | 40 | 120 | 200 | 150 | 80/70/60 | 60/90/120 | 5 | 80 |
| Chapado | 160 | 150 | 60 | 30 | 20 | 200 | 150 | 120 | 60/55/50 | 0 | 0 | 80 |
| Eletrico | 35/28/20 | 30/20/10 | 200 | 200 | 180 | 10/5/5 | 150 | 200 | 170/185/200 | 0 | 60/70/80 | 120/130/140 |
| Euforico | 60 | 50 | 120 | 100 | 150 | 30 | 250 | 150 | 120 | 0 | perfil | 120 |
| Tonto | 50 | 100 | 50 | 0 | 0 | 100 | 300 | 200 | 60 | 100 | 0 | 100 |
| Viajando | 130 | 120 | 80 | 80 | 60 | 100 | 200 | 250 | 70 | 0 | 20 | 100 |
| Paranoico | 60 | 30 | 50 | 0 | 0 | 0 | 300 | 150 | 70 | 0 | 0 | 100 |

No pico da paranoia, igual nos três níveis, ele decide mais vezes, gesticula muito, anda devagar e nunca escala, pula nem descansa: um peso a 0% vira 0.

**Perfis da queda**, iguais em todos os níveis, com a altura do pulo em 100:

| Queda de | Intervalo | Descanso | Andar | Escalar | Pular | Descansar | Gesto | Troca de cara | Velocidade | Cambaleio | Foguete |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Alegre | 120 | 150 | 80 | 60 | 50 | 200 | 80 | 100 | 90 | 0 | perfil |
| Ligado | 130 | 150 | 70 | 50 | 40 | 200 | 80 | 100 | 85 | 0 | perfil |
| Bebado | 150 | 200 | 60 | 20 | 10 | 250 | 80 | 80 | 75 | 30 | 5 |
| Chapado | 150 | 200 | 60 | 30 | 20 | 300 | 80 | 100 | 70 | 0 | 0 |
| Eletrico | 150 | 180 | 60 | 40 | 30 | 250 | 80 | 100 | 80 | 0 | perfil |
| Euforico | 140 | 150 | 70 | 60 | 50 | 200 | 80 | 100 | 85 | 0 | perfil |
| Tonto | 100 | 100 | 80 | 50 | 50 | 120 | 80 | 100 | 80 | 0 | perfil |
| Viajando | 120 | 120 | 80 | 70 | 60 | 150 | 100 | 150 | 85 | 0 | perfil |
| Paranoico | 120 | 150 | 80 | 50 | 50 | 150 | 100 | 100 | 85 | 0 | perfil |

**Gestos e caras por fase** (pesos). Na subida, a cara é só a da subida, e os gestos são os do pico.

| Onda | Gestos no pico | Caras no pico | Gestos na queda | Caras na queda |
|---|---|---|---|---|
| Satisfeito | Cocar 2, Espreguicar 2, Brincar 1 | Feliz 4, Rindo 1, Travesso 1, Sonolento 1 | — | — |
| Alegre | Brincar 2, Danca 2, Gargalhada 1 | Empolgado 3, Rindo 2, Feliz 2 | Espreguicar 1 | Entediado 2, Sonolento 2 |
| Relaxado | Espreguicar 2, Tosse 1, OlharAoRedor 1 | Pensativo 2, Neutro 2, Sonolento 1, Feliz 1 | — | — |
| Ligado | Tremedeira 2, OlharAoRedor 2, Brincar 1 | Determinado 2, Empolgado 2, Surpreso 1 | Espreguicar 2, Cocar 1 | Sonolento 3, Bocejando 1 |
| Bebado | Soluco 3, Danca 1, Gargalhada 1 | Bebado 4, Rindo 2, Feliz 1, Sonolento 1 | Soluco 1, Espreguicar 1 | Enjoado 3, Sonolento 2, Entediado 1 |
| Chapado | Gargalhada 3, OlharAoRedor 1, Cocar 1 | Chapado 4, Rindo 2, Pensativo 1, Sonolento 1 | Espreguicar 2 | Sonolento 3, Bocejando 2, Pensativo 1 |
| Eletrico | Tremedeira 3, Espirro 1, OlharAoRedor 1 | Eletrico 4, Determinado 1, Surpreso 1, Empolgado 1 | Espreguicar 1, Cocar 1 | Entediado 3, Sonolento 2, Pensativo 1 |
| Euforico | Danca 4, Brincar 1 | Apaixonado 3, Empolgado 2, Feliz 1, Rindo 1 | Espreguicar 1, OlharAoRedor 1 | Entediado 2, Pensativo 2, Sonolento 1 |
| Tonto | Gargalhada 2, OlharAoRedor 1 | Tonto 4, Rindo 2 | OlharAoRedor 1 | Sonolento 1, Surpreso 1, Neutro 1 |
| Viajando | OlharAoRedor 2, Danca 1, Espiar 1 | Viajando 4, Surpreso 1, Pensativo 1, Curioso 1, Rindo 1 | OlharAoRedor 2, Espiar 1 | Pensativo 3, Curioso 1, Sonolento 1 |
| Paranoico | OlharProTeto 4, Agachar 3, Tremedeira 2, OlharAoRedor 2, Espiar 1 | Paranoico 5, Assustado 2, Surpreso 1 | OlharAoRedor 2, Espreguicar 1 | Sonolento 2, Pensativo 1, Neutro 1 |

Cada troca de cara e cada gesto da onda gastam os mesmos sorteios de sempre: um sorteio ponderado no lugar de cada sorteio uniforme. O olhar pro teto do fim do uso que começou a paranoia não sorteia nada.

**Perfil efetivo e física efetiva** (`Maquina.PerfilEfetivo` e `Maquina.FisicaEfetiva`). Sem onda, ou com a chave desligada, valem o perfil de energia e a física de sempre. Com onda, pela fase dela:
- os intervalos entre decisões e de descanso vão a milissegundos inteiros × percentual / 100, truncados;
- os pesos de andar, escalar, pular, descansar, gesto e troca de cara vão a (peso × percentual + 50) / 100, arredondados; um peso positivo nunca vira 0, a não ser a 0%;
- a altura mínima e a máxima do pulo são arredondadas do mesmo jeito, com pelo menos 1 DIP;
- a chance do foguete é a da fase ou, sem ela, a do perfil;
- o tempo na parede e o tempo pendurado vão a × 100 / velocidade, só com a velocidade abaixo de 100%; senão, ficam os mesmos;
- o piso de 3 s do intervalo de acomodação continua valendo em todo agendamento;
- na física, só as velocidades de andar, escalar e pendurar mudam, × velocidade / 100.

O peso de atravessar monitores, quando a travessia entrar (passo P13 da Fase 5), segue o percentual de andar.

**Combinação, quando ele usa um item** (`AplicarNaOnda`: primeiro `Combinar`, com o alívio; depois a paranoia, no parágrafo seguinte):
- **alívio:** um item de alívio com onda na frente a acalma um passo e não começa a onda dele. A água acalma qualquer onda; a banana, o café e o energético, só uma onda de substância. O passo (`UmPassoAbaixo`): na subida ou no pico acima do nível 1, um nível abaixo, na mesma fase, sem mexer no temporizador; no nível 1, da subida ou do pico, a queda, com o mesmo pior, a duração cheia e a cara dela, ou o fim, se a onda não tem queda; na queda, o fim, e a de fundo volta, como no fim pelo temporizador. A de fundo nunca é tocada, e nada é sorteado. A regra do soltar termina em `; alivia Tipo/Fase/Nível -> Tipo/Fase/Nível`, `-> fim` ou `-> fim; a de fundo volta: …`. A água sem onda não faz nada; com uma onda leve na frente, a banana, o café e o energético seguem as linhas abaixo;
- **sem onda:** começa a do item, na subida, no nível da intensidade;
- **mesmo tipo da onda da frente:** os níveis somam até 3, o pior nível acompanha, e a fase recomeça: a subida continua subida, e o pico ou a queda viram pico;
- **mesmo tipo da onda de fundo:** os níveis dela somam, do mesmo jeito, e ela continua congelada;
- **precedência maior ou igual à da frente:** a do item vai para a frente, na subida; a da frente vira a de fundo, congelada, e a de fundo anterior é descartada;
- **precedência menor:** o item é absorvido; a onda e o temporizador não mudam.

Quando a onda da frente acaba, a de fundo volta à frente com a fase em que estava recomeçada na duração cheia e com a cara dessa fase; sem onda de fundo, a cara volta à de base, se está livre.

**Paranoia, depois da combinação** (`Maquina.Onda.cs`; DEC-028, itens 28 a 34). De desenho animado: ele acha que tem alguém no teto.
- **Carga do episódio** (`EstadoDoNucleo.Carga`, registro `CargaDaParanoia`): todo item de substância, isto é, que não é de alívio, entra na carga depois da combinação: uma substância a mais, a sintética se ele for e o item entre os distintos (`ConjuntoDeItens`, com igualdade por valor). No fim de todo evento, com a chave ligada, se nem a onda da frente nem a de fundo é de substância, a carga volta toda a `CargaDaParanoia.Nenhuma` (substâncias, sintética, distintos e o sorteio feito, `Sorteada`, juntos). A paranoia é de substância, então o episódio dura enquanto ela durar.
- **Mistura com sintética** (`MisturaComSintetica`): pelo menos uma sintética e pelo menos dois itens de substância distintos no episódio, contando o atual. Álcool, maconha, cigarro e cogumelo, sozinhos ou misturados entre si, e uma sintética sozinha, repetida, nunca são mistura com sintética.
- **Com a paranoia na frente:** qualquer substância a sobe um nível, até 3; o pior acompanha, e a fase recomeça como no mesmo tipo (a queda volta ao pico; a subida continua subida), com o temporizador na duração cheia. Sem sorteio. A regra termina em `; a paranoia sobe: Paranoico/Pico/1 -> Paranoico/Pico/2`.
- **O sorteio único do episódio:** sem a paranoia na frente, o uso de substância que fecha a mistura com sintética, num episódio que ainda não sorteou, faz o sorteio com a chance `ConfiguracaoDoNucleo.ChanceDaParanoia` (1 em 8). É um passo do gerador próprio da paranoia (`EstadoDoNucleo.AleatorioDaParanoia`; `Aleatorio.Sortear`: um inteiro uniforme de 1 a 8, que sai se for 1), semeado com semente × 41 + 13 (`SementeDaParanoia`, em aritmética de 64 bits sem sinal); o gerador principal nunca é usado, e a agenda, as caras e as reproduções gravadas não mudam por causa dele. O gerador da paranoia não volta ao começo com o episódio. Saindo ou não, o episódio fica marcado como sorteado e não sorteia mais até a carga zerar; o seguinte sorteia de novo.
- **Se sai:** a onda `Paranoico` vai à frente, na subida do nível 1; a frente vai para o fundo, congelada, e a de fundo anterior sai. A regra termina em `; a paranoia começa: Paranoico/Subida/1`, e o uso fica marcado (`Uso.ComecouAParanoia`) para o olhar pro teto no fim dele (seção 2.6). O sorteio que não sai não deixa texto; a raiz o registra na linha `PARANOIA` do log (seção 2.13.4).
- **O resto** é o de toda onda: os disparos do temporizador, o alívio que acalma um passo e a de fundo que volta quando ela acaba. Preso, continua preso; escondido, continua escondido.
- **Um caso de borda:** o cigarro absorvido pelo ligado do café ou do energético, sem onda de substância no fundo, entra na carga e a zera no fim do próprio soltar; assim, café, cigarro e MD nunca sorteiam, e cigarro e MD sorteiam.

**Baseado por conta própria** (`Maquina.Itens.cs`; DEC-028, itens 36 a 42). De desenho animado: às vezes, parado no chão, ele fuma um baseado sozinho.
- **A ação** `AcoesAutonomas.FumarBaseado` (64, no fim do enum) fica fora de `Todas`; só a configuração do aplicativo a liga, e ela só vale com a chave ligada.
- **A decisão** (`DecidirParado`): é a última opção do sorteio de `IDLE`, com o peso `PerfilDeEnergia.PesoFumarBaseado` (1 nos três níveis; a onda não o muda) quando ele pode fumar (`PodeFumarPorContaPropria`), e peso zero quando não pode. Pode com tudo junto: a chave ligada; `IDLE`; a âncora na borda de baixo da área útil do monitor dele; fora do esconderijo; a autonomia livre e o painel fechado; nenhum item na mão do usuário; e nem `Chapado` nem `Paranoico` na frente. Só a onda da frente conta: com o `Chapado` no fundo, ele fuma, e o fundo soma o nível, até 3.
- **O uso** (`FumarPorContaPropria`, que chama `ComecarOUso`, o caminho do soltar): o uso do baseado da tabela (`TabelaDeItens(Item.Baseado)`: fumar, 210 passos, a cara pensativa), sempre com o apoio no chão, com a combinação e a paranoia como no soltar do baseado; sem onda na frente, começa o `Chapado` na subida do nível 2. Nenhum item nasce nem sai, nenhum Id é gasto e não há efeito de janela de item. O gerador principal só anda o passo do sorteio da agenda. A regra é `IDLE + AUTONOMY_TIMER: Fumar Baseado por conta própria`, com `; a paranoia começa: Paranoico/Subida/1` no fim quando o sorteio sai. O fim é o de todo uso no chão: `SETTLING`, depois `IDLE`, com o olhar pro teto se o uso começou a paranoia.
- **Na paranoia:** é substância e não é sintética; entra na carga como o baseado solto. Sozinho, ou com álcool, cigarro e cogumelo, nunca sorteia; num episódio com sintética que ainda não sorteou, fecha a mistura e faz o sorteio; uma sintética dada depois, com ele chapado do baseado dele, também fecha a mistura. Como ele não fuma com a paranoia na frente, nunca a sobe.
- **O usuário prevalece** como em todo `USING` (seção 2.6): o clique interrompe, o menu não, e um item solto nele durante o uso é recusado.
- **Frequência:** na Média e só com a autonomia, cerca de um baseado a cada 3,9 min de tempo elegível (`IDLE` no chão, sem `Chapado` nem `Paranoico` na frente), ou um a cada 14,6 min no total, com o `Chapado` na frente cerca de 39% do tempo; os três níveis estão na DEC-028, item 41.
- **Na tela:** os quadros `fumando-1` a `fumando-5` do baseado solto, com o baseado na mão, a cara da pose e a fumaça do chapado; o app não mudou.

**Cambaleio** (`Maquina.FatorDoCambaleio`): o passo da caminhada é multiplicado por 1 + amplitude × (12 − d) / 1200, com d a distância, em passos, até o meio de uma volta de 48 passos (0,8 s). O fator vai de 1 − amplitude/100, no começo da volta, a 1 + amplitude/100, no meio, com média 1 na volta inteira; a 120%, vai de −0,2 a 2,2. Contas inteiras até a última divisão. A âncora fica presa entre as laterais, e o percurso que falta diminui pelo passo com sinal.

**Aceitação do soltar e apoio do uso** (`Maquina.AceitaItem`):

| Estado ao soltar | Resultado | Apoio do uso | Depois do uso |
|---|---|---|---|
| `IDLE` | uso | chão | `IDLE` |
| `WALKING` | uso; o plano é descartado | chão | `IDLE` |
| `CLIMBING`, preso ou não | uso | parede | agarrado à parede, preso se já estava |
| `HANGING`, preso ou não | uso | cipó | agarrado ao cipó, preso se já estava |
| `PEEKING` | uso | esconderijo | `PEEKING` na mesma borda |
| `RESTING` | acorda e usa | chão | `IDLE` |
| `REACTING`, `LANDING` | uso; a reação ou o pouso é cortado | pela posição | pela acomodação |
| `JUMPING`, `FALLING`, `USING`, `PRESSED`, `DRAGGING`, `SETTLING`, `HIDDEN`, `BOOTING`, `EXITING` | recusado: o item cai de onde foi solto | — | — |

O apoio do uso sai primeiro do estado e depois da posição: escondido numa borda, o esconderijo; em `CLIMBING` fora do chão, numa lateral, a parede, mesmo na quina; em `HANGING` na borda de cima, o cipó; senão, pela âncora: no chão, o chão; na borda de cima, o cipó; numa lateral, a parede; no ar, por toon force, o chão, e a acomodação decide no fim. Sem a física, o apoio é o chão ou o esconderijo. A acomodação do fim do uso recebe o apoio e, na parede ou no cipó, volta a ele mesmo a menos de 32 DIP do chão.

**"Sobre ele"** (`Maquina.SobreOPersonagem`): o retângulo do sprite do personagem encolhe 20% de cada lado (`MargemDoAlvo`), da largura nas laterais e da altura em cima e embaixo, arredondado ao pixel com a metade para cima; num sprite de 128 × 128 px, 26 px de cada lado, e sobra um miolo de 76 × 76 px. O item conta como solto sobre ele se o retângulo dele, já preso na área útil, tem ao menos um pixel em comum com esse miolo, com os retângulos semiabertos, como o `RECT` do Windows.

**Companheiras da emoção dominante** (DEC-027): no sorteio, a dominante tem peso 6, e cada companheira, peso 1.

| Dominante | Companheiras |
|---|---|
| Neutro | Feliz, Curioso, Pensativo, Entediado |
| Feliz | Rindo, Empolgado, Travesso, Curioso |
| Rindo | Feliz, Travesso, Empolgado, Surpreso |
| Curioso | Pensativo, Surpreso, Feliz, Travesso |
| Surpreso | Assustado, Curioso, Empolgado, Rindo |
| Assustado | Surpreso, Pensativo, Curioso, Neutro |
| Sonolento | Bocejando, Dormindo, Entediado, Neutro |
| Bocejando | Sonolento, Entediado, Neutro, Pensativo |
| Dormindo | Sonolento, Bocejando, Neutro, Feliz |
| Travesso | Rindo, Feliz, Curioso, Empolgado |
| Entediado | Sonolento, Bocejando, Pensativo, Neutro |
| Pensativo | Curioso, Neutro, Entediado, Determinado |
| Empolgado | Feliz, Rindo, Surpreso, Determinado |
| Determinado | Empolgado, Pensativo, Neutro, Feliz |

**Itens no mundo** (`Maquina.Itens.cs`; o item tem 48 × 48 DIP, `TamanhoDoItem`):
- **Nascimento**, no monitor do personagem: as posições candidatas ficam ao lado dele, a meia largura do personagem + 8 DIP + meia largura do item, e depois mais longe, de uma largura de item + 8 DIP de cada vez, até duas vezes; primeiro do lado para onde ele olha, depois do outro. Vale a primeira dentro dos limites laterais e sem cruzar outro item fora da mão no mesmo monitor; se nenhuma servir, a primeira, presa entre as laterais. A âncora começa 140 DIP acima dos pés dele (no chão, 140 DIP acima do chão), nunca acima da borda de cima. O item nasce caindo, com o próximo Id.
- **Queda,** a cada passo: a gravidade e a queda máxima do personagem, presa entre as laterais; ao tocar o chão a 300 DIP/s ou mais, quica uma vez, com 35% da velocidade, e depois para. Não achata ao pousar.
- **Segurar:** só um item visível; a pegada é o cursor menos a âncora; outro item na mão é largado de onde estava, com a captura solta; pegar um item no ar é aceito: ele para e fica na mão.
- **Arrastar:** a âncora é o cursor menos a pegada, sem prender, no monitor da âncora.
- **Soltar ou largar:** a âncora é presa na área útil do monitor dela; no chão, o item fica; no ar, cai de novo e pode quicar outra vez.
- **Topologia** (DEC-030): a regra do personagem (seção 2.8). No monitor só transladado, o item anda junto e continua caindo ou no chão, com a mesma velocidade; senão, a posição acompanha a topologia (`Rebasear`) e é reacomodada pela posição relativa, no monitor correspondente ou no sobrevivente, e, fora do chão, volta a cair. O item na mão anda com o monitor em que está, com a altura fina junto: soltá-lo logo depois o deixa no mesmo monitor físico.
- **Esconder e sair:** o item da mão solta a captura e fica no chão; os que caíam vão direto ao chão, cada um na coluna em que estava.
- **Janelas:** comparando o começo e o fim do evento, primeiro os que saíram (`RemoverItem`, com o motivo); depois, item a item, na ordem do Id: o que deixou de aparecer (`EsconderItem`), o que passou a aparecer ou nasceu à vista (`MostrarItem`) ou o que mudou de lugar à vista (`MoverItem`, com o monitor e a âncora).

**Menu** (`MenuNativo`). É o mesmo pelo botão direito no personagem, num item e no ícone da bandeja; os textos vêm de `Textos.resx` e só nomeiam as opções:

| Linha | Id | Tecla de acesso | Regra |
|---|---|---|---|
| Esconder Buzzy / Mostrar Buzzy | 1 | E / M | como antes |
| Pausar movimento / Retomar movimento | 3 | P / R | como antes |
| separador | | | |
| Emoção dominante ▸ | — | D | habilitada mesmo com o Buzzy escondido: o núcleo grava a escolha, e a cara aparece quando ele voltar |
| Itens ▸ | — | I | só existe com a chave do tamagotchi ligada; desabilitada com o Buzzy escondido |
| Conteúdo adulto | 7 | A | só com a chave do tamagotchi; marca de seleção quando ligado (DEC-033) |
| separador | | | |
| Sair | 2 | S | como antes |

- **Submenu "Emoção dominante":** "Automática" (id 999, tecla A), um separador e as 14 caras de humor na ordem de `Expressoes.DeHumor` (ids 1000 a 1013): Neutro (N), Feliz (F), Rindo (R), Curioso (C), Surpreso (S), Assustado (U), Sonolento (O), Bocejando (B), Dormindo (D), Travesso (T), Entediado (E), Pensativo (P), Empolgado (M) e Determinado (I). Todas são opções de rádio, com a marca na escolha atual, ou em "Automática" sem dominante; cada cara leva o rosto como ícone.
- **Submenu "Itens":** os 13 itens na ordem do enum `Item` (ids 2000 a 2012): Banana (B), Água (G), Vodka (V), Cerveja (C), Baseado (S), Cigarro (I), Cocaína (O), MD (M), Lança-perfume (L), Café (F), Energético (N), Cogumelo (U) e Bala (A), cada um com o desenho do chão como ícone; um separador; e "Recolher itens" (id 2999, tecla R), desabilitado sem itens na tela. Com o conteúdo adulto desligado, só a banana, a água, o café e o energético, com os mesmos ids.
- **Escolha:** o id que o Windows devolve vira comando, emoção ou item por listas fixas (`Expressoes.DeHumor` e `TabelaDoTamagotchi.Itens`), nunca por conversão do número; qualquer outro id não escolhe nada. A emoção envia `CMD_SET_DOMINANT_EMOTION`, o item `CMD_SUMMON_ITEM`, "Recolher itens" `CMD_CLEAR_ITEMS` e "Conteúdo adulto" `CMD_SET_ADULT_CONTENT` com o contrário do que o usuário leu. O estado que o menu mostra é lido na abertura (`ModeloAoAbrir`), e a escolha vale por ele, o texto que o usuário leu.
- **Ícones:** o rosto é a célula de 40 × 32 pixels de arte de `expressoes.png`, e o item, o desenho do chão de 24 × 24, os dois ampliados por vizinho mais próximo pelo fator inteiro do DPI do monitor em que o menu abre (`DpiAoAbrir`; o DPI dividido por 96, no mínimo 1: 1× até 191 DPI, 2× de 192 a 287, 3× de 288 a 383), porque o Windows não amplia o bitmap de um item de menu. Em alto contraste, só texto. Com a chave ligada, cada abertura cria 27 bitmaps (14 rostos e 13 itens), mesmo com o submenu "Itens" desabilitado, e apaga os 27.
- **Uma abertura:** a lista de entradas é uma função pura (`Entradas`), testável sem o Windows. A montagem (`ComMenuMontado` e `ComMenuMontadoNoDpi`) cria o menu, anexa cada submenu logo depois de criá-lo, põe as linhas por posição explícita, mostra o menu, destrói-o e só então apaga os bitmaps, mesmo com erro. Uma falha do Windows deixa a linha sem ícone, ou de fora, e vai ao log só com o código.

**Reproduções gravadas** (`tests/Buzzy.Core.Testes/Referencias/`), com as diretivas de cabeçalho `# tamagotchi: sim`, `# travessia: sim`, `# movimento: sim` e `# queda-fisica: sim`:
- **`06-travessia.txt`** (semente 606, `# acoes: IrAoOutroMonitor`, topologia `SecundarioAEsquerda`; DEC-032): solto perto da porta, ele vai ao monitor da esquerda; a pausa no meio espera o fim da travessia; na volta, o clique no meio a interrompe, e a acomodação o deixa inteiro num monitor;
- **`07-tamagotchi.txt`** (semente 2028): a emoção dominante; a vodka invocada, pega no chão e usada; o lança-perfume pego no ar e usado por cima, com o bêbado de fundo; pressionar no meio do uso; a água, que alivia o tonto (`; alivia Tonto/Queda/1 -> fim; a de fundo volta: Bebado/Pico/2`); a banana solta longe; e o bêbado até a queda; o retrato tem `carga=` durante o episódio. A vodka e o lança-perfume são uma mistura com sintética, e o lança-perfume faz o sorteio do episódio, que não sai e não deixa texto: por isso a 07 depende da semente 2028 e da conta semente × 41 + 13.
- **`08-paranoia.txt`** (semente 2048, em que o primeiro sorteio sai): a vodka e o baseado não sorteiam; a bala fecha a mistura e a paranoia começa; no fim do uso da bala, ele olha pro teto; a água acalma um passo, e o pico vira queda; a paranoia acaba, o eufórico da bala volta do fundo e vai até o fim, com a carga de volta a 0.
- **`09-baseado-por-conta-propria.txt`** (semente 2, `# acoes: TrocarExpressao,FumarBaseado`): a primeira decisão é o baseado, sem item no mundo, com 210 passos no chão, o `Chapado/Subida/2` e `carga=1`, sem sorteio; a subida vira pico antes da decisão seguinte, e, chapado, a agenda só troca a cara; a onda acaba, e a carga volta a 0; a decisão seguinte é o baseado de novo, e o clique o interrompe aos 60 passos, com a onda continuando. A ordem segue a do tempo real onde ele obriga, na energia Média (a subida, de 10 s, acaba antes da decisão seguinte, que vence 12,8 s ou mais depois do uso ou da reação); no resto, a história é roteirizada, com uma decisão por fase do chapado.
- As referências 01 a 08 não têm a ação nova (a 01, a 02 e a 03 usam todas as ações de sempre; a 04, a 05, a 07 e a 08 listam ações sem ela) e continuam idênticas byte a byte.
- Os eventos se escrevem `CmdSetDominantEmotion emocao=Nome` (ou `Automatica`), `CmdSetAdultContent ligado=sim|nao`, `CmdSummonItem item=Nome`, `CmdClearItems`, `ItemPress id= x= y=`, `ItemDragStart id=`, `ItemDragMove id= x= y=`, `ItemDragEnd id= x= y=`, `ItemRelease id=` e `ItemEffectTimer geracao=`; sem a geração, vale a agendada.

## 3. Decisões ainda pendentes

As escolhas do usuário e as pendências abertas estão numeradas (Q-01 em diante) em [DECISIONS.md](DECISIONS.md).

## 4. Histórico de mudanças arquiteturais

A tabela de mudanças, de 2026-09-25 a 2026-10-02, está em [arquivo/arquitetura-historico.md](arquivo/arquitetura-historico.md), seção 4; daqui em diante, cada mudança de desenho entra na DEC que a decide e no DEVELOPMENT_LOG.md.

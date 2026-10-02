# ARCHITECTURE.md — Arquitetura do Buzzy

> Esta página separa fatos implementados de desenho planejado. Nenhuma proposta aparece como arquitetura existente. Cada seção planejada carrega `STATUS: PLANNED`; escolhas que aguardam decisão do usuário carregam `STATUS: UNCERTAIN` e apontam para [DECISIONS.md](DECISIONS.md).
>
> Última atualização: 2026-10-02

## 1. Arquitetura implementada

Código das Fases 1 a 4, dos blocos A e B da Fase 5 (passos P1–P9) e da emoção dominante e do tamagotchi (núcleo, arte e app) em `src/`, organizado pelas três camadas de DEC-007 (detalhes em DEC-016, DEC-020 a DEC-022 e DEC-027 a DEC-030). O estado de verificação de cada critério está em TODO.md; nada aqui é VERIFIED por estar escrito. O código em `spikes/` continua descartável e não conta como módulo do Buzzy.

| Projeto ou pasta | Camada | O que existe |
|---|---|---|
| `src/Buzzy.Core` | núcleo puro (`net10.0`, sem WPF nem Windows) | Geometria em pixels físicos e DIPs; `Topologia` (monitores, principal, impressão digital, monitor que contém um ponto, monitor mais próximo, prender na área útil); `Posicionador` (âncora no centro da base, posição inicial, reacomodação pela posição relativa quando a topologia muda e, na Fase 5, restauração da posição salva em cascata e as posições que acompanham a topologia com o app aberto: `SoTranslacao`, `MonitorCorrespondente`, `Rebasear` e `AcompanharPonto`, seção 2.8). `Persistencia/` (Fase 5): esquema do `settings.json`, na v3 desde o passo P7, com a postura (DEC-029), e política de gravação (seção 2.12). `Personagem/` (Fase 2): máquina de estados da seção 2.6 como função pura de (estado, evento) para (estado, efeitos), fila com prioridade, agenda autônoma com semente, perfis de energia, retrato e gravação/reprodução de sequências; a emoção dominante (DEC-027) fica em `Maquina.cs`, sem chave. `Personagem/Movimento.cs` (Fase 4): superfícies do monitor da âncora (seção 2.5) e parâmetros da física de passo fixo (seção 2.9), aplicada pela máquina de estados. Tamagotchi (DEC-028): `Personagem/Tamagotchi.cs` (tipos, com a carga da paranoia), `TabelaDoTamagotchi.cs` (tabelas, com a classe de cada item e de cada onda), `Maquina.Onda.cs` (a onda, o alívio e a paranoia) e `Maquina.Itens.cs` (os itens, o estado `USING` e o baseado por conta própria), atrás da chave `ConfiguracaoDoNucleo.Tamagotchi`, ligada no aplicativo desde o passo T9 (seção 2.16). `Entrada/` (Fase 3): árbitro de gestos da seção 2.7. |
| `src/Buzzy.App/Plataforma` | adaptador de plataforma | Único arquivo com importações do Windows (`Win32.cs`); leitura da topologia, com a chave estável de cada monitor tirada da configuração de vídeo, só lida (`LeitorDeTopologia`, `ConfiguracaoDeVideo` e `ChavesDeMonitor`, passo P6; seção 2.4); janela de serviço oculta que recebe as mensagens de topologia, bandeja e `TaskbarCreated`; bandeja v4; menu nativo, com a lista de entradas separada da montagem e os submenus da emoção dominante e dos itens, com ícones em bitmaps criados e apagados a cada abertura (`MenuNativo`, `BitmapsDoMenu`; seção 2.16); instância única; log de diagnóstico opcional. Na Fase 5: pasta de dados, com os perfis de teste, e arquivo de configurações com gravação atômica (seção 2.12), usados pela raiz desde o passo P7. |
| `src/Buzzy.App/Apresentacao` | apresentação e adaptador do ponteiro | Janela WPF do personagem (sem borda, `AllowsTransparency`, não ativa, janela de ferramenta, sempre no topo, do tamanho do sprite; responde `MA_NOACTIVATE` e `WM_GETDPISCALEDSIZE`) e o sprite provisório gerado em código. Converte as mensagens de mouse entregues a ela em eventos de ponteiro em pixels físicos e segura a captura do mouse só durante um gesto começado no personagem (Fase 3). Escolhe o quadro da pixel art pelo retrato (`PoseDoPersonagem`, Fase 4; no tamagotchi, também o quadro de uso, os gestos da onda e a sobreposição, seção 2.10) e o renderiza uma vez por quadro e DPI, num cache limitado a 16 MiB (`CacheDeQuadros`). Tamagotchi (DEC-028): uma janela por item (`JanelaDoItem`), com a mesma receita da janela do personagem, e o sprite do item (`SpriteDoItem`). |
| `src/Buzzy.App/Composicao` | raiz de composição | `Aplicacao` liga janela, serviço, bandeja, menu, topologia, arbitragem e núcleo, e executa os efeitos do núcleo. A parte do tamagotchi fica em `Aplicacao.Itens.cs` e `GerenteDosItens`: as janelas dos itens, um árbitro de gestos só delas e o temporizador da onda (seção 2.3). A gravação do `settings.json` fica em `AgendaDeGravacao`, e a releitura da topologia, em `AgendaDaReleitura` (passos P7 e P9). Em repouso, só há timers de disparo único: a releitura da topologia, com as novas tentativas e a conferência tardia do lugar das janelas, as novas tentativas da bandeja, a próxima decisão da agenda autônoma, a gravação com atraso e as novas tentativas dela e, com uma onda de item em curso, o próximo disparo dela. O relógio de passo fixo só corre quando o núcleo pede (reação, pouso, gesto curto, movimento, uso de um item e item visível caindo). Enquanto corre, segue os quadros do compositor do WPF (`CompositionTarget.Rendering`): aplica num lote os passos acumulados e move a janela uma vez por quadro. O arraste não usa relógio. |
| `src/Buzzy.Visual` | apresentação | Gerador da identidade em pixel art (`Pixel/`, DEC-018), que o app usa para as poses provisórias e o ícone. Inclui a arte da emoção dominante e do tamagotchi (DEC-027 e DEC-028): itens, caras novas, poses de uso, sobreposições de efeito e ícones do menu (seção 2.10), que o app usa desde os passos T2, T7 e T8. O renderizador vetorial da direção anterior (DEC-017) está arquivado e sem uso. |

Fluxo implementado:

- **Configurações:** na partida, antes de tudo, a raiz lê o `settings.json` uma vez e entrega ao núcleo, na carga, a posição salva, a postura e as preferências; os efeitos de gravação do núcleo viram pedidos à agenda de gravação (seção 2.12).
- **Topologia:** o Windows avisa a janela de serviço, ou a do personagem no `WM_DPICHANGED`; a raiz agrupa as mensagens, com teto, e lê a topologia com as chaves estáveis; o núcleo acompanha ou revalida a posição (seção 2.8). Se a janela do personagem ou a de um item tiver saído do lugar do núcleo, a raiz reaplica esse lugar, na releitura e na conferência tardia dela, sem mexer na ordem Z.
- **Mouse:** a janela do personagem converte o mouse em eventos de ponteiro; a arbitragem produz gestos e diz se a captura continua; o núcleo aplica os gestos e devolve os efeitos.
- **Efeitos:** a raiz executa os efeitos do núcleo (mover, mostrar, esconder, relógio, agenda, menu adiado para fora do processamento, soltar a captura), no mesmo tratamento da mensagem.
- **Itens do tamagotchi:** cada janela de item converte o mouse entregue a ela em eventos de ponteiro; o árbitro dos itens produz os gestos, que viram eventos do item; o núcleo decide e devolve os efeitos das janelas dos itens e da onda, que a raiz executa (seções 2.3 e 2.7).

Da Fase 5, o app já tem a persistência ligada (passo P7), a chave estável do monitor (passo P6), a topologia em execução (passo P8) e a releitura robusta (passo P9). Ainda não existem a sessão, a energia e a minimização pelo Windows (passos P10–P12: o app ainda não recebe o bloqueio da sessão nem a suspensão), a travessia entre monitores e a escala mista (passos P13 e P14), a animação (Fase 6), o painel de energia e a tela cheia (Fase 8). O núcleo já tem as regras da tabela para essas capacidades; o app só liga cada uma quando a fase dela chega. Na Fase 4, o app liga todas as ações autônomas, a queda física e o movimento; desde 2026-10-02, também a do baseado por conta própria, que fica fora de `AcoesAutonomas.Todas` e só vale com a chave do tamagotchi (seção 2.16).

A emoção dominante (DEC-027) vale no núcleo e no esquema e é escolhida pelo menu (passo T2 da seção "Interação" de TODO.md); desde o passo P7 da Fase 5, a escolha é gravada e volta ao reabrir o app. O tamagotchi (DEC-028) está ligado no aplicativo desde o passo T9: o menu invoca os itens, cada um numa janela própria, e a apresentação mostra o uso, as caras de efeito, os gestos e as sobreposições da onda (passos T7 e T8). A regra da calma (seção 2.6; DEC-022, item 4) já valia no app antes da chave e não depende dela. A verificação de tela, com input SINTÉTICO, e o repouso de 10 minutos com uma onda ativa rodaram em 2026-10-01, sem falhas, antes do bloco B da Fase 5; depois dele, ainda não rodaram (TODO.md). Desde 2026-10-01, o alívio, a bala como droga sintética e a paranoia (adicional da DEC-028; seção 2.16) estão no núcleo e no app, verificados por testes automatizados e de integração; os casos de tela deles (V17, V17b e V18) ainda não rodaram. Desde 2026-10-02, o baseado que ele fuma por conta própria (adendo da DEC-028; seção 2.16) está no núcleo, sem código novo no app, verificado por testes automatizados e de integração; o caso de tela dele (V19) ainda não rodou.

## 2. Arquitetura planejada

STATUS: PLANNED. P1 e P2 foram aceitos nos limites documentados; P3 aguarda validação controlada. WPF/C#/.NET 10 está em DEC-006. As seções 2.1 a 2.12 e 2.16 descrevem o desenho do produto e a seção 2.13 seu encaixe em WPF. DEC-007 a DEC-014 definem o desenho planejado; DEC-015 registra a autorização de execução. P3 é gate antes da Fase 1; P7 é gate antes da Fase 8. P4 foi aposentado porque o produto não terá chat nem campo de texto.

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

**Efeitos do tamagotchi (DEC-028), que só saem com a chave ligada.** O temporizador da onda tem dois: `AgendarOnda`, um disparo único de 1 s ou mais, que volta como `ITEM_EFFECT_TIMER` com a mesma geração, e `CancelarOnda`. As janelas dos itens têm cinco: `MostrarItem`, `MoverItem`, `EsconderItem`, `RemoverItem`, com o motivo (usado, recolhido ou substituído), e `LiberarCapturaDoItem`. Num evento, os efeitos saem nesta ordem:

1. os de antes: soltar capturas e fechar o painel;
2. a janela do personagem;
3. as janelas dos itens: primeiro os removidos e depois, item a item, na ordem do Id, esconder o que deixou de aparecer, mostrar o que passou a aparecer ou mover o que mudou de lugar;
4. o relógio;
5. a agenda;
6. a onda;
7. os de depois: gravações, menu, painel, configurações e encerrar.

**Na raiz (passo T8; `GerenteDosItens` e `LigacaoDosItens`, em `Aplicacao.Itens.cs`):**

- `MostrarItem` cria a janela do item, já com o sprite no DPI do monitor, ou mostra de novo a escondida; `MoverItem` a leva ao lugar e só redesenha o sprite se o DPI mudou; `EsconderItem` esconde; `RemoverItem` fecha;
- `LiberarCapturaDoItem`: o árbitro dos itens esquece o gesto sem gerar `ITEM_RELEASE`, e a janela solta o mouse e volta para baixo do personagem; um soltar que chegue depois não vira nada;
- um `MoverItem` só é pulado quando há outro do mesmo Id adiante no lote, antes de um mostrar, esconder ou remover desse Id; nenhum outro efeito é pulado;
- mover, esconder, remover ou soltar a captura de um Id que a raiz não conhece é ignorado; os três primeiros vão para o log com `desconhecido=sim`;
- `AgendarOnda` arma o temporizador da onda, um `DispatcherTimer` de disparo único que se desliga antes de avisar e entrega `ITEM_EFFECT_TIMER` com a geração agendada; `CancelarOnda` o desarma;
- no fim de cada processamento, só com `--diagnostico`, cada item que acabou de parar no chão ganha uma linha de pouso no log (seção 2.13.4);
- saindo (`EXITING`), o núcleo não emite efeito de janela de item: ao encerrar, a raiz para o temporizador da onda, esquece o gesto sobre um item e fecha todas as janelas dos itens.

Um efeito que a raiz não conhece continua lançando exceção.

### 2.4 Coordenadas e desktop virtual

STATUS: PLANNED. Proposta registrada em DEC-008.

- O processo declara consciência de DPI **Per-Monitor V2** desde a Fase 1. Assim o Windows não virtualiza coordenadas nem estica a janela.
- **Sistema canônico:** pixels físicos do desktop virtual. A origem (0,0) é o canto superior esquerdo do monitor principal. Monitores à esquerda ou acima do principal têm coordenadas negativas.
- **Monitor:** chave estável, retângulo do monitor, retângulo da área útil (sem a barra de tarefas), escala (DPI dividido por 96), orientação e se é o principal.
- **Chave estável do monitor:** caminho do dispositivo obtido de `QueryDisplayConfig` e `DisplayConfigGetDeviceInfo`. O nome GDI (`\\.\DISPLAYn`) pode mudar entre sessões e serve apenas como alternativa. STATUS: UNCERTAIN até o protótipo P5 confirmar a estabilidade da chave. *Implementado no passo P6 da Fase 5 (DEC-030; `ChavesDeMonitor` e `ConfiguracaoDeVideo`):*
  - a chave é `mon:` seguido de 16 dígitos hexadecimais, os 8 primeiros bytes do SHA-256 do caminho em maiúsculas: tamanho fixo, só ASCII, sem espaço, `;`, `,`, `|` nem `=`. O caminho só serve para calcular o resumo e nunca sai do adaptador;
  - a consulta lê só os caminhos ativos, com o nome GDI da fonte e o caminho do dispositivo do alvo; um alvo marcado como indisponível, de um monitor que acabou de sair, fica de fora. Num clone, uma fonte com vários alvos, vale o menor caminho em comparação ordinal;
  - quando a consulta falha ou não traz o caminho de um monitor, vale a chave da última consulta boa para o mesmo nome GDI com uma tela do mesmo tamanho, transladada ou não, porque a troca de principal e o rearranjo movem as telas sem trocar os monitores. Sem ela, vale a reserva `gdi:` seguida do nome GDI. O DPI não conta;
  - primeiro são distribuídas as chaves lidas na consulta atual, depois as do cache e as reservas; nenhuma chave se repete. O cache só vive na execução, guarda também as chaves que continuaram por ele e só é trocado por uma leitura coerente, inteira e com a consulta boa;
  - o núcleo trata a chave como opaca e só a compara por igualdade.
- **Topologia:** a lista de monitores tem uma impressão digital, formada pelas chaves, retângulos e escalas. Uma impressão diferente significa mudança de configuração. *Leitura incoerente (passo P6):* uma falha ao ler a informação ou o DPI de um monitor, um DPI zero e um nome GDI ausente ou repetido tornam a leitura inteira incoerente: a anterior continua valendo, e a leitura é tentada de novo. Só como último recurso, depois das 5 leituras da partida e na última tentativa de uma rajada, vale a leitura parcial, em que o monitor que falha fica de fora e o cache não muda.
- **Mudanças:** `WM_DISPLAYCHANGE`, `WM_DPICHANGED` e `WM_SETTINGCHANGE` com `SPI_SETWORKAREA` disparam uma nova leitura. O adaptador agrupa rajadas de mensagens antes de publicar uma única `TOPOLOGY_CHANGED`. O intervalo de agrupamento é um valor a calibrar no protótipo P5. *Implementado no passo P9 (`AgendaDaReleitura`):* a `TaskbarCreated` também pede a releitura; cada mensagem vai ao log só com o tipo; a releitura sai 300 ms depois da última mensagem, com teto de 1 s desde a primeira; uma leitura incoerente é tentada de novo em 500 ms, 1 s e 2 s; e cada releitura publicada arma uma conferência tardia do lugar das janelas, 1,5 s depois. Os três tempos continuam provisórios até o protótipo P5. Regras na seção 2.8.
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
- limites laterais da âncora: o sprite inteiro fica dentro da área útil. Num monitor mais estreito que o sprite, os dois limites ficam no meio, onde a validação o põe (`PrenderNaAreaUtil`); antes, ele começava a escalar fora da lateral (correção de 2026-10-01, achada pelos testes da Fase 5);
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
| `USING` | usuário | Usa o item que o usuário soltou sobre ele: come, bebe, fuma, cheira, engole ou inala, de desenho animado, por um número fixo de passos, no apoio em que estava (chão, parede, cipó ou esconderijo). Também é o estado do baseado que ele fuma por conta própria, que entra pela agenda, só no chão e sem item no mundo (adendo de 2026-10-02 da DEC-028). O relógio corre, nada autônomo chega e um `PRESS` o segura na hora. Só existe com a chave do tamagotchi ligada (DEC-028). |

**Dimensões da Fase 4 ligadas às escolhas do usuário:**

| Dimensão | Valores | Efeito |
|---|---|---|
| Preso pelo usuário | sim ou não | O usuário o soltou na lateral ou no cipó: lá fica até o usuário tirá-lo. A agenda só o faz passear pela mesma superfície (DEC-024). Desde o passo P7 da Fase 5, vai para o `settings.json` com a posição (DEC-029). |
| Esconderijo | nenhum, baixo, esquerda ou direita | A borda atrás da qual ele está escondido. Sobrevive ao primeiro clique do clique duplo, a `HIDDEN` e às revalidações (DEC-025). Desde o passo P7 da Fase 5, vai para o `settings.json` com a posição (DEC-029). |

**Dimensões da emoção dominante e do tamagotchi (DEC-027, DEC-028):**

| Dimensão | Valores | Efeito |
|---|---|---|
| Emoção dominante | automática ou uma das 14 caras de humor | Cara de base e a mais sorteada nas trocas de cara. Nenhum efeito sobre estado, posição, ações ou física. É uma preferência, gravada no `settings.json` (seção 2.12). |
| Onda do item | nenhuma; ou a da frente (tipo, fase e nível de 1 a 3) e até uma de fundo, congelada | Pesos, intervalos, gestos e caras; por exceção ao invariante 12, as velocidades de andar, escalar e pendurar e o cambaleio. Avança só pelos disparos únicos do próprio temporizador. A onda `Paranoico` não vem de item: vem do sorteio da paranoia e nunca fica no fundo. Só em memória (seção 2.16). |
| Carga da paranoia (adicional de 2026-10-01) | nenhuma; ou o episódio: quantas substâncias, se alguma era droga sintética, os itens de substância distintos e se o episódio já sorteou (`CargaDaParanoia`) | Decide o sorteio único da paranoia no episódio (seção 2.16). Volta toda a zero no fim de todo evento sem onda de substância na frente nem no fundo. O retrato só mostra o número de substâncias. Só em memória. |
| Gerador da paranoia (adicional de 2026-10-01) | um gerador próprio (`AleatorioDaParanoia`), semeado com semente × 41 + 13 | Só o sorteio da paranoia o usa, um passo por episódio de mistura com droga sintética; ela nunca usa o gerador principal. Não volta ao começo com o episódio. Só em memória. |
| Itens no mundo | até 6, cada um caindo, no chão, segurado ou arrastado | Só em memória. Com um item na mão do usuário, o personagem fica atento: parado onde está, sem decisão autônoma, até o item sair da mão. |

**Dimensões ortogonais.** As informações abaixo acompanham o personagem sem fazer parte do estado de comportamento:

| Dimensão | Valores | Efeito |
|---|---|---|
| Expressão | as 14 caras de humor (feliz, curioso, sonolento e demais) e as 8 caras de efeito, que só a onda mostra (DEC-028; a paranoica, desde 2026-10-01) | Nenhum sobre comportamento ou posição |
| Gesto curto | nenhum, espiar, olhar ao redor, coçar-se, espreguiçar-se, brincar e demais definidos no manifesto; e os oito da onda (soluço, dança, gargalhada, espirro, tosse, tremedeira e, desde 2026-10-01, olhar pro teto e agachar), que só ela sorteia, também só em `IDLE`. O olhar pro teto também vem, sem sorteio, no fim do uso que começou a paranoia (DEC-028) | Ação visual de macaquinho com duração limitada, executada na superfície atual (`IDLE`, `CLIMBING` parado ou `HANGING`); não muda estado de comportamento, posição nem superfície; qualquer `PRESS`, `CMD_*` ou evento do sistema a encerra na hora |
| Autonomia pausada | sim ou não | Definida por `CMD_PAUSE_AUTONOMY`/`CMD_RESUME_AUTONOMY`. Enquanto sim, nenhum `AUTONOMY_TIMER` é agendado; queda ou pouso em curso terminam; arraste, clique, painel, ocultação e modo de tela cheia continuam funcionando. Com ela, ou com o painel aberto, quem está agarrado sem estar preso desce ou se solta (regra da calma, DEC-022, item 4), e o disparo da onda de um item só troca a cara (DEC-028) |
| Painel de energia | aberto ou fechado | Enquanto aberto, pausa a autonomia; o movimento físico em curso pode terminar. Arrastar o mascote fecha o painel. Ele contém somente o seletor Baixa/Média/Alta, sem conversa ou campo de texto. |
| Motivo do ocultamento | `POR_USUARIO`, `POR_SESSAO`, `POR_SUSPENSAO`, `POR_TELA_CHEIA` | Decide quais eventos podem tirar o personagem de `HIDDEN`; tela cheia não desfaz uma ocultação feita pelo usuário. Precedência (DEC-020): `POR_USUARIO` > `POR_SESSAO` > `POR_SUSPENSAO` > `POR_TELA_CHEIA`; em `HIDDEN`, um motivo só substitui outro de precedência menor |
| Nível de energia | `BAIXA`, `MEDIA`, `ALTA` | Altera frequência e duração das ações autônomas e a frequência de expressões; padrão `MEDIA` |
| Retorno temporário do modo de tela cheia | posição e chave do monitor apenas em memória, ou vazio | Restaura a posição prévia sem substituir a posição persistida escolhida pelo usuário. É descartado quando o usuário arrasta o personagem ou o mostra manualmente durante o modo: a escolha manual passa a valer. A posição gravada ao esconder ou sair é sempre o retorno, se houver, e nunca a posição temporária (DEC-020). Numa mudança de topologia, acompanha os monitores como a posição (seção 2.8) |

**Eventos**

| Origem | Eventos |
|---|---|
| Ponteiro, somente sobre as janelas do Buzzy ou com captura ativa | `POINTER_DOWN(p, botão)`, `POINTER_MOVE(p)`, `POINTER_UP(p, botão)`, `CAPTURE_LOST` |
| Gestos derivados pela arbitragem | `PRESS`, `CLICK`, `DOUBLE_CLICK`, `DRAG_START`, `DRAG_MOVE(p)`, `DRAG_END(p)`, `DRAG_CANCEL`, `CONTEXT_MENU` |
| Painel de energia | `ENERGY_PANEL_OPEN`, `ENERGY_SELECTED(nivel)`, `ENERGY_PANEL_CLOSE`; `ENERGY_SELECTED` aceita somente `BAIXA`, `MEDIA` ou `ALTA` |
| Sistema | `TOPOLOGY_CHANGED(topologia)`, `SESSION_LOCKED`, `SESSION_UNLOCKED`, `SUSPENDING`, `RESUMED`, `SESSION_ENDING` |
| Adaptador de janela ativa | `FULLSCREEN_TARGETS_CHANGED(monitoresOcupados)`; payload contém somente chaves de monitores, sem HWND, processo, título ou texto |
| Bandeja e menu | `CMD_HIDE`, `CMD_SHOW`, `CMD_PAUSE_AUTONOMY`, `CMD_RESUME_AUTONOMY`, `CMD_OPEN_SETTINGS`, `CMD_RESET_POSITION`, `CMD_EXIT`; `CMD_SET_DOMINANT_EMOTION(emoção ou automática)` (DEC-027); `CMD_SUMMON_ITEM(item)` e `CMD_CLEAR_ITEMS` (DEC-028) |
| Ponteiro sobre a janela de um item, por um árbitro de gestos próprio dela (DEC-028) | `ITEM_PRESS(id, p)`, `ITEM_DRAG_START(id)`, `ITEM_DRAG_MOVE(id, p)`, `ITEM_DRAG_END(id, p)` e `ITEM_RELEASE(id)`, este para clique, clique duplo ou gesto cancelado (captura perdida, movimento sem o botão ou outro botão pressionado sem soltar o anterior); o botão direito no item é o `CONTEXT_MENU` de sempre (seção 2.7) |
| Relógio | `TICK(dt)` com passo fixo, `AUTONOMY_TIMER`; `ITEM_EFFECT_TIMER(geração)`, o disparo da onda de um item, com a prioridade do relógio: não é descartado com o usuário no controle e não encerra um gesto (DEC-028) |
| Configurações | `SETTINGS_CHANGED(config)` |

**Transições principais**

| De | Evento | Para | Regra |
|---|---|---|---|
| `BOOTING` | configurações e topologia carregadas | `SETTLING` | Posição restaurada pela seção 2.8: com posição salva, `Posicionador.Restaurar` escolhe o monitor pela chave, pela tela do monitor da época ou, sem as duas, o principal, e a regra registrada termina em "posição salva restaurada pela chave", "pelo retângulo do monitor" ou "no monitor principal"; sem posição salva, vale a posição inicial e o texto não muda. O mesmo sufixo vale para "HIDDEN: pedido de mostrar anterior à carga" (DEC-030). Com posição salva, a carga também traz a postura gravada com ela (DEC-029): a acomodação o devolve escondido na mesma borda, se o esconderijo pelo clique duplo está ligado e a borda está na lista, ou agarrado e ainda preso; longe da parede e do cipó, a marca de preso se apaga. Sem posição salva, a postura não vale. Só a primeira carga vale; pedidos anteriores a ela (mostrar, esconder, sessão, topologia) ficam guardados e o personagem só aparece com a carga. Se o monitor restaurado estiver ocupado pela tela cheia (monitores em cache), aplica-se a linha de `FULLSCREEN_TARGETS_CHANGED` (DEC-020). |
| qualquer autônomo ou físico | `PRESS` sobre pixel opaco | `PRESSED` | Movimento autônomo congela no quadro atual. Vale também no meio de um pulo ou queda. |
| `PRESSED` | `DRAG_START` | `DRAGGING` | Plano autônomo descartado. |
| `PRESSED` | `CLICK` | `REACTING` | Reação curta. Depois, `SETTLING` decide o próximo estado. Se o monitor do personagem mudou ou sumiu enquanto o botão estava pressionado, a posição é validada já no `CLICK`, a partir da posição que acompanhou a topologia (DEC-020, DEC-030). A mesma regra vale para toda saída de `PRESSED` (linha `PRESSED`, `DRAGGING` / `TOPOLOGY_CHANGED`). |
| `DRAGGING` | `DRAG_MOVE(p)` | `DRAGGING` | Posição = cursor menos o deslocamento da pegada. |
| `DRAGGING` | `DRAG_END` ou `DRAG_CANCEL` | `SETTLING` | Validação da seção 2.7. |
| `SETTLING` | com apoio | `IDLE` | Autonomia retomada depois de um intervalo de acomodação, exceto se o painel de energia estiver aberto. |
| `SETTLING` | com esconderijo marcado | `PEEKING` | Volta ao esconderijo na mesma borda, perto do lugar validado (DEC-025). |
| `SETTLING` | sem apoio, a mais de 32 DIP do chão, com o topo do sprite a até 96 DIP da borda de cima | `HANGING` agarrado ao cipó | Parado e sem relógio. Se foi o usuário que o soltou ali (`DRAG_END`, `DRAG_CANCEL`), ou se ele já estava preso, fica preso pelo usuário (DEC-024). |
| `SETTLING` | sem apoio, a mais de 32 DIP do chão, com a âncora a até 64 DIP de uma lateral | `CLIMBING` agarrado à parede | Idem, olhando para a parede. Com as duas bordas perto, vale a mais próxima em proporção ao alcance. No fim do uso de um item na parede ou no cipó, vale o apoio do uso, mesmo a menos de 32 DIP do chão (DEC-028). |
| `SETTLING` | sem apoio | `FALLING` | Cai até o chão do monitor. |
| `PRESSED` (segundo clique), `IDLE`, `REACTING` | `DOUBLE_CLICK` ou menu "Energia" | `SETTLING` se vier de `PRESSED`; senão permanece | A partir da Fase 8, abre o painel compacto somente com o seletor de energia; pausa a autonomia enquanto estiver aberto. Antes da Fase 8, o segundo clique só produz reação não verbal. |
| qualquer estado visível com painel aberto | `ENERGY_SELECTED(nivel)` | permanece | Atualiza a mesma preferência persistida; o novo nível afeta as próximas decisões autônomas. |
| qualquer estado visível com painel aberto | `ENERGY_PANEL_CLOSE` | permanece | Fecha o painel e retoma a agenda após intervalo de acomodação. |
| `FALLING`, `LANDING` | contato com o chão | `LANDING`, depois `IDLE` | O painel não altera a física; se estiver aberto, a autonomia continua pausada. |
| `FALLING`, `JUMPING` | contato com o chão a 600 DIP/s ou mais, com a autonomia livre e menos de dois quiques seguidos | `JUMPING` (quique de borracha) | Toon force (DEC-023): volta a subir com metade da velocidade, rindo. Depois do segundo quique, ou com a autonomia pausada ou o painel aberto, vale a linha de contato com o chão. |
| `IDLE` | `AUTONOMY_TIMER` | `WALKING`, `CLIMBING`, `JUMPING`, `RESTING`, `USING` (o baseado por conta própria) ou permanece com um gesto curto | Escolha ponderada pela personalidade e pelo nível de energia, com semente. Só acontece com o painel de energia fechado e a autonomia não pausada. Com a chave do tamagotchi ligada e a ação `FumarBaseado` na configuração, como no aplicativo, a última opção é fumar um baseado por conta própria, com o peso `PesoFumarBaseado` do perfil, só no chão, fora do esconderijo, sem item na mão do usuário e sem `Chapado` nem `Paranoico` na frente; senão, o peso dela é zero. O uso começa como o do baseado solto pelo usuário (linha `ITEM_DRAG_END`), no chão e sem item no mundo, com a regra "IDLE + AUTONOMY_TIMER: Fumar Baseado por conta própria" (seção 2.16; adendo de 2026-10-02 da DEC-028). |
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
| estados autônomos, físicos, `REACTING` e `USING` | `TOPOLOGY_CHANGED` sem mudar a geometria do monitor do personagem: só outros monitores mudaram, ou o dele foi só transladado no desktop virtual (tela e área útil deslocadas pelo mesmo (dx, dy), com o mesmo DPI) ou só trocou de chave | permanece, com uma transição para o mesmo estado | O que está em curso continua: a caminhada, a escalada, o pulo, a reação, o uso, o esconderijo e o preso. A âncora, a janela e, nos estados de movimento, a posição fina andam pela translação; a posição passa a descrever o lugar novo; o relógio e a agenda não mudam. A regra registrada diz que o monitor do personagem "não mudou", "mudou de chave" ou "foi transladado (dx,dy)", sem a chave (DEC-030; invariante 19). |
| estados autônomos, físicos, `REACTING` e `USING` | `TOPOLOGY_CHANGED` com a geometria do monitor do personagem mudada (resolução, escala, orientação ou área útil) | `SETTLING` | Revalida na mesma posição relativa da área útil atual dele, com a regra "TOPOLOGY_CHANGED". Em `USING`, o uso acaba, e a onda do item continua (DEC-028). |
| estados autônomos, físicos, `REACTING` e `USING` | `TOPOLOGY_CHANGED` sem o monitor do personagem | `SETTLING` | Vai ao sobrevivente mais próximo do pixel dos pés medido nas coordenadas antigas, na mesma posição relativa (seção 2.8), com a regra "TOPOLOGY_CHANGED: o monitor do personagem foi desconectado". Em `USING`, o uso acaba, e a onda continua. |
| `PRESSED`, `DRAGGING` | `TOPOLOGY_CHANGED` | sem troca de estado | Nada é validado no gesto; a posição guardada e o retorno da tela cheia acompanham a topologia (seção 2.8). Em `PRESSED`, toda saída (`CLICK`, `DOUBLE_CLICK`, `DRAG_CANCEL` e `DRAG_START`) parte de onde ele estaria parado: se o monitor dele mudou ou sumiu, do lugar que a posição acompanhada descreve, já validado. Em `DRAGGING`, o lugar do arraste anda com o monitor em que está, e a janela vai junto, como o cursor; a validação acontece ao soltar (DEC-030). |
| `BOOTING`, `HIDDEN`, `EXITING` | `TOPOLOGY_CHANGED` | sem troca de estado | Em `BOOTING`, só atualiza a topologia em cache, e a validação acontece ao terminar de carregar. Em `HIDDEN`, a posição guardada e o retorno da tela cheia também acompanham a topologia, sem mover a janela (seção 2.8), e a validação acontece ao reaparecer. Em `EXITING`, nada acontece. |
| qualquer, exceto `EXITING` | `CMD_HIDE` | `HIDDEN(POR_USUARIO)` | Fecha o painel, encerra captura e arraste, grava a posição escolhida pelo usuário (o retorno temporário, se houver; DEC-020). |
| qualquer, exceto `EXITING` | `SESSION_LOCKED` | `HIDDEN(POR_SESSAO)` | Idem. Em `HIDDEN`, segue a precedência dos motivos: o motivo do usuário é preservado, e o da sessão substitui a suspensão e a tela cheia. |
| qualquer, exceto `EXITING` | `SUSPENDING` | `HIDDEN(POR_SUSPENSAO)` | Idem, com a mesma precedência: preserva o usuário e a sessão bloqueada (com a sessão bloqueada, `RESUMED` não mostra o personagem) e substitui a tela cheia. |
| `HIDDEN(POR_USUARIO)` | `CMD_SHOW` | `SETTLING` | Só o usuário desfaz o que o usuário pediu. |
| `HIDDEN(POR_SESSAO)` | `SESSION_UNLOCKED` ou `CMD_SHOW` | `SETTLING` | Revalida a posição contra a topologia atual. No `SESSION_UNLOCKED`, se o monitor em que ele reaparece estiver ocupado pela tela cheia (monitores em cache), aplica-se a linha de `FULLSCREEN_TARGETS_CHANGED`; o `CMD_SHOW` é escolha do usuário e não a reaplica (DEC-020). |
| `HIDDEN(POR_SUSPENSAO)` | `RESUMED` ou `CMD_SHOW` | `SETTLING` | Idem, com `RESUMED` no lugar de `SESSION_UNLOCKED`. |
| `HIDDEN(POR_USUARIO)` | `SESSION_UNLOCKED`, `RESUMED` | `HIDDEN(POR_USUARIO)` | O personagem **não** reaparece: um evento do sistema não desfaz uma ação direta do usuário. |
| qualquer | `CMD_EXIT`, `SESSION_ENDING` | `EXITING` | Grava configurações e a posição escolhida pelo usuário (o retorno temporário, se houver) e encerra. |
| `CLIMBING`, `HANGING` agarrados sem estarem presos, com a autonomia pausada ou o painel aberto | fim de qualquer evento | permanece; os passos seguintes o fazem descer ou se soltar | Regra da calma (DEC-022, item 4): deixa de estar agarrado. O preso fica; com um item na mão do usuário, o atento o segura até o item sair da mão. |
| qualquer carregado, exceto `EXITING` | `CMD_SET_DOMINANT_EMOTION(e)` | permanece, com uma transição para o mesmo estado que só registra a escolha | Grava a preferência. Sem onda e fora de `RESTING`, `REACTING` e `USING`, a cara muda na hora. Fora das 14 caras de humor ou igual à atual, é ignorado (DEC-027). |
| qualquer visível e carregado | `CMD_SUMMON_ITEM(item)` | permanece | O item nasce ao lado dele, acima do chão, e cai (seção 2.16). Com 6 itens, o mais antigo fora da mão sai. Parado e sem onda, ele fica empolgado. Escondido, antes da carga ou com um item fora do enum, nada acontece. |
| qualquer | `CMD_CLEAR_ITEMS` | permanece | Todos os itens saem; o da mão do usuário solta a captura antes. |
| qualquer visível, sobre um item visível | `ITEM_PRESS(id, p)` | `WALKING` e `RESTING` vão a `IDLE`; os outros permanecem | Atento: andando, para; descansando, acorda; na parede e no cipó, fica agarrado, e o foguete apaga; pulo e queda seguem até o chão. Nenhuma decisão autônoma até o item sair da mão. Outro item que estivesse na mão é largado antes, com a captura solta. |
| qualquer | `ITEM_DRAG_MOVE(id, p)` do item arrastado | permanece | A âncora do item é o cursor menos a pegada, sem prender, como no invariante 2. |
| `IDLE`, `WALKING`, `CLIMBING`, `HANGING`, `RESTING`, `REACTING`, `LANDING`, `PEEKING` | `ITEM_DRAG_END(id, p)` com o item sobre o personagem | `USING` | O item sai (`RemoverItem`, usado), o uso começa no apoio em que ele está e as ondas mudam na hora (seção 2.16): primeiro a combinação, em que um item de alívio acalma a onda da frente um passo, sem começar a dele; depois a paranoia, em que um item de substância entra na carga do episódio e o uso que fecha a mistura com droga sintética faz o sorteio único do episódio. A regra registrada termina com o que mudou: `; alivia X -> Y`, `; a paranoia começa: Paranoico/Subida/1` ou `; a paranoia sobe: X -> Y`; o sorteio que não sai não deixa texto. Descansando, acorda antes; o plano, a reação ou o pouso são cortados. |
| `JUMPING`, `FALLING`, `USING`, `PRESSED`, `DRAGGING` e os de transição ou sistema (`SETTLING`, `HIDDEN`, `BOOTING`, `EXITING`) | `ITEM_DRAG_END(id, p)` com o item sobre o personagem | permanece | Recusado: o item cai de onde foi solto. `PRESSED` e `DRAGGING` são inalcançáveis com um ponteiro só. |
| qualquer | `ITEM_DRAG_END` fora do personagem, `ITEM_RELEASE`, ou `ITEM_DRAG_END` de um item só segurado | permanece | O item cai de onde foi solto, ou fica, se já está no chão; nunca é usado. O atento acaba, e a agenda volta depois do intervalo de acomodação. |
| `USING` | fim dos passos do uso | `SETTLING`, depois `IDLE`, `CLIMBING` agarrado, `HANGING` agarrado ou `PEEKING` | Volta ao mesmo apoio, com a cara de base: quem estava preso continua preso, e quem estava escondido continua escondido. Se o uso começou a paranoia e ele volta a `IDLE` sem gesto, com ela ainda na frente, olha pro teto na hora (`OlharProTeto`, 90 passos, sem sorteio, também com a autonomia pausada), com a regra "IDLE: a paranoia começou, gesto OlharProTeto". |
| `USING` | `PRESS` | `PRESSED` | No mesmo evento: o uso acaba, e a onda continua. |
| `USING` | `TOPOLOGY_CHANGED` que revalida, tela cheia, `CMD_HIDE`, sessão, suspensão, `CMD_EXIT` | conforme a linha de cada evento | O uso acaba, e a onda continua; só `EXITING` cancela a onda. Um `TOPOLOGY_CHANGED` que não muda a geometria do monitor dele não interrompe o uso (DEC-030). |
| qualquer | `ITEM_EFFECT_TIMER(g)` da geração agendada | permanece | A onda avança uma fase ou um nível; quando acaba, a de fundo volta. A cara da fase entra na hora, exceto em `RESTING`, `REACTING` e `USING`. A agenda não é reagendada: com a autonomia pausada, o disparo só troca a cara. Um disparo de outra geração é ignorado. |

Toda decisão autônoma agendada respeita o intervalo de acomodação, em qualquer estado que decide (DEC-020).

Com a chave do tamagotchi desligada, os eventos dos itens e da onda são descartados antes de qualquer outra regra, inclusive antes de encerrar um gesto: nenhuma linha de item ou de onda acontece (DEC-028), e o baseado por conta própria tem peso zero. No aplicativo, a chave está ligada desde o passo T9. A linha da emoção dominante não depende da chave.

Com a chave ligada, no fim de todo evento, a carga da paranoia volta toda a zero se nem a onda da frente nem a de fundo é de substância: as substâncias, a sintética, os itens distintos e o sorteio feito, juntos. O episódio acabou, e o seguinte sorteia de novo (seção 2.16).

**Invariantes, verificáveis por teste automático**

1. Em `PRESSED`, `DRAGGING`, `SETTLING` e `USING`, nenhum evento autônomo muda estado ou posição.
2. Em `DRAGGING`, a posição do personagem é sempre o cursor menos o deslocamento da pegada. O personagem não anda, não pula, não foge e não começa escalada.
3. O painel de energia não recebe nem interpreta texto; teclas locais só alteram seu controle quando ele está focado.
4. Nenhum comportamento autônomo começa enquanto o painel de energia está aberto. Começar é entrar no estado: a transição para o mesmo estado, como a que registra a escolha da emoção dominante, não conta (DEC-027).
5. Depois de `SETTLING`, a âncora está dentro da área útil de algum monitor presente.
6. A expressão pode mudar em qualquer estado sem alterar estado de comportamento ou posição.
7. Com a mesma semente e a mesma sequência de eventos, a sequência de retratos é idêntica.
8. Abrir ou fechar o painel de energia nunca muda a posição do personagem.
9. Iniciar um arraste fecha o painel de energia; ao soltar, nenhuma preferência de energia é alterada implicitamente.
10. `SESSION_UNLOCKED` e `RESUMED` nunca fazem o personagem reaparecer quando ele foi escondido pelo usuário.
11. Todo estado tem pelo menos uma transição de entrada e uma de saída, com duas exceções por construção: `BOOTING`, que só tem saída, e `EXITING`, que só tem entrada. `USING` entra pelo soltar de um item ou pela agenda, no baseado por conta própria, e sai pelo fim do uso ou por uma interrupção; só aparece com a chave do tamagotchi ligada (DEC-028).
12. Um nível de energia mais alto pode aumentar frequência/duração de ações, mas nunca muda colisões, limites de superfície, segurança ou prioridade da ação direta. A onda de um item é a exceção documentada para as velocidades (invariante 26, DEC-028); o nível de energia continua sem mudar a física.
13. O adaptador nunca envia identidade ou conteúdo de outra janela ao núcleo; o modo de tela cheia recebe apenas os monitores cobertos pela janela ativa.
14. Em `PRESSED` e `DRAGGING`, `FULLSCREEN_TARGETS_CHANGED` não muda estado nem posição. Depois de um arraste ou de `CMD_SHOW` manual durante o modo de tela cheia, o fim da tela cheia não move o personagem.
15. Um gesto curto nunca muda estado de comportamento, posição ou superfície, e termina ao chegar qualquer evento de prioridade maior. Vale também para os oito gestos da onda (DEC-028), inclusive o olhar pro teto do começo da paranoia.
16. A posição gravada (`GravarPosicao`) nunca é a posição temporária do modo de tela cheia (DEC-020).
17. Todo `AgendarDecisao` tem atraso maior ou igual ao intervalo de acomodação (DEC-020).
18. Todo `GravarPosicao` (DEC-029):
    - sai só de `DRAG_END`, `DRAG_CANCEL`, `CMD_RESET_POSITION`, `CMD_HIDE`, `SESSION_LOCKED`, `SUSPENDING`, `CMD_EXIT` ou `SESSION_ENDING`; nunca do relógio, do movimento, da agenda autônoma, da troca de expressão, da carga, da topologia, da tela cheia nem das preferências, e por isso não há gravação periódica (DEC-011);
    - traz uma posição gravável: chave não vazia, frações em [0, 1] e a tela do monitor da época conhecida e não vazia, com a postura do estado depois do evento (a borda do esconderijo e a marca de preso, esquema v3);
    - volta igual do `settings.json`, com a postura: escrita e lida pelo esquema (seção 2.12), é válida, sem aviso e sem precisar de normalização.

    É conferido nas sequências aleatórias sem a física e também com a configuração do aplicativo (física, agarrar e esconderijo), e, nesta, também na partida seguinte.

Os invariantes 19 e 20 são da topologia em execução (passo P8 da Fase 5; DEC-030):

19. Nos estados que revalidam (autônomos, físicos, `REACTING` e `USING`), um `TOPOLOGY_CHANGED` que não muda a geometria do monitor do personagem (só outros monitores mudaram, ou o dele só foi transladado ou só trocou de chave, com a mesma tela) não muda o estado: há uma única transição, para o mesmo estado; a âncora, o retângulo e, nos estados de movimento, a posição fina andam exatamente pela translação; a posição descreve o lugar novo; o esconderijo, o preso, o uso, a direção e a cara continuam; e a agenda e o relógio não mudam, salvo pelo fim de um gesto curto (invariante 15) e pelos itens.
20. Depois de um `TOPOLOGY_CHANGED`, visível e fora de `PRESSED` e `DRAGGING`, o monitor do personagem é um da topologia nova e, quando o sprite cabe nele, a âncora está na área útil dele.

Os invariantes 19 e 20 são conferidos a cada `TOPOLOGY_CHANGED` nas sequências aleatórias, contra regras escritas no teste, à parte do núcleo, junto com as posições que acompanham a topologia, as saídas de `PRESSED`, o arraste e o item na mão (`InvariantesTestes`); e também com a física do aplicativo (`MudancaDeTopologiaTestes`). O número 21 fica reservado para a travessia da Fase 5. Os invariantes 22 a 29 são da emoção dominante e do tamagotchi (DEC-027, DEC-028; regras exatas na seção 2.16):

22. Sem itens, sem onda, com a emoção automática e sem o baseado por conta própria (a ação `FumarBaseado` fora das ações), retratos, transições e efeitos são idênticos aos de antes; as referências gravadas 01–05 continuam idênticas byte a byte. Com a configuração do aplicativo, que liga a ação, a agenda muda de propósito, porque ele fuma; as equivalências usam as ações de sempre (`Todas`).
23. Itens só nascem por `CMD_SUMMON_ITEM`, com o próximo Id, que nunca se repete; um uso vem do `ITEM_DRAG_END` do usuário sobre o personagem, num estado que aceita, ou da ação autônoma `FumarBaseado` (só o baseado, só em `IDLE` no chão, nunca cria item no mundo). Um item só sai usado, recolhido ou substituído pelo sétimo. Nada autônomo, do relógio ou do sistema invoca ou usa um item do mundo, e só se entra em `USING` por esses dois caminhos (o segundo desde 2026-10-02, adendo da DEC-028).
24. Em `USING`, `PRESS` leva a `PRESSED` no mesmo evento, e nenhum evento autônomo chega à máquina. O uso dura exatamente os passos do verbo e sai por `SETTLING` para o mesmo apoio, mantendo o preso e o esconderijo. Interrompido, só o uso acaba; a onda continua.
25. Com onda, fora de `EXITING`, há exatamente um `ITEM_EFFECT_TIMER` pendente, de 1 s ou mais; sem onda, nenhum. O nível fica entre 1 e 3 (na queda, 1), e há no máximo uma onda de fundo, de outro tipo. Sem item novo, a onda da frente acaba em no máximo 2 + nível disparos.
26. A onda só muda pesos, intervalos, gestos, caras, os tempos na parede e pendurado, as velocidades de andar, escalar e pendurar (de 50% a 200%) e o cambaleio, sempre no chão e entre as laterais. Gravidade, queda máxima, quique, foguete, colisões, limites, apoio e prioridade do usuário ficam intactos.
27. A emoção dominante é nula ou uma das 14 caras de humor. Com a mesma semente e os mesmos eventos, ligá-la muda só as expressões.
28. Há no máximo 6 itens. Fora da mão do usuário, todo item tem o sprite inteiro na área útil do monitor dele e, parado, os pés no chão. A janela de um item aparece se e somente se ele é visível: na mão do usuário, sempre; fora dela, só com o personagem visível e fora de um monitor ocupado pela tela cheia. Nenhum item invisível fica caindo.
29. O relógio corre se e somente se o personagem se move sem estar agarrado, está em `REACTING` ou `USING`, faz um gesto em `IDLE` ou há um item visível caindo.

Os invariantes 22 a 29 são conferidos a cada evento nas sequências aleatórias com a chave ligada, contra regras escritas no teste, à parte do núcleo (`InvariantesTestes`); o 22 e o 27 também com a física e a configuração do aplicativo (`ChaveLigadaTestes`; o 22 com as ações de sempre, sem o baseado por conta própria). O mesmo teste confere, a cada evento:
- que a energia e a emoção dominante só assumem valores das listas fechadas, e que um comando fora delas é ignorado sem gravar (a regra R1 do teste);
- que não há relógio em `IDLE` sem gesto, `RESTING`, `HIDDEN`, `PRESSED`, `DRAGGING`, `BOOTING` e `EXITING`, a não ser por um item visível caindo (a regra R-b, o critério 3 da Fase 2 com o invariante 29);
- no alívio (adicional de 2026-10-01), a cada item de alívio com onda na frente, que ele não sobe o nível nem muda o pior, não alonga a fase em curso (quando só o nível cai, o temporizador não muda; quando começa a queda ou a de fundo volta, há um disparo novo com a duração cheia), não toca a onda de fundo, não começa a onda do item e não sorteia nada. A classificação dos itens e das ondas vem da transcrição das tabelas, escrita à parte do núcleo;
- na paranoia (adicional de 2026-10-01), que a carga do núcleo é a contada pelo próprio teste (substâncias, sintética, itens distintos e sorteio feito, zerados juntos); que o soltar nunca muda o gerador principal; que o gerador da paranoia só anda um passo no uso que fecha a mistura com droga sintética, sem a paranoia na frente e num episódio ainda sem sorteio, e nunca fora de um uso; que a paranoia só começa num sorteio que saiu, com o temporizador recomeçado na duração cheia; que, na frente, o episódio dela é uma mistura com sintética que já sorteou; que ela nunca fica no fundo; e que o uso que a começou, até o fim, de volta a `IDLE` e com ela na frente, termina com o olhar pro teto por 90 passos, sem sorteio, enquanto qualquer outro fim de uso não traz gesto;
- no baseado por conta própria (adendo de 2026-10-02), que ele só fuma pela decisão que vale, com a ação ligada, em `IDLE` no chão e sem `Chapado` nem `Paranoico` na frente, numa transição só, de `IDLE` a `USING`, com a regra e o uso do baseado no chão, sem item, Id nem janela de item, e com o gerador principal andando exatamente um passo; que, em `IDLE` fora do chão, ele não fuma; e que o menu de contexto em `USING` só abre o menu, sem mudar o estado, o uso nem os passos. Metade das sequências do tamagotchi tem a ação, com peso 6, e nelas um gerador de eventos às vezes o leva a `IDLE` fora do chão e abre o menu no meio do uso de um baseado; os nove casos exigidos do baseado aparecem na semente canônica e nas sementes mestras.

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

**Janelas dos itens do tamagotchi (DEC-028; passo T8, em `JanelaDoItem` e `Aplicacao.Itens.cs`)**

- **Quem recebe o input:** cada item tem uma janela própria, com a receita da janela do personagem (seção 2.13.1). Só os pixels opacos do item recebem clique; a janela nunca é ativada e não tira o foco do aplicativo em uso. `WM_MOUSEMOVE` só é tratado durante um gesto começado no item.
- **Árbitro próprio:** uma segunda instância do árbitro de gestos, só para os itens, com as mesmas regras do personagem: limiar de arraste do sistema, clique duplo, `MK_LBUTTON`, ClickLock e captura perdida. Cada gesto vira um evento do item em que o botão esquerdo foi pressionado, mesmo que a mensagem chegue por outra janela:

  | Gesto do árbitro | Evento do item |
  |---|---|
  | `PRESS` | `ITEM_PRESS` |
  | `DRAG_START`, `DRAG_MOVE`, `DRAG_END` | `ITEM_DRAG_START`, `ITEM_DRAG_MOVE`, `ITEM_DRAG_END` |
  | `CLICK`, `DOUBLE_CLICK`, `DRAG_CANCEL` | `ITEM_RELEASE`: o item fica no chão, ou cai de onde está, e nunca é usado |
  | `CONTEXT_MENU` | o mesmo, sem tradução: abre o menu do personagem |

  Um botão pressionado noutro item, sem o soltar do anterior, larga o anterior antes de pegar o novo, nessa ordem.
- **Botão direito:** solto num item, fora de um gesto do botão esquerdo, abre o mesmo menu do personagem e da bandeja (Q-03). Tirar um item da tela é pelo "Recolher itens".
- **Captura:** a janela do item captura o mouse só no gesto e a solta no fim, sem que isso conte como captura perdida. Quando o núcleo encerra o gesto por conta própria (esconder, inclusive pela minimização da janela do personagem, bloquear a sessão, suspender, sair, recolher ou pegar outro item), ele emite `LiberarCapturaDoItem` (seção 2.3).
- **Entrega:** quem decide se o item foi solto sobre o personagem é o núcleo, pelo retângulo do item já preso na área útil contra o retângulo do personagem encolhido (seção 2.16); a raiz não informa a transparência. Com `--diagnostico`, a raiz refaz o mesmo teste só para o log.
- **Ordem Z, sempre por evento:** fora de um gesto, a janela do item fica logo abaixo da do personagem. No gesto sobre o item, ela vai ao topo do grupo "sempre no topo", para o item não sumir atrás do personagem justamente quando vai ser solto sobre ele, e volta para baixo no fim. Quando o personagem reaparece no topo, a ordem é reafirmada. Nunca por timer (SECURITY.md 2). Depois de uma releitura da topologia e na conferência tardia dela (seção 2.8), a raiz só devolve ao lugar do núcleo a janela à vista, fora de um gesto, que saiu dele, sem mexer na ordem Z (`SWP_NOZORDER`; DEC-030); a janela do item na mão fica onde o cursor a pôs.
- **Um ponteiro só:** um gesto num item e outro no personagem não coexistem, porque a captura do mouse é única.

**Foco do painel de energia**

- O painel contém apenas o controle de energia; não há campo de texto nem evento de envio de conteúdo.
- O MVP não tem atalhos de teclado para controlar o personagem (Q-06). Nenhuma tecla move o personagem, com ou sem foco. A navegação por teclado e leitor de tela aplica-se ao controle e às configurações, conforme Q-20.
- Na Fase 8, o painel abre por clique duplo ou pelo menu e recebe foco como consequência dessa ação explícita do usuário.
- O painel fecha com Esc, pelo botão de fechar ou ao perder foco, de acordo com a implementação validada na Fase 8.

### 2.8 Monitores: restauração, conexão e desconexão

STATUS: PLANNED. Proposta registrada em DEC-008. A posição com a tela do monitor, a restauração ao iniciar e a mudança de topologia com o app aberto (passos P1, P2, P8 e P9 da Fase 5) estão implementadas e cobertas por testes automatizados e de integração (DEC-030); desde o passo P7, o app entrega a posição salva ao núcleo (DEC-029). Os cenários reais continuam [MANUAL][HW] (TODO.md, Fase 5).

**Posição gravada:** chave do monitor, retângulo desse monitor na época, posição relativa da âncora dentro da área útil (frações de 0 a 1) e posição absoluta de reserva.

- *Implementação (`PosicaoDoPersonagem`):* o retângulo é `TelaDoMonitor`, a tela do monitor da chave na última vez em que a posição foi descrita nele; nulo quer dizer desconhecido. É preenchido por `Descrever`, pelos dois casos de `Reacomodar`, pela validação da máquina e por `Restaurar`, sempre com a tela real de um monitor presente. Nunca é deslocado por cálculo, nem quando a posição acompanha a translação da topologia em execução (`Rebasear`, abaixo): o arquivo não pode guardar uma tela que nunca existiu.
- A âncora absoluta vale para a execução. Na partida, a cascata abaixo a recalcula, porque noutra sessão, com outro principal, ela fica noutro referencial.
- A igualdade de `PosicaoDoPersonagem` inclui a tela: compare posições campo a campo ou obtenha as duas pelas mesmas funções do `Posicionador`.

**Restauração ao iniciar**

1. Se o monitor da chave gravada existe, a posição relativa é aplicada à área útil atual dele. Isso cobre troca de resolução e de escala.
2. Se não existe, mas há um monitor com o mesmo retângulo, ele é usado.
3. Caso contrário, a posição relativa é aplicada ao monitor principal.
4. Em todos os casos, a âncora é presa à área útil e o personagem passa por `SETTLING`.
5. Se o monitor original voltar depois, o personagem não pula de volta sozinho. A posição só muda por ação do usuário ou por movimento autônomo.

*Implementação (`Posicionador.Restaurar`, DEC-030):*
- No passo 2 vale o primeiro monitor da topologia com a tela igual à gravada; com monitores clonados ou sobrepostos, a escolha é determinística, mas arbitrária.
- Nos três passos, as frações são saneadas (NaN vira 0,5; o resto, inclusive ±∞, é preso em [0, 1]), e a âncora absoluta gravada não é usada.
- A posição nova passa a ter a chave e a tela do monitor escolhido. Restaurar de novo o resultado, na mesma topologia, não move o personagem.
- Em `SETTLING`, uma posição gravada no ar segue a acomodação de sempre: perto do teto agarra o cipó, junto de uma lateral gruda na parede e, senão, cai (DEC-024).
- Desde o passo P7, a carga também traz a postura gravada com a posição (seção 2.12): ele volta escondido na mesma borda ou agarrado e preso onde estava (linha de `BOOTING` da seção 2.6).
- Com a chave sumida, o passo 2 pode achar o sobrevivente que passou a ocupar a tela antiga, enquanto com o app aberto ele iria ao sobrevivente mais próximo (abaixo). É a semântica da cascata, e não um defeito.

**Durante a execução** (passos P8, no núcleo, e P9, na raiz, da Fase 5; DEC-030):

- **Posições guardadas:** em qualquer estado fora de `EXITING`, a posição do personagem e o retorno da tela cheia acompanham a topologia, sem mover a janela (`Posicionador.Rebasear`). Com o monitor correspondente, vale a mesma fração na área útil atual dele, com a chave e a tela dele. Sem ele, a chave, as frações e a tela ficam, e só a âncora anda, junto com o sobrevivente mais próximo do pixel dos pés medido nas coordenadas antigas (no empate, o principal; depois, a ordem da topologia nova). Se o monitor voltar antes de a posição ser usada, ela vale nele.
- **Monitor correspondente** (`MonitorCorrespondente`): o da mesma chave; sem ele, o primeiro com a mesma tela cuja chave não existia antes, como quando a chave passa de `gdi:` para `mon:` ou muda com a porta ou o driver. A condição da chave nova impede que o sobrevivente que o Windows põe na origem, quando o principal é desconectado, conte como o mesmo monitor.
- **O monitor do personagem não muda de geometria** (`SoTranslacao`: tela e área útil deslocadas pelo mesmo (dx, dy), com o mesmo DPI; a marca de principal não conta), como numa troca de principal, num rearranjo ou quando só outros monitores mudam: nos estados que revalidam, o estado continua, e a janela anda junto (seção 2.6, invariante 19).
- **O monitor onde o personagem está é desconectado:** ele vai para o sobrevivente mais próximo do pixel dos pés da última âncora, `(x, y − 1)`, medido nas coordenadas antigas, na mesma posição relativa, e passa por `SETTLING`. Quando o principal é desconectado, o Windows move a origem, e o mais próximo nas coordenadas novas seria outro. O pixel dos pés é a mesma convenção do monitor da âncora: com a barra oculta, a âncora no chão fica na base da tela, que já é o primeiro pixel do monitor de baixo (DEC-030).
- **Troca de resolução, escala ou orientação:** a posição relativa é mantida, o tamanho físico é recalculado e o personagem passa por `SETTLING`.
- **A barra de tarefas muda de lugar, de tamanho ou se esconde sozinha:** a área útil muda, as superfícies são recalculadas e o personagem se acomoda.
- **Mudança durante um gesto:** nada é validado com o botão pressionado. No arraste, o lugar anda com o monitor em que está, como a janela e o cursor, que o Windows leva com o monitor físico quando a origem muda; a validação acontece ao soltar. Pressionado sem arrastar, toda saída parte de onde ele estaria parado: um gesto que atravessa uma mudança em que o monitor dele sumiu termina na mesma posição relativa a que ele iria parado. O próprio Windows reposiciona o cursor se o monitor sumir.
- **Itens do tamagotchi:** seguem a mesma regra, e o item na mão anda com o monitor em que está (seção 2.16).
- **Releitura e conferência tardia** (passo P9, na raiz; `AgendaDaReleitura`): a releitura sai 300 ms depois da última mensagem, com teto de 1 s desde a primeira, e a rajada acaba quando ela sai. Uma leitura incoerente mantém a topologia anterior e é tentada de novo em 500 ms, 1 s e 2 s, a última como leitura parcial (seção 2.4); depois, a agenda desiste até a próxima mensagem. Cada releitura leva no máximo 32 motivos ao log. Depois de cada releitura publicada, a raiz devolve ao lugar do núcleo a janela do personagem e as dos itens à vista que saíram dele, fora de um gesto e sem mexer na ordem Z, e repete essa conferência 1,5 s depois, uma vez por rajada, nunca periódica: com "Lembrar locais das janelas" ligado, o Windows pode devolver uma janela ao monitor reconectado depois da releitura. A barra de tarefas recriada (`TaskbarCreated`) passa pela mesma releitura. Os três tempos são provisórios até o protótipo P5.

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
- **Regra da calma (DEC-022, item 4, nota de 2026-10-01):** com a autonomia pausada ou o painel aberto, quem está agarrado à parede ou ao cipó sem estar preso deixa de estar agarrado, no fim de qualquer evento, e os passos seguintes o fazem descer pela parede ou se soltar do cipó, como quem escala. O preso pelo usuário fica, e o atento a um item na mão do usuário também. Não depende da chave do tamagotchi.

**Implementado com o tamagotchi (DEC-028), só com a chave ligada** (tabelas na seção 2.16):

- **Velocidades da onda:** as velocidades passam pela física efetiva (`Maquina.FisicaEfetiva`) em cinco lugares: andar; escalar e o passeio do preso na parede; pendurado e o passeio do preso no cipó. Ficam de 50% a 200% das de sempre, e o resto da física não muda.
- **Cambaleio:** nas ondas do bêbado e do tonto, o passo de cada quadro da caminhada é multiplicado por uma onda triangular de 48 passos (`Maquina.FatorDoCambaleio`). Ele fica no chão, preso entre as laterais, e o recuo devolve distância ao percurso.
- **Itens:** caem com a mesma gravidade e a mesma queda máxima do personagem, presos entre as laterais do monitor deles, e quicam uma vez, de leve. Um item que deixa de aparecer vai direto ao chão.

### 2.10 Apresentação, assets e expressões

STATUS: PLANNED.

- O núcleo expõe um retrato do estado: estado de comportamento, direção, fase do movimento, expressão e sinais pontuais, como "pousou" ou "foi clicado".
- Um **manifesto de assets** em arquivo de dados liga cada estado a um clipe de animação e cada expressão a uma camada ou variante. O manifesto também define a âncora da imagem, o tamanho lógico e a taxa de quadros de cada clipe.
- **Trocar asset** significa trocar o manifesto e as imagens. O núcleo, o movimento, a arbitragem, o mundo do desktop e a segurança não mudam. Um teste automático roda a mesma suíte do núcleo com dois manifestos diferentes.
- Até a Fase 6, o app mostra quadros estáticos da pixel art, com o chapéu de palha (DEC-018 e DEC-019). Na Fase 4, `PoseDoPersonagem` escolhe uma pose provisória por estado: ciclo de caminhada e de escalada, pendurado, impulso, no ar, caindo, pousando, sentado ou dormindo, segurado, reagindo e os gestos, espelhada para a esquerda.
- **Toon force (DEC-023):** a pose também pode vir achatada (impacto) ou esticada (velocidade), pela dinâmica do movimento (`Dinamica`: velocidade vertical, quiques e foguete). A deformação é da própria pixel art (`Tela.Deformada`, vizinho mais próximo, pés na mesma linha), então a janela, a âncora e a regra do alfa não mudam.
- **Cipó (DEC-024):** na borda de cima, o personagem aparece pendurado num cipó (`cipo-1` a `cipo-3`). Balança em ciclo quando anda pela borda e fica no quadro do meio quando está agarrado. Agarrado à parede, a pose da escalada fica parada.
- **Esconderijo (DEC-025):** em `PEEKING`, e também em `PRESSED` e `REACTING` de quem continua escondido, aparece a pose `escondido`, só com o chapéu, a cabeça e as mãos na borda. Nas laterais, ela é girada 90° (`Tela.Girada`, sem perda).
- **Retrato da emoção dominante e do tamagotchi (DEC-027, DEC-028):** o retrato ganha `EmocaoDominante` (para a marca no menu), `Onda` e `OndaDeFundo` (para as sobreposições), `Uso` e `PassoDoUso` (para o quadro de uso) e `Itens`, inclusive o da mão. A linha canônica das reproduções ganha os trechos `onda=Tipo/Fase/Nível`, `fundo=Tipo/Fase/Nível`, `uso=Item/Verbo/PassodeDuração/Apoio`, `emocao=Nome` e `itens=[Id:Item:Situação:(x,y);…]`, só quando há valor: sem eles, a linha é a de antes. Desde 2026-10-01, o retrato também tem `Carga`, o número de substâncias do episódio da paranoia, e a linha, o trecho `carga=N`, só acima de 0, logo depois de `fundo=`. O resto da carga (a sintética, os itens distintos e o sorteio feito) e o gerador da paranoia ficam fora do retrato e da linha.
- **Arte da emoção dominante e do tamagotchi em `src/Buzzy.Visual/Pixel/`** (passos A1–A4; regras de desenho em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md)):
  - **Chaves:** os nomes dos enums do núcleo em minúsculas, para os itens (`ItensPixel`, 24 × 24 pixels de arte, na densidade do boneco), as 8 caras de efeito (`Rostos.DeEfeito`, com a `paranoico` no fim desde 2026-10-01) e as poses dos 8 gestos da onda (`PosesPixel.DosGestos`, com `olharproteto` e `agachar` no fim). Cada lado confere as mesmas listas nos próprios testes, e `NucleoEArteTestes` (passo T7) compara os enums do núcleo com as listas da arte: itens, verbos, passos do uso, caras e gestos.
  - **`UsosPixel`:** a sequência de quadros de cada verbo, no chão e de frente (`Sequencia`, só para leitura; `Passos`; `Quadro` pelo passo do uso), com a soma igual à duração do uso no núcleo.
  - **`PosesPixel.PorNome`** acha qualquer pose: as de estado e gesto (`Todas`), as dos gestos da onda (`DosGestos`) e as de uso (`UsosPixel.Poses`). As duas últimas ficam fora de `Todas`, e a folha nativa não muda.
  - **`BonecoPixel.Desenhar(pose, expressao, item, efeito, fase)`:** a pose diz como segura o item (`Segura`: nada, na mão B ou, na lança-perfume, o frasco na mão A e o lenço na B). Com um item, o braço que o leva ao rosto se dobra por cinemática inversa de dois ossos (`Alcancar`, `AjustadaAoItem`) até a ponta do item cair na boca ou no nariz. Nas poses de uso, a cara é a da própria pose (expressão nula). `Pontos` dá as mãos, a cabeça, a boca e o nariz da pose, e `ItensColocados`, onde cada item ficou. Da paranoia (2026-10-01): a mão `Mao.Apontando`, o punho com o indicador reto para cima, e o campo `Rosto.Gota`, a gota de suor da cara paranoica, desenhada por cima dos braços da frente.
  - **`EfeitosPixel`:** 9 sobreposições em 3 fases, a nona o suor da paranoia (2026-10-01); a fase 0 é a parada, para quando não há relógio. Elas nunca cobrem `AreaDoRosto(pose, expressao)`, um conjunto de pixels com os traços do rosto (olhos, sobrancelhas, nariz, rubor e boca) e, na cara paranoica, a gota de suor; nem, no cipó, o cipó e a mão que o segura. No suor, a gota que cairia ali salta para o espelho da posição dela, do outro lado da cabeça. `Modificar` muda a pose pela onda só quando `Modificavel` é verdadeiro: no chão e fora das poses de uso. A sobreposição de cada onda e os modificadores estão em IDENTIDADE_VISUAL.md, seção 7b.
  - **`IconesDoMenu`:** o rosto (o recorte de 40 × 32 de `expressoes.png`, pela mesma `Tela.Recortada` da prévia), o item (o desenho do chão), `Fator` pelo DPI, `Ampliar` por vizinho mais próximo, em BGRA com alfa só 0 ou 255, e `DeBaixoParaCima`, que inverte as linhas para um DIB de altura positiva.

- **Na apresentação do tamagotchi (passo T7; `PoseDoPersonagem`, `SpriteProvisorio` e `CacheDeQuadros`):**
  - **Quadro:** `QuadroDoSprite` ganhou o item na mão (a chave da arte, só nas poses de uso), a sobreposição da onda e a fase dela. O quadro inteiro, com o DPI, é a chave do cache.
  - **Uso no chão:** o quadro da sequência do verbo no passo do uso (`UsosPixel.Quadro`), com o item na mão e a cara da própria pose (expressão nula); de frente e sem espelho, como as outras poses de frente.
  - **Uso na parede, no cipó e no esconderijo,** que ainda não têm pose de uso: a pose de quem está ali (`escalando-1` virado para a parede, `cipo-2`, `escondido`, girada nas laterais), sem o objeto, com a cara do retrato, que o núcleo fixa na cara do item durante todo o uso. O apoio do uso decide a pose. No esconderijo, a boca fica fora do quadro, e só os olhos mostram a cara.
  - **Caras:** as caras de efeito e a emoção dominante chegam pela expressão do retrato, nas poses que mostram a cara dele: andando, pendurado no cipó, parado sem gesto, escondido (`PEEKING`) e nos apoios do uso. Nas outras, vale a cara da pose.
  - **Gestos da onda:** as poses de `PosesPixel.DosGestos`, pelo nome do gesto em minúsculas, com a cara da própria pose. A tremedeira alterna com o `parado` a cada 4 passos, com a mesma cara nos dois: só o corpo treme. Os dois da paranoia (2026-10-01), `olharproteto` e `agachar`, têm desenho próprio, com a cara paranoica, também no olhar pro teto do fim do uso que começou a paranoia.
  - **Sobreposição:** só a da onda da frente, pela tabela da seção 7b de IDENTIDADE_VISUAL.md, por cima de qualquer pose, inclusive dos quadros de uso; a onda de fundo não desenha nada. A onda `Paranoico` mostra o suor, com o tremidinho de 1 pixel onde a pose é modificável (2026-10-01). A fase é (passos no estado / 12) % 3 com o relógio ligado e 0 com ele parado; sem sobreposição, fica 0, para o cache não guardar o mesmo desenho uma vez por fase. O modificador de pose da onda só vale onde a pose é modificável.
  - **Cache:** LRU por bytes de pixels, com orçamento de 16 MiB: ler um quadro o renova, os mais antigos saem para caber um novo, e um quadro maior que o orçamento não é guardado. A 100%, cada quadro tem 64 KiB, e cabem 256; a 300%, só 28.
  - **Chave ausente:** a arte continua lançando exceção para um nome que não conhece. A proteção são testes: `PoseTestes` desenha todo quadro que a escolha pode pedir, com todos os valores dos enums (cerca de 3 mil quadros distintos), e `NucleoEArteTestes` compara os enums com as listas da arte.
- **Janela de um item (passo T8; `SpriteDoItem`):** o desenho do chão do item, ampliado pelo DPI sem suavização, com alfa só 0 ou 255, num cache próprio de 4 MiB por item e DPI. Os limites opacos e os pontos de teste da janela saem do mesmo desenho.
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

No tamagotchi (adendo de 2026-10-02 da DEC-028), o sorteio de `IDLE` ganhou uma última opção, fumar um baseado por conta própria, com o peso `PerfilDeEnergia.PesoFumarBaseado`: 1 nos três níveis, calibrado por simulação para cerca de um baseado a cada 4 minutos de tempo elegível na energia Média. O nível muda a frequência pelos intervalos e pelos outros pesos, como em toda ação. As condições estão na seção 2.16, e a calibração, na DEC-028, item 41.

### 2.12 Configurações, persistência e tempo

STATUS: PLANNED. Decisões registradas em DEC-010, DEC-011, DEC-027 e DEC-029. O esquema, na v3 desde o passo P7, a leitura e a gravação estão implementados e cobertos por testes automatizados (bloco A da Fase 5 e passo T1 da seção "Interação" de TODO.md). Desde o passo P7, a raiz lê o arquivo na partida e grava pela agenda, coberta por testes automatizados e de integração.

- **Arquivo:** um JSON com `schemaVersion` na pasta local do usuário. Sem pacote MSIX, a pasta é `%LOCALAPPDATA%\Buzzy`. Com MSIX, é a pasta local do pacote.
- **Conteúdo proposto:** última posição escolhida pelo usuário (seção 2.8), escala do personagem, sempre no topo, iniciar com o Windows, energia `BAIXA`/`MEDIA`/`ALTA` (padrão `MEDIA`), permitir atravessar monitores, modo de tela cheia ligado por padrão e idioma. Opacidade fica fora do MVP. As decisões de produto correspondentes estão em DEC-013/014 e Q-03 a Q-07, Q-09, Q-12 e Q-23; os campos entram nas fases previstas no TODO.md. O esquema v3 guarda a posição, com a borda do esconderijo e a marca de preso, a energia, o modo de tela cheia, atravessar monitores e a emoção dominante (DEC-027, DEC-029).
- **Leitura:** campo desconhecido é ignorado, valor fora da faixa é preso ao limite e arquivo ilegível é trocado pelo `.bak`, se ele for válido, ou pelos valores padrão (DEC-029). Uma cópia do arquivo ilegível é guardada para diagnóstico, no máximo uma.
- **Gravação:** escreve em um arquivo temporário e substitui o original de forma atômica, mantendo o último arquivo bom como `.bak`. A gravação acontece com atraso depois de soltar o personagem e sempre ao sair.
- **Tempo:** o relógio lógico só gera `TICK` enquanto há movimento, animação ou arraste. Em `IDLE` sem animação, em `RESTING` e em `HIDDEN`, não há timer periódico. A agenda autônoma usa um único timer até a próxima decisão. O modo de tela cheia é notificado por eventos limitados do Windows, sem polling global periódico.

**Implementado na Fase 5 (DEC-029: bloco A e passo P7), com a emoção dominante do passo T1 (DEC-027):**

*Esquema, no núcleo (`Buzzy.Core.Persistencia`, sem E/S); a v2 acrescentou a emoção dominante (DEC-027), e a v3, a postura (passo P7):*
- **Formato:** JSON em UTF-8 sem BOM, indentação de 2 espaços, fim de linha `\n`, campos em ordem fixa e números no formato mais curto, sem depender da cultura. Campos:
  - `schemaVersion`, hoje 3;
  - `posicao`, com `chaveMonitor`, `telaDoMonitor` (`esquerda`, `topo`, `direita`, `base`), `fracaoX`, `fracaoY`, `ancoraAbsoluta` (`x`, `y`) e, sempre escritos, `esconderijo` (`"nenhum"`, `"baixo"`, `"esquerda"` ou `"direita"`) e `presoPeloUsuario` (booleano). A postura só existe junto com uma posição;
  - `preferencias`, com `energia` (`"baixa"`, `"media"` ou `"alta"`), `modoTelaCheia`, `atravessarMonitores` e `emocaoDominante`, sempre escrito: `"automatica"` ou o nome de uma das 14 caras de humor em minúsculas ASCII, como `"feliz"`.

  As amostras de referência são `settings-v1.json`, `settings-v2.json` e `settings-v3.json`, em `tests/Buzzy.Core.Testes/Persistencia/Amostras/`. Arquivos v1 e v2 são lidos sem migração e sem aviso: sem a emoção, vale a automática; sem a postura, nenhuma borda e solto.
- **Leitura (`EsquemaDeConfiguracoes.Ler`), que nunca lança.** Só é ilegível o arquivo com mais de 64 KiB contando o BOM, UTF-8 inválido, JSON inválido (comentários e vírgula final são aceitos), mais de 8 níveis contando a raiz, raiz que não é objeto ou `schemaVersion` ausente, não inteiro ou menor que 1. Um escape de surrogate solto num texto lido também torna o arquivo inteiro ilegível.
- **Campo a campo,** com um aviso por caso:
  - campo desconhecido é ignorado, e um repetido vale na primeira ocorrência;
  - valor fora da faixa é preso: frações em [0, 1], coordenadas de −32768 a 32767;
  - tipo errado vale o padrão do campo;
  - a posição é tudo ou nada: exige a chave (1 a 1024 caracteres, UTF-16 válido, sem caractere de controle) e as duas frações finitas. Tela inválida vira desconhecida, e âncora inválida vira (0, 0);
  - a energia só vale pelos três nomes, sem diferenciar maiúsculas;
  - a emoção dominante só vale pelos 15 nomes (`automatica` e as 14 caras de humor), sem diferenciar maiúsculas e nunca por `Enum.Parse`; `null` ou ausente vale a automática, sem aviso; outro valor vale a automática, com aviso;
  - a borda do esconderijo só vale pelos quatro nomes, sem diferenciar maiúsculas; `null` ou ausente vale nenhuma, sem aviso; outro valor vale nenhuma, com aviso. `presoPeloUsuario` ausente vale falso, sem aviso; outro valor que não seja booleano vale falso, com aviso;
  - os avisos nunca levam valores nem nomes vindos do arquivo.
- **Versão futura** (`schemaVersion` maior que a atual): lê os campos que conhece e bloqueia a gravação nesta execução. Toda ampliação do esquema incrementa `schemaVersion`: um build da v2 vê um arquivo v3 como versão futura e não grava por cima dele.
- **Escrita (`Escrever`):** normaliza antes e nunca lança por causa do conteúdo. Frações saneadas e sem `-0`, coordenadas presas, tela vazia omitida, energia fora dos três níveis gravada como `media`, emoção fora das 14 caras de humor gravada como `automatica`, posição com chave inválida omitida e, sem posição, a postura em nenhuma borda e solto; uma borda fora do enum é gravada como `nenhum`. Ler o que foi escrito devolve as configurações normalizadas.

*Arquivos, no adaptador (`Plataforma/ArquivoDeConfiguracoes.cs`):*
- **Quatro nomes, na mesma pasta, e nenhum outro:**
  - `settings.json`, o principal;
  - `settings.json.bak`, a reserva: o principal anterior, válido quando foi substituído;
  - `settings.json.tmp`, o temporário de uma gravação, nunca lido;
  - `settings.corrupt.json`, o último principal ilegível substituído, uma cópia só, nunca lida.
- **Leitura sem efeito colateral:** o principal; se faltar ou não servir, a reserva; senão, os padrões. Não cria, não altera e não impede outro processo de usar nenhum arquivo.
  - Um arquivo com mais de 64 KiB é ilegível sem ser lido.
  - Um arquivo preso tem 3 tentativas, com duas pausas de 100 ms (cerca de 200 ms).
  - Principal inacessível depois delas: vale a reserva ou os padrões, e a gravação fica bloqueada nesta execução, para não sobrescrever o que não se conseguiu ler.
- **Gravação atômica:** apaga um temporário que tenha sobrado e cria outro do zero (`FileMode.CreateNew`, para nunca gravar através de um link), sem buffer e com `Flush(true)`. Depois confere o principal de novo e troca de uma vez:

  | Principal na hora | Ação |
  |---|---|
  | ausente | `File.Move` do temporário |
  | válido | `File.Replace`, guardando o anterior como `.bak` |
  | ilegível | `File.Replace`, guardando-o como `settings.corrupt.json`; a reserva fica como estava |
  | de versão futura | bloqueia a gravação |
  | inacessível | a tentativa falha |

  Uma queda em qualquer ponto deixa o principal anterior, o novo ou só a reserva, nunca um principal presente e ilegível. Uma falha de E/S é tentada de novo, dentro das tentativas pedidas; o erro leva só o tipo e o código da exceção, nunca o caminho. `File.Replace` exige NTFS local.

*Quando gravar (`PoliticaDeGravacao`, no núcleo):*
- com atraso de 2 s depois do último pedido, reiniciado a cada pedido;
- na hora quando o pedido vem de `SUSPENDING`, `SESSION_ENDING`, `CMD_EXIT` ou `SESSION_LOCKED`;
- depois de uma falha, novas tentativas únicas em 2, 10 e 60 s e, depois delas, só no próximo pedido; na hora, até 3 tentativas com 50 ms entre elas;
- nada é periódico: só eventos do usuário ou do sistema pedem gravação; para a posição, os do invariante 18 (seção 2.6).

*Agenda de gravação, na raiz (`Composicao/AgendaDeGravacao.cs`, passo P7), que aplica a política:*
- **Partida:** antes de tudo, a raiz cria o arquivo da execução pela regra da pasta, uma instância só, e o lê uma vez. Qualquer exceção da leitura, também a que não é de E/S, desliga a persistência nesta execução, sem derrubar a partida; o log leva só o tipo e o código. A carga (`Loaded`) leva a posição salva, com a tela do monitor da época, a postura e as preferências, com a emoção dominante e a travessia; sem arquivo, as padrão.
- **Pedidos:** cada `GravarPosicao` troca a posição e a postura do conteúdo desejado, e cada `GravarPreferencias`, as preferências; a raiz nunca lê o estado do núcleo para gravar. Com o mesmo conteúdo do disco, nada é gravado nem agendado. Sem nenhum pedido, nada fica pendente: a partida não grava sozinha. Vindo da reserva ou dos padrões, o primeiro pedido grava e recria o principal.
- **Gesto do usuário:** o disparo com atraso que chega com o botão pressionado, num arraste ou com um item na mão não rearma a espera, o que seria periódico com o ClickLock; o fim do gesto grava.
- **Falhas:** uma falha de E/S, também na gravação na hora, arma as novas tentativas, salvo depois de parar. Uma exceção que não é de E/S desliga a gravação nesta execução, sem derrubar o Buzzy.
- **Encerramento:** descarrega o pendente e para a agenda antes de desmontar o resto; o erro não tratado descarrega o que der, uma vez só e sem lançar. A descarga na suspensão entra com o tratador dela (passo P11): o app ainda não recebe o bloqueio da sessão nem a suspensão, e hoje só a saída e o fim de sessão gravam na hora.
- **Custo:** a E/S é síncrona, na thread da interface. O arquivo tem cerca de 500 bytes, e a gravação levou de 7,7 a 8,9 ms na integração; o tempo vai para o log (seção 2.13.4).

*Pasta e perfis de teste (`Plataforma/PastaDeDados.cs`):*
- **Uma regra só escolhe a pasta da execução, com falha fechada:**
  - persistência desligada: nenhuma;
  - com `--perfil-de-teste NOME`: `%LOCALAPPDATA%\Buzzy\testes\NOME`, e nenhuma se o nome for inválido, nunca a pasta real;
  - sem perfil: `%LOCALAPPDATA%\Buzzy`;
  - sem a pasta local do usuário: nenhuma.

  Sem pasta, nada é lido nem gravado como configuração.
- O arquivo só é criado por essa regra (`ArquivoDeConfiguracoes.DaExecucao`), e a pasta só na primeira gravação. As regras do nome estão em SECURITY.md 3.1.

*Fora do arquivo:* a posição temporária da tela cheia, os monitores ocupados, o cache das chaves dos monitores (seção 2.4) e, do tamagotchi, os itens, o uso, a onda e, desde 2026-10-01, a carga do episódio da paranoia e o gerador dela, que ficam só em memória (DEC-028). O esconderijo e a marca "preso pelo usuário" vão para o arquivo desde o passo P7, na v3, porque a v2 já é a da emoção dominante; a versão futura dos testes passou de 3 a 4 (DEC-029). A ocultação não vai: escondido pela bandeja ou pela sessão, ele aparece ao reabrir, na borda do esconderijo se estava nela. Com retorno de tela cheia guardado, a posição gravada é o retorno, mas a postura é a do estado atual.

### 2.13 Encaixe na stack recomendada

STATUS: PLANNED. WPF com C# e .NET 10 foi escolhida em DEC-006. P1 e P2 foram aceitos nos limites documentados; P3 ainda verifica arraste sem roubo de foco antes da Fase 1. Duas linhas da tabela da seção 2.13.3 dependem de outros protótipos: a identidade estável do monitor depende de P5, e a consulta de aplicativo em tela cheia depende de P7. Esta seção descreve WPF como janela e interface e limita chamadas Win32 diretas ao adaptador de plataforma.

#### 2.13.1 Janelas

| Janela | Tipo | Por quê |
|---|---|---|
| Personagem | `Window` WPF sem borda, do tamanho do sprite, com `AllowsTransparency`; a imagem tem alfa real e a janela não ativa | WPF usa o caminho layered para transparência por pixel. P1 confirma o click-through exato no Windows alvo |
| Painel de energia | Janela WPF própria, compacta e ativável, ancorada ao lado do personagem; contém somente três opções | O personagem não ativa; o painel recebe foco após ação explícita de dois cliques ou menu |
| Configurações | Janela WPF comum, aberta pelo menu | Usa controles e navegação de teclado do framework |
| Menu de contexto e bandeja | Menu nativo do Windows (`TrackPopupMenuEx`) com janela dona temporária, com os submenus da emoção dominante e dos itens e ícones em bitmap (DEC-027, DEC-028; seção 2.16); ícone de bandeja pelo adaptador, com `Shell_NotifyIcon` versão 4 (DEC-016) | Fecha ao clicar fora e devolve o foco mesmo aberto pelo personagem ou por um item, que não ativam; teclado e leitor de tela prontos; sem dependência de terceiros |
| Item do tamagotchi (DEC-028) | `Window` WPF sem borda, uma por item, de 48 × 48 DIP, com `AllowsTransparency`, sempre no topo, janela de ferramenta e não ativa (`WS_EX_NOACTIVATE`, `MA_NOACTIVATE`, `ShowActivated` falso); `WM_GETDPISCALEDSIZE` devolve o tamanho do item no DPI novo. Fica logo abaixo do personagem na ordem Z, e no topo só durante o gesto sobre o item (seção 2.7) | A mesma receita da janela do personagem, validada por P1 e P3, numa janela do tamanho do item. Só a raiz a fecha. Minimizada pelo Windows, volta ao normal na hora, sem esconder o personagem; a correção de DPI e a da minimização da Fase 5 (passos P12 e P14) valem também para ela |

A janela do personagem tem o tamanho do sprite, nunca o tamanho da tela; a de um item, o tamanho do item. A documentação recomenda que a janela layered seja a menor possível, porque cada atualização copia o bitmap inteiro para a memória do sistema, e há relatos de atraso de mouse no sistema todo com overlay de tela cheia.

#### 2.13.2 Onde cada componente da seção 2.2 mora

| Componente | Realização na stack recomendada |
|---|---|
| Núcleo do personagem, mundo do desktop, arbitragem de input, movimento, personalidade não verbal e esquema de configurações | Biblioteca C# pura sem referência a WPF nem a APIs Windows. Recebe geometria e tempo como entrada e pode ser testada sem abrir janelas |
| Adaptador de plataforma | Único módulo que traduz eventos WPF e chama APIs Windows quando necessário: captura e foco do mouse, topologia, DPI, bandeja, sessão, energia e caminho dos dados |
| Apresentação | Janela e composição visual WPF, com imagem transparente dimensionada ao sprite; o desenho não fica ativo quando o estado não muda |
| Configurações e persistência | Serialização JSON versionada; gravação atômica num adaptador de armazenamento local. *Fase 5:* o esquema fica no núcleo (`Buzzy.Core.Persistencia`), com `JsonDocument` e `Utf8JsonWriter`, sem `JsonSerializer` e sem reflexão; a pasta e o arquivo ficam em `Buzzy.App/Plataforma`, e a agenda de gravação, na raiz (seção 2.12) |
| Raiz de composição | Inicialização WPF, ligação entre janelas, adaptador e núcleo; agenda trabalho apenas enquanto necessário |

#### 2.13.3 Correspondência com as APIs do Windows

Cada linha liga uma decisão das seções anteriores ao mecanismo que a realiza. WPF cuida das janelas e controles; chamadas diretas ao Windows ficam no adaptador e usam APIs documentadas pela Microsoft. Nenhuma exige elevação.

| Assunto | Seção | API |
|---|---|---|
| Transparência por pixel e clique que atravessa | 2.7 | `Window.AllowsTransparency` em janela sem borda WPF; framework cria a janela layered. P1 confirma que os pixels alfa 0 deixam o clique passar a outro processo |
| Modo fantasma, click-through total | Q-21 | Acrescentar o estilo transparente à janela layered. **Não faz parte do MVP**, conforme decisão do usuário; um personagem que não recebe clique poderia ficar inacessível |
| Não roubar foco | 2.7 | Responder "não ativar" à mensagem de ativação por mouse, com o estilo que evita ativação, na janela do personagem e nas dos itens |
| Arraste | 2.7 | `SetCapture` ao pressionar, movimento do mouse, `ReleaseCapture` ao soltar, e a mensagem de mudança de captura como ponto único de término. Nunca o atalho que entrega a janela ao laço de mover do sistema, porque ele congela a física |
| Clique ou arraste | 2.7 | Retângulo de arraste do sistema, lido para o DPI do monitor, e o tempo de clique duplo do sistema |
| Coordenadas com sinal | 2.4 | Extrair as coordenadas das mensagens com as macros que preservam o sinal, nunca com as que tratam o valor como sem sinal |
| DPI por monitor | 2.4 | Declarar Per-Monitor V2 no manifesto, não por chamada de função, e tratar a mensagem de mudança de DPI aplicando o retângulo sugerido |
| Topologia | 2.4 e 2.8 | Enumerar monitores e ler informação de cada um, incluindo a área útil; mensagens de mudança de vídeo, de DPI e de mudança da área útil, com agrupamento de rajadas. *Passos P6 e P9:* agrupamento de 300 ms com teto de 1 s, a `TaskbarCreated` pela mesma releitura e uma conferência tardia do lugar das janelas, de disparo único; uma falha ao ler a informação ou o DPI de um monitor torna a leitura incoerente, e só a leitura parcial, o último recurso, deixa esse monitor de fora |
| Monitor que contém um ponto | 2.4, 2.7 e 2.8 | Consulta por ponto retornando nulo fora de qualquer monitor, para detectar o vão entre monitores, e a variante que retorna o mais próximo para prender a posição |
| Identidade estável do monitor | 2.4 | Consulta da configuração de vídeo, só leitura: `GetDisplayConfigBufferSizes` e `QueryDisplayConfig` com os caminhos ativos, e `DisplayConfigGetDeviceInfo` para o nome GDI da fonte e o caminho do dispositivo do alvo. O caminho vira um resumo na hora e nunca é gravado nem registrado (passo P6, DEC-030); o nome amigável e os códigos do EDID nunca são lidos. Nunca o identificador de execução, o identificador do adaptador nem o nome de vídeo, que não são estáveis. As três APIs ficam numa seção própria de `Win32.cs` e não estão na lista proibida; as que mudam a configuração de vídeo estão (SECURITY.md 3.2) |
| Bandeja | Q-03 | Notificação de ícone na versão 4, identificada por janela e número, com recriação quando a barra de tarefas reinicia |
| Menu com submenus e ícones | 2.16 | `InsertMenuItemW` com a posição explícita e `MENUITEMINFO` (80 bytes em x64); cada submenu é anexado logo depois de criado, para o `DestroyMenu` do principal destruí-lo junto, e sem `MNS_CHECKORBMP`, para a marca de rádio e o ícone ficarem lado a lado. O ícone é um DIB de 32 bits (`CreateDIBSection` sem DC, `BI_RGB`, `BITMAPINFOHEADER` de 40 bytes, altura positiva, com as linhas invertidas e preenchido por `Marshal.Copy`, sem `BitBlt`, `StretchBlt` nem `CreateDC`), apagado com `DeleteObject` depois do `DestroyMenu`, que não apaga o bitmap de um item. `AppendMenuW` saiu. As três APIs ficam em `Win32.cs` e não estão na lista proibida (SECURITY.md 3.2) |
| Ordem Z das janelas dos itens | 2.7 | `SetWindowPos` sem ativar: logo abaixo da janela do personagem ou, durante o gesto, `HWND_TOPMOST`. Na releitura da topologia e na conferência tardia, só o lugar, com `SWP_NOZORDER` (passo P9). As janelas dos itens não trouxeram P/Invoke novo |
| Sem botão na barra de tarefas | Q-03 | Estilo de janela de ferramenta |
| Fim de sessão | 2.6 | Responder sim de imediato à pergunta de encerramento e gravar na mensagem de encerramento, com gravação incremental antes. *Passo P7:* o WPF só expõe `SessionEnding`, na pergunta; a gravação acontece nele, na hora (DEC-016, item 10) |
| Bloqueio e desbloqueio de sessão | 2.6 | Registro de notificação de sessão da estação de trabalho, que entrega o bloqueio e o desbloqueio. Junto com a notificação de energia da linha seguinte, é um dos dois itens desta lista que exigem registro explícito; o resto chega nas mensagens comuns da janela |
| Suspensão e tela desligada | 2.6 | Notificação de suspensão e retomada, e notificação de estado da tela da sessão para parar de desenhar com o monitor desligado |
| Repouso | 2.12 | A fila de mensagens bloqueia quando não há nada a fazer. Animação com timer que o sistema pode agrupar, nunca elevando a resolução global do timer |
| Pasta de dados | 2.12 | Consulta de pasta conhecida, sem montar o caminho com texto. *Fase 5:* `Environment.GetFolderPath(LocalApplicationData, DoNotVerify)`; um resultado vazio ou relativo deixa o Buzzy sem pasta, sem configurações e sem log |
| Gravação atômica | 2.12 | `File.Replace` e `File.Move` da biblioteca base, que usam `ReplaceFileW` e `MoveFileExW`, e `FileStream.Flush(true)`; nenhum P/Invoke novo e nenhuma regra do portão afetada. Exige NTFS local |
| Mudança da janela em primeiro plano | DEC-013 | Eventos WinEvent de primeiro plano e mudança de geometria, fora do processo observado; filtrar a janela de nível superior ativa e emitir só os monitores que ela cobre. **Risco a medir em P7 (hipótese):** o evento de mudança de geometria assinado para todo o sistema também dispara por movimentos de cursor e de janelas de outros apps, o que pode acordar o Buzzy continuamente enquanto o usuário joga ou digita. Se P7 confirmar, restringir a assinatura de geometria à janela ativa (reassinando a cada troca de primeiro plano) ou revisar o desenho |
| Janela em tela cheia e monitor ocupado | Q-09 | `GetForegroundWindow`, `GetWindowRect`, `MonitorFromWindow`/`GetMonitorInfo`; comparar o retângulo ativo com os limites do monitor. `SHQueryUserNotificationState` pode ser sinal auxiliar, nunca a única fonte |

#### 2.13.4 Apresentação e repouso

WPF apresenta o sprite numa janela layered. O projeto não pressupõe que o framework seja barato em repouso: a aplicação deve suspender `CompositionTarget.Rendering`, animações e timers periódicos quando nada muda, e reativá-los apenas durante movimento, animação ou arraste. P2 mede CPU, memória, GPU e acordadas em repouso e em duas taxas de animação. Otimizações de desenho só são escolhidas com dados, na Fase 6.

**Implementado na Fase 4 (DEC-022):**

- **Inscrição:** a raiz só se inscreve em `CompositionTarget.Rendering` enquanto o núcleo pede o relógio (`LigarRelogio`) e cancela a inscrição no `DesligarRelogio`. Em repouso, inclusive em `RESTING`, não há inscrição.
- **Lote por quadro:** a cada quadro, o tempo decorrido vira passos fixos, que entram num lote na fila do núcleo. Só o último `MoverJanela` do lote é aplicado, e o sprite é trocado só quando o quadro muda.
- **Atraso:** o acumulador recupera no máximo 250 ms; o excesso é descartado e registrado no log.
- **Log de diagnóstico:** a posição só é registrada quando o personagem para, fora do movimento e do arraste.

**Implementado com o tamagotchi (DEC-027, DEC-028; passos T2 e T7–T9):**

- **Sprite:** troca quando o quadro muda: pose, cara, item, sobreposição, fase, deformação, giro ou DPI. A sobreposição só muda de fase com o relógio ligado; sem relógio, fica na fase 0, e nada no sprite muda sozinho.
- **Temporizador da onda:** um `DispatcherTimer` de disparo único, ao lado do da agenda, ligado só entre um agendamento e o disparo dele.
- **Log de diagnóstico**, só com `--diagnostico` e sem dado pessoal (SECURITY.md 6). É contrato dos testes de integração e da verificação de tela:
  - `MENU|exibindo`, com a emoção marcada (`emocaoMarcada=`, o nome ou `Automatica`) e o número de ícones; `MENU|fechado=`, com o nome do comando de `ComandoDoMenu`, o `argumento=` da emoção ou do item escolhido, `icones`, `bitmapsCriados`, `bitmapsApagados`, `lado` (o rosto), `ladoItem` e `dpi`, gravado depois de apagar os bitmaps; falhas do Windows como `MENU|icone=falhou`, `item`, `submenu` ou `menu`, só com o código;
  - `ITEM`: `mostrado` (com o HWND da janela, o retângulo, o DPI e os pontos de teste opaco e transparente), `movido=Id|parado=sim` (uma linha por pouso no chão, no fim do processamento, mesmo quando o passo do pouso não move a janela, e depois do `RELOGIO|ligado=nao` do mesmo passo; os testes de integração a exigem, e sem ela falham por tempo esgotado), `escondido`, `removido` com o motivo, `solto` (o fim do gesto, com `sobre`, `usado` e a latência M5 do arraste do item), `clique`, `capturaPerdida`, `capturaLiberada`, `menuPedido`, `minimizado`, `fechadas` no encerramento e `desconhecido=sim` para um Id que a raiz não conhece;
  - `ONDA`: `agendada` (com o atraso e a geração), `disparada` e `cancelada`;
  - `PARANOIA` (desde 2026-10-01): uma linha por sorteio da paranoia, saindo ou não, com `item` (o item do uso), `chance` (`1 em 8`), `saiu` (`sim` ou `nao`), `substancias` e `distintas` (os itens de substância distintos do episódio, na ordem do enum), por exemplo `PARANOIA|item=Bala|chance=1 em 8|saiu=sim|substancias=2|distintas=Vodka,Bala`. O núcleo não escreve o sorteio que não sai na regra do soltar, para a linha canônica não mudar; a raiz compara o gerador da paranoia antes e depois de cada evento do lote (`LigacaoDosItens.SorteioDaParanoia`) e escreve a linha quando ele andou. A subida de nível com a paranoia na frente não sorteia e não gera linha;
  - o baseado por conta própria (desde 2026-10-02) não tem linha própria nem linha `ITEM`, porque nenhum item aparece: a linha `NUCLEO` da transição traz a regra (`NUCLEO|evento=AutonomyTimer|…|de=Idle|para=Using|regra=IDLE + AUTONOMY_TIMER: Fumar Baseado por conta própria`), `ONDA|agendada` traz a subida do chapado, de 10.000 ms, `SPRITE` traz os quadros `fumando-*` com `item=baseado` e `efeito=Fumaca`, e `PARANOIA` traz `item=Baseado` quando o baseado dele faz o sorteio do episódio;
  - `SPRITE`: uma linha por quadro desenhado fora do cache, com `item`, `efeito`, `fase`, `bytesEmCache` e `descartados`, além dos campos de antes; `MEMORIA` ganhou `bytesEmCache` e `janelasDeItens`.
- **Leitura do log nos testes:** o log rotaciona ao passar de 1 MB (DEC-016, item 7). Os testes de integração e a verificação de tela leem pelo mesmo código (`LeituraDoLog`): a marca guarda o deslocamento e uma assinatura dos 64 bytes antes dele, e a leitura reconhece a rotação pelo conteúdo, lendo o resto de `diagnostico.1.log` antes do arquivo novo. Antes, as linhas entre a marca e a rotação podiam se perder. A leitura supõe que só o Buzzy rotaciona ou apaga o log durante uma execução. A medição de desempenho mantém a leitura própria, que avança a cada leitura.

**Implementado na Fase 5, bloco B (passos P6, P7 e P9; DEC-029 e DEC-030):**

- **Disparos únicos:** a gravação com atraso e as novas tentativas dela, a releitura da topologia e as novas tentativas dela e a conferência tardia usam o mesmo disparo único (`DisparoUnico`): o temporizador para antes de chamar a ação, e o encerramento cancela o que estiver armado. A releitura roda na prioridade normal do `Dispatcher`, e a gravação, na de fundo.
- **Log de diagnóstico**, só com `--diagnostico` e sem dado pessoal (SECURITY.md 6), também contrato dos testes de integração e da verificação de tela:
  - `TOPOLOGIA`, na partida (`motivo=início`) e a cada releitura publicada: `motivo` (as mensagens agrupadas, na ordem em que chegaram, no máximo 32, com `,+k` no fim quando outras passam do teto), `mudou` e `visivel` (nas releituras), `impressao` e os campos das chaves: `chaves=chave=\\.\DISPLAYn;…`, na ordem do Windows; `consulta`, `ok` ou a falha, só com a função e o código; as contagens `cache`, `reserva` e `semNome`; e, numa leitura parcial, `ignorados` e `falhaDoIgnorado`, só com a função e o código. Uma leitura incoerente sai como `TOPOLOGIA|motivo|erro|mantida=anterior|novaTentativa`;
  - `MENSAGEM|tipo=`: uma linha por mensagem que pode ter mudado a topologia, só com o tipo (`WM_DISPLAYCHANGE`, `SPI_SETWORKAREA`, `WM_DPICHANGED` com o DPI novo ou `TaskbarCreated`), crua, para calibrar o agrupamento no protótipo P5;
  - `POSICAO`: `monitor` é a chave opaca, e `gdi`, o nome GDI dessa chave na última leitura. `POSICAO|reaplicada=sim` leva o `motivo` (as mensagens da releitura ou `reafirmação tardia`), o retângulo `real` e o do `nucleo`;
  - `ITEM`: `monitor` é a chave opaca; `ITEM|reaplicado=Id`, uma linha por janela de item devolvida ao lugar, leva `motivo`, `real` e `nucleo`;
  - `CONFIG`: na partida, `lido` (`principal`, `reserva`, `padroes` ou `desligado`, este com `motivo=opcao`, `pasta` ou `erro` e, no erro, o tipo e o código), os estados do `principal` e da `reserva`, `versao` (`atual`, `anterior`, `futura` ou `-`, nunca o número lido), `avisos`, `tentativas`, `gravacao` (`bloqueada` ou `liberada`) e `pasta` (`perfil` ou `padrao`); a cada pedido, `pedido` (`posicao` ou `preferencias`), `evento`, `imediata` e `gravacao` (`ligada`, `bloqueada` ou `desligada`); a cada gravação, `gravado` (`sim`, `nao` ou `sem mudanca`), `motivo`, `bytes`, `ms`, `principalAntes`, `copiaDeDiagnostico`, `tentativas`, `erro` (o tipo e o código) e `novaTentativaMs`; e `adiado=gesto` quando o disparo chega no meio de um gesto. Nunca a pasta, a chave, a tela nem valores do arquivo.

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

### 2.16 Emoção dominante e tamagotchi adulto: tabelas e regras

STATUS: PLANNED. Implementado no núcleo e no app e verificado por testes automatizados, de integração e, em 2026-10-01, pela verificação de tela com input SINTÉTICO e pelo repouso de 10 minutos com uma onda ativa (DEC-027 e DEC-028; evidências em TODO.md). Continua PLANNED pelas conferências [MANUAL] e [HW] e pela revisão visual e de tom pelo usuário. O tamagotchi fica atrás da chave `ConfiguracaoDoNucleo.Tamagotchi`, ligada no aplicativo desde o passo T9 da seção "Interação" de TODO.md; a emoção dominante não tem chave. Estados, eventos, transições e invariantes estão na seção 2.6; a execução no app, nas seções 2.3, 2.7, 2.10 e 2.13. O alívio, a bala como droga sintética e a paranoia (adicional de 2026-10-01 da DEC-028, passos T10 e T11) estão no núcleo e no app, verificados por testes automatizados e de integração; os casos de tela deles (V17, V17b e V18) ainda não rodaram. O baseado por conta própria (adendo de 2026-10-02 da DEC-028, passo T12) também está no núcleo e no app, verificado por testes automatizados e de integração; o caso de tela dele (V19) ainda não rodou.

Os números são de jogo, escolhidos para o comportamento se ler na tela, sem relação com nada real (DEC-028). As classes dos itens e das ondas também são regra de jogo, pedida pelo usuário em 2026-10-01, e não dizem nada sobre o mundo real. As tabelas ficam em `TabelaDoTamagotchi`, e os testes podem trocá-las (`ConfiguracaoDoNucleo.TabelaDeItens` e `TabelaDeOndas`), como também a chance da paranoia (`ChanceDaParanoia`).

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

*Classe do item* (`DadosDoItem.Alivio` e `Sintetica`): o alívio é a comida e a bebida sem álcool; a droga sintética, "como bala, md, coca e lança", nas palavras do usuário, também é substância; o resto é substância. O cogumelo, mesmo comido, é substância. A bala é sintética e começa o Euforico no nível 1 (o MD, no 2) desde 2026-10-01; antes, era de alívio e começava o Alegre, que ficou sem item e continua no enum e nas tabelas, no mesmo lugar.

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

*Classe da onda* (`DadosDaOnda.DeSubstancia`): uma onda de substância é a que a banana, o café e o energético acalmam, e a que mantém aberto o episódio da paranoia; uma onda leve não mantém o episódio, e a comida e a bebida combinam com ela como antes. A água acalma as duas. A onda `Paranoico` não vem de item nenhum: vem do sorteio da paranoia, e a precedência 4 só ela tem.

*Tempos:*
- a onda nova começa na subida, no nível da intensidade do item;
- da subida, vai ao pico; no pico, cada disparo baixa um nível; no nível 1, vai para a queda, também no nível 1, ou acaba, se a onda não tem queda; a queda acaba no disparo seguinte;
- a queda dura a base × 100%, 125% ou 150%, pelo pior nível atingido no episódio (1, 2 ou 3);
- nenhuma fase dura menos de 1 s;
- sem item novo, a onda gera no máximo 2 + nível disparos. O episódio mais longo é o do Viajando no nível 3: 20 + 3 × 140 + 90 = 530 s.

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

O peso de atravessar monitores, quando a travessia entrar (passo P13 da Fase 5), vai seguir o percentual de andar.

**Combinação, quando ele usa um item** (`AplicarNaOnda`: primeiro `Combinar`, com o alívio; depois a paranoia, no parágrafo seguinte):
- **alívio** (pedido do usuário de 2026-10-01): um item de alívio com onda na frente a acalma um passo e não começa a onda dele. A água acalma qualquer onda; a banana, o café e o energético, só uma onda de substância. O passo (`UmPassoAbaixo`): na subida ou no pico acima do nível 1, um nível abaixo, na mesma fase, sem mexer no temporizador; no nível 1, da subida ou do pico, a queda, com o mesmo pior, a duração cheia e a cara dela, ou o fim, se a onda não tem queda; na queda, o fim, e a de fundo volta, como no fim pelo temporizador. A de fundo nunca é tocada, e nada é sorteado. A regra do soltar termina em `; alivia Tipo/Fase/Nível -> Tipo/Fase/Nível`, `-> fim` ou `-> fim; a de fundo volta: …`. A água sem onda não faz nada. Com uma onda leve na frente, a banana, o café e o energético seguem as linhas abaixo. Até 2026-10-01, só a água acalmava, com uma diferença: na subida do nível 1, ela acabava a onda;
- **sem onda:** começa a do item, na subida, no nível da intensidade;
- **mesmo tipo da onda da frente:** os níveis somam até 3, o pior nível acompanha, e a fase recomeça: a subida continua subida, e o pico ou a queda viram pico;
- **mesmo tipo da onda de fundo:** os níveis dela somam, do mesmo jeito, e ela continua congelada;
- **precedência maior ou igual à da frente:** a do item vai para a frente, na subida; a da frente vira a de fundo, congelada, e a de fundo anterior é descartada;
- **precedência menor:** o item é absorvido; a onda e o temporizador não mudam.

Quando a onda da frente acaba, a de fundo volta à frente com a fase em que estava recomeçada na duração cheia e com a cara dessa fase. Sem onda de fundo, a cara volta à de base, se está livre.

**Paranoia, depois da combinação** (`Maquina.Onda.cs`; pedidos do usuário de 2026-10-01 e a escolha dele das 23:03, DEC-028, itens 28 a 34). É de desenho animado: ele acha que tem alguém no teto.
- **Carga do episódio** (`EstadoDoNucleo.Carga`, registro `CargaDaParanoia`): todo item de substância, isto é, que não é de alívio, entra na carga depois da combinação: uma substância a mais, a sintética se ele for e o item entre os distintos (`ConjuntoDeItens`, com igualdade por valor). O item de alívio não entra. No fim de todo evento, com a chave ligada, se nem a onda da frente nem a de fundo é de substância, a carga volta toda a `CargaDaParanoia.Nenhuma`: as substâncias, a sintética, os distintos e o sorteio feito (`Sorteada`), juntos. A paranoia é de substância, então o episódio dura enquanto ela durar.
- **Mistura com sintética** (`MisturaComSintetica`): pelo menos uma sintética e pelo menos dois itens de substância distintos no episódio, contando o atual. Álcool, maconha, cigarro e cogumelo, sozinhos ou misturados entre si, e uma sintética sozinha, repetida, nunca são mistura com sintética.
- **Com a paranoia na frente:** qualquer substância a sobe um nível, até 3; o pior acompanha, e a fase recomeça, como no mesmo tipo (a queda volta ao pico; a subida continua subida), com o temporizador na duração cheia. Não há sorteio. A regra termina em `; a paranoia sobe: Paranoico/Pico/1 -> Paranoico/Pico/2`.
- **O sorteio único do episódio:** sem a paranoia na frente, o uso de substância que fecha a mistura com sintética, num episódio que ainda não sorteou, faz o sorteio, com a chance `ConfiguracaoDoNucleo.ChanceDaParanoia` (1 em 8; os testes a trocam, por exemplo por 1 em 1 ou 0 em 1). O sorteio é um passo do gerador próprio da paranoia (`EstadoDoNucleo.AleatorioDaParanoia`; `Aleatorio.Sortear`: um inteiro uniforme de 1 a 8, que sai se for 1), semeado com semente × 41 + 13 (`SementeDaParanoia`, em aritmética de 64 bits sem sinal). O gerador principal nunca é usado, e a agenda, as caras e as reproduções gravadas não mudam por causa dele. O gerador da paranoia não volta ao começo com o episódio. Saindo ou não, o episódio fica marcado como sorteado e não sorteia mais até a carga zerar: usar mais coisas na mesma leva não aumenta a chance, e a paranoia que acaba com o episódio continuando não volta por sorteio. O episódio seguinte sorteia de novo.
- **Se sai:** a onda `Paranoico` vai à frente, na subida do nível 1; a frente vai para o fundo, congelada, e a de fundo anterior sai, como manda a precedência 4. A regra termina em `; a paranoia começa: Paranoico/Subida/1`, e o uso fica marcado (`Uso.ComecouAParanoia`) para o olhar pro teto no fim dele (seção 2.6). O sorteio que não sai não deixa texto na regra; a raiz o registra na linha `PARANOIA` do log de diagnóstico (seção 2.13.4).
- **O resto** é o de toda onda: os disparos do temporizador, o alívio que acalma um passo e a de fundo que volta quando ela acaba. Preso, ele continua preso; escondido, continua escondido.
- **Um caso de borda:** o cigarro absorvido pelo ligado do café ou do energético, sem onda de substância no fundo, entra na carga e a zera no fim do próprio soltar. Assim, café, cigarro e MD nunca sorteiam, e cigarro e MD sorteiam.

**Baseado por conta própria** (`Maquina.Itens.cs`; pedido do usuário de 2026-10-01, 19:10, "uma funcionalidade que o macaco fume maconha à vontade quando ele quiser"; adendo de 2026-10-02 da DEC-028, itens 36 a 42). De desenho animado: às vezes, parado no chão, ele fuma um baseado sozinho.
- **A ação** `AcoesAutonomas.FumarBaseado` (64, no fim do enum) fica fora de `Todas`; só a configuração do aplicativo a liga, e ela só vale com a chave ligada.
- **A decisão** (`DecidirParado`): é a última opção do sorteio de `IDLE`, com o peso `PerfilDeEnergia.PesoFumarBaseado` (1 nos três níveis; a onda de um item não o muda) quando ele pode fumar (`PodeFumarPorContaPropria`), e peso zero quando não pode. Ele pode com todas as condições juntas: a chave ligada; `IDLE`; a âncora na borda de baixo da área útil do monitor dele; fora do esconderijo; a autonomia livre e o painel fechado; nenhum item na mão do usuário; e nem `Chapado` nem `Paranoico` na frente. Só a onda da frente conta: com o `Chapado` no fundo, ele fuma, e o fundo soma o nível, até 3, como manda a combinação.
- **O uso** (`FumarPorContaPropria`, que chama `ComecarOUso`, o mesmo caminho do soltar): o uso do baseado da tabela (`TabelaDeItens(Item.Baseado)`: fumar, 210 passos, a cara pensativa), sempre com o apoio no chão, com a combinação e a paranoia como no soltar do baseado; sem onda na frente, começa o `Chapado` na subida do nível 2. Nenhum item nasce nem sai, nenhum Id é gasto e não há efeito de janela de item. O gerador principal só anda o passo do sorteio da agenda. A regra é `IDLE + AUTONOMY_TIMER: Fumar Baseado por conta própria`, com `; a paranoia começa: Paranoico/Subida/1` no fim quando o sorteio sai. O fim é o de todo uso no chão: `SETTLING`, depois `IDLE`, com o olhar pro teto se o uso começou a paranoia.
- **Na paranoia:** é substância e não é sintética; entra na carga como o baseado solto. Sozinho, ou com o álcool, o cigarro e o cogumelo, nunca sorteia; num episódio que já tem sintética e ainda não sorteou, fecha a mistura e faz o sorteio do episódio; e uma sintética dada depois, com ele chapado do baseado dele, também fecha a mistura. Como ele não fuma com a paranoia na frente, o baseado dele nunca a sobe.
- **O usuário prevalece** como em todo `USING` (seção 2.6): o clique interrompe, o menu não, e um item solto nele durante o uso é recusado.
- **Frequência:** com a energia Média e só com a autonomia, cerca de um baseado a cada 3,9 min de tempo elegível (`IDLE` no chão, sem `Chapado` nem `Paranoico` na frente), ou um a cada 14,6 min no total, com o `Chapado` na frente cerca de 39% do tempo; os três níveis estão na DEC-028, item 41.
- **Na tela:** os quadros `fumando-1` a `fumando-5` do baseado solto, com o baseado na mão, a cara da pose e a fumaça do chapado; o app não mudou.

**Cambaleio** (`Maquina.FatorDoCambaleio`): o passo da caminhada é multiplicado por 1 + amplitude × (12 − d) / 1200, com d a distância, em passos, até o meio de uma volta de 48 passos (0,8 s). O fator vai de 1 − amplitude/100, no começo da volta, a 1 + amplitude/100, no meio, com média 1 na volta inteira; a 120%, vai de −0,2 a 2,2. As contas são inteiras até a última divisão. A âncora fica presa entre as laterais, e o percurso que falta diminui pelo passo com sinal.

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

**"Sobre ele"** (`Maquina.SobreOPersonagem`): o retângulo do sprite do personagem encolhe 20% de cada lado (`MargemDoAlvo`), da largura nas laterais e da altura em cima e embaixo, arredondado ao pixel com a metade para cima. Num sprite de 128 × 128 px, são 26 px de cada lado, e sobra um miolo de 76 × 76 px. O item conta como solto sobre ele se o retângulo dele, já preso na área útil, tem ao menos um pixel em comum com esse miolo, com os retângulos semiabertos, como o `RECT` do Windows.

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
- **Nascimento**, no monitor do personagem. As posições candidatas ficam ao lado dele, a meia largura do personagem + 8 DIP + meia largura do item, e depois mais longe, de uma largura de item + 8 DIP de cada vez, até duas vezes; primeiro do lado para onde ele olha, depois do outro. Vale a primeira dentro dos limites laterais e sem cruzar outro item fora da mão no mesmo monitor; se nenhuma servir, a primeira, presa entre as laterais. A âncora começa 140 DIP acima dos pés dele, onde quer que ele esteja (no chão, 140 DIP acima do chão), nunca acima da borda de cima. O item nasce caindo, com o próximo Id.
- **Queda**, a cada passo: a gravidade e a queda máxima do personagem, presa entre as laterais. Ao tocar o chão a 300 DIP/s ou mais, quica uma vez, com 35% da velocidade; depois para. Não achata ao pousar.
- **Segurar:** só um item visível. A pegada é o cursor menos a âncora. Outro item que estivesse na mão é largado de onde estava, com a captura solta. Pegar um item no ar é aceito: ele para e fica na mão.
- **Arrastar:** a âncora é o cursor menos a pegada, sem prender, no monitor da âncora.
- **Soltar ou largar:** a âncora é presa na área útil do monitor dela. No chão, o item fica; no ar, cai de novo e pode quicar outra vez.
- **Topologia** (passo P8 da Fase 5; DEC-030): a mesma regra do personagem (seção 2.8). No monitor que só foi transladado, o item anda junto e continua caindo ou no chão, com a mesma velocidade. Senão, a posição dele acompanha a topologia (`Rebasear`) e é reacomodada pela posição relativa, no monitor correspondente ou no sobrevivente; fora do chão, volta a cair. O item na mão anda com o monitor em que está, com a altura fina junto: soltá-lo logo depois o deixa no mesmo monitor físico.
- **Esconder e sair:** o item da mão solta a captura e fica no chão; os que caíam vão direto ao chão, cada um na coluna em que estava.
- **Janelas:** comparando o começo e o fim do evento, primeiro os que saíram (`RemoverItem`, com o motivo); depois, item a item, na ordem do Id: o que deixou de aparecer (`EsconderItem`), o que passou a aparecer ou nasceu à vista (`MostrarItem`) ou o que mudou de lugar à vista (`MoverItem`, com o monitor e a âncora).

**Menu** (passos T2 e T8; `MenuNativo`). É o mesmo pelo botão direito no personagem, num item e no ícone da bandeja. Os textos vêm de `Textos.resx` e só nomeiam as opções, sem descrição:

| Linha | Id | Tecla de acesso | Regra |
|---|---|---|---|
| Esconder Buzzy / Mostrar Buzzy | 1 | E / M | como antes |
| Pausar movimento / Retomar movimento | 3 | P / R | como antes |
| separador | | | |
| Emoção dominante ▸ | — | D | habilitada mesmo com o Buzzy escondido: o núcleo grava a escolha, e a cara aparece quando ele voltar |
| Itens ▸ | — | I | só existe com a chave do tamagotchi ligada; desabilitada com o Buzzy escondido |
| separador | | | |
| Sair | 2 | S | como antes |

- **Submenu "Emoção dominante":** "Automática" (id 999, tecla A), um separador e as 14 caras de humor na ordem de `Expressoes.DeHumor` (ids 1000 a 1013): Neutro (N), Feliz (F), Rindo (R), Curioso (C), Surpreso (S), Assustado (U), Sonolento (O), Bocejando (B), Dormindo (D), Travesso (T), Entediado (E), Pensativo (P), Empolgado (M) e Determinado (I). Todas são opções de rádio, com a marca na escolha atual, ou em "Automática" sem dominante; cada cara leva o rosto como ícone.
- **Submenu "Itens":** os 13 itens na ordem do enum `Item` (ids 2000 a 2012): Banana (B), Água (G), Vodka (V), Cerveja (C), Baseado (S), Cigarro (I), Cocaína (O), MD (M), Lança-perfume (L), Café (F), Energético (N), Cogumelo (U) e Bala (A), cada um com o desenho do chão como ícone; um separador; e "Recolher itens" (id 2999, tecla R), desabilitado sem itens na tela.
- **Escolha:** o id que o Windows devolve vira comando, emoção ou item por listas fixas (`Expressoes.DeHumor` e `TabelaDoTamagotchi.Itens`), nunca por conversão do número; qualquer outro id não escolhe nada. A emoção envia `CMD_SET_DOMINANT_EMOTION`, o item `CMD_SUMMON_ITEM` e "Recolher itens" `CMD_CLEAR_ITEMS`. O estado que o menu mostra é lido na abertura (`ModeloAoAbrir`), e a escolha vale por ele, que é o texto que o usuário leu.
- **Ícones:** o rosto é a célula de 40 × 32 pixels de arte de `expressoes.png`, e o item, o desenho do chão de 24 × 24, os dois ampliados por vizinho mais próximo pelo fator inteiro do DPI do monitor em que o menu abre (`DpiAoAbrir`; o fator é o DPI dividido por 96, no mínimo 1: 1× até 191 DPI, 2× de 192 a 287, 3× de 288 a 383), porque o Windows não amplia o bitmap de um item de menu. Em alto contraste, o menu fica só com texto. Com a chave ligada, cada abertura cria 27 bitmaps, 14 rostos e 13 itens, mesmo com o submenu "Itens" desabilitado, e apaga os 27.
- **Uma abertura:** a lista de entradas é uma função pura (`Entradas`), testável sem o Windows. A montagem (`ComMenuMontado` e `ComMenuMontadoNoDpi`) cria o menu, anexa cada submenu logo depois de criá-lo, põe as linhas por posição explícita, mostra o menu, destrói-o e só então apaga os bitmaps, mesmo com erro. Uma falha do Windows deixa a linha sem ícone, ou de fora, e vai para o log só com o código.

**Reprodução gravada:** a referência `tests/Buzzy.Core.Testes/Referencias/07-tamagotchi.txt` roda com a diretiva de cabeçalho `# tamagotchi: sim`, que liga a chave, junto com `# movimento: sim` e `# queda-fisica: sim`; a 06 fica reservada para a Fase 5. Ela cobre a emoção dominante; a vodka invocada, pega no chão e usada; o lança-perfume pego no ar e usado por cima, com o bêbado de fundo; pressionar no meio do uso; a água, que acaba a queda do tonto e devolve o bêbado; a banana solta longe; e o bêbado até a queda. Desde 2026-10-01, a linha da água registra o alívio (`; alivia Tonto/Queda/1 -> fim; a de fundo volta: Bebado/Pico/2`), e o retrato tem `carga=` durante o episódio. A vodka e o lança-perfume são uma mistura com sintética: o lança-perfume faz o sorteio do episódio, que não sai e não deixa texto. Por isso a 07 depende da semente 2028 e da conta semente × 41 + 13. A referência `08-paranoia.txt`, com as mesmas diretivas e a semente 2048, em que o primeiro sorteio da paranoia sai, conta a paranoia: a vodka e o baseado não sorteiam; a bala fecha a mistura com sintética, e a paranoia começa; no fim do uso da bala, ele olha pro teto, e a cara muda com o pico no meio do gesto; a água acalma um passo, e o pico vira queda; a paranoia acaba, e o eufórico da bala volta do fundo e vai até o fim, com a carga de volta a 0. A referência `09-baseado-por-conta-propria.txt` (desde 2026-10-02), com as mesmas diretivas, a semente 2 e `# acoes: TrocarExpressao,FumarBaseado`, conta o baseado por conta própria: a primeira decisão é o baseado, sem item no mundo, com o uso de 210 passos no chão, o `Chapado/Subida/2` e `carga=1`, sem sorteio; a subida vira pico antes da decisão seguinte, e, no pico 2, no pico 1 e na queda, chapado, a agenda só troca a cara; a onda acaba, e a carga volta a 0; a decisão seguinte é o baseado de novo, e o clique o interrompe aos 60 passos, com a onda continuando; depois da reação, a subida vira pico, e a agenda só troca a cara. A ordem segue a do tempo real onde ele obriga, na energia Média: a subida, de 10 s desde o começo do uso, acaba antes da decisão seguinte ao uso ou à reação, que vence 12,8 s ou mais depois deles. No resto, a história é roteirizada, com uma decisão por fase do chapado, quando, no tempo real, são várias, todas só de cara. As referências 01 a 08 não têm a ação nova (a 01, a 02 e a 03 usam todas as ações de sempre; a 04, a 05, a 07 e a 08 listam ações sem ela) e continuaram idênticas byte a byte. Os eventos novos se escrevem `CmdSetDominantEmotion emocao=Nome` (ou `Automatica`), `CmdSummonItem item=Nome`, `CmdClearItems`, `ItemPress id= x= y=`, `ItemDragStart id=`, `ItemDragMove id= x= y=`, `ItemDragEnd id= x= y=`, `ItemRelease id=` e `ItemEffectTimer geracao=`; sem a geração, vale a agendada.

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
| 2026-09-30 | Fase 5, bloco A: a posição guarda a tela do monitor; a partida restaura em cascata; a reacomodação mede pelo pixel dos pés; esquema v1 do `settings.json` no núcleo; arquivo com gravação atômica e perfis de teste no adaptador, ainda não usados pela raiz; invariante 18 (seções 1, 2.6, 2.8, 2.12 e 2.13). | DEC-029, DEC-030 |
| 2026-10-01 | Emoção dominante e tamagotchi adulto no núcleo e na arte, com a chave do tamagotchi desligada no aplicativo: estado `USING`, eventos e efeitos dos itens e da onda, dimensões novas, invariantes 22 a 29, esquema v2, retrato e linha das reproduções, referência 07, arte em `Buzzy.Visual` e regra da calma (seções 1, 2.3, 2.6, 2.9, 2.10, 2.12 e 2.16). | DEC-027, DEC-028 |
| 2026-10-01 | O app da emoção dominante e do tamagotchi, com a chave ligada no aplicativo: menu com submenus e ícones em bitmap; quadro de uso, caras, gestos e sobreposição da onda, com cache limitado a 16 MiB; uma janela por item, com árbitro próprio e ordem Z por evento; temporizador da onda de disparo único; contrato do log e leitura dele através da rotação (seções 1, 2.3, 2.6, 2.7, 2.10, 2.13 e 2.16). | DEC-027, DEC-028 |
| 2026-10-01 | Fase 5, bloco B (passos P6–P9): chave estável do monitor por um resumo do caminho do dispositivo, com cache e reserva, e leitura incoerente ou parcial; persistência ligada na raiz, com a agenda de gravação e o esquema v3, que guarda a postura; topologia em execução no núcleo, com as posições que acompanham os monitores, o estado que continua quando o monitor do personagem só foi transladado e as saídas do gesto; releitura agrupada com teto, novas tentativas, conferência tardia sem ordem Z e a barra recriada pela mesma releitura; invariantes 19 e 20; correção das laterais num monitor mais estreito que o sprite; contrato do log (seções 1, 2.4, 2.5, 2.6, 2.7, 2.8, 2.12, 2.13 e 2.16). | DEC-029, DEC-030 |
| 2026-10-01 | A pedido do usuário, alívio, bala como droga sintética e paranoia: classe de cada item e de cada onda; o alívio na combinação; a onda `Paranoico`, de precedência 4; a carga do episódio, com o sorteio único de 1 em 8 num gerador próprio; o olhar pro teto no fim do uso que começou a paranoia; a linha `PARANOIA` do log; o suor, a cara paranoica e os gestos `olharproteto` e `agachar` na arte e na apresentação; a referência 08 (seções 1, 2.6, 2.10, 2.12, 2.13.4 e 2.16). | DEC-028 |
| 2026-10-02 | A pedido do usuário, o baseado por conta própria: a ação autônoma `FumarBaseado`, fora de `Todas` e ligada só na configuração do aplicativo, com o peso `PesoFumarBaseado`; o uso do baseado da tabela, sem item no mundo, pelo mesmo caminho do soltar; os invariantes 11, 22 e 23 ajustados; a referência 09 (seções 1, 2.6, 2.11, 2.13.4 e 2.16). | DEC-028 |

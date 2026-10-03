# TODO.md — Fases e pendências do Buzzy

> Roadmap operativo único. Cada fase termina com build, testes e verificação do próprio escopo; a Fase 10 é a regressão integrada, não o primeiro momento de testar.
>
> **Formato.** Pendente: `- [ ]`. Concluída: `- [x]` com a data, o resultado e o arquivo de evidência, numa linha; o detalhe vai para [arquivo/todo-historico.md](arquivo/todo-historico.md), que guarda as evidências completas até 2026-10-02. Cada fase lista objetivo, inclui, exclui, depende de, critérios de aceitação, testes e condição de conclusão. Marcadores:
>
> - **[AUTO]** teste automatizado, executado no build.
> - **[MANUAL]** inspeção manual em Windows, com resultado registrado no DEVELOPMENT_LOG.md.
> - **[HW]** exige hardware específico: mais de um monitor, escalas diferentes, monitor em retrato ou possibilidade de conectar e desconectar. Nunca é declarado feito sem o hardware.
>
> Última atualização: 2026-10-02.

## Fase atual

**Fase 5 — multi-monitor completo e posição persistida, em andamento.** Os passos P1–P13 estão implementados e verificados por testes automatizados e de integração; o próximo é o P14, que depende do protótipo P6 [HW]. Faltam também a verificação de tela com input SINTÉTICO (bloco B e passo P15) e as medições de 10 minutos.

**Intercalada com a Fase 5: a seção [Interação — emoção dominante e tamagotchi adulto](#interação--emoção-dominante-e-tamagotchi-adulto)** (DEC-027, DEC-028). Os passos T1–T13 estão feitos, com a chave do tamagotchi ligada no app, e verificados por testes automatizados, de integração e pela tela com input SINTÉTICO (V1–V19); a V20, da chave "Conteúdo adulto" (T13), está escrita e não rodou. Faltam a revisão visual e de tom pelo usuário e as pendências [MANUAL] e [HW] da seção.

O usuário autorizou as Fases 1–11 sem aprovação rotineira (DEC-015). Todo input de `SendInput` citado nesta página é sintético; os dez movimentos de mouse de 26/09 foram exploração informal da namorada do usuário e não contam como evidência humana. O código de `spikes/` é descartável.

### Fechamento da Fase 0

- [x] Visão, arquitetura, segurança e roadmap nas fontes canônicas; WPF/C#/.NET 10 em DEC-006; decisões do usuário em DECISIONS.md; autorização contínua em DEC-015.
- [x] P1 e P2 aceitos nos limites desta página. P3 validado com receptor isolado em 2026-09-29: 3/3 rodadas, 28/28 cenários por rodada, inclusive B4b, com input SINTÉTICO (`spikes/resultados/p3-receptor.log`).
- [x] Fase 0 e Etapa 0B fechadas tecnicamente em 2026-09-29, limitadas ao ambiente medido; DPI misto continua sem hardware.

## Etapa 0B — Protótipos de viabilidade

STATUS: VERIFIED em 2026-09-29 para os gates P1–P3, no ambiente medido. O código é descartável e fica em `spikes/` (como rodar: `spikes/README.md`; scripts em `spikes/ferramentas/`). P5–P8 e P10 acontecem no início das fases que dependem deles. P4 foi aposentado e P9 encerrado; os protótipos não são renumerados.

**Regra de veredito.** O produto não injeta input: `SendInput` só vale em ferramentas de teste fora do executável (SECURITY.md 3.2). Registre input sintético e observação humana em separado. P3 usa um receptor controlado pelo spike e confere eventos e foco sem inspecionar outro aplicativo nem depender do Bloco de Notas. Um P1 ou P3 que falhe reabre DEC-006 antes do produto; o P2 informa as metas Q-08.

| ID | Resultado registrado | Limites e próxima ação |
|---|---|---|
| P1 | **PASS aceito pelo usuário em 2026-09-27, no ambiente medido.** Alfa 0 deixa o clique passar ao Bloco de Notas; alfa 1, 128 e 255 ficam no Buzzy — pela sonda `WindowFromPoint`, por 12/12 cliques **sintéticos** (`SendInput`) e por um clique **humano** na faixa alfa 1 —, sem roubo de foco; `WS_EX_LAYERED` confirmado. | Os cliques nas faixas 0, 128 e 255 foram sintéticos, a pedido do usuário. Vale para Windows 11 a 96 DPI; escala mista sem hardware. |
| P3 | **PASS técnico em 2026-09-29, input sintético.** 3/3 rodadas, 28/28 cenários em cada, inclusive B4b, clique após Alt+Tab e os nove marcadores entregues ao receptor com foco. M5: médias de 0,238 a 0,270 ms, p95 de 0,533 a 0,705 ms. Cursor sem deriva, ClickLock original restaurado, processos encerrados. Windows 11 build 26200, .NET 10.0.12, dois monitores 1920×1080 a 96 DPI, secundário em x negativo (`spikes/resultados/p3-receptor.log`, `spikes/resultados/p3.log`). | Sem escala mista; input sintético não é evidência humana. |
| P2 | **Medição de 2026-09-26, aceita como viabilidade em 2026-09-27.** Repouso de 60 min: CPU média 0,000% de um núcleo (máx. 0,14%), 0,15 troca de contexto por segundo, memória privada estável em ~61,6 MB, GPU 0%, nenhuma atividade periódica evitável. Animação: 10 qps pedidos → 9,1 entregues (0,48%); 60 qps por `DispatcherTimer` → 39,1 (1,8%); 60 qps pelo compositor → 40,3 (2,6%). A resolução do timer global nunca mudou. | Metas Q-08 aceitas, ainda não verificadas no app. A animação não chegou a 60 qps e a memória privada cresceu (~72 → 119 MB em 10 min): investigar antes da Fase 6. |

Protótipos pendentes ou encerrados (P1–P3 acima):

| ID | Pergunta | Teste mínimo | Resultado esperado | Hardware |
|---|---|---|---|---|
| P4 | **Obsoleto, não executar:** foco e IME de uma conversa, recurso removido pelo usuário. | — | Não cria gate; a Fase 8 verifica só o painel de energia. | — |
| P5 | Quais mensagens chegam quando a topologia muda, e a chave do monitor é estável? | Janela que registra mensagens e a leitura da topologia ao conectar, desconectar, rearranjar, girar, trocar o principal e trocar a escala. Reiniciar e trocar portas, comparando as chaves. Repetir com as opções do Windows 11 de lembrar posições e minimizar janelas ao desconectar. | Lista de mensagens por cenário, intervalo de agrupamento calibrado, estabilidade da chave confirmada ou refutada. | 2 monitores [HW] |
| P6 | Como a janela se comporta ao cruzar monitores de escalas diferentes? | Janela movida por código de um monitor a 100% para um a 200%, parando sobre a borda. | Ponto em que a escala troca, sem oscilação, com histerese, e pés sempre no chão. | 2 monitores com escalas diferentes [HW] |
| P7 | É possível detectar tela cheia e mover o Buzzy sem ler identidade ou conteúdo de outros apps? | Protótipo por eventos: observar mudanças da janela em primeiro plano e de geometria, consultando só o retângulo e o monitor da janela ativa. Testar vídeo em tela cheia, jogo sem borda e exclusivo; ir a um monitor livre, restaurar a posição, respeitar ocultação e arraste manual, ocultar sem monitor livre. Não enumerar processos ou janelas nem ler título, texto ou pixels. | Casos certos em 2 monitores, posição e preferências restauradas, sem roubo de foco nem timer em repouso; mover ou ocultar o Buzzy não tira o jogo do modo exclusivo nem o minimiza. Medir CPU e acordadas (M1/M2) com o Buzzy parado **enquanto o usuário move o mouse e digita em outro app**, contando os eventos por segundo, dentro das metas de repouso de Q-08. Arrastar ou mostrar durante a tela cheia prevalece. Se os eventos não bastarem, exigirem polling ou acordarem o Buzzy a cada movimento do mouse mesmo com a assinatura restrita à janela ativa, o modo fica sem implementação até revisar o desenho. | 2 monitores [HW]; reserva de 1 monitor em teste automatizado |
| P8 | A janela layered aguenta ser fotografada e continua funcionando? | Outro processo tira foto da janela durante a animação. | A atualização continua, ou falha e se recupera religando o estilo. | 1 monitor |
| P9 | **Encerrado:** comparava o esforço de uma interface nativa; perdeu a finalidade com a escolha do WPF. | — | Não bloqueia o projeto. | — |
| P10 | Build, tamanho e portão de segurança. | Build de release; medir o executável e o ZIP; rodar o ZIP sem assinatura em máquina virtual limpa, com o controle de aplicativos ligado; rodar o portão de APIs. | Tamanho registrado, aviso ou bloqueio sem assinatura conhecido, portão passando. Não publicar o build. | Máquina virtual |

## Roadmap do MVP

### Trabalho visual paralelo — identidade original

STATUS: VERIFIED como trabalho visual em 2026-09-29; os clipes e as expressões integrados ficam na Fase 6.

- [x] Identidade refeita como pixel art fiel às pranchas, com o chapéu de palha, a pedido do usuário (2026-09-29, DEC-018, DEC-019): critérios em IDENTIDADE_VISUAL.md, gerador em `src/Buzzy.Visual/Pixel/`, folha `assets/identidade/pixel/buzzy-poses.png` (21 poses, 14 expressões e o ícone da bandeja de 16 × 16; nenhuma pose encosta na borda). A direção vetorial anterior foi arquivada em `assets/identidade/arquivo-vetorial/`. O app mostra o quadro "parado" e o ícone da bandeja.

Toda fase começa PLANNED e não é concluída só porque compila ou porque as tarefas foram marcadas. A condição de conclusão de todas inclui:

- build limpo;
- testes automatizados verdes;
- portão de APIs proibidas e auditoria de dependências verdes;
- critérios de aceitação verificados e registrados;
- documentação sincronizada;
- gate registrado nesta página e no DEVELOPMENT_LOG.md.

### Fase 1 — Shell do desktop

STATUS: PLANNED. Implementação verificada por build, testes automatizados e verificação de tela com input sintético; faltam critérios manuais (DEC-015).

- **Objetivo:** janela do personagem transparente, posicionada corretamente no desktop virtual, com ciclo de vida completo.
- **Inclui:** projeto e build da stack; manifesto Per-Monitor V2 e `asInvoker`; janela do tamanho do sprite com transparência por pixel; sprite provisório estático; janela que não ativa; sempre no topo, bandeja e nenhum botão na barra de tarefas (Q-03); instância única, em que abrir de novo mostra o Buzzy existente; mundo do desktop com monitores e área útil; releitura da topologia e acomodação da posição; saída pelo menu; portão de APIs no build; script de medição de desempenho; instruções de build e teste em PROJECT_CONTEXT.md.
- **Exclui:** arraste, movimento, animação, persistência, painel de energia.
- **Depende de:** Fase 0 e gates P1, P2 e P3. Alvo Windows 11, Q-03 e o ZIP pessoal de Q-10 decididos.
- **Critérios de aceitação:**
  1. O app inicia e mostra o sprite sobre a área útil do monitor principal. [MANUAL]
  2. O Buzzy fica sempre no topo por padrão e o ícone da bandeja aparece. [MANUAL]
  3. O menu da bandeja esconde, restaura e encerra o Buzzy; o botão direito no personagem abre o mesmo menu. [MANUAL]
  4. Abrir o app uma segunda vez revela a janela existente sem criar outra instância. [MANUAL]
  5. Clicar em pixel transparente dentro do retângulo da janela entrega o clique ao aplicativo de baixo. [MANUAL]
  6. Clicar no sprite não tira o foco do aplicativo ativo. [MANUAL]
  7. O Buzzy não aparece na barra de tarefas nem no Alt+Tab (Q-03). [MANUAL]
  8. O sprite fica nítido com a escala do Windows em 100%, 150% e 200%; a escala do próprio Buzzy, em passos fixos, é da Fase 8. [MANUAL]
  9. Trocar resolução, escala ou posição da barra de tarefas com o app aberto mantém o sprite dentro da área útil. [MANUAL]
  10. Sair pelo menu encerra o processo e todos os processos filhos. [MANUAL]
  11. Parado por 10 min, o consumo de CPU, memória e acordadas fica registrado como linha de base. [MANUAL, instrumentado]
- **Testes automatizados:** mundo do desktop com topologias de exemplo (lado a lado, empilhado, em L, coordenadas negativas, principal fora da esquerda, retrato, escalas mistas, vão entre monitores); teste de fumaça que inicia o app, encontra a janela, confere os estilos e encerra; portão de APIs proibidas. [AUTO]
- **Verificação com hardware:** critério 6 com dois monitores. [HW]
- **Evidência (2026-09-29, input SINTÉTICO):** `resultados/verificacao-fase1.log`, 25 OK, 4 SIMULADO e 0 falhas: critérios 1, 2, 4, 5, 6, 7 e 10 e o menu do personagem passaram; o menu da bandeja foi SIMULADO, porque o ícone estava na área oculta. Critério 6 com dois monitores, ambos a 100%. Critério 11: 600 s em repouso, CPU média 0,000% de um núcleo, memória privada 56,67 → 56,51 MB, nenhuma rede em 58 verificações e nenhum processo filho em 629 (`resultados/desempenho-20260929-215550.txt`); não valida metas de 1 h e 8 h. Última regressão: 25 OK e 4 SIMULADO em 2026-10-01.
- **Pendentes:** critério 3 com a bandeja real; critério 8 a 150% e 200% (só observado a 100%); critério 9 com mudanças reais de resolução, escala e barra (só mensagens sintéticas na integração).

### Fase 2 — Núcleo do personagem

STATUS: VERIFIED em 2026-09-30 nos critérios da fase; o critério 4 foi verificado com input SINTÉTICO, não por gesto humano.

- **Objetivo:** núcleo determinístico, testável sem janela, com máquina de estados, eventos, relógio lógico e expressão separada.
- **Inclui:** tipos de estado, evento, retrato e efeito; tabela de transições de ARCHITECTURE.md 2.6; fila com prioridade; agenda autônoma com semente; energia como parâmetro do núcleo (padrão Média; a escolha pelo usuário chega na Fase 8); autonomia pausada e gesto curto como dimensões (ARCHITECTURE.md 2.6); expressão independente; relógio que para quando nada muda; gravação e reprodução de sequências de eventos para testes; ligação do núcleo à janela da Fase 1.
- **Exclui:** física de movimento (Fase 4), gestos reais de mouse (Fase 3), animação (Fase 6).
- **Depende de:** Fase 1.
- **Critérios de aceitação:**
  1. Toda transição da tabela tem teste. [AUTO]
  2. Invariantes 1, 6 e 7 de ARCHITECTURE.md 2.6 valem para milhares de sequências aleatórias de eventos. [AUTO]
  3. Sem movimento nem animação, nenhum `TICK` fica agendado. [AUTO]
  4. O app da Fase 1 continua funcionando com o núcleo ligado. [MANUAL]
- **Testes automatizados:** unidade, propriedade com sequências aleatórias e reproduções gravadas comparadas com um resultado de referência. [AUTO]
- **Evidência (2026-09-30):** critérios 1–3 por `TransicoesTestes`, `TransicoesComplementaresTestes` e `TelaCheiaTestes`, pela propriedade com 2.000 sequências de 200 eventos (`InvariantesTestes`), pelas reproduções 01–05 e pelo relógio desligado no núcleo e no app (`ComposicaoTestes`, log `RELOGIO`/`AGENDA`); a auditoria adversarial do gate corrigiu lacunas da tabela (DEC-020). Critério 4 com input SINTÉTICO: `Buzzy.Verificacao --injetar-input-na-tela` com o núcleo, 25 OK, 4 SIMULADO e 0 falhas, mais uma observação INFORMAL do usuário com a bandeja real. Repouso de 10 min com o núcleo: CPU média 0,010% de um núcleo, p95 0,000%, memória 57,99 → 58,00 MB, sem rede, filhos nem mudança do timer (`resultados/desempenho-20260930-112125.txt`).

### Fase 3 — Input e arraste

STATUS: PLANNED. Implementação verificada por testes automáticos, integração por mensagens postadas e verificação de tela com input SINTÉTICO (2026-09-30); pendem os critérios 4 (UAC), 5 (escalas diferentes) e 7 (ClickLock ligado).

- **Objetivo:** clique e arraste com prioridade absoluta do usuário.
- **Inclui:** arbitragem de gestos com o limiar de arraste do sistema lido por DPI; clique, clique duplo, botão direito e perda de captura; arraste manual, sem o loop modal de mover do Windows; validação ao soltar; menu de contexto mínimo; posição no chão ao soltar sem apoio (a queda animada é da Fase 4).
- **Exclui:** caminhada, escalada, pulo e queda com física; painel de energia (Fase 8).
- **Depende de:** Fase 2; P3.
- **Critérios de aceitação:**
  1. Durante o arraste, o personagem acompanha o cursor sem atraso visível e não anda, não pula, não foge e não escala. [MANUAL e AUTO]
  2. Soltar em qualquer ponto, inclusive num vão entre monitores, deixa a âncora dentro de uma área útil. [AUTO e MANUAL]
  3. Soltar sem passar do limiar gera clique; passar do limiar gera arraste. [AUTO e MANUAL]
  4. Alt+Tab, tecla Windows ou janela de UAC durante o arraste encerram o arraste sem travar o personagem. [MANUAL]
  5. Arrastar para outro monitor funciona com coordenadas negativas e escalas diferentes. [MANUAL][HW]
  6. Clicar e arrastar não tiram o foco do aplicativo ativo. [MANUAL]
  7. Com ClickLock ligado, o arraste funciona sem tempo limite. [MANUAL]
  8. O consumo de CPU durante o arraste fica medido e registrado. [MANUAL, instrumentado]
  9. Clique duplo é reconhecido dentro do intervalo do Windows, e o botão direito pede o menu de contexto. [AUTO e MANUAL]
- **Testes automatizados:** reconhecedor de gestos (limiar por DPI, clique duplo, perda de captura, botão direito); invariantes 1 e 2 sob sequências aleatórias; validação ao soltar sobre as topologias de exemplo. [AUTO]
- **Evidência (2026-09-30):** [AUTO] `ArbitroDeGestosTestes` (também ClickLock sem limite de tempo), `ArbitroPropriedadesTestes` (2.500 sequências; invariantes 1, 2 e 5), `SoltarNasTopologiasTestes` (8.381 soltares em 17 topologias, 5.140 em vãos ou fora dos monitores) e a integração `GestosTestes`. Com input SINTÉTICO (`--fase 3`, `resultados/verificacao-fase3.log`): 33 OK, 2 N/A e 0 falhas — critérios 1, 2, 3, 6 e 9; o 4 com Alt+Tab e tecla Windows; o 5 com coordenadas negativas, os dois monitores a 96 DPI; o 8 com 6,4% de um núcleo em 7,4 s de arraste e M5 p95 de 0,52 ms. Última regressão: 34 OK e 2 N/A em 2026-10-01.
- **Pendentes:** critério 4 com UAC (exige um pedido de elevação real) [MANUAL]; critério 5 com escalas diferentes [HW] (P6); critério 7 com ClickLock ligado [MANUAL] — está desligado na máquina e não é alterado, por ser configuração global.

### Fase 4 — Movimento e superfícies

STATUS: PLANNED. A implementação (DEC-022 e a toon force da DEC-023) está verificada por testes automáticos, integração e verificação de tela com input SINTÉTICO (2026-09-30); pende o critério 5.

- **Objetivo:** dar ao mascote liberdade para circular e agir como um macaquinho, de forma determinística, em um monitor.
- **Inclui:** superfícies aprovadas em Q-05; andar pelo chão e por bordas horizontais alcançáveis; subir e descer paredes laterais; ficar pendurado brevemente na borda superior; saltar, cair, pousar, descansar e pequenas ações autônomas; estados `WALKING`, `CLIMBING`, `HANGING`, `JUMPING`, `FALLING`, `LANDING` e `RESTING`; agenda ajustada pela energia; interrupção imediata por interação; poses provisórias por estado.
- **Incluído a pedido do usuário em 2026-09-30:** toon force (DEC-023: subir por qualquer lateral, inclusive a encostada em outro monitor; quique de borracha; foguete parede acima; achatar e esticar); cipó na borda de cima e agarrar onde é solto, preso até o usuário tirá-lo (DEC-024); esconderijo pelo clique duplo, na barra de tarefas ou nas laterais, só com a cabeça e as mãos para fora (DEC-025).
- **Exclui:** passagem entre monitores e troca de escala em movimento (Fase 5); animações completas (Fase 6).
- **Depende de:** Fase 3; Q-05.
- **Critérios de aceitação:**
  1. Com a mesma semente, a mesma sequência de movimentos se repete. [AUTO]
  2. O personagem nunca fica sem apoio fora de `JUMPING` e `FALLING`. [AUTO]
  3. Pressionar o personagem no meio de um pulo ou queda o segura na hora. [AUTO e MANUAL]
  4. Em `RESTING`, o relógio para e o consumo volta à linha de base. [AUTO e MANUAL, instrumentado]
  5. Gravando a tela a 120 quadros por segundo durante uma caminhada, a posição avança a cada quadro apresentado, sem quadro repetido nem salto maior que o passo esperado; repetir em 60 Hz e em outra taxa disponível. [MANUAL, instrumentado]
  6. Em cada nível de energia, a sequência é determinística para a mesma semente; Baixa produz menos e menores ações que Média, e Alta mais e mais longas, sem invalidar regra de apoio ou segurança. [AUTO]
  7. Escalar até a borda superior, ficar pendurado, percorrer a borda e voltar ou se soltar, sem ficar preso nem atravessar janela de aplicativo. [AUTO e MANUAL]
  8. Toon force (DEC-023): sobe por qualquer lateral da área útil, inclusive a que encosta em outro monitor, sem atravessar; uma queda alta quica e depois pousa; às vezes sobe a parede num foguete, com a frequência pela energia; as poses achatam no impacto e esticam na velocidade; nada disso quebra apoio, prioridade do usuário, determinismo ou a física igual em todos os níveis. [AUTO e MANUAL]
  9. Pedidos do usuário (DEC-024, DEC-025): solto no alto, agarra o cipó; solto junto a uma lateral, gruda na parede; posto lá pelo usuário, só sai quando o usuário o tira, sem gastar relógio parado; o clique duplo o esconde atrás da barra ou de uma lateral, e outro o tira de lá. [AUTO e MANUAL]
- **Testes automatizados:** trajetórias de referência com passo fixo, colisões contra superfícies de exemplo, propriedade de apoio. [AUTO]
- **Evidência (2026-09-30):**
  - [AUTO] `MovimentoTestes` (critérios 1, 2, 3 e 6 e a toon force; apoio em 60 simulações longas com gestos e mudanças de topologia; foguete em 8%, 30% e 47% pela energia), `AgarrarTestes`, `EsconderijoTestes`, `PoseTestes` e `MovimentoIntegracaoTestes`. Em 10 min simulados (critério 6): Baixa 6,4 ações e 18 s em movimento; Média 16,3 e 82 s; Alta 36,8 e 251 s, sempre com a mesma física.
  - Com input SINTÉTICO (`--fase 4`, `resultados/verificacao-fase4.log`): **28 OK, 0 falhas** — critério 3 (segurado no meio da queda); 4 (15 s em `RESTING` com 0,000% de CPU e relógio desligado); 7 (177/177 amostras na lateral, de volta ao chão em 16 s); 8 (dois quiques e subida pela lateral encostada no secundário, 29/29 amostras); 9 (cipó 194/194, parede 195/195, esconderijo 129/129 e de volta); foco mantido. O critério 5, só como medição de outro processo: maior passo 3 px, intervalo entre mudanças com mediana 15,8 ms e p95 31,2 ms.
  - Critério 4 instrumentado, 10 min pausado: CPU média 0,003%, p95 0,000%, memória estável (`resultados/desempenho-20260930-160012.txt`). Com autonomia, cerca de 3–4% de um núcleo em movimento; a memória sobe no aquecimento até o orçamento do GC e fica num patamar, sem vazamento (DEVELOPMENT_LOG.md).
- **Pendente [MANUAL]:** critério 5 com gravação de tela a 120 qps, em 60 Hz e em outra taxa disponível.

### Fase 5 — Multi-monitor completo e posição persistida

STATUS: PLANNED, em andamento. Blocos A (passos P1–P5, 2026-09-30) e B (passos P6–P9, 2026-10-01) e passos P10 a P13 (2026-10-02) implementados e verificados por testes automatizados e de integração: o app lembra a posição, a postura e a emoção entre execuções, usa a chave estável do monitor, acompanha a topologia aberto, ordena bloqueio, suspensão e retomada pela topologia, não se esconde quando o Windows o minimiza numa troca de monitores e atravessa de um monitor para outro. Faltam os passos P14–P16 e as pendências do fim desta seção.

- **Objetivo:** comportamento correto em qualquer topologia, com a posição restaurada entre execuções.
- **Inclui:** chave estável do monitor (protótipo P5); restauração com alternativas (ARCHITECTURE.md 2.8); circulação e travessia entre superfícies compatíveis de monitores; troca de escala em movimento com histerese (protótipo P6); conexão, desconexão, rearranjo, troca de principal, rotação e troca de escala; barra de tarefas movida ou oculta; recuperação se o Windows minimizar a janela ao desconectar um monitor; suspensão, retomada e bloqueio de sessão; **persistência mínima da posição escolhida pelo usuário** em `settings.json`, com versão de esquema e gravação atômica.
- **Exclui:** tela de configurações e demais preferências (Fase 8).
- **Depende de:** Fase 4; protótipos P5 e P6.
- **Critérios de aceitação, por cenário:**

  | ID | Cenário | Verificação |
  |---|---|---|
  | S1 | Dois monitores lado a lado, mesma escala | [AUTO] e [MANUAL][HW] |
  | S2 | Secundário à esquerda do principal, com x negativo | [AUTO] e [MANUAL][HW] |
  | S3 | Secundário acima do principal, com y negativo | [AUTO] e [MANUAL][HW] |
  | S4 | Monitores desalinhados, formando um degrau | [AUTO] e [MANUAL][HW] |
  | S5 | Escalas 100%, 150% e 200% | [AUTO] e [MANUAL][HW] |
  | S6 | Monitor em retrato | [AUTO] e [MANUAL][HW] |
  | S7 | Principal que não é o mais à esquerda | [AUTO] e [MANUAL][HW] |
  | S8 | Desconectar o monitor em que o Buzzy está | [MANUAL][HW] |
  | S9 | Reiniciar o app com o monitor salvo ausente, depois reconectar | [AUTO] e [MANUAL][HW] |
  | S10 | Trocar resolução, escala ou orientação com o app aberto | [MANUAL] |
  | S11 | Barra de tarefas em outra borda e oculta automaticamente | [MANUAL] |
  | S12 | Suspender, retomar e bloquear a sessão | [MANUAL] |

  Em todos os cenários, a âncora termina dentro de uma área útil, o personagem nunca some e o tamanho aparente é o mesmo em todos os monitores.
- **Critérios de travessia:** cruzar entre monitores vizinhos pelas passagens permitidas nos dois sentidos, inclusive com coordenadas negativas; não atravessar vãos sem superfície alcançável nem entrar em janelas de outros aplicativos. [AUTO e MANUAL][HW]
- **Testes automatizados:** topologias S1 a S7 e restauração S9 com dados de exemplo; gravação atômica simulando falha entre gravar e substituir. [AUTO]
- **Observação:** drivers de monitor virtual podem simular parte do hardware (STATUS: UNCERTAIN); isso não substitui [HW].
- **Passos de implementação.** Numerados de P1 a P16; no texto, "passo P7", e um P-número sem "passo" é um protótipo da Etapa 0B. Decisões: DEC-029 (persistência), DEC-030 (chave do monitor e topologia) e DEC-031 (sessão e energia); DEC-032 está reservada à travessia e à escala. Os invariantes 19 e 20 entraram no passo P8; o 21 e a referência gravada 06 ficam reservados para a travessia; os invariantes 22 a 29 e a referência 07 são da seção "Interação". Os pontos "(tamagotchi)" vieram da seção "Interação" e valem para os blocos seguintes. O desenho original da fase (2026-09-30, anterior à implementação), que o código cita como "desenho dos monitores" ou "crítica da Fase 5", está em [arquivo/desenho-fase5/](arquivo/desenho-fase5/); onde ele diverge das DEC, valem as DEC.
  - [x] P1 (2026-09-30). Tela do monitor na posição (`TelaDoMonitor`) e pixel dos pés (DEC-030).
  - [x] P2 (2026-09-30). Restauração na partida pela chave, pela tela do monitor ou no principal (DEC-030).
  - [x] P3 (2026-09-30). Esquema v1 e política de gravação no núcleo; `AtravessarMonitores` nas preferências, ainda sem efeito; invariante 18 (DEC-029).
  - [x] P4 (2026-09-30). Pasta de dados e arquivo de configurações com gravação atômica (DEC-029).
  - [x] P5 (2026-09-30). Isolamento dos testes por `--perfil-de-teste` (DEC-029).
  - [x] P6 (2026-10-01). Chave estável do monitor: resumo do caminho do dispositivo, com cache e reserva; leitura incoerente e, como último recurso, parcial (DEC-030). A estabilidade da chave continua a conferir no protótipo P5.
  - [x] P7 (2026-10-01). Persistência ligada: a raiz lê o arquivo na partida, entrega a posição ao núcleo e grava pela agenda; leitura protegida contra qualquer exceção; linhas `CONFIG`; esconderijo e "preso" no esquema v3; (tamagotchi) a emoção dominante no `Loaded` e a V16 (DEC-029).
  - [x] P8 (2026-10-01). Topologia em execução no núcleo: continua quando o monitor dele não muda de geometria; vai ao sobrevivente certo, medido nas coordenadas antigas, quando ele some; a tela gravada nunca é transladada; toda saída de `PRESSED` parte da posição que acompanhou a topologia; (tamagotchi) `USING` tratado como `REACTING` e os itens, inclusive o da mão, na mesma regra; invariantes 19 e 20 (DEC-030).
  - [x] P9 (2026-10-01). Releitura robusta no app: agrupamento com teto, novas tentativas com a leitura parcial no fim, barra recriada pela mesma releitura e conferência tardia do lugar das janelas, sem mexer na ordem Z (DEC-030).
  - [x] P10 (2026-10-02). Bloqueio e suspensão chegam ao núcleo na hora; desbloqueio e retomada esperam uma topologia coerente publicada; a retomada espera no mínimo 1,5 s, provisório até o protótipo P5 (DEC-031). [AUTO] 4 testes isolados do árbitro e integração por mensagens ao HWND do Buzzy de teste; `tools/testar.ps1 -Integracao` código 0, App 369/369.
  - [x] P11 (2026-10-02). A suspensão descarrega a gravação pendente, e a releitura imediata do mostrar vira uma função própria, que não arma a conferência tardia (DEC-031, adendo). [AUTO] `Suspensao_ComOPersonagemEscondido_DescarregaAGravacaoPendenteNaHora` e `Mostrar_ReleATopologiaNaHora_SemArmarAConferenciaTardia`, vistos falhando antes; `tools/testar.ps1 -Integracao` código 0, App 371/371.
  - [x] P12 (2026-10-02). A minimização do personagem com uma releitura pendente, ou com a topologia de agora diferente ou incoerente, é do sistema e não o esconde; sem nada disso, é do usuário e esconde (Q-03); já escondido, nunca vira `CMD_HIDE` (DEC-031, itens 7 e 8). As janelas dos itens já voltavam ao normal na hora, e a releitura reafirma o lugar delas. [AUTO] `MinimizacaoTestes` (4) e 2 testes de integração em `ComposicaoTestes`, vistos falhando antes.
  - [x] P13 (2026-10-02). Travessia entre monitores (DEC-032), com a toon force preservada: portas pela área útil; plana (atômica: a pausa e o item na mão esperam o fim); salto de degrau (até 480 DIP abaixo e 120 acima, arco validado na união das áreas úteis, com partida antes da lateral quando planejado); transbordo pela parede para o vizinho mais alto; sorteio na porta (`PesoAtravessar`) e ação `IrAoOutroMonitor`. (tamagotchi) O cambaleio não dispara a travessia pela lateral de trás, o atento espera a travessia acabar e os dois pesos seguem o percentual de andar da onda. A travessia em curso fica fora do invariante 19 e vai a `SETTLING` numa mudança de topologia; invariante 21 novo. [AUTO] `PassagensTestes` (8), `TravessiaTestes` (20), referência gravada `06-travessia.txt` e a propriedade com milhares de passos, com 18 mutações reprovadas; `tools/testar.ps1 -Integracao` às 16:54, código 0: Core 536/536, Portão 74/74 e App 380/380.
  - [ ] P14. Escala mista na janela do app (protótipo P6). (tamagotchi) A correção de DPI vale também para a janela do item; extrair o gancho comum às duas janelas. Herdado do bloco B, também para o P13: limitar as rodadas seguidas de reaplicação do lugar causadas só pelo próprio `WM_DPICHANGED`.
  - [ ] P15. Verificação de tela da Fase 5, com input SINTÉTICO e aviso ao usuário antes. (tamagotchi) A opção `--fase 5` convive com a `--fase tamagotchi` no mesmo programa. Inclui soltar, fechar e reabrir no mesmo lugar (S2 e S7), hoje coberto só pela integração.
    - Escrita em 2026-10-02, sem rodar: `Buzzy.Verificacao --injetar-input-na-tela --fase 5` (relatório `resultados/verificacao-fase5.log`, cerca de 2 minutos). T1: a travessia andando, com a semente escolhida por simulação do núcleo em que a primeira decisão é ir ao outro monitor; a janela amostrada fica na união das áreas úteis, passa montada sobre a borda e termina inteira no vizinho, com o foco no aplicativo em uso (N/A sem um vizinho de mesmo chão). S2 e S7: arrastar por input SINTÉTICO até o monitor em x negativo, gravar, fechar e reabrir sem limpar o perfil: o mesmo retângulo, restaurado pela chave. O salto de degrau, o transbordo e a escala mista não existem nesta máquina.
  - [ ] P16. Gate: documentação da fase e repouso de 10 min com perfil de teste.
- **Evidência [AUTO] (detalhe em [arquivo/todo-historico.md](arquivo/todo-historico.md)):**
  - bloco A: S1–S7 pela restauração com âncoras calculadas à mão e pela ida e volta do arquivo em cada monitor (`RestaurarTestes`, `EsquemaDeConfiguracoesTestes`); S5 e S6 a 144 DPI e girado; S9 com o monitor salvo ausente; S11 com a barra nas quatro bordas; S12 parcial no núcleo; gravação atômica com falha simulada em cada etapa e processo morto no meio (SECURITY.md 8, item 6); invariante 18; isolamento (`IsolamentoTestes`);
  - bloco B: P6 em `ChavesDeMonitorTestes` e `LeitorDeTopologiaTestes`; P7 em `AgendaDeGravacaoTestes`, `PosturaSalvaTestes` e o esquema v3 (amostra `settings-v3.json`); P8 em `RebasearTestes`, `MudancaDeTopologiaTestes` e nos invariantes 19 e 20 de `InvariantesTestes`; P9 em `AgendaDaReleituraTestes`. Mutações temporárias em cada passo e na correção, todas detectadas;
  - integração por mensagens postadas às janelas do próprio Buzzy, com o PID conferido: a mesma chave estável nos dois processos; `PersistenciaIntegracaoTestes` — solto a 25% do `\\.\DISPLAY2`, em x negativo, fechado e reaberto no mesmo lugar (S2 e S7), com a gravação cerca de 2 s depois de soltar, em 7,7 a 8,9 ms; postura e emoção voltam; o disparo no meio de um gesto espera o fim dele; arquivo ilegível dá os padrões e vira `settings.corrupt.json`; a barra recriada traz o ícone na hora; uma rajada de mensagens é relida no teto de 1 s; a conferência tardia devolve a janela e o item uma vez só;
  - cenários cobertos só por [AUTO]: S2 e S7 (troca de principal e persistência), S8 e S10 no núcleo, S9 também com o app aberto, S11 (barra recriada) na integração. Nenhum na tela nem com hardware;
  - regressão com input SINTÉTICO depois do bloco A, com o perfil `verificacao`: fases 1, 3 e 4 sem falhas. Não é evidência humana nem da Fase 5.
- **Pendentes da fase:**
  - [ ] verificação de tela com input SINTÉTICO depois do bloco B, avisando o usuário antes: `Buzzy.Verificacao --injetar-input-na-tela` com `--fase 1`, `--fase 3` e `--fase 4` (a `--fase tamagotchi`, com a V16, passou em 2026-10-02). As ferramentas mudaram no bloco B: o critério 5 da Fase 3 compara o nome GDI, o X1 acha a chave do secundário pela linha `TOPOLOGIA`, e cada execução confere a foto dos arquivos reais. Com DPI diferente entre os monitores, o critério 6 da Fase 1 pode sair INCONCLUSIVO pela conferência tardia [HW];
  - [ ] medições de 10 minutos com o perfil `desempenho`, avisando o usuário: `tools/medir-desempenho.ps1` (repouso) e `-Modo onda`. Os modos `repouso` e `autonomia` ainda não rodaram com o perfil de teste, e a gravação de 8 a 9 ms na thread da interface pode somar um quadro atrasado;
  - [ ] S1–S12 [MANUAL][HW] reais: S8 (desconectar o monitor do Buzzy, com e sem a opção do Windows de minimizar janelas, e reconectar; a minimização é do passo P12), S9 (partir com o monitor salvo ausente e reconectar), S10 (resolução, escala e orientação com o app aberto), S11 (a barra oculta sozinha e o Explorer reiniciado) e S12 (bloquear, suspender, hibernar, trocar de usuário, sair da conta ou reiniciar, conferindo a gravação), além do arraste e do item na mão numa troca de principal real (S2);
  - [ ] protótipo P5 [MANUAL][HW]: as mensagens e os tempos ao conectar, desconectar, rearranjar, trocar o principal, girar e mudar a escala, com "Minimizar janelas quando um monitor for desconectado" e "Lembrar locais das janelas" ligados e desligados, para calibrar os 300 ms, 1 s e 1,5 s; e a estabilidade da chave depois de reiniciar, trocar porta ou cabo (a chave deve mudar, e a restauração vai pelo retângulo ou para o principal), atualizar o driver, usar o modo clone e voltar a estender, consultar com a sessão bloqueada, por RDP e com um monitor DisplayPort que some com a tela apagada;
  - [ ] Process Monitor (SECURITY.md 8, item 4), agora com a persistência ligada [MANUAL];
  - [ ] confirmar com o usuário a gravação dos arquivos reais de configuração entre 12:12 e 12:22 de 2026-10-01 (SECURITY.md 10);
  - [ ] o caminho do erro não tratado da agenda de gravação, sem teste automático (DEC-029).

### Interação — emoção dominante e tamagotchi adulto

STATUS: PLANNED, em andamento, intercalada com a Fase 5. Núcleo, arte e app implementados, revisados e verificados por testes automatizados, de integração (mensagens postadas às janelas do próprio Buzzy) e pela verificação de tela com input SINTÉTICO: V1–V15 em 2026-10-01 e V16–V19 em 2026-10-02. A chave `Tamagotchi` está ligada no aplicativo. A seção continua sem VERIFIED pela revisão visual e de tom pelo usuário e pelas pendências [MANUAL] e [HW] abaixo.

- **Objetivo:** os pedidos do usuário de 2026-09-30 — a emoção dominante pelo menu, com os rostos de `expressoes.png` (DEC-027), e o tamagotchi adulto, com 13 itens invocados pelo menu, que caem ao lado dele e são usados quando o usuário os arrasta e solta sobre ele, com efeitos de desenho animado (DEC-028) — e os de 2026-10-01: o alívio e a paranoia por mistura (DEC-028, itens 28 a 35) e o baseado por conta própria (itens 36 a 42).
- **Inclui:** emoção dominante no núcleo, no esquema e no menu; tipos, tabelas, ondas, itens e o estado `USING`; a arte dos itens, caras, poses de uso, sobreposições e ícones; a apresentação; as janelas dos itens; a chave ligada; integração e verificação de tela; o alívio, a bala como droga sintética e a paranoia, com a arte dela; o baseado por conta própria, sem arte nova.
- **Exclui:** necessidades que decaem com o tempo (DEC-028, resposta 1); poses de uso na parede, no cipó e no esconderijo; gravar os itens e a onda.
- **Depende de:** bloco A da Fase 5. A persistência da emoção entrou no passo P7.
- **Desenho original** (2026-09-30, anterior à implementação): núcleo, app, arte e a crítica de integração, que deu a ordem T1–T9 e A1–A4 e que o código cita como "crítica, C11", "crítica, L13" e semelhantes, em [arquivo/desenho-tamagotchi/](arquivo/desenho-tamagotchi/); onde ele diverge das DEC, valem as DEC.
- **Passos** (regras em DEC-027, DEC-028 e ARCHITECTURE.md 2.6 e 2.16; cada passo termina com `tools/testar.ps1` verde e as referências 01–05 idênticas byte a byte):
  - [x] T1 (2026-10-01). Emoção dominante no núcleo e no esquema v2. [AUTO] `EmocaoDominanteTestes` (invariante 27 com a física do app) e testes do esquema e das reproduções.
  - [x] T2 (2026-10-01). Menu "Emoção dominante" com os rostos. [AUTO] `MenuNativoTestes`, `MontagemDoMenuTestes`, `BitmapsDoMenuTestes`; integração `MenuIntegracaoTestes` (20 aberturas com GDI e USER estáveis; a emoção escolhida por teclado troca a cara). V1 e V2 na tela.
  - [x] T3 (2026-10-01). Tipos e tabelas do tamagotchi, sem comportamento. [AUTO] `TabelaDoTamagotchiTestes`.
  - [x] T4 (2026-10-01). Onda da frente, atrás da chave. [AUTO] `OndaTestes` (durações, disparos, 6 h de repouso sem relógio, perfil e física efetivos, cambaleio, precedência sobre a dominante).
  - [x] T5 (2026-10-01). Itens, `USING`, onda de fundo e combinação, atrás da chave. [AUTO] `ItensTestes`, `UsoTestes`, `OndaTestes`, `FilaEAleatorioTestes` e os invariantes 22 a 29 em `InvariantesTestes`.
  - [x] T6 (2026-10-01). Referência gravada `07-tamagotchi.txt` (diretiva `# tamagotchi: sim`), revisada linha a linha.
  - [x] A1–A4 (2026-10-01). Arte: paleta e os 13 itens em 24 × 24 (A1); caras de efeito e passageiras, chapéu torto e rubor (A2); poses de uso no chão, com a ponta do item na boca ou no nariz (A3); sobreposições, modificadores, poses provisórias dos gestos e ícones do menu (A4). [AUTO] `CarimboTestes`, `ItensPixelTestes`, `RostosNovosTestes`, `UsosPixelTestes`, `EfeitosPixelTestes`, `IconesDoMenuTestes`, `GestosPixelTestes` e `RostoDoDesenhoTestes`; a ferramenta de prévias sai com código 1 se algo encosta na borda ou tem o contorno cortado.
  - [x] Revisões adversariais do núcleo e da arte, com mutações (2026-10-01); `ChaveLigadaTestes` confere os invariantes 22 e 27 com a física e a configuração do app, condição para o T9.
  - [x] T7 (2026-10-01). Apresentação, sem janelas de item: pose de `USING` por verbo, passo e apoio; caras de efeito; gestos da onda; sobreposição; cache de quadros de 16 MiB. [AUTO] `NucleoEArteTestes`, `PoseDeUsoTestes`, `CacheDeQuadrosTestes`, `PoseTestes` (cerca de 3 mil quadros distintos).
  - [x] T8 (2026-10-01). Janelas dos itens, submenu "Itens" e temporizador da onda. [AUTO] `SpriteDoItemTestes`, `GestosDosItensTestes`, `TemporizadorDaOndaTestes`, `GerenteDosItensTestes`, `JanelaDoItemTestes` (20 janelas criadas e fechadas sem vazar objetos) e os testes do menu (27 bitmaps criados = apagados por abertura).
  - [x] T9 (2026-10-01). Chave ligada em `DoAplicativo`; integração `ItensIntegracaoTestes`; tela `--fase tamagotchi` com 46 OK, 1 N/A (V16) e 0 falhas; repouso de 10 min com a onda de uma vodka (`-Modo onda`, `resultados/desempenho-20261001-085153.txt`, linha V13).
  - [x] T10 (2026-10-01). Alívio (DEC-028, item 30). [AUTO] `AlivioTestes` e as propriedades do alívio em `InvariantesTestes` (não sobe nível, não alonga fase, não toca a de fundo, não começa a onda do item, não sorteia); a referência 07 mudou só na linha da água. V18 na tela em 2026-10-02.
  - [x] T11 (2026-10-01). Bala como droga sintética e paranoia (DEC-028, itens 28, 29 e 31 a 35): arte da paranoia (`ParanoiaPixelTestes`; desenhos antigos idênticos por impressão digital); núcleo com a onda `Paranoico`, a carga do episódio e o sorteio único de 1 em 8 num gerador próprio (`ParanoiaTestes`, referência `08-paranoia.txt`); suor e gestos na apresentação; linha `PARANOIA` no log; integração 361/361. Chance real pelo núcleo: 491, 524 e 522 paranoias em 4.000 episódios de 2, 4 e 8 substâncias (faixa aceita de 417 a 583). V17 e V17b na tela em 2026-10-02.
  - [x] T13 (2026-10-02). Chave "Conteúdo adulto" (pedido do usuário; DEC-033): "Conteúdo &adulto" no menu, com marca, ligada por padrão e gravada (esquema v4); desligada, o submenu "Itens" só tem banana, água, café e energético, e o núcleo tira os itens adultos, as ondas de substância (com a paranoia), o uso adulto em curso e o baseado por conta própria. Invariante 30. [AUTO] `ConteudoAdultoTestes` (16), invariantes, esquema, menu e `ItensIntegracaoTestes.ConteudoAdulto_PeloMenu_…`, com mutações reprovadas; integração 374/374 às 13:50. V20 escrita, sem rodar.
  - [x] T12 (2026-10-02). Baseado por conta própria (DEC-028, itens 36 a 42): ação `FumarBaseado` fora de `Todas` e ligada no app, uso sem item pelo mesmo caminho do soltar, invariantes 11, 22 e 23 ajustados; o app sem código novo. [AUTO] `BaseadoPorContaPropriaTestes`, `InvariantesTestes`, referência `09-baseado-por-conta-propria.txt`; calibração de 100 h na Média: um baseado a cada 3,73 min elegíveis e 14,29 min no total; integração 364/364. V19 na tela em 2026-10-02.
  - [x] Build para a cópia que o usuário abre pela Área de Trabalho (`Desktop\net10.0-windows`): Release das 10:15 de 2026-10-02, com o SHA-256 conferido.
- **Verificação de tela** com input SINTÉTICO: na raiz, sem nenhum Buzzy aberto, `tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela --fase tamagotchi [--semente N]`. Abre o Buzzy várias vezes com o perfil `verificacao`, `--diagnostico` e uma semente (pausado, a de `--semente`, padrão 2028; com a agenda ligada, sementes escolhidas por simulação do núcleo). Tem um repouso de 60 s sem tocar em nada, leva de 7 a 13 minutos e grava `resultados\verificacao-tamagotchi.log`; antes, espera até 20 s sem uso do mouse e do teclado. Claude a roda depois de avisar o usuário; o resultado não é evidência humana.

  Última execução: 2026-10-02, das 10:22 às 10:29, **56 OK, 0 N/A, 0 SIMULADO e 0 falhas** (V1–V15 e X1 passaram também em 2026-10-01, 46 OK e 1 N/A, com a semente 2028 e os dois monitores a 96 DPI):

  | Caso | O que confere | Resultado |
  |---|---|---|
  | V1 | Menu de emoção pelo clique direito e pelo teclado: a cara muda e a opção fica marcada; "Automática" também funciona. | OK |
  | V2 | 20 aberturas do menu: objetos GDI e USER estáveis (±2) e bitmaps criados = apagados. | OK: GDI 32 → 32, USER 22 → 22; 27 ícones por abertura |
  | V3 | Invocar a banana: pousa ao lado em até 1,5 s e fica 3 s parada; o relógio desliga; o ponto opaco cai na janela do item e o transparente no receptor; o foco fica no receptor. | OK: pousou em 0,606 s |
  | V4 | Arrastar o item até ele: `USING`, os quadros de uso na ordem, a janela some, volta a `IDLE`; arraste do item com p95 abaixo de 16,7 ms. | OK: p95 de 0,685 ms |
  | V5 | Soltar longe: o item cai de onde foi solto. | OK |
  | V6 | Pressionar no meio do uso: `PRESSED` em até um quadro, e a onda continua. | OK |
  | V7 | Sétimo item: o mais antigo sai. | OK |
  | V8 | Com a agenda ligada, segurar um item: `WALKING` vira `IDLE`, nenhuma decisão durante o gesto, a próxima em 3 s ou mais. | OK, semente 34 |
  | V9 | Soltar um item nele preso no cipó, na parede e no esconderijo: volta preso ao mesmo lugar. | OK nos três |
  | V10 | Esconder e mostrar: as janelas dos itens acompanham. | OK, com 3 itens |
  | V11 | Cocaína contra baseado (ondas elétrica e chapada): velocidades de caminhada com tolerância de ±10%. | OK: 168,7 contra 166,5 px/s; 50,8 contra 49,5 |
  | V12 | Onda do bêbado no nível 3: recua na caminhada, sempre no chão. | OK, semente 49 |
  | V13 | Repouso pausado com a onda de uma vodka: CPU média até 0,1%, relógio desligado (60 s na verificação; 10 min em `tools\medir-desempenho.ps1 -Modo onda`, que entrega a vodka por mensagens postadas e conta as vezes que o relógio liga, esperado 0). | OK: 10 min com 0,003% de CPU, p95 0,000%, nenhum relógio ligado, 4 disparos únicos nos tempos da tabela, sem filhos nem conexões (`resultados/desempenho-20261001-085153.txt`) |
  | V14 | Sair com itens na tela: código 0 e nenhuma janela viva. | OK |
  | V15 | Botão direito num item abre o menu; "Recolher itens" tira todos. | OK |
  | V16 | Emoção gravada no perfil de teste e restaurada ao reabrir. | OK em 2026-10-02 |
  | V17 | Paranoia com a agenda ligada, na semente em que o sorteio sai na bala (a 20): a vodka e a bala; "a paranoia começa: Paranoico/Subida/1"; olhar pro teto no fim do uso e agachar no pico, com a cara paranoica e o suor; no pico, nada de `CLIMBING`, `JUMPING` ou `RESTING`; a água acalma um passo. | OK em 2026-10-02 |
  | V17b | Pausado, na semente em que o primeiro sorteio sai (a 6): vodka, baseado, cigarro e cogumelo não sorteiam; a bala no fim fecha a mistura e começa a paranoia. | OK em 2026-10-02 |
  | V18 | Pausado: a banana na onda da vodka acalma um passo (`Bebado/Subida/2 -> Bebado/Subida/1`), sem começar a onda dela nem reagendar o temporizador. | OK em 2026-10-02 |
  | V20 | Pausado, com a banana e a vodka no chão: "Conteúdo adulto" pelo menu (clique direito e A) fecha a janela da vodka (recolhida) e deixa a da banana; o menu seguinte mostra a chave desmarcada, e I e V não invocam nada; A religa. | Escrita, sem rodar |
  | V19 | Baseado por conta própria, com a agenda ligada e nenhum item, na semente em que a primeira decisão é o baseado (a 83, aos 11,8 s): `IDLE` → `USING` pelo `AUTONOMY_TIMER`, a subida do chapado de 10.000 ms, os quadros `fumando-*`, nenhuma linha `ITEM`; um `PRESS` depois do quadro `fumando-3` leva a `PRESSED`, e a onda não é cancelada. | OK em 2026-10-02 |
  | X1 | Arrastar um item até o outro monitor e soltar no ar: ele cai até o chão de lá. | OK, com os dois monitores a 96 DPI; entre escalas diferentes, continua [HW] |
  | Calma | Pausar com ele agarrado à parede sem estar preso: desce até o chão (regra da calma, DEC-022, item 4). | OK, semente 19 |
  | Foco | Depois de fechar cada menu, o foco volta ao aplicativo em uso. | OK nos 50 menus |

  Como ler o relatório: INVÁLIDA indica interferência humana (mouse mexido) e anula a rodada. FALHOU indica defeito, do Buzzy ou da ferramenta. O V4 sai INCONCLUSIVO, pedindo repetição, quando a interface atrasa e pula quadros; o V12, quando um quadro com dois passos esconde o recuo de menos de 1 pixel; a calma, se nenhum de 3 cliques nele escalando chegar; V17 e V19, se a agenda real sair da simulação ou o uso acabar antes do clique.
- **Pendências [MANUAL] e [HW]:**
  - a marca de rádio ao lado do rosto, e os ícones dos itens, nos temas claro, escuro e de alto contraste [MANUAL];
  - o menu e as janelas dos itens a 125, 150, 175 e 200% [MANUAL]; o usuário pode testar parte disso mudando a escala, o que Claude não faz, por ser configuração global. Os fatores 2 e 3 dos ícones só foram testados sem janela, e esta máquina está a 96 DPI;
  - a 300%, o cache de 16 MiB guarda só 28 quadros: andando com uma sobreposição, conferir os quadros descartados na linha `SPRITE` do log [MANUAL];
  - a altura das opções com o ícone de 40 × 32 a 96 DPI e o queixo cortado reto do recorte [MANUAL];
  - arrastar um item entre monitores de escalas diferentes [HW];
  - o Narrador lendo os submenus [MANUAL];
  - **revisão visual e de tom pelo usuário** [MANUAL]: as prévias de `assets/identidade/pixel/previa/`, os nomes e o app em uso. Pontos a olhar:
    - o espelhinho no nariz, que ainda pode parecer uma bandeja; o energético verde-neon; o baseado aceso no chão; o lenço dobrado; o MD e a bala na mão, com 5 × 5 e 9 × 4 pixels a 100%;
    - dois efeitos juntos nos quadros de uso que já têm efeito próprio, quando há uma onda;
    - a tremedeira, 7,5 vezes por segundo; o bêbado inclinado e parado na fase 0, sem relógio;
    - `escalando-1` com as caras de chapéu eriçado no uso na parede;
    - no esconderijo, a boca fica fora do quadro, e só os olhos mostram a cara do uso;
    - um possível quadro com o item por cima do personagem logo ao aparecer, nunca observado;
    - a paranoia: o suor, o tremidinho andando (5 vezes por segundo), o olhar pro teto com o pescoço esticado, o agachar, a cara paranoica e se a chance de 1 em 8 por mistura parece a certa;
    - o alívio aos poucos: cerveja e depois água dão 90 s de cara enjoada, e, com uma onda de substância na frente, o café e o energético não dão mais o ligado, nem a banana o satisfeito;
    - a bala, ainda desenhada como doce embrulhado em papel rosa listrado, com os corações do eufórico e podendo trazer paranoia;
    - o baseado por conta própria: a frequência e o tempo chapado (DEC-028, item 41) e a bala dada sozinha (item 39) foram confirmados pelo usuário em 2026-10-02; resta o tom e, se ele quiser decidir de novo, o chapado do fundo, que não impede outro baseado (item 37), e o item solto nele durante o baseado dele, que cai (item 40);
  - conforto para agarrar os itens pequenos [MANUAL]: a 100%, caixas opacas de banana 40 × 22, cerveja 36 × 36, vodka 20 × 42 e café 34 × 22 px; os finos, como o cigarro e o MD, ainda não foram medidos;
  - desconectar o monitor com itens na tela (cenário S8), depois do passo P12 [HW];
  - gravação a 120 qps das animações de uso [MANUAL], como o critério 5 da Fase 4;
  - Process Monitor confirmando que o Buzzy só grava em `%LOCALAPPDATA%\Buzzy`, agora com a persistência ligada [MANUAL].

### Fase 6 — Rendering, animações e expressões

- **Objetivo:** integrar a identidade visual e entregar animações e expressões sem afetar o comportamento.
- **Inclui:** arquivos-fonte da identidade; manifesto de assets; reprodutor de clipes; camadas ou variantes de expressão; clipes para caminhar, escalar, ficar pendurado, saltar, cair, espiar, se coçar, se espreguiçar e descansar; tendência de expressão por energia; redesenho só quando o quadro muda; máscara de clique por quadro; validação do manifesto no build.
- **Exclui:** funcionalidades fora destes critérios; não exige nova aprovação do conceito visual.
- **Depende de:** Fase 5.
- **Critérios de aceitação:**
  1. A suíte do núcleo passa igual com dois manifestos diferentes. [AUTO]
  2. Manifesto com estado sem clipe ou expressão ausente falha no build. [AUTO]
  3. Nenhum quadro tem pixel com alfa entre 1 e o limite definido pelo protótipo P1, fora de uma borda de dois pixels ao redor da silhueta; a verificação lê as imagens e falha o build. [AUTO]
  4. Trocar expressão não muda estado nem posição. [AUTO]
  5. CPU e GPU com animação ficam dentro das metas de Q-08. [MANUAL, instrumentado]

### Fase 7 — Personalidade e reações não verbais

- **Objetivo:** fazer o Buzzy parecer um mascote curioso e expressivo por reações, poses e pequenas ações determinísticas.
- **Inclui:** reações não verbais a clique; curiosidade por olhar, espiar, explorar bordas e gestos de macaquinho; a energia Baixa/Média/Alta nas escolhas de ação, pausas e expressões; prioridade para as ações diretas do usuário.
- **Pedido do usuário em 2026-09-30 (DEC-026), antecipado para logo depois da Fase 5, junto com o observador de janela ativa do P7:** com a mesma janela ativa por bastante tempo, ele chega perto e fica olhando com a cara curiosa; com a janela ativa há 30 s em outro monitor, ele muda de monitor para ver; usa só o monitor, o retângulo e o tempo em primeiro plano da janela ativa, nada de conteúdo; nunca age pausado, preso, escondido ou num monitor em tela cheia. Critérios: decisão determinística para a mesma sequência de eventos [AUTO]; o adaptador nunca entrega título, processo ou conteúdo [AUTO]; custo por eventos, sem polling, medido com o usuário digitando e mexendo o mouse [MANUAL, instrumentado].
- **Exclui:** chat, diálogo, campo de texto, respostas escritas, voz, IA, rede e memória. Painel de energia e configurações são da Fase 8.
- **Depende de:** Fase 6.
- **Critérios de aceitação:**
  1. Com a mesma semente e sequência de eventos, o núcleo produz as mesmas escolhas de ação e expressão. [AUTO]
  2. Reações a clique e ações de curiosidade são não verbais e não iniciam comportamento incompatível com arraste, pausa ou ocultação. [AUTO e MANUAL]
  3. Baixa, Média e Alta produzem frequências e pausas observavelmente distintas, com a mesma física e as mesmas regras de segurança. [AUTO e MANUAL]
  4. Expressões e gestos mudam sem alterar estado físico, posição ou superfície. [AUTO]
  5. O mascote não lê pixels, título ou conteúdo de outros aplicativos para decidir como agir. [AUTO e MANUAL]

### Fase 8 — Configurações e persistência local

- **Objetivo:** preferências completas, persistidas de forma previsível e recuperável.
- **Inclui:** interface de configurações; esquema completo; migração de versão; iniciar com o Windows, desligado por padrão e ativado pelo usuário (Q-04); energia Baixa/Média/Alta, Média padrão (DEC-014); painel compacto aberto pelo menu ("Energia…"; o clique duplo esconde o mascote, DEC-025) só com o seletor, ligado à mesma preferência; modo de tela cheia ligado por padrão e desligável (Q-09); recuperação de arquivo corrompido; cópia `.bak`.
- **Depende de:** Fases 5 e 7 e do P7 aprovado. Decisões Q-03, Q-04, Q-07, Q-09, Q-20 e Q-23 registradas.
- **Critérios de aceitação:**
  1. Gravar e ler devolvem as mesmas configurações. [AUTO]
  2. Arquivo de versão anterior é migrado. [AUTO]
  3. Arquivo corrompido gera os valores do `.bak` ou, sem ele, os padrões, guarda uma cópia e o app continua (DEC-029). [AUTO]
  4. Matar o processo durante a gravação nunca deixa o arquivo ilegível. [AUTO]
  5. Iniciar com o Windows liga, desliga e respeita a desativação feita pelo usuário nas Configurações do Windows. [MANUAL]
  6. O início automático vem desligado e só é ativado por ação explícita do usuário. [MANUAL]
  7. As configurações podem ser percorridas por teclado, com controles identificados por leitor de tela. [MANUAL]
  8. Sempre no topo pode ser ligado e desligado; a opacidade não aparece como opção do MVP. [MANUAL]
  9. A escala do Buzzy só pode ser escolhida entre os passos fixos oferecidos. [MANUAL]
  10. O painel aberto pelo menu e as configurações mostram o mesmo controle de energia em três posições compreensíveis: Baixa (mais tranquila), Média (equilibrada e padrão) e Alta (mais ativa). Mudar qualquer um atualiza a mesma preferência e afeta as próximas decisões autônomas, sem mudar física, segurança, prioridade do usuário ou modo de tela cheia. [AUTO e MANUAL]
  11. O painel compacto contém só o seletor de energia, fica dentro da área visível e funciona por teclado e leitor de tela. [MANUAL]
  12. Com uma janela em tela cheia no primeiro monitor, o Buzzy vai para o outro sem roubar foco; com todos ocupados, fica oculto. Ao sair da tela cheia, volta à posição anterior sem substituir a persistida. [MANUAL][HW]
  13. Ocultar, mostrar ou arrastar manualmente prevalece sobre a automação: um arraste durante a tela cheia não é interrompido nem desfeito ao sair dela, e mostrar pela bandeja enquanto está oculto por tela cheia o faz aparecer. Pausar a autonomia não desliga o modo. Desligar Q-09 impede a mudança automática. [AUTO e MANUAL]
  14. A avaliação de tela cheia não guarda nem registra nome de janela ou processo, título, texto ou pixels e não usa polling periódico em repouso. [AUTO e MANUAL]

### Fase 9 — Segurança e hardening

- **Objetivo:** confirmar na implementação cada regra de SECURITY.md.
- **Inclui:** revisão de SECURITY.md contra o código; portão de APIs e auditoria de dependências; sessão de 1 h sem conexão de rede; gravações só na pasta do Buzzy; ausência de elevação; entradas aleatórias no leitor de configurações e no seletor de energia; as condições do ZIP portátil pessoal e, se houver plano de distribuição pública, reabrir formato e assinatura (Q-10).
- **Critérios de aceitação:** todas as verificações da seção 8 de SECURITY.md passam e não há achado alto ou crítico aberto. [AUTO e MANUAL]

### Fase 10 — Verificação integrada

- **Objetivo:** confirmar o MVP inteiro em conjunto.
- **Inclui:** regressão automatizada completa; roteiro manual de aceitação no Windows 11 (alvo inicial 24H2 ou posterior, Q-02); matriz multi-monitor S1 a S12 [HW]; execução longa de 8 h com uso misto; extração e execução limpa do ZIP portátil em máquina virtual; correção dos problemas encontrados.
- **Critérios de aceitação:** todos os critérios das fases anteriores reexecutados e verdes; execução longa sem falha e sem crescimento contínuo de memória (Q-08).

### Fase 11 — Performance e acabamento

- **Objetivo:** atingir as metas de Q-08 e fazer o acabamento visual e de uso.
- **Inclui:** perfilagem, otimização e acabamento.
- **Critérios de aceitação:** metas de Q-08 atingidas e medidas; regressão da Fase 10 reexecutada e verde depois do acabamento (Q-11: a Fase 11 vem depois da Fase 10).

## MVP

O escopo está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md) e é coberto pelas Fases 1 a 11; funções pós-MVP só com decisão explícita do usuário (DEC-003, DEC-005).

- **Bugs:** nenhum bug confirmado aberto.
- **Bloqueios:** verificações [HW] completas exigem hardware adicional. O ambiente conhecido tem dois monitores 1920×1080 a 100%, com o secundário em x negativo; escala mista e retrato seguem pendentes.

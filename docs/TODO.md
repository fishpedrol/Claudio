# TODO.md — Fases e pendências do Buzzy

> Roadmap operativo único do projeto. Cada fase termina com build, testes e verificação do próprio escopo. A Fase 10 é a regressão integrada, não o primeiro momento de testar.
>
> **Formato.** Tarefa pendente: `- [ ]`. Tarefa concluída: `- [x]` com a data e, quando houver, a evidência. Cada fase lista objetivo, inclui, exclui, depende de, critérios de aceitação, testes automatizados, verificações manuais e condição de conclusão. Marcadores de verificação:
>
> - **[AUTO]** teste automatizado, executado no build.
> - **[MANUAL]** inspeção manual em Windows, com resultado registrado no DEVELOPMENT_LOG.md.
> - **[HW]** exige hardware específico: mais de um monitor, escalas diferentes, monitor em retrato ou possibilidade de conectar e desconectar. Nunca é declarado feito sem o hardware.
>
> Última atualização: 2026-09-30

## Fase atual

**Fase atual: Fase 4 — movimento e superfícies, em integração.** Situação das etapas anteriores:

- Etapa 0B: fechou tecnicamente em 2026-09-29, com P3 aprovado no ambiente medido por evidência sintética.
- Identidade visual original: criada.
- Fase 1: implementada e verificada por automação e input sintético. Continua PLANNED enquanto faltarem os critérios manuais 3 (bandeja real), 8 (escalas 150% e 200%) e 9 (mudanças reais de resolução, escala e barra).
- Fase 2: VERIFIED em 2026-09-30.
- Fase 3: implementada e verificada por automação e input sintético. Continua PLANNED por três pendências: UAC [MANUAL], escalas diferentes [HW] e ClickLock [MANUAL].

O usuário autorizou a execução das Fases 1–11 sem aprovação rotineira (DEC-015). Prossiga com trabalho independente.

Não conte os dez movimentos de mouse de 26/09 como evidência humana: foram exploração informal da namorada do usuário. Todo input de SendInput citado nesta página é sintético.

O conteúdo de `spikes/` continua descartável. Preserve alterações existentes e confira arquivos e resultados antes de atualizar status.

### Fechamento da Fase 0

- [x] Visão do produto, arquitetura, segurança e roadmap definidos nas fontes canônicas.
- [x] WPF/C#/.NET 10 selecionado e documentado em DEC-006.
- [x] Decisões do usuário registradas em DECISIONS.md; a autorização contínua está em DEC-015.
- [x] P1 e P2 executados e aceitos apenas nos limites descritos nesta página.
- [x] Validar P3 com receptor isolado, registrar resultado e limitações (2026-09-29): 3/3 rodadas, 28/28 cenários OK por rodada, incluindo B4b. Evidência SINTÉTICA; relatório em `spikes/resultados/p3-receptor.log`.
- [x] Fechar tecnicamente a Fase 0 e a Etapa 0B (2026-09-29), limitada ao ambiente medido; DPI misto continua sem hardware. A Fase 1 já estava autorizada por DEC-015.
## Etapa 0B — Protótipos de viabilidade

STATUS: VERIFIED em 2026-09-29 para os gates P1–P3 e o ambiente medido. P3 usou input SINTÉTICO, sem Bloco de Notas ou outro app como receptor. O código é descartável e fica em `spikes/`; as limitações de hardware permanecem descritas abaixo. P5–P8 e P10 continuam nas fases que dependem deles.

**Como rodar o que já existe:** `spikes/README.md`. Os scripts ficam em `spikes/ferramentas/`.

**Regra de veredito desta etapa.** O produto não injeta input. `SECURITY.md` 3.2 permite `SendInput` apenas em ferramentas de teste fora do executável. Registre separadamente input sintético e observações humanas; os dez movimentos anteriores, feitos pela namorada do usuário, foram exploração informal e não contam como teste formal. P3 deve usar um receptor controlado pelo spike e verificar eventos recebidos e foco sem inspecionar outro aplicativo. Não use o Bloco de Notas como dependência deste gate.

| ID | Resultado registrado | Próxima ação |
|---|---|---|
| P1 | **Usuário aceitou PASS em 2026-09-27, limitado ao ambiente medido.** Três evidências concordam: (1) sonda de teste de acerto `WindowFromPoint` — alfa 0 atravessou para o Bloco de Notas, alfa 1, 128 e 255 ficaram no Buzzy; (2) clique **sintético** por `SendInput` no centro das quatro faixas, 12 de 12 eventos aceitos, com o mesmo resultado e nenhum roubo de foco; (3) um clique **humano** na faixa alfa 1, sem roubo de foco. `WS_EX_LAYERED` confirmado. | Limitação: os cliques nas faixas alfa 0, 128 e 255 foram sintéticos, a pedido do usuário. Evidência vale para Windows 11 e DPI 96 medidos; escala mista continua sem hardware. |
| P3 | **PASS técnico no ambiente medido (2026-09-29, input sintético).** 3/3 rodadas completas; 28/28 cenários OK em cada uma, incluindo B4b, clique após Alt+Tab e entrega dos nove marcadores ao receptor com foco. M5: médias de 0,270/0,265/0,238 ms e p95 de 0,705/0,637/0,533 ms. Cursor restaurado sem deriva; ClickLock original restaurado; processos do teste encerrados. Windows 11 build 26200, .NET 10.0.12, dois monitores 1920×1080 a 96 DPI, secundário em x negativo. Relatórios: `spikes/resultados/p3-receptor.log` e `spikes/resultados/p3.log`. Limite: nenhuma escala mista; input sintético não é evidência humana. |
| P2 | **Medição concluída em 2026-09-26; usuário aceitou PASS como medição de viabilidade em 2026-09-27.** Repouso por 60 min: CPU média de 0,000 % de um núcleo (máx 0,14 %), 0,47 s de CPU na hora inteira, 0,15 troca de contexto por segundo em média (p95 0), memória privada estável em ~61,6 MB, GPU 0 %, 4 passagens de desenho no total; nenhuma atividade periódica evitável. Animação: 10 qps pedidos → 9,1 entregues, 0,48 % de um núcleo; 60 qps por `DispatcherTimer` → **39,1 entregues**, 1,8 %; 60 qps pelo compositor → **40,3 entregues**, 2,6 %, com o compositor disparando ~85 vezes por segundo. A resolução do timer global nunca mudou por causa do Buzzy. | As metas Q-08 foram aceitas, mas ainda não verificadas no aplicativo. A animação não atingiu 60 qps e a memória privada cresceu (~72 → 119 MB em 10 min); investigar antes da Fase 6. Isso não invalida a medição nem o resultado de repouso. |

| ID | Pergunta | Teste mínimo | Resultado esperado | Hardware |
|---|---|---|---|---|
| P1 | A janela WPF deixa passar o clique nos pixels transparentes? | Janela `AllowsTransparency`, do tamanho do sprite, sobre o Bloco de Notas. Clicar em pixel alfa 0, alfa 1 e pixel visível. | Alfa 0 entrega o clique ao Bloco de Notas; pixels visíveis e alfa 1 atingem a janela do Buzzy. Registrar Windows, DPI, renderização e limites encontrados. | 1 monitor |
| P2 | Quanto custa ficar parado e animar em WPF? | Sprite parado por 1 h sem timers. Depois, animação a 10 e 60 quadros por segundo por 10 min cada. Medir CPU, memória, GPU e acordadas por segundo com o mesmo protocolo em todos os estados. | Em repouso o app deixa de redesenhar e não gera atividade periódica evitável. Registrar números para Q-08; se falhar, ajustar ciclo visual e repetir. | 1 monitor |
| P3 | O arraste funciona sem roubar foco em WPF? | Janela que não ativa e harness isolado com receptor controlado. Simular clique, arraste para fora, soltura, ClickLock e Alt+Tab; confirmar o evento no receptor e comparar o foco antes/depois. Se a captura básica falhar, avaliar margem temporária alfa 1. | O gesto termina ao soltar, o receptor obtém os eventos esperados e o foco permanece na janela-alvo externa. Se não houver solução sem roubo de foco, reabrir DEC-006 antes da Fase 1. Marcar o input como sintético; não depender de Notepad ou de ação humana. | 1 monitor; repetir em 2 se disponível [HW] |
| P4 | **Obsoleto — não executar.** Verificava foco/IME de uma conversa e campo de texto, recursos removidos pelo usuário. | Não aplicável. | Não cria gate; a Fase 8 verifica apenas o painel de energia e suas opções acessíveis. | — |
| P5 | Quais mensagens chegam quando a topologia muda, e a chave do monitor é estável? | Janela que registra mensagens e a leitura da topologia ao conectar, desconectar, rearranjar, girar, trocar o principal e trocar a escala. Reiniciar e trocar portas, comparando as chaves. Repetir com as opções do Windows 11 de lembrar posições e minimizar janelas ao desconectar. | Lista de mensagens por cenário, intervalo de agrupamento calibrado, estabilidade da chave confirmada ou refutada. | 2 monitores [HW] |
| P6 | Como a janela se comporta ao cruzar monitores de escalas diferentes? | Janela movida por código de um monitor a 100% para um a 200%, parando sobre a borda. | Ponto em que a escala troca, ausência de oscilação com histerese e pés sempre sobre o chão. | 2 monitores com escalas diferentes [HW] |
| P7 | É possível detectar tela cheia e mover o Buzzy sem ler identidade ou conteúdo de outros apps? | Protótipo orientado a eventos: observar mudanças de janela em primeiro plano/geometria e consultar apenas o retângulo e o monitor da janela ativa. Testar vídeo em tela cheia, jogo sem borda e exclusivo; mudar para monitor livre, restaurar a posição, respeitar ocultação/arraste manual e ocultar quando nenhum monitor estiver livre. Não enumerar processos ou janelas, nem ler título, texto ou pixels. | Cobrir corretamente os casos em 2 monitores, restaurar posição e preferências, sem roubo de foco nem timer periódico em repouso. Mover ou ocultar o Buzzy não tira o jogo do modo exclusivo nem o minimiza. Medir CPU/acordadas (M1/M2) com o Buzzy parado **enquanto o usuário move o mouse e digita em outro app**, registrando quantas chamadas de evento chegam por segundo; o resultado precisa caber nas metas de repouso de Q-08. Arrastar ou mostrar manualmente durante a tela cheia prevalece sobre o retorno automático. Se eventos não bastarem, exigirem polling contínuo ou acordarem o Buzzy a cada movimento do mouse mesmo com a assinatura restrita à janela ativa, deixar o modo sem implementação até revisar o desenho. | 2 monitores [HW]; fallback de 1 monitor em teste automatizado |
| P8 | A janela layered aguenta ser fotografada e continua funcionando? | Outro processo chama a função que tira foto de janela durante a animação. | A atualização da janela continua funcionando, ou falha e é recuperada religando o estilo. | 1 monitor |
| P9 | Encerrado: comparar o esforço de interface nativa | Não executado. Perdeu a finalidade quando WPF foi escolhido como stack. | Sem resultado; não bloqueia o projeto. | — |
| P10 | Build, tamanho e portão de segurança | Build de release; medir executável e ZIP. Rodar o ZIP sem assinatura em máquina virtual limpa do Windows, com o controle de aplicativos ligado. Rodar o script que inspeciona as funções importadas pelo binário. | Tamanho registrado, comportamento de aviso/bloqueio sem assinatura conhecido e portão de APIs proibidas passando. Não publicar o build. | Máquina virtual |

**Ordem e papel dos protótipos.** P1, P3 e P2 foram executados na ordem prevista. P1 ou P3 que falhe reabre DEC-006 antes de produto; P2 informa as metas de Q-08 e pode exigir ajuste de repouso:

- **P1** verifica que a janela WPF cumpre o requisito central de clique por pixel entre processos.
- **P3** verifica input e foco no gesto de maior risco para a experiência.
- **P2** confirma o repouso real e define a base das metas Q-08.

P5 a P8 e P10 podem acontecer no início das fases que dependem deles. P4 foi aposentado; não renumerar os protótipos existentes.

## Roadmap do MVP

### Trabalho visual paralelo — identidade original

STATUS: VERIFIED como trabalho visual da Fase 1 em 2026-09-29; integração de clipes e expressões permanece na Fase 6.
- [x] Refazer a identidade como pixel art fiel às pranchas, com o chapéu de palha do Luffy, a pedido do usuário (2026-09-29, DEC-018 e DEC-019): critérios em `docs/IDENTIDADE_VISUAL.md`, gerador em `src/Buzzy.Visual/Pixel/`, folha em `assets/identidade/pixel/buzzy-poses.png` (21 poses, 14 expressões e ícone da bandeja 16 × 16; nenhuma pose encosta na borda). A direção vetorial anterior (`assets/identidade/arquivo-vetorial/`) foi substituída e arquivada. O app já mostra o quadro "parado" e o ícone da bandeja da pixel art; as animações entram na Fase 6.


Cada fase tem STATUS: PLANNED. Nenhuma fase é concluída só porque compila ou porque as tarefas foram marcadas. Em todas as fases, a condição de conclusão inclui:

- build limpo;
- testes automatizados verdes;
- portão de APIs proibidas e auditoria de dependências verdes;
- critérios de aceitação verificados e registrados;
- documentação sincronizada;
- gate PASS registrado conforme [PROMPT_MESTRE_BUZZY.md](PROMPT_MESTRE_BUZZY.md).

### Fase 1 — Shell do desktop

STATUS: PLANNED. Implementação verificada por build, testes automatizados e harnesses de tela com input sintético; ainda não concluída por critérios manuais pendentes (DEC-015).

- **Objetivo:** janela do personagem transparente, posicionada corretamente no desktop virtual, com ciclo de vida completo.
- **Inclui:** projeto e build da stack aprovada; manifesto Per-Monitor V2 e `asInvoker`; janela do tamanho do sprite com transparência por pixel; sprite provisório estático e original; janela que não ativa; sempre no topo, bandeja e ausência de botão na barra de tarefas conforme Q-03; instância única, em que abrir o app de novo mostra o Buzzy existente; módulo do mundo do desktop com consultas de monitor e área útil; releitura da topologia e acomodação da posição quando ela muda; saída pelo menu; portão de APIs proibidas no build; script de medição de desempenho; instruções de build e teste em PROJECT_CONTEXT.md.
- **Exclui:** arraste, movimento, animação, persistência, painel de energia.
- **Depende de:** Fase 0 concluída e gates técnicos P1, P2 e P3 satisfeitos. A autorização da Fase 1 já está registrada em DEC-015; não pedir novamente. O alvo Windows 11, os comportamentos Q-03 e o ZIP pessoal de Q-10 já foram decididos.
- **Critérios de aceitação:**
  1. O app inicia e mostra o sprite sobre a área útil do monitor principal. [MANUAL]
  2. O Buzzy fica sempre no topo por padrão e o ícone da bandeja aparece. [MANUAL]
  3. O menu da bandeja esconde, restaura e encerra o Buzzy; botão direito no personagem abre o mesmo menu. [MANUAL]
  4. Abrir o app uma segunda vez revela a janela existente sem criar outra instância. [MANUAL]
  5. Clicar em pixel transparente dentro do retângulo da janela entrega o clique ao aplicativo de baixo. [MANUAL]
  6. Clicar no sprite não tira o foco do aplicativo ativo. [MANUAL]
  7. O Buzzy não aparece na barra de tarefas nem no Alt+Tab, conforme Q-03. [MANUAL]
  8. O sprite fica nítido quando o Windows usa escala de 100%, 150% e 200%; a escala do próprio Buzzy é ajustada em passos fixos na Fase 8. [MANUAL]
  9. Trocar resolução, escala ou posição da barra de tarefas com o app aberto mantém o sprite dentro da área útil. [MANUAL]
  10. Sair pelo menu encerra o processo e todos os processos filhos. [MANUAL]
  11. Parado por 10 min, o consumo de CPU, memória e acordadas fica registrado como linha de base. [MANUAL, instrumentado]
- **Evidências de 2026-09-29:** `resultados/verificacao-fase1.log` registrou 25 OK, 4 SIMULADO e 0 falhas; todos os cliques/teclas injetados foram SINTÉTICOS. Critérios 1, 2, 4, 5, 6, 7 e 10 passaram no harness de tela; o menu do personagem passou. Os comportamentos do menu da bandeja foram SIMULADOS porque o ícone estava na área oculta e não foi clicado de verdade. Critério 6 usou a máquina com dois monitores, ambos a 100%.
- Critério 8 só foi observado a 100%; 150% e 200% permanecem PENDENTES. No critério 9, mensagens sintéticas de mudança passaram no teste de integração, mas mudanças reais de resolução, escala ou barra permanecem PENDENTES. A medição do critério 11 durou 600 s; CPU média de um núcleo 0,000%, memória privada 56,67 → 56,51 MB, zero rede e zero processos filhos observados. Detalhes em `resultados/desempenho-20260929-215550.txt`. Nenhum destes resultados é gesto humano.
- **Testes automatizados:** mundo do desktop com topologias de exemplo (lado a lado, empilhado, em L, coordenadas negativas, principal fora da esquerda, retrato, escalas mistas, vão entre monitores); teste de fumaça que inicia o app, encontra a janela, confere os estilos e encerra; portão de APIs proibidas. [AUTO]
- **Verificação com hardware:** critério 6 com dois monitores. [HW]

### Fase 2 — Núcleo do personagem

STATUS: VERIFIED em 2026-09-30 nos critérios da fase. O critério 4 foi verificado com input SINTÉTICO, não por gesto humano. As pendências manuais da Fase 1 (bandeja real, escalas de 150% e 200%, mudanças reais de resolução, escala e barra) continuam registradas na Fase 1.

- **Objetivo:** núcleo determinístico, testável sem janela, com máquina de estados, eventos, relógio lógico e expressão separada.
- **Inclui:** tipos de estado, evento, retrato e efeito; tabela de transições de ARCHITECTURE.md, seção 2.6; fila com prioridade; agenda autônoma com semente; nível de energia como parâmetro do núcleo (padrão Média; a escolha pelo usuário chega na Fase 8); autonomia pausada e gesto curto como dimensões (ARCHITECTURE.md 2.6); expressão como dimensão independente; relógio que para quando nada muda; gravação e reprodução de sequências de eventos para testes; ligação do núcleo à janela da Fase 1.
- **Exclui:** física de movimento (Fase 4), gestos reais de mouse (Fase 3), animação (Fase 6).
- **Depende de:** Fase 1.
- **Critérios de aceitação:**
  1. Toda transição da tabela tem teste. [AUTO]
  2. Invariantes 1, 6 e 7 de ARCHITECTURE.md, seção 2.6, valem para milhares de sequências aleatórias de eventos. [AUTO]
  3. Sem movimento nem animação, nenhum `TICK` fica agendado. [AUTO]
  4. O app da Fase 1 continua funcionando com o núcleo ligado. [MANUAL]
- **Testes automatizados:** testes de unidade, testes de propriedade com sequências aleatórias e reproduções gravadas comparadas com um resultado de referência. [AUTO]
- **Evidências de 2026-09-30:**
  - Critérios 1 a 3 [AUTO] (contagens atuais em DEVELOPMENT_LOG.md):
    - tabela de transições em `TransicoesTestes.cs`, `TransicoesComplementaresTestes.cs` e `TelaCheiaTestes.cs`;
    - propriedade com 2.000 sequências de 200 eventos, inclusive lotes processados de uma vez, em `InvariantesTestes.cs`;
    - reproduções gravadas 01–05;
    - relógio desligado sem movimento nem animação, conferido no núcleo e no app (`ComposicaoTestes`, log `RELOGIO`/`AGENDA`).
  - Uma auditoria adversarial independente (quatro auditores e verificação cética) confirmou lacunas da tabela no modo de tela cheia, na gravação da posição temporária, na carga e na energia inválida. Foram corrigidas (DEC-020), com cobertura nova.
  - Critério 4 [MANUAL], com input SINTÉTICO:
    - `Buzzy.Verificacao --injetar-input-na-tela` com o núcleo ligado deu 25 OK, 4 SIMULADO (os mesmos da bandeja na Fase 1) e 0 falhas;
    - a integração na tela ficou verde;
    - houve uma observação INFORMAL do usuário: menu do personagem e restauração pelo clique real no ícone da bandeja funcionando com o núcleo.
  - Repouso de 10 min com o núcleo (`resultados/desempenho-20260930-112125.txt`): CPU média de 0,010% de um núcleo, p95 0,000%, memória privada 57,99 → 58,00 MB, nenhuma rede, nenhum processo filho, resolução do timer inalterada.
- **Subtarefas:**
  - [x] núcleo puro, fila, agenda e gravação/reprodução (2026-09-29);
  - [x] núcleo ligado a `Aplicacao.cs`, com eventos, efeitos e logs (2026-09-30);
  - [x] integração da tela e critério 4 com input sintético (2026-09-30);
  - [x] documentação da fase sincronizada (2026-09-30).

### Fase 3 — Input e arraste

STATUS: PLANNED. A implementação está verificada por testes automáticos, integração por mensagens postadas e verificação de tela com input SINTÉTICO (2026-09-30). Pendem o critério 4 com janela de UAC [MANUAL], o critério 5 com escalas diferentes [HW] e o critério 7 com ClickLock ligado [MANUAL].

- **Objetivo:** clique e arraste com prioridade absoluta do usuário.
- **Inclui:** arbitragem de gestos com o limiar de arraste do sistema lido por DPI; clique, clique duplo, botão direito e perda de captura; arraste manual, sem o loop modal de mover do Windows, a menos que P3 indique outra solução; validação ao soltar; menu de contexto mínimo; posicionamento no chão ao soltar sem apoio (a queda animada entra na Fase 4).
- **Exclui:** caminhada, escalada, pulo, queda com física; painel de energia (Fase 8). Até lá, o clique duplo é reconhecido, mas só produz reação.
- **Depende de:** Fase 2; P3.
- **Critérios de aceitação:**
  1. Durante o arraste, o personagem acompanha o cursor sem atraso visível e não anda, não pula, não foge e não escala. [MANUAL e AUTO]
  2. Soltar em qualquer ponto, inclusive num vão entre monitores, deixa a âncora dentro de uma área útil. [AUTO e MANUAL]
  3. Soltar sem mover além do limiar gera clique. Passar do limiar gera arraste. [AUTO e MANUAL]
  4. Alt+Tab, tecla Windows ou janela de UAC durante o arraste encerram o arraste sem travar o personagem. [MANUAL]
  5. Arrastar para outro monitor funciona com coordenadas negativas e escalas diferentes. [MANUAL][HW]
  6. Clicar e arrastar não tiram o foco do aplicativo ativo. [MANUAL]
  7. Com ClickLock ligado, o arraste funciona sem tempo limite. [MANUAL]
  8. O consumo de CPU durante o arraste fica medido e registrado. [MANUAL, instrumentado]
  9. Clique duplo é reconhecido dentro do intervalo do Windows e botão direito solicita o menu de contexto. [AUTO e MANUAL]
- **Testes automatizados:** reconhecedor de gestos (limiar por DPI, clique duplo, perda de captura, botão direito); invariantes 1 e 2 sob sequências aleatórias; validação ao soltar sobre as topologias de exemplo. [AUTO]
- **Evidências de 2026-09-30:**
  - [AUTO]:
    - árbitro puro (`ArbitroDeGestosTestes`: limiar por DPI, clique duplo pelas regras do Windows, captura perdida, botão direito, soltar rápido fora do limiar, ClickLock sem limite de tempo);
    - propriedade com 2.500 sequências e 361 mil passos, que conferiu gramática, captura, coerência com o núcleo e os invariantes 1, 2 e 5 (`ArbitroPropriedadesTestes`);
    - 8.381 soltares sobre as 17 topologias de exemplo, 5.140 em vãos ou fora dos monitores (`SoltarNasTopologiasTestes`);
    - integração por mensagens postadas (`GestosTestes`: arraste, clique, clique duplo, menu pelo botão direito, captura perdida).
  - [MANUAL] com input SINTÉTICO (`Buzzy.Verificacao --fase 3`, `resultados/verificacao-fase3.log`): 33 OK, 2 N/A e 0 falhas.
    - Passaram os critérios 1, 2, 3 e 6.
    - Critério 4: Alt+Tab e tecla Windows.
    - Critério 5: coordenadas negativas, os dois monitores a 96 DPI.
    - Critério 8: CPU de 6,4% de um núcleo em 7,4 s de arraste contínuo; M5 medido pelo app com p95 de 0,52 ms, dentro de Q-08.
    - Critério 9: clique duplo e menu.
  - Pendentes:
    - janela de UAC no meio do arraste (exige um pedido de elevação real);
    - escalas diferentes (hardware; P6);
    - ClickLock ligado: está desligado na máquina e não foi alterado, porque é configuração global. O árbitro não tem regra de tempo.

### Fase 4 — Movimento e superfícies

STATUS: PLANNED. A implementação (DEC-022 e a toon force pedida pelo usuário em DEC-023) está verificada por testes automáticos, integração e verificação de tela com input SINTÉTICO (2026-09-30). Pende o critério 5 com gravação de tela a 120 qps e em outra taxa de atualização [MANUAL].

- **Objetivo:** dar ao mascote liberdade para circular e agir como um macaquinho, de forma determinística, em um monitor.
- **Inclui:** superfícies aprovadas em Q-05; andar pelo chão e por bordas horizontais alcançáveis; subir e descer paredes laterais; ficar pendurado brevemente na borda superior; saltar, cair, pousar, descansar e executar pequenas ações autônomas; estados `WALKING`, `CLIMBING`, `HANGING`, `JUMPING`, `FALLING`, `LANDING` e `RESTING`; agenda ajustada pelo nível de energia; interrupção imediata por interação; poses provisórias por estado.
- **Incluído a pedido do usuário em 2026-09-30:**
  - toon force (DEC-023): subir por qualquer lateral da área útil, inclusive a encostada em outro monitor; quique de borracha; foguete de borracha parede acima; achatar e esticar nas poses provisórias;
  - cipó na borda de cima; agarrar o cipó ou a parede onde é solto e ficar preso até o usuário tirá-lo (DEC-024);
  - esconderijo pelo clique duplo, na barra de tarefas ou nas laterais, só com a cabeça e as mãos para fora (DEC-025).
- **Exclui:** passagem entre monitores e troca de escala em movimento (Fase 5); animações completas (Fase 6).
- **Depende de:** Fase 3; Q-05.
- **Critérios de aceitação:**
  1. Com a mesma semente, a mesma sequência de movimentos se repete. [AUTO]
  2. O personagem nunca fica sem apoio fora dos estados `JUMPING` e `FALLING`. [AUTO]
  3. Pressionar o personagem no meio de um pulo ou queda o segura na hora. [AUTO e MANUAL]
  4. Em `RESTING`, o relógio para e o consumo volta à linha de base. [AUTO e MANUAL, instrumentado]
  5. Gravando a tela a 120 quadros por segundo durante uma caminhada, a posição do personagem avança a cada quadro apresentado, sem quadro repetido nem salto maior que o passo esperado. Repetir em 60 Hz e em outra taxa de atualização disponível. [MANUAL, instrumentado]
  6. Em cada nível de energia, a sequência é determinística para a mesma semente; Baixa produz menos/menores ações que Média, e Alta produz mais/mais longas, sem invalidar nenhuma regra de apoio ou segurança. [AUTO]
  7. Escalar até a borda superior, ficar pendurado, percorrer a borda alcançável e voltar ou se soltar sem ficar preso nem atravessar uma janela de aplicativo. [AUTO e MANUAL]
  8. Toon force (DEC-023):
     - sobe por qualquer lateral da área útil, inclusive a que encosta em outro monitor, sem atravessar para ele;
     - uma queda alta quica e depois pousa;
     - às vezes sobe a parede num foguete, com a frequência seguindo a energia;
     - as poses achatam no impacto e esticam na velocidade;
     - nada disso quebra o apoio, a prioridade do usuário, o determinismo ou a mesma física em todos os níveis. [AUTO e MANUAL]
  9. Pedidos do usuário (DEC-024 e DEC-025):
     - solto no alto, agarra o cipó da borda de cima; solto junto a uma lateral, gruda na parede;
     - posto lá pelo usuário, só sai quando o usuário o tira, sem gastar relógio parado;
     - o clique duplo o esconde atrás da barra de tarefas ou de uma lateral, só com a cabeça e as mãos para fora, e outro o tira de lá. [AUTO e MANUAL]
- **Testes automatizados:** trajetórias de referência com passo fixo, colisões contra superfícies de exemplo, testes de propriedade de apoio. [AUTO]
- **Evidências de 2026-09-30:**
  - [AUTO] `MovimentoTestes`, 14 testes com relógio virtual:
    - caminhada em passos fixos de 1 ou 2 px;
    - passagem não atravessada, mas escalável (toon force);
    - escalada, pendurar e volta em 40 sementes;
    - pulo balístico que pousa;
    - pressionar no meio do pulo ou da queda segura na hora (critério 3);
    - mesma semente, mesma trajetória, em cada nível (critérios 1 e 6);
    - pausa termina parado no chão;
    - propriedade de apoio em 60 simulações longas, com o usuário pressionando e arrastando e com mudanças de topologia (critério 2);
    - quique: duas vezes, cada uma mais baixa; nenhum em queda baixa ou pausado; pressionar no meio segura;
    - foguete até pendurar;
    - chance do foguete pela energia: 8%, 30% e 47%.
  - [AUTO] Energia por 10 min simulados (critério 6), sempre com a mesma física:

    | Nível | Ações | Tempo em movimento |
    |---|---|---|
    | Baixa | 6,4 | 18 s |
    | Média | 16,3 | 82 s |
    | Alta | 36,8 | 251 s |

  - [AUTO] App:
    - `PoseTestes`: pose por estado e dinâmica; achatar e esticar mantêm os pés, o alfa só 0 ou 255 e os cantos transparentes;
    - `MovimentoIntegracaoTestes`: a janela anda dentro da área útil, e pressionar no meio do movimento segura.
    - Contagens atuais em DEVELOPMENT_LOG.md.
  - [MANUAL] com input SINTÉTICO (`Buzzy.Verificacao --fase 4`, `resultados/verificacao-fase4.log`): **28 OK, 0 falhas.**
    - Critério 3: segurado no meio da queda, parado 600 ms, depois caiu e pousou.
    - Critério 5, só como medição instrumentada: posição da janela amostrada de outro processo na caminhada; maior passo 3 px, intervalo entre mudanças com mediana de 15,8 ms, p95 de 31,2 ms e máximo de 41,8 ms.
    - Critério 7: 177/177 amostras encostadas na lateral, 131 posições diferentes no teto, de volta ao chão em 16 s.
    - Critério 4: 15 s em RESTING com 0,000% de CPU e relógio desligado.
    - Critério 8: dois quiques e pouso; subida pela lateral esquerda do principal (encostada no secundário) com 29/29 amostras na lateral, sem sair do principal, terminando pendurado.
    - Critério 9, com a agenda ligada e 12 s ou 8 s de observação em cada caso:
      - arrastado para o alto, agarrou o cipó e ficou (194/194 amostras na borda de cima, nenhuma saída);
      - arrastado para a lateral direita, grudou e ficou (195/195);
      - arrastado para o chão, ficou livre;
      - clique duplo no chão: escondeu atrás da barra e ficou (129/129); outro clique duplo o tirou;
      - na parede: escondeu atrás da lateral (129/129) e voltou à parede.
    - Foco mantido no aplicativo em uso em todos os cenários.
  - [AUTO] `AgarrarTestes` (7) e `EsconderijoTestes` (5): agarrar, ficar preso, passeios na mesma superfície, clique e pausa sem tirá-lo; esconder, trocar de cara, reagir escondido, arrastar para tirar e voltar ao esconderijo depois de esconder e mostrar pela bandeja.
  - Critério 4 [MANUAL, instrumentado], 10 min pausado: CPU média de 0,003% de um núcleo, p95 0,000%, memória estável (`resultados/desempenho-20260930-160012.txt`).
  - Com autonomia: cerca de 3–4% de um núcleo em movimento. A memória sobe no aquecimento até o orçamento do GC e fica num patamar, sem vazamento. Diagnóstico em DEVELOPMENT_LOG.md.
  - Pendente [MANUAL]: critério 5 com gravação de tela a 120 qps, em 60 Hz e em outra taxa disponível.

### Fase 5 — Multi-monitor completo e posição persistida

- **Objetivo:** comportamento correto em qualquer topologia, com a posição restaurada entre execuções.
- **Inclui:** chave estável do monitor conforme P5; restauração com alternativas (ARCHITECTURE.md, seção 2.8); circulação e travessia entre superfícies compatíveis de monitores; troca de escala em movimento com histerese conforme P6; conexão, desconexão, rearranjo, troca de principal, rotação e troca de escala; barra de tarefas movida ou oculta automaticamente; recuperação se o Windows minimizar a janela ao desconectar um monitor; suspensão, retomada e bloqueio de sessão; **persistência mínima da posição escolhida pelo usuário** em `settings.json` com versão de esquema e gravação atômica.
- **Exclui:** tela de configurações e demais preferências (Fase 8).
- **Depende de:** Fase 4; P5 e P6.
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
- **Observação:** drivers de monitor virtual podem simular parte do hardware. STATUS: UNCERTAIN; isso não substitui [HW].

### Fase 6 — Rendering, animações e expressões

- **Objetivo:** integrar a identidade visual original criada em paralelo e entregar animações e expressões sem afetar o comportamento.
- **Inclui:** integrar os arquivos-fonte da identidade visual original; manifesto de assets; reprodutor de clipes; camadas ou variantes de expressão; clipes coerentes para caminhar, escalar, ficar pendurado, saltar, cair, espiar, se coçar, se espreguiçar e descansar; tendência de expressão por nível de energia; redesenho só quando o quadro muda; máscara de clique por quadro; validação do manifesto no build.
- **Exclui:** funcionalidades fora dos critérios desta fase; não exige nova aprovação do conceito visual.
- **Depende de:** Fase 5.
- **Critérios de aceitação:**
  1. A suíte do núcleo passa igual com dois manifestos diferentes. [AUTO]
  2. Manifesto com estado sem clipe ou expressão ausente falha no build. [AUTO]
  3. Nenhum quadro do asset tem pixel com alfa entre 1 e o limite definido pelo protótipo P1, fora de uma borda de dois pixels ao redor da silhueta. A verificação lê os arquivos de imagem e falha o build. [AUTO]
  4. Trocar expressão não muda estado nem posição. [AUTO]
  5. CPU e GPU com animação ficam dentro das metas de Q-08. [MANUAL, instrumentado]

### Fase 7 — Personalidade e reações não verbais

- **Objetivo:** fazer o Buzzy parecer um mascote curioso e expressivo por reações, poses e pequenas ações determinísticas.
- **Inclui:** reações não verbais a clique; curiosidade expressa por olhar, espiar, explorar bordas e gestos de macaquinho; integrar a personalidade Baixa/Média/Alta às escolhas de ação, pausas e expressões; garantir prioridade para ações diretas do usuário.
- **Pedido do usuário em 2026-09-30 (DEC-026), antecipado para logo depois da Fase 5 junto com o observador de janela ativa do P7:**
  - com a mesma janela ativa por bastante tempo, ele chega perto dela e fica olhando com a cara curiosa;
  - com a janela ativa há 30 s em outro monitor, ele muda de monitor para ver;
  - usa só o monitor, o retângulo e o tempo em primeiro plano da janela ativa, nada de conteúdo;
  - nunca age pausado, preso, escondido ou num monitor em tela cheia.
  - Critérios: a decisão é determinística para a mesma sequência de eventos [AUTO]; o adaptador nunca entrega título, processo ou conteúdo [AUTO]; custo por eventos, sem polling, medido com o usuário digitando e mexendo o mouse [MANUAL, instrumentado].
- **Exclui:** chat, diálogo, campo de texto, respostas escritas, voz, IA, rede e memória. O painel de energia e as configurações pertencem à Fase 8.
- **Depende de:** Fase 6. P4 não se aplica.
- **Critérios de aceitação:**
  1. Com a mesma semente e sequência de eventos, o núcleo produz as mesmas escolhas de ação e expressão. [AUTO]
  2. Reações a clique e ações de curiosidade são não verbais e não iniciam comportamento incompatível com arraste, pausa ou ocultação. [AUTO e MANUAL]
  3. Baixa, Média e Alta produzem frequências e pausas observavelmente distintas, mantendo a mesma física e as mesmas regras de segurança. [AUTO e MANUAL]
  4. Expressões e gestos podem mudar sem alterar estado físico, posição ou superfície. [AUTO]
  5. O mascote não lê pixels, título ou conteúdo de outros aplicativos para decidir como agir. [AUTO e MANUAL]

### Fase 8 — Configurações e persistência local

- **Objetivo:** preferências completas, persistidas de forma previsível e recuperável.
- **Inclui:** interface de configurações; esquema completo; migração de versão; opção de iniciar com o Windows desligada por padrão e ativada pelo usuário (Q-04); energia Baixa/Média/Alta, com Média como padrão (DEC-014); painel compacto aberto pelo menu ("Energia…"; o clique duplo passou a esconder o mascote, DEC-025) contendo somente o seletor, ligado à mesma preferência; modo de tela cheia ligado por padrão e desligável (Q-09); recuperação de arquivo corrompido; cópia `.bak`.
- **Depende de:** Fases 5 e 7 e do P7 aprovado. As decisões Q-03, Q-04, Q-07, Q-09, Q-20 e Q-23 estão registradas.
- **Critérios de aceitação:**
  1. Gravar e ler devolvem as mesmas configurações. [AUTO]
  2. Arquivo de versão anterior é migrado. [AUTO]
  3. Arquivo corrompido gera valores padrão, guarda uma cópia e o app continua. [AUTO]
  4. Matar o processo durante a gravação nunca deixa o arquivo ilegível. [AUTO]
  5. Iniciar com o Windows liga, desliga e respeita a desativação feita pelo usuário nas Configurações do Windows. [MANUAL]
  6. O início automático vem desligado e só é ativado após uma ação explícita do usuário. [MANUAL]
  7. As configurações podem ser percorridas por teclado e seus controles são identificados por um leitor de tela. [MANUAL]
  8. Sempre no topo pode ser ligado e desligado; a opacidade não aparece como opção do MVP. [MANUAL]
  9. A escala do Buzzy pode ser escolhida somente entre os passos fixos oferecidos. [MANUAL]
  10. O painel aberto pelo menu e a janela de configurações mostram o mesmo controle de energia em três posições compreensíveis: Baixa (mais tranquila), Média (equilibrada e padrão) e Alta (mais ativa). Alterar qualquer um deles atualiza a mesma preferência persistida e afeta as próximas decisões autônomas, sem mudar física, segurança, prioridade do usuário ou modo de tela cheia. [AUTO e MANUAL]
  11. O painel compacto contém somente o seletor de energia, permanece dentro da área visível e permite usar o controle por teclado e leitor de tela. [MANUAL]
  12. Com uma janela em tela cheia no primeiro monitor, o Buzzy move-se para o outro sem roubar foco; se todos estiverem ocupados, fica oculto. Ao sair da tela cheia, volta à posição anterior sem substituir a posição persistida pelo usuário. [MANUAL][HW]
  13. Ocultar, mostrar ou arrastar o Buzzy manualmente prevalece sobre a automação: um arraste durante a tela cheia não é interrompido nem desfeito ao sair dela, e mostrar pela bandeja enquanto está oculto por tela cheia o faz aparecer. Pausar a autonomia não desliga o modo. Desligar Q-09 impede a mudança automática. [AUTO e MANUAL]
  14. A avaliação de tela cheia não guarda nem registra nome de janela/processo, título, texto ou pixels e não usa polling periódico em repouso. [AUTO e MANUAL]

### Fase 9 — Segurança e hardening

- **Objetivo:** confirmar na implementação cada regra de SECURITY.md.
- **Inclui:** revisão de SECURITY.md contra o código; portão de APIs e auditoria de dependências; sessão de 1 h sem nenhuma conexão de rede; gravações só na pasta do Buzzy; ausência de elevação; teste com entradas aleatórias no leitor de configurações e no seletor de energia; confirmar as condições do ZIP portátil pessoal e, se houver plano de distribuição pública, reabrir formato e assinatura (Q-10).
- **Critérios de aceitação:** todas as verificações da seção 8 de SECURITY.md passam e não há achado alto ou crítico aberto. [AUTO e MANUAL]

### Fase 10 — Verificação integrada

- **Objetivo:** confirmar o MVP inteiro em conjunto.
- **Inclui:** regressão automatizada completa; roteiro manual de aceitação no Windows 11 (alvo inicial Windows 11 24H2 ou posterior, Q-02); matriz multi-monitor S1 a S12 [HW]; execução longa de 8 h com uso misto; extração e execução limpa do ZIP portátil em máquina virtual; correção dos problemas encontrados.
- **Critérios de aceitação:** todos os critérios das fases anteriores reexecutados e verdes; execução longa sem falha e sem crescimento contínuo de memória, conforme Q-08.

### Fase 11 — Performance e acabamento

- **Objetivo:** atingir as metas de Q-08 e fazer o acabamento visual e de uso.
- **Inclui:** perfilagem, otimização e acabamento.
- **Critérios de aceitação:** metas de Q-08 atingidas e medidas; regressão da Fase 10 reexecutada e verde depois do acabamento.
- **Observação:** Q-11 foi respondida pelo usuário: a Fase 11 vem depois da verificação da Fase 10 e termina com a regressão reexecutada.

## MVP

O escopo do MVP está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md) e é coberto pelas Fases 1 a 11.

### Bugs

Nenhum bug confirmado nos arquivos de produto; a implementação inicial da Fase 1 está em andamento e precisa das verificações previstas.

### Bloqueios

- A Fase 1 depende apenas do fechamento técnico da Fase 0 e dos gates aplicáveis. A autorização do usuário já está registrada em DEC-015. P1/P2 e Q-08 foram aceitos dentro dos limites registrados; isso não valida o aplicativo. P4 foi aposentado porque o produto não terá chat nem entrada de texto.
- Verificações [HW] completas exigem hardware adicional. O ambiente conhecido tem dois monitores 1920×1080 a 100%, com o secundário em x negativo; escala mista e retrato seguem pendentes. Os movimentos anteriormente relatados em dois monitores foram exploração informal da namorada do usuário e não contam como evidência formal.

## Escopo fora do MVP

Consulte [PRODUCT_SPEC.md](PRODUCT_SPEC.md) e DEC-003/DEC-005 para limites do produto. Não adicionar funções pós-MVP sem decisão explícita do usuário.

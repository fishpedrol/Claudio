# TODO.md — Fases e pendências do Buzzy

> Roadmap operativo único do projeto. Cada fase termina com build, testes e verificação do próprio escopo. A Fase 10 é a regressão integrada, não o primeiro momento de testar.
>
> **Formato.** Tarefa pendente: `- [ ]`. Tarefa concluída: `- [x]` com a data e, quando houver, a evidência. Cada fase lista objetivo, inclui, exclui, depende de, critérios de aceitação, testes automatizados, verificações manuais e condição de conclusão. Marcadores de verificação:
>
> - **[AUTO]** teste automatizado, executado no build.
> - **[MANUAL]** inspeção manual em Windows, com resultado registrado no DEVELOPMENT_LOG.md.
> - **[HW]** exige hardware específico: mais de um monitor, escalas diferentes, monitor em retrato ou possibilidade de conectar e desconectar. Nunca é declarado feito sem o hardware.
>
> Última atualização: 2026-10-01

## Fase atual

**Fase atual: Fase 5 — multi-monitor completo e posição persistida, em andamento.** O bloco A (passos P1–P5) está implementado e verificado por testes; o próximo passo da fase é o P6.

**Intercalada com a Fase 5, antes dos blocos B a D: a seção [Interação — emoção dominante e tamagotchi adulto](#interação--emoção-dominante-e-tamagotchi-adulto)** (DEC-027 e DEC-028). O núcleo, a arte e o app estão implementados, com a chave do tamagotchi ligada no aplicativo, e verificados por testes automatizados, de integração e, em 2026-10-01, pela verificação de tela com input SINTÉTICO e pelo repouso de 10 minutos com uma onda ativa: os passos T1–T9 estão feitos. Faltam a revisão visual e de tom pelo usuário, as outras pendências [MANUAL] e [HW] da seção e a persistência da emoção (passo P7).

Situação das etapas anteriores:

- Etapa 0B: fechou tecnicamente em 2026-09-29, com P3 aprovado no ambiente medido por evidência sintética.
- Identidade visual original: criada.
- Fase 1: implementada e verificada por automação e input sintético. Continua PLANNED enquanto faltarem os critérios manuais 3 (bandeja real), 8 (escalas 150% e 200%) e 9 (mudanças reais de resolução, escala e barra).
- Fase 2: VERIFIED em 2026-09-30.
- Fase 3: implementada e verificada por automação e input sintético. Continua PLANNED por três pendências: UAC [MANUAL], escalas diferentes [HW] e ClickLock [MANUAL].
- Fase 4: implementada e verificada por automação e input sintético, com os pedidos do usuário (DEC-023 a DEC-025). Continua PLANNED pelo critério 5, a gravação de tela a 120 qps [MANUAL].

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

STATUS: PLANNED, em andamento. O bloco A (passos P1–P5) está implementado e verificado por testes automatizados e pela regressão com input SINTÉTICO (2026-09-30). O app ainda não lembra a posição: isso é o passo P7.

- **Objetivo:** comportamento correto em qualquer topologia, com a posição restaurada entre execuções.
- **Inclui:** chave estável do monitor conforme o protótipo P5; restauração com alternativas (ARCHITECTURE.md, seção 2.8); circulação e travessia entre superfícies compatíveis de monitores; troca de escala em movimento com histerese conforme o protótipo P6; conexão, desconexão, rearranjo, troca de principal, rotação e troca de escala; barra de tarefas movida ou oculta automaticamente; recuperação se o Windows minimizar a janela ao desconectar um monitor; suspensão, retomada e bloqueio de sessão; **persistência mínima da posição escolhida pelo usuário** em `settings.json` com versão de esquema e gravação atômica.
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
- **Observação:** drivers de monitor virtual podem simular parte do hardware. STATUS: UNCERTAIN; isso não substitui [HW].
- **Passos de implementação.** A fase segue 16 passos, numerados de P1 a P16; no texto, aparecem como "passo P7", e um P-número sem "passo" é um dos protótipos P1 a P10 da Etapa 0B. As decisões são DEC-029 (persistência) e DEC-030 (chave do monitor e topologia). Para os blocos seguintes estão reservadas DEC-031 (sessão, energia e minimização) e DEC-032 (travessia e escala). Os invariantes 19 a 21 e a referência gravada 06 ficam reservados para a Fase 5; os invariantes 22 a 29 e a referência 07 são da seção "Interação". Os pontos marcados "(tamagotchi)" abaixo vieram da seção "Interação" e precisam ser preservados nos blocos B a D.
  - [x] **Bloco A, passos P1–P5 (2026-09-30):**
    - [x] P1. Tela do monitor na posição (`TelaDoMonitor`) e pixel dos pés (DEC-030).
    - [x] P2. Restauração com alternativas na partida: chave, tela do monitor, principal (DEC-030).
    - [x] P3. Esquema v1 do `settings.json` e política de gravação no núcleo; `AtravessarMonitores` nas preferências, ainda sem efeito; invariante 18 (DEC-029).
    - [x] P4. Pasta de dados e arquivo de configurações com gravação atômica, ainda desligados do app (DEC-029).
    - [x] P5. Isolamento dos testes por `--perfil-de-teste` (DEC-029).
  - [ ] P6. Chave estável do monitor, antes de gravar no disco real; a estabilidade depende do protótipo P5 (DEC-030).
  - [ ] P7. Persistência ligada: a raiz lê o arquivo na partida, entrega a posição salva ao núcleo e grava pela agenda (DEC-029). Inclui:
    - proteger a leitura contra qualquer exceção, com a persistência desligada naquela execução e o log só com tipo e código; estreitar junto a captura ampla dentro da leitura do esquema;
    - criar o arquivo só pela regra da pasta, com uma instância por execução, porque o bloqueio da gravação não volta atrás;
    - linhas `CONFIG` só com enums, contagens e tempos;
    - (tamagotchi) entregar no `Loaded` as preferências lidas, com a emoção dominante (DEC-027), e ligar a verificação V16 da seção "Interação";
    - gravar o esconderijo e a marca "preso pelo usuário" na v3 do esquema, porque a v2 já é a da emoção dominante; a versão futura dos testes passa de 3 a 4;
    - corrigir o comentário de `Programa.cs` que diz que, sem `--diagnostico`, o Buzzy não grava nada;
    - integração: soltar, fechar e reabrir no mesmo lugar (S2 e S7); partida com arquivo ilegível; o arquivo real intocado pelos testes.
  - [ ] P8. Topologia em execução, no núcleo (DEC-030):
    - o personagem continua quando só outros monitores mudam ou quando o dele é só transladado, e vai ao sobrevivente certo quando o dele some;
    - a tela gravada nunca é transladada;
    - o clique depois de o monitor mudar valida a partir da posição que acompanhou a topologia, o que resolve as frações da área antiga;
    - (tamagotchi) `USING` é tratado como `REACTING` nas revalidações, e a reacomodação dos itens segue a mesma regra.
  - [ ] P9. Releitura robusta no app: barra recriada, agrupamento de mensagens com teto e conferência tardia do lugar da janela (DEC-030).
  - [ ] P10–P12. Sessão, energia, fim de sessão e minimização pelo Windows. (tamagotchi) A recuperação da minimização vale também para as janelas dos itens.
  - [ ] P13. Travessia entre monitores, em três partes (plana, salto de degrau e transbordo), respeitando a DEC-023. (tamagotchi):
    - o recuo do cambaleio não pode disparar a travessia pela lateral de trás;
    - o personagem atento a um item espera a travessia em curso terminar, como a pausa;
    - o peso de atravessar entra no perfil efetivo da onda, com o percentual de andar (ARCHITECTURE.md 2.16).
  - [ ] P14. Escala mista na janela do app (protótipo P6). (tamagotchi) A correção de DPI vale também para a janela do item; nesse passo, extrair o gancho comum às duas janelas.
  - [ ] P15. Verificação de tela da Fase 5, com input SINTÉTICO e aviso ao usuário antes. (tamagotchi) A opção `--fase 5` convive com a `--fase tamagotchi` no mesmo programa de verificação.
  - [ ] P16. Gate: documentação da fase e repouso de 10 min com perfil de teste.
- **Evidências de 2026-09-30 (bloco A):**
  - [AUTO], só na parte de restauração e persistência de cada cenário; a travessia e as mudanças com o app aberto ficam para os passos seguintes:
    - S1–S7: `RestaurarTestes.PelaChave_S1aS7_AncorasCalculadasAMao`, com âncoras calculadas à mão e coordenadas negativas; `RestaurarTestes.Carga_S1aS7_VoltaAoMesmoLugarEmCadaMonitor`; `EsquemaDeConfiguracoesTestes.S1aS7_GravarLerRestaurar_MesmoLugarEmCadaMonitor`, que grava, lê e restaura em cada monitor;
    - S5 e S6: `RestaurarTestes.MesmaChaveComOutraGeometria_VaiPelaFracaoNaoPelaAncora`, a 144 DPI e com o monitor girado, além da troca de principal;
    - S9: `RestaurarTestes.MonitorSalvoAusente_NoPrincipalComAsMesmasFracoes`, `ChaveAusenteComAMesmaTela_PeloRetangulo` e `Carga_MonitorSalvoAusente_NaoVoltaQuandoReconectaEGravaOPrincipal`;
    - S11: o mesmo `MesmaChaveComOutraGeometria_…`, com a barra de tarefas nas quatro bordas;
    - S12, parcial no núcleo: `S12_BloquearESuspender_EmitemGravarPosicaoImediata` e `Politica_Imediata_SoSuspensaoFimDeSessaoSairEBloqueio`;
    - gravação atômica simulando falha entre gravar e substituir: `Gravar_FalhaSimuladaEmCadaEtapa_NuncaIlegivel`, `Ler_EstadosDeQuedaExaustivos` e `MatarDuranteGravacoes_NuncaDeixaIlegivel` (SECURITY.md 8, item 6);
    - invariante 18 nas sequências aleatórias sem a física (`InvariantesTestes`) e com a configuração do aplicativo, em 150 sequências (`PropriedadesDaPersistenciaTestes.NaConfiguracaoDoAplicativo_GravacoesECargasSeguemAsRegras`);
    - propriedades com 5.000 casos: a restauração segue a cascata e termina na área útil, e a ida e volta pelo arquivo equivale a reacomodar na mesma topologia;
    - isolamento: `IsolamentoTestes` e `ArquivoDeConfiguracoesTestes.PastaDasConfiguracoes_FalhaFechadaNumaRegraSo`.
  - [AUTO] suíte completa, com o Buzzy do usuário fechado: `tools/testar.ps1` (Release) com Core 309/309, portão 73/73 e App 59 (mais 11 de integração); `-Integracao` com App 70/70. Histórico em DEVELOPMENT_LOG.md.
  - [MANUAL] com input SINTÉTICO, só regressão das Fases 1, 3 e 4, com o perfil `verificacao`: 25 OK e 4 SIMULADO; 34 OK e 2 N/A; 28 OK; nenhuma falha. Não é evidência humana nem da Fase 5.
  - Pendentes:
    - S9 [MANUAL][HW] real e os demais [MANUAL][HW] da tabela;
    - Process Monitor (SECURITY.md 8, item 4), depois do passo P7;
    - a persistência ligada (passo P7) e os passos seguintes;
    - `tools/medir-desempenho.ps1` com o perfil `desempenho`: rodou pela primeira vez em 2026-10-01, só no `-Modo onda` da seção "Interação" (V13), sem pasta de uma medição anterior para limpar; os modos `repouso` e `autonomia` ainda não rodaram com ele (avisar o usuário antes). A limpeza das pastas reais de perfil ainda não apagou nada, porque o app só grava nelas a partir do passo P7.

### Interação — emoção dominante e tamagotchi adulto

STATUS: PLANNED, em andamento, intercalada com a Fase 5, antes dos blocos B a D. O núcleo (passos T1 e T3–T6) e a arte (A1–A4) foram implementados em 2026-09-30; o app (T2 e T7–T9), em 2026-10-01. Tudo foi revisado, corrigido e verificado por testes automatizados e de integração, com mensagens postadas às janelas do próprio Buzzy, em 2026-10-01. A chave `Tamagotchi` está ligada no aplicativo: o usuário já escolhe a emoção e invoca e usa os itens pelo menu. Em 2026-10-01, a verificação de tela com input SINTÉTICO e o repouso de 10 minutos com uma onda ativa fecharam o passo T9 (evidências abaixo). A seção continua sem VERIFIED pela revisão visual e de tom pelo usuário e pelas outras pendências [MANUAL] e [HW] abaixo; a persistência da emoção entre execuções é do passo P7 da Fase 5, e a V16 depende dele.

- **Objetivo:** os pedidos do usuário de 2026-09-30. Escolher a emoção dominante pelo menu, com os rostos de `expressoes.png` (DEC-027), e o tamagotchi adulto: 13 itens invocados pelo menu, que caem ao lado dele e são usados quando o usuário os arrasta e solta sobre ele, com efeitos de desenho animado no comportamento, nas caras e nas animações (DEC-028).
- **Inclui:** a emoção dominante no núcleo, no esquema e no menu; tipos, tabelas, ondas, itens e o estado `USING` no núcleo; a arte dos itens, das caras, das poses de uso, das sobreposições e dos ícones; a apresentação; as janelas dos itens; a chave ligada; integração e verificação de tela.
- **Exclui:** necessidades que decaem com o tempo (resposta 1 do usuário na DEC-028); poses de uso na parede, no cipó e no esconderijo, que ficam para depois; gravar os itens e a onda.
- **Depende de:** bloco A da Fase 5, já feito. A persistência da emoção entre execuções depende do passo P7.
- **Passos** (regras em DEC-027, DEC-028 e ARCHITECTURE.md 2.6 e 2.16; cada passo termina com `tools/testar.ps1` verde e as referências gravadas 01–05 idênticas byte a byte):
  - [x] **T1. Emoção dominante no núcleo e no esquema v2** (2026-10-01). [AUTO] `EmocaoDominanteTestes`, com 8 testes: listas fixas, escolher, valores fora das 14, carga, a dominante em pelo menos metade das trocas nas 14 opções, os três ganchos da cara de base, esconderijo e preso, e o invariante 27 com a física do aplicativo. Também `EsquemaDeConfiguracoesTestes` (texto da v2, v1 sem aviso, valores inválidos, versão futura 3), `ReproducaoTestes`, `PropriedadesDaPersistenciaTestes`, `InvarianteDezoito`, `InvariantesTestes` e `ArbitroPropriedadesTestes`, com o sorteio pelas 14 caras.
  - [x] **T2. Menu "Emoção dominante" com os rostos** (DEC-027; 2026-10-01). [AUTO] `MenuNativoTestes` (a lista do menu: ordem, ids, marca na atual, "Automática" sem dominante, alto contraste), `MontagemDoMenuTestes` (o menu montado de verdade e lido de volta, sem janela), `BitmapsDoMenuTestes` e `PlataformaTestes` (textos e teclas de acesso sem repetir). [AUTO, integração] `MenuIntegracaoTestes`: 20 aberturas com os objetos GDI e USER estáveis e bitmaps criados = apagados; a emoção escolhida por teclado postado ao dono do menu troca a cara e fica marcada. V1 e V2 passaram na verificação de tela com input SINTÉTICO, no passo T9.
  - [x] **T3. Tipos e tabelas do tamagotchi, sem comportamento** (2026-10-01). [AUTO] `TabelaDoTamagotchiTestes`, com 8 testes.
  - [x] **T4. Onda da frente, atrás da chave** (2026-10-01). [AUTO] `OndaTestes`: durações; disparo velho ou repetido ignorado; pausado, o disparo só troca a cara; 6 horas simuladas de repouso sem relógio; perfil e física efetivos; velocidades nos cinco lugares do passo físico; cambaleio; caras e gestos da fase com os mesmos sorteios; precedência sobre a dominante; chave desligada; retrato, gravação, fila e simulador.
  - [x] **T5. Itens, `USING`, onda de fundo e combinação, atrás da chave** (2026-10-01). [AUTO] `ItensTestes`, `UsoTestes`, o resto de `OndaTestes`, `FilaEAleatorioTestes` e `InvariantesTestes`, com os invariantes 22 a 29.
  - [x] **T6. Referência gravada `07-tamagotchi.txt`**, com a diretiva `# tamagotchi: sim` (2026-10-01). Revisada linha a linha.
  - [x] **A1–A4. Arte** (2026-10-01; feita numa cópia isolada e mesclada na árvore principal, com o hash de cada arquivo conferido). [AUTO] `CarimboTestes`, `ItensPixelTestes`, `RostosNovosTestes`, `UsosPixelTestes`, `EfeitosPixelTestes`, `IconesDoMenuTestes`, `GestosPixelTestes` e `RostoDoDesenhoTestes`. A ferramenta de prévias sai com código 1 se algo encosta na borda do quadro ou tem o contorno cortado.
    - A1: paleta, legenda que recusa letra repetida, carimbo girado e os 13 itens em 24 × 24;
    - A2: caras de efeito e passageiras, chapéu torto e rubor;
    - A3: poses de uso no chão, com a ponta do item na boca ou no nariz;
    - A4: sobreposições, modificadores, poses provisórias dos gestos e ícones do menu.
  - [x] **Revisões adversariais e correções** (2026-10-01), do núcleo e da arte, com mutações temporárias. O que mudou está nos desvios da DEC-028. Entre os testes novos está `ChaveLigadaTestes`, que confere os invariantes 22 e 27 com a física e a configuração do aplicativo: é condição para o T9.
  - [x] **T7. Apresentação, sem janelas de item** (2026-10-01): a pose de `USING` por verbo, passo e apoio; as caras de efeito; as poses dos gestos da onda; a sobreposição pela onda; o cache de quadros limitado a 16 MiB. [AUTO] `NucleoEArteTestes` (os enums do núcleo contra as listas da arte), `PoseDeUsoTestes`, `CacheDeQuadrosTestes` e `PoseTestes`, que desenha cerca de 3 mil quadros distintos, com todos os usos, caras, gestos, sobreposições, fases e giros.
  - [x] **T8. No app, as janelas dos itens, o submenu "Itens" e o temporizador da onda,** com a chave ainda desligada (2026-10-01). [AUTO] `SpriteDoItemTestes`, `GestosDosItensTestes`, `TemporizadorDaOndaTestes` (o temporizador real desliga depois de cada disparo), `GerenteDosItensTestes`, `JanelaDoItemTestes` (janelas reais criadas sem mostrar: 20 criadas e fechadas sem vazar objetos) e os testes do menu com o submenu "Itens" (ids, teclas de acesso e 27 bitmaps criados = apagados por abertura). Antes do T9, a ligação foi exercitada numa cópia isolada, com a chave ligada só lá.
  - [x] **T9. Ligar a chave** em `DoAplicativo`, com a integração, a verificação de tela abaixo e um repouso de 10 minutos com uma onda ativa; documentação do app e do produto (PRODUCT_SPEC.md) (2026-10-01):
    - [x] chave ligada e testes que esperavam a chave desligada ajustados (2026-10-01);
    - [x] integração `ItensIntegracaoTestes`, ampliada pela revisão de correção (2026-10-01; evidências abaixo);
    - [x] verificação de tela (`--fase tamagotchi`) e medição com onda (`tools/medir-desempenho.ps1 -Modo onda`) escritas e compiladas (2026-10-01);
    - [x] revisão de correção e das regras do projeto, com as correções aplicadas, e a documentação do app e do produto (2026-10-01);
    - [x] verificação de tela executada com input SINTÉTICO, depois de o usuário liberar o canto da tela (2026-10-01): 46 OK, 1 N/A (V16) e 0 falhas na execução das 10:26, depois de duas correções na ferramenta (resultados abaixo);
    - [x] repouso de 10 minutos com uma onda ativa (`-Modo onda`; 2026-10-01): `resultados/desempenho-20261001-085153.txt`, na linha V13 abaixo.
- **Evidências de 2026-10-01, núcleo e arte** (contagens e histórico em DEVELOPMENT_LOG.md):
  - [AUTO] suíte completa depois da mescla da arte: `tools/testar.ps1` (Release) código 0, com Core 393, portão 73, App 150 (mais 11 de integração, que não rodaram), portão de APIs aprovado e nenhum pacote vulnerável;
  - [AUTO] referências 01–05 idênticas byte a byte, conferidas por SHA-256 e `git diff`;
  - [AUTO] propriedade (`InvariantesTestes`): a execução principal repetida com a chave ligada e sem itens dá o mesmo registro (invariantes 7 e 22); a execução com o tamagotchi, em 1.000 sequências e cerca de 471 mil eventos, confere os invariantes 22 a 29 contra regras escritas no teste; a comparação com a emoção confere o invariante 27;
  - [AUTO] `ChaveLigadaTestes`, com a física e a configuração do aplicativo: a chave ligada sem itens dá o mesmo resultado que desligada (60 sementes × 10 min, cerca de 508 mil eventos), e a dominante só muda a cara, com itens e ondas (40 sementes, cerca de 126 mil eventos, 108 usos);
  - [AUTO] simulação longa com a física (4 horas simuladas): usos até o fim no chão, na parede e no cipó, presos e livres, e no esconderijo;
  - [AUTO] mutações temporárias em cada passo, nas revisões e nas correções. As que escaparam mostraram lacunas, que viraram testes novos, ou eram código equivalente;
  - observação informal: Claude olhou as prévias `itens-8x.png`, `rostos-efeito.png`, `usos.png` e `icones-menu.png` e as achou legíveis e no estilo. Não é aprovação do usuário.
- **Evidências de 2026-10-01, app** (passos T2 e T7–T9, na última rodada da correção do app; histórico em DEVELOPMENT_LOG.md):
  - [AUTO] `tools/testar.ps1` (Release) código 0, com Core 393, portão 73, App 249 (mais 22 de integração, que só rodam com `-Integracao`), 0 avisos, portão de APIs aprovado e nenhum pacote vulnerável; referências 01–05 idênticas byte a byte;
  - [AUTO, integração por mensagens postadas às janelas do próprio Buzzy, com o PID conferido antes de cada uma] `tools/testar.ps1 -Integracao` código 0, com App 271/271:
    - `MenuIntegracaoTestes`: 20 aberturas do menu com GDI 32 → 32 e USER 22 → 22, cada abertura apagando os bitmaps que criou; e a emoção escolhida pelas teclas D e F, postadas ao dono do menu, troca a cara e fica marcada na abertura seguinte;
    - `ItensIntegracaoTestes`, 9 testes: invocar pelo menu do personagem e pela bandeja; a janela do item aparece sem ativar e sem tirar o primeiro plano, cai, para no chão e fica logo abaixo do personagem; arrastar até ele leva a `USING` e a janela some, e pressionar no meio do uso dá `PRESSED` no mesmo evento; soltar longe; botão direito no item e "Recolher itens"; esconder e mostrar; arrastar o personagem com um item na tela; minimizar no meio do arraste de um item; sair com itens na tela e um na mão, com código 0 e nenhuma janela viva; o temporizador da onda dispara uma vez, reagenda e é cancelado ao sair;
    - o arraste de um item teve p95 de 1,125 ms entre o movimento do mouse e a janela no lugar;
  - [AUTO] `LigacaoDosItensTestes` e `LeituraDoLogTestes`, da correção, e mutações temporárias em cada passo e na correção: as que escaparam viraram testes mais fortes ou eram código equivalente;
  - a integração não é verificação de tela nem evidência humana. Durante ela, o foco pisca, porque cada menu dá o primeiro plano ao dono temporário (DEC-016).
- **Verificação de tela** com input SINTÉTICO, escrita no passo T9 e **executada em 2026-10-01**: na raiz do repositório, sem nenhum Buzzy aberto, `tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela --fase tamagotchi [--semente N]`. Ela abre o Buzzy várias vezes com o perfil `verificacao`, `--diagnostico` e uma semente (pausado, a de `--semente`, padrão 2028; com a agenda ligada, sementes escolhidas por simulação do núcleo). Tem um repouso de 60 s em que nada deve ser tocado e grava `resultados\verificacao-tamagotchi.log`; em 2026-10-01, levou cerca de 5 minutos. Claude a roda depois de avisar o usuário; o resultado não é evidência humana.

  Antes de rodá-la, Claude repetiu `tools/testar.ps1` (código 0, com Core 393, portão 73 e App 249, mais 22 de integração) e `tools/testar.ps1 -Integracao` (271/271); depois de corrigir a ferramenta, repetiu `tools/testar.ps1`, de novo com código 0. A execução válida é a das 10:26 às 10:31, a última do relatório, com a semente 2028 e os dois monitores a 96 DPI: **46 OK, 1 N/A e 0 falhas**. As duas anteriores do mesmo dia falharam pelo ambiente e por dois defeitos da ferramenta, já corrigidos; nenhuma falha foi do Buzzy (DEVELOPMENT_LOG.md). Resultado por caso:

  | Caso | O que confere | Resultado em 2026-10-01 (input SINTÉTICO) |
  |---|---|---|
  | V1 | Menu de emoção pelo clique direito e pelo teclado: a cara muda e a opção fica marcada; "Automática" também funciona. | OK: Feliz e depois "Automática", pelas teclas D e a letra; o núcleo registrou o comando, o sprite parado ficou com a cara feliz, e a opção escolhida estava marcada na abertura seguinte. |
  | V2 | 20 aberturas do menu: objetos GDI e USER estáveis (±2) e bitmaps criados = apagados. | OK: GDI 32 → 32 e USER 22 → 22; nas 20 aberturas, 27 ícones (14 rostos e 13 itens), com criados = apagados. |
  | V3 | Invocar a banana: ela pousa ao lado em até 1,5 s e fica 3 s parada; o relógio desliga; o ponto opaco do item cai na janela do item e o transparente no receptor; o foco fica no receptor. | OK: pousou ao lado dele, sem cobri-lo, 0,606 s depois de aparecer, e ficou 3 s parada; o relógio ligou na queda e desligou; o ponto opaco era da janela da banana, e o clique no transparente chegou ao receptor, que continuou na frente. |
  | V4 | Arrastar o item até ele: `USING` no log, os quadros de uso na ordem, a janela do item some e ele volta a `IDLE`; o arraste do item com p95 abaixo de 16,7 ms. | OK: `USING` ("Comer Banana"), os quadros `comendo-1` a `comendo-8` na ordem, a janela removida com o motivo Usado e a volta a `IDLE`; o arraste do item teve p95 de 0,685 ms em 47 movimentos. |
  | V5 | Soltar longe: o item cai de onde foi solto. | OK: solto 250 px acima e longe dele, caiu na mesma coluna até o chão, sem uso. |
  | V6 | Pressionar no meio do uso: `PRESSED` em até um quadro, e a onda continua. | OK: `USING` → `PRESSED` no próprio `PRESS`, visto no log 108 ms depois (o log é lido a cada 100 ms); soltar virou clique; a onda não foi cancelada, e o disparo seguinte veio depois do `PRESS`. |
  | V7 | Sétimo item: o mais antigo sai. | OK: a banana, o item mais antigo, saiu com o motivo Substituido, e os outros seis ficaram à vista. |
  | V8 | Com a agenda ligada, segurar um item: `WALKING` vira `IDLE`, nenhuma decisão durante o gesto, e a próxima em 3 s ou mais. | OK, semente 34: `WALKING` → `IDLE` no `ITEM_PRESS`; nenhum agendamento nem disparo da agenda durante o gesto de 1,5 s; a próxima decisão veio 8.287 ms depois de soltar. |
  | V9 | Soltar um item nele preso no cipó, na parede e no esconderijo: ele volta preso ao mesmo lugar. | OK nos três (banana no cipó, cerveja na parede e café no esconderijo): usou o item, voltou ao mesmo retângulo e, pausado, ficou lá por 5 s (81/81 amostras). |
  | V10 | Esconder e mostrar: as janelas dos itens acompanham. | OK, com 3 itens: escondido pelo menu e mostrado ao abrir o app de novo, ele levou junto as 3 janelas de item, escondidas sem fechar e de volta as mesmas, abaixo do personagem na ordem Z; reaparecer não tirou o foco. |
  | V11 | Cocaína contra baseado (as ondas elétrica e chapada): velocidades de caminhada com tolerância de ±10%. | OK: cocaína (onda elétrica, pico 2, 185%) a 168,7 px/s, contra 166,5 esperados; baseado (onda chapada, pico 2, 55%) a 50,8 px/s, contra 49,5. |
  | V12 | Onda do bêbado no nível 3: recua na caminhada, sempre no chão. | OK, semente 49: 2 recuos de 1 px em 4,0 s de caminhada, com os pés sempre no chão. |
  | V13 | Repouso pausado com a onda de uma vodka ativa: CPU média de até 0,1%, relógio desligado. Na verificação, 60 s. Os 10 minutos ficam em `powershell -NoProfile -File tools\medir-desempenho.ps1 -Modo onda` (cerca de 11 minutos, relatório em `resultados\desempenho-AAAAMMDD-HHMMSS.txt`), que invoca e entrega a vodka por mensagens postadas, sem `SendInput`, separa a CPU com a onda e depois dela e conta quantas vezes o relógio ligou na janela medida (esperado 0). | OK. Nos 60 s da verificação: CPU 0,000% de um núcleo, relógio desligado e 1 disparo único da onda, que só trocou a cara. Nos 10 minutos (`resultados\desempenho-20261001-085153.txt`, iniciado às 08:51): CPU média de 0,005% de um núcleo com a onda e de 0,000% depois dela, 0,003% nos 10 min e p95 de 0,000%; nenhuma linha de relógio ligado; 4 disparos únicos nos tempos da tabela (8 s, 100 s, 100 s e 112,5 s; o primeiro, antes da janela medida); nenhum processo filho, nenhuma conexão e a resolução do timer inalterada; memória privada +0,21 MB; encerramento limpo, com código 0. |
  | V14 | Sair com itens na tela: código 0 e nenhuma janela viva. | OK, com 3 itens: código 0, as 3 janelas de item fechadas pela raiz e nenhuma janela nem processo do Buzzy depois. |
  | V15 | Botão direito num item abre o menu. | OK: o clique direito na bala abriu o mesmo menu do personagem. Nele, "Recolher itens" (I e R) recolheu os 6 itens, sem nenhuma janela de item viva. |
  | V16 | Emoção gravada no perfil de teste e restaurada ao reabrir. | N/A: a persistência entre execuções só é ligada no passo P7. |
  | X1 | Dois monitores: arrastar um item até o outro monitor e soltar no ar; ele cai até o chão de lá. Roda nesta máquina, que tem um segundo monitor à esquerda, na mesma escala; entre monitores de escalas diferentes, continua [HW]. | OK, com os dois monitores a 96 DPI: arrastado do principal até o `\\.\DISPLAY2`, em x negativo, e solto no ar, o item caiu até o chão da área útil de lá. Entre monitores de escalas diferentes, continua [HW]. |
  | Calma | Pausar com ele agarrado à parede sem estar preso: ele desce até o chão (regra da calma, DEC-022, item 4). Essa regra já valia no app antes da chave e só tinha testes automatizados. | OK, semente 19: clicado enquanto escalava, ficou grudado na parede sem estar preso; pausado pelo menu, desceu pela mesma lateral (26/26 amostras) e pôs os pés no chão em 1,2 s (`CLIMBING` → `IDLE`). |
  | Foco | Depois de fechar cada menu do tamagotchi, o foco volta ao aplicativo em uso. | OK nos 50 menus: a ferramenta não precisou devolver o primeiro plano ao aplicativo em uso nenhuma vez, e todo o texto digitado chegou a ele, na ordem. |

  Os 46 OK somam os casos da tabela (a V9 conta três vezes), "Recolher itens", 4 conferências de regressão da Fase 1 no começo, os 18 textos digitados que chegaram ao aplicativo em uso, o foco até o primeiro menu, a limpeza e a conferência final do texto. A regressão com input SINTÉTICO, no mesmo dia, deu 25 OK e 4 SIMULADO (a bandeja) na `--fase 1`, 34 OK e 2 N/A na `--fase 3` e 28 OK na `--fase 4`, sem falhas.

  Como ler o relatório: a execução não pode sair INVÁLIDA, que indica interferência humana. O V4 sai INCONCLUSIVO, com os quadros pulados, quando a interface atrasa e pula quadros, e pede repetição; FALHOU indica defeito, do Buzzy ou da própria ferramenta, como nas duas falhas da primeira rodada depois de liberado o canto, em 2026-10-01. O V12 pode sair INCONCLUSIVO, porque o recuo é menor que 1 pixel por volta e um quadro com dois passos o esconde. O cenário da calma sai INCONCLUSIVO se nenhum de 3 cliques nele escalando chegar ao Buzzy, porque ele se move entre a conferência do ponto e o clique.
- **Pendências [MANUAL] e [HW]:**
  - a marca de rádio ao lado do rosto, e os ícones dos itens, nos temas claro, escuro e de alto contraste [MANUAL];
  - o menu e as janelas dos itens a 125, 150, 175 e 200% [MANUAL]; o usuário consegue testar parte disso mudando a escala, o que Claude não faz, por ser uma configuração global. Os fatores 2 e 3 dos ícones só foram testados sem janela, e esta máquina está a 96 DPI;
  - a 300%, o cache de 16 MiB guarda só 28 quadros: andando com uma sobreposição, conferir os quadros descartados na linha `SPRITE` do log [MANUAL];
  - a altura das opções com o ícone de 40 × 32 a 96 DPI e o queixo cortado reto do recorte [MANUAL];
  - arrastar um item entre monitores de escalas diferentes [HW];
  - o Narrador lendo os submenus [MANUAL];
  - **revisão visual e de tom pelo usuário** [MANUAL]: as prévias de `assets/identidade/pixel/previa/`, os nomes e o app em uso. Pontos a olhar:
    - o espelhinho no nariz, que ainda pode parecer uma bandeja; o energético verde-neon; o baseado aceso no chão; o lenço dobrado; o MD e a bala na mão, com 5 × 5 e 9 × 4 pixels a 100%;
    - dois efeitos juntos nos quadros de uso que já têm efeito próprio, quando há uma onda;
    - a tremedeira, 7,5 vezes por segundo; o bêbado inclinado e parado na fase 0, sem relógio;
    - `escalando-1` com as caras de chapéu eriçado, como surpreso e empolgado, no uso na parede;
    - no esconderijo, a boca fica fora do quadro, e só os olhos mostram a cara do uso;
    - um possível quadro com o item por cima do personagem logo ao aparecer, antes de ir para baixo dele, nunca observado;
  - conforto para agarrar os itens pequenos [MANUAL]. A 100%, as caixas opacas medidas na tela foram banana 40 × 22, cerveja 36 × 36, vodka 20 × 42 e café 34 × 22 px; os itens finos, como o cigarro e o MD, ainda não foram medidos;
  - desconectar o monitor com itens na tela (cenário S8), depois do passo P12 [HW];
  - gravação a 120 qps das animações de uso [MANUAL], como o critério 5 da Fase 4, que já estava pendente;
  - Process Monitor confirmando que o Buzzy só grava em `%LOCALAPPDATA%\Buzzy`, depois do passo P7 [MANUAL].

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
  3. Arquivo corrompido gera os valores do `.bak` ou, sem ele, os valores padrão, guarda uma cópia e o app continua (DEC-029). [AUTO]
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

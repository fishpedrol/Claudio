# TODO.md — Fases e pendências do Buzzy

> Roadmap operativo único do projeto. Cada fase termina com build, testes e verificação do próprio escopo. A Fase 10 é a regressão integrada, não o primeiro momento de testar.
>
> **Formato.** Tarefa pendente: `- [ ]`. Tarefa concluída: `- [x]` com a data e, quando houver, a evidência. Cada fase lista objetivo, inclui, exclui, depende de, critérios de aceitação, testes automatizados, verificações manuais e condição de conclusão. Marcadores de verificação:
>
> - **[AUTO]** teste automatizado, executado no build.
> - **[MANUAL]** inspeção manual em Windows, com resultado registrado no DEVELOPMENT_LOG.md.
> - **[HW]** exige hardware específico: mais de um monitor, escalas diferentes, monitor em retrato ou possibilidade de conectar e desconectar. Nunca é declarado feito sem o hardware.
>
> Última atualização: 2026-09-26

## Fase atual

**Fase 0 — Descoberta e arquitetura. STATUS: PLANNED.** A comparação e a proposta estão documentadas. O usuário delegou a escolha da stack ao Codex; WPF/C#/.NET 10 foi selecionada em DEC-006. As escolhas de produto necessárias agora estão respondidas. Faltam os protótipos P1–P3, a revisão independente dos resultados e o fechamento documental da fase. Q-08 será decidida após a medição P2; formato e assinatura de distribuição pública ficam para depois. A Fase 0 **não** está concluída e nenhum protótipo foi executado.

Ainda não há código do aplicativo. A Etapa 0B permite somente protótipos descartáveis e isolados em `spikes/`.

### Pendências da Fase 0

- [x] 2026-09-26 — Consolidar a visão do Buzzy e os limites do MVP em uma fonte de produto. Evidência: docs/PRODUCT_SPEC.md.
- [x] 2026-09-26 — Separar a especificação estável do produto do prompt mestre variável por fase. Evidência: docs/PRODUCT_SPEC.md e context codex/PROMPT_MESTRE_BUZZY.md.
- [x] 2026-09-26 — Definir o ciclo Claude → revisão independente do Codex → decisão do usuário. Evidência: AGENTS.md e docs/PLAN_REVIEW.md.
- [x] 2026-09-26 — Criar README com links para as fontes canônicas. Evidência: README.md.
- [x] 2026-09-26 — Preservar o trabalho documental anterior. Evidência: commit 257a05f, que guarda a documentação como estava antes do planejamento técnico. A parte de indicar como recuperar a revisão de fundo não foi cumprida e está na tarefa seguinte.
- [x] 2026-09-26 — Manter em PROJECT_CONTEXT.md um inventário resumido de todas as áreas do projeto. Evidência: docs/PROJECT_CONTEXT.md, seções 8 e 9.
- [x] 2026-09-26 — Confirmar que a revisão documental de fundo não pode ser recuperada deste repositório e registrar a limitação. Os números citados no DEVELOPMENT_LOG.md vêm de uma transcrição externa, não versionada, e aquela revisão tratava dos documentos de 2026-09-25, anteriores à reestruturação do Buzzy; portanto, não substitui a revisão atual. A revisão independente válida da Fase 0 foi feita pelo Codex e está registrada em PLAN_REVIEW.md.
- [x] 2026-09-26 — Inicializar Git e `.gitignore` mínimo. Criado sem remoto. O remoto `origin` passou a existir depois, fora desta rodada.
- [x] 2026-09-26 — Recuperar o workflow `buzzy-stack-research` em vez de iniciar pesquisa duplicada. Ele foi retomado do mesmo script e concluiu as três etapas. Evidência no repositório: a comparação em DEC-006 e as fontes que ela lista. Os números do processo, como quantidade de pesquisadores e de verificações, estão no DEVELOPMENT_LOG.md e vêm da saída da sessão, fora do repositório; não são reproduzíveis a partir dos arquivos versionados.
- [x] 2026-09-26 — Completar a comparação de alternativas de stack com critérios, fontes e trade-offs. A pesquisa original avaliou nove stacks; a escolha final WPF/C#/.NET 10 foi feita por delegação do usuário e está em DEC-006.
- [x] 2026-09-26 — Atualizar o encaixe de WPF na arquitetura (ARCHITECTURE.md 2.13), com janelas, integração Windows, repouso e portões técnicos pendentes.
- [x] 2026-09-26 — Propor arquitetura mínima, fluxo, máquina de estados, arbitragem de input, clique e arraste, foco da caixa de texto e ciclo de arraste. Proposta em ARCHITECTURE.md, STATUS: PLANNED.
- [x] 2026-09-26 — Propor modelo do desktop virtual, superfícies, DPI, reconexão e restauração. Proposta em ARCHITECTURE.md e DEC-008.
- [x] 2026-09-26 — Completar permissões e riscos por stack em SECURITY.md 4, incluindo processos, runtime externo, superfície extra e como cada alternativa obteria o click-through.
- [x] 2026-09-26 — Propor critérios de aceitação e testes por fase (este arquivo) e plano de medição de desempenho (DEC-011).
- [x] 2026-09-26 — Revisar independentemente o plano conforme PLAN_REVIEW.md. A arquitetura está alinhada ao MVP e a ordem continua segura; WPF foi escolhido por delegação, com P1–P3 como gates antes da Fase 1. Atualizadas as referências visuais para deixar claro que são ponto de partida para uma nova proposta original. A revisão não verificou código nem encerrou a Fase 0.
- [x] 2026-09-26 — Registrar as respostas do usuário para Q-02 a Q-07, Q-09, Q-11, Q-12, Q-14, Q-20 e Q-21 e o ZIP portátil sem assinatura para uso pessoal/testes em Q-10. Evidências: DECISIONS.md e DECISOES_DO_USUARIO.md. Q-08 foi adiada para depois de P2; distribuição pública e assinatura continuam para decisão futura em Q-10. Q-01 e Q-13 foram resolvidas por delegação do usuário; Q-15 foi resolvida como questão operacional; Q-16 a Q-19 foram esclarecidas como referências visuais; Q-22 não se aplica após a escolha de WPF.
- [ ] Etapa 0B, protótipos descartáveis P1, P2 e P3 com WPF, antes da Fase 1. P9 foi encerrado sem execução porque avaliava interface Win32 manual.
- [ ] Registrar instruções de build e teste em PROJECT_CONTEXT.md quando o aplicativo WPF da Fase 1 for criado.

### Critério para concluir a Fase 0

- Comparação técnica e escolha de stack registradas em DECISIONS.md, com evidências, riscos e alternativas. **Feito; WPF foi escolhido por delegação do usuário.**
- ARCHITECTURE.md descreve o desenho WPF como PLANNED, sem apresentá-lo como implementado. **Feito; P1–P3 ainda precisam ser verificados.**
- Permissões e riscos por stack registrados em SECURITY.md. **Feito na seção 4.**
- Máquina de estados, fluxo de arraste, modelo do desktop virtual, segurança, persistência, testes e métricas têm critérios verificáveis. **Feito como proposta; as metas numéricas dependem de Q-08 e da medição em P2.**
- O Codex revisou o plano e as correções aceitas foram aplicadas.
- A escolha da stack, da ordem dos protótipos e das decisões de produto para a Fase 1 está registrada. Q-02 e Q-03 foram respondidas; ZIP portátil sem assinatura para uso pessoal/testes está escolhido em Q-10. A forma de empacotar o runtime ainda será definida no plano do build; distribuição pública pode ser decidida mais tarde.
- A documentação e a seção Fase atual do prompt mestre descrevem a mesma próxima fase.
- O usuário decide iniciar a Fase 1.

Antes da revisão final, nenhum espaço reservado pode permanecer como se fosse um entregável concluído. Toda caixa `[x]` precisa corresponder a conteúdo verificável e registrado.

## Etapa 0B — Protótipos de viabilidade

STATUS: PLANNED. Fazer antes da Fase 1 com a stack escolhida, WPF/C#/.NET 10. O código é descartável, fica em `spikes/` fora do produto e não segue para a Fase 1. Cada protótipo registra método, hardware, métricas e resultado no DEVELOPMENT_LOG.md.

| ID | Pergunta | Teste mínimo | Resultado esperado | Hardware |
|---|---|---|---|---|
| P1 | A janela WPF deixa passar o clique nos pixels transparentes? | Janela `AllowsTransparency`, do tamanho do sprite, sobre o Bloco de Notas. Clicar em pixel alfa 0, alfa 1 e pixel visível. | Alfa 0 entrega o clique ao Bloco de Notas; pixels visíveis e alfa 1 atingem a janela do Buzzy. Registrar Windows, DPI, renderização e limites encontrados. | 1 monitor |
| P2 | Quanto custa ficar parado e animar em WPF? | Sprite parado por 1 h sem timers. Depois, animação a 10 e 60 quadros por segundo por 10 min cada. Medir CPU, memória, GPU e acordadas por segundo com o mesmo protocolo em todos os estados. | Em repouso o app deixa de redesenhar e não gera atividade periódica evitável. Registrar números para Q-08; se falhar, ajustar ciclo visual e repetir. | 1 monitor |
| P3 | O arraste funciona sem roubar foco em WPF? | Janela que não ativa, com captura ao pressionar. Arrastar rápido para fora da janela, soltar fora, usar Alt+Tab no meio, ligar ClickLock e digitar no Bloco de Notas antes/depois; repetir entre monitores quando houver dois disponíveis. Se a captura básica falhar, pode-se testar uma margem temporária de captura alfa 1 enquanto o gesto ocorre. | O arraste acompanha o cursor e termina ao soltar; o Bloco de Notas mantém o foco. Qualquer alternativa que mude o foco não conta como aprovação do requisito; se não houver solução sem roubo de foco, reabrir DEC-006 antes da Fase 1. | 1 monitor; 2 para repetição [HW] |
| P4 | A caixa de texto recebe foco e IME? | Abrir a caixa em resposta a clique duplo e, em outro teste, por timer. Digitar com acentos e com um IME. Fechar a caixa e ver para onde vai o foco. | Com clique, o foco chega. Por timer, o Windows nega o foco e a digitação continua no outro app. IME funciona. O destino do foco ao fechar fica registrado. | 1 monitor |
| P5 | Quais mensagens chegam quando a topologia muda, e a chave do monitor é estável? | Janela que registra mensagens e a leitura da topologia ao conectar, desconectar, rearranjar, girar, trocar o principal e trocar a escala. Reiniciar e trocar portas, comparando as chaves. Repetir com as opções do Windows 11 de lembrar posições e minimizar janelas ao desconectar. | Lista de mensagens por cenário, intervalo de agrupamento calibrado, estabilidade da chave confirmada ou refutada. | 2 monitores [HW] |
| P6 | Como a janela se comporta ao cruzar monitores de escalas diferentes? | Janela movida por código de um monitor a 100% para um a 200%, parando sobre a borda. | Ponto em que a escala troca, ausência de oscilação com histerese e pés sempre sobre o chão. | 2 monitores com escalas diferentes [HW] |
| P7 | É possível saber que há um app em tela cheia? | Consulta a cada 2 a 5 s com vídeo em tela cheia no navegador, jogo em janela sem borda e jogo em tela cheia exclusiva. | Estado correto nos três casos, com custo de CPU desprezível. O caso sem borda precisa ser confirmado. | 1 monitor |
| P8 | A janela layered aguenta ser fotografada e continua funcionando? | Outro processo chama a função que tira foto de janela durante a animação. | A atualização da janela continua funcionando, ou falha e é recuperada religando o estilo. | 1 monitor |
| P9 | Encerrado: comparar o esforço de interface nativa | Não executado. Perdeu a finalidade quando WPF foi escolhido como stack. | Sem resultado; não bloqueia o projeto. | — |
| P10 | Build, tamanho e portão de segurança | Build de release; medir o executável e o pacote. Rodar como portátil em máquina virtual limpa do Windows, com e sem assinatura, com o controle de aplicativos ligado. Rodar o script que inspeciona as funções importadas pelo binário. | Tamanho registrado, comportamento do aviso do sistema conhecido e portão de APIs proibidas passando. | Máquina virtual |

**Ordem e papel dos protótipos.** Executar P1 primeiro, depois P3 e por último P2. P1 ou P3 que falhe reabre DEC-006 antes de produto; P2 informa as metas de Q-08 e pode exigir ajuste de repouso:

- **P1** verifica que a janela WPF cumpre o requisito central de clique por pixel entre processos.
- **P3** verifica input e foco no gesto de maior risco para a experiência.
- **P2** confirma o repouso real e define a base das metas Q-08.

P4 a P8 e P10 podem acontecer no início das fases que dependem deles.

## Roadmap do MVP

Cada fase tem STATUS: PLANNED. Nenhuma fase é concluída só porque compila ou porque as tarefas foram marcadas. Em todas as fases, a condição de conclusão inclui:

- build limpo;
- testes automatizados verdes;
- portão de APIs proibidas e auditoria de dependências verdes;
- critérios de aceitação verificados e registrados;
- documentação sincronizada;
- gate PASS registrado conforme o prompt mestre.

### Fase 1 — Shell do desktop

- **Objetivo:** janela do personagem transparente, posicionada corretamente no desktop virtual, com ciclo de vida completo.
- **Inclui:** projeto e build da stack aprovada; manifesto Per-Monitor V2 e `asInvoker`; janela do tamanho do sprite com transparência por pixel; sprite provisório estático e original; janela que não ativa; sempre no topo, bandeja e ausência de botão na barra de tarefas conforme Q-03; instância única, em que abrir o app de novo mostra o Buzzy existente; módulo do mundo do desktop com consultas de monitor e área útil; releitura da topologia e acomodação da posição quando ela muda; saída pelo menu; portão de APIs proibidas no build; script de medição de desempenho; instruções de build e teste em PROJECT_CONTEXT.md.
- **Exclui:** arraste, movimento, animação, persistência, conversa.
- **Depende de:** Fase 0 concluída; P1, P2 e P3 aprovados; e autorização explícita do usuário para iniciar a Fase 1. O alvo Windows 11, os comportamentos Q-03 e o ZIP de teste Q-10 já foram decididos. Q-01 e Q-13 estão resolvidas; P9 foi encerrado.
- **Critérios de aceitação:**
  1. O app inicia e mostra o sprite sobre a área útil do monitor principal. [MANUAL]
  2. Clicar em pixel transparente dentro do retângulo da janela entrega o clique ao aplicativo de baixo. [MANUAL]
  3. Clicar no sprite não tira o foco do aplicativo ativo. [MANUAL]
  4. O Buzzy não aparece na barra de tarefas nem no Alt+Tab, conforme Q-03. [MANUAL]
  5. O sprite fica nítido com o monitor a 100%, 150% e 200%, trocando a escala nas Configurações com o app aberto. [MANUAL]
  6. Trocar resolução, escala ou posição da barra de tarefas com o app aberto mantém o sprite dentro da área útil. [MANUAL]
  7. Sair pelo menu encerra o processo e todos os processos filhos. [MANUAL]
  8. Parado por 10 min, o consumo de CPU, memória e acordadas fica registrado como linha de base. [MANUAL, instrumentado]
- **Testes automatizados:** mundo do desktop com topologias de exemplo (lado a lado, empilhado, em L, coordenadas negativas, principal fora da esquerda, retrato, escalas mistas, vão entre monitores); teste de fumaça que inicia o app, encontra a janela, confere os estilos e encerra; portão de APIs proibidas. [AUTO]
- **Verificação com hardware:** critério 6 com dois monitores. [HW]

### Fase 2 — Núcleo do personagem

- **Objetivo:** núcleo determinístico, testável sem janela, com máquina de estados, eventos, relógio lógico e expressão separada.
- **Inclui:** tipos de estado, evento, retrato e efeito; tabela de transições de ARCHITECTURE.md, seção 2.6; fila com prioridade; agenda autônoma com semente; expressão como dimensão independente; relógio que para quando nada muda; gravação e reprodução de sequências de eventos para testes; ligação do núcleo à janela da Fase 1.
- **Exclui:** física de movimento (Fase 4), gestos reais de mouse (Fase 3), animação (Fase 6).
- **Depende de:** Fase 1.
- **Critérios de aceitação:**
  1. Toda transição da tabela tem teste. [AUTO]
  2. Invariantes 1, 6 e 7 de ARCHITECTURE.md, seção 2.6, valem para milhares de sequências aleatórias de eventos. [AUTO]
  3. Sem movimento nem animação, nenhum `TICK` fica agendado. [AUTO]
  4. O app da Fase 1 continua funcionando com o núcleo ligado. [MANUAL]
- **Testes automatizados:** testes de unidade, testes de propriedade com sequências aleatórias e reproduções gravadas comparadas com um resultado de referência. [AUTO]

### Fase 3 — Input e arraste

- **Objetivo:** clique e arraste com prioridade absoluta do usuário.
- **Inclui:** arbitragem de gestos com o limiar de arraste do sistema lido por DPI; clique, clique duplo, botão direito e perda de captura; arraste manual, sem o loop modal de mover do Windows, a menos que P3 indique outra solução; validação ao soltar; menu de contexto mínimo; posicionamento no chão ao soltar sem apoio (a queda animada entra na Fase 4).
- **Exclui:** caminhada, escalada, pulo, queda com física, caixa de texto.
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
- **Testes automatizados:** reconhecedor de gestos (limiar por DPI, clique duplo, perda de captura, botão direito); invariantes 1 e 2 sob sequências aleatórias; validação ao soltar sobre as topologias de exemplo. [AUTO]

### Fase 4 — Movimento e superfícies

- **Objetivo:** caminhada, escalada, salto, queda, pouso e descanso determinísticos sobre as superfícies aprovadas em Q-05, em um monitor.
- **Inclui:** superfícies da área útil; estados `WALKING`, `CLIMBING`, `JUMPING`, `FALLING`, `LANDING` e `RESTING`; agenda autônoma com personalidade; interrupção por pressionar no meio de qualquer movimento; poses provisórias por estado.
- **Exclui:** passagem entre monitores e troca de escala em movimento (Fase 5); animações completas (Fase 6).
- **Depende de:** Fase 3; Q-05.
- **Critérios de aceitação:**
  1. Com a mesma semente, a mesma sequência de movimentos se repete. [AUTO]
  2. O personagem nunca fica sem apoio fora dos estados `JUMPING` e `FALLING`. [AUTO]
  3. Pressionar o personagem no meio de um pulo ou queda o segura na hora. [AUTO e MANUAL]
  4. Em `RESTING`, o relógio para e o consumo volta à linha de base. [AUTO e MANUAL, instrumentado]
  5. Gravando a tela a 120 quadros por segundo durante uma caminhada, a posição do personagem avança a cada quadro apresentado, sem quadro repetido nem salto maior que o passo esperado. Repetir em 60 Hz e em outra taxa de atualização disponível. [MANUAL, instrumentado]
- **Testes automatizados:** trajetórias de referência com passo fixo, colisões contra superfícies de exemplo, testes de propriedade de apoio. [AUTO]

### Fase 5 — Multi-monitor completo e posição persistida

- **Objetivo:** comportamento correto em qualquer topologia, com a posição restaurada entre execuções.
- **Inclui:** chave estável do monitor conforme P5; restauração com alternativas (ARCHITECTURE.md, seção 2.8); passagens entre monitores; troca de escala em movimento com histerese conforme P6; conexão, desconexão, rearranjo, troca de principal, rotação e troca de escala; barra de tarefas movida ou oculta automaticamente; recuperação se o Windows minimizar a janela ao desconectar um monitor; suspensão, retomada e bloqueio de sessão; **persistência mínima da posição** em `settings.json` com versão de esquema e gravação atômica.
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
- **Testes automatizados:** topologias S1 a S7 e restauração S9 com dados de exemplo; gravação atômica simulando falha entre gravar e substituir. [AUTO]
- **Observação:** drivers de monitor virtual podem simular parte do hardware. STATUS: UNCERTAIN; isso não substitui [HW].

### Fase 6 — Rendering, animações e expressões

- **Objetivo:** animações e expressões com asset provisório substituível, sem afetar o comportamento.
- **Inclui:** manifesto de assets; reprodutor de clipes; camadas ou variantes de expressão; asset provisório animado e original; redesenho só quando o quadro muda; máscara de clique por quadro; validação do manifesto no build.
- **Exclui:** arte definitiva, que é produzida separadamente.
- **Depende de:** Fase 5.
- **Critérios de aceitação:**
  1. A suíte do núcleo passa igual com dois manifestos diferentes. [AUTO]
  2. Manifesto com estado sem clipe ou expressão ausente falha no build. [AUTO]
  3. Nenhum quadro do asset tem pixel com alfa entre 1 e o limite definido pelo protótipo P1, fora de uma borda de dois pixels ao redor da silhueta. A verificação lê os arquivos de imagem e falha o build. [AUTO]
  4. Trocar expressão não muda estado nem posição. [AUTO]
  5. CPU e GPU com animação ficam dentro das metas de Q-08. [MANUAL, instrumentado]

### Fase 7 — Interação e caixa de texto

- **Objetivo:** reações a clique e conversa local pela caixa de texto.
- **Inclui:** reações a clique; janela da conversa, ativável e ancorada ao personagem; abertura por clique duplo ou menu (Q-07); foco, Esc, botão de fechar e fechamento por inatividade; limite de tamanho; motor local de respostas; conteúdo no idioma de Q-12; **arquivo de personalidade**, com os pesos descritos em ARCHITECTURE.md 2.11, já consumido pela agenda autônoma da Fase 4 e pelas frases desta fase.
- **Exclui:** LLM, API de IA, RAG, rede, voz, memória de conversa.
- **Depende de:** Fase 6; P4.
- **Critérios de aceitação:**
  1. Nenhuma tecla digitada na caixa muda estado ou posição do personagem. [AUTO e MANUAL]
  2. A digitação em outro aplicativo não é afetada enquanto a caixa está fechada ou sem foco. [MANUAL]
  3. Acentos e IME funcionam. [MANUAL]
  4. A mesma entrada com a mesma semente dá a mesma resposta. [AUTO]
  5. Trocar o arquivo de personalidade muda a frequência dos comportamentos autônomos e o conjunto de frases, sem alterar nenhuma transição da máquina de estados. A mesma suíte do núcleo passa com duas personalidades diferentes. [AUTO]
  6. O texto digitado não aparece em nenhum arquivo da pasta de dados nem em log. [AUTO e MANUAL com Process Monitor]
  7. Arrastar o personagem com a caixa aberta leva a caixa junto. [MANUAL]
  8. Ao fechar a caixa, o destino do foco segue o comportamento aprovado depois de P4. [MANUAL]

### Fase 8 — Configurações e persistência local

- **Objetivo:** preferências completas, persistidas de forma previsível e recuperável.
- **Inclui:** interface de configurações; esquema completo; migração de versão; opção de iniciar com o Windows desligada por padrão e ativada pelo usuário (Q-04); recuperação de arquivo corrompido; cópia `.bak`; comportamento em tela cheia condicionado ao resultado P7 (Q-09).
- **Depende de:** Fase 7 e do resultado P7 para decidir o custo do comportamento em tela cheia. As decisões Q-03, Q-04, Q-07 e Q-12 já estão registradas.
- **Critérios de aceitação:**
  1. Gravar e ler devolvem as mesmas configurações. [AUTO]
  2. Arquivo de versão anterior é migrado. [AUTO]
  3. Arquivo corrompido gera valores padrão, guarda uma cópia e o app continua. [AUTO]
  4. Matar o processo durante a gravação nunca deixa o arquivo ilegível. [AUTO]
  5. Iniciar com o Windows liga, desliga e respeita a desativação feita pelo usuário nas Configurações do Windows. [MANUAL]

### Fase 9 — Segurança e hardening

- **Objetivo:** confirmar na implementação cada regra de SECURITY.md.
- **Inclui:** revisão de SECURITY.md contra o código; portão de APIs e auditoria de dependências; sessão de 1 h sem nenhuma conexão de rede; gravações só na pasta do Buzzy; ausência de elevação; teste com entradas aleatórias no leitor de configurações e na caixa de texto; configuração segura do motor web, se houver; confirmar as condições do ZIP portátil pessoal e, se houver plano de distribuição pública, reabrir formato e assinatura (Q-10).
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

### Mudanças propostas em relação ao roadmap anterior

| Mudança | Motivo |
|---|---|
| A persistência mínima da posição entra na Fase 5. | O critério da Fase 5 exige posição persistida correta, mas a persistência só chegava na Fase 8. |
| A Fase 1 ganha sprite provisório estático, portão de APIs proibidas e medição de desempenho. | Sem sprite não há como verificar transparência e clique. Segurança e desempenho são medidos desde o início, não só nas Fases 9 e 11. |
| Toda fase tem critérios com marcador [AUTO], [MANUAL] ou [HW]. | Separar evidência automática, inspeção manual e dependência de hardware. |
| A Fase 11 termina reexecutando a regressão da Fase 10. | Otimização e acabamento podem quebrar o que já foi verificado. O usuário aceitou essa ordem em Q-11. |
| Etapa 0B com protótipos WPF P1, P2 e P3 antes da Fase 1. | A escolha WPF foi aceita, mas transparência por pixel, repouso e arraste ainda precisam de evidência no ambiente real. Q-13 resolvida. |

## MVP

O escopo do MVP está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md) e é coberto pelas Fases 1 a 11.

### Bugs

Nenhum. Não há código.

### Bloqueios

- A Fase 1 depende de P1–P3 aprovados, Fase 0 concluída e autorização explícita do usuário. As decisões Q-02 e Q-03 e o ZIP pessoal de Q-10 estão registrados; Q-08 será definida depois de P2. Q-01, Q-13 e Q-22 foram resolvidas.
- Verificações [HW] completas exigem dois monitores, sendo um capaz de outra escala ou orientação. Disponibilidade desse hardware: STATUS: UNCERTAIN; P3 tem uma verificação básica com um monitor e uma repetição pendente [HW].

## POST-MVP

STATUS: PLANNED apenas como lista; nada aqui tem data.

- Janelas de outros aplicativos como superfícies, fora do MVP por decisão Q-05.
- Toque e caneta, fora do MVP por decisão Q-14.
- Atalhos para controlar o personagem, fora do MVP por decisão Q-06.
- Mais idiomas, fora do MVP por decisão Q-12.
- Arte definitiva, animações adicionais e sons.
- Distribuição pela Microsoft Store.

## FUTURO

- IA opcional, fora do núcleo determinístico, conforme DEC-003. Só com decisão explícita do usuário.

## Fora do MVP

LLM, RAG, embeddings, vector database, APIs de IA, voz, reconhecimento de voz, backend, sincronização em nuvem, telemetria, analytics, atualização automática, execução arbitrária de comandos, shell genérico, keylogging, captura silenciosa de tela e controle genérico do computador.

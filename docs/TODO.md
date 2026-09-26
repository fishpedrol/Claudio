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

**Fase 0 — Descoberta e arquitetura. STATUS: PLANNED.** A proposta está parcial. DEC-006, ARCHITECTURE.md 2.13 e SECURITY.md 4 ainda são espaços reservados; a comparação de stack não pode ser considerada entregue. A Fase 0 não está pronta para revisão final nem concluída.

Nenhum código de produto é iniciado nesta fase.

### Pendências da Fase 0

- [x] Consolidar a visão do Buzzy e os limites do MVP em uma fonte de produto.
- [x] Separar a especificação estável do produto do prompt mestre variável por fase.
- [x] Definir o ciclo Claude/Fable → revisão independente do Codex → decisão do usuário.
- [x] Criar README com links para as fontes canônicas.
- [x] Preservar o trabalho documental anterior e indicar como recuperar a revisão de fundo iniciada por Claude/Fable.
- [x] Manter em PROJECT_CONTEXT.md um inventário resumido de todas as áreas do projeto, marcando o que ainda é planejado ou incerto.
- [ ] Confirmar a recuperação da revisão documental de fundo. O TODO anterior dizia que ela avaliou a versão de 2026-09-25, mas DEVELOPMENT_LOG.md ainda não contém esse resultado. Só concluir esta tarefa se a evidência estiver disponível e registrada.
- [x] 2026-09-26 — Inicializar Git local e `.gitignore` mínimo, sem remoto.
- [ ] Recuperar ou retomar o workflow existente `buzzy-stack-research`, pausado pelo limite de uso; usar seus resultados e evitar iniciar pesquisa duplicada. A captura mais recente mostra a etapa de verificação em 68/84.
- [ ] Completar a comparação de alternativas de stack com critérios, fontes e trade-offs e recomendar uma. DEC-006 ainda contém apenas um espaço reservado; Q-01 permanece UNCERTAIN até aprovação.
- [ ] Preencher o encaixe da recomendação na arquitetura (ARCHITECTURE.md 2.13) e as permissões por stack (SECURITY.md 4), mantendo incertezas e protótipos necessários explícitos.
- [x] 2026-09-26 — Propor arquitetura mínima, fluxo, máquina de estados, arbitragem de input, clique e arraste, foco da caixa de texto e ciclo de arraste. Proposta em ARCHITECTURE.md, STATUS: PLANNED.
- [x] 2026-09-26 — Propor modelo do desktop virtual, superfícies, DPI, reconexão e restauração. Proposta em ARCHITECTURE.md e DEC-008.
- [ ] Completar permissões e riscos específicos da stack em SECURITY.md 4; a seção ainda está vazia.
- [x] 2026-09-26 — Propor critérios de aceitação e testes por fase (este arquivo) e plano de medição de desempenho (DEC-011).
- [ ] Revisão do plano pelo Codex, conforme PLAN_REVIEW.md.
- [ ] Decisões do usuário Q-01 a Q-14, listadas em DECISIONS.md. Q-15 foi resolvida como questão operacional e não bloqueia a fase.
- [ ] Etapa 0B, protótipos de viabilidade, se o usuário aprovar Q-13.
- [ ] Registrar instruções de build e teste em PROJECT_CONTEXT.md quando a stack for aprovada e o projeto criado.

### Critério para concluir a Fase 0

- Comparação técnica e recomendação de stack registradas em DECISIONS.md, com evidências, riscos e alternativas. **Pendente: DEC-006 está vazio.**
- ARCHITECTURE.md descreve o desenho proposto como PLANNED, sem apresentá-lo como implementado. **Parcial: o desenho geral está registrado, mas o encaixe na stack em 2.13 está vazio.**
- Máquina de estados, fluxo de arraste, modelo do desktop virtual, segurança, persistência, testes e métricas têm critérios verificáveis. **Feito como proposta; as metas numéricas dependem de Q-08.**
- O Codex revisou o plano e as correções aceitas foram aplicadas.
- O usuário aprovou a stack (Q-01) e as decisões que bloqueiam a Fase 1: Q-02, Q-03, Q-10 e Q-13.
- A documentação e a seção Fase atual do prompt mestre descrevem a mesma próxima fase.
- O usuário decide iniciar a Fase 1.

Antes da revisão final, nenhum espaço reservado pode permanecer como se fosse um entregável concluído. Toda caixa `[x]` precisa corresponder a conteúdo verificável e registrado.

## Etapa 0B — Protótipos de viabilidade

STATUS: PLANNED. Só acontece se o usuário aprovar Q-13. O código é descartável, fica em uma pasta `spikes/` fora do produto e não segue para a Fase 1. Cada protótipo registra método, hardware e resultado no DEVELOPMENT_LOG.md.

| ID | Pergunta | Teste mínimo | Resultado esperado | Hardware |
|---|---|---|---|---|
| P1 | O clique atravessa os pixels transparentes? | Janela do tamanho do sprite na stack recomendada, com sprite sobre o Bloco de Notas. Clicar em pixel com alpha 0, com alpha 1 e no sprite. | Alpha 0 entrega o clique ao Bloco de Notas. Alpha 1 e o sprite entregam ao Buzzy. | 1 monitor |
| P2 | Quanto custa ficar parado e animar? | Sprite parado por 1 h sem timers. Depois, animação a 10 e a 60 quadros por segundo por 10 min cada. Medir CPU, memória, GPU e acordadas por segundo. | Parado: CPU próxima de zero e nenhuma acordada periódica vinda do app. Os números viram a base de Q-08. | 1 monitor |
| P3 | O arraste funciona sem roubar foco? | Janela que não ativa, com captura do mouse ao pressionar. Arrastar rápido para fora da janela e para outro monitor, soltar fora, usar Alt+Tab no meio, ligar ClickLock, digitar no Bloco de Notas antes e depois. | O arraste acompanha o cursor e termina ao soltar em qualquer lugar. O Bloco de Notas mantém o foco. Se falhar, testar as alternativas: margem de captura com alpha 1 durante o arraste, ativação só durante o arraste, ou loop modal de mover com animação por timer. | 2 monitores [HW] |
| P4 | A caixa de texto recebe foco e IME? | Abrir a caixa em resposta a clique duplo e, em outro teste, por timer. Digitar com acentos e com um IME. Fechar a caixa e ver para onde vai o foco. | Com clique, o foco chega. Por timer, o Windows nega o foco e a digitação continua no outro app. IME funciona. O destino do foco ao fechar fica registrado. | 1 monitor |
| P5 | Quais mensagens chegam quando a topologia muda, e a chave do monitor é estável? | Janela que registra mensagens e a leitura da topologia ao conectar, desconectar, rearranjar, girar, trocar o principal e trocar a escala. Reiniciar e trocar portas, comparando as chaves. Repetir com as opções do Windows 11 de lembrar posições e minimizar janelas ao desconectar. | Lista de mensagens por cenário, intervalo de agrupamento calibrado, estabilidade da chave confirmada ou refutada. | 2 monitores [HW] |
| P6 | Como a janela se comporta ao cruzar monitores de escalas diferentes? | Janela movida por código de um monitor a 100% para um a 200%, parando sobre a borda. | Ponto em que a escala troca, ausência de oscilação com histerese e pés sempre sobre o chão. | 2 monitores com escalas diferentes [HW] |
| P7 | É possível saber que há um app em tela cheia? | Consulta a cada 2 a 5 s com vídeo em tela cheia no navegador, jogo em janela sem borda e jogo em tela cheia exclusiva. | Estado correto nos três casos, com custo de CPU desprezível. O caso sem borda precisa ser confirmado. | 1 monitor |
| P8 | Riscos próprios da stack recomendada | Definido em DEC-006, conforme a stack. | Definido em DEC-006. | Conforme o caso |

Q-13 decide se P1 a P3 são pré-requisito para aprovar a stack. P4 a P8 podem acontecer no início das fases que dependem deles.

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
- **Depende de:** Fase 0 aprovada (Q-01, Q-02, Q-03, Q-10); P1 e P2 se Q-13 exigir.
- **Critérios de aceitação:**
  1. O app inicia e mostra o sprite sobre a área útil do monitor principal. [MANUAL]
  2. Clicar em pixel transparente dentro do retângulo da janela entrega o clique ao aplicativo de baixo. [MANUAL]
  3. Clicar no sprite não tira o foco do aplicativo ativo. [MANUAL]
  4. O Buzzy não aparece na barra de tarefas nem no Alt+Tab, se Q-03 confirmar. [MANUAL]
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
  5. O movimento parece contínuo em 60 Hz e em outra taxa de atualização disponível. [MANUAL]
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
  3. O asset não tem halo de alpha baixo acima de um limite definido nesta fase, para a área clicável acompanhar a silhueta. [AUTO]
  4. Trocar expressão não muda estado nem posição. [AUTO]
  5. CPU e GPU com animação ficam dentro das metas de Q-08. [MANUAL, instrumentado]

### Fase 7 — Interação e caixa de texto

- **Objetivo:** reações a clique e conversa local pela caixa de texto.
- **Inclui:** reações a clique; janela da conversa, ativável e ancorada ao personagem; abertura por clique duplo ou menu (Q-07); foco, Esc, botão de fechar e fechamento por inatividade; limite de tamanho; motor local de respostas; conteúdo no idioma de Q-12.
- **Exclui:** LLM, API de IA, RAG, rede, voz, memória de conversa.
- **Depende de:** Fase 6; P4.
- **Critérios de aceitação:**
  1. Nenhuma tecla digitada na caixa muda estado ou posição do personagem. [AUTO e MANUAL]
  2. A digitação em outro aplicativo não é afetada enquanto a caixa está fechada ou sem foco. [MANUAL]
  3. Acentos e IME funcionam. [MANUAL]
  4. A mesma entrada com a mesma semente dá a mesma resposta. [AUTO]
  5. O texto digitado não aparece em nenhum arquivo da pasta de dados nem em log. [AUTO e MANUAL com Process Monitor]
  6. Arrastar o personagem com a caixa aberta leva a caixa junto. [MANUAL]
  7. Ao fechar a caixa, o destino do foco segue o comportamento aprovado depois de P4. [MANUAL]

### Fase 8 — Configurações e persistência local

- **Objetivo:** preferências completas, persistidas de forma previsível e recuperável.
- **Inclui:** interface de configurações; esquema completo; migração de versão; iniciar com o Windows se Q-04 aprovar; recuperação de arquivo corrompido; cópia `.bak`.
- **Depende de:** Fase 7; Q-03, Q-04, Q-07, Q-09 e Q-12.
- **Critérios de aceitação:**
  1. Gravar e ler devolvem as mesmas configurações. [AUTO]
  2. Arquivo de versão anterior é migrado. [AUTO]
  3. Arquivo corrompido gera valores padrão, guarda uma cópia e o app continua. [AUTO]
  4. Matar o processo durante a gravação nunca deixa o arquivo ilegível. [AUTO]
  5. Iniciar com o Windows liga, desliga e respeita a desativação feita pelo usuário nas Configurações do Windows. [MANUAL]

### Fase 9 — Segurança e hardening

- **Objetivo:** confirmar na implementação cada regra de SECURITY.md.
- **Inclui:** revisão de SECURITY.md contra o código; portão de APIs e auditoria de dependências; sessão de 1 h sem nenhuma conexão de rede; gravações só na pasta do Buzzy; ausência de elevação; teste com entradas aleatórias no leitor de configurações e na caixa de texto; configuração segura do motor web, se houver; revisão de distribuição e assinatura (Q-10).
- **Critérios de aceitação:** todas as verificações da seção 8 de SECURITY.md passam e não há achado alto ou crítico aberto. [AUTO e MANUAL]

### Fase 10 — Verificação integrada

- **Objetivo:** confirmar o MVP inteiro em conjunto.
- **Inclui:** regressão automatizada completa; roteiro manual de aceitação do MVP nas versões do Windows de Q-02; matriz multi-monitor S1 a S12 [HW]; execução longa de 8 h com uso misto; instalação limpa em máquina virtual; correção dos problemas encontrados.
- **Critérios de aceitação:** todos os critérios das fases anteriores reexecutados e verdes; execução longa sem falha e sem crescimento contínuo de memória, conforme Q-08.

### Fase 11 — Performance e acabamento

- **Objetivo:** atingir as metas de Q-08 e fazer o acabamento visual e de uso.
- **Inclui:** perfilagem, otimização e acabamento.
- **Critérios de aceitação:** metas de Q-08 atingidas e medidas; regressão da Fase 10 reexecutada e verde depois do acabamento.
- **Observação:** a ordem entre as Fases 10 e 11 é a escolha em aberto Q-11.

### Mudanças propostas em relação ao roadmap anterior

| Mudança | Motivo |
|---|---|
| A persistência mínima da posição entra na Fase 5. | O critério da Fase 5 exige posição persistida correta, mas a persistência só chegava na Fase 8. |
| A Fase 1 ganha sprite provisório estático, portão de APIs proibidas e medição de desempenho. | Sem sprite não há como verificar transparência e clique. Segurança e desempenho são medidos desde o início, não só nas Fases 9 e 11. |
| Toda fase tem critérios com marcador [AUTO], [MANUAL] ou [HW]. | Separar evidência automática, inspeção manual e dependência de hardware. |
| A Fase 11 termina reexecutando a regressão da Fase 10. | Otimização e acabamento podem quebrar o que já foi verificado. A alternativa de inverter as fases está em Q-11. |
| Etapa 0B de protótipos antes da Fase 1. | Algumas afirmações técnicas só se confirmam em execução. Decisão em Q-13. |

## MVP

O escopo do MVP está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md) e é coberto pelas Fases 1 a 11.

### Bugs

Nenhum. Não há código.

### Bloqueios

- A Fase 1 depende da aprovação da stack (Q-01) e das escolhas Q-02, Q-03, Q-10 e Q-13.
- Verificações [HW] exigem pelo menos dois monitores, sendo um capaz de outra escala ou orientação. Disponibilidade desse hardware: STATUS: UNCERTAIN.

## POST-MVP

STATUS: PLANNED apenas como lista; nada aqui tem data.

- Janelas de outros aplicativos como superfícies, se Q-05 as deixar fora do MVP.
- Toque e caneta, se Q-14 os deixar fora do MVP.
- Atalhos de teclado, se Q-06 os deixar fora do MVP.
- Mais idiomas, se Q-12 escolher apenas um.
- Arte definitiva, animações adicionais e sons.
- Distribuição pela Microsoft Store.

## FUTURO

- IA opcional, fora do núcleo determinístico, conforme DEC-003. Só com decisão explícita do usuário.

## Fora do MVP

LLM, RAG, embeddings, vector database, APIs de IA, voz, reconhecimento de voz, backend, sincronização em nuvem, telemetria, analytics, atualização automática, execução arbitrária de comandos, shell genérico, keylogging, captura silenciosa de tela e controle genérico do computador.

# Prompt mestre dinâmico — Buzzy

Use este prompt para conduzir Claude/Fable no Buzzy. As regras abaixo são estáveis; a seção **Fase atual** é variável e deve ser atualizada por Codex quando o usuário mudar de fase ou aceitar uma decisão que altere o trabalho. A visão do produto continua em docs/PRODUCT_SPEC.md; não copie a especificação inteira para este prompt.

## Papéis

- **Claude, no modelo escolhido pelo usuário (Fable ou Opus):** prepara o plano da fase atual e implementa somente o escopo que o usuário autorizou.
- **Codex:** segundo olhar independente. Compara o plano de Claude com a visão do produto, os documentos e o estado real; aponta inconsistências, riscos e lacunas; propõe um plano corrigido e mantém o contexto operativo atualizado.
- **Usuário:** decide mudanças de produto e quando iniciar ou avançar fases. Quando delegar explicitamente uma decisão ao Codex, essa decisão será registrada nos documentos com os critérios e limites usados.

Claude, independentemente de o usuário escolher Fable ou Opus, é uma ferramenta de desenvolvimento; isso não significa IA dentro do aplicativo. O MVP continua sem IA integrada.

## Fontes obrigatórias

Leia AGENTS.md e CLAUDE.md, depois docs/PROJECT_CONTEXT.md, docs/PRODUCT_SPEC.md, docs/TODO.md, docs/ARCHITECTURE.md, docs/DECISIONS.md, docs/SECURITY.md, docs/DEVELOPMENT_LOG.md e docs/PLAN_REVIEW.md.

Inspecione os arquivos reais antes de afirmar o que está implementado. PRODUCT_SPEC.md registra a intenção estável; TODO.md é a lista operacional única de fases e pendências; PROJECT_CONTEXT.md registra o estado atual; este prompt registra a tarefa da fase vigente.

Se documentos divergirem, cite a divergência e seu impacto. A instrução mais recente do usuário prevalece sobre decisões antigas. Não trate plano como implementação nem hipótese como decisão aceita.

## Ciclo de trabalho

1. **Planejar:** Claude/Fable propõe um plano somente para a fase atual, com objetivo, escopo incluído e excluído, dependências, arquivos afetados, riscos, critérios de aceitação, testes e alterações documentais.
2. **Revisar:** Codex aplica docs/PLAN_REVIEW.md, preserva os pontos corretos, aponta problemas e apresenta uma versão corrigida. Revisão de plano não implementa código.
3. **Decidir:** o usuário avalia o plano e autoriza ou altera o trabalho.
4. **Executar:** Claude/Fable implementa somente a fase autorizada.
5. **Verificar:** executar build, testes e verificações manuais pertinentes; comparar cada resultado com os critérios de aceitação.
6. **Sincronizar:** atualizar a documentação e a seção Fase atual deste prompt; parar no gate da fase e aguardar a decisão do usuário.

Uma fase não é concluída apenas porque compila ou porque suas tarefas foram marcadas. Se um critério falhar, registre o bloqueio, corrija o que couber na fase e repita a verificação.

## Regras de produto

Siga integralmente docs/PRODUCT_SPEC.md. Em especial:

- Buzzy é um mascote moderno e original para desktop Windows, inspirado na experiência dos mascotes clássicos; não copie personagem, nome, visual, voz, marca ou conteúdo existente.
- A ação direta do usuário tem prioridade máxima. Durante DRAGGING, interrompa movimento autônomo incompatível; acompanhe o cursor; ao soltar, valide posição, monitor e superfície.
- Considere desktop virtual, disposições arbitrárias de monitores, coordenadas negativas, resoluções, orientação, DPI, reconexão e persistência.
- Movimento e conversa do MVP funcionam localmente, sem IA integrada.
- Não adicione LLM, RAG, embeddings, APIs de IA, voz, backend, nuvem, telemetria, analytics, auto-updater, comandos arbitrários, shell/PowerShell/CMD arbitrários, keylogging, captura silenciosa de tela ou controle genérico do computador.
- Segurança e performance entram no desenho desde a Fase 0. Não crie complexidade futura sem necessidade demonstrada pelo MVP.
- Quando a tarefa envolver desenho, animação ou assets do personagem, abra as duas referências em docs/PRODUCT_SPEC.md. São pontos de partida para uma proposta original melhor, não especificações literais nem arte aprovada. Preserve os limites de produto e apresente a nova proposta ao usuário antes de tratá-la como final.

## Revisão do plano

Considere como plano do projeto a documentação canônica, principalmente TODO.md, ARCHITECTURE.md e DECISIONS.md. Não invente um plano separado de Claude/Fable. Se o usuário fornecer outro plano, leia-o por inteiro e compare-o também.

Use docs/PLAN_REVIEW.md. Separe fatos do repositório, requisitos, inferências e recomendações. Para cada problema, informe gravidade, fonte, impacto, correção sugerida e critério de verificação. Entregue resumo executivo, problemas críticos/importantes, pontos bons, lacunas, análise de segurança/performance/arquitetura, ordem corrigida e plano corrigido.

Use a skill code-review somente quando houver um diff útil e uma base de comparação identificável. Uma revisão de plano pode e deve ser feita sem Git; não invente uma base nem trate ausência de diff como falha do plano.

Não escolha tecnologia por preferência ou popularidade. Compare alternativas com evidências técnicas atuais, critérios do produto, riscos e trade-offs. Se o usuário delegar a escolha ao Codex, escolha a opção que melhor atende ao projeto completo, explique o motivo e registre os portões de reversão. Sem delegação explícita, apresente opções concisas e aguarde o usuário.

## Estado do gate

Ao terminar uma fase, registre um resultado de gate: **PASS**, **FAIL** ou **BLOCKED**. Esse resultado descreve a revisão da fase e não substitui os status documentais do projeto:

- STATUS: VERIFIED — implementado e testado; os testes pertinentes foram executados.
- STATUS: PLANNED — planejado, ainda não verificado.
- STATUS: UNCERTAIN — escolha ou estado indefinido.

PASS exige critérios aceitos verificados, build e testes executados, comportamento conferido no ambiente apropriado, ausência de achados críticos/altos abertos, compatibilidade com fases anteriores e documentação sincronizada. Diferencie teste automatizado, verificação manual e limitações do ambiente. Não use VERIFIED sem evidência.

## Sincronização documental ao concluir uma fase

Siga AGENTS.md:

1. docs/PROJECT_CONTEXT.md;
2. docs/DEVELOPMENT_LOG.md;
3. docs/ARCHITECTURE.md, se o desenho mudou;
4. docs/DECISIONS.md, se uma decisão mudou;
5. docs/SECURITY.md, se o modelo de segurança mudou;
6. docs/TODO.md;
7. atualize **Fase atual** neste prompt para a próxima fase somente após o gate e a decisão do usuário;
8. confira todas as afirmações contra o estado real.

Registre decisões substituídas sem apagá-las. Não mantenha roadmap ou regras duplicados em arquivos paralelos.

## Fase atual — seção variável

- **Data da atualização:** 2026-09-26.
- **Fase:** Fase 0 — Descoberta e arquitetura, Etapa 0B — Viabilidade WPF.
- **Status:** STATUS: PLANNED.
- **Objetivo:** executar protótipos descartáveis na stack escolhida antes de construir o shell do Buzzy.
- **Estado observado:** existe documentação e Git com remoto `origin`; ainda não há código do aplicativo, build nem teste. Confirme novamente antes de depender desse estado.
- **Stack:** WPF com C# e .NET 10 LTS, aceita por delegação explícita do usuário e registrada em DEC-006. Não declare a stack validada antes de P1–P3.
- **Escopo deste ciclo:** concluir apenas P1 (clique por pixel), P3 (arraste sem roubo de foco) e P2 (medição em repouso e animação), nessa ordem, em código descartável sob `spikes/`. Não iniciar a Fase 1 nem criar funcionalidades do produto.
- **Visual:** as duas imagens em `assets/references/` são referências, não especificações. Depois de entregar os protótipos técnicos, Claude pode apresentar uma proposta conceitual preliminar e separada para o personagem. Não alterar nem adicionar assets, não tratar a proposta como aprovada e não deixar o trabalho visual atrasar os protótipos.

### Critério de conclusão da Etapa 0B

Os três protótipos devem ter método, versão do Windows, hardware, evidência observável e resultado registrados. P1 e P3 precisam passar para recomendar a continuação com WPF. P2 informa a linha de base e as metas Q-08; se o resultado for ruim, primeiro corrigir o ciclo de renderização e medir de novo. Falha de P1 ou P3 suspende a Fase 1 e devolve a stack ao Codex para revisão.

**Próxima ação permitida:** Claude lê as fontes canônicas, confere se já existe trabalho parcial e continua sem sobrescrever resultados, executa somente P1, P3 e P2 conforme `prompt_usuario.md`, e sincroniza os documentos com evidências reais. Depois dos protótipos, pode incluir uma proposta conceitual visual curta no relatório, sem criar nem alterar assets. Claude então para. Codex revisa protótipos, resultados, proposta e documentos. Q-08 será proposta a partir da medição P2; nenhuma resposta já registrada deve ser perguntada novamente. Não avançar à Fase 1 sem fechamento da Fase 0 e autorização do usuário.

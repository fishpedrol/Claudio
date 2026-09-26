# DEVELOPMENT_LOG.md — Registro cronológico

> Entradas cronológicas sobre mudanças relevantes. Cada entrada registra objetivo, alterações, verificações e pendências. Não declare teste executado se ele não foi executado.

## 2026-09-25 — Fase 0: Bootstrap da documentação

**Objetivo:** criar documentação viva antes do código.

**Alterações realizadas:**

- Criados PROJECT_CONTEXT.md, ARCHITECTURE.md, DEVELOPMENT_LOG.md, DECISIONS.md, SECURITY.md e TODO.md.
- Criado CLAUDE.md com leitura obrigatória do contexto do projeto e sincronização documental.
- Registrada DEC-001 sobre documentação viva.

**Testes executados:** nenhum. Não havia código ou testes.

**Problemas encontrados:** não havia código, Git, objetivo, escopo ou stack registrados nos documentos criados naquele momento.

**Problemas corrigidos:** estrutura inicial de documentação criada.

**Pendências:** reconciliar a documentação inicial com o contexto do Buzzy e concluir a Fase 0 antes de iniciar o shell desktop.

## 2026-09-26 — Fase 0: Alinhamento do produto e revisão contínua dos planos

**Objetivo:** tornar a visão do Buzzy a referência de produto, separar essa referência do estado da fase e estabelecer uma revisão independente do plano preparado pelo agente de implementação.

**Alterações realizadas:**

- Atualizados PROJECT_CONTEXT.md, ARCHITECTURE.md, DECISIONS.md, SECURITY.md e TODO.md para refletirem o Buzzy e o próximo passo correto: Fase 0.
- Criados PRODUCT_SPEC.md e PLAN_REVIEW.md como referências estáveis.
- Criado docs/PLAN_REVIEW.md e definido context codex/PROMPT_MESTRE_BUZZY.md como instrução variável por fase; context codex/context.md passa a ser um índice e HANDOFF.md uma ponte de compatibilidade.
- Definido o papel de Claude no Fable, usando o modelo escolhido pelo usuário, como agente de planejamento e implementação; Codex faz a revisão independente e o usuário decide.
- Mantida a stack como indefinida até a comparação técnica da Fase 0.
- Ampliado PROJECT_CONTEXT.md com um inventário resumido por área, preservando a exigência inicial de orientar um agente novo sem conversa prévia.
- Atualizado prompt_usuario.md para continuar o trabalho documental anterior e recuperar a revisão de fundo mencionada no histórico, se ela ainda estiver disponível.

**Testes executados:** nenhum. Esta alteração é documental e não havia código para testar.

**Verificação realizada:** conferidos o inventário de arquivos e a coerência entre visão, escopo, fase atual, arquitetura planejada, segurança e prompt mestre variável.

**Problemas encontrados:** o contexto anterior dizia que objetivo e escopo eram desconhecidos, embora o handoff descrevesse o produto. O TODO indicava Fase 1, enquanto o handoff apontava Fase 0. O documento de contexto também misturava especificação, roadmap e prompt operacional. O histórico do Fable informa que uma revisão adversarial foi iniciada em segundo plano, mas o texto recebido não contém seus resultados.

**Problemas corrigidos:** fontes documentais separadas, próximas ações alinhadas ao contexto do Buzzy e inventário de estado resumido no PROJECT_CONTEXT.md. O prompt de continuação orienta a preservar os arquivos e recuperar a revisão anterior antes de repeti-la.

**Pendências:** recuperar ou repetir de forma não concorrente a revisão documental anterior, concluir a comparação de stack, definir a arquitetura de Fase 0, registrar critérios verificáveis para cada fase e, só então, iniciar Fase 1.

## 2026-09-26 — Referência visual do personagem Buzzy

**Objetivo:** incluir a imagem de conceito fornecida pelo usuário no projeto e torná-la utilizável por Claude/Fable como referência de direção visual.

**Alterações realizadas:**

- Copiada a imagem para `assets/references/buzzy-character-concept.png`.
- Atualizado PRODUCT_SPEC.md com a referência, seus traços visuais gerais e limites de interpretação: não é sprite sheet final, e o cursor ilustrado não faz parte do personagem.
- Atualizado PROMPT_MESTRE_BUZZY.md para que tarefas futuras de personagem/arte consultem a referência canônica em PRODUCT_SPEC.md.

**Verificações realizadas:** confirmada a existência do arquivo copiado (2.332.894 bytes) e conferido o link relativo da especificação. Nenhum teste de aplicação foi executado; não há código de produto nesta alteração.

**Pendências:** Claude/Fable deve considerar a referência no trabalho em andamento sem substituir o planejamento ou descartar decisões documentadas. Detalhes definitivos de arte e animação dependem da aprovação do usuário.

## 2026-09-26 — Revisão parcial da proposta da Fase 0

**Objetivo:** conferir se os entregáveis registrados no roadmap existem nos documentos antes da revisão final da Fase 0.

**Problemas encontrados:** TODO.md marcava a comparação de stack e as permissões por stack como concluídas, mas DEC-006, ARCHITECTURE.md 2.13 e SECURITY.md 4 ainda eram espaços reservados. TODO.md também dizia que uma revisão adversarial anterior havia sido recuperada, sem resultado correspondente no histórico.

**Alterações realizadas:** corrigidos os estados da Fase 0 e as tarefas pendentes em TODO.md e PROJECT_CONTEXT.md; atualizados README.md e PROMPT_MESTRE_BUZZY.md para refletir a continuação; registrada a resolução operacional de Q-15, pois o usuário confirmou que já enviou o pedido inicial ao Fable e limpou o arquivo; preparado um novo prompt de continuidade em prompt_usuario.md. Os documentos passam a dizer Claude no Fable com o modelo escolhido pelo usuário, incluindo Opus, sem fixar uma versão.

**Verificações realizadas:** inspeção dos documentos canônicos e do diff local. Nenhum teste de aplicação foi executado; não há código de produto.

**Próximos passos:** Claude/Fable deve concluir apenas as seções ausentes da proposta e confirmar se há evidência recuperável da revisão anterior. Depois, Codex fará a revisão integral da Fase 0. Nenhuma escolha de produto ou de stack foi aprovada.

### Atualização — workflow de pesquisa ainda ativo

O usuário mostrou que o workflow `buzzy-stack-research` continua pausado pelo limite de uso; a captura exibe Pesquisa 14/14 e Verificação 68/84. PROJECT_CONTEXT.md, TODO.md e o prompt de continuidade foram atualizados para instruir Claude a retomar ou recuperar esse mesmo workflow e usar os resultados existentes antes de preencher DEC-006. A pesquisa não deve ser duplicada. Esta é uma atualização de estado baseada na captura fornecida pelo usuário, não uma confirmação de que o workflow terminou.

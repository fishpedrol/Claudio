# Instruções para agentes — Buzzy

## Fontes do projeto

Leia docs/PROJECT_CONTEXT.md antes de qualquer trabalho. Leia docs/PRODUCT_SPEC.md antes de planejar ou alterar comportamento do produto.

Para um plano ou revisão de plano, leia context codex/PROMPT_MESTRE_BUZZY.md, TODO.md, ARCHITECTURE.md, DECISIONS.md e PLAN_REVIEW.md. Leia SECURITY.md antes de decisões sobre permissões, dados, processos ou rede. Inspecione os arquivos reais antes de afirmar o estado de implementação.

PRODUCT_SPEC.md define a intenção estável. PROJECT_CONTEXT.md define o estado atual. TODO.md define a fase e as tarefas. context codex/PROMPT_MESTRE_BUZZY.md define a missão operativa variável. Nenhum plano de agente substitui decisão explícita do usuário.

## Papéis e ciclo de trabalho

Claude/Fable 5.1 prepara o plano da fase e implementa apenas o escopo autorizado pelo usuário. Codex revisa o plano de forma independente contra o produto, a fase, as decisões e o estado real; apresenta riscos, lacunas e correções. O usuário decide se a implementação começa e quando uma fase avança.

Durante revisão de plano, não implemente. Durante implementação autorizada, conclua uma fase por vez e não introduza IA integrada ao MVP.

## Documentação viva

Ao concluir cada fase, sincronize nesta ordem:

1. docs/PROJECT_CONTEXT.md
2. docs/DEVELOPMENT_LOG.md
3. docs/ARCHITECTURE.md, se o desenho ou a arquitetura mudou
4. docs/DECISIONS.md, se houve decisão relevante
5. docs/SECURITY.md, se o modelo de segurança mudou
6. docs/TODO.md
7. context codex/PROMPT_MESTRE_BUZZY.md, apontando para a fase seguinte
8. Confira cada afirmação contra os arquivos reais e os resultados executados

Cada documento começa com o formato das suas próprias entradas; siga-o. Mantenha cada fato em uma fonte canônica e use links em vez de duplicar o conteúdo.

## Veracidade

Toda funcionalidade, teste ou decisão registra status:

- STATUS: VERIFIED — implementado e testado; os testes correspondentes foram executados.
- STATUS: PLANNED — planejado e ainda não verificado.
- STATUS: UNCERTAIN — escolha ou estado ainda indefinido.

Não declare fase concluída só porque o código compila. Cada fase exige build, testes, correção de erros, verificação do comportamento e documentação sincronizada.

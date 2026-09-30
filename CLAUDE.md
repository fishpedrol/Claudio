# Instruções para Claude

Leia AGENTS.md e siga as fontes e o fluxo descritos ali. PROJECT_CONTEXT.md traz o estado atual; PRODUCT_SPEC.md traz os requisitos estáveis; prompt_usuario.md traz a missão atual; context codex/PROMPT_MESTRE_BUZZY.md contém regras estáveis.

Para uma revisão independente solicitada pelo usuário, use docs/PLAN_REVIEW.md e não implemente durante a revisão. A autorização contínua de implementação e progressão de fases está registrada em docs/DECISIONS.md (DEC-015) e prompt_usuario.md; não peça uma nova aprovação de fase ou de plano. Siga os gates técnicos, teste, corrija e atualize a documentação antes de avançar.

**Regra do usuário — backup de continuidade:** mantenha `BACKUP_CLAUDE.md`, na raiz, atualizado periodicamente durante todo trabalho: ao concluir cada etapa relevante (teste executado, módulo escrito, decisão tomada, falha encontrada) e, em trabalho longo, pelo menos a cada cerca de 30 minutos. Ele é o backup para o Codex continuar se o limite do usuário acabar: tarefa atual, o que foi feito com evidência, o que está em andamento, próximos passos em ordem, decisões ainda não registradas nos documentos canônicos e armadilhas conhecidas.

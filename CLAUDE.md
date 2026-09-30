# Instruções para Claude

Leia AGENTS.md e siga as fontes e o fluxo descritos ali. PROJECT_CONTEXT.md traz o estado atual; PRODUCT_SPEC.md traz os requisitos estáveis; prompt_usuario.md traz a missão atual; docs/PROMPT_MESTRE_BUZZY.md contém regras estáveis.

Para uma revisão independente solicitada pelo usuário, use docs/PLAN_REVIEW.md e não implemente durante a revisão. A autorização contínua de implementação e progressão de fases está registrada em docs/DECISIONS.md (DEC-015) e prompt_usuario.md; não peça uma nova aprovação de fase ou de plano. Siga os gates técnicos, teste, corrija e atualize a documentação antes de avançar.

**Regra do usuário — handoff compartilhado:** mantenha `CONTINUIDADE.md`, na raiz, como o único backup e registro de comunicação entre Claude e Codex. Atualize-o em marcos relevantes (teste, módulo, decisão ou falha) e, em trabalho longo, ao menos a cada cerca de 30 minutos. Registre estado, evidências, limitações e próximo passo em ordem; decisões estáveis ficam nas fontes canônicas e são referenciadas por link.

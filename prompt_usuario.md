# Diretiva atual — Buzzy

Atualizada em 2026-10-02. **Claude é o desenvolvedor principal; o Codex é o segundo desenvolvedor e o revisor, quando o usuário pedir** (DEC-015). O trabalho a dois na mesma árvore segue o protocolo de [AGENTS.md](AGENTS.md). O estado atual e as próximas ações estão em [CONTINUIDADE.md](CONTINUIDADE.md), seção "Retomada atual", e em [docs/PROJECT_CONTEXT.md](docs/PROJECT_CONTEXT.md).

## Para Claude — desenvolvedor principal

Implemente, teste, corrija e documente o MVP seguindo as fases e dependências de `docs/TODO.md`, sem pedir aprovação rotineira de plano ou avanço; a autorização contínua está em DEC-015. Tome decisões técnicas reversíveis dentro de `docs/PRODUCT_SPEC.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md` e `docs/SECURITY.md`; registre decisões materiais na fonte canônica.

Ao retomar, comece pelas "Próximas ações em ordem" do `CONTINUIDADE.md`. Peça ajuda apenas diante de uma ação exclusivamente humana indispensável, permissão externa não concedida ou decisão de produto fora da especificação; continue todo o trabalho independente. Não publique nem distribua o aplicativo.

## Para Codex — segundo desenvolvedor e revisor

Você trabalha quando o usuário pede, num de dois modos; o pedido dele diz qual. Fale com ele em português.

### Modo desenvolvedor: o usuário te dá uma tarefa

1. **Contexto.** Leia `AGENTS.md`; no `CONTINUIDADE.md`, a "Retomada atual" e o "Em andamento"; `docs/PROJECT_CONTEXT.md`, `docs/PRODUCT_SPEC.md` e `docs/TODO.md`; e as seções de `docs/ARCHITECTURE.md`, `docs/DECISIONS.md` e `docs/SECURITY.md` que a tarefa toca.
2. **Base.** Confira o `git status` e rode `powershell -NoProfile -File tools\testar.ps1`. Pronto quando a base estiver verde (código 0), ou quando você tiver relatado ao usuário o que já estava vermelho antes de você.
3. **Registro.** Anote a tarefa em "Em andamento" no `CONTINUIDADE.md`, como manda o protocolo do `AGENTS.md`.
4. **Implementação.** Teste primeiro (vermelho → verde), no estilo do código ao redor. Para uma decisão de produto fora da especificação, pergunte ao usuário.
5. **Verificação.** Rode `tools\testar.ps1`, e também com `-Integracao` quando tocar no app, sem nenhum Buzzy aberto. Pronto quando o resultado for código 0 e cada teste novo tiver sido visto falhando antes da correção.
6. **Documentação.** Sincronize na ordem do `AGENTS.md`, com STATUS VERIFIED só onde houver evidência executada.
7. **Fechamento.** Tire a tarefa de "Em andamento", acrescente uma entrada ao log do `CONTINUIDADE.md` (o que mudou, evidência, limitações e próximo passo) e resuma ao usuário.

### Modo revisor: o usuário pede uma revisão

Siga `docs/PLAN_REVIEW.md`. O resultado é um relatório para o usuário, com os achados do mais grave ao menos grave e a evidência de cada um; a árvore fica como você a encontrou.

### Nos dois modos

Avise o usuário antes de qualquer verificação que abra janelas ou mova o cursor. Os commits são do usuário.

## Produto e evidência (para os dois)

- Siga a identidade aprovada em DEC-018/DEC-019 e `docs/IDENTIDADE_VISUAL.md`: pixel art fiel às pranchas, com o chapéu de palha.
- Diferencie teste automatizado, input sintético, observação informal, verificação manual e [HW]; registre cada funcionalidade, teste ou decisão como VERIFIED, PLANNED ou UNCERTAIN. Build sozinho não fecha fase.
- O Buzzy é um mascote local e não verbal: sem chat, texto conversacional, voz, IA integrada, rede ou outras capacidades vetadas por `docs/SECURITY.md`.

## Handoff entre agentes

`CONTINUIDADE.md` é o único backup e registro compartilhado entre Claude e Codex. Confira o checkout antes de confiar no snapshot. A cada marco, teste, falha ou decisão material, atualize ali o estado resumido, o próximo passo e a evidência; detalhes estáveis ficam nas fontes canônicas.

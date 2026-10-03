# Diretiva atual — Buzzy

Atualizada em 2026-10-02. Papéis e regras: [AGENTS.md](AGENTS.md). Estado e próximas ações: [CONTINUIDADE.md](CONTINUIDADE.md).

## Claude — desenvolvedor principal

Implemente, teste, corrija e documente o MVP pelas fases de `docs/TODO.md`, começando pelas "Próximas ações em ordem" do CONTINUIDADE.md.

## Codex — segundo desenvolvedor e revisor

Você trabalha quando o usuário pede, num de dois modos; o pedido dele diz qual.

### Modo desenvolvedor: o usuário te dá uma tarefa

1. **Contexto:** a leitura de abertura do AGENTS.md, mais as seções de ARCHITECTURE, DECISIONS e SECURITY que a tarefa toca.
2. **Base:** `git status` e `powershell -NoProfile -File tools\testar.ps1`. Pronto quando a base estiver verde (código 0) ou quando você tiver relatado ao usuário o que já estava vermelho antes de você.
3. **Registro:** a tarefa em "Em andamento" no CONTINUIDADE.md.
4. **Implementação:** teste primeiro (vermelho → verde), no estilo do código ao redor.
5. **Verificação:** `tools\testar.ps1`, e com `-Integracao` quando tocar no app, sem nenhum Buzzy aberto. Pronto quando sair código 0 e cada teste novo tiver sido visto falhando antes da correção.
6. **Documentação:** sincronizada como manda o AGENTS.md.
7. **Fechamento:** tire a tarefa de "Em andamento", registre no log do CONTINUIDADE.md o que mudou, a evidência, as limitações e o próximo passo, e resuma ao usuário.

### Modo revisor: o usuário pede uma revisão

Siga `docs/PLAN_REVIEW.md`. O resultado é um relatório para o usuário, com os achados do mais grave ao menos grave e a evidência de cada um; a árvore fica como você a encontrou.

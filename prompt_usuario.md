# Diretiva atual para Claude — Buzzy

Atualizada em 2026-09-30. Esta diretiva substitui divisões de trabalho anteriores entre Claude e Codex e prompts que mandavam aguardar. O usuário pediu que Claude retome a implementação do que falta.

## Mandato

Implemente, teste, corrija e documente o MVP seguindo as fases e dependências de `docs/TODO.md`, sem pedir aprovação rotineira de plano ou avanço. A autorização contínua está em DEC-015 e continua válida. Tome decisões técnicas reversíveis dentro de `docs/PRODUCT_SPEC.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md` e `docs/SECURITY.md`; registre decisões materiais na fonte canônica.

Peça ajuda apenas diante de uma ação exclusivamente humana indispensável, permissão externa não concedida ou decisão de produto fora da especificação. Continue todo o trabalho independente. Não publique nem distribua o aplicativo.

## Retomada imediata — Fase 2

`docs/TODO.md` e `docs/PROJECT_CONTEXT.md` ainda dizem que a ligação do núcleo ao app está pendente. A inspeção de 2026-09-30 encontrou uma integração com `Nucleo` em `src/Buzzy.App/Composicao/Aplicacao.cs` e expectativas correspondentes em `tests/Buzzy.App.Testes/Integracao/IntegracaoTestes.cs`. Considere essa integração **presente, mas não verificada** até conferir o checkout atual e executar as verificações.

1. Leia `AGENTS.md`, `CLAUDE.md`, `docs/PROJECT_CONTEXT.md`, `docs/PRODUCT_SPEC.md`, `docs/TODO.md` e `docs/SECURITY.md`. Consulte arquitetura e decisões pertinentes, o histórico de desenvolvimento e `CONTINUIDADE.md`. Confira `git status`, diffs, código e resultados atuais; preserve alterações existentes.
2. Inspecione a composição e os testes da Fase 2. Execute build e verificações automatizadas do projeto; corrija falhas e repita as verificações afetadas. Não refaça uma integração antes de entender a que já está no checkout.
3. Avise o usuário antes de verificações que abrem janelas ou movem o cursor. Verifique o critério manual da Fase 2 quando possível. Se não for possível, registre-o como pendente, sem PASS/VERIFIED, e continue o trabalho independente.
4. Ao fechar o gate da Fase 2, sincronize a documentação na ordem de `AGENTS.md`, atualize o estado do TODO com evidências atuais e prossiga para a Fase 3 sem aguardar confirmação. Continue as fases seguintes na ordem do roadmap.

As pendências manuais da Fase 1 — menu real da bandeja, escalas de 150%/200% e mudanças reais de resolução/escala/barra de tarefas — permanecem abertas. Não altere configurações globais para testá-las; registre-as como pendentes e avance nas fases que não dependam delas, retornando na integração final.

## Produto, segurança e evidência

- `docs/PRODUCT_SPEC.md` define o produto; `docs/TODO.md`, as fases e gates; `docs/PROJECT_CONTEXT.md`, o estado atual; `docs/DECISIONS.md`, decisões; `docs/ARCHITECTURE.md`, desenho; e `docs/SECURITY.md`, limites. Essas fontes e os arquivos reais prevalecem sobre backups antigos.
- Siga a identidade aprovada em DEC-018/DEC-019 e `docs/IDENTIDADE_VISUAL.md`, incluindo a pixel art e o chapéu de palha. Não use a direção vetorial antiga nem a orientação anterior que contradiz essas decisões.
- Diferencie testes automatizados, input sintético, observação informal, verificação manual e [HW]. Registre cada funcionalidade, teste ou decisão como VERIFIED, PLANNED ou UNCERTAIN. Build sozinho não fecha fase.
- Preserve o trabalho existente. Não leia conteúdo de outros aplicativos, não encerre processos do usuário, não altere configurações globais do Windows para simular hardware, e não introduza chat, texto conversacional, voz, IA integrada, rede ou capacidades proibidas.
- Ao concluir cada fase, siga a ordem documental de `AGENTS.md` e confira cada afirmação contra os arquivos e resultados executados.

## Handoff entre agentes

Use `CONTINUIDADE.md` como o único backup e registro compartilhado entre Claude e Codex. Confira o checkout antes de confiar no snapshot; após cada marco, teste, falha ou decisão material, atualize ali o estado resumido, o próximo passo e a evidência, deixando detalhes estáveis nas fontes canônicas. Não faça commit automaticamente.

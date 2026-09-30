# Continuidade compartilhada — Buzzy

> Registro único de handoff e backup para Claude e Codex. Atualizado em 2026-09-30.
> As fontes canônicas em `docs/`, o código e as instruções atuais do usuário prevalecem. Este arquivo registra o estado de trabalho entre sessões; confirme tudo no checkout antes de agir.

## Retomada atual

O usuário pediu que Claude volte a implementar o que falta. A diretiva está em [prompt_usuario.md](prompt_usuario.md); a autorização contínua de Claude está em DEC-015. A divisão anterior que deixava a Fase 2 com Codex foi substituída.

**Próximo trabalho: validar e fechar a Fase 2.** O núcleo puro e a integração com o app precisam ser distinguidos:

- `docs/TODO.md` marca o núcleo puro implementado, as referências 02–05 revisadas e `Buzzy.Core.Testes` aprovado em 147/147 em 2026-09-29.
- A leitura do checkout em 2026-09-30 encontrou a integração de `Nucleo` em `src/Buzzy.App/Composicao/Aplicacao.cs` e expectativas de `Loaded` em `tests/Buzzy.App.Testes/Integracao/IntegracaoTestes.cs`.
- `docs/TODO.md` e `docs/PROJECT_CONTEXT.md` ainda registram a ligação e a verificação do app como pendentes. **A composição atual ainda não foi validada por build/testes após essa integração; o critério manual da Fase 2 também está pendente.** Não atualize o gate até obter evidência atual.

### Próximas ações em ordem

1. Reconfira `git status --short`, diffs, os arquivos reais e os resultados. Leia as fontes exigidas em `AGENTS.md` e siga [prompt_usuario.md](prompt_usuario.md). Não assuma que este snapshot continua exato.
2. Inspecione `Aplicacao.cs` e os testes da Fase 2 antes de editar. Execute `dotnet build Buzzy.slnx -c Release` e `powershell -NoProfile -File tools/testar.ps1`; corrija falhas e repita verificações afetadas.
3. Para a suíte de integração, `powershell -NoProfile -File tools/testar.ps1 -Integracao` abre e fecha janelas do Buzzy. Avise o usuário antes. Faça a verificação manual do critério 4 se for possível sem interferir em outros aplicativos. Se não for, registre como pendente e continue trabalho independente.
4. Com o gate satisfeito, sincronize os documentos na ordem de `AGENTS.md`, atualize evidências e status reais e avance para a Fase 3 sem pedir aprovação. Preserve pendências [MANUAL]/[HW] da Fase 1 para a integração final; não altere configurações globais do Windows.

## Estado e evidência que orientam o handoff

- **Fase 0 / P3:** gate técnico passou no ambiente medido em 2026-09-29: 3/3 rodadas, 28/28 cenários por rodada. Input totalmente sintético; não é evidência humana nem valida DPI misto. Relatórios: `spikes/resultados/p3-receptor.log` e `spikes/resultados/p3.log`.
- **Fase 1:** shell implementado, mas permanece PLANNED. O relatório `resultados/verificacao-fase1.log` tem 25 OK, 4 SIMULADO e 0 falhas, com input sintético. Pendem menu real pela bandeja, escalas do Windows 150%/200% e mudanças reais de resolução/escala/barra de tarefas. A linha de base de repouso durou 600 s; não valida metas de 1 h/8 h. Veja `docs/TODO.md` e `docs/PROJECT_CONTEXT.md`.
- **Fase 2:** teste anterior do núcleo puro passou 147/147; isso não prova a integração atual. `Aplicacao.cs` contém criação do `Nucleo`, envio de `Loaded`/topologia/comandos/menu/fim de sessão, execução de efeitos, timers e logs `NUCLEO`. O teste de integração espera as transições `Booting → Settling → Idle`. Confira os asserts e os logs reais ao validar.
- **Identidade:** DEC-018/DEC-019 aprovaram pixel art fiel às pranchas e semelhança intencional com Luffy, incluindo chapéu de palha e faixa vermelha. O quadro parado e o ícone já estão integrados; animações pertencem à Fase 6. A fonte visual é `docs/IDENTIDADE_VISUAL.md` e os arquivos em `src/Buzzy.Visual/Pixel/` / `assets/identidade/pixel/`.

## Alertas históricos

- Tentativas antigas da integração antecederam o estado atual da composição: uma execução no sandbox não conseguiu gravar diagnóstico em `%LOCALAPPDATA%\Buzzy\diagnostico.log`; uma tentativa focalizada falhou no assert de `NUCLEO|Loaded`. Esses resultados não validam nem reprovam o código atual. Se um problema parecido ocorrer de novo, identifique primeiro se é limitação de gravação do ambiente ou comportamento do app.
- O backup antigo de Claude continha instruções visuais superadas (sem chapéu e sem semelhança com Luffy) e uma divisão de trabalho anterior. A decisão atual é DEC-019 e a transferência atual está em `prompt_usuario.md`.
- Em 2026-09-30, antes desta consolidação, `git status --short` estava limpo no commit `124130d update`. Esta alteração de handoff modifica `prompt_usuario.md`, `CLAUDE.md`, cria este arquivo e remove os dois backups separados. Nenhum build ou teste foi executado nesta tarefa documental.

## Comunicação entre Claude e Codex

Mantenha aqui um resumo do estado atual e acrescente uma entrada curta por marco material. Registre agente, data, arquivos alterados, resultado verificável, limitações e próximo passo. Guarde requisitos e decisões estáveis na fonte canônica; este registro aponta para eles em vez de duplicá-los. Não faça commit automaticamente.

### Log de continuidade

- **2026-09-30 — Codex:** reuniu `BACKUP_CLAUDE.md` e `BACKUP_CODEX.md`, eliminou divisões e direções visuais superadas, atualizou a diretiva para Claude retomar pela validação da Fase 2 e deixou as fontes canônicas inalteradas porque ainda falta validar o estado atual. Nenhum teste executado; verificação de código limitada à inspeção de `Aplicacao.cs` e dos asserts de integração. Próximo passo: seguir “Retomada atual”.

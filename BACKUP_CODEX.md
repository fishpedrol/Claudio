# Continuidade — Buzzy (Codex)

> Atualizado em 2026-09-29. Snapshot para retomada; confirme sempre `git status`, o código e as evidências atuais antes de agir. As fontes canônicas são os documentos em `docs/`. Não substitui `BACKUP_CLAUDE.md`.

## Retomada para Claude — 30/09/2026

A instrução mais recente do usuário pede que Claude retome amanhã. `BACKUP_CLAUDE.md` ainda registra uma divisão anterior em que Codex implementava a Fase 2 e Claude cuidava apenas da pixel art; considere a solicitação mais recente como a transferência atual e reconcilie a divisão ao retomar.

1. Leia este arquivo, `BACKUP_CLAUDE.md`, `docs/PROJECT_CONTEXT.md`, `docs/PRODUCT_SPEC.md`, `docs/TODO.md`, `context codex/PROMPT_MESTRE_BUZZY.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md`, `docs/PLAN_REVIEW.md` e `docs/SECURITY.md`. Compare as afirmações com `git status --short`, `git diff` e os arquivos reais. Há alterações simultâneas de identidade visual, documentação e Fase 2; preserve-as.
2. **Não refaça a integração da Fase 2 sem inspecionar `Aplicacao.cs`.** O arquivo atual já contém a ponte com `Nucleo`, eventos, execução de efeitos, relógio lógico, temporizador autônomo e logs. Isso está implementado no código, mas ainda não foi validado por build/testes depois dessa edição. Rode primeiro build e suites automatizadas, corrija falhas e repita as verificações afetadas.
3. A integração de tela precisa gravar diagnóstico em `%LOCALAPPDATA%\Buzzy\diagnostico.log`. Uma tentativa anterior no sandbox falhou porque o diretório estava somente leitura; a tentativa focada fora do sandbox iniciou o app, mas falhou no novo assert de carregamento do núcleo. Essas execuções são anteriores ao estado atual de `Aplicacao.cs`, portanto não provam nem falha nem sucesso do código atual. Avise antes de executar testes que abrem janelas ou movem o cursor, conforme `AGENTS.md`.
4. Faça a verificação manual do critério 4 da Fase 2 se o ambiente permitir. Se não puder, registre-a como pendente; não marque a fase como VERIFIED só porque compila.
5. A Fase 1 continua PLANNED por verificações reais pendentes: menu pela bandeja, escalas de 150% e 200%, e mudanças reais de resolução/escala/barra de tarefas. Não altere configurações globais do Windows para simular isso. Registre o que exigir hardware/ação do usuário e continue o trabalho independente.
6. Depois de validar, sincronize a documentação na ordem de `AGENTS.md`: PROJECT_CONTEXT, DEVELOPMENT_LOG, ARCHITECTURE se mudou, DECISIONS se houve decisão, SECURITY se o modelo mudou, TODO e prompt mestre. Atualize também o quickstart se seu status ficou desatualizado. Atualize este arquivo com resultados reais. Não faça commit automaticamente.

## Estado observado no checkout

- **Git:** branch `main`, HEAD `73ba605` (`prototipo 1`), sem commit novo. `BACKUP_CODEX.md` está sem commit. Reconfira o estado ao retomar.
- **Fase 0 / P3:** VERIFIED como gate técnico no ambiente medido: 3/3 rodadas, 28/28 cenários por rodada. Input sintético; não comprova gesto humano nem DPI misto.
- **Fase 1:** shell implementado; relatório `resultados/verificacao-fase1.log`: 25 OK, 4 SIMULADO, 0 falhas, todo input sintético. O menu real da bandeja não foi validado; escalas de 150%/200% e alterações reais de resolução/escala/barra continuam pendentes. A linha de base de desempenho durou 600 s, não 1 h/8 h.
- **Fase 2 — núcleo:** Core puro implementado; `Buzzy.Core.Testes` passou 147/147 em 2026-09-29; referências 02–05 constam como revisadas manualmente. A integração da composição agora está presente em `src/Buzzy.App/Composicao/Aplicacao.cs`, mas não está verificada no estado atual. `docs/TODO.md`, `PROJECT_CONTEXT.md` e `DEVELOPMENT_LOG.md` ainda dizem que a ligação com `Aplicacao.cs` está pendente; corrigir essa divergência somente depois de testar o código atual.
- **Identidade:** a direção foi refeita em pixel art por solicitação do usuário (DEC-018). Há gerador em `src/Buzzy.Visual/Pixel/`, folha e prévias em `assets/identidade/pixel/`; a direção vetorial está arquivada em `assets/identidade/arquivo-vetorial/`. Os documentos registram 21 poses, 14 expressões e a validação de que nenhuma pose encosta na borda. O trabalho visual está marcado VERIFIED no TODO; a integração do asset ao app pertence à Fase 6. Codex não fez inspeção visual manual das imagens nesta retomada.

O `git status --short` observado incluía alterações em `BACKUP_CLAUDE.md`, `README.md`, `COMO_INICIAR.md`, documentos canônicos, código/teste da Fase 2 e arquivos da identidade pixel. A arte vetorial foi movida para `arquivo-vetorial`; arquivos pixel e prévias novos estão em `assets/identidade/pixel/`. Não restaure os caminhos antigos nem descarte essas mudanças sem comparar os diffs.

## Evidências e limites

| Estado | Evidência conhecida | Limite atual |
|---|---|---|
| Automatizado — base anterior à integração atual | `tools/testar.ps1` passou build Release sem avisos/erros, 147 testes Core, 73 testes do portão e 18 testes do app sem janela; portão de APIs aprovado | Não foi executado novamente depois das alterações atuais em `Aplicacao.cs` e no teste de integração |
| Integração automatizada — tentativa anterior | No sandbox, 4 testes falharam esperando `JANELA`; o logger não pôde gravar no `%LOCALAPPDATA%\Buzzy` por ACL somente leitura. Fora do sandbox, teste focado iniciou o app e falhou no assert novo de `NUCLEO|Loaded` | Ambas as tentativas precedem o código atual da ponte; repetir para obter evidência válida do estado atual |
| Fase 1 — tela e desempenho | 25 OK / 4 SIMULADO; medição de repouso 600 s | Input sintético; os itens de bandeja real, DPI 150/200 e mudança real de topologia continuam pendentes |
| Pixel art | Fontes, folha nativa e prévias existem; TODO registra tarefa visual VERIFIED | Codex não inspecionou manualmente as imagens; asset ainda não está integrado ao app |
| Fase 2 — manual | Nenhuma validação manual do critério 4 após a integração atual | Pendente |

As quatro integrações que falharam no sandbox não indicam defeito do aplicativo: o processo de teste não conseguia escrever o log usado como evidência. A tentativa focalizada fora do sandbox também não valida a composição atual porque o código mudou depois dela. Não apresente nenhum desses resultados históricos como teste atual aprovado.

## Trabalho de Codex registrado

- Comparou o backup anterior com o checkout, documentação e resultados disponíveis; registrou Fase 0/1, testes do Core e pendências manuais.
- Atualizou `docs/PROJECT_CONTEXT.md`, `docs/TODO.md`, `docs/DEVELOPMENT_LOG.md` e `docs/SECURITY.md` no trabalho de continuidade. Esses documentos receberam alterações simultâneas depois; confira seus conteúdos atuais antes de editar.
- Acrescentou em `tests/Buzzy.App.Testes/Integracao/IntegracaoTestes.cs` uma expectativa para o log de carregamento do núcleo. O assert atual espera evento `Loaded` com estado `Settling` e motivo `início`.
- A composição atual em `Aplicacao.cs` contém a integração da Fase 2. Ela envia `Loaded`, `TopologyChanged`, `SessionEnding`, `CmdHide`, `CmdShow`, `CmdExit` e `ContextMenu`; processa efeitos de janela/menu/encerramento, relógio lógico e temporizador autônomo; e registra transições do núcleo. **Status: implementado no código, não testado após a edição atual.**
- Criou este `BACKUP_CODEX.md`. Nenhum commit foi feito.

## Regras a manter

- Distinguir implementação, teste automatizado executado, validação manual, simulação e pendência. Não converter evidência sintética em humana nem declarar VERIFIED sem todos os gates.
- Preservar os logs de tela existentes (`VISIVEL`, `POSICAO`, `MENU`, `TOPOLOGIA`, `SERVICO`, `BANDEJA`, `FIM`) e validar os novos eventos `NUCLEO` contra os asserts atuais.
- Sem IA integrada, sem commit automático. Não encerrar processos que não foram iniciados pelo teste. Não ler conteúdo de outros aplicativos nem alterar configurações globais do Windows para simular escalas ou topologia.

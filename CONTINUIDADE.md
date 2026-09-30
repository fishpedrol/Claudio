# Continuidade compartilhada — Buzzy

> Registro único de handoff e backup para Claude e Codex. Atualizado em 2026-09-30.
> As fontes canônicas em `docs/`, o código e as instruções atuais do usuário prevalecem. Este arquivo registra o estado de trabalho entre sessões; confirme tudo no checkout antes de agir.

## Retomada atual

O usuário pediu que Claude volte a implementar o que falta. A diretiva está em [prompt_usuario.md](prompt_usuario.md); a autorização contínua de Claude está em DEC-015. A divisão anterior que deixava a Fase 2 com Codex foi substituída.

**Estado em 2026-09-30 12:30 (Claude), nada commitado:**

- **Fase 2:** VERIFIED no `docs/TODO.md`. O critério 4 foi verificado com input SINTÉTICO.
- **Fase 3:** implementada e verificada por automação e input SINTÉTICO; continua PLANNED por três pendências: UAC [MANUAL], DPI misto [HW] e ClickLock [MANUAL].
- **Fase 4:** implementada e verificada numa **cópia isolada** fora do repositório:
  - caminho: `C:\Users\Cliente\AppData\Local\Temp\claude\C--Users-Cliente-Documents-claudio\3b0e750b-9776-418f-aa13-3a4fa1644cac\scratchpad\f4`;
  - motivo da cópia: um workflow de testes estava compilando a árvore principal;
  - **a Fase 4 ainda não está no repositório.**

Arquivos da Fase 4 em `f4` que precisam entrar na árvore principal:

- núcleo:
  - `src/Buzzy.Core/Personagem/Movimento.cs` (novo);
  - `Configuracao.cs` (`Movimento`, `Fisica`, faixas de distância, pulo, parede e pendurado por energia);
  - `EstadoDoNucleo.cs` (campo `Movimento`);
  - `Maquina.cs` (física: `PassoAndando`, `PassoEscalando`, `PassoPendurado`, `PassoNoAr`, `Planejar*`, `SaltarDaParede`, `SairDoTeto`);
- app: `Aplicacao.cs`, com a configuração `Acoes = Todas`, `QuedaFisica = true`, `Movimento = true`;
- testes:
  - `tests/Buzzy.Core.Testes/Movimento/*` (novos);
  - `tests/Buzzy.App.Testes/Integracao/MovimentoIntegracaoTestes.cs` (novo);
  - parâmetro `semente` em `BuzzyEmTeste.cs`;
- harness: `tests/Buzzy.Verificacao/VerificacaoFase4.cs` (novo), `Programa.cs` (`--fase 4`) e `Injetor.cs` (`esperarCliqueDuplo`).

Mescle arquivo por arquivo com diff, porque a árvore principal recebeu mudanças depois da cópia. Não copie a pasta inteira por cima.

**Atenção:** `tests/Buzzy.App.Testes/Integracao/GestosTestes.cs` na árvore principal já espera a queda da Fase 4. Até a mescla, o teste de integração de arraste falha na árvore principal.

### Próximas ações em ordem

1. Esperar o workflow `fase2-cobertura-testes` (run `wf_3fd26000-099`), que escreve testes em `tests/Buzzy.Core.Testes/Personagem`. Revisar o que ele escreveu, inclusive qualquer relato `// DEFEITO:`.
2. Mesclar a Fase 4 de `f4`, rodar `tools/testar.ps1`, `-Integracao` (avise o usuário antes) e `Buzzy.Verificacao --injetar-input-na-tela --fase 1|3|4` (abre janelas e move o cursor; avise antes).
3. Sincronizar os documentos das Fases 3 e 4:
   - TODO;
   - PROJECT_CONTEXT;
   - DEVELOPMENT_LOG;
   - ARCHITECTURE 1, 2.5, 2.9 e 2.13.4;
   - DEC-022, com o rascunho em `scratchpad\fase4\DEC-022.md` e os valores finais de `f4`;
   - SECURITY (APIs `SetCapture`, `ReleaseCapture`, `GetDoubleClickTime` e `GetWindowRect` na própria janela);
   - COMO_INICIAR;
   - README.
4. Seguir para a Fase 5 sem pedir aprovação (DEC-015).

## Estado e evidência que orientam o handoff

- **Fase 0 / P3:** gate técnico passou no ambiente medido em 2026-09-29: 3/3 rodadas, 28/28 cenários por rodada. Input totalmente sintético; não é evidência humana nem valida DPI misto. Relatórios: `spikes/resultados/p3-receptor.log` e `spikes/resultados/p3.log`.
- **Fase 1:** shell implementado, mas permanece PLANNED. O relatório `resultados/verificacao-fase1.log` tem 25 OK, 4 SIMULADO e 0 falhas, com input sintético. Pendem menu real pela bandeja, escalas do Windows 150%/200% e mudanças reais de resolução/escala/barra de tarefas. A linha de base de repouso durou 600 s; não valida metas de 1 h/8 h. Veja `docs/TODO.md` e `docs/PROJECT_CONTEXT.md`.
- **Fase 2:** VERIFIED em 2026-09-30.
  - `tools/testar.ps1` passou antes das mudanças de hoje (Core 147/147); a integração passou 22/22.
  - Verificação de tela com o núcleo: 25 OK, 4 SIMULADO, 0 falhas.
  - Repouso de 10 min com CPU média de 0,010%.
  - Auditoria adversarial corrigida em DEC-020; a cobertura nova está sendo escrita.
- **Fase 3:** árbitro puro `src/Buzzy.Core/Entrada/ArbitroDeGestos.cs` e adaptador em `JanelaPersonagem.cs` (DEC-021).
  - Testes: Core 171/171 e integração 25/25.
  - `--fase 3`: 33 OK, 2 N/A, 0 falhas.
- **Fase 4 (em `f4`):**
  - Core 180/180; integração 29/29.
  - `--fase 4`: 17 OK, 0 falhas. Segurou no meio da queda; caminhada com passo máximo de 3 px e intervalo p95 de 31,9 ms; subiu 176/176, pendurou-se 146 vezes e voltou ao chão em 16 s; repouso com 0,000% de CPU e relógio desligado.
  - Pendência [MANUAL]: critério 5, gravação a 120 qps.
  - As 21 falhas de `PortaoApis` dentro de `f4` são do ambiente (sem `spikes/`), não do código.
- **Identidade:** DEC-018/DEC-019 aprovaram pixel art fiel às pranchas e semelhança intencional com Luffy, incluindo chapéu de palha e faixa vermelha. O quadro parado e o ícone já estão integrados; animações pertencem à Fase 6. A fonte visual é `docs/IDENTIDADE_VISUAL.md` e os arquivos em `src/Buzzy.Visual/Pixel/` / `assets/identidade/pixel/`.

## Alertas históricos

- Tentativas antigas da integração antecederam o estado atual da composição: uma execução no sandbox não conseguiu gravar diagnóstico em `%LOCALAPPDATA%\Buzzy\diagnostico.log`; uma tentativa focalizada falhou no assert de `NUCLEO|Loaded`. Esses resultados não validam nem reprovam o código atual. Se um problema parecido ocorrer de novo, identifique primeiro se é limitação de gravação do ambiente ou comportamento do app.
- O backup antigo de Claude continha instruções visuais superadas (sem chapéu e sem semelhança com Luffy) e uma divisão de trabalho anterior. A decisão atual é DEC-019 e a transferência atual está em `prompt_usuario.md`.
- Em 2026-09-30, antes desta consolidação, `git status --short` estava limpo no commit `124130d update`. Esta alteração de handoff modifica `prompt_usuario.md`, `CLAUDE.md`, cria este arquivo e remove os dois backups separados. Nenhum build ou teste foi executado nesta tarefa documental.

## Comunicação entre Claude e Codex

Mantenha aqui um resumo do estado atual e acrescente uma entrada curta por marco material. Registre agente, data, arquivos alterados, resultado verificável, limitações e próximo passo. Guarde requisitos e decisões estáveis na fonte canônica; este registro aponta para eles em vez de duplicá-los. Não faça commit automaticamente.

### Log de continuidade

- **2026-09-30 — Codex:** reuniu `BACKUP_CLAUDE.md` e `BACKUP_CODEX.md`, eliminou divisões e direções visuais superadas, atualizou a diretiva para Claude retomar pela validação da Fase 2 e deixou as fontes canônicas inalteradas porque ainda falta validar o estado atual. Nenhum teste executado; verificação de código limitada à inspeção de `Aplicacao.cs` e dos asserts de integração. Próximo passo: seguir “Retomada atual”.
- **2026-09-30 11:13 — Claude (Opus 5.5):** checkout limpo em `6028968`. Sem alterar código: `tools/testar.ps1` código 0 — build Release 0 avisos/0 erros, Core 147/147, portão 73/73, App 18/18 sem janelas, portão de APIs APROVADO, 0 pacotes vulneráveis. `tools/testar.ps1 -Integracao` código 0 — App 22/22 com os 4 testes de tela; o log real mostra `NUCLEO` `Loaded` Booting→Settling→Idle, `CmdHide` por minimização, `CmdShow` pela segunda instância e `CmdExit` por WM_CLOSE; M6 = 725 ms. Próximo: `Buzzy.Verificacao --injetar-input-na-tela` (input SINTÉTICO) como evidência do critério 4 da Fase 2, revisão do código de integração e fechamento do gate.
- **2026-09-30 11:40 — Claude:** Fase 2, critério 4: `Buzzy.Verificacao --injetar-input-na-tela` com o núcleo ligado = 25 OK, 4 SIMULADO (bandeja), 0 falhas, código 0 (input SINTÉTICO; `resultados/verificacao-fase1.log`). Observação INFORMAL do usuário (não é teste): ele fechou sem querer a primeira medição; o log mostra menu do personagem (Esconder/Sair) e restauração por clique real no ícone da bandeja funcionando com o núcleo. Repouso de 10 min com o núcleo ligado (`resultados/desempenho-20260930-112125.txt`): CPU média 0,010% de um núcleo, p95 0,000%, privada 57,99→58,00 MB, 0 filhos, 0 rede, resolução do timer inalterada, M6 528 ms. Auditoria adversarial do gate da Fase 2 rodando em workflow (achados ainda não aplicados).
- **2026-09-30 11:45 — Claude:** Fase 3 iniciada (sem commit): árbitro puro `src/Buzzy.Core/Entrada/ArbitroDeGestos.cs` (limiar SM_CXDRAG por DPI, clique duplo pelas regras do Windows, captura perdida, botão direito); adaptador em `JanelaPersonagem.cs` (SetCapture/ReleaseCapture, WM_CAPTURECHANGED/WM_CANCELMODE, MK_LBUTTON), raiz em `Aplicacao.cs` (menu adiado para fora do processamento, M5 no log `ARRASTE`, POSICAO só fora do arraste). `tools/testar.ps1` código 0: Core 171/171, portão APROVADO; `Buzzy.App.Testes --integracao` 25/25, incluindo 3 testes novos de gestos por mensagens POSTADAS (arraste, clique/duplo/direito, captura perdida). Próximo: verificação de tela da Fase 3 com input SINTÉTICO em `Buzzy.Verificacao`, depois fechar docs das Fases 2 e 3.
- **2026-09-30 12:20 — Claude:** `Buzzy.Verificacao --injetar-input-na-tela --fase 3` (novo modo; input SINTÉTICO): 33 OK, 2 N/A, 0 falhas, código 0 (`resultados/verificacao-fase3.log`). Pendentes: critério 4 com UAC [MANUAL], critério 5 com escalas diferentes [HW], critério 7 ClickLock (desligado na máquina e NÃO alterado). CPU durante 7,4 s de arraste = 6,4% de um núcleo; M5 p95 0,52 ms. Auditoria adversarial da Fase 2 terminou com achados reais; corrigidos em `Maquina.cs` e `Aplicacao.cs` (DEC-020): precedência dos motivos, retorno da tela cheia nunca gravado nem reaproveitado, só a primeira carga vale, CLICK valida se o monitor mudou, piso de acomodação em todo agendamento, energia inválida, menu adiado, teto do relógio, janela reafirmada na releitura, RELOGIO/AGENDA no log. Docs ARCHITECTURE 1/2.6/2.7 e DECISIONS DEC-020/021 atualizados. Em andamento: workflow `fase2-cobertura-testes` escrevendo os testes pedidos pela auditoria em `tests/Buzzy.Core.Testes/Personagem` (não mexe em src/). Depois: build + `testar.ps1` + integração (3 testes novos em `ComposicaoTestes.cs`), fechar TODO/PROJECT_CONTEXT/DEVELOPMENT_LOG/SECURITY das Fases 2 e 3 e seguir para a Fase 4.
- **2026-09-30 12:30 — Claude:** Fase 4 desenvolvida na cópia isolada `scratchpad\f4` (caminho em “Retomada atual”).
  - Movimento determinístico pelo relógio de passo fixo ligado ao compositor (`CompositionTarget.Rendering`), com lote de passos e um `MoverJanela` por quadro.
  - Andar, escalar, pendurar-se, pular, cair e pousar, com velocidades em DIP/s convertidas pela escala do monitor.
  - Superfícies vindas da área útil; lateral é parede, ou passagem se houver vizinho encostado.
  - Pausar/Retomar movimento no menu; argumentos `--pausado` e `--semente N`.
  - Ajuste após a verificação de tela: escalada a 110 DIP/s, pendurado a 80 DIP/s e tempo na parede de 10–20, 15–35 ou 20–45 s. O disparo em 12 s cortava a subida de 12,9 s.
  - Resultados:
    - `f4`: Core 180/180, integração 29/29, `--fase 4` 17 OK / 0 falhas;
    - energia por 10 min simulados: Baixa com 6,4 ações e 16 s em movimento; Média com 17,1 ações e 86 s; Alta com 35,5 ações e 275 s.
  - `docs/TODO.md` atualizado com as Fases 2 (VERIFIED) e 3 (PLANNED com pendências). Workflow de cobertura ainda rodando.
  - Próximo: mesclar `f4` na árvore principal.
- **2026-09-30 12:45 — Claude:** o redator do workflow de cobertura achou um DEFEITO real, com o teste `TelaCheiaTestes.Q1_DesligarOModoComRetornoGuardado_NaoTornaAPosicaoTemporariaDefinitiva`.
  - Defeito: `SETTINGS_CHANGED` desligando o modo de tela cheia descartava o retorno sem restaurá-lo, com o personagem escondido por outro motivo ou pressionado. A posição temporária virava definitiva.
  - Correção feita **só em `f4`** (`Maquina.cs`: `MudarPreferencias`, `FimDoGestoDoUsuario` e `Esconder`). A regra está em ARCHITECTURE 2.6 (linha nova) e DEC-020. Também precisa ir para a árvore principal na mescla.
  - `f4`, com os testes do redator copiados para lá, deu Core 238/238 (229 do redator + 9 de movimento).
  - Um backup dos arquivos da Fase 4 de antes dessas mudanças está em `scratchpad\f4-fase4-original`.
  - Documentos:
    - DEC-022 registrada;
    - ARCHITECTURE 1, 2.5, 2.9 e 2.13.4 atualizados para a Fase 4;
    - SECURITY 3.1 com as APIs da Fase 3;
    - COMO_INICIAR e README atualizados;
    - `PROXIMA_SESSAO.md` virou só um ponteiro para este arquivo.
  - `tools/medir-desempenho.ps1` ganhou `-Modo repouso|autonomia` e `-Semente`. `repouso`, o padrão, abre com `--pausado`.
  - Rodando em paralelo:
    - revisão do workflow de cobertura (`wf_3fd26000-099`);
    - desenho da Fase 5, só leitura (`wf_0c1129a6-6ad`).
- **2026-09-30 15:45 — Claude:** o usuário pediu: "quero que o bixinho suba pelas laterais do monitor também, tenha tipo toon force". Foi registrado como DEC-023 e implementado **em `f4`**.
  - Mudanças:
    - toda lateral é escalável, inclusive a que encosta em outro monitor (`Superficies.NaLateral`; `ParedeX` virou `PassagemX`);
    - quique de borracha (`Maquina.Quicar`, `EstadoDoMovimento.Quiques`);
    - foguete de borracha (`TalvezFoguete`, `EstadoDoMovimento.Foguete`, `PerfilDeEnergia.ChanceDoFoguete` 10/30/50);
    - achatar e esticar a pixel art (`Tela.Deformada` em Buzzy.Visual; `Deformacao`/`Dinamica` em `PoseDoPersonagem.cs`; `SpriteProvisorio`; `Aplicacao.AtualizarSprite`);
    - testes novos: 5 em `MovimentoTestes`, mais `PoseTestes.cs` no app;
    - harness: `F4QuiqueDeBorracha` e `F4SobePelaLateralInterna`.
  - Resultados em `f4`:
    - build 0/0; portão APROVADO; Core 243/243; App 23/23 + integração 34/34;
    - `--fase 4`: **22 OK / 0 falhas**, com input SINTÉTICO. Houve 2 quiques antes de pousar; a subida pela lateral esquerda do principal, encostada no secundário, deu 26/26 amostras na lateral, sem sair do principal, e terminou pendurado.
  - Documentos: DEC-023, ARCHITECTURE 2.5/2.6/2.9/2.10 e TODO Fase 4 (inclusão e critério 8).
  - Pendente: mesclar `f4` na árvore principal (o workflow de cobertura ainda está nas correções) e depois rodar tudo de novo lá.
- **2026-09-30 16:00 — Claude:** o workflow de cobertura terminou.
  - Os testes finais estão na árvore principal: `TransicoesComplementaresTestes`, `TelaCheiaTestes`, `InvariantesTestes`, `ReproducaoTestes` e `Cenario`.
  - O estado intermediário entrou no commit **`828c1f7`**, feito às 13:38 com a identidade git do usuário, durante a pausa por limite de uso. Claude estava parado nessa hora e não faz commit.
  - **Fase 4 e toon force mesclados na árvore principal** a partir de `f4`, arquivo por arquivo, com diff conferido:
    - `src/` inteiro de `f4`;
    - testes novos (`Movimento/`, `MovimentoIntegracaoTestes`, `PoseTestes`);
    - `BuzzyEmTeste`;
    - harness (`Injetor`, `Programa`, `VerificacaoFase4`).
  - Na árvore principal ficaram os testes finais do workflow, com o `movimento` recolocado no cabeçalho das reproduções. O teste do defeito virou regressão, e o teste de propriedade passou a conferir os ramos do defeito.
  - O cenário de soltar no ar da Fase 3 foi atualizado para a queda animada.
  - Resultados na árvore principal:
    - `tools/testar.ps1` código 0: build 0/0, Core **255/255**, portão 73/73, App 23/23, portão de APIs APROVADO, 0 vulneráveis;
    - integração **34/34**, M6 485 ms;
    - `Buzzy.Verificacao` (input SINTÉTICO):
      - `--fase 1`: 25 OK, 4 SIMULADO (bandeja), 0 falhas;
      - `--fase 3`: 33 OK, 2 N/A, 0 falhas (CPU no arraste 8,4%, M5 p95 0,59 ms);
      - `--fase 4`: **22 OK, 0 falhas**, com quique e subida pela lateral interna.
  - `f4` não é mais a fonte; a árvore principal é.
  - Próximo: medições de 10 min (`medir-desempenho.ps1 -Modo repouso` e `-Modo autonomia`), sincronizar PROJECT_CONTEXT, DEVELOPMENT_LOG e SECURITY 10, e depois a Fase 5. Os desenhos de 3 áreas estão em `scratchpad\fase5\`; o de travessia ainda está rodando.

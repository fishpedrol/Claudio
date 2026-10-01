# Continuidade compartilhada — Buzzy

> Registro único de handoff e backup para Claude e Codex. Atualizado em 2026-09-30.
> As fontes canônicas em `docs/`, o código e as instruções atuais do usuário prevalecem. Este arquivo registra o estado de trabalho entre sessões; confirme tudo no checkout antes de agir.

## Retomada atual

O usuário pediu que Claude volte a implementar o que falta. A diretiva está em [prompt_usuario.md](prompt_usuario.md); a autorização contínua de Claude está em DEC-015. A divisão anterior que deixava a Fase 2 com Codex foi substituída.

**Estado em 2026-09-30 20:35 (Claude). Os commits `828c1f7`, `803a54b`, `0699c08` e `87c4203` foram feitos com a identidade git do usuário; o resto está no checkout, sem commit:**

- **Fase 2:** VERIFIED.
- **Fases 3 e 4:** implementadas na árvore principal e verificadas por automação e input SINTÉTICO; continuam PLANNED por pendências [MANUAL]/[HW]:
  - Fase 3: UAC, DPI misto e ClickLock;
  - Fase 4: gravação de tela a 120 qps.
- **Fase 4 inclui os pedidos do usuário:**
  - toon force (DEC-023);
  - cipó e "preso onde você solta" (DEC-024);
  - esconderijo pelo clique duplo (DEC-025).
- **Fase 5:** bloco A (P1–P5) implementado pelo workflow `wf_10b65660-ce6` e em correção; **ainda não verificado por Claude** (detalhes no log das 20:35).
- As cópias `scratchpad\f4` e `scratchpad\cipo` estão obsoletas; a árvore principal é a fonte.

### Próximas ações em ordem

1. **Fase 5**, seguindo a ordem P1–P16 de `scratchpad\fase5\critico-integracao.md`, com as resoluções C1–C18 e os desenhos em `scratchpad\fase5\projetista-*.md`:
   - numeração das DECs da Fase 5: DEC-029 (persistência), DEC-030 (chave e topologia), DEC-031 (sessão, energia e minimização) e DEC-032 (travessia e escala). DEC-027 e DEC-028 foram para os pedidos novos do usuário;
   - na travessia, respeite a DEC-023: na passagem, a agenda sorteia entre atravessar e escalar (C14);
   - isolamento dos testes por `--perfil-de-teste` antes de gravar qualquer arquivo real (C17).
2. **Pedidos novos do usuário (2026-09-30, 18:00)** — emoção dominante no menu (DEC-027) e tamagotchi adulto com 13 itens em pixel art (DEC-028):
   - itens invocados pelo menu, que caem no chão ao lado dele, e usados quando o usuário os arrasta e solta sobre ele, com animação de fumar, cheirar, beber, comer, engolir ou inalar;
   - efeitos cartunescos no comportamento, sem necessidades que decaem com o tempo;
   - o desenho está sendo feito pelo workflow só de leitura `wf_401d9729-204` (resultado em `scratchpad\tamagotchi\`, a extrair);
   - implementar depois do bloco A da Fase 5 (`wf_10b65660-ce6`, rodando), antes dos blocos B a D.
3. Depois da Fase 5: a curiosidade (DEC-026) com o observador de janela ativa (P7), junto com o modo de tela cheia.
4. Toda verificação de tela avisa o usuário antes. Nada de commit automático.

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
- **Fase 4 (árvore principal):**
  - Core 267/267; App 37/37 com a integração.
  - `--fase 4`: 28 OK, 0 falhas, cobrindo queda segurada, caminhada, escalada, repouso, quique, lateral interna, cipó, parede e esconderijo.
  - Pendência [MANUAL]: critério 5, gravação a 120 qps.
- **Fase 5 (em andamento):** P1–P5 com evidência só dos agentes do workflow, em Debug: Core 307/307, portão 73/73, App 52 (+11 de integração não executados). Falta a bateria em Release, que depende de o usuário fechar o Buzzy aberto a partir da pasta Release, e a integração e as verificações de tela com `--perfil-de-teste`.
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
- **2026-09-30 17:30 — Claude:** os pedidos do usuário foram implementados e verificados **na árvore principal**:
  - cipó na borda de cima;
  - agarrar onde é solto, preso até o usuário tirar (DEC-024);
  - esconderijo pelo clique duplo (estado `PEEKING`, DEC-025). O painel de energia vai para o menu na Fase 8.
  - Resultados:
    - `tools/testar.ps1` código 0: Core 267/267, portão 73/73, App 26/26;
    - integração 37/37;
    - `--fase 3`: 34 OK e 2 N/A; `--fase 4`: 28 OK.
  - Medições de 10 min feitas:
    - repouso: 0,003% de CPU;
    - autonomia: 3–4% de CPU em movimento. A memória fica num patamar depois da 1ª coleta do GC; não é vazamento. O diagnóstico está no DEVELOPMENT_LOG, e o log de diagnóstico ganhou as linhas `MEMORIA` e `SPRITE`, só por evento.
  - Curiosidade (DEC-026) entra depois da Fase 5.
  - Commits `828c1f7` e `803a54b` foram feitos com a identidade do usuário; Claude não commita.
  - **Próximo: Fase 5**, na ordem P1–P16 da crítica de integração (`scratchpad\fase5\critico-integracao.md`). As DECs da Fase 5 passam a ser DEC-027 a DEC-030, porque 023 a 026 já estão ocupadas. Os desenhos das 4 áreas estão em `scratchpad\fase5\projetista-*.md`.
- **2026-09-30 20:35 — Claude:** andamento da Fase 5 e dos pedidos novos. **Nada abaixo foi verificado por Claude ainda.**
  - **Bloco A (P1–P5)**, workflow `wf_10b65660-ce6`, na árvore principal:
    - P1–P2: `PosicaoDoPersonagem.TelaDoMonitor` (`RetanguloPx?`), `Posicionador.PixelDosPes` e `Restaurar` único (chave → retângulo do monitor → principal, com frações saneadas); `Loaded` com 5 ou 9 campos nas reproduções.
    - P3: esquema v1 no núcleo (`Buzzy.Core/Persistencia`: `EsquemaDeConfiguracoes`, `ConfiguracoesSalvas`, `PoliticaDeGravacao`), `Preferencias.AtravessarMonitores` (padrão ligado, ainda sem efeito) e invariante 18 nos testes.
    - P4–P5: `PastaDeDados`, `ArquivoDeConfiguracoes` (gravação atômica com `.bak` e `settings.corrupt.json`) e `--perfil-de-teste` nos lançadores de teste. **Ainda não ligado ao app** (é o P7).
    - Resultado relatado pelos agentes, em Debug: build 0/0, Core 307/307, portão 73/73, App 52 (+11 de integração não executados), portão de APIs APROVADO, 0 vulneráveis.
    - **Release não rodou:** um Buzzy aberto a partir de `src\Buzzy.App\bin\Release\net10.0-windows` trava as DLLs (PID 10676 às 17:56; às 20:31, PID 15008, aberto às 19:43). É processo do usuário e não foi encerrado.
  - **Revisões de correção e de segurança:** nenhum defeito bloqueante. Achados em correção:
    - variações de `--perfil-de-teste` (`=NOME`, outra caixa, `/`) caem na pasta real;
    - a escolha da pasta de dados não tem regra testada e pode falhar aberta no P7;
    - o `.tmp` segue um link que já exista;
    - exceção que não é de E/S escapa de `Ler()`;
    - `e.Message` no log (anterior à Fase 5);
    - o invariante 18 aceita eventos proibidos;
    - a tela nova em `Validar` não tem teste;
    - os invariantes aleatórios nunca ligam a física da Fase 4;
    - `medir-desempenho` não limpa o perfil;
    - uma asserção depende do relógio.
  - **Corretor:** a 1ª tentativa parou às 19:28 (interrupção) e a 2ª começou às 20:20 e está rodando.
  - **Commits:** `0699c08` (17:50) e `87c4203` (18:24) guardaram estados intermediários dos passos. Estão fora do Git `ArquivoDeConfiguracoesTestes.cs`, `IsolamentoTestes.cs`, `Integracao/PerfilDeTeste.cs` e `Persistencia/InvarianteDezoito.cs`; um commit só dos arquivos rastreados não compila.
  - **Tamagotchi (DEC-027/028)**, workflow `wf_401d9729-204`:
    - os projetistas de núcleo, app e pixel terminaram;
    - o crítico parou às 19:37 e recomeçou às 20:20.
  - **Numeração:** as DECs da Fase 5 são DEC-029 a DEC-032; os relatórios dos agentes ainda citam 027–030.
  - **Dúvida de produto:** o esconderijo (DEC-025) e a marca "preso pelo usuário" (DEC-024) não são gravados, então quem sai escondido reaparece inteiro na mesma borda.
    - Às 20:40 o usuário fechou o Buzzy e deixou a escolha com Claude ("faz o que achar melhor").
    - **Decisão:** gravar os dois no esquema v1, no P7, e registrar na DEC de persistência (DEC-029). Ele volta escondido no mesmo lado e continua preso onde o usuário o deixou.
  - **Próximo:** quando o corretor e o crítico terminarem:
    1. pedir ao usuário para fechar o Buzzy;
    2. rodar `testar.ps1` em Release, `-Integracao` e `Buzzy.Verificacao --fase 1|3|4`, avisando antes;
    3. sincronizar os docs da Fase 5;
    4. implementar DEC-027/028;
    5. seguir para o bloco B (P6–P9).
- **2026-09-30 20:55 — Claude:** **Fase 5, bloco A (P1–P5) verificado por Claude.**
  - O workflow `wf_10b65660-ce6` terminou. O corretor aplicou os achados das revisões e adiou dois:
    - exceção que não é de E/S em `Ler()`, para o P7;
    - frações da área antiga em `Validar`, para o P8 (R14).
  - Relatórios em `scratchpad\wf_10b65660-ce6-relatorios.md` e `scratchpad\blocoA-corretor.md`.
  - **Execução de Claude, com o Buzzy do usuário fechado:**
    - `tools/testar.ps1` (Release) código 0: build com 0 avisos, Core **309/309**, portão 73/73, App 59 (+11 de integração), portão de APIs APROVADO (agora também sobre `src\Buzzy.Visual`), 0 vulneráveis;
    - `tools/testar.ps1 -Integracao` código 0: App **70/70**, em 45 s;
    - `Buzzy.Verificacao --injetar-input-na-tela`, com input SINTÉTICO e `--perfil-de-teste verificacao`:
      - `--fase 1`: 25 OK, 4 SIMULADO, 0 falhas;
      - `--fase 3`: 34 OK, 2 N/A, 0 falhas;
      - `--fase 4`: 28 OK, 0 falhas.
    - Logs em `scratchpad\claude-*.txt`.
  - **Desenho do tamagotchi pronto** (workflow `wf_401d9729-204`), em `scratchpad\tamagotchi\`:
    - `projetista-nucleo.md`, recomposto: o começo, D1–D6, tinha ficado fora do resultado do workflow;
    - `projetista-app.md` e `projetista-pixel.md`;
    - `critico-integracao.md`, que manda nos conflitos (C1–C30, L1–L20) e dá a ordem T1–T9 + A1–A4.
  - **Próximo:** sincronizar os docs do bloco A (DEC-029 etc.); implementar T1 e T3–T6 no núcleo e A1–A4 na arte, numa cópia isolada; depois T2 e T7–T9 no app.
- **2026-09-30 21:00 — Claude:** workflow **`wf_cb6cb763-1bc`** (tamagotchi, bloco 1) lançado, com três trilhas em paralelo:
  1. **Docs do bloco A** da Fase 5: DEC-029 (persistência, com a decisão de gravar esconderijo e "preso" no P7) e DEC-030 (chave e topologia), ARCHITECTURE, SECURITY, TODO etc. Não toca em `CONTINUIDADE.md`.
  2. **Núcleo, na árvore principal:** T1 (emoção dominante no núcleo e no esquema v2) → T3–T4 (tipos, tabelas, onda) → T5–T6 (itens, `USING`, invariantes 22–29, referência 07) → revisor → corretor. A chave `Tamagotchi` continua desligada em `DoAplicativo`.
  3. **Arte, na cópia isolada `scratchpad\arte`:**
     - A1–A2: itens 24×24 e caras de efeito;
     - A3–A4: poses de uso no chão, sobreposições e ícones do menu (`Tela.Recortada` única);
     - revisor e corretor.
     - O corretor devolve a lista de arquivos para a mescla.
  - **Decisões do coordenador passadas aos agentes** (acima dos desenhos):
    - enum `Item` na ordem do usuário e verbo por item;
    - uso por verbo: Comer 150, Beber 120, Fumar 210, Cheirar 120, Engolir 90, Inalar 120 passos;
    - 7 caras de efeito (`eletrico`, não `acelerado`) e 6 gestos no fim dos enums;
    - dominante só entre as 14 caras;
    - L16: tempos na parede e pendurado escalados por 100/Velocidade.
  - **Depois:**
    1. mesclar a arte na árvore principal;
    2. workflow 2: T2 (menu de emoção com rostos), T7 (apresentação), T8 (janelas dos itens, submenu "Itens", temporizador da onda) e T9 (ligar a chave, integração e verificação de tela, avisando o usuário antes).
- **2026-10-01 01:30 — Claude:** o workflow `wf_cb6cb763-1bc` parou às 23:53 por **limite de sessão** (os revisores estavam no meio). Às 01:25 Claude o parou e o **retomou do mesmo run** (task `w3jyq0abq`); os resultados prontos vieram do cache. Relatórios em `scratchpad\bloco1\`.
  - **Docs do bloco A:** prontos. DEC-029 e DEC-030 registradas, com notas nas DEC-008, 010, 024 e 025; ARCHITECTURE, SECURITY, TODO (Fase 5 em andamento, P1–P5 [x]), PROMPT_MESTRE (todo lançador usa `--perfil-de-teste`), PRODUCT_SPEC, COMO_INICIAR e README.
  - **T1:** pronto. Emoção dominante no núcleo e no esquema v2.
  - **T3–T4:** prontos, Core 347. Tipos, tabelas e a onda da frente.
    - L16 só alonga os tempos quando a fase é mais lenta.
    - A onda continua com o personagem escondido.
  - **T5–T6:** prontos, com `testar.ps1` Release código 0, **Core 385**, referências 01–05 idênticas e a referência 07 revisada.
    - Itens, `USING`, onda de fundo, água, invariantes 22–29: 1.000 sequências e 471 mil eventos.
    - 100 mutações; uma revelou um buraco no gerador, que foi corrigido.
    - A chave `Tamagotchi` segue desligada.
  - **Arte A1–A4:** prontos na cópia `scratchpad\arte`, com App.Testes 128 (+11). Itens redesenhados olhando as prévias, 7 caras de efeito, poses de uso com cinemática inversa até a boca e o nariz, `UsosPixel.Sequencia` com as somas exatas, `EfeitosPixel`, `IconesDoMenu` com o recorte 40×32 de `expressoes.png` e o cigarro engordado para 8 px de altura.
    - Achado antigo: o chapéu eriçado encosta na linha 0 em andando-2 e 4 e em escalando-1 e 2, sem cortar nada.
  - **Ambiente:** o usuário abriu o Buzzy a partir de `bin\Release` às 01:15 (PID 37000). Não foi fechado; os agentes usam Debug se o Release travar.
- **2026-10-01 03:20 — Claude:** **bloco 1 do tamagotchi concluído e mesclado.**
  - Workflow `wf_cb6cb763-1bc` terminou: 10 agentes, 0 erros. Relatórios em `scratchpad\bloco1\`.
    - **corretor-nucleo:** os 9 achados de código aplicados, Core 393, entre eles:
      - uso na parede perto do chão volta à parede;
      - **regra da calma:** com a autonomia pausada ou o painel aberto, quem está agarrado sem estar preso desce ou se solta. O preso e o atento ficam. Mexe num caminho da Fase 4, sustentado pela DEC-022 item 4;
      - `DragEnd` sem `DragStart` larga o item;
      - formato de reprodução com lista fechada.
    - **corretor-arte:** espelhinho numa mão só, cotovelos para fora, gestos provisórios em `PosesPixel.DosGestos`, chapéu sem corte, fumaça e bolhas pela boca, energético verde-neon.
  - **Mescla da arte por Claude:** 34 arquivos da cópia `scratchpad\arte`, com hash conferido. A versão da árvore principal era igual à base da cópia; `ArquivoDeConfiguracoesTestes.cs` ficou com a versão da árvore principal.
  - **`tools/testar.ps1` depois da mescla, código 0:** Core **393**, portão 73, App **150** (+11 de integração), portão de APIs APROVADO, 0 vulneráveis.
  - Claude olhou as prévias `itens-8x.png`, `rostos-efeito.png`, `usos.png` e `icones-menu.png`: legíveis e no estilo. Isto é observação, não aprovação do usuário; a revisão visual e de tom do usuário segue pendente.
  - **Workflow `wf_399bbc4e-7c9` (bloco 2) lançado:**
    - docs do núcleo e da arte, em paralelo com a cadeia do app;
    - app: T2 (menu de emoção com rostos) → T7 (apresentação) → T8 (janelas dos itens, submenu Itens, temporizador) → T9 (liga `Tamagotchi` em `DoAplicativo`, `ItensIntegracaoTestes`, `VerificacaoTamagotchi` escrita mas NÃO executada) → 2 revisores (correção e regras) → corretor → docs do app.
    - Os agentes podem rodar `-Integracao` (o usuário foi avisado às 03:15); só Claude roda a verificação de tela e o `medir-desempenho`.
- **2026-10-01 08:50 — Claude:** **bloco 2 do tamagotchi (app) concluído pelo workflow `wf_399bbc4e-7c9`** (9 agentes, 0 erros, cerca de 5,6 h). Relatórios em `scratchpad\bloco2\`.
  - **Docs do núcleo e da arte:** DEC-027 e DEC-028 detalhadas; ARCHITECTURE (1, 2.3, 2.6, 2.9, 2.10, 2.12 e 2.16 nova); SECURITY 5, 7, 8.1 e 10; IDENTIDADE_VISUAL; TODO com a seção "Interação"; PROJECT_CONTEXT, DEVELOPMENT_LOG e README.
    - Correção: trocas de cara que não mudam nada são cerca de **40%**, não 36%.
  - **T2:** menu nativo com o submenu "Emoção dominante" ("Automática" + as 14 caras com os ícones 40×32 de `expressoes.png`, ampliados pelo DPI; sem ícones em alto contraste).
    - Usa `BitmapsDoMenu` (DIB 32 bpp, `DeleteObject` depois do `DestroyMenu`) e `InsertMenuItemW`, `CreateDIBSection` e `DeleteObject` em `Win32.cs`.
    - O log MENU mantém `fechado=` e ganhou contagens de bitmaps.
  - **T7:** apresentação.
    - Poses de uso por verbo e passo no chão; na parede, no cipó e no esconderijo, a pose do apoio com a cara do item.
    - Gestos novos, sobreposição da onda por fase e cache LRU de 16 MB.
    - Testes que amarram os enums do núcleo à arte.
  - **T8:**
    - `JanelaDoItem` (sem ativar) e `SpriteDoItem`;
    - `GerenteDosItens`, `Aplicacao.Itens.cs` com um segundo árbitro e `LigacaoDosItens`;
    - temporizador da onda de disparo único;
    - ordem Z por evento;
    - submenu "Itens" (13 + "Recolher itens");
    - logs ITEM e ONDA.
  - **T9:** **chave `Tamagotchi` LIGADA em `DoAplicativo`**.
    - `ItensIntegracaoTestes`.
    - `VerificacaoTamagotchi` (V1–V15) com `--fase tamagotchi`, escrita e NÃO executada.
    - `medir-desempenho.ps1 -Modo onda`.
  - **Revisores (correção e regras) e corretor:** todos os achados de código aplicados; B5 e B6 descartados como equivalentes.
    - `LeituraDoLog`: a marca do log tem assinatura, para sobreviver à rotação.
    - Exceção na exibição do menu ainda apaga os bitmaps.
    - Testes de minimizar no meio do arraste e de arrastar o personagem com um item na tela.
  - **Contagens do corretor:** `testar.ps1` código 0, Core 393, portão 73, App 249 (+22 de integração); `-Integracao` 271/271; GDI 32→32, USER 22→22; p95 do arraste do item 1,1 ms.
  - **NÃO VERIFICADO ainda:**
    - `Buzzy.Verificacao --injetar-input-na-tela --fase tamagotchi` (8 a 9 min, move o cursor);
    - `medir-desempenho.ps1 -Modo onda` (cerca de 11,5 min);
    - revisão visual e de tom pelo usuário.
    - A DEC-028 e o T9 não podem virar VERIFIED antes disso.
  - **Docs do app** (agente `docs:app`) também concluídos; conferir.
  - **Próximo:** conferir `testar.ps1` e `-Integracao`, avisar o usuário, rodar as verificações de tela (fases 1, 3 e 4 de regressão, mais tamagotchi) e a medição, e então sincronizar os docs.
- **2026-10-01 09:00 — Claude:** conferência do bloco 2.
  - **Executado por Claude:**
    - `tools/testar.ps1` (Release) código 0: Core **393**, portão 73, App **249** (+22 de integração), portão de APIs APROVADO, 0 vulneráveis;
    - `tools/testar.ps1 -Integracao` código 0: App **271/271**, em 83 s.
  - **Verificação de tela bloqueada pelo ambiente:** `--fase 1`, `3`, `4` e `tamagotchi` saíram com FALHOU logo no início, nas 2 tentativas.
    - O teste de acerto do Windows nos pontos do Buzzy em (1568,904)-(1696,1032) devolve uma janela de **outro processo**, que não é o receptor nem o Buzzy. Ela cobre o canto inferior direito da tela principal; a ativação do receptor em (1072,602) funcionou.
    - A ferramenta não identifica janelas alheias, de propósito. Provável notificação do Windows parada enquanto o usuário está ausente (sem input havia 5,6 h).
    - **Não é defeito do Buzzy**, e nada foi clicado.
    - O usuário foi chamado a fechar o que estiver no canto; depois, rodar de novo: `Buzzy.Verificacao --injetar-input-na-tela --fase 1|3|4|tamagotchi`.
  - **Rodando em segundo plano:** `medir-desempenho.ps1 -Modo onda` (só mensagens postadas; não depende do canto).
- **2026-10-01 09:10 — Claude:** **V13 medida por Claude**: `medir-desempenho.ps1 -Modo onda`, código 0, relatório `resultados/desempenho-20261001-085153.txt`.
  - **Preparação**, só com mensagens postadas: vodka invocada pelo menu (I, V), arrastada até ele, `Using` → `Settling`.
  - **Onda nos tempos da tabela:** Bebado/Subida/2 → Pico/2 (8 s) → Pico/1 (100 s) → Queda/1 (100 s) → fim (112,5 s). Foram 4 disparos únicos e **0** `RELOGIO|ligado=sim` na janela medida.
  - **CPU:** média de 0,005% com a onda e 0,000% depois; 0,003% em 10 min; p95 de 0,000%.
  - **Resto:** 0 filhos, 0 conexões, resolução do timer inalterada, memória +0,21 MB e encerramento limpo.
  - **O canto continua coberto:** uma janela *topmost* de outro processo em (1524,832)-(1920,1032), com 396×200 px, acima da barra de tarefas. A verificação de tela espera o usuário fechá-la.
- **2026-10-01 10:35 — Claude:** **tamagotchi verificado com input SINTÉTICO por Claude.**
  - **Ambiente:** às 10:00 o usuário liberou o canto da tela (a janela *topmost* alheia sumiu).
  - **Primeira rodada:**
    - `--fase 1`: 25 OK, 4 SIMULADO;
    - `--fase 3`: 34 OK, 2 N/A;
    - `--fase 4`: **INVÁLIDA** (o mouse se mexeu no meio);
    - `--fase tamagotchi`: 2 FALHOU, **ambas defeitos da ferramenta, não do Buzzy**:
      - V1 casava com a linha `NUCLEO|efeitoPendente=GravarPreferencias` em vez da transição;
      - o cenário "parede sem estar preso" clicava nele em movimento, e o clique não chegava. Ainda assim ele desceu ao pausar.
  - **Correção da ferramenta por Claude**, em `VerificacaoTamagotchi.cs`:
    - V1 exige a linha com `para=`;
    - o cenário da parede usa `PontoFirmeDoCorpo()` (vizinhos a ±6 px também opacos), com até 3 tentativas confirmadas por `NUCLEO Press`, e é INCONCLUSIVO, não FALHOU, se nenhuma chegar.
  - `testar.ps1` código 0 (Core 393, portão 73, App 249).
  - **Segunda rodada (10:23–10:31):**
    - `--fase 4`: **28 OK**, 0 falhas;
    - `--fase tamagotchi`: **46 OK, 1 N/A (V16, depende do P7), 0 falhas**. Passaram V1–V15, X1 (arrastar um item até o DISPLAY2), "Recolher itens", a parede sem estar preso e o foco de volta depois dos 50 menus.
  - **Status do tamagotchi:** automatizado, integração, tela (SINTÉTICO) e medição V13 de 10 min feitos. Pendentes:
    - [MANUAL]: revisão visual e de tom pelo usuário, temas e alto contraste, Narrador, conforto para agarrar itens pequenos;
    - [HW]: escalas mistas;
    - V16, depende do P7.
    - Não é VERIFIED enquanto o [MANUAL] não for feito.
  - **Próximo:** workflow do **bloco B da Fase 5 (P6–P9)**, com a sincronização dos docs do tamagotchi em paralelo.
- **2026-10-01 10:40 — Claude:** workflow **`wf_247fdf06-c9a`** (Fase 5, bloco B) lançado:
  - em paralelo, os docs do tamagotchi verificado (TODO T9, DEC-027/028 STATUS, SECURITY 10, DEVELOPMENT_LOG com os incidentes da execução);
  - **P6** (chave estável do monitor) → **P7** (persistência ligada) → **P8** (topologia em execução) → **P9** (releitura robusta) → revisores de correção e de segurança → corretor → docs do bloco B.
  - O P7 inclui:
    - esquema **v3** com esconderijo e "preso" (decisão da DEC-029);
    - emoção no `Loaded`;
    - `Ler()` protegido;
    - V16 ligada na ferramenta (sem executar);
    - teste de integração que confere o `settings.json` real intocado.
  - O P8 inclui R14, `USING` como `REACTING` nas revalidações e itens rebaseados.
  - Os agentes podem rodar `-Integracao`. Só Claude roda a verificação de tela e a medição.

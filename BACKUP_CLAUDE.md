# BACKUP_CLAUDE.md — estado do trabalho do Claude (backup para continuidade)

> **Para o Codex (ou outro agente) continuar se o limite do usuário acabar.** O Claude atualiza este
> arquivo periodicamente (regra em `CLAUDE.md`). Ele descreve o trabalho em andamento, que pode ainda
> não estar nos documentos canônicos. Para *decisões*, valem `docs/` e `AGENTS.md`; para o *estado do
> trabalho em andamento*, vale este arquivo junto com os arquivos reais.
>
> **Última atualização:** 2026-09-29, ~23:50 (horário de Brasília). Nada foi commitado (o usuário não pediu).
>
> **Divisão do trabalho agora (pedido do usuário):** o **Codex está implementando a Fase 2** e vai registrar o
> estado dele em `BACKUP_CODEX`. O Claude faz **apenas a pixel art** (identidade visual) e não mexe em
> `src/Buzzy.Core/Personagem`, `src/Buzzy.App/Composicao/Aplicacao.cs` nem nos testes da Fase 2. As seções 3–5
> abaixo descrevem o que o Claude deixou pronto antes da troca; o que valer depois disso está no backup do Codex.

## 0. Resumo em 30 segundos

- **P3 (Etapa 0B) PASSOU** hoje às 21:44–21:47: 3 de 3 rodadas completas, 28 cenários OK em cada, inclusive
  B4b; input 100% sintético. Falta só **escrever isso nos docs** (TODO, DEVELOPMENT_LOG, PROJECT_CONTEXT).
- **Fase 1 implementada e verificada em tela** (sintético): integração 4/4, Verificação 25 OK / 4 SIMULADO /
  0 falhas, medição de repouso de 10 min limpa. Pendências que dependem do usuário: critério 3 pela bandeja
  real, critério 8 (escalas 150/200%) e critério 9 com troca real de resolução/escala/barra. **Não marcar a
  Fase 1 como concluída/VERIFIED** enquanto essas três estiverem pendentes (DEC-015).
- **Fase 2 em andamento:** núcleo (máquina de estados) escrito e compilando; **147/147 testes verdes**. Falta
  revisar à mão as referências gravadas 02–05, ligar o núcleo ao app (`Aplicacao.cs`) e rodar de novo os testes de tela.

## 1. Diretiva vigente e regras

- `prompt_usuario.md` (29/09): fechar P3, criar a identidade visual (feito) e implementar/testar/documentar as
  **Fases 1 a 11** na ordem de `docs/TODO.md`, **sem pedir aprovação** (DEC-015). [HW]/manual indisponível fica
  PENDENTE, nunca PASS/VERIFIED; continuar o trabalho independente.
- **Avisar o usuário antes de abrir janelas ou mexer no cursor.** Os harnesses esperam 20 s sem input e
  abortam se o usuário mexer (GetLastInputInfo). Os dez movimentos de mouse de 26/09 foram exploração
  informal da namorada do usuário: **não são evidência humana de P3**.
- Proibido: ler/inspecionar o Bloco de Notas ou outros apps; mudar configurações globais do Windows
  (inclusive escala de tela e visibilidade de ícones da bandeja); encerrar apps do usuário; publicar;
  chat/rede/IA. `SendInput` só em ferramentas de teste, rotulado SINTÉTICO.
- Manter este arquivo atualizado.

## 2. P3 — resultado (documentar)

- Comando: `spikes\SondaP3\bin\Release\net10.0-windows\SondaP3.exe --injetar-input-na-tela --repeticoes 3`
  (a opção explícita agora é obrigatória; sem ela sai com 2 sem abrir nada).
- Resultado: **3/3 rodadas com todos os cenários OK**: A1, A3, B7, B2, B3, B6-controle, B6 ClickLock, B5 ida e
  volta ao `\\.\DISPLAY2` (x negativo), B4a, **B4b** (Alt+Tab com Alt segurado 500 ms), clique seguinte depois
  de cada Alt+Tab, entrega dos 9 marcadores ao receptor com foco, limpeza (ClickLock, cursor, processos).
  M5 (latência do arraste): média 0,24–0,27 ms, p95 0,53–0,71 ms. Deriva do cursor: 0 px. Ambiente: Windows 11
  25H2 (26200), .NET 10.0.12, 2 monitores 1920×1080 a 96 DPI, limiar de arraste 4×4, ClickLock desligado.
- Relatórios (fora do Git): `spikes/resultados/p3-receptor.log`, `spikes/resultados/p3.log`.
- Limitações: tudo sintético; DPI misto sem hardware. Veredito: **P3 PASS no ambiente medido, evidência
  sintética**; DEC-006 não é reaberta. Com isso a Etapa 0B e a Fase 0 fecham tecnicamente.
- Correções no protótipo (spikes/): fim de gesto duplicado, dono novo da captura por lParam, soltar fora do
  limiar = arraste; receptor com cursor de texto no fim e máscara [A-Za-z0-9;-].

## 3. Fase 1 — o que foi verificado hoje

- `tools\testar.ps1` (sem tela): build da `Buzzy.slnx` com 0 avisos, portão de APIs **APROVADO** (4
  importações do apphost permitidas), auditoria 0 pacotes, testes verdes.
- `tests\Buzzy.App.Testes\bin\Release\net10.0-windows\Buzzy.App.Testes.exe --integracao`: **4/4 OK**
  (fumaça, instância única, agrupamento de topologia, recriação da bandeja). M6 ≈ 500 ms.
- `tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela`:
  **25 OK, 4 SIMULADO, 0 falhas**. Critérios 1, 2, 4, 5, 6, 6 [HW] (clique no sprite no monitor 2), 7 (pelos
  estilos), 10 OK. Critério 3 pelo menu do personagem OK; **pela bandeja: SIMULADO** — o ícone do Buzzy está na
  área de ícones ocultos do Windows 11 e o clique real caiu no botão "^". Relatório: `resultados/verificacao-fase1.log`.
- `tools\medir-desempenho.ps1` (critério 11): relatório `resultados/desempenho-20260929-215550.txt`. CPU
  **0,000%** de um núcleo em 600 s; trocas de contexto média **0,100/s** (p95 0, máx 18,9); memória privada
  56,5 MB estável (−0,38 MB/h); GPU 0%; sem filhos; sem rede; resolução do timer não mudou por causa do Buzzy.
- **Pendentes (dependem do usuário):** critério 3 pela bandeja real (o usuário pode fixar o ícone visível ou
  testar à mão); critério 8 (100/150/200% exige mudar a escala do Windows); critério 9 com troca real (as
  mensagens foram simuladas no teste de integração e passaram).
- Correções de hoje no produto: (1) `JanelaDeServico` distingue menu da bandeja aberto pelo mouse
  (WM_RBUTTONUP antes de WM_CONTEXTMENU) ou pelo teclado; `Aplicacao.AbrirMenu` só chama `NIM_SETFOCUS` quando
  foi pelo teclado (antes podia roubar foco depois de um cancelamento com o mouse); log `MENU|peloTeclado`.
  (2) `JanelaPersonagem` ignora `DpiChanged` quando o DPI não mudou (o WPF avisa à toa depois de mostrar).
- Correção nos testes: `WM_SETTINGCHANGE` não pode ser postada entre processos (PostMessage falha com 1159);
  o teste agora usa `SendMessageTimeout`. A Verificação posta WM_RBUTTONUP antes de WM_CONTEXTMENU e
  `Nativo.OciosoMs` usa diferença com sinal.

## 4. Documentação — feito e falta

- **Feito hoje:** `docs/DECISIONS.md` DEC-016 (estrutura, executor sem NuGet, menu nativo, bandeja, instância
  única, recusa elevado, log, sprite, portão + exceção do apphost, limitação do SessionEnding) e DEC-017
  (identidade em fontes vetoriais); `docs/SECURITY.md` (exceção do apphost em 3.2; implementações em 8.1 e 8.5);
  `docs/ARCHITECTURE.md` (seção 1 com o implementado, menu nativo em 2.13.1/2.13.5, linha no histórico);
  `docs/IDENTIDADE_VISUAL.md` (linha `bocejando`; gestos existentes são olhando/cocando/espreguicando).
- **Falta (fazer já, na ordem do AGENTS.md):**
  1. `docs/TODO.md`: marcar P3 e o fechamento da Fase 0 com data e evidência; tabela P3; Fase 1 com o status
     acima (implementada, verificação sintética, pendências 3-bandeja/8/9); tarefa da identidade visual [x];
     "Fase atual" → Fase 2 em andamento com Fase 1 aguardando pendências do usuário.
  2. `docs/PROJECT_CONTEXT.md`: estado atual + **instruções de build e teste** (critério da Fase 1):
     `dotnet build Buzzy.slnx -c Release`; `powershell -File tools\testar.ps1`; os três executáveis de tela
     acima (avisar o usuário); `tools\medir-desempenho.ps1`; `--diagnostico` grava em `%LOCALAPPDATA%\Buzzy`.
  3. `docs/DEVELOPMENT_LOG.md`: marcos de 29/09 — P3 (método, resultado, limitações), fechamento da Fase 0,
     Fase 1 (implementação, verificação, pendências), identidade visual, início da Fase 2.
  4. `docs/SECURITY.md` seção 10: portão aprovado em todo build; rede zero e sem filhos na medição de 10 min.
  5. `spikes/README.md`: opção `--injetar-input-na-tela` e faixas novas da SondaP3.
  - Feito depois (pedido do usuário): `COMO_INICIAR.md` na raiz (compilar, abrir, usar, problemas comuns),
    com link no `README.md`. Build conferido: `dotnet build src\Buzzy.App\Buzzy.App.csproj -c Release` → 0 erros,
    portão APROVADO. Quando a Fase 2 ligar o núcleo ao app (ou a Fase 3 trouxer o arraste), atualizar o aviso
    de "Estado atual" desse arquivo.
  6. `docs/ARCHITECTURE.md` 2.6/histórico — esclarecimentos da Fase 2 (ver seção 5).
  - Rascunho antigo em `%TEMP%\claude\...\scratchpad\rascunho-docs.md`: **tem uma frase errada** ("os gestos
    humanos de 26/09 continuam sendo evidência") — não copiar.

## 5. Fase 2 — núcleo do personagem (em andamento)

- **Código novo (compila, 0 avisos):** `src/Buzzy.Core/Personagem/` — `Tipos.cs` (enums `Estado` com os nomes da
  spec, grupos, motivos, energia, 14 expressões, gestos, sinais de movimento, `AcoesAutonomas`), `Eventos.cs`
  (um record por evento da tabela de 2.6 + `Loaded`, `MovementSignal`, `ExpressionChange`; `Origem` = prioridade;
  `MonitoresOcupados`; `Preferencias`), `Efeitos.cs` (MoverJanela, Mostrar/Esconder, Ligar/DesligarRelogio,
  AgendarDecisao(atraso, geração), CancelarDecisao, LiberarCaptura, AbrirMenu, painel, GravarPosicao,
  GravarPreferencias, Encerrar; `Transicao`), `Aleatorio.cs` (SplitMix64 como valor), `Configuracao.cs`
  (`ConfiguracaoDoNucleo`: tamanho, passos, reação 36, pouso 12, acomodação 3 s, `QuedaFisica` (Fase 4),
  `PainelDeEnergiaDisponivel` (Fase 8), `Acoes`; `PerfilDeEnergia` Baixa/Média/Alta), `EstadoDoNucleo.cs`
  (estado imutável + `Retrato`), `Maquina.cs` (função pura `Aplicar(estado, evento, config)`), `Nucleo.cs`
  (fila com prioridade; descarta evento autônomo com o usuário no controle), `Gravacao.cs` (gravação/reprodução
  em texto).
- **Decisões de desenho (registrar no ARCHITECTURE/DECISIONS):** SETTLING é transitório (resolve no mesmo
  evento); sem apoio → FALLING só com `QuedaFisica`, senão prende no chão (TODO Fase 3); monitor da âncora = o
  que contém o pixel logo acima dela; relógio só com movimento, reação, pouso ou gesto; um temporizador único
  com geração (disparo antigo é ignorado); precedência de ocultação Usuário > Sessão > Suspensão > Tela cheia
  (esclarecimento: RESUMED não mostra com a sessão ainda bloqueada); PRESSED + DRAG_CANCEL → SETTLING; esconder
  ou sair no meio do arraste fixa a posição validada do cursor; topologia com a mesma impressão digital não
  revalida; sinais de movimento (parede, borda, contato com o chão) são injetados pelos testes até a Fase 4.
- **Testes novos:** `tests/Buzzy.Core.Testes/Personagem/` — `TransicoesTestes.cs` (uma verificação por linha da
  tabela de 2.6, critério 1), `InvariantesTestes.cs` (2000 sequências × 200 eventos: invariantes 1, 2, 5, 6, 7,
  8, 9, 10, 11, 14, 15 e critério 3 do relógio — **passa**), `ReproducaoTestes.cs` + `Referencias/01..05*.txt`
  (reproduções gravadas), `FilaEAleatorioTestes.cs`, `Cenario.cs` (monta estados pela própria máquina).
- **Estado exato (22:27):** `Buzzy.Core.Testes.exe` → **147/147 verdes** depois de recompilar (a falha anterior
  era a cópia antiga das referências em `bin`). Para regravar:
  `$env:BUZZY_ATUALIZAR_REFERENCIAS='1'` e rodar com `--filtro Reproducao`, depois recompilar.
  **Revisar à mão** as saídas de `02`, `03`, `04` e `05` antes de aceitar (a `01` já foi revisada e está certa;
  a `02` mudou com a correção do arraste interrompido; a `03` foi reescrita com as chaves de monitor corretas).
- **Falta na Fase 2:**
  1. Fechar as referências (acima) e rodar `tools\testar.ps1`.
  2. **Ligar o núcleo ao app** (`src/Buzzy.App/Composicao/Aplicacao.cs`): criar `Nucleo` com
     `ConfiguracaoDoNucleo { Tamanho = SpriteProvisorio.TamanhoLogico, Acoes = Descansar | TrocarExpressao }`
     (sem física nem painel nesta fase), semente de `Environment.TickCount64` registrada no log; método
     `Enviar(evento, motivo)` que enfileira, processa (reentrância: se já processando, só enfileira) e executa os
     efeitos com o motivo do evento (dicionário por referência). Mapear: início → `Loaded`; bandeja Selecionar e
     segunda instância → reler topologia + `CmdShow`; menu → `CmdHide`/`CmdShow`/`CmdExit`; minimizar →
     `CmdHide`; WM_CLOSE → `CmdExit`; SessionEnding → `SessionEnding`; agrupador de topologia → `TopologyChanged`;
     botão direito no personagem → `Adiar(() => Enviar(new ContextMenu(p)))` → efeito `AbrirMenu`. Efeitos:
     MoverJanela = `AplicarNaJanela`; Mostrar/Esconder mantêm os logs `VISIVEL|visivel|motivo` (os testes de tela
     dependem deles, e de `POSICAO`, `JANELA`, `SERVICO`, `TOPOLOGIA`, `MENU|aberto=bandeja`); AgendarDecisao =
     DispatcherTimer de disparo único que entrega `AutonomyTimer(geração)`; relógio = DispatcherTimer ~15 ms com
     acumulador de passo fixo (só ligado por efeito). Registrar transições no log (`NUCLEO|de|para|regra`).
     Se já estiver visível e chegar CmdShow, manter `ReafirmarTopo` + log `VISIVEL sim` (compatível com os testes).
  3. Avisar o usuário e rodar de novo: integração, Verificação e uma medição curta de repouso (critério 4:
     "o app da Fase 1 continua funcionando com o núcleo ligado").
  4. Docs da Fase 2 (TODO, DEVELOPMENT_LOG, ARCHITECTURE 2.6 esclarecimentos, DEC nova se preciso).

## 6. Identidade visual — REFEITA EM PIXEL ART (ver `docs/IDENTIDADE_VISUAL.md`, DEC-018)

- O usuário não gostou da direção vetorial e pediu algo **mais fiel às pranchas, em pixel art**. Nova direção:
  macaquinho azul-marinho (#283A5F), rosto creme (#FDD5A6), olhos castanhos, orelhas pêssego (#F6996D), cauda com
  ponta creme, tufo bagunçado no alto da cabeça no lugar do chapéu (chapéu e faixa vermelha continuam proibidos
  pela PRODUCT_SPEC). Quadro **64 × 64 pixels** mostrado em 128 DIP (2×/3×/4× em 100/150/200%).
- Gerador: `src/Buzzy.Visual/Pixel/` (`Paleta`, `Tela`/`Mascara` com contorno e sombra automáticos, `Carimbo`,
  `Rostos` — 14 expressões em carimbos desenhados à mão —, `BonecoPixel` — esqueleto posável —, `PosesPixel` —
  21 poses, inclusive `espiando` (campo `Borda`: só cabeça e mãos acima de uma borda) e `brincando` —, `Icone` —
  ícone de bandeja 16×16 desenhado à mão). Tufos de pelo nas bochechas como nas pranchas. Prévias: `dotnet run --project
  tools/Buzzy.Identidade -c Release` (padrão = pixel; `--vetorial` = direção antiga arquivada). Saída em
  `assets/identidade/pixel/` (`buzzy-poses.png` = folha nativa; `previa/` = ampliações). Nenhuma pose na borda.
- Direção vetorial antiga arquivada (não apagada) em `assets/identidade/arquivo-vetorial/`; DEC-017 SUPERSEDED.
- Docs já atualizados: IDENTIDADE_VISUAL.md (reescrito), DECISIONS.md (DEC-018), ARCHITECTURE.md, PROJECT_CONTEXT.md,
  TODO.md (trabalho visual), DEVELOPMENT_LOG.md. **O app ainda mostra o sprite provisório**: o usuário pediu "faça
  apenas a pixel art" — não integrar no app agora (a integração é da Fase 6).

- **Atualização ~23:45 — DEC-019 (pedido do usuário):** o Buzzy deve lembrar o Luffy DE PROPÓSITO — chapéu de palha
  com faixa vermelha e personalidade do Luffy ("a ideia central do projeto é essa"). Feito: chapéu na pixel art
  (reage às emoções: salta no susto/risada, desce no sono), esqueleto 1 px menor para caber, ícone 16×16 com chapéu;
  PRODUCT_SPEC (Visão reescrita, sem regras de distância), DECISIONS (DEC-019; DEC-002/014/018, Q-17/Q-23
  atualizados), IDENTIDADE_VISUAL, PROMPT_MESTRE do Codex, ARCHITECTURE, TODO, PROJECT_CONTEXT, DEVELOPMENT_LOG,
  DECISOES_DO_USUARIO e COMO_INICIAR alinhados.
- **O app agora mostra a pixel art** (pedido do usuário para executar o app): `src/Buzzy.App/Apresentacao/SpriteProvisorio.cs`
  usa `BonecoPixel`/`Icone` (mesma interface; `Aplicacao.cs` não foi tocado — é do Codex); `Buzzy.App.csproj` referencia
  `Buzzy.Visual` e o portão lê `src/Buzzy.Visual` também. Build do app: 0 erros, portão APROVADO; `Buzzy.App.Testes`
  18/18 sem janela. O Buzzy foi aberto (Start-Process) às ~23:40 — **se o Codex for rodar testes de tela, feche-o antes
  (botão direito → Sair)**, senão os testes abortam por "Buzzy já aberto".

## 6b. Identidade vetorial anterior (histórico, substituída)

- Sagui-acrobata violeta-índigo (#574AA0), rosto pêssego, olhos e ponta da cauda menta (#3CCFA3), topete de 3
  tufos, cauda em espiral. Fontes: `assets/identidade/buzzy-partes.svg` e `assets/identidade/buzzy-poses.json`
  (19 poses, 14 expressões). Renderizador `src/Buzzy.Visual`; prévias por `tools/Buzzy.Identidade` (0 problemas).
  Integração animada na Fase 6 (espiar e brincar ainda sem pose).

## 7. Fases seguintes (TODO.md)

3 input/arraste (reconhecedor de gestos puro no Core + captura no adaptador; P3 já validou a janela) → 4
movimento (produz os `MovementSignal`; liga `QuedaFisica`) → 5 multi-monitor + persistência (P5/P6 [HW]) → 6
animação com a identidade (investigar 60 qps e memória de P2) → 7 personalidade → 8 configurações + painel +
tela cheia (P7; `GetForegroundWindow` só entra aqui, DEC-013) → 9 segurança → 10 verificação integrada → 11
desempenho.

## 8. Armadilhas conhecidas

- `.ps1` em UTF-8 **com BOM**; `[IO.File]` usa o diretório do processo (use caminhos absolutos).
- **Não usar `cd` no Bash** do Claude Code: muda o diretório de trabalho da sessão.
- Heredoc do Bash come barras invertidas (`\\.\DISPLAY1` virou `\.\DISPLAY1`): gerar esses arquivos com
  node e `String.fromCharCode(92)`.
- `PostMessage(WM_SETTINGCHANGE)` a outro processo falha (1159): use `SendMessageTimeout`.
- As referências gravadas são copiadas para `bin` no build: recompile depois de regravar.
- Na mesma leva, a fila processa `Press` antes de `Loaded` (prioridade): nos testes, processe a carga antes.
- XML de csproj não aceita `--` em comentário; projetos WPF não têm `System.IO` nos usings implícitos.
- `INPUT` do SendInput = 40 bytes em x64; tool window não fecha com CloseMainWindow (use WM_CLOSE).
- O ícone novo da bandeja fica na área de ícones ocultos do Windows 11: clique real no ícone não é possível
  sem o usuário fixá-lo (não mudar essa configuração por conta própria).
- Processo de teste precisa de manifesto PMv2 para ver coordenadas físicas (já feito nos testes).
- Máquina: Windows 11 25H2, Ryzen 7 7800X3D, RTX 5060, 2 monitores 1920×1080 a 100% (secundário em x negativo).

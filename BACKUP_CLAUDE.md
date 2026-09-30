# BACKUP_CLAUDE.md — estado do trabalho do Claude (backup para continuidade)

> **Para o Codex (ou outro agente) continuar se o limite do usuário acabar.** O Claude atualiza este
> arquivo periodicamente (regra em `CLAUDE.md`). Ele descreve o trabalho em andamento, que pode ainda
> não estar nos documentos canônicos. Para *decisões*, valem `docs/` e `AGENTS.md`; para o *estado do
> trabalho em andamento*, vale este arquivo junto com os arquivos reais.
>
> **Última atualização:** 2026-09-29, ~21:27 (horário de Brasília).

## 1. Diretiva vigente

- `prompt_usuario.md` (atualizado pelo usuário em 29/09): fechar a Etapa 0B (P3) com o harness isolado,
  **criar a identidade visual original agora** e implementar/testar/documentar as **Fases 1 a 11** na
  ordem de `docs/TODO.md`, **sem pedir aprovação** de plano, conceito ou fase (DEC-015). [HW] indisponível
  fica PENDENTE, sem PASS/VERIFIED; continuar o trabalho independente.
- Regras do usuário nesta sessão: **avisar antes de abrir janelas ou mexer no cursor** (ele usa o PC);
  manter este backup atualizado; os dez movimentos de mouse de 26/09 foram exploração informal da
  namorada do usuário, **não evidência humana de P3** (P3 se apoia só na evidência sintética rotulada).
- Os documentos foram enxugados pelo Codex/usuário em 29/09 (DEVELOPMENT_LOG tem só marcos; PROJECT_CONTEXT
  é curto). Reler antes de editar.

## 2. P3 (Etapa 0B) — falta só a nova rodada completa

- Receptor: `spikes/BuzzySpike/Receptor.cs` (`--modo receptor`). Harness: `spikes/SondaP3` (C#).
  Relatório: `spikes/resultados/p3-receptor.log` (fora do Git).
- 3 rodadas de 19:35: todos os cenários exigidos OK 3/3 (A1, A3, B7, B2, B3, B6-controle, B6 ClickLock,
  B5 ida/volta ao monitor em x negativo, B4a, entrega dos marcadores com foco, limpeza). B4b (Alt+Tab
  segurado) não rodou: a salvaguarda recusou clicar numa janela alheia que cobria o ponto de reativação.
  Corrigido (volta por Alt+Tab). **Falta reexecutar** `SondaP3.exe --injetar-input-na-tela --repeticoes 3`
  (a opção explícita está sendo acrescentada agora pelo workflow `wf_cc841001-85e`).
- Correções do protótipo: fim de gesto duplicado, dono novo da captura por lParam, soltar fora do limiar
  sem movimento = arraste. Documentar no DEVELOPMENT_LOG depois da rodada.

## 3. Fase 1 — implementada, falta verificação de tela e docs

### Feito (tudo compila com 0 avisos; `dotnet build Buzzy.slnx -c Release`)
- `Buzzy.slnx` (10 projetos, contando Buzzy.Visual e Buzzy.Identidade), `global.json`, `Buzzy.Build.props` + `Directory.Build.props` em src/tests/tools.
- `src/Buzzy.Core` (puro): Geometria, Topologia (`MonitorDoDesktop`, impressão digital invariante de
  cultura, lista somente leitura), Posicionador (âncora = centro da base; inicial no chão do principal a
  85%; Reacomodar). **88 testes verdes** em `tests/Buzzy.Core.Testes` (17 topologias de exemplo, 5 testes
  de propriedade com 5000 casos, semente 20260929).
- `src/Buzzy.App` (WPF): janela layered que não ativa, sprite provisório gerado em código (128×128 DIP,
  alfa 0/255), bandeja v4 (+ formato antigo de reserva, recriação por TaskbarCreated com ícone no DPI
  atual, NIM_SETFOCUS), menu nativo com dono temporário (registra `MENU|exibindo|dono|donoEmPrimeiroPlano`),
  instância única (mutex+evento `Local\...SID`), recusa rodar elevado (código 5), topologia com
  agrupamento de 300 ms + até 3 novas tentativas, escondido só atualiza a topologia, WM_GETDPISCALEDSIZE,
  log `--diagnostico` em `%LOCALAPPDATA%\Buzzy\diagnostico.log`.
- **Portão de APIs** (`tools/Buzzy.PortaoApis`, 73 testes verdes em `tests/Buzzy.PortaoApis.Testes`)
  roda depois de cada build do app (`AfterTargets="Build"` no csproj): **APROVADO**, 4 importações do
  apphost do SDK permitidas e explicadas (ShellExecuteW, GetProcAddress, LoadLibraryExW, LoadLibraryA)
  — **precisa de DEC nova + ajuste em SECURITY.md 3.2/8.1** (achado confirmado da revisão).
- `tests/Buzzy.App.Testes`: 14 testes sem janela verdes; 4 `[Integracao]` não executados.
- `tests/Buzzy.Verificacao`: verificação sintética dos critérios manuais (não executada).
- `tools/medir-desempenho.ps1` (não executado; 68 testes de componentes do agente), `tools/testar.ps1`.
- Revisão adversarial `wf_eda47e7e-33e`: 15 achados confirmados; os do produto foram corrigidos
  (elevação, exceções da instância única, topologia escondido, menu decide pelo estado ao abrir +
  EndMenu ao encerrar, bandeja robusta, DPI, log com rotação separada). Os das ferramentas estão sendo
  corrigidos agora pelo workflow `wf_cc841001-85e`.

### Pendente na Fase 1
1. Integrar o resultado do workflow das ferramentas; `tools\testar.ps1` completo verde.
2. **Avisar o usuário** e rodar o lote de tela: SondaP3 (P3 com B4b), `Buzzy.App.Testes.exe --integracao`,
   `Buzzy.Verificacao.exe --injetar-input-na-tela`, `tools\medir-desempenho.ps1` (10 min, critério 11).
3. Critérios 8 (escalas 150/200%) e 9 (trocar resolução/escala/barra de verdade) não são verificáveis sem
   mudar configurações globais (proibido): registrar PENDENTE. Critério 3 pela bandeja real: provavelmente
   SIMULADO (ícone na área oculta do Windows 11).
4. Docs: DEVELOPMENT_LOG (P3 + Fase 1), PROJECT_CONTEXT (instruções de build/teste), TODO, DECISIONS (nova
   DEC: executor sem NuGet — MSTest traz telemetria; menu nativo; instância única; log; sprite; exceções do
   apphost; limitação do SessionEnding do WPF), SECURITY (apphost), ARCHITECTURE (menu nativo em vez de WPF).

## 4. Identidade visual — criada (~21:40)
- Documento canônico: `docs/IDENTIDADE_VISUAL.md` (conceito, silhueta, proporções, paleta, estilo, regras
  técnicas, 14 expressões, 19 poses, distinção/originalidade, arquivos, histórico).
- Conceito: **sagui-acrobata violeta-índigo** (#574AA0), rosto em coração pêssego (#F6DFC6), olhos e ponta da
  cauda **menta** (#3CCFA3), topete de 3 tufos que reage à emoção, orelhas redondas baixas, braços ~48 DIP,
  cauda em espiral; sem roupa/acessório; paleta sem vermelho/azul-marinho/amarelo-palha.
- Fontes editáveis: `assets/identidade/buzzy-partes.svg` (63 partes, subconjunto de SVG com classes da paleta)
  e `assets/identidade/buzzy-poses.json` (poses como árvores de nós; expressões como camadas @olhos etc.).
- Renderizador reutilizável: `src/Buzzy.Visual` (BibliotecaDePartes + Boneco; borda dura 0/255) — será usado
  pelo app na Fase 6. Ferramenta de prévias: `tools/Buzzy.Identidade` (renderiza fora da tela; confere que toda
  pose cabe no quadro 128×128 e usa partes existentes — hoje: **0 problemas**). Prévias em
  `assets/identidade/previa/` (folha-de-modelo, poses claro/escuro, expressões, silhuetas, tamanho real).
- Ambos os projetos entraram na `Buzzy.slnx`. A Fase 1 continua com o placeholder (`SpriteProvisorio`).

## 5. Fases seguintes (ordem de TODO.md)
Fase 2 núcleo (máquina de estados de ARCHITECTURE 2.6) → 3 input/arraste → 4 movimento → 5 multi-monitor +
persistência (P5/P6 [HW]) → 6 animação com a identidade → 7 personalidade → 8 configurações + painel de
energia + tela cheia (P7) → 9 segurança → 10 verificação integrada → 11 desempenho.

## 6. Armadilhas conhecidas
- `.ps1` em UTF-8 **com BOM**; `[IO.File]` usa o diretório do processo (use caminhos absolutos — um
  comando meu criou arquivos vazios na raiz por isso; já removidos).
- XML de csproj não aceita `--` dentro de comentário.
- Projetos WPF não têm `System.IO` nos usings implícitos.
- `INPUT` do SendInput = 40 bytes em x64; tool window não fecha com CloseMainWindow (use WM_CLOSE).
- `GetLastInputInfo` também é atualizado pelo input injetado.
- Depois de Alt+Tab, `SetWindowPos(HWND_TOP)` de outro processo não passa à frente do primeiro plano.
- Processo de teste sem manifesto PMv2 vê coordenadas virtualizadas (corrigido nos testes pelo workflow).
- Máquina: Windows 11 25H2, 2 monitores 1920×1080 a 100% (secundário em x negativo).

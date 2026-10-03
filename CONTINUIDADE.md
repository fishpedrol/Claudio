# Continuidade compartilhada — Buzzy

> Único backup e registro de handoff entre Claude e Codex (regra em [AGENTS.md](AGENTS.md)). As fontes canônicas em `docs/`, o código e as instruções do usuário prevalecem; confirme no checkout antes de agir. O histórico até 2026-10-02 está em [docs/arquivo/continuidade-historico.md](docs/arquivo/continuidade-historico.md), que não é leitura obrigatória.

## Estado atual

Atualizado em 2026-10-02.

- **Git:** último commit `979bf82` (10:41 de 2026-10-02), do usuário. No checkout, sem commit: o P10 do Codex e, de Claude, o P11, a chave "Conteúdo adulto" (DEC-033), o P12, o P13 (DEC-032) e a documentação enxuta.
- **Fases e última validação:** [docs/PROJECT_CONTEXT.md](docs/PROJECT_CONTEXT.md); critérios e pendências em [docs/TODO.md](docs/TODO.md).
- **Cópia da Área de Trabalho** (`Desktop\net10.0-windows`, o Buzzy que o usuário abre): Release das 10:15 de 2026-10-02, sem nada do P10 em diante (nem a chave adulta, nem a travessia).

### Em andamento

Quadro de quem mexe em quê, pelo protocolo de [AGENTS.md](AGENTS.md). Uma linha por tarefa: agente — tarefa — arquivos ou áreas — início. Tire a linha ao terminar.

### Próximas ações em ordem

Conduzidas por Claude; o Codex pega uma delas quando o usuário pedir, registrando-a em "Em andamento".

1. **Verificação de tela com input SINTÉTICO, só com o ok do usuário** (move o cursor; nenhum Buzzy aberto, nem o da Área de Trabalho): a `--fase 5` nova (a travessia na máquina dele e soltar, fechar e reabrir; cerca de 2 min); a fase `tamagotchi` de novo, agora com a V20 e com a travessia ligada no app; o bloco B (`--fase 1`, `3` e `4`); e as medições de 10 minutos ([docs/TODO.md](docs/TODO.md), Fase 5).
2. **Fase 5, passo P14** (escala mista): depende do protótipo P6 [HW], sem monitores de DPI diferente aqui; dá para adiantar o gancho de DPI comum às janelas do personagem e dos itens e o limite de reaplicações causadas só pelo próprio `WM_DPICHANGED`. Depois, o P16 (gate da fase).
3. **Revisão pelo usuário** [MANUAL]: a lista do que é adulto (DEC-033, item 1: café e energético ficaram); a frequência da travessia (DEC-032, item 3); e o visual e o tom da paranoia, do alívio, da bala e do baseado por conta própria.
4. **Build novo para a cópia da Área de Trabalho**, com o Buzzy fechado, quando o usuário quiser.

## Log

Uma entrada por marco: `- **AAAA-MM-DD HH:MM — Agente: assunto.**` O que mudou; evidência (comando, resultado, arquivo); limitações; próximo passo. Decisões e requisitos vão para a fonte canônica, com link aqui. Entradas que não orientam mais o trabalho vão para o arquivo morto.

- **2026-10-02 02:21 — Claude: fim da leva de 2026-10-01.** Paranoia com um sorteio por mistura e baseado por conta própria prontos e documentados (DEC-028, itens 28 a 42; TODO, passos T11 e T12). `tools\testar.ps1 -Integracao` código 0: Core 492, Portão 74, App 364.
- **2026-10-02 10:09 — Claude: papéis redefinidos pelo usuário** (DEC-015, atualização de 2026-10-02): Claude principal; Codex segundo desenvolvedor e revisor, quando o usuário pedir. Substitui a passagem ao Codex pedida às 19:10 de 2026-10-01.
- **2026-10-02 10:19 — Claude: escolhas do usuário.** A frequência do baseado por conta própria e a bala dada sozinha ficam como estão (DEC-028, itens 39 e 41). A cópia da Área de Trabalho recebeu o Release das 10:15, com o SHA-256 conferido.
- **2026-10-02 10:32 — Claude: tela do tamagotchi**, com input SINTÉTICO e o ok do usuário, das 10:22 às 10:29: 56 OK e 0 falhas, inclusive V16 a V19 (`resultados/verificacao-tamagotchi.log`).
- **2026-10-02 — Codex: Fase 5, passo P10 (DEC-031).** Bloqueio, suspensão e retomada chegam ao núcleo ordenados pela topologia publicada; a retomada espera ao menos 1,5 s, provisório até o protótipo P5. `tools\testar.ps1 -Integracao` código 0: App 369, nenhuma integração ignorada.
- **2026-10-02 13:20 — Claude: Fase 5, passo P11 (DEC-031, adendo).** A suspensão descarrega a gravação pendente, e o mostrar relê a topologia na hora (`TOPOLOGIA|imediata=sim`), sem conferência tardia. 2 testes de integração novos, vistos falhando antes. `tools\testar.ps1 -Integracao` às 13:16, código 0: Core 492, Portão 74, App 371, arquivos reais intocados. Limitação: a suspensão real é o S12 [MANUAL][HW]. Próximo: P12.
- **2026-10-02 14:40 — Claude: chave "Conteúdo adulto", P12 e começo do P13; limite de uso.**
  - **Conteúdo adulto** (pedido do usuário; DEC-033 ainda por escrever): "Conteúdo &adulto" no menu, com marca, ligado por padrão. Desligado, o submenu "Itens" só tem banana, água, café e energético, e o núcleo tira os itens adultos, as ondas de substância (com a paranoia), o uso adulto em curso e o baseado por conta própria. Esquema do settings.json na v4 (`preferencias.conteudoAdulto`), invariante 30, V20 escrita e não rodada. `tools\testar.ps1 -Integracao` às 13:50: código 0; Core 509, Portão 74, App 374.
  - **P12 (minimização):** do sistema (releitura pendente, leitura diferente ou incoerente) não esconde; do usuário esconde (Q-03); já escondido, nunca vira CMD_HIDE. Bateria às 13:57: código 0, App 379. A regra "já escondido" e o teste dela entraram depois e só rodaram filtrados (7/7).
  - **P13a (em andamento, sem bateria):** portas pela área útil (`Passagens`), travessia plana atômica, sorteio na porta (`PesoAtravessar`) e a ação `IrAoOutroMonitor` (128, fora de `Todas`), ligada no app com `Travessia = true`. Em Debug: Core 526/526 e App sem janela 342/342. Faltam a bateria com integração, as mutações dos testes novos, o salto de degrau (P13b), o transbordo (P13c) e a DEC-032.
  - **Docs:** a limpeza dos .md por um agente foi parada no meio. O conjunto de partida caiu de ~118 KB para ~28 KB, com o histórico em `docs/arquivo/`; PROMPT_MESTRE_BUZZY.md e PROXIMA_SESSAO.md foram removidos. Os documentos grandes (TODO, ARCHITECTURE, DECISIONS) podem estar parcialmente enxugados: conferir links e os números citados pelo código antes de seguir. Os desenhos originais da Fase 5 (travessia, sistema e monitores) só existem no scratchpad da sessão `3b0e750b`, em `fase5/`: trazer para `docs/arquivo/`.
  - **Próximo passo:** bateria completa; docs de DEC-033, P12 e P13a; depois P13b e P13c. Nada de commit.
- **2026-10-02 16:57 — Claude: passo P13 completo e documentação em dia.**
  - **P13 (DEC-032):** além da travessia plana, o salto de degrau (até 480 DIP abaixo e 120 acima, com um solucionador de arco que mantém o sprite na união das áreas úteis) e o transbordo pela parede; o invariante 21; a referência gravada 06; 18 mutações reprovadas. O desenho original tinha dois furos, corrigidos: o pouso encostado na lateral de entrada não tinha arco válido, e nenhuma subida cabia saindo da beirada. `tools\testar.ps1 -Integracao` às 16:54, código 0: Core 536, Portão 74, App 380, arquivos reais intocados.
  - **Docs:** a limpeza do agente terminou (conjunto de partida 118 → 30 KB; Markdown vivo 729 → 453 KB; histórico integral e os desenhos da Fase 5 e do tamagotchi em `docs/arquivo/`; 0 links quebrados; as referências do código preservadas). Escritas a DEC-032, a DEC-033 e o adendo P12 da DEC-031, com TODO, PROJECT_CONTEXT, ARCHITECTURE (2.4, 2.5, 2.6, 2.9, 2.12 e 2.16), SECURITY, PRODUCT_SPEC, DEVELOPMENT_LOG, COMO_INICIAR e README.
  - **Limitações:** a travessia, a chave adulta e o P12 só têm evidência de testes automatizados e de integração; a V20 e a `--fase 5` não rodaram na tela; os valores da travessia são iniciais.
  - **Próximo passo:** "Próximas ações em ordem", no topo. Nada de commit.
- **2026-10-02 21:50 — Claude: passo P15 escrito, sem rodar.** `Buzzy.Verificacao --injetar-input-na-tela --fase 5` (`tests/Buzzy.Verificacao/VerificacaoFase5.cs`): T1, a travessia andando na máquina do usuário, com a semente escolhida por simulação; e S2/S7, soltar no monitor em x negativo, fechar e reabrir no mesmo lugar. Build Release sem avisos; sem `--injetar-input-na-tela`, a ferramenta recusa e sai com 2. Detalhes em [docs/TODO.md](docs/TODO.md), Fase 5, passo P15. Limitação: só roda com o ok do usuário (move o cursor). Próximo: "Próximas ações em ordem".

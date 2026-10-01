# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Resumo operacional. Requisitos ficam em PRODUCT_SPEC.md, fases e critérios em TODO.md, decisões em DECISIONS.md, arquitetura em ARCHITECTURE.md e segurança em SECURITY.md.
>
> Atualizado em 2026-10-01. STATUS: PLANNED. Fase 2 VERIFIED; Fases 1, 3 e 4 implementadas, com verificações manuais ou de hardware pendentes; a Fase 5 está em andamento (bloco A, passos P1–P5, implementado e verificado por testes). Intercalada com ela, a emoção dominante e o tamagotchi adulto (DEC-027 e DEC-028) têm o núcleo, a arte e o app verificados por testes automatizados, de integração, pela verificação de tela com input SINTÉTICO e pelo repouso de 10 minutos com uma onda ativa, com a chave do tamagotchi ligada no aplicativo; faltam a revisão visual e de tom pelo usuário e conferências [MANUAL] e [HW].

## Estado

- **Fase 0 / P3 — STATUS: VERIFIED no ambiente medido.** O harness isolado completou 3/3 rodadas com 28/28 cenários OK por rodada, incluindo B4b. Todo o input foi sintético (SendInput); não é validação humana. O resultado e os limites estão em [TODO.md](TODO.md) e `spikes/resultados/p3-receptor.log`.
- **Fase 1 — STATUS: PLANNED.** O shell do aplicativo está implementado. Build, testes automatizados, verificação em tela com input sintético e linha de base de dez minutos passaram nos limites registrados em TODO.md. Continuam pendentes a verificação real do menu pela bandeja, as escalas de 150% e 200% e a troca real de resolução/escala/barra de tarefas. Não declarar a fase concluída enquanto esses critérios estiverem pendentes (DEC-015).
- **Fase 2 — STATUS: VERIFIED em 2026-09-30.** O núcleo comanda a janela pela raiz de composição. A auditoria adversarial e a cobertura nova corrigiram as lacunas da tabela (DEC-020). O critério 4 foi verificado com input sintético.
- **Fase 3 — STATUS: PLANNED.** Clique, clique duplo, arraste, menu e captura do mouse funcionam, com o árbitro puro (DEC-021). Pendem UAC [MANUAL], escalas mistas [HW] e ClickLock ligado [MANUAL].
- **Fase 4 — STATUS: PLANNED.** O personagem anda, escala, pendura-se, pula, cai e descansa sozinho num monitor, com física de passo fixo no núcleo (DEC-022).
  - A pedido do usuário, tem toon force (DEC-023): sobe por qualquer lateral, inclusive a encostada no outro monitor; quica como borracha; às vezes sobe a parede num foguete; achata e estica.
  - Solto no alto, agarra um cipó na borda de cima; solto junto a uma lateral, gruda na parede. Posto lá pelo usuário, só sai quando o usuário o tira (DEC-024).
  - Dois cliques o escondem atrás da barra de tarefas ou de uma lateral, só com a cabeça e as mãos para fora; outros dois o tiram de lá (DEC-025). O painel de energia da Fase 8 abre pelo menu.
  - "Pausar movimento" no menu deixa ele quieto.
  - Pende a gravação de tela a 120 qps (critério 5).
- **Fase 5 — STATUS: PLANNED, em andamento.** O bloco A (passos P1–P5 da ordem registrada em [TODO.md](TODO.md)) está implementado e verificado por testes automatizados:
  - a posição guarda a tela do monitor da época, e a partida restaura em cascata: chave, tela do monitor, principal (DEC-030);
  - o esquema do `settings.json` fica no núcleo, e o arquivo, com gravação atômica, no adaptador (DEC-029); o esquema nasceu na v1 e passou à v2 com a emoção dominante (DEC-027);
  - testes e ferramentas abrem o Buzzy com `--perfil-de-teste NOME`, isolados das configurações reais (DEC-029).
  - **Ainda não ligado:** o app não lê nem grava o `settings.json` e ainda não lembra a posição; isso é o passo P7. O próximo passo da fase é o P6, a chave estável do monitor.
  - Os passos da Fase 5 se chamam P1 a P16 e, no texto, aparecem como "passo P7"; um P-número sem "passo" é um dos protótipos P1 a P10 da Etapa 0B.
- **Interação — emoção dominante e tamagotchi adulto (DEC-027, DEC-028) — STATUS: PLANNED, em andamento, intercalada com a Fase 5.** Os passos e as pendências estão na seção "Interação" de [TODO.md](TODO.md).
  - **Pronto e verificado (2026-10-01):** o núcleo (passos T1 e T3–T6), a arte (A1–A4) e o app (T2 e T7–T9), com as correções das revisões, por testes automatizados e de integração e pela verificação de tela com input SINTÉTICO (46 OK, 1 N/A e 0 falhas). O repouso de 10 minutos com a onda de uma vodka ficou em 0,003% de CPU de um núcleo, sem o relógio ligar. A chave `Tamagotchi` está ligada no aplicativo. Nada disso é gesto humano.
  - **O que o usuário já vê:** no menu do botão direito, "Emoção dominante", com os rostos da pixel art, e "Itens", com os 13 itens e "Recolher itens". O item cai ao lado dele, e, arrastado e solto sobre ele, é usado, com a animação do verbo e a onda de desenho animado. Como usar: [COMO_INICIAR.md](../COMO_INICIAR.md).
  - **Ainda não:** a escolha da emoção só sobrevive a reabrir o app depois do passo P7 da Fase 5; os itens somem ao sair, de propósito.
  - **Pendente:** a revisão visual e de tom pelo usuário e as conferências [MANUAL] e [HW] da seção "Interação" de [TODO.md](TODO.md): os temas claro, escuro e de alto contraste, o Narrador, o conforto para agarrar os itens pequenos e as escalas de 125% a 200%, também entre monitores de escalas diferentes. A V16, a emoção restaurada ao reabrir, depende do passo P7.
  - **Já valia no app antes da chave:** a regra da calma (DEC-022, item 4): pausado ou com o painel aberto, quem está agarrado à parede ou ao cipó sem estar preso desce ou se solta. Verificada por testes automatizados e, em 2026-10-01, na verificação de tela com input SINTÉTICO.
  - **Próximo passo:** o bloco B da Fase 5 (passos P6–P9). A revisão visual e de tom depende do usuário.
- **Identidade visual:** refeita em 2026-09-29 a pedido do usuário como **pixel art fiel às pranchas, com o chapéu de palha e a personalidade do Luffy — semelhança intencional** (DEC-018, DEC-019).
  - O app mostra poses provisórias dela por estado (Fase 4) e o ícone novo.
  - Ganhou a arte do tamagotchi (itens, caras novas, poses de uso, sobreposições e ícones do menu), conferida por testes e mostrada pelo app desde os passos T2, T7 e T8. A revisão visual e de tom pelo usuário está pendente.
  - Está documentada em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md). O gerador fica em `src/Buzzy.Visual/Pixel/`, a folha e as prévias em `assets/identidade/pixel/`; a direção vetorial anterior está arquivada em `assets/identidade/arquivo-vetorial/`.
  - As animações completas são da Fase 6.
- **Input humano:** nenhum gesto informal de 26/09 é evidência aprovada. Os resultados por SendInput permanecem classificados como sintéticos.

## Evidências atuais

O checkout, com o app da emoção dominante e do tamagotchi e a chave ligada, foi validado em 2026-10-01, por volta das 08:15, na última rodada da correção do app. `powershell -NoProfile -File tools/testar.ps1` (Release) saiu com código 0:
- 393 testes do Core;
- 73 testes do portão;
- 249 testes do app sem janela (mais 22 de integração, que só rodam com `-Integracao`);
- 0 avisos no build, portão de APIs aprovado, também sobre `src\Buzzy.Visual`;
- nenhum pacote vulnerável.

`tools/testar.ps1 -Integracao` também saiu com código 0, com 271/271 no app, por mensagens postadas às janelas do próprio Buzzy. As referências gravadas 01–05 continuam idênticas byte a byte. Detalhes na seção "Interação" de [TODO.md](TODO.md). Antes das verificações de tela, Claude repetiu as duas, com código 0 e as mesmas contagens.

Depois, ainda em 2026-10-01, Claude rodou as verificações de tela com input SINTÉTICO, com o Buzzy aberto no perfil de teste `verificacao`, e a medição de 10 minutos com uma onda ativa, no perfil `desempenho`:

| Verificação | Relatório | Resultado |
|---|---|---|
| Fase 1 (regressão) | `resultados/verificacao-fase1.log` | 25 OK, 4 SIMULADO (bandeja) |
| Fase 3 (regressão) | `resultados/verificacao-fase3.log` | 34 OK, 2 N/A |
| Fase 4 (regressão) | `resultados/verificacao-fase4.log` | 28 OK |
| Tamagotchi (`--fase tamagotchi`) | `resultados/verificacao-tamagotchi.log` | 46 OK, 1 N/A (V16, que depende do passo P7) |
| Repouso com a onda de uma vodka (V13, `-Modo onda`) | `resultados/desempenho-20261001-085153.txt` | 0,003% de CPU de um núcleo em 10 min, sem o relógio ligar; nenhum processo filho e nenhuma conexão |

Nenhuma verificação de tela da tabela teve falha. Os relatórios acumulam as execuções; a desta validação é a última de cada arquivo. As anteriores do mesmo dia falharam pelo ambiente (uma janela de outro programa no canto da tela) ou por dois defeitos da ferramenta, já corrigidos, ou saíram INVÁLIDAS porque o mouse foi mexido; nenhuma falha foi do Buzzy ([DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md)).

O bloco A não tem commit. Os commits `0699c08` e `87c4203` guardaram estados intermediários dele, e quatro arquivos de teste novos estão fora do Git: um commit só dos arquivos rastreados não compila (detalhes em [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md)). O núcleo, a arte e o app do tamagotchi também estão só no checkout, sem commit, com arquivos novos fora do Git.

A linha de base da Fase 1 está em `resultados/desempenho-20260929-215550.txt`. A medição de repouso durou 600 s e observou 0,000% de CPU de um núcleo, 56,67 → 56,51 MB de memória privada, nenhuma conexão TCP/UDP em 58 verificações e nenhum processo filho em 629 verificações. A linha de base curta não valida metas de 1 h ou 8 h. As medições com a Fase 4 estão em [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md).

O ambiente medido tinha Windows 11 25H2, build 26200, .NET 10.0.12 e dois monitores 1920×1080 a 96 DPI, com o secundário à esquerda. Escalas mistas e retrato permanecem pendentes.

## Build e verificações

Na raiz do repositório, usando PowerShell:

```powershell
dotnet build Buzzy.slnx -c Release
powershell -NoProfile -File tools\testar.ps1
```

`tools\testar.ps1` faz build, testes sem janelas, portão de APIs e auditoria de pacotes vulneráveis. Para a suíte de integração, use `powershell -NoProfile -File tools\testar.ps1 -Integracao`; janelas do Buzzy aparecem e somem, então avise o usuário antes.

Tudo o que abre o `Buzzy.exe` para testar usa um perfil de teste e, sem nenhum Buzzy aberto, apaga a pasta dele antes de abrir a primeira instância: `integracao` nos testes de integração, `verificacao` no `Buzzy.Verificacao` e `desempenho` na medição. Assim nada toca as configurações reais do usuário (DEC-029).

Verificações de tela — todas usam input sintético e exigem aviso prévio porque abrem janelas; a Sonda P3 e a Verificação também movem o cursor:

```powershell
spikes\SondaP3\bin\Release\net10.0-windows\SondaP3.exe --injetar-input-na-tela --repeticoes 3
tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela [--fase 1|3|4|tamagotchi] [--semente N]
tools\medir-desempenho.ps1 [-Modo repouso|autonomia|onda] [-Semente N]
```

`--semente N` só vale com `--fase tamagotchi`. Essa fase levou cerca de 5 minutos em 2026-10-01, com um repouso de 60 s em que nada deve ser tocado, e grava `resultados\verificacao-tamagotchi.log`. O `-Modo onda` abre o Buzzy pausado, entrega uma vodka por mensagens postadas às janelas do próprio Buzzy, sem `SendInput` nem mover o cursor, e leva cerca de 11 minutos, com 10 minutos de janela medida; o menu toma o primeiro plano por um instante, como sempre (DEC-016).

Opções de linha de comando do app:

| Opção | Efeito |
|---|---|
| `--diagnostico` | Grava o log local em `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB). Sem ela, o app não grava o log de diagnóstico. |
| `--pausado` | Começa com o movimento pausado, como esperam os testes de gesto, as verificações de tela e a medição de repouso. |
| `--semente N` | Fixa a agenda autônoma, para diagnóstico e testes. |
| `--perfil-de-teste NOME` | Isola os dados do Buzzy em `%LOCALAPPDATA%\Buzzy\testes\NOME`, para testes e ferramentas. NOME tem de 1 a 32 caracteres entre a–z, 0–9 e hífen, sem começar por hífen nem ser um nome reservado do Windows. Só vale com essa grafia exata, separada do nome por espaço; sem nome, com nome inválido ou com outra grafia (`--perfil-de-teste=NOME`, maiúsculas, `/perfil-de-teste`), a persistência fica desligada naquela execução. O log de diagnóstico continua na pasta do Buzzy. |

## Fontes canônicas

- Produto: [PRODUCT_SPEC.md](PRODUCT_SPEC.md)
- Fases e critérios: [TODO.md](TODO.md)
- Arquitetura: [ARCHITECTURE.md](ARCHITECTURE.md)
- Decisões: [DECISIONS.md](DECISIONS.md)
- Segurança: [SECURITY.md](SECURITY.md)
- Evidências e histórico: [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md), `resultados/` e `spikes/resultados/`
- Diretiva operativa: [prompt_usuario.md](../prompt_usuario.md)
- Continuidade entre Claude e Codex: [CONTINUIDADE.md](../CONTINUIDADE.md)

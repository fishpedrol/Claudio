# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Estado de cada fase, última validação e como validar. Atualizado em 2026-10-03. Critérios, passos, pendências e evidências ficam em [TODO.md](TODO.md); o andamento e as próximas ações, em [CONTINUIDADE.md](../CONTINUIDADE.md).

## Fases

| Fase | Status | O que falta |
|---|---|---|
| 0 — Etapa 0B, protótipos P1–P3 | VERIFIED no ambiente medido, com input sintético (2026-09-29) | escala mista [HW]; os protótipos P5–P8 e P10 entram nas fases que dependem deles |
| 1 — Shell do desktop (DEC-016) | PLANNED: implementada e verificada por automação e input sintético | bandeja real, escalas de 150% e 200%, troca real de resolução, escala e barra [MANUAL] |
| 2 — Núcleo do personagem (DEC-020) | VERIFIED em 2026-09-30 | — |
| 3 — Input e arraste (DEC-021) | PLANNED: implementada e verificada | UAC [MANUAL], escalas mistas [HW], ClickLock ligado [MANUAL] |
| 4 — Movimento e superfícies, com toon force, cipó, "preso" e esconderijo (DEC-022 a DEC-025) | PLANNED: implementada e verificada | gravação de tela a 120 qps, critério 5 [MANUAL] |
| 5 — Multi-monitor e posição persistida (DEC-029 a DEC-032) | PLANNED: implementada e verificada por automação e input sintético (P1–P16, gate em 2026-10-03); o app grava a posição, a postura, a emoção e a chave adulta em `%LOCALAPPDATA%\Buzzy\settings.json` (esquema v4), acompanha a topologia aberto, não se esconde quando o Windows o minimiza numa troca de monitores e atravessa entre monitores (andando, num salto de degrau ou pela parede) | S1–S12 [MANUAL][HW]; a escala mista real (S5, S10 e o protótipo P6); o protótipo P5; Process Monitor [MANUAL] |
| Modo de tela cheia (Q-09; DEC-013), antecipado pelo relato do usuário (DEC-034) | Implementado e verificado por testes automatizados e de integração: o observador da janela em primeiro plano, os usos restritos no portão de APIs, a travessia que não entra num monitor ocupado a chave "Desviar da tela cheia" no menu e o pulo até o cipó na troca de monitor (DEC-035) | a cópia da Área de Trabalho atualizada; a validação com o jogo do usuário, com `--diagnostico` (faz as vezes do protótipo P7); vídeo em tela cheia e jogo exclusivo [MANUAL][HW] |
| Interação — emoção dominante e tamagotchi adulto (DEC-027, DEC-028), com a chave `Tamagotchi` ligada, e a chave "Conteúdo adulto" (DEC-033) | PLANNED, em andamento: passos T1–T13 e arte A1–A4 feitos e verificados, inclusive na tela (V1–V20, input SINTÉTICO) | revisão visual e de tom pelo usuário, inclusive da lista do que é adulto; as conferências [MANUAL] e [HW] da seção |
| 6 — Rendering, animações e expressões (DEC-036) | PLANNED: implementada e verificada por automação (F6-P1–F6-P6, gate em 2026-10-03): manifesto de clipes embutido e validado no build, clipes animados novos, tendência de expressão por energia | revisão visual das animações pelo usuário [MANUAL] |
| 7 a 11 | não começadas | — |

Os passos da Fase 5 se chamam "passo P1" a "passo P16"; um P-número sem "passo" é um protótipo da Etapa 0B. Nada feito com input SINTÉTICO é evidência humana. Como usar o app: [COMO_INICIAR.md](../COMO_INICIAR.md).

## Última validação

2026-10-03, 17:48, gate da Fase 6 (DEC-036): `tools\testar.ps1 -Integracao` com código 0: Core 561, Portão 79, App 410, o manifesto aprovado no build Release, arquivos reais intocados. Medição com animação: na autonomia de 10 min com as animações novas, CPU média de 0,630% de um núcleo, cerca de 4,1% por segundo com o relógio ligado (92 s de 633; meta de animação de Q-08: até 5%), p95 de 4,65% e GPU em 0%; sem timer periódico, processo filho nem rede; memória privada +62 MB, com o heap vivo em 1,2 MB na primeira coleta do GC, aos 8 min 30 s (o orçamento da geração 0, como em 2026-09-30), o que fica para a medição de 1 h e 8 h da Fase 10 (`resultados/desempenho-20261003-174907.txt`).

Anterior: 2026-10-03, 13:36, pulo e pulinho da tela cheia (DEC-035) e esconderijo na borda de cima (DEC-025, item 6): `tools\testar.ps1 -Integracao` com código 0 — build Release sem avisos, Core 559/559, Portão 79/79, App 393/393 com a integração, portão de APIs aprovado com os três usos restritos no relatório, nenhum pacote vulnerável e os arquivos reais do usuário intocados.

2026-10-03, 13:56, passo P14: `tools\testar.ps1 -Integracao` com código 0 — Core 559/559, Portão 79/79, App 399/399, arquivos reais intocados.

Telas mais recentes, com input SINTÉTICO: em 2026-10-03, das 13:56 às 14:01, `--fase 5` com 6 OK, e as fases 1, 3 e 4 com 25 OK e 4 SIMULADO, 34 OK e 2 N/A, e 28 OK, sem falhas (`resultados/verificacao-fase5.log`, `-fase1.log`, `-fase3.log`, `-fase4.log`); tamagotchi com a V20 em 2026-10-03, das 14:44 às 14:51: 58 OK e 0 falhas (`resultados/verificacao-tamagotchi.log`), depois de duas tentativas invalidadas por uso do mouse.

Medições de 10 min de 2026-10-03, com o perfil `desempenho`: repouso a 0,000% de um núcleo, onda a 0,010%, autonomia a 0,221%; sem timer periódico, processo filho nem rede (`resultados/desempenho-20261003-140447.txt` (repouso), `-141534.txt` (autonomia) e `-142614.txt` (onda)).

Ambiente medido: Windows 11 25H2 (build 26200), .NET 10.0.12, dois monitores 1920×1080 a 96 DPI, o secundário à esquerda, em x negativo; sem escalas mistas nem retrato.

## Build e verificações

Na raiz do repositório, em PowerShell:

```powershell
dotnet build Buzzy.slnx -c Release
powershell -NoProfile -File tools\testar.ps1              # build, testes sem janela, portão de APIs, auditoria de pacotes
powershell -NoProfile -File tools\testar.ps1 -Integracao  # inclui a integração: janelas do Buzzy aparecem e somem
```

Tudo o que abre o `Buzzy.exe` para testar usa um perfil de teste (`integracao` e `persistencia` na integração, `verificacao` na tela, `desempenho` na medição), cuja pasta é apagada antes da primeira instância (DEC-029). Integração, tela e medição conferem por fora que os arquivos reais do usuário não mudaram e falham se mudarem; um Buzzy do usuário aberto durante elas pode causar isso.

Verificações de tela, com input sintético e aviso ao usuário antes (abrem janelas; a Sonda P3 e a Verificação movem o cursor):

```powershell
spikes\SondaP3\bin\Release\net10.0-windows\SondaP3.exe --injetar-input-na-tela --repeticoes 3
tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela [--fase 1|3|4|5|tamagotchi] [--semente N]
tools\medir-desempenho.ps1 [-Modo repouso|autonomia|onda] [-Semente N]
```

`--semente N` só vale com `--fase tamagotchi` (7 a 13 min, com um repouso de 60 s sem tocar em nada). A Verificação espera até 20 s sem uso do mouse e do teclado antes de começar e exige que não haja outro Buzzy aberto. O `-Modo onda` (cerca de 11 min) entrega uma vodka só por mensagens postadas, sem mover o cursor.

Opções do app: `--diagnostico` grava o log local `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB; sem ela, não há log); `--pausado` começa com o movimento pausado; `--semente N` fixa a agenda autônoma; `--sem-tela-cheia` não liga o observador de tela cheia (os testes de integração, menos o do próprio observador, e a verificação de tela usam, para o lugar inicial não depender do que estiver em tela cheia na máquina; DEC-034); `--perfil-de-teste NOME` isola os dados em `%LOCALAPPDATA%\Buzzy\testes\NOME` (regras do nome em [SECURITY.md](SECURITY.md) 3.1; outra grafia ou nome inválido desligam a persistência naquela execução). Sem perfil, o Buzzy lê e grava `%LOCALAPPDATA%\Buzzy\settings.json`.

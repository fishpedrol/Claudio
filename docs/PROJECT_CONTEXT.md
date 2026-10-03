# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Estado de cada fase, última validação e como validar. Atualizado em 2026-10-02. Critérios, passos, pendências e evidências ficam em [TODO.md](TODO.md); o andamento e as próximas ações, em [CONTINUIDADE.md](../CONTINUIDADE.md).

## Fases

| Fase | Status | O que falta |
|---|---|---|
| 0 — Etapa 0B, protótipos P1–P3 | VERIFIED no ambiente medido, com input sintético (2026-09-29) | escala mista [HW]; os protótipos P5–P8 e P10 entram nas fases que dependem deles |
| 1 — Shell do desktop (DEC-016) | PLANNED: implementada e verificada por automação e input sintético | bandeja real, escalas de 150% e 200%, troca real de resolução, escala e barra [MANUAL] |
| 2 — Núcleo do personagem (DEC-020) | VERIFIED em 2026-09-30 | — |
| 3 — Input e arraste (DEC-021) | PLANNED: implementada e verificada | UAC [MANUAL], escalas mistas [HW], ClickLock ligado [MANUAL] |
| 4 — Movimento e superfícies, com toon force, cipó, "preso" e esconderijo (DEC-022 a DEC-025) | PLANNED: implementada e verificada | gravação de tela a 120 qps, critério 5 [MANUAL] |
| 5 — Multi-monitor e posição persistida (DEC-029 a DEC-032) | PLANNED, em andamento: passos P1–P13 verificados por testes automatizados e de integração; o app grava a posição, a postura, a emoção e a chave adulta em `%LOCALAPPDATA%\Buzzy\settings.json` (esquema v4), acompanha a topologia aberto, não se esconde quando o Windows o minimiza numa troca de monitores e atravessa entre monitores (andando, num salto de degrau ou pela parede) | P14 (depende do protótipo P6 [HW]), P15 e P16; a tela do bloco B (`--fase 1`, `3` e `4`) e as medições de 10 min; S1–S12 [MANUAL][HW]; o protótipo P5 |
| Interação — emoção dominante e tamagotchi adulto (DEC-027, DEC-028), com a chave `Tamagotchi` ligada, e a chave "Conteúdo adulto" (DEC-033) | PLANNED, em andamento: passos T1–T13 e arte A1–A4 feitos e verificados, inclusive na tela (V1–V19, input SINTÉTICO) | a V20 na tela; revisão visual e de tom pelo usuário, inclusive da lista do que é adulto; as conferências [MANUAL] e [HW] da seção |
| 6 a 11 | não começadas; a identidade em pixel art (DEC-018, DEC-019) já aparece em quadro parado e poses provisórias | — |

Os passos da Fase 5 se chamam "passo P1" a "passo P16"; um P-número sem "passo" é um protótipo da Etapa 0B. Nada feito com input SINTÉTICO é evidência humana. Como usar o app: [COMO_INICIAR.md](../COMO_INICIAR.md).

## Última validação

2026-10-02, 16:54, fim do passo P13: `tools\testar.ps1 -Integracao` com código 0 — build Release sem avisos, Core 536/536, Portão 74/74, App 380/380 com a integração, portão de APIs aprovado, nenhum pacote vulnerável e os arquivos reais do usuário intocados.

Telas mais recentes, com input SINTÉTICO: tamagotchi em 2026-10-02, 56 OK e 0 falhas (`resultados/verificacao-tamagotchi.log`); fases 1, 3 e 4 em 2026-10-01, antes do bloco B: 25 OK e 4 SIMULADO, 34 OK e 2 N/A, 28 OK (`resultados/verificacao-fase1.log`, `-fase3.log`, `-fase4.log`).

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

Opções do app: `--diagnostico` grava o log local `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB; sem ela, não há log); `--pausado` começa com o movimento pausado; `--semente N` fixa a agenda autônoma; `--perfil-de-teste NOME` isola os dados em `%LOCALAPPDATA%\Buzzy\testes\NOME` (regras do nome em [SECURITY.md](SECURITY.md) 3.1; outra grafia ou nome inválido desligam a persistência naquela execução). Sem perfil, o Buzzy lê e grava `%LOCALAPPDATA%\Buzzy\settings.json`.

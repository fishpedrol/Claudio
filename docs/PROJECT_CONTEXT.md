# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Resumo operacional. Requisitos ficam em PRODUCT_SPEC.md, fases e critérios em TODO.md, decisões em DECISIONS.md, arquitetura em ARCHITECTURE.md e segurança em SECURITY.md.
>
> Atualizado em 2026-10-02. STATUS: PLANNED. Fase 2 VERIFIED; Fases 1, 3 e 4 implementadas, com verificações manuais ou de hardware pendentes; a Fase 5 está em andamento (blocos A e B, passos P1–P9, implementados e verificados por testes automatizados e de integração; a verificação de tela do bloco B, com input SINTÉTICO, está pendente). Intercalada com ela, a emoção dominante e o tamagotchi adulto (DEC-027 e DEC-028) têm o núcleo, a arte e o app verificados por testes automatizados, de integração, pela verificação de tela com input SINTÉTICO e pelo repouso de 10 minutos com uma onda ativa, com a chave do tamagotchi ligada no aplicativo; faltam a revisão visual e de tom pelo usuário e conferências [MANUAL] e [HW]. Os pedidos de 2026-10-01, o alívio, a bala como droga sintética e a paranoia com o sorteio único por mistura, estão verificados por testes automatizados e de integração; os casos de tela deles (V17, V17b e V18) ainda não rodaram. O baseado que ele fuma por conta própria, pedido às 19:10 de 2026-10-01 e implementado em 2026-10-02, também está verificado por testes automatizados e de integração; o caso de tela dele (V19) ainda não rodou.

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
- **Fase 5 — STATUS: PLANNED, em andamento.** Os blocos A e B (passos P1–P9 da ordem registrada em [TODO.md](TODO.md)) estão implementados e verificados por testes automatizados e de integração; o bloco B foi revisado e corrigido em 2026-10-01:
  - a posição guarda a tela do monitor da época, e a partida restaura em cascata: chave, tela do monitor, principal (DEC-030);
  - **chave estável do monitor** (passo P6): um resumo do caminho do dispositivo, que nunca é gravado nem registrado, com o nome GDI como reserva (DEC-030);
  - **persistência ligada** (passo P7): o app lê o `settings.json` na partida e grava pela agenda, cerca de 2 s depois de soltar, esconder ou escolher uma emoção, e na hora ao sair ou no fim da sessão. Ao reabrir, voltam a posição, o esconderijo, o "preso" e a emoção dominante. O esquema está na v3 (DEC-029). Sem perfil de teste, o Buzzy do usuário grava `%LOCALAPPDATA%\Buzzy\settings.json`;
  - **topologia em execução** (passo P8): quando só outros monitores mudam ou o dele só é transladado, como numa troca de principal, ele continua o que fazia; quando o dele muda de geometria ou some, revalida na mesma posição relativa, no sobrevivente certo (DEC-030);
  - **releitura robusta** (passo P9): mensagens agrupadas em 300 ms, com teto de 1 s, novas tentativas, a barra recriada pela mesma releitura e a conferência tardia do lugar das janelas 1,5 s depois, sem mexer na ordem Z (DEC-030);
  - testes e ferramentas abrem o Buzzy com `--perfil-de-teste NOME`, isolados das configurações reais, e conferem por fora que os arquivos reais não mudaram (DEC-029).
  - **Ainda não:** sessão, energia e minimização pelo Windows (passos P10–P12; o app ainda não recebe o bloqueio da sessão nem a suspensão), travessia entre monitores e escala mista (passos P13–P14).
  - **Pendente do bloco B:** a verificação de tela com input SINTÉTICO (`--fase 1`, `3`, `4` e `tamagotchi`, esta com a V16) e as medições de 10 minutos, que Claude roda avisando o usuário; as conferências [MANUAL] e [HW] da Fase 5 em [TODO.md](TODO.md); e confirmar com o usuário a gravação dos arquivos reais de configuração entre 12:12 e 12:22 de 2026-10-01 ([SECURITY.md](SECURITY.md), seção 10).
  - Os passos da Fase 5 se chamam P1 a P16 e, no texto, aparecem como "passo P7"; um P-número sem "passo" é um dos protótipos P1 a P10 da Etapa 0B.
- **Interação — emoção dominante e tamagotchi adulto (DEC-027, DEC-028) — STATUS: PLANNED, em andamento, intercalada com a Fase 5.** Os passos e as pendências estão na seção "Interação" de [TODO.md](TODO.md).
  - **Pronto e verificado (2026-10-01):** o núcleo (passos T1 e T3–T6), a arte (A1–A4) e o app (T2 e T7–T9), com as correções das revisões, por testes automatizados e de integração e pela verificação de tela com input SINTÉTICO (46 OK, 1 N/A e 0 falhas). O repouso de 10 minutos com a onda de uma vodka ficou em 0,003% de CPU de um núcleo, sem o relógio ligar. A chave `Tamagotchi` está ligada no aplicativo. Nada disso é gesto humano.
  - **O que o usuário já vê:** no menu do botão direito, "Emoção dominante", com os rostos da pixel art, e "Itens", com os 13 itens e "Recolher itens". O item cai ao lado dele, e, arrastado e solto sobre ele, é usado, com a animação do verbo e a onda de desenho animado. Como usar: [COMO_INICIAR.md](../COMO_INICIAR.md).
  - **Alívio, bala e paranoia (pedidos do usuário de 2026-10-01; DEC-028, itens 28 a 35; passos T10 e T11):** a banana, a água, o café e o energético acalmam o efeito aos poucos, um passo por item; a bala é droga sintética no jogo, com a onda do eufórico; e misturar uma droga sintética (bala, MD, cocaína ou lança-perfume) com outra substância faz um único sorteio de 1 em 8 por mistura, que pode deixá-lo paranoico, de desenho animado. Estão no núcleo e no app, verificados por testes automatizados (núcleo 479) e de integração (361/361 às 23:48 de 2026-10-01). A V17, a V17b e a V18 estão escritas e compiladas, e ainda não rodaram na tela.
  - **Baseado por conta própria (pedido do usuário de 2026-10-01, 19:10; DEC-028, itens 36 a 42; passo T12):** de vez em quando, parado no chão, ele fuma um baseado sozinho, sem item na tela, com o mesmo fumar e o mesmo chapado do baseado do menu; não fuma já chapado nem paranoico, e um clique o interrompe. Na energia Média, sai cerca de um a cada 4 minutos de tempo elegível (parado no chão, sem estar chapado nem paranoico), ou um a cada 14,6 minutos no total; só com a autonomia, ele fica chapado cerca de 39% do tempo. O baseado dele conta na mistura da paranoia, então uma bala dada sozinha pode fechá-la. Está no núcleo, sem código novo no app, verificado por testes automatizados (núcleo 492) e de integração (364/364 às 01:46 de 2026-10-02). A V19 está escrita e compilada, e ainda não rodou na tela.
  - **Ao reabrir:** desde o passo P7 da Fase 5, a emoção escolhida volta; os itens, os efeitos e o episódio da paranoia somem ao sair, de propósito.
  - **Cópia antiga na Área de Trabalho:** o Buzzy que o usuário abre por `Desktop\net10.0-windows` foi compilado às 18:32 de 2026-10-01 e ainda tem a regra antiga: a paranoia na 4ª substância e a bala como alívio; também não tem o baseado por conta própria. Falta entregar um build novo para ela, com o Buzzy dele fechado.
  - **Pendente:** a revisão visual e de tom pelo usuário, agora também da paranoia, do alívio e da bala, que continua desenhada como um doce, mostra os corações do eufórico e pode trazer paranoia; do baseado por conta própria, o usuário confirmar a frequência e o tempo chapado e se uma bala dada sozinha, com ele chapado do baseado dele, deve poder deixá-lo paranoico (DEC-028, itens 39 e 41); e as conferências [MANUAL] e [HW] da seção "Interação" de [TODO.md](TODO.md): os temas claro, escuro e de alto contraste, o Narrador, o conforto para agarrar os itens pequenos e as escalas de 125% a 200%, também entre monitores de escalas diferentes. A V16, a emoção restaurada ao reabrir, foi escrita no passo P7, a V17, a V17b e a V18, em 2026-10-01, e a V19, em 2026-10-02; nenhuma rodou na tela.
  - **Já valia no app antes da chave:** a regra da calma (DEC-022, item 4): pausado ou com o painel aberto, quem está agarrado à parede ou ao cipó sem estar preso desce ou se solta. Verificada por testes automatizados e, em 2026-10-01, na verificação de tela com input SINTÉTICO.
  - **Passagem para o Codex:** na mensagem das 19:10 de 2026-10-01, o usuário pediu que, acabado o trabalho em curso, Claude pare de produzir o projeto e atualize o [CONTINUIDADE.md](../CONTINUIDADE.md) para o Codex assumir; o baseado por conta própria foi o último pedido dessa leva.
  - **Próximo passo, para quem continuar:** a verificação de tela depois do bloco B da Fase 5, com a V16, a V17, a V17b, a V18 e a V19; depois, o passo P10. A revisão visual e de tom e as escolhas sobre o baseado por conta própria dependem do usuário.
- **Identidade visual:** refeita em 2026-09-29 a pedido do usuário como **pixel art fiel às pranchas, com o chapéu de palha e a personalidade do Luffy — semelhança intencional** (DEC-018, DEC-019).
  - O app mostra poses provisórias dela por estado (Fase 4) e o ícone novo.
  - Ganhou a arte do tamagotchi (itens, caras novas, poses de uso, sobreposições e ícones do menu), conferida por testes e mostrada pelo app desde os passos T2, T7 e T8. A revisão visual e de tom pelo usuário está pendente.
  - Está documentada em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md). O gerador fica em `src/Buzzy.Visual/Pixel/`, a folha e as prévias em `assets/identidade/pixel/`; a direção vetorial anterior está arquivada em `assets/identidade/arquivo-vetorial/`.
  - As animações completas são da Fase 6.
- **Input humano:** nenhum gesto informal de 26/09 é evidência aprovada. Os resultados por SendInput permanecem classificados como sintéticos.

## Evidências atuais

A última validação do checkout é de 2026-10-02, das 01:46 às 01:50, no fim da correção do baseado por conta própria: `tools/testar.ps1 -Integracao` e depois `tools/testar.ps1` (Release), os dois com código 0, com 492 testes do Core, 74 do portão e 332 do app sem janela, mais 32 de integração (364/364 com `-Integracao`), build sem avisos, portão de APIs aprovado, nenhum pacote vulnerável e os arquivos reais de configuração do usuário intocados. As referências gravadas 01–05 e 07 são idênticas às do commit `7fe929e`; a 08, da paranoia, foi reescrita no refino, e a 09, do baseado por conta própria, é nova, as duas revisadas linha a linha. Detalhes na seção "Interação" de [TODO.md](TODO.md) (passos T10 a T12). A verificação de tela não rodou depois disso.

Antes, em 2026-10-01, das 23:48 às 23:52, no fim da correção do sorteio único da paranoia, as mesmas duas execuções deram código 0, com 479 testes do Core, 74 do portão e 330 do app, mais 31 de integração (361/361).

O checkout com o bloco B da Fase 5 (passos P6–P9) foi validado em 2026-10-01, por volta das 14:30, na última rodada do corretor do bloco. `powershell -NoProfile -File tools/testar.ps1` (Release) saiu com código 0:
- 441 testes do Core;
- 74 testes do portão;
- 312 testes do app sem janela (mais 30 de integração, que só rodam com `-Integracao`);
- portão de APIs aprovado, com as mesmas quatro permissões do apphost;
- nenhum pacote vulnerável.

`tools/testar.ps1 -Integracao` também saiu com código 0, com 342/342 no app, por mensagens postadas às janelas do próprio Buzzy, e os arquivos reais de configuração do usuário intocados, conferidos só por fora. As referências gravadas 01–05 e 07 continuam idênticas byte a byte. Detalhes na Fase 5 de [TODO.md](TODO.md). A verificação de tela e as medições de 10 minutos ainda não rodaram depois do bloco B.

Antes do bloco B, com o app da emoção dominante e do tamagotchi validado por volta das 08:15 (Core 393, portão 73, App 249 mais 22 de integração; 271/271 na integração), Claude rodou em 2026-10-01 as verificações de tela com input SINTÉTICO, com o Buzzy aberto no perfil de teste `verificacao`, e a medição de 10 minutos com uma onda ativa, no perfil `desempenho`:

| Verificação | Relatório | Resultado |
|---|---|---|
| Fase 1 (regressão) | `resultados/verificacao-fase1.log` | 25 OK, 4 SIMULADO (bandeja) |
| Fase 3 (regressão) | `resultados/verificacao-fase3.log` | 34 OK, 2 N/A |
| Fase 4 (regressão) | `resultados/verificacao-fase4.log` | 28 OK |
| Tamagotchi (`--fase tamagotchi`) | `resultados/verificacao-tamagotchi.log` | 46 OK, 1 N/A (V16, que dependia do passo P7) |
| Repouso com a onda de uma vodka (V13, `-Modo onda`) | `resultados/desempenho-20261001-085153.txt` | 0,003% de CPU de um núcleo em 10 min, sem o relógio ligar; nenhum processo filho e nenhuma conexão |

Nenhuma verificação de tela da tabela teve falha. Os relatórios acumulam as execuções; a desta validação é a última de cada arquivo. As anteriores do mesmo dia falharam pelo ambiente (uma janela de outro programa no canto da tela) ou por dois defeitos da ferramenta, já corrigidos, ou saíram INVÁLIDAS porque o mouse foi mexido; nenhuma falha foi do Buzzy ([DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md)).

Commits: o `af586e1`, feito às 12:08 de 2026-10-01 com a identidade git do usuário, fora de Claude, guardou a árvore daquele momento: o bloco A, o tamagotchi, os passos P6 e P7 e o começo do P8, e compila sozinho. O `7fe929e`, às 18:51 do mesmo dia, também com a identidade do usuário e fora de Claude, guardou o resto do bloco B, o alívio, a arte da paranoia, o núcleo da paranoia com o gatilho antigo (na 4ª substância) e a apresentação dela. O refino da paranoia, o sorteio único, a linha `PARANOIA` e os testes e a verificação de tela deles estão só no checkout, sem commit, com o arquivo novo `tests/Buzzy.App.Testes/Integracao/SementesDaParanoia.cs` fora do Git: sem ele, os testes do app e a verificação de tela não compilam (detalhes em [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md)). O baseado por conta própria também está só no checkout, com mais três arquivos novos fora do Git: `tests/Buzzy.App.Testes/Integracao/SementesDoBaseado.cs`, sem o qual os testes do app e a verificação de tela também não compilam; a referência `tests/Buzzy.Core.Testes/Referencias/09-baseado-por-conta-propria.txt`, sem a qual a reprodução gravada do núcleo falha; e `tests/Buzzy.Core.Testes/Tamagotchi/BaseadoPorContaPropriaTestes.cs`, com os 13 testes do baseado.

A linha de base da Fase 1 está em `resultados/desempenho-20260929-215550.txt`. A medição de repouso durou 600 s e observou 0,000% de CPU de um núcleo, 56,67 → 56,51 MB de memória privada, nenhuma conexão TCP/UDP em 58 verificações e nenhum processo filho em 629 verificações. A linha de base curta não valida metas de 1 h ou 8 h. As medições com a Fase 4 estão em [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md).

O ambiente medido tinha Windows 11 25H2, build 26200, .NET 10.0.12 e dois monitores 1920×1080 a 96 DPI, com o secundário à esquerda. Escalas mistas e retrato permanecem pendentes.

## Build e verificações

Na raiz do repositório, usando PowerShell:

```powershell
dotnet build Buzzy.slnx -c Release
powershell -NoProfile -File tools\testar.ps1
```

`tools\testar.ps1` faz build, testes sem janelas, portão de APIs e auditoria de pacotes vulneráveis. Para a suíte de integração, use `powershell -NoProfile -File tools\testar.ps1 -Integracao`; janelas do Buzzy aparecem e somem, então avise o usuário antes.

Tudo o que abre o `Buzzy.exe` para testar usa um perfil de teste e, sem nenhum Buzzy aberto, apaga a pasta dele antes de abrir a primeira instância: `integracao` e `persistencia` nos testes de integração, `verificacao` no `Buzzy.Verificacao` e `desempenho` na medição. Assim nada toca as configurações reais do usuário (DEC-029). Desde o passo P7, o Buzzy aberto sem perfil grava as configurações reais; por isso a integração, a verificação de tela e a medição tiram uma foto dos arquivos reais antes e depois, só por fora (existência, tamanho e datas), e falham se ela mudar. Um Buzzy do usuário aberto durante essas execuções pode gravar esses arquivos e fazê-las falhar.

Verificações de tela — todas usam input sintético e exigem aviso prévio porque abrem janelas; a Sonda P3 e a Verificação também movem o cursor:

```powershell
spikes\SondaP3\bin\Release\net10.0-windows\SondaP3.exe --injetar-input-na-tela --repeticoes 3
tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela [--fase 1|3|4|tamagotchi] [--semente N]
tools\medir-desempenho.ps1 [-Modo repouso|autonomia|onda] [-Semente N]
```

`--semente N` só vale com `--fase tamagotchi`. Essa fase levou cerca de 5 minutos em 2026-10-01, com um repouso de 60 s em que nada deve ser tocado, e grava `resultados\verificacao-tamagotchi.log`; desde o passo P7, inclui a V16, cerca de 15 s a mais; desde o fim de 2026-10-01, a V17, a V17b e a V18 da paranoia e do alívio, cerca de 1,5 a 2 minutos a mais; e, desde 2026-10-02, a V19 do baseado por conta própria, cerca de 20 a 30 s a mais. O texto de uso da ferramenta fala em 7 a 13 minutos. Antes de começar, ela espera até 20 s sem uso do mouse e do teclado, e não pode haver outro Buzzy aberto. O `-Modo onda` abre o Buzzy pausado, entrega uma vodka por mensagens postadas às janelas do próprio Buzzy, sem `SendInput` nem mover o cursor, e leva cerca de 11 minutos, com 10 minutos de janela medida; o menu toma o primeiro plano por um instante, como sempre (DEC-016).

Opções de linha de comando do app:

| Opção | Efeito |
|---|---|
| `--diagnostico` | Grava o log local em `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB). Sem ela, o app não grava o log de diagnóstico. |
| `--pausado` | Começa com o movimento pausado, como esperam os testes de gesto, as verificações de tela e a medição de repouso. |
| `--semente N` | Fixa a agenda autônoma, para diagnóstico e testes. |
| `--perfil-de-teste NOME` | Isola os dados do Buzzy em `%LOCALAPPDATA%\Buzzy\testes\NOME`, para testes e ferramentas. NOME tem de 1 a 32 caracteres entre a–z, 0–9 e hífen, sem começar por hífen nem ser um nome reservado do Windows. Só vale com essa grafia exata, separada do nome por espaço; sem nome, com nome inválido ou com outra grafia (`--perfil-de-teste=NOME`, maiúsculas, `/perfil-de-teste`), a persistência fica desligada naquela execução. O log de diagnóstico continua na pasta do Buzzy. Sem a opção, o Buzzy lê e grava `%LOCALAPPDATA%\Buzzy\settings.json` (passo P7). |

## Fontes canônicas

- Produto: [PRODUCT_SPEC.md](PRODUCT_SPEC.md)
- Fases e critérios: [TODO.md](TODO.md)
- Arquitetura: [ARCHITECTURE.md](ARCHITECTURE.md)
- Decisões: [DECISIONS.md](DECISIONS.md)
- Segurança: [SECURITY.md](SECURITY.md)
- Evidências e histórico: [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md), `resultados/` e `spikes/resultados/`
- Diretiva operativa: [prompt_usuario.md](../prompt_usuario.md)
- Continuidade entre Claude e Codex: [CONTINUIDADE.md](../CONTINUIDADE.md)

# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Resumo operacional. Requisitos ficam em PRODUCT_SPEC.md, fases e critérios em TODO.md, decisões em DECISIONS.md, arquitetura em ARCHITECTURE.md e segurança em SECURITY.md.
>
> Atualizado em 2026-09-30. STATUS: PLANNED. Fase 2 VERIFIED; Fases 1, 3 e 4 implementadas, com verificações manuais ou de hardware pendentes; a próxima é a Fase 5.

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
- **Identidade visual:** refeita em 2026-09-29 a pedido do usuário como **pixel art fiel às pranchas, com o chapéu de palha e a personalidade do Luffy — semelhança intencional** (DEC-018, DEC-019).
  - O app mostra poses provisórias dela por estado (Fase 4) e o ícone novo.
  - Está documentada em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md). O gerador fica em `src/Buzzy.Visual/Pixel/`, a folha e as prévias em `assets/identidade/pixel/`; a direção vetorial anterior está arquivada em `assets/identidade/arquivo-vetorial/`.
  - As animações completas são da Fase 6.
- **Input humano:** nenhum gesto informal de 26/09 é evidência aprovada. Os resultados por SendInput permanecem classificados como sintéticos.

## Evidências atuais

O checkout foi validado em 2026-09-30 com `powershell -NoProfile -File tools/testar.ps1`, com código de saída 0:
- build Release com 0 avisos e 0 erros;
- 267 testes do Core;
- 73 testes do portão;
- 26 testes do app sem janela;
- portão de APIs aprovado;
- nenhum pacote vulnerável.

A integração (`-Integracao`) passou 37/37. As verificações de tela usaram input SINTÉTICO:

| Fase | Relatório | Resultado |
|---|---|---|
| 1 | `resultados/verificacao-fase1.log` | 25 OK, 4 SIMULADO (bandeja) |
| 3 | `resultados/verificacao-fase3.log` | 34 OK, 2 N/A |
| 4 | `resultados/verificacao-fase4.log` | 28 OK |

Nenhuma teve falha.

A linha de base da Fase 1 está em `resultados/desempenho-20260929-215550.txt`. A medição de repouso durou 600 s e observou 0,000% de CPU de um núcleo, 56,67 → 56,51 MB de memória privada, nenhuma conexão TCP/UDP em 58 verificações e nenhum processo filho em 629 verificações. A linha de base curta não valida metas de 1 h ou 8 h. As medições com a Fase 4 estão em [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md).

O ambiente medido tinha Windows 11 25H2, build 26200, .NET 10.0.12 e dois monitores 1920×1080 a 96 DPI, com o secundário à esquerda. Escalas mistas e retrato permanecem pendentes.

## Build e verificações

Na raiz do repositório, usando PowerShell:

```powershell
dotnet build Buzzy.slnx -c Release
powershell -NoProfile -File tools\testar.ps1
```

`tools\testar.ps1` faz build, testes sem janelas, portão de APIs e auditoria de pacotes vulneráveis. Para a suíte de integração, use `powershell -NoProfile -File tools\testar.ps1 -Integracao`; janelas do Buzzy aparecem e somem, então avise o usuário antes.

Verificações de tela — todas usam input sintético e exigem aviso prévio porque abrem janelas; a Sonda P3 e a Verificação também movem o cursor:

```powershell
spikes\SondaP3\bin\Release\net10.0-windows\SondaP3.exe --injetar-input-na-tela --repeticoes 3
tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela [--fase 1|3|4]
tools\medir-desempenho.ps1 [-Modo repouso|autonomia] [-Semente N]
```

Opções de linha de comando do app:

| Opção | Efeito |
|---|---|
| `--diagnostico` | Grava o log local em `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB). Sem ela, o app não grava o log de diagnóstico. |
| `--pausado` | Começa com o movimento pausado, como esperam os testes de gesto, as verificações de tela e a medição de repouso. |
| `--semente N` | Fixa a agenda autônoma, para diagnóstico e testes. |

## Fontes canônicas

- Produto: [PRODUCT_SPEC.md](PRODUCT_SPEC.md)
- Fases e critérios: [TODO.md](TODO.md)
- Arquitetura: [ARCHITECTURE.md](ARCHITECTURE.md)
- Decisões: [DECISIONS.md](DECISIONS.md)
- Segurança: [SECURITY.md](SECURITY.md)
- Evidências e histórico: [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md), `resultados/` e `spikes/resultados/`
- Diretiva operativa: [prompt_usuario.md](../prompt_usuario.md)
- Continuidade entre Claude e Codex: [CONTINUIDADE.md](../CONTINUIDADE.md)

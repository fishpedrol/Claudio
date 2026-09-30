# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Resumo operacional. Requisitos ficam em PRODUCT_SPEC.md, fases e critérios em TODO.md, decisões em DECISIONS.md, arquitetura em ARCHITECTURE.md e segurança em SECURITY.md.
>
> Atualizado em 2026-09-29. STATUS: PLANNED — Fase 2 em integração; a Fase 1 ainda tem verificações manuais pendentes.

## Estado

- **Fase 0 / P3 — STATUS: VERIFIED no ambiente medido.** O harness isolado completou 3/3 rodadas com 28/28 cenários OK por rodada, incluindo B4b. Todo o input foi sintético (SendInput); não é validação humana. O resultado e os limites estão em [TODO.md](TODO.md) e `spikes/resultados/p3-receptor.log`.
- **Fase 1 — STATUS: PLANNED.** O shell do aplicativo está implementado. Build, testes automatizados, verificação em tela com input sintético e linha de base de dez minutos passaram nos limites registrados em TODO.md. Continuam pendentes a verificação real do menu pela bandeja, as escalas de 150% e 200% e a troca real de resolução/escala/barra de tarefas. Não declarar a fase concluída enquanto esses critérios estiverem pendentes (DEC-015).
- **Fase 2 — STATUS: PLANNED, trabalho em andamento.** A máquina de estados pura, agenda, efeitos e gravação/reprodução estão implementados; `Buzzy.Core.Testes` passou 147/147 no checkout atual. As referências 02–05 foram revistas manualmente e conferem com os fluxos descritos. A integração desse núcleo à raiz de composição do app e sua verificação ainda estão pendentes.
- **Identidade visual:** refeita em 2026-09-29 a pedido do usuário como **pixel art fiel às pranchas, com o chapéu de palha e a personalidade do Luffy — semelhança intencional** (DEC-018, DEC-019); o app já mostra o quadro "parado" dela e o ícone novo; documentada em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md); gerador em `src/Buzzy.Visual/Pixel/`, folha e prévias em `assets/identidade/pixel/`; a direção vetorial anterior está arquivada em `assets/identidade/arquivo-vetorial/`. O placeholder continua no app até a integração da Fase 6.
- **Input humano:** nenhum gesto informal de 26/09 é evidência aprovada. Os resultados por SendInput permanecem classificados como sintéticos.

## Evidências atuais

O checkout foi validado em 2026-09-29 com `powershell -NoProfile -File tools/testar.ps1`: código de saída 0; build Release com 0 avisos e 0 erros; 147 testes do Core; 73 testes do portão; portão de APIs aprovado. O script terminou sem falhas. Consulte os relatórios existentes para os resultados de tela e desempenho. Uma execução posterior, após a integração da Fase 2, ainda é necessária.

Os relatórios da Fase 1 estão em `resultados/verificacao-fase1.log` e `resultados/desempenho-20260929-215550.txt`. A verificação em tela registrou 25 OK, 4 SIMULADO e 0 falhas; todos os cliques e teclas injetados são sintéticos. A medição de repouso durou 600 s e observou 0,000% de CPU de um núcleo, 56,67 → 56,51 MB de memória privada, nenhuma conexão TCP/UDP em 58 verificações e nenhum processo filho em 629 verificações. A linha de base curta não valida metas de 1 h ou 8 h.

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
tests\Buzzy.Verificacao\bin\Release\net10.0-windows\Buzzy.Verificacao.exe --injetar-input-na-tela
tools\medir-desempenho.ps1
```

Executar o app com `--diagnostico` grava o log local em `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB). Sem essa opção, o app não grava o log de diagnóstico.

## Fontes canônicas

- Produto: [PRODUCT_SPEC.md](PRODUCT_SPEC.md)
- Fases e critérios: [TODO.md](TODO.md)
- Arquitetura: [ARCHITECTURE.md](ARCHITECTURE.md)
- Decisões: [DECISIONS.md](DECISIONS.md)
- Segurança: [SECURITY.md](SECURITY.md)
- Evidências e histórico: [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) e `spikes/resultados/`
- Diretiva operativa: [prompt_usuario.md](../prompt_usuario.md)

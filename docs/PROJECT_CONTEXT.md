# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Resumo operacional. Requisitos ficam em PRODUCT_SPEC.md, tarefas e gates em TODO.md, decisões em DECISIONS.md, arquitetura em ARCHITECTURE.md e segurança em SECURITY.md.
>
> Atualizado em 2026-09-29. STATUS: PLANNED; a Fase 1 está em implementação inicial e não foi declarada concluída.

## Estado e autorização

- **Gates:** P1 e P2 foram aceitos pelo usuário nos limites de TODO.md. O harness controlado de P3 tem evidência de três rodadas para os cenários centrais (foco, clique, arraste, ClickLock, travessia de monitor e entrega ao receptor). O relatório agregado ainda registra 0/3 rodadas completas por uma falha de salvaguarda durante B4b, cenário adicional; a correção informada ainda precisa de nova execução registrada. Não marque P3/Etapa 0B como concluídos sem resolver e documentar esse estado.
- **Implementação:** há arquivos iniciais do aplicativo e testes em src/ e tests/, além de ferramentas em tools/. Claude está desenvolvendo a Fase 1. Inspecione os arquivos e os resultados atuais; não declare build, testes ou critérios como aprovados sem evidência.
- **Autorização:** o usuário autorizou Claude a criar a identidade visual original e executar/testar as fases do MVP em sequência, sem aprovações rotineiras. A Fase 1 está autorizada após o fechamento técnico da Fase 0; veja DEC-015 e prompt_usuario.md.
- **Identidade visual:** as duas imagens em assets/references/ são inspiração para uma proposta original. Claude está autorizado a criar o design sem aprovação rotineira; Fase 1 usa placeholder estático e os assets animados entram na Fase 6.
- **P3 humano:** os dez movimentos anteriores foram feitos pela namorada do usuário como exploração informal, não como teste formal nem evidência humana aprovada. SendInput é evidência sintética.

## Evidências e limites atuais

- **P1:** aceito como PASS apenas no ambiente medido (Windows 11, 96 DPI). Parte dos cliques foi sintética; escalas diferentes não foram verificadas.
- **P2:** aceito como medição de viabilidade, não como validação do app. Repouso passou; animação entregou cerca de 39–40 qps quando solicitados 60, e a memória cresceu durante dez minutos. Investigar antes da Fase 6. Números em TODO.md.
- **P3:** consulte spikes/resultados/p3-receptor.log. Cenários centrais aparecem como OK em três rodadas; o relatório completo não passou por causa da tentativa adicional B4b, que abortou antes de clicar em janela alheia. O Claude registrou uma correção, sem nova saída de teste após ela.
- **Hardware conhecido:** Windows 11, dois monitores 1920×1080 a 100%, secundário à esquerda (x negativo). Escala mista, retrato e outros cenários seguem pendentes até teste.

## Direção atual

Leia AGENTS.md, CLAUDE.md, PRODUCT_SPEC.md, TODO.md e SECURITY.md antes de trabalhar; consulte os demais documentos quando a tarefa exigir seus detalhes. A diretiva operativa é prompt_usuario.md. Preserve alterações em andamento. Continue implementação independente; registre [HW] indisponível como pendente e nunca marque VERIFIED sem evidência.

## Fontes canônicas

- Produto: [PRODUCT_SPEC.md](PRODUCT_SPEC.md)
- Fases e critérios: [TODO.md](TODO.md)
- Arquitetura: [ARCHITECTURE.md](ARCHITECTURE.md)
- Decisões e autorização: [DECISIONS.md](DECISIONS.md)
- Segurança: [SECURITY.md](SECURITY.md)
- Evidências e histórico: [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) e spikes/resultados/
- Método de revisão Codex, quando solicitado: [PLAN_REVIEW.md](PLAN_REVIEW.md)
- Diretiva atual para Claude: [prompt_usuario.md](../prompt_usuario.md)

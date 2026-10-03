# PLAN_REVIEW.md — Revisão independente dos planos do Buzzy

> Procedimento da revisão do Codex, quando o usuário pede. A revisão não é gate do ciclo de implementação: Claude executa sob a autorização contínua da DEC-015, e o usuário pode mudar o escopo ou revogar a autorização a qualquer momento.

## Fontes e precedência

1. A instrução mais recente do usuário define mudanças de intenção e escopo.
2. PRODUCT_SPEC.md é a referência estável do produto.
3. Código, configurações e resultados de teste mostram o que existe de fato.
4. PROJECT_CONTEXT.md resume estado e fase; ARCHITECTURE.md, DECISIONS.md, SECURITY.md e TODO.md guardam o detalhe.
5. AGENTS.md e prompt_usuario.md orientam a execução, sem substituir a especificação nem a DEC-015.
6. O plano ou o código de Claude é o objeto revisado, não uma fonte de verdade.

Quando duas fontes discordarem, aponte a divergência e a consequência. Intenção PLANNED não é comportamento existente, e nada é VERIFIED sem testes executados.

## Roteiro

Verifique os itens que se aplicam e, para cada um que não se aplica à fase atual, diga por quê:

- **Objetivo e MVP:** atende a personagem, desktop Windows, transparência, posicionamento, arraste, movimento, escalada, salto, queda, multi-monitor, curiosidade não verbal, reações, seletor de energia, configurações, persistência local, segurança e desempenho, sem chat nem conversa digitada?
- **Não-objetivos:** mantém fora IA integrada, RAG, backend, nuvem, voz, atualização automática, telemetria, analytics, execução de comandos arbitrários e controle genérico do computador?
- **Fase:** resolve só a fase atual, declara pré-requisitos e não pula decisão aberta?
- **Arquitetura:** separa comportamento determinístico, input, movimento, apresentação, integração Windows e armazenamento só onde isso reduz acoplamento real, sem abstrações futuras sem necessidade?
- **Estados e eventos:** define transições, eventos, clique contra arraste, painel de energia e o cancelamento de comportamentos incompatíveis com a interação direta?
- **Arraste:** cobre mouse down, DRAGGING, interrupção da autonomia, acompanhamento do cursor, mouse up, validação da posição e retomada?
- **Monitores:** cobre desktop virtual, arranjos arbitrários, resolução, DPI, orientação, área útil, principal, desconexão e reconexão e persistência?
- **Tecnologia:** compara opções com evidência e critérios do produto, expondo justificativa, trade-offs, riscos e alternativas? Fatos atuais de API se conferem em fontes oficiais.
- **Segurança:** limita permissões e dados, sem capacidades ocultas, rede desnecessária ou execução genérica?
- **Desempenho:** trata repouso, loops, timers, eventos, listeners, renderização, memória e estabilidade, dizendo o que medir e quando?
- **Testabilidade:** cada etapa tem critérios de aceitação observáveis, testes apropriados e conclusão verificável?
- **Complexidade:** evita backend, banco de dados, IPC, plugins ou frameworks sem necessidade demonstrada?
- **Documentação:** identifica as atualizações necessárias e não contradiz as fontes canônicas?

## Formato do relatório

1. **Resumo executivo:** consistente, parcialmente consistente ou precisa de revisão significativa, sem nota numérica.
2. **Problemas críticos:** problema, por que importa, impacto e correção sugerida.
3. **Problemas importantes.**
4. **Pontos bons a preservar.**
5. **Decisões questionáveis e lacunas.**
6. **Segurança, desempenho e arquitetura.**
7. **Ordem corrigida das fases**, com dependências justificadas.
8. **Plano corrigido:** manter, remover, alterar e adicionar.

Separe fatos do repositório, requisitos do produto, inferências e recomendações; não crie objeções artificiais. Durante a revisão, a árvore fica como você a encontrou, a menos que o usuário peça a atualização.

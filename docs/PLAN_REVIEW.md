# PLAN_REVIEW.md — Revisão independente dos planos do Buzzy

> Procedimento para revisão independente do Codex, quando solicitada. A revisão não é gate do ciclo de implementação.

## Papel

Claude, usando o modelo escolhido pelo usuário (Fable ou Opus), prepara e executa o plano da fase atual dentro da autorização contínua registrada em DEC-015. Codex é o segundo olhar quando o usuário pedir uma revisão; essa revisão não suspende a execução já autorizada. O usuário pode mudar o escopo ou revogar a autorização a qualquer momento.

## Fontes e precedência

1. A instrução mais recente do usuário define mudanças de intenção e escopo.
2. PRODUCT_SPEC.md é a referência estável do produto.
3. Código, configurações e resultados de teste mostram o que existe de fato.
4. PROJECT_CONTEXT.md resume estado e fase; ARCHITECTURE.md, DECISIONS.md, SECURITY.md e TODO.md guardam seus detalhes.
5. prompt_usuario.md orienta a execução atual; context codex/PROMPT_MESTRE_BUZZY.md contém regras estáveis. Nenhum substitui a especificação nem DEC-015.
6. O plano de Claude é o objeto revisado, não uma fonte de verdade.

Quando duas fontes discordarem, apontar a divergência e sua consequência. Não tratar intenção PLANNED como comportamento existente, nem declarar VERIFIED sem testes executados.

## Roteiro da revisão

Verificar todos os itens aplicáveis:

- **Objetivo e MVP:** o plano atende a personagem, desktop Windows, transparência, posicionamento, drag, movimento, escalada, salto, queda, multi-monitor, curiosidade não verbal, reações, seletor de energia, configurações, persistência local, segurança e performance, respeitando a decisão de não incluir chat nem conversa digitada?
- **Não-objetivos:** o plano mantém IA integrada, RAG, backend, nuvem, voz, atualização automática, telemetria, analytics, execução de comandos arbitrários e controle genérico do computador fora do MVP?
- **Fase:** o plano resolve somente a fase atual, declara pré-requisitos e não pula uma decisão que ainda está aberta?
- **Arquitetura:** separa comportamento determinístico, input, movimento, apresentação, integração Windows e armazenamento apenas onde isso reduz acoplamento real? Evita abstrações futuras sem necessidade presente?
- **Estados e eventos:** define transições, eventos, distinção entre clique e drag, comportamento do painel de energia e cancelamento de comportamentos incompatíveis com interação direta?
- **Drag:** cobre mouse down, DRAGGING, interrupção de autonomia, acompanhamento do cursor, mouse up, validação de posição e retomada?
- **Monitores:** cobre desktop virtual, posições relativas arbitrárias, resolução, DPI, orientação, área útil, monitor principal, desconexão/reconexão e persistência?
- **Tecnologia:** compara opções com evidências e critérios do produto; expõe justificativa, trade-offs, riscos e alternativas sem escolher por popularidade?
- **Segurança:** limita permissões e dados; não adiciona capacidades ocultas, rede desnecessária ou execução genérica?
- **Performance:** trata idle, loops, timers, eventos, listeners, rendering, memória e estabilidade; define o que será medido e quando?
- **Testabilidade:** cada etapa tem critérios de aceitação observáveis, testes apropriados e um critério de conclusão verificável?
- **Complexidade:** evita backend, banco de dados, IPC, plugin system ou frameworks sem necessidade demonstrada?
- **Documentação:** identifica atualizações necessárias e não contradiz as fontes canônicas?

Se uma área não se aplicar à fase atual, dizer por quê. Se a stack depender de fatos atuais ou APIs específicas, verificar fontes oficiais antes de aceitar afirmações técnicas. A aplicação deste roteiro só é obrigatória quando o usuário solicitar uma revisão do Codex; não é um gate entre fases para Claude.

## Formato do relatório

Apresentar:

1. **Resumo executivo:** consistente, parcialmente consistente ou precisa de revisão significativa; sem nota numérica.
2. **Problemas críticos:** problema, por que importa, impacto e correção sugerida.
3. **Problemas importantes.**
4. **Pontos bons a preservar.**
5. **Decisões questionáveis e lacunas.**
6. **Segurança, performance e arquitetura.**
7. **Ordem corrigida das fases**, com dependências justificadas.
8. **Plano corrigido:** manter, remover, alterar e adicionar.

Separar fatos do repositório, requisitos do produto, inferências e recomendações. Não criar objeções artificiais. Não mudar documentos ou código durante uma revisão de plano, a menos que o usuário peça essa atualização.

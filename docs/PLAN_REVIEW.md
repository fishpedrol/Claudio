# PLAN_REVIEW.md — Revisão independente dos planos do Buzzy

> Procedimento estável para o Codex revisar o planejamento produzido por Claude/Fable. A revisão não implementa o plano.

## Papel

Claude, usando o modelo escolhido pelo usuário (Fable ou Opus), prepara o plano da fase atual e, após decisão do usuário, pode implementar a fase autorizada. Codex é o segundo olhar: verifica se o plano é coerente com a intenção do produto, a fase, as decisões, a arquitetura e o estado real. O usuário decide se e quando o trabalho começa.

## Fontes e precedência

1. A instrução mais recente do usuário define mudanças de intenção e escopo.
2. PRODUCT_SPEC.md é a referência estável do produto.
3. Código, configurações e resultados de teste mostram o que existe de fato.
4. PROJECT_CONTEXT.md resume estado e fase; ARCHITECTURE.md, DECISIONS.md, SECURITY.md e TODO.md guardam seus detalhes.
5. context codex/PROMPT_MESTRE_BUZZY.md diz o que o agente deve fazer nesta fase, sem substituir a especificação.
6. O plano de Claude é o objeto revisado, não uma fonte de verdade.

Quando duas fontes discordarem, apontar a divergência e sua consequência. Não tratar intenção PLANNED como comportamento existente, nem declarar VERIFIED sem testes executados.

## Roteiro da revisão

Verificar todos os itens aplicáveis:

- **Objetivo e MVP:** o plano atende a personagem, desktop Windows, transparência, posicionamento, drag, movimento, escalada, salto, queda, multi-monitor, expressões, interação, caixa de texto, configurações, persistência local, segurança e performance, respeitando as decisões registradas?
- **Não-objetivos:** o plano mantém IA integrada, RAG, backend, nuvem, voz, atualização automática, telemetria, analytics, execução de comandos arbitrários e controle genérico do computador fora do MVP?
- **Fase:** o plano resolve somente a fase atual, declara pré-requisitos e não pula uma decisão que ainda está aberta?
- **Arquitetura:** separa comportamento determinístico, input, movimento, apresentação, integração Windows e armazenamento apenas onde isso reduz acoplamento real? Evita abstrações futuras sem necessidade presente?
- **Estados e eventos:** define transições, eventos, distinção entre clique e drag, foco da caixa de texto e cancelamento de comportamentos incompatíveis com interação direta?
- **Drag:** cobre mouse down, DRAGGING, interrupção de autonomia, acompanhamento do cursor, mouse up, validação de posição e retomada?
- **Monitores:** cobre desktop virtual, posições relativas arbitrárias, resolução, DPI, orientação, área útil, monitor principal, desconexão/reconexão e persistência?
- **Tecnologia:** compara opções com evidências e critérios do produto; expõe justificativa, trade-offs, riscos e alternativas sem escolher por popularidade?
- **Segurança:** limita permissões e dados; não adiciona capacidades ocultas, rede desnecessária ou execução genérica?
- **Performance:** trata idle, loops, timers, eventos, listeners, rendering, memória e estabilidade; define o que será medido e quando?
- **Testabilidade:** cada etapa tem critérios de aceitação observáveis, testes apropriados e um critério de conclusão verificável?
- **Complexidade:** evita backend, banco de dados, IPC, plugin system ou frameworks sem necessidade demonstrada?
- **Documentação:** identifica atualizações necessárias e não contradiz as fontes canônicas?

Se uma área não se aplicar à fase atual, dizer por quê. Se a stack depender de fatos atuais ou APIs específicas, verificar fontes oficiais antes de aceitar afirmações técnicas.

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

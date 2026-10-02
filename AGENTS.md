# Instruções para agentes — Buzzy

## Fontes do projeto

Leia docs/PROJECT_CONTEXT.md antes de qualquer trabalho. Leia docs/PRODUCT_SPEC.md antes de planejar ou alterar comportamento do produto.

Para um plano ou revisão de plano, leia docs/PROMPT_MESTRE_BUZZY.md, TODO.md, ARCHITECTURE.md, DECISIONS.md e PLAN_REVIEW.md. Leia SECURITY.md antes de decisões sobre permissões, dados, processos ou rede. Inspecione os arquivos reais antes de afirmar o estado de implementação.

PRODUCT_SPEC.md define a intenção estável. PROJECT_CONTEXT.md define o estado atual. TODO.md define a fase e as tarefas. docs/PROMPT_MESTRE_BUZZY.md contém regras estáveis; prompt_usuario.md define a diretiva operativa atual. Nenhum plano de agente substitui decisão explícita do usuário.

## Papéis e ciclo de trabalho

**Claude é o desenvolvedor principal.** O usuário concedeu a Claude autorização contínua para criar a identidade visual original e executar o projeto fase a fase, conforme DEC-015 e a diretiva em prompt_usuario.md. Claude planeja, implementa, testa, corrige e sincroniza a documentação sem pedir aprovação de plano ou confirmação entre fases. Pode tomar decisões técnicas dentro dos limites de produto e segurança já registrados; registre escolhas materiais em DECISIONS.md.

**Codex é o segundo desenvolvedor e o revisor, quando o usuário pedir** (DEC-015, atualização de 2026-10-02). Como desenvolvedor, faz as tarefas que o usuário lhe der, com as mesmas regras, gates e documentação deste arquivo. Como revisor, segue docs/PLAN_REVIEW.md e entrega um relatório, deixando a árvore como a encontrou. A revisão não é gate para Claude avançar.

Conclua os critérios de cada fase antes de declarar seu status. Se um teste [HW] não puder ser feito no equipamento disponível, registre-o como pendente e continue o trabalho que não dependa dessa verificação; retorne aos itens pendentes na integração final. Nunca declare VERIFIED nem fase concluída sem a evidência exigida.

Peça ajuda ao usuário apenas quando houver um bloqueio que realmente exija uma ação dele, uma decisão de produto fora da especificação ou uma permissão externa não concedida. Antes disso, procure uma alternativa segura e isolada; agrupe qualquer solicitação indispensável e continue o trabalho independente. Não leia conteúdo de outros aplicativos, não encerre processos do usuário e não altere configurações globais do Windows para simular testes.

Durante execução, implemente as fases na ordem de TODO.md, atualizando o status e a documentação ao fechar cada gate e avançando sem aguardar confirmação. Não introduza IA integrada ao MVP.

## Trabalho a dois na mesma árvore

Claude e Codex trabalham no mesmo checkout, às vezes ao mesmo tempo. A seção "Em andamento" do CONTINUIDADE.md é o quadro de quem mexe em quê.

1. Antes da primeira edição, leia "Em andamento" e o `git status`. Mudanças que você não fez são trabalho do outro agente ou do usuário: preserve-as.
2. Registre a sua tarefa em "Em andamento" (agente, tarefa, arquivos ou áreas, início) e edite só fora das áreas que o outro registrou. Se precisar de uma delas, combine pelo usuário.
3. Um Buzzy por vez: antes do `-Integracao` ou de uma verificação de tela, confirme que nenhum Buzzy está aberto; se houver, espere ou avise o usuário.
4. Ao terminar, tire a tarefa de "Em andamento" e registre o marco no log do CONTINUIDADE.md.
5. Os commits são do usuário.

## Documentação viva

Ao concluir cada fase, sincronize nesta ordem:

1. docs/PROJECT_CONTEXT.md
2. docs/DEVELOPMENT_LOG.md
3. docs/ARCHITECTURE.md, se o desenho ou a arquitetura mudou
4. docs/DECISIONS.md, se houve decisão relevante
5. docs/SECURITY.md, se o modelo de segurança mudou
6. docs/TODO.md
7. docs/PROMPT_MESTRE_BUZZY.md, apontando para a fase seguinte
8. Confira cada afirmação contra os arquivos reais e os resultados executados

Cada documento começa com o formato das suas próprias entradas; siga-o. Mantenha cada fato em uma fonte canônica e use links em vez de duplicar o conteúdo.

## Veracidade

Toda funcionalidade, teste ou decisão registra status:

- STATUS: VERIFIED — implementado e testado; os testes correspondentes foram executados.
- STATUS: PLANNED — planejado e ainda não verificado.
- STATUS: UNCERTAIN — escolha ou estado ainda indefinido.

Não declare fase concluída só porque o código compila. Cada fase exige build, testes, correção de erros, verificação do comportamento e documentação sincronizada.

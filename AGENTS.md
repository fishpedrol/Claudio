# Instruções para agentes — Buzzy

## Fontes do projeto

Leia docs/PROJECT_CONTEXT.md antes de qualquer trabalho. Leia docs/PRODUCT_SPEC.md antes de planejar ou alterar comportamento do produto.

Para um plano ou revisão de plano, leia context codex/PROMPT_MESTRE_BUZZY.md, TODO.md, ARCHITECTURE.md, DECISIONS.md e PLAN_REVIEW.md. Leia SECURITY.md antes de decisões sobre permissões, dados, processos ou rede. Inspecione os arquivos reais antes de afirmar o estado de implementação.

PRODUCT_SPEC.md define a intenção estável. PROJECT_CONTEXT.md define o estado atual. TODO.md define a fase e as tarefas. context codex/PROMPT_MESTRE_BUZZY.md contém regras estáveis; prompt_usuario.md define a diretiva operativa atual. Nenhum plano de agente substitui decisão explícita do usuário.

## Papéis e ciclo de trabalho

O usuário concedeu a Claude autorização contínua para criar a identidade visual original e executar o projeto fase a fase, conforme DEC-015 e o prompt atual em prompt_usuario.md. Claude planeja, implementa, testa, corrige e sincroniza a documentação sem pedir aprovação de plano ou confirmação entre fases. Pode tomar decisões técnicas dentro dos limites de produto e segurança já registrados; registre escolhas materiais em DECISIONS.md.

Codex revisa planos ou código quando o usuário pedir; essa revisão não é gate obrigatório para Claude avançar sob esta autorização. Conclua os critérios de cada fase antes de declarar seu status. Se um teste [HW] não puder ser feito no equipamento disponível, registre-o como pendente e continue o trabalho que não dependa dessa verificação; retorne aos itens pendentes na integração final. Nunca declare VERIFIED nem fase concluída sem a evidência exigida.

Peça ajuda ao usuário apenas quando houver um bloqueio que realmente exija uma ação dele, uma decisão de produto fora da especificação ou uma permissão externa não concedida. Antes disso, procure uma alternativa segura e isolada; agrupe qualquer solicitação indispensável e continue o trabalho independente. Não leia conteúdo de outros aplicativos, não encerre processos do usuário e não altere configurações globais do Windows para simular testes.

Durante execução, implemente as fases na ordem de TODO.md, atualizando o status e a documentação ao fechar cada gate e avançando sem aguardar confirmação. P3 continua um gate técnico para a Fase 0: Claude deve fechá-lo autonomamente com um receptor controlado pelo spike, sem depender do Bloco de Notas. A autorização da Fase 1 já foi dada; não peça autorização novamente quando a Fase 0 estiver tecnicamente fechada. Não introduza IA integrada ao MVP.

## Documentação viva

Ao concluir cada fase, sincronize nesta ordem:

1. docs/PROJECT_CONTEXT.md
2. docs/DEVELOPMENT_LOG.md
3. docs/ARCHITECTURE.md, se o desenho ou a arquitetura mudou
4. docs/DECISIONS.md, se houve decisão relevante
5. docs/SECURITY.md, se o modelo de segurança mudou
6. docs/TODO.md
7. context codex/PROMPT_MESTRE_BUZZY.md, apontando para a fase seguinte
8. Confira cada afirmação contra os arquivos reais e os resultados executados

Cada documento começa com o formato das suas próprias entradas; siga-o. Mantenha cada fato em uma fonte canônica e use links em vez de duplicar o conteúdo.

## Veracidade

Toda funcionalidade, teste ou decisão registra status:

- STATUS: VERIFIED — implementado e testado; os testes correspondentes foram executados.
- STATUS: PLANNED — planejado e ainda não verificado.
- STATUS: UNCERTAIN — escolha ou estado ainda indefinido.

Não declare fase concluída só porque o código compila. Cada fase exige build, testes, correção de erros, verificação do comportamento e documentação sincronizada.

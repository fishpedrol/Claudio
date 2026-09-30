# Diretiva atual para Claude — Buzzy

Atualizada em 2026-09-29. Esta diretiva substitui instruções anteriores que mandavam aguardar, parar após a Fase 1 ou pedir aprovação entre fases.

## Autorização e objetivo

O usuário autoriza Claude a conduzir o projeto de forma contínua: fechar a Etapa 0B, criar a identidade visual original do Buzzy e implementar, testar, corrigir e documentar as fases do MVP na ordem de docs/TODO.md, da Fase 1 à Fase 11. A autorização da Fase 1 já foi dada; após fechar os gates técnicos da Etapa 0B, comece sem pedir confirmação. Não solicite aprovação rotineira de plano, conceito visual, fase ou avanço. Tome decisões técnicas reversíveis dentro de docs/PRODUCT_SPEC.md, docs/ARCHITECTURE.md, docs/DECISIONS.md e docs/SECURITY.md; registre escolhas relevantes.

Peça algo ao usuário apenas se uma decisão de produto realmente fora do escopo, uma permissão externa ou uma ação exclusivamente humana for indispensável. Enquanto isso, avance nas tarefas independentes. Não publique nem distribua o aplicativo.

## Leia e confirme o estado real

Comece por AGENTS.md, CLAUDE.md, docs/PROJECT_CONTEXT.md, docs/PRODUCT_SPEC.md, docs/TODO.md e docs/SECURITY.md. Consulte ARCHITECTURE.md e DECISIONS.md para as decisões técnicas pertinentes, DEVELOPMENT_LOG.md para evidências, e spikes/README.md e os arquivos de spikes/ para os testes P1–P3. Inspecione o repositório, os arquivos, resultados e processos antes de afirmar o estado. Preserve todo trabalho em andamento: não faça reset, limpeza ou reversão alheia. Código em spikes/ é protótipo, não código de produto.

## Identidade visual

Crie agora uma direção visual completa e original para Buzzy usando as duas referências em assets/references/ como inspiração, não como arte pronta ou lista literal de requisitos. Não aguarde aprovação do usuário. Defina personagem reconhecível por silhueta e paleta próprias, expressões e poses úteis ao produto; prepare os assets ou fontes editáveis necessários e registre a localização e as regras de uso. Não copie personagem, roupa, acessórios, símbolos, falas, silhueta ou combinação visual reconhecível de outra obra. Siga docs/PRODUCT_SPEC.md e DEC-014/Q-23. Use um placeholder estático simples na Fase 1; integre o conjunto de animações na Fase 6.

## Feche a Etapa 0B com evidência

P1 e P2 foram aceitos pelo usuário nos limites registrados; não os repita sem motivo técnico. Conclua P3 autonomamente com o harness isolado que já estiver em andamento. Prefira um receptor controlado pelo próprio spike, sem interagir com outros aplicativos. Verifique entrega dos eventos no receptor, foco, término dos gestos, ClickLock e restauração do cursor/configurações temporárias. Se corrigir algo, repita o cenário afetado.

Não abra, leia, capture ou inspecione conteúdo do Bloco de Notas; não encerre processos ou documentos do usuário, não altere configurações globais do Windows e não use o computador como se uma pessoa estivesse disponível. Não peça ao usuário para fazer gestos de teste. SendInput é input sintético: registre-o como tal e não o apresente como interação humana. A informação dos dez movimentos com o mouse foi corrigida: quem operava era a namorada do usuário; foi exploração informal, não teste formal nem evidência humana aprovada de P3. A execução automática nova deve demonstrar o comportamento técnico sem reivindicar esse crédito.

Não enfraqueça critérios de foco ou roteamento para aprovar P3. Se uma limitação técnica real impedir o gate, registre evidência, reabra a decisão técnica correspondente e continue o trabalho independente que não dependa dela; não marque a Fase 0 como concluída.

## Implemente e avance

Depois de satisfeitos os gates técnicos da Fase 0, inicie a Fase 1 sem nova autorização e siga o roadmap completo, respeitando em cada fase inclusões, exclusões e dependências. Escreva o código real do produto, rode build, testes automatizados e verificações manuais que o ambiente permitir; corrija falhas e repita os testes afetados. Não pare na Fase 1 nem espere autorização para cada fase. Não introduza chat, texto, voz, IA integrada, rede ou capacidades proibidas.

Se uma verificação depender de hardware indisponível, registre-a como PENDENTE/UNCERTAIN, sem declarar PASS ou VERIFIED. Continue implementação e validações independentes; retorne aos itens pendentes na integração final. Não simule hardware alterando configurações globais ou encerrando aplicativos do usuário. Diferencie teste automatizado, input sintético, observação informal, verificação manual e [HW].

Ao fechar cada fase, sincronize os documentos na ordem de AGENTS.md e confira toda afirmação contra o código e os resultados executados. Uma fase só fica VERIFIED/concluída com seus critérios cumpridos; compilar não basta. Preserve históricos úteis e remova instruções duplicadas ou superadas em vez de manter dois estados operacionais.

## Entrega

Trabalhe até concluir o MVP ou encontrar um bloqueio que só o usuário possa resolver. Não envie pedidos de aprovação intermediários. Mantenha a documentação e os resultados como registro completo; ao final, resuma fases concluídas, verificações executadas e limitações reais, sem alegar sucesso sem evidência.

# DEVELOPMENT_LOG.md — Registro cronológico

### Respostas do usuário às escolhas de produto (2026-09-26)

O usuário respondeu às escolhas imediatas e de fases futuras registradas em `docs/DECISOES_DO_USUARIO.md`. Foram aceitas as recomendações para Windows 11, comportamento da janela, início com o Windows, superfícies, atalhos, gestos, comportamento em tela cheia (condicionado a P7), ordem de verificação/otimização, idioma, toque/caneta, acessibilidade e modo fantasma. Para Q-10, o primeiro build será um ZIP portátil sem assinatura nem instalador para uso pessoal e testes; eventual distribuição pública e assinatura ficam para decisão futura. As metas Q-08 serão escolhidas depois de medir P2.

As respostas foram registradas em `docs/DECISIONS.md` e resumidas em `docs/DECISOES_DO_USUARIO.md`; PRODUCT_SPEC.md, ARCHITECTURE.md, SECURITY.md, PROJECT_CONTEXT.md e TODO.md foram alinhados. A Etapa 0B continua aberta: P1, P3 e P2 ainda não foram executados nem verificados, e a Fase 1 não está autorizada.

### Guia simples das decisões do usuário (2026-09-26)

Criado `docs/DECISOES_DO_USUARIO.md` como lista legível das escolhas em aberto, separadas pelos momentos em que afetam o roadmap. DECISIONS.md permanece como fonte oficial; README.md e o mapa de documentação em PROJECT_CONTEXT.md apontam para o novo resumo. Q-01, Q-13, Q-15, Q-16 a Q-19 e Q-22 aparecem como resolvidas/esclarecidas, não como escolhas a responder.

### Atualização — referências visuais e recomendação de stack (2026-09-26)

O usuário esclareceu que as duas pranchas do personagem são referências para Claude criar uma proposta melhor e original, e não arte final nem especificação literal. PRODUCT_SPEC.md agora inclui ambas e registra que nome, acessórios, poses e estilo não são aprovados automaticamente; Buzzy permanece o nome atual e as superfícies seguem limitadas pela decisão Q-05. Q-16 a Q-19 foram mantidas no histórico e esclarecidas, sem bloquear o planejamento.

Na revisão independente, o Codex comparou a recomendação documental original de Win32 nativo com a necessidade prática de uma pessoa construir e manter a interface do Buzzy com apoio de Claude. Por delegação explícita do usuário, DEC-006 agora registra **WPF com C# e .NET 10 LTS como stack escolhida**. A documentação oficial da Microsoft descreve transparência WPF por janela layered ([AllowsTransparency](https://learn.microsoft.com/dotnet/api/system.windows.window.allowstransparency) e [regiões tecnológicas do WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/technology-regions-overview)); o suporte do .NET 10 LTS vai até novembro de 2028 ([ciclo de suporte .NET](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)). Essas fontes sustentam a escolha inicial, mas não verificam o comportamento do Buzzy.

O próximo marco é executar P1, P3 e P2 como protótipos descartáveis WPF, nessa ordem. P9 e Q-22 foram encerrados porque avaliavam o esforço de uma interface Win32 manual, que deixou de ser a opção escolhida. Foram atualizados PRODUCT_SPEC.md para incluir as duas referências visuais como direção conceitual, ARCHITECTURE.md 2.13, SECURITY.md 4, TODO.md, PROJECT_CONTEXT.md e o prompt dinâmico e operacional de Claude.

A revisão independente conforme PLAN_REVIEW.md concluiu que o MVP, os limites de segurança e a sequência das fases continuam coerentes. Os riscos que ainda podem mudar a stack foram transformados em gates verificáveis: P1 para clique por pixel, P3 para arraste/foco e P2 para custo em repouso. A documentação e os prompts foram sincronizados; a Fase 0 permanece aberta até os protótipos e as escolhas de Windows/janela/distribuição que bloqueiam a Fase 1.

**Status:** escolha registrada como ACCEPTED por delegação; Fase 0 continua aberta. Nenhum protótipo nem teste foi executado e não há código de produto.

> Entradas cronológicas sobre mudanças relevantes. Cada entrada registra objetivo, alterações, verificações e pendências. Não declare teste executado se ele não foi executado.

## 2026-09-25 — Fase 0: Bootstrap da documentação

**Objetivo:** criar documentação viva antes do código.

**Alterações realizadas:**

- Criados PROJECT_CONTEXT.md, ARCHITECTURE.md, DEVELOPMENT_LOG.md, DECISIONS.md, SECURITY.md e TODO.md.
- Criado CLAUDE.md com leitura obrigatória do contexto do projeto e sincronização documental.
- Registrada DEC-001 sobre documentação viva.

**Testes executados:** nenhum. Não havia código ou testes.

**Problemas encontrados:** não havia código, Git, objetivo, escopo ou stack registrados nos documentos criados naquele momento.

**Problemas corrigidos:** estrutura inicial de documentação criada.

**Pendências:** reconciliar a documentação inicial com o contexto do Buzzy e concluir a Fase 0 antes de iniciar o shell desktop.

## 2026-09-26 — Fase 0: Alinhamento do produto e revisão contínua dos planos

**Objetivo:** tornar a visão do Buzzy a referência de produto, separar essa referência do estado da fase e estabelecer uma revisão independente do plano preparado pelo agente de implementação.

**Alterações realizadas:**

- Atualizados PROJECT_CONTEXT.md, ARCHITECTURE.md, DECISIONS.md, SECURITY.md e TODO.md para refletirem o Buzzy e o próximo passo correto: Fase 0.
- Criados PRODUCT_SPEC.md e PLAN_REVIEW.md como referências estáveis.
- Criado docs/PLAN_REVIEW.md e definido context codex/PROMPT_MESTRE_BUZZY.md como instrução variável por fase; context codex/context.md passa a ser um índice e HANDOFF.md uma ponte de compatibilidade.
- Definido o papel de Claude no Fable, usando o modelo escolhido pelo usuário, como agente de planejamento e implementação; Codex faz a revisão independente e o usuário decide.
- Mantida a stack como indefinida até a comparação técnica da Fase 0.
- Ampliado PROJECT_CONTEXT.md com um inventário resumido por área, preservando a exigência inicial de orientar um agente novo sem conversa prévia.
- Atualizado prompt_usuario.md para continuar o trabalho documental anterior e recuperar a revisão de fundo mencionada no histórico, se ela ainda estiver disponível.

**Testes executados:** nenhum. Esta alteração é documental e não havia código para testar.

**Verificação realizada:** conferidos o inventário de arquivos e a coerência entre visão, escopo, fase atual, arquitetura planejada, segurança e prompt mestre variável.

**Problemas encontrados:** o contexto anterior dizia que objetivo e escopo eram desconhecidos, embora o handoff descrevesse o produto. O TODO indicava Fase 1, enquanto o handoff apontava Fase 0. O documento de contexto também misturava especificação, roadmap e prompt operacional. O histórico do Fable informa que uma revisão adversarial foi iniciada em segundo plano, mas o texto recebido não contém seus resultados.

**Problemas corrigidos:** fontes documentais separadas, próximas ações alinhadas ao contexto do Buzzy e inventário de estado resumido no PROJECT_CONTEXT.md. O prompt de continuação orienta a preservar os arquivos e recuperar a revisão anterior antes de repeti-la.

**Pendências:** recuperar ou repetir de forma não concorrente a revisão documental anterior, concluir a comparação de stack, definir a arquitetura de Fase 0, registrar critérios verificáveis para cada fase e, só então, iniciar Fase 1.

## 2026-09-26 — Referência visual do personagem Buzzy

**Objetivo:** incluir a imagem de conceito fornecida pelo usuário no projeto e torná-la utilizável por Claude/Fable como referência de direção visual.

**Alterações realizadas:**

- Copiada a imagem para `assets/references/buzzy-character-concept.png`.
- Atualizado PRODUCT_SPEC.md com a referência, seus traços visuais gerais e limites de interpretação: não é sprite sheet final, e o cursor ilustrado não faz parte do personagem.
- Atualizado PROMPT_MESTRE_BUZZY.md para que tarefas futuras de personagem/arte consultem a referência canônica em PRODUCT_SPEC.md.

**Verificações realizadas:** confirmada a existência do arquivo copiado e conferido o link relativo da especificação. Correção de 2026-09-26: o tamanho registrado nesta entrada (2.332.894 bytes) não corresponde a nenhum arquivo em disco. Os tamanhos reais, medidos depois, são 2.238.564 bytes para `buzzy-character-concept.png` e 2.411.232 bytes para `buzzy-character-concept2.png`. Nenhum teste de aplicação foi executado; não há código de produto nesta alteração.

**Pendências:** Claude/Fable deve considerar a referência no trabalho em andamento sem substituir o planejamento ou descartar decisões documentadas. Detalhes definitivos de arte e animação dependem da aprovação do usuário.

## 2026-09-26 — Revisão parcial da proposta da Fase 0

**Objetivo:** conferir se os entregáveis registrados no roadmap existem nos documentos antes da revisão final da Fase 0.

**Problemas encontrados:** TODO.md marcava a comparação de stack e as permissões por stack como concluídas, mas DEC-006, ARCHITECTURE.md 2.13 e SECURITY.md 4 ainda eram espaços reservados. TODO.md também dizia que uma revisão adversarial anterior havia sido recuperada, sem resultado correspondente no histórico.

**Alterações realizadas:** corrigidos os estados da Fase 0 e as tarefas pendentes em TODO.md e PROJECT_CONTEXT.md; atualizados README.md e PROMPT_MESTRE_BUZZY.md para refletir a continuação; registrada a resolução operacional de Q-15, pois o usuário confirmou que já enviou o pedido inicial ao Fable e limpou o arquivo; preparado um novo prompt de continuidade em prompt_usuario.md. Os documentos passam a dizer Claude no Fable com o modelo escolhido pelo usuário, incluindo Opus, sem fixar uma versão.

**Verificações realizadas:** inspeção dos documentos canônicos e do diff local. Nenhum teste de aplicação foi executado; não há código de produto.

**Próximos passos:** Claude/Fable deve concluir apenas as seções ausentes da proposta e confirmar se há evidência recuperável da revisão anterior. Depois, Codex fará a revisão integral da Fase 0. Nenhuma escolha de produto ou de stack foi aprovada.

### Atualização — workflow de pesquisa ainda ativo

O usuário mostrou que o workflow `buzzy-stack-research` continua pausado pelo limite de uso; a captura exibe Pesquisa 14/14 e Verificação 68/84. PROJECT_CONTEXT.md, TODO.md e o prompt de continuidade foram atualizados para instruir Claude a retomar ou recuperar esse mesmo workflow e usar os resultados existentes antes de preencher DEC-006. A pesquisa não deve ser duplicada. Esta é uma atualização de estado baseada na captura fornecida pelo usuário, não uma confirmação de que o workflow terminou.

## 2026-09-26 — Fase 0: comparação de stack, encaixe na arquitetura e permissões por stack

**Objetivo:** completar os três entregáveis que ainda eram espaços reservados (DEC-006, ARCHITECTURE.md 2.13 e SECURITY.md 4), recuperando o workflow de pesquisa já iniciado em vez de repetir a pesquisa.

**Workflow recuperado:** o workflow `buzzy-stack-research` foi retomado a partir do mesmo script persistido, e não recriado. Ele concluiu as três etapas: 14 pesquisadores (9 stacks e 5 tópicos do Windows independentes de stack), 84 verificações adversariais e 3 painéis de recomendação com lentes diferentes. Total de 101 subagentes.

**Limite importante dessa verificação:** os pesquisadores produziram 420 alegações, das quais 300 foram marcadas como decisivas para a escolha. O script limitava a verificação a seis alegações decisivas por assunto, então **apenas 84 das 300 foram verificadas de forma adversarial e 216 não foram**. Das 84 verificadas, 54 foram refutadas ou corrigidas, ou seja, cerca de dois terços. Isso reforça que a comparação é de mecanismo, não de medição, e que as alegações não verificadas mantêm a confiança que o pesquisador original declarou.

**Alterações realizadas:**

- `docs/DECISIONS.md`: DEC-006 escrita por completo. Método, critério que decide a comparação, tabela das nove stacks nas nove dimensões, ranking dos três painéis, recomendação (Win32 nativo, com WPF como plano B e Qt 6 Widgets como terceira opção), alternativas com o motivo de cada descarte, trade-offs, 12 riscos com mitigação, as quatro condições de protótipo e os limites da evidência. Estado UNCERTAIN: é recomendação, não decisão. Q-13 atualizada para incluir P9.
- `docs/ARCHITECTURE.md`: seção 2.13 escrita. Quais janelas existem e por quê, onde cada componente da seção 2.2 mora, correspondência entre cada decisão e a API do Windows que a realiza, escolha de desenho na CPU, o que a stack não entrega e precisa ser escrito, o que muda no plano B e sete regras válidas para qualquer stack aprovada.
- `docs/SECURITY.md`: seção 4 escrita. Processos, runtime externo, superfície extra e forma de obter o click-through por stack; seis consequências para a segurança; riscos de distribuição comuns a todas.
- `docs/TODO.md`: acrescentados os protótipos P8, P9 e P10 e a ordem de execução dos protótipos; tarefas da Fase 0 atualizadas conforme o que passou a existir; critério de conclusão corrigido.
- `docs/PROJECT_CONTEXT.md`: seções de estado, stack, rendering, limitações, decisões e próxima fase sincronizadas.
- `docs/SECURITY.md`, também: o título da seção 3 passou de "Capabilities" para "Capacidades", para não ser o único em inglês.
- `docs/TODO.md`, também: a grafia "alpha" passou a "alfa", como no resto dos documentos.
- `README.md`: nenhuma alteração nesta rodada. O índice havia sido completado com AGENTS.md, DECISIONS.md e DEVELOPMENT_LOG.md na rodada anterior, no commit f485e74. CLAUDE.md, context.md e HANDOFF.md continuam fora do índice de propósito, porque são alcançados a partir dos arquivos listados.

**Resultado da comparação:** o requisito que separa as stacks é o clique atravessar os pixels transparentes e chegar a um aplicativo de outro processo. Segundo a documentação da Microsoft, só uma janela do tipo layered com conteúdo entregue por `UpdateLayeredWindow` tem esse teste feito pelo próprio sistema. Três stacks estão nesse caminho, o que foi confirmado lendo o código de cada uma: Win32 nativo, WPF com `AllowsTransparency` e Qt 6 Widgets com janela translúcida rasterizada. As outras seis precisam de consulta periódica do cursor, de hook global de mouse ou de recorte da janela por região, e cada contorno colide com outro requisito do produto. Os três painéis, com lentes independentes, colocaram Win32 nativo em primeiro.

**Fontes consultadas:** documentação oficial da Microsoft sobre janelas layered e teste de acerto, DirectComposition, captura do mouse, ativação e foco, limiar de arraste, desktop virtual, informação de monitor, identidade de monitor por configuração de vídeo, consciência de DPI por monitor, mensagens de mudança de vídeo e de DPI, bandeja, fim de sessão, energia, inicialização com o Windows, hooks e input bruto, empacotamento, assinatura de código e pastas de dados. Também foram consultados o código-fonte público das stacks (WPF, Qt, Tauri, tao, wry, Avalonia, Electron, Flutter, Godot), as respectivas notas de versão e os registros de problemas abertos. As alegações de consumo de terceiros foram marcadas como fracas ou corrigidas durante a verificação.

**Revisão adversarial documental anterior:** ela terminou. O resultado foi lido em um arquivo de transcrição da sessão de Claude, fora deste repositório, na pasta temporária da sessão, sob o identificador de tarefa `wuqoucbsi`. Ele registra 220 subagentes, 35 achados confirmados e 64 refutados. **Esse arquivo não é evidência durável do projeto:** ele não está versionado, não foi copiado para cá e pode desaparecer com a sessão. Um leitor futuro não consegue reproduzir esses números a partir dos arquivos do repositório. Além disso, a revisão avaliou a versão dos documentos de 2026-09-25, anterior à reestruturação do Buzzy, então boa parte dos achados aponta para texto que não existe mais. O que era aproveitável foi incorporado como regra de escrita nesta rodada: status explícito em cada seção, nenhuma referência cruzada que o leitor não consiga resolver, nenhum caminho absoluto da máquina do usuário e cada documento declarando o formato das próprias entradas. Ela **não** vale como revisão do plano atual; a revisão da Fase 0 continua sendo do Codex.

**Testes executados:** nenhum. Não há código de produto. As verificações desta entrada são documentais.

**Verificações realizadas:** inspeção da árvore de arquivos e do histórico do Git antes e depois das alterações; conferência de que PRODUCT_SPEC.md, PLAN_REVIEW.md, AGENTS.md, CLAUDE.md e a seção "Fase atual" do prompt mestre não foram alterados por esta rodada; conferência da numeração dos protótipos citada em DECISIONS.md, ARCHITECTURE.md e TODO.md; conferência das datas de fim de suporte do Windows 10 nas páginas de ciclo de vida da Microsoft (suporte encerrado em 2025-10-14; atualizações de segurança estendidas para consumidores até 2027-10-12).

**Problemas encontrados:**

- Nenhuma das nove stacks tem número oficial de consumo de memória, CPU ou GPU no Windows. Cerca de metade das alegações-chave foi corrigida na verificação por exagerar o que a fonte sustenta, quase sempre em números de terceiros. A comparação de consumo é de mecanismo, não de medição.
- O limite de buscas na web da sessão foi esgotado durante a pesquisa. A verificação adversarial trabalhou abrindo as URLs já citadas.
- O esforço de desenvolvimento da stack recomendada não é estimável por documentação. A primeira posição depende dessa premissa, o que tornou o protótipo P9 condição da recomendação.

**Problemas corrigidos:** os três espaços reservados deixaram de existir e a numeração dos protótipos ficou consistente entre os documentos. O índice do README não foi alterado nesta rodada.

**Pendências:** revisão da Fase 0 pelo Codex; decisões do usuário Q-01 a Q-14 e Q-16 a Q-22; se Q-13 aprovar, os protótipos P1, P2, P3 e P9 antes de confirmar a stack. Nenhuma stack está aprovada e nenhum código pode começar.

### Atualização — leitura das pranchas conceituais

As duas pranchas em `assets/references/` foram lidas. Elas geraram quatro escolhas novas em DECISIONS.md, seção "Escolhas levantadas pela referência visual":

- Q-16: as pranchas intitulam o personagem "PIXEL — DESKTOP COMPANION", enquanto a documentação o chama de Buzzy.
- Q-17: o personagem é um primata de chapéu de palha com faixa vermelha, combinação que é a assinatura visual de um personagem de mangá conhecido. DEC-002 compromete o projeto a não copiar visual existente, então a escolha precisa ser do usuário e não pode ser presumida.
- Q-18: as poses mostram o personagem escalando, sentando e espiando sobre janelas de outros aplicativos, o que amplia o escopo de superfícies discutido em Q-05.
- Q-19: o estilo de render tridimensional com pelo e sombra suave conflita com a regra de que a silhueta precisa terminar em alfa exatamente 0, porque no Windows qualquer pixel com alfa maior que zero captura o clique.

Nenhuma alteração foi feita em PRODUCT_SPEC.md: as pranchas são direção visual do usuário, e mudar o escopo do produto por inferência é proibido nesta fase.

### Correção — auditoria da própria proposta

Depois de escrever DEC-006, ARCHITECTURE.md 2.13 e SECURITY.md 4, a proposta passou por uma auditoria independente contra os arquivos reais, com refutação adversarial de cada achado. Ela encontrou erros nesta mesma rodada, corrigidos agora:

- **Referências cruzadas erradas.** ARCHITECTURE.md 2.9 mandava calibrar o passo fixo "no protótipo P4", que é o protótipo de foco e método de entrada; DEC-006 apontava o risco de foto de janela para P1 em vez de P8; Q-18 citava um "protótipo P5 de escalada" que não existe, porque P5 é o de topologia de monitores. A conferência de numeração declarada na entrada acima estava, portanto, incompleta. As três referências foram corrigidas e nenhum protótipo novo foi numerado sem decisão do usuário.
- **Fontes atribuídas ao documento errado.** SECURITY.md remetia a DEC-006 para as fontes sobre hooks e capacidades, mas DEC-006 não tem lista de fontes. As remissões agora apontam para a pesquisa registrada neste log.
- **Estado do Git desatualizado.** Os documentos diziam "repositório local, sem remoto". O remoto `origin` passou a existir, apontando para `https://github.com/fishpedrol/Claudio.git`, com `main` rastreando `origin/main`. Se o envio chegou ao servidor não foi verificado.
- **Inventário incompleto.** A árvore de diretórios listava uma prancha conceitual; existem duas.
- **Três defeitos reais na máquina de estados**, todos corrigidos na seção 2.6:
  1. `CONVERSING` levava a `DRAGGING` prometendo voltar, mas `SETTLING` só tinha saída para `IDLE` e `FALLING`, o que retomava a autonomia com a caixa de texto aberta e violava o invariante 4. A caixa de texto passou a ser uma dimensão ortogonal ao estado, como já era a expressão.
  2. `TOPOLOGY_CHANGED` valia para "qualquer estado exceto arraste", o que incluía `HIDDEN`: uma mudança de vídeo com a sessão bloqueada traria o personagem de volta à tela. O escopo foi restringido e os estados de sistema agora só atualizam a topologia em cache.
  3. `HIDDEN` não guardava o motivo do ocultamento, então desbloquear a sessão ressuscitava um personagem que o usuário tinha escondido pela bandeja. Isso invertia a prioridade declarada em DEC-004. O motivo passou a ser guardado, e um evento de sistema não desfaz mais uma ação direta do usuário.
- **Pendências anunciadas sem rastreamento.** O nível de acessibilidade e o "modo fantasma" apareciam no texto como assunto do usuário, sem escolha correspondente. Viraram Q-20 e Q-21. O modo fantasma saiu do resultado esperado do protótipo P8, onde havia entrado como se fosse requisito.
- **Tarefas marcadas sem evidência.** Seis tarefas concluídas ganharam data e o arquivo que as comprova, conforme o formato que o próprio TODO.md declara.

Quatro invariantes novos (8 a 11) foram acrescentados à máquina de estados para que esses três defeitos possam ser detectados por teste automático quando a Fase 2 existir.

**Divergências que não foram corrigidas aqui, por estarem fora do escopo desta tarefa:**

- `context codex/PROMPT_MESTRE_BUZZY.md`, seção "Fase atual", ainda descreve DEC-006, ARCHITECTURE.md 2.13 e SECURITY.md 4 como espaços reservados. Essa seção só muda com aprovação do usuário e é o Codex quem a atualiza.
- `prompt_usuario.md` contém um rascunho de prompt que também descreve esses entregáveis como pendentes. O arquivo é do usuário.
- `AGENTS.md` cita `TODO.md`, `ARCHITECTURE.md`, `DECISIONS.md`, `PLAN_REVIEW.md` e `SECURITY.md` sem o prefixo `docs/`, e o item 7 da ordem de sincronização não repete o gate de aprovação do usuário que os outros documentos exigem.
- `docs/PRODUCT_SPEC.md` referencia apenas a primeira prancha conceitual e repete em texto livre escolhas que já têm identificador em DECISIONS.md. É documento de produto e só o usuário decide alterá-lo.

Uma verificação final da própria máquina de estados, feita com o invariante 11 como roteiro, encontrou mais três lacunas, também corrigidas: `CLIMBING` e `RESTING` não tinham nenhuma transição de saída na tabela, e os eventos `TEXT_FOCUS_GAINED` e `TEXT_FOCUS_LOST` estavam declarados sem nenhum uso. Os dois estados ganharam saída explícita e os dois eventos passaram a declarar que não mudam estado. Os quinze estados agora têm entrada e saída, exceto `BOOTING`, que só tem saída, e `EXITING`, que só tem entrada.

### Correção — tarefa da revisão de fundo devolvida a pendente

A instrução do usuário para esta fase era explícita: confirmar se a evidência da revisão adversarial anterior existe nos arquivos ou no histórico e, **se não existir, registrar que não foi possível recuperá-la e deixar a tarefa pendente**. A evidência não existe no repositório, o que já estava registrado, mas a tarefa tinha ficado marcada como concluída. Ela voltou a `- [ ]` em docs/TODO.md, com o texto que explica por que não é recuperável. O que existe é um arquivo de transcrição da sessão de Claude, fora do repositório, que não é evidência durável do projeto.

### Acréscimo — fontes primárias em DEC-006

O pedido da Fase 0 exigia que a comparação de stack registrasse "fontes primárias consultáveis". DEC-006 descrevia o método e citava a documentação em texto, mas não dava ao leitor como chegar às páginas. Foi acrescentada a seção "Fontes primárias", com os endereços das fontes oficiais que sustentam os pontos decisivos, agrupados por assunto: transparência e clique, input e foco, monitores e DPI, consumo em repouso, bandeja e distribuição, e a documentação de cada stack avaliada. A pesquisa abriu 192 páginas oficiais distintas em alegações-chave; a seção lista as que pesam na recomendação. As remissões de SECURITY.md passaram a apontar para essa seção.

### Acréscimo — critérios oficiais de repouso em DEC-011

Ao conferir se as fontes citadas em DEC-006 realmente abrem, a avaliação oficial de eficiência de energia em repouso da Microsoft mostrou uma lista de critérios que eu havia tratado como inexistente. DEC-011 dizia que não havia número oficial de referência; isso era verdade para memória e CPU, mas não para comportamento em repouso. A decisão passou a registrar seis critérios oficiais com alvo zero, entre eles "nenhum processo muda a resolução do timer do sistema" e "nenhuma atividade periódica de CPU com intervalo de até 300 ms". São os mesmos critérios que reprovam o contorno de consulta periódica do cursor usado por Tauri e Flutter, e o comportamento de timer do Godot, o que agora está ancorado em fonte oficial e não só no mecanismo. Memória e CPU animando continuam dependendo da medição do protótipo P2.

### Correção — verificação da própria entrega contra o pedido

A entrega documental passou por uma verificação independente, item a item, contra o texto do pedido da Fase 0, com refutação adversarial. Ela encontrou os problemas abaixo, todos corrigidos.

**Fatos errados sobre as pranchas conceituais.** Q-16 dizia que as duas pranchas trazem o título "PIXEL — DESKTOP COMPANION"; só a primeira traz. Q-19 atribuía render tridimensional às duas; a primeira é ilustração bidimensional. Q-18 descrevia as superfícies das poses como janelas de outros aplicativos; as pranchas não as identificam, então o texto passou a descrever o que se vê e a marcar a leitura como interpretação. O parágrafo sobre "interação com o cursor" foi reescrito: PRODUCT_SPEC.md já fechou esse ponto ao registrar que o cursor desenhado não pertence ao produto, e reabri-lo era ampliar escopo por inferência.

**Referências que não resolviam.** SECURITY.md remetia a DEC-006 para as fontes sobre hooks e input bruto, mas essas páginas não estavam na lista. Foram acrescentadas, junto com as fontes das quatro stacks descartadas que ainda não apareciam: Avalonia, Tauri, Flutter e Godot, mais a camada visual do WinUI 3.

**Promessa de proteção maior do que a real.** A seção de distribuição dizia que o pacote MSIX protege os binários sem exigir administrador. Passou a delimitar o que isso cobre, que é só a troca de arquivos, e a registrar que o aplicativo continua rodando com confiança total e que instalar fora da Store exige um certificado já confiável, cuja instalação pede administrador. A seção também dizia que a consulta periódica do cursor "não é falha de segurança"; isso minimizava o fato de ela ler a posição do cursor em todo o desktop, além da capacidade permitida na seção 3.1.

**Dimensões do pedido que faltavam.** O pedido exigia registrar superfície de rede e arquivos por stack. A seção 4 só tratava disso para duas stacks. Ganhou uma tabela nova com o que cada stack faz por conta própria, que registra o ponto decisivo: nas duas stacks com motor web, "rede zero" não é verificável só pelo código do aplicativo.

**Lacuna contra PRODUCT_SPEC.md.** A personalidade local é item do MVP e não tinha componente, modelo de dados nem critério de aceitação. Virou um componente na seção 2.2, ganhou a seção 2.11 com os pesos que a compõem, entrou no escopo da Fase 7 e ganhou um critério que verifica que trocar a personalidade muda o comportamento sem mudar a máquina de estados.

**Critérios que não eram observáveis.** Dois critérios de aceitação foram reescritos: "o movimento parece contínuo" virou uma medição por gravação de tela, e o limite de alfa do asset deixou de ser definido dentro da própria fase que ele deveria julgar, passando a vir do protótipo P1.

**Pendências sem rastreamento.** O orçamento de tempo do protótipo P9 era condição da recomendação e não existia como escolha; virou Q-22. Sem ele, P9 não teria critério de reprovação.

**Outras correções.** O invariante 11 reprovava o estado `BOOTING`, que por construção não tem entrada. Q-08 declarava bloquear só a Fase 11, mas as Fases 6 e 10 também comparam medições com as metas. A seção 2.13.4 atribuía ao protótipo P2 uma comparação entre desenho na CPU e na GPU que ele não faz. O histórico de mudanças arquiteturais não registrava o preenchimento da seção 2.13 nem as correções da máquina de estados. Duas tarefas marcadas como concluídas afirmavam evidência que não as sustentava, e foram reescritas para separar o que está no repositório do que veio da saída da sessão. Um caminho absoluto da máquina do usuário foi retirado do log, já que o próprio log declara essa regra de escrita.

**Mudança de termo em uma escolha.** Q-13 foi escrita citando "P1 a P3", que é como ela chegou ao usuário. Depois P9 passou a existir e a opção (a) foi alterada em silêncio. O texto voltou a "P1, P2 e P3" e registra, de forma visível, que a recomendação é incluir P9 e por quê.

### Acréscimo — fontes das leituras de código e dos números de tamanho

A verificação da entrega deixou um achado de pé depois da refutação adversarial: afirmações que pesam na comparação não tinham fonte que o leitor conseguisse abrir. Eram três grupos: a leitura do código de cada stack, que sustenta a conclusão central de quais stacks usam o caminho de janela com teste por pixel; os números de tamanho de runtime e de binário que aparecem na tabela comparativa; e a lista de vulnerabilidades do Qt.

A seção "Fontes primárias" de DEC-006 ganhou duas subseções novas. "Leituras de código-fonte" lista os quinze arquivos abertos, por stack, com o ramo ou versão lidos. "Números de tamanho e de manutenção" lista os quatro endereços de onde vieram os valores da tabela. A seção registra que endereços de ramo podem mudar, então uma releitura futura pode não achar o mesmo trecho. As duas afirmações de SECURITY.md que dependiam dessas fontes passaram a remeter a DEC-006.

Com isso, o item 1 do pedido, que exigia fontes primárias consultáveis, fica atendido também para as afirmações que não vinham de documentação oficial.

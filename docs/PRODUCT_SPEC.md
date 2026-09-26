# PRODUCT_SPEC.md — Visão e escopo do Buzzy

> Referência estável do produto. Atualize quando o usuário mudar intenção ou escopo; o estado de implementação fica em PROJECT_CONTEXT.md.
>
> STATUS: PLANNED. Esta especificação descreve o produto desejado, não funcionalidades implementadas.

## Visão

Buzzy é um mascote digital interativo para desktop Windows: um companheiro moderno inspirado na sensação lúdica dos mascotes de desktop da era do BonziBuddy. Deve parecer um personagem vivendo no desktop, não uma janela convencional.

Buzzy terá identidade e personagem originais. Não copiar nome, personagem, visual, voz, conteúdo ou marca de qualquer mascote existente.

O personagem conceitual é um pequeno primata arbóreo antropomórfico, amigável, curioso, brincalhão, ágil e expressivo: cabeça relativamente grande, olhos expressivos, focinho curto, orelhas arredondadas, braços longos, mãos adequadas à escalada, pernas compactas e cauda longa e expressiva. A arte definitiva será produzida separadamente; o software começa com asset provisório substituível.

### Referências visuais fornecidas pelo usuário

As duas pranchas em `assets/references/buzzy-character-concept.png` e `assets/references/buzzy-character-concept2.png` são referências para Claude desenvolver uma proposta visual original e mais coesa para o Buzzy. Não são arte final, folha de sprites aprovada nem lista de requisitos. Claude pode usar delas a direção geral que ajude a representar o personagem, mas deve propor um resultado próprio para revisão do usuário.

![Primeira prancha conceitual de referência](../assets/references/buzzy-character-concept.png)

![Segunda prancha conceitual de referência](../assets/references/buzzy-character-concept2.png)

Elementos que aparecem nas pranchas — incluindo o nome "Pixel", chapéu, poses sobre superfícies, estilo de renderização, cores e acessórios — não ficam aprovados automaticamente. O nome do produto permanece Buzzy conforme esta especificação; qualquer mudança depende de decisão explícita do usuário. As poses não ampliam o escopo atual de superfícies do MVP. A proposta nova deve manter identidade visual distinta de personagens existentes, conforme DEC-002.

O cursor de mouse que aparece em um quadro é apenas parte ilustrativa e não pertence ao personagem nem ao produto. O design final, as poses e as animações continuam pendentes de aprovação do usuário.

## Escopo do MVP

O MVP deve incluir:

- Um personagem visível no desktop Windows.
- Integração com desktop e janela transparente.
- Posicionamento livre na tela.
- Arraste direto pelo mouse.
- Movimento local determinístico: caminhada, escalada, salto e queda.
- Suporte a múltiplos monitores.
- Expressões separadas da lógica de movimento.
- Interação por clique e caixa de texto com respostas locais.
- Configurações e persistência local.
- Segurança, previsibilidade e desempenho adequados para uso prolongado.

O MVP começa com uma personalidade local. Ela pode influenciar frases, reações, expressões e intensidade do comportamento sem depender de IA.

## Decisões de produto confirmadas

As decisões abaixo foram respondidas pelo usuário em 2026-09-26 e detalhadas em [DECISIONS.md](DECISIONS.md). São escopo planejado, ainda não implementado:

- **Plataforma:** Windows 11, com Windows 11 24H2 ou posterior como alvo inicial de teste.
- **Janela e acesso:** sempre no topo por padrão, opção para desligar, ícone e menu na bandeja, sem botão na barra de tarefas, ocultar/restaurar pela bandeja, escala em passos fixos, sem controle de opacidade no MVP e segunda abertura revelando a instância existente. O botão direito abre o menu.
- **Controle e movimento:** clique reage, clique duplo abre conversa e botão direito abre menu. No MVP, o personagem usa mouse e touchpad e se apoia apenas nas bordas das áreas úteis dos monitores; atravessar monitores fica ligado por padrão e pode ser desligado. Janelas de outros aplicativos, atalhos de controle, toque e caneta ficam fora do MVP.
- **Conversa e configurações:** somente português do Brasil. As janelas de conversa e configurações devem permitir navegação por teclado e leitor de tela; a janela do personagem não é alvo de leitor de tela.
- **Preferências:** iniciar com o Windows será opcional, desligado por padrão. O Buzzy será escondido ou ficará quieto durante outro aplicativo em tela cheia apenas se P7 confirmar custo desprezível. Modo fantasma não entra no MVP.
- **Build inicial:** ZIP portátil sem assinatura nem instalador para uso pessoal e testes. Distribuição pública e assinatura só serão decididas se o usuário optar por publicar para outras pessoas. O modo de empacotar o runtime .NET será definido no plano de build.

## Interação e prioridade do usuário

Prioridade de controle:

1. Ação direta do usuário.
2. Input local.
3. Eventos do sistema.
4. Comportamento autônomo.
5. Qualquer integração futura de IA.

Ciclo obrigatório de arraste:

1. Mouse down inicia DRAGGING.
2. Comportamento autônomo incompatível é interrompido.
3. O personagem acompanha o cursor sem andar, pular, fugir ou iniciar escalada.
4. Mouse up valida a posição no desktop virtual.
5. O sistema identifica monitor e superfície e só então retoma comportamento permitido.

O mascote não disputa o cursor com o usuário.

O desenho de input deve distinguir clique de arraste e garantir que teclas digitadas com a caixa de texto em foco não acionem movimento do personagem.

## Movimento, expressão e apresentação

Movimento usa máquina de estados, eventos e regras determinísticas; não depende de IA. Expressão é uma dimensão visual separada e pode mudar sem alterar a máquina de movimento.

O asset provisório não determina o formato da arquitetura. A troca futura de arte e animações deve preservar o núcleo de estado, input, movimento, desktop e segurança.

## Desktop e múltiplos monitores

O projeto deve suportar o desktop virtual real: monitores podem estar acima, abaixo, à esquerda ou à direita, com resolução, orientação e DPI diferentes. Considerar monitor primário, área útil, conexão/desconexão, mudanças de resolução/DPI e persistência de posição.

Não assumir que os monitores estão lado a lado. A Fase 0 define a abstração e os cenários verificáveis; a Fase 5 completa o tratamento de topologia e casos de borda.

## Conversa e IA

A caixa de texto do MVP usa respostas locais para validar a interação. O MVP não integra LLM, RAG, embeddings, vector database, APIs de IA, voz, speech recognition, backend, atualização automática, sincronização em nuvem, telemetria ou analytics.

Claude pode ser usado como assistente de desenvolvimento nos modelos que o usuário escolher, incluindo Fable e Opus. Isso não é uma capacidade do produto em execução. Se IA for considerada em uma versão futura, ela será opcional e externa ao núcleo determinístico; o Buzzy continuará funcionando sem ela.

## Configurações e dados locais

Persistência local pode guardar configurações, posição, monitor preferido, tamanho e preferências de comportamento aprovadas. Opacidade não é uma configuração do MVP. Não há memória de IA nem sincronização em nuvem no MVP. O esquema e a localização planejados estão em ARCHITECTURE.md e SECURITY.md.

## Segurança

Buzzy deve ser legítimo, previsível e auditável. O MVP não terá execução arbitrária, shell, PowerShell, CMD, keylogging, captura silenciosa de tela, processos ocultos, persistência escondida, coleta remota, downloads/execução de código desconhecido ou controle genérico de mouse/teclado.

Qualquer capacidade futura do sistema operacional deve seguir: intenção explícita → capacidade específica → permissão limitada → ação auditável. A necessidade real de permissões é definida antes da implementação.

## Desempenho

O aplicativo pode permanecer aberto por horas. Priorizar baixa atividade em idle, renderização eficiente, eventos em vez de polling desnecessário e dependências justificadas. A Fase 0 define metas mensuráveis de CPU, RAM, responsividade e estabilidade; a Fase 11 mede os resultados.

## Decisões abertas

- Metas numéricas de desempenho, a definir depois de medir o protótipo P2 (Q-08).
- Modelo detalhado de física, superfícies, animações e assets, conforme as fases correspondentes.
- Detalhes finais do formato de persistência e permissões específicas, seguindo ARCHITECTURE.md e SECURITY.md.
- Formato e assinatura para eventual distribuição pública, a decidir antes de publicar para outras pessoas (Q-10).

Essas decisões não devem ser inventadas pelo plano de implementação. Registrar alternativas, trade-offs e recomendação antes de codificar a parte correspondente.

# PRODUCT_SPEC.md — Visão e escopo do Buzzy

> Referência estável do produto. Atualize quando o usuário mudar intenção ou escopo; o estado de implementação fica em PROJECT_CONTEXT.md.
>
> STATUS: PLANNED. Esta especificação descreve o produto desejado, não funcionalidades implementadas.

## Visão

Buzzy é um mascote digital interativo para desktop Windows: um companheiro moderno inspirado na sensação lúdica dos mascotes de desktop da era do BonziBuddy. Deve parecer um personagem vivendo no desktop, não uma janela convencional.

Buzzy terá identidade e personagem originais. Não copiar nome, personagem, visual, voz, conteúdo ou marca de qualquer mascote existente.

O personagem conceitual é um pequeno primata arbóreo antropomórfico, amigável, curioso, brincalhão, ágil e expressivo: cabeça relativamente grande, olhos expressivos, focinho curto, orelhas arredondadas, braços longos, mãos adequadas à escalada, pernas compactas e cauda longa e expressiva. A arte definitiva será produzida separadamente; o software começa com asset provisório substituível.

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

Always-on-top, tray, minimizar/restaurar, redimensionamento, opacidade e comportamento de inicialização precisam ser avaliados na Fase 0 e registrados como decisões antes de entrarem no escopo confirmado.

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

Fable 5.1/Claude podem ser usados como assistentes de desenvolvimento. Isso não é uma capacidade do produto em execução. Se IA for considerada em uma versão futura, ela será opcional e externa ao núcleo determinístico; o Buzzy continuará funcionando sem ela.

## Configurações e dados locais

Persistência local pode guardar configurações, posição, monitor preferido, tamanho, opacidade e preferências de comportamento quando aprovadas. Não há memória de IA nem sincronização em nuvem no MVP. A lista exata, formato e localização serão decididos na Fase 0 após a escolha da stack.

## Segurança

Buzzy deve ser legítimo, previsível e auditável. O MVP não terá execução arbitrária, shell, PowerShell, CMD, keylogging, captura silenciosa de tela, processos ocultos, persistência escondida, coleta remota, downloads/execução de código desconhecido ou controle genérico de mouse/teclado.

Qualquer capacidade futura do sistema operacional deve seguir: intenção explícita → capacidade específica → permissão limitada → ação auditável. A necessidade real de permissões é definida antes da implementação.

## Desempenho

O aplicativo pode permanecer aberto por horas. Priorizar baixa atividade em idle, renderização eficiente, eventos em vez de polling desnecessário e dependências justificadas. A Fase 0 define metas mensuráveis de CPU, RAM, responsividade e estabilidade; a Fase 11 mede os resultados.

## Decisões abertas

- Stack, framework e APIs de janela.
- Comportamento de always-on-top, tray, minimizar/restaurar, redimensionamento, opacidade e inicialização.
- Modelo detalhado de física, superfícies, animações e assets.
- Formato de persistência e permissões específicas.
- Metas de desempenho e critérios de aceitação por funcionalidade.

Essas decisões não devem ser inventadas pelo plano de implementação. Registrar alternativas, trade-offs e recomendação antes de codificar a parte correspondente.

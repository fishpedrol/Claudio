# DECISIONS.md — Decisões do Buzzy

> Registro permanente. Decisões substituídas permanecem no histórico e apontam para a decisão nova.
>
> **Formato de cada decisão:** título com o ID (`DEC-nnn`, numeração sequencial, nunca reutilizada), seguido de data, estado da decisão, STATUS, problema, decisão, alternativas consideradas, motivo, trade-offs e consequências. Decisões novas entram no fim da lista principal. Escolhas que dependem do usuário ficam em "Escolhas em aberto", com ID `Q-nn`.
>
> **Estado da decisão:** ACCEPTED (aceita pelo usuário), SUPERSEDED por DEC-xxx (substituída), UNCERTAIN (proposta ou escolha ainda em avaliação).
>
> **STATUS** segue AGENTS.md: VERIFIED significa implementação testada; PLANNED significa que a decisão está aceita, mas sua realização ainda não foi verificada; UNCERTAIN significa que a escolha segue aberta.

## DEC-001 — Documentação viva com PROJECT_CONTEXT.md como resumo do estado

- **Data:** 2026-09-25
- **Estado da decisão:** ACCEPTED
- **STATUS:** PLANNED
- **Problema:** contexto de projeto poderia ficar disperso entre conversas.
- **Decisão:** manter documentação especializada, com PROJECT_CONTEXT.md para estado atual, DEVELOPMENT_LOG.md para histórico, ARCHITECTURE.md para arquitetura, DECISIONS.md para escolhas, SECURITY.md para segurança e TODO.md para tarefas. PRODUCT_SPEC.md registra a intenção estável do produto; PROMPT_MESTRE_BUZZY.md contém a instrução variável da fase atual; PLAN_REVIEW.md registra o método de revisão.
- **Alternativas consideradas:** concentrar tudo em um README, depender do histórico do Git ou misturar instruções operacionais e produto em um único handoff.
- **Motivo:** cada fonte tem um papel único, o estado atual fica curto e a instrução de cada fase pode mudar sem reescrever a visão do produto.
- **Trade-offs:** é preciso sincronizar fontes ao final de cada fase e apontar claramente qual documento é autoritativo para cada tipo de informação.
- **Consequências:** toda fase termina com a sincronização descrita em AGENTS.md. Um novo agente começa por PROJECT_CONTEXT.md e segue os links para o detalhe.
- **Histórico:** o texto de 2026-09-25 previa seis documentos. Em 2026-09-26 ele foi ampliado para incluir PRODUCT_SPEC.md, PLAN_REVIEW.md e o prompt mestre, sem mudar a essência da decisão.

## DEC-002 — Buzzy é um mascote de desktop original

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED
- **STATUS:** PLANNED
- **Problema:** definir a identidade do produto sem perder a experiência lúdica dos antigos mascotes de desktop nem copiar um personagem existente.
- **Decisão:** criar um companheiro moderno de desktop para Windows, inspirado na categoria e na sensação de interação dos mascotes da era do BonziBuddy, mas com personagem, nome, identidade visual, conteúdo e implementação originais.
- **Alternativas consideradas:** copiar diretamente um personagem existente ou criar uma janela convencional sem presença no desktop.
- **Motivo:** preservar a nostalgia da interação enquanto se cria um produto próprio, moderno e auditável.
- **Trade-offs:** a experiência precisa parecer integrada ao desktop sem recorrer a comportamentos intrusivos ou identidade copiada.
- **Consequências:** o asset provisório e a arte final precisam ser visualmente distintos de mascotes conhecidos. O conceito de primata, somado à referência declarada, exige cuidado extra com silhueta e cor. Essa categoria de software também tem histórico de adware, então qualquer comportamento que pareça coleta de dados prejudica a confiança e a reputação no SmartScreen (DEC-005, SECURITY.md).

## DEC-003 — MVP local sem IA integrada

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED
- **STATUS:** PLANNED
- **Problema:** permitir que o mascote funcione sem dependência de modelos, serviços externos ou recursos que aumentem superfície e complexidade.
- **Decisão:** o MVP usa comportamento determinístico e respostas locais. LLM, RAG, embeddings, APIs de IA, voz, backend, sincronização em nuvem, telemetria, analytics e atualização automática ficam fora do MVP. Usar Claude nos modelos escolhidos pelo usuário, incluindo Fable e Opus, como assistente de desenvolvimento não significa integrar IA ao produto.
- **Alternativas consideradas:** incluir IA online desde a primeira versão ou criar abstrações para providers antes de existir uma necessidade do MVP.
- **Motivo:** o núcleo deve funcionar quando qualquer IA futura estiver desligada ou indisponível; reduzir custo, dependências e riscos de dados.
- **Trade-offs:** a conversa do MVP será limitada a respostas locais.
- **Consequências:** a conversa usa uma tabela local de intenções (ARCHITECTURE.md, seção 2.11). O portão de APIs proibidas bloqueia rede no build (SECURITY.md, seção 8).

## DEC-004 — A ação direta do usuário tem prioridade máxima

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED
- **STATUS:** PLANNED
- **Problema:** movimento autônomo pode disputar controle com quem tenta pegar ou reposicionar o personagem.
- **Decisão:** interação direta, especialmente o arraste, interrompe movimento autônomo incompatível. Durante DRAGGING o personagem acompanha o cursor; ao soltar, valida posição, monitor e superfície antes de retomar comportamento.
- **Alternativas consideradas:** deixar o movimento autônomo continuar e ajustar a posição em paralelo.
- **Motivo:** o personagem deve parecer um objeto que o usuário controla diretamente, sem lutar contra o cursor.
- **Trade-offs:** o sistema de input precisa coordenar cancelamento e retomada de estados.
- **Consequências:** a máquina de estados tem os estados `PRESSED`, `DRAGGING` e `SETTLING` e invariantes testáveis de que nada autônomo acontece neles (ARCHITECTURE.md, seção 2.6).

## DEC-005 — Segurança explícita e capacidades limitadas

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED
- **STATUS:** PLANNED
- **Problema:** um mascote residente no desktop pode adquirir permissões e comportamento invasivos se a fronteira com o sistema operacional não for limitada.
- **Decisão:** sem execução arbitrária de comandos, shell, PowerShell, CMD, keylogging, captura silenciosa de tela, processos ocultos, persistência escondida, coleta remota ou telemetria não solicitada. Qualquer integração futura com o sistema deve usar intenção explícita, capacidade específica e permissão limitada.
- **Alternativas consideradas:** expor comandos genéricos ou permissões amplas e confiar apenas na interface.
- **Motivo:** manter o produto previsível, auditável e seguro por desenho.
- **Trade-offs:** cada nova capacidade de sistema precisa de justificativa, permissão e documentação próprias.
- **Consequências:** SECURITY.md lista as capacidades permitidas e proibidas. Como o Windows não pede permissão para hooks globais nem para captura de tela, a garantia vem de um portão automático no build. Uma stack que só consegue o comportamento do MVP com hook global de mouse entra em conflito com esta decisão (DEC-006).

## DEC-006 — Stack de desktop (recomendação pendente)

(Seção preenchida após a comparação de tecnologias.)

## DEC-007 — Estrutura: um processo, núcleo puro, um adaptador de plataforma

- **Data:** 2026-09-26
- **Estado da decisão:** UNCERTAIN (proposta da Fase 0, aguardando revisão do Codex e aprovação do usuário)
- **STATUS:** UNCERTAIN
- **Problema:** definir fronteiras que permitam testar o comportamento sem Windows, trocar a arte sem mexer na lógica e manter a superfície de segurança pequena, sem criar complexidade especulativa.
- **Decisão:** um executável, um processo e três camadas de código. O núcleo puro reúne mundo do desktop, estados, arbitragem de input, movimento, conversa e esquema de configurações, sem nenhuma chamada ao sistema. O adaptador de plataforma é o único código que chama o Windows. A raiz de composição liga os dois. Detalhes em ARCHITECTURE.md, seções 2.1 a 2.3.
- **Alternativas consideradas:** (a) tudo na camada de UI do framework, mais rápido de começar, mas difícil de testar e acoplado à stack; (b) processos separados para núcleo e janela, sem necessidade no MVP e com comunicação entre processos a proteger; (c) sistema de plugins para comportamentos, abstração especulativa.
- **Motivo:** a maior parte dos critérios de aceitação (estados, arraste, monitores, movimento) vira teste automático sem janela. O código que toca o Windows fica concentrado e auditável.
- **Trade-offs:** o núcleo precisa de tipos próprios para eventos e geometria, e o adaptador precisa traduzir mensagens do Windows para esses tipos.
- **Consequências:** a estrutura de diretórios do código segue essas três camadas. Testes do núcleo rodam em qualquer máquina de build. O portão de APIs proibidas precisa olhar só o adaptador e as dependências.

## DEC-008 — Coordenadas canônicas e modelo de monitores

- **Data:** 2026-09-26
- **Estado da decisão:** UNCERTAIN (proposta)
- **STATUS:** UNCERTAIN
- **Problema:** o Buzzy anda, cai e é arrastado entre monitores de qualquer disposição, resolução, orientação e escala, que podem aparecer e sumir.
- **Decisão:** processo Per-Monitor V2 declarado no manifesto; coordenadas canônicas em pixels físicos do desktop virtual, aceitando valores negativos; monitor identificado pelo caminho de dispositivo obtido por `QueryDisplayConfig`, com o nome GDI e o retângulo como alternativas; física em DIPs convertida pela escala do monitor da âncora; posição persistida como monitor mais posição relativa na área útil, com restauração em cascata. Detalhes em ARCHITECTURE.md, seções 2.4, 2.5 e 2.8.
- **Alternativas consideradas:** (a) coordenadas em DIPs do framework, que em escalas mistas formam "ilhas" com lacunas e sobreposições, segundo a documentação do Qt e relatos do Electron e do WPF; (b) usar o retângulo envolvente do desktop virtual como mundo, o que inclui áreas vazias fora de qualquer monitor; (c) persistir `HMONITOR` ou o nome `\\.\DISPLAYn`, que não são estáveis entre sessões.
- **Motivo:** documentação da Microsoft. O monitor principal está em (0,0), outros podem ter coordenadas negativas e o desktop virtual tem áreas vazias. `HMONITOR` só vale durante a execução. PMv2 é o modo recomendado e o único em que o Windows não estica a janela nem virtualiza coordenadas. Fontes: The Virtual Screen, HMONITOR and the Device Context, DISPLAYCONFIG_TARGET_DEVICE_NAME e High DPI Desktop Application Development, no Microsoft Learn.
- **Trade-offs:** mais código de adaptação no adaptador de plataforma. A estabilidade da chave do monitor precisa de protótipo (P5).
- **Consequências:** o mundo do desktop tem testes com topologias de exemplo desde a Fase 1. A matriz S1 a S12 da Fase 5 (TODO.md) usa esse modelo.

## DEC-009 — Arbitragem de input: janela que não ativa, caixa de texto separada, limiar do sistema

- **Data:** 2026-09-26
- **Estado da decisão:** UNCERTAIN (proposta; depende de P3 e P4)
- **STATUS:** UNCERTAIN
- **Problema:** separar clique de arraste, não roubar foco do aplicativo do usuário e garantir que teclas digitadas na caixa de texto nunca virem comandos de movimento.
- **Decisão:** a janela do personagem não ativa ao ser clicada (`MA_NOACTIVATE`). O arraste é manual, com captura do mouse, sem o loop modal de mover do Windows. Clique e arraste se separam pelo retângulo `SM_CXDRAG`/`SM_CYDRAG` lido por DPI, sem limiar de tempo. A caixa de texto é outra janela, ativável, que pede foco só em resposta a clique do usuário. Não há hook global, atalho global nem comando de movimento por teclado no MVP proposto. Detalhes em ARCHITECTURE.md, seção 2.7.
- **Alternativas consideradas:** (a) arrastar pelo loop modal do sistema (`HTCAPTION`, `DragMove`, região de arraste do CSS), que congela a lógica própria e impede separar clique de arraste; (b) limiar de tempo para separar clique de arraste, que a Microsoft não define e que prejudica quem usa ClickLock; (c) caixa de texto na mesma janela do personagem, impossível numa janela layered com conteúdo próprio, porque controles filhos não são desenhados nela; (d) ativar a janela a cada clique, que rouba o foco de quem está digitando.
- **Motivo:** documentação da Microsoft sobre `SetCapture`, `WM_MOUSEACTIVATE`, `WS_EX_NOACTIVATE`, `SetForegroundWindow`, `GetSystemMetrics` e `WM_ENTERSIZEMOVE`, reunida pela pesquisa de 2026-09-26.
- **Trade-offs:** segundo a documentação de `SetCapture`, só a janela em primeiro plano captura o mouse plenamente. Uma janela que não ativa pode perder o arraste em movimentos rápidos. P3 testa isso e as alternativas.
- **Consequências:** P3 e P4 são pré-requisitos das Fases 3 e 7. Se P3 falhar, a decisão é revista antes da Fase 3.

## DEC-010 — Persistência local: JSON versionado com gravação atômica

- **Data:** 2026-09-26
- **Estado da decisão:** UNCERTAIN (proposta)
- **STATUS:** UNCERTAIN
- **Problema:** guardar posição e preferências de forma previsível, recuperável e sem dados sensíveis.
- **Decisão:** um arquivo `settings.json` com `schemaVersion`, em `%LOCALAPPDATA%\Buzzy` sem pacote ou na pasta local do pacote com MSIX. Validação ao ler, gravação em arquivo temporário seguida de substituição atômica, cópia `.bak` e gravação incremental. Nada de texto digitado. Detalhes em ARCHITECTURE.md, seção 2.12, e SECURITY.md, seção 5.
- **Alternativas consideradas:** (a) registro do Windows, menos transparente para o usuário e mais difícil de inspecionar e migrar; (b) banco de dados local, sem necessidade para uma dezena de campos; (c) gravar só ao sair, o que perde dados, porque o Windows pode encerrar o processo no desligamento e dá cerca de 2 s na suspensão.
- **Motivo:** documentação da Microsoft sobre `WM_ENDSESSION`, `WM_POWERBROADCAST` e pastas conhecidas; baixo volume de dados.
- **Trade-offs:** a pasta de dados muda entre distribuição sem pacote e MSIX, e trocar de modelo exige migração explícita.
- **Consequências:** a gravação da posição começa na Fase 5 e o esquema completo na Fase 8.

## DEC-011 — Tempo ocioso por eventos e plano de medição de desempenho

- **Data:** 2026-09-26
- **Estado da decisão:** UNCERTAIN (proposta; metas numéricas em Q-08)
- **STATUS:** UNCERTAIN
- **Problema:** o Buzzy fica aberto por horas e precisa gastar quase nada parado, sem metas inventadas.
- **Decisão:** sem timer periódico quando nada se move ou anima; passo fixo de simulação só com movimento, animação ou arraste; redesenho só quando o quadro muda; timers de animação sem forçar resolução de 1 ms. Medição com o protocolo abaixo, desde a Fase 1.

  | ID | Métrica | Como medir | Quando |
  |---|---|---|---|
  | M1 | CPU do processo e dos filhos, por estado (`RESTING`, `IDLE`, `WALKING`, `DRAGGING`, `CONVERSING`) | Contador `\Process(*)\% Processor Time` via `Get-Counter`, amostra de 1 s por 10 min em cada estado; média e percentil 95 | Toda fase a partir da 1 |
  | M2 | Acordadas por segundo em repouso | Process Explorer (variação de trocas de contexto) ou Windows Performance Recorder | Fases 1, 4, 6, 11 |
  | M3 | Memória privada da árvore de processos | Contadores `Private Bytes` e `Working Set - Private` no início, em 1 h e em 8 h | Fases 1, 10, 11 |
  | M4 | GPU por processo | Contadores `GPU Engine` | Fases 1, 6, 11 |
  | M5 | Atraso do arraste | Carimbo de tempo entre receber o movimento do mouse e aplicar a posição da janela, registrado pelo próprio app em modo de diagnóstico | Fases 3, 11 |
  | M6 | Tempo até o primeiro quadro | Carimbo do início do processo até o primeiro quadro desenhado | Fases 1, 11 |
  | M7 | Estabilidade longa | 8 h de uso misto: sem falha, sem crescimento contínuo de memória nem de objetos GDI e USER | Fase 10 |

- **Metas:** não há número oficial de referência para apps residentes (pesquisa de 2026-09-26). A proposta é derivar as metas das medições do protótipo P2, feitas no hardware do usuário. Candidatas para a decisão Q-08:
  - repouso sem acordada periódica vinda do app e CPU indistinguível da linha de base de P2;
  - arraste com a janela no máximo um quadro atrás do cursor, porque a posição é aplicada no mesmo tratamento da mensagem e o compositor acrescenta um quadro;
  - memória depois de 8 h igual à memória depois de 1 h mais uma margem definida pelo usuário;
  - CPU e GPU com animação dentro de um múltiplo da linha de base de P2 definido pelo usuário.
- **Alternativas consideradas:** (a) loop fixo a 60 Hz sempre ligado, simples e caro em repouso; (b) metas copiadas de benchmarks de terceiros, que a pesquisa encontrou sem metodologia confiável.
- **Motivo:** documentação do Windows sobre `GetMessage`, `UpdateLayeredWindow` e sinais de energia. Frameworks com loop de renderização contínuo (`CompositionTarget.Rendering`, WebView com animação ativa) só ficam ociosos se o loop for desligado explicitamente.
- **Trade-offs:** o núcleo precisa informar quando pode dormir. Medições dependem do hardware e precisam ser repetidas na mesma máquina.
- **Consequências:** um script de medição é entregue na Fase 1. Os resultados de cada fase entram no DEVELOPMENT_LOG.md.

## DEC-012 — Revisão do roadmap de fases

- **Data:** 2026-09-26
- **Estado da decisão:** UNCERTAIN (proposta)
- **STATUS:** UNCERTAIN
- **Problema:** a sequência anterior tinha dependências invertidas e deixava testes, segurança e desempenho para o fim.
- **Decisão:** as mudanças listadas em TODO.md, seção "Mudanças propostas em relação ao roadmap anterior". As principais são persistência mínima da posição na Fase 5, sprite estático, portão de segurança e medição na Fase 1, critérios [AUTO], [MANUAL] e [HW] em toda fase, e a Etapa 0B de protótipos.
- **Alternativas consideradas:** manter a sequência anterior, em que o critério da Fase 5 dependia de uma persistência que só chegava na Fase 8.
- **Motivo:** cada fase só pode ser verificada com o que já existe.
- **Trade-offs:** a Fase 5 fica maior.
- **Consequências:** TODO.md é o roadmap único. A ordem entre as Fases 10 e 11 continua em Q-11.

## Escolhas em aberto

Decisões que só o usuário pode tomar. Cada uma tem uma recomendação, mas nenhuma está aceita. Todas são STATUS: UNCERTAIN.

| ID | Escolha | Opções | Recomendação | Bloqueia |
|---|---|---|---|---|
| Q-01 | Stack | Ver DEC-006 | Ver DEC-006 | Fase 1 |
| Q-02 | Versões do Windows | (a) só Windows 11; (b) Windows 11 e Windows 10 22H2 | (a) Windows 11 24H2 ou posterior como alvo testado. O Windows 10 perdeu suporte em 2025-10-14 e o ESU para consumidores vai até 2027-10-12, segundo as páginas de ciclo de vida da Microsoft lidas em 2026-09-26. | Fase 1 |
| Q-03 | Janela | Sempre no topo; ícone na bandeja; botão na barra de tarefas; minimizar; redimensionar; opacidade | Sempre no topo ligado por padrão e desligável. Ícone na bandeja com menu. Sem botão na barra de tarefas. "Minimizar" vira esconder e mostrar pela bandeja. Tamanho em passos fixos de escala. Opacidade fica para depois do MVP. O mesmo menu abre com o botão direito sobre o personagem, porque o Windows 11 pode esconder o ícone no menu de ícones ocultos. Abrir o app de novo mostra o Buzzy existente. | Fase 1 |
| Q-04 | Iniciar com o Windows | Não oferecer; oferecer desligado por padrão; ligado por padrão | Oferecer, desligado por padrão, ativado só pelo usuário. O mecanismo depende de Q-10. | Fase 8 |
| Q-05 | Superfícies do MVP | (a) só bordas das áreas úteis; (b) também janelas de outros aplicativos | (a). Janelas como superfície exigem rastrear outras janelas, custam CPU e dependem de UIPI com apps elevados; ficam para depois do MVP. Atravessar monitores sozinho: ligado por padrão e desligável. | Fase 4 |
| Q-06 | Atalhos de teclado | Nenhum; só com o personagem focado; atalho global | Nenhum no MVP. | Fase 7 |
| Q-07 | Gestos | Clique, clique duplo, botão direito | Clique: reação. Clique duplo: conversar. Botão direito: menu. | Fase 3 |
| Q-08 | Metas de desempenho | Números para M1 a M7 | Definir depois de P2, a partir das candidatas de DEC-011. | Fase 1 (linha de base) e Fase 11 |
| Q-09 | App em tela cheia | (a) nada; (b) esconder ou ficar quieto enquanto houver app em tela cheia | (b), com consulta a cada poucos segundos, se P7 confirmar custo desprezível. | Fase 8 |
| Q-10 | Distribuição e assinatura | Zip portátil; instalador por usuário; MSIX direto; Microsoft Store; certificado OV | Zip portátil sem assinatura só para testes. Para usuários, decidir entre Store (MSIX, sem aviso do SmartScreen) e instalador assinado com certificado OV. | Fase 1 (formato do build) e Fase 9 |
| Q-11 | Ordem das Fases 10 e 11 | (a) manter: 10 verifica, 11 otimiza e repete a regressão; (b) inverter: otimizar antes da verificação final | (a), que respeita a numeração atual e fecha com a regressão reexecutada. | Fase 10 |
| Q-12 | Idioma | Só português do Brasil; vários idiomas | Só português do Brasil no MVP, com textos fora do código. | Fase 7 |
| Q-13 | Protótipos antes da stack | (a) aprovar a stack só depois de P1 a P3; (b) aprovar já e fazer os protótipos no início da Fase 1 | (a). Transparência com clique, custo em repouso e arraste sem roubar foco são os três riscos que decidem a stack. | Fase 1 |
| Q-14 | Toque e caneta | Incluir no MVP; deixar para depois | Deixar para depois; o MVP mira mouse e touchpad. | Fase 3 |
**Q-15 — RESOLVIDA (processo, sem impacto no produto):** o usuário enviou o pedido inicial ao Fable pelo terminal e limpou `prompt_usuario.md` depois de copiar o texto. Não é necessário restaurar aquele pedido. O arquivo será usado como rascunho do próximo prompt e pode ser limpo após o envio; o prompt mestre continua sendo a instrução operacional canônica.

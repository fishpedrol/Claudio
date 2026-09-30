# DECISIONS.md — Decisões do Buzzy

> Registro permanente. Decisões substituídas permanecem no histórico e apontam para a decisão nova.
>
> **Formato de cada decisão:** título com o ID (`DEC-nnn`, numeração sequencial, nunca reutilizada), seguido de data, estado da decisão, STATUS, problema, decisão, alternativas consideradas, motivo, trade-offs e consequências. Decisões novas entram no fim da lista principal. Escolhas do usuário ficam nesta seção com ID `Q-nn`, separadas entre respostas registradas e pendências.
>
> **Estado da decisão:** ACCEPTED (aceita pelo usuário), SUPERSEDED por DEC-xxx (substituída), UNCERTAIN (proposta ou escolha ainda em avaliação).
>
> **Como uma escolha `Q-nn` é encerrada:** quando o usuário decide, registre a resposta e a data na tabela de decisões de produto. Se a resposta muda o produto ou a arquitetura, ela também vira uma decisão `DEC-nnn` nova, ou muda o estado de uma existente. Uma escolha nunca é apagada.
>
> **STATUS** segue AGENTS.md: VERIFIED significa implementação testada; PLANNED significa que a decisão está aceita, mas sua realização ainda não foi verificada; UNCERTAIN significa que a escolha segue aberta.

## DEC-001 — Documentação viva com PROJECT_CONTEXT.md como resumo do estado

- **Data:** 2026-09-25
- **Estado da decisão:** ACCEPTED
- **STATUS:** PLANNED
- **Problema:** contexto de projeto poderia ficar disperso entre conversas.
- **Decisão:** manter documentação especializada, com PROJECT_CONTEXT.md para estado atual, DEVELOPMENT_LOG.md para histórico, ARCHITECTURE.md para arquitetura, DECISIONS.md para escolhas, SECURITY.md para segurança e TODO.md para tarefas. PRODUCT_SPEC.md registra a intenção estável do produto; TODO.md define as fases; PROMPT_MESTRE_BUZZY.md contém regras estáveis; prompt_usuario.md registra a diretiva de execução atual; PLAN_REVIEW.md descreve a revisão do Codex quando solicitada.
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
- **Consequências:** o Buzzy não copia outros mascotes de desktop. *Atualização de 2026-09-29 (DEC-019):* a semelhança com o Luffy — chapéu de palha e personalidade — é intencional, por decisão do usuário; a regra anterior de distância visual do Luffy foi retirada. Essa categoria de software também tem histórico de adware, então qualquer comportamento que pareça coleta de dados prejudica a confiança e a reputação no SmartScreen (DEC-005, SECURITY.md).

## DEC-003 — MVP local sem IA integrada

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED
- **STATUS:** PLANNED
- **Problema:** permitir que o mascote funcione sem dependência de modelos, serviços externos ou recursos que aumentem superfície e complexidade.
- **Decisão:** Buzzy é um mascote de desktop com personalidade expressa por movimento, expressões e gestos não verbais. Não terá chat, conversa digitada, campo de texto, respostas escritas ou assistência para tarefas gerais. O aplicativo não terá IA integrada no escopo atual ou futuro planejado. LLM, RAG, embeddings, APIs de IA, voz, backend, sincronização em nuvem, telemetria, analytics e atualização automática ficam fora do produto. Usar Claude nos modelos escolhidos pelo usuário, incluindo Fable e Opus, como ferramenta de desenvolvimento não significa integrar IA ao produto. Reabrir a decisão de IA somente se o usuário pedir isso explicitamente.
- **Alternativas consideradas:** incluir respostas generativas ou preparar abstrações para provedores de IA. Ambas desviam o produto do mascote simples e acrescentam custo, dependências e risco de dados sem necessidade.
- **Motivo:** a diversão vem do personagem, das animações, do movimento e das reações. O usuário quer um mascote no desktop enquanto programa ou joga, não um sistema para realizar tarefas gerais.
- **Trade-offs:** personalidade e autonomia são definidas por regras determinísticas e não verbais; não existe entrada de texto, diálogo ou interpretação de pedidos.
- **Consequências:** a personalidade usa dados locais de comportamento e expressões (ARCHITECTURE.md, seção 2.11). Não há interface de provedor de IA nem caminho de rede no aplicativo. O portão de APIs proibidas bloqueia rede no build (SECURITY.md, seção 8).

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

## DEC-006 — Stack de desktop: WPF com C# e .NET 10

- **Data:** 2026-09-26
- **Estado:** ACCEPTED por delegação explícita do usuário ao Codex.
- **STATUS:** PLANNED; a stack foi escolhida, mas a aplicação ainda não foi validada.
- **Problema:** criar um mascote de desktop com transparência por pixel, clique que atravessa a área invisível, arraste sem roubo de foco, DPI por monitor, baixo custo em repouso e capacidades limitadas.

### Decisão e motivo

Usar WPF, C# e .NET 10 LTS para janelas e controles. Concentrar chamadas Win32 num adaptador pequeno; manter o núcleo determinístico independente de WPF e do Windows.

O requisito decisivo é o clique atravessar pixels invisíveis para uma janela de outro processo. A janela layered do Windows oferece o teste por alfa; WPF expõe esse caminho por AllowsTransparency. Consultar o cursor continuamente, usar hook global ou recortar a janela a cada quadro conflita com os limites de segurança ou repouso do produto.

Win32 nativo também oferece controle por pixel, mas exigiria construir e manter manualmente a maior parte da interface. Qt foi considerado por transparência, com custo de dependências e implantação. As demais stacks avaliadas não atendiam ao requisito central diretamente ou dependiam de contornos. A escolha pondera a viabilidade de uma pessoa desenvolver e manter o projeto, não apenas desempenho teórico.

### Riscos, gates e reversão

| Risco | Validação |
|---|---|
| Transparência e click-through reais | P1; resultado limitado ao ambiente registrado em TODO.md |
| Captura de mouse sem ativar ou roubar foco | P3; se falhar após alternativas seguras, reabrir esta decisão antes da Fase 1 |
| Ociosidade, timers e animação | P2 estabelece a linha de base; investigar desempenho/memória antes da Fase 6 |
| Conversão entre DIPs do WPF e pixels do desktop, escalas mistas | Concentrar no adaptador; validar nas fases e em P6 quando houver hardware |
| Runtime .NET e tamanho do pacote | Manter SDK/runtime suportados; rever formato antes de eventual distribuição |

P1 e P2 foram aceitos pelo usuário nos limites descritos em TODO.md. P3 continua gate técnico da Fase 1; nenhum resultado ou autorização permite ignorar uma falha real. P9 foi encerrado como não aplicável porque comparava o esforço de uma interface Win32 manual.

### Fontes técnicas

- Microsoft, [Window.AllowsTransparency](https://learn.microsoft.com/dotnet/api/system.windows.window.allowstransparency) e [regiões tecnológicas do WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/technology-regions-overview).
- Microsoft, [comportamento de janelas layered](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features), [UpdateLayeredWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-updatelayeredwindow) e [WM_NCHITTEST](https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest).
- Microsoft, [ciclo de suporte do .NET](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support) e [Idle Energy Efficiency Assessment](https://learn.microsoft.com/en-us/windows-hardware/test/assessments/results-for-the-idle-energy-efficiency-assessment).

Essas fontes justificam o protótipo, mas não substituem as verificações de P1–P3 no ambiente real. A comparação detalhada das stacks e as fontes secundárias foram removidas nesta limpeza; os critérios e a escolha vigente ficam registrados acima.

### Consequências

- ARCHITECTURE.md 2.13 e SECURITY.md descrevem WPF como stack escolhida; comportamento não executado permanece PLANNED.
- O produto não usa overlay do tamanho da tela, hook global de input ou polling contínuo do cursor.
- O portão automático de APIs proibidas continua obrigatório desde a Fase 1.
## DEC-007 — Estrutura: um processo, núcleo puro, um adaptador de plataforma

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED pelo usuário em 2026-09-27
- **STATUS:** PLANNED (decisão aceita; arquitetura ainda não implementada)
- **Problema:** definir fronteiras que permitam testar o comportamento sem Windows, trocar a arte sem mexer na lógica e manter a superfície de segurança pequena, sem criar complexidade especulativa.
- **Decisão:** um executável, um processo e três camadas de código. O núcleo puro reúne mundo do desktop, estados, arbitragem de input, movimento, personalidade não verbal e esquema de configurações, sem nenhuma chamada ao sistema. O adaptador de plataforma é o único código que chama o Windows. A raiz de composição liga os dois. Detalhes em ARCHITECTURE.md, seções 2.1 a 2.3.
- **Alternativas consideradas:** (a) tudo na camada de UI do framework, mais rápido de começar, mas difícil de testar e acoplado à stack; (b) processos separados para núcleo e janela, sem necessidade no MVP e com comunicação entre processos a proteger; (c) sistema de plugins para comportamentos, abstração especulativa.
- **Motivo:** a maior parte dos critérios de aceitação (estados, arraste, monitores, movimento) vira teste automático sem janela. O código que toca o Windows fica concentrado e auditável.
- **Trade-offs:** o núcleo precisa de tipos próprios para eventos e geometria, e o adaptador precisa traduzir mensagens do Windows para esses tipos.
- **Consequências:** a estrutura de diretórios do código segue essas três camadas. Testes do núcleo rodam em qualquer máquina de build. O portão de APIs proibidas precisa olhar só o adaptador e as dependências.

## DEC-008 — Coordenadas canônicas e modelo de monitores

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED pelo usuário em 2026-09-27; estabilidade da chave será verificada em P5
- **STATUS:** PLANNED (decisão aceita; arquitetura ainda não implementada)
- **Problema:** o Buzzy anda, cai e é arrastado entre monitores de qualquer disposição, resolução, orientação e escala, que podem aparecer e sumir.
- **Decisão:** processo Per-Monitor V2 declarado no manifesto; coordenadas canônicas em pixels físicos do desktop virtual, aceitando valores negativos; monitor identificado pelo caminho de dispositivo obtido por `QueryDisplayConfig`, com o nome GDI e o retângulo como alternativas; física em DIPs convertida pela escala do monitor da âncora; posição persistida como monitor mais posição relativa na área útil, com restauração em cascata. Detalhes em ARCHITECTURE.md, seções 2.4, 2.5 e 2.8.
- **Alternativas consideradas:** (a) coordenadas em DIPs do framework, que em escalas mistas formam "ilhas" com lacunas e sobreposições, segundo a documentação do Qt e relatos do Electron e do WPF; (b) usar o retângulo envolvente do desktop virtual como mundo, o que inclui áreas vazias fora de qualquer monitor; (c) persistir `HMONITOR` ou o nome `\\.\DISPLAYn`, que não são estáveis entre sessões.
- **Motivo:** documentação da Microsoft. O monitor principal está em (0,0), outros podem ter coordenadas negativas e o desktop virtual tem áreas vazias. `HMONITOR` só vale durante a execução. PMv2 é o modo recomendado e o único em que o Windows não estica a janela nem virtualiza coordenadas. Fontes: The Virtual Screen, HMONITOR and the Device Context, DISPLAYCONFIG_TARGET_DEVICE_NAME e High DPI Desktop Application Development, no Microsoft Learn.
- **Trade-offs:** mais código de adaptação no adaptador de plataforma. A estabilidade da chave do monitor precisa de protótipo (P5).
- **Consequências:** o mundo do desktop tem testes com topologias de exemplo desde a Fase 1. A matriz S1 a S12 da Fase 5 (TODO.md) usa esse modelo.

## DEC-009 — Arbitragem de input: janela que não ativa e limiar do sistema

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED pelo Codex em 2026-09-28, sob a delegação do usuário; validação por P3. P4 ficou obsoleto quando o usuário retirou conversa e entrada de texto do produto.
- **STATUS:** PLANNED (desenho aceito; comportamento ainda depende dos protótipos)
- **Problema:** separar clique de arraste e não roubar o foco do aplicativo em que o usuário está trabalhando.
- **Decisão:** a janela do personagem não ativa ao ser clicada (`MA_NOACTIVATE`). O arraste é manual, com captura do mouse, sem o loop modal de mover do Windows. Clique e arraste se separam pelo retângulo `SM_CXDRAG`/`SM_CYDRAG` lido por DPI, sem limiar de tempo. O painel de energia só recebe foco em resposta à ação explícita de dois cliques ou comando de menu; não há hook global, atalho global nem comando de movimento por teclado no MVP proposto. Detalhes em ARCHITECTURE.md, seção 2.7.
- **Alternativas consideradas:** (a) arrastar pelo loop modal do sistema (`HTCAPTION`, `DragMove`, região de arraste do CSS), que congela a lógica própria e impede separar clique de arraste; (b) limiar de tempo para separar clique de arraste, que a Microsoft não define e que prejudica quem usa ClickLock; (c) ativar a janela do personagem a cada clique, que rouba o foco de quem está digitando.
- **Motivo:** documentação da Microsoft sobre `SetCapture`, `WM_MOUSEACTIVATE`, `WS_EX_NOACTIVATE`, `SetForegroundWindow`, `GetSystemMetrics` e `WM_ENTERSIZEMOVE`, reunida pela pesquisa de 2026-09-26.
- **Trade-offs:** segundo a documentação de `SetCapture`, só a janela em primeiro plano captura o mouse plenamente. Uma janela que não ativa pode perder o arraste em movimentos rápidos. P3 é gate antes da Fase 1 para a janela do personagem; se falhar, parar e revisar a estratégia antes de implementar o shell. O antigo P4 verificava foco e IME de uma conversa; não se executa mais porque o usuário decidiu que não haverá chat nem campo de texto.
- **Consequências:** a Fase 1 pode implementar apenas o shell que não ativa depois de P3 aprovado. O painel de energia recebe foco por interação explícita e sua acessibilidade é verificada na Fase 8. Nenhum hook global, polling do cursor ou comando global de teclado entra no MVP.

## DEC-010 — Persistência local: JSON versionado com gravação atômica

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED pelo usuário em 2026-09-27
- **STATUS:** PLANNED (decisão aceita; persistência ainda não implementada)
- **Problema:** guardar posição e preferências de forma previsível, recuperável e sem dados sensíveis.
- **Decisão:** um arquivo `settings.json` com `schemaVersion`, em `%LOCALAPPDATA%\Buzzy` sem pacote ou na pasta local do pacote com MSIX. Validação ao ler, gravação em arquivo temporário seguida de substituição atômica, cópia `.bak` e gravação incremental. Nada de texto digitado. Detalhes em ARCHITECTURE.md, seção 2.12, e SECURITY.md, seção 5.
- **Alternativas consideradas:** (a) registro do Windows, menos transparente para o usuário e mais difícil de inspecionar e migrar; (b) banco de dados local, sem necessidade para uma dezena de campos; (c) gravar só ao sair, o que perde dados, porque o Windows pode encerrar o processo no desligamento e dá cerca de 2 s na suspensão.
- **Motivo:** documentação da Microsoft sobre `WM_ENDSESSION`, `WM_POWERBROADCAST` e pastas conhecidas; baixo volume de dados.
- **Trade-offs:** a pasta de dados muda entre distribuição sem pacote e MSIX, e trocar de modelo exige migração explícita.
- **Consequências:** a gravação da posição começa na Fase 5 e o esquema completo na Fase 8.

## DEC-011 — Tempo ocioso por eventos e plano de medição de desempenho

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED pelo Codex em 2026-09-28, sob a delegação do usuário; metas Q-08 aceitas pelo usuário em 2026-09-27
- **STATUS:** PLANNED (desenho aceito; implementação e métricas ainda não verificadas)
- **Problema:** o Buzzy fica aberto por horas e precisa gastar quase nada parado, sem metas inventadas.
- **Decisão:** sem timer periódico quando nada se move ou anima; passo fixo de simulação só com movimento, animação ou arraste; redesenho só quando o quadro muda; timers de animação sem forçar resolução de 1 ms. Medição com o protocolo abaixo, desde a Fase 1.

  | ID | Métrica | Como medir | Quando |
  |---|---|---|---|
  | M1 | CPU do processo e dos filhos, por estado (`RESTING`, `IDLE`, `WALKING`, `DRAGGING`, `CLIMBING`, `JUMPING`) | Contador `\Process(*)\% Processor Time` via `Get-Counter`, amostra de 1 s por 10 min em cada estado; média e percentil 95 | Toda fase a partir da 1 |
  | M2 | Acordadas por segundo em repouso | Process Explorer (variação de trocas de contexto) ou Windows Performance Recorder | Fases 1, 4, 6, 11 |
  | M3 | Memória privada da árvore de processos | Contadores `Private Bytes` e `Working Set - Private` no início, em 1 h e em 8 h | Fases 1, 10, 11 |
  | M4 | GPU por processo | Contadores `GPU Engine` | Fases 1, 6, 11 |
  | M5 | Atraso do arraste | Carimbo de tempo entre receber o movimento do mouse e aplicar a posição da janela, registrado pelo próprio app em modo de diagnóstico | Fases 3, 11 |
  | M6 | Tempo até o primeiro quadro | Carimbo do início do processo até o primeiro quadro desenhado | Fases 1, 11 |
  | M7 | Estabilidade longa | 8 h de uso misto: sem falha, sem crescimento contínuo de memória nem de objetos GDI e USER | Fase 10 |

- **Metas:** a Microsoft não publica meta de memória nem de CPU para aplicativos residentes. Publica, porém, critérios de comportamento em repouso, na avaliação oficial [Idle Energy Efficiency Assessment](https://learn.microsoft.com/en-us/windows-hardware/test/assessments/results-for-the-idle-energy-efficiency-assessment), consultada em 2026-09-26. Esses critérios valem para o sistema inteiro numa janela de 10 minutos, não por aplicativo, mas dão limites defensáveis que o Buzzy não deve violar sozinho:

  | Critério oficial | Alvo | O que significa para o Buzzy |
  |---|---|---|
  | Processos que mudam a resolução do timer do sistema | 0 | O Buzzy nunca eleva a resolução global do timer. É o item que reprova Godot |
  | Atividade periódica de CPU com intervalo de até 100 ms | 0 | Descarta consultar a posição do cursor a cada quadro, que é o contorno de click-through de Tauri e Flutter |
  | Atividade periódica de CPU com intervalo de 101 a 300 ms | 0 | Nenhum timer do Buzzy em repouso pode ficar nessa faixa |
  | Processos com mais de 1% de CPU | 0 | Em repouso, o Buzzy fica abaixo de 1% de um núcleo |
  | Pedidos de disponibilidade que impedem o sistema de dormir | 0 | O Buzzy nunca impede a máquina de suspender nem a tela de desligar |
  | Escrita periódica em disco com intervalo menor que 10 minutos | 0 | A gravação de configurações é por evento e com atraso, nunca por timer curto |

  Os números de memória e de CPU animando não têm referência oficial e saem da medição do protótipo P2, no hardware do usuário. Candidatas para a decisão Q-08:
  - repouso sem acordada periódica vinda do app e CPU indistinguível da linha de base de P2;
  - arraste com a janela no máximo um quadro atrás do cursor, porque a posição é aplicada no mesmo tratamento da mensagem e o compositor acrescenta um quadro;
  - memória depois de 8 h igual à memória depois de 1 h mais uma margem definida pelo usuário;
  - CPU e GPU com animação dentro de um múltiplo da linha de base de P2 definido pelo usuário.

### Metas Q-08 aceitas pelo usuário — 2026-09-27

P2 mostra repouso praticamente sem custo, mas não valida animação a 60 qps nem estabilidade longa de memória. Em 2026-09-27, o usuário aceitou os alvos abaixo. Eles passam a ser requisitos planejados; ainda precisam ser medidos no aplicativo real nas fases indicadas.

- **Repouso (M1/M2):** média de CPU do processo até 0,1% de um núcleo em 10 min e p95 até 1%; nenhum timer ou atividade periódica do Buzzy com intervalo de até 300 ms; nenhuma mudança na resolução global do timer.
- **Memória (M3/M7):** após o aquecimento, comparar `Private Bytes` em 1 h e 8 h; crescimento máximo proposto de 10% sobre a leitura de 1 h e sem tendência contínua. Os 10% são um limite proposto, não algo demonstrado por P2.
- **Animação (M1/M4):** meta de 60 qps entregues, com 30 qps como piso de aceitabilidade durante uso típico; média de CPU até 5% de um núcleo e GPU até 5% no hardware de referência. P2 mediu médias abaixo desses limites, mas só 39–40 qps e por apenas 10 min; a Fase 6 ainda precisa demonstrar a meta e investigar o crescimento de memória.
- **Arraste (M5):** p95 até 16,7 ms (um quadro a 60 Hz). P3 mediu cerca de 0,2 ms de latência típica no protótipo, não no aplicativo final.
- **Primeiro quadro (M6):** medir na Fase 1 e propor o limite final depois de haver um executável real; P2 não fornece essa evidência.
- **Estabilidade (M7):** oito horas de uso misto sem falha, crescimento contínuo de memória nem crescimento contínuo de objetos GDI/USER.

O usuário aceitou estas metas em 2026-09-27. A Fase 1 deve apenas estabelecer a linha de base do aplicativo; os limites de animação e estabilidade serão verificados nas fases indicadas no plano. O aceite dos alvos não apaga os resultados de P2: a animação medida ficou em ~39–40 qps e o crescimento de memória ainda precisa ser investigado antes da Fase 6. Em 2026-09-28, sob a delegação do usuário para tomar decisões de andamento, o Codex aceitou separadamente o desenho de ociosidade por eventos descrito em DEC-011.
- **Alternativas consideradas:** (a) loop fixo a 60 Hz sempre ligado, simples e caro em repouso; (b) metas copiadas de benchmarks de terceiros, que a pesquisa encontrou sem metodologia confiável.
- **Motivo:** documentação do Windows sobre `GetMessage`, `UpdateLayeredWindow` e sinais de energia. Frameworks com loop de renderização contínuo (`CompositionTarget.Rendering`, WebView com animação ativa) só ficam ociosos se o loop for desligado explicitamente.
- **Trade-offs:** o núcleo precisa informar quando pode dormir. Medições dependem do hardware e precisam ser repetidas na mesma máquina.
- **Consequências:** um script de medição é entregue na Fase 1. Os resultados de cada fase entram no DEVELOPMENT_LOG.md.

## DEC-012 — Revisão do roadmap de fases

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED pelo usuário em 2026-09-27
- **STATUS:** PLANNED (roadmap aceito; fases ainda não executadas)
- **Problema:** a sequência anterior tinha dependências invertidas e deixava testes, segurança e desempenho para o fim.
- **Decisão:** as mudanças listadas em TODO.md, seção "Mudanças propostas em relação ao roadmap anterior". As principais são persistência mínima da posição na Fase 5, sprite estático, portão de segurança e medição na Fase 1, critérios [AUTO], [MANUAL] e [HW] em toda fase, e a Etapa 0B de protótipos.
- **Alternativas consideradas:** manter a sequência anterior, em que o critério da Fase 5 dependia de uma persistência que só chegava na Fase 8.
- **Motivo:** cada fase só pode ser verificada com o que já existe.
- **Trade-offs:** a Fase 5 fica maior.
- **Consequências:** TODO.md é o roadmap único. A ordem entre as Fases 10 e 11 continua em Q-11.

## DEC-013 — Modo automático em tela cheia com privacidade

- **Data:** 2026-09-28
- **Estado da decisão:** ACCEPTED pelo Codex sob a delegação explícita do usuário para tomar decisões do projeto
- **STATUS:** PLANNED (escopo e arquitetura decididos; P7 e implementação ainda não verificados)
- **Problema:** o mascote precisa sair da tela em que o usuário está jogando, sem observar tudo o que acontece no computador.
- **Decisão:** com o modo de tela cheia ligado por padrão, uma janela em tela cheia em primeiro plano aciona a mudança do Buzzy para um monitor que não esteja coberto por essa janela. Se não houver monitor livre, Buzzy fica oculto até a tela cheia terminar. Ao sair do modo, ele volta à posição anterior; se o monitor tiver sumido, usa o fallback normal de restauração de posição. Uma ação direta do usuário tem prioridade; arrastar ou ocultar manualmente o Buzzy não será desfeito por um ciclo de correção contínua. *(Detalhamento de 2026-09-28, revisão documental:)* um arraste em curso nunca é interrompido pelo modo; se o usuário arrastar ou mostrar o Buzzy durante a tela cheia, a escolha manual passa a valer e o retorno automático à posição anterior é descartado; ocultar pela bandeja nesse período transforma a ocultação em ocultação do usuário. Pausar a autonomia não desliga o modo. As transições estão em ARCHITECTURE.md 2.6.
- **Como detectar:** preferir eventos de mudança da janela em primeiro plano e de geometria, consultando apenas o retângulo e o monitor da janela ativa. Não identificar o processo como jogo, não enumerar processos/janelas, nem ler título, texto ou pixels. O modo usa tela cheia como sinal prático: vídeos e apresentações também podem acioná-lo; jogos em janela comum não são detectados automaticamente. P7 precisa confirmar os casos de vídeo em tela cheia, jogo sem borda e exclusivo, retorno de posição e custo. Se o método de eventos falhar ou exigir atividade periódica incompatível com DEC-011, não implementar a automação até revisar a alternativa. **Hipótese a verificar em P7:** o evento de mudança de geometria, se assinado para todo o sistema, também dispara com movimentos do cursor e de outras janelas; isso não é polling, mas pode acordar o Buzzy continuamente durante jogo ou digitação. P7 mede essa taxa e, se preciso, restringe a assinatura de geometria à janela ativa, refeita a cada troca de primeiro plano. Mover uma janela sempre no topo sobre um jogo em tela cheia exclusiva também pode tirá-lo desse modo; P7 verifica que isso não ocorre.
- **Alternativas consideradas:** (a) identificar jogos pelo processo, caminho ou nome, o que amplia a leitura de metadados e exige lista de jogos; (b) consultar todo o desktop por polling contínuo, que eleva custo e escopo de observação; (c) só esconder o mascote, que não atende ao uso do segundo monitor quando ele está disponível.
- **Motivo:** a tela e a geometria da janela ativa bastam para evitar sobreposição sem saber qual jogo está aberto. O usuário mantém a presença do mascote no outro monitor e a decisão não depende de modelo, lista externa ou conteúdo da tela.
- **Trade-offs:** qualquer app em tela cheia pode disparar a troca, e um jogo em janela comum não dispara automaticamente. Em sistema de um monitor ou quando o jogo cobre todos os monitores, o mascote desaparece temporariamente. A viabilidade e o custo precisam passar por P7.
- **Consequências:** Q-09 e P7 foram atualizados; SECURITY.md permite somente a observação transitória e limitada da geometria da janela em primeiro plano. O alvo e a posição anteriores não são gravados como nova preferência durante a troca automática. A Fase 8 implementa o modo apenas depois de P7 aprovado.
- **Fontes primárias:** [SetWinEventHook](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook), [eventos WinEvent](https://learn.microsoft.com/en-us/windows/win32/winauto/event-constants), [GetForegroundWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getforegroundwindow), [GetWindowRect](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect), [MonitorFromWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-monitorfromwindow) e [SHQueryUserNotificationState](https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shqueryusernotificationstate).

## DEC-014 — Inspiração visual e personalidade espuleta ajustável

- **Data:** 2026-09-28
- **Estado da decisão:** ACCEPTED pelo usuário quanto à inspiração e ao controle de energia; a parte "sem copiar elementos reconhecíveis" foi SUPERSEDED por DEC-019 (o usuário quer semelhança com o Luffy)
- **STATUS:** PLANNED (conteúdo, opções e comportamento ainda não implementados)
- **Problema:** o conceito do Buzzy parte de inspiração em Luffy, mas precisa continuar um mascote próprio; o usuário também quer regular o quanto ele fica espuleta enquanto programa ou joga.
- **Decisão:** preservar Luffy, de *One Piece*, como inspiração visual e de personalidade em sentido amplo, traduzindo a ideia para um primata original sem copiar elementos reconhecíveis do personagem ou da obra. O que tomar, o que não usar e o critério de distinção ficam em PRODUCT_SPEC.md, seção Visão (acrescentado em 2026-09-28 porque só havia a lista do que evitar). O temperamento pode ser livre, otimista, curioso, impulsivo, aventureiro e brincalhão, expresso por movimento, expressões e gestos, sem conversa ou texto. O controle terá três posições em português — **Baixa**, **Média** (padrão) e **Alta** — equivalentes a Low/Mid/High. Baixa significa mais pausas e menos ações; Média mantém um ritmo brincalhão equilibrado; Alta aumenta a frequência e a duração das ações e reações não verbais. Dois cliques abrem um painel compacto somente com o controle de energia; as configurações apresentam a mesma preferência. Movimento permanece determinístico e seguro em qualquer nível; arraste, ocultar/pausar e modo de tela cheia sempre prevalecem.
- **Alternativas consideradas:** intensidade fixa, controle contínuo sem significado claro ou sliders separados para movimento e expressões.
- **Motivo:** uma escolha simples permite ajustar o temperamento sem virar um painel técnico nem introduzir IA.
- **Trade-offs:** os três níveis precisam ter diferença observável e testável, mas não podem alterar regras físicas nem permitir comportamento intrusivo.
- **Consequências:** guardar o nível em `settings.json`; oferecer um único controle de três posições no painel aberto por dois cliques e nas configurações, ambos ligados à mesma preferência. As opções de energia são concluídas na Fase 8, sem atrasar o fechamento técnico atual. Fases 2, 4, 6 e 7 usam o mesmo nível, com limites e testes em TODO.md.

## DEC-015 — Execução contínua autorizada pelo usuário

- **Data:** 2026-09-29
- **Estado da decisão:** ACCEPTED pelo usuário
- **STATUS:** PLANNED; a autorização está vigente, mas o trabalho do MVP ainda precisa cumprir seus gates e critérios.
- **Problema:** evitar que planos e aprovações rotineiras interrompam o desenvolvimento já delegado.
- **Decisão:** Claude está autorizado a criar a identidade visual original a partir das referências fornecidas e a conduzir implementação, testes, correções e documentação do MVP em ordem, das fases 1 a 11. A Fase 1 já está autorizada para começar assim que a Etapa 0B fechar os gates técnicos. Não é necessária aprovação rotineira de plano, conceito visual ou avanço entre fases. Codex revisa quando o usuário pedir.
- **Alternativas consideradas:** exigir aprovação a cada plano ou fase, ou limitar a autorização à Fase 1. Não foram escolhidas porque o usuário autorizou a execução contínua.
- **Limites:** valem PRODUCT_SPEC.md, ARCHITECTURE.md, TODO.md e SECURITY.md. Não introduzir funcionalidades proibidas nem distribuir o aplicativo. Testes [HW] indisponíveis ficam pendentes, sem PASS/VERIFIED; Claude continua tarefas independentes e retorna a eles na integração. Não inventar evidência nem declarar uma fase concluída sem seus critérios.
- **Motivo:** o usuário quer que Claude avance com autonomia e dependa pouco dele.
- **Trade-offs:** autonomia exige registrar decisões técnicas materiais e manter evidência rigorosa; o usuário continua necessário para bloqueios que só ele pode resolver.
- **Consequências:** AGENTS.md e prompt_usuario.md orientam a execução; PROJECT_CONTEXT.md, TODO.md e este registro refletem a autorização. Bloqueios realmente exclusivos do usuário continuam podendo ser comunicados, enquanto trabalho independente prossegue.

## DEC-016 — Estrutura de código, testes e integrações Windows da Fase 1

- **Data:** 2026-09-29
- **Estado da decisão:** ACCEPTED por Claude sob a autorização de DEC-015 (decisão técnica de implementação; Codex revisa quando o usuário pedir).
- **STATUS:** PLANNED até o gate da Fase 1 ser registrado em TODO.md.
- **Problema:** a Fase 1 precisava de uma organização de código, de testes sem dependências indesejadas, de bandeja, menu e instância única que não roubem foco e de um portão de APIs que rode em todo build.
- **Decisão:**
  1. **Solução.** `Buzzy.slnx` com `src/Buzzy.Core` (núcleo puro, `net10.0`, sem WPF), `src/Buzzy.App` (WPF; `Plataforma/` é o único adaptador que chama Win32, `Apresentacao/` desenha, `Composicao/` é a raiz de composição), `src/Buzzy.Visual` (renderizador da identidade, DEC-017), testes em `tests/` e ferramentas em `tools/`. SDK fixado em `global.json`; `Buzzy.Build.props` liga avisos como erro, nulabilidade, análise `latest`, build determinístico, arquivo de trava de pacotes e auditoria NuGet.
  2. **Nenhum pacote NuGet.** Os testes usam um executor próprio mínimo (`tests/Buzzy.Testes.Executor`: atributos `[Teste]` e `[Integracao]`, asserções, saída 0 = verde, 1 = falha, 2 = nenhum teste). O MSTest 4.0.2 traz `Microsoft.Testing.Extensions.Telemetry` e `Microsoft.ApplicationInsights` como dependências transitivas, e o projeto proíbe rede e telemetria. `tools/testar.ps1` roda a suíte.
  3. **Menu de contexto nativo** (`TrackPopupMenuEx` com uma janela dona temporária que recebe o primeiro plano e é destruída depois), no lugar do menu WPF previsto em ARCHITECTURE.md 2.13.1. O menu precisa fechar ao clicar fora e devolver o foco quando aberto pela bandeja ou pelo personagem, que não ativa; o menu nativo segue a regra documentada da Shell (dono em primeiro plano e `WM_NULL` depois) e já funciona com teclado e leitor de tela. Os rótulos vêm de `Textos.resx` (Q-12).
  4. **Bandeja** por `Shell_NotifyIcon` versão 4, identificada por janela e número (sem GUID, que prende o ícone ao caminho do executável), recriada em `TaskbarCreated` com o ícone no DPI atual, com `NIM_SETFOCUS` ao fechar o menu sem comando e o formato antigo de notificação como reserva.
  5. **Instância única** por mutex e evento nomeados em `Local\`, com o SID do usuário no nome. A primeira instância espera o evento com `RegisterWaitForSingleObject`, sem polling; a segunda sinaliza e sai. Se os objetos não puderem ser criados, o app sai com código 6.
  6. **Recusa rodar elevado** (SECURITY.md 8, item 5): com token elevado, mostra um aviso e sai com código 5.
  7. **Log de diagnóstico opcional**, só com `--diagnostico`, em `%LOCALAPPDATA%\Buzzy\diagnostico.log`, com limite de 1 MB e uma rotação. Registra apenas eventos do próprio Buzzy; é a fonte dos testes de integração.
  8. **Sprite provisório** gerado em código (128 × 128 DIP, alfa forçado a 0 ou 255). Posição inicial: pés no chão da área útil do principal, a 85% da largura. Mudanças de topologia são agrupadas em 300 ms, valor provisório até P5, com até três releituras se a leitura falhar.
  9. **Portão de APIs** como ferramenta própria (`tools/Buzzy.PortaoApis`), executada por um alvo MSBuild depois de cada build do app: lê as importações nativas e as declarações P/Invoke dos binários, procura as chamadas no código-fonte e confere o manifesto (`asInvoker` e Per-Monitor V2). O `Buzzy.exe` é o apphost genérico do SDK, que só localiza o runtime e entrega a execução ao `Buzzy.dll`. Ele importa quatro funções que coincidem com a lista de SECURITY.md 3.2 (`LoadLibraryExW`, `LoadLibraryA`, `GetProcAddress` e `ShellExecuteW`, esta usada só para abrir a página de download do .NET quando o runtime falta e o usuário aceita). Elas são permitidas **somente no apphost nativo**, por nome exato e com o motivo no relatório; nunca valem para as DLLs. Uma importação nova reprova o build até ser revisada.
  10. **Fim de sessão.** O `System.Windows.Application` expõe só `SessionEnding`, disparado na pergunta `WM_QUERYENDSESSION`; o Buzzy encerra limpo nesse evento. Limitação aceita: se outro aplicativo cancelar o desligamento depois, o Buzzy já terá fechado. A gravação de estado entra com a persistência (Fase 5).
- **Alternativas consideradas:** MSTest ou xUnit (dependências de telemetria ou pacotes extras); menu WPF, que numa janela que não ativa não fecha nem devolve o foco de modo confiável; `FindWindow` com `SetForegroundWindow` para a segunda instância, que exigiria procurar janelas e mexer no primeiro plano; `UseAppHost=false`, que obrigaria abrir o app por `dotnet Buzzy.dll`.
- **Motivo:** menos dependências, menor superfície de segurança e mais comportamento testável sem janela.
- **Trade-offs:** mais código próprio (executor, portão, interop); sem integração com o Test Explorer; a permissão do apphost precisa ser revista quando o SDK mudar.
- **Consequências:** ARCHITECTURE.md 1 e 2.13.1 descrevem o implementado; SECURITY.md 3.2 e 8 registram a exceção do apphost; PROJECT_CONTEXT.md traz os comandos de build e teste.

## DEC-017 — Identidade visual original em fontes vetoriais

- **Data:** 2026-09-29
- **Estado da decisão:** SUPERSEDED por DEC-018 (o usuário não gostou da direção e pediu pixel art fiel às pranchas). Arquivos guardados em `assets/identidade/arquivo-vetorial/`.
- **STATUS:** PLANNED na época; substituída antes de qualquer integração.
- **Problema:** criar a aparência própria do Buzzy a partir das referências e da inspiração de DEC-014, respeitando o clique por alfa de P1 (só alfa 0 deixa o clique passar), a nitidez de 100% a 200% e a troca de arte sem mexer no núcleo (ARCHITECTURE.md 2.10).
- **Decisão:** o Buzzy é um sagui-acrobata violeta-índigo, com rosto pêssego, olhos e ponta da cauda menta, topete de três tufos e cauda em espiral, descrito em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md). A arte é um **boneco de recorte vetorial**: biblioteca de partes num subconjunto de SVG (`assets/identidade/buzzy-partes.svg`) e poses e expressões como árvores de transformações em JSON (`assets/identidade/buzzy-poses.json`), renderizadas pela biblioteca `src/Buzzy.Visual` no DPI de destino com borda dura (alfa 0 ou 255). `tools/Buzzy.Identidade` gera as prévias e confere que toda pose cabe no quadro de 128 × 128 DIP e só usa partes existentes.
- **Alternativas consideradas:** (a) sprites em PNG desenhados por escala, com três conjuntos para manter; (b) pixel art, que combina com a borda dura mas fica pequena ou borrada em 150% e 200%; (c) arte 3D pré-renderizada, como numa das pranchas, difícil de editar e com sombra suave incompatível com a regra de alfa.
- **Motivo:** uma fonte única atende todas as escalas; expressões são camadas independentes do corpo (invariante 6 de ARCHITECTURE.md 2.6); poses novas são dados; as fontes são texto editável e versionável.
- **Trade-offs:** o renderizador faz parte do app, e o custo de rasterizar precisa ser medido na Fase 6, junto com a investigação de animação e memória de P2; o subconjunto de SVG aceito é restrito.
- **Consequências:** a Fase 6 decide, com medição, entre rasterizar cada quadro sob demanda ou manter um cache por DPI. O critério 3 da Fase 6 continua o mesmo, aplicado aos quadros rasterizados a partir das fontes em cada escala suportada, porque não há PNG de produção versionado. As prévias em `assets/identidade/previa/` são geradas, não editadas à mão.

## DEC-018 — Identidade em pixel art fiel às pranchas de referência

- **Data:** 2026-09-29
- **Estado da decisão:** ACCEPTED — pedido do usuário ("não gostei do design do Buzzy, queria que fosse mais fiel às imagens de referência"; "usar essas imagens de referência para você gerar um boneco meio pixel art"). Detalhes técnicos definidos por Claude sob DEC-015.
- **STATUS:** PLANNED; 21 poses, 14 expressões e o ícone da bandeja gerados e conferidos pela ferramenta de prévias; a integração animada é da Fase 6.
- **Problema:** a direção vetorial de DEC-017 se afastava das pranchas (cor, rosto, proporções) e o usuário quer um boneco em pixel art parecido com elas, sem perder o clique por alfa de P1 nem a nitidez nas escalas do Windows.
- **Decisão:** pixel art em quadro de **64 × 64 pixels**, mostrado em **128 × 128 DIP** (1 pixel de arte = 2 DIP), com paleta, proporções, rosto, orelhas, olhos, cauda e poses tirados das pranchas (descrição em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md)). A arte é gerada por código em `src/Buzzy.Visual/Pixel/`: esqueleto posável rasterizado sem meio-tom, contorno de 1 pixel, sombra de 1 pixel e carimbos desenhados à mão para o rosto; a folha nativa fica em `assets/identidade/pixel/buzzy-poses.png`. O chapéu de palha com faixa vermelha entra (DEC-019); o tufo de pelo aparece por baixo dele quando o chapéu salta.
- **Alternativas consideradas:** (a) ajustar a direção vetorial às pranchas, que o usuário não pediu e não resolve o estilo; (b) reduzir as imagens das pranchas direto para 64 pixels, que traria o chapéu, borrões e poses que não existem nelas (ciclo de caminhada, escalada); (c) pixel art desenhada só à mão, quadro a quadro, mais lenta de manter consistente entre 19 poses.
- **Motivo:** o quadro de 64 pixels em 128 DIP dá ampliação inteira em 100%, 150% e 200% (2×, 3× e 4×), sempre nítida; a pixel art já é alfa 0 ou 255 por natureza (regra de P1); o gerador mantém as poses consistentes e permite retocar à mão depois.
- **Trade-offs:** escalas intermediárias (125%, 175%) exigem vizinho mais próximo com pixels desiguais ou arredondar para o passo inteiro mais próximo (decidir nas Fases 6 e 8); detalhe fino limitado à grade de 64 pixels.
- **Consequências:** IDENTIDADE_VISUAL.md foi reescrito; TODO.md (trabalho visual), ARCHITECTURE.md e PROJECT_CONTEXT.md apontam para a pixel art; a Fase 6 usa a folha nativa, com o critério 3 conferido direto nos pixels; o código vetorial de `src/Buzzy.Visual` fica sem uso e pode ser removido na Fase 6.

## DEC-019 — Semelhança intencional com o Luffy: chapéu de palha e personalidade

- **Data:** 2026-09-29
- **Estado da decisão:** ACCEPTED pelo usuário ("quero que seja tipo o Luffy, coloque o chapéu de palha sim"; "eu quero que a personalidade dele seja tipo a do Luffy [...] a ideia central do projeto é essa").
- **STATUS:** PLANNED; o chapéu já está na pixel art e no sprite parado do app; a personalidade é implementada nas Fases 6 e 7.
- **Problema:** PRODUCT_SPEC.md, DEC-002, DEC-014, Q-17 e o prompt mestre tinham virado a inspiração em regra de distância ("não usar o chapéu", "ninguém deve reconhecer o Luffy"), o contrário do que o usuário quer.
- **Decisão:** o Buzzy é um macaquinho com o jeito do Luffy: **chapéu de palha com faixa vermelha**, como nas pranchas, e **personalidade do Luffy** (livre, impulsivo, otimista, aventureiro, de sorriso e risada fáceis, energia inesgotável), expressa sem fala. Outros elementos do personagem não são proibidos e entram se o usuário pedir. Continuam valendo: nome Buzzy, sem fala, chat, texto ou voz (DEC-003), sem IA, uso pessoal sem distribuição (Q-10).
- **Alternativas consideradas:** manter a regra de distância (rejeitada pelo usuário) ou copiar o Luffy inteiro com roupa e cicatriz (não pedido).
- **Motivo:** é a ideia central do projeto, dita pelo usuário.
- **Trade-offs:** *One Piece* é obra e marca de terceiros; para uso pessoal (Q-10) não há impedimento prático, mas qualquer distribuição pública exige rever o chapéu e a semelhança antes.
- **Consequências:** PRODUCT_SPEC.md (Visão) reescrito; DEC-002, DEC-014, DEC-018, Q-17 e Q-23 atualizados; IDENTIDADE_VISUAL.md e o prompt mestre do Codex alinhados; a pixel art ganhou o chapéu, que reage às emoções.

## Decisões de produto registradas pelo usuário

Em 2026-09-26 e 2026-09-27 o usuário respondeu às escolhas abaixo em `docs/DECISOES_DO_USUARIO.md`. Elas estão aceitas como escopo planejado; ainda não significam que qualquer comportamento esteja implementado ou verificado.

| ID | Decisão aceita | Fase/efeito | STATUS |
|---|---|---|---|
| Q-02 | Apoiar Windows 11; Windows 11 24H2 ou posterior é o alvo inicial de teste. Windows 10 não faz parte do alvo. | Define a matriz de aceitação do MVP. | PLANNED |
| Q-03 | Sempre no topo ligado por padrão e desligável; ícone e menu na bandeja; sem botão na barra de tarefas; minimizar esconde e a bandeja restaura; escala em passos fixos; sem opacidade no MVP; botão direito abre o mesmo menu; segunda instância revela a existente. | Fase 1 e configurações da Fase 8. | PLANNED |
| Q-04 | Oferecer iniciar com o Windows, desligado por padrão e ativado apenas pelo usuário. | Fase 8; mecanismo técnico segue a forma de pacote escolhida para cada distribuição. | PLANNED |
| Q-05 | No MVP, superfícies apenas nas bordas das áreas úteis dos monitores; janelas de outros aplicativos ficam fora. Atravessar monitores fica ligado por padrão e pode ser desligado. | Fase 4; janelas de outros aplicativos ficam para depois do MVP. | PLANNED |
| Q-06 | Sem atalhos de teclado para controlar o personagem no MVP. O painel de energia e as configurações são navegáveis por teclado conforme Q-20. | Fase 8. | PLANNED |
| Q-07 | Clique provoca reação não verbal; clique duplo abre o painel compacto somente com o seletor de energia; botão direito abre o menu. | Reações na Fase 7; painel e persistência de energia na Fase 8. | PLANNED |
| Q-08 | Metas de desempenho aceitas: repouso CPU média ≤0,1% de um núcleo em 10 min e p95 ≤1%, sem atividade periódica até 300 ms nem mudança da resolução global do timer; memória em 8 h até 10% acima da leitura de 1 h após aquecimento e sem tendência contínua; animação alvo 60 qps, piso 30 qps, CPU/GPU média ≤5%; arraste p95 ≤16,7 ms; estabilidade 8 h sem falha ou crescimento contínuo; meta de primeiro quadro a definir após haver aplicativo real. | Fases 1, 3, 6, 10 e 11, conforme M1–M7. | PLANNED; metas aceitas, ainda não verificadas no aplicativo |
| Q-09 | Modo de tela cheia ligado por padrão: quando uma janela em tela cheia está em primeiro plano, mover o Buzzy para um monitor livre; se não existir monitor livre, ocultá-lo até a tela cheia terminar. Restaurar a posição anterior depois. Desligável nas configurações. A detecção é por tela e posição da janela ativa, não por identidade do jogo; tela cheia de vídeo e apresentação também pode ativar o modo. | Fase 8, depois de P7 validar detecção, retorno, custo e jogos exclusivos/sem borda. | PLANNED; desenho aceito sob delegação do usuário em 2026-09-28 |
| Q-10 | Uso pessoal, sem plano de divulgar ou distribuir o aplicativo pronto. O código poderá, no máximo, ficar no GitHub. O primeiro pacote próprio/teste será ZIP portátil sem assinatura nem instalador; não preparar releases binários para terceiros sem nova decisão. | Build próprio e testes; visibilidade do repositório e eventual release binário ficam para quando forem necessários. | PLANNED; visibilidade/release futura UNCERTAIN |
| Q-11 | Manter a ordem: Fase 10 verifica o MVP; Fase 11 otimiza e repete a regressão. | Fases 10 e 11. | PLANNED |
| Q-12 | Apenas português do Brasil no MVP; manter textos fora do código. | Rótulos do menu e da bandeja (Fase 1), do painel de energia e das configurações (Fase 8); o personagem não exibe texto. Outros idiomas ficam para depois. | PLANNED |
| Q-14 | Mouse e touchpad no MVP; toque e caneta ficam para depois. | Fases 3 e 4. | PLANNED |
| Q-20 | Painel de energia e configurações navegáveis por teclado e utilizáveis por leitor de tela; janela do personagem não é alvo de leitor de tela. | Fase 8. | PLANNED |
| Q-21 | Não incluir modo fantasma (click-through total) no MVP. | Fase 8; evita deixar o personagem inacessível. | PLANNED |
| Q-23 | O conceito visual e o temperamento se inspiram no Luffy, com semelhança intencional (DEC-019, 2026-09-29): chapéu de palha e personalidade dele, num macaquinho chamado Buzzy. Personalidade não verbal, sem conversa ou chat. Controle de energia com três posições — Baixa/Média/Alta (Low/Mid/High), Média padrão; Baixa é mais tranquila, Média equilibrada e Alta mais ativa. O seletor aparece ao dar dois cliques e nas configurações, ligado à mesma preferência. | Comportamento não verbal nas Fases 2, 4, 6 e 7; painel e persistência na Fase 8. | PLANNED; inspiração e controle pedidos pelo usuário em 2026-09-28 |

### Decisões que continuam pendentes

| ID | Pendência | Como resolver | STATUS |
|---|---|---|---|
| Q-10 (GitHub/release) | Se o repositório será público e se algum dia haverá release binário para outras pessoas. | Não bloqueia o desenvolvimento local. Reabrir antes de tornar o repositório público ou preparar distribuição de aplicativo pronto; assinatura só será analisada se houver release binário. | UNCERTAIN |

### Escolhas já resolvidas

**Q-15 — RESOLVIDA em 2026-09-26 (processo, sem impacto no produto):** o usuário enviou o pedido inicial ao Fable pelo terminal e limpou `prompt_usuario.md` depois de copiar o texto. Não é necessário restaurar aquele pedido. O arquivo será usado como rascunho do próximo prompt e pode ser limpo após o envio; o prompt mestre continua sendo a instrução operacional canônica.

**Q-01 — RESOLVIDA em 2026-09-26:** o usuário delegou ao Codex a escolha da stack e dos próximos passos. DEC-006 escolhe WPF com C# e .NET 10 LTS. P1, P2 e P3 continuam como portões técnicos antes da Fase 1; a aprovação da stack não declara esses testes passados.

**Q-13 — RESOLVIDA em 2026-09-26:** fazer os protótipos descartáveis antes da Fase 1, na ordem P1 (clique por pixel), P3 (arraste sem roubo de foco) e P2 (medição de desempenho), usando WPF. Na data desta decisão, nenhum protótipo havia sido executado; os resultados posteriores estão registrados em TODO.md e DEVELOPMENT_LOG.md.

**Aceites técnicos — 2026-09-27:** o usuário aceitou o parecer PASS de P1, limitado ao ambiente medido, e PASS de P2 como medição de viabilidade; a investigação de animação e memória continua pendente. Também aceitou Q-08 e DEC-007, DEC-008, DEC-010 e DEC-012. Naquele registro, DEC-009 ainda aparecia condicionada a P3/P4. Esse status foi atualizado pela delegação técnica de 2026-09-28 abaixo. Os aceites de P1/P2 são decisões sobre os gates dos protótipos, não validação do aplicativo nem fechamento da Etapa 0B.

**Delegação e decisão técnica — 2026-09-28:** o usuário autorizou o Codex a tomar as decisões que julgar melhores para o andamento, preservando a intenção pessoal do projeto. Naquele momento, DEC-009 previa P3 antes do shell e P4 antes do diálogo local; a clarificação posterior do usuário removeu chat e entrada de texto, aposentando P4. DEC-011 adota ociosidade por eventos. Naquela data, isso não aprovava o resultado de P3 nem autorizava a Fase 1; a autorização contínua posterior está registrada em DEC-015. O usuário não planeja divulgar nem distribuir o aplicativo pronto; visibilidade do código no GitHub e eventual release binário continuam para decisão se/quando forem necessários.

**Clarificação de produto e modo de tela cheia — 2026-09-28:** o usuário esclareceu que quer um mascote interativo, sem IA, para ficar no desktop enquanto programa ou joga; quando um app estiver em tela cheia, o Buzzy deve ir automaticamente para outro monitor. Sob a delegação já dada, o Codex registrou DEC-013: detecção limitada pela janela em primeiro plano e sua geometria, retorno à posição anterior, ocultação quando não houver monitor livre e modo configurável. P7 precisa validar o comportamento antes da Fase 8. Isso não identifica jogos pelo processo nem detecta automaticamente jogo em janela comum.

**Personalidade e liberdade de movimento — 2026-09-28:** o usuário quer o Buzzy circulando pelas telas, subindo bordas e agindo como um macaquinho interativo, com design e temperamento inspirados amplamente em Luffy e energia Baixa/Média/Alta. Sob a delegação, o Codex definiu Média como padrão e limitou o movimento às superfícies aprovadas do desktop, com travessia entre monitores ligada por padrão; arraste, pausa, ocultação e modo de tela cheia prevalecem. A aparência e o conteúdo permanecem originais.

**Seletor de energia e personalidade não verbal — 2026-09-28:** o usuário esclareceu que quer um mascote curioso que percorre os monitores, sem chat nem conversa digitada. Q-07/DEC-014 especificam que dois cliques abrem somente o controle Baixa/Média/Alta; curiosidade e humor aparecem por gestos, expressões e ações. P4, que verificaria uma janela de conversa, deixou de ser aplicável. Os níveis precisam ser visivelmente distintos sem mudar as regras físicas nem interromper o usuário.

**Q-22 — RESOLVIDA em 2026-09-26:** o teto de tempo para P9 não se aplica porque a interface nativa Win32 deixou de ser o caminho escolhido; P9 foi encerrado sem execução.

### Escolhas levantadas pela referência visual

Em 2026-09-26 o usuário esclareceu que as pranchas em `assets/references/` são referências, não requisitos literais. Em 2026-09-29 autorizou Claude a criar a identidade original sem aprovação rotineira. Q-16 a Q-19 não bloqueiam o planejamento; nome e limites de originalidade continuam definidos em PRODUCT_SPEC.md. STATUS: CLARIFIED.

| ID | Escolha | Situação observada | Recomendação | Bloqueia |
|---|---|---|---|---|
| Q-16 | Nome do personagem | Uma prancha usa o título "PIXEL"; o produto está definido como Buzzy. | Referência visual não altera o nome. Manter Buzzy; só mudar com decisão explícita do usuário. | Nenhuma; esclarecida |
| Q-17 | Acessórios e semelhança visual | As pranchas mostram o chapéu de palha com faixa vermelha do Luffy. | Resolvida pelo usuário em 2026-09-29 (DEC-019): o chapéu entra e a semelhança com o Luffy é desejada; não aplicar regra de distância visual, respeitando PRODUCT_SPEC.md; não há aprovação rotineira como gate. | Nenhuma; esclarecida |
| Q-18 | Poses e superfícies | As pranchas mostram poses em superfícies que não estão identificadas. | São referências, não ampliação do escopo. Vale a decisão de superfícies Q-05; não adicionar janelas de outros aplicativos sem decisão explícita. | Nenhuma; esclarecida |
| Q-19 | Estilo e borda do sprite | As pranchas exploram estilos visuais diferentes; o funcionamento do clique transparente continua sujeito ao protótipo P1. | Claude pode propor uma direção visual nova. O requisito técnico de alfa/clique de P1 continua independente da referência estética. | Nenhuma; esclarecida |

As pranchas também mostram uma pose de "pendurado". Em 2026-09-26 ela não existia na máquina de estados; em 2026-09-28 o usuário pediu que o Buzzy fique pendurado brevemente, e o estado `HANGING` entrou em ARCHITECTURE.md 2.6 (Q-05, DEC-014). A pose das pranchas continua sendo referência, não arte aprovada. Os quadros rotulados "interagindo com cursor" não abrem escolha: PRODUCT_SPEC.md já registra que o cursor desenhado na prancha não pertence ao personagem nem ao produto, e a arquitetura já prevê que o Buzzy só lê o mouse nas próprias janelas.

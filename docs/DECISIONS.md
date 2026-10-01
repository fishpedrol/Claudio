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
- **Decisão:** manter documentação especializada, com PROJECT_CONTEXT.md para estado atual, DEVELOPMENT_LOG.md para histórico, ARCHITECTURE.md para arquitetura, DECISIONS.md para escolhas, SECURITY.md para segurança e TODO.md para tarefas. PRODUCT_SPEC.md registra a intenção estável do produto; TODO.md define as fases; [PROMPT_MESTRE_BUZZY.md](PROMPT_MESTRE_BUZZY.md) contém regras estáveis; prompt_usuario.md registra a diretiva de execução atual; PLAN_REVIEW.md descreve a revisão do Codex quando solicitada.
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
- **Consequências:** o mundo do desktop tem testes com topologias de exemplo desde a Fase 1. A matriz S1 a S12 da Fase 5 (TODO.md) usa esse modelo. *Atualização de 2026-09-30 (DEC-030):* a restauração em cascata da partida está implementada no núcleo, e o retângulo do monitor passou a ser usado nela. A chave pelo caminho do dispositivo é do passo P6 da Fase 5. *Atualização de 2026-10-01 (DEC-030):* a chave pelo caminho do dispositivo está implementada no passo P6, como um resumo: o caminho em si nunca é gravado nem registrado. O nome GDI passou a ser só a reserva, quando o caminho não pode ser lido. Com o app aberto, o retângulo também acha o monitor que só trocou de chave (passo P8). A estabilidade da chave continua a conferir no protótipo P5.

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
- **Consequências:** a gravação da posição começa na Fase 5 e o esquema completo na Fase 8. *Atualização de 2026-09-30 (DEC-029):* a gravação atômica, o `.bak` e a cópia de diagnóstico estão implementados no adaptador, ainda sem uso pelo app. O `.bak` também serve à leitura (principal → `.bak` → padrões), o que muda o critério 3 da Fase 8. *Atualização de 2026-10-01 (DEC-029):* desde o passo P7, o app lê o arquivo na partida e grava pela agenda da raiz.

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
  8. **Sprite provisório** gerado em código (128 × 128 DIP, alfa forçado a 0 ou 255). Posição inicial: pés no chão da área útil do principal, a 85% da largura. Mudanças de topologia são agrupadas em 300 ms, valor provisório até P5, com até três releituras se a leitura falhar. *Atualização de 2026-10-01 (DEC-030):* desde o passo P9 da Fase 5, o agrupamento tem teto de 1 s desde a primeira mensagem, as novas tentativas saem em 500 ms, 1 s e 2 s, e cada releitura publicada arma uma conferência tardia do lugar das janelas.
  9. **Portão de APIs** como ferramenta própria (`tools/Buzzy.PortaoApis`), executada por um alvo MSBuild depois de cada build do app: lê as importações nativas e as declarações P/Invoke dos binários, procura as chamadas no código-fonte e confere o manifesto (`asInvoker` e Per-Monitor V2). O `Buzzy.exe` é o apphost genérico do SDK, que só localiza o runtime e entrega a execução ao `Buzzy.dll`. Ele importa quatro funções que coincidem com a lista de SECURITY.md 3.2 (`LoadLibraryExW`, `LoadLibraryA`, `GetProcAddress` e `ShellExecuteW`, esta usada só para abrir a página de download do .NET quando o runtime falta e o usuário aceita). Elas são permitidas **somente no apphost nativo**, por nome exato e com o motivo no relatório; nunca valem para as DLLs. Uma importação nova reprova o build até ser revisada.
  10. **Fim de sessão.** O `System.Windows.Application` expõe só `SessionEnding`, disparado na pergunta `WM_QUERYENDSESSION`; o Buzzy encerra limpo nesse evento. Limitação aceita: se outro aplicativo cancelar o desligamento depois, o Buzzy já terá fechado. A gravação de estado entra com a persistência (Fase 5). *Atualização de 2026-10-01 (DEC-029):* desde o passo P7, o `SESSION_ENDING` grava a posição e as preferências na hora, dentro do próprio `SessionEnding`, antes de o app encerrar.
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

## DEC-020 — Esclarecimentos da máquina de estados depois da auditoria da Fase 2

- **Data:** 2026-09-30
- **Estado da decisão:** ACCEPTED por Claude sob a autorização de DEC-015 (decisão técnica dentro de PRODUCT_SPEC.md e DEC-013; Codex revisa quando o usuário pedir).
- **STATUS:** PLANNED até o gate da Fase 2 ser registrado em TODO.md; as regras estão implementadas em `src/Buzzy.Core/Personagem/Maquina.cs` e cobertas por testes. As que dependem do modo de tela cheia só serão exercitadas pelo app na Fase 8.
- **Problema:** uma auditoria independente do gate da Fase 2, com quatro auditores e verificação cética, achou lacunas na tabela de ARCHITECTURE.md 2.6. Algumas só apareciam em sequências raras, mas violavam a especificação:
  - a posição temporária da tela cheia seria gravada como a escolhida pelo usuário;
  - o retorno temporário sobrava de um episódio de tela cheia para o seguinte;
  - desbloquear a sessão fazia o personagem reaparecer sobre um jogo ainda em tela cheia;
  - um "mostrar" anterior à carga fazia a carga ser ignorada;
  - um nível de energia inválido derrubava o núcleo;
  - a precedência entre motivos de ocultamento não estava escrita.
- **Decisão:**
  1. **Precedência dos motivos de ocultamento:** `POR_USUARIO` > `POR_SESSAO` > `POR_SUSPENSAO` > `POR_TELA_CHEIA`. Em `HIDDEN`, um motivo só substitui outro de precedência menor. Com a sessão bloqueada, suspender e retomar não mostram o personagem.
  2. **Retorno temporário da tela cheia:**
     - fica só em memória;
     - `GravarPosicao` grava sempre o retorno, se houver, e nunca a posição temporária;
     - quando outro motivo substitui `POR_TELA_CHEIA`, o retorno vira a posição, sem reaparecer;
     - se a tela cheia termina com o personagem escondido por outro motivo, a posição de antes volta a valer;
     - um `CMD_SHOW` manual durante o episódio descarta o retorno;
     - um arraste sempre descarta o retorno; clique, clique duplo ou cancelamento em `PRESSED` só o descartam se a tela cheia mudou durante o gesto;
     - desligar o modo (`SETTINGS_CHANGED`) desfaz o efeito temporário. Escondido por outro motivo, a posição de antes volta a valer sem reaparecer. Em `PRESSED`/`DRAGGING`, o fim do gesto decide: um arraste escolhe a posição; um clique ou cancelamento leva de volta à posição de antes. Com o modo desligado, o retorno não sobra fora de um gesto. Esta regra foi achada pelos testes de cobertura do gate, depois da auditoria.
  3. **Reaparecer por evento do sistema** (`SESSION_UNLOCKED`, `RESUMED`) e a carga reaplicam o modo de tela cheia contra os monitores ocupados em cache. `CMD_SHOW` é escolha do usuário e não o reaplica.
  4. **Carga:** só o primeiro `Loaded` vale. Pedidos anteriores a ele ficam guardados, e o personagem só aparece com a carga.
  5. **`CLICK` depois de mudança de topologia:** se o monitor do personagem mudou ou sumiu com o botão pressionado, a posição é validada já no `CLICK`, sem sair da reação.
  6. **Intervalo de acomodação** em todo agendamento autônomo, em qualquer estado que decide.
  7. **Energia:** um nível fora de `BAIXA`/`MEDIA`/`ALTA` é ignorado quando vem do painel e vira `MEDIA` na carga e nas configurações (SECURITY.md 7).
  8. **Na raiz de composição:**
     - o laço modal do menu roda depois do processamento do núcleo, e não dentro dele;
     - o relógio de passo fixo recupera no máximo 250 ms de atraso de uma vez;
     - depois de reler a topologia, a janela volta ao lugar do núcleo se tiver sido movida por fora;
     - o relógio e a agenda passam a ser registrados no log de diagnóstico.
- **Alternativas consideradas:**
  - manter o comportamento anterior e só documentar, o que deixaria violações reais do invariante 14 e da regra de não persistir a posição temporária (SECURITY.md 5);
  - manter "oculto por tela cheia" como condição separada do motivo, mais estado para pouco ganho;
  - validar a posição sempre no `CLICK`, que mudaria a física no meio de um pulo a partir da Fase 4.
- **Motivo:** a especificação já dava a intenção: ação manual prevalece, a posição temporária fica só em memória e evento do sistema não desfaz ação do usuário nem o modo de tela cheia. Faltavam as regras para as combinações.
- **Trade-offs:** mais regras na tabela e mais estado (`Carregado`, `TelaCheiaMudouNoGesto`); o campo `TelaCheiaAdiada`, nunca usado, saiu.
- **Consequências:** ARCHITECTURE.md 2.6 atualizado (linhas da tabela, precedência e invariantes 16 e 17); testes de transição, de tela cheia e de propriedade ampliados; as referências gravadas não mudaram.

## DEC-021 — Arbitragem de input e captura do mouse (Fase 3)

- **Data:** 2026-09-30
- **Estado da decisão:** ACCEPTED por Claude sob a autorização de DEC-015 (decisão técnica de implementação de DEC-009 e ARCHITECTURE.md 2.7).
- **STATUS:** PLANNED até o gate da Fase 3 ser registrado em TODO.md.
- **Problema:** implementar clique, clique duplo, arraste e menu sem roubar o foco, sem hook global e sem deixar o personagem preso ao cursor, de forma testável sem janela.
- **Decisão:**
  1. **Árbitro puro** em `src/Buzzy.Core/Entrada/ArbitroDeGestos.cs`: recebe eventos de ponteiro com tempo e métricas e devolve gestos do núcleo e se a captura continua. Regras:
     - limiar `SM_CXDRAG`/`SM_CYDRAG` "de cada lado", sem limite de tempo;
     - clique duplo pela regra do sistema, medido entre os dois pressionar, com a metade inteira do retângulo de clique duplo;
     - soltar rápido fora do limiar vira arraste;
     - `DRAG_CANCEL` quando a captura se perde, quando chega um novo pressionar sem soltar ou quando há movimento sem `MK_LBUTTON`;
     - o botão direito abre o menu só fora de um gesto.
  2. **Adaptador** na janela do personagem:
     - converte as coordenadas das mensagens com sinal para a tela (`ClientToScreen`);
     - lê as métricas pelo DPI atual da janela (`GetSystemMetricsForDpi`, `GetDoubleClickTime`);
     - chama `SetCapture` no primeiro botão pressionado e `ReleaseCapture` no fim do gesto, com uma marca que evita tomar a própria liberação por captura perdida;
     - trata `WM_CAPTURECHANGED` como ponto único de interrupção, e `WM_CANCELMODE` também solta a captura;
     - não registra a nova dona da captura, que pode ser de outro aplicativo (SECURITY.md 6).
  3. **Raiz de composição:** aplica cada movimento de arraste no mesmo tratamento da mensagem e mede a latência M5, com resumo `ARRASTE` no log de diagnóstico. Durante o arraste, não grava cada posição; registra só a posição validada ao soltar.
  4. **Verificação:**
     - `Buzzy.Verificacao --fase 3` exercita os critérios com input SINTÉTICO;
     - o ClickLock só é exercitado se o usuário já o tiver ligado, e a ferramenta nunca altera configurações globais (AGENTS.md);
     - a janela de UAC depende de um pedido de elevação real e fica para verificação manual.
- **Alternativas consideradas:**
  - arraste pelo laço modal do Windows (`DragMove`/`HTCAPTION`), descartado em DEC-009;
  - `GetKeyState` para conferir o botão, proibido pelo portão de APIs;
  - limiar só por tempo, que prejudica o ClickLock;
  - métricas pelo DPI do sistema em vez do DPI da janela, que erraria em escalas mistas;
  - ligar o ClickLock "em memória" para o teste, como fez o protótipo P3, recusado porque altera configuração global.
- **Motivo:** mantém a regra do Windows, a prioridade do usuário (DEC-004) e a segurança (nenhuma leitura de input fora do gesto), e deixa quase toda a lógica testável sem janela.
- **Trade-offs:**
  - a captura de uma janela que não está em primeiro plano só recebe o mouse com um botão pressionado, o que basta para o gesto;
  - a latência medida vem do próprio app e não inclui o compositor;
  - UAC, ClickLock ligado e escalas mistas dependem de verificação manual ou de hardware.
- **Consequências:** ARCHITECTURE.md 2.7 registra as regras concretas; SECURITY.md registra as APIs novas no adaptador; TODO.md registra as evidências e as pendências da Fase 3.

## DEC-022 — Movimento, superfícies e relógio de quadros (Fase 4)

- **Data:** 2026-09-30
- **Estado da decisão:** ACCEPTED por Claude sob a autorização de DEC-015 (decisão técnica dentro de ARCHITECTURE.md 2.5, 2.6 e 2.9 e de Q-05).
- **STATUS:** PLANNED até o gate da Fase 4 ser registrado em TODO.md.
- **Problema:** dar ao macaquinho liberdade para andar, escalar, pendurar-se, pular e cair num monitor, de forma determinística. Nenhuma janela de outro aplicativo pode servir de superfície, não pode haver timer em repouso e o movimento precisa ser suave na tela.
- **Decisão:**
  1. **Física no núcleo puro:**
     - o passo fixo de 1/60 s do `TICK` move o personagem em `WALKING`, `CLIMBING`, `HANGING`, `JUMPING` e `FALLING`;
     - a posição fina da âncora (pixels físicos, com fração) e a velocidade ficam no estado do núcleo; a janela usa a posição arredondada;
     - integração semi-implícita: velocidade primeiro, depois posição;
     - velocidades em DIPs por segundo, convertidas pela escala do monitor da âncora (ARCHITECTURE.md 2.9): caminhada 90, escalada 110, pendurado 80;
     - gravidade de 2200 DIP/s² e queda máxima de 1500 DIP/s;
     - os mesmos valores valem em todos os níveis de energia (invariante 12).
  2. **Superfícies (Q-05, ARCHITECTURE.md 2.5):** são as do monitor da âncora, com o sprite inteiro na área útil:
     - chão: a borda de baixo da área útil;
     - paredes: as laterais em que nenhum outro monitor encosta com sobreposição vertical;
     - borda superior: onde ele se pendura.

     Uma lateral encostada em outro monitor é passagem. A travessia é da Fase 5. *Substituído em parte pela DEC-023:* em vez de dar meia-volta na passagem, ele a trata como parede e pode escalá-la.
  3. **Ações e planos, com semente:**
     - andar: direção sorteada e distância do perfil de energia; se não houver espaço à frente, vira;
     - escalar: se já está numa parede, sobe; senão, anda até a parede escalável mais próxima e sobe. Com a DEC-023, as duas laterais são escaláveis;
     - no topo, pendura-se e segue pela borda para dentro. No fim da borda, desce pela parede (se houver parede ali), volta pela borda ou se solta;
     - pular: arco balístico com distância e altura do perfil de energia, calculado para pousar no chão;
     - na parede, a agenda salta para longe dela ou se solta. Pendurado, continua pela borda, desce (só na quina), salta ou se solta;
     - cair: gravidade até o chão; depois pousa e volta a `IDLE`.

     O tempo na parede e o tempo pendurado ("por pouco tempo") são faixas do perfil de energia.
  4. **Autonomia pausada ou painel aberto:** nada autônomo começa, e o movimento em curso termina com o personagem parado e com os pés no chão:
     - a caminhada para na hora;
     - a escalada desce até o chão;
     - quem está pendurado se solta;
     - pulo, queda e pouso terminam.

     *Atualização de 2026-10-01 (DEC-028), a "regra da calma":* quem está agarrado à parede ou ao cipó sem ter sido posto lá pelo usuário (depois da reação a um clique, de uma revalidação, do fim do uso de um item ou do estado atento) também não fica esperando: com a autonomia pausada ou o painel aberto, deixa de estar agarrado e desce pela parede ou se solta do cipó. A regra roda no fim de todo evento, e por isso vale também para um agarre que acontece já pausado. Ficam onde estão o preso pelo usuário (DEC-024) e quem está atento a um item na mão do usuário, até o item sair da mão. Isso mudou um caminho que já existia na Fase 4 e vale no app sem depender da chave do tamagotchi, que naquele momento ainda estava desligada: agarrado sem estar preso depois de um clique e pausado, ele agora desce. As referências 01–05 não mudaram, porque rodam sem a física.
  5. **Energia (critério 6):** Baixa, Média e Alta mudam pesos, intervalos, distâncias, alturas e os tempos na parede e pendurado; a física é a mesma. Valores iniciais:

     | Faixa | Baixa | Média | Alta |
     |---|---|---|---|
     | Caminhada (DIP) | 80–250 | 150–500 | 250–900 |
     | Distância do pulo (DIP) | 60–120 | 80–200 | 120–320 |
     | Altura do pulo (DIP) | 30–60 | 50–100 | 70–150 |
     | Tempo na parede (s) | 10–20 | 15–35 | 20–45 |
     | Tempo pendurado (s) | 2–5 | 3–8 | 5–12 |

     O tempo na parede cobre a subida inteira de um monitor de 1080 px a 100%, cerca de 8 s a 110 DIP/s. Na primeira calibração, com escalada a 70 DIP/s, a verificação de tela registrou um disparo aos 12 s que cortou uma subida de 12,9 s.
  6. **Relógio de quadros no app:**
     - enquanto o núcleo pede o relógio, a raiz se inscreve em `CompositionTarget.Rendering` (quadros do compositor do WPF);
     - a cada quadro, aplica num lote os passos fixos acumulados e move a janela uma vez, para a posição do último passo;
     - sem movimento, não há inscrição nem quadros (DEC-011);
     - o acumulador recupera no máximo 250 ms, e o descarte vai para o log.
  7. **Poses provisórias por estado:**
     - a apresentação escolhe a pose da pixel art pelo retrato: andando em ciclo, escalando, pendurado, impulso ou no ar, caindo, pousando, sentado ou dormindo, segurado, reagindo e os gestos;
     - a pose é espelhada para a esquerda e renderizada uma vez por pose, espelho, expressão e DPI;
     - o tamanho da janela e a âncora (centro da base) são os mesmos em todas as poses.
  8. **Menu e linha de comando:**
     - o menu ganha "Pausar movimento" e "Retomar movimento" (`CMD_PAUSE_AUTONOMY`/`CMD_RESUME_AUTONOMY`);
     - `--pausado` começa pausado (usado pelas verificações de tela e pelos testes de gesto);
     - `--semente N` fixa a semente da agenda (diagnóstico e testes).
- **Alternativas consideradas:**
  - física no app, com animações do WPF, que tiraria o determinismo e os testes sem janela;
  - `DispatcherTimer` a 60 Hz, que entregou cerca de 39 qps em P2 e não acompanha a taxa do monitor;
  - uma thread com `DwmFlush` a cada quadro: mais precisa, porém com mais código e uma thread própria. Fica como alternativa se o compositor do WPF não bastar na medição;
  - interpolação entre passos para monitores de 120 Hz ou mais, adiada para as Fases 6 e 11, com medição;
  - poses só na Fase 6, o que deixaria o movimento da Fase 4 sem leitura visual.
- **Motivo:** o núcleo continua sendo a única fonte de comportamento; o app só executa efeitos e desenha.
- **Trade-offs:** o movimento fica limitado ao passo físico de 60 Hz. Em monitores de 120 Hz ou mais, o critério 5 da Fase 4 (um avanço a cada quadro apresentado) exige interpolação ou passo menor, a medir nas Fases 6 e 11.
- **Consequências:** ARCHITECTURE.md 2.5, 2.9 e 2.13.4 descrevem o implementado; TODO.md registra as evidências e as pendências da Fase 4.

## DEC-023 — Toon force: física de desenho animado, de borracha como o Luffy

- **Data:** 2026-09-30
- **Estado da decisão:** pedido do usuário, com a interpretação técnica de Claude sob DEC-015 e DEC-019. O usuário pode ajustar a interpretação a qualquer momento.
- **STATUS:** PLANNED até o gate da Fase 4 ser registrado em TODO.md.
- **Pedido do usuário (2026-09-30):** "quero que o bixinho suba pelas laterais do monitor também, tenha tipo toon force".
- **Problema:**
  - Na DEC-022, uma lateral encostada em outro monitor era passagem, e o personagem dava meia-volta nela. Na máquina do usuário, com dois monitores lado a lado, só uma lateral de cada monitor era escalável.
  - "Toon force" é a física de desenho animado: o personagem dobra as regras físicas de um jeito cômico. É a marca do Gear 5 do Luffy, que DEC-019 já tomou como inspiração de personalidade.
- **Decisão:**
  1. **Toda lateral da área útil é escalável**, inclusive a que encosta em outro monitor: a borda da tela vira parede para o macaquinho. Na Fase 5, uma passagem poderá ser atravessada ou escalada; a escolha fica com a agenda.
  2. **Quique de borracha:** uma queda ou um pulo que toca o chão a pelo menos 600 DIP/s quica rindo, em vez de pousar.
     - devolve metade da velocidade para cima e perde 30% da horizontal;
     - no máximo dois quiques seguidos, depois pousa;
     - com a autonomia pausada ou o painel aberto, pousa sem quicar.
  3. **Foguete de borracha:** parte das subidas a partir do chão dispara parede acima a 1000 DIP/s até a borda superior, onde ele se pendura.
     - chance por nível de energia: Baixa 10%, Média 30%, Alta 50%. É frequência de uma ação; a velocidade é a mesma em todos os níveis (invariante 12);
     - pausado, o foguete apaga e ele desce como numa escalada normal.
  4. **Esticar e achatar nas poses provisórias:**
     - o impacto (pouso e começo do quique) aparece achatado: 1,3 × mais largo e 0,7 × mais alto;
     - a velocidade (foguete, subida rápida do quique e queda rápida, a partir de 700 DIP/s) aparece esticada: 0,8 × mais largo e até 1,25 × mais alto;
     - a deformação é da pixel art, por vizinho mais próximo, com os pés na mesma linha e uma margem de 3 pixels até as bordas do quadro;
     - a janela e a âncora não mudam, e o alfa continua só 0 ou 255;
     - poses que já ocupam a altura do quadro só afinam.
  5. **Limites preservados:**
     - o usuário prevalece: pressionar segura na hora, inclusive no quique e no foguete;
     - fora de `JUMPING` e `FALLING` sempre há apoio (chão, lateral ou borda superior), e o sprite fica na área útil;
     - a física é a mesma em todos os níveis de energia;
     - tudo é determinístico pela semente;
     - não há relógio em repouso.
- **Fica para as Fases 6 e 7:** membros de borracha que esticam além do quadro de 128 DIP (exige janela maior ou camada própria), inflar como balão, olhos saltando, rodopios e "flutuar um instante antes de cair". Nenhum desses entra sem medir o custo de desenho (Q-08).
- **Alternativas consideradas:**
  - manter a lateral encostada como passagem só para a Fase 5, que contraria o pedido;
  - deformar o bitmap no WPF (`ScaleTransform`), que borraria a pixel art e mudaria o teste de clique por alfa;
  - desenhar poses novas achatadas e esticadas à mão: melhor resultado final, mas é trabalho da Fase 6.
- **Motivo:** atende ao pedido com mudanças pequenas na física, testáveis sem janela, sem mudar a tabela de estados além de uma linha (quique) e sem relaxar nenhum invariante.
- **Trade-offs:**
  - a deformação por vizinho mais próximo repete ou pula pixels e fica menos limpa que um desenho à mão; aparece só por instantes;
  - o esticar vertical é pequeno nas poses altas.
- **Consequências:**
  - ARCHITECTURE.md 2.5, 2.6 (linha do quique), 2.9 e 2.10 foram atualizados;
  - TODO.md inclui a toon force na Fase 4;
  - os testes de movimento e de pose cobrem quique, foguete, laterais e deformação.

## DEC-024 — Cipó na borda de cima e agarrar onde o usuário solta

- **Data:** 2026-09-30
- **Estado da decisão:** pedidos do usuário, com a interpretação técnica de Claude sob DEC-015.
- **STATUS:** PLANNED até o gate da Fase 4 ser registrado em TODO.md.
- **Pedidos do usuário (2026-09-30):**
  - "e se a animação fosse ele escalando com um cipó? pelo menos quando estiver na borda de cima";
  - "quando eu arrastar ele pra cima ele ficar no cipó e tal, também quero que quando eu arraste ele pras laterais ele fique preso lá";
  - "não literalmente preso, mas só sai de lá quando eu tirar".
- **Decisão:**
  1. **Cipó:** na borda de cima, o personagem fica pendurado num cipó que desce da borda da tela até a mão, em vez de se segurar direto na borda. Andando pela borda, balança em três quadros (esquerda, meio, direita).
     - É a única pose que encosta numa borda do quadro, a de cima, onde o cipó se prende na tela.
     - A paleta ganha verdes de mata para o cipó e as folhas.
  2. **Agarrar onde é solto:** sem apoio, a acomodação (`SETTLING`) agarra em vez de cair. Se as duas coisas valem, fica com a mais próxima, em proporção ao alcance de cada uma. Perto do chão (menos de 32 DIP), cai como antes.
     - Com o topo do sprite a até 96 DIP da borda de cima, agarra o cipó (`HANGING`).
     - Com a âncora a até 64 DIP de uma lateral, gruda na parede, olhando para ela (`CLIMBING`).
  3. **Agarrado:** parado, o relógio fica desligado e a agenda decide depois.
  4. **Preso pelo usuário** (`PresoPeloUsuario`): quando é o usuário que o solta ali, ele só sai quando o usuário o tira.
     - A agenda nunca o faz saltar, se soltar ou descer ao chão.
     - Ou ele fica, trocando de cara, ou passeia de 40 a 220 DIP pela mesma superfície: sobe ou desce pela parede sem chegar ao chão, ou vai e volta pelo cipó, dando meia-volta nas quinas.
     - Um clique não o tira de lá, e pausado ele fica parado.
     - Arrastado para outro lugar, a acomodação decide de novo.
  5. **Agarrado sem ter sido posto pelo usuário** (por exemplo, a reação a um clique no meio de uma escalada): volta a escalar, salta ou se solta, como na Fase 4. Antes ele caía depois da reação.

     *Atualização de 2026-10-01 (DEC-028):* com a autonomia pausada ou o painel aberto, o agarrado sem estar preso não espera a agenda: desce pela parede ou se solta do cipó (regra da calma, DEC-022, item 4). O preso continua onde está. No fim do uso de um item na parede ou no cipó, ele volta agarrado ao mesmo apoio, mesmo a menos de 32 DIP do chão, e só fica preso se já estava.
- **Motivo:** o macaquinho fica onde o usuário o põe, como pedido, sem custo parado e sem nenhum invariante de apoio relaxado. A lateral, o cipó e o chão são todos apoio.
- **Trade-offs:** o preso só é lembrado durante a execução. A persistência da Fase 5 grava a posição, não a condição de preso. *Atualização de 2026-09-30 (DEC-029):* a marca de preso passa a ser gravada no passo P7 da Fase 5; depois de reabrir, ele continua preso onde o usuário o deixou. *Atualização de 2026-10-01 (DEC-029 e DEC-030):* feito no passo P7, no esquema v3, e coberto por testes automatizados e de integração. Com o app aberto, uma mudança de topologia que só translada o monitor dele, como a troca de principal, o deixa preso onde está, sem passar pela acomodação (passo P8).
- **Consequências:** ARCHITECTURE 2.6 (linhas de `SETTLING`, `CLIMBING` e `HANGING`), 2.9 e 2.10; TODO Fase 4 (critério 8); testes `AgarrarTestes` e a verificação de tela.

## DEC-025 — Esconderijo pelo clique duplo; painel de energia pelo menu

- **Data:** 2026-09-30
- **Estado da decisão:** pedido do usuário, com a interpretação técnica de Claude sob DEC-015. **Muda uma decisão anterior do usuário:** o clique duplo não abre mais o painel de energia (Q-23, DEC-014).
- **STATUS:** PLANNED até o gate da Fase 4 ser registrado em TODO.md.
- **Pedido do usuário (2026-09-30):** "quero que crie um modo dele escondido, que ele fica só com a cabecinha e as mãos para fora escondendo seu corpo, tanto na barra de tarefas quanto nas laterais" e "quero que esse modo seja ativado com 2 cliques nele".
- **Decisão:**
  1. **Estado novo `PEEKING`:** o personagem escondido atrás da borda de baixo da área útil (a barra de tarefas, com a barra embaixo) ou de uma lateral, só com o chapéu, a cabeça e as mãos, que seguram a borda.
     - A janela fica inteira na área útil, com a borda da pose na borda da tela.
     - Na lateral, a pose é a de baixo girada 90°, sem perda na pixel art.
  2. **O clique duplo alterna o modo:**
     - esconde atrás da lateral mais próxima, se o personagem está no alto e junto dela (na parede, por exemplo); senão, atrás da borda de baixo, no mesmo x;
     - escondido, outro clique duplo o tira de lá: na borda de baixo, fica de pé no chão; na lateral, volta a grudar na parede, preso pelo usuário (DEC-024).
  3. **Escondido, só o usuário o tira:**
     - a agenda só troca a cara (curioso, travesso, feliz, surpreso, pensativo, rindo), e não há relógio;
     - um clique simples faz a cabeça reagir, e ele continua escondido;
     - pressionar mostra a cara de surpresa sem o corpo aparecer;
     - arrastar o tira do esconderijo;
     - esconder e mostrar pela bandeja, mudar a topologia ou transferir pela tela cheia o devolvem ao esconderijo na mesma borda.
  4. **Painel de energia (Fase 8):** abre pelo menu de contexto ("Energia…"), já previsto na tabela de ARCHITECTURE 2.6. O núcleo guarda a regra antiga atrás de `EsconderijoNoCliqueDuplo = false`, para os testes das Fases 2 e 8.
  5. **Configuração do app:** fica numa fonte única, `ConfiguracaoDoNucleo.DoAplicativo`, usada pelo app e pelas simulações que escolhem sementes nos testes, que antes tinham cópias da configuração que podiam divergir do app.
- **Motivo:** o usuário pediu o gesto e o modo; o painel continua acessível pelo menu, que funciona por teclado e leitor de tela.
- **Trade-offs:** um terceiro clique rápido não conta como outro clique duplo, como no Windows. *Atualização de 2026-09-30 (DEC-029):* o esconderijo passa a ser gravado no passo P7 da Fase 5; ao reabrir, ele volta escondido no mesmo lado. *Atualização de 2026-10-01 (DEC-029 e DEC-030):* feito no passo P7, no esquema v3, e coberto por testes automatizados e de integração. A borda gravada só volta com o esconderijo pelo clique duplo ligado na configuração do núcleo, como no aplicativo. Com o app aberto, uma mudança de topologia que só translada o monitor dele o deixa escondido onde está, sem passar pela acomodação (passo P8).
- **Consequências:**
  - ARCHITECTURE 2.6 (estado `PEEKING` e linhas do clique duplo) e 2.10;
  - PRODUCT_SPEC (controle);
  - TODO: Fase 4 (critério 8) e Fase 8, critérios 10 e 11: o painel vem pelo menu;
  - testes `EsconderijoTestes`, `PoseTestes`, integração e verificação de tela.

## DEC-026 — Curiosidade: ir ver o que o usuário está fazendo

- **Data:** 2026-09-30
- **Estado da decisão:** pedido do usuário. Desenho de Claude sob DEC-015, a implementar depois da Fase 5.
- **STATUS:** PLANNED.
- **Pedido do usuário (2026-09-30):** "quero também que o mascote seja curioso: se eu estou fazendo alguma tarefa por muito tempo ele fica perto olhando com uma cara curiosa e tal; caso eu fique por 30 segundos em uma tela específica ele muda de tela para ver o que estou fazendo".
- **Decisão (desenho):**
  1. **O que ele observa:** só em que monitor está a janela ativa, o retângulo dela e há quanto tempo ela está em primeiro plano. Usa os eventos do Windows limitados à janela ativa, o mesmo mecanismo e a mesma privacidade de DEC-013 (modo de tela cheia, protótipo P7).
     - Nunca título, conteúdo, nome ou caminho de processo, pixels, teclado ou mouse fora da janela dele.
     - Nada é gravado nem registrado; o dado é transitório.
  2. **Mesma janela ativa por bastante tempo:** ele se aproxima dela pelo chão ou pela borda (sem cobrir o meio) e fica olhando com a cara `Curioso`, com o gesto de espiar de vez em quando.
  3. **Janela ativa há 30 s num monitor onde ele não está:** ele muda de monitor para ver. Anda e atravessa pelas passagens da Fase 5; a toon force pode abreviar o caminho.
  4. **Nunca age** pausado, preso pelo usuário (DEC-024), escondido (DEC-025), sendo arrastado, escondido pela bandeja ou num monitor em tela cheia. O usuário sempre prevalece.
  5. **Custo:** por eventos, sem polling. P7 mede o custo com o usuário digitando e mexendo o mouse, como DEC-013 já exige.
- **Dependências:** Fase 5 (travessia entre monitores) e P7 (observador da janela ativa). A curiosidade entra depois da Fase 5, junto com o observador e o modo de tela cheia, que antecipam parte das Fases 7 e 8.
- **Consequências:** SECURITY 3.1 passa a citar a curiosidade como segundo uso, igualmente limitado, da geometria da janela ativa; TODO Fases 7 e 8.

## DEC-027 — Emoção dominante escolhida pelo menu

- **Data:** 2026-09-30; detalhada em 2026-10-01, com o núcleo e o menu implementados.
- **Estado da decisão:** pedido do usuário, com o desenho técnico de Claude sob DEC-015.
- **STATUS:** PLANNED. O núcleo e o esquema v2 (passo T1 da seção "Interação" de [TODO.md](TODO.md)) e o menu com os rostos (passo T2) estão implementados e verificados por testes automatizados, por integração com mensagens postadas às janelas do próprio Buzzy e, em 2026-10-01, pela verificação de tela com input SINTÉTICO (V1, V2 e o foco de volta ao aplicativo em uso depois dos menus): o usuário já escolhe a emoção pelo menu. Desde o passo P7 da Fase 5 (2026-10-01), a escolha é gravada e volta ao reabrir o app, coberta por testes automatizados e de integração (DEC-029). Continua sem VERIFIED pelas conferências [MANUAL] do menu e pela V16 da verificação de tela, escrita no passo P7 e ainda não executada.
- **Pedido do usuário (2026-09-30):** "crie uma opção quando clicar com o botão direito para escolher a emoção dominante pra ele, use as expressões png pra criar essas opções".
- **Problema:**
  - dar ao personagem um humor de base escolhido pelo usuário, sem mudar física, apoio, ações nem a prioridade do usuário;
  - a troca de cara da agenda sorteava pelo tamanho do enum `Expressao`: acrescentar as caras de efeito da DEC-028 mudaria os sorteios e as reproduções gravadas.
- **Decisão** (regras exatas em ARCHITECTURE.md 2.6, 2.12 e 2.16):
  1. **Opções:** as 14 caras de humor de `expressoes.png`, na ordem dela, que é a do enum (`Expressoes.DeHumor`), mais "Automática", o comportamento de sempre. É uma lista fechada: as caras de efeito da DEC-028 nunca são a dominante.
  2. **Comando `CMD_SET_DOMINANT_EMOTION`**, do usuário:
     - é ignorado antes da carga, com uma cara fora das 14 ou igual à escolha atual;
     - senão, grava `Preferencias.EmocaoDominante`, emite `GravarPreferencias` e registra uma transição para o mesmo estado, que só marca a escolha;
     - a cara muda na hora, menos em `RESTING`, `REACTING` e `USING`, que mantêm a própria cara até acabar, e com a onda de um item, que tem precedência (DEC-028);
     - "Automática" mantém a cara atual até a próxima troca.
  3. **Sorteio com a dominante:** um único sorteio ponderado, com peso 6 para ela e 1 para cada uma de quatro companheiras fixas (tabela em ARCHITECTURE.md 2.16). Vale nas trocas de cara parado, escondido (`PEEKING`) e preso pelo usuário. A dominante sai em 60% dos sorteios. Como o sorteio pode repetir a cara atual, cerca de 36% das trocas sorteiam de novo a dominante que já está na tela, e uns 4% repetem uma companheira: perto de 40% das trocas não mudam a cara. É assim que ela fica a mais frequente.
  4. **Sorteio automático pela lista fixa:** na automática, a troca de cara sorteia entre as outras 13 caras de `DeHumor`, com o mesmo número de sorteios de antes. As referências 01–05 continuaram idênticas byte a byte.
  5. **Cara de base:** a da fase da onda de um item, se houver (DEC-028); senão, a dominante; senão, a neutra. Ele volta a ela no fim da reação ao clique, do pouso e do uso de um item, e ao acordar. Na automática e sem onda, tudo continua como antes.
  6. **Carga e configurações:** o `Loaded` com a dominante já começa com a cara dela. `SETTINGS_CHANGED` com outra emoção troca a cara na hora, sem gravar. Uma emoção fora das 14 vira automática.
  7. **Só a cara muda:** cada sorteio de cara continua gastando exatamente um número do gerador. Com a mesma semente e os mesmos eventos, ligar a dominante muda só as expressões (invariante 27).
  8. **Esquema v2 do `settings.json`:** `preferencias.emocaoDominante` é sempre escrito, como `"automatica"` ou o nome da cara em minúsculas. A leitura usa uma lista fechada (SECURITY.md 7). Um arquivo v1, sem o campo, é lido sem migração e sem aviso, e a versão futura dos testes passou a 3.
  9. **Ícone de cada opção:** o recorte exato de 40 × 32 pixels de arte em (12, 0) do quadro parado com a cara, a mesma célula de `expressoes.png`. Uma função só faz o recorte para a prévia e para o menu (`IconesDoMenu.Rosto`, por `Tela.Recortada`), e o ícone é ampliado por um fator inteiro do DPI, por vizinho mais próximo ([IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md)).
  10. **Reproduções gravadas:** `emocao=` aparece no retrato e nas preferências só quando há escolha, e o comando se grava como `CmdSetDominantEmotion emocao=Feliz` ou `emocao=Automatica`. A leitura aceita só o nome exato, ou o número de um valor fora do enum (para os testes de saneamento), nunca listas.
  11. **Menu (passo T2; ordem, ids e teclas em ARCHITECTURE.md 2.16):** o submenu "Emoção dominante" entra no menu de sempre, aberto pelo personagem, por um item ou pela bandeja, entre "Pausar movimento" e "Sair". Tem "Automática", um separador e as 14 caras, todas como opções de rádio, com a marca na escolha atual e uma tecla de acesso diferente para cada uma.
      - Cada cara leva o rosto do item 9 como ícone, ampliado pelo fator inteiro do DPI do monitor em que o menu abre, porque o Windows não amplia o bitmap de um item de menu. Em alto contraste, o menu fica só com texto.
      - Os bitmaps são criados na memória do próprio Buzzy a cada abertura e apagados depois de o menu ser destruído, mesmo com erro. Uma falha do Windows deixa a opção sem ícone e vai para o log só com o código.
      - O submenu continua habilitado com o Buzzy escondido: o núcleo grava a escolha, e a cara aparece quando ele voltar.
      - O id que o Windows devolve vira a emoção por uma lista fixa, nunca por conversão do número; qualquer outro id não escolhe nada.
- **Alternativas consideradas:**
  - a emoção mudar os pesos das ações: o pedido fala em cara de base e na mais frequente; mudar o comportamento exige um pedido novo do usuário;
  - 19 opções, com as caras de efeito e sem dormindo e bocejando: contraria o "use as expressões png" e a lista fechada;
  - um sorteio com peso 6 contra 2 que nunca repete a cara atual: ficou o de cima, com a dominante na tela em 60% das trocas;
  - manter o esquema na v1, com o campo só quando há escolha: um build antigo apagaria o campo ao gravar, e a versão descreve o formato do arquivo, não quem o grava;
  - um ícone de 32 × 32 com o contorno refeito: não seria a célula de `expressoes.png` que o usuário citou;
  - menu do WPF, recusado na DEC-016 por foco e ativação; menu desenhado pelo próprio Buzzy, com mais código e acessibilidade própria; bitmaps guardados a sessão inteira, que ocupariam objetos GDI à toa.
- **Motivo:** atende o pedido com as caras que o usuário já viu, sem mudar o determinismo nem as reproduções; a v2 impede que um build antigo grave por cima do campo novo. O menu nativo já tem teclado e leitor de tela prontos.
- **Trade-offs:**
  - parte das trocas não muda a cara (item 3);
  - a 96 DPI, o ícone de 40 × 32 deixa cada opção do submenu com cerca de 32 px de altura, perto do dobro do normal, e o queixo sai cortado reto na borda de baixo. O passo T2 manteve a célula como está, e a conferência no menu de verdade é [MANUAL];
  - a cada abertura do menu, o dono temporário recebe o primeiro plano (DEC-016): nos testes de integração, que abrem o menu muitas vezes, o foco pisca;
  - até o passo P7, o app partia das preferências padrão e só registrava `GravarPreferencias` no log, e a escolha não sobrevivia a reabrir o app. Desde ele, o `Loaded` leva a emoção lida do arquivo, e o `GravarPreferencias` vira um pedido à agenda de gravação (DEC-029).
- **Desvios na implementação do menu (2026-10-01):**
  - a abertura inteira (montar, mostrar, destruir o menu e só então apagar os bitmaps) é uma função testável sem exibir, e também o estado e o DPI lidos na abertura; a revisão de correção pediu as duas últimas, porque o caminho real do menu não tinha teste;
  - `AppendMenuW` saiu: todo item entra por `InsertMenuItemW`, numa posição explícita;
  - o log leva o lado do rosto como texto (`lado=40x32`), porque o ícone não é quadrado.
- **Pendente** ([TODO.md](TODO.md), seção "Interação"):
  - [MANUAL]: a marca de rádio ao lado do rosto nos temas claro, escuro e de alto contraste; o Narrador lendo o submenu; a altura das opções e o queixo do recorte; o menu a 125%, 150%, 175% e 200%, porque os fatores 2 e 3 dos ícones só foram testados sem janela;
  - a V16 (a emoção gravada no perfil de teste e restaurada ao reabrir), escrita no passo P7 da Fase 5: falta rodá-la na verificação de tela com input SINTÉTICO.
- **Consequências:**
  - ARCHITECTURE.md 2.6 (dimensão, evento, transição e invariantes 4 e 27), 2.12 (esquema v2), 2.13.3 e 2.16 (menu); SECURITY.md 3.1, 5 e 7;
  - dois oráculos do teste de propriedade foram refinados, sem mudar as contagens: no invariante 4, começar é entrar no estado, e a transição para o mesmo estado não conta; a conferência das trocas de cara sem transição deixa a carga de fora;
  - testes `EmocaoDominanteTestes`, `EsquemaDeConfiguracoesTestes`, `PropriedadesDaPersistenciaTestes`, `ReproducaoTestes`, `InvariantesTestes`, `ArbitroPropriedadesTestes`, `InvarianteDezoito`, `ChaveLigadaTestes`, `ArquivoDeConfiguracoesTestes` e `IconesDoMenuTestes`; do menu, `MenuNativoTestes`, `MontagemDoMenuTestes`, `BitmapsDoMenuTestes`, `PlataformaTestes` e, na integração, `MenuIntegracaoTestes`.

## DEC-028 — Tamagotchi adulto: itens invocados pelo menu e usados por arrastar e soltar

- **Data:** 2026-09-30; detalhada em 2026-10-01, com o núcleo, a arte e o app implementados.
- **Estado da decisão:** pedido do usuário, com as respostas dele às três escolhas de desenho; desenho técnico de Claude sob DEC-015.
- **STATUS:** PLANNED. O núcleo (passos T3–T6 da seção "Interação" de [TODO.md](TODO.md)), a arte (A1–A4) e o app (T7–T9), com as correções das revisões, estão implementados e verificados por testes automatizados, por integração com mensagens postadas às janelas do próprio Buzzy e, em 2026-10-01, pela verificação de tela com input SINTÉTICO (V1–V15, X1 com os dois monitores na mesma escala, a regra da calma e o foco depois dos menus) e pelo repouso de 10 minutos com a onda de uma vodka (V13). A chave `Tamagotchi` está ligada no aplicativo desde o passo T9, que está fechado: o usuário já invoca e usa os itens pelo menu. A decisão continua sem VERIFIED pelas pendências [MANUAL] e [HW]: a revisão visual e de tom pelo usuário, os temas claro, escuro e de alto contraste, o Narrador, o conforto para agarrar os itens pequenos e as escalas de 125% a 200%, também entre monitores de escalas diferentes.
- **Pedido do usuário (2026-09-30):** um "tamagotchi virtual adulto". Pelo menu de contexto, "Itens", o usuário invoca itens em pixel art: baseado, banana, água, vodka, cocaína, MD, lança-perfume "e tudo mais".
  - O item só é invocado quando o usuário clica nele no menu e só é usado quando o usuário o arrasta e solta sobre o personagem.
  - Cada uso tem uma animação própria (fumar, cheirar, ingerir) e afeta o comportamento: "mais animado", "meio chapado de erva", "bêbado" e assim por diante.
- **Respostas do usuário:**
  1. **Sem necessidades que mudam com o tempo:** nada de fome, sede ou sono decaindo. Só os itens e as interações mudam humor e comportamento.
  2. **Primeira leva, 13 itens:** banana, água, vodka, cerveja, baseado, cigarro, cocaína, MD, lança-perfume, café, energético, cogumelo e bala (doce).
  3. **Onde o item aparece:** no chão, ao lado do personagem, caindo com física de desenho, e fica esperando.
- **Decisão (princípios):**
  - **Tom:** cartunesco e cômico, para uso privado de um adulto. Nenhuma informação real sobre drogas (dose, obtenção, preparo); só efeitos de desenho animado no comportamento, nas caras e nas animações.
  - **Regras que continuam valendo:**
    - o usuário prevalece: pressionar e arrastar interrompem o uso;
    - nenhum timer periódico em repouso: os efeitos acabam por disparos únicos;
    - o núcleo continua determinístico;
    - sem rede, texto, conversa ou voz;
    - as janelas dos itens são do próprio Buzzy e não leem nada de outros aplicativos.
  - **Exceção à física por energia:** os efeitos podem mudar a velocidade de propósito (um bêbado cambaleia, um agitado corre). Isso é uma exceção cartunesca documentada ao invariante 12, que continua valendo para o nível de energia.
- **Decisão (desenho), no núcleo** (tabelas e regras exatas em ARCHITECTURE.md 2.16; estado, eventos, transições e invariantes em 2.6):
  1. **O item é uma entidade do núcleo.** A invocação, a queda, o arraste e o teste de "solto sobre ele" ficam no núcleo, e a reprodução gravada cobre o ciclo inteiro. O app só desenha uma janela por item, a partir dos efeitos (ARCHITECTURE.md 2.3).
  2. **Itens e verbos.** O enum `Item` segue a ordem da resposta do usuário: Banana, Agua, Vodka, Cerveja, Baseado, Cigarro, Cocaina, Md, LancaPerfume, Cafe, Energetico, Cogumelo e Bala. A chave da arte é o nome em minúsculas (`lancaperfume`, `md`). O verbo de cada item fica numa tabela só, no núcleo:
     - comer: banana e cogumelo;
     - beber: água, vodka, cerveja, café e energético;
     - fumar: baseado e cigarro;
     - cheirar: cocaína;
     - engolir: MD e bala;
     - inalar: lança-perfume.
  3. **A duração do uso é a do verbo**, igual à soma dos quadros da animação na arte: comer 150 passos, beber 120, fumar 210, cheirar 120, engolir 90 e inalar 120, isto é, de 1,5 s a 3,5 s a 60 passos por segundo.
  4. **Estado novo `USING`,** no grupo do usuário: o relógio corre, nada autônomo chega, `PRESS` o segura no mesmo evento e, no fim, a acomodação o devolve ao mesmo apoio.

     *Atualização de 2026-10-01 (DEC-030, passo P8 da Fase 5):* numa mudança de topologia, `USING` é tratado como `REACTING`: o uso continua quando o monitor dele não mudou de geometria, inclusive quando só foi transladado, e acaba numa revalidação, com a onda seguindo.
  5. **Sete caras de efeito e seis gestos novos, no fim dos enums.** As caras (`Bebado`, `Enjoado`, `Chapado`, `Eletrico`, `Apaixonado`, `Tonto` e `Viajando`) só aparecem pela onda. Os gestos (`Soluco`, `Danca`, `Gargalhada`, `Espirro`, `Tosse` e `Tremedeira`) só acontecem em `IDLE`, e só a onda os sorteia. Os sorteios de sempre usam listas fixas, e as referências 01–05 não mudaram.
  6. **A onda de desenho animado.** Cada item, menos a água, começa uma onda: Satisfeito, Alegre, Relaxado, Ligado, Bebado, Chapado, Eletrico, Euforico, Tonto ou Viajando.
     - Ela passa por subida, pico com níveis de 1 a 3 e queda; algumas ondas não têm queda.
     - Cada fase, e cada nível do pico, dura um disparo único de 1 s ou mais de um temporizador só (`AgendarOnda`, `ITEM_EFFECT_TIMER` e `CancelarOnda`), sem relógio de passo fixo. Uma onda gera no máximo 2 + nível disparos, e o episódio mais longo dura 530 s.
     - A onda vale desde o soltar: interromper o uso não a desfaz. Ela continua com o personagem escondido e só é cancelada ao sair.
     - O disparo não reagenda a agenda autônoma; com a autonomia pausada, só troca a cara.
  7. **Combinação de itens.**
     - Do mesmo tipo da onda da frente: os níveis somam até 3, e a fase recomeça (a queda volta ao pico).
     - De precedência maior ou igual: vai para a frente, e a anterior fica atrás, congelada. Só cabem duas: uma terceira descarta a de fundo.
     - De precedência menor: é absorvida, sem mudar a onda nem o temporizador.
     - Do mesmo tipo da onda de fundo: os níveis dela somam, e ela continua congelada; se estava na queda, volta ao pico.
     - A água refresca: baixa um nível ou encerra a queda.
     - Quando a da frente acaba, a de fundo volta com a fase recomeçada, na duração cheia.
  8. **Comportamento pelo perfil efetivo (`PerfilEfetivo`).** A fase da onda aplica percentuais ao perfil de energia: intervalos entre decisões e de descanso, pesos das ações, altura do pulo e chance do foguete. Cada fase também tem os próprios gestos e caras, com pesos. Com a velocidade da fase abaixo de 100%, o tempo na parede e o tempo pendurado crescem por 100/velocidade, para a agenda não cortar a subida mais lenta (o problema do item 5 da DEC-022); acima de 100%, ficam os mesmos.
  9. **Física pela física efetiva (`FisicaEfetiva`), a exceção documentada ao invariante 12.** A onda multiplica só as velocidades de andar, escalar e pendurar, de 50% a 200%. A gravidade, a queda máxima, o quique, a velocidade do foguete, o agarrar, as colisões e os limites não mudam, e o nível de energia continua sem mexer na física.
  10. **Cambaleio:** nas ondas do bêbado e do tonto, o passo da caminhada oscila numa onda triangular de 48 passos (0,8 s), e o bêbado no nível mais forte dá pequenos recuos. As contas são inteiras até a última divisão, para dar o mesmo resultado em qualquer máquina. Ele fica sempre no chão, preso entre as laterais, e o recuo devolve distância ao percurso.
  11. **Precedência das caras:** a cara da fase vence a emoção dominante (DEC-027), que volta quando a onda acaba. Em `RESTING`, `REACTING` e `USING`, a cara da fase espera o fim do estado.
  12. **Itens no mundo:**
      - no máximo 6: o sétimo tira o mais antigo que não está na mão do usuário, e "Recolher itens" tira todos;
      - o item nasce 140 DIP acima dos pés dele, ao lado, primeiro do lado para onde ele olha. Cai com a mesma gravidade e a mesma queda máxima dele, quica uma vez, de leve, e não achata ao pousar;
      - parado e sem onda, ele fica empolgado com o item novo;
      - os itens só existem em memória e somem ao sair.

      *Atualização de 2026-10-01 (DEC-030, passo P8 da Fase 5):* numa mudança de topologia, os itens seguem a regra do personagem: no monitor que só foi transladado, andam junto, caindo ou no chão, com a mesma velocidade; senão, vão pela posição relativa no monitor correspondente ou no sobrevivente. O item na mão anda com o monitor em que está, e soltá-lo logo depois o deixa no mesmo monitor físico.
  13. **"Solto sobre ele":** o retângulo do item, já preso na área útil, cruza o retângulo do sprite do personagem encolhido 20% de cada lado. O núcleo decide sozinho, sem a raiz informar a transparência.
  14. **Aceitação do soltar:**
      - aceito com ele parado, andando (o plano é descartado), na parede ou no cipó (preso ou não), escondido na borda, descansando (acorda antes), reagindo ou pousando (os dois são cortados);
      - recusado com ele pulando, caindo, usando outro item, pressionado ou arrastado: o item cai de onde foi solto;
      - clique, clique duplo ou captura perdida sobre o item nunca o usam;
      - o fim do arraste de um item só segurado, sem o começo do arraste, larga o item, como o clique.
  15. **Volta ao mesmo apoio:**
      - o apoio do uso sai primeiro do estado e depois da posição: esconderijo, parede (fora do chão, inclusive na quina), cipó ou chão;
      - no fim do uso, do chão ele volta a `IDLE`; da parede e do cipó, agarrado, e preso se já estava (DEC-024); do esconderijo, a `PEEKING` na mesma borda (DEC-025);
      - na parede ou no cipó, volta agarrado mesmo a menos de 32 DIP do chão, sem ficar preso se não estava.
  16. **Atento:** enquanto o usuário segura um item, a agenda pausa e o personagem fica onde está:
      - andando, para; descansando, acorda;
      - na parede e no cipó, fica agarrado, e o foguete apaga;
      - pulo e queda seguem até o chão;
      - sem onda, olha curioso.

      Quando o item sai da mão, a agenda volta depois do intervalo de acomodação.
  17. **Visibilidade e captura:**
      - o item na mão do usuário sempre aparece, para o gesto nunca sumir no meio;
      - fora da mão, o item só aparece com o personagem visível e fora de um monitor ocupado pela tela cheia, com o modo ligado (Q-09);
      - um item que cai e deixa de aparecer vai direto ao chão: o relógio nunca corre por um item invisível;
      - esconder, sair, recolher ou pegar outro item soltam a captura do item que estava na mão, e ele fica no chão.
  18. **Regra da calma:** com a autonomia pausada ou o painel aberto, quem está agarrado à parede ou ao cipó sem estar preso desce ou se solta, no fim de qualquer evento. O preso fica (DEC-024), e o atento também, até o item sair da mão. Ela não depende da chave: vale no app desde já (notas na DEC-022, item 4, e na DEC-024).
  19. **Chave:** tudo o que é do tamagotchi fica atrás de `ConfiguracaoDoNucleo.Tamagotchi`, desligada por padrão e ligada em `DoAplicativo` desde o passo T9.
      - Desligada, os eventos dos itens e da onda são descartados antes de encerrar qualquer gesto, uma onda no estado não vale e nenhum efeito novo sai do núcleo.
      - Ela ligou no passo T9, com o app pronto e os testes de equivalência verdes (`ChaveLigadaTestes`): a chave ligada sem itens dá o mesmo resultado que desligada, e a dominante só muda a cara, mesmo com itens e ondas.
      - A emoção dominante (DEC-027) não tem chave.
- **Desenho da arte** (passos A1–A4; regras e arquivos em [IDENTIDADE_VISUAL.md](IDENTIDADE_VISUAL.md)):
  - **Itens:** 24 × 24 pixels de arte, mostrados em 48 DIP, na densidade do boneco. Pousam na última linha, com 1 px livre no topo e nas laterais e alfa só 0 ou 255. Não têm texto, marca nem folha, têm pelo menos 8 px de altura e de largura, para dar para agarrar, e não usam as cores do pelo. O desenho do chão também é o ícone do menu.
  - **Itens na mão:** variantes por verbo, com a pega, onde fica a mão, e a ponta, o pixel que encosta na boca ou no nariz.
  - **Caras:** as 7 de efeito, com as chaves do núcleo (`eletrico`, não `acelerado`), e 7 passageiras, que só a arte usa nas poses de uso. O chapéu do bêbado entorta 12°, e o bêbado e o enjoado ganham um rubor maior, forte ou verde.
  - **Poses de uso:** só no chão e de frente, uma sequência de quadros por verbo cuja soma é a duração do núcleo. O braço leva a ponta do item à boca ou ao nariz por cinemática inversa, qualquer que seja o tamanho do item, e a cara é a da própria pose. Na parede, no cipó e no esconderijo, a apresentação mostra a pose do apoio com a cara do uso e a sobreposição, sem o objeto; poses de uso por apoio ficam para depois.
  - **Sobreposições:** 8 efeitos em 3 fases, com a fase 0 parada para quando não há relógio, mais modificadores de pose só no chão. Nunca cobrem os traços do rosto.
  - **Gestos da onda:** poses provisórias, feitas das que já existem.
  - **Ícones do menu:** o rosto da DEC-027 e o desenho do chão de cada item, ampliados por um fator inteiro do DPI.
- **Decisão (desenho), no app** (passos T7–T9; regras exatas em ARCHITECTURE.md 2.3, 2.7, 2.10, 2.13 e 2.16):
  20. **Apresentação (passo T7):** o quadro do sprite passa a levar o item na mão, a sobreposição da onda da frente e a fase dela.
      - No chão, o uso mostra a sequência de quadros do verbo na arte, com a cara de cada quadro. Na parede, no cipó e no esconderijo, aparece a pose de quem está ali, sem o objeto, com a cara do item durante o uso, que o núcleo fixa do começo ao fim.
      - Os gestos da onda usam as poses provisórias da arte, com a cara delas.
      - A sobreposição vale por cima de qualquer pose e só troca de fase com o relógio ligado; parado, fica na fase 0.
      - O cache de quadros passou a ter um orçamento de 16 MiB de pixels: os menos usados saem primeiro.
  21. **Uma janela por item (passo T8),** com a receita da janela do personagem: do tamanho do item (48 × 48 DIP), transparente fora do desenho, sempre no topo, fora da barra de tarefas e sem nunca ativar nem tirar o foco. A captura do mouse só existe no gesto sobre o item, e só a raiz fecha a janela.
  22. **Ordem Z por evento:** o item fica logo abaixo do personagem; no gesto sobre ele, vai para o topo, para não sumir atrás do personagem justamente quando vai ser solto sobre ele, e volta para baixo no fim. Quando o personagem reaparece, a ordem é reafirmada. Nunca por timer (SECURITY.md 2).

      *Atualização de 2026-10-01 (DEC-030, passo P9 da Fase 5):* a releitura da topologia e a conferência tardia dela, que é um temporizador de disparo único, devolvem só o lugar das janelas dos itens, sem mexer na ordem Z. A ordem continua mudando apenas nos três eventos acima.
  23. **Árbitro dos itens:** uma segunda instância do árbitro de gestos da DEC-021, com as mesmas regras do personagem. Os gestos viram eventos do item em que o botão foi pressionado; clique, clique duplo ou gesto cancelado viram `ITEM_RELEASE`, e o item fica no chão, ou cai de onde está, sem uso. O botão direito num item abre o mesmo menu do personagem; tirar um item da tela é pelo "Recolher itens".
  24. **Temporizador da onda:** um segundo temporizador do app, ao lado do da agenda, só de disparo único: ele se desliga antes de avisar, nunca fica periódico e para ao encerrar.
  25. **Submenu "Itens" (passo T8):** só existe com a chave ligada e fica desabilitado com o Buzzy escondido. Tem os 13 itens na ordem do enum, só com o nome e o desenho do chão como ícone, um separador e "Recolher itens", desabilitado sem itens na tela. O id escolhido vira o item por uma lista fixa.
  26. **Diagnóstico:** linhas `ITEM` e `ONDA` no log de diagnóstico, só com `--diagnostico` e sem dado pessoal, para os testes de integração e a verificação de tela (contrato em ARCHITECTURE.md 2.13.4).
  27. **Chave ligada (passo T9)** em `DoAplicativo`, com os testes de integração dos itens. A verificação de tela (`--fase tamagotchi`) e a medição do repouso com onda (`tools/medir-desempenho.ps1 -Modo onda`) foram escritas e compiladas no mesmo passo, para Claude rodar depois de avisar o usuário.
- **Desvios do desenho, na implementação (2026-09-30) e nas revisões (2026-10-01):**
  - **Núcleo:**
    - a revisão adversarial, com mutações temporárias, achou um defeito real: no fim de um uso na parede perto do chão, ele caía. Agora volta agarrado (item 15);
    - os invariantes 22 e 27 passaram a ser conferidos também com a física e a chave ligada (`ChaveLigadaTestes`), condição para ligar a chave (item 19);
    - a regra da calma ficou no fim de todo evento, e não só ao pausar, para cobrir pausar com o item ainda na mão e o agarre que acontece já pausado (item 18);
    - o fim de arraste sem o começo larga o item (item 14), e o formato de reprodução lê `emocao=` e `item=` por lista fechada;
    - um item do mesmo tipo da onda de fundo na queda a leva de volta ao pico: a regra literal do desenho deixaria uma queda no nível 2, que o invariante 25 proíbe;
    - o tempo na parede e pendurado só cresce com a velocidade abaixo de 100% (item 8);
    - a onda continua com o personagem escondido; pegar um item no ar é aceito (ele para e fica na mão); ao sair, o núcleo não emite efeitos de janela de item, e a raiz fecha todas.
  - **Arte** (revisão adversarial, depois retoques olhando as prévias):
    - o espelhinho vai numa mão só: segurado com as duas mãos na altura da barriga, parecia roupa;
    - os cotovelos vão para fora na tragada e no inalar: o braço cruzava o queixo, e os dois braços pareciam um colete;
    - a fumaça e as bolhas saem do canto da boca, de frente: saindo da orelha, pareciam raiva e suor. A área protegida do rosto virou uma máscara dos traços, no lugar de um retângulo;
    - com o chapéu eriçado, a copa desce 1 pixel em escalando-2, para o preenchimento não chegar à borda do quadro; em andando-2, andando-4 e escalando-1, o contorno fica na linha 0, inteiro, como exceção documentada;
    - itens refeitos: o baseado aceso, o espelhinho com moldura, o lenço dobrado em triângulo, a bala sem o papel na boca, o energético verde-neon (escuro, tinha as cores do pelo e lembrava a paleta de uma marca conhecida) e o cigarro com 8 px de altura;
    - os testes passaram a achar a boca e o nariz pelo desenho, e conferem que os olhos ficam à vista com o item no rosto;
    - **descartados:** aumentar os itens na mão, que seguem a escala do boneco, com até 14 px, e refazer o contorno do recorte do rosto, que mudaria `expressoes.png`.
  - **App** (passos T7–T9, uma revisão de correção, uma das regras do projeto e um corretor, 2026-10-01):
    - o quadro de uso no chão não espelha pela direção, como nenhuma pose de frente: a luz vem da esquerda, a trama do chapéu é fixa e o item fica na mão B;
    - a sobreposição também cobre os quadros de uso, porque a onda começa ao soltar o item. Onde o quadro já tem efeito próprio, como a fumaça do fumar, podem aparecer dois juntos; fica para a revisão visual;
    - nos apoios, a cara vem do retrato, e não direto da tabela: no núcleo as duas são a mesma durante o uso, e assim a apresentação respeita uma tabela trocada pela configuração;
    - uma chave de arte ausente continua lançando exceção, sem quadro de reserva; a proteção é um teste que desenha todo quadro que a escolha pode pedir, com todos os valores dos enums, e outro que compara os enums do núcleo com as listas da arte;
    - o gerente das janelas, o árbitro dos itens, o temporizador da onda e a ligação da raiz com eles são classes pequenas, testadas sem janela, com janelas falsas. A revisão de correção achou a ligação da raiz e a montagem real do menu sem teste; as duas viraram funções testáveis;
    - um `MoverItem` só é pulado se o próximo do mesmo Id vier antes de um mostrar, esconder ou remover desse Id; o lugar final é o mesmo;
    - o pouso de um item vai para o log uma vez por pouso, no fim do processamento, mesmo quando o último passo não move a janela; a revisão achou casos em que a linha não saía, e os testes passaram a exigi-la;
    - "sobre ele" é recalculado na raiz só para o log, com a mesma função do núcleo; quem decide continua sendo o núcleo;
    - o ícone do item no menu tem os 24 × 24 pixels da arte vezes o fator do DPI, e não os 32 px do desenho do app; com a chave ligada, cada abertura do menu cria e apaga 27 bitmaps, mesmo com o submenu "Itens" desabilitado;
    - antes de ligar a chave, a ligação inteira foi exercitada numa cópia isolada, com a chave ligada só lá: invocar, cair, arrastar até ele e usar, a onda agendada e disparada, o botão direito no item, recolher e sair, com a ordem Z certa nas janelas reais;
    - esconder ou minimizar no meio do arraste de um item não tinha teste de integração; agora tem, com a captura solta e o soltar tardio sem efeito;
    - os leitores do log nos testes de integração e na verificação de tela perdiam linhas quando o log rotacionava; agora reconhecem a rotação pelo conteúdo (ARCHITECTURE.md 2.13.4). A medição de desempenho manteve a leitura dela, que avança a cada leitura;
    - na verificação de tela, três critérios (o tempo até o pouso, a agenda cancelada ao segurar um item e o recuo do bêbado) dariam falha sem defeito no app e foram corrigidos antes de rodar, conferidos contra o núcleo com a topologia desta máquina; o critério dos quadros de uso sai INCONCLUSIVO, pedindo repetição, quando a interface atrasa e pula quadros;
    - o repouso com onda dura 60 s na verificação de tela; os 10 minutos ficam no modo `-Modo onda` da medição. Uma vodka dura cerca de 5 min 20 s, então o relatório separa a CPU com a onda e depois dela;
    - duas mutações sobreviventes da revisão foram descartadas como equivalentes na prática: uma só importaria com um item à vista e o personagem escondido no meio do arraste, e a outra repetia uma captura e uma ordem Z que já valiam;
    - mutações temporárias em todos os passos e na correção, cerca de 150 aplicações: as que escaparam viraram testes mais fortes ou eram código equivalente, e as que não compilavam foram reescritas.
- **Alternativas consideradas:**
  - o efeito do item como uma alteração única, que substitui a anterior, com a água e a banana como restauradoras: um lança-perfume de 30 s "curaria" uma bebedeira;
  - a raiz decidir "solto sobre ele" pela transparência da janela: a regra ficaria no adaptador e fora da reprodução;
  - consumir o item no fim da animação e recriá-lo se o uso fosse interrompido, ou pegar o item no ar e usá-lo ao pousar: mais estado, sem ganho para o usuário;
  - ficar atento só quando o arraste do item começa: no pressionar, ele já para e facilita o soltar;
  - um pulso periódico ou um decaimento contínuo da onda: violam a DEC-011;
  - `Math.Sin` no cambaleio: sem garantia de resultado idêntico entre máquinas;
  - ligar a chave desde o começo: o app pararia com um efeito ou uma pose que ainda não conhece;
  - itens de 20 px, no máximo 5 na tela e nascendo 24 DIP acima, com impulso: ficaram o tamanho, o limite e a queda acima;
  - desenhar os itens na janela do personagem, que teria de ficar grande, contra a regra da janela do tamanho do sprite, ou numa janela Win32 crua, com mais código nativo e sem a validação de P1;
  - o botão direito num item removê-lo: ficou abrir o mesmo menu, como no personagem;
  - reafirmar a ordem Z das janelas por timer: proibido (SECURITY.md 2).
- **Motivo:** cumpre o pedido com regras pequenas e testáveis sem janela, sem relaxar os limites duros: o usuário prevalece, não há relógio em repouso, o núcleo é determinístico e nada sai da máquina.
- **Trade-offs:**
  - as tabelas são grandes, com números de jogo escolhidos para o comportamento se ler na tela, sem relação com nada real;
  - a onda muda a velocidade de propósito, uma exceção ao invariante 12;
  - os usos só têm pose no chão; nos outros apoios, ele usa sem o objeto à vista;
  - as poses dos gestos da onda são provisórias;
  - itens pequenos podem ser difíceis de agarrar, mesmo com o mínimo de 8 px de lado;
  - a suíte do núcleo ficou mais lenta: de cerca de 6,7 s para cerca de 10,6 s em Release;
  - os nomes dos itens ficam visíveis no código e nos textos do menu; se o repositório virar público (Q-10), a decisão é do usuário, como foi com o chapéu na DEC-019;
  - cada item é uma janela do WPF: a correção de DPI e a recuperação da minimização da Fase 5 (passos P12 e P14) valem também para elas. Até lá, uma janela de item minimizada pelo Windows volta ao normal na hora, sem esconder o personagem;
  - a 300%, o cache de 16 MiB guarda só 28 quadros do personagem, e andar com uma sobreposição já usa cerca de 24: pode haver redesenho repetido (conferência [MANUAL]; o log conta os quadros descartados).
- **Pendente** ([TODO.md](TODO.md), seção "Interação"): a revisão visual e de tom pelo usuário; as outras pendências [MANUAL] e [HW]; e os pontos herdados pelos blocos seguintes da Fase 5 (passos P12, P13 e P14). Os dos passos P7 e P8 foram feitos em 2026-10-01: a emoção no `Loaded` e a V16 escrita, ainda sem rodar (DEC-029), e o `USING` e os itens na mudança de topologia (DEC-030).
- **Consequências:**
  - o roadmap ganhou a seção "Interação — emoção dominante e tamagotchi adulto", intercalada com a Fase 5;
  - ARCHITECTURE.md 1, 2.3, 2.6, 2.7, 2.9, 2.10, 2.12, 2.13 e 2.16; SECURITY.md 2, 3.1, 3.2, 5, 6, 7 e 10; IDENTIDADE_VISUAL.md; PRODUCT_SPEC.md, seção "Emoção dominante e tamagotchi adulto"; notas na DEC-022 e na DEC-024;
  - testes do núcleo `TabelaDoTamagotchiTestes`, `OndaTestes`, `ItensTestes`, `UsoTestes`, `ChaveLigadaTestes`, `InvariantesTestes` (invariantes 22–29) e a referência `07-tamagotchi.txt`; da arte, `CarimboTestes`, `ItensPixelTestes`, `RostosNovosTestes`, `UsosPixelTestes`, `EfeitosPixelTestes`, `IconesDoMenuTestes`, `GestosPixelTestes` e `RostoDoDesenhoTestes`; do app, `NucleoEArteTestes`, `PoseDeUsoTestes`, `PoseTestes`, `CacheDeQuadrosTestes`, `SpriteDoItemTestes`, `GestosDosItensTestes`, `TemporizadorDaOndaTestes`, `GerenteDosItensTestes`, `JanelaDoItemTestes`, `LigacaoDosItensTestes` e `LeituraDoLogTestes`; na integração, `ItensIntegracaoTestes` e `MenuIntegracaoTestes`;
  - a verificação de tela ganhou `--fase tamagotchi`, e a medição de desempenho, `-Modo onda`.

## DEC-029 — Persistência mínima da posição: esquema v1, arquivo atômico e perfis de teste (Fase 5)

- **Data:** 2026-09-30; ligada no app em 2026-10-01 (passo P7).
- **Estado da decisão:** ACCEPTED por Claude sob a autorização de DEC-015 (decisão técnica dentro de DEC-010, DEC-011, ARCHITECTURE.md 2.12 e SECURITY.md 5). O item 11 é uma escolha de produto que o usuário deixou com Claude em 2026-09-30, às 20:40 ("faz o que achar melhor").
- **STATUS:** PLANNED até o gate da Fase 5 ser registrado em TODO.md. O esquema, o arquivo, a política de gravação e o isolamento dos testes (passos P3–P5) estão implementados e cobertos por testes automatizados. Desde o passo P7 (2026-10-01), o app lê o arquivo na partida e grava pela agenda, com o esquema v3, coberto por testes automatizados e de integração. Faltam a verificação de tela com input SINTÉTICO (a V16) e as conferências [MANUAL]: o Process Monitor (SECURITY.md 8, item 4) e o cenário S12 real.
- **Problema:** a Fase 5 precisa restaurar a posição entre execuções (DEC-010):
  - sem nunca deixar o arquivo ilegível;
  - sem gravação periódica (DEC-011);
  - sem dado pessoal no arquivo nem no log (SECURITY.md 5 e 6);
  - sem que testes e ferramentas leiam ou gravem as configurações reais do usuário.
- **Decisão** (regras exatas em ARCHITECTURE.md 2.12):
  1. **Onde mora cada parte:**
     - no núcleo puro (`Buzzy.Core.Persistencia`): o esquema v1 (tipos, padrões, validação, normalização e conversão de bytes) e a política de quando gravar;
     - no adaptador: a pasta, a leitura dos arquivos e o protocolo atômico (`PastaDeDados`, `ArquivoDeConfiguracoes`);
     - na raiz (`AgendaDeGravacao`, passo P7): a leitura da partida, o atraso, a gravação na hora e a do encerramento.

     A máquina de estados não ganha evento nem efeito: `GravarPosicao` e `GravarPreferencias` já existem, e a raiz recebe o evento que gerou cada efeito. No passo P7, `GravarPosicao` e `Loaded` só ganharam a postura (item 11). A restauração na partida é da DEC-030.
  2. **Formato:** JSON em UTF-8 sem BOM, com `schemaVersion` inteiro e nomes em português, em camelCase. A leitura extrai campo a campo de um `JsonDocument`, e a escrita usa `Utf8JsonWriter`. Não há `JsonSerializer` nem reflexão no núcleo ou no app, e System.Text.Json vem do framework, sem pacote NuGet (DEC-016).
  3. **Conteúdo do v1:**
     - a posição do efeito `GravarPosicao`, com a tela do monitor da época (DEC-030);
     - as preferências que o núcleo conhece: energia, modo de tela cheia e atravessar monitores (Q-05).

     `Preferencias` ganha `AtravessarMonitores` como parâmetro posicional com padrão ligado. Ainda não tem efeito: a travessia entra nos passos seguintes da Fase 5. Guardá-la desde já evita uma migração só por ela, e, a partir do passo P7, editar o arquivo com o Buzzy fechado é o jeito de desligá-la antes da tela de configurações da Fase 8.
  4. **Qual posição vai para o disco:** exatamente a do efeito `GravarPosicao`, isto é, onde o Buzzy estava ao soltar, cancelar o arraste, esconder, bloquear, suspender, redefinir ou sair.
     - Inclui o lugar a que o passeio autônomo o levou.
     - Com retorno de tela cheia guardado, vai o retorno, nunca a posição temporária (invariante 16). A postura que vai junto (item 11) é a do estado depois do evento.
     - A raiz nunca lê o estado do núcleo para gravar: a posição e a postura vêm do `GravarPosicao`, e as preferências, do `GravarPreferencias` e da leitura da partida.
     - Fica registrada, sem bloquear, a alternativa de gravar só a última posição escolhida pelo usuário (arraste ou redefinição). Ela faria o Buzzy "voltar para casa" a cada partida e exigiria um campo novo no estado do núcleo; o usuário pode pedi-la.
  5. **Leitura tolerante:** só a estrutura torna o arquivo ilegível; o resto é avaliado campo a campo, com aviso e o padrão do campo. A energia só vale pelos três nomes, nunca por `Enum.Parse`, que aceita `"1"` e `"Baixa,Alta"`. Comentários e vírgula final são aceitos, porque o arquivo pode ser editado à mão. Detalhes de implementação:
     - a chave exige UTF-16 válido: um surrogate solto não tem representação em JSON, e aceitá-lo faria a escrita lançar ou a posição não voltar igual da leitura;
     - um escape de surrogate solto num texto lido torna o arquivo inteiro ilegível, única exceção à tolerância campo a campo;
     - as frações usam a mesma regra de saneamento da restauração, e um `-0` vira `0`;
     - escrever ou normalizar um argumento nulo lança, porque o defeito é de quem chamou; preferências nulas valem o padrão, e uma chave nula descarta a posição;
     - os avisos têm texto fixo, só com nomes de campo do esquema e o motivo.
  6. **Versão futura:** o arquivo com `schemaVersion` maior é lido pelas regras da versão atual, e a gravação fica bloqueada nesta execução. Toda ampliação do esquema incrementa `schemaVersion`.
  7. **Leitura em cascata, sem efeito colateral:** principal, depois `.bak`, depois os padrões. Um principal inacessível depois de 3 tentativas faz valer a reserva ou os padrões e bloqueia a gravação nesta execução. Isso muda o critério 3 da Fase 8: um arquivo corrompido dá os valores do `.bak` ou, sem ele, os padrões.
  8. **Protocolo atômico:**
     - o temporário é apagado e recriado com `CreateNew`, sem buffer, e descarregado com `Flush(true)`;
     - o principal é conferido de novo, e a troca é por `File.Move` ou `File.Replace`;
     - o `.bak` só recebe um arquivo recém-validado, literalmente "o último arquivo bom";
     - a cópia de diagnóstico sai na mesma troca e substitui a anterior;
     - o temporário nunca é lido.

     Recriar o temporário evita gravar através de um link, físico ou simbólico, que já exista com esse nome.
  9. **Quando gravar** (`PoliticaDeGravacao`):
     - 2 s depois do último pedido, com a espera reiniciada a cada pedido;
     - na hora em `SUSPENDING`, `SESSION_ENDING`, `CMD_EXIT` e também `SESSION_LOCKED`: bloquear e depois suspender é o caminho comum antes do sono, e a suspensão com o personagem já escondido pela sessão não gera outro pedido;
     - novas tentativas em 2, 10 e 60 s; na hora, 3 tentativas com 50 ms entre elas.

     A agenda da raiz (`AgendaDeGravacao`, implementada no passo P7) também:
     - lê o arquivo uma vez, antes de tudo, numa instância só por execução; qualquer exceção da leitura desliga a persistência nesta execução, sem derrubar a partida;
     - não grava nem agenda quando o disco já tem o conteúdo pedido; sem nenhum pedido na execução, nada fica pendente, e a partida nunca grava sozinha. Vindo da reserva ou dos padrões, o primeiro pedido grava e recria o principal;
     - quando o disparo com atraso chega durante um gesto do usuário (botão pressionado, arraste ou um item na mão), não rearma a espera: o fim do gesto grava;
     - descarrega o pendente e para antes de desmontar o resto, ao encerrar, e descarrega o que der, uma vez só e sem lançar, no erro não tratado. A descarga na suspensão entra com o tratador dela (passo P11): hoje, entre os eventos de gravação na hora, o app só recebe a saída e o fim de sessão;
     - também agenda as novas tentativas quando a gravação na hora falha, salvo depois de parar;
     - desliga a gravação nesta execução, sem derrubar o Buzzy, quando ela lança uma exceção que não é de E/S;
     - faz a E/S de forma síncrona, na thread da interface: o arquivo tem cerca de 500 bytes, a gravação levou de 7,7 a 8,9 ms na integração, e o custo vai para o log.
  10. **Isolamento dos testes:** `--perfil-de-teste NOME` põe os dados em `%LOCALAPPDATA%\Buzzy\testes\NOME`; o log de diagnóstico continua na pasta do Buzzy.
      - **Falha fechada:** nome inválido, opção sem nome ou opção escrita de outro jeito desligam a persistência naquela execução.
      - **Nome:** validado caractere a caractere, sem expressão regular, porque `$` numa regex do .NET aceita um `\n` no fim. Os nomes reservados incluem `com0` e `lpt0`.
      - **Regra única:** só uma função escolhe a pasta (`PastaDeDados.DasConfiguracoes`), e só ela cria o arquivo (`ArquivoDeConfiguracoes.DaExecucao`), com teste de tabela e teste de fonte.
      - **Lançadores:** os testes de integração (`integracao` e, nos da persistência, `persistencia`), o `Buzzy.Verificacao` (`verificacao`) e a medição de desempenho (`desempenho`) passam o perfil e apagam a pasta dele antes de abrir o Buzzy, recusando junção ou link no caminho. Desde a revisão do bloco B da Fase 5 (2026-10-01), cada trecho do caminho que existe é conferido antes de tudo, também quando a pasta do perfil ainda não existe.
      - **Foto dos arquivos reais** (desde a mesma revisão): a integração, a verificação de tela e a medição de desempenho olham os arquivos reais de configuração do usuário antes e depois, só por fora (existência, tamanho e datas de criação e de escrita, nunca o conteúdo), e falham se algo mudar.
  11. **Esconderijo e "preso pelo usuário" também vão para o disco** (escolha de produto, 2026-09-30). O bloco A grava só a posição:
      - quem saía escondido reaparecia inteiro, na mesma borda;
      - quem estava preso na parede ou no cipó voltava agarrado, mas a agenda podia tirá-lo de lá.

      No passo P7, o esquema passa a guardar a borda do esconderijo (DEC-025) e a marca de preso (DEC-024). Ao reabrir, ele volta escondido no mesmo lado e continua preso onde o usuário o deixou. A ampliação incrementa `schemaVersion` (item 6). *Atualização de 2026-10-01 (DEC-027):* a v2 já é a da emoção dominante; o esconderijo e o preso entram na v3, e a versão futura dos testes passa de 3 a 4.

      *Implementado no passo P7 (2026-10-01):*
      - a v3 escreve sempre, dentro de `posicao`, `esconderijo` (`nenhum`, `baixo`, `esquerda` ou `direita`, lido por lista fechada) e `presoPeloUsuario` (booleano). Os dois só existem junto com uma posição; arquivos v1 e v2, sem eles, continuam lidos sem aviso, sem esconderijo e solto;
      - no núcleo, a postura vai nos efeitos `GravarPosicao` e na carga (`Loaded`), e não em `PosicaoDoPersonagem`: a camada geométrica não depende do personagem, o estado não ganha duas fontes de verdade, e a restauração da DEC-030 não mudou;
      - a carga só aplica a postura quando há posição salva; a borda só vale com o esconderijo pelo clique duplo ligado na configuração e dentro do enum. A acomodação o devolve escondido na mesma borda, ou agarrado e ainda preso; longe da parede e do cipó, a marca se apaga;
      - escondido pela bandeja ou pela sessão, a borda vai junto, mas a ocultação não: ao reabrir, ele aparece;
      - as reproduções gravadas escrevem `esconderijo=` e `preso=sim` só quando há postura, em `Loaded` e em `GravarPosicao`. As referências 01–05 e 07 não mudaram.
  12. **Log sem dado pessoal:**
      - erros do sistema vão só com o tipo e o código da exceção, nunca com a mensagem, que pode trazer um caminho com o nome do usuário ou o SID da conta. Isso já vale para a gravação, a instância única e o erro não tratado;
      - o nome de perfil recusado e a chave lida do arquivo não vão para o log;
      - as linhas `CONFIG` (passo P7; contrato em ARCHITECTURE.md 2.13.4) levam só enums, contagens e tempos. A pasta vai como `perfil` ou `padrao`, e a versão do arquivo, que é um valor lido dele, como `atual`, `anterior`, `futura` ou `-`, nunca o número. Nunca a pasta, valores do arquivo, nomes de campos desconhecidos nem o texto automático dos registros, que imprime a chave e a tela.
  13. **Reproduções gravadas:** as preferências escrevem `travessia=nao` só quando a travessia está desligada, em `Loaded`, `SettingsChanged` e `GravarPreferencias`. As referências 01–05 não mudam.
- **Alternativas consideradas:**
  - tudo no app, com a validação testável só com arquivos; E/S no núcleo, que quebraria a pureza; um efeito novo para descarregar a gravação, que mudaria as referências sem ganho;
  - `JsonSerializer`, por reflexão ou com contexto gerado: é tudo ou nada por tipo, sem tolerância por campo;
  - desligar a reflexão do JSON no projeto do app: ganha pouco e quebraria `Buzzy.Visual/Boneco.cs`, se ele voltar a ser usado;
  - versão futura tratada como ilegível, que destrói os dados novos num downgrade, ou gravada por cima, que apaga os campos novos;
  - escrever no próprio arquivo, ou `Move` com sobrescrita e cópia manual do `.bak`: deixam o arquivo rasgado ou um instante sem principal; recuperar pelo `.tmp`, com risco de ler um arquivo parcial;
  - padrões puros mesmo com um `.bak` bom;
  - gravar só ao sair, gravar a cada soltar sem atraso ou gravar numa thread de fundo;
  - isolar os testes por caminho arbitrário, que gravaria fora da pasta do Buzzy (SECURITY.md 3.1), ou por variável de ambiente, invisível.
- **Motivo:** DEC-007 e ARCHITECTURE.md 2.13.2 põem o esquema no núcleo e a E/S no adaptador. Assim o formato, a validação e a cascata são testados sem disco, e o protocolo, com disco real e sem abrir janela.
- **Trade-offs:**
  - um escape de surrogate solto num texto torna o arquivo inteiro ilegível, mais rígido que o resto da leitura;
  - um principal preso por cerca de 200 ms na partida bloqueia a gravação a sessão inteira;
  - a cópia de diagnóstico pode ficar registrada de forma imprecisa quando uma tentativa anterior de `File.Replace` já tinha movido o arquivo ilegível;
  - `File.Replace` exige NTFS local, e a queda de energia no meio da gravação não é testável (SECURITY.md 9);
  - os testes de queda com processo real deixaram a suíte do app sem janelas mais lenta: de menos de 1 s para cerca de 8 s;
  - cada gravação custa de 8 a 9 ms na thread da interface, 2 s depois do último pedido ou na hora ao sair. Isso pode atrasar um quadro numa verificação de tela;
  - o caminho do erro não tratado só foi revisto no código: não tem teste automático, e um gancho no produto só para testá-lo foi recusado. O descarregar do encerramento é a rede de segurança, porque a saída já grava na hora;
  - o Buzzy do usuário, aberto sem perfil de teste, passa a criar `%LOCALAPPDATA%\Buzzy\settings.json` ao sair.
- **Desvios na implementação do passo P7 (2026-10-01):**
  - a postura vai no efeito e na carga, e não na posição (item 11);
  - as preferências vêm só dos efeitos `GravarPreferencias` e da leitura da partida: o desenho sugeria lê-las do estado do núcleo, o que o item 4 proíbe;
  - uma exceção que não é de E/S na gravação desliga a gravação nesta execução, sem derrubar o Buzzy; o desenho a deixava propagar;
  - o gesto que adia a gravação inclui o item na mão, além de pressionar e arrastar, para a E/S não cair no meio do arraste de um item;
  - "sem pedido, nada pendente": sem isso, encerrar sem nenhum pedido recriaria o principal com os padrões;
  - um nome de campo com um surrogate solto às vezes é só um campo desconhecido e às vezes torna o arquivo ilegível; o teste exige apenas que a leitura nunca lance;
  - o gerador das sequências aleatórias de `InvariantesTestes` não mudou, para preservar as contagens de evidência; a cobertura da postura na carga ficou nos testes da persistência;
  - os testes de integração conferem os arquivos reais só por metadados: um resumo do conteúdo exigiria ler o `settings.json` real, o que a regra proíbe.
- **Adiado, com dono:**
  - **feito no passo P7 (2026-10-01):** `Iniciar` protege a leitura contra qualquer exceção, também a que não é de E/S, como um link plantado pelo próprio usuário para algo que não é arquivo: a persistência fica desligada naquela execução, e o log leva só o tipo e o código. A captura dentro da leitura do esquema ficou só com `InvalidOperationException`;
  - **aceito:** o `diagnostico.log`, anterior à Fase 5, segue um link que já exista (SECURITY.md 9).
- **Consequências:**
  - ARCHITECTURE.md 1, 2.6 (linha de `BOOTING` e invariante 18), 2.12 e 2.13 (linhas `CONFIG` em 2.13.4); SECURITY.md 3.1, 5, 6, 7, 8, 9 e 10; TODO.md, Fase 5 (passos e evidências), seção "Interação" (V16) e Fase 8 (critério 3);
  - notas nas DEC-010, DEC-016 (item 10), DEC-024, DEC-025 e DEC-027;
  - testes `EsquemaDeConfiguracoesTestes` (com a amostra `settings-v3.json`), `PropriedadesDaPersistenciaTestes`, `PosturaSalvaTestes`, `InvariantesTestes`, `InvarianteDezoito`, `ReproducaoTestes`, `ArquivoDeConfiguracoesTestes`, `AgendaDeGravacaoTestes`, `IsolamentoTestes` e `PlataformaTestes`; na integração, `PersistenciaIntegracaoTestes`; na verificação de tela, a V16, ainda não executada;
  - todo teste, verificação ou ferramenta que abre o `Buzzy.exe` passa `--perfil-de-teste` ([PROMPT_MESTRE_BUZZY.md](PROMPT_MESTRE_BUZZY.md)).

## DEC-030 — Chave do monitor e topologia (Fase 5)

- **Data:** 2026-09-30; passos P6, P8 e P9 em 2026-10-01.
- **Estado da decisão:** ACCEPTED por Claude sob a autorização de DEC-015 (decisão técnica dentro de DEC-008 e de ARCHITECTURE.md 2.4 e 2.8).
- **STATUS:** PLANNED até o gate da Fase 5 ser registrado em TODO.md. A restauração na partida (passos P1–P2), a chave estável do monitor (passo P6), a topologia em execução no núcleo (passo P8) e a releitura robusta no app (passo P9) estão implementadas, revisadas e cobertas por testes automatizados e de integração (2026-10-01); desde o passo P7, o app entrega a posição salva ao núcleo (DEC-029). Faltam a verificação de tela com input SINTÉTICO depois desses passos e as conferências [MANUAL] e [HW]: a estabilidade da chave e a calibração dos tempos no protótipo P5 e os cenários S8, S10, S11 e S12 reais.
- **Problema:**
  - até a Fase 4, a carga restaurava como a execução: pela mesma chave ou pelo monitor mais próximo da âncora absoluta salva. Numa partida com outra topologia (outro principal, monitor ausente, chave nova), essa âncora fica noutro referencial;
  - a posição não guardava o retângulo do monitor que ARCHITECTURE.md 2.8 pede;
  - uma carga com fração NaN deixava NaN no estado do núcleo e, depois, num `GravarPosicao`.
- **Decisão:**
  1. **Tela do monitor na posição:** `PosicaoDoPersonagem.TelaDoMonitor` é `RetanguloPx?`, com `init` e fora do construtor posicional; nula quer dizer desconhecida.
     - Semântica única: a tela do monitor da chave na última vez em que a posição foi descrita nele.
     - Quem preenche: `Descrever`, os dois casos de `Reacomodar`, a validação da máquina e `Restaurar`.
     - Nunca é deslocada por cálculo: com o app aberto, a posição que acompanha a topologia não translada a tela (item 8). Assim o arquivo nunca guarda uma tela que não existiu.
  2. **Restauração na partida, numa função só:** `Posicionador.Restaurar`, com `OrigemDaRestauracao` = `PelaChave`, `PeloRetangulo` ou `NoPrincipal`. Vai ao monitor da chave; senão, ao primeiro com a mesma tela; senão, ao principal.
     - Nos três passos valem as frações salvas, saneadas: NaN vira 0,5, e o resto é preso em [0, 1].
     - A âncora salva não é usada.
     - A posição nova fica com a chave e a tela do destino, e restaurar de novo não move o personagem.
     - Durante a execução valem as regras do item 8.
  3. **Texto da regra:** "BOOTING: configurações e topologia carregadas; posição salva restaurada pela chave | pelo retângulo do monitor | no monitor principal". O caminho "HIDDEN: pedido de mostrar anterior à carga" recebe o mesmo sufixo. Sem posição salva, o texto não muda, e as referências 01–05 ficam idênticas.
  4. **Pixel dos pés:** o monitor do personagem é o que contém `(x, y − 1)` (`Posicionador.PixelDosPes`). Vale em `Maquina.MonitorDaAncora`, sem mudança de comportamento, e no caso do monitor ausente de `Reacomodar`, que passou a medir a distância a partir dele. Com a barra oculta, a âncora no chão fica na base da tela, que já é o primeiro pixel do monitor de baixo, e a âncora crua escolheria o monitor errado.
  5. **Reproduções gravadas:** a posição salva do `Loaded` usa 9 campos (`chave;fx;fy;ax;ay;esquerda;topo;direita;base`) só quando a tela é conhecida e não vazia, para a reprodução restaurar pelo retângulo como a partida; senão, 5. `GravarPosicao` continua com 5 campos, e a leitura aceita 5 ou 9.
  6. **Chave estável do monitor** (passo P6, 2026-10-01; `ChavesDeMonitor` e `ConfiguracaoDeVideo`, no adaptador; regras exatas em ARCHITECTURE.md 2.4):
     - `mon:` seguido de 16 dígitos hexadecimais: os 8 primeiros bytes do SHA-256 do caminho do dispositivo do monitor, em maiúsculas. Esse cálculo nunca pode mudar entre versões, e testes fixam valores dele. O caminho, que identifica o hardware, vira resumo na hora: nunca vai para o arquivo nem para o log, e o nome amigável e o EDID nunca são lidos (SECURITY.md 3.1);
     - o caminho e o nome GDI de cada monitor vêm da configuração de vídeo, só lida (`GetDisplayConfigBufferSizes`, `QueryDisplayConfig` e `DisplayConfigGetDeviceInfo`). No modo clone, vale o menor caminho, para a escolha não depender da ordem do Windows;
     - quando a consulta falha, por exemplo numa sessão remota ou bloqueada, ou não traz o caminho de um monitor, vale a chave da última consulta boa para o mesmo nome GDI com uma tela do mesmo tamanho, transladada ou não; sem ela, a reserva `gdi:` seguida do nome GDI. O DPI não conta, porque é a escala que o usuário escolhe;
     - nenhuma chave se repete: as lidas agora são distribuídas primeiro, e uma do cache que repetiria outra vai para a reserva. O cache só vive na execução e só é trocado por uma leitura coerente, inteira e com a consulta boa;
     - para o núcleo, a chave continua opaca: ele só a compara por igualdade.
  7. **Leitura incoerente e leitura parcial** (passo P6 e revisão do bloco B):
     - a leitura inteira é incoerente quando a informação ou o DPI de um monitor não podem ser lidos, o DPI vem zero, falta o nome GDI ou ele se repete: a topologia nunca sai sem um monitor nem com uma escala inventada. A anterior continua valendo, e a leitura é tentada de novo;
     - como último recurso, depois das 5 leituras da partida e na última tentativa de cada rajada de releituras, vale a leitura parcial: o monitor que falha fica de fora, contado no log com a falha (função e código), e o cache não muda. Sem ela, uma falha persistente impedia a partida e, com o app aberto, nenhuma releitura saía.
  8. **Topologia em execução, no núcleo** (passo P8; `Posicionador` e `Maquina.MudarTopologia`; regras exatas em ARCHITECTURE.md 2.6 e 2.8):
     - em qualquer estado fora de `EXITING`, a posição guardada e o retorno da tela cheia acompanham a topologia, sem mover a janela (`Rebasear`). Com o monitor correspondente, a mesma fração na área útil atual dele, com a chave e a tela dele. Sem ele, a chave, as frações e a tela ficam, e só a âncora anda, junto com o sobrevivente mais próximo do pixel dos pés medido nas coordenadas antigas;
     - o monitor correspondente é o da mesma chave; sem ele, o primeiro com a mesma tela cuja chave não existia antes. É o apelido por retângulo da DEC-008, para a chave que passou de `gdi:` para `mon:` ou mudou com a porta ou o driver;
     - nos estados que revalidam (autônomos, físicos, `REACTING` e `USING`), se a geometria do monitor do personagem não mudou (só outros monitores mudaram, ou o dele só foi transladado ou só trocou de chave), o estado continua, e a janela anda junto (`ContinuarNoMonitor`, invariante 19). Se mudou, `SETTLING` na mesma posição relativa. Se ele sumiu, `SETTLING` no sobrevivente mais próximo medido nas coordenadas antigas;
     - no gesto, nada é validado: toda saída de `PRESSED` parte de onde ele estaria parado, e, em `DRAGGING`, o lugar do arraste anda com o monitor em que está, como a janela e o cursor;
     - os itens seguem a mesma regra, e o item na mão anda com o monitor em que está (DEC-028).
  9. **Releitura robusta, no app** (passo P9; `AgendaDaReleitura`; regras exatas em ARCHITECTURE.md 2.4 e 2.8):
     - as mensagens que podem mudar a topologia, inclusive a `TaskbarCreated`, vão ao log só com o tipo (no `WM_DPICHANGED`, também o DPI novo) e pedem a releitura, que sai 300 ms depois da última, com teto de 1 s desde a primeira. A espera mínima que a retomada vai pedir (passo P10) prevalece sobre as duas;
     - a rajada acaba quando a releitura sai. Uma leitura incoerente é tentada de novo em 500 ms, 1 s e 2 s; depois, a agenda desiste até a próxima mensagem. Cada releitura leva no máximo 32 motivos;
     - cada releitura publicada devolve a janela do personagem e as dos itens ao lugar do núcleo, se saíram dele, sem mexer na ordem Z, e arma a conferência tardia do mesmo lugar 1,5 s depois, no máximo uma por rajada, nunca periódica. Com "Lembrar locais das janelas" ligado, o Windows pode devolver uma janela ao monitor reconectado depois da releitura;
     - com a barra de tarefas recriada, o ícone volta na hora, e a topologia vai ao núcleo pela releitura agrupada: a raiz não troca mais a topologia por fora do núcleo;
     - os três tempos são provisórios até o protótipo P5.
  10. **Log** (contrato em ARCHITECTURE.md 2.13.4; SECURITY.md 6): a linha `TOPOLOGIA` leva cada chave ao lado do nome GDI, o resultado da consulta e as contagens; `POSICAO` ganhou o nome GDI; `MENSAGEM` leva só o tipo; `POSICAO|reaplicada` e `ITEM|reaplicado` levam só retângulos das janelas do Buzzy. O caminho do dispositivo e o nome do monitor nunca vão.
- **Alternativas consideradas:**
  - continuar carregando por `Reacomodar`, que escolhe o monitor mais próximo de uma âncora de outra sessão;
  - "monitor mais próximo da âncora salva" no último passo: menos previsível, porque o Buzzy poderia surgir numa TV recém-ligada;
  - a tela como quinto parâmetro posicional, que quebraria as construções de posição nos testes, ou guardada só no arquivo, que a perderia quando o monitor some com o personagem escondido;
  - um retângulo vazio para "desconhecido", em vez de nulo;
  - duas funções de restauração, com textos de regra diferentes;
  - guardar o caminho do dispositivo, que poria um identificador de hardware no arquivo e no log, ou o nome GDI, que muda entre sessões (DEC-008);
  - exigir a mesma tela para usar o cache: com a consulta negada, a troca de principal punha todas as chaves na reserva, e o apelido por retângulo levava o personagem ao outro monitor;
  - revalidar em toda mudança de topologia: uma troca de principal interrompia a caminhada, a reação e o uso;
  - transladar a tela guardada junto com a posição: o arquivo guardaria uma tela que nunca existiu;
  - medir o sobrevivente nas coordenadas novas: com o principal desconectado, o Windows move a origem, e o mais próximo seria outro;
  - agrupar sem teto, que deixa uma rajada sem fim adiar a releitura para sempre; conferir o lugar das janelas periodicamente, contra a DEC-011; reafirmar a ordem Z dos itens na releitura e na conferência tardia, que poria um temporizador mexendo na ordem Z (SECURITY.md 2).
- **Motivo:** é a cascata que DEC-008 e ARCHITECTURE.md 2.8 já descreviam. A fração é estável entre sessões; a âncora absoluta não é. A chave pelo resumo é tão estável quanto o caminho, sem gravar o identificador do hardware. Numa troca de principal ou num rearranjo, o Windows só translada os monitores, e para o personagem nada mudou.
- **Trade-offs:**
  - a igualdade de `PosicaoDoPersonagem` passou a incluir a tela: compare campo a campo ou obtenha as duas posições pelas funções do `Posicionador`;
  - o pixel dos pés muda a escolha em execução em casos de borda: a âncora exatamente na base da tela com um monitor logo abaixo, ou um empate de distância deslocado em 1 px;
  - com monitores clonados ou sobrepostos, a escolha "pela tela" é determinística, mas arbitrária;
  - a consulta da configuração de vídeo e a enumeração dos monitores são duas chamadas: uma reconfiguração entre elas pode trocar chaves até a próxima releitura, cerca de 300 ms depois. Cada releitura passou a custar a consulta, só em eventos;
  - com a consulta negada, um nome GDI reaproveitado por outro monitor do mesmo tamanho herda a chave; antes, isso só acontecia com a mesma tela;
  - o caminho inclui a porta: trocar o cabo de porta muda a chave, e a restauração vai pelo retângulo ou para o principal;
  - com a leitura parcial, depois de cerca de 3,8 s de falhas seguidas, o monitor que falha fica de fora; se for o do personagem, ele migra e não volta sozinho;
  - na partida seguinte, com a chave sumida, o apelido por retângulo pode cair no sobrevivente que passou a ocupar a tela antiga, enquanto com o app aberto ele iria ao sobrevivente mais próximo: é a semântica da DEC-008;
  - os monitores ocupados pela tela cheia ficam guardados pelas chaves e não seguem um apelido até a raiz mandar outra lista (Fase 8);
  - a conferência tardia pode desfazer, até 1,5 s depois de uma releitura publicada, um lugar que outro programa deu à janela. Com DPI diferente entre os monitores, isso pode deixar INCONCLUSIVO o critério 6 da Fase 1 na verificação de tela [HW];
  - o arraste e o item na mão supõem que o Windows leva a janela junto com o monitor numa troca de principal, o que só um S2 real confirma [HW].
- **Resolvido no passo P8 (2026-10-01):** um clique depois de o monitor mudar com o botão pressionado validava a posição com a tela nova, mas com as frações da área antiga, até o fim da reação. Agora toda saída de `PRESSED` parte da posição que acompanhou a topologia, e as frações gravadas descrevem o lugar validado (item 8). O teste que documentava o defeito foi reescrito.
- **Desvios na implementação (2026-10-01):**
  - **chave:** a distribuição em duas passagens (numa só, a chave lida agora podia ir para a reserva); nome GDI vazio ou repetido torna a leitura incoerente, como antes, quando o nome era a chave; a leitura da topologia é uma classe, e não um registro, para o texto automático não imprimir tudo no log, e o texto do alvo da consulta esconde o caminho;
  - **topologia:** perto da borda de cima, a revalidação o faz agarrar o cipó de novo (DEC-024), em vez de cair, como previa o desenho, anterior ao agarrar; a posição fina só anda nos estados de movimento, porque os outros partem da âncora; quando o estado continua, a posição passa a descrever o lugar novo, e frações arbitrárias vindas do arquivo viram as exatas da mesma âncora; o invariante 20 só confere a âncora na área útil quando o sprite cabe nela;
  - **correção anterior à fase,** exposta pelo fluxo aleatório novo dos testes: num monitor mais estreito que o sprite, ele começava a escalar fora da lateral. Agora as duas laterais ficam no meio, onde a validação o põe (ARCHITECTURE.md 2.5);
  - **releitura:** a lógica ficou numa classe própria, testável sem janela; uma mensagem durante uma nova tentativa começa outra rajada, com as tentativas do zero, como o código anterior fazia; o mesmo prazo não reagenda; a barra recriada mantém uma leitura só para o tamanho do ícone e pede a releitura mesmo sem a bandeja criada;
  - **revisões de correção e de segurança:** o cache pelo tamanho da tela, a regra do gesto em todas as saídas de `PRESSED`, o arraste e o item na mão que andam com o monitor, a releitura sem ordem Z, o teto de 32 motivos e a leitura parcial entraram por elas (DEVELOPMENT_LOG.md).
- **Pendente, com dono:**
  - **passo P10:** o sinal do árbitro do sistema entre o log `MENSAGEM` e o pedido de releitura, que passa a levar a espera mínima; reler a topologia antes de `SESSION_UNLOCKED` e `RESUMED`;
  - **passo P11:** a releitura imediata do mostrar vira uma função própria; esse caminho não arma a conferência tardia;
  - **passo P12:** a minimização consulta a releitura pendente da agenda;
  - **passo P13:** a travessia em curso fica fora do invariante 19 e vai a `SETTLING` numa mudança de topologia;
  - **passos P13 e P14:** limitar as rodadas seguidas de reaplicação do lugar causadas só pelo próprio `WM_DPICHANGED`, quando o sprite atravessar monitores ou com a histerese de escala;
  - **protótipo P5 [MANUAL][HW]:** a estabilidade da chave (reiniciar, trocar porta ou cabo, atualizar o driver, modo clone, sessão bloqueada, RDP e um monitor DisplayPort que some com a tela apagada) e a calibração de 300 ms, 1 s e 1,5 s.
- **Consequências:**
  - ARCHITECTURE.md 1, 2.4, 2.5, 2.6 (linhas de `BOOTING`, `PRESSED`, `TOPOLOGY_CHANGED` e `USING`; invariantes 19 e 20), 2.7, 2.8, 2.13.3, 2.13.4 e 2.16; SECURITY.md 2, 3.1, 3.2, 5, 6 e 10; notas nas DEC-008, DEC-016 (item 8), DEC-024, DEC-025 e DEC-028;
  - testes `RestaurarTestes` (cascata, S1–S7, S9, S11, frações inválidas, idempotência, carga e reprodução), `PropriedadesDaPersistenciaTestes`, `ReacomodarTestes`, `PosicionadorTestes`, `PropriedadesTestes` e a conferência da carga em `InvariantesTestes`;
  - dos passos P6, P8 e P9: `ChavesDeMonitorTestes`, `LeitorDeTopologiaTestes`, `PlataformaTestes`, `RebasearTestes`, `MudancaDeTopologiaTestes`, `InvariantesTestes` (invariantes 19 e 20 e as saídas do gesto), `TransicoesComplementaresTestes`, `MovimentoTestes`, `AgendaDaReleituraTestes` e `GerenteDosItensTestes`; na integração, a fumaça (a mesma chave nos dois processos), a barra recriada, a rajada sem fim, a conferência tardia e a releitura com itens;
  - na verificação de tela, o critério 5 da Fase 3 compara o nome GDI, e o X1 do tamagotchi acha a chave do secundário pela linha `TOPOLOGIA`;
  - o mapa dos cenários S1–S12 fica em TODO.md, Fase 5.

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

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

## DEC-006 — Stack de desktop: WPF com C# e .NET 10

- **Data:** 2026-09-26
- **Estado da decisão:** ACCEPTED em 2026-09-26 por delegação explícita do usuário ao Codex para escolher a stack e organizar os próximos passos.
- **STATUS:** PLANNED. A stack foi escolhida; a implementação e a viabilidade no Buzzy ainda não foram verificadas.
- **Problema:** escolher a tecnologia de desktop Windows que sustenta o MVP do Buzzy: personagem sobre o desktop em janela transparente onde o clique atravessa só os pixels invisíveis, arraste com prioridade do usuário, multi-monitor real com DPI misto, consumo quase nulo com o app aberto por horas e nenhuma capacidade além do necessário.

### Método

Nove alternativas foram pesquisadas em 2026-09-26, cada uma nas nove dimensões pedidas pela Fase 0, com preferência por documentação oficial. Parte das alegações que pesam na escolha passou por verificação adversarial: um verificador independente tentou refutar cada uma abrindo a fonte citada. **Cobertura da verificação, que é um limite importante desta comparação:** a pesquisa produziu 420 alegações, das quais 300 foram marcadas como decisivas. A verificação foi limitada a seis por assunto, então 84 foram verificadas e **216 não foram**. Das 84 verificadas, 54, ou cerca de dois terços, foram refutadas ou corrigidas por afirmarem mais do que a fonte sustenta, quase sempre em números de consumo vindos de terceiros. As 216 não verificadas mantêm a confiança que o pesquisador original declarou e devem ser lidas com essa ressalva. Três painéis independentes classificaram as stacks com lentes diferentes (risco, esforço até o MVP, desempenho e segurança). Nenhum escolheu por popularidade ou familiaridade.

Os limites do método estão na seção "Incertezas que permanecem" e as fontes na seção "Fontes primárias", as duas ao final desta decisão. Essa seção inclui os arquivos de código lidos e os endereços dos números de tamanho usados na tabela.

### Critério que decide a comparação

O requisito mais restritivo do Buzzy não é desempenho nem esforço: é **o clique atravessar os pixels transparentes da janela e chegar ao aplicativo de baixo, que é de outro processo**. Sem isso, o retângulo do sprite bloqueia cliques no desktop, e o mascote deixa de ser um personagem sobre a tela para virar uma janela no caminho.

Segundo a documentação da Microsoft, no Windows há um único caminho em que o próprio sistema faz esse teste por pixel: uma janela com o estilo layered cujo conteúdo é entregue por `UpdateLayeredWindow`. Aí os pixels com alfa exatamente 0 deixam as mensagens de mouse passarem, sem nenhum código do aplicativo. O caminho alternativo, uma janela sem superfície de redirecionamento composta por DirectComposition, faz o teste no retângulo inteiro: pixels transparentes continuam capturando o clique. Retornar "transparente" no teste de acerto não resolve, porque a mensagem só é repassada a janelas da mesma linha de execução do mesmo processo.

Quem não está no caminho layered precisa de um dos três contornos, e cada um colide com outro requisito do produto:

| Contorno | Colisão |
|---|---|
| Consultar a posição do cursor várias vezes por segundo e ligar ou desligar o click-through | Cria atividade periódica permanente enquanto o Buzzy estiver visível, o que contraria a meta de consumo quase nulo em repouso |
| Hook global de mouse | É exatamente a API que DEC-005 proíbe e que o portão de segurança do build bloqueia. O Windows também remove esse hook em silêncio se o tratamento demorar mais de um segundo |
| Recortar a janela por região a cada quadro | Recorte de um bit, serrilhado, recalculado a cada pose, com custo de CPU e sem nenhuma fonte que valide o resultado sobre conteúdo composto na GPU |

Três stacks herdam o caminho layered, e isso foi confirmado lendo o código de cada uma: Win32 nativo por construção, WPF com `AllowsTransparency` e Qt 6 Widgets com janela translúcida rasterizada. As outras seis dependem de contorno.

### Tabela comparativa

Legenda: **ok** atende com API documentada; **parcial** atende com trabalho extra ou incerteza; **contorno** só atende por meio de um dos contornos acima; **não** não atende.

| Dimensão | Win32 nativo | WPF (.NET 10) | Qt 6 Widgets (C++) | Avalonia 12 | Tauri 2 | Electron 44 | WinUI 3 | Flutter 3.47 | Godot 4.7 |
|---|---|---|---|---|---|---|---|---|---|
| Transparência por pixel | ok | ok | ok | ok | parcial | ok | contorno | contorno | parcial |
| **Clique atravessa pixel invisível** | **ok** | **ok** | **ok** | não | contorno | contorno | contorno | contorno | contorno |
| Sem borda, sempre no topo, bandeja, ciclo de vida | parcial, tudo à mão | parcial, bandeja vem do WinForms | ok | ok | parcial, sem evento de fim de sessão | ok | parcial, bandeja de terceiros | contorno, plugins | parcial, aparece na barra de tarefas |
| Arraste e captura do mouse | ok | ok | ok | ok | parcial, bugs abertos | parcial | parcial | parcial | parcial |
| Não roubar foco e caixa de texto | parcial, depende de P4 | parcial | parcial | parcial, bug de IME | parcial | parcial | parcial | parcial | parcial |
| Rendering e animação | ok, cópia do bitmap por quadro | parcial, leitura da GPU por quadro | ok | ok | parcial | parcial | parcial | parcial | ok |
| Multi-monitor, coordenadas negativas, DPI por monitor | ok | parcial, posição ambígua com DPI misto | parcial, espaço lógico em ilhas | parcial, bugs abertos | parcial, sem evento de mudança | parcial, bug aberto de DPI misto | ok | não, plugin converte errado | não, consciência de DPI do sistema |
| Segurança e permissões | ok, um processo, sem runtime | parcial, runtime externo | parcial, runtime do compilador | parcial | parcial, motor web multiprocesso | parcial, contorno usa hook global | parcial, runtime próprio | parcial, plugins nativos de terceiros | parcial, módulos de rede no build padrão |
| Tamanho e distribuição | ok, executável portátil | parcial, 60 MB de runtime ou 150 MB embutidos | parcial, DLLs e redistribuível com elevação | ok | ok, binário pequeno | não, 151 MB de runtime | parcial | parcial | parcial, 104 MB |
| Estabilidade e manutenção | parcial, biblioteca Rust em série 0.x | ok, suporte até 2028 | parcial, fim do Windows 10 após 6.12 | parcial, linha 12 recente | parcial, versão 3 em alfa | não, troca de versão maior a cada 8 semanas | parcial, atualização a cada 6 meses | parcial, plugins de um mantenedor | parcial |
| Consumo em repouso, pelo mecanismo | melhor: a fila de mensagens bloqueia e o sistema guarda a imagem | depende de desligar o laço de renderização | bloqueia, mas animações usam timer de 1 ms | corrigido só na linha 12 | contorno de consulta periódica impede repouso | multiprocesso, sem repouso real | superfície extra por quadro | só desenha quando pedido | sempre eleva a resolução do timer |

### Classificação da pesquisa técnica original

| Lente | 1º | 2º | 3º |
|---|---|---|---|
| Risco primeiro | Win32 nativo | Qt 6 Widgets | WPF |
| Esforço até o MVP | Win32 nativo (Rust) | WPF | Qt 6 Widgets |
| Desempenho e segurança | Win32 nativo | Qt 6 Widgets | WPF |

Os três painéis da pesquisa classificaram Win32 nativo em primeiro nas dimensões técnicas que receberam. Esse resultado permanece como evidência comparativa, mas não decide sozinho o melhor caminho para este projeto: o custo de construir e manter controles de interface manualmente pesa bastante para um projeto individual conduzido com assistência de IA.

### Resolução da escolha

- **Stack escolhida:** WPF, C# e .NET 10 LTS. Usar WPF para as janelas e controles da aplicação; concentrar chamadas Win32 necessárias num único adaptador pequeno. O núcleo determinístico permanece numa biblioteca C# sem dependência de WPF ou Windows.
- **Motivo:** a documentação oficial confirma que `Window.AllowsTransparency` usa janela layered, o caminho documentado para teste de acerto por alfa. Isso atende ao requisito decisivo sem acrescentar polling global nem hook de mouse, enquanto controles e ferramentas de C# reduzem a quantidade de infraestrutura visual escrita à mão. .NET 10 tem suporte LTS até novembro de 2028.
- **Por que não Win32 como primeira escolha:** a pesquisa original identificou corretamente suas vantagens de controle direto, baixo número de dependências e eficiência potencial. Para o Buzzy, porém, os controles, foco, acessibilidade, bandeja e manutenção da interface feitos à mão aumentariam o trabalho e a chance de inconsistência. O usuário delegou a escolha considerando o projeto como um todo, então a capacidade de construir e manter é um critério de primeira ordem.
- **Riscos aceitos para validar:** runtime .NET maior que um executável nativo; integração de coordenadas WPF em DIPs com as coordenadas físicas do desktop; garantir que WPF pare desenho e timers em repouso; e confirmar que uma janela WPF sem ativação pode arrastar sem roubar o foco. P1, P2 e P3 verificam esses pontos antes da Fase 1.
- **Regra de reversão:** se P1 não demonstrar clique passando por alfa 0, ou se P3 não conseguir um arraste utilizável sem roubar foco após as alternativas definidas, parar antes da Fase 1 e o Codex reabre a escolha entre Win32 nativo e outras opções. P2 define a linha de base e as metas de desempenho; resultado ruim exige corrigir o desenho de repouso antes de avançar.
- **P9 deixa de ser gate:** medir o custo de uma interface Win32 manual perdeu relevância depois da escolha de WPF. A escolha Q-22 de orçamento para esse protótipo fica encerrada como não aplicável.

As fontes principais desta resolução são a documentação oficial de [Window.AllowsTransparency](https://learn.microsoft.com/dotnet/api/system.windows.window.allowstransparency), [regiões tecnológicas do WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/technology-regions-overview) e [suporte do .NET](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support). A documentação dá base para escolher e desenhar os protótipos, mas não substitui P1 a P3 no ambiente real do Buzzy.

### Alternativas consideradas e por que não venceram

- **WinUI 3 / Windows App SDK**, a stack oficial da Microsoft para desktop: não tem API de transparência por pixel; o pedido foi fechado sem entrega e nenhuma versão até a 2.5.1 acrescentou o recurso. O painel de composição com conteúdo externo não suporta transparência. Sobra recorte por região e bibliotecas mantidas por indivíduos para bandeja e transparência.
- **Electron**: tem as APIs mais completas de monitores e ciclo de vida, mas o click-through utilizável instala hook global de mouse, o que DEC-005 proíbe. É sempre multiprocesso, o runtime tem cerca de 151 MB e sai uma versão maior a cada oito semanas, com apenas três suportadas.
- **Tauri 2**: binário pequeno e o melhor modelo de permissões entre as stacks com motor web, mas o motor web recebe o clique mesmo em pixel invisível, e o contorno da comunidade consulta o cursor a cada 16 ms. Não há evento de conexão de monitor nem de fim de sessão, e o comportamento crítico acaba em código Win32 de qualquer forma. A versão 3 está em alfa, sem janela de manutenção anunciada para a 2.
- **Avalonia**: a documentação oficial diz que não há click-through como no WPF, e os mantenedores evitam o estilo que o habilitaria. Na linha 11 o laço de renderização acorda o processo mesmo parado; a correção existe só na linha 12, sobre a qual há pouca evidência.
- **Flutter**: janela transparente não tem suporte oficial; o pedido está aberto desde 2020. Janela, bandeja e monitores dependem de plugins de um único mantenedor, em migração para um núcleo ainda em desenvolvimento, e a transparência usa uma função não documentada do Windows.
- **Godot**: é consciente de DPI apenas no nível do sistema, o que contraria o requisito de DPI por monitor do MVP; sempre eleva a resolução do timer; o binário tem 104 MB; o build padrão inclui módulos de rede, o que atrapalha a auditoria de "sem rede".
- **Um overlay transparente do tamanho da tela**, em qualquer stack: descartado. A documentação recomenda que a janela layered seja a menor possível, e há relatos de atraso de mouse no sistema inteiro e centenas de MB com overlay de tela cheia.

### Motivo

Três razões, em ordem de peso:

1. WPF oferece o caminho de janela layered necessário para o clique atravessar os pixels invisíveis, sem consulta periódica, sem hook global e sem recorte por região.
2. O projeto é conduzido por uma pessoa que quer evoluir o produto com assistência de Claude e revisão independente do Codex. C# e os controles do WPF reduzem o trabalho repetitivo da interface e deixam mais do comportamento visível em código de alto nível.
3. Mantemos os trechos mais sensíveis de Windows concentrados num adaptador pequeno, e o núcleo fica puro e testável em C#. O custo em repouso, a coordenada por DPI e o foco serão medidos em protótipos, sem presumir que o framework os resolve sozinho.

### Trade-offs

- Menos código de janela e controles prontos, ao custo de runtime .NET e maior tamanho de distribuição.
- A transparência WPF usa janela layered para o teste por pixel; P1 ainda precisa confirmar o comportamento exato com a janela, o sprite e o Windows alvo.
- Duas janelas continuam necessárias: a do personagem não ativa e recebe clique por pixel; a conversa é separada e ativável para receber teclado.
- WPF pode manter uma renderização ativa se o aplicativo deixar o ciclo visual rodando. A aplicação deve suspender animação e temporizadores sem trabalho e medir isso em P2.
- A interface WPF usa DIP e o mundo do Buzzy mantém coordenadas físicas. A conversão e a mudança de DPI devem ficar concentradas no adaptador e ser verificadas em P6.

### Riscos

| Risco | Mitigação |
|---|---|
| Conflito entre não roubar foco e capturar o mouse: a documentação diz que só a janela em primeiro plano captura plenamente | P3. Uma alternativa que rouba foco não passa o critério; falha exige reabrir a escolha antes da Fase 1 |
| Saltos ou repetição de troca de escala ao mover a janela por código entre monitores de escalas diferentes | Âncora nos pés e histerese na fronteira; P6 |
| A mensagem de mudança de vídeo pode não cobrir todo caso de conexão, rearranjo ou rotação | P5 define quais mensagens chegam e se é preciso reconferir a topologia ao se mover |
| O Windows 11 pode minimizar a janela ao desconectar um monitor, e o Buzzy não tem botão na barra de tarefas | Tratar a minimização e se restaurar sozinho; cenário S8 da Fase 5 |
| Vazamento de memória ou de handles no código de fronteira com o sistema, num app aberto por horas | Módulos de fronteira pequenos e revisados; execução longa de 8 h a 24 h medindo memória privada, handles e objetos gráficos (M7) |
| Mudança incompatível no runtime ou nas dependências .NET | Fixar SDK e dependências, manter lock/restore reproduzível e revisar atualizações antes de adotá-las |
| Asset com sombra ou halo de alfa baixo captura cliques além da silhueta, porque só alfa exatamente 0 é transparente ao clique | Regra de asset saída de P1: nada de área grande com alfa baixo, ou sombra em janela separada |
| Ferramenta de terceiros que tira foto da janela pode derrubar o modo layered | Detectar a falha e religar o estilo; verificado em P8 |
| Sem assinatura, o Windows avisa e pode bloquear o executável | Escolha Q-10. Assinatura para pessoa física fora dos EUA e do Canadá exige certificado pago |
| Sempre no topo é melhor esforço: outras janelas do mesmo tipo podem cobrir o Buzzy, e ele pode atrapalhar jogos em tela cheia | Não reafirmar o estilo em laço; escolha Q-09 com consulta de baixa frequência, validada em P7 |
| A janela WPF pode não cumprir um requisito de input sem código Win32 adicional | Manter esse código no adaptador e concluir P1/P3 antes de iniciar a Fase 1 |

### Portões antes da Fase 1

A escolha da stack está aceita. Estes protótipos, descritos em TODO.md, Etapa 0B, são portões técnicos antes da janela de produto:

| Protótipo | O que decide |
|---|---|
| P1 | Se a janela WPF atravessa um pixel de alfa 0 e recebe clique em pixels visíveis na versão atual do Windows |
| P2 | Consumo WPF em repouso e animando; estabelece a linha de base para Q-08 e precisa mostrar que o app realmente dorme quando parado |
| P3 | Se a janela pode não ativar e ainda completar arraste sem roubar foco; falha exige reabrir decisão da stack antes de Fase 1 |

### Incertezas que permanecem

- Só 84 das 300 alegações decisivas passaram por verificação adversarial, e dois terços dessas 84 precisaram de correção. A taxa de erro encontrada sugere que parte das 216 não verificadas também afirma mais do que a fonte sustenta.

- Nenhuma das nove stacks tem número oficial de consumo de memória, CPU ou GPU no Windows. Todos os números que a pesquisa encontrou são de terceiros, antigos, medidos em outro sistema operacional ou sem metodologia; vários foram corrigidos na verificação. A comparação de consumo é de mecanismo, não de medição.
- O consumo e o esforço real de WPF no Buzzy não são dedutíveis só da documentação; P1 a P3 validam os riscos que podem mudar a escolha.
- A documentação é ambígua sobre capturar o mouse numa janela que não ativa. Só P3 responde.
- Parte dos mecanismos citados depende de fontes antigas ou arquivadas: o funcionamento do teste por pixel e a falha ao tirar foto da janela vêm de um artigo de 2008; os números de CPU de janela layered são da época do Windows XP e Vista; a cobertura de conexão de monitor pela mensagem de mudança de vídeo aparece num guia arquivado.
- Não foram pesquisados: áreas de trabalho virtuais do Windows (Task View), política de certificação da Microsoft Store para aplicativos sempre no topo, comportamento em sessão remota e em máquina sem GPU utilizável.

### Fontes primárias

Fontes oficiais que sustentam os pontos que decidem a comparação, consultadas em 2026-09-26. A pesquisa abriu 192 páginas oficiais distintas; estas são as que pesam na recomendação.

**O requisito central: transparência e clique**

- Window Features, comportamento de janelas layered e teste de acerto por alfa: <https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features>
- `UpdateLayeredWindow`, incluindo a recomendação de manter a janela o menor possível: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-updatelayeredwindow>
- `WM_NCHITTEST` e o alcance de `HTTRANSPARENT`, que não atravessa processos: <https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest>
- Estilos estendidos de janela, incluindo `WS_EX_TRANSPARENT` e `WS_EX_NOACTIVATE`: <https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles>
- Composição por DirectComposition e o teste de acerto uniforme que ela implica: <https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/june/windows-with-c-high-performance-window-layering-using-the-windows-composition-engine>
- Janelas transparentes no WPF, com o mecanismo do teste por pixel e o custo de composição: <https://learn.microsoft.com/en-us/archive/blogs/dwayneneed/transparent-windows-in-wpf>
- Desenho de janela layered com Direct2D, sobre o custo da cópia por quadro: <https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/december/windows-with-c-layered-windows-with-direct2d>

**Input, arraste e foco**

- `SetCapture` e a ressalva de que só a janela em primeiro plano captura plenamente: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcapture>
- `WM_CAPTURECHANGED`, o ponto único de término do arraste: <https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-capturechanged>
- `WM_LBUTTONDOWN` e as coordenadas com sinal em multi-monitor: <https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondown>
- `GetSystemMetrics`, com `SM_CXDRAG` e `SM_CYDRAG`, o limiar de arraste do sistema: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics>
- `WM_ENTERSIZEMOVE`, que mostra por que o arraste pelo laço modal do sistema congela a lógica própria: <https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-entersizemove>
- `WM_MOUSEACTIVATE` e a resposta que entrega o clique sem ativar a janela: <https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-mouseactivate>
- `SetForegroundWindow` e as condições em que o foco é concedido: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow>
- `SetFocus` e a exigência de janela ativável: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setfocus>

**Monitores, DPI e sempre no topo**

- A tela virtual, com origem no monitor principal e coordenadas negativas: <https://learn.microsoft.com/en-us/windows/win32/gdi/the-virtual-screen>
- Desenvolvimento com DPI alto e a recomendação de Per-Monitor V2: <https://learn.microsoft.com/en-us/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows>
- `SetWindowPos` e a ordem Z, incluindo o grupo sempre no topo: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos>
- Por que não existe "mais no topo que os outros no topo": <https://devblogs.microsoft.com/oldnewthing/20110310-00/?p=11253>
- `SHQueryUserNotificationState`, a única forma de saber que há aplicativo em tela cheia: <https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shqueryusernotificationstate>
- Observação de eventos de janela sem injeção de código: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook>

**Consumo em repouso**

- Avaliação oficial de eficiência de energia em repouso, que define o que conta como atividade periódica indevida: <https://learn.microsoft.com/en-us/windows-hardware/test/assessments/results-for-the-idle-energy-efficiency-assessment>

**Bandeja, inicialização e distribuição**

- Estrutura do ícone de bandeja e as implicações de identificá-lo por GUID: <https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataw>
- Chaves `Run` e `RunOnce`, para iniciar com o Windows sem pacote: <https://learn.microsoft.com/en-us/windows/win32/setupapi/run-and-runonce-registry-keys>
- `StartupTask`, a alternativa com pacote MSIX: <https://learn.microsoft.com/en-us/uwp/api/windows.applicationmodel.startuptask>
- Caminhos de distribuição e o que cada um exige: <https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/choose-distribution-path>
- Opções de assinatura de código, com as restrições por país e tipo de entidade: <https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options>
- Reputação no SmartScreen e o fim da liberação imediata por certificado estendido: <https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation>

**Documentação das stacks avaliadas**

- Hooks globais de teclado e mouse, sem exigência de permissão: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw>
- Input bruto em segundo plano, também sem exigência de permissão: <https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawinputdevice>
- Capacidades declaradas por aplicativos empacotados, e por que elas não restringem um aplicativo de confiança total: <https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/app-capability-declarations>
- Avalonia, ausência de click-through como no WPF: <https://docs.avaloniaui.net/docs/guides/platforms/windows/>
- Tauri, opção de ignorar eventos do cursor, que vale para a janela inteira: <https://v2.tauri.app/reference/javascript/api/namespacewindow/>
- Tauri, distribuição e instalador no Windows: <https://v2.tauri.app/distribute/windows-installer/>
- Flutter, pedido aberto de janela transparente no Windows: <https://github.com/flutter/flutter/issues/71735>
- Flutter, plataformas suportadas: <https://docs.flutter.dev/reference/supported-platforms>
- Godot, servidor de exibição, com as opções de transparência e de passagem do mouse: <https://docs.godotengine.org/en/stable/classes/class_displayserver.html>
- WinUI 3, camada visual e o que não suporta transparência: <https://learn.microsoft.com/en-us/windows/apps/develop/composition/visual-layer>

- Electron, limitações da janela transparente: <https://www.electronjs.org/docs/latest/tutorial/custom-window-styles>
- Electron, interações e click-through: <https://www.electronjs.org/docs/latest/tutorial/custom-window-interactions>
- Electron, módulo de monitores: <https://www.electronjs.org/docs/latest/api/screen>
- Qt 6, exemplo oficial de janela recortada, que documenta o clique atravessando o pixel não pintado: <https://doc.qt.io/qt-6/qtwidgets-widgets-shapedclock-example.html>
- Qt 6, distribuição no Windows: <https://doc.qt.io/qt-6/windows-deployment.html>
- Windows App SDK, canais e ciclo de suporte: <https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/release-channels>
- WebView2, distribuição do runtime e o caso do aplicativo aberto por dias: <https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution>
- .NET, modelos de publicação: <https://learn.microsoft.com/en-us/dotnet/core/deploying/>
- .NET, incompatibilidades de corte de código e compilação nativa: <https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/incompatibilities>

**Leituras de código-fonte**

A afirmação de que WPF e Qt caem no mesmo caminho de janela do Win32, e de que as demais não caem, veio de ler o código de cada uma. Os arquivos:

- WPF, apresentação de janela layered: <https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/d3ddevice.cpp>
- Qt, criação e estilos da janela no Windows: <https://github.com/qt/qtbase/blob/dev/src/plugins/platforms/windows/qwindowswindow.cpp>
- Qt, caminho de renderização com Direct3D usado por QML: <https://github.com/qt/qtbase/blob/dev/src/gui/rhi/qrhid3d11.cpp>
- Qt, laço de eventos e timers, que sustenta a afirmação sobre repouso: <https://github.com/qt/qtbase/blob/dev/src/corelib/kernel/qeventdispatcher_win.cpp>
- Qt, captura automática do mouse: <https://github.com/qt/qtbase/blob/dev/src/plugins/platforms/windows/qwindowspointerhandler.cpp>
- Qt, correção de repintura de janela translúcida após mudança de DPI, presente só no ramo de desenvolvimento: <https://github.com/qt/qtbase/commit/b53d8305cc8065e5d5fd7f03ff7d673d6f932522>
- Tauri, criação da janela transparente no Windows: <https://github.com/tauri-apps/tao/blob/dev/src/platform_impl/windows/window.rs>
- Tauri, estilos de janela, incluindo o que ignora o cursor: <https://github.com/tauri-apps/tao/blob/dev/src/platform_impl/windows/window_state.rs>
- Tauri, laço de eventos e saída no fim de sessão: <https://github.com/tauri-apps/tao/blob/dev/src/platform_impl/windows/event_loop.rs>
- Tauri, hospedagem do motor web: <https://github.com/tauri-apps/wry/blob/dev/src/webview2/mod.rs>
- Avalonia, conexão com o compositor do sistema: <https://github.com/AvaloniaUI/Avalonia/blob/11.3.22/src/Windows/Avalonia.Win32/WinRT/Composition/WinUiCompositorConnection.cs>
- Avalonia, correção do laço de renderização em repouso, presente só na linha 12: <https://github.com/AvaloniaUI/Avalonia/pull/20873>
- Godot, servidor de exibição no Windows, com a chamada que eleva a resolução do timer: <https://github.com/godotengine/godot/blob/4.7/platform/windows/display_server_windows.cpp>
- Electron, implementação do click-through com hook global: <https://github.com/electron/electron/blob/main/shell/browser/native_window_views_win.cc>
- Electron, manifesto de consciência de DPI: <https://github.com/electron/electron/blob/main/shell/browser/resources/win/dpi_aware.manifest>

**Números de tamanho e de manutenção**

- Electron, tamanho do runtime publicado: <https://github.com/electron/electron/releases/tag/v44.4.5>
- .NET, metadados de versão usados para o tamanho do runtime instalado: <https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json>
- Godot, tamanho do binário oficial: <https://godotengine.org/download/windows/>
- Qt, lista oficial de vulnerabilidades conhecidas: <https://wiki.qt.io/List_of_known_vulnerabilities_in_Qt_products>

Os endereços de código apontam para o ramo ou a versão lidos em 2026-09-26. Como ramos mudam, uma releitura futura pode não encontrar o mesmo trecho.

Além dessas, foram consultadas as notas de versão e os registros de problemas abertos de cada stack, citados ao longo de DEC-006.

### Consequências

- A stack WPF/.NET 10 foi aceita por delegação explícita. P1 a P3 ainda precisam passar antes da janela de produto.
- ARCHITECTURE.md 2.13 e SECURITY.md 4 passam a descrever WPF como stack escolhida; todo comportamento ainda não executado permanece PLANNED.
- A regra "nenhum overlay do tamanho da tela" vale para qualquer stack escolhida.
- O portão de APIs proibidas do build ganha um motivo extra: ele é o que impede que o contorno por hook global entre no produto por acidente.

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
- **Trade-offs:** segundo a documentação de `SetCapture`, só a janela em primeiro plano captura o mouse plenamente. Uma janela que não ativa pode perder o arraste em movimentos rápidos. P3 testa isso; se não houver solução que preserve o foco, o requisito precisa voltar ao usuário antes da Fase 1.
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

- **Metas:** a Microsoft não publica meta de memória nem de CPU para aplicativos residentes. Publica, porém, critérios de comportamento em repouso, na avaliação oficial de eficiência de energia em repouso (fonte na seção "Fontes primárias" de DEC-006, verificada em 2026-09-26). Esses critérios valem para o sistema inteiro numa janela de 10 minutos, não por aplicativo, mas dão limites defensáveis que o Buzzy não deve violar sozinho:

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

## Decisões de produto registradas pelo usuário

Em 2026-09-26 o usuário respondeu às escolhas abaixo em `docs/DECISOES_DO_USUARIO.md`. Elas estão aceitas como escopo planejado; ainda não significam que qualquer comportamento esteja implementado ou verificado.

| ID | Decisão aceita | Fase/efeito | STATUS |
|---|---|---|---|
| Q-02 | Apoiar Windows 11; Windows 11 24H2 ou posterior é o alvo inicial de teste. Windows 10 não faz parte do alvo. | Define a matriz de aceitação do MVP. | PLANNED |
| Q-03 | Sempre no topo ligado por padrão e desligável; ícone e menu na bandeja; sem botão na barra de tarefas; minimizar esconde e a bandeja restaura; escala em passos fixos; sem opacidade no MVP; botão direito abre o mesmo menu; segunda instância revela a existente. | Fase 1 e configurações da Fase 8. | PLANNED |
| Q-04 | Oferecer iniciar com o Windows, desligado por padrão e ativado apenas pelo usuário. | Fase 8; mecanismo técnico segue a forma de pacote escolhida para cada distribuição. | PLANNED |
| Q-05 | No MVP, superfícies apenas nas bordas das áreas úteis dos monitores; janelas de outros aplicativos ficam fora. Atravessar monitores fica ligado por padrão e pode ser desligado. | Fase 4; janelas de outros aplicativos ficam para depois do MVP. | PLANNED |
| Q-06 | Sem atalhos de teclado para controlar o personagem no MVP. Isso não remove a navegação por teclado da conversa e das configurações prevista em Q-20. | Fase 7. | PLANNED |
| Q-07 | Clique reage; clique duplo abre a conversa; botão direito abre o menu. | Fases 3 e 7. | PLANNED |
| Q-09 | Esconder ou deixar o Buzzy quieto enquanto outro aplicativo estiver em tela cheia, desde que P7 confirme custo desprezível. Se P7 não confirmar, voltar ao usuário antes de ampliar a implementação. | Fase 8, condicionado a P7. | PLANNED |
| Q-10 | Para uso pessoal e testes, gerar primeiro um ZIP portátil, sem assinatura e sem instalador. O projeto é pessoal; a publicação futura, possivelmente no Git, e o formato/assinatura para ela ficam para decidir antes de uma distribuição pública. O modo de empacotar o runtime .NET será definido no plano do build. | ZIP libera o formato do primeiro build; distribuição pública não bloqueia a Fase 1. | PLANNED; publicação pública UNCERTAIN |
| Q-11 | Manter a ordem: Fase 10 verifica o MVP; Fase 11 otimiza e repete a regressão. | Fases 10 e 11. | PLANNED |
| Q-12 | Apenas português do Brasil no MVP; manter textos fora do código. | Fase 7; outros idiomas ficam para depois. | PLANNED |
| Q-14 | Mouse e touchpad no MVP; toque e caneta ficam para depois. | Fases 3 e 4. | PLANNED |
| Q-20 | Caixa de conversa e configurações navegáveis por teclado e utilizáveis por leitor de tela; janela do personagem não é alvo de leitor de tela. | Fases 7 e 8. | PLANNED |
| Q-21 | Não incluir modo fantasma (click-through total) no MVP. | Fase 8; evita deixar o personagem inacessível. | PLANNED |

### Decisões que continuam pendentes

| ID | Pendência | Como resolver | STATUS |
|---|---|---|---|
| Q-08 | Metas numéricas de desempenho para M1–M7. | Medir P2; o Codex apresenta uma recomendação baseada nos resultados para o usuário aceitar ou ajustar antes das fases cujos critérios dependem delas. | UNCERTAIN |
| Q-10 (distribuição pública) | Se e como publicar uma versão para outras pessoas, possivelmente pelo Git, e se ela deve ser assinada. | Reabrir antes da primeira distribuição pública. O ZIP sem assinatura aceito pelo usuário é para uso pessoal e testes. | UNCERTAIN |

### Escolhas já resolvidas

**Q-15 — RESOLVIDA em 2026-09-26 (processo, sem impacto no produto):** o usuário enviou o pedido inicial ao Fable pelo terminal e limpou `prompt_usuario.md` depois de copiar o texto. Não é necessário restaurar aquele pedido. O arquivo será usado como rascunho do próximo prompt e pode ser limpo após o envio; o prompt mestre continua sendo a instrução operacional canônica.

**Q-01 — RESOLVIDA em 2026-09-26:** o usuário delegou ao Codex a escolha da stack e dos próximos passos. DEC-006 escolhe WPF com C# e .NET 10 LTS. P1, P2 e P3 continuam como portões técnicos antes da Fase 1; a aprovação da stack não declara esses testes passados.

**Q-13 — RESOLVIDA em 2026-09-26:** fazer os protótipos descartáveis antes da Fase 1, na ordem P1 (clique por pixel), P3 (arraste sem roubo de foco) e P2 (medição de desempenho), usando WPF. Nenhum protótipo foi executado ainda.

**Q-22 — RESOLVIDA em 2026-09-26:** o teto de tempo para P9 não se aplica porque a interface nativa Win32 deixou de ser o caminho escolhido; P9 foi encerrado sem execução.

### Escolhas levantadas pela referência visual

Em 2026-09-26 o usuário esclareceu que as duas pranchas em `assets/references/` servem de referência para Claude criar uma proposta melhor e original. Elas não fixam elementos individuais do design. Q-16 a Q-19 ficam esclarecidas como referências, não escolhas independentes que bloqueiam o planejamento. STATUS: CLARIFIED; nenhuma pose, nome, acessório ou estilo mostrado nas referências foi aprovado automaticamente.

| ID | Escolha | Situação observada | Recomendação | Bloqueia |
|---|---|---|---|---|
| Q-16 | Nome do personagem | Uma prancha usa o título "PIXEL"; o produto está definido como Buzzy. | Referência visual não altera o nome. Manter Buzzy; só mudar com decisão explícita do usuário. | Nenhuma; esclarecida |
| Q-17 | Acessórios e semelhança visual | Uma prancha mostra chapéu de palha com faixa vermelha, combinação que pode lembrar personagem conhecido. | Claude deve propor um design claramente original e pode reinterpretar ou omitir acessórios das referências. O usuário aprova o conceito antes da arte final. | Nenhuma; esclarecida |
| Q-18 | Poses e superfícies | As pranchas mostram poses em superfícies que não estão identificadas. | São referências, não ampliação do escopo. Vale a decisão de superfícies Q-05; não adicionar janelas de outros aplicativos sem decisão explícita. | Nenhuma; esclarecida |
| Q-19 | Estilo e borda do sprite | As pranchas exploram estilos visuais diferentes; o funcionamento do clique transparente continua sujeito ao protótipo P1. | Claude pode propor uma direção visual nova. O requisito técnico de alfa/clique de P1 continua independente da referência estética. | Nenhuma; esclarecida |

As pranchas também mostram uma pose de "pendurado", que não existe na máquina de estados da seção 2.6 de ARCHITECTURE.md. Se ela for confirmada como requisito, entra como estado novo. Os quadros rotulados "interagindo com cursor" não abrem escolha: PRODUCT_SPEC.md já registra que o cursor desenhado na prancha não pertence ao personagem nem ao produto, e a arquitetura já prevê que o Buzzy só lê o mouse nas próprias janelas.

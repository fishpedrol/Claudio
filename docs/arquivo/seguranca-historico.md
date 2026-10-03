> **Arquivo morto.** Cópia integral de `docs/SECURITY.md` como estava em 2026-10-02, antes da limpeza da documentação, com as notas de implementação datadas e o relato completo de cada verificação (seção 10). É só registro histórico, não é leitura obrigatória; o modelo vigente está em [SECURITY.md](../SECURITY.md).

# Histórico da segurança — arquivo morto

> Regras de segurança do produto. As Fases 1 a 4 estão implementadas e verificadas nos limites descritos na seção 10; a Fase 5 está em andamento, intercalada com a emoção dominante e o tamagotchi adulto (DEC-027 e DEC-028). O modelo completo de segurança continua PLANNED até a Fase 9.
>
> Última atualização: 2026-10-02

## 1. Modelo de segurança

STATUS: PLANNED. Os princípios vêm de DEC-005 e de [PRODUCT_SPEC.md](../PRODUCT_SPEC.md).

O Buzzy é um aplicativo local, de um usuário, sem privilégio de administrador e sem rede. Ele só enxerga o próprio input: cliques e teclas dirigidos às suas janelas. Ele só grava na própria pasta de dados. Toda capacidade do sistema operacional segue a regra:

intenção explícita → capacidade específica → permissão limitada → ação permitida e auditável.

**Fato que molda o modelo:** um processo desktop comum do Windows, rodando como o usuário, pode instalar hooks globais de teclado e mouse, ler input em segundo plano e capturar a tela sem pedir permissão e sem mostrar indicador ao usuário. Fontes: documentação da Microsoft sobre `SetWindowsHookEx` e `RAWINPUTDEVICE`; consulte as fontes técnicas em DEC-006. Por isso, a garantia de que o Buzzy não é keylogger nem captura a tela não vem do Windows. Ela vem do próprio projeto: APIs proibidas, verificação automática no build e documentação pública.

## 2. Permissões

STATUS: PLANNED.

| Necessidade | Como | Permissão do sistema | Quando |
|---|---|---|---|
| Executar | Processo comum, manifesto `asInvoker`, sem elevação | Nenhuma além do usuário | Sempre |
| Janela transparente sobre o desktop | Janela sem borda, do tamanho do sprite, com transparência por pixel | Nenhuma | Fase 1 |
| Sempre no topo | Estilo topmost aplicado uma vez, nunca reafirmado por timer. As janelas dos itens do tamagotchi só mudam de lugar na ordem Z por evento: o item aparecer, o gesto sobre ele e o personagem reaparecer (DEC-028). A releitura da topologia e a conferência tardia dela, um disparo único, devolvem só o lugar das janelas, sem reafirmar a ordem Z (DEC-030) | Nenhuma | Q-03 |
| Receber clique e arraste | Mensagens de mouse das próprias janelas (o personagem e os itens do tamagotchi) e captura do mouse durante o gesto | Nenhuma | Fase 3; itens no passo T8 |
| Ajustar energia | Controle com três valores permitidos (`BAIXA`, `MEDIA`, `ALTA`) no painel do mascote e nas configurações | Nenhuma | Fase 8 |
| Monitores, DPI e área útil | Leitura de topologia e mensagens de mudança; a configuração de vídeo, só lida, para a chave estável do monitor | Nenhuma | Fase 1; chave no passo P6 da Fase 5 |
| Ícone na bandeja | `Shell_NotifyIcon` identificado por janela e ID, sem GUID | Nenhuma | Q-03 |
| Gravar configurações | Arquivo na pasta local do usuário | Acesso do próprio usuário | Fase 5 (posição, postura e emoção dominante, desde o passo P7) e Fase 8 |
| Iniciar com o Windows | Opcional, desligado por padrão, ativado pelo usuário | Chave `Run` do usuário, atalho na pasta Inicializar ou `StartupTask` em MSIX | Q-04 |
| Mover o mascote para fora de tela cheia | Eventos WinEvent limitados; leitura transitória apenas do retângulo e monitor da janela de primeiro plano | Nenhuma; sem elevação ou processo auxiliar | DEC-013/Q-09 |

Nenhum item exige administrador, rede, acesso a arquivos do usuário fora da pasta do Buzzy, identificação de processos ou leitura do conteúdo de outras janelas. DEC-013 é a única exceção de observação: a geometria da janela em primeiro plano é consultada em memória para escolher um monitor e descartada imediatamente.

Os riscos da stack WPF selecionada estão resumidos na seção 4.

## 3. Capacidades

### 3.1 Permitidas

STATUS: PLANNED.

- Criar, mover, mostrar e esconder as próprias janelas, inclusive uma janela por item do tamagotchi, que só a raiz fecha (DEC-028).
- Ler posição do cursor e botões apenas nas mensagens entregues às próprias janelas, ou durante a captura de um arraste iniciado pelo usuário. *Implementação (Fase 3, DEC-021):* `SetCapture` só depois de um botão pressionado sobre um pixel visível do Buzzy, e `ReleaseCapture` no fim do gesto. `WM_MOUSEMOVE` só é tratado enquanto a janela tem a captura. A perda da captura (`WM_CAPTURECHANGED`, `WM_CANCELMODE`) encerra o gesto. O limiar de arraste e as regras de clique duplo são lidos do sistema (`GetSystemMetricsForDpi`, `GetDoubleClickTime`) sem alterar nada. `GetWindowRect` só é chamado sobre a janela do próprio Buzzy. *Tamagotchi (DEC-028, passo T8):* as janelas dos itens seguem as mesmas regras, com um árbitro de gestos próprio: a captura só existe durante um gesto começado num pixel visível do item, e a janela nunca é ativada. Quando o núcleo encerra esse gesto por conta própria (esconder, minimizar, bloquear a sessão, suspender, sair, recolher os itens ou pegar outro), a janela solta a captura na hora, e um soltar tardio não vira nada. Na perda da captura, só o fato conta, nunca qual janela ficou com o mouse.
- Montar o menu nativo com ícones (DEC-027 e DEC-028, passos T2 e T8): `InsertMenuItemW` e bitmaps de 32 bits criados só na memória do próprio Buzzy, por `CreateDIBSection` sem DC e preenchidos por `Marshal.Copy`, sem `BitBlt`, `StretchBlt` nem `CreateDC`. Os bitmaps de cada abertura são apagados com `DeleteObject` depois de o menu ser destruído, inclusive quando a exibição lança; o log confere criados = apagados, e uma falha vai para ele só com o código.
- Ler geometria, escala e orientação dos monitores e a área útil de cada um. *Implementação da chave estável (passo P6 da Fase 5; DEC-030):* a configuração de vídeo é só lida (`GetDisplayConfigBufferSizes`, `QueryDisplayConfig` com os caminhos ativos e `DisplayConfigGetDeviceInfo`, para o nome GDI da fonte e o caminho do dispositivo do alvo). O caminho, que identifica o hardware, vira na hora um resumo de 8 bytes do SHA-256 e não sai do adaptador: nunca vai para o arquivo, o log ou o texto automático de um registro. O nome amigável do monitor e os códigos do EDID nunca são lidos, e um teste de fonte confere esses limites. Nada muda a configuração de vídeo (seção 3.2).
- Observar eventos de mudança de janela em primeiro plano e de geometria via `SetWinEventHook` em modo out-of-context, filtrados para a janela de nível superior ativa; ler somente `GetWindowRect` e o monitor associado. Converter em uma lista transitória dos monitores ocupados e descartar HWND/retângulo após o cálculo. Nunca ler título, nome, caminho de processo, texto, pixels ou conteúdo; nunca enumerar janelas/processos; nunca persistir ou registrar esses dados. (DEC-013) *Planejado (DEC-026):* a curiosidade pedida pelo usuário usa os mesmos dados e os mesmos limites: o monitor e o retângulo da janela ativa, transitórios, e há quanto tempo ela está em primeiro plano, sem identidade, título, processo ou conteúdo.
- Receber mensagens de sessão, energia, bloqueio e encerramento.
- Ler os próprios assets, o manifesto de assets e o perfil local de comportamento, somente leitura.
- Ler e gravar os arquivos da própria pasta de dados. *Implementação (Fase 5; DEC-029), usada pelo app desde o passo P7:*
  - as configurações só usam os nomes fixos `settings.json`, `settings.json.bak`, `settings.json.tmp` e `settings.corrupt.json`, numa pasta escolhida por uma regra única, com falha fechada (ARCHITECTURE.md 2.12). Sem perfil de teste, o Buzzy do usuário lê o `settings.json` real na partida e o grava pela agenda: pouco depois de soltá-lo, escondê-lo ou escolher uma emoção, e na hora ao sair ou no fim da sessão;
  - os perfis de teste (`--perfil-de-teste NOME`) ficam em `%LOCALAPPDATA%\Buzzy\testes\NOME`, sempre filha direta de `testes`. O nome tem de 1 a 32 caracteres, só a–z, 0–9 e hífen, sem hífen inicial, e não pode ser um nome reservado do Windows (`con`, `prn`, `aux`, `nul`, `com0`–`com9`, `lpt0`–`lpt9`). Maiúsculas são recusadas, não convertidas;
  - nome inválido, opção sem nome ou opção escrita de outro jeito (`--perfil-de-teste=NOME`, outra caixa, `/perfil-de-teste`) desligam a persistência naquela execução, em vez de cair na pasta real;
  - a limpeza de um perfil antes de cada uso confere, antes de tudo, cada trecho do caminho que existe (a pasta do Buzzy, `testes` e a do perfil), também quando a pasta do perfil ainda não existe, e recusa junção ou link: com a pasta do Buzzy ou a dos testes como junção, o Buzzy de teste e o próprio teste gravariam do outro lado. Vale nos testes de integração, na verificação de tela, que usa a mesma limpeza, e na medição de desempenho.

### 3.2 Proibidas no MVP

STATUS: PLANNED. Cada item vira regra de verificação automática na Fase 1 (seção 8).

| Capacidade proibida | Exemplos de API ou recurso |
|---|---|
| Input global | `SetWindowsHookEx` para teclado/mouse, `RegisterRawInputDevices` com `RIDEV_INPUTSINK` ou `RIDEV_EXINPUTSINK`, `GetAsyncKeyState`, `GetKeyboardState` usado para observar teclado fora do foco, `RegisterHotKey` sem decisão aprovada. O evento WinEvent limitado de DEC-013 não recebe input e só é permitido conforme a seção 3.1. |
| Injetar input | `SendInput`, `mouse_event`, `keybd_event` no produto; permitido apenas em ferramentas de teste fora do executável |
| Captura de tela | `BitBlt` ou `PrintWindow` sobre o desktop ou janelas de outros processos, Windows Graphics Capture, Desktop Duplication |
| Processos | `CreateProcess`, `ShellExecute(Ex)`, `WinExec`, execução de shell, PowerShell ou CMD |
| Rede | Sockets, WinHTTP, WinINet, `URLDownloadToFile`, `fetch` ou equivalentes, qualquer conteúdo remoto no WebView |
| Ler outros aplicativos | Área de transferência, títulos, nomes, caminhos de processos, texto ou conteúdo de outras janelas, UI Automation sobre outros processos, memória de outros processos ou `EnumWindows`. A única exceção é consultar temporariamente o retângulo/monitor da janela ativa conforme DEC-013. |
| Persistência escondida | Serviço, tarefa agendada, chave `Run` criada sem ação do usuário, cópia do executável para outros locais |
| Código dinâmico | `eval`, carregamento de scripts, DLLs ou assets de caminhos fornecidos pelo usuário |
| Alterar configuração global | `SetDisplayConfig`, `DisplayConfigSetDeviceInfo`, `ChangeDisplaySettings` e `ChangeDisplaySettingsEx`, com as variantes A e W, que mudam a topologia, a resolução, a orientação, a escala ou o modo de vídeo do sistema todo. Só o usuário muda as configurações do Windows (AGENTS.md); a chave estável do monitor só lê a configuração de vídeo (seção 3.1). Regra do portão desde a revisão do bloco B da Fase 5 (2026-10-01) |

**Exceção do apphost (DEC-016).** O `Buzzy.exe` é o lançador nativo genérico do SDK do .NET, não código do Buzzy: localiza o runtime instalado, carrega `hostfxr.dll` e entrega a execução ao `Buzzy.dll`. Ele importa `LoadLibraryExW`, `LoadLibraryA` e `GetProcAddress` para carregar o runtime, e `ShellExecuteW` para abrir a página de download do .NET quando o runtime falta e o usuário aceita. O portão permite essas quatro importações só nesse arquivo nativo, por nome exato e com o motivo no relatório; qualquer outra importação proibida, ou qualquer uma delas no `Buzzy.dll` e nas demais DLLs, reprova o build.

**Ferramentas de teste (regra registrada com o tamagotchi, DEC-028).** Os testes de integração e a medição de desempenho (`-Modo onda`) só postam mensagens às janelas do Buzzy que eles mesmos abriram, com o PID conferido antes de cada uma, sem `SendInput` e sem mover o cursor; o `SendInput` fica com a verificação de tela, que é input SINTÉTICO e avisa o usuário antes. As consultas só de teste (`GetGuiResources`, `GetGUIThreadInfo` da thread do Buzzy, `GetMenuItemInfoW`, `GetDIBits` e outras) ficam fora do executável e leem objetos do próprio Buzzy ou do próprio teste. Ao conferir a ordem Z, a varredura das janelas abaixo do personagem para na primeira janela de outro processo, da qual lê só o PID. Nos testes da janela intrusa do tamagotchi, a conferência de que uma janela está acima de outra sobe pela ordem Z comparando só handles, sem ler nada das janelas do caminho, que podem ser de outros aplicativos; as duas comparadas são do teste ou do Buzzy que ele abriu.

*Desde a revisão do bloco B da Fase 5 (2026-10-01):*
- nos testes de integração, toda mensagem postada ou enviada às janelas do Buzzy, e todo `SetWindowPos` ou minimização "por fora", passa pelas portas de `BuzzyEmTeste` (`Postar`, `Enviar`, `MoverPorFora` e `MinimizarPorFora`, além de `FecharPorWmClose` e do `Dispose`), que conferem o PID imediatamente antes. Um teste de fonte proíbe as chamadas diretas fora delas;
- a integração, a verificação de tela e a medição de desempenho tiram uma foto dos arquivos reais de configuração do usuário antes e depois, só por fora (existência, tamanho e datas de criação e de escrita, nunca o conteúdo), e falham se ela mudar: a integração e a verificação de tela com código 1, e a medição com `exit 1`.

## 4. Stack selecionada e riscos de distribuição

STATUS: PLANNED. DEC-006 selecionou WPF, C# e .NET 10; P1/P2 foram aceitos nos limites documentados e P3 passou como gate técnico no ambiente medido, com input sintético. A seleção não valida por si só o aplicativo.

- WPF executa em um processo do usuário e não exige administrador, motor web ou rede própria. O runtime .NET continua sendo dependência externa: manter versão suportada, fixar o SDK e auditar as dependências do aplicativo.
- O portão de segurança deve inspecionar binário e código quanto às capacidades proibidas da seção 3.2. Um desktop comum roda com os direitos do usuário; MSIX comum não é sandbox e não substitui esse portão.
- O primeiro pacote será ZIP portátil sem assinatura para uso pessoal e testes (Q-10). Avisos ou bloqueios do Windows são possíveis. Reavaliar formato, assinatura e instruções antes de qualquer distribuição a terceiros; nenhuma distribuição está autorizada.
## 5. Dados armazenados

STATUS: PLANNED. Esquema proposto em [ARCHITECTURE.md](../ARCHITECTURE.md), seção 2.12, e em DEC-010.

| Dado | Onde | Por quê |
|---|---|---|
| Posição do personagem (chave do monitor, tela desse monitor na época, posição relativa e absoluta) | `settings.json` | Restaurar onde o usuário deixou. Desde o passo P7 da Fase 5, também a borda do esconderijo (`nenhum`, `baixo`, `esquerda` ou `direita`) e a marca "preso pelo usuário", um booleano (esquema v3, DEC-029). A chave é um resumo opaco do caminho do dispositivo (`mon:` e 16 dígitos hexadecimais), nunca o caminho; quando o caminho não pode ser lido, a reserva `gdi:` seguida do nome GDI (DEC-030) |
| Escala, sempre no topo, iniciar com o Windows, nível de energia (`BAIXA`/`MEDIA`/`ALTA`), modo de tela cheia ligado/desligado, atravessar monitores, emoção dominante, idioma | `settings.json` | Preferências do usuário; opacidade fica fora do MVP. A posição temporária do modo de tela cheia, os monitores ocupados e o cache das chaves dos monitores **não** são gravados. O esquema v3 guarda só a energia, o modo de tela cheia, atravessar monitores e a emoção dominante: `"automatica"` ou o nome de uma das 14 caras de humor (DEC-027) |
| Última cópia boa e, no máximo, uma cópia ilegível para diagnóstico | `settings.json.bak`, `settings.corrupt.json` | Recuperação. A leitura usa o principal, depois o `.bak`, depois os padrões; a cópia de diagnóstico nunca é lida |
| Temporário de uma gravação | `settings.json.tmp` | Gravação atômica; apagado e recriado a cada gravação, nunca lido |
| Dados dos perfis de teste | `testes\NOME\`, com os mesmos nomes de arquivo | Isolar testes e ferramentas das configurações reais; a pasta é apagada antes de cada uso, recusando junção ou link no caminho, conferido antes de tudo (seção 3.1) |

Local: `%LOCALAPPDATA%\Buzzy` sem pacote, obtido pela API de pastas conhecidas; pasta local do pacote com MSIX. Nenhum segredo é armazenado, por isso DPAPI não é usado. Logs de diagnóstico, se existirem, ficam na mesma pasta e têm tamanho limitado. Um arquivo de versão futura ou um principal inacessível na partida bloqueiam a gravação nesta execução, para não sobrescrever o que não se conhece nem se conseguiu ler.

Desde o passo P7 da Fase 5, o Buzzy do usuário, aberto sem perfil de teste, lê e grava o `settings.json` real e o `.bak` nessa pasta; os testes e as ferramentas usam a pasta do perfil deles, em `testes\NOME`. O arquivo tem cerca de 500 bytes.

Os itens do tamagotchi, o uso de um item e a onda de desenho animado (DEC-028) ficam só em memória: nada deles vai para o `settings.json` nem para outro arquivo, fora as linhas do log de diagnóstico opcional (seção 6), e tudo some ao sair do app. Desde 2026-10-01, o mesmo vale para a paranoia: a carga do episódio (as substâncias, a sintética, os itens distintos e o sorteio feito) e o gerador próprio dela ficam só em memória e recomeçam a cada abertura, a partir da semente do núcleo. Os dados novos gravados desde o passo P7 são a emoção dominante e a postura (a borda do esconderijo e a marca de preso).

## 6. Dados que nunca devem ser coletados

STATUS: PLANNED.

- Teclas digitadas em outros aplicativos ou fora dos controles próprios do Buzzy.
- Conteúdo da tela, de outras janelas ou da área de transferência.
- Títulos, nomes ou lista de outros aplicativos e janelas.
- Geometria ou identificador da janela em primeiro plano. A única leitura permitida é transitória, em memória, durante a avaliação imediata de DEC-013; o retângulo e o identificador não são guardados, registrados nem transmitidos.
- Arquivos do usuário.
- Identificadores do usuário ou da máquina: nome, e-mail, conta, número de série, endereço de rede.
- Localização, microfone, câmera, histórico de navegação.
- Qualquer dado enviado para fora da máquina. O MVP não tem rede.

*Implementação no log de diagnóstico (Fase 5, bloco A; DEC-029):*
- erros do sistema vão só com o tipo e o código da exceção, nunca com a mensagem, que pode trazer um caminho com o nome do usuário ou o SID da conta. Isso vale para a gravação das configurações, para `INSTANCIA` e para `ERRO`; um teste de fonte lista as duas mensagens que ainda vão para o log, ambas de exceções do próprio Buzzy;
- os avisos da leitura do `settings.json` só levam nomes de campo do esquema e o motivo, nunca valores nem nomes vindos do arquivo;
- o nome de perfil recusado e a chave lida do arquivo não vão para o log, e o caminho da pasta de dados nunca vai.

*No tamagotchi e no menu (DEC-027 e DEC-028; contrato em ARCHITECTURE.md 2.13.4):* as linhas `MENU`, `ITEM` e `ONDA` levam só nomes de comandos, de caras e de itens, Ids, pontos e retângulos das janelas do próprio Buzzy, o DPI e o HWND delas, contagens e tempos. Na perda da captura, não vai qual janela ficou com o mouse, e uma falha do Windows vai só com o código. A linha `PARANOIA` (desde 2026-10-01) leva só o nome do item, a chance, `sim` ou `nao` e as contagens do episódio, com os nomes dos itens distintos.

*Na Fase 5, bloco B (passos P6, P7 e P9; DEC-029 e DEC-030; contrato em ARCHITECTURE.md 2.13.4):*
- as linhas `TOPOLOGIA`, `POSICAO` e `ITEM` levam a chave do monitor, que é o resumo opaco, e o nome GDI (`\\.\DISPLAYn`); nunca o caminho do dispositivo nem o nome do monitor. A falha da consulta da configuração de vídeo ou da leitura de um monitor vai só com a função e o código;
- `MENSAGEM` leva só o tipo da mensagem do Windows (no `WM_DPICHANGED`, também o DPI novo), e uma releitura leva no máximo 32 deles; `POSICAO|reaplicada` e `ITEM|reaplicado` levam só retângulos das janelas do próprio Buzzy;
- as linhas `CONFIG` levam só enums, contagens e tempos: a pasta como `perfil` ou `padrao` e a versão do arquivo como `atual`, `anterior`, `futura` ou `-`, nunca o número lido, a pasta, a chave, a tela nem outros valores do arquivo; um erro vai com o tipo e o código.

## 7. Superfícies de ataque relevantes

STATUS: PLANNED.

| Superfície | Risco | Defesa planejada |
|---|---|---|
| Arquivo de configurações | Arquivo adulterado ou corrompido trava o app ou causa comportamento inesperado | Tamanho máximo, esquema validado, valores presos a faixas, padrões em caso de erro. *Implementado no bloco A da Fase 5 (DEC-029; regras em ARCHITECTURE.md 2.12):* tamanho conferido antes de ler (64 KiB, contando o BOM); UTF-8 validado; JSON lido sem reflexão, com profundidade máxima de 8; tolerância campo a campo, com valores presos a faixas; energia só pelos três nomes, e a emoção dominante só pela lista fechada da linha própria, desde o esquema v2; um escape de surrogate solto torna o arquivo inteiro ilegível; leitura principal → `.bak` → padrões; versão futura ou principal inacessível bloqueiam a gravação; temporário recriado para nunca gravar através de um link. *Esquema v3 (passo P7):* a borda do esconderijo só vale pelos quatro nomes, por lista fechada, e a marca de preso só como booleano; as duas só valem junto com uma posição, e a borda, só com o esconderijo pelo clique duplo ligado; o aviso não repete o valor lido. Na partida, qualquer exceção da leitura desliga a persistência nesta execução, em vez de fechar o app, e a leitura do esquema só captura `InvalidOperationException` |
| `--perfil-de-teste` | Um teste ou ferramenta com a opção escrita errado leria e gravaria as configurações reais do usuário | Nome validado caractere a caractere; nome inválido, opção sem nome ou outra grafia da opção desligam a persistência (falha fechada); regra única da pasta, com teste de tabela; testes de fonte exigem o perfil em todo lançador do `Buzzy.exe` (DEC-029) |
| Controle de energia | Valor de configuração adulterado ou fora do conjunto permitido | Enumeração estrita de três níveis, validação no carregamento e valor padrão seguro em caso de erro |
| Emoção dominante (DEC-027) | Valor adulterado no arquivo, ou fora das 14 caras de humor | Lista fechada de 15 nomes no arquivo (`automatica` e as 14 caras), sem diferenciar maiúsculas e nunca por `Enum.Parse`, que aceitaria números, listas e as caras de efeito do tamagotchi. Valor inválido vira a automática, com um aviso que não repete o valor lido. No núcleo, um comando fora das 14 é ignorado, e uma carga ou configuração fora delas vira a automática. No menu, o id escolhido vira a emoção, ou o item do tamagotchi, por listas fixas, nunca por conversão do número, e qualquer outro id não escolhe nada. Implementado e coberto por testes automatizados |
| Evento de janela ativa (DEC-013) | Evento inesperado ou frequência alta pode gerar reposicionamento/custo | Observar somente eventos aprovados; filtrar para a janela ativa e mudanças relevantes; agrupar eventos; descartar metadados; medir em P7 a taxa de chamadas com mouse e teclado em uso, restringindo a assinatura de geometria à janela ativa se a global acordar o Buzzy continuamente; nunca usar loop de polling em repouso |
| Assets, manifesto e perfil de comportamento | Arquivo trocado por outro processo do usuário | Carregados só da pasta de instalação; manifesto validado. Proteção contra troca de binários depende da forma de distribuição (Q-10) |
| Dependências de terceiros | Código vulnerável ou malicioso entrando pelo build | Poucas dependências, versões fixas com lockfile, auditoria automática no build, revisão antes de adicionar |
| Carregamento de DLL | DLL plantada na pasta do app ou no caminho de busca | Instalação em pasta própria e busca de DLL restrita, conforme a stack |
| Distribuição futura | Binário adulterado ou avisos/bloqueios do Windows | Não há distribuição autorizada; se ela for decidida, reavaliar formato, assinatura e integridade antes de publicar |

## 8. Práticas de segurança

STATUS: PLANNED. Cada prática vira item de teste a partir da Fase 1.

1. **Portão de APIs proibidas no build.** Um script inspeciona as importações do executável e das DLLs próprias e falha se encontrar APIs da seção 3.2. O mesmo script procura as chamadas equivalentes no código-fonte da stack escolhida. A exceção `SetWinEventHook` só passa se ficar restrita aos eventos e filtros de DEC-013; hooks de input continuam proibidos. *Implementação (Fase 1, DEC-016):* `tools/Buzzy.PortaoApis` roda depois de cada build de `src/Buzzy.App` e lê as importações nativas, as declarações P/Invoke dos binários gerenciados, o código-fonte de `src/Buzzy.App`, `src/Buzzy.Core` e `src/Buzzy.Visual` e o manifesto (`asInvoker`, Per-Monitor V2). Desde 2026-09-30, o relatório de `tools/testar.ps1` também confere `src/Buzzy.Visual`, como o portão do build já fazia. A única exceção é a do apphost descrita na seção 3.2. A arte do tamagotchi em `src/Buzzy.Visual` (2026-10-01) passou pelo portão de fonte sem nenhum nome da lista proibida, nem em comentários: a ampliação dos ícones do menu é feita em memória, por vizinho mais próximo, sem `BitBlt`, `StretchBlt` nem `CreateDC`. Desde a revisão do bloco B da Fase 5 (2026-10-01), a lista proibida tem a categoria "Alterar configuração global" (seção 3.2), e um teste confere que as três funções de leitura da configuração de vídeo continuam permitidas.
2. **Auditoria de dependências** a cada build, com a ferramenta oficial do ecossistema da stack.
3. **Verificação de rede zero:** durante os testes manuais de cada fase, confirmar que o processo do Buzzy e os processos filhos não abrem conexão. Comando de referência:

   ```powershell
   Get-NetTCPConnection -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   Get-NetUDPEndpoint -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   ```

4. **Verificação de gravação:** com o Process Monitor da Sysinternals, confirmar que o Buzzy só grava na própria pasta de dados. *Pendente [MANUAL] na Fase 5:* com a persistência ligada desde o passo P7, confirmar que as gravações ficam em `%LOCALAPPDATA%\Buzzy` e, nos testes, em `%LOCALAPPDATA%\Buzzy\testes\`. Até lá, a evidência automática é parcial: a integração confere que o Buzzy de teste gravou o arquivo do perfil dele e que a foto dos arquivos reais ficou igual antes e depois (seção 3.2), mas não vê outras pastas.
5. **Sem elevação:** manifesto `asInvoker`; o app recusa rodar elevado ou avisa e continua sem usar o privilégio. O ZIP portátil inicial deve executar sem instalação nem elevação; P10 verifica o comportamento. *Implementação (Fase 1, DEC-016):* o Buzzy recusa rodar elevado — mostra um aviso e sai com código 5.
6. **Gravação atômica** das configurações e validação ao ler. *Implementação (Fase 5, bloco A; DEC-029):* no adaptador; o app a usa desde o passo P7, pela agenda de gravação. Evidência [AUTO]:
   - `Gravar_FalhaSimuladaEmCadaEtapa_NuncaIlegivel`: queda simulada em cada etapa da gravação, a partir de quatro estados iniciais;
   - `Ler_EstadosDeQuedaExaustivos`: o disco em cada estado possível de uma queda, com o temporário cortado em cada byte;
   - `MatarDuranteGravacoes_NuncaDeixaIlegivel`: 25 rodadas matando um processo filho que grava sem parar, aberto pelo próprio teste;
   - `Gravar_TemporarioQueEhLinkParaOutroArquivo_NaoGravaAtravesDele`: com link físico. O link simbólico exige o modo de desenvolvedor, e sem ele esse caso é pulado.
7. **Nenhum segredo** no repositório nem no app.

## 9. Limitações

- O Windows não oferece permissão nem indicador para input global ou captura de tela em apps desktop comuns. O usuário precisa confiar no código publicado e nas verificações do build. Fontes oficiais sobre hooks e input bruto estão ligadas em DEC-006.
- Um pacote MSIX comum roda com confiança total. O manifesto não restringe hooks, rede ou arquivos. O isolamento real exigiria AppContainer, com riscos de compatibilidade ainda não testados.
- Uma instalação por usuário sem MSIX deixa os binários graváveis por qualquer processo do mesmo usuário.
- Sem assinatura de código, o Windows mostra avisos do SmartScreen, e o Smart App Control pode bloquear o app. Essa limitação foi aceita para uso pessoal e testes; assinatura e formato devem ser reavaliados em Q-10 antes de qualquer distribuição pública. Assinatura para pessoa física fora dos EUA e do Canadá exige certificado OV pago.

- Sem atualização automática, que é proibida no MVP, correções de segurança dependem de o usuário instalar a nova versão.
- Queda de energia no meio da gravação das configurações não é testável. A defesa é descarregar o temporário no disco (`Flush(true)`) antes da troca atômica do NTFS; no pior caso volta a versão anterior. `File.Replace` exige NTFS local: noutro sistema de arquivos, a gravação falha sempre (DEC-029).
- Links plantados pelo próprio usuário na pasta do Buzzy não cruzam fronteira de privilégio, mas podem desviar uma gravação. O temporário das configurações é recriado para nunca gravar através de um link, e a limpeza dos perfis de teste recusa junção ou link no caminho. **Aceito:** o `diagnostico.log`, anterior à Fase 5, é aberto para acrescentar e segue um link que já exista; só vale com `--diagnostico`, e só o próprio usuário consegue plantar esse link.

## 10. Verificações realizadas

- 2026-09-26: pesquisa de segurança em fontes oficiais da Microsoft; fontes e limites estão em DEC-006.
- 2026-09-29: `powershell -NoProfile -File tools/testar.ps1` terminou com código 0; build Release sem avisos/erros, 73 testes do portão aprovados, portão binário/fonte aprovado (quatro permissões estritamente no apphost) e auditoria de pacotes sem falhas. O relatório do portão descreve cada permissão.
- 2026-09-29: em `resultados/desempenho-20260929-215550.txt`, a medição de dez minutos observou zero processos filhos em 629 verificações e zero conexões TCP/UDP em 58 verificações; a resolução do timer global não mudou durante a medição. Isso cobre somente a execução medida e não substitui a sessão de uma hora nem os demais itens da Fase 9.
- 2026-09-30, depois das Fases 2 a 4 (inclusive a toon force):
  - `tools/testar.ps1` código 0: portão binário e de fonte APROVADO, com as mesmas quatro permissões do apphost, 73 testes do portão e nenhum pacote vulnerável. As APIs novas da Fase 3 (`SetCapture`, `ReleaseCapture`, `GetDoubleClickTime`, `GetWindowRect` na própria janela) não estão na lista proibida.
  - Duas medições de dez minutos, repouso (`resultados/desempenho-20260930-160012.txt`) e autonomia com movimento (`resultados/desempenho-20260930-161054.txt`): cada uma teve 629 verificações sem processo filho e 59 sem conexão TCP/UDP, e a resolução global do timer não mudou.
- 2026-09-30, Fase 5, bloco A (passos P1–P5):
  - uma revisão de segurança dos passos não achou problema bloqueante. Os achados foram corrigidos: outra grafia de `--perfil-de-teste` caía na pasta real; a escolha da pasta não tinha regra testada; o temporário seguia um link já existente; mensagens de exceção do sistema iam para o log; a varredura dos lançadores era fraca. Os adiados estão em DEC-029 e na seção 9;
  - `tools/testar.ps1` (Release) código 0: portão binário e de fonte APROVADO, com as mesmas quatro permissões do apphost, agora também sobre `src/Buzzy.Visual` no relatório; 73 testes do portão; nenhum pacote vulnerável. A gravação atômica usa só a biblioteca base, sem P/Invoke novo;
  - pendentes: o Process Monitor (seção 8, item 4) depois do passo P7, e a sessão de uma hora sem rede da Fase 9.
- 2026-10-01, emoção dominante e tamagotchi no núcleo e na arte (DEC-027 e DEC-028), com a chave do tamagotchi desligada no aplicativo:
  - `tools/testar.ps1` (Release) código 0 depois da mescla da arte: portão binário e de fonte APROVADO, com as mesmas quatro permissões do apphost, 73 testes do portão e nenhum pacote vulnerável. O núcleo e a arte não chamam o Windows: nenhum P/Invoke novo;
  - a emoção dominante é lida por lista fechada, e o aviso não repete o valor do arquivo (seção 7); os itens, o uso e a onda ficam só em memória (seção 5);
  - a revisão do núcleo não achou informação real sobre drogas em nomes, comentários, testes ou textos, só nomes e efeitos de desenho animado, e os itens da arte não têm texto, marca nem folha (DEC-028);
  - pendentes no app naquele momento: as APIs do menu com os rostos e a captura do mouse nas janelas dos itens (seção 3.1), feitas depois nos passos T2 e T8 (entrada seguinte).
- 2026-10-01, o app da emoção dominante e do tamagotchi (passos T2 e T7–T9), com a chave do tamagotchi ligada no aplicativo, na última rodada da correção do app:
  - `tools/testar.ps1` (Release) código 0: portão binário e de fonte APROVADO, com as mesmas quatro permissões do apphost, 73 testes do portão e nenhum pacote vulnerável. As três APIs novas do menu (`InsertMenuItemW`, `CreateDIBSection` e `DeleteObject`) ficam em `Win32.cs` e não estão na lista proibida; a apresentação, as janelas dos itens e a ligação da chave não trouxeram P/Invoke novo ao produto;
  - integração por mensagens postadas (`tools/testar.ps1 -Integracao`, 271/271): 20 aberturas do menu com os objetos GDI e USER no mesmo patamar e cada abertura apagando os bitmaps que criou; a janela de um item aparece sem ativar e sem tirar o primeiro plano; esconder, ou minimizar, no meio do arraste de um item solta a captura; sair com itens na tela termina com código 0 e nenhuma janela viva;
  - os textos do menu só nomeiam as emoções e os itens, sem descrição, e os itens só existem em memória (seção 5);
  - pendentes naquele momento: a verificação de tela com input SINTÉTICO (`--fase tamagotchi`), que também confere o foco depois dos menus, e o repouso de 10 minutos com uma onda ativa (`tools/medir-desempenho.ps1 -Modo onda`), que também conta processos filhos e conexões, feitos depois (entrada seguinte); e a revisão de tom pelo usuário, que continua pendente.
- 2026-10-01, verificação de tela do tamagotchi com input SINTÉTICO (`--fase tamagotchi`, `resultados/verificacao-tamagotchi.log`; a execução das 10:26 deu 46 OK, 1 N/A e 0 falhas) e repouso de 10 minutos com a onda de uma vodka (`tools/medir-desempenho.ps1 -Modo onda`, `resultados/desempenho-20261001-085153.txt`); resultados por caso em TODO.md, seção "Interação":
  - o `SendInput` ficou na ferramenta de verificação, fora do executável (seção 3.2). A medição preparou a onda só com mensagens postadas às janelas do próprio Buzzy, com o PID conferido, sem mover o cursor. O Buzzy abriu nos perfis de teste `verificacao` e `desempenho`, sem tocar nas configurações reais (DEC-029);
  - na medição: nenhum processo filho em 629 verificações, nenhuma conexão TCP/UDP em 58, a resolução global do timer sem mudança atribuível ao Buzzy e o encerramento por `WM_CLOSE`, com código 0;
  - na tela: a janela de um item não tirou o foco do aplicativo em uso, e o clique no ponto transparente dela chegou ao aplicativo de baixo; depois de cada um dos 50 menus, o foco voltou sozinho ao aplicativo em uso; sair com itens na tela terminou com código 0, sem janela nem processo do Buzzy;
  - quando uma janela sempre no topo de outro programa cobriu o canto da tela onde o Buzzy nasce, a ferramenta não clicou nem identificou a janela: só conferiu que o ponto não era do Buzzy nem do receptor da própria ferramenta (seção 3.2), e a verificação esperou o usuário liberar o canto;
  - pendentes: a revisão de tom pelo usuário, o Process Monitor depois do passo P7 (seção 8, item 4) e a sessão de uma hora sem rede da Fase 9.
- 2026-10-01, Fase 5, bloco B (passos P6–P9), depois de uma revisão de correção, uma de segurança e um corretor, por volta das 14:30:
  - `tools/testar.ps1` (Release) código 0: portão binário e de fonte APROVADO, com as mesmas quatro permissões do apphost, 74 testes do portão e nenhum pacote vulnerável. As três funções de leitura da configuração de vídeo ficam numa seção própria de `Win32.cs` e não estão na lista proibida; as que mudam a configuração global passaram a estar (seção 3.2). As agendas de gravação e de releitura não trouxeram P/Invoke novo;
  - um teste de fonte confere que o caminho do dispositivo só aparece onde vira resumo, e que o nome amigável e o EDID nunca são lidos (seção 3.1);
  - `tools/testar.ps1 -Integracao` código 0, com 342/342 no app: os Buzzy de teste abriram nos perfis `integracao` e `persistencia`, e a foto dos arquivos reais do usuário, só por fora, ficou igual antes e depois;
  - a revisão de segurança achou, e o corretor corrigiu: a linha `CONFIG` levava o número da versão lida do arquivo; havia chamadas dos testes às janelas do Buzzy fora das portas com o PID conferido; a lista de motivos de uma releitura não tinha teto; a limpeza de um perfil de teste só conferia junção ou link quando a pasta do perfil já existia, também em `tools/medir-desempenho.ps1`; faltava uma regra do portão para as APIs que mudam a configuração global; e faltava a foto dos arquivos reais na integração, na verificação de tela e na medição. As duas revisões acharam que a releitura e a conferência tardia reafirmavam a ordem Z dos itens, e a conferência é um temporizador: agora as duas só devolvem o lugar (seção 2);
  - **a confirmar com o usuário:** a revisão de segurança apontou que os arquivos reais de configuração (`settings.json` e `settings.json.bak`) foram criados às 12:12 e gravados até as 12:22 de 2026-10-01. O provável é que o próprio usuário tenha aberto o Buzzy pelo `bin\Release`, que desde o passo P7 grava as configurações reais. Desde a correção, toda execução da integração, da verificação de tela e da medição falha se essa foto mudar;
  - não ficou para este bloco: limitar as rodadas seguidas de reaplicação do lugar causadas pelo próprio `WM_DPICHANGED`, que só podem acontecer com o sprite atravessando monitores ou com a histerese de escala (passos P13 e P14; DEC-030);
  - pendentes: a verificação de tela e as medições de 10 minutos depois do bloco B, o Process Monitor (seção 8, item 4) e a sessão de uma hora sem rede da Fase 9.
- 2026-10-01, alívio, bala como droga sintética e paranoia (adicional da DEC-028), na última rodada da correção, das 23:48 às 23:52:
  - `tools/testar.ps1 -Integracao` e depois `tools/testar.ps1` (Release), os dois com código 0: portão binário e de fonte APROVADO, com as mesmas quatro permissões do apphost, 74 testes do portão e nenhum pacote vulnerável; na integração, 361/361 no app, com a foto dos arquivos reais do usuário igual antes e depois. O núcleo da paranoia não chama o Windows, e a linha `PARANOIA` usa o log de sempre: nenhum P/Invoke novo;
  - a carga, o episódio e o gerador da paranoia ficam só em memória (seção 5), e a linha `PARANOIA` não leva dado pessoal (seção 6);
  - a revisão adversarial do refino conferiu o tom: de desenho animado, sem informação real sobre drogas; a classificação dos itens é regra de jogo pedida pelo usuário, sem dose, obtenção, preparo nem efeito real;
  - pendentes: a revisão de tom pelo usuário, a verificação de tela (V17, V17b e V18) e o Process Monitor (seção 8, item 4).
- 2026-10-02, o baseado por conta própria (adendo da DEC-028), na última rodada da correção, das 01:46 às 01:50:
  - `tools/testar.ps1 -Integracao` e depois `tools/testar.ps1` (Release), os dois com código 0: portão binário e de fonte APROVADO, com as mesmas quatro permissões do apphost, 74 testes do portão e nenhum pacote vulnerável; na integração, 364/364 no app, com a foto dos arquivos reais do usuário igual antes e depois;
  - o app não mudou: nenhum P/Invoke novo, nenhuma janela nova (o baseado dele não cria item nem janela de item), nenhum tipo de linha de log novo e nenhum dado novo em disco; o uso, a onda e a carga ficam só em memória (seção 5);
  - a revisão adversarial conferiu o tom: de desenho animado, sem informação real sobre drogas, e sem texto novo para o usuário além da regra no log de diagnóstico;
  - pendentes: a revisão de tom pelo usuário, a verificação de tela (V19) e o Process Monitor (seção 8, item 4).
- O resultado de dez minutos e o portão aprovado não verificam todas as práticas da seção 8; a Fase 9 permanece pendente.

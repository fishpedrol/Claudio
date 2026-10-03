# SECURITY.md — Modelo de segurança do Buzzy

> Regras de segurança do produto. As seções numeradas são citadas pelo código: não renumere. O modelo completo continua PLANNED até a Fase 9; o que já foi verificado está na seção 10. O relato completo das verificações e as notas datadas até 2026-10-02 estão em [arquivo/seguranca-historico.md](arquivo/seguranca-historico.md).
>
> Última atualização: 2026-10-03.

## 1. Modelo de segurança

Princípios da DEC-005 e de [PRODUCT_SPEC.md](PRODUCT_SPEC.md). O Buzzy é um aplicativo local, de um usuário, sem privilégio de administrador e sem rede. Só enxerga o próprio input — cliques e teclas dirigidos às suas janelas — e só grava na própria pasta de dados. Toda capacidade do sistema operacional segue a regra:

intenção explícita → capacidade específica → permissão limitada → ação permitida e auditável.

**Fato que molda o modelo:** um processo desktop comum do Windows, rodando como o usuário, pode instalar hooks globais de teclado e mouse, ler input em segundo plano e capturar a tela sem pedir permissão nem mostrar indicador (documentação da Microsoft sobre `SetWindowsHookEx` e `RAWINPUTDEVICE`; fontes em DEC-006). A garantia de que o Buzzy não é keylogger nem captura a tela vem, portanto, do projeto: APIs proibidas, verificação automática no build e documentação pública.

## 2. Permissões

| Necessidade | Como | Permissão do sistema | Quando |
|---|---|---|---|
| Executar | Processo comum, manifesto `asInvoker`, sem elevação | Nenhuma além do usuário | Sempre |
| Janela transparente sobre o desktop | Janela sem borda, do tamanho do sprite, com transparência por pixel | Nenhuma | Fase 1 |
| Sempre no topo | Estilo topmost aplicado uma vez, nunca reafirmado por timer. As janelas dos itens só mudam de lugar na ordem Z por evento — o item aparecer, o gesto sobre ele e o personagem reaparecer (DEC-028); a releitura da topologia e a conferência tardia, um disparo único, devolvem só o lugar, sem reafirmar a ordem Z (DEC-030). O personagem volta ao topo do grupo, sem ativar, quando o usuário o mostra e quando a tela cheia acaba (DEC-034), uma vez por evento | Nenhuma | Q-03 |
| Receber clique e arraste | Mensagens de mouse das próprias janelas (o personagem e os itens) e captura do mouse durante o gesto | Nenhuma | Fase 3; itens no passo T8 |
| Ajustar energia | Controle com três valores permitidos (`BAIXA`, `MEDIA`, `ALTA`) no painel e nas configurações | Nenhuma | Fase 8 |
| Monitores, DPI e área útil | Leitura de topologia e mensagens de mudança; a configuração de vídeo, só lida, para a chave estável do monitor | Nenhuma | Fase 1; chave no passo P6 |
| Ícone na bandeja | `Shell_NotifyIcon` identificado por janela e ID, sem GUID | Nenhuma | Q-03 |
| Gravar configurações | Arquivo na pasta local do usuário | Acesso do próprio usuário | Fase 5 (desde o passo P7) e Fase 8 |
| Iniciar com o Windows | Opcional, desligado por padrão, ativado pelo usuário | Chave `Run` do usuário, atalho na pasta Inicializar ou `StartupTask` em MSIX | Q-04 |
| Mover o mascote para fora de tela cheia | Eventos WinEvent limitados; leitura transitória só do retângulo e da thread da janela de primeiro plano e do estado do shell | Nenhuma; sem elevação nem processo auxiliar | DEC-013, Q-09; implementado pela DEC-034 |

Nenhum item exige administrador, rede, acesso a arquivos do usuário fora da pasta do Buzzy, identificação de processos ou leitura do conteúdo de outras janelas. A DEC-013 é a única exceção de observação: a geometria da janela em primeiro plano é consultada em memória para escolher um monitor e descartada na hora. Riscos da stack WPF na seção 4.

## 3. Capacidades

### 3.1 Permitidas

- Criar, mover, mostrar e esconder as próprias janelas, inclusive uma janela por item do tamagotchi, que só a raiz fecha (DEC-028).
- Ler a posição do cursor e os botões só nas mensagens entregues às próprias janelas, ou durante a captura de um arraste iniciado pelo usuário (DEC-021): `SetCapture` só depois de um botão pressionado sobre um pixel visível do Buzzy, e `ReleaseCapture` no fim do gesto; `WM_MOUSEMOVE` só é tratado com a captura; a perda da captura (`WM_CAPTURECHANGED`, `WM_CANCELMODE`) encerra o gesto, e só o fato conta, nunca qual janela ficou com o mouse; o limiar de arraste e o clique duplo são lidos do sistema (`GetSystemMetricsForDpi`, `GetDoubleClickTime`) sem alterar nada; `GetWindowRect` só sobre a janela do próprio Buzzy. As janelas dos itens seguem as mesmas regras, com um árbitro próprio, e nunca são ativadas; quando o núcleo encerra o gesto por conta própria (esconder, minimizar, bloquear, suspender, sair, recolher ou pegar outro item), a janela solta a captura na hora, e um soltar tardio não vira nada.
- Montar o menu nativo com ícones (DEC-027, DEC-028): `InsertMenuItemW` e bitmaps de 32 bits só na memória do próprio Buzzy, por `CreateDIBSection` sem DC, preenchidos por `Marshal.Copy`, sem `BitBlt`, `StretchBlt` nem `CreateDC`, e apagados com `DeleteObject` depois de o menu ser destruído, mesmo quando a exibição lança; o log confere criados = apagados, e uma falha vai ao log só com o código.
- Ler geometria, escala e orientação dos monitores e a área útil de cada um. A chave estável (DEC-030) só lê a configuração de vídeo (`GetDisplayConfigBufferSizes`, `QueryDisplayConfig` com os caminhos ativos e `DisplayConfigGetDeviceInfo`, para o nome GDI da fonte e o caminho do dispositivo do alvo). O caminho, que identifica o hardware, vira na hora um resumo de 8 bytes do SHA-256 e não sai do adaptador: nunca vai ao arquivo, ao log ou ao texto automático de um registro. O nome amigável do monitor e os códigos do EDID nunca são lidos (um teste de fonte confere). Nada muda a configuração de vídeo (seção 3.2).
- Observar eventos de mudança da janela em primeiro plano e de geometria via `SetWinEventHook` fora do processo (out-of-context), filtrados para a janela de nível superior ativa; ler só `GetWindowRect` e o monitor associado; converter numa lista transitória dos monitores ocupados e descartar HWND e retângulo depois do cálculo. Nunca ler título, nome, caminho de processo, texto, pixels ou conteúdo; nunca enumerar janelas ou processos; nunca persistir nem registrar esses dados (DEC-013). A curiosidade planejada (DEC-026) usa os mesmos dados e limites, mais há quanto tempo a janela está em primeiro plano.
  - **Como está implementado** (`ObservadorDeTelaCheia`, DEC-034): a troca de primeiro plano é assinada no sistema todo, fora do processo e sem o próprio processo (`WINEVENT_SKIPOWNPROCESS`); a geometria, só na thread da janela em primeiro plano, que `GetWindowThreadProcessId` dá sem o processo (o ponteiro dele vai nulo), reassinada quando ela muda. O tratamento de um evento só conta e sinaliza; o do cursor e o de outras janelas são descartados. A avaliação, num disparo único, lê `GetForegroundWindow`, `GetWindowRect` dela e `SHQueryUserNotificationState` (um estado do shell, sem identidade), calcula os monitores cobertos e descarta o resto; a thread só serve para reassinar e para reconhecer a janela do próprio Buzzy. Nada vai ao arquivo; o log `TELA_CHEIA` leva só as chaves opacas dos monitores ocupados, os motivos, o estado do shell, quantos monitores a janela cobria e as contagens de eventos.
- Receber mensagens de sessão, energia, bloqueio e encerramento.
- Ler os próprios assets, o manifesto de assets e o perfil local de comportamento, só leitura.
- Ler e gravar os arquivos da própria pasta de dados (DEC-029; ARCHITECTURE.md 2.12):
  - as configurações só usam os nomes fixos `settings.json`, `settings.json.bak`, `settings.json.tmp` e `settings.corrupt.json`, numa pasta escolhida por uma regra única, com falha fechada. Sem perfil de teste, o Buzzy do usuário lê o `settings.json` real na partida e o grava pela agenda: pouco depois de soltá-lo, escondê-lo ou escolher uma emoção, e na hora ao sair ou no fim da sessão;
  - os perfis de teste (`--perfil-de-teste NOME`) ficam em `%LOCALAPPDATA%\Buzzy\testes\NOME`, sempre filha direta de `testes`. O nome tem de 1 a 32 caracteres, só a–z, 0–9 e hífen, sem hífen inicial, e não pode ser um nome reservado do Windows (`con`, `prn`, `aux`, `nul`, `com0`–`com9`, `lpt0`–`lpt9`); maiúsculas são recusadas, não convertidas;
  - nome inválido, opção sem nome ou opção escrita de outro jeito (`--perfil-de-teste=NOME`, outra caixa, `/perfil-de-teste`) desligam a persistência naquela execução, em vez de cair na pasta real;
  - a limpeza de um perfil antes de cada uso confere antes de tudo cada trecho do caminho que existe (a pasta do Buzzy, `testes` e a do perfil), também quando a pasta do perfil ainda não existe, e recusa junção ou link: com a pasta do Buzzy ou a dos testes como junção, o Buzzy de teste e o próprio teste gravariam do outro lado. Vale na integração, na verificação de tela e na medição de desempenho.

### 3.2 Proibidas no MVP

Cada item é regra de verificação automática desde a Fase 1 (seção 8).

| Capacidade proibida | Exemplos de API ou recurso |
|---|---|
| Input global | `SetWindowsHookEx` para teclado/mouse, `RegisterRawInputDevices` com `RIDEV_INPUTSINK` ou `RIDEV_EXINPUTSINK`, `GetAsyncKeyState`, `GetKeyboardState` usado para observar teclado fora do foco, `RegisterHotKey` sem decisão aprovada. O evento WinEvent limitado da DEC-013 não recebe input e só é permitido conforme a seção 3.1. |
| Injetar input | `SendInput`, `mouse_event`, `keybd_event` no produto; permitido só em ferramentas de teste fora do executável |
| Captura de tela | `BitBlt` ou `PrintWindow` sobre o desktop ou janelas de outros processos, Windows Graphics Capture, Desktop Duplication |
| Processos | `CreateProcess`, `ShellExecute(Ex)`, `WinExec`, execução de shell, PowerShell ou CMD |
| Rede | Sockets, WinHTTP, WinINet, `URLDownloadToFile`, `fetch` ou equivalentes, qualquer conteúdo remoto no WebView |
| Ler outros aplicativos | Área de transferência, títulos, nomes, caminhos de processos, texto ou conteúdo de outras janelas, UI Automation sobre outros processos, memória de outros processos ou `EnumWindows`. A única exceção é o retângulo e o monitor da janela ativa, temporários, conforme a DEC-013, e a thread dela, só para restringir a assinatura de geometria (DEC-034). |
| Persistência escondida | Serviço, tarefa agendada, chave `Run` criada sem ação do usuário, cópia do executável para outros locais |
| Código dinâmico | `eval`, carregamento de scripts, DLLs ou assets de caminhos fornecidos pelo usuário |
| Alterar configuração global | `SetDisplayConfig`, `DisplayConfigSetDeviceInfo`, `ChangeDisplaySettings` e `ChangeDisplaySettingsEx`, nas variantes A e W, que mudam topologia, resolução, orientação, escala ou modo de vídeo do sistema todo. Só o usuário muda as configurações do Windows (AGENTS.md); a chave estável só lê a configuração de vídeo (seção 3.1) |

**Exceção do apphost (DEC-016).** O `Buzzy.exe` é o lançador nativo genérico do SDK do .NET, não código do Buzzy: localiza o runtime, carrega `hostfxr.dll` e entrega a execução ao `Buzzy.dll`. Ele importa `LoadLibraryExW`, `LoadLibraryA` e `GetProcAddress`, para carregar o runtime, e `ShellExecuteW`, para abrir a página de download do .NET quando o runtime falta e o usuário aceita. O portão permite essas quatro importações só nesse arquivo nativo, por nome exato e com o motivo no relatório; qualquer outra importação proibida, ou qualquer uma delas no `Buzzy.dll` e nas demais DLLs, reprova o build.

**Ferramentas de teste.** A integração e a medição de desempenho só postam mensagens às janelas do Buzzy que elas mesmas abriram, com o PID conferido antes de cada uma, sem `SendInput` nem mover o cursor; o `SendInput` fica com a verificação de tela, que é input SINTÉTICO e avisa o usuário antes. Na integração, toda mensagem postada ou enviada às janelas do Buzzy e todo `SetWindowPos` ou minimização "por fora" passam pelas portas de `BuzzyEmTeste` (`Postar`, `Enviar`, `MoverPorFora`, `MinimizarPorFora`, `FecharPorWmClose` e o `Dispose`), que conferem o PID imediatamente antes; um teste de fonte proíbe as chamadas diretas. As consultas só de teste (`GetGuiResources`, `GetGUIThreadInfo` da thread do Buzzy, `GetMenuItemInfoW`, `GetDIBits` e outras) ficam fora do executável e leem objetos do próprio Buzzy ou do teste. Ao conferir a ordem Z, a varredura abaixo do personagem para na primeira janela de outro processo, da qual lê só o PID, e a conferência de que uma janela está acima de outra compara só handles. A integração, a verificação de tela e a medição tiram uma foto dos arquivos reais de configuração do usuário antes e depois, só por fora (existência, tamanho e datas de criação e de escrita, nunca o conteúdo), e falham se ela mudar.

## 4. Stack selecionada e riscos de distribuição

A DEC-006 selecionou WPF, C# e .NET 10; P1–P3 passaram no ambiente medido.

- O WPF roda num processo do usuário, sem administrador, motor web ou rede própria. O runtime .NET é dependência externa: manter versão suportada, fixar o SDK e auditar as dependências.
- O portão de segurança inspeciona binário e código quanto às capacidades da seção 3.2. Um desktop comum roda com os direitos do usuário; MSIX comum não é sandbox e não substitui o portão.
- O pacote é um ZIP portátil sem assinatura, para uso pessoal e testes (Q-10); avisos ou bloqueios do Windows são possíveis. Reavaliar formato, assinatura e instruções antes de qualquer distribuição a terceiros, que não está autorizada.

## 5. Dados armazenados

Esquema em [ARCHITECTURE.md](ARCHITECTURE.md), seção 2.12, e na DEC-010.

| Dado | Onde | Por quê |
|---|---|---|
| Posição do personagem (chave do monitor, tela desse monitor na época, posição relativa e absoluta), com a borda do esconderijo (`nenhum`, `baixo`, `esquerda` ou `direita`) e a marca "preso pelo usuário" (esquema v3, DEC-029) | `settings.json` | Restaurar onde o usuário deixou. A chave é um resumo opaco do caminho do dispositivo (`mon:` e 16 dígitos hexadecimais), nunca o caminho; sem o caminho, a reserva `gdi:` seguida do nome GDI (DEC-030) |
| Preferências: hoje energia (`BAIXA`/`MEDIA`/`ALTA`), modo de tela cheia, atravessar monitores, a emoção dominante (`"automatica"` ou uma das 14 caras de humor, DEC-027) e o conteúdo adulto (booleano, ligado por padrão; esquema v4, DEC-033); na Fase 8, também escala, sempre no topo, iniciar com o Windows e idioma | `settings.json` | Preferências do usuário; opacidade fica fora do MVP. A posição temporária do modo de tela cheia, os monitores ocupados e o cache das chaves dos monitores **não** são gravados |
| Última cópia boa e, no máximo, uma cópia ilegível para diagnóstico | `settings.json.bak`, `settings.corrupt.json` | Recuperação: a leitura usa o principal, depois o `.bak`, depois os padrões; a cópia de diagnóstico nunca é lida |
| Temporário de uma gravação | `settings.json.tmp` | Gravação atômica; apagado e recriado a cada gravação, nunca lido |
| Dados dos perfis de teste | `testes\NOME\`, com os mesmos nomes de arquivo | Isolar testes e ferramentas das configurações reais; a pasta é apagada antes de cada uso, recusando junção ou link no caminho (seção 3.1) |

Local: `%LOCALAPPDATA%\Buzzy` sem pacote, pela API de pastas conhecidas; a pasta local do pacote com MSIX. Nenhum segredo é armazenado, por isso DPAPI não é usado. O log de diagnóstico, só com `--diagnostico`, fica na mesma pasta, com tamanho limitado. Um arquivo de versão futura ou um principal inacessível na partida bloqueiam a gravação naquela execução, para não sobrescrever o que não se conhece nem se conseguiu ler. O Buzzy do usuário, aberto sem perfil de teste, lê e grava o `settings.json` real (cerca de 500 bytes) e o `.bak` nessa pasta; os testes e as ferramentas usam a pasta do perfil deles.

Os itens do tamagotchi, o uso de um item, a onda de desenho animado, a carga do episódio da paranoia e o gerador dela (DEC-028) ficam só em memória: nada vai ao `settings.json` nem a outro arquivo, fora as linhas do log de diagnóstico opcional (seção 6), e tudo some ao sair; o gerador recomeça a cada abertura, a partir da semente do núcleo.

## 6. Dados que nunca devem ser coletados

- Teclas digitadas em outros aplicativos ou fora dos controles próprios do Buzzy.
- Conteúdo da tela, de outras janelas ou da área de transferência.
- Títulos, nomes ou lista de outros aplicativos e janelas.
- Geometria ou identificador da janela em primeiro plano, salvo a leitura transitória, em memória, da avaliação imediata da DEC-013; nada disso é guardado, registrado nem transmitido.
- Arquivos do usuário.
- Identificadores do usuário ou da máquina: nome, e-mail, conta, número de série, endereço de rede.
- Localização, microfone, câmera, histórico de navegação.
- Qualquer dado enviado para fora da máquina; o MVP não tem rede.

**No log de diagnóstico** (contrato em ARCHITECTURE.md 2.13.4):
- erros do sistema vão só com o tipo e o código da exceção, nunca com a mensagem, que pode trazer um caminho com o nome do usuário ou o SID da conta — na gravação das configurações, em `INSTANCIA` e em `ERRO`; um teste de fonte lista as duas mensagens que ainda vão ao log, ambas de exceções do próprio Buzzy;
- os avisos da leitura do `settings.json` só levam nomes de campo do esquema e o motivo, nunca valores nem nomes vindos do arquivo; o nome de perfil recusado e a chave lida do arquivo não vão ao log, e o caminho da pasta de dados nunca vai;
- as linhas `MENU`, `ITEM` e `ONDA` levam só nomes de comandos, caras e itens, Ids, pontos e retângulos das janelas do próprio Buzzy, o DPI e o HWND delas, contagens e tempos; na perda da captura, não vai qual janela ficou com o mouse, e uma falha do Windows vai só com o código. A linha `PARANOIA` leva só o nome do item, a chance, `sim` ou `nao` e as contagens do episódio, com os nomes dos itens distintos;
- `TOPOLOGIA`, `POSICAO` e `ITEM` levam a chave do monitor (o resumo opaco) e o nome GDI (`\\.\DISPLAYn`), nunca o caminho do dispositivo nem o nome do monitor; a falha da consulta da configuração de vídeo ou da leitura de um monitor vai só com a função e o código;
- `MENSAGEM` leva só o tipo da mensagem do Windows (no `WM_DPICHANGED`, também o DPI novo), e uma releitura leva no máximo 32 deles; `POSICAO|reaplicada` e `ITEM|reaplicado` levam só retângulos das janelas do próprio Buzzy;
- as linhas `CONFIG` levam só enums, contagens e tempos — a pasta como `perfil` ou `padrao`, a versão do arquivo como `atual`, `anterior`, `futura` ou `-` —, nunca o número da versão lido, a pasta, a chave, a tela nem outros valores do arquivo; um erro vai com o tipo e o código.

## 7. Superfícies de ataque relevantes

| Superfície | Risco | Defesa |
|---|---|---|
| Arquivo de configurações | Arquivo adulterado ou corrompido trava o app ou causa comportamento inesperado | Implementado (DEC-029; ARCHITECTURE.md 2.12): tamanho conferido antes de ler (64 KiB, contando o BOM); UTF-8 validado; JSON lido sem reflexão, com profundidade máxima de 8; tolerância campo a campo, com valores presos a faixas; energia só pelos três nomes, a emoção dominante e a borda do esconderijo só por listas fechadas, a marca de preso só como booleano, as duas só junto com uma posição e a borda só com o esconderijo pelo clique duplo ligado; um escape de surrogate solto torna o arquivo inteiro ilegível; os avisos não repetem o valor lido; leitura principal → `.bak` → padrões; versão futura ou principal inacessível bloqueiam a gravação; temporário recriado para nunca gravar através de um link. Na partida, qualquer exceção da leitura desliga a persistência naquela execução, em vez de fechar o app, e a leitura do esquema só captura `InvalidOperationException` |
| `--perfil-de-teste` | Um teste ou ferramenta com a opção escrita errado leria e gravaria as configurações reais do usuário | Nome validado caractere a caractere; nome inválido, opção sem nome ou outra grafia desligam a persistência (falha fechada); regra única da pasta, com teste de tabela; testes de fonte exigem o perfil em todo lançador do `Buzzy.exe` (DEC-029) |
| Controle de energia | Valor fora do conjunto permitido | Enumeração estrita de três níveis, validação na carga e padrão seguro em caso de erro |
| Emoção dominante (DEC-027) | Valor adulterado no arquivo, ou fora das 14 caras de humor | Lista fechada de 15 nomes no arquivo (`automatica` e as 14 caras), sem diferenciar maiúsculas e nunca por `Enum.Parse`, que aceitaria números, listas e as caras de efeito; valor inválido vira a automática, com um aviso que não repete o valor. No núcleo, um comando fora das 14 é ignorado, e uma carga ou configuração fora delas vira a automática. No menu, o id escolhido vira a emoção ou o item por listas fixas, nunca por conversão do número, e qualquer outro id não escolhe nada |
| Evento de janela ativa (DEC-013) | Evento inesperado ou frequência alta pode gerar reposicionamento ou custo | Só os eventos aprovados, filtrados para a janela ativa e as mudanças relevantes; agrupar eventos; descartar metadados; medir no P7 a taxa de chamadas com mouse e teclado em uso, restringindo a assinatura de geometria à janela ativa se a global acordar o Buzzy continuamente; nunca polling em repouso |
| Assets, manifesto e perfil de comportamento | Arquivo trocado por outro processo do usuário | Carregados só da pasta de instalação; manifesto validado. A proteção contra troca de binários depende da forma de distribuição (Q-10) |
| Dependências de terceiros | Código vulnerável ou malicioso entrando pelo build | Nenhum pacote NuGet hoje (DEC-016); qualquer dependência nova com versão fixa e lockfile, auditoria automática no build e revisão antes de adicionar |
| Carregamento de DLL | DLL plantada na pasta do app ou no caminho de busca | Instalação em pasta própria e busca de DLL restrita, conforme a stack |
| Distribuição futura | Binário adulterado ou avisos e bloqueios do Windows | Nenhuma distribuição autorizada; se for decidida, reavaliar formato, assinatura e integridade antes de publicar |

## 8. Práticas de segurança

Cada prática é item de teste desde a Fase 1.

1. **Portão de APIs proibidas no build** (`tools/Buzzy.PortaoApis`, DEC-016): roda depois de cada build de `src/Buzzy.App` e lê as importações nativas, as declarações P/Invoke dos binários gerenciados, o código-fonte de `src/Buzzy.App`, `src/Buzzy.Core` e `src/Buzzy.Visual` e o manifesto (`asInvoker`, Per-Monitor V2); o relatório de `tools/testar.ps1` também confere `src/Buzzy.Visual`. Falha com qualquer API da seção 3.2 nos binários ou chamada equivalente no código-fonte (a arte do tamagotchi passou sem nenhum nome da lista, nem em comentários). As exceções são a do apphost (seção 3.2) e os usos restritos da DEC-034 (`UsosRestritos`): `SetWinEventHook`, `GetForegroundWindow` e `GetWindowThreadProcessId` passam só declaradas no tipo `ObservadorDeTelaCheia` do `Buzzy.dll` (ou num tipo aninhado nele) e citadas só no arquivo dele, cada uma listada no relatório como "uso restrito"; em qualquer outro tipo, assembly ou arquivo, reprovam. Hooks de input continuam proibidos. Um teste confere que as três funções de leitura da configuração de vídeo continuam permitidas.
2. **Auditoria de dependências** a cada build, com a ferramenta oficial do ecossistema.
3. **Verificação de rede zero:** nos testes manuais de cada fase, confirmar que o processo do Buzzy e os filhos não abrem conexão. Referência:

   ```powershell
   Get-NetTCPConnection -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   Get-NetUDPEndpoint -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   ```

4. **Verificação de gravação:** com o Process Monitor da Sysinternals, confirmar que o Buzzy só grava na própria pasta de dados — `%LOCALAPPDATA%\Buzzy` e, nos testes, `%LOCALAPPDATA%\Buzzy\testes\`. *Pendente [MANUAL]* desde que a persistência foi ligada (passo P7); até lá, a evidência automática é parcial: a integração confere que o Buzzy de teste gravou o arquivo do perfil dele e que a foto dos arquivos reais ficou igual, mas não vê outras pastas.
5. **Sem elevação:** manifesto `asInvoker`; o Buzzy recusa rodar elevado — mostra um aviso e sai com código 5 (DEC-016). O ZIP portátil executa sem instalação nem elevação (o P10 verifica).
6. **Gravação atômica** das configurações e validação ao ler (DEC-029), com evidência [AUTO]: `Gravar_FalhaSimuladaEmCadaEtapa_NuncaIlegivel` (queda simulada em cada etapa, a partir de quatro estados iniciais); `Ler_EstadosDeQuedaExaustivos` (o disco em cada estado possível de uma queda, com o temporário cortado em cada byte); `MatarDuranteGravacoes_NuncaDeixaIlegivel` (25 rodadas matando um processo filho que grava sem parar); `Gravar_TemporarioQueEhLinkParaOutroArquivo_NaoGravaAtravesDele` (com link físico; o link simbólico exige o modo de desenvolvedor, e sem ele o caso é pulado).
7. **Nenhum segredo** no repositório nem no app.

## 9. Limitações

- O Windows não oferece permissão nem indicador para input global ou captura de tela em apps desktop comuns: o usuário confia no código publicado e nas verificações do build (fontes em DEC-006).
- Um pacote MSIX comum roda com confiança total; o manifesto não restringe hooks, rede ou arquivos. O isolamento real exigiria AppContainer, com riscos de compatibilidade ainda não testados.
- Uma instalação por usuário sem MSIX deixa os binários graváveis por qualquer processo do mesmo usuário.
- Sem assinatura de código, o Windows mostra avisos do SmartScreen, e o Smart App Control pode bloquear o app; aceito para uso pessoal e testes, a reavaliar em Q-10 antes de qualquer distribuição pública. Assinatura para pessoa física fora dos EUA e do Canadá exige certificado OV pago.
- Sem atualização automática, proibida no MVP, correções de segurança dependem de o usuário instalar a nova versão.
- Queda de energia no meio da gravação das configurações não é testável. A defesa é descarregar o temporário no disco (`Flush(true)`) antes da troca atômica do NTFS; no pior caso, volta a versão anterior. `File.Replace` exige NTFS local: noutro sistema de arquivos, a gravação sempre falha (DEC-029).
- Links plantados pelo próprio usuário na pasta do Buzzy não cruzam fronteira de privilégio, mas podem desviar uma gravação: o temporário das configurações é recriado para nunca gravar através de um link, e a limpeza dos perfis de teste recusa junção ou link no caminho. **Aceito:** o `diagnostico.log`, anterior à Fase 5, é aberto para acrescentar e segue um link que já exista; só vale com `--diagnostico`, e só o próprio usuário consegue plantar esse link.

## 10. Verificações realizadas

O relato completo de cada verificação está no arquivo morto citado no topo.

- **2026-09-26:** pesquisa de segurança em fontes oficiais da Microsoft (DEC-006).
- **Portão e auditoria em toda bateria:** `tools/testar.ps1` com código 0 em cada marco desde 2026-09-29 — portão binário e de fonte APROVADO, sempre com as mesmas quatro permissões no apphost, e nenhum pacote vulnerável; 74 testes do portão desde o bloco B da Fase 5. As APIs que entraram depois da Fase 1 não estão na lista proibida: `SetCapture`, `ReleaseCapture`, `GetDoubleClickTime` e `GetWindowRect` na própria janela (Fase 3); `InsertMenuItemW`, `CreateDIBSection` e `DeleteObject` (menu do tamagotchi); `GetDisplayConfigBufferSizes`, `QueryDisplayConfig` e `DisplayConfigGetDeviceInfo` (chave do monitor). O núcleo, a arte, as janelas dos itens, as agendas, a gravação atômica, a paranoia e o baseado por conta própria não trouxeram P/Invoke novo ao produto.
- **Rede, processos e timer nas medições de 10 min:** nenhum processo filho em 629 verificações, nenhuma conexão TCP/UDP em 58 a 59 e nenhuma mudança da resolução global do timer atribuível ao Buzzy — linha de base da Fase 1 (`resultados/desempenho-20260929-215550.txt`), repouso e autonomia da Fase 4 (`resultados/desempenho-20260930-160012.txt`, `-161054.txt`) repouso com a onda de uma vodka (`resultados/desempenho-20261001-085153.txt`) e, já com o observador de tela cheia (DEC-034), repouso, autonomia e onda de 2026-10-03 (`resultados/desempenho-20261003-140447.txt` (repouso), `-141534.txt` (autonomia) e `-142614.txt` (onda)), esta preparada só por mensagens postadas, com o PID conferido, e encerrada por `WM_CLOSE` com código 0. Cobrem só as execuções medidas, não a sessão de uma hora da Fase 9.
- **Revisões de segurança da Fase 5:** a do bloco A (2026-09-30) corrigiu outra grafia de `--perfil-de-teste` caindo na pasta real, a escolha da pasta sem regra testada, o temporário seguindo um link existente, mensagens de exceção no log e a varredura fraca dos lançadores. A do bloco B (2026-10-01) corrigiu o número da versão do arquivo na linha `CONFIG`, chamadas dos testes às janelas do Buzzy fora das portas com o PID, a lista de motivos de uma releitura sem teto, a limpeza do perfil que só conferia junção ou link com a pasta já existente (também na medição), a falta da regra do portão para as APIs que mudam a configuração global e a falta da foto dos arquivos reais; e, com a de correção, a releitura e a conferência tardia reafirmando a ordem Z dos itens por temporizador.
- **Tela do tamagotchi, com input SINTÉTICO** (`resultados/verificacao-tamagotchi.log`; 46 OK e 1 N/A em 2026-10-01, 56 OK e 0 falhas em 2026-10-02): o `SendInput` ficou na ferramenta, fora do executável; o Buzzy abriu nos perfis de teste; a janela de um item não tirou o foco, e o clique no ponto transparente dela chegou ao aplicativo de baixo; o foco voltou sozinho depois de cada um dos 50 menus; sair com itens na tela terminou com código 0, sem janela nem processo. Quando uma janela sempre no topo de outro programa cobriu o canto onde o Buzzy nasce, a ferramenta não clicou nem identificou a janela: só conferiu que o ponto não era do Buzzy nem do receptor.
- **Integração** (`tools/testar.ps1 -Integracao`; a última, 371/371, em 2026-10-02): os Buzzy de teste abrem nos perfis `integracao` e `persistencia`, e a foto dos arquivos reais do usuário fica igual antes e depois.
- **Tom do tamagotchi:** as revisões do núcleo, do refino da paranoia e do baseado por conta própria não acharam informação real sobre drogas em nomes, comentários, testes ou textos — só nomes e efeitos de desenho animado; os itens da arte não têm texto, marca nem folha, e o menu só nomeia as opções.
- **A confirmar com o usuário:** os arquivos reais de configuração (`settings.json` e `.bak`) foram criados às 12:12 e gravados até as 12:22 de 2026-10-01, provavelmente pelo Buzzy do próprio usuário aberto de `bin\Release`, que desde o passo P7 grava as configurações reais. Desde então, toda integração, verificação de tela e medição falha se a foto desses arquivos mudar.
- **2026-10-03, modo de tela cheia (DEC-034):** o portão ganhou os usos restritos e a regra de `GetWindowThreadProcessId` (127 regras; 79 testes do portão, com o uso restrito na fonte, nos binários e de ponta a ponta); o relatório do build real lista as três funções como "uso restrito" em `ObservadorDeTelaCheia+Nativo` e aprova. `UnhookWinEvent`, `SHQueryUserNotificationState` e `GetCurrentThreadId` entraram fora da lista. A integração confere que o observador liga na partida, sai no encerramento e que nenhuma linha `TELA_CHEIA` leva janela ou retângulo. Falta medir no uso real a taxa de eventos de geometria num jogo (as contagens vão ao log).
- **Pendentes:** o Process Monitor (seção 8, item 4); a sessão de uma hora sem rede e as demais práticas da Fase 9; a revisão de tom pelo usuário; limitar as rodadas seguidas de reaplicação do lugar causadas pelo próprio `WM_DPICHANGED` (passos P13 e P14; DEC-030). O portão aprovado e as medições de 10 minutos não verificam todas as práticas da seção 8.

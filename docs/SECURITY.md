# SECURITY.md — Modelo de segurança do Buzzy

> Regras de segurança do produto. A Fase 1 está implementada e verificada nos limites descritos na seção 10; a Fase 2 está em andamento. O modelo completo de segurança continua PLANNED.
>
> Última atualização: 2026-09-29

## 1. Modelo de segurança

STATUS: PLANNED. Os princípios vêm de DEC-005 e de [PRODUCT_SPEC.md](PRODUCT_SPEC.md).

O Buzzy é um aplicativo local, de um usuário, sem privilégio de administrador e sem rede. Ele só enxerga o próprio input: cliques e teclas dirigidos às suas janelas. Ele só grava na própria pasta de dados. Toda capacidade do sistema operacional segue a regra:

intenção explícita → capacidade específica → permissão limitada → ação permitida e auditável.

**Fato que molda o modelo:** um processo desktop comum do Windows, rodando como o usuário, pode instalar hooks globais de teclado e mouse, ler input em segundo plano e capturar a tela sem pedir permissão e sem mostrar indicador ao usuário. Fontes: documentação da Microsoft sobre `SetWindowsHookEx` e `RAWINPUTDEVICE`; consulte as fontes técnicas em DEC-006. Por isso, a garantia de que o Buzzy não é keylogger nem captura a tela não vem do Windows. Ela vem do próprio projeto: APIs proibidas, verificação automática no build e documentação pública.

## 2. Permissões

STATUS: PLANNED.

| Necessidade | Como | Permissão do sistema | Quando |
|---|---|---|---|
| Executar | Processo comum, manifesto `asInvoker`, sem elevação | Nenhuma além do usuário | Sempre |
| Janela transparente sobre o desktop | Janela sem borda, do tamanho do sprite, com transparência por pixel | Nenhuma | Fase 1 |
| Sempre no topo | Estilo topmost aplicado uma vez, nunca reafirmado por timer | Nenhuma | Q-03 |
| Receber clique e arraste | Mensagens de mouse da própria janela e captura do mouse durante o arraste | Nenhuma | Fase 3 |
| Ajustar energia | Controle com três valores permitidos (`BAIXA`, `MEDIA`, `ALTA`) no painel do mascote e nas configurações | Nenhuma | Fase 8 |
| Monitores, DPI e área útil | Leitura de topologia e mensagens de mudança | Nenhuma | Fase 1 |
| Ícone na bandeja | `Shell_NotifyIcon` identificado por janela e ID, sem GUID | Nenhuma | Q-03 |
| Gravar configurações | Arquivo na pasta local do usuário | Acesso do próprio usuário | Fase 5 (posição) e Fase 8 |
| Iniciar com o Windows | Opcional, desligado por padrão, ativado pelo usuário | Chave `Run` do usuário, atalho na pasta Inicializar ou `StartupTask` em MSIX | Q-04 |
| Mover o mascote para fora de tela cheia | Eventos WinEvent limitados; leitura transitória apenas do retângulo e monitor da janela de primeiro plano | Nenhuma; sem elevação ou processo auxiliar | DEC-013/Q-09 |

Nenhum item exige administrador, rede, acesso a arquivos do usuário fora da pasta do Buzzy, identificação de processos ou leitura do conteúdo de outras janelas. DEC-013 é a única exceção de observação: a geometria da janela em primeiro plano é consultada em memória para escolher um monitor e descartada imediatamente.

Os riscos da stack WPF selecionada estão resumidos na seção 4.

## 3. Capacidades

### 3.1 Permitidas

STATUS: PLANNED.

- Criar, mover, mostrar e esconder as próprias janelas.
- Ler posição do cursor e botões apenas nas mensagens entregues às próprias janelas, ou durante a captura de um arraste iniciado pelo usuário.
- Ler geometria, escala e orientação dos monitores e a área útil de cada um.
- Observar eventos de mudança de janela em primeiro plano e de geometria via `SetWinEventHook` em modo out-of-context, filtrados para a janela de nível superior ativa; ler somente `GetWindowRect` e o monitor associado. Converter em uma lista transitória dos monitores ocupados e descartar HWND/retângulo após o cálculo. Nunca ler título, nome, caminho de processo, texto, pixels ou conteúdo; nunca enumerar janelas/processos; nunca persistir ou registrar esses dados. (DEC-013)
- Receber mensagens de sessão, energia, bloqueio e encerramento.
- Ler os próprios assets, o manifesto de assets e o perfil local de comportamento, somente leitura.
- Ler e gravar os arquivos da própria pasta de dados.

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

**Exceção do apphost (DEC-016).** O `Buzzy.exe` é o lançador nativo genérico do SDK do .NET, não código do Buzzy: localiza o runtime instalado, carrega `hostfxr.dll` e entrega a execução ao `Buzzy.dll`. Ele importa `LoadLibraryExW`, `LoadLibraryA` e `GetProcAddress` para carregar o runtime, e `ShellExecuteW` para abrir a página de download do .NET quando o runtime falta e o usuário aceita. O portão permite essas quatro importações só nesse arquivo nativo, por nome exato e com o motivo no relatório; qualquer outra importação proibida, ou qualquer uma delas no `Buzzy.dll` e nas demais DLLs, reprova o build.

## 4. Stack selecionada e riscos de distribuição

STATUS: PLANNED. DEC-006 selecionou WPF, C# e .NET 10; P1/P2 foram aceitos nos limites documentados e P3 passou como gate técnico no ambiente medido, com input sintético. A seleção não valida por si só o aplicativo.

- WPF executa em um processo do usuário e não exige administrador, motor web ou rede própria. O runtime .NET continua sendo dependência externa: manter versão suportada, fixar o SDK e auditar as dependências do aplicativo.
- O portão de segurança deve inspecionar binário e código quanto às capacidades proibidas da seção 3.2. Um desktop comum roda com os direitos do usuário; MSIX comum não é sandbox e não substitui esse portão.
- O primeiro pacote será ZIP portátil sem assinatura para uso pessoal e testes (Q-10). Avisos ou bloqueios do Windows são possíveis. Reavaliar formato, assinatura e instruções antes de qualquer distribuição a terceiros; nenhuma distribuição está autorizada.
## 5. Dados armazenados

STATUS: PLANNED. Esquema proposto em [ARCHITECTURE.md](ARCHITECTURE.md), seção 2.12, e em DEC-010.

| Dado | Onde | Por quê |
|---|---|---|
| Posição do personagem (chave e retângulo do monitor, posição relativa e absoluta) | `settings.json` | Restaurar onde o usuário deixou |
| Escala, sempre no topo, iniciar com o Windows, nível de energia (`BAIXA`/`MEDIA`/`ALTA`), modo de tela cheia ligado/desligado, atravessar monitores, idioma | `settings.json` | Preferências do usuário; opacidade fica fora do MVP. A posição temporária do modo de tela cheia e os monitores ocupados **não** são gravados |
| Última cópia boa e, no máximo, uma cópia ilegível para diagnóstico | `settings.json.bak`, `settings.corrupt.json` | Recuperação |

Local: `%LOCALAPPDATA%\Buzzy` sem pacote, obtido pela API de pastas conhecidas; pasta local do pacote com MSIX. Nenhum segredo é armazenado, por isso DPAPI não é usado. Logs de diagnóstico, se existirem, ficam na mesma pasta e têm tamanho limitado.

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

## 7. Superfícies de ataque relevantes

STATUS: PLANNED.

| Superfície | Risco | Defesa planejada |
|---|---|---|
| Arquivo de configurações | Arquivo adulterado ou corrompido trava o app ou causa comportamento inesperado | Tamanho máximo, esquema validado, valores presos a faixas, padrões em caso de erro |
| Controle de energia | Valor de configuração adulterado ou fora do conjunto permitido | Enumeração estrita de três níveis, validação no carregamento e valor padrão seguro em caso de erro |
| Evento de janela ativa (DEC-013) | Evento inesperado ou frequência alta pode gerar reposicionamento/custo | Observar somente eventos aprovados; filtrar para a janela ativa e mudanças relevantes; agrupar eventos; descartar metadados; medir em P7 a taxa de chamadas com mouse e teclado em uso, restringindo a assinatura de geometria à janela ativa se a global acordar o Buzzy continuamente; nunca usar loop de polling em repouso |
| Assets, manifesto e perfil de comportamento | Arquivo trocado por outro processo do usuário | Carregados só da pasta de instalação; manifesto validado. Proteção contra troca de binários depende da forma de distribuição (Q-10) |
| Dependências de terceiros | Código vulnerável ou malicioso entrando pelo build | Poucas dependências, versões fixas com lockfile, auditoria automática no build, revisão antes de adicionar |

| Carregamento de DLL | DLL plantada na pasta do app ou no caminho de busca | Instalação em pasta própria e busca de DLL restrita, conforme a stack |
| Distribuição futura | Binário adulterado ou avisos/bloqueios do Windows | Não há distribuição autorizada; se ela for decidida, reavaliar formato, assinatura e integridade antes de publicar |

## 8. Práticas de segurança

STATUS: PLANNED. Cada prática vira item de teste a partir da Fase 1.

1. **Portão de APIs proibidas no build.** Um script inspeciona as importações do executável e das DLLs próprias e falha se encontrar APIs da seção 3.2. O mesmo script procura as chamadas equivalentes no código-fonte da stack escolhida. A exceção `SetWinEventHook` só passa se ficar restrita aos eventos e filtros de DEC-013; hooks de input continuam proibidos. *Implementação (Fase 1, DEC-016):* `tools/Buzzy.PortaoApis` roda depois de cada build de `src/Buzzy.App` e lê as importações nativas, as declarações P/Invoke dos binários gerenciados, o código-fonte de `src/Buzzy.App` e `src/Buzzy.Core` e o manifesto (`asInvoker`, Per-Monitor V2). A única exceção é a do apphost descrita na seção 3.2.
2. **Auditoria de dependências** a cada build, com a ferramenta oficial do ecossistema da stack.
3. **Verificação de rede zero:** durante os testes manuais de cada fase, confirmar que o processo do Buzzy e os processos filhos não abrem conexão. Comando de referência:

   ```powershell
   Get-NetTCPConnection -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   Get-NetUDPEndpoint -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   ```

4. **Verificação de gravação:** com o Process Monitor da Sysinternals, confirmar que o Buzzy só grava na própria pasta de dados.
5. **Sem elevação:** manifesto `asInvoker`; o app recusa rodar elevado ou avisa e continua sem usar o privilégio. O ZIP portátil inicial deve executar sem instalação nem elevação; P10 verifica o comportamento. *Implementação (Fase 1, DEC-016):* o Buzzy recusa rodar elevado — mostra um aviso e sai com código 5.
6. **Gravação atômica** das configurações e validação ao ler.
7. **Nenhum segredo** no repositório nem no app.

## 9. Limitações

- O Windows não oferece permissão nem indicador para input global ou captura de tela em apps desktop comuns. O usuário precisa confiar no código publicado e nas verificações do build. Fontes oficiais sobre hooks e input bruto estão ligadas em DEC-006.
- Um pacote MSIX comum roda com confiança total. O manifesto não restringe hooks, rede ou arquivos. O isolamento real exigiria AppContainer, com riscos de compatibilidade ainda não testados.
- Uma instalação por usuário sem MSIX deixa os binários graváveis por qualquer processo do mesmo usuário.
- Sem assinatura de código, o Windows mostra avisos do SmartScreen, e o Smart App Control pode bloquear o app. Essa limitação foi aceita para uso pessoal e testes; assinatura e formato devem ser reavaliados em Q-10 antes de qualquer distribuição pública. Assinatura para pessoa física fora dos EUA e do Canadá exige certificado OV pago.

- Sem atualização automática, que é proibida no MVP, correções de segurança dependem de o usuário instalar a nova versão.

## 10. Verificações realizadas

- 2026-09-26: pesquisa de segurança em fontes oficiais da Microsoft; fontes e limites estão em DEC-006.
- 2026-09-29: `powershell -NoProfile -File tools/testar.ps1` terminou com código 0; build Release sem avisos/erros, 73 testes do portão aprovados, portão binário/fonte aprovado (quatro permissões estritamente no apphost) e auditoria de pacotes sem falhas. O relatório do portão descreve cada permissão.
- 2026-09-29: em `resultados/desempenho-20260929-215550.txt`, a medição de dez minutos observou zero processos filhos em 629 verificações e zero conexões TCP/UDP em 58 verificações; a resolução do timer global não mudou durante a medição. Isso cobre somente a execução medida e não substitui a sessão de uma hora nem os demais itens da Fase 9.
- O resultado de dez minutos e o portão aprovado não verificam todas as práticas da seção 8; a Fase 9 permanece pendente.

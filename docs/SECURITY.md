# SECURITY.md — Modelo de segurança do Buzzy

> Registra as regras de segurança do produto e separa intenção de implementação real. Não há código: todo o conteúdo abaixo é STATUS: PLANNED, salvo quando indicado de outra forma. Fontes das afirmações sobre o Windows estão em DEC-006, em [DECISIONS.md](DECISIONS.md).
>
> Última atualização: 2026-09-26

## 1. Modelo de segurança

STATUS: PLANNED. Os princípios vêm de DEC-005 e de [PRODUCT_SPEC.md](PRODUCT_SPEC.md).

O Buzzy é um aplicativo local, de um usuário, sem privilégio de administrador e sem rede. Ele só enxerga o próprio input: cliques e teclas dirigidos às suas janelas. Ele só grava na própria pasta de dados. Toda capacidade do sistema operacional segue a regra:

intenção explícita → capacidade específica → permissão limitada → ação permitida e auditável.

**Fato que molda o modelo:** um processo desktop comum do Windows, rodando como o usuário, pode instalar hooks globais de teclado e mouse, ler input em segundo plano e capturar a tela sem pedir permissão e sem mostrar indicador ao usuário. Fontes: documentação de `SetWindowsHookEx`, `RAWINPUTDEVICE` e das capacidades de apps empacotados, citadas em DEC-006. Por isso, a garantia de que o Buzzy não é keylogger nem captura a tela não vem do Windows. Ela vem do próprio projeto: APIs proibidas, verificação automática no build e documentação pública.

## 2. Permissões

STATUS: PLANNED.

| Necessidade | Como | Permissão do sistema | Quando |
|---|---|---|---|
| Executar | Processo comum, manifesto `asInvoker`, sem elevação | Nenhuma além do usuário | Sempre |
| Janela transparente sobre o desktop | Janela sem borda, do tamanho do sprite, com transparência por pixel | Nenhuma | Fase 1 |
| Sempre no topo | Estilo topmost aplicado uma vez, nunca reafirmado por timer | Nenhuma | Q-03 |
| Receber clique e arraste | Mensagens de mouse da própria janela e captura do mouse durante o arraste | Nenhuma | Fase 3 |
| Receber texto | Campo de texto na janela da conversa, só com foco dado pelo usuário | Nenhuma | Fase 7 |
| Monitores, DPI e área útil | Leitura de topologia e mensagens de mudança | Nenhuma | Fase 1 |
| Ícone na bandeja | `Shell_NotifyIcon` identificado por janela e ID, sem GUID | Nenhuma | Q-03 |
| Gravar configurações | Arquivo na pasta local do usuário | Acesso do próprio usuário | Fase 5 (posição) e Fase 8 |
| Iniciar com o Windows | Opcional, desligado por padrão, ativado pelo usuário | Chave `Run` do usuário, atalho na pasta Inicializar ou `StartupTask` em MSIX | Q-04 |
| Saber se há app em tela cheia | Consulta periódica de baixa frequência a `SHQueryUserNotificationState` | Nenhuma | Q-09 |

Nenhum item exige administrador, rede, acesso a arquivos do usuário fora da pasta do Buzzy ou leitura de outras janelas.

As permissões que cada alternativa de stack acrescenta por conta própria estão na seção 4.

## 3. Capabilities

### 3.1 Permitidas

STATUS: PLANNED.

- Criar, mover, mostrar e esconder as próprias janelas.
- Ler posição do cursor e botões apenas nas mensagens entregues às próprias janelas, ou durante a captura de um arraste iniciado pelo usuário.
- Ler geometria, escala e orientação dos monitores e a área útil de cada um.
- Receber mensagens de sessão, energia, bloqueio e encerramento.
- Ler os próprios assets e o próprio arquivo de conteúdo, somente leitura.
- Ler e gravar os arquivos da própria pasta de dados.

### 3.2 Proibidas no MVP

STATUS: PLANNED. Cada item vira regra de verificação automática na Fase 1 (seção 8).

| Capacidade proibida | Exemplos de API ou recurso |
|---|---|
| Input global | `SetWindowsHookEx` com qualquer hook, `RegisterRawInputDevices` com `RIDEV_INPUTSINK` ou `RIDEV_EXINPUTSINK`, `GetAsyncKeyState`, `GetKeyboardState` usado para observar teclado fora do foco, `RegisterHotKey` sem decisão aprovada |
| Injetar input | `SendInput`, `mouse_event`, `keybd_event` no produto; permitido apenas em ferramentas de teste fora do executável |
| Captura de tela | `BitBlt` ou `PrintWindow` sobre o desktop ou janelas de outros processos, Windows Graphics Capture, Desktop Duplication |
| Processos | `CreateProcess`, `ShellExecute(Ex)`, `WinExec`, execução de shell, PowerShell ou CMD |
| Rede | Sockets, WinHTTP, WinINet, `URLDownloadToFile`, `fetch` ou equivalentes, qualquer conteúdo remoto no WebView |
| Ler outros aplicativos | Área de transferência, títulos e conteúdo de outras janelas, UI Automation sobre outros processos, memória de outros processos, `EnumWindows` sem a decisão Q-05 aprovar janelas como superfície |
| Persistência escondida | Serviço, tarefa agendada, chave `Run` criada sem ação do usuário, cópia do executável para outros locais |
| Código dinâmico | `eval`, carregamento de scripts, DLLs ou assets de caminhos fornecidos pelo usuário |

## 4. Permissões por alternativa de stack

STATUS: UNCERTAIN até a aprovação de DEC-006.

(Seção preenchida após a comparação de tecnologias.)

## 5. Dados armazenados

STATUS: PLANNED. Esquema proposto em [ARCHITECTURE.md](ARCHITECTURE.md), seção 2.12, e em DEC-010.

| Dado | Onde | Por quê |
|---|---|---|
| Posição do personagem (chave e retângulo do monitor, posição relativa e absoluta) | `settings.json` | Restaurar onde o usuário deixou |
| Escala, opacidade, sempre no topo, iniciar com o Windows, nível de autonomia, atravessar monitores, idioma | `settings.json` | Preferências do usuário |
| Última cópia boa e, no máximo, uma cópia ilegível para diagnóstico | `settings.json.bak`, `settings.corrupt.json` | Recuperação |

Local: `%LOCALAPPDATA%\Buzzy` sem pacote, obtido pela API de pastas conhecidas; pasta local do pacote com MSIX. Nenhum segredo é armazenado, por isso DPAPI não é usado. Logs de diagnóstico, se existirem, ficam na mesma pasta, têm tamanho limitado e nunca contêm texto digitado.

## 6. Dados que nunca devem ser coletados

STATUS: PLANNED.

- Teclas digitadas fora da caixa de texto do Buzzy.
- O texto digitado na caixa, além do tempo necessário para escolher a resposta. Ele não vai para disco, log nem memória de conversa.
- Conteúdo da tela, de outras janelas ou da área de transferência.
- Títulos, nomes ou lista de outros aplicativos e janelas.
- Arquivos do usuário.
- Identificadores do usuário ou da máquina: nome, e-mail, conta, número de série, endereço de rede.
- Localização, microfone, câmera, histórico de navegação.
- Qualquer dado enviado para fora da máquina. O MVP não tem rede.

## 7. Superfícies de ataque relevantes

STATUS: PLANNED.

| Superfície | Risco | Defesa planejada |
|---|---|---|
| Arquivo de configurações | Arquivo adulterado ou corrompido trava o app ou causa comportamento inesperado | Tamanho máximo, esquema validado, valores presos a faixas, padrões em caso de erro |
| Texto da caixa de conversa | Entrada enorme ou maliciosa | Limite de tamanho, tratamento como dado puro, sem expressões regulares construídas a partir da entrada |
| Assets e arquivo de conteúdo | Arquivo trocado por outro processo do usuário | Carregados só da pasta de instalação; manifesto validado. Proteção contra troca de binários depende da forma de distribuição (Q-10) |
| Dependências de terceiros | Código vulnerável ou malicioso entrando pelo build | Poucas dependências, versões fixas com lockfile, auditoria automática no build, revisão antes de adicionar |
| Motor web embutido, se a stack tiver um | Navegação remota, execução de script, ponte nativa ampla | Seção 4 |
| Carregamento de DLL | DLL plantada na pasta do app ou no caminho de busca | Instalação em pasta própria e busca de DLL restrita, conforme a stack |
| Distribuição | Binário adulterado, aviso do SmartScreen, bloqueio pelo Smart App Control | Assinatura de código (Q-10); hash publicado junto do pacote |

## 8. Práticas de segurança

STATUS: PLANNED. Cada prática vira item de teste a partir da Fase 1.

1. **Portão de APIs proibidas no build.** Um script inspeciona as importações do executável e das DLLs próprias e falha se encontrar APIs da seção 3.2. O mesmo script procura as chamadas equivalentes no código-fonte da stack escolhida.
2. **Auditoria de dependências** a cada build, com a ferramenta oficial do ecossistema da stack.
3. **Verificação de rede zero:** durante os testes manuais de cada fase, confirmar que o processo do Buzzy e os processos filhos não abrem conexão. Comando de referência:

   ```powershell
   Get-NetTCPConnection -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   Get-NetUDPEndpoint -OwningProcess (Get-Process buzzy).Id -ErrorAction SilentlyContinue
   ```

4. **Verificação de gravação:** com o Process Monitor da Sysinternals, confirmar que o Buzzy só grava na própria pasta de dados.
5. **Sem elevação:** manifesto `asInvoker`; o app recusa rodar elevado ou avisa e continua sem usar o privilégio. A escolha é parte de Q-10.
6. **Gravação atômica** das configurações e validação ao ler.
7. **Nenhum segredo** no repositório nem no app.

## 9. Limitações

- O Windows não oferece permissão nem indicador para input global ou captura de tela em apps desktop comuns. O usuário precisa confiar no código publicado e nas verificações do build. Fonte em DEC-006.
- Um pacote MSIX comum roda com confiança total. O manifesto não restringe hooks, rede ou arquivos. O isolamento real exigiria AppContainer, com riscos de compatibilidade ainda não testados.
- Uma instalação por usuário sem MSIX deixa os binários graváveis por qualquer processo do mesmo usuário.
- Sem assinatura de código, o Windows mostra avisos do SmartScreen, e o Smart App Control pode bloquear o app. Assinatura para pessoa física fora dos EUA e do Canadá exige certificado OV pago. Decisão em Q-10.
- Se a stack usar WebView2 no modo Evergreen, um processo aberto por dias continua na versão antiga do runtime até recriar o ambiente ou reiniciar.
- Sem atualização automática, que é proibida no MVP, correções de segurança dependem de o usuário instalar a nova versão.

## 10. Verificações realizadas

- 2026-09-26: pesquisa documental em fontes oficiais da Microsoft sobre hooks, raw input, capacidades de apps empacotados, assinatura, SmartScreen, Smart App Control, WebView2 e pastas de dados. Resultado resumido em DEC-006.
- Nenhuma verificação de código, porque não há código. Nenhum teste de segurança foi executado.

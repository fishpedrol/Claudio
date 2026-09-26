# SECURITY.md — Modelo de segurança do Buzzy

> Registra as regras de segurança do produto e separa intenção de implementação real. Não há código: todo o conteúdo abaixo é STATUS: PLANNED, salvo quando indicado de outra forma. As afirmações sobre o comportamento do Windows vêm da pesquisa documental de 2026-09-26; as fontes estão listadas em DEC-006, seção "Fontes primárias", em [DECISIONS.md](DECISIONS.md).
>
> Última atualização: 2026-09-26

## 1. Modelo de segurança

STATUS: PLANNED. Os princípios vêm de DEC-005 e de [PRODUCT_SPEC.md](PRODUCT_SPEC.md).

O Buzzy é um aplicativo local, de um usuário, sem privilégio de administrador e sem rede. Ele só enxerga o próprio input: cliques e teclas dirigidos às suas janelas. Ele só grava na própria pasta de dados. Toda capacidade do sistema operacional segue a regra:

intenção explícita → capacidade específica → permissão limitada → ação permitida e auditável.

**Fato que molda o modelo:** um processo desktop comum do Windows, rodando como o usuário, pode instalar hooks globais de teclado e mouse, ler input em segundo plano e capturar a tela sem pedir permissão e sem mostrar indicador ao usuário. Fontes: documentação da Microsoft sobre `SetWindowsHookEx`, `RAWINPUTDEVICE` e as capacidades de aplicativos empacotados, listada em DEC-006, seção "Fontes primárias", sob "Documentação das stacks avaliadas". Por isso, a garantia de que o Buzzy não é keylogger nem captura a tela não vem do Windows. Ela vem do próprio projeto: APIs proibidas, verificação automática no build e documentação pública.

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

## 3. Capacidades

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

## 4. Permissões e riscos por alternativa de stack

STATUS: PLANNED. DEC-006 escolheu WPF com C# e .NET 10. Esta seção mantém a comparação das alternativas e registra o que cada uma acrescenta além das permissões da seção 2. Nenhuma exige elevação para **executar** nem permissão em tempo de execução: um aplicativo desktop comum roda com os direitos do usuário. Por isso a comparação é de **superfície e auditabilidade**, não de permissões concedidas.

### 4.1 O que cada stack acrescenta

| Stack | Processos | Runtime externo | Superfície extra | Como o click-through é obtido |
|---|---|---|---|---|
| **WPF/.NET 10** (escolhida) | 1 | Runtime compartilhado ou incluído no ZIP portátil, a escolher no plano de build | Runtime gerenciado; ZIP para uso pessoal e testes aceito em Q-10; tamanho e modo de empacotamento verificados em P10; sem motor web | Pelo sistema, via janela layered criada pelo WPF |
| **Win32 nativo** (alternativa se P1/P3 falharem) | 1 | Nenhum. Só bibliotecas do Windows e a biblioteca da linguagem, que pode ser ligada de forma estática | Apenas o próprio executável, os assets e o arquivo de configurações | Pelo sistema, sem código do aplicativo |
| **Qt 6 Widgets** | 1 | Redistribuível do compilador, cuja instalação oficial exige elevação | DLLs e plugins carregados da pasta do executável, o que exige instalar em pasta protegida; a lista oficial de vulnerabilidades conhecidas do Qt, citada em DEC-006, registra dezenas de entradas recentes, e os módulos de rede, imagem vetorial e XML não devem ser ligados | Pelo sistema, sem código do aplicativo |
| **Avalonia** | 1 | Nenhum, com compilação nativa | Coleta de dados durante o build por uma dependência transitiva, que exige desativação explícita | Não suportado; contorno não validado |
| **Tauri** | Vários: o aplicativo mais os processos do motor web | Motor web do sistema, que atualiza sozinho | Motor web completo; o aplicativo separa o núcleo da interface por permissões próprias, o que é o melhor modelo entre as stacks com motor web; em contrapartida, segundo a leitura do código citada em DEC-006, o projeto desliga por padrão a proteção interna de reputação do motor | Consulta periódica do cursor, cerca de 60 vezes por segundo |
| **Electron** | Quatro ou mais | Nenhum; embute o próprio motor web e ambiente de execução | Motor web e ambiente de execução completos dentro do aplicativo, com toda a manutenção de segurança por conta do projeto | **Hook global de mouse.** Proibido por DEC-005 |
| **WinUI 3** | 1 | Runtime próprio da plataforma, mais o .NET se for C# | Runtime da plataforma em vários pacotes; bandeja e transparência dependem de bibliotecas mantidas por indivíduos | Recorte da janela por região, não validado |
| **Flutter** | 1 | Nenhum além das bibliotecas do compilador | Plugins nativos de terceiros rodando dentro do processo, de um único mantenedor, em migração; a transparência usa uma função não documentada do Windows | Consulta periódica do cursor |
| **Godot** | 1 | Nenhum | O build padrão inclui módulos de rede, que só saem recompilando o motor, o que atrapalha a auditoria de "sem rede" | Recorte por polígono |

### 4.2 Rede e arquivos que cada stack traz por conta própria

O MVP não usa rede. Esta tabela registra o que cada stack faz **sem o aplicativo pedir**, porque é isso que a verificação de "rede zero" da seção 8 precisa alcançar.

| Stack | Rede por conta própria | Arquivos que grava por conta própria |
|---|---|---|
| **Win32 nativo** | Nenhuma. Nenhuma biblioteca de rede é ligada ao binário | Nenhum |
| **WPF** | Nenhuma no aplicativo. O runtime compartilhado atualiza por fora pelo mecanismo de atualização do sistema | Arquivos temporários de um executável de arquivo único, se essa forma de publicação for usada |
| **Qt 6** | Nenhuma, desde que o módulo de rede não seja ligado. É uma regra de build a verificar | Nenhum, com os módulos mínimos |
| **Avalonia** | Nenhuma em execução. Uma dependência coleta dados durante o build e exige desativação explícita | Nenhum |
| **Tauri** | **Sim.** O motor web verifica e baixa atualizações sozinho, fora do controle do aplicativo | Pasta de dados do motor web: cache, banco de dados local e estado de navegação |
| **Electron** | **Sim.** O motor web embutido faz requisições próprias, como verificação de certificados, salvo se desligadas uma a uma | Mesma pasta de dados de motor web |
| **WinUI 3** | Nenhuma no aplicativo. O runtime da plataforma atualiza por fora | Nenhum relevante |
| **Flutter** | Nenhuma no aplicativo | Nenhum relevante |
| **Godot** | Nenhuma em execução, mas o build padrão **inclui** os módulos de rede, que só saem recompilando o motor | Nenhum relevante |

Consequência: nas duas stacks com motor web, a promessa de "rede zero" não é verificável apenas pelo código do aplicativo, porque o motor tem tráfego próprio. Nas demais, ela é verificável pela ausência das bibliotecas no binário e pela conferência de conexões descrita na seção 8.

### 4.3 Consequências para a segurança

1. **WPF traz o runtime .NET como dependência.** É um processo e não há motor web, mas o portão do projeto não audita o código do runtime como se fosse código do Buzzy. Manter .NET 10 numa versão suportada, fixar a versão do SDK e revisar dependências do aplicativo. O usuário escolheu ZIP portátil para uso pessoal e testes; o build define se o runtime será compartilhado ou incluído e P10 registra tamanho e comportamento. Se houver distribuição pública, formato e assinatura voltam a Q-10.
2. **Electron é incompatível com DEC-005 como está.** O único caminho de click-through utilizável instala hook global de mouse, exatamente a API que o produto se compromete a não usar e que o portão do build bloqueia. Isso vale independentemente da intenção: a API estaria no binário.
3. **Consulta periódica do cursor amplia a leitura de input e quebra a meta de repouso.** Tauri e Flutter só atravessam o clique consultando a posição do cursor várias vezes por segundo, em todo o desktop, enquanto o Buzzy estiver visível. Isso vai além da capacidade permitida na seção 3.1, que limita a leitura do ponteiro às mensagens entregues às próprias janelas, e reprova o critério oficial de repouso registrado em DEC-011.
4. **Um pacote MSIX comum não isola nada.** Ele roda com confiança total; o manifesto não restringe hooks, rede nem arquivos. O isolamento real exigiria o modo de contêiner, que roda em integridade baixa e pode impedir ler a geometria de janelas de outros aplicativos. O usuário deixou janelas de outros aplicativos fora do MVP em Q-05; essa limitação só volta a importar se o escopo mudar.
5. **A garantia não vem do sistema.** Como o Windows não pede permissão nem mostra indicador para hooks globais e captura de tela, nenhuma stack prova por si que o Buzzy não é keylogger. A garantia vem do portão do build e do código publicado. WPF acrescenta o runtime .NET, que deve permanecer suportado; isso amplia o código-base externo em relação a Win32, mas não cria tráfego de rede por si só. Um motor web acrescentaria também superfície de rede e atualização própria, razão para evitá-lo no MVP.
6. **Cada dependência nativa de terceiros é código no processo.** Se for necessário adicionar uma biblioteca externa para bandeja ou transparência, fixar sua versão e revisá-la antes de entrar. A arquitetura escolhida prefere recursos já incluídos no .NET ou a API da Shell para evitar dependência extra.

### 4.4 Riscos de distribuição, iguais para todas

- Sem assinatura de código, o Windows mostra aviso e o controle de aplicativos do Windows 11 pode bloquear a execução. Nenhum certificado dá reputação imediata; o tipo estendido perdeu essa vantagem em 2024.
- Assinatura para pessoa física fora dos Estados Unidos e do Canadá não tem a opção mensal barata da Microsoft; sobra certificado pago com chave em dispositivo físico, projeto de código aberto com patrocínio de assinatura, ou a Microsoft Store, que reassina sem custo.
- Aplicativos dessa categoria têm histórico de associação com adware. Comportamento que pareça coleta de dados pode gerar reputação negativa para o certificado.
- Instalação por usuário sem pacote deixa os binários graváveis por qualquer processo do mesmo usuário. Um pacote MSIX instalado deixa os arquivos do pacote somente leitura e impede a execução se forem adulterados, sem exigir administrador. Isso protege os binários contra troca, e **nada mais**: o aplicativo continua rodando com confiança total, com os mesmos direitos do usuário. Fora da Store, instalar o pacote ainda exige um certificado em que a máquina já confie, e instalar esse certificado exige administrador.

Em Q-10, o usuário aceitou um ZIP portátil sem assinatura para uso pessoal e testes. Como o Windows pode avisar ou bloquear arquivos sem assinatura, qualquer publicação para outras pessoas exige reavaliar formato, assinatura e instruções antes de distribuir.

## 5. Dados armazenados

STATUS: PLANNED. Esquema proposto em [ARCHITECTURE.md](ARCHITECTURE.md), seção 2.12, e em DEC-010.

| Dado | Onde | Por quê |
|---|---|---|
| Posição do personagem (chave e retângulo do monitor, posição relativa e absoluta) | `settings.json` | Restaurar onde o usuário deixou |
| Escala, sempre no topo, iniciar com o Windows, nível de autonomia, atravessar monitores, idioma | `settings.json` | Preferências do usuário; opacidade fica fora do MVP |
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
5. **Sem elevação:** manifesto `asInvoker`; o app recusa rodar elevado ou avisa e continua sem usar o privilégio. O ZIP portátil inicial deve executar sem instalação nem elevação; P10 verifica o comportamento.
6. **Gravação atômica** das configurações e validação ao ler.
7. **Nenhum segredo** no repositório nem no app.

## 9. Limitações

- O Windows não oferece permissão nem indicador para input global ou captura de tela em apps desktop comuns. O usuário precisa confiar no código publicado e nas verificações do build. Fonte: documentação da Microsoft sobre hooks e input bruto, listada em DEC-006, seção "Fontes primárias".
- Um pacote MSIX comum roda com confiança total. O manifesto não restringe hooks, rede ou arquivos. O isolamento real exigiria AppContainer, com riscos de compatibilidade ainda não testados.
- Uma instalação por usuário sem MSIX deixa os binários graváveis por qualquer processo do mesmo usuário.
- Sem assinatura de código, o Windows mostra avisos do SmartScreen, e o Smart App Control pode bloquear o app. Essa limitação foi aceita para uso pessoal e testes; assinatura e formato devem ser reavaliados em Q-10 antes de qualquer distribuição pública. Assinatura para pessoa física fora dos EUA e do Canadá exige certificado OV pago.
- Se a stack usar WebView2 no modo Evergreen, um processo aberto por dias continua na versão antiga do runtime até recriar o ambiente ou reiniciar.
- Sem atualização automática, que é proibida no MVP, correções de segurança dependem de o usuário instalar a nova versão.

## 10. Verificações realizadas

- 2026-09-26: pesquisa documental em fontes oficiais da Microsoft sobre hooks, raw input, capacidades de apps empacotados, assinatura, SmartScreen, Smart App Control, WebView2 e pastas de dados. Resultado usado nas seções 1 a 4 deste documento e na comparação de DEC-006.
- Nenhuma verificação de código, porque não há código. Nenhum teste de segurança foi executado.

> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de o desenho da chave estável do monitor, da restauração ao iniciar e das mudanças de topologia em execução (decisões D…, regras R…; o código o cita como "desenho dos monitores"), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a Fase 5. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-029 a DEC-031) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

# Fase 5: desenho das áreas (a) chave estável do monitor, (b) restauração ao iniciar e (c) mudanças de topologia em execução

Base da leitura: o núcleo com física vem de `scratchpad\f4` (`Maquina.cs`, `Movimento.cs`, `Configuracao.cs`, `EstadoDoNucleo.cs`). Todo o resto vem da árvore principal. Não editei, compilei nem executei nada; o único cálculo feito foi o SHA-256 das duas strings de exemplo do teste R1, com `sha256sum`. "Hoje" significa o código que li. Tudo abaixo está com STATUS: PLANNED.

**Resumo**
- **Chave:** `mon:` seguido de 16 dígitos hexadecimais do SHA-256 do `monitorDevicePath` do alvo ativo. Quando o caminho não pode ser lido, a chave de reserva é `gdi:` seguido do nome GDI.
  - Para o núcleo, a chave continua sendo uma `string` opaca.
  - `MonitorDoDesktop` não muda.
  - O teste `Retrato.ChaveMonitor` e as topologias de exemplo continuam como estão.
- **Posição guardada:** `PosicaoDoPersonagem` ganha `TelaDoMonitor` como propriedade `init`, fora do construtor posicional.
- **Restauração:** uma função nova, `Posicionador.Restaurar`, segue a ordem chave → mesma tela → monitor principal e vale só na carga. Durante a execução continua valendo `Reacomodar`.
- **`TOPOLOGY_CHANGED` em execução:** três classes, conforme o que aconteceu com o monitor do personagem.
  - **A/T (mesma geometria):** o estado continua. Se o monitor só mudou de lugar no desktop virtual, a âncora é transladada junto.
  - **B (geometria mudou):** `SETTLING` na mesma posição relativa.
  - **C (monitor sumiu):** o personagem vai para o monitor sobrevivente mais próximo, medido nas coordenadas antigas. Passa por `SETTLING` e cai se não houver chão.
- **Posições guardadas:** a posição e o retorno da tela cheia acompanham a topologia em qualquer estado, pela função `Rebasear`.
- **Raiz de composição:**
  - uma minimização feita pelo sistema deixa de virar `CMD_HIDE`;
  - o lugar da janela é conferido de novo pouco depois da releitura;
  - a rajada de mensagens tem um teto;
  - o log ganha o que é preciso para calibrar P5.
- **Núcleo:** nenhum evento, efeito ou campo novo.

---

## 1. Decisões

**D1 — Formato da chave.**
- **Decisão:** a chave é `"mon:"` seguido do hexadecimal minúsculo dos 8 primeiros bytes de `SHA256(UTF8(monitorDevicePath.ToUpperInvariant()))`. Exemplo: `mon:6852e0b1cd2318a2`. Quando o caminho não pode ser lido, usa-se a reserva `"gdi:" + szDevice`, por exemplo `gdi:\\.\DISPLAY2`.
- **Motivo:**
  - ARCHITECTURE 2.4 e DEC-008 pedem o caminho do dispositivo. O resumo é tão estável quanto ele, porque é uma função pura do caminho.
  - Tem tamanho fixo e usa só ASCII, sem espaço, `;`, `,` ou `|`. Esses caracteres quebrariam `Gravacao`, `MonitoresOcupados` e o log.
  - `settings.json` e `diagnostico.log` não guardam o código PnP do modelo nem o caminho de instância, no espírito de SECURITY 6.
  - Os prefixos separam os dois espaços de chaves, então uma chave `gdi:` nunca colide com uma `mon:`.
- **Descartadas:**
  - caminho cru: grava identificador de hardware e é longo;
  - nome GDI como chave principal: muda entre sessões e até na mesma sessão, depois de religar o monitor;
  - `HMONITOR`: só vale durante a execução;
  - códigos EDID mais `connectorInstance`: não distinguem dois monitores iguais e exigem ler campos EDID;
  - WinRT `DisplayMonitor`: acrescenta uma dependência.

**D2 — Clones.**
- **Decisão:** quando uma fonte GDI tem vários alvos ativos, a chave vem do menor caminho, em maiúsculas e comparação ordinal.
- **Motivo:** é determinístico. Se um membro sai do clone ou o clone vira "estender", um dos monitores fica com a chave.
- **Descartadas:** concatenar os caminhos, porque a chave muda sempre que um membro sai; usar a ordem de `QueryDisplayConfig`, que não é garantida.

**D3 — Falha na consulta da chave nunca derruba a leitura.**
- **Decisão:**
  - `ERROR_INSUFFICIENT_BUFFER` leva a até 3 tentativas completas.
  - Qualquer outro erro, ou falha de `DisplayConfigGetDeviceInfo` num caminho, leva à chave do cache (mesmo nome GDI e mesma tela da última consulta boa). Sem cache válido, vale a reserva `gdi:`.
  - A `Topologia` só volta nula pelos motivos de hoje.
- **Motivo:** `LerTopologiaNaPartida` encerra o app com `TopologiaIlegivel` quando a topologia não vem. Sessão remota ou bloqueada pode negar a consulta, o que continua UNCERTAIN até P5.
- **Descartada:** tratar a falha da chave como leitura incoerente. Adiaria cada mudança em cerca de 3,5 s e poderia fechar o app na partida.

**D4 — O núcleo não conhece o formato da chave.**
- **Decisão:** `MonitorDoDesktop(Chave, Tela, AreaUtil, Dpi, Principal)` não muda. O nome GDI fica só no adaptador, em `LeituraDaTopologia.NomeGdiPorChave`, e serve ao log.
- **Motivo:** o núcleo só usa a chave por igualdade: em `PorChave`, `MonitoresOcupados`, `Superficies.Encostado` e na impressão digital. Nenhuma decisão depende do texto. Por isso as topologias de exemplo continuam com `\\.\DISPLAYn`.
- **Descartada:** um campo `NomeGdi` no registro. Ele entraria na igualdade usada por `Clicar` e pela classificação da seção 4, e uma simples renumeração GDI passaria a contar como mudança de monitor.

**D5 — `PosicaoDoPersonagem.TelaDoMonitor`.**
- **Decisão:**
  - A propriedade é `RetanguloPx? { get; init; }`, fora do construtor posicional. Nulo significa desconhecido.
  - É preenchida por `Descrever`, `Reacomodar`, `Restaurar`, `Rebasear` e `Maquina.Validar`.
  - `Gravacao.DescreverPosicao` continua com 5 campos, e as referências 01–05 não mudam.
  - `LerPosicao` passa a aceitar 5 ou 9 campos.
- **Motivo:** as cerca de 20 construções posicionais nos testes continuam compilando. O retângulo "da época" fica junto da posição, inclusive no retorno de tela cheia cujo monitor sumiu.
- **Descartadas:** um 5º parâmetro posicional, que quebraria as construções; guardar o retângulo só no arquivo, que o perde quando o monitor some.

**D6 — Restauração com função própria.**
- **Decisão:**
  - A ordem é: `Restaurar` pela chave, depois pela mesma `Tela`, depois pelo monitor principal.
  - As frações são saneadas e a âncora absoluta não é usada.
  - Em seguida vem `SETTLING`, e o personagem cai se estiver no ar.
- **Motivo:** é o que ARCHITECTURE 2.8 descreve. A âncora de outra sessão fica em outro referencial quando o principal mudou.
- **Descartada:** carregar por `Reacomodar`, como hoje, que escolhe o monitor "mais próximo" de uma âncora de outra sessão.

**D7 — Três classes de `TOPOLOGY_CHANGED` em execução (seção 4, R11).**
- **Motivo:** conectar outro monitor, trocar o principal ou rearranjar não mudam o que está sob os pés do personagem. Interromper uma caminhada ou escalada nesses casos é artefato.
- **Descartadas:**
  - `SETTLING` sempre, como hoje;
  - reancorar também na classe B: exige mais estado, e mudança de chão, escala ou área útil justifica revalidar.

**D8 — Monitor que sumiu: sobrevivente medido nas coordenadas antigas.**
- **Decisão:** o substituto é o sobrevivente mais próximo nas coordenadas antigas, e a âncora é transladada junto com ele (`Rebasear`).
- **Motivo:** quando o principal é desconectado, o Windows move a origem, e o "mais próximo" nas coordenadas novas erra (exemplo em R8).
- **Descartada:** usar `MonitorMaisProximo` com a âncora velha nas coordenadas novas.

**D9 — Mesmo monitor com chave nova (apelido por retângulo).**
- **Decisão:** a chave sumiu, mas existe um monitor com a mesma `Tela` cuja chave não existia antes. É tratado como o mesmo monitor.
- **Motivo:**
  - DEC-008 prevê o retângulo como alternativa de identificação.
  - Cobre a chave que alterna entre `gdi:` e `mon:` e um caminho que muda com o driver.
  - A condição "chave nova" impede confundir com o sobrevivente que o Windows põe na origem quando o principal é desconectado.

**D10 — Pixel dos pés também em `Reacomodar`.**
- **Decisão:** o monitor substituto é o mais próximo de `(x, y − 1)`, a mesma convenção de `Maquina.MonitorDaAncora`.
- **Motivo:** com a barra oculta ou fora da borda de baixo, a âncora no chão fica em `Tela.Base`, que já é o primeiro pixel do monitor de baixo.
- **Descartada:** a âncora crua, que escolheria o monitor de baixo.

**D11 — Posições guardadas acompanham a topologia em qualquer estado.**
- **Decisão:**
  - `Posicao` e `RetornoDaTelaCheia` acompanham toda mudança, sem mover a janela.
  - Escondido, o personagem continua ligado à chave do seu monitor: se esse monitor voltar antes do personagem reaparecer, ele reaparece lá.
- **Descartada:** validar depois com a âncora velha, que estaria em outro referencial.

**D12 — `CLICK` depois de uma mudança valida a partir da posição acompanhada.**
- **Decisão:** usa `Posicao.AncoraAbsoluta`, e não `Lugar.Ancora`.
- **Motivo:** se o principal é trocado com o botão pressionado, `Lugar.Ancora` fica nas coordenadas antigas.

**D13 — Minimização pelo sistema.**
- **Decisão:**
  - Conta como minimização do sistema quando acontece até 3 s depois de um `WM_DISPLAYCHANGE`, quando há uma releitura pendente, ou quando um `WM_DISPLAYCHANGE` chega até 750 ms depois dela.
  - Nesses casos a janela é restaurada sem ativar e a topologia é relida.
  - Fora deles, continua virando `CMD_HIDE` (Q-03).
- **Motivo:** com a opção "Minimizar janelas quando um monitor for desconectado" do Windows 11, hoje o Buzzy some como se o usuário tivesse pedido.
- **Descartadas:** ignorar toda minimização, que quebra Q-03; e manter `CMD_HIDE` sempre, que é o defeito atual.

**D14 — Conferência tardia do lugar da janela.**
- **Decisão:** 1,5 s depois de toda releitura publicada, chamar `ReafirmarLugarDaJanela` uma vez.
- **Motivo:** com "Lembrar locais das janelas", o Windows pode devolver a janela ao monitor reconectado depois da releitura. O núcleo decide ("não pula de volta").
- **Descartada:** tratar `WM_WINDOWPOSCHANGED`. Ele dispara também com movimentos do próprio WPF e tornaria `JanelaMovidaPorFora_...` dependente de corrida.

**D15 — Agrupamento das mensagens.**
- **Decisão:**
  - Os 300 ms continuam provisórios.
  - Uma rajada espera no máximo 1 s desde a primeira mensagem.
  - Cada mensagem crua vai ao log (`MENSAGEM`).
  - `TaskbarCreated` também agenda uma releitura.

**D16 — Nada novo no núcleo.**
- **Decisão:** nenhum evento, efeito ou campo novo. `EstadoDoNucleo`, `Eventos.cs`, `Efeitos.cs` e `Configuracao.cs` só mudam comentários.
- **Motivo:** tudo cabe em `TopologyChanged`, `Loaded.PosicaoSalva` e `GravarPosicao`, o que reduz conflito com as outras áreas.

---

## 2. Tipos e arquivos novos

| Tipo | Arquivo | Camada | Responsabilidade |
|---|---|---|---|
| `AlvoAtivo`, `OrigemDaChave`, `ChaveAtribuida`, `ChavesDeMonitor` | `src/Buzzy.App/Plataforma/ChavesDeMonitor.cs` (novo) | adaptador, lógica pura testável sem hardware | resumo do caminho; clone; cache, reserva e desduplicação |
| `ConsultaDeVideo`, `ConfiguracaoDeVideo` | `src/Buzzy.App/Plataforma/ConfiguracaoDeVideo.cs` (novo) | adaptador (Windows) | `GetDisplayConfigBufferSizes`, `QueryDisplayConfig`, `DisplayConfigGetDeviceInfo` |
| `LeituraDaTopologia` | `src/Buzzy.App/Plataforma/LeitorDeTopologia.cs` | adaptador | topologia, nomes GDI por chave, contagem de reservas e erro da consulta |
| `OrigemDaRestauracao` | `src/Buzzy.Core/Posicionador.cs` | núcleo | `MesmaChave`, `MesmoRetangulo`, `Principal` |

```csharp
// ChavesDeMonitor.cs (Buzzy.App.Plataforma)
internal readonly record struct AlvoAtivo(string NomeGdi, string CaminhoDoDispositivo);
internal enum OrigemDaChave { Caminho, Cache, Reserva }
internal readonly record struct ChaveAtribuida(string NomeGdi, string Chave, OrigemDaChave Origem);

internal static class ChavesDeMonitor
{
    internal const string PrefixoEstavel = "mon:";
    internal const string PrefixoDeReserva = "gdi:";
    internal static string DoCaminho(string caminho);                 // R1
    internal static string DeReserva(string nomeGdi);                  // "gdi:" + nomeGdi
    internal static IReadOnlyDictionary<string, string> Mapear(IEnumerable<AlvoAtivo> alvos); // nome GDI -> chave (R2)
    internal static IReadOnlyList<ChaveAtribuida> Atribuir(
        IReadOnlyList<(string NomeGdi, RetanguloPx Tela)> enumerados,
        IReadOnlyDictionary<string, string>? mapa,                    // nulo = consulta falhou
        IReadOnlyDictionary<string, (string Chave, RetanguloPx Tela)> cache); // R3–R5
}

// ConfiguracaoDeVideo.cs
internal sealed record ConsultaDeVideo(IReadOnlyList<AlvoAtivo> Alvos, string? Erro, int CaminhosSemNome);
internal static class ConfiguracaoDeVideo { internal static ConsultaDeVideo Consultar(); } // nunca lança (R6)

// LeitorDeTopologia.cs
internal sealed record LeituraDaTopologia(Topologia Topologia, IReadOnlyDictionary<string, string> NomeGdiPorChave,
                                          int ChavesDeReserva, string? ErroDaConsulta);
internal static LeituraDaTopologia? LerDetalhado(out string? erro);   // novo; só na thread da interface
internal static Topologia? Ler(out string? erro) => LerDetalhado(out erro)?.Topologia; // mantém os testes

// Posicionador.cs (Buzzy.Core)
public sealed record PosicaoDoPersonagem(string ChaveMonitor, double FracaoX, double FracaoY, PontoPx AncoraAbsoluta)
{
    /// Tela do monitor quando a posição foi descrita (ARCHITECTURE 2.8); nulo = desconhecida.
    public RetanguloPx? TelaDoMonitor { get; init; }
}
public enum OrigemDaRestauracao { MesmaChave, MesmoRetangulo, Principal }
public static PontoPx PixelDosPes(PontoPx ancora) => new(ancora.X, ancora.Y - 1);
public static (Posicionamento Resultado, PosicaoDoPersonagem NovaPosicao, OrigemDaRestauracao Origem)
    Restaurar(Topologia topologia, PosicaoDoPersonagem salva, TamanhoDip tamanho);            // R7
public static bool SoTranslacao(MonitorDoDesktop antes, MonitorDoDesktop depois, out int dx, out int dy); // R9
public static MonitorDoDesktop? MonitorCorrespondente(Topologia antiga, Topologia nova, string chave, RetanguloPx? tela); // R10
public static PosicaoDoPersonagem Rebasear(Topologia antiga, Topologia nova, PosicaoDoPersonagem p, TamanhoDip tamanho); // R8
```

**Testes novos** (detalhes na seção 5):
- `tests/Buzzy.Core.Testes/RestaurarTestes.cs`
- `tests/Buzzy.Core.Testes/RebasearTestes.cs`
- `tests/Buzzy.Core.Testes/Personagem/RestauracaoTestes.cs`
- `tests/Buzzy.Core.Testes/Personagem/MudancaDeTopologiaTestes.cs`
- `tests/Buzzy.Core.Testes/Movimento/TopologiaEmMovimentoTestes.cs`
- `tests/Buzzy.App.Testes/ChavesDeMonitorTestes.cs`
- `tests/Buzzy.App.Testes/LeitorDeTopologiaTestes.cs`
- `tests/Buzzy.App.Testes/Integracao/MultiMonitorIntegracaoTestes.cs`

**Auxiliares de teste:**
- Em `TopologiasDeExemplo`, fora de `Todas` para não mudar as contagens das propriedades:
  - `Rebaseada(t, novoPrincipal)`;
  - `SemOPrincipal(t, removido, novoPrincipal)`;
  - `ComChavesRenomeadas(t, Func<string,string>)`;
  - `TresEmLinhaComPrincipalNoMeio`.
- Em `GeradorDeTopologias`: o método novo `MudarComIdentidade(t)` (renomear chaves, só trocar o principal, desconectar com rebase). Fica separado para não alterar o fluxo aleatório de `Mudar`, que as propriedades existentes usam.

---

## 3. Mudanças em arquivos existentes

### `src/Buzzy.App/Plataforma/Win32.cs`

Nova seção "Configuração de vídeo (chave estável do monitor)":

```csharp
internal const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;
internal const uint DISPLAYCONFIG_PATH_ACTIVE = 0x00000001;
internal const int DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;
internal const int DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2;
internal const int ERROR_INSUFFICIENT_BUFFER = 122;
internal const int SW_SHOWNOACTIVATE = 4;

[StructLayout(LayoutKind.Sequential)] internal struct LUID { public uint LowPart; public int HighPart; }            // 8
[StructLayout(LayoutKind.Sequential)] internal struct DISPLAYCONFIG_RATIONAL { public uint Numerator, Denominator; } // 8
[StructLayout(LayoutKind.Sequential)] internal struct DISPLAYCONFIG_PATH_SOURCE_INFO
{ public LUID adapterId; public uint id; public uint modeInfoIdx; public uint statusFlags; }                       // 20
[StructLayout(LayoutKind.Sequential)] internal struct DISPLAYCONFIG_PATH_TARGET_INFO
{ public LUID adapterId; public uint id; public uint modeInfoIdx; public int outputTechnology; public int rotation;
  public int scaling; public DISPLAYCONFIG_RATIONAL refreshRate; public int scanLineOrdering; public int targetAvailable;
  public uint statusFlags; }                                                                                        // 48
[StructLayout(LayoutKind.Sequential)] internal struct DISPLAYCONFIG_PATH_INFO
{ public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo; public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo; public uint flags; } // 72
// A união (targetMode/sourceMode/desktopImageInfo) nunca é lida; só o tamanho importa.
[StructLayout(LayoutKind.Explicit, Size = 64)] internal struct DISPLAYCONFIG_MODE_INFO
{ [FieldOffset(0)] public int infoType; [FieldOffset(4)] public uint id; [FieldOffset(8)] public LUID adapterId; }  // 64
[StructLayout(LayoutKind.Sequential)] internal struct DISPLAYCONFIG_DEVICE_INFO_HEADER
{ public int type; public uint size; public LUID adapterId; public uint id; }                                      // 20
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] internal struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
{ public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string viewGdiDeviceName; }                          // 84
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] internal struct DISPLAYCONFIG_TARGET_DEVICE_NAME
{ public DISPLAYCONFIG_DEVICE_INFO_HEADER header; public uint flags; public int outputTechnology;
  public ushort edidManufactureId; public ushort edidProductCodeId; public uint connectorInstance;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string monitorFriendlyDeviceName;  // recebido e descartado
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string monitorDevicePath; }                         // 420

[DllImport("user32.dll", ExactSpelling = true)]
internal static extern int GetDisplayConfigBufferSizes(uint flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);
[DllImport("user32.dll", ExactSpelling = true)]
internal static extern int QueryDisplayConfig(uint flags, ref uint numPathArrayElements, [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
    ref uint numModeInfoArrayElements, [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, nint currentTopologyId /* sempre 0 */);
[DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo", ExactSpelling = true)]
internal static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SOURCE_DEVICE_NAME requestPacket);
[DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo", ExactSpelling = true)]
internal static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME requestPacket);
[DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
internal static extern bool ShowWindow(nint hWnd, int nCmdShow);   // só na janela do próprio Buzzy
```

- As funções devolvem o código Win32 direto, sem `SetLastError`.
- `header.size` recebe `(uint)Marshal.SizeOf<T>()`.
- Nenhuma dessas funções está na lista proibida de `tools/Buzzy.PortaoApis/Regras.cs`. Não usar `CreateDC`, que é proibida.

### `src/Buzzy.App/Plataforma/LeitorDeTopologia.cs`

`LerDetalhado` faz a leitura assim:
1. `ConfiguracaoDeVideo.Consultar()`.
2. `EnumDisplayMonitors` como hoje, guardando `(szDevice, rcMonitor)` na ordem do Windows.
3. `Atribuir(...)`.
4. Se a consulta não teve `Erro`, substitui o cache estático (`nomeGdi → (chave, tela)`) só pelas chaves de origem `Caminho`.
5. Cria `MonitorDoDesktop(chave, …)` e `new Topologia(...)` com as validações de hoje.

O cache pertence à thread da interface. `Ler(out erro)` fica como invólucro para `IntegracaoTestes`, `GestosTestes` e `MovimentoIntegracaoTestes`.

### `src/Buzzy.Core/Topologia.cs`

Só o comentário de `MonitorDoDesktop.Chave`: "chave estável opaca, `mon:`/`gdi:` no app; o núcleo só compara por igualdade".

### `src/Buzzy.Core/Posicionador.cs`

- `PosicaoDoPersonagem.TelaDoMonitor` (D5).
- `Descrever` passa a preencher `TelaDoMonitor = p.Monitor.Tela`.
- `Reacomodar`:
  - no ramo da mesma chave, passa a devolver `atual with { AncoraAbsoluta = r.Ancora, TelaDoMonitor = mesmo.Tela }`;
  - no ramo do monitor ausente, usa `nova.MonitorMaisProximo(PixelDosPes(atual.AncoraAbsoluta))` (D10).
- Novas: `PixelDosPes`, `Restaurar`, `SoTranslacao`, `MonitorCorrespondente`, `Rebasear` e `OrigemDaRestauracao` (seção 4).

### `src/Buzzy.Core/Personagem/Maquina.cs` (versão de f4)

- **`MonitorDaAncora`:** passa a usar `Posicionador.PixelDosPes`, sem mudar o comportamento.
- **`Carregar`:**
  - com `PosicaoSalva`, chama `Posicionador.Restaurar(...)`;
  - a regra passa a ser `"BOOTING: configurações e topologia carregadas (posição salva: mesma chave|mesmo retângulo|monitor principal)"`;
  - sem posição salva, o texto não muda (as referências continuam iguais).
- **`MudarTopologia`:** reescrita (R11).
- **`ContinuarNoMonitor(MonitorDoDesktop novo, int dx, int dy)`:** novo método privado (R12).
- **`Clicar`:** a âncora desejada da validação passa a ser `_s.Posicao?.AncoraAbsoluta ?? lugar.Ancora` (R14).
- **`Validar`:** `preferida with { AncoraAbsoluta = presa, TelaDoMonitor = monitor.Tela }`.
- **Não toca** em `PassoAndando`, `PassoEscalando`, `PassoPendurado`, `PassoNoAr` nem nos `Planejar*`. São da área de travessia.

### `src/Buzzy.Core/Personagem/Gravacao.cs`

- `LerPosicao` aceita 5 campos (`chave;fx;fy;ax;ay`) ou 9 (`…;l;t;r;b` = `TelaDoMonitor`). Qualquer outra quantidade lança `FormatException`.
- Nova `DescreverPosicaoCompleta`, usada só por `Escrever(Loaded)` quando `TelaDoMonitor` existe.
- `DescreverPosicao`, usada por `GravarPosicao`, continua com 5 campos.

### `Eventos.cs`, `Efeitos.cs`, `EstadoDoNucleo.cs` e `Configuracao.cs`

Só comentários:
- `Loaded`: "posição salva restaurada por `Posicionador.Restaurar`".
- `GravarPosicao`: "inclui `TelaDoMonitor`".
- `EstadoDoNucleo.Posicao` e `RetornoDaTelaCheia`: "acompanham a topologia em qualquer estado".

### `src/Buzzy.App/Apresentacao/JanelaPersonagem.cs`

- `AoMudarEstado` deixa de fazer `WindowState = Normal`. Só dispara `Minimizada`, e quem decide é a raiz.
- `internal void RestaurarSemAtivar() => Win32.ShowWindow(Hwnd, Win32.SW_SHOWNOACTIVATE);`
- `internal void RestaurarEstadoNormal() => WindowState = WindowState.Normal;` mantém o comportamento atual no caso do usuário.

### `src/Buzzy.App/Composicao/Aplicacao.cs`

**Campos novos:**
- `LeituraDaTopologia _leitura`
- `DispatcherTimer _confirmarMinimizacao` (750 ms, dispara uma vez)
- `DispatcherTimer _reafirmarDepois` (1,5 s, dispara uma vez)
- `long _ultimoDisplayChange` e `long _inicioDaRajada` (marcas do `Stopwatch`)
- `bool _minimizacaoPendente`

**Constantes novas**, todas provisórias até P5: `AgrupamentoMaximo = 1 s`, `JanelaDaMinimizacaoDoSistema = 3 s`, `EsperaPelaMensagemDepoisDeMinimizar = 750 ms`, `ReafirmacaoTardia = 1,5 s`.

**Métodos que mudam:**
- **`Iniciar`:**
  - `LerTopologiaNaPartida` passa a usar `LerDetalhado`;
  - `Loaded(topologia, posicaoSalva, preferencias)`, com `posicaoSalva` vindo da área de persistência;
  - `_personagem.Minimizada += AoMinimizar;` no lugar do `CmdHide` direto.
- **`AoPossivelMudancaDeTopologia(motivo)`:**
  - registra `MENSAGEM|tipo=motivo`;
  - se `motivo == "WM_DISPLAYCHANGE"`, atualiza `_ultimoDisplayChange` e, se `_minimizacaoPendente`, chama `Adiar(() => RecuperarMinimizacaoDoSistema("minimizada antes da mensagem"))`;
  - depois, `AgendarReleitura(motivo)` (R17).
- **`AoAgrupar`:**
  - usa `LerDetalhado` e guarda `_leitura`;
  - `Enviar(TopologyChanged)`, `ReafirmarLugarDaJanela`, reinicia `_reafirmarDepois`;
  - `TOPOLOGIA` ganha `chaves=mon:…=\\.\DISPLAY1;…`, `consulta=ok|<erro>` e `reserva=n`.
- **`RegistrarPosicao`:** acrescenta `("gdi", _leitura.NomeGdiPorChave[p.Monitor.Chave])`. O campo `monitor` continua sendo a chave.
- **`AoRecriarBarra`:** continua lendo para o DPI do ícone e passa a chamar `AoPossivelMudancaDeTopologia("TaskbarCreated")`. Hoje ele atualiza `_topologia` sem avisar o núcleo.
- **`MostrarPorComando`:** usa `LerDetalhado`.
- **`EncerrarAplicacao`:** para os dois temporizadores novos.

**Métodos novos:** `AoMinimizar`, `AoConfirmarMinimizacao`, `RecuperarMinimizacaoDoSistema` e `AgendarReleitura` (R16–R18).

### Testes existentes que mudam

- **`TelaCheiaTestes.FimDaTelaCheia_MonitorDoRetornoRemovido_RestauraNoMonitorMaisProximo`, linha 129:** `.Percorreu(Estado.Idle, Estado.Settling, Estado.Idle)` passa a ser `.Percorreu(Estado.Idle, Estado.Idle)`. O DISPLAY1 não mudou; é classe A.
- **`PropriedadesTestes.ReacomodarSempreTerminaNaAreaUtilDeUmMonitorDaNovaTopologia`:**
  - linha 111: `posicao == (atual with { AncoraAbsoluta = r.Ancora, TelaDoMonitor = r.Monitor.Tela })`;
  - linhas 115–118: distâncias medidas a `Posicionador.PixelDosPes(atual.AncoraAbsoluta)`.
- **`ReacomodarTestes`, linha 139:** só o comentário muda (√161858 vira √161153); a afirmação continua válida.
- **`PlataformaTestes.EstruturasNativasTemOTamanhoQueOWindowsEspera`:** passa a verificar os tamanhos 8/20/48/72/64/20/84/420.
- **`NativoTeste`:** ganha `IsIconic`.
- **`tests/Buzzy.Verificacao/VerificacaoFase3.cs`, linhas 260–261:** comparar `posicao?["gdi"]` com `sec.Info.szDevice`.
- **`VerificacaoFase4.TopologiaLida` (f4)** continua válida com `szDevice` como chave, porque nenhuma decisão depende do texto da chave (D4).

### `docs/ARCHITECTURE.md`

**2.4:** formato da chave, reserva e apelido por retângulo.

**2.8:**
- restauração por `Restaurar`;
- regras de execução com as classes A/T, B e C;
- sobrevivente medido nas coordenadas antigas;
- posições acompanhando a topologia.

**2.13.3:** linha "Identidade estável do monitor" com as APIs acima.

**2.6, linha que é substituída:** `estados autônomos, físicos e REACTING | TOPOLOGY_CHANGED | SETTLING | Revalida a posição.` sai e entram as três linhas abaixo.

| De | Evento | Para | Regra |
|---|---|---|---|
| estados autônomos, físicos e `REACTING` | `TOPOLOGY_CHANGED` sem mudança na geometria do monitor do personagem: só outros monitores mudaram, ou o monitor foi só transladado no desktop virtual (rearranjo, troca de principal), ou só a chave mudou com a mesma tela | permanece | O movimento continua. A âncora, a posição fina e a janela acompanham a translação. Se o apoio do estado deixou de existir (a lateral da escalada virou passagem), vai para `SETTLING`. Uma caminhada que ia escalar e perdeu a parede-alvo termina no passo seguinte. |
| estados autônomos, físicos e `REACTING` | `TOPOLOGY_CHANGED` com o monitor do personagem mudado (resolução, escala, orientação ou área útil) | `SETTLING` | Mesma posição relativa na área útil nova e tamanho físico recalculado. Sem apoio, `FALLING`. |
| estados autônomos, físicos e `REACTING` | `TOPOLOGY_CHANGED` sem o monitor do personagem | `SETTLING` | Vai ao sobrevivente mais próximo da última âncora, medido nas coordenadas de antes e transladado junto dele, na mesma posição relativa. A posição passa a ser desse monitor e não volta sozinha. Sem apoio, cai. |

**2.6, linhas alteradas:**
- **`PRESSED, DRAGGING | TOPOLOGY_CHANGED` e `BOOTING, HIDDEN, EXITING | TOPOLOGY_CHANGED`:** acrescentar "a posição e o retorno da tela cheia acompanham a nova topologia (translação ou sobrevivente), sem mover a janela; escondido, continua ligado à chave do monitor".
- **`BOOTING | carregadas`:** "restaurada por chave → mesma tela → principal (2.8)".
- **`PRESSED | CLICK`:** "valida a partir da posição que acompanhou a topologia".

**2.6, invariantes novos** (numeração provisória):
- **18.** `TOPOLOGY_CHANGED` que não muda a geometria do monitor do personagem não troca o estado de comportamento, salvo perda de apoio. A âncora se desloca exatamente pela translação do monitor.
- **19.** Depois de `TOPOLOGY_CHANGED`, visível e fora de `PRESSED`/`DRAGGING`, `Lugar.Monitor` pertence à topologia nova e a âncora está na área útil dele.

### Outros documentos

- **`docs/DECISIONS.md`:** uma DEC nova (número provisório DEC-023) com D1–D16.
- **`docs/SECURITY.md`:**
  - 2 e 3.1: "configuração de vídeo lida só para a chave; o caminho vira resumo e nunca é gravado nem registrado; nome amigável e códigos EDID descartados; `ShowWindow` só na própria janela";
  - 5: "chave = resumo do caminho".
- **`docs/TODO.md`:** tarefas e evidências da Fase 5.

---

## 4. Regras exatas

**R1. Chave estável.**
`DoCaminho(c) = "mon:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(c.ToUpperInvariant())).AsSpan(0, 8))`.
Valores fixos para o teste de regressão, com a caixa original dos exemplos:
- `@"\\?\DISPLAY#DEL40F4#5&2a3b4c5d&0&UID4352#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7}"` → `mon:6852e0b1cd2318a2`
- `@"\\?\DISPLAY#GSM5B09#4&1C2D3E4F&0&UID4353#{E6F07B5F-EE97-4A90-B076-33F57BF4EAA7}"` → `mon:0994facb2eabe85c`

Essa função nunca pode mudar entre versões, senão as chaves gravadas deixam de valer.

**R2. `Mapear`.**
- Agrupa os alvos por `NomeGdi` (`OrdinalIgnoreCase`) e descarta caminhos vazios.
- Escolhe o menor `CaminhoDoDispositivo.ToUpperInvariant()` por `string.CompareOrdinal` e aplica `DoCaminho(menor)`.
- No clone dos dois exemplos, `DEL…` < `GSM…` e a chave é `mon:6852e0b1cd2318a2`.

**R3. `Atribuir`, na ordem de `EnumDisplayMonitors`.**
1. Se `mapa` contém o nome, a origem é `Caminho`.
2. Senão, se `cache[nome].Tela == tela`, a origem é `Cache`.
3. Senão, `DeReserva(nome)` com origem `Reserva`.

**R4. Nenhuma chave repetida.** Uma chave já usada vira `DeReserva(nome)`. Nomes GDI são únicos, então a reserva também é, e `new Topologia` nunca lança por chave repetida.

**R5. Cache.** Só é substituído (inteiro) quando `Consultar()` volta sem `Erro`. Um caminho sem nome conta em `CaminhosSemNome` e não invalida os outros.

**R6. `Consultar`.**
- Até 3 vezes:
  1. `GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out np, out nm)`. Se o retorno não é 0, erro.
  2. Aloca `PATH_INFO[np]` e `MODE_INFO[nm]`.
  3. `QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref np, …, ref nm, …, 0)`. Com 122, tenta de novo; com outro valor diferente de 0, erro.
  4. Corta os vetores para os `np` devolvidos.
- Para cada caminho com `(flags & DISPLAYCONFIG_PATH_ACTIVE) != 0` e `targetInfo.targetAvailable != 0`:
  - nome da fonte: `GET_SOURCE_NAME` com `header.adapterId = sourceInfo.adapterId` e `header.id = sourceInfo.id`, guardado em memória por `(adapterId, id)` para os clones;
  - caminho do alvo: `GET_TARGET_NAME` com `targetInfo.adapterId` e `targetInfo.id`;
  - retorno diferente de 0 ou texto vazio conta como caminho sem nome.
- `EntryPointNotFoundException` e `DllNotFoundException` viram `Erro`.

**R7. `Restaurar(t, s, tam)`.** Com `S(f)` = NaN → 0,5 e depois `Clamp(f, 0, 1)`:
1. Se `M = t.PorChave(s.ChaveMonitor)` existe: `r = NoMonitor(M, S(fx), S(fy))`. Devolve `(r, s with { FracaoX=S(fx), FracaoY=S(fy), AncoraAbsoluta=r.Ancora, TelaDoMonitor=M.Tela }, MesmaChave)`.
2. Senão, se `s.TelaDoMonitor` não é nulo e existe `R` com `R.Tela == s.TelaDoMonitor`: igual ao passo 1 com `ChaveMonitor = R.Chave`, origem `MesmoRetangulo`. Não há dois monitores com a mesma tela.
3. Senão, `r = NoMonitor(t.Principal, S(fx), S(fy))` e devolve `(r, Descrever(r), Principal)`.

Em seguida, `Carregar` chama `Acomodar(r.Ancora, regra, posicao)`: `SETTLING`, depois `IDLE` com apoio ou `FALLING` sem apoio. A âncora absoluta salva nunca é usada.

**R8. `Rebasear(antiga, nova, p, tam)`.**
1. Se `N = MonitorCorrespondente(antiga, nova, p.ChaveMonitor, p.TelaDoMonitor)` existe: `a = NoMonitor(N, fx, fy).Ancora` e devolve `p with { ChaveMonitor = N.Chave, AncoraAbsoluta = a, TelaDoMonitor = N.Tela }`.
2. Senão, com `ref = PixelDosPes(p.AncoraAbsoluta)`, os sobreviventes são os `n ∈ nova` com `O = antiga.PorChave(n.Chave)` não nulo.
   - `S` é o que minimiza `O.Tela.DistanciaAoQuadrado(ref)`. No empate vence `n.Principal` e depois a ordem de `nova.Monitores`.
   - Sem sobrevivente, devolve `p`.
   - Com `(dx, dy) = (S.Tela.Esquerda − O_S.Tela.Esquerda, S.Tela.Topo − O_S.Tela.Topo)`, devolve `p with { AncoraAbsoluta = p.AncoraAbsoluta + (dx,dy), TelaDoMonitor = p.TelaDoMonitor?.Deslocado(dx, dy) }`.
   - A chave e as frações ficam iguais.

**R9. `SoTranslacao(M, N, out dx, out dy)`.**
- `dx = N.Tela.Esquerda − M.Tela.Esquerda` e `dy = N.Tela.Topo − M.Tela.Topo`.
- É verdadeiro se, e só se, `N.Tela == M.Tela.Deslocado(dx,dy)`, `N.AreaUtil == M.AreaUtil.Deslocado(dx,dy)` e `N.Dpi == M.Dpi`. `Principal` é ignorado.
- `dx = dy = 0` é a classe A pura.

**R10. `MonitorCorrespondente(antiga, nova, k, tela)`.**
- Devolve `nova.PorChave(k)`.
- Se for nulo e `tela` não for nulo, devolve o primeiro `n ∈ nova` com `n.Tela == tela` e `antiga.PorChave(n.Chave) == null`.
- Caso contrário, nulo.

**R11. `Maquina.MudarTopologia(nova)`:**
```
antiga = _s.Topologia; _s.Topologia = nova
se antiga ≠ null e antiga.MesmaConfiguracao(nova): fim            // como hoje
se antiga ≠ null: Posicao = Rebasear(antiga, nova, Posicao); RetornoDaTelaCheia = Rebasear(antiga, nova, RetornoDaTelaCheia)
se estado ∉ {autônomos, físicos, REACTING} ou Posicao/Lugar nulos: fim   // PRESSED, DRAGGING, BOOTING, HIDDEN e EXITING
N = antiga ≠ null ? MonitorCorrespondente(antiga, nova, Lugar.Monitor.Chave, Lugar.Monitor.Tela) : nova.PorChave(...)
mesmaGeometria = N ≠ null e SoTranslacao(Lugar.Monitor, N, out dx, out dy)
se mesmaGeometria e ContinuarNoMonitor(N, dx, dy): fim             // classes A e T
(r, pos) = Reacomodar(nova, Posicao, Tamanho)                      // mesma chave: frações; ausente: pixel dos pés
regra = N == null ? "TOPOLOGY_CHANGED: o monitor do personagem foi desconectado"
      : mesmaGeometria ? "TOPOLOGY_CHANGED: o apoio deixou de existir" : "TOPOLOGY_CHANGED"   // classe B mantém o texto (referência 05)
Acomodar(r.Ancora, regra, pos)
```

**R12. `ContinuarNoMonitor(N, dx, dy)`.** Devolve falso sem alterar nada se o apoio deixou de existir.
1. Calcula `mv = Movimento with { X += dx, Y += dy }` e `sup = Superficies.Do(nova, N, Lugar.Tamanho)`.
2. Se `_cfg.Movimento`, o estado é `CLIMBING` e não vale `sup.NaParede(mv.X, out lado) && lado == Sentido`, devolve falso.
3. Se `_cfg.Movimento`, o estado é `WALKING`, `mv.QuerEscalar` é verdadeiro e a lateral à frente não é parede (`Sentido>0 ? !sup.ParedeDireita : !sup.ParedeEsquerda`), faz `mv = mv with { QuerEscalar = false, Restante = 0 }`: no passo seguinte vale `"WALKING: fim do percurso"`.
4. Faz `Lugar = new Posicionamento(N, Ancora+(dx,dy), Tamanho, Retangulo.Deslocado(dx,dy))` e `Movimento = mv`.
5. Registra `Transicao(e, e, dx==0&&dy==0 ? (N.Chave==chaveAntiga ? "TOPOLOGY_CHANGED: o monitor do personagem não mudou" : "TOPOLOGY_CHANGED: o monitor do personagem mudou de chave") : $"TOPOLOGY_CHANGED: o monitor do personagem foi transladado ({dx},{dy})")`, com cultura invariante.
6. Não ativa `_reagendar`, e o relógio não muda.
7. `Concluir` emite `MoverJanela` só se `Lugar` mudou por valor.

**R13. Estado × classe** (A/T = mesma geometria; B = geometria mudou; C = sumiu):

| Estado | A/T | B | C |
|---|---|---|---|
| `IDLE`, `RESTING`, `LANDING`, `REACTING` | permanece; âncora + (dx,dy) | `SETTLING` → `IDLE` no chão novo (a reação termina) | `SETTLING` → `IDLE`/`FALLING` |
| `WALKING` | continua (X fino, `Restante` e `Sentido` mantidos); regra do passo 3 de R12 | `SETTLING` → `IDLE` (fy = 1) | `SETTLING` → `IDLE` |
| `CLIMBING` | continua se a lateral ainda é parede; senão `SETTLING` → `FALLING` | `SETTLING` → `FALLING` na altura relativa | `SETTLING` → `FALLING` |
| `HANGING` | continua (o teto translada junto) | `SETTLING` → `FALLING` | `SETTLING` → `FALLING` |
| `JUMPING`, `FALLING` | continua com `VX`/`VY` mantidos | `SETTLING` → `FALLING` com velocidade zerada (`IrPara` reinicia `Movimento`) | idem |
| `PRESSED`, `DRAGGING`, `HIDDEN`, `BOOTING`, `EXITING` | só cache e posições acompanhadas (R8) | idem | idem |

**R14. `Clicar` com o monitor mudado.** A condição de hoje continua: `!Equals(topologia.PorChave(lugar.Monitor.Chave), lugar.Monitor)`. Ela passa a chamar `Validar(_s.Posicao?.AncoraAbsoluta ?? lugar.Ancora, _s.Posicao)`.

**R15. Exemplos com coordenadas negativas** (sprite de 128 DIP):

| Caso | Topologia e mudança | Personagem antes | Resultado esperado |
|---|---|---|---|
| a. Troca de principal | S2 `SecundarioAEsquerda` → `Rebaseada(S2, DISPLAY2)`: DISPLAY2 vai a (0,0)-(1920,1080) e DISPLAY1 a (1920,0)-(3840,1080) | andando no DISPLAY2 em (−1440,1032) | classe T, d = (+1920, 0): continua andando em (480,1032) |
| b. Desconectar o principal | S2, DISPLAY1 desconectado; o DISPLAY2 vira principal em (0,0) | DISPLAY1, fx 0,85, (1632,1032) | sem apelido, porque o DISPLAY2 já existia; sobrevivente DISPLAY2, d = (+1920,0); âncora rebaseada (3552,1032); `Reacomodar` escolhe o DISPLAY2 em (1632,1032); `SETTLING` → `IDLE` |
| c. D8 decide | `TresEmLinhaComPrincipalNoMeio` \[2\](−1920..0) \[1*\](0..1920) \[3\](1920..3840); desconectar o 1 e promover o 2: rebase +1920, 2 → (0..1920), 3 → (3840..5760) | 1, fx 0,85, (1632,1032) | nas coordenadas antigas, o 3 fica a 288 px e o 2 a 1633; sobrevivente 3, d = (+1920,0); âncora (3552,1032); resultado no 3 em (5472,1032). Com a âncora velha nas coordenadas novas, iria errado para o 2. Promovendo o 3 (rebase −1920), o resultado é o 3 em (1632,1032) |
| d. y negativo | S3 `EmpilhadoSecundarioAcima`; DISPLAY2 área (−320,−1440)-(2240,−48), desconectado | pendurado em (1000, −1312); fx = 1320/2560 = 0,515625; fy = 128/1392 | vai ao DISPLAY1: x = 990, y = round(94,9) = 95, preso a 128 → (990,128) → `FALLING` → pousa em (990,1032) |
| e. Troca de principal | S3 → `Rebaseada(S3, DISPLAY2)`, d = (+320, +1440) | andando no DISPLAY1 em (1632,1032) | continua em (1952,2472) |
| f. Restauração pelo retângulo | S2 com as chaves renomeadas para `mon:…1`/`mon:…2`; salva `\\.\DISPLAY2`, fx 0,25, fy 1, tela (−1920,0)-(0,1080) | — | `MesmoRetangulo`, (−1440,1032), chave `mon:…2` |
| g. S9 | salva no DISPLAY2 com tela; `Loaded(SemMonitor(S2, DISPLAY2))` | — | `Principal` em (480,1032). `TopologyChanged(S2)`: classe A no DISPLAY1, fica em (480,1032); `CMD_EXIT` grava DISPLAY1 |
| h. Classe B | S2, DISPLAY1 a 144 DPI com área (0,0)-(1920,1008) | andando em (1632,1032) | `SETTLING` → (1632,1008) com 192×192 px → `IDLE` |

**R16. Minimização.**
- **Em `AoMinimizar`:** se `_agrupador.IsEnabled` ou se `_ultimoDisplayChange ≠ 0` e `Stopwatch.GetElapsedTime(_ultimoDisplayChange) ≤ 3 s`, chama `Adiar(() => RecuperarMinimizacaoDoSistema("depois da mensagem"))`. Senão, `_minimizacaoPendente = true` e reinicia `_confirmarMinimizacao` (750 ms).
- **Na mensagem:** um `WM_DISPLAYCHANGE` durante a espera faz a recuperação.
- **No disparo do temporizador:** se ainda pendente, `RestaurarEstadoNormal()`, `JANELA|minimizada=usuario` e `Enviar(new CmdHide(), "minimizado")`, como hoje.
- **`RecuperarMinimizacaoDoSistema(ordem)`:** zera a pendência, para o temporizador, chama `RestaurarSemAtivar()`, registra `JANELA|minimizada=sistema|ordem=…` e `AgendarReleitura("minimizada pelo sistema")`.
- `WM_DPICHANGED` e `SPI_SETWORKAREA` não contam para essa correlação. O `WM_DPICHANGED` da própria janela aparece quando ela cruza monitores.

**R17. `AgendarReleitura(motivo)`.**
- Faz `_motivosPendentes.Add(motivo)` e `_releiturasFalhas = 0`.
- Se o temporizador está ligado e `Stopwatch.GetElapsedTime(_inicioDaRajada) ≥ 1 s`, deixa disparar.
- Senão, `_inicioDaRajada` recebe o instante atual (só se o temporizador estava parado) e o temporizador reinicia com 300 ms.

**R18. Conferência tardia.**
- Toda releitura publicada em `AoAgrupar` reinicia `_reafirmarDepois` (1,5 s). No disparo: `ReafirmarLugarDaJanela("reafirmação tardia")`.
- A função de hoje já ignora `PRESSED`/`DRAGGING` e janela invisível.
- Não é periódica: há um disparo por rajada.

---

## 5. Testes [AUTO] propostos

### `tests/Buzzy.Core.Testes/RestaurarTestes.cs` (S1–S7, S9)

- **`Restaurar_ChaveExistente_AplicaAPosicaoRelativaNaAreaUtilAtual`:** o DISPLAY2 de S2 passa a 2560×1440 em (−2560,−360)-(0,1080). Espera a origem `MesmaChave`, x = −2560 + 0,25·2560 = −1920, y = 1032 e `TelaDoMonitor` nova.
- **`Restaurar_ChaveAusenteComMesmoRetangulo_UsaEsseMonitor`:** exemplo R15f.
- **`Restaurar_SemChaveNemRetangulo_VaiAoPrincipalMesmoQueNaoSejaOMaisAEsquerda`:** S7 `PrincipalADireita` com fx 0,1. Espera o DISPLAY1 em (192,1032) e a origem `Principal`.
- **`Restaurar_TelaNula_PulaOPasso2`:** posição posicional com chave ausente vai ao principal.
- **`Restaurar_FracoesInvalidas_SaoSaneadas`:** NaN vira 0,5, 3 vira 1, −1 vira 0.
- **`Restaurar_EmTodasAsTopologiasDeExemplo`:** para cada topologia de `Todas`, cada monitor e as frações {0; 0,25; 0,5; 1}, confere:
  - a mesma chave;
  - a âncora na área útil;
  - o tamanho `128·dpi/96`;
  - a idempotência: `Restaurar(t, resultado.NovaPosicao)` dá o mesmo resultado.
- **`Restaurar_EscalasMistas_S5`:** DISPLAY3 a 192 DPI, fx 0,5 e fy 1 dão âncora (−1920,1344), sprite 256×256 e retângulo (−2048,1088)-(−1792,1344).

### `tests/Buzzy.Core.Testes/RebasearTestes.cs` (S2, S3, S7)

- **`SoTranslacao_TrocaDePrincipalERearranjo`:** exemplos R15a e R15e dão verdadeiro com (dx,dy). Mudar `AreaUtil` ou `Dpi` dá falso; topologia idêntica dá (0,0).
- **`MonitorCorrespondente_ApelidoPorRetangulo`:** chaves renomeadas com as mesmas telas encontram o monitor.
- **`MonitorCorrespondente_SobreviventeNaOrigemNaoEhApelido`:** no exemplo R15b, o resultado é nulo.
- **`Rebasear_MonitorPresente_AcompanhaAsFracoes`:** a âncora vai para as coordenadas novas.
- **`Rebasear_MonitorSumido_TransladaComOSobreviventeNasCoordenadasAntigas`:** R15c nos dois sentidos de promoção.
- **`Rebasear_SemSobrevivente_DevolveAPosicao`:** todas as chaves são novas e as telas diferentes.
- **`Reacomodar_MonitorSumido_UsaOPixelDosPes`:**
  - topologia `{X (0,0)-(1920,1080) com área = tela; D2 (0,1080)-(1920,2160)}`;
  - posição com chave ausente e âncora (960,1080);
  - espera o monitor X; a âncora crua escolheria o D2.

### `tests/Buzzy.Core.Testes/Personagem/RestauracaoTestes.cs` (máquina)

- **`Loaded_ChaveAusenteMesmoRetangulo_RestauraNoMonitorDoRetangulo`:** caminho `Booting → Settling → Idle` e regra contendo "mesmo retângulo".
- **`Loaded_MonitorSalvoAusente_VaiAoPrincipalENaoVoltaQuandoReconecta`:** exemplo R15g, em S9. Confere:
  - `Idle → Idle` com a regra "não mudou" e sem `MoverJanela`;
  - `Retrato.ChaveMonitor == DISPLAY1`;
  - `CmdExit` produz `GravarPosicao` no DISPLAY1.
- **`Loaded_PosicaoNoAr_SettlingECai`:** com a configuração de `MovimentoTestes.Fase4()` e fy 0,5, `Booting → Settling → Falling`; depois dos passos, `IDLE` no chão.
- **`Loaded_S1aS7_CadaMonitor`:** a mesma chave em cada monitor; invariante 5.

### `tests/Buzzy.Core.Testes/Personagem/MudancaDeTopologiaTestes.cs` (S1–S8, S10–S12)

- **`OutroMonitorMuda_NaoInterrompeNenhumEstadoQueRevalida`:**
  - estados `IDLE`, `RESTING`, `REACTING`, `LANDING`, `WALKING`, `JUMPING`, `FALLING`, `HANGING` e `CLIMBING` (este no cenário da Fase 2);
  - `UmMonitor` → `LadoALado`;
  - espera uma única transição `origem→origem`, nenhum `MoverJanela` e `Geracao`/`DecisaoAgendada` inalterados.
- **`TrocaDePrincipal_TransladaSemInterromper`:** exemplo R15a/R15e. Espera `MoverJanela` com o retângulo transladado e `Posicao.AncoraAbsoluta` transladada.
- **`MonitorDoPersonagemMudouDeGeometria_Settling`:** S10/S11/S5/S6, a partir do repouso e da caminhada, com:
  - `BarraNoTopo`, `BarraAEsquerda`, `BarraADireita` e `BarraOculta`;
  - resolução de 2560×1440;
  - 144 DPI (R15h);
  - rotação do DISPLAY2 de `Retrato` para (1920,0)-(3840,1080).

  Espera a mesma fração e `Settling → Idle`.
- **`Desconectado_S1aS7_VaiAoSobreviventeECai`:**
  - para cada topologia S1–S7, cada monitor M que tenha sobrevivente, estados {`IDLE`, `WALKING`, `CLIMBING`, `HANGING`, `JUMPING`} (configuração da Fase 4) e fx {0,1; 0,85};
  - `SemMonitor`, ou `SemOPrincipal` quando M é o principal;
  - confere a chave esperada (calculada à mão por R8 em tabela no teste), a âncora pela fração, o caminho `→ Settling → Idle|Falling` e, depois dos passos, `IDLE` no chão;
  - inclui R15b, R15c e R15d.
- **`ChavesRenomeadasNoMeioDaCaminhada_Continua`:** a regra "mudou de chave"; `Retrato.ChaveMonitor` e `Posicao.ChaveMonitor` novos.
- **`Escondido_TrocaDePrincipalEDesconexao_ReapareceNoLugarCerto`:**
  - escondido no DISPLAY1 de S2;
  - `Rebaseada(S2, DISPLAY2)` translada `Posicao` para (3552,1032);
  - `CMD_SHOW` faz reaparecer em (3552,1032).
- **`Escondido_MonitorSaiEVoltaAntesDeMostrar_ReapareceNele`:** mais um caso para D11.
- **`EscondidoPorSessao_TopologiaMudaEDesbloqueia`** (S12, no núcleo): `SESSION_LOCKED`, depois R15c, depois `SESSION_UNLOCKED` → DISPLAY3 em (5472,1032).
- **`Pressed_TrocaDePrincipalEClick_ValidaNoLugarTransladado`:** `REACTING` em (3552,1032) sem trocar de monitor.
- **`RetornoDaTelaCheia_AcompanhaATrocaDePrincipal`:** no fim da tela cheia, volta ao DISPLAY1 nas coordenadas novas.

### `tests/Buzzy.Core.Testes/Movimento/TopologiaEmMovimentoTestes.cs` (`SimuladorDeTempo`, `Fase4()`)

- **`Escalando_ParedeViraPassagem_Cai`:**
  - `UmMonitor`, escalando a parede direita em x = 1856;
  - `TopologyChanged(LadoALado)`;
  - espera `Settling → Falling` e, depois, `LANDING` → `IDLE` em (1856,1032).
- **`AndandoParaEscalar_ParedeAlvoViraPassagem_TerminaNoProximoPasso`:** `QuerEscalar` para a direita, depois `LadoALado`; espera `IDLE` depois de um passo, sem caminhada infinita e sem relógio.
- **`TrocaDePrincipalNoMeioDaCaminhada_TrajetoriaIgualATransladada`:**
  - duas simulações com a mesma semente, uma com `Rebaseada` no passo k;
  - depois de k, `âncora₂ = âncora₁ + (dx,dy)` em todos os passos, e os estados são iguais.
- **`ChaveOpaca_RenomearTodasAsChaves_MesmaTrajetoria`:** três minutos em cada topologia S1–S7, com e sem `ComChavesRenomeadas`. As sequências de estados e âncoras são idênticas, o que prova D4.
- **`ApoioValeComMudancasDaFase5`:**
  - como `ApoioValeEmMilharesDePassos…`, mas com `GeradorDeTopologias.MudarComIdentidade`;
  - `ConferirApoio` depois de cada evento;
  - depois de cada `TopologyChanged`: `Lugar.Monitor ∈ topologia` e âncora na área útil.

### Conferências novas em `InvariantesTestes.Conferir`

- Invariantes 18 e 19 para todo `TopologyChanged` com a configuração diferente da anterior.
- Casos exigidos novos: "mudança de topologia que só translada o monitor do personagem" e "mudança de topologia sem o monitor do personagem". A regra de `Mudar` para trocar o principal já os produz.

### `tests/Buzzy.App.Testes/ChavesDeMonitorTestes.cs` (sem hardware)

- **`DoCaminho_ValoresFixados`:** os dois valores de R1.
- **`DoCaminho_IgnoraMaiusculas`.**
- **`Mapear_CloneUsaOMenorCaminho`:** resultado `mon:6852e0b1cd2318a2`.
- **`Mapear_CaminhoVazioNaoMapeia`.**
- **`Atribuir_MapaCacheReserva`.**
- **`Atribuir_CacheSoValeComAMesmaTela`.**
- **`Atribuir_NuncaRepeteChave`:** o cache velho aponta dois nomes para a mesma chave; a segunda vira `gdi:`.
- **`DeReserva_Formato`:** `gdi:\\.\DISPLAY2`.

### `tests/Buzzy.App.Testes/PlataformaTestes.cs` e `LeitorDeTopologiaTestes.cs`

- Tamanhos das estruturas: `LUID` 8, fonte 20, alvo 48, caminho 72, modo 64, cabeçalho 20, nome da fonte 84, nome do alvo 420.
- **`LeituraReal_ChavesUnicasEEstaveisEntreDuasLeituras`:**
  - roda na máquina, sem janela;
  - chaves iguais nas duas leituras;
  - se `ErroDaConsulta` for nulo, todas começam com `mon:`;
  - `NomeGdiPorChave` cobre todos os monitores.

### `tests/Buzzy.App.Testes/Integracao/MultiMonitorIntegracaoTestes.cs` (`[Integracao]`, só mensagens postadas às janelas do Buzzy)

- **`Minimizacao_LogoDepoisDeDisplayChange_NaoEsconde`:** posta `WM_DISPLAYCHANGE` e depois `ShowWindow(SW_SHOWMINNOACTIVE)`. Espera `JANELA|minimizada=sistema`, nenhum `VISIVEL|visivel=nao` em 2 s, `IsWindowVisible` e `!IsIconic`.
- **`Minimizacao_AntesDoDisplayChange_TambemEhDoSistema`:** a mesma coisa na ordem inversa, com 200 ms entre elas.
- **`ReafirmacaoTardia_JanelaMovidaDepoisDaReleitura_VoltaAoLugar`:** depois do `TOPOLOGIA`, move a janela por fora. Espera `POSICAO|reaplicada=sim|motivo=reafirmação tardia` e o retângulo de volta.
- **`Fumaca`, existente:** continua passando, com `principal.Chave == posicao["monitor"]`. Acrescentar `posicao["gdi"]` igual ao nome GDI do principal.
- **`InstanciaUnica_…`, existente:** continua escondendo, agora depois de até 750 ms.

---

## 6. Verificações [MANUAL] e [HW] pendentes

Toda mudança de configuração de vídeo é configuração global do Windows (AGENTS.md). **Só o usuário pode fazê-la**, seguindo um roteiro, com o Buzzy rodando com `--diagnostico`. Um script de leitura, `tools/ler-p5.ps1` (novo, só lê o `diagnostico.log`), resume por releitura: chaves, `gdi`, telas, DPI, `consulta`, `reserva`, intervalos entre `MENSAGEM` e `JANELA|minimizada`.

| Item | Por que fica pendente | Possível com o equipamento atual? |
|---|---|---|
| P5: estabilidade da chave depois de reiniciar, trocar portas e atualizar driver | exige reiniciar e trocar cabos | sim, feito pelo usuário |
| P5: sequência de mensagens ao conectar, desconectar, rearranjar (secundário acima, y negativo = S3), trocar o principal, girar e mudar a escala; calibração dos 300 ms e 1 s e das constantes de R16/R18 | exige mudar a configuração | sim (1920×1080; escala 125/150/175 %) |
| Ordem entre minimização e `WM_DISPLAYCHANGE` e movimento da janela, com "Minimizar janelas quando um monitor for desconectado" e "Lembrar locais das janelas" ligadas e desligadas (D13, D14) | depende do Windows real | sim, ao desconectar o cabo |
| `QueryDisplayConfig` com a sessão bloqueada (mudança durante Win+L) e em sessão remota (D3, UNCERTAIN) | comportamento não documentado com precisão | bloqueio sim; remota depende de ter RDP |
| Clone (Win+P "Duplicar") → chave do menor caminho (D2) | configuração global | sim, pelo usuário |
| S5 com 200 % e DPI misto real (monitor 4K) | falta hardware; P6 é da área de travessia | só 100/125/150/175 % |
| S6 retrato | configuração global (orientação) | sim, pelo usuário |
| S8 desconectar o monitor do Buzzy | ação física | sim |
| S9 reiniciar sem o monitor salvo e reconectar | ação física; depende da persistência de outra área | sim, depois da persistência |
| S10 resolução, escala e orientação com o app aberto | configuração global | sim |
| S11 barra em outra borda | o Windows 11 não oferece barra no topo nem nas laterais sem mexer no registro, o que é proibido; só "ocultar automaticamente" | parcial; outras bordas ficam UNCERTAIN, cobertas só por [AUTO] |
| Critério 9 da Fase 1 | mesmas mudanças reais de S10/S11 | sim, com o roteiro acima |

Nada disso vira VERIFIED sem a execução real. O código não depende do resultado de P5: se a chave se mostrar instável, a restauração pela tela (R7, passo 2) e o apelido em execução (R10) absorvem a diferença.

---

## 7. Riscos, dúvidas e pontos de contato

1. **A função de resumo é permanente.** Mudar R1 invalida todas as posições gravadas. O teste com valores fixos protege isso; qualquer troca exigiria migração (Fase 8).
2. **Consulta negada com a sessão bloqueada ou remota** (UNCERTAIN). O cache (nome GDI + tela), a reserva `gdi:` e o apelido por retângulo evitam troca de monitor. Uma posição gravada com chave `gdi:` é restaurada pela tela.
3. **Heurística de minimização** (D13): uma minimização feita pelo usuário nos 3 s seguintes a um `WM_DISPLAYCHANGE` real não esconde o Buzzy, e uma feita fora dessa janela de tempo leva até 750 ms para esconder. P5 calibra os valores.
4. **Conferência tardia de 1,5 s** (D14) pode não bastar se o Windows reposicionar a janela mais tarde. P5 mede isso.
5. **Testes que mudam por decisão desta área:** `TelaCheiaTestes:129`, `PropriedadesTestes:111` e `115–118` e `VerificacaoFase3:260`. As referências 01–05 não mudam: a classe B mantém o texto `TOPOLOGY_CHANGED`, `DescreverPosicao` mantém 5 campos e a carga sem posição salva mantém a regra.
6. **Posição salva × integração** (ponto com a persistência): `MovimentoIntegracaoTestes` (f4), `VerificacaoFase4`, as verificações de tela e o medidor de desempenho supõem a posição inicial (85 % no principal). Com `settings.json` real, o app começa onde o usuário deixou e as previsões por semente falham. É preciso uma opção de isolamento, como `--dados <pasta>` ou `--ignorar-posicao-salva`, usada por `BuzzyEmTeste` e `Buzzy.Verificacao`.
7. **`Maquina.cs`** (ponto com a travessia):
   - esta área é dona de `Carregar`, `MudarTopologia`, `ContinuarNoMonitor`, `Clicar` e `Validar`; a travessia é dona dos `Passo*`, `Planejar*` e `Superficies`;
   - se a travessia criar estado novo (progresso da passagem, monitor de destino, "monitor da escala" com histerese P6), esse estado precisa entrar em `ContinuarNoMonitor` (translação) e na classificação de R11;
   - se o personagem estiver no meio da passagem, basta que o `Passo*` revalide a passagem a cada passo, como já faz com as superfícies;
   - o passo 3 de R12 (a caminhada para escalar perde a parede) pode ser substituído pela travessia quando ela estiver ligada.
8. **`Aplicacao.cs`** (ponto com persistência, sessão e travessia):
   - esta área mexe em `Iniciar` (`Minimizada`), `AoPossivelMudancaDeTopologia`, `AoAgrupar`, `AoRecriarBarra`, `MostrarPorComando`, `RegistrarPosicao` e `EncerrarAplicacao`;
   - a persistência executa `GravarPosicao` e fornece `PosicaoSalva`;
   - a área de sessão precisa **reler a topologia e enviar `TOPOLOGY_CHANGED` antes de `SESSION_UNLOCKED` e `RESUMED`**, como `MostrarPorComando` já faz antes de `CMD_SHOW`;
   - a travessia provoca `WM_DPICHANGED` na própria janela a cada troca de escala, o que agenda releituras inócuas (topologia igual). Pode suprimi-las se quiser, mas R16 não depende disso.
9. **`Win32.cs`:** outras áreas acrescentam notificação de sessão e de energia (e, na Fase 8, `SetWinEventHook`). Cada área deve usar seções separadas por comentário para reduzir conflitos de mescla. O portão precisa continuar aprovando: `QueryDisplayConfig`, `GetDisplayConfigBufferSizes`, `DisplayConfigGetDeviceInfo` e `ShowWindow` não estão na lista proibida.
10. **`EstadoDoNucleo`, `Configuracao` e `Eventos`/`Efeitos`:** esta área não acrescenta nada. A preferência "atravessar monitores" (Q-05) é da travessia e deve entrar em `Preferencias` como propriedade `init` com padrão `true`, para não quebrar `new Preferencias(nivel, telaCheia)` nos testes.
11. **Igualdade de `PosicaoDoPersonagem`** agora inclui `TelaDoMonitor`. Testes novos devem comparar campo a campo, com os auxiliares `MesmaPosicao`, e não com `==`.
12. **Numeração:** DEC-023 e os invariantes 18 e 19 são provisórios. Renumerar na integração se outra área registrar antes.
13. **Pré-requisito:** R11 e R12 se aplicam sobre o `Maquina.cs` de `f4`, que ainda não foi mesclado. A mescla da Fase 4 precisa vir antes.
14. **Correção do enunciado:** `TopologiasDeExemplo` fica em `tests/Buzzy.Core.Testes/TopologiasDeExemplo.cs`, e não em `src/Buzzy.Core`. Os cenários S1–S7 já existem lá (`LadoALado`, `SecundarioAEsquerda`, `EmpilhadoSecundarioAcima`, `DegrauDesalinhado`, `EscalasMistas`, `Retrato`, `PrincipalADireita`/`TresMonitores`).
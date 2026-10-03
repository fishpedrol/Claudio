> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de o desenho da persistência mínima da posição (`settings.json`, gravação atômica e perfis de teste), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a Fase 5. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-029 a DEC-031) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

# Fase 5: desenho da persistência mínima da posição (`settings.json`)

- **Raiz do repositório:** `C:\Users\Cliente\Documents\claudio`. Todos os caminhos abaixo são relativos a ela.
- **Núcleo com física:** lido da cópia `C:\Users\Cliente\AppData\Local\Temp\claude\C--Users-Cliente-Documents-claudio\3b0e750b-9776-418f-aa13-3a4fa1644cac\scratchpad\f4` (`Maquina.cs`, `Movimento.cs`, `Configuracao.cs`, `EstadoDoNucleo.cs`).
- **Todo o resto:** app, testes e documentos foram lidos da árvore principal.
- **Onde se aplicam as mudanças:** o que está proposto em `Maquina.cs` vale para a versão da f4, depois da mesclagem.

## Ponto de partida (o que o código faz hoje)

- **Onde `GravarPosicao` sai.** `Maquina.Passo` já emite o efeito em cinco lugares:
  - `Soltar` (DRAG_END);
  - `CancelarArraste` (DRAG_CANCEL em DRAGGING);
  - `Esconder` → `GravarPosicaoDoUsuario` (CMD_HIDE, SESSION_LOCKED, SUSPENDING). Só grava quando sai de um estado não oculto: em HIDDEN só troca o motivo;
  - `RedefinirPosicao` (CMD_RESET_POSITION);
  - `Sair` → `GravarPosicaoDoUsuario` e depois `Encerrar` (CMD_EXIT, SESSION_ENDING).
- **Qual posição é gravada.** `GravarPosicaoDoUsuario` grava `RetornoDaTelaCheia ?? Posicao` (invariante 16).
- **`GravarPreferencias`** só sai de `EscolherEnergia`, isto é, do painel da Fase 8.
- **Na raiz:**
  - `Aplicacao.ExecutarEfeito` trata `GravarPosicao` e `GravarPreferencias` como `efeitoPendente` (só registra no log);
  - `Aplicacao.Iniciar` envia `Loaded(topologia, PosicaoSalva: null, Preferencias.Padrao)`.
- **`Passo.Carregar` restaura com `Posicionador.Reacomodar`.** É a regra de execução: mesma chave → frações; senão, o monitor mais próximo da `AncoraAbsoluta`. Não é a cascata de partida da ARCHITECTURE 2.8 (chave → mesmo retângulo → principal).
  - **Defeito latente:** `Reacomodar` preserva as frações recebidas. Uma carga com fração NaN deixa NaN em `EstadoDoNucleo.Posicao` e depois num `GravarPosicao`.
- **Falta o retângulo do monitor.** `PosicaoDoPersonagem(ChaveMonitor, FracaoX, FracaoY, AncoraAbsoluta)` não tem o "retângulo desse monitor na época" que a 2.8 exige.
- **Pasta de dados e documentação do `Programa`:**
  - `Diagnostico.PastaDeDados()` já resolve `%LOCALAPPDATA%\Buzzy` pela pasta conhecida, com `Environment.GetFolderPath(LocalApplicationData, DoNotVerify)`;
  - o `Programa` documenta que "sem `--diagnostico` o Buzzy não grava nada". Isso deixa de valer.
- **Testes que abrem o Buzzy.exe real esperam a posição inicial:** `IntegracaoTestes.Fumaca_…` e `Verificacao.AbrirBuzzy`, que usa `_esperado = Posicionador.Inicial(...)`. Sem isolamento, a persistência:
  - deixaria esses testes dependentes da ordem;
  - gravaria no arquivo real do usuário.

---

## 1. Decisões

As decisões D1 a D15 vão para uma DEC nova da Fase 5: DEC-023, ou o próximo número livre se outra área da Fase 5 já tiver usado esse.

**D1 — Onde mora cada parte.**
- **Núcleo puro (`src/Buzzy.Core/Persistencia/`):** o esquema v1, ou seja, tipos, padrões, validação, normalização e conversão entre bytes e `ConfiguracoesSalvas`. Também a política de "quando gravar" (constantes e `Imediata(Evento)`) e a cascata de restauração (`Posicionador.Restaurar`).
- **Adaptador (`src/Buzzy.App/Plataforma/`):** a pasta, a leitura dos arquivos, o protocolo atômico e a cópia de diagnóstico.
- **Raiz (`src/Buzzy.App/Composicao/`):** o atraso, a gravação imediata e a gravação no encerramento (`AgendaDeGravacao`).
- **Máquina de estados:** nenhum evento e nenhum efeito novo.

*Motivo:* DEC-007 e a ARCHITECTURE 2.13.2 põem o "esquema de configurações" no núcleo e a E/S no adaptador. Assim o formato, a validação e a cascata são testados sem disco, e o protocolo é testado com disco real sem abrir janela.

*Descartadas:*
- tudo no app: a validação só seria testável com arquivos;
- núcleo fazendo E/S: quebra a pureza;
- efeito novo `DescarregarGravacao`: mudaria as referências gravadas sem ganho, porque a raiz já recebe o evento que gerou o efeito em `ExecutarEfeito(evento, efeito, motivo)`.

**D2 — Formato e biblioteca.**
- JSON em UTF‑8 sem BOM, `schemaVersion` inteiro e demais nomes em português, em camelCase.
- Leitura por `JsonDocument` com extração manual campo a campo; escrita por `Utf8JsonWriter`.
- Nenhum `JsonSerializer` no Core nem no App.
- `<JsonSerializerIsReflectionEnabledByDefault>false</…>` no `Buzzy.App.csproj`.

*Motivo:*
- zero reflexão;
- tolerância por campo: um campo ruim não derruba o arquivo;
- System.Text.Json faz parte do framework compartilhado, então não entra pacote NuGet (DEC-016).

*Descartadas:*
- `JsonSerializer` por reflexão: é "reflexão insegura" e falha no arquivo inteiro;
- `JsonSerializerContext` gerado: também é tudo ou nada por tipo e ainda exigiria validação depois;
- parser próprio.

**D3 — Conteúdo da posição, conforme a 2.8.**
- Campos gravados: `chaveMonitor`, `telaDoMonitor` (o retângulo do monitor na época), `fracaoX`, `fracaoY` e `ancoraAbsoluta`.
- `PosicaoDoPersonagem` ganha a propriedade **`init` `TelaDoMonitor`**, não posicional. Ela é preenchida por quem descreve a posição: `Descrever`, `Reacomodar` no mesmo monitor, `Maquina.Validar` e `Restaurar`.

*Motivo:*
- O retângulo "na época" só é conhecido quando a posição é descrita. Se a raiz o buscasse na topologia na hora de gravar, perderia o dado quando o monitor some com o personagem escondido.
- Como propriedade `init`, os construtores e a desconstrução não mudam. Os 15 `new PosicaoDoPersonagem(...)` dos testes continuam compilando, e os testes que comparam campo a campo (`MesmaPosicao`) continuam valendo.

*Descartadas:* um campo novo em `EstadoDoNucleo`; um tipo persistido à parte, convertido na raiz.

**D4 — Qual posição vai para o disco.**
- Vai exatamente a posição do efeito `GravarPosicao`. É a posição atual no momento de soltar, cancelar, esconder, bloquear, suspender, redefinir ou sair. Se houver retorno de antes da tela cheia, vai o retorno; nunca a posição temporária (invariante 16).
- A raiz **nunca** lê `Estado.Posicao` nem `Lugar` para gravar.
- Com isso, entra a posição a que o movimento autônomo levou. Isso cumpre o objetivo da Fase 5 ("posição restaurada entre execuções") e a própria 2.6, que chama de "posição escolhida pelo usuário" o retorno de antes da tela cheia, que também pode ser autônomo.

*Descartada (e levada como dúvida ao usuário, na seção 7):* gravar só a última escolha explícita, por arraste ou redefinição. Exigiria o campo `PosicaoEscolhida` em `EstadoDoNucleo` e faria o Buzzy "voltar para casa" a cada partida.

**D5 — Restauração na partida.**
- A cascata da 2.8 vira `Posicionador.Restaurar`: chave → primeiro monitor com o mesmo retângulo de tela → principal.
- As frações salvas, normalizadas, são sempre mantidas.
- `Reacomodar` continua sendo a regra de execução, sem mudança.

*Descartadas:*
- continuar usando `Reacomodar` na carga: contradiz os passos 2 e 3 da 2.8;
- "monitor mais próximo da âncora absoluta" no passo 3: menos previsível, porque o Buzzy poderia surgir numa TV recém-ligada.

**D6 — O v1 já guarda as preferências que o núcleo conhece.**
- Guarda `energia` e `modoTelaCheia`, e ainda `atravessarMonitores` (Q‑05).
- A Fase 8 acrescenta as demais no v2.

*Motivo:*
- `Loaded` já recebe `Preferencias`;
- é a única forma de desligar a travessia antes da Fase 8: editar o arquivo com o Buzzy fechado.

*Descartada:* um v1 só com a posição. Obrigaria uma migração só para as preferências.

**D7 — Validação tolerante.**
- Só é **ilegível** o arquivo que tem mais de 64 KiB, UTF‑8 inválido, JSON inválido ou profundidade acima de 8, raiz que não é objeto, ou `schemaVersion` ausente, não inteiro ou menor que 1.
- Todo o resto é avaliado campo a campo:
  - campo desconhecido é ignorado;
  - campo repetido: vale o primeiro;
  - número fora da faixa é preso ao limite;
  - tipo errado vira o padrão do campo; a posição é tudo ou nada nos campos obrigatórios.
- Enumerações só pelos três nomes explícitos, nunca por `Enum.Parse`, que aceita "1" e "Baixa,Alta".
- Comentários e vírgula final são aceitos, porque o arquivo pode ser editado à mão.

**D8 — Versão futura (`schemaVersion` maior que 1).**
- Lê os campos que conhece com as regras do v1.
- **Bloqueia a gravação** nesta execução.
- Regra para a Fase 8: toda ampliação do esquema incrementa `schemaVersion`.

*Descartadas:*
- tratar como ilegível: destrói os dados novos num downgrade;
- gravar mesmo assim: apaga os campos novos;
- preservar os campos desconhecidos: complexidade sem caso real, já que o uso é pessoal.

**D9 — Protocolo atômico.**
- Escreve `settings.json.tmp` (sem buffer, `Flush(true)`).
- Depois **confere o principal atual** e decide:

  | Principal | Ação |
  |---|---|
  | ausente | `File.Move(tmp, principal)` |
  | válido | `File.Replace(tmp, principal, "settings.json.bak")` |
  | ilegível | `File.Replace(tmp, principal, "settings.corrupt.json")` |
  | versão futura | bloqueia |
  | inacessível | a tentativa falha |

- O `.tmp` nunca é lido.

*Motivo:*
- o `.bak` só recebe um arquivo recém-validado, ou seja, literalmente "o último arquivo bom";
- a cópia de diagnóstico sai na mesma operação atômica e sobrescreve a anterior: no máximo uma.

*Descartadas:*
- escrever no próprio arquivo: pode ficar rasgado;
- `Move(overwrite)` mais cópia manual do `.bak`: há uma janela sem principal e o `.bak` não é atômico;
- recuperar pelo `.tmp`: ganho raro e risco de ler um arquivo parcial.

**D10 — Leitura sem efeito colateral, em cascata: principal → `.bak` → padrões.**
- **Principal ilegível:** usa o `.bak`, se for válido. A cópia de diagnóstico é feita na primeira gravação.
- **Principal inacessível** (E/S depois de 3 tentativas): usa o `.bak` ou os padrões e bloqueia a gravação nesta execução, para não sobrescrever o que não se conseguiu ler.

*Descartada:* padrões puros quando há um `.bak` bom. A 2.12 diz "padrão", mas o `.bak` existe para isso. Ajustar o texto da 2.12 e do critério 3 da Fase 8.

**D11 — Quando gravar.**
- **Com atraso:** 2 s depois do último pedido. É um disparo único, reiniciado a cada pedido e adiado enquanto o núcleo estiver em PRESSED ou DRAGGING.
- **Imediato e síncrono:** `GravarPosicao` vindo de SUSPENDING, SESSION_ENDING ou CMD_EXIT.
- **Descarga do pendente:** `EncerrarAplicacao` e o tratador de suspensão descarregam o que estiver pendente.
- **Sem gravação inútil:** se o conteúdo desejado é igual ao que se sabe estar no disco, nada é gravado.
- **Em caso de falha:**
  - novas tentativas únicas em 2 s, 10 s e 60 s; depois desiste até o próximo pedido;
  - no caminho imediato, até 3 tentativas com 50 ms entre elas.

*Motivo:*
- ARCHITECTURE 2.12: "com atraso depois de soltar e sempre ao sair";
- DEC‑010 (c);
- DEC‑011: nenhuma escrita periódica, porque só eventos do usuário ou do sistema geram pedidos;
- 2 s é menos que o intervalo de acomodação (3 s): a gravação cai com o personagem parado.

*Descartadas:* gravar só ao sair; gravar a cada DRAG_END sem atraso; gravar numa thread de fundo.

**D12 — E/S síncrona na thread da interface.**
- O arquivo tem menos de 1 KiB e o custo fica registrado no log (`ms=`).

*Descartada:* uma fila em `Task.Run`. Ordem e encerramento ficariam mais difíceis, sem ganho medido.

**D13 — Isolamento de testes: `--perfil-de-teste NOME`.**
- Os dados vão para `%LOCALAPPDATA%\Buzzy\testes\NOME`.
- Nome inválido desliga a persistência nesta execução: falha fechada.
- O log de diagnóstico continua na raiz da pasta.

*Descartadas:*
- opção com caminho arbitrário: gravaria fora da pasta do Buzzy (SECURITY 3.1);
- variável de ambiente: invisível;
- não isolar.

**D14 — O formato das reproduções gravadas não muda.**
- `Gravacao.DescreverPosicao` e `LerPosicao` continuam com 5 campos.
- `TelaDoMonitor` não aparece no texto; uma posição lida de uma reprodução tem o retângulo desconhecido.
- As 5 referências continuam idênticas byte a byte. O retângulo é coberto por testes de unidade e de propriedade.

**D15 — Log sem dado pessoal.**
- As linhas `CONFIG` registram origem, situação, contagens e tempos.
- Nunca registram:
  - o caminho da pasta, que contém o nome do usuário (SECURITY 6);
  - valores lidos do arquivo;
  - nomes de campos desconhecidos.

---

## 2. Tipos e arquivos novos

### Núcleo, `src/Buzzy.Core/Persistencia/` (namespace `Buzzy.Core.Persistencia`)

**`ConfiguracoesSalvas.cs`**
```csharp
public sealed record ConfiguracoesSalvas(PosicaoDoPersonagem? Posicao, Preferencias Preferencias)
{ public static readonly ConfiguracoesSalvas Padrao = new(null, Preferencias.Padrao); }

public enum SituacaoDaLeitura { Valida, VersaoFutura, Ilegivel }

public sealed record LeituraDasConfiguracoes(
    SituacaoDaLeitura Situacao, int? Versao, ConfiguracoesSalvas Configuracoes,
    IReadOnlyList<string> Avisos,       // nomes de campo nossos + motivo; sem valores do arquivo
    string? MotivoIlegivel);            // "tamanho" | "utf8" | "json" | "raiz" | "schemaVersion"
```

**`EsquemaDeConfiguracoes.cs`**, com as regras da seção 4:
```csharp
public static class EsquemaDeConfiguracoes
{
    public const int VersaoAtual = 1;
    public const int TamanhoMaximoEmBytes = 65_536;
    public const int ProfundidadeMaxima = 8;
    public const int ComprimentoMaximoDaChave = 1_024;
    public const int CoordenadaMinima = -32_768, CoordenadaMaxima = 32_767;

    public static LeituraDasConfiguracoes Ler(ReadOnlyMemory<byte> conteudo);   // nunca lança
    public static byte[] Escrever(ConfiguracoesSalvas configuracoes);            // normaliza; nunca lança
    public static ConfiguracoesSalvas Normalizar(ConfiguracoesSalvas configuracoes);
    public static string NomeDaEnergia(NivelDeEnergia nivel);                    // "baixa" | "media" | "alta"
    public static bool TentarLerEnergia(string texto, out NivelDeEnergia nivel); // só os 3 nomes, OrdinalIgnoreCase
}
```

**`PoliticaDeGravacao.cs`**
```csharp
public static class PoliticaDeGravacao
{
    public static readonly TimeSpan Atraso = TimeSpan.FromSeconds(2);
    public static readonly IReadOnlyList<TimeSpan> EsperasDeNovaTentativa =
        [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60)];
    public const int TentativasImediatas = 3;
    public static readonly TimeSpan PausaEntreTentativasImediatas = TimeSpan.FromMilliseconds(50);
    public static bool Imediata(Evento evento) => evento is Suspending or SessionEnding or CmdExit;
}
```

**Acréscimos em `src/Buzzy.Core/Posicionador.cs`**, que já existe:
```csharp
public enum PassoDaRestauracao { PelaChave, PeloRetangulo, NoPrincipal }
public static (Posicionamento Resultado, PosicaoDoPersonagem NovaPosicao, PassoDaRestauracao Passo)
    Restaurar(Topologia topologia, PosicaoDoPersonagem salva, TamanhoDip tamanho);
```

### Adaptador, `src/Buzzy.App/Plataforma/`

**`PastaDeDados.cs`**
```csharp
internal static class PastaDeDados
{
    internal static string? DoBuzzy();                   // GetFolderPath(LocalApplicationData, DoNotVerify)+"Buzzy"; null se vazio
    internal static bool NomeDePerfilValido(string nome); // ^[a-z0-9][a-z0-9-]{0,31}$ e não reservado (con, prn, aux, nul, com1–9, lpt1–9)
    internal static string? DoPerfilDeTeste(string nome); // DoBuzzy()\testes\nome; null se inválido
}
```

**`ArquivoDeConfiguracoes.cs`**
```csharp
internal enum EstadoDoArquivo { Ausente, Inacessivel, Ilegivel, Valido, VersaoFutura }
internal enum OrigemDasConfiguracoes { Principal, Reserva, Padroes }
internal enum EtapaDaGravacao { TemporarioAberto, TemporarioEscrito, TemporarioDescarregado, PrincipalConferido, Substituido }

internal sealed record LeituraDoArquivo(ConfiguracoesSalvas Configuracoes, OrigemDasConfiguracoes Origem,
    EstadoDoArquivo Principal, EstadoDoArquivo? Reserva, bool GravacaoBloqueada, int Avisos);
internal sealed record ResultadoDaGravacao(bool Gravou, int Bytes, EstadoDoArquivo? PrincipalAntes,
    bool CopiaDeDiagnostico, string? Erro);

internal sealed class ArquivoDeConfiguracoes
{
    internal const string NomePrincipal = "settings.json", NomeReserva = "settings.json.bak",
                          NomeTemporario = "settings.json.tmp", NomeIlegivel = "settings.corrupt.json";
    internal ArquivoDeConfiguracoes(string pasta, Action<EtapaDaGravacao>? aoConcluirEtapa = null); // gancho só para testes
    internal string Pasta { get; }
    internal bool GravacaoBloqueada { get; }
    internal LeituraDoArquivo Ler();                                                  // não altera nenhum arquivo
    internal ResultadoDaGravacao Gravar(ConfiguracoesSalvas configuracoes, int tentativas = 1);
}
```

### Raiz, `src/Buzzy.App/Composicao/AgendaDeGravacao.cs`
```csharp
internal sealed class AgendaDeGravacao
{
    internal AgendaDeGravacao(ArquivoDeConfiguracoes? arquivo, LeituraDoArquivo leitura,
        Func<bool> gestoDoUsuarioEmCurso, Func<TimeSpan, Action, Action> agendarUmaVez); // devolve "cancelar"
    internal ConfiguracoesSalvas Desejadas { get; }
    internal void Pedir(ConfiguracoesSalvas novas, bool imediata, string motivo);
    internal bool Descarregar(string motivo);   // síncrono; até PoliticaDeGravacao.TentativasImediatas
    internal void Parar();
    internal static Action AgendarNoDispatcher(TimeSpan atraso, Action acao); // DispatcherTimer(Background), disparo único
}
```

### Testes novos

A seção 5 detalha o que cada um verifica.

| Pasta | Arquivos |
|---|---|
| `tests/Buzzy.Core.Testes/Persistencia/` | `EsquemaDeConfiguracoesTestes.cs`, `RestauracaoTestes.cs`, `PropriedadesDaPersistenciaTestes.cs` e a amostra `Amostras/settings-v1.json`, lida da pasta-fonte por `[CallerFilePath]`, como `ReproducaoTestes.PastaDasFontes` |
| `tests/Buzzy.App.Testes/` | `ArquivoDeConfiguracoesTestes.cs`, `AgendaDeGravacaoTestes.cs`, `GravadorSemParar.cs` |
| `tests/Buzzy.App.Testes/Integracao/` | `PersistenciaIntegracaoTestes.cs` |

---

## 3. Mudanças em arquivos existentes

| Arquivo | Tipo ou função | Mudança |
|---|---|---|
| `src/Buzzy.Core/Posicionador.cs` | `PosicaoDoPersonagem` | Acrescentar `public RetanguloPx TelaDoMonitor { get; init; }`, com documentação: "retângulo de tela do monitor da chave quando a posição foi descrita; vazio = desconhecido". |
| 〃 | `Descrever` | Passa a retornar `new(...) { TelaDoMonitor = p.Monitor.Tela }`. |
| 〃 | `Reacomodar` | No ramo do mesmo monitor: `atual with { AncoraAbsoluta = r.Ancora, TelaDoMonitor = mesmo.Tela }`. O outro ramo já usa `Descrever`. |
| 〃 | `Restaurar` e `PassoDaRestauracao` | Novos (regra 4.7). A normalização de frações reusa o `Fracao` privado. |
| 〃 | `FracaoInicialX` | Documentação: vale para a primeira execução e para quando não há posição salva. |
| `src/Buzzy.Core/Personagem/Maquina.cs` (f4) | `Passo.Carregar` | Com `e.PosicaoSalva`, usar `Posicionador.Restaurar` no lugar de `Reacomodar`. A regra da transição passa a ser `"BOOTING: configurações e topologia carregadas; posição salva restaurada {pela chave \| pelo retângulo do monitor \| no monitor principal}"`. Sem posição salva, o texto não muda, e as referências continuam idênticas. |
| 〃 | `Passo.Validar` | `preferida with { AncoraAbsoluta = presa, TelaDoMonitor = monitor.Tela }`. |
| 〃 | resto | Sem mudança. Nenhum evento ou efeito novo. `Soltar`, `CancelarArraste`, `Esconder`, `RedefinirPosicao` e `Sair` já emitem o que a raiz precisa. |
| `src/Buzzy.Core/Personagem/Eventos.cs` | `Loaded` | Documentação: "`PosicaoSalva` vem de `settings.json` (Fase 5)". |
| 〃 | `Preferencias` | Precisa de `bool AtravessarMonitores = true`, com `Padrao = new(Media, true, true)`, se a área de travessia não criar um equivalente. **Coordenar o nome** (seção 7). |
| `src/Buzzy.Core/Personagem/Efeitos.cs` | `GravarPosicao`, `GravarPreferencias` | Só documentação: "a raiz grava com atraso; SUSPENDING, SESSION_ENDING e CMD_EXIT gravam na hora". |
| `src/Buzzy.Core/Personagem/Gravacao.cs` | — | **Sem mudança** (D14). |
| `src/Buzzy.Core/Personagem/EstadoDoNucleo.cs`, `Configuracao.cs`, `Movimento.cs`, `Topologia.cs` | — | Sem mudança. |
| `src/Buzzy.App/Composicao/Aplicacao.cs` | `OpcoesDaAplicacao` | Acrescentar `string? PerfilDeTeste = null` e `bool PersistenciaDesligada = false`. |
| 〃 | campos | `_arquivo` (`ArquivoDeConfiguracoes?`) e `_gravacao` (`AgendaDeGravacao`). |
| 〃 | `Iniciar` | Antes de `new Nucleo(...)`: montar `_arquivo`, conforme o perfil e a pasta (4.8), chamar `Ler()`, registrar `CONFIG`, criar `_gravacao` com `gestoDoUsuarioEmCurso = () => _nucleo?.Estado.Estado is Estado.Pressed or Estado.Dragging`. Depois: `Enviar(new Loaded(topologia, leitura.Configuracoes.Posicao, leitura.Configuracoes.Preferencias), "início")`. |
| 〃 | `ExecutarEfeito` | `GravarPosicao g` → `_gravacao.Pedir(_gravacao.Desejadas with { Posicao = g.Posicao, Preferencias = _nucleo!.Estado.Preferencias }, PoliticaDeGravacao.Imediata(evento), evento.GetType().Name)`. `GravarPreferencias p` → a mesma chamada, com `{ Preferencias = p.Preferencias }`. Os dois saem do grupo `efeitoPendente`. |
| 〃 | `EncerrarAplicacao` | Logo depois de `_encerrando = true`: `_gravacao?.Descarregar($"encerrar: {motivo}")` e depois `_gravacao?.Parar()`, antes de desmontar a bandeja e as janelas e antes do `Shutdown`. |
| 〃 | `DispatcherUnhandledException` | `Descarregar("erro")` dentro de `try/catch(Exception)`, com uma marca contra reentrada. |
| 〃 | tratador de SUSPENDING (criado pela área de sessão e energia) | Depois de `Enviar(new Suspending(), …)`, chamar sempre `_gravacao.Descarregar("SUSPENDING")`. Isso cobre o caso HIDDEN, que não emite `GravarPosicao`. |
| `src/Buzzy.App/Programa.cs` | `LerOpcoes` e documentação | Ler `--perfil-de-teste NOME` (4.8). Nome inválido ou ausente → `PersistenciaDesligada = true` e `ARGUMENTO\|ignorado=--perfil-de-teste`. A documentação passa a dizer: "sem `--diagnostico` o Buzzy só grava `settings.json` na pasta dele". |
| `src/Buzzy.App/Plataforma/Diagnostico.cs` | `PastaDeDados()` | Passa a chamar `PastaDeDados.DoBuzzy() ?? ""`, mantendo o teste `PlataformaTestes.PastaDeDadosFicaNoLocalAppDataDoUsuario`. |
| `src/Buzzy.App/Plataforma/Win32.cs` | — | **Sem mudança.** `File.Replace` e `File.Move` são da biblioteca base (ReplaceFileW e MoveFileExW); nenhum P/Invoke novo; nenhuma regra do portão é afetada. |
| `src/Buzzy.App/Buzzy.App.csproj` | PropertyGroup | `<JsonSerializerIsReflectionEnabledByDefault>false</JsonSerializerIsReflectionEnabledByDefault>`. |
| `tests/Buzzy.App.Testes/Integracao/BuzzyEmTeste.cs` | `IniciarProcesso` e `Iniciar` | Parâmetros `perfil = "integracao"` e `limpar = true`. Passa `--perfil-de-teste`. Antes de iniciar, apaga `DoPerfilDeTeste(perfil)` só se estiver sob `…\Buzzy\testes\`. Acrescentar `EventoDoLog.Instante`, que lê o prefixo `[hh:mm:ss.fff]`. |
| `tests/Buzzy.App.Testes/Programa.cs` | `Main` | `args is ["--gravar-sem-parar", var pasta] ? GravadorSemParar.Executar(pasta) : Executor.Executar(...)`. |
| `tests/Buzzy.Verificacao/Verificacao.cs` | `AbrirBuzzy` (e o lançamento da segunda instância) | Acrescentar `--perfil-de-teste verificacao` e limpar a pasta antes. `_esperado = Posicionador.Inicial(...)` só vale com o perfil vazio. |
| `tests/Buzzy.Core.Testes/PropriedadesTestes.cs` | `ReacomodarSempreTerminaNaAreaUtilDeUmMonitorDaNovaTopologia`, linha 107 | Esperar `atual with { AncoraAbsoluta = r.Ancora, TelaDoMonitor = r.Monitor.Tela }`. |
| `tests/Buzzy.Core.Testes/Personagem/InvariantesTestes.cs` | verificador de passo (junto de R8) | Invariante 18 (abaixo), mais a ida e volta pelo esquema. |
| `tools/medir-desempenho.ps1` | lançamento | `--perfil-de-teste desempenho`. A medição não deve mexer na posição real do usuário. |
| `docs/ARCHITECTURE.md` | 1, 2.6, 2.8, 2.12, 2.13.3 | Ver abaixo. |
| `docs/SECURITY.md` | 3.1, 5, 7, 8.6, 10 | Arquivos, incluindo o `.tmp` transitório; limites; perfil de teste; a pasta de testes fica dentro da pasta do Buzzy. |
| `docs/DECISIONS.md` | DEC‑023 | D1 a D15. DEC‑016 item 10: "a gravação entrou na Fase 5, dentro de SessionEnding". |
| `docs/TODO.md` | Fase 5 | Subtarefas de persistência e mapeamento S1–S12. Fase 8, critério 3: "valores do `.bak` ou, sem ele, os padrões". |
| `docs/PROJECT_CONTEXT.md`, `DEVELOPMENT_LOG.md`, `COMO_INICIAR.md`, `CONTINUIDADE.md` | — | Estado. Em `COMO_INICIAR.md`: "para voltar à posição inicial, feche o Buzzy e apague `settings.json` e `settings.json.bak`". |

**ARCHITECTURE 2.6.** Não há eventos nem efeitos novos. Texto proposto:

| De | Evento | Para | Regra (texto novo ou acrescentado) |
|---|---|---|---|
| `BOOTING` | configurações e topologia carregadas | `SETTLING` | "… Posição restaurada pela seção 2.8: a raiz lê `settings.json` antes (2.12), e `Posicionador.Restaurar` aplica chave → mesmo retângulo → principal, com as frações salvas. …" (o resto não muda) |
| `DRAGGING` | `DRAG_END` ou `DRAG_CANCEL` | `SETTLING` | "Validação da seção 2.7; grava a posição validada (com atraso, 2.12)." |
| qualquer, exceto `EXITING` | `CMD_HIDE`, `SESSION_LOCKED` | `HIDDEN(...)` | acrescentar "(gravação com atraso)" |
| qualquer, exceto `EXITING` | `SUSPENDING` | `HIDDEN(POR_SUSPENSAO)` | acrescentar "a gravação vai ao disco antes de o tratamento da suspensão terminar" |
| qualquer | `CMD_EXIT`, `SESSION_ENDING` | `EXITING` | "… grava na hora, antes de encerrar." |
| **nova:** qualquer, exceto `BOOTING`, `PRESSED`, `DRAGGING` e `EXITING` | `CMD_RESET_POSITION` | `SETTLING` se visível; `HIDDEN` sem troca se escondido | "Leva à posição inicial (chão do principal a 85%), descarta o retorno temporário e grava essa posição (com atraso). Escondido, só troca a posição guardada." A linha documenta o que `RedefinirPosicao` já faz e hoje falta na tabela. |

**Invariante 18, novo.** "Todo `GravarPosicao` traz uma posição gravável:
- chave não vazia;
- `TelaDoMonitor` não vazio;
- frações finitas.

Ele só sai de eventos com origem ≥ Sistema, nunca de `Tick`, `AutonomyTimer`, `MovementSignal` nem `ExpressionChange`. Isso garante 'sem escrita periódica' (DEC‑011) (DEC‑023)."

**Outras seções da ARCHITECTURE:**
- **2.8, posição gravada:** acrescentar "a âncora absoluta vale para a execução; na partida, a cascata a recalcula".
- **2.12, leitura:** "principal → `.bak` → padrões".
- **2.12, gravação:** protocolo da D9 e atraso de 2 s.
- **2.13.3, linha "Fim de sessão":** gravar em `SessionEnding` (WM_QUERYENDSESSION), conforme DEC‑016 item 10.
- **2.13.3, linha nova "Gravação atômica":** `File.Replace`/`File.Move`/`FileStream.Flush(true)`, da biblioteca base.

---

## 4. Regras exatas

### 4.1 Formato v1

Amostra de referência: posição S2, secundário à esquerda. O arquivo é UTF‑8 sem BOM, com indentação de 2 espaços, fim de linha `\n` e `\n` final.

```json
{
  "schemaVersion": 1,
  "posicao": {
    "chaveMonitor": "\\\\.\\DISPLAY2",
    "telaDoMonitor": {
      "esquerda": -1920,
      "topo": 0,
      "direita": 0,
      "base": 1080
    },
    "fracaoX": 0.25,
    "fracaoY": 1,
    "ancoraAbsoluta": {
      "x": -1440,
      "y": 1032
    }
  },
  "preferencias": {
    "energia": "media",
    "modoTelaCheia": true,
    "atravessarMonitores": true
  }
}
```

**Como escrever:**
- `Utf8JsonWriter` com `Indented = true`, `NewLine = "\n"` e o codificador padrão, que escapa `\` e caracteres fora do ASCII.
- A ordem dos campos é fixa: a acima.
- `posicao` é omitido quando nulo; `telaDoMonitor` é omitido quando vazio.
- Números em ponto flutuante no formato mais curto que reproduz o valor. Os bytes não dependem da cultura.

### 4.2 Leitura: `EsquemaDeConfiguracoes.Ler`, sem lançar

**Checagens de estrutura, nesta ordem.** Qualquer falha dá `Ilegivel`, com as configurações padrão:
1. mais de 65.536 bytes → `"tamanho"`;
2. retirar o BOM `EF BB BF`;
3. `System.Text.Unicode.Utf8.IsValid` falso → `"utf8"`;
4. `JsonDocument.Parse(mem, new() { MaxDepth = 8, CommentHandling = Skip, AllowTrailingCommas = true })` lança `JsonException` → `"json"`;
5. raiz diferente de objeto → `"raiz"`;
6. `schemaVersion`: não ser número, falhar em `TryGetInt32` ou ser menor que 1 → `"schemaVersion"`;
7. qualquer outra exceção durante a extração, por exemplo de transcodificação → `"json"`.

**Campos:**
- Os objetos são percorridos com `EnumerateObject()`. Cada nome conhecido é reconhecido por `NameEquals`, que é ordinal e diferencia maiúsculas; guarda-se a primeira ocorrência. Uma repetição gera o aviso `campo repetido: <nosso nome>`, e os desconhecidos só contam.
- `schemaVersion` maior que `VersaoAtual` → `VersaoFutura`, e os campos conhecidos são lidos pelas mesmas regras.
- `posicao` ou `preferencias` ausentes ou `null` → sem posição / padrão, sem aviso. Com outro tipo → a mesma coisa, com aviso.

| Campo | Obrigatório | Regra |
|---|---|---|
| `posicao.chaveMonitor` | sim | String com 1 a 1024 caracteres e nenhum `char.IsControl`. Senão, **toda a posição é descartada**. |
| `posicao.fracaoX` e `fracaoY` | sim | Número com `TryGetDouble` e `double.IsFinite`, preso em [0, 1]. Senão, a posição é descartada. |
| `posicao.telaDoMonitor` | não | Objeto com `esquerda`, `topo`, `direita` e `base`. Cada um é inteiro (`TryGetInt64`), preso em [‑32768, 32767]. Depois disso, exige `esquerda < direita` e `topo < base`. Qualquer falha → `default` (desconhecido), com aviso. |
| `posicao.ancoraAbsoluta` | não | Objeto `{x, y}` de inteiros presos na mesma faixa. Falha → `(0, 0)`, com aviso. A partida recalcula a âncora de qualquer forma. |
| `preferencias.energia` | não | `"baixa"`, `"media"` ou `"alta"`, sem diferenciar maiúsculas (ordinal). Qualquer outra coisa, inclusive `"1"`, `"média"` e `"Baixa,Alta"` → `Media`, com aviso. |
| `preferencias.modoTelaCheia` | não | `true` ou `false`. Senão → `true`, com aviso. |
| `preferencias.atravessarMonitores` | não | `true` ou `false`. Senão → `true`, com aviso. |

### 4.3 Escrita: `Escrever` e `Normalizar`

**Como `Normalizar` trata cada valor:**
- posição com chave inválida (regra da 4.2) → posição omitida;
- fração NaN → 0,5; ±∞ ou fora de [0, 1] → presa;
- coordenadas presas em [‑32768, 32767];
- retângulo vazio → `telaDoMonitor` omitido;
- energia fora das três opções → `"media"`.

**Garantias:**
- `Escrever` nunca lança e produz no máximo 64 KiB. O pior caso é uma chave de 1024 caracteres escapados em `\uXXXX`: cerca de 6 KiB.
- `Ler(Escrever(c))` é `Valida`, com `Configuracoes == Normalizar(c)`.
- `Escrever(Normalizar(c))` é igual a `Escrever(c)`.

### 4.4 Protocolo em disco: `ArquivoDeConfiguracoes.Gravar(c, n)`

1. Com a gravação bloqueada → `Gravou = false`, com o erro `"bloqueada"`.
2. `dados = Escrever(c)`.
3. `Directory.CreateDirectory(Pasta)`.
4. Escrever o temporário e fechá-lo:
   ```csharp
   new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1)
   ```
   As etapas do gancho são, em ordem: `TemporarioAberto` → `Write` → `TemporarioEscrito` → `Flush(true)` → `TemporarioDescarregado`.
5. Conferir o principal com o mesmo leitor da partida, numa só tentativa. Etapa `PrincipalConferido`.
6. Agir conforme o estado do principal:

   | Principal | Ação |
   |---|---|
   | `Ausente` | `File.Move(tmp, principal, overwrite: false)` |
   | `Valido` | `File.Replace(tmp, principal, bak, ignoreMetadataErrors: true)` |
   | `Ilegivel` | `File.Replace(tmp, principal, corrupt, true)` → `CopiaDeDiagnostico = true` |
   | `VersaoFutura` | bloqueia, não grava |
   | `Inacessivel` | a tentativa falha |

7. Etapa `Substituido`.

Nas etapas 3 a 6, `IOException` ou `UnauthorizedAccessException` → nova tentativa se ainda houver, com `Thread.Sleep(50)` entre elas. Qualquer outra exceção propaga: é defeito.

**Estado do disco depois de uma queda do processo:**

| Onde caiu | Disco | `Ler` devolve |
|---|---|---|
| durante a etapa 4 | principal A (ou ausente), `.tmp` parcial | A; sem principal, `.bak` ou padrões |
| entre as etapas 4 e 6 | principal A, `.tmp` = N completo | A |
| dentro do `Replace`, no caso Valido | sem principal, `.bak` = A, `.tmp` = N | A (reserva); N se perde |
| dentro do `Replace`, no caso Ilegivel | sem principal, cópia de diagnóstico = lixo, `.tmp` = N | `.bak` B, ou padrões |
| depois da etapa 6 | principal N, `.bak` = A | N |

O Buzzy nunca deixa um principal presente e ilegível.

### 4.5 Leitura dos arquivos: `ArquivoDeConfiguracoes.Ler()`, sem efeito colateral

1. `LerUm(principal)`:
   - abre com `FileShare.Read | Write | Delete`;
   - `Length` maior que 65.536 → `Ilegivel`, sem ler o conteúdo;
   - lê no máximo 65.537 bytes;
   - `FileNotFound` ou `DirectoryNotFound` → `Ausente`;
   - outra `IOException` ou `UnauthorizedAccessException` → até 3 tentativas com 100 ms; depois, `Inacessivel`.
2. Conforme o resultado:
   - `Valido` → origem Principal;
   - `VersaoFutura` → origem Principal, com a gravação bloqueada;
   - `Inacessivel` → bloqueia a gravação e segue para o `.bak`;
   - `Ilegivel` ou `Ausente` → segue para o `.bak`.
3. `LerUm(.bak)`:
   - `Valido` → origem Reserva;
   - `VersaoFutura` → origem Reserva, com a gravação bloqueada;
   - qualquer outro → origem Padroes.

O `.tmp` e o `settings.corrupt.json` nunca são lidos.

### 4.6 Quando gravar (`AgendaDeGravacao` com `PoliticaDeGravacao`)

| Evento (emissor em `Maquina.Passo`) | Efeito | Na raiz |
|---|---|---|
| `DRAG_END`, `DRAG_CANCEL` em DRAGGING (`Soltar`, `CancelarArraste`) | `GravarPosicao` da posição validada | atraso de 2 s |
| `CMD_RESET_POSITION` (`RedefinirPosicao`) | posição inicial | atraso |
| `CMD_HIDE`, `SESSION_LOCKED` a partir de um estado não oculto (`Esconder`) | `RetornoDaTelaCheia ?? Posicao` | atraso |
| `SUSPENDING` a partir de um estado não oculto | idem | **imediata**; além disso, o tratador de suspensão chama `Descarregar` |
| `CMD_EXIT`, `SESSION_ENDING` (`Sair`) | `GravarPosicao` e depois `Encerrar` | **imediata**; `EncerrarAplicacao` chama `Descarregar` antes do `Shutdown` |
| `ENERGY_SELECTED` (Fase 8) | `GravarPreferencias` | atraso |
| `Tick`, `AutonomyTimer`, `MovementSignal`, `TopologyChanged`, `Loaded`, `FullscreenTargetsChanged`, `SettingsChanged` | nunca (invariante 18) | — |

**`Pedir(novas, imediata, motivo)`:**
- `Desejadas = novas`;
- se `Desejadas == _noDisco` → cancela o disparo e não grava;
- se `imediata` → chama `Descarregar`;
- senão → reinicia o disparo único de 2 s e zera as falhas.

**Quando o disparo chega:**
- se `gestoDoUsuarioEmCurso()` → rearma 2 s;
- senão → grava com uma tentativa.

**Se a gravação falhar:**
- rearma com `EsperasDeNovaTentativa[falhas − 1]`;
- depois da terceira falha, desiste até o próximo `Pedir`;
- `Descarregar` sempre tenta 3 vezes.

**`_noDisco` inicial:**
- é `leitura.Configuracoes` somente se a origem é Principal e o principal é `Valido`;
- em qualquer outro caso é `null`: a próxima necessidade grava e recria o principal.

### 4.7 Restauração (`Posicionador.Restaurar`)

**A cascata:**
```
destino = topologia.PorChave(salva.ChaveMonitor)                                  → PelaChave
       ?? (!salva.TelaDoMonitor.Vazio ? primeiro m de Monitores com m.Tela == salva.TelaDoMonitor : null) → PeloRetangulo
       ?? topologia.Principal                                                      → NoPrincipal
fx = Fracao(salva.FracaoX); fy = Fracao(salva.FracaoY)       // NaN → 0,5; preso em [0, 1]
r  = NoMonitor(destino, fx, fy, tamanho)
nova = salva with { ChaveMonitor = destino.Chave, FracaoX = fx, FracaoY = fy,
                    AncoraAbsoluta = r.Ancora, TelaDoMonitor = destino.Tela }
```

**Fórmulas de `NoMonitor`.** A é a área útil do destino e w×h é o sprite no DPI dele.
- **Ponto desejado:**
  - `x₀ = A.E + RoundAFZ(fx·A.L)`
  - `y₀ = A.T + RoundAFZ(fy·A.A)`
- **Horizontal:**
  - se w ≤ A.L: `x = clamp(x₀, A.E + ⌊w/2⌋, A.D − (w − ⌊w/2⌋))`;
  - senão: centraliza.
- **Vertical:**
  - se h ≤ A.A: `y = clamp(y₀, A.T + h, A.B)`;
  - senão: `y = A.B`.
- **Inversa (`Descrever`):** `fx = (x − A.E)/A.L`.

**Coordenadas negativas:** `fx·A.L` é sempre ≥ 0. O arredondamento nunca vê número negativo; o sinal só entra pela soma com `A.E` ou `A.T`. O monitor da âncora continua sendo o que contém `(x, y − 1)` (`Maquina.MonitorDaAncora`).

**Exemplos,** com sprite de 128 DIP:

| Caso | Salvo | Topologia na partida | Resultado |
|---|---|---|---|
| S1 | `LadoALado`/DISPLAY2, fx 0,5 | a mesma | (2880, 1032), pela chave |
| S2 | `SecundarioAEsquerda`/DISPLAY2, fx 0,25 | a mesma | (‑1440, 1032) |
| S3 | `EmpilhadoSecundarioAcima`/DISPLAY2, fx 0,5 | a mesma | (960, ‑48); o pixel (960, ‑49) está no DISPLAY2 |
| S4 | `DegrauDesalinhado`/DISPLAY3, fx 0,5 | a mesma | (4800, 1832) |
| S5 | `EscalasMistas`/DISPLAY3 a 192 DPI, fx 0,5 | a mesma; depois o DISPLAY3 a 144 DPI, área útil até 1368 | (‑1920, 1344) com sprite de 256 px; depois (‑1920, 1368) com 192 px |
| S6 | `Retrato`/DISPLAY2, fx 0,5 | a mesma; depois girado para `Ret(1920,0,3840,1080)`, área útil até 1032 | (2460, 1452); depois (2880, 1032) |
| S7 | `PrincipalADireita`/DISPLAY2, fx 0,5 | a mesma | (‑1280, 1032) |
| Troca de principal | `LadoALado`/DISPLAY1, fx 0,85, âncora (1632, 1032) | DISPLAY2 vira principal em (0,0); DISPLAY1 passa a (‑1920,0)-(0,1080) | (‑288, 1032), pela chave; vale a fração, não a âncora |
| S9, monitor ausente | `SecundarioAEsquerda`/DISPLAY2, fx 0,25 | `UmMonitor` | (480, 1032), no principal |
| S9, chave nova | chave antiga, tela (‑1920,0)-(0,1080), fx 0,25 | `SecundarioAEsquerda` | (‑1440, 1032), pelo retângulo; a chave passa a DISPLAY2 |
| Salvo no ar | fy = 0,321705 | a mesma | y = 332; SETTLING sem apoio → `FALLING` (queda física da Fase 4) |

### 4.8 Pasta e perfil de teste

- **Produção:** `PastaDeDados.DoBuzzy()`. Se voltar vazio, a persistência fica desligada e o log registra `CONFIG|lido=desligado|motivo=pasta`.
- **Perfil de teste:** `--perfil-de-teste NOME` → `DoBuzzy()\testes\NOME`.
- **Nomes de arquivo:** só as quatro constantes, montadas com `Path.Combine(Pasta, constante)`.
- **Criação da pasta:** a pasta só é criada na primeira gravação.

### 4.9 Log (só com `--diagnostico`)

| Momento | Linha |
|---|---|
| Partida | `CONFIG\|lido=principal\|reserva\|padroes\|desligado\|principal=<EstadoDoArquivo>\|versao=<n\|->\|avisos=<n>\|gravacao=liberada\|bloqueada\|pasta=padrao\|teste:<nome>` |
| Pedido | `CONFIG\|pedido=posicao\|preferencias\|evento=<Tipo>\|imediata=sim\|nao` |
| Gravação | `CONFIG\|gravado=sim\|nao\|sem mudanca\|motivo=<atraso\|DragEnd\|CmdExit\|SUSPENDING\|encerrar: …\|erro>\|bytes=<n>\|ms=<x.xxx>\|principalAntes=<estado>\|copiaDeDiagnostico=sim\|nao\|erro=<TipoDaExcecao>\|novaTentativaMs=<n>` |

---

## 5. Testes [AUTO] propostos

### `tests/Buzzy.Core.Testes/Persistencia/EsquemaDeConfiguracoesTestes.cs`

| Teste | Cenário → o que afirma |
|---|---|
| `Escrever_ExemploS2_IgualAAmostraV1` | A posição S2 com as preferências padrão → bytes iguais a `Amostras/settings-v1.json`. Um formato alterado por acidente falha. |
| `Ler_AmostraV1_DevolveOsValores` | Resultado `Valida`, versão 1; chave, retângulo, frações e âncora exatos. |
| `IdaEVolta_5000Aleatorias_VoltamNormalizadas` | Semente 20260930 → `Ler(Escrever(c)).Configuracoes == Normalizar(c)`, e `Escrever` é idempotente. |
| `Escrever_IndependeDaCultura` | pt‑BR, `Cultura.ComMenosTipografico()` e cultura invariante → bytes iguais, com `-1440` e `0.25`. |
| `Escrever_NuncaLanca` | NaN, ±∞, chave vazia, chave de 1025 caracteres, energia `(NivelDeEnergia)7` → saída `Valida` e normalizada. |
| `Ler_IlegivelEstrutural` | `""`, `"null"`, `"[]"`, `"{"`, `{"schemaVersion":1` (truncado), bytes `C3 28`, profundidade 9, sem `schemaVersion`, `"1"`, `1.5`, `0`, `-3`, `2147483648` → `Ilegivel`, com o `MotivoIlegivel` certo e configurações padrão. |
| `Ler_LimiteDeTamanho` | JSON válido completado com espaços até exatamente 65.536 bytes → `Valida`; 65.537 bytes → `"tamanho"`. |
| `Ler_BomComentarioEVirgulaFinal_Aceitos` | Os três casos → `Valida`. |
| `Ler_CamposDesconhecidos_Ignorados` | Campos desconhecidos na raiz, em `posicao` e em `preferencias`, e `"Energia"` com maiúscula → ignorados; os valores conhecidos continuam valendo. |
| `Ler_CampoRepetido_ValeOPrimeiro` | Dois `energia` → o primeiro vale, e há um aviso. |
| `Ler_ForaDaFaixa_PresoAoLimite` | fracaoX 1,5 → 1; ‑0,25 → 0; 1e308 → 1; esquerda ‑99999 → ‑32768; direita 99999 → 32767; y 40000 → 32767. |
| `Ler_TipoErrado_PadraoDoCampo` | energia `2`, modoTelaCheia `"sim"`, atravessarMonitores `null`, `preferencias: []` → padrões; `posicao: "x"` → nula; o resto é preservado. |
| `Ler_Energia_SoOsTresNomes` | `"baixa"`, `"MEDIA"` e `"Alta"` são aceitos; `"1"`, `"0"`, `"Baixa,Alta"`, `" media"`, `"média"`, `"turbo"` e `""` → `Media`. |
| `Ler_PosicaoTudoOuNada` | Sem chave, chave vazia, 1025 caracteres, `"\u0001"`, fracaoX em texto ou fracaoY ausente → `Posicao` nula, preferências intactas. |
| `Ler_RetanguloInvalido_ViraDesconhecido` | esquerda igual a direita, falta `base`, tipo errado → `TelaDoMonitor` padrão; a posição continua. |
| `Ler_VersaoFutura_LeOQueConhece` | `schemaVersion` 2 com campo novo → `VersaoFutura`, versão 2, valores do v1 lidos. |
| `Politica_Imediata_SoSuspensaoFimDeSessaoESair` | Uma instância de cada tipo de `Evento` → `true` só para os três. |

### `tests/Buzzy.Core.Testes/Persistencia/RestauracaoTestes.cs`

| Teste | Cenário → o que afirma |
|---|---|
| `S1aS7_GravarLerRestaurar_MesmoLugarEmCadaMonitor` | Topologias `LadoALado` (S1), `SecundarioAEsquerda` (S2), `EmpilhadoSecundarioAcima` (S3), `DegrauDesalinhado` (S4), `EscalasMistas` (S5), `Retrato` (S6) e `PrincipalADireita` (S7). Em todo monitor, com frações {(0,1), (0,25,1), (0,5,1), (0,85,1), (1,1), (0,5,0,5)}: `Descrever(NoMonitor)` → `Escrever` → `Ler` → `Restaurar` → mesmo `Posicionamento`, passo `PelaChave`. Pelo `Cenario` com `Loaded` → mesma âncora e mesmo `Retrato.ChaveMonitor`, e `Retrato.Tamanho` = 128·dpi/96 (tamanho aparente). |
| `S1aS7_AncorasCalculadasAMao` | As âncoras literais da tabela 4.7. |
| `S5_MesmaChaveOutraEscala_MesmasFracoes` | (‑1920, 1344) com 256 px → (‑1920, 1368) com 192 px. |
| `S6_MesmaChaveGirado_MesmasFracoes` | (2460, 1452) → (2880, 1032). |
| `TrocaDePrincipal_VaiPelaFracaoNaoPelaAncora` | (‑288, 1032). |
| `S9_MonitorSalvoAusente_PrincipalComAsMesmasFracoes` | (480, 1032), `NoPrincipal`; a regra da transição contém "no monitor principal". |
| `S9_ChaveNovaMesmoRetangulo_PeloRetangulo` | (‑1440, 1032), `PeloRetangulo`; a nova chave é DISPLAY2. |
| `S9_Reconectar_NaoPulaDeVolta_ESaiGravandoOPrincipal` | `Loaded(UmMonitor, salva no DISPLAY2)` → `TopologyChanged(SecundarioAEsquerda)` → âncora (480, 1032) → `CmdExit` → `GravarPosicao` com a chave DISPLAY1 e `TelaDoMonitor` (0,0)-(1920,1080). |
| `S11_BarraEmOutraBorda_MesmasFracoes` | Salvo em (1632, 1032) → `BarraNoTopo` (1632, 1080), `BarraADireita` (1579, 1080), `BarraAEsquerda` (1641, 1080). |
| `Restaurar_FracoesInvalidas_Normalizadas` | NaN → 0,5; 7 → 1; ‑1 → 0, também em `NovaPosicao`. Corrige o NaN que hoje escapa por `Carregar`. |
| `Carregar_SemPosicaoSalva_RegraInalterada` | O texto continua "BOOTING: configurações e topologia carregadas". |
| `GravarPosicao_TrazORetanguloDaEpoca` | `DRAG_END` no DISPLAY2 de `LadoALado` → (1920,0)-(3840,1080). Depois `TopologyChanged` com o DISPLAY2 em 2560×1440 e `CMD_HIDE` → retângulo novo. |
| `S12_BloquearESuspender_EmitemGravarPosicao` | `SessionLocked` → um `GravarPosicao`, e `Imediata` é falso. `Suspending` → um `GravarPosicao`, e `Imediata` é verdadeiro. Em `HIDDEN(POR_USUARIO)`, `Suspending` não emite. |

### `tests/Buzzy.Core.Testes/Persistencia/PropriedadesDaPersistenciaTestes.cs`

Mesmo estilo de `PropriedadesTestes`: 5.000 casos, semente fixa e `GeradorDeTopologias`.

| Teste | O que afirma |
|---|---|
| `RestaurarSegueACascataETerminaNaAreaUtil` | O passo certo segundo a chave e o retângulo; `VerificarPosicionamento`; `nova.ChaveMonitor == r.Monitor.Chave`; `nova.TelaDoMonitor == r.Monitor.Tela`; frações normalizadas; determinismo; `Reacomodar(t, nova)` não move. |
| `ArquivoIdaEVolta_EquivaleAReacomodarNaMesmaTopologia` | `Descrever` → `Escrever` → `Ler` → `Restaurar(t)` dá o mesmo resultado que `Reacomodar(t, pos)`. |

### Testes do núcleo que mudam

- **`InvariantesTestes`:** em todo `GravarPosicao` das 2.000 sequências, o invariante 18 vale; além disso, `Ler(Escrever(new(g.Posicao, depois.Preferencias)))` é `Valida`, com a posição normalizada.
- **`PropriedadesTestes`:** a linha 107, como na seção 3.

### `tests/Buzzy.App.Testes/ArquivoDeConfiguracoesTestes.cs`

Pasta própria em `Directory.CreateTempSubdirectory("buzzy-config-")`, apagada no `Dispose`.

| Teste | Cenário → o que afirma |
|---|---|
| `Ler_PastaVazia_PadroesSemCriarNada` | Origem Padroes; a pasta não é criada. |
| `Gravar_PrimeiraVez_SemBak` | Principal válido; nenhum `.bak`. |
| `Gravar_TresVezes_BakGuardaAAnterior` | A 2ª gravação deixa `.bak` = 1ª; a 3ª deixa `.bak` = 2ª. Confere que `ReplaceFile` sobrescreve o `.bak`. |
| `Gravar_FalhaSimuladaEmCadaEtapa_NuncaIlegivel` | Estados iniciais {vazio, principal A, principal A mais `.bak` B} × cada `EtapaDaGravacao`: o gancho lança `FalhaSimulada`, que não é `IOException`. Uma instância nova lê A ou N, nunca Padroes se A existia; nenhuma cópia de diagnóstico; a gravação seguinte funciona. |
| `Ler_EstadosDeQuedaExaustivos` | Para k de 0 a len(N): {principal A, `.tmp` N[..k]} → A; {`.bak` A, `.tmp` N[..k]} → A (reserva); {só `.tmp` N} → Padroes. |
| `Gravar_PrincipalAbertoPorOutro_FalhaSemEstragar` | `FileStream` no principal com `FileShare.Read` → `Gravou` falso e A intacto; depois de liberar, a gravação funciona. |
| `Ler_PrincipalIlegivel_UsaBak_EPrimeiraGravacaoGuardaUmaCopiaSo` | Lixo no principal e `.bak` B → Reserva B, e `Ler` não muda nenhum arquivo. `Gravar` → cópia de diagnóstico = lixo, principal N, `.bak` continua B. Com um novo lixo e nova gravação, continua havendo **uma** cópia, com o lixo novo. |
| `Ler_PrincipalIlegivelSemBak_Padroes` | Origem Padroes. |
| `Ler_VersaoFutura_BloqueiaGravacao` | `Gravar` → bloqueada; bytes inalterados. |
| `Ler_PrincipalInacessivel_UsaBakEBloqueia` | Principal aberto com `FileShare.None` → Inacessivel depois de cerca de 300 ms; usa o `.bak`; `Gravar` → bloqueada. |
| `Ler_ArquivoDe70KB_Ilegivel` | Não lê além do limite. |
| `Operacoes_SoTocamOsQuatroArquivos` | Depois de uma sequência completa, a pasta‑mãe contém só a pasta do Buzzy, e esta só tem os quatro nomes permitidos. |
| `PerfilDeTeste_ValidaNomeENuncaSaiDaPasta` | `integracao` e `a-1` aceitos. Rejeitados: `..`, `a/b`, `a\b`, `A`, `""`, 33 caracteres, `con`, `lpt1`. O caminho fica sempre sob `DoBuzzy()\testes`. |
| `MatarDuranteGravacoes_NuncaDeixaIlegivel` | 25 iterações, semente fixa. Inicia `Environment.ProcessPath --gravar-sem-parar <pasta>`, espera "pronto", aguarda de 0 a 40 ms e chama `Kill()` só no processo filho que o teste abriu. Depois: sem cópia de diagnóstico; se o principal existe, é `Valida`; senão, o `.bak` é `Valida`; `Ler().Origem` nunca é Padroes. |

### `tests/Buzzy.App.Testes/AgendaDeGravacaoTestes.cs`

Usa um agendador falso injetado em `agendarUmaVez` e uma pasta temporária.

| Teste | O que afirma |
|---|---|
| `ComAtraso_SoGravaNoDisparo_EReiniciaACadaPedido` | Nada é gravado antes do disparo; um novo pedido reinicia a espera. |
| `Imediato_GravaNaHora_ECancelaODisparo` | Grava na hora e cancela o disparo pendente. |
| `IgualAoDisco_NaoGrava` | Nenhuma gravação quando o conteúdo já está no disco. |
| `DisparoDuranteGesto_Adia2s` | Com gesto em curso, rearma 2 s em vez de gravar. |
| `Falhas_2_10_60sEDesiste` | Arquivo bloqueado: tentativas em 2 s, 10 s e 60 s, e depois para. |
| `Descarregar_GravaOPendente_ESemPendenteNaoToca` | Grava o pendente; sem pendente, não toca no disco. |
| `SemArquivo_NadaAcontece` | Com a persistência desligada, nada é gravado. |

### `tests/Buzzy.App.Testes/Integracao/PersistenciaIntegracaoTestes.cs` ([Integracao])

**`SoltarFecharEReabrir_VoltaAoMesmoLugar`** (perfil `persistencia`) cobre S2 e S7 na máquina do usuário e S1 por analogia. Passos:
1. Arrasta com `PostarMouse`, como em `GestosTestes`, até 25% da área útil do monitor com `Tela.Esquerda < 0`. Na falta dele, usa o principal.
2. Espera `CONFIG|gravado=sim|motivo=atraso`, entre 1,9 s e 4 s depois do `DragEnd`, medido por `EventoDoLog.Instante`.
3. Guarda o retângulo R1 e fecha com `FecharPorWmClose` → código 0 e `CONFIG|gravado=sem mudanca|motivo=CmdExit`.
4. O `settings.json` lido pelo `EsquemaDeConfiguracoes` é `Valida`, e a chave é a do log `POSICAO`.
5. Reabre com `limpar: false` → `CONFIG|lido=principal`, a regra contém "pela chave" e o retângulo é igual a R1.

**`PartidaComArquivoIlegivel_UsaPadroesEGuardaUmaCopia`:**
1. Grava `{ lixo` no `settings.json` do perfil.
2. Na partida: `CONFIG|lido=padroes|principal=Ilegivel` e a posição inicial.
3. Depois de fechar: a cópia de diagnóstico contém `{ lixo` e o `settings.json` é válido.

**`PerfilDeTeste_NaoTocaNoArquivoReal`:** existência, `LastWriteTimeUtc` e tamanho do `%LOCALAPPDATA%\Buzzy\settings.json` real ficam iguais antes e depois de uma sessão de integração.

### Mapeamento S1–S12 (parte de persistência)

| ID | [AUTO] | Observação |
|---|---|---|
| S1 | `S1aS7_…`, integração (por analogia) | — |
| S2 | `S1aS7_…`, âncora (‑1440, 1032), integração no hardware real | — |
| S3 | `S1aS7_…`, âncora (960, ‑48) | — |
| S4 | `S1aS7_…`, âncora (4800, 1832) | — |
| S5 | `S1aS7_…`, `S5_MesmaChaveOutraEscala_…` | — |
| S6 | `S1aS7_…`, `S6_MesmaChaveGirado_…` | — |
| S7 | `S1aS7_…`, `TrocaDePrincipal_…`, integração | O principal da máquina do usuário não é o mais à esquerda. |
| S8 | — | A queda na execução é de outra área; aqui o que importa é que a saída grave o monitor novo. [MANUAL] |
| S9 | três testes `S9_…` | — |
| S10 | `GravarPosicao_TrazORetanguloDaEpoca` | Restaurar depois de trocar a resolução. |
| S11 | `S11_…` | — |
| S12 | `S12_…` e `Politica_Imediata_…` | — |

A gravação atômica "simulando falha" é coberta por `Gravar_FalhaSimulada…`, `Ler_EstadosDeQueda…` e `MatarDuranteGravacoes…`.

---

## 6. Verificações [MANUAL]/[HW] que ficam pendentes

| Item | Por que fica pendente | Como verificar |
|---|---|---|
| S1 (secundário à direita), S3, S4 | Exigem rearranjar os monitores no Windows. É configuração global: só o usuário pode mudar. | O usuário rearranja; soltar, sair, reabrir; conferir o mesmo lugar e a regra "pela chave". |
| S5, escalas 100/150/200% | Não há hardware com escalas mistas. [HW] | Restaurar no monitor a 150% e 200%; tamanho aparente igual. |
| S6, retrato | Exige girar o monitor pelo Windows. [HW]/[MANUAL] | Salvo em paisagem, reaberto em retrato: frações mantidas, dentro da área útil. |
| S8, desconectar em execução | Ação física. [HW] | Desconectar o monitor do Buzzy, sair, reabrir: vai para o monitor em que ficou. |
| S9 real | Exige que o usuário use Win+P, "Somente tela do PC" e depois "Estender". | Reabrir → aparece no principal ("no monitor principal"); reconectar → não pula de volta. |
| S10, S11 | Mudanças reais de resolução, escala ou barra. [MANUAL] | Mudar com o app aberto, sair, reabrir: mesma posição relativa. |
| S12 | Suspensão e bloqueio reais. [MANUAL] | Conferir `CONFIG\|gravado=sim\|motivo=SUSPENDING` antes do sono, o horário do arquivo e a volta depois de desbloquear. |
| Fim de sessão real | Sair da conta ou reiniciar o Windows. [MANUAL] | Posição restaurada; log com `motivo=SessionEnding`. |
| Estabilidade da chave (P5), troca de porta | Depende da área de chave e de hardware. [HW] | Trocar a porta do cabo: deve restaurar pela chave estável ou, pelo menos, pelo retângulo. |
| Gravação só na pasta (SECURITY 8.4) | Exige o Process Monitor. [MANUAL] | Só `%LOCALAPPDATA%\Buzzy\` (e `testes\` nos testes). |
| Queda de energia no meio da gravação | Não dá para testar. | Limitação documentada: `Flush(true)` antes de substituir e rename atômico do NTFS; no pior caso volta a versão anterior. |

---

## 7. Riscos, dúvidas e interseções com outras áreas

**Dúvidas para o usuário** (não bloqueiam; a DEC‑015 dispensa aprovação):
1. **Qual posição restaurar (D4).** A adotada é "onde o Buzzy estava ao sair", incluindo o passeio autônomo. A alternativa é "a última posição em que o usuário o soltou". Para reverter:
   - campo `PosicaoEscolhida` em `EstadoDoNucleo`, atualizado só em `Soltar`, `CancelarArraste`, `FixarArrasteInterrompido`, `RedefinirPosicao` e `Carregar`;
   - `GravarPosicaoDoUsuario` passa a usar `RetornoDaTelaCheia ?? PosicaoEscolhida`.
2. **Posição no ar ou na parede.** Salva no meio de um pulo, de uma escalada ou pendurada, ela volta assim e o personagem cai na partida (SETTLING → FALLING). Achatar para o chão ao gravar seria uma regra nova no núcleo. Não foi feito.

**Riscos técnicos:**
- **Chave GDI instável.** Se a área de chave estável ficar com o nome GDI (P5 negativo), o passo 1 ("pela chave") pode escolher o monitor errado quando o Windows troca `DISPLAY1` e `DISPLAY2`. O personagem continua visível, só que no outro monitor. Se isso acontecer, reavaliar a ordem "retângulo antes da chave" para chaves GDI.
- **Formato da chave muda entre versões.** Um arquivo com chave GDI (de antes da chave estável) cai no passo "pelo retângulo" ou "no principal". É aceitável.
- **Histerese de escala (P6).** Pode deixar a âncora fora do monitor de `Posicao` → frações fora de [0, 1] → presas na gravação. A diferença na restauração é no máximo a largura da faixa de histerese.
- **Edição à mão com o Buzzy aberto** é sobrescrita na saída. Documentar: editar com o Buzzy fechado. Não há `FileSystemWatcher` (DEC‑011).
- **Reflexão desligada.** `JsonSerializerIsReflectionEnabledByDefault=false` quebraria `Buzzy.Visual/Boneco.cs`, que usa `JsonSerializer` por reflexão. Hoje o app não o chama (o vetor foi arquivado, DEC‑018). Se a Fase 6 voltar a usá-lo, é preciso migrar para um contexto gerado ou retirar o switch.
- **Gravação no encerramento.** A gravação síncrona em `SessionEnding` fica dentro do prazo do Windows (menos de 50 ms no pior caso de 3 tentativas mais o `Flush`). Uma falha de disco na saída só gera log; nunca impede o encerramento.
- **Atraso contra acomodação.** O atraso de 2 s pressupõe `IntervaloDeAcomodacao` de 3 s. Se alguém baixar esse intervalo, a gravação pode coincidir com o início de um movimento e causar um quadro atrasado, sem efeito funcional.
- **Isolamento dos testes.** Todo teste ou ferramenta que abra o `Buzzy.exe` **precisa** passar por `BuzzyEmTeste.IniciarProcesso` ou usar `--perfil-de-teste`. Sem isso, lê e grava a posição real do usuário e fica dependente da ordem de execução.

**Arquivos que outras áreas também mudam:**
- **`Maquina.cs`:** mudanças em `Carregar` e `Validar`. Travessia e escala alteram o movimento (`MoverPara` usa `Descrever`, então herda `TelaDoMonitor`). A área de sessão pode mexer em `Esconder` ou `Reaparecer`: manter o `GravarPosicao` de `Esconder` e `Sair`.
- **`Posicionador.cs` e `PosicaoDoPersonagem`:** o campo novo participa da igualdade. Quem criar ou comparar posições deve usar `Descrever`, `Reacomodar` ou `Restaurar`, ou comparar campo a campo. Mesclar este campo primeiro.
- **`Eventos.cs` e `Preferencias`:** o campo de travessia pertence à área de travessia. **Coordenar o nome no núcleo** (sugestão: `AtravessarMonitores`, padrão `true`). O nome no JSON, `atravessarMonitores`, fica fixo no v1. `Gravacao` só imprime energia e tela cheia; incluir a travessia mudaria as referências.
- **`Efeitos.cs`:** só documentação.
- **`EstadoDoNucleo.cs` e `Configuracao.cs`:** sem mudança nesta área.
- **`Aplicacao.cs`:** conflitos prováveis em `Iniciar`, `ExecutarEfeito`, `EncerrarAplicacao` e `OpcoesDaAplicacao`.
  - A área de sessão e energia cria o tratador de suspensão, que deve chamar `_gravacao.Descarregar("SUSPENDING")`.
  - A área de recuperação da janela minimizada pode mudar o `Minimizada` → `CmdHide`. Se deixar de ser `CmdHide`, deixa de haver `GravarPosicao`, sem outro efeito.
- **`Programa.cs`:** outras áreas podem acrescentar opções; juntar em `LerOpcoes`.
- **`Win32.cs`:** nenhuma mudança por esta área.
- **`Buzzy.Verificacao`:** uma eventual `VerificacaoFase5.cs` deve ser compartilhada entre as áreas. O cenário de persistência com input sintético ("soltar no secundário, sair pelo menu, reabrir") entra como seção dela.
- **Testes do núcleo compartilhados:** `PropriedadesTestes.cs` (linha 107) e `InvariantesTestes.cs` (invariante 18). As referências de `ReproducaoTestes` **não mudam** por esta área (D14); se outra área decidir imprimir o retângulo, precisará regravá-las com `BUZZY_ATUALIZAR_REFERENCIAS=1`.
- **Documentos:** ARCHITECTURE 2.6, 2.8, 2.12 e 2.13.3, SECURITY 5 e TODO Fase 5 serão editados por várias áreas. A numeração DEC‑023 e seguintes se resolve na integração.

**Opcional, fora do mínimo:** item "Redefinir posição" no menu (`CmdResetPosition`, já tratado pelo núcleo), como forma de recuperação se uma restauração cair num lugar indesejado. Exige textos novos em `Textos.resx` e mudança em `MenuNativo`.
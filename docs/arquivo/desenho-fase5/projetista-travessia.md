> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de o desenho da circulação e da travessia entre monitores no núcleo, escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a Fase 5. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-029 a DEC-031) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

# Desenho: circulação e travessia entre monitores no núcleo (Fase 5)

Tudo o que cito foi lido no código, sem compilar nem executar nada. O núcleo com física vem da cópia `f4` (`Maquina.cs`, `Movimento.cs`, `Configuracao.cs`, `EstadoDoNucleo.cs`, `Tipos.cs`, `Eventos.cs`). O app, os testes e os documentos vêm da árvore principal.

Dois fatos pesam no desenho:
- `tests/Buzzy.Core.Testes/TopologiasDeExemplo.cs` fica em **tests**, não em `src`.
- O hardware real, pelos logs de verificação, é igual a `SecundarioAEsquerda`: DISPLAY2 (-1920,0)-(0,1080), com área útil (-1920,0)-(0,1032); DISPLAY1 (0,0)-(1920,1080), com área útil (0,0)-(1920,1032); ambos a 96 DPI. Chãos e tetos coincidem. Só a travessia plana, andando e pendurado, é verificável em hardware hoje.

---

## 1. Decisões

**D1. Uma passagem nasce da adjacência exata das ÁREAS ÚTEIS, não das telas.**
- Regra: a lateral direita de A tem uma porta para N quando `N.AreaUtil.Esquerda == A.AreaUtil.Direita` e os intervalos verticais das duas áreas úteis se sobrepõem. A lateral esquerda segue a regra espelhada.
- O resto da lateral é parede, dividida em trechos.
- Motivo: `Superficies.Encostado` (f4) compara `Tela`. Com uma barra de tarefas vertical entre os monitores, ou com a barra de um deles na lateral, isso marcaria passagem e faria o sprite atravessar uma área que não é útil. Pela área útil, o invariante "sprite dentro de áreas úteis" continua exato.
- Descartadas:
  - adjacência de tela, pelo motivo acima;
  - tolerância a vãos (`VaoEntreMonitores` precisa continuar sem travessia);
  - sobreposição de telas, que o Windows não produz no modo estendido.

**D2. Compatibilidade dos chãos em três classes, medida em pixels físicos e convertida em DIP pela MENOR escala dos dois monitores.**
- `Δ = yN − yA`, com y = `AreaUtil.Base`. Δ positivo significa que o destino é mais baixo.
- `Δdip = Δ / min(sA, sN)`.
- As classes:
  - **Plana:** `Δ == 0`. Atravessa andando.
  - **Degrau:** `Δ ≠ 0` e `−SubidaMaxima ≤ Δdip ≤ DescidaMaxima`, com arco válido (D6). Atravessa por **salto de travessia**.
  - **Incompatível:** qualquer outro caso.
- Motivo da menor escala: é conservador e simétrico. A mesma passagem recebe o mesmo número nos dois sentidos.
- Descartadas:
  - tolerância "andável" com `Δ ≠ 0`: durante a travessia, o canto do sprite entraria até Δ px na barra do vizinho e quebraria o invariante de área útil;
  - converter pela escala de origem, que dá classificação assimétrica.
- Passo de 1 px vira um pulinho. É aceitável e fica com cara de macaquinho.

**D3. O alcance é fixo e igual em todos os níveis de energia (invariante 12).**
- `SubidaMaxima = 120 DIP` e `DescidaMaxima = 480 DIP`, em `ParametrosDeMovimento`.
- A energia só muda com que frequência ele anda até lá, nunca se a passagem vale.
- Um vão ou degrau fora do alcance nunca vira salto (ARCHITECTURE 2.5).
- Descartada: usar `AlturaDoPulo*` do perfil, que dependeria da energia.

**D4. Monitores empilhados (S3) não têm travessia vertical no MVP.**
- O chão continua sólido, mesmo com outra área útil logo abaixo (2.5).
- A borda superior é borda de pendurar, mesmo com outro monitor logo acima. Não existe "subir pelo teto".
- Monitores empilhados só se ligam por portas laterais de um terceiro monitor.
- Descartada: atravessar o chão ou o teto, porque contradiz 2.5 e cria "queda espontânea".

**D5. O degrau para cima além do alcance se vence pela parede, com o transbordo.**
- Andando no chão mais baixo, a lateral junto ao chão é um trecho de parede, o espelho do degrau.
- Se esse trecho tem pelo menos a altura do sprite, ele escala.
- No topo do trecho, se o chão do vizinho começa exatamente ali (`N.AreaUtil.Base == topo do trecho`), ele sobe para o chão do vizinho por um arco curto (D6).
- O transbordo não usa `SubidaMaxima`: sobe uma altura de corpo a partir da parede.
- Motivo: torna S4, S6 e `TresMonitores` percorríveis nos dois sentidos, e é o "caminho apoiado" de 2.5.
- Descartada: "descer escalando" pela beirada do chão mais alto. O salto para baixo dentro do alcance já cobre esse caso, e isso evita mais um movimento novo.

**D6. Salto de travessia e transbordo usam balística fechada, com a gravidade real e a escala da origem congelada.**
- A posição em cada passo é analítica: `x0 + vx·t` e `y0 + vy0·t + ½·g·t²`.
- O tempo de voo é o primeiro de uma lista fixa em que TODOS os passos mantêm o sprite na união das duas áreas úteis. Se nenhum servir, a porta é incompatível.
- É determinístico, não consome o gerador pseudoaleatório (`Aleatorio`) e pousa exatamente no chão de destino.
- Descartadas:
  - integrar pelo `PassoNoAr` atual: ele prende x aos limites do monitor da âncora e mudaria g e as velocidades na troca de escala no ar;
  - arco "de script" com gravidade inventada, que viola ARCHITECTURE 2.9 ("mesma gravidade").

**D7. A travessia é atômica.**
- Ela começa quando a âncora passaria do limite da origem numa porta compatível. Termina quando o sprite fica inteiro no destino.
- No meio dela, não há parada, meia-volta nem decisão. Autonomia pausada, painel aberto, fim do `Restante` e preferência desligada só valem depois.
- Só a ação do usuário interrompe (`PRESS`, e `CMD_HIDE`, sessão, suspensão e saída pelas linhas existentes) ou uma `TOPOLOGY_CHANGED`, que leva a `SETTLING`.
- Motivo: nenhum repouso autônomo fica montado entre dois monitores, e a escala troca uma única vez por travessia.
- Descartada: permitir `IDLE` montado entre dois monitores e tornar `Validar` ciente de portas. Seria mais estado e deixaria o sprite sobre a borda, exatamente onde P6 teme a oscilação.

**D8. A âncora troca de monitor exatamente na borda, pela âncora ARREDONDADA.**
- A regra é a mesma de `MonitorDaAncora`: pixel `(x, y−1)`.
- A troca acontece na hora em que a âncora cruza; o tamanho físico passa a ser o do novo monitor (2.4).
- A velocidade em px continua sendo `DIP/s × escala do monitor da âncora`, a cada passo.
- `Restante` da caminhada passa a ser guardado em **DIP** (hoje está em px da escala em que foi planejado).

**D9. A histerese de P6 tem duas camadas.**
- **(a) Movimento autônomo:** a atomicidade de D7 já é a histerese. A volta só é possível depois de uma travessia completa, ou seja, depois de a âncora andar `wA/2 + wB/2` px. Não há oscilação possível no núcleo. A troca continua exata na borda, como pede 2.4.
- **(b) Arraste:** um gatilho de Schmitt no monitor que define o tamanho.
  - Só vale quando os DPIs diferem.
  - Com `HistereseDeEscala = 8 DIP`: o monitor anterior continua valendo enquanto o pixel da âncora estiver a até H px da tela dele.
  - A âncora nunca muda por causa disso (invariante 2). A validação ao soltar continua pela regra simples.
- Descartada: histerese na regra do monitor para tudo, porque atrasaria a troca autônoma e contradiria 2.4.

**D10. Escalada e borda superior.**
- **Não escala uma passagem.** Uma parede só é escalável a partir do chão se o trecho de parede que contém `[chão−h, chão)` existe.
- **Sobe até o topo desse trecho:**
  - se o topo do trecho é o topo da área útil, vale a regra da Fase 4 (`BordaSuperior` e depois `HANGING`);
  - senão, vale `TopoDaParede`, que leva ao transbordo (D5) ou faz descer.
- **Descer pela parede a partir do teto** (quina da Fase 4) exige parede de altura inteira `[Topo, Base)`.
- **Pendurado atravessa** uma porta lateral quando:
  - os tetos coincidem (`A.AreaUtil.Topo == N.AreaUtil.Topo`);
  - a porta cobre `[Topo, Topo + max(hA, hN))`;
  - o sprite cabe em N.
- A travessia pendurado é atômica, como D7. Com tetos diferentes, vale `FimDaBorda` da Fase 4, com `paredeAdiante` calculado pelos trechos.

**D11. A preferência "atravessar monitores" (Q-05) existe em dois níveis.**
- **Capacidade:** `ConfiguracaoDoNucleo.Travessia` (padrão `false`, e o app liga na Fase 5), como `Movimento` na Fase 4.
- **Escolha do usuário:** `Preferencias.AtravessarMonitores` (padrão `true`).
- **Desligada:** a passagem vira **beirada**, nunca parede. Ele dá meia-volta, como na Fase 4, e não escala uma abertura. Uma travessia em curso termina (D7).
- Descartada: "vira parede escalável", porque não há onde se agarrar numa abertura.

**D12. Vários vizinhos no mesmo lado: a escolha é determinística e sem o gerador pseudoaleatório.**
- **Plana:** só pode haver uma, a porta cuja faixa contém `yA − 1`.
- **Degrau:** menor `|Δ|`; no empate, menor `Δ` (subir primeiro); depois, chave ordinal.
- Com isso, as sequências da Fase 4 continuam idênticas para a mesma semente.

---

## 2. Tipos e arquivos novos

### `src/Buzzy.Core/Personagem/Passagens.cs` (núcleo puro, novo)

```csharp
/// Porta: trecho [Topo, Base) da lateral x = Borda de um monitor encostado na área útil do vizinho.
public readonly record struct Porta(string ChaveVizinho, int Lado, int Borda, int Topo, int Base);

public enum TipoDeTravessia { Andando, Pendurado, Salto, Transbordo }

/// Plano de travessia em curso (parte de EstadoDoMovimento); coordenadas em px físicos.
public sealed record Travessia(
    TipoDeTravessia Tipo, string ChaveOrigem, string ChaveDestino, int Lado, int Borda,
    double XDestino, double YDestino,
    // Só Salto/Transbordo: partida, velocidades e gravidade em px/s da escala de origem, e duração em passos.
    double X0 = 0, double Y0 = 0, double VX = 0, double VY0 = 0, double G = 0, int PassosTotais = 0, int Passo = 0);

public static class Passagens
{
    /// Portas da lateral lado (−1/+1) do monitor, ordenadas por Topo. Adjacência exata da área útil (D1).
    public static IReadOnlyList<Porta> Portas(Topologia t, MonitorDoDesktop m, int lado);

    /// Trechos [Topo, Base) da lateral que são parede: [AreaUtil.Topo, AreaUtil.Base) menos as portas.
    public static IReadOnlyList<(int Topo, int Base)> TrechosDeParede(Topologia t, MonitorDoDesktop m, int lado);

    /// Andando no chão de m para o lado: travessia plana ou salto de degrau, ou nulo (incompatível ou sem porta). D2, D3, D6, D12.
    public static Travessia? NoChao(Topologia t, MonitorDoDesktop m, int lado, TamanhoDip sprite, ParametrosDeMovimento f, int passosPorSegundo);

    /// Pendurado na borda superior de m para o lado: travessia pendurado ou nulo (D10).
    public static Travessia? NoTeto(Topologia t, MonitorDoDesktop m, int lado, TamanhoDip sprite);

    /// Escalando no topo do trecho de parede que termina em topoDoTrecho: transbordo para o chão do vizinho, ou nulo (D5).
    public static Travessia? Transbordo(Topologia t, MonitorDoDesktop m, int lado, int topoDoTrecho, TamanhoDip sprite, ParametrosDeMovimento f, int passosPorSegundo);

    /// Posição do salto no passo k (1..PassosTotais), analítica; no último, exatamente (XDestino, YDestino).
    public static (double X, double Y) PosicaoNoSalto(Travessia s, int k, int passosPorSegundo);

    /// Monitor da âncora durante a travessia (D8): destino se a âncora arredondada passou da borda.
    public static bool PassouDaBorda(Travessia s, PontoPx ancora) => s.Lado > 0 ? ancora.X >= s.Borda : ancora.X < s.Borda;

    /// Soma das interseções do retângulo com as áreas úteis == área do retângulo (monitores não se sobrepõem).
    public static bool NaUniaoDasAreasUteis(Topologia t, RetanguloPx r);

    /// Monitor que dá o tamanho durante o arraste, com a histerese de D9(b).
    public static MonitorDoDesktop MonitorDoArraste(Topologia t, PontoPx ancora, MonitorDoDesktop? anterior, int histerese);
}
```

---

## 3. Mudanças em arquivos existentes

### `Personagem/Movimento.cs`

**`ParametrosDeMovimento`** ganha:
- `SubidaMaxima = 120`;
- `DescidaMaxima = 480` (DIP);
- `TemposDoSalto = [0.30, 0.40, 0.50, 0.65, 0.80]` (s);
- `HistereseDeEscala = 8` (DIP).

**`EstadoDoMovimento`:**
- `Restante` passa a ser em **DIP**.
- Novo parâmetro final `Travessia? Travessia = null`.
- Atualizar `Nenhum` e o `new EstadoDoMovimento(...)` de `IrPara` (lá sempre com `Travessia = null`).

**`Superficies.Do`:**
- `ParedeEsquerda` e `ParedeDireita` passam a significar "escalável a partir do chão": existe um trecho de parede que contém `[Chao − h, Chao)`.
- Campos novos:
  - `int TopoDaParedeEsquerda` e `int TopoDaParedeDireita`: o y do topo desse trecho, ou `Chao` se não houver;
  - `bool ParedeInteiraEsquerda` e `bool ParedeInteiraDireita`: o trecho cobre `[AreaUtil.Topo, AreaUtil.Base)`, usado na descida a partir do teto.
- `NaParede(x, out lado)` passa a usar `ParedeInteira*`: é a quina do teto que o `Decidir` usa em `HANGING`.
- `Encostado` sai e é substituído por `Passagens.Portas`.

### `Personagem/Configuracao.cs`

- `ConfiguracaoDoNucleo.Travessia` (bool, padrão `false`).

### `Personagem/Eventos.cs`

- `Preferencias(NivelDeEnergia Energia, bool ModoTelaCheia, bool AtravessarMonitores = true)`.
- `Padrao` continua `(Media, true, true)`.

### `Personagem/Tipos.cs`

- `SinalDeMovimento.Degrau`: "andando, chegou a uma passagem com degrau alcançável".
- Não há evento novo nem efeito novo. `MoverJanela` já leva o monitor e o tamanho, e `Retrato` já tem `ChaveMonitor` e `Tamanho`.

### `Personagem/Gravacao.cs`

- `SettingsChanged` e `Loaded` escrevem `travessia=nao` **só quando for falso**. `LerPreferencias` lê com padrão `sim`.
- Com isso, as referências 01 a 05 não mudam.
- `ReproducaoTestes.LerCabecalho` aceita a diretiva `# travessia: sim`.

### `Personagem/Maquina.cs` (f4)

**`PassoAndando`:**
1. Se `mv.Travessia is {Tipo: Andando}`, avança sem conferir `Calmo` nem `Restante`, com a regra 4.1. Ao completar, põe `Travessia = null` e, se `Calmo` ou `Restante ≤ 0`, faz `IrPara(Idle, "WALKING: fim do percurso depois da travessia")`.
2. Senão, na borda (`naBorda`), consulta a porta **antes** de testar a parede:
   - se `_cfg.Travessia && Preferencias.AtravessarMonitores && !Calmo` e `Passagens.NoChao(...)` devolve `Andando`: grava o plano, faz `Sinalizar(Passagem)` (transição `Walking→Walking`, "WALKING: passagem (atravessa)") e **não** prende x ao limite;
   - se devolve `Salto`: grava o plano e faz `Sinalizar(Degrau)`, que leva a `JUMPING`;
   - senão, se a lateral é parede escalável, vale o caminho atual: `QuerEscalar` ou `Sinalizar(Parede)`;
   - senão, meia-volta como hoje, com a regra "WALKING: passagem incompatível ou travessia desligada: vira".
3. `Restante −= VelocidadeAndando / PassosPorSegundo`, que agora é DIP.

**`PassoEscalando`:**
- Subindo, o limite é `TopoDaParede(lado) + h`.
- Se `TopoDaParede == AreaUtil.Topo`, vale a regra atual (`BordaSuperior`).
- Senão:
  - `Sinalizar(TopoDaParede, transbordo)`, com `transbordo = Passagens.Transbordo(...)` se a travessia estiver habilitada e não houver calma;
  - com transbordo, vai a `JUMPING` com o plano;
  - sem transbordo, `SentidoVertical = +1` (desce).
- Com `Movimento = false` (sinal injetado), `TopoDaParede` mantém o `Escolher` ponderado da Fase 2.

**`PassoPendurado`:**
- Com `Travessia {Tipo: Pendurado}`: avança atômico, com y igual a `AreaUtil.Topo + h` do monitor da âncora.
- Na borda:
  - `NoTeto` compatível e travessia habilitada → plano e `Sinalizar(Passagem)`, "HANGING: passagem compatível (atravessa)";
  - senão → `FimDaBorda` com `paredeAdiante = ParedeInteira*`.
- `Calmo` com travessia em curso espera o fim. Depois vem `Falling`, a regra atual.

**`PassoNoAr`:**
- Com `Travessia {Tipo: Salto or Transbordo}`: `Passo + 1` e `PosicaoNoSalto`.
- O monitor é o destino se a âncora já passou da borda; senão, a origem.
- No último passo:
  - `MoverPara(destino, XDestino, YDestino)`;
  - zera VX e VY;
  - `Travessia = null`;
  - `Sinalizar(ContatoComOChao)`, que leva a `LANDING`.
- Sem plano: o comportamento atual. Pulos e quedas autônomos continuam presos ao monitor da âncora e nunca atravessam.

**`Sinalizar`:**
- Casos novos:
  - `(Walking, Degrau)` → `IrPara(Jumping, "WALKING: degrau alcançável (salto de travessia)")`, preservando o plano;
  - `(Hanging, Passagem)` → `Hanging`;
  - `(Climbing, TopoDaParede)` com física: transbordo ou descida.
- Atenção: `IrPara` zera `Movimento` ao entrar em `JUMPING`. É preciso gravar o plano **depois** do `IrPara`, como `PlanejarPulo` já faz com `VX` e `VY`.

**`Pausar`:**
- Hoje, `Walking` pausado vai direto a `Idle`. Passa a só fazer isso se `Movimento.Travessia is null`. Com travessia em curso, `PassoAndando` para ao completar.

**`PlanejarCaminhada`:**
- Com porta compatível à frente e travessia habilitada, `livre = +∞`: não vira.
- `Restante = dip`, já sem multiplicar pela escala.

**`DecidirParado` e `PlanejarEscalada`:**
- Continuam usando `ParedeEsquerda` e `ParedeDireita`, agora com a semântica nova.

**`Arrastar`:**
- `LugarLivre` passa a receber o monitor anterior: `Passagens.MonitorDoArraste(t, ancora, _s.Lugar?.Monitor, H)`, com `H = round(HistereseDeEscala × anterior.Escala)`.
- `Soltar` e `Validar` ficam como estão.

**`MudarPreferencias`:**
- Sem transição. Desligar a travessia não cancela o plano em curso (D7, D11).

### `Composicao/Aplicacao.cs`

- `ConfiguracaoDoNucleo { ..., Travessia = true }`.
- As outras mudanças dessa área (P6) estão na seção 7.

### Linhas novas ou alteradas da tabela de ARCHITECTURE 2.6

| De | Evento | Para | Regra |
|---|---|---|---|
| `WALKING` | passagem compatível de mesma altura | `WALKING` | Atravessa andando. A travessia é atômica: nenhuma parada, meia-volta ou decisão até o sprite estar inteiro no destino. O monitor e o tamanho trocam quando a âncora cruza a borda. |
| `WALKING` | passagem com degrau alcançável (`Degrau`) | `JUMPING` | Salto de travessia com arco validado. O contato com o chão do destino leva a `LANDING`. |
| `WALKING` | passagem incompatível, travessia desligada ou autonomia calma | `WALKING` (vira) ou `IDLE` | Uma abertura nunca é escalada nem vira queda. |
| `CLIMBING` | topo de um trecho de parede com o chão do vizinho logo acima | `JUMPING` ou `CLIMBING` (desce) | Transbordo para o chão do vizinho. Sem ele (desligado, calmo ou arco inválido), desce. |
| `HANGING` | passagem compatível (tetos iguais) | `HANGING` | Atravessa pendurado, de forma atômica. |
| `DRAGGING` | `DRAG_MOVE` entre monitores de DPI diferente | `DRAGGING` | O tamanho só troca depois de a âncora passar a histerese; a âncora continua sendo o cursor menos a pegada. |
| qualquer | `SETTINGS_CHANGED` que desliga a travessia | permanece | Uma travessia em curso termina; novas não começam. |

---

## 4. Regras exatas

**Notação para o monitor M.**
- `U = M.AreaUtil`; `s = M.Dpi/96`; `(w, h) = Tamanho.ParaPixels(M.Dpi)`; `a = w/2` (divisão inteira); `b = w − a`.
- Limites da âncora: `esq = U.Esquerda + a`, `dir = max(esq, U.Direita − b)`, `chao = U.Base`, `teto = min(chao, U.Topo + h)`.
- Todo retângulo é semiaberto, e todo arredondamento é `MidpointRounding.AwayFromZero`, como em `MoverPara`.

### 4.1 Travessia andando (Plana)

**Condições** (A é a origem, N o vizinho do lado `L`, E a borda):
1. `Δ == 0`.
2. Existe uma porta em E com `porta.Topo ≤ chao − max(hA, hN)` e `porta.Base == chao`.
3. `N.dir ≥ N.esq`, ou seja, o sprite cabe em N.

**A cada passo:**
- `x += L × VelocidadeAndando × s(monitor da âncora) / PassosPorSegundo`.
- `ancora = round(x)`.
- O monitor é N se `PassouDaBorda`; senão, A.
- `MoverPara(monitor, x, chao)`.
- Completa quando o sprite está inteiro em N: `L > 0` e `round(x) ≥ N.esq`, ou `L < 0` e `round(x) ≤ N.dir`.
- Na hora da troca de monitor, x fica contínuo em px (o desktop virtual é contínuo em pixels físicos). Só a velocidade e o tamanho mudam.

**Coordenadas negativas (hardware real, E = 0):**
- Âncora 0 fica em DISPLAY1; −1 fica em DISPLAY2.
- `round(−0.5) = −1`, então a troca é simétrica em torno de 0.
- Indo para a esquerda:
  - início da travessia quando `x < esqA = 64`;
  - âncora em DISPLAY2 quando `round(x) ≤ −1`;
  - completa em `round(x) ≤ N.dir = 0 − 64 = −64`.
- Indo para a direita, a partir de DISPLAY2:
  - início quando `x > dir = −64`;
  - troca em `round(x) ≥ 0`;
  - completa em `round(x) ≥ 64`.

### 4.2 Salto de travessia (Degrau) e transbordo

**Partida e destino.**
- Degrau: partida `P0 = (L > 0 ? dirA : esqA, chaoA)`.
- Transbordo: partida `P0 = (limite da parede, topoDoTrecho + hA)`.
- Destino `P1 = (L > 0 ? N.esq : N.dir, chaoN)`.
- `g = Gravidade × sA`, fixo durante todo o voo.

**Escolha do tempo de voo.** Para cada T em `TemposDoSalto`:
- `vx = (x1 − x0) / T` e `vy0 = (y1 − y0 − ½·g·T²) / T`;
- `n = ceil(T × PassosPorSegundo)`;
- para `k = 1..n`: `t = min(k/60, T)`, posição analítica, âncora arredondada, monitor pela borda, retângulo com o tamanho desse monitor;
- exige `Passagens.NaUniaoDasAreasUteis(t, r)` em todos os k.

O primeiro T válido vence. Se nenhum servir, a porta é incompatível.

**Cálculo da união.** `Σ_M |r ∩ M.AreaUtil| == |r|`, em `long`, sobre todos os monitores. É exato porque as áreas úteis não se sobrepõem.

### 4.3 Travessia pendurado

**Condições:**
- `U_A.Topo == U_N.Topo`;
- a porta tem `Topo == U.Topo` e `Base ≥ U.Topo + max(hA, hN)`;
- `N.dir ≥ N.esq`.

**A cada passo:**
- y é `U.Topo + h` do monitor da âncora. As mãos ficam na borda; a âncora desce ou sobe com o tamanho.
- Velocidade: `VelocidadePendurado`.

### 4.4 Vizinhos por lateral

`Portas` percorre `topologia.Monitores` e ignora `Chave == m.Chave`. Com monitores sem sobreposição, as portas de um lado são disjuntas. A escolha entre elas segue D12.

### 4.5 Histerese no arraste

Em `MonitorDoArraste(t, p, anterior, H)`:
1. `m = MonitorDaAncora(t, p)`.
2. Se `anterior` existe em `t`, `m.Chave ≠ anterior.Chave`, `m.Dpi ≠ anterior.Dpi` e `anterior.Tela.DistanciaAoQuadrado((p.X, p.Y−1)) ≤ H²`, devolve `anterior`.
3. Senão, devolve `m`.

### 4.6 Apoio durante as travessias

- **Andando e pendurado:** o y está sempre no chão ou no teto do monitor da âncora, e o sprite fica na união das áreas úteis (garantido pelas condições de 4.1 e 4.3).
- **Salto:** é `JUMPING`, e o arco foi validado passo a passo.

### 4.7 Interrupções

| Interrupção | Efeito |
|---|---|
| `PRESS` | Segura na hora, mesmo montado entre dois monitores. Depois vem `SETTLING` com `Validar` (prende no monitor da âncora, salto de no máximo meio sprite). |
| `TOPOLOGY_CHANGED` com configuração diferente | `SETTLING` e reacomodação; o plano cai, porque `IrPara` o zera ao reentrar em movimento. |
| `HIDDEN` | O plano é ignorado ao reaparecer (`SETTLING`). |
| Pausa ou painel aberto | Esperam o fim da travessia. |

### 4.8 Casos que nunca atravessam

- `VaoEntreMonitores`: não há adjacência (1920 ≠ 2120).
- `QuinaComQuina`: a sobreposição vertical é vazia.
- `EmpilhadoSecundarioAcima`: não há porta lateral (D4).
- Barra vertical entre dois monitores: as áreas úteis não se tocam, então é parede.

### 4.9 Resultados esperados nas topologias de exemplo

Estes valores servem de base para os testes, calculados à mão.

| Topologia | Passagem | Resultado |
|---|---|---|
| `LadoALado`, `SecundarioAEsquerda`, `PrincipalADireita` | D1–D2 | Plana, porta [0,1032). `PrincipalADireita` também é plana: D2 tem área útil com base em 1032 e porta [0,1032). |
| `EmL` | D1–D2 | Plana. |
| `EmL` | D1–D3 | Não há porta lateral, então não atravessa. |
| `DegrauDesalinhado` | D1→D2 | Porta [400,1032); Δ = +400 px = 400 DIP, dentro de 480: salto. |
| `DegrauDesalinhado` | D2→D1 | Δ = −400, além de 120. O trecho de parede [1032,1432) na lateral esquerda de D2 tem 400 ≥ 128: escala e faz transbordo no topo 1032, que é a base da área útil de D1. |
| `DegrauDesalinhado` | D2–D3 | Mesmo padrão de D1–D2. |
| `Retrato` | D1→D2 | Δ = +420 DIP: salto. |
| `Retrato` | D2→D1 | Parede [1032,1452) com 420 ≥ 128: transbordo. |
| `Retrato` | tetos | 0 ≠ −420: não atravessa pendurado. |
| `EscalasMistas` | D1↔D2 | Δ = ±24 px; menor escala 1 → 24 DIP: salto nos dois sentidos. |
| `EscalasMistas` | D1↔D3 | Δ = ∓24 px; menor escala 1,5 → 16 DIP: salto. |
| `EscalasMistas` | tamanhos | O tamanho troca na borda: 192 → 128 px e 192 → 256 px. |
| `TresMonitores` | D1→D2 | Δ = −168 DIP (subida): parede [1212,1380), 168 ≥ 160: transbordo. |
| `TresMonitores` | D2→D1 | +168 DIP: salto. |
| `TresMonitores` | D1→D3 | −360 px = 288 DIP, além de 120: transbordo pela parede [1020,1380). |
| `TresMonitores` | D3→D1 | +288 DIP: salto. |

---

## 5. Testes [AUTO] propostos

### `tests/Buzzy.Core.Testes/Movimento/PassagensTestes.cs` (geometria pura, valores literais)

- **`Portas_PorAdjacenciaDaAreaUtil_NasTopologiasDeExemplo`:** tabela esperada de portas e trechos de parede para cada topologia de 4.9, mais `UmMonitor` sem portas.
- **`Portas_BarraVerticalEntreMonitores_EhParede`:** topologia nova `BarraVerticalEntre`: `LadoALado` com a área útil de D2 começando em 1982.
- **`Classificacao_PlanaDegrauIncompativel_PelaMenorEscala`:** os casos de 4.9, incluindo o 24 px → 16 DIP e o limite 120/121 e 480/481 DIP em topologias sintéticas.
- **`Escolha_VariosVizinhosNoMesmoLado_Deterministica`:** dois monitores empilhados à direita de um alto (D12).
- **`SaltoDeTravessia_ArcoValidadoPousaExatoESpriteSempreNaUniao`:** todos os passos de `PosicaoNoSalto` estão na união; o último é igual a P1.
- **`SaltoDeTravessia_PortaBaixaDemais_Incompativel`:** uma porta de 100 px com sprite de 128 px.
- **`MonitorDoArraste_HistereseSoComDpiDiferente`:**
  - `EscalasMistas` na borda x = 2560: 2560+7 continua em D1; 2560+13 vai para D2 (H = round(8 × 1,5) = 12);
  - a volta é simétrica;
  - com o mesmo DPI (`LadoALado`), a troca é imediata.

### `tests/Buzzy.Core.Testes/Movimento/TravessiaTestes.cs` (com `SimuladorDeTempo`, `Fase5 = Fase4 with { Travessia = true }`)

**Travessia plana e cenários S:**
- **`S2_AtravessaAndandoParaXNegativoEVolta`:** `SecundarioAEsquerda`.
  - A âncora troca para DISPLAY2 exatamente em `round(x) = −1`.
  - Não há passo com avanço maior que 2 px.
  - Completa em x ≤ −64; depois atravessa de volta.
  - Nunca está em Idle com x em (−64, 64).
  - Com `ConferirApoio` novo a cada evento.
- **`S1_LadoALado_AtravessaNosDoisSentidos`** e **`S7_PrincipalADireita_Atravessa`**.
- **`S3_Empilhados_NuncaMudaDeMonitorSozinho`:** também numa variante sem barra, com as áreas úteis encostadas no eixo vertical.
- **`S4_Degrau_DesceSaltandoESobePelaParedeComTransbordo`:** sementes fixas.
  - Afirma `WALKING → JUMPING → LANDING` em D2 e `CLIMBING → JUMPING → LANDING` em D1.
- **`S5_EscalasMistas_TamanhoTrocaNaBordaEVelocidadeEmDip`:**
  - no passo da troca, o `Tamanho` passa de 192×192 para 128×128;
  - o avanço por passo passa de ~2,25 px para ~1,5 px;
  - `Restante` é consumido em DIP: distância igual em DIP antes e depois.
- **`S6_Retrato_SaltaParaBaixoESobePorTransbordo`**.
- **`TresMonitores_TransbordoEmEscala125`**.

**Casos que não atravessam:**
- **`Vao_E_Quina_NuncaAtravessam`:** `VaoEntreMonitores` e `QuinaComQuina`, 30 sementes, 3 min cada. A chave do monitor nunca muda.
- **`TravessiaDesligada_DaMeiaVoltaENaoEscalaAPassagem`:** `Preferencias(..., AtravessarMonitores: false)` e `Travessia = true` na configuração.

**Interrupções:**
- **`PausaOuPainelNoMeio_CompletaATravessiaEDepoisPara`:** para em x ≤ −64, nunca montado entre os monitores.
- **`PressNoMeio_SeguraMontadoESoltarAcomodaNoMonitorDaAncora`**.
- **`TopologiaMudaNoMeio_VaiParaSettlingEDescartaOPlano`:** cenário S8 no núcleo, com `SemMonitor` do destino.
- **`HangingAtravessaComTetosIguais_NaoComTetosDiferentes`:** `SecundarioAEsquerda` contra `Retrato`.

**Propriedades e compatibilidade:**
- **`MesmaSemente_MesmaTravessia`:** invariante 7, com `LadoALado` e `EscalasMistas`.
- **`ApoioEUniao_EmMilharesDePassos`:** versão Fase 5 de `ApoioValeEmMilharesDePassosComInteracoesEMudancasDeTopologia`, com `GeradorDeTopologias` e travessia ligada.
- **`Fase4SemTravessia_ContinuaIgual`:** a mesma semente com `Travessia = false` gera a mesma trilha da Fase 4 em `UmMonitor`.

### Arquivos existentes

**`MovimentoTestes.ConferirApoio`:**
- Troca `area.Contem(l.Retangulo)` por `Passagens.NaUniaoDasAreasUteis`.
- `CLIMBING` passa a conferir o trecho de parede.
- `Passagem_ParaOutroMonitorNaoEhAtravessadaNaFase4` continua com `Fase4` (sem travessia).

**Outros:**
- `InvariantesTestes.SinalCoerente`: acrescentar `Degrau` a `Walking`.
- `TransicoesTestes`: uma linha nova por linha da tabela da seção 3, com sinais injetados.
- Reprodução `Referencias/06-travessia.txt`, com cabeçalho `# movimento: sim`, `# travessia: sim` e topologia `SecundarioAEsquerda`, entrando em `ReproducaoTestes.Referencias`.
- `GeradorDeTopologias.Mudar`: nada muda (já cobre conexão, desconexão e DPI).

### Integração `[Integracao]`

**`tests/Buzzy.App.Testes/Integracao/TravessiaIntegracaoTestes.cs`:**
- Escolhe, por simulação do núcleo com a topologia real (como `EscolherSemente`), uma semente que atravessa nos primeiros 30 s.
- Afirma que o retângulo da janela passa de DISPLAY1 para x negativo e que fica sempre na união das áreas úteis.
- Só posta mensagens à própria janela.

---

## 6. Pendências [MANUAL] e [HW]

| Pendência | Por quê |
|---|---|
| S2 [MANUAL][HW] | É possível agora: `Buzzy.Verificacao --fase 5` com input SINTÉTICO só para pausar e retomar. A observação é a janela cruzando x = 0 pelo log `POSICAO`, sem gravar cada passo. |
| S1, S3, S4, S6, S7 [HW] | Exigem rearranjar ou girar monitores. Isso é configuração global do Windows e o agente não a altera. |
| S5 e P6 [HW] | Não há DPI misto. Ficam pendentes: o ponto real em que o Windows troca o DPI da janela, a ausência de oscilação e a calibração de `HistereseDeEscala`. |
| S8 [HW] | Desconectar no meio de uma travessia. |
| Critério 5 da Fase 4 (120 qps) com travessia [MANUAL] | Exige gravação de tela a 120 quadros por segundo. |
| Evidência humana | Todo input da verificação é sintético e não conta como evidência humana. |

---

## 7. Riscos, dúvidas e arquivos compartilhados com outras áreas

**Riscos:**
1. **P6 no app, fora do núcleo** (coordenar com a área de troca de escala).
   - Com a âncora exatamente na borda e largura par, a janela fica 50/50 entre os monitores. O Windows pode manter o DPI antigo e o WPF desenharia o bitmap do DPI novo pela metade, por um quadro.
   - Proposta, a ser feita em `JanelaPersonagem.cs`:
     - (a) tratar `WM_DPICHANGED` sem aplicar o retângulo sugerido, reaplicando o lugar do núcleo;
     - (b) dimensionar a `Image` em DIP pelo DPI atual da janela, para o bitmap preencher a janela sempre pixel a pixel.
   - `Win32.cs` precisa da constante `WM_DPICHANGED = 0x02E0`.
2. **`Maquina.cs` é o ponto de maior conflito.**
   - Mudam `PassoAndando`, `PassoEscalando`, `PassoPendurado`, `PassoNoAr`, `Sinalizar`, `Pausar`, `PlanejarCaminhada`, `Arrastar`/`LugarLivre` e `MudarPreferencias`.
   - As áreas de restauração e persistência mexem em `Carregar`, `MudarTopologia` e `GravarPosicao`, e as de sessão e suspensão em `Esconder`/`Reaparecer`.
   - Mesclar nessa ordem: primeiro a f4, depois esta área.
3. **`EstadoDoNucleo.cs` e `Movimento.cs`:** `EstadoDoMovimento` ganha `Travessia`. O `Restante` em DIP muda a semântica que a Fase 4 testou; conferir `Caminhada_AndaSoPeloChao...` e o teste de energia.
4. **`Eventos.cs` (`Preferencias`) e `Gravacao.cs`:** a área de `settings.json` também acrescenta campos. É preciso combinar o nome (`atravessarMonitores`) e o padrão `true`. A carga saneia os valores (SECURITY.md 7).
5. **`Configuracao.cs`:** `Travessia` e os parâmetros novos. A área de histerese e P6 pode querer `HistereseDeEscala` noutro lugar; o núcleo precisa dela só no arraste.
6. **`MonitorDoDesktop.Chave` (P5):** portas, planos e histerese comparam por `Chave`. Chave estável não quebra nada. O plano guarda chaves e, na troca de topologia, é descartado.
7. **`TopologiasDeExemplo.cs`:** entradas novas (`BarraVerticalEntre`, `SecundarioAEsquerdaSemBarra`, empilhado sem barra) passam a ser percorridas por `SoltarNasTopologiasTestes` e pelos demais testes que usam `Todas`. Os testes calculam o esperado de forma independente, mas outras áreas também vão acrescentar topologias ali.

**Dúvidas para registrar em DEC-023:**
- Os valores 120 e 480 DIP, os tempos de salto e os 8 DIP são iniciais. Calibrar nas verificações de tela e em P6.
- É preciso confirmar se "passagem vira beirada" com a travessia desligada é aceitável como leitura de Q-05.

**Documentos a sincronizar na implementação:**
- ARCHITECTURE 2.4: exceção de histerese no arraste.
- ARCHITECTURE 2.5: portas por área útil, degrau e transbordo, empilhados.
- ARCHITECTURE 2.6: linhas novas.
- ARCHITECTURE 2.9: salto de travessia.
- TODO, Fase 5: mapa S1–S12.
- DEC-023 nova.
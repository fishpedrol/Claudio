> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de o desenho do núcleo da emoção dominante e do tamagotchi adulto (o código o cita como "desenho do núcleo", seções 2.4 e 4.1 a 4.11), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a emoção dominante e o tamagotchi. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-027 e DEC-028) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

# Desenho do núcleo puro: emoção dominante (DEC-027) e tamagotchi adulto (DEC-028)

**Escopo desta área:** `src/Buzzy.Core` e os testes dele. O desenho sai do código atual, que li inteiro nestes arquivos:

- `Maquina.cs`, `EstadoDoNucleo.cs`, `Eventos.cs`, `Efeitos.cs`, `Tipos.cs`, `Configuracao.cs`, `Movimento.cs`, `Aleatorio.cs`, `Nucleo.cs`, `Gravacao.cs`;
- `Persistencia/*`, `Posicionador.cs`, `Entrada/ArbitroDeGestos.cs`;
- `Aplicacao.cs`, `PoseDoPersonagem.cs`, `JanelaPersonagem.cs`, `MenuNativo.cs`;
- os testes `InvariantesTestes`, `ReproducaoTestes`, `Cenario`, `SimuladorDeTempo`, `EsconderijoTestes` e a referência 04;
- a crítica de integração da Fase 5.

Não editei nem compilei nada.

**Fora desta área (outras áreas):** a janela do item no app, os submenus e ícones, a arte (caras, gestos, poses de uso e itens) e o tratamento dos efeitos novos em `Aplicacao.ExecutarEfeito`. O contrato que essas áreas consomem está na seção 2.4.

---

## 1. Decisões numeradas

| # | Decisão | Motivo | Alternativas descartadas |
|---|---|---|---|
| D1 | **O item é uma entidade do núcleo.** A invocação, a queda, o arraste e o teste de "soltou sobre ele" ficam no núcleo; o app só desenha as janelas dos itens a partir dos efeitos. | A DEC-022 recusou física no app porque tiraria o determinismo e os testes sem janela. Assim, a reprodução gravada cobre o ciclo inteiro. | O app decidir "soltou sobre o personagem" por hit-test (não reproduzível e regra no adaptador); a queda animada no WPF. |
| D2 | **"Sobre o personagem"** quer dizer que o retângulo do item, já preso na área útil, cruza o retângulo do sprite encolhido 20% de cada lado (`MargemDoAlvo = 20`). | O núcleo não conhece o alfa. O miolo do quadro cobre o corpo em todas as poses, e a regra é generosa sem aceitar soltar no canto transparente. | O pixel do cursor sobre um pixel opaco (o alfa é da apresentação); o centro do item dentro do sprite (difícil de acertar). |
| D3 | **Estado novo `USING`** (enum `Estado.Using`), no grupo *usuário*. O relógio corre durante o uso. A saída é pela acomodação (`Acomodar`), como no fim de `REACTING`. | Ele reaproveita a volta ao apoio certo que já existe: chão → `IDLE`; parede e cipó → agarrado, mantendo `PresoPeloUsuario`; esconderijo → `PEEKING`. No grupo usuário, o `Nucleo` já descarta o `AUTONOMY_TIMER`. | Um gesto curto (gestos só existem em `IDLE` e não guardam o apoio); uma dimensão ortogonal (a agenda poderia interromper o uso). |
| D4 | **Tabela de aceitação** (seção 4.6). Quando o soltar é recusado, o item cai de onde foi solto e continua esperando. | É previsível e não perde o item. | Fila de itens para usar depois; pegar no ar (um estado pendente a mais). |
| D5 | **Atento:** enquanto o usuário segura um item, a agenda fica pausada e o movimento para no lugar. A caminhada para, a parede e o cipó ficam agarrados, quem descansa acorda, e pulo e queda terminam. | Facilita soltar o item nele. O `Calmo` atual não serve: faz descer da parede e soltar do cipó, o que contraria a DEC-024. | Não reagir (o usuário teria de perseguir o personagem); reusar `Calmo`. |
| D6 | **A onda começa no instante em que o uso é aceito;** a subida cobre a animação. Interromper o uso não devolve o item. | Um caminho de código só, sem estado de "item na mão" que precise voltar ao chão. | Consumir no fim da animação e devolver o item ao chão se o uso for interrompido. |
| D7 | **Fases com disparos únicos:** Subida →
Pico por níveis → Queda → fim. Existe **um temporizador só** (`AgendarOnda`/`ItemEffectTimer`), sem relógio de passo fixo. | Cumpre a DEC-011: só há disparos únicos, espaçados de 1 s ou mais. O núcleo não tem relógio de parede, então ele só avança nos próprios disparos e nunca precisa saber quanto tempo passou. | Pulso periódico (viola a DEC-011); tempo real dentro dos eventos (muda o contrato do núcleo); decaimento contínuo por `Tick` (relógio ligado em repouso). |
| D8 | **Combinação dos itens:** o mesmo tipo acumula, com teto no nível 3. Um tipo de precedência maior ou igual vai para a frente, e a onda anterior espera atrás, congelada; cabem só duas. Um tipo de precedência menor é absorvido. A água baixa um nível. | Com "a última manda", o lança-perfume (30 s) "curaria" uma bebedeira. Com uma só onda na frente, nunca é preciso saber o tempo que já passou (D7). | "A última manda"; um vetor de ondas simultâneas (exigiria o tempo decorrido); soma sem teto. |
| D9 | **Comportamento pelo `PerfilEfetivo`:** percentuais aplicados sobre o perfil de energia, mais gestos e caras por fase. Com a autonomia pausada, o disparo da onda só troca a cara. | Uma função pura, que o teste R11 reusa como oráculo. | Um perfil de energia inteiro por onda (multiplicaria as combinações com Baixa/Média/Alta). |
| D10 | **Exceção documentada ao invariante 12:** a onda multiplica só as velocidades de andar, escalar e pendurar, entre 50% e 200%, e faz a caminhada cambalear. A gravidade, a queda máxima, o quique, a velocidade do foguete, as colisões e os limites não mudam. O cambaleio usa uma onda triangular com aritmética básica. | O pedido fala em "bêbado cambaleia" e "agitado corre". `+ − × ÷` dão o mesmo resultado em qualquer máquina (IEEE). | Não mudar a velocidade (efeito fraco); `Math.Sin` (sem garantia de resultado idêntico entre plataformas). |
| D11 | **Sete caras de efeito novas no fim do enum `Expressao`:** `Bebado`, `Enjoado`, `Chapado`, `Eletrico`, `Apaixonado`, `Tonto`, `Viajando`. O sorteio automático passa a usar a lista fixa das 14 caras de humor. | Assim a agenda faz exatamente os mesmos sorteios de hoje, e as referências 01–05 não mudam. Hoje ela usa `Enum.GetValues<Expressao>().Length`, que mudaria com os valores novos. | Uma camada de efeito sobreposta à cara (arte mais cara); só as 14 caras (o efeito não se leria). |
| D12 | **Seis gestos novos no fim de `Gesto`:** `Soluco`, `Danca`, `Gargalhada`, `Espirro`, `Tosse`, `Tremedeira`. Só acontecem em `IDLE` e só a onda os sorteia. | Mantém o invariante 15 e o sorteio atual, que vai de `Espiar` a `Brincar`. | Gestos na parede ou no cipó (mudaria o invariante 15). |
| D13 | **Emoção dominante** é `Preferencias.EmocaoDominante` (`Expressao?`, com nulo = automática), limitada às 14 caras de humor. Ela é a cara de base e pesa 6 no sorteio, contra 1 de cada uma das quatro companheiras. Nunca muda ações nem física. Cada sorteio de cara continua consumindo **exatamente um** `Sortear()`. | O último ponto dá um teste forte: com a mesma semente, a dominante só muda as caras. | Mexer nos pesos das ações (fora do escopo da DEC-027); sortear a dominante com um `Entre` a mais (desalinha o gerador). |
| D14 | **Persistência:** `preferencias.emocaoDominante` guarda `"automatica"` ou o nome da cara, e o esquema passa a `schemaVersion` 2. Um arquivo v1 é lido sem migração e sem aviso. A onda, o uso e os itens **não** são gravados. | Segue a regra "toda ampliação do esquema incrementa a versão": um build antigo vê `VersaoFutura` e não grava por cima. | Manter v1 (um build antigo apagaria o campo); gravar a onda (dado inútil e sensível). |
| D15 | **Os itens ficam atrás de `ConfiguracaoDoNucleo.Tamagotchi`**, desligada por padrão. `DoAplicativo` só a liga na mesma entrega em que o app e a arte estiverem prontos. A emoção dominante não tem chave. | Com `Tamagotchi` desligada, nenhum efeito novo sai do núcleo, e `Aplicacao.ExecutarEfeito` lança exceção para efeito desconhecido. As referências também não mudam. | Ligar direto (o app quebraria). |
| D16 | **Nomes:** eventos em inglês, como na tabela 2.6; tipos em português. O efeito do item se chama **`Onda`**, porque `Efeito` já é o pedido do núcleo à raiz. | Evita a colisão com `Efeito` em `Efeitos.cs`. | `Efeito`, `EfeitoDoItem`. |
| D17 | **No máximo 6 itens.** O sétimo tira o mais antigo que não está na mão do usuário. O menu ganha "Recolher itens". Os itens somem ao sair do app. | O item espera indefinidamente, então precisa de limite. | Recusar o 7º (o usuário clicou de propósito). |
| D18 | **O item não tem janela visível** com o personagem escondido nem num monitor ocupado pela tela cheia (modo ligado). Ao esconder, os itens que estavam caindo vão direto ao chão. | A Q-09 pede não cobrir um app em tela cheia, e em `HIDDEN` não há relógio. | Os itens acompanharem o personagem entre monitores. |

---

## 2. Tipos e arquivos novos

### 2.1 `src/Buzzy.Core/Personagem/Tamagotchi.cs` (novo)

```csharp
public enum Item { Banana, Agua, Vodka, Cerveja, Baseado, Cigarro, Cocaina, Md, LancaPerfume, Cafe, Energetico, Cogumelo, Bala } // ordem do menu
public enum VerboDeUso { Comer, Beber, Fumar, Cheirar, Engolir, Inalar }
public enum Onda { Satisfeito, Alegre, Relaxado, Ligado, Bebado, Chapado, Eletrico, Euforico, Tonto, Viajando }
public enum FaseDaOnda { Subida, Pico, Queda }
public enum ApoioDoUso { Chao, Parede, Cipo, Esconderijo }
public enum SituacaoDoItem { Caindo, NoChao, Segurado, Arrastado }
public enum MotivoDaRemocao { Usado, Recolhido, Substituido }

/// Intensidade: níveis que o item soma à onda (1 ou 2; 0 na água). Onda nula só na água.
public sealed record DadosDoItem(Item Item, VerboDeUso Verbo, int PassosDoUso, Expressao CaraDurante, Onda? Onda, int Intensidade);

/// Percentuais sobre o perfil de energia (100 = igual). ChanceDoFoguete nula = a do perfil.
public sealed record PerfilDaOnda(int Intervalo, int Descanso, int Andar, int Escalar, int Pular, int Descansar,
    int Gesto, int TrocarCara, int Velocidade, int Cambaleio, int? ChanceDoFoguete, int AlturaDoPulo,
    IReadOnlyList<(Gesto Gesto, int Peso)> Gestos, IReadOnlyList<(Expressao Cara, int Peso)> Caras);

public sealed record DadosDaOnda(Onda Onda, int Precedencia, TimeSpan Subida, TimeSpan NivelDoPico, TimeSpan QuedaBase,
    Expressao CaraDaSubida, Expressao CaraDoPico, Expressao? CaraDaQueda,
    IReadOnlyList<PerfilDaOnda> PicoPorNivel, PerfilDaOnda? Queda)
{
    public PerfilDaOnda Perfil(FaseDaOnda fase, int nivel);   // Subida = PicoPorNivel[0] com Caras = [(CaraDaSubida, 1)]
    public TimeSpan Duracao(FaseDaOnda fase, int pior);        // Queda × 100/125/150 % pelo pior nível; mínimo 1 s
    public Expressao Cara(FaseDaOnda fase);                    // Queda sem cara própria → CaraDoPico
}

public sealed record EstadoDaOnda(Onda Tipo, FaseDaOnda Fase, int Nivel, int Pior); // Nivel 1..3; Pior = maior nível atingido
public sealed record Uso(Item Item, VerboDeUso Verbo, int Passos, ApoioDoUso Apoio);  // o restante fica em PassosRestantes

public sealed record ItemNoMundo(int Id, Item Item, SituacaoDoItem Situacao, Posicionamento Lugar, PosicaoDoPersonagem Posicao)
{
    public double Y { get; init; }  public double VY { get; init; }  public int Quiques { get; init; }  public PontoPx Pegada { get; init; }
}

/// Imutável, ordenado por Id, igualdade por valor (como MonitoresOcupados).
public sealed class ItensNoMundo : IEquatable<ItensNoMundo>
{
    public static readonly ItensNoMundo Nenhum;
    public IReadOnlyList<ItemNoMundo> Todos { get; }
    public int Quantidade { get; }
    public ItemNoMundo? PorId(int id);
    public ItemNoMundo? NaMao { get; }        // Segurado ou Arrastado (no máximo um)
    public bool AlgumCaindo { get; }
    public ItensNoMundo Com(ItemNoMundo item); // acrescenta ou substitui pelo Id
    public ItensNoMundo Sem(int id);
    public override string ToString();         // "1:Banana:NoChao:(1728,1032);2:…"
}
```

### 2.2 `src/Buzzy.Core/Personagem/TabelaDoTamagotchi.cs` (novo, só dados)

```csharp
public static class TabelaDoTamagotchi
{
    public static IReadOnlyList<Item> Itens { get; }   // Enum.GetValues<Item>(), ordem do menu
    public static DadosDoItem DoItem(Item item);       // seção 4.1; ArgumentOutOfRangeException fora do enum
    public static DadosDaOnda DaOnda(Onda onda);        // seções 4.2 a 4.4
}
```

### 2.3 `Maquina` dividida em arquivos parciais

`public static partial class Maquina` e `private sealed partial class Passo`, em três arquivos:

- **`Maquina.cs`:** o que já existe.
- **`Maquina.Itens.cs` (novo):**
  - `InvocarItem(Item)`, `RecolherItens()`;
  - `PegarItem(int, PontoPx)`, `IniciarArrasteDoItem(int)`, `ArrastarItem(int, PontoPx)`, `SoltarItem(int, PontoPx)`, `LargarItem(int)`;
  - `FicarAtento()`, `UsarItem(ItemNoMundo)`, `FimDoUso()`;
  - `PassoDosItens()`, `ReacomodarItens(Topologia)`, `AssentarItens()`, `LiberarItemNaMao()`;
  - `LugarDeNascimento()`, `EfeitosDosItens(List<Efeito>)`.
- **`Maquina.Onda.cs` (novo):**
  - `AplicarNaOnda(DadosDoItem)`, `Refrescar()`, `AvancarOnda(long)`, `IniciarFase(EstadoDaOnda)`, `FimDaFrente()`;
  - `CaraDeBase()`, `VoltarACaraDeBase()`, `EscolherEmocao(Expressao?)`;
  - `SortearCara(...)` e `EfeitosDaOnda(List<Efeito>)`.

Estáticos públicos, que os testes usam como oráculo:

```csharp
public static bool AceitaItem(Estado estado);                                      // tabela 4.6
public static bool SobreOPersonagem(RetanguloPx item, RetanguloPx personagem, int margemPercentual);
public static PerfilDeEnergia PerfilEfetivo(EstadoDoNucleo s, ConfiguracaoDoNucleo cfg);
public static ParametrosDeMovimento FisicaEfetiva(EstadoDoNucleo s, ConfiguracaoDoNucleo cfg);
public static double FatorDoCambaleio(long passo, int amplitudePercentual);
public static bool ItemVisivel(EstadoDoNucleo s, ItemNoMundo item);
```

### 2.4 Contrato com as outras áreas (app, menu e arte)

- **Eventos que o app envia:**
  - menu: `CmdSummonItem(Item)`, `CmdClearItems`, `CmdSetDominantEmotion(Expressao?)`;
  - gestos de uma janela de item (um `ArbitroDeGestos` próprio): Press → `ItemPress(id, p)`, DragStart → `ItemDragStart(id)`, DragMove → `ItemDragMove(id, p)`, DragEnd → `ItemDragEnd(id, p)`, e Click, DoubleClick ou DragCancel → `ItemRelease(id)`;
  - botão direito no item: o `ContextMenu(p)` de hoje;
  - temporizador da onda disparado: `ItemEffectTimer(g)`.
- **Efeitos que o app executa:** `MostrarItem`, `MoverItem` (só o último por item num lote), `EsconderItem`, `RemoverItem`, `LiberarCapturaDoItem`, `AgendarOnda` (um segundo `DispatcherTimer` de disparo único) e `CancelarOnda`. `EncerrarAplicacao` fecha as janelas dos itens e para esse temporizador.
- **Retrato, para a pose:**
  - `Uso` e `PassoDoUso`, para o verbo, o quadro e o apoio;
  - `Onda` e `OndaDeFundo`;
  - `EmocaoDominante`, para a marca no submenu;
  - `Itens`, que inclui o item na mão;
  - o balanço do cambaleio sai de `EstadoDoNucleo.Passos`, que o app já lê.
- **Listas únicas do menu:** `Expressoes.DeHumor` (14, na ordem de `expressoes.png`) e `TabelaDoTamagotchi.Itens`.
- **Nomes na arte:** `NomeDaExpressao` gera `bebado`, `enjoado`, `chapado`, `eletrico`, `apaixonado`, `tonto` e `viajando`. Os gestos novos pedem as poses `soluco`, `danca`, `gargalhada`, `espirro`, `tosse` e `tremedeira`.

---

## 3. Mudanças em arquivos existentes (arquivo → função → o quê)

### 3.1 `Tipos.cs`

- **`Estado`:** acrescentar `Using` no fim, com o comentário da tabela 2.6.
- **`Estados.Grupo`:** `Using` fica com `Pressed`, `Dragging`, `Settling` e `Reacting`, no grupo **Usuario**.
- **`Estados.AceitaPressionar`:** passa a aceitar `... || estado is Estado.Reacting or Estado.Using`.
- **`Expressao`:** acrescentar, no fim e nesta ordem, `Bebado, Enjoado, Chapado, Eletrico, Apaixonado, Tonto, Viajando` (valores 14–20).
- **`Gesto`:** acrescentar no fim `Soluco, Danca, Gargalhada, Espirro, Tosse, Tremedeira`.
- **Classe nova `Expressoes`:**
  - `DeHumor` (Neutro…Determinado, 14) e `DeEfeito` (7);
  - `EhDeHumor(e) => e is >= Neutro and <= Determinado`;
  - `Companheiras(dominante)` (seção 4.7).

### 3.2 `Eventos.cs`

Records novos:

| Record | `Origem` |
|---|---|
| `CmdSummonItem(Item Item)`, `CmdClearItems`, `CmdSetDominantEmotion(Expressao? Emocao)` | `ComandoDoUsuario` |
| `ItemPress(int Id, PontoPx Cursor)`, `ItemDragStart(int Id)`, `ItemDragMove(int Id, PontoPx Cursor)`, `ItemDragEnd(int Id, PontoPx Cursor)`, `ItemRelease(int Id)` | `AcaoDireta` |
| `ItemEffectTimer(long Geracao)` | `Relogio`, para **não** ser descartado com o usuário no controle nem encerrar gestos |

`Preferencias` ganha `public Expressao? EmocaoDominante { get; init; }`, **fora** do construtor posicional. Assim não conflita com `AtravessarMonitores`, que a Fase 5 deixou posicional (C6). `Padrao` continua igual.

### 3.3 `Efeitos.cs`

- `AgendarOnda(TimeSpan Atraso, long Geracao)` e `CancelarOnda`.
- `MostrarItem(int Id, Item Item, Posicionamento Lugar)`, `MoverItem(int Id, Posicionamento Lugar)`, `EsconderItem(int Id)`, `RemoverItem(int Id, MotivoDaRemocao Motivo)` e `LiberarCapturaDoItem(int Id)`.

### 3.4 `Configuracao.cs`

**`ConfiguracaoDoNucleo`** ganha:

- `bool Tamagotchi`;
- `TamanhoDip TamanhoDoItem = new(48, 48)` (24 × 24 px de arte);
- `int MaximoDeItens = 6`;
- `int MargemDoAlvo = 20`;
- `Func<Item, DadosDoItem> TabelaDeItens = TabelaDoTamagotchi.DoItem`;
- `Func<Onda, DadosDaOnda> TabelaDeOndas = TabelaDoTamagotchi.DaOnda`, que os testes trocam por durações curtas.

`DoAplicativo` recebe `Tamagotchi = true` só na entrega do app e da arte (D15).

### 3.5 `Movimento.cs`

**`ParametrosDeMovimento`** ganha:

- `AlturaDaQuedaDoItem = 140` DIP;
- `FolgaDoItem = 8` DIP;
- `RestituicaoDoItem = 0.35`;
- `ImpactoMinimoDoItem = 300` DIP/s;
- `QuiquesDoItem = 1`.

### 3.6 `EstadoDoNucleo.cs`

- **Campos novos:** `EstadoDaOnda? Onda`, `EstadoDaOnda? OndaDeFundo`, `long GeracaoDaOnda`, `bool OndaAgendada`, `Uso? Uso`, `ItensNoMundo Itens = Nenhum`, `int ProximoIdDeItem = 1` e o derivado `bool Atento => Itens.NaMao is not null`. O comentário de `PassosRestantes` passa a incluir `USING`.
- **`Retrato`:** ganha propriedades `init` fora da lista posicional: `Onda`, `OndaDeFundo`, `Uso`, `PassoDoUso` (`Uso.Passos − PassosRestantes`), `EmocaoDominante` e `Itens`. `EstadoDoNucleo.Retrato()` as preenche.
- **`Retrato.Descrever()`:** acrescenta cada trecho **só quando há valor**, e as referências 01–05 não mudam. O formato é:

  ```
  onda=Bebado/Pico/2
  fundo=Tonto/Subida/2
  uso=Vodka/Beber/37de150/Chao
  emocao=Feliz
  itens=[1:Banana:NoChao:(1728,1032)]
  ```

### 3.7 `Maquina.cs`, função a função

**`Tratar`:** casos novos.

- `CmdSummonItem` → `InvocarItem`.
- `CmdClearItems` → `RecolherItens`.
- `CmdSetDominantEmotion` → `EscolherEmocao`.
- `ItemPress`, `ItemDragStart`, `ItemDragMove`, `ItemDragEnd`, `ItemRelease` → os métodos de item.
- `ItemEffectTimer` → `AvancarOnda`.

Com `!_cfg.Tamagotchi`, os eventos de item e de onda são ignorados. O fim do gesto por prioridade já vale sozinho, porque `AcaoDireta` e `ComandoDoUsuario` são ≥ `Sistema`.

**`Perfil` (propriedade):** passa a ser `Maquina.PerfilEfetivo(_s, _cfg)`. Isso aplica a onda sozinho a `DecidirParado`, `Sinalizar`, `Decidir`, `TalvezFoguete`, `PlanejarCaminhada`, `PlanejarPulo` e `SortearAtraso`.

**Velocidades:** trocar `_cfg.Fisica.Velocidade*` por `Maquina.FisicaEfetiva(_s, _cfg).Velocidade*` em cinco lugares:

- `PassoAndando` (`VelocidadeAndando`);
- `PassoEscalando` e `PassoPresoNaParede` (`VelocidadeEscalando`);
- `PassoPresoNoCipo` e `PassoPendurado` (`VelocidadePendurado`).

**`PassoAndando`:**

```csharp
passo *= FatorDoCambaleio(_s.Passos, perfilDaOnda.Cambaleio);
// só com cambaleio ≠ 0:
x = Math.Clamp(x, sup.Esquerda, sup.Direita);
// Restante -= passo, com o sinal: o recuo devolve distância
```

**`Passar`:**

- depois de `Passos + 1`, e antes do `switch`, chamar `PassoDosItens()`;
- caso novo `Estado.Using`: `PassosRestantes--`, e com `<= 0`, `FimDoUso()` (`VoltarACaraDeBase()` e depois `Acomodar(Lugar.Ancora, $"USING: fim do uso de {item}")`);
- em `Landing`, no fim: `VoltarACaraDeBase()` antes de `IrPara(Idle)`;
- em `Reacting`, no fim: `VoltarACaraDeBase()` antes de `Acomodar`.

**`Decidir`:**

- adicionar `|| _s.Atento` à guarda.
- `Resting`: trocar `Expressao = Expressao.Neutro` por `Expressao = CaraDeBase()`. Na automática, sem onda, o resultado é o mesmo `Neutro`.
- `Peeking`: com onda ou dominante, **um** `Ponderado` no conjunto da seção 4.7 (ou nas caras da fase) substitui o `Entre`. A automática não muda.

**`DecidirParado`:**

- `TrocarExpressao` na automática: `Entre(0, Expressoes.DeHumor.Count − 2)`, depois `DeHumor[e >= iAtual ? e + 1 : e]` (com `iAtual = −1`, fica `DeHumor[e]`). Para as 14 caras, é idêntico ao código de hoje.
- Com onda: `Ponderado` das caras da fase. Com dominante: `Ponderado([6, 1, 1, 1, 1])`.
- `Gesto` com onda: `Ponderado` dos gestos da fase e depois `Entre(PassosDoGestoMinimo, Maximo)`. Sem onda, o de hoje.

**`DecidirPreso`:** o sorteio `cara` (`a2.Entre(0, ExpressoesDoPreso.Length − 1)`) vira um `Ponderado` quando há onda ou dominante. O número de sorteios não muda.

**`IrPara`:** ao sair de `Using` (`de == Using && novo != Using`), fazer `Uso = null`.

**`Pressionar`:** nada muda; o `AceitaPressionar` novo já cobre `USING`.

**`MudarTopologia`:**

- `revalida` inclui `Estado.Using`;
- `ReacomodarItens(nova)` roda sempre, em qualquer estado, **antes** do retorno de `mesma`: se a configuração é a mesma, não faz nada.

**`Carregar`:** depois de `Preferencias = Sanear(...)`, `if (EmocaoDominante is {} d) Expressao = d`.

**`Sanear`:** também zera a emoção que não é de humor: `EmocaoDominante is {} e && !EhDeHumor(e) → null`.

**`MudarPreferencias`:** se a emoção mudou e é não nula, não há onda e `CaraLivre(Estado)`, então `Expressao = nova`. Não emite `GravarPreferencias`, como hoje.

**`Esconder`, `Sair` e o ramo "nenhum monitor livre" de `SairDoMonitorOcupado`:** `LiberarItemNaMao()` (efeito `LiberarCapturaDoItem` em `_antes`; o item fica no chão, na coluna em que estava) e `AssentarItens()`.

**`Concluir`:**

- relógio: `relogio = (EmMovimento && !agarrado) || Reacting || Using || (Idle && Gesto) || (Visivel && Itens.AlgumCaindo)`;
- agenda: `querDecisao = ... && !_s.Atento`;
- depois do bloco da agenda, `EfeitosDaOnda(tempo)`;
- depois dos efeitos da janela do personagem, `EfeitosDosItens(janela)`.

A ordem final dos efeitos fica: `_antes` → janela do personagem → janelas dos itens (removidos e depois por Id) → relógio → agenda → onda → `_depois`.

### 3.8 `Gravacao.cs`

**`Escrever` e `Ler`:**

```
CmdSummonItem item=Banana
CmdClearItems
CmdSetDominantEmotion emocao=Feliz|Automatica
ItemPress id=1 x= y=
ItemDragStart id=1
ItemDragMove id=1 x= y=
ItemDragEnd id=1 x= y=
ItemRelease id=1
ItemEffectTimer geracao=N
```

Sem `geracao`, `ItemEffectTimer` usa `atual.GeracaoDaOnda`, como `AutonomyTimer`.

**`DescreverEfeito`:**

```
AgendarOnda atrasoMs= geracao=
CancelarOnda
MostrarItem id= item= monitor= ancora=(x,y) retangulo=…
MoverItem id= ancora=(x,y)
EsconderItem id=
RemoverItem id= motivo=
LiberarCapturaDoItem id=
```

**`DescreverPreferencias` e `LerPreferencias`:** ` emocao=Feliz` só quando há emoção; a ausência vale nula (padrão). `Ler` aceita qualquer nome de `Expressao`, para os testes de saneamento.

### 3.9 `Persistencia/EsquemaDeConfiguracoes.cs`

- `VersaoAtual = 2`.
- `CamposDasPreferencias` ganha `"emocaoDominante"` no fim.
- `Escrever` sempre grava `"emocaoDominante": "automatica"` ou o nome em minúsculas ASCII.
- `LerPreferencias` usa `TentarLerEmocao(string, out Expressao?)`, que compara com os 14 nomes sem diferenciar maiúsculas (nunca `Enum.Parse`):
  - `"automatica"`, `null` ou ausente → nula, sem aviso;
  - qualquer outro valor → nula, com o aviso `"preferencias.emocaoDominante: não é uma das expressões; vale automatica"`.
- `NormalizarPreferencias` zera a emoção que não é de humor.
- Funções públicas novas: `NomeDaEmocao(Expressao?)` e `TentarLerEmocao`.

### 3.10 Documentos

**ARCHITECTURE 2.6.** Estado novo:

| Estado | Grupo | Significado |
|---|---|---|
| `USING` | usuário | Usa o item que o usuário soltou sobre ele (comer, beber, fumar, cheirar, engolir, inalar), por um número fixo de passos, no apoio em que estava: chão, parede, cipó ou esconderijo. O relógio corre (DEC-028). |

Dimensões novas:

| Dimensão | Valores | Efeito |
|---|---|---|
| Onda do item | nenhuma; ou a da frente (tipo, fase, nível 1–3) e até uma de fundo, congelada | Pesos, intervalos, gestos e caras. Por exceção ao invariante 12, também as velocidades de andar, escalar e pendurar e o cambaleio. Um disparo único até a próxima fase. |
| Emoção dominante | automática ou uma das 14 expressões | Cara de base e a mais sorteada. Nenhum efeito sobre estado, posição ou física. |
| Itens no mundo | até 6, cada um caindo, no chão, segurado ou arrastado | Enquanto o usuário segura um item, o personagem fica atento: para no lugar e a agenda pausa. |

Mudanças nas linhas existentes:

- **Expressão:** "inclusive as caras de efeito".
- **Gesto curto:** acrescentar os seis gestos novos.
- **Autonomia pausada:** "a onda continua só nas caras".

Eventos novos:

| Origem | Eventos |
|---|---|
| Menu | `CMD_SUMMON_ITEM(item)`, `CMD_CLEAR_ITEMS`, `CMD_SET_DOMINANT_EMOTION(expressão ou automática)` |
| Ponteiro sobre a janela de um item | `ITEM_PRESS(id,p)`, `ITEM_DRAG_START(id)`, `ITEM_DRAG_MOVE(id,p)`, `ITEM_DRAG_END(id,p)`, `ITEM_RELEASE(id)` |
| Relógio | `ITEM_EFFECT_TIMER(geração)` |

Transições novas:

| De | Evento | Para | Regra |
|---|---|---|---|
| qualquer visível e carregado | `CMD_SUMMON_ITEM(item)` | permanece | O item aparece acima do chão, ao lado dele, e cai. Com 6 itens, o mais antigo fora da mão some. Escondido, o pedido é ignorado. |
| qualquer visível | `ITEM_PRESS(id)` | `WALKING` e `RESTING` → `IDLE`; os outros permanecem | Atento: na parede e no cipó fica agarrado; pulo e queda terminam; nenhuma decisão autônoma até o item ser solto. |
| `IDLE`, `WALKING`, `CLIMBING`, `HANGING`, `RESTING`, `REACTING`, `LANDING`, `PEEKING` | `ITEM_DRAG_END(id)` com o item sobre o personagem | `USING` | O item some e a onda começa (seção 2.16). |
| `JUMPING`, `FALLING`, `USING`, `PRESSED`, `DRAGGING` | idem | permanece | Recusado: o item cai de onde foi solto. |
| qualquer | `ITEM_DRAG_END` fora do personagem, ou `ITEM_RELEASE` | permanece | O item cai de onde foi solto; a agenda volta depois do intervalo de acomodação. |
| `USING` | fim dos passos do uso | `SETTLING`, depois `IDLE`, `CLIMBING` agarrado, `HANGING` agarrado ou `PEEKING` | Volta ao mesmo apoio. Quem estava preso continua preso; quem estava escondido continua escondido. |
| `USING` | `PRESS` | `PRESSED` | O uso acaba na hora; a onda continua. |
| `USING` | `TOPOLOGY_CHANGED`, tela cheia, `CMD_HIDE`, sessão, suspensão, `CMD_EXIT` | conforme a linha de cada evento | O uso acaba; a onda continua. |
| qualquer | `ITEM_EFFECT_TIMER` da geração agendada | permanece | A onda avança de fase ou de nível; quando acaba, a de fundo volta. A cara da fase entra na hora, exceto em `RESTING`, `REACTING` e `USING`. |
| qualquer carregado, exceto `EXITING` | `CMD_SET_DOMINANT_EMOTION(e)` | permanece | Grava a preferência. Sem onda, a cara muda na hora. Um valor fora das 14 é ignorado. |
| qualquer | `CMD_CLEAR_ITEMS` | permanece | Todos os itens somem. |

**Invariantes:**

- o 1 passa a valer também em `USING`;
- o 12 ganha "exceto a onda de um item (invariante 26)";
- o 15 vale também para os gestos da onda;
- os novos são o 22 ao 29 (seção 4.11).

**Outros documentos:**

- ARCHITECTURE: seção nova 2.16 com as tabelas 4.1–4.10.
- DEC-027 e DEC-028: detalhar com D1–D18.
- SECURITY 5: guarda-se só a emoção dominante; a onda, os itens e o uso ficam só em memória.
- SECURITY 7: a emoção vem de uma lista fechada de 14 nomes.
- IDENTIDADE_VISUAL 6 e 7: outra área.

---

## 4. Regras exatas

### 4.1 Itens

| Item | Verbo | Passos do uso | Cara durante | Onda | Intensidade | Cara ao terminar o uso | Duração sozinho |
|---|---|---|---|---|---|---|---|
| Banana | Comer | 180 | Feliz | Satisfeito | 1 | Feliz | 63 s |
| Agua | Beber | 90 | Feliz | nenhuma (refresco) | 0 | base | — |
| Vodka | Beber | 150 | Determinado | Bebado | 2 | Feliz | 5 min 20 s |
| Cerveja | Beber | 120 | Feliz | Bebado | 1 | Feliz | 3 min 18 s |
| Baseado | Fumar | 210 | Pensativo | Chapado | 2 | Pensativo | 5 min 42 s |
| Cigarro | Fumar | 180 | Pensativo | Relaxado | 1 | Pensativo | 63 s |
| Cocaina | Cheirar | 90 | Surpreso | Eletrico | 2 | Surpreso | 4 min 25 s |
| Md | Engolir | 60 | Travesso | Euforico | 2 | Feliz | 6 min 25 s |
| LancaPerfume | Inalar | 90 | Surpreso | Tonto | 2 | Tonto (a subida de 1 s acaba durante o uso) | 43,5 s |
| Cafe | Beber | 120 | Determinado | Ligado | 1 | Surpreso | 2 min 5 s |
| Energetico | Beber | 120 | Empolgado | Ligado | 2 | Surpreso | 3 min 31 s |
| Cogumelo | Comer | 120 | Curioso | Viajando | 2 | Curioso | 6 min 15 s |
| Bala | Comer | 120 | Feliz | Alegre | 1 | Empolgado | 62 s |

A "cara ao terminar o uso" é `CaraDeBase()` no fim do uso, isto é, a cara da fase em que a onda estiver.

### 4.2 Ondas: precedência, fases e caras

| Onda | Precedência | Subida | Cada nível do pico | Queda base | Cara na subida | Cara no pico | Cara na queda |
|---|---|---|---|---|---|---|---|
| Satisfeito | 1 | 3 s | 60 s | — | Feliz | Feliz | — |
| Alegre | 1 | 2 s | 40 s | 20 s | Empolgado | Empolgado | Entediado |
| Relaxado | 1 | 3 s | 60 s | — | Pensativo | Pensativo | — |
| Ligado | 2 | 5 s | 75 s | 45 s | Surpreso | Determinado | Sonolento |
| Bebado | 3 | 8 s | 100 s | 90 s | Feliz | **Bebado** | **Enjoado** |
| Chapado | 3 | 10 s | 110 s | 90 s | Pensativo | **Chapado** | Sonolento |
| Eletrico | 3 | 3 s | 75 s | 90 s | Surpreso | **Eletrico** | Entediado |
| Euforico | 3 | 15 s | 110 s | 120 s | Feliz | **Apaixonado** | Entediado |
| Tonto | 3 | 1 s | 15 s | 10 s | Surpreso | **Tonto** | Sonolento |
| Viajando | 3 | 20 s | 140 s | 60 s | Curioso | **Viajando** | Pensativo |

Regras de tempo:

- **Pico:** a cada disparo, se o nível passa de 1, cai um nível e o temporizador recomeça; no nível 1, vai para a queda. Sem queda, a onda acaba.
- **Queda:** base × 100%, 125% ou 150%, conforme o pior nível atingido (1, 2 ou 3).
- **Disparos por episódio:** 1 + nível + (1 se houver queda). A vodka sozinha dá 4. O maior episódio possível é o do Viajando no nível 3: 20 + 420 + 90 = 530 s.

### 4.3 Perfis da onda (percentuais; "a/b/c" = níveis 1, 2 e 3 do pico; a subida usa o nível 1)

| Onda | Intervalo | Descanso | Andar | Escalar | Pular | Descansar | Gesto | Troca de cara | Velocidade | Cambaleio | Foguete | Altura do pulo |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Satisfeito | 100 | 100 | 100 | 100 | 100 | 150 | 150 | 150 | 100 | 0 | perfil | 100 |
| Alegre | 60/50/40 | 60 | 130 | 130 | 200 | 30 | 150 | 100 | 120/125/130 | 0 | perfil | 120 |
| Relaxado | 130 | 120 | 70 | 50 | 30 | 150 | 120 | 100 | 90 | 0 | perfil | 100 |
| Ligado | 70/55/40 | 60/45/30 | 150 | 150 | 150/200/250 | 30/15/5 | 120 | 100 | 115/130/145 | 0 | 40/50/60 | 110/125/140 |
| Bebado | 100 | 120 | 130 | 40 | 40 | 120 | 200 | 150 | 80/70/60 | 60/90/120 | 5 | 80 |
| Chapado | 160 | 150 | 60 | 30 | 20 | 200 | 150 | 120 | 60/55/50 | 0 | 0 | 80 |
| Eletrico | 35/28/20 | 30/20/10 | 200 | 200 | 180 | 10/5/5 | 150 | 200 | 170/185/200 | 0 | 60/70/80 | 120/130/140 |
| Euforico | 60 | 50 | 120 | 100 | 150 | 30 | 250 | 150 | 120 | 0 | perfil | 120 |
| Tonto | 50 | 100 | 50 | 0 | 0 | 100 | 300 | 200 | 60 | 100 | 0 | 100 |
| Viajando | 130 | 120 | 80 | 80 | 60 | 100 | 200 | 250 | 70 | 0 | 20 | 100 |

Na queda, o perfil não depende do nível. A altura do pulo é sempre 100 e o foguete é o do perfil, salvo onde indicado.

| Queda de | Intervalo | Descanso | Andar | Escalar | Pular | Descansar | Gesto | Troca de cara | Velocidade | Cambaleio |
|---|---|---|---|---|---|---|---|---|---|---|
| Alegre | 120 | 150 | 80 | 60 | 50 | 200 | 80 | 100 | 90 | 0 |
| Ligado | 130 | 150 | 70 | 50 | 40 | 200 | 80 | 100 | 85 | 0 |
| Bebado (ressaca; foguete 5) | 150 | 200 | 60 | 20 | 10 | 250 | 80 | 80 | 75 | 30 |
| Chapado (foguete 0) | 150 | 200 | 60 | 30 | 20 | 300 | 80 | 100 | 70 | 0 |
| Eletrico | 150 | 180 | 60 | 40 | 30 | 250 | 80 | 100 | 80 | 0 |
| Euforico | 140 | 150 | 70 | 60 | 50 | 200 | 80 | 100 | 85 | 0 |
| Tonto | 100 | 100 | 80 | 50 | 50 | 120 | 80 | 100 | 80 | 0 |
| Viajando | 120 | 120 | 80 | 70 | 60 | 150 | 100 | 150 | 85 | 0 |

**`PerfilEfetivo`:** sem onda, devolve a mesma instância do perfil de energia. Com onda, devolve o perfil com os campos abaixo trocados:

```csharp
Pct(TimeSpan t, int p) = TimeSpan.FromMilliseconds((long)t.TotalMilliseconds * p / 100)
Peso(int w, int p)     = w <= 0 || p <= 0 ? 0 : Math.Max(1, (w * p + 50) / 100)
Dip(int d, int p)      = Math.Max(1, (d * p + 50) / 100)
```

| Campo do perfil | Novo valor |
|---|---|
| `DecisaoMinima`, `DecisaoMaxima` | `Pct(…, Intervalo)` |
| `DescansoMinimo`, `DescansoMaximo` | `Pct(…, Descanso)` |
| `PesoAndar` | `Peso(…, Andar)` |
| `PesoEscalar` | `Peso(…, Escalar)` |
| `PesoPular` | `Peso(…, Pular)` |
| `PesoDescansar` | `Peso(…, Descansar)` |
| `PesoGesto` | `Peso(…, Gesto)` |
| `PesoTrocarExpressao` | `Peso(…, TrocarCara)` |
| `AlturaDoPuloMinima`, `AlturaDoPuloMaxima` | `Dip(…, AlturaDoPulo)` |
| `ChanceDoFoguete` | `ChanceDoFoguete ?? perfil` |
| `PesoAtravessar` (Fase 5, se existir) | `Peso(…, Andar)` |

Os tempos na parede e pendurado não mudam. O piso de 3 s (intervalo de acomodação) continua valendo em `SortearAtraso`.

**`FisicaEfetiva`:** `VelocidadeAndando`, `VelocidadeEscalando` e `VelocidadePendurado` são multiplicadas por `Velocidade / 100.0`. Todo o resto é idêntico.

### 4.4 Gestos e caras por fase (pesos)

| Onda | Gestos no pico | Caras no pico | Gestos na queda | Caras na queda |
|---|---|---|---|---|
| Satisfeito | Cocar 2, Espreguicar 2, Brincar 1 | Feliz 4, Rindo 1, Travesso 1, Sonolento 1 | — | — |
| Alegre | Brincar 2, Danca 2, Gargalhada 1 | Empolgado 3, Rindo 2, Feliz 2 | Espreguicar 1 | Entediado 2, Sonolento 2 |
| Relaxado | Espreguicar 2, Tosse 1, OlharAoRedor 1 | Pensativo 2, Neutro 2, Sonolento 1, Feliz 1 | — | — |
| Ligado | Tremedeira 2, OlharAoRedor 2, Brincar 1 | Determinado 2, Empolgado 2, Surpreso 1 | Espreguicar 2, Cocar 1 | Sonolento 3, Bocejando 1 |
| Bebado | Soluco 3, Danca 1, Gargalhada 1 | Bebado 4, Rindo 2, Feliz 1, Sonolento 1 | Soluco 1, Espreguicar 1 | Enjoado 3, Sonolento 2, Entediado 1 |
| Chapado | Gargalhada 3, OlharAoRedor 1, Cocar 1 | Chapado 4, Rindo 2, Pensativo 1, Sonolento 1 | Espreguicar 2 | Sonolento 3, Bocejando 2, Pensativo 1 |
| Eletrico | Tremedeira 3, Espirro 1, OlharAoRedor 1 | Eletrico 4, Determinado 1, Surpreso 1, Empolgado 1 | Espreguicar 1, Cocar 1 | Entediado 3, Sonolento 2, Pensativo 1 |
| Euforico | Danca 4, Brincar 1 | Apaixonado 3, Empolgado 2, Feliz 1, Rindo 1 | Espreguicar 1, OlharAoRedor 1 | Entediado 2, Pensativo 2, Sonolento 1 |
| Tonto | Gargalhada 2, OlharAoRedor 1 | Tonto 4, Rindo 2 | OlharAoRedor 1 | Sonolento 1, Surpreso 1, Neutro 1 |
| Viajando | OlharAoRedor 2, Danca 1, Espiar 1 | Viajando 4, Surpreso 1, Pensativo 1, Curioso 1, Rindo 1 | OlharAoRedor 2, Espiar 1 | Pensativo 3, Curioso 1, Sonolento 1 |

Na subida, as caras são só `[(CaraDaSubida, 1)]` e os gestos são os do pico.

### 4.5 Combinação (`AplicarNaOnda`)

```csharp
if (d.Onda is null) { Refrescar(); return; }                     // água
var (f, b) = (_s.Onda, _s.OndaDeFundo);
if (f is null)            IniciarFase(new(tipo, Subida, d.Intensidade, d.Intensidade));
else if (f.Tipo == tipo)  { int n = Math.Min(3, f.Nivel + d.Intensidade);
                            IniciarFase(f with { Nivel = n, Pior = Math.Max(f.Pior, n),
                                                 Fase = f.Fase == Subida ? Subida : Pico }); } // reinicia a fase
else if (b?.Tipo == tipo) _s = _s with { OndaDeFundo = b with { Nivel = Math.Min(3, b.Nivel + d.Intensidade), Pior = … } };
else if (Precedencia(tipo) >= Precedencia(f.Tipo))
                          { _s = _s with { OndaDeFundo = f };      // a de fundo anterior é descartada
                            IniciarFase(new(tipo, Subida, d.Intensidade, d.Intensidade)); }
// senão: absorvido — a onda e o temporizador não mudam
```

- **`IniciarFase(o)`:** `Onda = o`, `_reagendarOnda = true`, e, se `CaraLivre`, `Expressao = dados.Cara(o.Fase)`.
- **`FimDaFrente()`:**
  - com onda de fundo: `Onda = fundo`, `OndaDeFundo = null`, a fase dela recomeça com a duração cheia, e a cara é a da fase dela;
  - sem onda de fundo: `Onda = null`, e se `CaraLivre`, `Expressao = EmocaoDominante ?? Neutro`.
- **`AvancarOnda(g)`:**
  - ignora o disparo se `!OndaAgendada || g != GeracaoDaOnda || Onda is null`;
  - senão, faz `OndaAgendada = false` e avança: subida → pico; pico com nível > 1 → nível − 1; pico no nível 1 → queda (nível 1) ou fim; queda → fim;
  - registra `Transicao(e, e, "ITEM_EFFECT_TIMER: onda …")`;
  - **não** reagenda a decisão autônoma.
- **`Refrescar()` (água):**
  - queda → `FimDaFrente()`;
  - subida no nível 1 → `FimDaFrente()`;
  - pico no nível 1 → queda, ou fim se não houver queda;
  - nos demais casos, nível − 1, sem mexer no temporizador;
  - sem onda, nada.
- **`EfeitosDaOnda`:**
  - com onda e fora de `EXITING`, se `_reagendarOnda || !OndaAgendada`: `AgendarOnda(Max(1 s, Duracao(fase, pior)), ++GeracaoDaOnda)`;
  - sem onda (ou em `EXITING`) e com `OndaAgendada`: `CancelarOnda`.
- **`CaraLivre(e)`:** `e is not (Resting or Reacting or Using)`.

### 4.6 Aceitação do soltar sobre o personagem

| Estado ao soltar | Resultado | Apoio do uso | Depois do uso |
|---|---|---|---|
| `IDLE` (o gesto termina) | `USING` | `Chao` | `IDLE` |
| `WALKING` | `USING`; o plano é descartado | `Chao` | `IDLE` |
| `CLIMBING`, preso ou não | `USING` | `Parede` | `CLIMBING` agarrado, preso se já estava |
| `HANGING`, preso ou não | `USING` | `Cipo` | `HANGING` agarrado, preso se já estava |
| `PEEKING` | `USING` | `Esconderijo` | `PEEKING` na mesma borda |
| `RESTING` | `Sinal.Acordou`, depois `USING` | `Chao` | `IDLE` |
| `REACTING`, `LANDING` | `USING`; a reação ou o pouso é cortado | pela geometria | pela acomodação |
| `JUMPING`, `FALLING`, `USING` | recusado: o item cai | — | — |
| `PRESSED`, `DRAGGING` | recusado (inalcançável com um ponteiro) | — | — |
| `SETTLING` (transitório), `HIDDEN`, `BOOTING`, `EXITING` | recusado ou ignorado | — | — |

O apoio sai da geometria, nesta ordem:

1. com `Esconderijo` marcado → `Esconderijo`;
2. `Ancora.Y == sup.Chao` → `Chao`;
3. `Ancora.Y == sup.Teto` → `Cipo`;
4. `NaLateral(Ancora.X)` → `Parede`;
5. senão → `Chao`, flutuando por toon force; a acomodação decide no fim.

A painel aberto e a autonomia pausada não impedem o uso.

**`UsarItem`:**

1. Remove o item (`RemoverItem Usado`).
2. Faz `Uso = new(...)`, `PassosRestantes = PassosDoUso` e `Expressao = CaraDurante`.
3. Chama `AplicarNaOnda`.
4. Faz `IrPara(Using, $"ITEM_DRAG_END sobre o personagem: {verbo} {item}")`.

### 4.7 Emoção dominante

- **`EscolherEmocao(e)`:**
  - ignora se não está carregado, se `e` não é de humor ou se é igual à atual;
  - senão, grava na preferência, emite `GravarPreferencias`, registra `Transicao(e, e, "CMD_SET_DOMINANT_EMOTION: …")`, e se não há onda e `CaraLivre`, `Expressao = e`;
  - escolher "Automática" mantém a cara atual até a próxima troca.
- **Sorteio com dominante:** `Ponderado([6, 1, 1, 1, 1])` sobre `[dominante, companheiras…]`. A dominante sai em 60% dos sorteios.
- **Cara de base:**
  - `CaraDeBase()` = cara da fase da onda, senão a dominante, senão `Neutro`;
  - `VoltarACaraDeBase()` só age com onda ou dominante, e é chamada no fim de `REACTING`, `LANDING` e `USING`;
  - ao acordar vale sempre `CaraDeBase()`, que na automática sem onda é `Neutro`, como hoje.

| Dominante | Companheiras |
|---|---|
| Neutro | Feliz, Curioso, Pensativo, Entediado |
| Feliz | Rindo, Empolgado, Travesso, Curioso |
| Rindo | Feliz, Travesso, Empolgado, Surpreso |
| Curioso | Pensativo, Surpreso, Feliz, Travesso |
| Surpreso | Assustado, Curioso, Empolgado, Rindo |
| Assustado | Surpreso, Pensativo, Curioso, Neutro |
| Sonolento | Bocejando, Dormindo, Entediado, Neutro |
| Bocejando | Sonolento, Entediado, Neutro, Pensativo |
| Dormindo | Sonolento, Bocejando, Neutro, Feliz |
| Travesso | Rindo, Feliz, Curioso, Empolgado |
| Entediado | Sonolento, Bocejando, Pensativo, Neutro |
| Pensativo | Curioso, Neutro, Entediado, Determinado |
| Empolgado | Feliz, Rindo, Surpreso, Determinado |
| Determinado | Empolgado, Pensativo, Neutro, Feliz |

### 4.8 Itens no mundo

**Nascimento (`LugarDeNascimento`), no monitor da âncora do personagem:**

- `supI = Superficies.Do(topologia, m, TamanhoDoItem em px)`.
- Posições candidatas: `x = ancora.X + s × (meiaLarguraDoPersonagem + folga + meiaLarguraDoItem + k × (larguraDoItem + folga))`:
  - `k` vai de 0 a 2;
  - `s` alterna entre o lado para onde ele olha (`Direcao`) e o outro;
  - vale a primeira dentro de `[supI.Esquerda, supI.Direita]` e sem cruzar outro item no chão ou caindo;
  - se nenhuma servir, a primeira candidata, presa aos limites.
- Altura inicial: `y = Math.Max(supI.Teto, Math.Min(ancora.Y, supI.Chao) − 140 DIP × escala)`.
- Estado inicial: `Caindo`, com `VY = 0` e `Id = ProximoIdDeItem++`.
- Com 6 itens, antes de nascer o novo sai o de menor Id que não está na mão (`RemoverItem Substituido`).
- No `IDLE`, sem onda, a cara vira `Empolgado`.

**Queda, a cada `Tick`, para cada item `Caindo`:**

- `vy = min(vy + g × escala × dt, queda máxima × escala)` e `y += vy × dt`, com a mesma gravidade e a mesma queda máxima do personagem.
- No chão: se `Quiques < 1` e `vy ≥ 300 × escala`, então `vy = −0,35 × vy` e `Quiques++`; senão `NoChao`, com `VY = 0`.
- `Lugar` e `Posicao` são recalculados com `Posicionador.Descrever`.

**Segurar e arrastar:**

- `ItemPress`: `Segurado`, `Pegada = cursor − âncora`, e `FicarAtento()`. Um outro item na mão é largado antes.
- `FicarAtento()`:
  - `WALKING` → `IDLE` ("ITEM_PRESS: para e olha o item");
  - `RESTING` → `IDLE` com `Sinal.Acordou`;
  - `CLIMBING` ou `HANGING` em movimento → `Movimento.Agarrado = true`;
  - sem onda, a cara vira `Curioso` em `IDLE`, `CLIMBING`, `HANGING` e `PEEKING`.
- `ItemDragMove`: âncora = cursor − pegada, sem prender (como no invariante 2), pelo `MonitorDaAncora`.

**Soltar (`ItemDragEnd` e `ItemRelease`):**

- A âncora é presa na área útil com `PrenderNaAreaUtil`.
- Com `ItemDragEnd`, `SobreOPersonagem(retânguloDoItem, Lugar.Retangulo, 20)` e `AceitaItem(Estado)` → `UsarItem`.
- Em qualquer outro caso: `Caindo`, ou `NoChao` se já estiver no chão.

**Mudança de topologia:** para cada item fora da mão, `Posicionador.Reacomodar(nova, Posicao, TamanhoDoItem)`. Fora do chão, fica `Caindo`; se o personagem está escondido, vai direto ao chão.

**Visibilidade:** `ItemVisivel = s.Estado.Visivel() && !(s.Preferencias.ModoTelaCheia && s.Ocupados.Contem(item.Lugar.Monitor.Chave))`.

**Efeitos das janelas dos itens (`EfeitosDosItens`), comparando `_inicio` com `_s`:**

- os removidos → `RemoverItem`;
- visível → invisível → `EsconderItem`;
- invisível ou novo → visível → `MostrarItem`;
- visível nos dois, com `Lugar` diferente → `MoverItem`.

### 4.9 Cambaleio

```csharp
FatorDoCambaleio(passo, a) = a == 0 ? 1 : 1 + a / 100.0 * (1 - 4 * Math.Abs((passo % 48) / 48.0 - 0.5))
```

É uma onda triangular de 48 passos (0,8 s). A 120%, o fator vai de −0,2 a 2,2: pequenos recuos. A velocidade instantânea continua dentro de −12% a 132% da caminhada normal.

### 4.10 O que não muda

- A física das quedas, dos quiques e do foguete, os limites, `Superficies` e `Acomodar`, e as regras de apoio.
- A prioridade do usuário e a tela cheia, salvo o que está descrito acima.
- `ExpressionChange`.
- A agenda sem onda, sem dominante e sem itens: sorteios, efeitos e textos idênticos.

### 4.11 Invariantes novos

Os números 18 a 21 são da Fase 5.

22. Sem itens, sem onda e com a emoção automática, retratos, transições e efeitos são idênticos aos de antes, com as referências 01–05 e a 06 da Fase 5 iguais byte a byte.
23. Um item só nasce por `CMD_SUMMON_ITEM` e só é usado por `ITEM_DRAG_END` do usuário sobre o personagem, num estado da tabela 4.6. Cada Id é usado no máximo uma vez. Nada autônomo, do relógio ou do sistema invoca ou usa um item.
24. Em `USING`, `PRESS` leva a `PRESSED` no mesmo evento. Nenhum evento autônomo chega à máquina. O uso dura exatamente `PassosDoUso` passos sem interrupção e sai por `SETTLING` para o mesmo apoio, mantendo `PresoPeloUsuario` e o esconderijo.
25. Com onda há exatamente um `ITEM_EFFECT_TIMER` pendente, de atraso ≥ 1 s; sem onda, nenhum. O nível fica entre 1 e 3 e há no máximo uma onda de fundo. Sem item novo, a onda da frente acaba em no máximo 2 + nível disparos.
26. A onda só muda pesos, intervalos, gestos, caras, as três velocidades (entre 50% e 200%) e o cambaleio, sempre sobre o chão e dentro de `[Esquerda, Direita]`. Gravidade, queda máxima, quique, foguete, colisões, limites, apoio e prioridade do usuário ficam intactos.
27. A emoção dominante é nula ou de humor. Com a mesma semente e os mesmos eventos, ligá-la muda só as expressões.
28. Há no máximo `MaximoDeItens` itens. Fora da mão, todo item tem o sprite inteiro na área útil do monitor dele e, parado, os pés no chão. Nenhum item tem janela visível com o personagem escondido ou num monitor ocupado.
29. O relógio corre se e somente se o personagem se move (sem estar agarrado), está em `REACTING` ou `USING`, faz um gesto em `IDLE`, ou há um item visível caindo.

---

## 5. Testes

### 5.1 Testes novos [AUTO] (`tests/Buzzy.Core.Testes/Tamagotchi/`)

**`TabelaDoTamagotchiTestes.cs`**

| Teste | O que afirma |
|---|---|
| `TodoItemTemVerboPassosECaraDeHumor` | 13 itens; passos de 60 a 240; a cara durante é de humor; só a água não tem onda. |
| `TodaOndaTemFasesFinitasEConjuntosValidos` | Subida ≥ 1 s; velocidade de 50 a 200; precedência de 1 a 3; as caras de efeito só aparecem nas tabelas das ondas; três níveis de pico. |
| `EpisodioMaisLongoCabeEmDezMinutos` | O maior episódio, com o pior nível 3, cabe em 10 min. |

**`ItensTestes.cs`**

| Teste | O que afirma |
|---|---|
| `Invocar_ApareceDoLadoQueEleOlhaCaiQuicaUmaVezEPara` | x = 1632 + 64 + 8 + 24 em `UmMonitor`; o relógio só liga durante a queda. |
| `Invocar_SemEspacoDoLado_UsaOOutro` | Perto da lateral, nasce do outro lado. |
| `Invocar_SetimoTiraOMaisAntigoForaDaMao` | Com 6 itens, o 7º tira o de menor Id que não está na mão. |
| `Invocar_EscondidoSemOModoOuForaDoEnum_EhIgnorado` | Em `HIDDEN`, com `Tamagotchi` desligada ou com um item fora do enum, nada acontece. |
| `SegurarItem_FicaAtentoSemAgenda` | Andando → `IDLE`; na parede → agarrado; descansando → acorda; `CancelarDecisao`; depois de soltar fora, `AgendarDecisao` ≥ 3 s. |
| `ArrastarItem_EhOCursorMenosAPegada` | Durante o arraste, a âncora do item é o cursor menos a pegada. |
| `SoltarFora_CaiDeOndeFoiSolto` | O item cai do ponto solto e espera no chão. |
| `SoltarSobreOPersonagem_TabelaDeEstados` | Um caso por linha da tabela 4.6: aceito leva a `USING` e `RemoverItem Usado`; recusado deixa o item `Caindo`. |
| `ReleaseSobreOPersonagem_NaoUsa` | Um clique ou uma captura perdida sobre ele não usa o item. |
| `EsconderEMostrar_ItensSomemEVoltam` | Os itens caindo vão direto ao chão ao esconder. |
| `TopologiaMuda_ItensReacomodados` | Os itens são reacomodados e voltam ao chão. |
| `TelaCheia_ItemNoMonitorOcupadoSemJanela` | O item no monitor ocupado perde a janela e a recupera quando o monitor é liberado. |
| `RecolherItens_TodosSomem` | Depois do comando, nenhum item sobra. |
| `SemComando_NenhumItemEm30Min` | Em 30 min simulados com a agenda ligada, nenhum item aparece. |

**`UsoTestes.cs`**

| Teste | O que afirma |
|---|---|
| `Usar_DuraOsPassosDoItem` (13 itens) | O retrato mostra o uso e a cara durante; o relógio fica ligado; `AutonomyTimer` é descartado pelo `Nucleo`; no fim, `IDLE`. |
| `Usar_NaParedePresa_VoltaGrudadaEPresa` | Depois do uso, continua agarrado e preso na parede. |
| `Usar_NoCipo_VoltaAoCipo` | Depois do uso, volta a ficar agarrado no cipó. |
| `Usar_Escondido_VoltaAoEsconderijo` | Depois do uso, continua escondido na mesma borda. |
| `Usar_PressNoMeio_SeguraNaHoraEOndaContinua` | `PRESSED` no mesmo evento, `Uso` nulo, a onda continua e nenhum `CancelarOnda`. |
| `Usar_EsconderOuTopologiaNoMeio_EncerraSemPerderAOnda` | O uso acaba e a onda continua. |
| `Usar_SegundoItemDuranteOUso_Cai` | O segundo item é recusado e cai. |

**`OndaTestes.cs`**

| Teste | O que afirma |
|---|---|
| `CadaItem_FasesComAsDuracoesDaTabela` | Atrasos de `AgendarOnda` exatos; número de disparos; no fim, onda nula e cara de base. |
| `MesmoTipo_AcumulaAteONivel3` | Cerveja + vodka chegam ao nível 3, com o pior nível 3 e queda de 135 s. |
| `MaisForte_VaiParaAFrenteEAAnteriorVolta` | Com bebado e depois lança-perfume, o tonto vai para a frente e o bebado volta depois, na fase em que estava, com a duração cheia. |
| `MaisFraca_EhAbsorvida` | A onda não muda e nenhum `AgendarOnda` sai. |
| `SoCabemDuas` | Bebado, chapado e eletrico deixam só o chapado atrás. |
| `Agua_BaixaUmNivelEEncerraAQueda` | A água segue a regra de `Refrescar`. |
| `DisparoAntigo_Ignorado` | Um disparo de geração antiga não faz nada. |
| `Pausado_SoMudaACara` | Sem transições de estado e sem relógio. |
| `EmRestingReactingUsing_ACaraEsperaOFim` | Nesses estados, a cara da fase só entra quando eles acabam. |
| `EmRepouso_NaoLigaORelogio` | Uma hora simulada com itens aleatórios: todo atraso ≥ 1 s e disparos ≤ ao limite do invariante 25. |
| `PerfilEfetivo_BateComATabela` | O perfil efetivo segue a seção 4.3, e `AgendarDecisao` fica dentro da faixa efetiva e acima do piso. |
| `FisicaEfetiva_SoAsTresVelocidades` | Só as três velocidades mudam. |
| `Bebado3_CambaleiaDeterministicoNoChao` | x não monótono, y sempre no chão, e a segunda execução é idêntica. |
| `Eletrico_MaisAcoesChapado_Menos` | 10 min com `TabelaDeOndas` de pico longo: ações e tempo em movimento, eletrico > sem onda > chapado. |
| `GestosDaOnda_SoEmIdleEInterrompidos` | Os gestos da onda só acontecem em `IDLE` e terminam com evento de prioridade maior (invariante 15). |

**`EmocaoDominanteTestes.cs`**

| Teste | O que afirma |
|---|---|
| `Escolher_TrocaACaraEGrava` | A cara muda na hora e sai `GravarPreferencias`; repetir a mesma escolha não faz nada. |
| `ForaDasQuatorze_Ignorada` | O comando com uma cara de efeito é ignorado; em `SettingsChanged`, ela vira nula. |
| `Dominante_Sai60PorCentoNasTrocas` | 500 trocas: só a dominante e as companheiras aparecem, e a dominante sai em ≥ 50%. |
| `Dominante_SoMudaACara` | Propriedade com 40 sementes × 10 min, dominante pelo `Loaded` contra automática: transições, âncoras e efeitos idênticos. |
| `VoltaDepoisDeReacaoPousoEAcordar` | Nos três ganchos, a cara volta à dominante. |
| `Onda_TemPrecedenciaEDepoisVolta` | Com onda, a cara é a da onda; quando ela acaba, volta a dominante. |
| `Carga_ComDominante_ComecaComACara` | O `Loaded` com a dominante já começa com a cara dela. |

### 5.2 Ajustes em testes existentes

- **`EsquemaDeConfiguracoesTestes`:**
  - texto exato do v2;
  - `Ler_V1_SemEmocao_ValeAutomaticaSemAviso`;
  - `Ler_EmocaoInvalida_ComAviso`: `"bebado"`, `3`, `true` e `""` viram nula; `"FELIZ"` vira Feliz;
  - `Ler_VersaoFutura` passa a usar `schemaVersion` 3.
- **`PropriedadesDaPersistenciaTestes`:** ida e volta com a emoção aleatória, inclusive nula.
- **`ReproducaoTestes`:**
  - os eventos novos em `EscreverELerDevolvemOMesmoEvento`;
  - teste `Preferencias_EmocaoSoApareceQuandoEscolhida`;
  - diretiva `# tamagotchi: sim` em `LerCabecalho`;
  - referência nova `07-tamagotchi.txt` na lista `Referencias` (a 06 é da Fase 5).
- **`SimuladorDeTempo`:** trata `AgendarOnda` e `CancelarOnda`. `Avancar` entrega o que vence primeiro; num empate, a onda vem antes da agenda, pela prioridade `Relogio`.
- **`Cenario.Em(Estado.Using)`:** invocar, `Tick` até o chão, `ItemPress`, `ItemDragStart`, `ItemDragMove` e `ItemDragEnd` sobre a âncora.
- **`FilaEAleatorioTestes`:** `ItemEffectTimer` não é descartado em `PRESSED`; `AutonomyTimer` é descartado em `USING`.
- **`InvariantesTestes`:**
  - o gerador ganha um `Random` próprio, semeado com `semente * 37 + 11`. Ele liga `Tamagotchi` e insere invocação, arraste até o personagem ou para outro lugar, `ItemRelease`, `ItemEffectTimer` (atual ou velho) e `CmdSetDominantEmotion` (inclusive com caras de efeito). Assim os sorteios do gerador principal não mudam;
  - `ExpressionChange` fica preso em `rnd.Next(14)`;
  - oráculos novos:
    - relógio pelo invariante 29;
    - agenda com `!Atento`;
    - R11 pelo `Maquina.PerfilEfetivo`;
    - R1 também para a emoção;
    - invariante 11 para `Using`;
    - invariantes 22–28 conferidos a cada evento aplicado;
  - `CasosExigidos` novos: "item usado", "item solto fora", "USING interrompido por PRESS", "onda avançou", "disparo de onda velho", "onda de fundo voltou", "água baixou a onda", "emoção escolhida", "emoção inválida ignorada", "sétimo item".

### 5.3 Referência `07-tamagotchi.txt` (entradas; a saída é gravada com `BUZZY_ATUALIZAR_REFERENCIAS=1` e revisada)

```
# descricao: emoção dominante, vodka usada, lança-perfume por cima (onda de fundo), água, banana solta fora, pressionar no uso
# semente: 2028
# queda-fisica: sim
# movimento: sim
# tamagotchi: sim
# acoes: Descansar,Gesto,TrocarExpressao
> Loaded topologia=UmMonitor energia=Media telaCheia=sim
> CmdSetDominantEmotion emocao=Travesso
> CmdSummonItem item=Vodka
> Tick vezes=60
> ItemPress id=1 x=1728 y=1010
> ItemDragStart id=1
> ItemDragMove id=1 x=1632 y=990
> ItemDragEnd id=1 x=1632 y=990
> Tick vezes=150
> ItemEffectTimer
> CmdSummonItem item=LancaPerfume
> (arrastar o id 2 até (1632,990) e soltar)
> Press x=1632 y=1000
> Click
> Tick vezes=40
> ItemEffectTimer
> ItemEffectTimer
> ItemEffectTimer
> CmdSummonItem item=Agua
> (arrastar o id 3 e soltar sobre ele; Tick vezes=90)
> CmdSummonItem item=Banana
> (arrastar o id 4 para x=900 e soltar fora; Tick vezes=60)
> ItemEffectTimer
> ItemEffectTimer
```

As coordenadas de pegar os itens 2, 3 e 4 saem de `MostrarItem` na primeira gravação.

### 5.4 Verificação de tela (`Buzzy.Verificacao --tamagotchi`, input SINTÉTICO)

A verificação usa `--perfil-de-teste` e `--semente` e avisa o usuário antes.

| Caso | Passos | Critério |
|---|---|---|
| T1 | Invocar a banana pelo menu (clique direito e teclado). | A janela do item aparece ao lado dele e a base chega ao chão; parado 3 s sem mudar. |
| T2 | Arrastar o item até ele. | Log `Using`; o sprite de uso aparece; a janela do item some; depois de 3 s ± 1 quadro, `Idle`. |
| T3 | Soltar fora. | O item cai de onde foi solto. |
| T4 | Pressionar no meio do uso. | `PRESSED` em até 1 quadro. |
| T5 | Cocaína e baseado, com a agenda ligada. | Caminhada de cerca de 153–180 contra 45–54 DIP/s (±10%). |
| T6 | Vodka duas vezes e cerveja (nível 3). | Recuos na caminhada, sempre no chão. |
| T7 | Pausado, com vodka. | CPU perto de 0%, relógio desligado, 4 disparos `ONDA` e 3 trocas de cara no log. |
| T8 | Emoção pelo menu. | O arquivo do perfil de teste tem `"emocaoDominante": "feliz"`; ao reabrir, começa feliz. |
| Todos | — | O foco continua no app em uso. |

---

## 6. Riscos e pontos de contato

1. **Fase 5 em andamento.**
   - `Maquina.cs` (em `MudarTopologia`, `Carregar`, `PassoAndando` com a travessia e `Decidir` com a trava L4), `Gravacao.cs`, `Tipos.cs` (a Fase 5 também acrescenta valores no fim de enums) e `InvariantesTestes` vão ter conflito.
   - Implemente depois do bloco A e antes dos blocos B–D.
   - Nos blocos B–D, `Using` é tratado como `Reacting` nas revalidações, e o atento espera a travessia em curso terminar, como a pausa e o painel.
   - O cambaleio multiplica o `passo` que a travessia usa. Confira que um recuo não dispara a travessia pela lateral de trás.
2. **Persistência.**
   - Mudar para a v2 muda o texto exato das amostras v1 dos testes da Fase 5 e o teste de versão futura (2 → 3).
   - A emoção como propriedade `init` evita conflito com o construtor posicional de `Preferencias`.
3. **App.**
   - `Aplicacao.ExecutarEfeito` lança exceção para efeito desconhecido.
   - `DoAplicativo.Tamagotchi = true` só na mesma entrega dos `case` novos, das janelas dos itens (não ativadas, sempre no topo, abaixo do personagem na ordem Z) e dos logs `ITEM` e `ONDA` com os pontos de teste.
4. **Arte.**
   - As 7 caras de efeito, os 6 gestos e as poses de uso por verbo e apoio precisam existir antes de ligar a chave. Até lá, `PoseDoPersonagem` cai no `parado`.
   - A DEC-027 pode entrar antes, sozinha: só usa as 14 caras que já existem e não depende dos itens.
5. **Oráculos.** Os invariantes 1, 12 e 15 mudam de texto, e o relógio, a agenda e o R11 mudam nos testes. Um oráculo esquecido falha de forma visível, sem mascarar nada.
6. **Numeração.** Os invariantes 22–29 e a referência 07 supõem que a Fase 5 fique com os invariantes 18–21 e a referência 06. Renumere no fechamento.
7. **Conteúdo.** Os nomes vêm do pedido do usuário. A intensidade (1–2) é ponto de jogo, não dose. As durações são de desenho animado, sem relação com a farmacologia. Não há descrição de efeito na interface, só rótulos em `Textos.resx`.
8. **Ordem de implementação.**
   1. DEC-027 inteira: seções 3.1, 3.2 (emoção), 3.7 (caras), 3.8, 3.9 e os testes da emoção.
   2. Os tipos, a tabela e a onda, com a chave desligada.
   3. Os itens e `USING`.
   4. `InvariantesTestes` e a referência 07.
   5. A entrega do app e da arte (outras áreas), a chave ligada e a verificação de tela.
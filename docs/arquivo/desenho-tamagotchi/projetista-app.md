> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de o desenho do app da emoção dominante e do tamagotchi adulto (o código o cita como "desenho do app": D…, riscos R…, seção 4.6), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a emoção dominante e o tamagotchi. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-027 e DEC-028) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

# Desenho: emoção dominante no menu (DEC-027) e tamagotchi adulto com itens (DEC-028)

> Desenho feito só por leitura, sobre o checkout de 2026-09-30: commit `87c4203` mais as alterações sem commit do bloco A da Fase 5. Nada foi editado, compilado ou executado.
>
> Todo arquivo, tipo e função citado existe hoje, salvo os marcados como **novo**. As linhas citadas são as do checkout atual.
>
> Ordem de entrada, conforme `CONTINUIDADE.md`: depois do bloco A da Fase 5 e antes dos blocos B a D. O bloco A mexe em `Preferencias`, `Gravacao`, `Persistencia/*`, `BuzzyEmTeste` e nos lançadores do `Buzzy.Verificacao`.

## 0. Visão geral

```
Menu nativo ──CMD_SUMMON_ITEM(tipo)──▶ Núcleo: ItemNoMundo nasce no ar ao lado dele ──AtualizarItens──▶ GerenteDosItens ▶ JanelaDoItem
JanelaDoItem (mouse) ▶ ArbitroDeGestos (instância dos itens) ▶ ITEM_PRESS / ITEM_DRAG_START / ITEM_DRAG_MOVE / ITEM_DROP(sobre?) / ITEM_RELEASE / ITEM_REMOVE ▶ Núcleo
Núcleo: ITEM_DROP(sobre=sim) ▶ USING (roteiro do item; relógio só durante a animação) ▶ fim do uso ▶ Alteração (tipo × intensidade × fase) + Acomodar
Alteração ▶ AgendarFimDaAlteracao (disparo único) ▶ ITEM_EFFECT_PHASE_END ▶ próxima fase … ▶ Nenhuma
Apresentação: PoseDoPersonagem escolhe pose de uso, objeto na mão, rosto do efeito e partículas; SpriteProvisorio compõe tudo com Buzzy.Visual.
Menu: submenus "Emoção dominante" (Automática + 14 rostos) e "Itens" (13 + Recolher), com ícones HBITMAP de 32 bits pré-multiplicados.
```

---

## 1. Decisões

Cada decisão traz o motivo e as alternativas descartadas.

**D1. Itens, uso e efeito ficam no núcleo, dentro de `EstadoDoNucleo`.**
- Motivo:
  - um relógio só, calculado em `Maquina.Passo.Concluir`, que liga também para item caindo;
  - uma fila só, `Nucleo`;
  - uma reprodução gravada só, `Gravacao`;
  - os invariantes são conferidos juntos por `InvariantesTestes`;
  - "mesma semente + mesmos eventos = mesmo resultado" vale para tudo.
- Descartadas:
  - um módulo puro separado, coordenado pela raiz: duas máquinas, dois relógios e duas reproduções;
  - lógica na raiz: não determinística e testável só com janela.

**D2. Estado novo `USING`, no grupo usuário.**
- Motivo:
  - o `Nucleo` já descarta `AUTONOMY_TIMER` no grupo usuário;
  - aceita `PRESS`, como `REACTING`;
  - relógio só durante a animação;
  - termina por `Acomodar`, que já devolve ao cipó, à parede (preso ou não, DEC-024) e ao esconderijo (DEC-025).
- Descartadas:
  - gesto curto: o invariante 15 exige que ele termine com qualquer evento e só exista em `IDLE`;
  - estender `REACTING`: misturaria regras e reações.

**D3. Efeito do item é uma dimensão ortogonal, com fases e disparo único.**
- No código chama-se `Alteracao`, porque `Efeito` já é o pedido do núcleo à raiz. As fases são `Subida → Pico → Rebote → Nenhuma`, e cada uma termina por um temporizador único (`AgendarFimDaAlteracao`).
- Descartadas:
  - necessidades que decaem com o tempo: o usuário recusou;
  - duração contada em decisões autônomas: irregular e parada com a autonomia pausada;
  - timer periódico: proibido pela DEC-011.

**D4. Uma alteração por vez.**
- Mesmo tipo soma intensidade, com teto 3. Outro tipo substitui a atual. Água e banana são restauradoras.
- Motivo: caras e regras previsíveis e testáveis.
- Descartada: pilha de efeitos combinados, que explode em combinações de caras e pesos.

**D5. O que um efeito pode mudar.**
- Muda só:
  - a velocidade de andar, escalar e pendurar-se, com fator entre 0,4 e 2,2;
  - pesos, intervalos e durações da agenda;
  - distância e altura dos pulos (faixas do perfil);
  - a cara.
- Nunca muda gravidade, queda máxima, quique, foguete, impulso da parede, apoio, colisão nem a prioridade do usuário. É a exceção limitada ao invariante 12 que a DEC-028 aceitou.
- Descartada: física alterada (cair ou quicar diferente), que tira a previsibilidade.

**D6. As caras de efeito são só apresentação; o enum `Expressao` fica com 14 valores.**
- As caras novas são entradas novas em `Buzzy.Visual` (`Rostos.DeEfeito`).
- Motivo:
  - `DecidirParado` sorteia `Enum.GetValues<Expressao>().Length - 2`, e um valor novo mudaria os sorteios e as referências gravadas 01 a 05;
  - o menu listaria cara de droga como emoção;
  - `InvariantesTestes.Sortear` usa `Enum.GetValues<Expressao>()`.
- Descartado: acrescentar valores a `Expressao`.

**D7. A emoção dominante é uma preferência persistida, `Preferencias.EmocaoDominante` (`Expressao?`), mudada por `CMD_SET_DOMINANT_EMOTION`.**
- Ela vira a cara de base e a mais frequente (peso 6 contra 2 das vizinhas).
- Dá uma tendência leve aos pesos da agenda, sempre dentro do invariante 12.
- Descartadas:
  - trocar a cara uma vez só: sumiria na próxima troca;
  - travar a cara: mataria a expressividade;
  - guardar só em memória: a DEC-027 pede o `settings.json`.

**D8. Cada item tem sua própria janela WPF (`JanelaDoItem`), com a receita da `JanelaPersonagem`.**
- A receita: `AllowsTransparency`, `WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`, `MA_NOACTIVATE`, `Topmost`, `WM_GETDPISCALEDSIZE`, captura só no gesto. P1 e P3 validaram esse caminho.
- Descartadas:
  - desenhar os itens na janela do personagem: exige janela grande, proibido por ARCHITECTURE 2.13.7-1;
  - janela Win32 crua com `UpdateLayeredWindow`: mais código nativo e sem a validação de P1;
  - uma janela do tamanho da tela: overlay proibido.

**D9. Gestos no item passam por uma segunda instância de `ArbitroDeGestos`, com os gestos traduzidos para eventos de item.**
- Motivo: reaproveita o limiar `SM_CXDRAG`, a captura perdida, o `MK_LBUTTON` e o ClickLock.
- Descartada: lógica própria, que duplicaria regras já testadas.

**D10. "Solto sobre ele" é decidido pela raiz.**
- A regra: o retângulo dos pixels opacos do item, no ponto solto, encosta no retângulo dos pixels opacos do quadro atual do personagem, ampliado em 12 DIP.
- A conta é a função pura `Entrega.Acertou`, no núcleo.
- O núcleo recebe o resultado como fato do adaptador, como já faz com "PRESS sobre pixel opaco".
- Descartadas:
  - pixel opaco sob o cursor: exigente demais, porque soltar entre o braço e o corpo falharia;
  - a janela inteira de 128 DIP: aceitaria soltar sobre o transparente;
  - `WindowFromPoint`: proibido em `Regras.cs`.

**D11. Invocar.**
- O item surge no ar ao lado dele, sobe 40 DIP, cai e quica uma vez. Fica do lado para onde ele olha; sem espaço, do outro lado, em vagas de 44 DIP.
- No máximo 5 itens; no limite, some o mais antigo que está parado.
- Descartadas:
  - surgir já no chão: sem a física de desenho pedida;
  - recusar o sexto item: atrito.

**D12. Enquanto o usuário arrasta um item, ele para e espera ("esperando item").**
- Não decide nada; andando, para; subindo, fica agarrado onde está.
- Motivo: fica fácil acertar nele.
- Descartadas:
  - continuar andando;
  - pausar a autonomia global, que mudaria a escolha do usuário.

**D13. Ele usa onde está.**
- Chão, cipó, parede ou esconderijo. No ar, pega o item e usa ao terminar o pouso.
- Descartadas:
  - usar só no chão: tiraria do cipó ou da parede contra a DEC-024 e do esconderijo contra a DEC-025;
  - recusar fora do chão: frustra o usuário.

**D14. Interrupção.**
- Pressionar, esconder, sair ou uma revalidação interrompem o uso.
- Antes do ponto de consumo, o item volta ao mundo; depois, o efeito vale na hora.
- Descartadas: sempre perder o item ou sempre aplicar o efeito.

**D15. Menu nativo com submenus e `hbmpItem` DIB de 32 bits pré-multiplicado.**
- Os bitmaps são criados a cada abertura e apagados (`DeleteObject`) depois do `DestroyMenu`.
- Ícone de 32·k px: k = 1 até 191 DPI, 2 em 192 e 3 em 288.
- Em alto contraste, sem ícones.
- Descartadas:
  - `ContextMenu` WPF: a DEC-016 o recusou por foco e ativação;
  - owner-draw: mais código e acessibilidade própria;
  - cache de HBITMAP pela sessão inteira: GDI fixo à toa.

**D16. Arte dos itens.**
- Um carimbo por item, de no máximo 12×14, sem contorno externo (o `Tela.Contornar` põe o contorno).
- Quadro do item de 20×20 pixels de arte = 40×40 DIP, com a mesma escala de pixel do personagem.
- Carimbo menor na mão só para cigarro, MD e bala.
- Descartadas:
  - reduzir a arte grande: perde olhos e detalhes;
  - escala de pixel diferente da do personagem: quebra o estilo.

**D17. O cache de quadros do personagem vira LRU limitado a 16 MB.**
- Uso, partículas e objetos multiplicam os quadros possíveis (M3/M7).
- Descartados: cache ilimitado; nenhum cache.

**D18. Itens, uso e efeito nunca vão para o disco; só a emoção dominante.**
- Descartada: persistir os itens do chão, sem valor e com mais superfície de ataque.

**D19. O código novo fica em arquivos parciais e próprios, para reduzir conflito com a Fase 5.**
- Novos: `Maquina.Itens.cs`, `Aplicacao.Itens.cs`, `EventosDosItens.cs` e `EfeitosDosItens.cs`.
- Nos arquivos existentes, só ganchos de uma a três linhas.

**D20. Testes.**
- Núcleo primeiro: cenários, propriedades com gerador próprio para não mudar os sorteios de hoje e as referências **10** e **11**. As referências 06 a 09 ficam reservadas à Fase 5, cuja crítica já cita a 06.
- Integração só com mensagens postadas às janelas do próprio Buzzy, inclusive teclado postado ao dono do menu.
- Verificação com input SINTÉTICO: `--fase itens`.

**D21. Tom.**
- Nomes e efeitos de desenho animado. Nenhuma dose, obtenção ou preparo, nem texto sobre efeito real.
- O menu só nomeia os itens.
- Logs sem dado pessoal.

---

## 2. Tipos e arquivos novos (assinaturas)

### 2.1 `Buzzy.Core` (net10.0, puro)

**`src/Buzzy.Core/Itens/Catalogo.cs`** (novo)

```csharp
namespace Buzzy.Core.Itens;

// Ordem = ordem do menu (grupos separados por separador: comer e beber | álcool | fumo | festa).
public enum TipoDeItem { Banana, Agua, Cafe, Energetico, Bala, Cerveja, Vodka, Cigarro, Baseado, Cocaina, MD, LancaPerfume, Cogumelo }
public enum ModoDeUso { Comer, Beber, Fumar, Cheirar, Engolir, Inalar }

/// <param name="Doses">Quanto soma na intensidade (1 a 3). Restauradores (água, banana) usam regra própria (4.3.3).</param>
public sealed record DefinicaoDeItem(TipoDeItem Tipo, ModoDeUso Modo, TipoDeAlteracao Alteracao, int Doses, bool Restaurador, RoteiroDeUso Roteiro);

/// <param name="Duracao">Passos do relógio (60/s) de USING.</param>
/// <param name="PassoDoConsumo">A partir deste passo o item foi consumido: interromper aplica o efeito.</param>
public sealed record RoteiroDeUso(int Duracao, int PassoDoConsumo);

public static class CatalogoDeItens
{
    public static IReadOnlyList<DefinicaoDeItem> Todos { get; }   // os 13, na ordem do enum
    public static DefinicaoDeItem De(TipoDeItem tipo);            // lança para valor fora do enum
}
```

**`src/Buzzy.Core/Itens/Alteracao.cs`** (novo)

```csharp
public enum TipoDeAlteracao { Nenhuma, Saciado, Refrescado, Cafeina, Acucar, Alcool, Relaxado, Chapado, Acelerado, Amoroso, Zonzo, Viajando }
public enum FaseDaAlteracao { Nenhuma, Subida, Pico, Rebote }

public sealed record AlteracaoAtiva(TipoDeAlteracao Tipo, int Intensidade, FaseDaAlteracao Fase)
{
    public static readonly AlteracaoAtiva Nenhuma = new(TipoDeAlteracao.Nenhuma, 0, FaseDaAlteracao.Nenhuma);
}

/// Percentuais inteiros sobre os pesos do PerfilDeEnergia; fatores double sobre tempos e velocidade.
public sealed record ModificadoresDoHumor(
    double Velocidade, double Intervalo, double Descanso, double Salto,
    int Andar, int Escalar, int Pular, int Descansar, int Gesto, int TrocarExpressao,
    int FogueteExtra, IReadOnlyList<(Gesto Gesto, int Peso)> Gestos)
{
    public static readonly ModificadoresDoHumor Neutros; // 1,1,1,1, 100×5, 100, 0, []
}

public static class TabelaDeAlteracoes
{
    public static TimeSpan? Duracao(TipoDeAlteracao tipo, int intensidade, FaseDaAlteracao fase); // nulo = fase inexistente
    public static FaseDaAlteracao Primeira(TipoDeAlteracao tipo);                                   // Subida se houver, senão Pico
    public static FaseDaAlteracao Seguinte(AlteracaoAtiva a);                                       // pula fases sem duração
    public static ModificadoresDoHumor Modificadores(AlteracaoAtiva a);
}

public static class Humor
{
    /// Identidade exata (mesma instância) com Nenhuma e dominante nula: as referências 01–05 não mudam.
    public static PerfilDeEnergia Ajustar(PerfilDeEnergia perfil, AlteracaoAtiva alteracao, Expressao? dominante);
    /// Fator de locomoção, preso em [0,4; 2,2]; 1,0 exato sem alteração.
    public static double Velocidade(AlteracaoAtiva alteracao);
}
```

**`src/Buzzy.Core/Itens/ItensNoMundo.cs`** (novo)

```csharp
public enum SituacaoDoItem { Caindo, NoChao, Segurado, Arrastado }
public enum LugarDoUso { Chao, Cipo, Parede, Esconderijo }
public enum MotivoDaRemocao { Usado, PeloUsuario, Recolhido, Limite }

public sealed record ItemNoMundo(int Id, TipoDeItem Tipo, SituacaoDoItem Situacao, Posicionamento Lugar, PosicaoDoPersonagem Posicao, double X, double Y, double VY)
{
    public int PassosNoChao { get; init; }   // passos desde o último toque no chão
    public bool Quicou { get; init; }
    public PontoPx Pegada { get; init; }     // cursor − âncora no ITEM_PRESS
}

/// Coleção imutável ordenada por Id, com igualdade por valor (como MonitoresOcupados).
public sealed class ItensNoMundo : IEquatable<ItensNoMundo>
{
    public static readonly ItensNoMundo Vazio;
    public IReadOnlyList<ItemNoMundo> Todos { get; }
    public int Count { get; }
    public ItemNoMundo? PorId(int id);
    public ItensNoMundo Com(ItemNoMundo item);   // acrescenta ou substitui pelo Id
    public ItensNoMundo Sem(int id);
    public ItemNoMundo? EmGesto { get; }         // Segurado ou Arrastado (no máximo um)
    public bool AlgumArrastado { get; }
    public bool PrecisaDeRelogio(int passosDoAchatamento); // algum Caindo, ou NoChao com PassosNoChao < passos
}

public sealed record ParametrosDosItens
{
    public static readonly TamanhoDip TamanhoPadrao = new(40, 40);  // 20×20 px de arte; fonte única p/ app
    public TamanhoDip Tamanho { get; init; } = TamanhoPadrao;
    public int Maximo { get; init; } = 5;
    public double DistanciaDoSurgimento { get; init; } = 60;   // DIP, âncora do personagem → âncora do item
    public double EspacoEntreItens { get; init; } = 44;        // DIP entre vagas
    public double AlturaDoSurgimento { get; init; } = 24;      // DIP acima do chão
    public double ImpulsoDoSurgimento { get; init; } = 420;    // DIP/s para cima
    public double RestituicaoDoQuique { get; init; } = 0.35;
    public double ImpactoMinimoDoQuique { get; init; } = 250;  // DIP/s
    public int PassosDoAchatamento { get; init; } = 5;
    public double MargemDeEntrega { get; init; } = 12;         // DIP (usado pela raiz)
}

public sealed record UsoEmCurso(TipoDeItem Item, LugarDoUso Onde, int Passo, int Duracao, int PassoDoConsumo);
public sealed record ItemVisivel(int Id, TipoDeItem Tipo, SituacaoDoItem Situacao, Posicionamento Lugar, double VelocidadeVerticalDip, int PassosNoChao, bool Visivel);
```

**`src/Buzzy.Core/Itens/Entrega.cs`** (novo)

```csharp
public static class Entrega
{
    /// O retângulo opaco do item encosta no retângulo opaco do personagem ampliado de margemPx de cada lado.
    public static bool Acertou(RetanguloPx itemOpaco, RetanguloPx personagemOpaco, int margemPx);
}
```

**`src/Buzzy.Core/Personagem/EmocaoDominante.cs`** (novo)

```csharp
public static class EmocaoDominante
{
    public static IReadOnlyList<Expressao> Vizinhas(Expressao dominante);                                        // tabela 4.4
    public static ModificadoresDoHumor Tendencia(Expressao dominante);                                           // tabela 4.4
    public static IReadOnlyList<(Gesto Gesto, int Peso)> GestosPreferidos(Expressao dominante);
}
```

**`src/Buzzy.Core/Personagem/EventosDosItens.cs`** (novo)

```csharp
public sealed record CmdSetDominantEmotion(Expressao? Emocao) : Evento { public override Origem Origem => Origem.ComandoDoUsuario; } // nula = Automática
public sealed record CmdSummonItem(TipoDeItem Tipo) : Evento { … ComandoDoUsuario }
public sealed record CmdClearItems : Evento { … ComandoDoUsuario }
public sealed record ItemPress(int Id, PontoPx Cursor) : Evento { … AcaoDireta }
public sealed record ItemDragStart(int Id) : Evento { … AcaoDireta }
public sealed record ItemDragMove(int Id, PontoPx Cursor) : Evento { … AcaoDireta }
public sealed record ItemDrop(int Id, PontoPx Cursor, bool SobreOPersonagem) : Evento { … AcaoDireta }
public sealed record ItemRelease(int Id) : Evento { … AcaoDireta }   // clique, clique duplo ou captura perdida
public sealed record ItemRemove(int Id) : Evento { … AcaoDireta }    // botão direito solto no item
public sealed record ItemEffectPhaseEnd(long Geracao) : Evento { … Relogio } // nunca descartado pelo Nucleo
```

**`src/Buzzy.Core/Personagem/EfeitosDosItens.cs`** (novo)

```csharp
/// Estado desejado das janelas de item (instantâneo completo); a raiz aplica só o último de um lote.
public sealed record AtualizarItens(IReadOnlyList<ItemVisivel> Itens, IReadOnlyList<(int Id, MotivoDaRemocao Motivo)> Removidos) : Efeito;
public sealed record AgendarFimDaAlteracao(TimeSpan Atraso, long Geracao) : Efeito;
public sealed record CancelarFimDaAlteracao : Efeito;
public sealed record LiberarCapturaDoItem : Efeito;
```

**`src/Buzzy.Core/Personagem/Maquina.Itens.cs`** (novo). É um `partial` de `Maquina.Passo`, com os métodos privados:
- `EscolherEmocao`, `InvocarItem`, `RecolherItens`;
- `PressionarItem`, `IniciarArrasteDoItem`, `ArrastarItem`, `SoltarItem`, `LiberarItem`, `RemoverItem`;
- `PassoDosItens`, `ReacomodarItens`, `AcomodarItem`;
- `OndeUsar`, `ComecarUso`, `PassoDoUso`, `TerminarUso`, `InterromperUso`;
- `LargarItemDaMao`, `SoltarItemEmGesto`, `ComecarUsoDoItemNaMao`;
- `AplicarItem`, `IniciarFase`, `AvancarAlteracao`;
- `Ritmo`, `CaraDeRepouso`, `SortearNovaExpressao`, `SortearGesto`, `EsperarItem`, `ItensVisiveis`.

### 2.2 `Buzzy.Visual` (continua sem referência ao Core; chaves por nome, como `Rostos`)

- **`Pixel/Itens.cs`** (novo): `public static class DesenhosDosItens`
  - `Tela NoChao(string item)`: quadro de 20×20, carimbo embaixo e no centro, contornado.
  - `ObjetoNaMao NaMao(string item, ModoDoObjeto modo, bool aceso)`.
  - `Tela Icone(string item)`: célula de 16×16.
  - `IReadOnlyList<string> Nomes`: os 13 nomes, `"banana"` a `"cogumelo"`.
- **`Pixel/Particulas.cs`** (novo): `public static class Particulas`
  - `void Desenhar(Tela tela, string tipo, int fase, PontosDaPose pontos, string? item = null)`;
  - tipos: `fumaca`, `fumaca-lenta`, `po`, `nevoa`, `bolhas`, `coracoes`, `estrelas`, `brilhos`, `suor`, `migalhas`, `soluco`, `arco`.
- **`Pixel/RetratoDoRosto.cs`** (novo): `public static Tela Desenhar(string expressao)` recorta 32×32 em (16, 1) do quadro `parado`, o mesmo desenho de `expressoes.png`. Tem cache por nome.
- **`Pixel/BonecoPixel.cs`**:
  - `public sealed record ObjetoNaMao(Tela Desenho, (int X, int Y) Pega)`;
  - `public enum ModoDoObjeto { NaMao, NaBoca, NoNariz }`;
  - `public enum LadoDoBraco { A, B }`;
  - `public readonly record struct PontosDaPose((double X, double Y) Boca, (double X, double Y) Nariz, (double X, double Y) TopoDaCabeca, (double X, double Y) MaoA, (double X, double Y) MaoB)`;
  - `public static PontosDaPose Pontos(PosePixel pose)`.

### 2.3 `Buzzy.App`

**`Apresentacao/JanelaDoItem.cs`** (novo)

```csharp
internal sealed class JanelaDoItem : Window
{
    internal JanelaDoItem(int id);
    internal int Id { get; }
    internal nint Hwnd { get; }
    internal event Action<int, EventoDePonteiro>? Ponteiro;
    internal void DefinirSprite(BitmapSource sprite);
    internal void AplicarRetangulo(RetanguloPx r);                   // SWP_NOZORDER | SWP_NOACTIVATE
    internal void ColocarAbaixoDe(nint hwndPersonagem);              // SetWindowPos(Hwnd, hwndPersonagem, …, NOMOVE|NOSIZE|NOACTIVATE)
    internal void TrazerParaFrente();                                // HWND_TOPMOST, durante o arraste do item
    internal void Capturar(); internal void SoltarCaptura(); internal bool Capturando { get; }
    internal RetanguloPx? RetanguloReal();
    // Gancho: cópia do de JanelaPersonagem (MA_NOACTIVATE, WM_GETDPISCALEDSIZE com o tamanho do item,
    // L/R down/up, MOUSEMOVE só capturando, CANCELMODE, CAPTURECHANGED).
}
```

**`Apresentacao/SpriteDoItem.cs`** (novo)

```csharp
internal enum QuadroDoItem { Normal, Achatado, Esticado }
internal static class SpriteDoItem
{
    internal static BitmapSource Renderizar(TipoDeItem tipo, QuadroDoItem quadro, int dpi); // cache (tipo, quadro, dpi)
    internal static RetanguloPx LimitesOpacos(TipoDeItem tipo, int dpi);                     // local à janela
    internal static PontoPx PontoOpaco(TipoDeItem tipo, int dpi);                            // conferido opaco
    internal static QuadroDoItem Escolher(ItemVisivel item);                                 // regra 4.5.2
    internal static string Nome(TipoDeItem tipo) => tipo.ToString().ToLowerInvariant();
}
```

**Outros arquivos novos em `Apresentacao/`:**
- **`RoteirosDeUso.cs`**: `internal static QuadroDeUso Quadro(DefinicaoDeItem def, int passo)` e o record `QuadroDeUso(PoseDoUso Pose, string Rosto, ModoDoObjeto? Objeto, bool Aceso, string? Particulas, int FaseDasParticulas, int DeslocamentoX)`. As faixas estão na tabela 4.2.
- **`ArteDoMenu.cs`**: `internal static int[] Rosto(Expressao e, int lado)` e `internal static int[] Item(TipoDeItem t, int lado)`. Devolvem BGRA de cima para baixo, com transparente = 0, e `internal static int Lado(int dpi) => 32 * (dpi >= 288 ? 3 : dpi >= 192 ? 2 : 1)`.
- **`CacheDeQuadros.cs`**: LRU por bytes, `internal sealed class CacheDeQuadros<TChave>(long orcamentoBytes)` com `TentarObter`, `Guardar(chave, BitmapSource)`, `Quantos`, `Bytes`.

**`Composicao/GerenteDosItens.cs`** (novo)

```csharp
internal sealed class GerenteDosItens
{
    internal GerenteDosItens(Func<nint> hwndDoPersonagem);
    internal event Action<int, EventoDePonteiro>? Ponteiro;
    internal void Aplicar(AtualizarItens pedido);      // cria/move/troca sprite/mostra/esconde/fecha + logs ITEM
    internal JanelaDoItem? Janela(int id);
    internal void ReordenarAbaixoDoPersonagem();
    internal void FecharTodas();                      // encerramento
    internal int Quantas { get; }
}
```

**Outros arquivos novos na raiz e no adaptador:**
- **`Composicao/Aplicacao.Itens.cs`**: `partial` de `Aplicacao`. Reúne `IniciarItens`, `AoPonteiroDoItem`, `EntregaAcertou`, `ExecutarEfeitoDeItem`, `AgendarFimDaAlteracaoNoTimer`, `CancelarTimerDaAlteracao`, `EncerrarItens` e os campos `_arbitroDosItens`, `_itemDoGesto`, `_itens`, `_fimDaAlteracao`.
- **`Plataforma/BitmapsDoMenu.cs`**: `internal sealed class BitmapsDoMenu : IDisposable` com `nint Criar(int[] bgraDeCimaParaBaixo, int lado)`, `int Criados`, `int Apagados`, `Dispose()` e `internal static int[] DeBaixoParaCima(int[] p, int lado)`.
- **`Plataforma/MenuNativo.cs`**: ganha os records `ModeloDoMenu(bool BuzzyVisivel, bool MovimentoPausado, Expressao? EmocaoDominante, int ItensNaTela, int Dpi, Func<Expressao,int,int[]> ArteDoRosto, Func<TipoDeItem,int,int[]> ArteDoItem)` e `EscolhaDoMenu(ComandoDoMenu Comando, Expressao? Emocao = null, TipoDeItem? Item = null)`.

---

## 3. Mudanças em arquivos existentes

### 3.1 Núcleo

| Arquivo → função | Mudança |
|---|---|
| `Personagem/Tipos.cs` → `enum Estado` | `Using` no fim, depois de `Peeking`: usando um item (DEC-028). |
| `Tipos.cs` → `Estados.Grupo` | `Estado.Using => GrupoDoEstado.Usuario`. O switch é exaustivo e lança no default. |
| `Tipos.cs` → `Estados.AceitaPressionar` | `\|\| estado is Estado.Reacting or Estado.Using`. |
| `Tipos.cs` → `enum Gesto` | `Cambalear, Solucar, Dancar, Tremer, Viajar` no **fim**. `DecidirParado` sorteia de `Espiar` a `Brincar`, que não muda. |
| `Personagem/EstadoDoNucleo.cs` | Novos campos, com padrão: `ItensNoMundo Itens = Vazio`, `int ProximoIdDeItem = 1`, `TipoDeItem? ItemNaMao`, `UsoEmCurso? Uso`, `AlteracaoAtiva Alteracao = Nenhuma`, `long GeracaoDaAlteracao`, `bool FimDaAlteracaoAgendado`. Calculado, sem campo: `bool AguardandoItem => Itens.AlgumArrastado`. |
| `EstadoDoNucleo.Retrato()` / `record Retrato` | Parâmetros **opcionais no fim**: `Expressao? EmocaoDominante = null`, `AlteracaoAtiva? Alteracao = null`, `UsoEmCurso? Uso = null`, `TipoDeItem? ItemNaMao = null`, `int Itens = 0`, `bool AguardandoItem = false`. |
| `Retrato.Descrever` | Acrescenta só o que não é padrão: ` emocao=X`, ` alteracao=Tipo/int/Fase`, ` uso=Item/Onde/passo`, ` naMao=Item`, ` itens=N`, ` esperandoItem=sim`. As referências 01 a 05 ficam iguais. |
| `Personagem/Eventos.cs` → `Preferencias` | 4º parâmetro opcional `Expressao? EmocaoDominante = null`. `Padrao` não muda. **É arquivo do bloco A.** |
| `Personagem/Configuracao.cs` → `ConfiguracaoDoNucleo` | `bool Itens` (padrão falso, então os testes de hoje não mudam) e `ParametrosDosItens ParametrosDosItens { get; init; } = new()`. |
| `Configuracao.cs` → `DoAplicativo` | `Itens = true`. |
| `Personagem/Maquina.cs` → classes | `public static partial class Maquina` e `private sealed partial class Passo`. |
| `Maquina.cs` → `Tratar` | Dez `case` novos, um por evento da seção 2.1. |
| `Maquina.cs` → `Carregar` | Depois de `Preferencias = Sanear(…)`: com dominante, `Expressao = dominante`. |
| `Maquina.cs` → `Sanear` | `EmocaoDominante` fora do enum vira nula. |
| `Maquina.cs` → `MudarPreferencias` | Se a dominante mudou e não é nula, `Expressao = dominante`. |
| `Maquina.cs` → `MudarTopologia` | Depois de `if (mesma) return;`, chama `ReacomodarItens(nova)`. `revalida` passa a incluir `Estado.Using`. |
| `Maquina.cs` → `Pressionar` | Antes de `IrPara(Pressed)`: `InterromperUso("PRESS"); LargarItemDaMao("PRESS");`. |
| `Maquina.cs` → `Esconder` | No caminho que não está escondido, antes de `IrPara(Hidden)`: `InterromperUso(regra); LargarItemDaMao(regra); SoltarItemEmGesto(regra);`. |
| `Maquina.cs` → `Sair` | `InterromperUso; SoltarItemEmGesto;` e depois `Itens = Vazio, ItemNaMao = null`. A raiz fecha as janelas. |
| `Maquina.cs` → `Acomodar` | Primeira linha: `if (_s.Uso is not null) InterromperUso(regra);`. Cobre tela cheia, `CMD_SHOW`, topologia e fim de reação. |
| `Maquina.cs` → `Passar` | 1. No início: `if (_cfg.Itens && _s.Estado.Visivel()) PassoDosItens();`. 2. `case Estado.Using: PassoDoUso()`. 3. Fim de `REACTING` e fim de `LANDING`: `Expressao = CaraDeRepouso()` quando há dominante. 4. Fim de `LANDING`: `ComecarUsoDoItemNaMao()`. |
| `Maquina.cs` → `Decidir` | Ao acordar de `RESTING`: `Expressao = EmocaoDominante ?? Neutro`. Em `Peeking`: a cara sai de `SortearCaraEntre(ExpressoesDoEscondido)`, com a dominante de peso 3 quando existe. |
| `Maquina.cs` → `DecidirPreso` | Mesma regra de cara com `ExpressoesDoPreso`. |
| `Maquina.cs` → `DecidirParado` | No `case Gesto`: `SortearGesto(perfil)`, que usa os gestos da alteração ou os preferidos da dominante e, sem nenhum dos dois, o código de hoje. No `case TrocarExpressao`: sem dominante, o código de hoje; com dominante, `SortearNovaExpressao()` (4.4). |
| `Maquina.cs` → `Perfil` | `Humor.Ajustar(_cfg.Perfil(energia), _s.Alteracao, _s.Preferencias.EmocaoDominante)`. |
| `Maquina.cs` → `PassoAndando`, `PassoEscalando` (só sem foguete), `PassoPresoNaParede`, `PassoPresoNoCipo`, `PassoPendurado` | A velocidade em DIP/s é multiplicada por `Ritmo` (1,0 exato sem alteração). `PassoNoAr`, `Quicar` e `TalvezFoguete` não mudam. |
| `Maquina.cs` → `Concluir` | 1. Relógio: `\|\| _s.Estado == Estado.Using \|\| (_s.Estado.Visivel() && _s.Itens.PrecisaDeRelogio(P.PassosDoAchatamento))`. 2. `querDecisao &= !_s.AguardandoItem`. 3. `janela.Add(new AtualizarItens(...))` quando os itens, a visibilidade, `Ocupados`/`ModoTelaCheia` ou a lista `_removidos` mudaram, mesmo em `EXITING` sai vazio. 4. `tempo.Add(AgendarFimDaAlteracao/CancelarFimDaAlteracao)` quando a geração ou a fase mudaram (4.3). |
| `Personagem/Gravacao.cs` → `Escrever`, `Ler` | Formatos da seção 4.8. `ItemEffectPhaseEnd` sem geração usa a atual. **É arquivo do bloco A.** |
| `Gravacao.cs` → `DescreverEfeito` | `AtualizarItens [1 Cerveja Caindo (x,y) visivel] … -2:Usado`, `AgendarFimDaAlteracao atrasoMs= geracao=`. |
| `Gravacao.cs` → `DescreverPreferencias` e `LerPreferencias` | ` emocao=X` só quando a dominante não é nula. |
| `Persistencia/EsquemaDeConfiguracoes.cs` | `CamposDasPreferencias += "emocaoDominante"`. Na escrita, o campo só sai com valor, com um dos 14 nomes em minúsculas (`neutro` a `determinado`). Na leitura, cadeia entre os 14 sem diferenciar maiúsculas; ausente ou `null` vale Automática; outra coisa vale Automática com o aviso `preferencias.emocaoDominante: não é uma das 14 expressões; vale automática`. `NormalizarPreferencias` saneia. Versão: fica 1 se, quando isto entrar, `Aplicacao.ExecutarEfeito` ainda registrar `GravarPreferencias` como `efeitoPendente` (P7 da Fase 5 ainda desligado); senão, `VersaoAtual = 2`, e o v1 é lido como válido. **É arquivo do bloco A.** |

### 3.2 `Buzzy.Visual`

| Arquivo → função | Mudança |
|---|---|
| `Pixel/Paleta.cs` → `enum Cor` e `Argb` | Trinta cores no fim (4.7.1). O `switch` de `Argb` ganha todas; hoje ele lança para cor desconhecida. |
| `Pixel/Carimbo.cs` | `Legenda` ganha `x`, `z`, `y`, `q`, `g`, `t` para os olhos novos. Novo `LegendaDosItens`. Novo construtor `Carimbo(IReadOnlyDictionary<char, Cor> legenda, params string[] linhas)`. |
| `Pixel/Rosto.cs` → `Rostos` | Carimbos `Olhos["vesgo-e","vesgo-d","chapado","pupila-pequena","espiral","coracao","caleidoscopio"]` e `Bocas["dentes"]`. Novo dicionário `DeEfeito` com dez rostos (4.7.3). Novo `public static Rosto Obter(string nome)`, que procura em `Expressoes` e depois em `DeEfeito`. `Expressoes` continua com as 14. |
| `Pixel/BonecoPixel.cs` → `PosePixel` | `bool BracosNaFrenteDaCabeca` (desenha a cabeça antes dos braços, como já faz com `Borda`) e `LadoDoBraco MaoDoObjeto = B`. |
| `BonecoPixel.Desenhar` | Assinatura `Desenhar(PosePixel pose, string? expressao = null, ObjetoNaMao? objeto = null)`. O rosto sai de `Rostos.Obter`. Com objeto: o braço que segura é desenhado por último, e antes dele o objeto, com a `Pega` sobre o centro de `MaoDoBraco(...)`. Com `BracosNaFrenteDaCabeca`, o objeto vem depois da cabeça. O `Contornar` final também contorna o objeto. |
| `BonecoPixel.Pontos(pose)` (novo) | Boca, nariz, topo da cabeça e mãos pelo `Esqueleto`. Na frente: boca = (ex, ey+7), nariz = (ex−0,5, ey+4,5). |
| `Pixel/PosesPixel.cs` → `Todas` | Catorze poses novas (4.7.4). |
| `Pixel/Tela.cs` | `Tela Deslocada(int dx, int dy)`, `Tela Recortada(int x, int y, int w, int h)` (sai do `PreviaPixel.Recortar` privado) e `void Colar(Tela outra, int x, int y)`, que copia só os pixels opacos. |

### 3.3 `Buzzy.App`

| Arquivo → função | Mudança |
|---|---|
| `Plataforma/Win32.cs` | Bloco novo "Menu com ícones", listado depois desta tabela. |
| `Plataforma/MenuNativo.cs` → `ComandoDoMenu` | `Emocao = 4`, `Item = 5`, `RecolherItens = 6`. Os IDs do Win32 ficam em `EscolhaDoMenu` (4.6). |
| `MenuNativo.Mostrar` | Nova assinatura: `EscolhaDoMenu Mostrar(PontoPx ponto, ModeloDoMenu modelo, bool abrirParaCima)`. Monta os submenus (4.6); `using var bitmaps = new BitmapsDoMenu()` fica declarado **antes** do `try`, para o `Dispose` vir depois do `DestroyMenu` do `finally`; o log ganha `MENU\|icones=\|bitmapsCriados=\|bitmapsApagados=\|lado=`. |
| `Textos.resx` e `Textos.cs` | Chaves `MenuEmocaoDominante`, `MenuEmocaoAutomatica`, `Emocao{Nome}` ×14, `MenuItens`, `Item{Nome}` ×13 e `MenuRecolherItens`; os métodos `Emocao(Expressao)` e `Item(TipoDeItem)`; e a lista `Chaves` ampliada. |
| `Apresentacao/PoseDoPersonagem.cs` → `QuadroDoSprite` | Campos opcionais: `ObjetoDoQuadro? Objeto`, `string? Particulas`, `int FaseDasParticulas`, `int DeslocamentoX`, com `readonly record struct ObjetoDoQuadro(string Item, ModoDoObjeto Modo, bool Aceso)`. |
| `PoseDoPersonagem.Escolher` | Na ordem: (a) `Using`, pela tabela 4.2 e pelo lugar; (b) `ItemNaMao` no ar: pose atual com o objeto na mão B; (c) gestos da alteração em `IDLE`; (d) rosto: uso, depois alteração (4.3.4), depois expressão; (e) partículas ambientes da alteração; (f) cambaleio (`DeslocamentoX ±1` a cada 8 passos) andando com Álcool 2 ou mais e com Zonzo. |
| `Apresentacao/SpriteProvisorio.cs` → `Renderizar(QuadroDoSprite,int)` | 1. `BonecoPixel.Desenhar(pose, rosto, objeto)`. 2. `Particulas.Desenhar(...)`, depois do contorno e antes de espelhar ou girar. 3. `Deslocada`. 4. Espelho, giro e deformação, como hoje. O `Cache` vira `CacheDeQuadros` de 16 MB, e `QuadrosEmCache` passa a ler `Quantos`. |
| `SpriteProvisorio.LimitesOpacos(QuadroDoSprite, int dpi)` (novo) | `Tela.Limites()` ampliado pela escala do DPI, local ao sprite. |
| `Composicao/Aplicacao.cs` | Hooks listados depois desta tabela. |

`Win32.cs`, bloco novo "Menu com ícones":

```csharp
internal const uint MF_POPUP = 0x10, MF_GRAYED = 0x1;
internal const uint MIIM_STATE = 0x1, MIIM_ID = 0x2, MIIM_SUBMENU = 0x4, MIIM_STRING = 0x40, MIIM_BITMAP = 0x80, MIIM_FTYPE = 0x100;
internal const uint MFT_STRING = 0, MFT_RADIOCHECK = 0x200, MFT_SEPARATOR = 0x800;
internal const uint MFS_ENABLED = 0, MFS_GRAYED = 0x3, MFS_CHECKED = 0x8;
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct MENUITEMINFO { public int cbSize; public uint fMask, fType, fState, wID; public nint hSubMenu, hbmpChecked, hbmpUnchecked, dwItemData; public string? dwTypeData; public uint cch; public nint hbmpItem; } // 80 bytes em x64
[DllImport("user32.dll", EntryPoint = "InsertMenuItemW", CharSet = CharSet.Unicode, SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)] internal static extern bool InsertMenuItem(nint hMenu, uint item, [MarshalAs(UnmanagedType.Bool)] bool porPosicao, ref MENUITEMINFO mii);
[StructLayout(LayoutKind.Sequential)]
internal struct BITMAPINFOHEADER { public int biSize, biWidth, biHeight; public ushort biPlanes, biBitCount; public uint biCompression, biSizeImage; public int biXPelsPerMeter, biYPelsPerMeter; public uint biClrUsed, biClrImportant; } // 40 bytes
internal const uint BI_RGB = 0, DIB_RGB_COLORS = 0;
[DllImport("gdi32.dll", SetLastError = true)] internal static extern nint CreateDIBSection(nint hdc, ref BITMAPINFOHEADER bmi, uint uso, out nint bits, nint secao, uint deslocamento);
[DllImport("gdi32.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool DeleteObject(nint objeto);
```

Nenhuma delas está em `ListaProibida`. `BitBlt`, `StretchBlt` e `CreateDC` continuam proibidas e não são usadas: o DIB é escrito por `Marshal.Copy`, com `hdc = 0`.

`Composicao/Aplicacao.cs`, ganchos:
1. `internal sealed partial class Aplicacao`.
2. `Iniciar`: `IniciarItens()` logo depois de criar `_personagem`.
3. `ExibirMenuDoDesktop`:
   - monta `ModeloDoMenu`: dominante e contagem vêm de `_nucleo.Estado`, e o DPI é o de `_topologia.MonitorMaisProximo(ponto).Dpi`;
   - trata `Emocao`, `Item` e `RecolherItens` com `Enviar(new CmdSetDominantEmotion(...)/CmdSummonItem/CmdClearItems, "menu")`.
4. `ProcessarFilaDoNucleo`: pula um `AtualizarItens` quando há outro depois dele no mesmo lote, como `MovimentacaoPosterior` faz para `MoverJanela`.
5. `ExecutarEfeito`: casos novos que chamam `ExecutarEfeitoDeItem`. O `default` de hoje lança exceção.
6. `EncerrarAplicacao`: `EncerrarItens()` antes de `_personagem?.Close()`.
7. `MostrarPorComando`: `_itens?.ReordenarAbaixoDoPersonagem()` depois de `ReafirmarTopo()`.
8. `RegistrarMemoria`: campos `bytesEmCache` e `janelasDeItens`.
9. `AtualizarSprite`: log `ROSTO|mostrado=|origem=` quando o rosto mostrado muda.

### 3.4 Ferramentas, testes e documentos existentes

- **`tools/Buzzy.Identidade/PreviaPixel.Gerar`**:
  - prévias novas `itens.png` (8×), `rostos-efeito.png`, `uso.png` (lugares × segurar/usar/cheirar, com cerveja e maço) e `icones-menu.png` (32 e 64 px);
  - a conferência de "NA BORDA DO QUADRO" passa a incluir as poses com objeto, com as exceções de hoje: cipó em cima e esconderijo embaixo.
- **Testes existentes a ajustar:** estão na seção 5.5.
- **`docs/ARCHITECTURE.md` 2.6** — linhas novas, para colar:

**Estados de comportamento**

| Estado | Grupo | Significado |
|---|---|---|
| `USING` | usuário | Usando o item que o usuário entregou (DEC-028): fumar, cheirar, beber, comer, engolir ou inalar, no chão, no cipó, na parede ou no esconderijo. O relógio só corre durante a animação. |

**Dimensões ortogonais**

| Dimensão | Valores | Efeito |
|---|---|---|
| Emoção dominante | Automática ou uma das 14 expressões | É a cara de base e a mais frequente nas trocas. Dá uma tendência leve aos pesos da agenda. Nunca muda física, apoio nem prioridade. É persistida (DEC-027). |
| Itens no chão | 0 a 5, cada um com monitor, posição e situação (caindo, no chão, segurado, arrastado) | Só em memória. O usuário invoca, arrasta, entrega ou remove (DEC-028). |
| Item na mão | nenhum ou um tipo | Pego no ar; é usado quando ele tem apoio de novo. |
| Efeito do item (`Alteracao`) | tipo × intensidade 1 a 3 × fase (subida, pico, rebote) | Muda a cara, a velocidade de locomoção e os pesos, intervalos e durações da agenda. Termina por disparo único. |
| Esperando item | sim ou não | Vale enquanto o usuário arrasta um item: não há decisão autônoma; andando, para; subindo ou no cipó, fica agarrado. |

**Eventos**

| Origem | Eventos |
|---|---|
| Menu | `CMD_SET_DOMINANT_EMOTION(emoção\|automática)`, `CMD_SUMMON_ITEM(tipo)`, `CMD_CLEAR_ITEMS` |
| Ponteiro sobre uma janela de item | `ITEM_PRESS(id,p)`, `ITEM_DRAG_START(id)`, `ITEM_DRAG_MOVE(id,p)`, `ITEM_DROP(id,p,sobreOPersonagem)`, `ITEM_RELEASE(id)`, `ITEM_REMOVE(id)` |
| Relógio | `ITEM_EFFECT_PHASE_END(geração)` |

**Transições principais**

| De | Evento | Para | Regra |
|---|---|---|---|
| visível, exceto `EXITING` | `CMD_SUMMON_ITEM(tipo)` | permanece | O item surge no ar ao lado dele (regra 4.5.1) e cai. No limite de 5, some o mais antigo parado. Escondido ou antes da carga, é ignorado. |
| qualquer | `CMD_CLEAR_ITEMS` | permanece | Remove os itens que não estão num gesto. |
| autônomos e físicos | `ITEM_DRAG_START` | `IDLE` se `WALKING` ou `RESTING`; senão permanece | Esperando o item: a agenda é cancelada; subindo ou no cipó, fica agarrado; pulo e queda seguem. Ao fim do arraste do item, a agenda volta depois do intervalo de acomodação. |
| `IDLE`, `WALKING`, `RESTING`, `REACTING`, `LANDING`, com apoio no chão | `ITEM_DROP(sobre=sim)` | `USING` (chão) | Para o que fazia e roda o roteiro do item; o item sai do mundo. |
| `HANGING`, `CLIMBING`, ou `REACTING` no alto | `ITEM_DROP(sobre=sim)` | `USING` (cipó ou parede) | Fica agarrado e usa com uma mão. |
| `PEEKING` | `ITEM_DROP(sobre=sim)` | `USING` (esconderijo) | Usa escondido. |
| `JUMPING`, `FALLING` | `ITEM_DROP(sobre=sim)` sem nada na mão | permanece | Pega no ar; no fim do pouso (`LANDING` → `IDLE`), começa o uso. |
| `USING`, `PRESSED`, `DRAGGING`, `HIDDEN`, `BOOTING`, `EXITING`, ou com algo na mão | `ITEM_DROP(sobre=sim)` | permanece | Recusa: o item cai de onde foi solto. |
| qualquer | `ITEM_DROP(sobre=não)`, `ITEM_RELEASE` | permanece | O item cai do ponto solto até o chão do monitor da âncora dele. |
| `USING` | `TICK` até o fim do roteiro | `SETTLING` → `IDLE`, agarrado (preso se estava, DEC-024) ou `PEEKING` (DEC-025) | Aplica o efeito do item (4.3). |
| `USING` | `PRESS`, `CMD_HIDE`, `SESSION_LOCKED`, `SUSPENDING`, `CMD_EXIT`, revalidação | conforme a linha do evento | Antes do ponto de consumo, o item volta ao mundo na mão dele; depois, o efeito vale na hora. |
| qualquer | `ITEM_EFFECT_PHASE_END(g)` da geração atual | permanece; exceção: Álcool 3, no fim do pico, parado em `IDLE` sem gesto e com a autonomia livre → `RESTING` ("apagou") | Passa para a fase seguinte e agenda o próximo disparo único. Uma geração antiga é ignorada. |
| visível | `CMD_SET_DOMINANT_EMOTION` | permanece | Grava a preferência. Sem efeito de item ativo, a cara vira a escolhida na hora. |

**Invariantes novos** (numerados de 21 em diante, porque a Fase 5 usa 18 a 20; renumere se ela usar mais):

21. Um item só é usado por `ITEM_DROP` com `sobreOPersonagem`. Nenhuma decisão autônoma invoca, pega, move, usa ou remove item.
22. Fora de um gesto do usuário, todo item está na área útil de um monitor presente e, parado, com a base no chão dela.
23. Nenhum efeito de item muda gravidade, queda máxima, quique, foguete, impulso, apoio, colisão ou a prioridade da ação direta. Ele só muda a velocidade de andar, escalar e pendurar-se (fator entre 0,4 e 2,2), a frequência, os pesos e as durações da agenda, e a cara.
24. Sem item caindo ou achatando, sem uso e sem animação, o relógio fica desligado. O fim de fase é um disparo único.
25. Itens, uso e efeitos nunca vão para o disco.
26. A emoção dominante nunca muda estado, posição, física ou prioridade.

**ARCHITECTURE, demais seções:**
- 2.7: janelas de item, árbitro próprio e entrega (D9, D10).
- 2.10: composição com o objeto, partículas e LRU.
- 2.12: `emocaoDominante`.
- 2.13.1: linha "Item: `Window` WPF 40×40 DIP, layered, sem ativação, topmost, logo abaixo do personagem na ordem Z".
- 2.13.3: `InsertMenuItemW` + `CreateDIBSection` + `DeleteObject`.

**Outros documentos:**
- **DECISIONS:** corpo completo de DEC-027 e DEC-028 (apêndice C).
- **SECURITY:**
  - 3.1: janelas de item e bitmaps do menu, só em memória própria;
  - 5: `emocaoDominante`, e itens e efeitos não gravados;
  - 7: validação da emoção.
- **IDENTIDADE_VISUAL:** paleta, seção nova "Itens", rostos de efeito, poses e partículas.
- **PRODUCT_SPEC:** escopo do MVP e prioridade (arrastar um item é ação direta).
- **TODO:** seção "Interação — emoção dominante e tamagotchi adulto", com os critérios da seção 5.

---

## 4. Regras exatas

### 4.1 Catálogo

60 passos = 1 s.

| # | Item (menu) | Modo | Alteração | Doses | Roteiro (duração/consumo) | Na boca |
|---|---|---|---|---|---|---|
| 0 | &Banana | Comer (3 mordidas) | Saciado (restauradora) | 1 | 96 / 24 | espelhado; ponta na boca |
| 1 | Á&gua | Beber | restauradora (4.3.3) | — | 108 / 30 | giro de 90° anti-horário |
| 2 | Ca&fé | Beber (xícara) | Cafeína | 1 | 108 / 30 | sem giro (borda na boca) |
| 3 | E&nergético | Beber (lata) | Cafeína | 2 | 108 / 30 | giro anti-horário |
| 4 | B&ala | Engolir (joga e pega) | Açúcar | 1 | 72 / 30 | arco (partícula) |
| 5 | &Cerveja | Beber | Álcool | 1 | 108 / 30 | giro anti-horário |
| 6 | &Vodka | Beber (gole longo + arrepio) | Álcool | 2 | 132 / 30 | giro anti-horário |
| 7 | C&igarro | Fumar (2 tragadas) | Relaxado | 1 | 156 / 42 | espelhado (filtro na boca), aceso |
| 8 | Ba&seado | Fumar (3 tragadas) | Chapado | 1 | 216 / 42 | espelhado, aceso |
| 9 | C&ocaína | Cheirar | Acelerado | 1 | 90 / 30 | em pé, no nariz |
| 10 | &MD | Engolir | Amoroso | 1 | 72 / 30 | arco |
| 11 | &Lança-perfume | Inalar | Zonzo | 1 | 60 / 24 | em pé, no nariz |
| 12 | Cog&umelo | Comer | Viajando | 1 | 96 / 24 | sem giro (morde o chapéu) |

Teclas de acesso do submenu, todas diferentes: B, G, F, N, A, C, V, I, S, O, M, L, U, e R para "&Recolher itens".

### 4.2 Roteiros de uso (apresentação; `RoteirosDeUso.Quadro`)

Passo = `Retrato.Uso.Passo`.

| Modo | Faixas de passos → pose, rosto, objeto e partículas |
|---|---|
| Comer (96) | [0,12) Segurar, empolgado. [12,24) Usar, feliz. [24,36) Segurar, feliz, migalhas. [36,48) Usar. [48,60) Segurar, migalhas. [60,72) Usar. [72,96) Parado, sem objeto, rosto da alteração. |
| Beber (108) | [0,12) Segurar, empolgado. [12,72) Usar, objeto na boca, bocejando; bolhas na cerveja e no energético. [72,84) Segurar, feliz. [84,108) Parado, sem objeto. |
| Beber, vodka (132) | Como Beber, mas: [12,96) Usar. [96,108) Segurar, assustado, `DeslocamentoX ±1` por passo (arrepio). [108,132) Parado. |
| Fumar, n tragadas (12+60n+24) | [0,12) Segurar, travesso, aceso. Tragada k: [12+60k, 42+60k) Usar, sonolento, aceso; [42+60k, 72+60k) Segurar, relaxado, aceso, fumaça na fase (passo − 42 − 60k). Fim: [12+60n, 12+60n+24) Parado, sem objeto, fumaça final. |
| Cheirar (90) | [0,18) Segurar, travesso. [18,42) Cheirar, no nariz, dormindo, pó. [42,54) Segurar sem objeto, surpreso. [54,90) Parado, acelerado, `DeslocamentoX ±1` a cada 2 passos. |
| Engolir (72) | [0,12) Segurar, travesso. [12,30) Parado, pegando, arco (fase = passo − 12, o item em partícula). [30,42) Parado, bocejando. [42,72) Parado, rosto da alteração. |
| Inalar (60) | [0,12) Segurar, travesso. [12,36) Cheirar, no nariz, dormindo, névoa. [36,60) Parado, zonzo, estrelas. |

Pose por lugar:

| Pose do uso | Chão | Cipó (mão A) | Parede (espelhada pela direção) | Esconderijo (girado pela borda) |
|---|---|---|---|---|
| Segurar | `segurar` | `cipo-segurar` | `parede-segurar` | `escondido-segurar` |
| Usar | `usar` | `cipo-usar` | `parede-usar` | `escondido-usar` |
| Cheirar | `cheirar` | `cipo-usar` | `parede-usar` | `escondido-usar` |
| Parado | `parado` | `cipo-2` | `escalando-1` | `escondido` |

### 4.3 Alterações

#### 4.3.1 Fases e durações (s)

Pico e rebote por intensidade 1/2/3; — quer dizer que a fase não existe.

| Tipo | Subida | Pico | Rebote | Rosto: subida · pico (int 1/2/3) · rebote |
|---|---|---|---|---|
| Saciado | — | 45/60/75 | — | · feliz · — |
| Refrescado | — | 30/30/30 | — | · feliz · — |
| Cafeína | — | 90/120/150 | —/30/45 | · determinado / empolgado / acelerado · sonolento |
| Açúcar | — | 45/60/75 | 20/25/30 | · empolgado · sonolento |
| Álcool | — | 90/120/150 | —/40/60 | · feliz / bebado / bebado · ressaca |
| Relaxado | — | 60/75/90 | — | · relaxado · — |
| Chapado | 5 | 150/200/240 | 45/60/75 (larica) | curioso · chapado · larica |
| Acelerado | — | 60/75/90 | 45/60/75 | · acelerado · ressaca |
| Amoroso | 10 | 150/180/210 | 60/75/90 | surpreso · apaixonado · ressaca |
| Zonzo | — | 15/20/25 | 10/12/15 | · zonzo · sonolento |
| Viajando | 15 | 180/240/300 | 30/30/30 | curioso · viajando · pensativo |

- Sem sorteio de duração: tudo é fixo e testável.
- `Seguinte` pula as fases sem duração.
- No fim da última fase, a alteração vira `Nenhuma` e `Expressao = EmocaoDominante ?? Neutro`.

#### 4.3.2 Modificadores, no pico salvo indicação

Pesos em % sobre o `PerfilDeEnergia`. Arredondamento do peso: `b == 0 || pct == 0 ? 0 : max(1, (b*pct+50)/100)`.

| Alteração | Veloc. | Interv. | Descanso | Salto | Andar | Escalar | Pular | Descansar | Gesto | TrocarExpr | Foguete | Gestos (peso) |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Saciado | 1,0 | 1,2 | 1,0 | 1,0 | 100 | 100 | 100 | 150 | 100 | 0 | 0 | Espreguicar 2, Cocar 1 |
| Refrescado | 1,1 | 0,9 | 1,0 | 1,0 | 120 | 100 | 100 | 80 | 100 | 0 | 0 | — |
| Cafeína 1 | 1,2 | 0,7 | 0,8 | 1,0 | 130 | 130 | 100 | 30 | 100 | 0 | 0 | OlharAoRedor 2 |
| Cafeína 2 | 1,4 | 0,5 | 0,7 | 1,15 | 150 | 150 | 200 | 20 | 100 | 0 | +10 | Tremer 2, OlharAoRedor 1 |
| Cafeína 3 | 1,6 | 0,4 | 0,6 | 1,3 | 170 | 170 | 250 | 0 | 100 | 0 | +20 | Tremer 3 |
| Açúcar | 1,4 | 0,5 | 0,7 | 1,3 | 200 | 150 | 300 | 0 | 150 | 0 | +10 | Brincar 3 |
| Álcool 1 | 0,95 | 1,0 | 1,0 | 0,9 | 120 | 50 | 80 | 100 | 150 | 0 | −10 | Dancar 3, Solucar 1 |
| Álcool 2 | 0,7 | 1,0 | 1,2 | 0,7 | 150 | 30 | 50 | 150 | 150 | 0 | −20 | Cambalear 3, Solucar 2 |
| Álcool 3 | 0,55 | 1,2 | 1,5 | 0,6 | 120 | 0 | 0 | 250 | 150 | 0 | −30 | Cambalear 4, Solucar 2 |
| Relaxado | 0,85 | 1,4 | 1,2 | 1,0 | 80 | 80 | 50 | 150 | 100 | 0 | 0 | Espreguicar 2 |
| Chapado | 0,6 | 1,8 | 1,5 | 0,8 | 50 | 30 | 0 | 300 | 150 | 0 | −30 | OlharAoRedor 2, Viajar 1 |
| Acelerado | 1,8 | 0,35 | 0,5 | 1,4 | 200 | 200 | 300 | 0 | 100 | 0 | +30 | Tremer 3 |
| Amoroso | 1,0 | 0,9 | 1,0 | 1,0 | 100 | 80 | 100 | 50 | 200 | 0 | 0 | Dancar 4 |
| Zonzo | 0,8 | 1,0 | 1,0 | 1,0 | 0 | 0 | 0 | 0 | 100 | 0 | −30 | Cambalear 1 |
| Viajando | 0,85 | 1,3 | 1,0 | 0,9 | 80 | 60 | 50 | 100 | 200 | 0 | 0 | Viajar 4, OlharAoRedor 1 |
| Rebote "cansado" (Cafeína, Açúcar, Zonzo, Acelerado, Amoroso, ressaca do Álcool) | 0,8 | 1,5 | 1,3 | 0,8 | 70 | 50 | 30 | 250 | 80 | 0 | −20 | Espreguicar 1 |
| Rebote "larica" (Chapado) | 0,9 | 1,0 | 1,0 | 1,0 | 120 | 50 | 50 | 100 | 100 | 0 | 0 | OlharAoRedor 2 |
| Rebote do Viajando | 0,9 | 1,2 | 1,0 | 1,0 | 100 | 100 | 100 | 100 | 100 | 0 | 0 | — |
| Subida (Chapado, Amoroso, Viajando) | 1,0 | 1,0 | 1,0 | 1,0 | 100 | 100 | 100 | 100 | 100 | 0 | 0 | — |

Como as tabelas são aplicadas:
- **Intensidade.** Nos tipos sem linha por intensidade, a velocidade vira `1 + (v−1)·{1; 1,2; 1,4}` e é presa em [0,4; 2,2].
- **Intervalo.** Multiplica `DecisaoMin/Max`, `TempoNaParede*` e `TempoPendurado*`.
- **Descanso.** Multiplica `DescansoMin/Max`.
- **Salto.** Multiplica `DistanciaDoPulo*` e `AlturaDoPulo*`.
- **Foguete.** `ChanceDoFoguete` fica presa entre 0 e 90.
- **Piso.** O piso de 3 s de `SortearAtraso` continua valendo.

Duração dos gestos da alteração, em passos:

| Gesto | Passos |
|---|---|
| Cambalear | 120–240 |
| Solucar | 30 |
| Dancar | 180–300 |
| Tremer | 60–120 |
| Viajar | 180–300 |

No fim do uso de lança-perfume, se o estado final é `IDLE`, começa `Cambalear` com `PassosDoGesto` igual à duração do pico em passos (900 na intensidade 1). Quando a alteração muda de fase, um gesto que não está na lista nova termina (`EncerrarGesto`).

#### 4.3.3 Combinação (`AplicarItem`)

- **Água:**
  - sem alteração, ou com Saciado ou Refrescado: vira Refrescado 1, pico;
  - com outra, na subida ou no pico, com intensidade maior que 1: a intensidade cai 1 e o pico recomeça;
  - com outra, na subida ou no pico, com intensidade 1, ou em rebote: vira Refrescado 1.
- **Banana:**
  - Chapado em rebote (larica): vira Saciado 1 ("curou a larica");
  - sem alteração ou com Refrescado: vira Saciado 1;
  - com Saciado: +1, com teto 3, e o pico recomeça;
  - com qualquer outra: nada muda, só a animação.
- **Substâncias:**
  - mesmo tipo: `min(3, i + Doses)`; na subida, continua na subida; senão, o pico recomeça;
  - outro tipo: substitui, com intensidade `min(3, Doses)` e `Primeira(tipo)`.
- **Toda troca de fase ou de alteração:** `GeracaoDaAlteracao++`. O `Concluir` emite `AgendarFimDaAlteracao(Duracao, geração)`; ao chegar a `Nenhuma` com um disparo pendente, emite `CancelarFimDaAlteracao`.

#### 4.3.4 Partículas ambientes do pico

A fase só anda com o relógio ligado; em repouso, o quadro fica parado.

| Alteração | Partícula |
|---|---|
| Chapado | `fumaca-lenta` |
| Acelerado, Cafeína 3 | `suor` |
| Amoroso | `coracoes` |
| Zonzo | `estrelas` |
| Viajando, Açúcar | `brilhos` |

### 4.4 Emoção dominante

| Dominante | Vizinhas | Andar | Escalar | Pular | Descansar | Gesto | Gestos preferidos (peso 3, os outros 1) |
|---|---|---|---|---|---|---|---|
| Neutro | Feliz, Curioso, Pensativo | 100 | 100 | 100 | 100 | 100 | — |
| Feliz | Rindo, Empolgado, Neutro | 100 | 100 | 100 | 100 | 100 | — |
| Rindo | Feliz, Travesso, Empolgado | 100 | 100 | 150 | 70 | 150 | Brincar |
| Curioso | Pensativo, Surpreso, Travesso | 130 | 100 | 100 | 100 | 150 | OlharAoRedor, Espiar |
| Surpreso | Curioso, Assustado, Empolgado | 100 | 130 | 100 | 80 | 100 | Espiar |
| Assustado | Surpreso, Curioso, Pensativo | 100 | 130 | 100 | 80 | 100 | Espiar |
| Sonolento | Bocejando, Entediado, Dormindo | 80 | 70 | 50 | 200 | 100 | Espreguicar |
| Bocejando | Sonolento, Entediado, Neutro | 80 | 70 | 50 | 200 | 100 | Espreguicar |
| Dormindo | Sonolento, Bocejando | 80 | 70 | 50 | 200 | 100 | Espreguicar |
| Travesso | Rindo, Curioso, Determinado | 100 | 100 | 150 | 70 | 150 | Brincar |
| Entediado | Sonolento, Pensativo, Bocejando | 80 | 70 | 50 | 200 | 100 | Espreguicar |
| Pensativo | Curioso, Neutro, Entediado | 130 | 100 | 100 | 100 | 150 | OlharAoRedor |
| Empolgado | Rindo, Feliz, Determinado | 100 | 100 | 150 | 70 | 150 | Brincar |
| Determinado | Empolgado, Travesso, Neutro | 120 | 150 | 100 | 70 | 100 | — |

- **`SortearNovaExpressao`:** `Ponderado([dominante: 6, cada vizinha: 2])`, sem a cara atual. Se a atual é a dominante, sorteia só entre as vizinhas.
- **Volta à dominante:**
  - no fim de `REACTING`;
  - no fim de `LANDING`;
  - ao acordar;
  - no fim do uso;
  - ao soltar um item que não foi entregue;
  - no fim da alteração;
  - na carga.
- **Arrastar um item:** põe `Expressao = Empolgado`.
- **Com alteração ativa:** a tendência da dominante não vale; só vale a da alteração.
- **Automática (nula):** o comportamento é idêntico ao de hoje.

### 4.5 Itens no mundo

#### 4.5.1 Surgimento (`InvocarItem`), em pixels físicos do monitor da âncora

`e = dpi/96`, `t = Tamanho.ParaPixels(dpi)`.
1. `limEsq = area.Esquerda + t.L/2`; `limDir = area.Direita − (t.L − t.L/2)`; `refX = clamp(âncora.X, limEsq, limDir)`.
2. `d = round(60e)`, `passo = round(44e)`, `lado0 = +1` se ele olha para a direita, senão −1.
3. Para `k = 0..3` e, em cada k, lado0 e depois −lado0: `x = refX + lado·(d + k·passo)`. Vence o primeiro x dentro de [limEsq, limDir] que não fica a menos de `t.L` de outro item fora de gesto no mesmo monitor.
4. Se nenhum servir: `clamp(refX + lado0·d, limEsq, limDir)`.
5. O item nasce com `Y = area.Base − round(24e)`, `VY = −420e` px/s, `Situacao = Caindo` e o próximo Id.
6. Com `Count ≥ 5`, antes de tudo remove o de menor Id que está `Caindo` ou `NoChao` (motivo `Limite`).

#### 4.5.2 Física (`PassoDosItens`, a cada `Tick`, com o personagem visível)

- `vy = min(VY + 2200e/60, 1500e)`, `Y += vy/60`. Sem velocidade horizontal. O monitor é o da chave, ou o mais próximo do pixel dos pés.
- No chão (`Y ≥ Base`):
  - se `!Quicou` e `vy ≥ 250e`: `VY = −0,35·vy`, `Quicou = true` e `PassosNoChao = 0`;
  - senão: `NoChao`, `Y = Base` e `PassosNoChao = 0`.
- `NoChao` com `PassosNoChao < 5`: conta +1 por passo.
- Do surgimento: sobe 40 DIP, cai 64 DIP, bate a cerca de 530 DIP/s, quica cerca de 8 DIP e para. São cerca de 0,7 s, uns 41 passos.
- Quadro do item:
  - `Caindo` com VY ≥ 500 DIP/s: `Esticado` (0,85 × 1,2);
  - `NoChao` com `PassosNoChao < 5`: `Achatado` (1,25 × 0,75);
  - senão: `Normal`.

#### 4.5.3 Gestos no item

- **`ITEM_PRESS`:** só com o item `Caindo` ou `NoChao`, sem outro em gesto e com o personagem visível. O item fica `Segurado` e congela no ar. `Pegada = cursor − âncora`.
- **`ITEM_DRAG_START`:** o item fica `Arrastado` e o personagem passa a esperar (regra de 2.6).
- **`ITEM_DRAG_MOVE`:** `âncora = cursor − Pegada`, sem limite. `Lugar` segue o monitor da âncora, com o tamanho do DPI dele.
- **`ITEM_DROP(sobre=não)`, `ITEM_RELEASE`:** `PrenderNaAreaUtil` no monitor da âncora, ou no mais próximo. Acima do chão, fica `Caindo` com VY 0; no chão, `NoChao` com `PassosNoChao = 0`.
- **`ITEM_DROP(sobre=sim)`:**
  - com `PodeReceber`, sai do mundo (`Usado`) e vai para `ComecarUso` ou para `ItemNaMao`;
  - sem `PodeReceber`, segue a regra do sobre=não.
- **`ITEM_REMOVE`:** remove (`PeloUsuario`). Se estava arrastado, a agenda é reagendada; se estava em gesto, `LiberarCapturaDoItem`.
- **`OndeUsar`** (`Superficies` do monitor), na ordem:
  1. esconderijo marcado → Esconderijo;
  2. `âncora.Y ≥ Chao` → Chão;
  3. `âncora.Y ≤ Teto` → Cipó;
  4. `NaLateral` → Parede;
  5. senão → no ar.
- **`InterromperUso`:**
  - com `Passo ≥ PassoDoConsumo`, aplica o efeito;
  - senão, recria o item com o mesmo tipo e **Id novo**, na mão do personagem (`Pontos(pose).MaoB` ou `MaoA`, convertido para a tela) ou aos pés dele no chão, `Caindo`.
- **`LargarItemDaMao`:** mesma regra, sempre `Caindo`.

#### 4.5.4 Visibilidade, topologia e encerramento

- **Visível** se estiver em gesto, ou se o personagem está visível e o monitor do item não está ocupado pela tela cheia com o modo ligado.
- **`TOPOLOGY_CHANGED`:** cada item fora de gesto segue `Posicionador.Reacomodar(nova, item.Posicao, Tamanho)`. Se estava `NoChao`, volta ao chão do monitor resultante; se caía, continua caindo.
- **`EXITING`:** a raiz fecha as janelas (`EncerrarItens`). Nada é gravado.

#### 4.5.5 Entrega (raiz, `EntregaAcertou`)

- Com o personagem invisível, é falso.
- `corpo = SpriteProvisorio.LimitesOpacos(_quadroAtual, _dpiDoSprite)`, deslocado para `_personagem.RetanguloReal() ?? _posicionamento.Retangulo`.
- `itemRect`: o retângulo do item com âncora `cursor − Pegada`, no DPI do monitor dessa âncora, somado a `SpriteDoItem.LimitesOpacos(tipo, dpi)`.
- `sobre = Entrega.Acertou(itemRect, corpo, round(12·dpi/96))`.
- Log: `ITEM|evento=soltar|id|sobre|corpo|item`.

### 4.6 Menu

Ordem, com os IDs de `EscolhaDoMenu`:

```
&Esconder Buzzy | &Mostrar Buzzy          (1)
&Pausar movimento | &Retomar movimento    (3)
────
Emoção &dominante ▸  (MF_POPUP)
    ◉ &Automática                          (999)   MFT_RADIOCHECK, sem ícone
    ────
    [rosto] &Neutro … Determ&inado         (1000 + (int)Expressao)  MFT_RADIOCHECK; MFS_CHECKED no atual
&Itens ▸  (MF_POPUP; MFS_GRAYED se o Buzzy está escondido)
    [ícone] &Banana Á&gua Ca&fé E&nergético B&ala ── &Cerveja &Vodka ── C&igarro Ba&seado ── C&ocaína &MD &Lança-perfume Cog&umelo
                                           (2000 + (int)TipoDeItem)
    ────
    &Recolher itens                        (2999; MFS_GRAYED sem itens)
────
&Sair                                      (2)
```

**Textos e teclas de acesso**

| Onde | Textos | Teclas |
|---|---|---|
| Menu principal | como hoje, mais "Emoção &dominante" e "&Itens" | E/M, P/R, D, I, S — todas diferentes |
| Submenu de emoções | &Automática, &Neutro, &Feliz, &Rindo, &Curioso, &Surpreso, Ass&ustado, S&onolento, &Bocejando, &Dormindo, &Travesso, &Entediado, &Pensativo, E&mpolgado, Determ&inado | todas diferentes |

**Montagem dos itens do menu**
- `InsertMenuItem` com posição explícita (um contador por menu).
- Item comum: `fMask = MIIM_ID|MIIM_STRING|MIIM_FTYPE|MIIM_STATE|MIIM_BITMAP` (sem `MIIM_TYPE`).
- Submenu: `MIIM_SUBMENU|MIIM_STRING|MIIM_FTYPE|MIIM_STATE`.
- Sem `MNS_CHECKORBMP`: com ele, a marca esconderia o rosto.
- O `DestroyMenu` do menu principal destrói os submenus.

**Ícones**
- Lado `32·k` px, com k do DPI do monitor do ponto.
- Rosto: `RetratoDoRosto` (32×32 px de arte) ampliado ×k.
- Item: `DesenhosDosItens.Icone` (16×16) ampliado ×2k.
- DIB: `biWidth = lado`, `biHeight = +lado` (de baixo para cima, como o exemplo de "Vista style menus"), 32 bpp, `BI_RGB`, `CreateDIBSection(0, …, DIB_RGB_COLORS, out bits, 0, 0)` e `Marshal.Copy` das linhas invertidas.
- Pixel: `(int)Paleta.Argb(cor)` (0xAARRGGBB = bytes B, G, R, A). Transparente vale 0, que já é pré-multiplicado, porque o alfa é sempre 0 ou 255. Nenhum GDI toca o DIB antes, então `GdiFlush` não é necessário.
- Apagar: `DeleteObject` de cada um no `Dispose`, depois do `DestroyMenu`. `MENU|bitmapsCriados=N|bitmapsApagados=N` deve dar igual.
- **Alto contraste** (`SystemParameters.HighContrast`): `hbmpItem = 0`, só texto.

**Leitor de tela:** o menu nativo expõe o nome sem `&`, o papel de submenu e os estados "marcado" (`MFS_CHECKED`) e "indisponível". Os ícones são decorativos.

**Teste manual da marca:** se o tema não mostrar a marca num item com ícone, desenhar no bitmap do atual uma moldura de 1 px em `Contorno` e manter `MFS_CHECKED`.

### 4.7 Arte

#### 4.7.1 Paleta nova

As cores entram no fim do enum `Cor`, nesta ordem:

| Cores | Hex |
|---|---|
| Amarelo, AmareloClaro, AmareloEscuro | #F6D13A, #FFF09A, #C9A227 |
| MarromEscuro | #4A2A17 |
| Vidro, VidroSombra | #CFE8EC, #8FB7BF |
| AguaAzul, AguaEscura | #4FB3E8, #2A7FB8 |
| Ambar, AmbarEscuro | #E0A030, #9C6414 |
| Papel, PapelSombra, Filtro | #F3EFE6, #C9C2B5, #D98A3D |
| Brasa, BrasaClara | #FF5A1F, #FFC53D |
| Rosa, RosaEscuro | #F27FB8, #C24D8C |
| Metal, MetalEscuro | #BFC8D2, #7F8C99 |
| AzulLata, AzulLataEscuro | #2F5BD3, #1E3C94 |
| Vermelho, VermelhoEscuro | #D8342E, #9E221E |
| Fumaca, FumacaClara | #AEB5BF, #E6E9EE |
| OlhoVermelho | #F2A0A0 |
| Coracao | #FF4F7B |
| Roxo, VerdeVivo, AzulVivo | #9B5DE5, #3FD17A, #3FA7FF |

#### 4.7.2 Legendas

- **`LegendaDosItens`:** K Contorno, W Branco, y/Y/n amarelos, m MarromEscuro, g/G vidro, a/A água, b/B âmbar, p/P papel, f Filtro, x/X brasa, v/V folha (as cores de hoje), r/R rosa, t/T metal, z/Z lata, e/E vermelho, c/s creme (as de hoje), u/U fumaça.
- **`Legenda` do rosto, acréscimos:** x OlhoVermelho, z Coracao, y Amarelo, q Roxo, g VerdeVivo, t AzulVivo.

Os carimbos estão nos apêndices A e B.

#### 4.7.3 Rostos de efeito (`Rostos.DeEfeito`)

Campos na ordem de `Rosto`: OlhoE, OlhoD, Sobrancelhas, Boca, Topete, OlhoPerfil, BocaPerfil, Corado.

| Nome | OlhoE, OlhoD | Sobrancelhas | Boca | Topete | Perfil (olho, boca) | Corado |
|---|---|---|---|---|---|---|
| bebado | vesgo-e, vesgo-d | caidas | ondulada | Caido | aberto, reta | sim |
| chapado | chapado ×2 | retas | sorriso | Caido | aberto, sorriso | não |
| acelerado | pupila-pequena ×2 | erguidas | dentes | Ericado | arregalado, reta | não |
| zonzo | espiral ×2 | preocupadas | ondulada | Ericado | arregalado, o | não |
| viajando | caleidoscopio ×2 | erguidas | sorriso | Normal | aberto, sorriso | sim |
| apaixonado | coracao ×2 | erguidas | aberta | Normal | feliz, aberta | sim |
| relaxado | feliz ×2 | neutras | sorriso | Normal | feliz, sorriso | não |
| ressaca | sonolento ×2 | preocupadas | ondulada | Caido | aberto, reta | não |
| larica | cima ×2 | erguidas | lingua | Normal | aberto, aberta | não |
| pegando | cima ×2 | erguidas | aberta | Ericado | arregalado, aberta | não |

#### 4.7.4 Poses novas

Os ângulos são os de `PosePixel`. São valores iniciais: ajuste por `uso.png`. O critério de aceitação: o objeto encosta na boca (x 28–36, y 28–31 no quadro de frente) ou no nariz sem cobrir os olhos, e nada encosta na borda do quadro.

| Pose | Base | Parâmetros |
|---|---|---|
| segurar | parado | BracoB (20,150), MaoB Fechada |
| usar | parado | BracoB (110,−94), MaoB Fechada, BracosNaFrenteDaCabeca, Cabeca −4 |
| cheirar | usar | BracoB (100,−80), mão cerca de 5 px abaixo, no queixo |
| cipo-segurar | cipo-2 | BracoA (−20,−150), MaoA Fechada, MaoDoObjeto A |
| cipo-usar | cipo-2 | BracoA (−110,94), MaoA Fechada, MaoDoObjeto A, BracosNaFrenteDaCabeca |
| parede-segurar | escalando-1 | BracoB (60,170) |
| parede-usar | escalando-1 | BracoB (120,−20) |
| escondido-segurar | escondido | BracoB (40,160) |
| escondido-usar | escondido | BracoB (110,−94) |
| cambaleando-1 | parado | Tronco 9, Cabeca 6, BracoA (−70,−40), BracoB (60,30), PernaA (−15,−5), PernaB (10,5), Cauda Alta |
| cambaleando-2 | parado | Tronco −9, Cabeca −6, BracoA (−60,−30), BracoB (70,40), Cauda Alta |
| dancando-1 | parado | QuadrilX 31, Tronco −6, BracoA (−150,−170), BracoB (40,20), PernaA (−20,10), Cauda Alta |
| dancando-2 | parado | QuadrilX 33, Tronco 6, BracoA (−40,−20), BracoB (150,170), PernaB (20,−10), Cauda Alta |
| solucando | parado | QuadrilY 47,5, BracoA (−30,−20), BracoB (30,20) |

Mapa em `IDLE`:

| Gesto | Quadro |
|---|---|
| Cambalear | `cambaleando-{1 + passos/10 % 2}` |
| Solucar | `passos % 40 < 8` ? solucando + `soluco` : parado |
| Dancar | `dancando-{1 + passos/12 % 2}` |
| Tremer | parado com `DeslocamentoX = passos/2 % 2 == 0 ? 1 : −1` |
| Viajar | olhando + `brilhos` |

#### 4.7.5 Partículas

Carimbos com `LegendaDosItens`, desenhados depois do contorno e dentro de [2, 61] px.

| Partícula | Desenho | Trajetória |
|---|---|---|
| fumaça | 3×3, 4×4, 5×5 pela idade | a cada 10 passos nasce um sopro na boca; sobe 1 px a cada 6 passos e anda +1 px a cada 10 para a direita; some com 36 passos de idade |
| pó | 3 pontos W | do topo do objeto até o nariz em 12 passos |
| névoa | 4 pontos `g` | abre em leque diante do nariz em 12 passos |
| bolhas | anel 3×3 `a` | 2 bolhas sobem da boca do objeto |
| corações | 5×4 `z` | 2 sobem dos lados da cabeça a cada 60 passos |
| estrelas | 3×3 `y` | 3 em órbita elíptica de 12×4 px sobre o chapéu; ângulo `passo·6° + k·120°` |
| brilhos | 3×3 | piscam em 4 pontos fixos ao redor da cabeça, fase `passo/8` |
| suor | 2×3 `a` | escorre pela lateral da cabeça a cada 40 passos |
| migalhas | 1 px `Y` | 2 caem da boca |
| soluço | anel `g` | sobe da boca |
| arco | carimbo da mão do item | parábola de 18 passos da mão à boca, 12 px acima |

Cache: a fase das partículas é quantizada em 8, com `FaseDasParticulas = passo/4 % 8`.

### 4.8 Formatos de gravação e logs

**Linhas de evento:**
- `CmdSetDominantEmotion emocao=Travesso|Automatica`
- `CmdSummonItem item=Cerveja`
- `CmdClearItems`
- `ItemPress id=1 x= y=`
- `ItemDragStart id=1`
- `ItemDragMove id=1 x= y=`
- `ItemDrop id=1 x= y= sobre=sim|nao`
- `ItemRelease id=1`
- `ItemRemove id=1`
- `ItemEffectPhaseEnd [geracao=N]`

**Log de diagnóstico** (só com `--diagnostico`, sem dado pessoal):

| Chave | Campos | Quando |
|---|---|---|
| `ITEM` | `evento=criado\|id\|tipo\|hwnd\|monitor\|retangulo` | ao criar a janela |
| `ITEM` | `evento=parado\|id\|retangulo\|pontoOpaco=x,y` | quando o item assenta |
| `ITEM` | `evento=soltar\|id\|sobre\|corpo\|item` | no soltar |
| `ITEM` | `evento=removido\|id\|motivo` | ao remover |
| `ITEM` | `evento=escondido` / `evento=mostrado` | ao esconder e mostrar |
| `ARRASTE_ITEM` | `fim\|movimentos\|m5P95Ms` | no fim do arraste do item |
| `ALTERACAO` | `tipo\|intensidade\|fase\|atrasoMs\|geracao` | ao agendar o fim da fase |
| `ALTERACAO` | `disparo=g` / `cancelada=sim` | no disparo e no cancelamento |
| `ROSTO` | `mostrado=bebado\|origem=alteracao` | quando o rosto mostrado muda |
| `MENU` | `fechado=Item\|argumento=Banana` | a escolha no menu; o campo `fechado` é o mesmo de hoje |
| `MENU` | `icones\|bitmapsCriados\|bitmapsApagados\|lado\|dpi` | a cada abertura |
| `MENU` | `emocaoMarcada=Feliz` | a cada abertura |

O `NUCLEO` segue igual e já registra a regra de cada transição.

---

## 5. Testes

### 5.1 `Buzzy.Core.Testes` [AUTO]

| Arquivo | Teste → cenário → o que afirma |
|---|---|
| `Itens/CatalogoDeItensTestes.cs` | `Catalogo_Tem13ItensNaOrdemDoMenu`. `Catalogo_RoteirosValidos`: 0 < consumo < duração; doses de 1 a 3. `TabelaDeAlteracoes_TodaSequenciaTerminaEmNenhumaEmAte3Fases`. `Humor_VelocidadeEntre04e22EmTodasAsCombinacoes`. `Humor_SemAlteracaoESemDominante_DevolveOMesmoPerfil`, por referência. `Humor_NuncaZeraTodosOsPesos`: com `Acoes = Todas`, algum peso > 0 (Zonzo só com Gesto). |
| `Itens/InvocarItemTestes.cs` | `Parado_ItemNasceAFrenteA60DipE24DipDoChao_Caindo`. `SemEspacoNaFrente_UsaOOutroLado`: âncora a 40 px da lateral direita. `VagaOcupada_PoeNaProxima`. `CaiQuicaUmaVezEParaEmCercaDe41Passos_ERelogioDesliga`: com `SimuladorDeTempo`, contar os passos; relógio desligado depois de `PassosNoChao = 5`. `NoLimite_TiraOMaisAntigoParado_MotivoLimite`. `EscondidoOuAntesDaCarga_Ignora`. `NaParedeNoAlto_ItemNoChaoAbaixoDele`. `NaoMudaEstadoNemLugarDoPersonagem`. `ConfigSemItens_TodosOsEventosDeItemSaoInertes`. |
| `Itens/ArrastarItemTestes.cs` | `Press_CongelaOItemCaindo`. `DragMove_SegueCursorMenosPegada_SemLimite`. `DragStart_Andando_ParaECancelaAgenda`: `CancelarDecisao`; `AguardandoItem`. `DragStart_SubindoOuNoCipo_FicaAgarrado`. `DragStart_Descansando_AcordaParaIdle`. `DropFora_CaiNoChaoDoMonitorDaAncora`. `DropNoVao_VaiParaOMonitorMaisProximo`. `Release_SemArraste_FicaOndeEstava`. `Remove_TiraEReagenda`. `FimDoArraste_AgendaComAtraso>=Acomodacao` (invariante 17). |
| `Itens/UsoTestes.cs` | `DropSobre_Parado_EntraEmUsing_RelogioLigado`. `Using_AoFimDoRoteiro_AplicaEAcomoda`: Cerveja dá `Idle` e Álcool 1 no pico. `Using_NoCipoPreso_TerminaNoCipoAindaPreso` (DEC-024). `Using_NaParede_TerminaGrudado`. `Using_NoEsconderijo_TerminaEmPeeking` (DEC-025). `DropSobre_NoAr_PegaEUsaAoFimDoPouso`. `DropSobre_DuranteUsing_Recusa_ItemCai`. `Press_AntesDoConsumo_DevolveItemComIdNovo_SemAlteracao`. `Press_DepoisDoConsumo_AlteracaoNaHora_SemItem`. `CmdHide_NoMeioDoUso_InterrompeEEscondeItens`. `TopologyChanged_NoMeioDoUso_InterrompeERevalida`. `Nucleo_DescartaAutonomyTimerEmUsing`. |
| `Itens/AlteracoesTestes.cs` | `Cerveja1_VodkaSoma2_Teto3`. `Agua_BaixaUm_NoUltimoViraRefrescado`. `Banana_CuraLarica_ForaDelaNaoMexe`. `OutroTipo_Substitui_GeracaoNova_DisparoAntigoIgnorado`. `MD_Subida10_Pico150_Rebote60_Nenhuma`: atrasos exatos dos `AgendarFimDaAlteracao`. `Alcool3_FimDoPico_ApagaSeParadoLivre`. `Bebado_PassoDeAndar0v7_QuedaIgual`: Δx por passo igual a 0,7 vezes o sóbrio; ΔY da queda idêntico. `Chapado_DescansaMaisQue2xOSobrio`: 2000 decisões com semente fixa. `Zonzo_SoCambaleiaDuranteOPico`. `FimDaAlteracao_VoltaParaDominanteOuNeutro`. `FimDeFase_SemTransicaoExcetoApagao`. |
| `Personagem/EmocaoDominanteTestes.cs` | `Escolher_TrocaACaraEGravaPreferencias`. `EscolherAMesma_NaoGrava`. `Automatica_NaoTrocaACara`. `TrocarExpressao_SoDominanteOuVizinhas_DominanteMaisDe50Porcento`: 500 decisões. `FimDaReacao_Pouso_Acordar_VoltamADominante`. `Sonolento_DescansaMais_FisicaIgual`. `ComAlteracao_AAlteracaoPrevalece`. `Carga_ComDominante_ComecaComEla`. `Sanear_ForaDoEnum_ViraAutomatica`. |
| `Itens/EntregaTestes.cs` | `Acertou_EncostaNaMargem_Sim`. `Acertou_UmPixelAlemDaMargem_Nao`. `Acertou_Vazio_Nao`. |
| `Persistencia/EsquemaDeConfiguracoesTestes.cs` | `EmocaoDominante_IdaEVolta`. `Ausente_Automatica_SemAviso`. `Invalida_AutomaticaComAviso`. `Nula_NaoEscreveOCampo`: a amostra v1 fica igual byte a byte. |
| `Personagem/ReproducaoTestes.cs` | Referências `10-emocao-dominante.txt` e `11-itens-e-efeitos.txt`, com cabeçalho `# itens: sim`. A 11 cobre invocar, cair, arrastar, entregar, usar, a fase acabar, água, remover e recolher. `EscreverELerDevolvemOMesmoEvento` ganha os dez eventos. `Preferencias_EmocaoSoApareceComValor`. |

### 5.2 `Buzzy.App.Testes`, sem janela [AUTO]

- **`ItensPixelTestes`:**
  - os 13 carimbos têm até 12×14;
  - o quadro de 20×20 tem margem ≥ 3 nas laterais e em cima;
  - alfa só 0 ou 255;
  - área opaca ≥ 24 px;
  - `NaMao` nos quatro lugares cabe no quadro de 64 sem encostar na borda (exceções de hoje);
  - nenhuma `Cor` fora do enum.
- **`RostosTestes`:**
  - `Rostos.Expressoes` tem exatamente os 14 nomes de `PoseDoPersonagem.NomeDaExpressao`;
  - todo rosto citado em 4.2, 4.3.1 e 4.7.3 existe em `Rostos.Obter`;
  - `RetratoDoRosto` devolve 32×32.
- **`ArteDoMenuTestes`:**
  - `Lado(96)=32`, `Lado(144)=32`, `Lado(192)=64`, `Lado(288)=96`;
  - os 27 conjuntos de pixels têm transparente = 0 e opaco com alfa 255;
  - `BitmapsDoMenu.DeBaixoParaCima`: a primeira linha do buffer é a última da imagem.
- **`MenuNativoTestes`:**
  - ida e volta `EscolhaDoMenu` para os IDs 1–6, 999, 1000–1013, 2000–2012 e 2999;
  - IDs desconhecidos dão `Nenhum`.
- **`PlataformaTestes`:**
  - `MENUITEMINFO` = 80 e `BITMAPINFOHEADER` = 40;
  - textos novos;
  - `TeclasDeAcessoDoMenuNaoSeRepetem` estendido ao principal com D e I, às 15 emoções e aos 14 do submenu de itens.
- **`PoseDeUsoTestes`:**
  - `Escolher` para `Using` em cada lugar e faixa da tabela 4.2 (nome da pose, objeto, aceso, partícula);
  - precedência do rosto: uso, depois alteração, depois expressão;
  - item na mão no ar.
- **`CacheDeQuadrosTestes`:** 500 quadros distintos mantêm os bytes ≤ 16 MB e expulsam o menos usado.

### 5.3 Integração, só com mensagens postadas [AUTO, com aviso ao usuário]

**Novo** `Integracao/ItensIntegracaoTestes.cs`. O `BuzzyEmTeste` ganha `PostarMouse(nint janela, int msg, nint wParam, PontoPx tela)` e `PostarChar(nint janela, char c)`, e o `NativoTeste` ganha `WM_CHAR`, `WM_KEYDOWN` e `GetGuiResources`. Todo HWND lido no log é conferido por `PidDe` antes de receber mensagem.

**Abrir o menu e escolher um item** (`AbrirMenuEEscolher`):
1. Postar `WM_RBUTTONDOWN` e `WM_RBUTTONUP` no `pontoOpaco` do personagem.
2. Esperar `MENU|exibindo`, com o dono.
3. Postar `WM_CHAR 'i'` e depois `WM_CHAR` com a letra do item ao dono.
4. Esperar `MENU|fechado=Item|argumento=…`.
5. Se o laço do menu ignorar teclado postado, o teste fica N/A com o motivo, e a cobertura fica na seção 5.4 (risco R2).

| Teste | Afirma |
|---|---|
| `Banana_PeloMenu_CaiNoChaoAoLado` | Há `ITEM\|criado`, e o `hwnd` é deste processo. Depois de `ITEM\|parado`, a base do retângulo é a da área útil. O item não intersecta o corpo opaco. O estilo estendido tem `WS_EX_NOACTIVATE`, `WS_EX_TOOLWINDOW`, `WS_EX_TOPMOST` e `WS_EX_LAYERED`. |
| `ArrastarParaOPersonagem_UsaEAplica` | Sequência DOWN, MOVE com `MK_LBUTTON` e UP no `pontoOpaco` do personagem. Log: `ITEM\|soltar\|sobre=sim`; `NUCLEO ItemDrop → Using`; `Tick Using → Settling → Idle`; `ALTERACAO\|tipo=Saciado`; `IsWindow(item) = false`. Depois, `RELOGIO\|ligado=nao`. |
| `SoltarLonge_VoltaAoChaoEEspera` | `sobre=nao`, `ITEM\|parado` de novo e nenhum `Using`. |
| `BotaoDireitoNoItem_Remove` | `ITEM\|removido\|motivo=PeloUsuario`. |
| `Esconder_EscondeItens_MostrarReexibe` | Pelo menu (`WM_CHAR 'e'`) e depois pela segunda instância: `IsWindowVisible` falso e depois verdadeiro. |
| `SairComItens_FechaTudo` | Código 0 e nenhuma janela de item viva. |
| `Menu20Vezes_GdiEstavel` | `GetGuiResources(GR_GDIOBJECTS)` antes e depois de 20 aberturas e cancelamentos (`WM_CANCELMODE`), com diferença ≤ 2. Em cada abertura, `bitmapsCriados == bitmapsApagados`. |
| `VinteItensCriadosERemovidos_UserEGdiEstaveis` | `GR_USEROBJECTS` e `GR_GDIOBJECTS` voltam ao patamar, com diferença ≤ 5. |
| `EmocaoPeloMenu_TrocaACaraEMarca` | Com `WM_CHAR 'd'` e `'f'`: `NUCLEO CmdSetDominantEmotion` e `ROSTO\|mostrado=feliz`. Na abertura seguinte, `MENU\|emocaoMarcada=Feliz`. |

### 5.4 `Buzzy.Verificacao --injetar-input-na-tela --fase itens` (input SINTÉTICO)

`Programa`: `--fase` aceita `itens`, que chama `ExecutarItens()` e grava `resultados/verificacao-itens.log`. O Buzzy abre com `--pausado --perfil-de-teste verificacao`, salvo no I4. Para abrir o menu, clique direito sintético no `pontoOpaco`; para escolher, as teclas I e a letra do item, ou D e a letra da emoção.

| Cenário | Verifica |
|---|---|
| I1: Itens ▸ Banana | Cai no chão ao lado em ≤ 1,5 s; o foco continua no receptor. |
| I2: arrastar a banana até ele | Come; Saciado; a janela do item some; sem desativar o receptor; `ARRASTE_ITEM` com p95 < 16,7 ms. |
| I3: soltar longe | O item cai de volta e espera. |
| I4: com autonomia e semente de caminhada cedo (como `EscolherSemente`) | Arrastar um item no meio da caminhada: WALKING vira IDLE, nenhuma `AGENDA` durante o arraste e agenda de novo com ≥ 3 s. |
| I5: botão direito no item | Remove. |
| I6: os 13 itens em sequência | Para cada um: `USING` com a duração da tabela 4.1 (±3 passos), `ALTERACAO` e `ROSTO` esperados e relógio desligado depois. |
| I7: cipó, parede e esconderijo | Prender no cipó (arraste sintético para o alto) e entregar uma cerveja: volta ao cipó preso. Repetir na parede e no esconderijo (clique duplo). |
| I8: Esconder pelo menu | Itens ocultos; mostrar pela segunda instância reexibe. |
| I9: sair | Fecha tudo. |
| E1: Emoção ▸ Feliz | A cara muda e fica marcada. E2: Automática. |
| M1: 20 aberturas de menu | GDI estável. |
| R1: repouso com Chapado ativo, parado, 10 s | CPU média ≤ 0,1% de um núcleo; `RELOGIO\|ligado=nao`. |
| X1: dois monitores | Arrastar um item para o outro monitor: cai no chão dele. Com um monitor, N/A. |

### 5.5 Ajustes obrigatórios em testes existentes

**`InvariantesTestes`:**
- **Configuração e gerador:**
  - `GerarSequencia`: metade das sequências com `Itens = true`, sorteada num `Random itens` próprio, como o `travessia` da Fase 5, para não mudar os outros sorteios;
  - `Sortear`: depois de cada lote, com o gerador `itens`, entra um evento de item (invocar, press, drag, drop sobre o corpo ou longe, release, remove, `ItemEffectPhaseEnd` com geração atual ou antiga) ou `CmdSetDominantEmotion`.
- **Conferências a ajustar:**
  - critério 3: `precisa |= depois.Estado == Using || (depois.Estado.Visivel() && depois.Itens.PrecisaDeRelogio(5))`;
  - a verificação R-b sem relógio em IDLE, PRESSED e DRAGGING passa a exigir também "sem item precisando de relógio";
  - agenda: `autonomiaLivre &= !depois.AguardandoItem`;
  - R11: a faixa vem de `Humor.Ajustar(cfg.Perfil(...), depois.Alteracao, depois.Preferencias.EmocaoDominante)`.
- **Invariantes novos:** conferir 21, 22, 24 e 26.
- **`CasosExigidos`:**
  - "ITEM_DROP entregue no chão/cipó/parede/esconderijo/no ar";
  - "USING interrompido antes/depois do consumo";
  - "fim de fase antigo ignorado";
  - "item quicou e assentou";
  - "invocar no limite".
- **Invariante 11:** `USING` precisa ter entrada e saída, e o gerador acima garante isso.

**Outros:**
- **`SimuladorDeTempo`:** segundo temporizador virtual para `AgendarFimDaAlteracao` e `CancelarFimDaAlteracao`, que entrega `ItemEffectPhaseEnd`.
- **`ReproducaoTestes.LerCabecalho`:** chave `itens`; lista `Referencias` com 10 e 11.
- **`GestosTestes.CliqueCliqueDuploEBotaoDireito…`:** o menu tem submenus, mas cancelar com `WM_CANCELMODE` continua igual. Conferir.

---

## 6. Riscos e interseções

### 6.1 Arquivos compartilhados com a Fase 5

| Arquivo | Quem mais mexe | Como reduzir |
|---|---|---|
| `Eventos.cs` (`Preferencias`), `Gravacao.cs`, `Persistencia/EsquemaDeConfiguracoes.cs` | bloco A (P2, P3) | Entrar **depois** do bloco A. Na persistência, só um campo novo e a versão (regra de 3.1). |
| `Maquina.cs` | P1 e P2 (`Carregar`, `Validar`), P8 (`MudarTopologia`, `Clicar`), P13a–c (`PassoAndando`, `PassoEscalando`, `PassoNoAr`, `Sinalizar`) | A lógica fica em `Maquina.Itens.cs`. No arquivo, só ganchos de uma a três linhas; a velocidade entra como fator `* Ritmo`. |
| `Tipos.cs` | P13a acrescenta valores de enum no fim | Os dois acrescentam no fim, em enums diferentes (`Estado`/`Gesto` e `SinalDeMovimento`). |
| `Configuracao.cs` (`DoAplicativo`) | P13a (`Travessia = true`) | Mescla trivial. |
| `EstadoDoNucleo.cs` / `Retrato` | P8 e P13 podem acrescentar campos | Só campos novos no fim; `Descrever` acrescenta só o que não é padrão. |
| `Aplicacao.cs` | P7 (`Iniciar`, `ExecutarEfeito`, `EncerrarAplicacao`), P9, P11, P12 | Arquivo `partial` e ganchos pontuais (3.3). |
| `Win32.cs` | P6 e P11 (bloco "Janela própria", C10) | Bloco novo separado, só acréscimos. |
| `JanelaPersonagem.cs` | P12, P14 (L1) | Não é tocado. `JanelaDoItem` copia o gancho; extrair um auxiliar comum só depois da Fase 5. |
| `BuzzyEmTeste.cs`, `Buzzy.Verificacao/Programa.cs` e `Verificacao.cs` | P5, P15 | Depois do bloco A: só acréscimos (`PostarMouse(hwnd,…)`, `PostarChar`, `--fase itens`). |
| Referências gravadas | P13a cita a "referência 06" | As nossas são a 10 e a 11. |
| Invariantes de ARCHITECTURE 2.6 | Fase 5: 18 a 20 | Os nossos são 21 a 26; renumerar no fechamento se preciso. |
| `docs/ARCHITECTURE.md` 2.6, `DECISIONS`, `SECURITY`, `TODO` | P16 (gate da Fase 5) | Seções e linhas separadas, com as DECs 027 e 028 já reservadas. |

### 6.2 Riscos técnicos

1. **R1, marca com ícone.** Num tema do Windows 11, um item marcado com `hbmpItem` pode não mostrar a marca. Verificação de tela [MANUAL]; alternativa em 4.6.
2. **R2, teclado postado ao menu.** O `WM_CHAR` postado ao dono do menu pode não ser tratado pelo laço modal. A técnica conhecida, postar `VK_DOWN` antes do `TrackPopupMenu`, indica que funciona, mas não foi medido. Alternativa: N/A na integração e cobertura pela verificação sintética.
3. **R3, janelas WPF por item.** Cada `Window` layered custa memória (estimativa de 0,5 a 2 MB) e 10 a 30 ms para criar. Com o limite de 5, medir `MEMORIA` e `conjuntoMB` no I6 e no teste `VinteItens…`.
4. **R4, cache de quadros.** Uso, partículas e objetos multiplicam os quadros; sem o LRU, M3/M7 falhariam. Um orçamento de 16 MB a 200% dá cerca de 64 quadros e mais trocas de bitmap: medir o SPRITE no I6.
5. **R5, ordem Z.** O `ReafirmarTopo` do personagem e o `Show` das janelas de item disputam a ordem Z: sempre `ReordenarAbaixoDoPersonagem` depois. No arraste do item, `TrazerParaFrente`.
6. **R6, relógio com itens.** Item caindo liga o relógio mesmo em PRESSED e DRAGGING, o que muda uma conferência de hoje (5.5). Com o personagem escondido, a física do item congela, e o relógio fica desligado em HIDDEN.
7. **R7, CPU durante animações.** Gestos longos da alteração (Zonzo cambaleia 15 a 25 s, Dancar até 5 s) mantêm o relógio ligado, com CPU de uns 3 a 4% durante a animação, sem custo em repouso. Registrar em DEC-028.
8. **R8, arte provisória.** Os carimbos e ângulos dos apêndices são rascunhos. A pega e a boca exigem ajuste visual por `uso.png`, e o critério de pronto está em 4.7.4. As animações definitivas continuam na Fase 6 (manifesto).
9. **R9, alto contraste.** Nele os ícones somem por decisão. O leitor de tela continua com nome, estado e submenu.
10. **R10, conteúdo adulto.** Só nomes e efeitos de desenho. Nenhum texto de efeito real no app, na documentação ou nos logs. O app segue privado, sem distribuição (Q-10).
11. **R11, duas capturas.** Um gesto no item e outro no personagem não coexistem, porque a captura é única. Com mensagens postadas, o árbitro dos itens cancela o anterior (`DragCancel` vira `ITEM_RELEASE` do Id antigo antes do `ITEM_PRESS` novo): o mapeamento precisa respeitar essa ordem.
12. **R12, observador de tela cheia.** Na app, ele só entra na Fase 8. A regra dos itens em monitor ocupado fica no núcleo e só é testada em [AUTO].

### 6.3 Pendências a registrar no TODO

| Pendência | Tipo |
|---|---|
| Marca e ícones no menu com tema claro, escuro e alto contraste | [MANUAL] |
| Menu e janelas de item em 150% e 200% | [HW] |
| Arrastar um item entre monitores de escalas diferentes | [HW] |
| Leitor de tela no menu (Narrador) | [MANUAL] |

---

## Apêndice A: carimbos dos itens (`LegendaDosItens`, sem contorno externo)

Todos ficam embaixo e no centro do quadro de 20×20.

```
banana (12×8)        agua (6×12)   cafe (9×7)     energetico (6×11)  bala (11×6)     cerveja (5×12)
..........mm         ..AA..        WWWWWWW..      .tTTt.             n.........n     .tt..
.........yYm         ..AA..        WmmmmmW..      tzzzzT             yn.rrrrr.ny     .bB..
........yyn.         .gWgg.        WWWWWWWss      tzzyzT             yyrWrrrrRyy     .bB..
.......yyYn.         gWggGG        WWWWWWW.s      tzyyzT             yyrrWrrrRyy     bbbBB
......yyyn..         gWaaaG        sWWWWWWss      tyyyzT             yn.rRRRR.ny     bWbbB
m...yyyyYn..         gaAaaG        .sWWWWs..      tzyyzT             n.........n     ppppP
nyyyyyyYnn..         gWaaaG        ..sssss..      tzyzzT                             peeeP
.nnnnnnn....         gWggGG                       tzzzzT                             ppppP
                     gWggGG                       tzzzzT                             bWbbB
                     gWggGG                       tzzzzT                             bWbbB
                     gWggGG                       .TTTT.                             bbbBB
                     .GGGG.                                                          .BBB.

vodka (6×14)   cigarro/maço (8×10)  cigarro na mão (7×2; aceso: última coluna X/x)  baseado (11×4; aceso: última coluna X/x)
..ee..         ..ff....             ffppppp                                          ......pppv.
..ee..         ..pp....             ffPPPPP                                          fpppppvppvp
..gG..         eeppeeeE                                                              fPPPPPPpvPp
..gG..         eeeeeeeE                                                              ......PPPP.
.ggGG.         WWWWWWWP
gWgggG         WeeeeWWP
gWzzzG         WWWWWWWP
gzWWzG         WWWWWWWP
gzzzzG         WWWWWWWP
gWgggG         PPPPPPPP
gWgggG
gWgggG
ggGGGG
.GGGG.

cocaina (8×9)  md chão (8×8)  md mão (4×4)  lancaperfume (6×12)  cogumelo (10×9)   bala mão (5×3)
eeeeeeee       ..rrrr..       .rr.          ..TT..               ..eeeeee..        nrrrn
gGGGGGGg       .rWrrrr.       rWrr          ..tT..               .eWeeeWee.        yrWry
gWWWWWWG       rWrRrRrr       rrRR          .tttT.               eeeeWeeeeE        nRRRn
gWWWWWWG       rrRRRRRr       .RR.          ttttTT               eWeeeeeWeE
gWWWWWWG       rrRRRRRr                     tzzzZT               EEEEEEEEEE
gWWWWWWG       rrrRRRrr                     tzWzZT               ...cccs...
gWWWWWWG       .rrrRrr.                     tzzzZT               ...cccs...
ggWWWWGG       ..RRRR..                     tzWzZT               ...cccs...
.gGGGGG.                                    tzzzZT               ..sssss...
                                            ttttTT
                                            ttttTT
                                            .TTTT.
```

Pontos de pega iniciais, na coluna e linha do carimbo em pé:

| Item | Pega |
|---|---|
| Garrafas, latas, lança-perfume e saquinho | centro da última linha |
| Xícara | alça, (8,3) |
| Banana | (1,6) |
| Cigarro e baseado | (1,1) |
| Cogumelo | centro do pé |

"Na boca" segue a coluna da tabela 4.1.

## Apêndice B: carimbos novos do rosto (7×8, `Legenda` do rosto)

```
vesgo-e   vesgo-d   chapado   pupila-pequena  espiral   coracao   caleidoscopio   boca "dentes" (9×4)
.KKKKK.   .KKKKK.   .......   .KKKKK.         .KKKKK.   .KKKKK.   .KKKKK.         .........
KKWWWKK   KKWWWKK   .......   KWWWWWK         KWWWWWK   KWWWWWK   KKWWWKK         .KKKKKKK.
KWWWiiK   KiiWWWK   .KKKKK.   KWWiWWK         KWKKKKK   KzzWzzK   KWyqgWK         .KWKWKWK.
KWWiuuK   KuuiWWK   KpppppK   KWiuiWK         KWKWWWK   KzzzzzK   KWtuyWK         .KKKKKKK.
KWWiuuK   KuuiWWK   KpppppK   KWWiWWK         KWKWKWK   KzzzzzK   KWguqWK
KWWWiIK   KIiWWWK   KxiuixK   KWWWWWK         KWKKKWK   KWzzzWK   KWqytWK
KWWWWWK   KWWWWWK   KxxiIxK   KWWWWWK         KWWWWWK   KWWzWWK   KWWWWWK
.KKKKK.   .KKKKK.   .KKKKK.   .KKKKK.         .KKKKK.   .KKKKK.   .KKKKK.
```

## Apêndice C: corpo para DEC-027 e DEC-028

Substitui o parágrafo "desenho técnico em andamento".

### DEC-027

**Decisão:**
- `Preferencias.EmocaoDominante` (`Expressao?`, com nula = Automática), mudada pelo menu (`CMD_SET_DOMINANT_EMOTION`) e persistida em `preferencias.emocaoDominante`;
- é a cara de base: a mais frequente nas trocas (peso 6 contra 2 das vizinhas) e a cara de volta na carga e no fim de reação, pouso, descanso, uso e efeito;
- dá uma tendência leve nos pesos da agenda (tabela 4.4);
- os efeitos de itens têm precedência enquanto duram;
- o submenu mostra as 14 expressões com o rosto gerado do mesmo desenho de `expressoes.png`, mais "Automática", com a atual marcada.

**Alternativas:** trocar a cara uma vez; travar a cara; só em memória; novos valores em `Expressao`.

**Motivo:** o pedido do usuário, com as regras de determinismo, os invariantes 6 e 12 e as referências gravadas intactas.

**Trade-offs:** menu mais alto (cerca de 540 px a 100%); ícones fora em alto contraste.

**Consequências:** ARCHITECTURE 2.6 e 2.12; SECURITY 5 e 7; testes das seções 5.1 a 5.4.

### DEC-028

**Decisão:**
- **Itens:** 13 itens em pixel art; cada um surge no ar ao lado dele, cai com um quique e espera no chão; no máximo 5.
- **Janelas:** uma janela própria por item, sem ativação.
- **Arraste e entrega:** arrastar faz ele esperar. Soltar sobre ele (retângulo opaco + 12 DIP) inicia `USING`, com o roteiro do modo (fumar, cheirar, beber, comer, engolir, inalar), no chão, no cipó, na parede ou no esconderijo. No ar, ele pega e usa ao pousar.
- **Remover e recolher:** botão direito no item remove; o menu recolhe todos.
- **Efeitos:** uma alteração por vez, com tipo, intensidade 1 a 3 e fases que terminam por disparo único (tabelas 4.3). Água e banana restauram. Os efeitos mudam só a locomoção (entre 0,4 e 2,2), a agenda e a cara (invariantes 21 a 26).
- **Persistência:** nada de itens ou efeitos vai para o disco.

**Alternativas:** as de D1 a D14.

**Motivo:** o pedido do usuário, com as regras duras (usuário prevalece, sem timer periódico, núcleo determinístico, sem leitura de outros apps).

**Trade-offs:**
- relógio ligado durante animações de uso e gestos de efeito;
- uma janela WPF por item;
- arte provisória até a Fase 6.

**Consequências:**
- ARCHITECTURE 2.6, 2.7, 2.10 e 2.13;
- SECURITY 3.1 e 5;
- IDENTIDADE_VISUAL: paleta, itens, rostos, poses e partículas;
- TODO: seção "Interação";
- testes das seções 5.1 a 5.5.
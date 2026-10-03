> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de o desenho da arte em pixel art do tamagotchi (o código o cita como "desenho da arte", seções 4.8 e 4.9), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a emoção dominante e o tamagotchi. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-027 e DEC-028) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

# Desenho da arte: itens, poses de uso, caras novas, efeitos e ícones do menu (DEC-027 e DEC-028)

Modo só leitura: nada foi editado, compilado ou executado. Tudo abaixo foi conferido contra o código real de `src/Buzzy.Visual/Pixel/` (`BonecoPixel`, `PosesPixel`, `Rostos`, `Paleta`, `Tela`, `Carimbo`, `Icone`), `tools/Buzzy.Identidade/PreviaPixel.cs`, `src/Buzzy.App/Apresentacao/{PoseDoPersonagem,SpriteProvisorio}.cs`, `MenuNativo.cs`, `Regras.cs` do portão e os documentos.

**O que já existe.** DEC-027 e DEC-028 já estão em `docs/DECISIONS.md`, só com os princípios; este desenho detalha as duas e não cria número novo. O `docs/TODO.md` ainda **não** tem a seção "Interação".

Os ângulos dos braços e as mãos resultantes foram calculados pela fórmula de `BonecoPixel.MaoDoBraco`: ombro B em (37,5; 33,7) e ombro A em (26,5; 33,7) na pose `parado`. **A arte em texto não foi renderizada**, porque nada podia ser executado. O primeiro passo da implementação é gerar as prévias e retocar os carimbos.

---

## 1. Decisões

**A1. Itens em grade de 24×24, na mesma densidade do personagem.**
- 1 pixel de arte = 2 DIP. A janela do item tem 48×48 DIP e é ampliada 2×, 3× e 4× a 100%, 150% e 200%.
- A janela é quadrada. A âncora fica no centro da base.
- O contorno de baixo do item fica na última linha: ele "pousa" como os pés. Nada encosta no topo nem nas laterais: há 1 px de folga depois do contorno.
- Alfa só 0 ou 255.
- *Motivo:* o item no chão e o item na mão ficam coerentes com o boneco, e 48 DIP é um alvo confortável para arrastar.
- *Descartadas:* 32×32, grande demais ao lado de um personagem de cerca de 60 px; 16×16, pequeno demais para agarrar e ler.

**A2. Dois desenhos por item: "chão" e "mão".**
- O desenho do chão também é o ícone do menu. O da mão é um carimbo pequeno, desenhado em pé, com a ponta da boca para cima.
- Nas poses "na boca", o carimbo da mão gira 90° no sentido anti-horário, sem perda, com o novo `Carimbo.Girado`. A ponta da boca vai para a esquerda, em direção à boca.
- Não há rotação arbitrária.
- *Motivo:* reduzir um desenho de 24 px para 8 px apaga detalhes, e girar pixel art em ângulos quaisquer serrilha.
- *Descartadas:* item geométrico rasterizado em qualquer ângulo, que fica feio em 3–8 px; um carimbo por ângulo, que é caro de manter.

**A3. Seis verbos, com poses compartilhadas.**
- Os verbos são comer, beber, fumar, cheirar, engolir e inalar, cada um com 3 ou 4 quadros, todos de frente.
- O item é colocado na **pega**, um ponto do carimbo, sobre o centro da mão calculado pelo mesmo esqueleto (`MaoDoBraco`). Não há pose por item.
- O desenho vira em `Direcao.Esquerda` com `Tela.Espelhada`, como as outras poses.

**A4. Ordem de desenho.**
- O item é desenhado **antes** da mão que o segura: a palma cobre a pega.
- Duas flags novas na pose, `BracoANaFrente` e `BracoBNaFrente`, desenham o braço depois da cabeça. Hoje isso só acontece com `Borda`.
- O item ganha uma linha interna de `Contorno` onde encosta no corpo, pelo novo parâmetro `linhaInterna` de `Tela.Carimbar`.

**A5. Caras novas.**
- Sete caras de efeito entram também no `Expressao` do núcleo: `chapado`, `bebado`, `tonto`, `viajando`, `acelerado`, `apaixonado` e `enjoado`.
- Sete caras passageiras existem **só** em `Rostos`, porque são usadas pelas poses de uso: `mordendo`, `mastigando`, `engolindo`, `tragando`, `soltando`, `fungando` e `tossindo`.
- O chapéu reage pelo `Topete`. O novo `Topete.Torto` é o chapéu girado −12°, para o bêbado.
- *Descartado:* chapéu "flutuando" mais alto que o `Ericado`. O `Ericado` já leva o contorno do topo à linha 1, e subir mais encostaria na borda.

**A6. Efeitos visuais como sobreposição.**
- Há 8 efeitos, desenhados em `BonecoPixel` antes do `Contornar`, com 3 fases cada.
- A posição sai do centro da cabeça e da boca do esqueleto e nunca cobre olhos nem boca.
- Parado, sem relógio, vale sempre a fase 0, estática (DEC-011).

**A7. Modificadores de pose por efeito.**
- O bêbado balança o tronco, o chapado abaixa a cabeça, o tonto gira a cabeça e assim por diante.
- São feitos com `pose with {...}` sobre as poses existentes, sem pose nova por efeito.

**A8. Ícones do menu.**
- Rosto: recorte de 32×32 do `parado` desenhado com a expressão, na janela (16, 2), com o contorno refeito no anel da borda. É "gerado a partir do próprio desenho", como diz o DEC-027, e é o mesmo recorte de `expressoes.png`.
- Item: o próprio desenho do chão, 24×24.
- Ampliação inteira `max(1, dpi / 96)` por vizinho mais próximo, sem suavização.
- *Descartados:* reduzir o sprite a 16 px, que apaga os olhos (é o motivo de `Icone` ter desenho próprio); 36×34 com a aba inteira, que deixa o menu alto demais.

**A9. Paleta e legenda.**
- 35 cores novas são **acrescentadas ao fim** de `Cor`, sem mudar os bytes atuais.
- Uma legenda única em `Carimbo.Legenda` recebe os caracteres novos, sem colisão com os existentes.

**A10. Nada de texto na arte (Q-12) e nada de marca.**
- O rótulo da vodka é uma faixa sem letras. A lata de energético é genérica.
- Não há folha de maconha, só a forma do cigarro enrolado e pontinhos verdes.
- Nenhuma informação real (dose, preparo, obtenção).

**A11. Prévias e conferência.**
- `tools/Buzzy.Identidade` passa a gerar as prévias de itens, usos, efeitos e ícones.
- A saída 1 passa a valer também para item, quadro de uso ou efeito que encoste na borda.

---

## 2. Tipos e arquivos novos

Raiz: `C:\Users\Cliente\Documents\claudio\`.

**`src\Buzzy.Visual\Pixel\ItensPixel.cs`**
```csharp
public enum Verbo { Comer, Beber, Fumar, Cheirar, Engolir, Inalar }

/// <summary>Carimbo do item na mão, em pé (ponta da boca para cima). Pega = onde fica o centro da mão.</summary>
public sealed record ItemNaMao(Carimbo Desenho, int PegaX, int PegaY, int BocaX, int BocaY)
{
    /// <summary>Girado 90° anti-horário, sem perda: (x, y) → (y, Largura − 1 − x); a boca vai para a esquerda.</summary>
    public ItemNaMao Girado();
}

public static class ItensPixel
{
    public const int Lado = 24;                    // grade do chão e do ícone
    public const double TamanhoLogicoDip = 48;     // 1 px = 2 DIP
    public static readonly IReadOnlyList<string> Todos;   // ordem do menu: banana, agua, vodka, cerveja, baseado,
                                                          // cigarro, cocaina, md, lanca, cafe, energetico, cogumelo, bala
    public static Verbo VerboDe(string item);
    public static Tela Desenhar(string item);      // 24×24: carimbo colocado em x=(24−L)/2, y=23−A, depois Contornar(Contorno)
    public static ItemNaMao NaMao(string item, string variante = "normal");   // variantes na seção 4.4
    public static ((int X, int Y) Opaco, (int X, int Y) Transparente) PontosDeTeste(string item); // em px de arte
}
```

**`src\Buzzy.Visual\Pixel\EfeitosPixel.cs`**
```csharp
public enum EfeitoVisual { Nenhum, Fumaca, Bolhas, Brilhos, Estrelinhas, Coracoes, Cores, Poeira, Borrifo }

public static class EfeitosPixel
{
    public const int Fases = 3;
    internal static void Desenhar(Tela tela, (double X, double Y) cabeca, (double X, double Y) boca, EfeitoVisual efeito, int fase);
    /// <summary>Modificador de pose do efeito (seção 4.9), sem pose nova.</summary>
    public static PosePixel Modificar(PosePixel pose, EfeitoVisual efeito, int fase);
}
```

**`src\Buzzy.Visual\Pixel\IconesDoMenu.cs`**
```csharp
public static class IconesDoMenu
{
    public const int LadoDoRosto = 32, LadoDoItem = 24;
    public static Tela Rosto(string expressao);    // qualquer chave de Rostos.Expressoes
    public static Tela Item(string item);          // == ItensPixel.Desenhar(item)
    public static int Fator(int dpi) => Math.Max(1, dpi / 96);   // 96–191 → 1×, 192–287 → 2×, 288 → 3×
    /// <summary>Pixels ampliados por vizinho mais próximo, 0xAARRGGBB (BGRA na memória). Com alfa 0/255, pré-multiplicado = direto.</summary>
    public static uint[] Ampliar(Tela t, int fator);
}
```

**`src\Buzzy.Visual\Pixel\BonecoPixel.cs`**: tipos novos no mesmo arquivo.
```csharp
public enum Segura { Nada, MaoB, MaoBGirado, DuasMaos, Inalar }
public readonly record struct PontosDoEsqueleto((double X, double Y) MaoA, (double X, double Y) MaoB,
    (double X, double Y) Cabeca, (double X, double Y) Boca, (double X, double Y) Nariz);
```
- Boca = (cx − 1, cy + 7,5).
- Nariz = (cx − 0,5, cy + 4,5).
- Mãos por `MaoDoBraco`.

**Testes:** arquivos novos em `tests\Buzzy.App.Testes\`, descritos na seção 5.
- `ItensPixelTestes.cs`
- `UsosPixelTestes.cs`
- `RostosNovosTestes.cs`
- `EfeitosPixelTestes.cs`
- `IconesDoMenuTestes.cs`

---

## 3. Mudanças em arquivos existentes

| Arquivo | Função ou tipo | O quê |
|---|---|---|
| `src\Buzzy.Visual\Pixel\Paleta.cs` | `Cor`, `Paleta.Argb` | +35 cores no fim, na ordem da seção 4.1 |
| `src\Buzzy.Visual\Pixel\Carimbo.cs` | `Legenda` | +35 caracteres (seção 4.1) |
| `src\Buzzy.Visual\Pixel\Carimbo.cs` | novos membros | `public Carimbo Girado(bool horario)`, com a semântica de `Tela.Girada`, e um construtor privado `(int l, int a, Cor?[] px)` |
| `src\Buzzy.Visual\Pixel\Tela.cs` | `Carimbar` | parâmetro `Cor? linhaInterna = null`: antes de carimbar, pinta de `linhaInterna` os vizinhos (vizinhança de 4) opacos que não pertencem ao carimbo, como em `Pintar` |
| `src\Buzzy.Visual\Pixel\Rosto.cs` | `Rosto` | parâmetros novos `Cor? Rubor = null` e `bool RuborGrande = false` |
| `src\Buzzy.Visual\Pixel\Rosto.cs` | `Topete` | `+ Torto` |
| `src\Buzzy.Visual\Pixel\Rosto.cs` | `Rostos` | carimbos e expressões da seção 4.6; lista nova `public static readonly IReadOnlyList<string> OpcoesDeEmocao` (seção 4.10) |
| `src\Buzzy.Visual\Pixel\BonecoPixel.cs` | `PosePixel` | `Segura Segura = Nada`, `string VarianteDoItem = "normal"`, `bool BracoANaFrente`, `bool BracoBNaFrente`, `EfeitoVisual EfeitoDaPose = Nenhum`, `int FaseDoEfeito = 0` |
| `src\Buzzy.Visual\Pixel\BonecoPixel.cs` | `Desenhar` | passa a ser `Desenhar(PosePixel pose, string? expressao = null, string? item = null, EfeitoVisual efeito = Nenhum, int fase = 0)`; ordem abaixo da tabela |
| `src\Buzzy.Visual\Pixel\BonecoPixel.cs` | `Pontos(PosePixel)` | novo, público |
| `src\Buzzy.Visual\Pixel\BonecoPixel.cs` | `DesenharCabecaDeFrente` | rubor pela seção 4.6 |
| `src\Buzzy.Visual\Pixel\BonecoPixel.cs` | `DesenharChapeu` | caso `Torto` (seção 4.7) |
| `src\Buzzy.Visual\Pixel\PosesPixel.cs` | `Todas` | +20 poses de uso (seção 4.5), com `Estado = "uso: <verbo>"`, depois de `escalando-2` |
| `tools\Buzzy.Identidade\PreviaPixel.cs` | `Gerar` | conferência e prévias novas (seção 5) |
| `tools\Buzzy.Identidade\Programa.cs` | `Main` | mensagem e código de saída somam os problemas novos |
| `docs\IDENTIDADE_VISUAL.md` | seções 3 a 10 | lista abaixo desta tabela |
| `docs\ARCHITECTURE.md` | seção 2.10 | bullets novos (lista abaixo) |
| `docs\ARCHITECTURE.md` | seção 2.6 | **nenhuma linha nova desta área**. Estados e eventos de uso são da área do núcleo. |
| `docs\DECISIONS.md` | DEC-027 | acrescenta a regra do ícone (A8) |
| `docs\DECISIONS.md` | DEC-028 | acrescenta "Desenho da arte" (A1–A7, A9–A11) |

**Nova ordem de `BonecoPixel.Desenhar`.** O cipó continua antes de tudo.
1. De frente: cauda, pernas e tronco; depois os braços com `BracoXNaFrente = false`, e o item de cada um logo antes da sua mão; depois a cabeça; depois os braços com `BracoXNaFrente = true`, cada um com o seu item antes; depois o efeito da pose e o efeito pedido.
2. Borda, se houver.
3. `Contornar`.

**Quem segura o quê.**
- Com `Segura.DuasMaos`, o espelho é desenhado depois da cabeça e antes dos dois braços.
- Com `Segura.Inalar`, o frasco é o item da mão A e o lenço o da mão B.
- `item == null` desenha a pose sem item, para as prévias.

**`docs\IDENTIDADE_VISUAL.md`:**
- 3: tabela da paleta com as 35 cores, agrupadas.
- 4: regras dos itens.
- 5: regra 4 ganha "o item pousa na última linha, como os pés; efeitos ficam dentro do quadro".
- 6: +14 caras.
- 7: +20 poses de uso.
- 7a (nova): Itens.
- 9: arquivos novos.
- 10: histórico, 2026-09-30.

**`docs\ARCHITECTURE.md` 2.10, bullets novos:**
- itens de 24 px na mesma densidade do boneco;
- poses de uso por verbo, com o item na pega da mão;
- sobreposições de efeito em 3 fases, com a fase 0 parada;
- ícones do menu gerados do desenho.

**Fora da arte, mas dependente dela** (seção 6): `QuadroDoSprite` ganha `Item`, `Efeito` e `Fase`, `SpriteProvisorio.Renderizar` repassa os três, e `PoseDoPersonagem` escolhe os quadros de uso.

---

## 4. Regras exatas

### 4.1 Paleta: acréscimos ao fim de `Cor`, na ordem, com o caractere da legenda

| Cor | Hex | Car. | Cor | Hex | Car. |
|---|---|---|---|---|---|
| Banana | `#F7D548` | `y` | Cerveja | `#F2A93B` | `B` |
| BananaClara | `#FFF08A` | `Y` | CervejaEscura | `#C77B1E` | `D` |
| BananaEscura | `#C9A227` | `n` | Cafe | `#5B3A21` | `k` |
| Agua | `#5EC8F2` | `a` | Neon | `#B6F23A` | `N` |
| AguaClara | `#BDEBFF` | `A` | NeonEscuro | `#6FA81E` | `P` |
| AguaEscura | `#2E8FC7` | `d` | Rosa | `#FF6FB5` | `R` |
| Vidro | `#DDF3FA` | `g` | RosaEscura | `#D2458C` | `S` |
| VidroSombra | `#A2CCDA` | `G` | Lilas | `#B889F2` | `V` |
| Metal | `#B9C2CC` | `m` | LilasEscuro | `#7F58C4` | `X` |
| MetalClaro | `#E6EBF0` | `M` | Coracao | `#E8344E` | `1` |
| MetalEscuro | `#7D8794` | `E` | Estrela | `#FFE45C` | `2` |
| Papel | `#F4F1E8` | `w` | EstrelaEscura | `#E0A526` | `3` |
| PapelSombra | `#CFC8B6` | `x` | EscleraVermelha | `#F6B8AE` | `5` |
| Filtro | `#E59A4C` | `t` | OlhoVermelho | `#D9534F` | `6` |
| FiltroEscuro | `#B86B2A` | `T` | Enjoo | `#9BCB6B` | `7` |
| Brasa | `#FF6A2B` | `q` | BochechaForte | `#EE7F72` | `8` |
| BrasaClara | `#FFD24A` | `Q` | Cinza | `#8A8A8A` | `z` |
| Fumaca | `#D9DCE0` | `Z` | | | |

A legenda também ganha `J` → `Cor.Cipo` e `L` → `Cor.CipoEscuro`, que são cores existentes.

### 4.2 Regras comuns dos carimbos de item e de efeito

- Os carimbos são **só o preenchimento, sem contorno externo**. O contorno de 1 px vem do `Contornar` final, do item ou do sprite.
- Luz de cima e da esquerda: realce no canto superior esquerdo, sombra embaixo e à direita.
- `.` = transparente.

### 4.3 Itens no chão (grade 24×24)

Colocação: `x = (24 − L) / 2`, `y = 23 − A`, e depois `Contornar`. Todos têm L ≤ 20 e A ≤ 21, o que garante a folga.

| Item | Tamanho | Silhueta e cores | Verbo |
|---|---|---|---|
| banana | 18×8 | crescente amarelo, ponta escura, cabinho | Comer |
| agua | 8×16 | garrafinha PET de tampa azul e rótulo branco com gota | Beber |
| vodka | 8×19 | garrafa alta de vidro, tampa prata, rótulo vermelho sem letras | Beber |
| cerveja | 16×16 | caneca de chope com espuma e alça em D | Beber |
| baseado | 17×5 | cone de papel com torção e pontinhos verdes | Fumar |
| cigarro | 16×4 | reto, filtro laranja, cinza na ponta | Fumar |
| cocaina | 14×6 | espelhinho em 3/4 com moldura prata e carreira branca | Cheirar |
| md | 10×7 | comprimido lilás com coração branco | Engolir |
| lanca | 16×15 | frasco fino com válvula de metal e lenço listrado ao lado | Inalar |
| cafe | 16×9 | xícara branca com café e pires | Beber |
| energetico | 8×14 | lata prata com raio verde-neon | Beber |
| cogumelo | 14×13 | chapéu vermelho de bolinhas brancas, pé creme | Engolir |
| bala | 16×7 | bala embrulhada em gravata-borboleta rosa com listras | Engolir |

```text
banana (18×8)          agua (8×16)   vodka (8×19)   cerveja (16×16)
................kk     ..dddd..      ...mm...       ..WWW.WW........
...............nyk     ..dAdd..      ...Mm...       .WWWWWWWWW......
k.............nyy.     ...gg...      ...mE...       WWCWWWWWWWx.....
kyY..........nyyn.     ..gAAg..      ...gG...       WWWWWWWWWWx.....
.yYYY......nyyyn..     .gAaaaG.      ...gG...       gYBBBBBBBDGggg..
.nyyYYYYyyyyyyn...     gAWaaaaG      ...gG...       gYBBBBBBBDG..gg.
..nnyyyyyyyynn....     gWaaaaaG      ..gWgG..       gYBBBBBBBDG...G.
....nnnnnnnn......     WWWWWWWG      .gWggGG.       gYBBBBBBBDG...G.
                       WWWaWWWG      gWggggGG       gYBBBBBBBDG...G.
                       WWaaaWWG      gWgggggG       gYBBBBBBBDG..GG.
                       gaaaaaaG      ffffffFF       gYBBBBBBBDGGGG..
                       gAaaaaaG      fWWWWWfF       gYBBBBBBBDG.....
                       gaaaaaaG      ffWffffF       gBBBBBBBBDG.....
                       gaaaaaaG      ffffffFF       gDDDDDDDDDG.....
                       gaaaaadG      gWgggggG       ggggggggggG.....
                       .GGGGGG.      gWgggggG       .GGGGGGGGG......
                                     gggggggG
                                     gggggggG
                                     .GGGGGG.

baseado (17×5)         cigarro (16×4)      cocaina (14×6)    md (10×7)
..........wwwww..      tTtwwwwwwwwwwwwz    ...MMMMMMMMMMm    ...VVVV...
.....wwwwwwwwwwJ.      ttTwwwwwwwwwwwwz    ..MAAAAAAAWAAE    .VVVWVWVV.
wwwwwwwwwwwwwwJJw      tTtwwwwwwwwwwwzq    .MAaWWWWWWaaE.    VVVWWWWWVV
.xxxxxxxxxxxxxJx.      TTTxxxxxxxxxxxzz    .MaaGGGGGGaaE.    VVVVWWWVVV
........xxxxxxx..                          MaaaaaaaaaaE..    XVVVVWVVVX
                                           mEEEEEEEEEEE..    .XXVVVVXX.
                                                             ...XXXX...

lanca (16×15)          cafe (16×9)         energetico (8×14)  cogumelo (14×13)   bala (16×7)
.EmE............       ..WWWWWWWWW.....    .MMmmmE.           ....ffffff....     RR....RRRR....RR
..mm............       ..WkkkkkkkW.....    MmmmmmmE           ..ffWWffffff..     RRR..RWRRRR..RRR
.mMmE...........       ..WWWWWWWWxWW...    MMmmmmEE           .fWWWWffffWff.     RRRRRWRRWRRRRRRS
.mmmE...........       ..WWWWWWWWx..W..    MmmmNNmE           ffWWfffffWWWfF     RRRRWRRWRRWRRRSS
..gg............       ..WWWWWWWWx..W..    MmmNNmmE           fffffWWfffWffF     RRRRRRWRRWRRSSSS
.gYYG...........       ...WWWWWWxWWx...    MmNNNNmE           FffffWWffffffF     SSS..SRRWRRS..SS
.gYYG...........       ....WWWWxx......    MmmmNNmE           .FFFFFFFFFFFF.     SS....SSSS....SS
.gYYG...........       WWWWWWWWWWWWWx..    MmmNNmmE           ....cccccs....
.gYYG...........       .xxxxxxxxxxxxx..    MmNNmmmE           ....cCccss....
.gYYG....WWWWW..                           MmNmmmmE           ....cCccss....
.gYYG..WWWWWWWWW                           MmmmmmmE           ...ccCcccss...
.gYYG.WaaaaaaaaW                           MmmmmmmE           ...cccccsss...
.gYYG.WWWWWWWWWG                           MMmmmmEE           ....ssssss....
.GGGG.GWWWWWWWGG                           .EEEEEE.
......GGGGGGGG..
```

### 4.4 Itens na mão (em pé, boca para cima)

A pega e a boca são dadas em (x, y) dentro do carimbo. Nas poses "girado", usa-se `NaMao(...).Girado()`.

| Item e variante | L×A | Pega | Boca |
|---|---|---|---|
| banana normal / descascada / casca | 4×9 / 4×8 / 4×6 | (1, A−4) | (1, 0) |
| agua | 4×8 | (1,5) | (1,0) |
| vodka | 4×10 | (1,6) | (1,0) |
| cerveja | 7×8 | (3,5) | (2,0) |
| cafe | 6×5 | (2,4) | (2,0) |
| energetico | 4×8 | (1,5) | (1,0) |
| baseado aceso / aceso-forte | 3×8 | (1,5) | (1,0) |
| cigarro aceso / aceso-forte | 2×8 | (0,5) | (0,0) |
| md | 3×2 | (1,1) | (1,0) |
| bala | 5×3 | (2,2) | (2,0) |
| cogumelo | 5×5 | (2,4) | (2,0) |
| lanca: frasco | 3×9 | (1,5) | — |
| lanca: lenço | 6×5 | centro (3,2), no nariz | — |
| cocaina: espelho cheia / meia / vazia | 11×4 | centro (5,2), no meio das mãos, 1 px acima | — |

```text
banana  desc.  casca  agua  vodka  cerveja  cafe    energ.  baseado cigarro  md   bala   cogumelo  frasco lenco
.nn.    .cC.   y..n   .dd.  .m..   WWWWW..  WWWWW.  .mm.    .x.     tt       VWV  R.R.R  .fff.     EmE    .WWWW.
yYYn    .cC.   y..n   .gg.  .gG.   WWWWx..  WkkkW.  MmmE    .w.     tT       XVX  RRWRS  fWffF     .m.    WWWWWG
yYyn    ycCn   yyyn   gAaG  .gG.   YBBDGg.  WWWWxW  MmNE    ww.     ww            R.S.S  ffWfF     mMm    WaaaaG
yYyn    yyyn   yyn.   WWWG  gWgG   YBBDG.g  .WWWxW  MNNE    www     ww                   .cCs.     .g.    WWWWWG
yyyn    yyn.   .yn.   gaaG  gWgG   YBBDG.g  ..xx..  MmNE    wwx     ww                   .cs..     gYG    .GGGG.
yyn.    yyn.   .kk.   gaaG  ffFF   YBBDGg.          MNmE    wJx     wx                             gYG
yyn.    .yn.          gaaG  fWfF   DDDDG..          MmmE    JJx     zz                             gYG
.yn.    .kk.          .GG.  gWgG   .GGG...          .EE.    qQq     qQ                             gYG
.kk.                        gggG                                                                   .G.
                            .GG.
```

- **Variantes acesas.** "aceso-forte" troca a última linha por `QQQ` no baseado e `QQ` no cigarro.
- **Espelho.** Cheia: `..MMMMMMMMm` / `.MAWWWWWWAE` / `MaaGGGGGGaE` / `mEEEEEEEEE.`. Meia: linhas 2–3 = `.MAWWWAAAAE` / `MaaGGGaaaaE`. Vazia: linhas 2–3 = `.MAAAAAAAAE` / `MaaaaaaaaaE`.

### 4.5 Verbos e quadros

**Configurações de braço** (Superior, Inferior em graus), com a mão resultante na pose de base (quadril 32; 48,5, tronco 0). Nas configurações de pegar, a mão é `Mao.Fechada`.

| Nome | Braço | Ângulos | Mão ≈ |
|---|---|---|---|
| B_REPOUSO | B | (24, 8), mão aberta | padrão |
| B_BAIXO | B | (24, 8) | (43,4; 54,3) |
| B_PEITO | B | (20, −150) | (35,8; 34,5) |
| B_BOCA | B | (60, −150) | (41,5; 29,7) |
| B_NARIZ | B | (−74, 153) | (32,0; 27,0) |
| A_PEITO | A | (−20, 150) | (28,2; 34,5) |
| DUAS_BAIXO | A e B | A (−20, 155) / B (20, −155) | (27,4; 34,1) / (36,6; 34,1) |
| DUAS_ALTO | A e B | A (−35, 160) / B (35, −160) | (24,0; 32,4) / (40,0; 32,4) |

- A boca na pose de base fica em cerca de (31; 29,8). Com `CabecaDescida = d`, a boca e o nariz descem d.
- **Tolerância aceita:** a ponta da boca do item a até 2 px da boca, ou do nariz no inalar. Se o teste falhar, ajuste os ângulos em ±5°.

**Quadros.** Passos a 60 por segundo. "Ordem" é a sequência de quadros que a apresentação mostra. `BracoBNaFrente = true` em todo quadro com o item perto do rosto; nas poses de cheirar, os dois braços na frente.

| Verbo e itens | Quadro | Braços e item | Cara (chave em `Rostos`) | Efeito da pose | Passos |
|---|---|---|---|---|---|
| **Comer**: banana | comendo-1 | B_BAIXO, `MaoB`, normal | feliz | — | 14 |
| | comendo-2 | B_BOCA, `MaoBGirado`, descascada | mordendo | — | 18 |
| | comendo-3 | B_PEITO, `MaoB`, casca | mastigando | — | 12 |
| | comendo-4 | B_PEITO, `MaoB`, casca | feliz | — | 12 |
| | ordem | 1, 2, 3, 4, 3, 4 | | | **80** |
| **Beber**: agua, vodka, cerveja, cafe, energetico | bebendo-1 | B_BAIXO, `MaoB` | feliz | — | 12 |
| | bebendo-2 | B_BOCA, `MaoBGirado` | engolindo | — | 48 |
| | bebendo-3 | B_PEITO, `MaoB` | rindo | — | 24 |
| | ordem | 1, 2, 3 | | | **84** |
| **Fumar**: baseado, cigarro | fumando-1 | B_PEITO, `MaoB`, aceso | neutro | — | 12 |
| | fumando-2 | B_BOCA, `MaoBGirado`, aceso-forte | tragando | — | 36 |
| | fumando-3 | B_PEITO, aceso | soltando | Fumaca, fase 0 | 30 |
| | fumando-4 | B_BAIXO, aceso | sonolento | Fumaca, fase 1 | 24 |
| | ordem | 1, 2, 3, 4 | | | **102** |
| **Cheirar**: cocaina | cheirando-1 | DUAS_BAIXO, `DuasMaos`, cheia; `CabecaDescida` 2 | determinado | — | 24 |
| | cheirando-2 | DUAS_ALTO, meia; descida 3 | fungando | Poeira, fase 0, no nariz | 30 |
| | cheirando-3 | DUAS_BAIXO, vazia; descida −1,5 | acelerado | Brilhos, fase 0 | 30 |
| | ordem | 1, 2, 3 | | | **84** |
| **Engolir**: md, bala, cogumelo | engolindo-1 | B_PEITO, `MaoB` | curioso | — | 12 |
| | engolindo-2 | B_BOCA, `MaoBGirado` | surpreso | — | 16 |
| | engolindo-3 | B_REPOUSO, sem item | engolindo | — | 24 |
| | ordem | 1, 2, 3 | | | **52** |
| **Inalar**: lanca | inalando-1 | A_PEITO frasco + B_PEITO lenço, `Inalar` | travesso | Borrifo, fase 0, entre as mãos | 24 |
| | inalando-2 | A (−24, −8) frasco + B_NARIZ lenço | engolindo | — | 42 |
| | inalando-3 | A (−24, −8) frasco + B_BAIXO lenço | tonto | Estrelinhas, fase 0 | 30 |
| | ordem | 1, 2, 3 | | | **96** |

Nas poses de uso, a cara é a da pose: a apresentação passa `expressao = null`.

### 4.6 Caras

**Carimbos novos de olho de frente (7×8):**
```text
semicerrado-vermelho  bebado-e  bebado-d  espiral   arco-iris pontinho  coracao   apertado-e apertado-d
.......               .......   .......   .KKKKK.   .KKKKK.   .KKKKK.   .......   .......    .......
.......               .......   .......   KWWWWWK   KRRRRRK   KWWWWWK   .11.11.   .......    .......
.......               .KKKKK.   .......   KKKKKWK   KRWNNRK   KWWWWWK   1111111   .KK....    ....KK.
.KKKKK.               KpppppK   .......   KWWWKWK   KRNaNRK   KWWuWWK   1W11111   ...KK..    ..KK...
KpppppK               KWiuuiK   KKKKKKK   KWKWKWK   KRNuNRK   KWWWWWK   1111111   .....KK    KK.....
K5iuu5K               KWiuIiK   KiuuiWK   KWKKKWK   KRNNNRK   KWWWW5K   .11111.   ...KK..    ..KK...
K65556K               K5WiiWK   K5iIW5K   KWWWWWK   KRRRRRK   KWWWWWK   ..1F1..   .KK....    ....KK.
.KKKKK.               .KKKKK.   .KKKKK.   .KKKKK.   .KKKKK.   .KKKKK.   ...F...   .......    .......
```

**Olhos de perfil novos (5×8):**
```text
semicerrado-vermelho  semicerrado  espiral  arco-iris  coracao
.....                 .....        .KKK.    .KKK.      .....
.....                 .....        KWWWK    KRRRK      11.11
.....                 .....        KKKWK    KNNRK      11111
.KKK.                 .KKK.        KWKWK    KauRK      1W111
KpppK                 KpppK        KWKWK    KauRK      11111
K5iuK                 KWiuK        KWWKK    KNNRK      .111.
K655K                 KWiIK        KWWWK    KRRRK      ..1..
.KKK.                 .KKK.        .KKK.    .KKK.      .....
```

**Bocas novas de frente (9×4):**
```text
bobo        dentes      torta
K.......K   KKKKKKKKK   .........
.KbbbbbK.   KWWWWWWWK   .K.......
..KKlKK..   KWKWKWKWK   ..KK...K.
.........   .KKKKKKK.   ....KKK..
```

**Expressões novas.** Colunas na ordem do construtor de `Rosto`, depois `Corado`, `Rubor` e `RuborGrande`.

| Chave | Olho E | Olho D | Sobrancelhas | Boca | Topete | Olho perfil | Boca perfil | Corado / Rubor / Grande | No núcleo? |
|---|---|---|---|---|---|---|---|---|---|
| chapado | semicerrado-vermelho | = | caidas | bobo | Caido | semicerrado-vermelho | sorriso | não | **sim** |
| bebado | bebado-e | bebado-d | uma-erguida | torta | **Torto** | semicerrado | sorriso | sim / BochechaForte / sim | **sim** |
| tonto | espiral | = | preocupadas | ondulada | Ericado | espiral | o | não | **sim** |
| viajando | arco-iris | = | erguidas | aberta | Ericado | arco-iris | aberta | não | **sim** |
| acelerado | pontinho | = | erguidas | dentes | Ericado | arregalado | aberta | não | **sim** |
| apaixonado | coracao | = | neutras | sorriso | Normal | coracao | sorriso | sim | **sim** |
| enjoado | apertado-e | apertado-d | preocupadas | ondulada | Caido | semicerrado | reta | sim / Enjoo / sim | **sim** |
| mordendo | fechado | = | erguidas | risada | Ericado | feliz | aberta | não | não |
| mastigando | feliz | = | neutras | ondulada | Normal | feliz | reta | sim | não |
| engolindo | fechado | = | caidas | firme | Normal | feliz | reta | não | não |
| tragando | sonolento | = | caidas | firme | Normal | aberto | reta | não | não |
| soltando | sonolento | = | caidas | o | Caido | aberto | o | não | não |
| fungando | apertado-e | apertado-d | bravas | reta-pequena | Normal | concentrado | reta | não | não |
| tossindo | apertado-e | apertado-d | preocupadas | o | Ericado | concentrado | o | não | não |

"=" significa o mesmo carimbo do olho esquerdo.

**Rubor.**
- A cor é `Rubor ?? Cor.Bochecha`.
- `Corado` mantém os 4 pixels atuais: (ex−9, ey+5), (ex−8, ey+5), (ex+7, ey+5), (ex+8, ey+5).
- `RuborGrande` acrescenta os mesmos x na linha ey+4, mais (ex−10, ey+5) e (ex+9, ey+5).

### 4.7 Chapéu `Topete.Torto`

- Deslocamento dx = +1,0 e dy = +0,5.
- Todas as formas giram −12° (anti-horário) em torno de (abaX, abaY):
  - a aba e a copa com `Elipse(..., graus: −12)`, com o centro da copa girado;
  - as três faixas horizontais atuais (`acimaDaAba`, faixa vermelha, lábio) viram polígonos de 4 cantos girados em torno do mesmo pivô.
- A sombra da aba, a trama (x + 2y) % 5 e o tufo (pontos `Normal`) não mudam.
- O topo continua na linha 3 ou abaixo.

### 4.8 Efeitos: carimbos, posições e fases

(hx, hy) é a cabeça e (bx, by) a boca, de `Pontos(pose)`. As coordenadas abaixo são o **centro** do carimbo. Depois de colocado, o carimbo é deslocado para dentro do quadro de 2 a 61, para caber o contorno. Nada é desenhado no retângulo dos olhos e da boca (hx−9..hx+9, hy−5..hy+10). Os efeitos são desenhados depois de cabeça e braços, antes do `Contornar`.

```text
fumaca-p  fumaca-m  fumaca-g  bolha  brilho-p  brilho  estrela  coracao  cor-p (R/V/N/a)  poeira  borrifo
.Z.       .ZZ.      .ZZZ.     .A.    .2.       ..2..   ..2..    .1.1.    .R.              .W.     .A.
ZZZ       ZZZZ      ZZZZZ     AWa    2W2       .222.   .222.    1W111    RWR              WWW     AAA
.z.       ZZZz      ZZZZz     .a.    .2.       22W22   22222    .111.    .R.              .W.     .A.
          .zz.      .zzz.                      .222.   .222.    ..1..
                                               ..2..   .3.3.
```

| Efeito | Fase 0 | Fase 1 | Fase 2 |
|---|---|---|---|
| Fumaca | p (bx+6, by−3); m (hx+15, hy−4) | m (bx+7, by−5); g (hx+17, hy−10) | g (hx+18, hy−14); p (hx+14, hy−2) |
| Bolhas | (bx+7, by−2) | (bx+9, by−6); (bx+7, by−1) | (bx+12, by−11); (bx+9, by−5) |
| Brilhos | brilho (hx−17, hy+2); brilho-p (hx+18, hy−6) | brilho (hx+18, hy−6); brilho-p (hx−15, hy−11) | brilho (hx−15, hy−11); brilho-p (hx−17, hy+2) |
| Estrelinhas | 3 estrelas em (hx−18, hy−14), (hx, hy−16)\*, (hx+18, hy−14) | x−6 / x+6 alternados, y ±1 | de volta à fase 0, espelhada |
| Coracoes | (hx−17, hy−3); (hx+17, hy−10) | (hx+18, hy+2); (hx−16, hy−11) | (hx−18, hy+4); (hx+16, hy−13) |
| Cores | cor-p R (hx−16, hy−8), V (hx+16, hy−8) | N (hx−17, hy+2), a (hx+17, hy+2) | R (hx+15, hy−13), V (hx−15, hy−13) |
| Poeira | (nx+3, ny+2), no nariz | igual | igual |
| Borrifo | no meio das mãos A e B | igual | igual |

\* A estrela do centro só aparece se couber acima do chapéu. Se não couber, é omitida (o teste de borda decide).

- **Fase:** com o relógio ligado, `fase = (passosNoEstado / 12) % 3`, isto é, 5 trocas por segundo. Com o relógio parado, fase 0.
- **Efeito × cara padrão:** Fumaca = chapado; Bolhas = bebado; Brilhos = acelerado; Estrelinhas = tonto; Coracoes = apaixonado; Cores = viajando.

### 4.9 Modificadores de pose (`EfeitosPixel.Modificar`), aplicados a qualquer pose fora das de uso

| Efeito | Modificação |
|---|---|
| Bolhas (bêbado) | `Tronco += ±6` (fase par +, ímpar −); `Cabeca += ±4` |
| Fumaca (chapado) | `CabecaDescida += 1,5` |
| Estrelinhas (tonto) | `Cabeca += [−8, 0, +8][fase]` |
| Brilhos (acelerado) | `QuadrilY −= fase % 2`; cauda `Alta` (de frente) ou `PerfilAlta` (de perfil) |
| Coracoes (apaixonado) | `QuadrilY −= 1,5 × (fase % 2)`; cauda `Alta` ou `PerfilAlta` |
| Cores (viajando) | `Cabeca += [−4, +4, 0][fase]` |

Os modificadores nunca mexem em `Borda` nem em `Cipo`, e são desligados em `escondido` e `espiando`.

### 4.10 Ícones

**`Rosto(e)`, em quatro passos:**
1. `BonecoPixel.Desenhar(parado, e)`.
2. Copiar a janela (16, 2) de 32×32.
3. Zerar o anel de 1 px da borda.
4. `Contornar(Cor.Contorno)`: os cortes (pontas da aba, orelhas, queixo) ganham contorno.

**Ampliação:** vizinho mais próximo, fator `Fator(dpi)` (96 → 1×, 144 → 1×, 192 → 2×, 288 → 3×).

**`Rostos.OpcoesDeEmocao`** (recomendação para o menu, que decide):
- neutro, feliz, rindo, curioso, surpreso, assustado, travesso, pensativo, empolgado, determinado, entediado, sonolento;
- chapado, bebado, tonto, viajando, acelerado, apaixonado, enjoado.

São 19 opções. `dormindo` e `bocejando` ficam fora porque são caras de estado; as caras passageiras também ficam fora.

### 4.11 Efeito de cada item: proposta para a área do núcleo, que decide

A arte só depende do nome da cara e da sobreposição.

| Item | Cara e sobreposição | Duração | Passo (proposta) |
|---|---|---|---|
| banana | feliz | 20 s | 1,0× |
| agua | feliz; limpa bebado, tonto e enjoado | 10 s | 1,0× |
| vodka | bebado + Bolhas | 90 s | 0,8× |
| cerveja | bebado + Bolhas (só a fase 0) | 45 s | 0,9× |
| baseado | chapado + Fumaca | 120 s | 0,6× |
| cigarro | pensativo + Fumaca (só a fase 0) | 20 s | 1,0× |
| cocaina | acelerado + Brilhos | 60 s | 1,5× |
| md | apaixonado + Coracoes | 120 s | 1,1× |
| lanca | tonto + Estrelinhas | 20 s | 0,5× |
| cafe | empolgado + Brilhos (só a fase 0) | 45 s | 1,2× |
| energetico | acelerado + Brilhos | 45 s | 1,4× |
| cogumelo | viajando + Cores | 150 s | 0,8× |
| bala | feliz + Brilhos (só a fase 0) | 15 s | 1,0× |

---

## 5. Testes [AUTO] e verificações de tela

**`ItensPixelTestes.cs`**

| Teste | O que afirma |
|---|---|
| `OsTrezeItensNaOrdemDoMenu` | `ItensPixel.Todos` é igual à lista da seção 2 |
| `CadaItemTem24x24EAlfaBinario` | tamanho 24×24; `ParaArgb` só com alfa 0 ou 255 |
| `NenhumItemEncostaEPousaNaUltimaLinha` | `Limites()`: esquerda ≥ 1, topo ≥ 1, direita ≤ 23, base == 24 |
| `CadaItemTemSuaCorAssinatura` | banana tem `Banana`; agua `Agua`; vodka `Faixa`; cerveja `Cerveja`; baseado `Cipo`; cigarro `Filtro`; cocaina `Metal`; md `Lilas`; lanca `Vidro`; cafe `Cafe`; energetico `Neon`; cogumelo `Faixa` com `Branco`; bala `Rosa` |
| `SilhuetasDistintas` | índice de Jaccard das máscaras opacas < 0,8 para cada par |
| `PontosDeTesteBatem` | o ponto opaco é opaco e o (1, 1) é transparente |
| `NaMaoGiradoPreservaPegaEBoca` | a boca girada fica na coluna 0 ou 1 e a pega mapeada é (y, L−1−x) |

**`UsosPixelTestes.cs`**

| Teste | O que afirma |
|---|---|
| `CadaVerboTemDeTresAQuatroQuadros` | as poses `<verbo>-N` existem em `PosesPixel.Todas` |
| `QuadrosDeUsoCabemNoQuadro` | cada item do verbo, espelhado ou não: alfa binário, nada encosta no topo e nas laterais, pés na penúltima linha perto do centro (como `OsPesTocamABordaDeBaixoPertoDoCentro`) |
| `ItemChegaNaBocaNosQuadrosGirados` | nos quadros `MaoBGirado`, a boca do item fica a ≤ 2 px de `Pontos(pose).Boca`; no inalando-2, o centro do lenço a ≤ 2 px do nariz |
| `AMaoCobreAPega` | o pixel no centro da mão que segura é `Creme` ou `CremeSombra`, não a cor do item |
| `CheirarEsvaziaAcarreira` | a contagem de `Branco` no espelho cai: cheia > meia > vazia (0) |
| `DesenharSemItemNaoFalha` | toda pose de uso desenha com `item: null` |

**`RostosNovosTestes.cs`**

| Teste | O que afirma |
|---|---|
| `TodaChaveDeCarimboExiste` | as 14 caras novas resolvem olho, sobrancelha, boca e perfil |
| `CarasNovasDiferemDoNeutro` | pelo menos 10 pixels diferentes no retângulo do rosto do `parado` |
| `EnumDoNucleoCobreOsRostos` | todo `Expressao` em minúsculas existe em `Rostos.Expressoes` |
| `OpcoesDeEmocaoExistem` | toda chave de `OpcoesDeEmocao` existe em `Rostos.Expressoes` |
| `ChapeuTortoCabeENaoEncosta` | com `bebado`, em todas as poses: topo ≥ 1 e pixels do chapéu diferentes dos do `neutro` |
| `RuborEnjoadoEhVerde` | o `parado` + `enjoado` tem pixels `Enjoo` |

**`EfeitosPixelTestes.cs`**

| Teste | O que afirma |
|---|---|
| `EfeitosNaoCobremOlhosEBoca` | o recorte hx−9..hx+9, hy−5..hy+10 é idêntico com e sem sobreposição |
| `EfeitosCabemEmTodasAsPoses` | toda pose sem cipó × 8 efeitos × 3 fases: alfa binário, sem tocar a borda |
| `FasesSaoDeterministicas` | duas renderizações iguais |
| `ModificadorNaoMudaCipoNemBorda` | `Cipo` e `Borda` intactos |

**`IconesDoMenuTestes.cs`**

| Teste | O que afirma |
|---|---|
| `RostoTem32EAnelSoContornoOuNada` | tamanho 32×32; o anel da borda só tem `Nada` ou `Contorno` |
| `RostoMostraOsOlhosDaExpressao` | `neutro` tem `Branco` e `Iris`; `arco-iris` tem `Rosa`; `coracao` tem `Coracao` |
| `IconeDeItemEhODesenhoDoChao` | sequência igual a `ItensPixel.Desenhar` |
| `FatorPorDpi` | 96, 120, 144 e 168 → 1; 192 → 2; 288 → 3 |
| `AmpliarMantemAlfaBinario` | tamanho igual ao lado × fator |

**Ferramenta de prévias** (`tools\Buzzy.Identidade`):
- Gera `itens-8x.png`, `itens-na-mao-8x.png`, `usos.png` (cada verbo com cada item), `efeitos.png` (parado × 8 efeitos × 3 fases), `icones-menu.png` (rostos e itens a 4×) e `expressoes.png` com 28 caras.
- Gera também a folha nativa `assets\identidade\pixel\buzzy-itens.png`.
- Sai com 1 se qualquer item, quadro ou efeito encostar na borda.
- O `poses-claro.png` atual está desatualizado (sem cipó e sem `escondido`) e deve ser regenerado.

**Verificação de tela com input sintético.**
- Nova opção, de preferência em `tests\Buzzy.Verificacao\VerificacaoItens.cs`, **depois** do merge da Fase 5.
- Tudo é pelo log de diagnóstico e por `WindowFromPoint`, sem ler pixels (a verificação não captura a tela).

1. **Invocar pelo menu.** Menu do personagem → "Itens" → tecla de acesso. Esperar `ITEM invocado` com o retângulo e os pontos opaco e transparente de `ItensPixel.PontosDeTeste`. `WindowFromPoint(opaco)` deve ser a janela do item, do processo do Buzzy; `WindowFromPoint(transparente)` deve ser o receptor. O foco deve continuar no receptor.
2. **Arrastar e soltar.** `SendInput` do ponto opaco do item até o ponto opaco do personagem. O log `SPRITE` deve mostrar `pose=<verbo>-1`, `-2`… na ordem da seção 4.5. A janela do item deve sumir. Depois deve aparecer `SPRITE expressao=<cara do efeito> efeito=<sobreposição>`.
3. **Emoção dominante.** Menu → "Emoção dominante" → opção. O log deve mostrar `MENU icone=32×32 fator=1` a 96 DPI, e `SPRITE expressao=<escolhida>` quando o personagem estiver parado.
4. **Pressionar no meio do uso.** O log deve mostrar `pose=segurado` na hora (DEC-004).

---

## 6. Riscos e pontos que tocam outras áreas ou a Fase 5

1. **Núcleo, `Tipos.cs`.** O `Expressao` precisa das 7 caras de efeito, **acrescentadas no fim**. Assim as referências gravadas em `tests\Buzzy.Core.Testes\Referencias\*.txt` e o `Retrato.Descrever` não mudam. As caras passageiras não entram no núcleo. O teste `EnumDoNucleoCobreOsRostos` amarra os dois lados.
2. **Apresentação.**
   - `QuadroDoSprite` ganha `Item`, `Efeito` e `Fase`. O cache de `SpriteProvisorio`, sem limite, cresce: 13 itens × até 4 quadros × espelho × DPI, mais efeitos × 3 fases × poses. É preciso limitá-lo (por exemplo, 256 quadros, descartando os mais antigos) por causa da meta de memória de Q-08.
   - `PoseDoPersonagem.Escolher` passa `expressao = null` nas poses de uso.
   - A janela do item usa 48 DIP e `Tela.Deformada(1,3; 0,7)` no pouso. Com a margem de 3 px num quadro de 24, o achatamento fica menor, e o teste deve aceitar isso.
3. **Menu nativo** (`MenuNativo.cs`, `Win32.cs`, `Textos.resx`).
   - O `hbmpItem` não é ampliado pelo Windows. O menu cria o HBITMAP com `CreateDIBSection`, 32 bpp e topo para baixo, com os pixels já ampliados por `IconesDoMenu.Ampliar`.
   - `BitBlt`, `StretchBlt` e `CreateDC` estão **proibidos** em `Regras.cs` e não podem ser usados para ampliar.
   - `CreateDIBSection` não está na lista, mas é gdi32 novo no adaptador: confira no portão.
   - Os rótulos com acento ("Bêbado", "Café") ficam no `.resx`; as chaves de arte ficam sem acento.
4. **Portão de fonte.** `src\Buzzy.Visual` é lido pelo portão. Nenhum identificador proposto contém `Socket`, `HttpClient`, `Clipboard`, `Process.Start`, `CopyFromScreen` nem nome de função proibida; mantenha assim nos comentários também.
5. **Fase 5 em curso.** A arte só toca `src\Buzzy.Visual\Pixel\*`, `tools\Buzzy.Identidade\*`, testes **novos** em `tests\Buzzy.App.Testes\` e `docs\IDENTIDADE_VISUAL.md`. Não há conflito com os arquivos da Fase 5.
   - A verificação de tela nova deve esperar o merge de `tests\Buzzy.Verificacao\{Programa,Verificacao,VerificacaoFase4,Apoio}.cs`, que estão modificados agora.
   - Em `ARCHITECTURE.md`, mexa só na 2.10; a Fase 5 mexe na 2.8 e na 2.12.
   - Em `DECISIONS.md`, detalhe DEC-027 e DEC-028 e não use os números 029–032.
6. **Ajuste fino da arte.** Os ângulos e os carimbos foram calculados ou desenhados sem renderizar. Os testes com tolerância de ±2 px e as prévias são o critério. Espere uma rodada de retoque, principalmente em cerveja, lança-perfume, espiral de perfil e B_BOCA.
7. **Tom.** Os itens são genéricos, sem marca, sem texto e sem instrução de uso. A folha de maconha foi evitada de propósito. Se o usuário quiser mais explícito ou mais leve, só os carimbos mudam.
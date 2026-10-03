# IDENTIDADE_VISUAL.md — Identidade visual do Buzzy

> Fonte canônica da aparência do Buzzy: conceito, proporções, paleta, estilo, expressões, poses, itens, efeitos e regras técnicas dos assets. As seções numeradas são citadas pelo código: não renumere. O produto está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md) (Visão); a direção, em DEC-018 (pixel art) e DEC-019 (semelhança intencional com o Luffy); a arte da emoção dominante e do tamagotchi, em DEC-027 e DEC-028. Os valores numéricos são a referência do gerador em `src/Buzzy.Visual/Pixel/`; mudanças de direção vão para a DEC correspondente. O histórico até 2026-10-02 está em [arquivo/identidade-visual-historico.md](arquivo/identidade-visual-historico.md).
>
> STATUS: PLANNED. A pixel art fiel às pranchas de `assets/references/` existe desde 2026-09-29, com a folha nativa e as prévias em `assets/identidade/pixel/`. O app mostra o quadro parado, as poses provisórias por estado e a arte do tamagotchi (no menu, no personagem e nas janelas dos itens), conferidas por testes automatizados; as animações completas são da Fase 6. A revisão visual e de tom pelo usuário está pendente.

## 1. Conceito

**Buzzy é um macaquinho de pelo azul-marinho em pixel art**, tirado das pranchas de referência do
usuário: rosto creme grande com um bico de pelo entre os olhos, olhos castanhos enormes com dois
brilhos, orelhas redondas cor de pêssego, barriga creme, braços compridos que chegam aos joelhos,
mãos e pés creme, **cauda longa que termina num cacho com a ponta creme** e, como nas pranchas, o
**chapéu de palha com a faixa vermelha** do Luffy (DEC-019). Por baixo do chapéu há um tufo de pelo
bagunçado, que aparece quando o chapéu salta.

A personalidade é a do Luffy (PRODUCT_SPEC.md, Visão): sorriso e risada fáceis, postura solta,
impulsividade, otimismo, energia e movimento elástico, nas poses e nas expressões. O chapéu reage junto:
salta no susto e na risada, desce no sono.

## 2. Grade, escala e proporções

- **Quadro de 64 × 64 pixels de arte**, mostrado em **128 × 128 DIP**: cada pixel de arte vale 2 DIP.
  Em 100%, 150% e 200% de escala do Windows, isso dá ampliação inteira de 2×, 3× e 4×, sempre nítida
  (sem suavização). Escalas intermediárias (125%, 175%) usam vizinho mais próximo; a Fase 8 decide os
  passos oferecidos ao usuário.
- **Âncora:** centro da borda de baixo do quadro, entre os pés (ARCHITECTURE.md 2.4 e 2.10). Em pé,
  o contorno da sola ocupa a última linha. Pendurado, a âncora da Fase 6 é o ponto das mãos.
- **Esqueleto** (pixels de arte), em `BonecoPixel`:

| Parte | Medida | Observação |
|---|---|---|
| Altura em pé | ~60 | do topo do chapéu à sola |
| Chapéu | aba de 34 × 5,6; copa de 18,8 × 11 | aba larga e copa baixa com a faixa vermelha, como nas pranchas |
| Cabeça | círculo de raio 11,2 | ~40% da altura, como nas pranchas |
| Quadril → pescoço | 18 | tronco em pera: peito estreito, barriga larga |
| Braço | 10,8 + 9,6 + mão | em pé, as mãos ficam na altura dos joelhos |
| Perna | 5,6 + 5,2 + pé | pés creme grandes, com risco de dedo |
| Cauda | ~30 | sai do quadril e enrola num cacho; o último terço é creme |

## 3. Paleta

Tons-base lidos da prancha de referência (swatches e figura de frente).

| Nome no código | Cor | Uso |
|---|---|---|
| `Contorno` | `#121830` | contorno externo, linhas entre partes, detalhes do rosto |
| `PeloEscuro` | `#1B2748` | sombra do pelo, membros do lado de trás no perfil |
| `Pelo` | `#283A5F` | pelo (azul-marinho da prancha) |
| `PeloClaro` | `#3B5486` | realce do pelo na borda iluminada |
| `Creme` | `#FDD5A6` | rosto, barriga, mãos, pés, ponta da cauda |
| `CremeSombra` | `#E6B083` | sombra do creme, dedos |
| `Pessego` | `#F6996D` | dentro das orelhas, nariz |
| `PessegoEscuro` | `#D2704A` | sombra do pêssego |
| `Iris` / `IrisClara` | `#6A3419` / `#9A5236` | olhos castanhos |
| `Pupila` | `#140A06` | pupila |
| `Branco` | `#FFFFFF` | esclera e brilhos |
| `Boca` / `Lingua` | `#5A1D22` / `#E8727A` | boca aberta |
| `Bochecha` | `#F4A987` | rubor das expressões felizes |
| `Sobrancelha` | `#2E2230` | sobrancelhas |
| `Palha` / `PalhaClara` / `PalhaEscura` | `#EFB262` / `#FCCB7E` / `#C27F45` | chapéu de palha (com pontos de trama) |
| `Faixa` / `FaixaEscura` | `#B83A37` / `#862A2B` | faixa vermelha do chapéu |
| `Cipo` / `CipoEscuro` / `CipoClaro` | `#6B8A34` / `#46601F` / `#93B24F` | cipó da borda de cima (DEC-024) |
| `Folha` / `FolhaEscura` | `#4EA24A` / `#2E6B2E` | folhas do cipó |

**Cores do tamagotchi (DEC-028).** São 35, acrescentadas sempre no fim de `Cor`, para as antigas não
mudarem de byte nem de cor (um teste trava isso). Cada uma tem uma letra na legenda dos carimbos,
que recusa letra repetida.

| Nome no código | Cor | Uso principal |
|---|---|---|
| `Banana` / `BananaClara` / `BananaEscura` | `#F7D548` / `#FFF08A` / `#C9A227` | banana; o claro também no líquido do frasco |
| `Agua` / `AguaClara` / `AguaEscura` | `#5EC8F2` / `#BDEBFF` / `#2E8FC7` | água, bolhas, gotas, vidro do espelhinho, lenço |
| `Vidro` / `VidroSombra` | `#DDF3FA` / `#A2CCDA` | garrafas, caneca e frasco |
| `Metal` / `MetalClaro` / `MetalEscuro` | `#B9C2CC` / `#E6EBF0` / `#7D8794` | tampas, válvula e bordas da lata |
| `Papel` / `PapelSombra` | `#F4F1E8` / `#CFC8B6` | papel do cigarro e do baseado |
| `Filtro` / `FiltroEscuro` | `#E59A4C` / `#B86B2A` | filtro do cigarro |
| `Brasa` / `BrasaClara` | `#FF6A2B` / `#FFD24A` | brasa; a clara na tragada |
| `Fumaca` / `Cinza` | `#D9DCE0` / `#8A8A8A` | fumaça; cinza na ponta do cigarro e do baseado |
| `Cerveja` / `CervejaEscura` | `#F2A93B` / `#C77B1E` | chope |
| `Cafe` | `#5B3A21` | café |
| `Neon` / `NeonEscuro` | `#B6F23A` / `#6FA81E` | lata do energético; verde dos losangos e do olho de arco-íris |
| `Rosa` / `RosaEscura` | `#FF6FB5` / `#D2458C` | bala; losangos e olho de arco-íris |
| `Lilas` / `LilasEscuro` | `#B889F2` / `#7F58C4` | MD; losangos |
| `Coracao` | `#E8344E` | corações, olhos de coração, chapéu do cogumelo |
| `Estrela` / `EstrelaEscura` | `#FFE45C` / `#E0A526` | estrelinhas, brilhos, moldura do espelhinho, raio do energético |
| `EscleraVermelha` / `OlhoVermelho` | `#F6B8AE` / `#D9534F` | olhos do chapado e do bêbado |
| `Enjoo` | `#9BCB6B` | rubor verde do enjoado |
| `BochechaForte` | `#EE7F72` | rubor forte do bêbado |

## 4. Estilo

- Pixel art de 1 pixel = 1 cor, sem meio-tom nem suavização: todo pixel é alfa 0 ou 255, o que
  cumpre de saída a regra do clique por pixel de P1 (só alfa 0 deixa o clique passar).
- Contorno externo de 1 pixel em `Contorno`, na vizinhança de 4 (cantos limpos).
- Luz de cima e da esquerda: sombra de 1 pixel na borda de baixo e da direita de cada parte; realce
  discreto na cabeça e no tronco.
- Partes sobrepostas (braço na frente do tronco, orelha no perfil) ganham uma linha interna escura.
- No perfil, braço e perna do lado de trás usam o tom escuro.
- Olhos, sobrancelhas, nariz e bocas são carimbos desenhados pixel a pixel (`Rostos`), não geometria.
- Sem texto na arte (Q-12, DEC-003); sem sombra projetada no sprite.

## 5. Regras técnicas

1. **Borda dura** garantida pela própria pixel art (alfa 0 ou 255 em todo pixel).
2. **Tamanho físico** = 64 pixels de arte × escala inteira do DPI do monitor da âncora (2×, 3× ou 4×).
3. **Espelhamento:** poses de perfil olham para a direita; a esquerda é o espelho horizontal da tela
   inteira. Os brilhos dos olhos ficam do mesmo lado nos dois olhos.
4. **Quadro:** o corpo nunca encosta na borda do quadro (conferido por `tools/Buzzy.Identidade`). Há duas exceções, nas duas a borda do quadro coincide com uma borda da tela:
   - o cipó encosta na borda de cima, onde se prende na tela (DEC-024);
   - o esconderijo tem a borda atrás da qual o corpo some na última linha do quadro, e as mãos a seguram (DEC-025).

   Com qualquer cara e em qualquer pose, o preenchimento nunca fica na borda de cima nem nas laterais
   do quadro: o contorno sempre aparece inteiro (desde 2026-10-01, conferido por teste e pela
   ferramenta, que acusa "contorno cortado na borda"). **Exceção documentada:** com as caras de chapéu
   eriçado, o contorno de cima do chapéu fica na linha 0, inteiro, em `andando-2`, `andando-4` e
   `escalando-1`; mudar isso mudaria quadros de caminhada e de escalada que o app já mostra. Em `escalando-2`, a
   copa desce 1 pixel com essas caras, para o preenchimento não chegar à borda.
5. **Âncora por pose:** declarada no manifesto da Fase 6.
6. **Deformação e giro (Fase 4):**
   - achatar e esticar (DEC-023) redimensionam o desenho por vizinho mais próximo, com os pés no lugar;
   - o esconderijo nas laterais é a pose de baixo girada 90° (DEC-025).

   Nenhum dos dois cria cor nova nem meio-tom.
7. **Itens do tamagotchi (DEC-028; seção 7a):** grade de 24 × 24 pixels de arte, mostrada em
   48 × 48 DIP, na mesma densidade do boneco (1 pixel de arte = 2 DIP). O contorno de baixo do item
   fica na última linha, e ele pousa como os pés; há 1 pixel livre no topo e nas laterais. Alfa só 0
   ou 255; nada de texto, marca ou folha. Todo item tem pelo menos 8 pixels de altura e de largura,
   contando o contorno, para dar para agarrar; nenhum usa as cores do pelo; e, nas poses de uso, nenhum fica
   no meio do corpo, abaixo do peito, onde pareceria roupa.
8. **Sobreposições de efeito (seção 7b):** ficam a 2 pixels das bordas do quadro, para o contorno
   caber, e nunca cobrem os traços do rosto nem a gota de suor da cara paranoica.

## 6. Expressões

Carimbos do rosto, independentes do corpo (trocar expressão não muda estado nem posição).

| Expressão | Olhos | Sobrancelhas | Boca | Tufo |
|---|---|---|---|---|
| `neutro` | abertos | neutras | sorriso | normal |
| `feliz` | arcos | neutras | aberta, com rubor | normal |
| `rindo` | arcos | erguidas | risada, com rubor | eriçado |
| `curioso` | olhando para o lado | uma erguida | canto | normal |
| `surpreso` | arregalados | erguidas | "o" | eriçado |
| `assustado` | arregalados | preocupadas | ondulada | eriçado |
| `sonolento` | meio fechados | caídas | reta | caído |
| `bocejando` | fechados | caídas | bocejo | caído |
| `dormindo` | fechados | nenhuma | reta pequena | caído |
| `travesso` | piscada | uma erguida | língua de fora | normal |
| `entediado` | meio fechados | retas | reta | caído |
| `pensativo` | olhando para cima | uma erguida | canto | normal |
| `empolgado` | brilho de estrela | erguidas | aberta, com rubor | eriçado |
| `determinado` | abertos | bravas | firme | normal |

As 14 caras acima são as de humor. Ficam em `previa/expressoes.png`, nesta ordem, que é a do enum
`Expressao` do núcleo e a das opções da emoção dominante (DEC-027). Cada célula dessa prévia é o
ícone da opção no menu (seção 7b).

**Caras de efeito (DEC-028).** Só a onda as mostra; a chave é o nome do valor no fim do
enum `Expressao`, em minúsculas (`eletrico`, não `acelerado`). A `paranoico` é a da onda
da paranoia, que nenhum item começa. Aparecem em `previa/rostos-efeito.png`, de frente e de perfil.

| Expressão | Olhos | Sobrancelhas | Boca | Tufo e bochechas |
|---|---|---|---|---|
| `bebado` | desencontrados, um mais fechado, com a esclera avermelhada | uma erguida | torta | chapéu torto; rubor forte e grande |
| `enjoado` | apertados (> <) | preocupadas | ondulada | caído; rubor verde e grande |
| `chapado` | meio fechados, vermelhos | caídas | boba, com a língua | caído |
| `eletrico` | arregalados, com a pupila bem pequena | erguidas | de dentes | eriçado |
| `apaixonado` | corações | neutras | sorriso | normal; com rubor |
| `tonto` | espirais (de perfil, revirados) | preocupadas | ondulada | eriçado |
| `viajando` | arco-íris | erguidas | aberta | eriçado |
| `paranoico` | arregalados, todo brancos e sem íris, com a pupila de 2 × 2 colada no alto, olhando para cima (de frente e de perfil) | aflitas: preocupadas e erguidas, com uma linha livre acima do olho | tensa: pequena, fechada, com os dentes cerrados | eriçado; gota de suor na têmpora |

- **Chapéu torto** (`Topete.Torto`): o chapéu do bêbado gira 12° no sentido anti-horário, em torno do
  centro da aba, que desce meio pixel e vai um pixel para a frente; o tufo fica o normal.
- **Rubor grande:** no bêbado e no enjoado, uma mancha de 3 × 2 pixels em cada bochecha, na cor própria
  (`BochechaForte` ou `Enjoo`), que aparece também de perfil, para ler andando. O rubor das caras
  antigas não mudou.
- **Gota de suor** (`Rosto.Gota`, só na `paranoico`): de frente, na têmpora da esquerda da tela, entre
  a aba e a orelha, porque o braço que aponta o teto passa pela orelha direita; de perfil, atrás do
  olho, sob a aba. É desenhada por cima dos braços da frente, para ficar sempre à vista, e entra na
  área protegida do rosto.
- **A 1×, a cara paranoica perde sinal:** a pupila some na linha de cima do olho, e as sobrancelhas e
  a gota quase somem. No app, o tamanho mínimo é 2× (128 DIP a 100%), e as caras de efeito nunca
  viram ícone do menu.

**Caras passageiras (DEC-028).** Existem só nos rostos da arte, para as poses de uso e dos gestos; o
núcleo não as conhece. Também aparecem em `previa/rostos-efeito.png`, de frente.

| Expressão | Olhos | Sobrancelhas | Boca | Tufo |
|---|---|---|---|---|
| `mordendo` | fechados | erguidas | risada | eriçado |
| `mastigando` | arcos, com rubor | neutras | ondulada | normal |
| `engolindo` | fechados | caídas | firme | normal |
| `tragando` | meio fechados | caídas | firme | normal |
| `soltando` | meio fechados | caídas | "o" | caído |
| `fungando` | apertados | bravas | reta pequena | normal |
| `tossindo` | apertados | preocupadas | "o" | eriçado |

A `tossindo` é a cara dos gestos de tosse e de espirro.

## 7. Poses

Poses-chave em `PosesPixel`, uma por estado ou gesto de ARCHITECTURE.md 2.6. Desde a Fase 6 (DEC-036), a ordem dos
quadros e os tempos de cada animação ficam no manifesto de clipes (`src/Buzzy.App/Apresentacao/clipes.json`), e os
quadros extras, em `PosesPixel.DosClipes`, fora de `Todas` (a folha nativa não muda), cada um feito da pose-chave com
poucos ângulos mudados, para o corpo não pular entre os quadros:

| Quadro extra | Sai de | O que muda |
|---|---|---|
| `cocando-2` | `cocando` | a mão desce pela cabeça, que inclina mais |
| `espreguicando-1`, `espreguicando-2` | `espreguicando` | os braços sobem pelos lados, sonolento; no fim, na ponta dos pés |
| `olhando-2` | `olhando` | a outra mão faz a aba nos olhos, e a cabeça vira para o outro lado |
| `espiando-2` | `espiando` | a cabeça sobe 2 pixels acima da borda |
| `brincando-2` | `brincando` | troca o braço e a perna levantados |
| `caindo-2` | `caindo` | os braços debatem e as pernas chutam |
| `reagindo-2` | `reagindo` | um pulinho de 1 pixel, com os braços abertos (também na gargalhada) |

A prévia `previa/clipes.png` mostra cada quadro extra ao lado da pose-chave.

| Pose | Vista | Estado ou gesto |
|---|---|---|
| `parado` | frente | `IDLE` |
| `andando-1` a `andando-4` | perfil | `WALKING` (ciclo de quatro quadros) |
| `escalando-1`, `escalando-2` | perfil | `CLIMBING` |
| `pendurado` | frente | `HANGING` sem cipó (guardada; o app usa o cipó desde DEC-024) |
| `cipo-1` a `cipo-3` | frente | `HANGING` no cipó da borda de cima: balanço esquerda, meio e direita (DEC-024) |
| `escondido` | frente (girada nas laterais) | `PEEKING`: atrás da borda, só chapéu, cabeça e mãos (DEC-025) |
| `impulso`, `no-ar` | perfil | `JUMPING` |
| `caindo` | frente | `FALLING` |
| `pousando` | frente | `LANDING` |
| `sentado`, `dormindo` | frente | `RESTING` |
| `segurado` | frente | `PRESSED`/`DRAGGING` |
| `reagindo` | frente | `REACTING` |
| `olhando`, `cocando`, `espreguicando`, `brincando` | frente | gestos curtos (olhar ao redor, coçar-se, espreguiçar-se, brincar) |
| `espiando` | frente | gesto curto: espiar por trás de uma borda (só cabeça e mãos acima dela; campo `Borda` da pose) |

**Poses de uso (DEC-028), em `UsosPixel`.** São 32, só no chão e de frente, e ficam fora de
`PosesPixel.Todas`: sem o item elas não se leem, e assim a folha nativa e as prévias de poses não
mudam. `PosesPixel.PorNome` acha qualquer pose. Cada verbo tem uma sequência de quadros, com os
goles, as mordidas, as tragadas e as fungadas repetidos, e a soma dos passos (60 por segundo) é a
duração do uso no núcleo:

| Verbo | Quadros | Passos | O que acontece |
|---|---|---|---|
| comer | `comendo-1` a `comendo-8` | 150 | mostra o item, morde, mastiga, morde de novo, mastiga o resto e ri |
| beber | `bebendo-1` a `bebendo-4` | 120 | mostra a bebida, três goles (a cabeça sobe 1 pixel entre eles) e ri |
| fumar | `fumando-1` a `fumando-5` | 210 | mostra o item aceso, duas tragadas, cada uma seguida da fumaça, e relaxa |
| cheirar | `cheirando-1` a `cheirando-6` | 120 | o espelhinho na palma, duas fungadas com poeira e, de mãos vazias, elétrico, com brilhos |
| engolir | `engolindo-1` a `engolindo-4` | 90 | mostra o item, leva à boca, engole e fica travesso |
| inalar | `inalando-1` a `inalando-5` | 120 | borrifa o frasco no lenço, inala e fica tonto, com estrelinhas e a cabeça balançando |

- A cara é a da própria pose: as caras passageiras e algumas de humor e de efeito. A apresentação
  passa a expressão nula.
- No app, os quadros de uso são de frente e sem espelho, como as outras poses de frente: a luz vem da
  esquerda, a trama do chapéu é fixa e o item fica na mão B.
- O item fica na pega da mão B, fechada (na lança-perfume, o frasco vai na mão A e o lenço na B).
  Nos quadros em que o item vai ao rosto, o braço se dobra por cinemática inversa até a ponta do
  item cair na boca ou, no cheirar e no inalar, no nariz, qualquer que seja o tamanho do item. Os
  ângulos da pose só dizem para que lado o cotovelo dobra; na tragada e no inalar, os cotovelos vão
  para fora, para o braço não cruzar o queixo nem cobrir a barriga.
- O item ganha uma linha de contorno onde encosta no corpo, e os olhos ficam à vista com ele no rosto.
- Não há poses de uso na parede, no cipó nem no esconderijo: ali vale a pose do apoio (`escalando-1`
  virado para a parede, `cipo-2` e `escondido`, girada nas laterais), com a cara do uso e a
  sobreposição, sem o objeto. No esconderijo, a boca fica fora do quadro, e só os olhos mostram a cara.
  Poses de uso por apoio ficam para depois.
- O baseado que ele fuma por conta própria (DEC-028, itens 36 a 42) não tem arte nova: usa
  os mesmos quadros `fumando-1` a `fumando-5`, com o baseado na mão, a cara da pose e a fumaça do
  chapado por cima, sempre no chão.

**Poses dos gestos da onda (DEC-028), em `PosesPixel.DosGestos`.** Os oito gestos que só a onda
sorteia têm os nomes do enum `Gesto` em minúsculas e ficam fora de `Todas`. Os seis primeiros não
têm desenho próprio ainda e usam as poses existentes; os dois da paranoia, no fim da
lista, têm desenho próprio, com a cara `paranoico`:

| Pose | Feita de |
|---|---|
| `soluco` | `parado`, com a cara de surpreso |
| `danca` | `brincando` |
| `gargalhada` | `reagindo` |
| `espirro` | `parado`, com a cara `tossindo` e a poeira |
| `tosse` | `parado`, com a cara `tossindo` e a fumaça |
| `tremedeira` | `parado` deslocado 1 pixel, com a cara elétrica; a apresentação alterna as duas a cada 4 passos (7,5 vezes por segundo), com a cara elétrica também no `parado`: só o corpo treme |
| `olharproteto` | desenho próprio: de joelhos dobrados, olha para cima e aponta o teto com o indicador reto para cima; o braço B sobe pela frente da orelha, longe do rosto, com o punho à direita da ponta da aba, para o dedo não encostar nela; o punho A aperta o peito; a cauda fica alta |
| `agachar` | desenho próprio: agachado, com a cabeça encolhida entre os ombros, segura a aba do chapéu com os dois punhos e espia para cima; os cotovelos abrem para fora, e os antebraços sobem ao lado dos olhos, sem cobrir o rosto |

- **Mão apontando** (`Mao.Apontando`, no fim do enum): o indicador sai do meio do alto do punho, reto
  para cima, com 1 pixel de largura e 4 acima do punho. Na direção do antebraço, como no primeiro
  desenho, ele lia como um joinha.
- **Desvios da paranoia:** de frente, o boneco não tem como inclinar a cabeça para trás; no
  `olharproteto`, o pescoço estica 1 pixel, como quem se estica para ver o teto. Não há arte de andar
  na ponta dos pés: ele anda como sempre, com a cara, o suor e o tremidinho. Com as mãos abertas, no
  `agachar`, as mãos pareciam orelhas, por isso viraram punhos.
- O app mostra os dois gestos sempre com a cara da própria pose. Com outra cara, o `agachar` ficaria
  com os punhos na copa e sem a gota da têmpora.

## 7a. Itens do tamagotchi

Os 13 itens da DEC-028, em `ItensPixel`, genéricos e de desenho animado, sem texto, marca nem folha
(regras técnicas na seção 5, item 7). A chave é o nome do enum `Item` do núcleo em minúsculas, e a
ordem é a do menu. O desenho do chão também é o ícone do menu e está na folha nativa
`buzzy-itens.png`. É ele que a janela do item mostra, com 48 × 48 DIP (2× a 100%), ampliado pelo DPI
sem suavização; só os pixels opacos recebem clique.

| Chave | No chão | Verbo | Na mão (variantes, na ordem da animação) |
|---|---|---|---|
| `banana` | banana deitada, curva para cima, com o cabinho | comer | `aberto` (descascada em cima, segura pela casca), `mordido`, `resto` |
| `agua` | garrafinha de plástico gordinha, de tampa azul, com rótulo branco e uma gota | beber | `normal`, `gole` (a garrafa deita para a boca) |
| `vodka` | garrafa alta de vidro, tampa de metal e rótulo vermelho liso | beber | `normal`, `gole` (deita) |
| `cerveja` | caneca de chope com espuma transbordando e alça em D | beber | `normal`, `gole` (segura pela alça; a caneca fica em pé) |
| `baseado` | cone de papel com piteira de papelão, pontinhos verdes e a ponta larga acesa, com cinza e brasa | fumar | `aceso` (brasa para cima), `tragando` (deitado, com a piteira na boca e a brasa mais clara) |
| `cigarro` | reto, com filtro laranja, papel branco, cinza e brasa; 8 pixels de altura com o contorno | fumar | `aceso`, `tragando` |
| `cocaina` | espelhinho deitado, em perspectiva, com moldura dourada, vidro com brilho e duas carreiras curtas | cheirar | `cheia` (deitado na palma, como uma bandeja), `meia` e `vazia` (seguro pela ponta, com a borda de cima no nariz) |
| `md` | comprimido lilás com um coração em relevo | engolir | `normal` e `na-boca`, o mesmo comprimido, na ponta dos dedos |
| `lancaperfume` | frasco fino de vidro com válvula de metal e, ao lado, um lenço dobrado em triângulo, com uma barra azul | inalar | `frasco` (na mão A) e `lenco` (na mão B, com o meio no nariz) |
| `cafe` | xícara branca com café, alça e pires | beber | `normal`, `gole` (segura pela alça; a xícara fica em pé) |
| `energetico` | lata fina verde-neon, com um raio amarelo e bordas de metal | beber | `normal`, `gole` (deita) |
| `cogumelo` | chapéu vermelho de bolinhas brancas e pé creme | comer | `aberto` (seguro pelo pé), `mordido`, `resto` |
| `bala` | bala embrulhada em papel rosa listrado, torcido nas pontas | engolir | `normal` (embrulhada, na ponta dos dedos), `na-boca` (sem o papel) |

- **Na mão:** cada variante é um carimbo com a **pega**, o ponto que fica sob o centro da mão, e a
  **ponta**, o pixel que encosta na boca ou no nariz; a casca e o frasco não têm ponta. A palma é
  desenhada por cima da pega. Os itens na mão seguem a escala do boneco, com até 14 pixels, e os do
  mesmo verbo têm as mesmas variantes. "Girado" leva o carimbo 90° no sentido anti-horário, sem perda,
  com a ponta de cima indo para a esquerda, rumo à boca.
- **Pontos de teste:** cada item tem um ponto opaco, no corpo, e um transparente, no canto, para a
  verificação de tela conferir o clique no item.
- **A bala** continua desenhada como um doce embrulhado em papel rosa listrado. Desde 2026-10-01, ela é
  droga sintética na regra do jogo (DEC-028): começa a onda do eufórico, com os corações dele, e pode
  trazer paranoia. O desenho não mudou; o tom fica para a revisão do usuário.

## 7b. Efeitos, modificadores e ícones

**Sobreposições (`EfeitosPixel`).** Nove efeitos de desenho animado, cada um em 3 fases; o nono, o
suor, é da paranoia (2026-10-01). A fase 0 é a parada: sem relógio, vale sempre ela (DEC-011); com o
relógio ligado, a apresentação troca a fase a cada 12 passos no estado, 5 vezes por segundo. Os
carimbos são só o preenchimento, com uma linha de contorno onde passam por cima do corpo.

| Efeito | Desenho | Onde |
|---|---|---|
| fumaça | nuvens cinza-claro que crescem a cada fase | sai da boca: de frente, do canto da boca, por fora da bochecha; de perfil, para a frente |
| bolhas | bolhas de soluço azul-claras | saem da boca e sobem, como a fumaça |
| brilhos | brilhos amarelos | em volta da cabeça, trocando de lado |
| estrelinhas | estrelas amarelas | girando em volta da cabeça, na altura da aba do chapéu |
| corações | corações vermelhos | em volta da cabeça |
| cores | losangos rosa, lilás, verde e azul | em volta da cabeça |
| poeira | nuvenzinhas brancas | saem do nariz: de frente, dos dois lados; de perfil, à frente do focinho |
| borrifo | gotinhas azuis | do frasco para o lenço, entre as mãos |
| suor | gotas de suor azul-claras, uma grande, de 10 pixels, e uma pequena, de 7, cada uma com 1 pixel na ponta e 3 na base, saltando da cabeça num arco: 2, 2 e 3 por fase | de frente, da têmpora da esquerda e por cima da ponta direita da aba, longe do braço que aponta o teto; na fase 2, a gota da direita cai sobre a ponta da aba, e uma nova sai da têmpora; de perfil, da nuca e por cima da aba, à frente |

- Nunca cobrem os traços do rosto: uma máscara dos olhos, das sobrancelhas, do nariz, do rubor e da
  boca, com 1 pixel de folga (`EfeitosPixel.AreaDoRosto`), que, na cara paranoica, inclui a gota de
  suor da têmpora com o contorno dela (`AreaDoRosto(pose, expressao)`; para as outras caras, a área é
  a de antes). Por ser uma máscara, e não um retângulo, a fumaça e as bolhas podem sair do canto da
  boca.
- Ficam a 2 pixels das bordas do quadro. O suor fica a 3 pixels ou mais da mão que aponta, nas três
  fases e com o tremidinho.
- Pendurado no cipó, nenhum carimbo cobre o cipó, as folhas ou a mão que o segura. No suor, a gota que
  cairia ali salta para o espelho da posição dela, do outro lado da cabeça, e o cipó fica com 2, 2 e 3
  gotas; nos outros efeitos, o carimbo bloqueado só some, como antes.
- **Pela onda:** bêbado → bolhas; chapado → fumaça; elétrico → brilhos; tonto → estrelinhas;
  eufórico → corações; viajando → cores; paranoico → suor. Satisfeito, alegre, relaxado e ligado não
  têm sobreposição. A poeira e o borrifo são das poses de uso (cheirar e inalar) e dos gestos de
  espirro e tosse (poeira e fumaça).
- **No app (passo T7):** só a onda da frente desenha; a de fundo, não. A sobreposição vai por cima de
  qualquer pose, inclusive dos quadros de uso, porque a onda começa ao soltar o item. Nos quadros de
  uso que já têm efeito próprio, como a fumaça do fumar ou a poeira do cheirar, podem aparecer dois
  efeitos juntos; fica para a revisão visual.

**Modificadores de pose (`EfeitosPixel.Modificar`).** O corpo reage à onda sem pose nova, só no chão:
parado, andando, descansando e nos gestos. Na parede, no cipó, no ar, segurado, escondido, espiando e
nas poses de uso, a pose fica como está (`EfeitosPixel.Modificavel`). Cada balanço passa um terço do
tempo de cada lado e um terço no meio, sem saltar mais de 10° de uma fase para a outra.

| Efeito | Modificação, nas fases 0, 1 e 2 |
|---|---|
| bolhas (bêbado) | tronco +6, 0 e −6; cabeça +4, 0 e −4 |
| fumaça (chapado) | a cabeça desce 1,5 pixel |
| estrelinhas (tonto) | cabeça −5°, +5° e 0, e 1 pixel mais baixa na fase 2 |
| brilhos (elétrico) | o quadril treme de lado: 0, +1 e −1 pixel; cauda erguida |
| corações (apaixonado) | tronco −3, 0 e +3; cauda erguida |
| cores (viajando) | cabeça −4, +4 e 0 |
| suor (paranoico) | o tremidinho: o corpo inteiro vai 0, −1 e +1 pixel de lado, sem mexer na cauda |

**Ícones do menu (`IconesDoMenu`).**
- **Rosto** (as opções da emoção dominante, DEC-027): o recorte de 40 × 32 pixels em (12, 0) do quadro
  `parado` com a cara, exatamente a célula de `previa/expressoes.png`. A prévia e o menu usam a mesma
  função (`Tela.Recortada`), e um teste compara o ícone com a célula pixel a pixel. O recorte corta o
  queixo reto na borda de baixo; refazer o contorno mudaria `expressoes.png`, e o passo T2 manteve a
  célula como está. A altura das opções e o queixo, no menu de verdade, ficam para a conferência
  [MANUAL] (TODO.md).
- **Item:** o desenho do chão, de 24 × 24, e não um ícone redesenhado de 32 pixels.
- **Ampliação:** um fator inteiro, `max(1, dpi / 96)`: 1× até 191 DPI, 2× de 192 a 287 e 3× a 288 DPI.
  É por vizinho mais próximo, sem suavização, em BGRA com alfa só 0 ou 255; `DeBaixoParaCima` inverte
  as linhas para um DIB de altura positiva.
- **No menu (passos T2 e T8):** o fator é o do DPI do monitor em que o menu abre; a marca de rádio
  vai ao lado do rosto, o que ainda falta conferir nos temas claro, escuro e de alto contraste
  [MANUAL]; em alto contraste, o menu fica só com texto, sem ícones.
- A prévia `previa/icones-menu.png` mostra os ícones a 96, 192 e 288 DPI, sobre o fundo do menu claro
  e do escuro.

## 8. Fidelidade às pranchas e ao Luffy

- **Tomado das pranchas:** pelo azul-marinho, rosto creme com bico de pelo entre os olhos, olhos
  castanhos grandes com brilho, orelhas cor de pêssego, barriga, mãos e pés creme, braços longos,
  cauda com cacho e ponta creme, tufos de pelo nas bochechas, proporções e poses (inclusive espiar
  por cima de uma borda) e o **chapéu de palha com a faixa vermelha**.
- **Semelhança com o Luffy é intencional** (DEC-019): chapéu e personalidade. Outros elementos do
  personagem podem entrar se o usuário pedir.
- **Não usado:** o nome "Pixel" da prancha (o produto é Buzzy); fala, texto ou voz (DEC-003).

## 9. Arquivos

| Arquivo | Conteúdo |
|---|---|
| `src/Buzzy.Visual/Pixel/` | Gerador: paleta, grade e máscaras (`Tela`), carimbos do rosto (`Rostos`), esqueleto e desenho (`BonecoPixel`), poses (`PosesPixel`) e ícone da bandeja de 16 × 16 desenhado à mão (`Icone`). Do tamagotchi: itens no chão e na mão (`ItensPixel`), poses de uso (`UsosPixel`), sobreposições e modificadores (`EfeitosPixel`) e ícones do menu (`IconesDoMenu`). É a fonte editável. |
| `src/Buzzy.App/Apresentacao/clipes.json` e `src/Buzzy.Visual/Animacao/` | Manifesto de clipes (DEC-036): a ordem dos quadros, os tempos, a cara e a deformação de cada situação; leitor, reprodutor e validação. O build do app reprova um manifesto que cite pose ou cara ausente (`tools/Buzzy.ValidadorDeClipes`). |
| `assets/identidade/pixel/buzzy-poses.png` | Folha nativa (64 × 64 por quadro, na ordem de `PosesPixel`); é o asset que a Fase 6 vai usar. As poses de uso e as dos gestos da onda não entram nela. |
| `assets/identidade/pixel/buzzy-itens.png` | Folha nativa dos itens: 24 × 24 por item, na ordem do menu (312 × 24). |
| `assets/identidade/pixel/previa/` | Prévias ampliadas sem suavização, geradas, não editadas à mão: parado e andando a 8×, poses em fundo claro e escuro, `expressoes.png` (as 14 caras de humor, uma por ícone do menu), tamanho real; do tamagotchi, `rostos-efeito.png` (caras de efeito, de frente e de perfil, e passageiras), `itens-8x.png`, `itens-na-mao-8x.png` (com a pega e a ponta marcadas), `itens-tamanho-real.png` (ao lado do boneco, a 2× e 1×, em fundo claro e escuro), `usos.png` (cada verbo com cada item, a 2×), `usos-tamanho-real.png`, `efeitos.png` (os nove efeitos), `gestos.png` (os oito gestos, com a sobreposição da onda), `paranoico-8x.png` (a cara `paranoico` de frente e de perfil e os gestos `olharproteto` e `agachar` com o suor na fase parada, a 8×) e `icones-menu.png`. Desde 2026-10-01, `rostos-efeito.png` tem as 8 caras de efeito. |
| `assets/identidade/arquivo-vetorial/` | Direção vetorial anterior (DEC-017), substituída; guardada só como histórico. |

**Como editar:** mude poses ou carimbos em `src/Buzzy.Visual/Pixel/`, rode
`dotnet run --project tools/Buzzy.Identidade -c Release` e confira as prévias. A ferramenta sai com
código 1 se uma pose, um item, um quadro de uso ou um efeito encostar na borda do quadro, ou se um
contorno ficar cortado (seção 5). Retoques feitos à mão na folha PNG (num editor de pixel art)
valem, desde que o gerador seja atualizado junto ou a folha passe a ser a fonte, registrado aqui.

## 10. Histórico

As mudanças de direção de 2026-09-29 a 2026-10-01 — a direção vetorial (DEC-017), a pixel art (DEC-018), o chapéu e a personalidade do Luffy (DEC-019), a toon force (DEC-023), o cipó e o esconderijo (DEC-024, DEC-025), a arte do tamagotchi e da paranoia e as correções das revisões adversariais (DEC-027, DEC-028) — estão na tabela do arquivo morto citado no topo. Daqui em diante, cada mudança de direção entra na DEC que a decide.

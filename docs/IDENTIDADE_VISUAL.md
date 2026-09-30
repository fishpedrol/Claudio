# IDENTIDADE_VISUAL.md — Identidade visual do Buzzy

> Fonte canônica da aparência do Buzzy: conceito, proporções, paleta, estilo, expressões, poses e
> regras técnicas dos assets. O produto está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md) (seção Visão); a
> direção atual está em DEC-018 (pixel art) e DEC-019 (semelhança intencional com o Luffy).
>
> **Formato:** cada seção descreve uma regra estável; mudanças de direção entram no histórico ao
> final, com data. Os valores numéricos são a referência para o gerador em
> `src/Buzzy.Visual/Pixel/`.
>
> STATUS: PLANNED — direção em pixel art criada em 2026-09-29, a pedido do usuário, fiel às pranchas
> de `assets/references/`. Folha nativa e prévias em `assets/identidade/pixel/`. A Fase 1 ainda
> mostra o sprite provisório de `src/Buzzy.App/Apresentacao/SpriteProvisorio.cs`; a integração
> animada é da Fase 6.

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
4. **Quadro:** nenhuma pose encosta na borda (conferido por `tools/Buzzy.Identidade`).
5. **Âncora por pose:** declarada no manifesto da Fase 6.

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

## 7. Poses

Poses-chave em `PosesPixel`, uma por estado ou gesto de ARCHITECTURE.md 2.6. As animações completas
(mais quadros e tempo) são da Fase 6.

| Pose | Vista | Estado ou gesto |
|---|---|---|
| `parado` | frente | `IDLE` |
| `andando-1` a `andando-4` | perfil | `WALKING` (ciclo de quatro quadros) |
| `escalando-1`, `escalando-2` | perfil | `CLIMBING` |
| `pendurado` | frente | `HANGING` |
| `impulso`, `no-ar` | perfil | `JUMPING` |
| `caindo` | frente | `FALLING` |
| `pousando` | frente | `LANDING` |
| `sentado`, `dormindo` | frente | `RESTING` |
| `segurado` | frente | `PRESSED`/`DRAGGING` |
| `reagindo` | frente | `REACTING` |
| `olhando`, `cocando`, `espreguicando`, `brincando` | frente | gestos curtos (olhar ao redor, coçar-se, espreguiçar-se, brincar) |
| `espiando` | frente | gesto curto: espiar por trás de uma borda (só cabeça e mãos acima dela; campo `Borda` da pose) |

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
| `src/Buzzy.Visual/Pixel/` | Gerador: paleta, grade e máscaras (`Tela`), carimbos do rosto (`Rostos`), esqueleto e desenho (`BonecoPixel`), poses (`PosesPixel`) e ícone da bandeja de 16 × 16 desenhado à mão (`Icone`). É a fonte editável. |
| `assets/identidade/pixel/buzzy-poses.png` | Folha nativa (64 × 64 por quadro, na ordem de `PosesPixel`); é o asset que a Fase 6 vai usar. |
| `assets/identidade/pixel/previa/` | Prévias ampliadas sem suavização (parado e andando a 8×, poses em fundo claro e escuro, expressões, tamanho real). Geradas, não editadas à mão. |
| `assets/identidade/arquivo-vetorial/` | Direção vetorial anterior (DEC-017), substituída; guardada só como histórico. |

**Como editar:** mude poses ou carimbos em `src/Buzzy.Visual/Pixel/`, rode
`dotnet run --project tools/Buzzy.Identidade -c Release` e confira as prévias. Retoques feitos à mão
na folha PNG (num editor de pixel art) valem, desde que o gerador seja atualizado junto ou a folha
passe a ser a fonte, registrado aqui.

## 10. Histórico

| Data | Mudança |
|---|---|
| 2026-09-29 | Primeira direção (vetorial): sagui-acrobata violeta-índigo, topete de três tufos, cauda com ponta menta. Registrada em DEC-017. |
| 2026-09-29 | O usuário não gostou da direção vetorial e pediu algo mais fiel às pranchas, em pixel art. Nova direção (DEC-018): macaquinho azul-marinho em pixel art 64 × 64, paleta tirada da prancha, sem o chapéu. A vetorial foi arquivada em `assets/identidade/arquivo-vetorial/`. |
| 2026-09-29 | O usuário pediu o chapéu de palha e a personalidade do Luffy ("a ideia central do projeto é essa"). O chapéu entrou na pixel art e reage às emoções; o esqueleto encolheu cerca de um pixel para caber no quadro; o ícone da bandeja ganhou o chapéu (DEC-019). |

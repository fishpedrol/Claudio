# IDENTIDADE_VISUAL.md — Identidade visual original do Buzzy

> Fonte canônica da aparência do Buzzy: conceito, silhueta, proporções, paleta, estilo, expressões,
> poses e regras técnicas dos assets. O produto e os limites de originalidade estão em
> [PRODUCT_SPEC.md](PRODUCT_SPEC.md) (seção Visão) e em DEC-014; a autorização para criar esta
> identidade sem aprovação rotineira está em DEC-015.
>
> **Formato:** cada seção descreve uma regra estável; mudanças de direção entram no histórico ao
> final, com data. Os valores numéricos são a referência para os arquivos-fonte em
> `assets/identidade/`.
>
> STATUS: PLANNED — direção criada em 2026-09-29; fontes editáveis e prévias em `assets/identidade/`.
> A Fase 1 usa o placeholder de `src/Buzzy.App/Apresentacao/SpriteProvisorio.cs`; a integração
> animada é da Fase 6.

## 1. Conceito

**Buzzy é um sagui-acrobata de pelo violeta-índigo**, pequeno, de cabeça grande, olhos verde-menta,
**topete de três tufos** que reage às emoções e **cauda longa que termina numa espiral com a ponta
menta**. Não usa roupa nem acessório: é reconhecido só pela silhueta (topete + orelhas redondas +
cauda em espiral) e pela paleta (violeta-índigo, pêssego e menta).

Da inspiração pedida pelo usuário (PRODUCT_SPEC.md, "Tradução da inspiração"), o Buzzy toma o
**espírito**: sorriso largo e fácil, postura solta e confiante, impulsividade, otimismo, curiosidade
aventureira e movimento elástico. Isso aparece no sorriso padrão aberto, no topete que se eriça, nos
braços longos e soltos e nos princípios de animação da seção 7. Não toma nenhum elemento visual
reconhecível (seção 8).

## 2. Silhueta e proporções

Quadro lógico de **128 × 128 DIP**, com a **âncora no centro da borda inferior**, entre os pés
(ARCHITECTURE.md 2.4 e 2.10). Coordenadas das fontes: origem na âncora, x para a direita, y para
cima negativo (convenção do SVG com a âncora em (0, 0)).

| Parte | Medida (DIP) | Observação |
|---|---|---|
| Altura em pé, até a ponta do topete | ~124 | cabe no quadro com folga |
| Cabeça | círculo de raio 27, centro a 84 acima da âncora | ~44% da altura: proporção de filhote |
| Topete | 3 tufos, até 15 acima da cabeça | inclinado para trás; varia com a emoção |
| Orelhas | círculos de raio 10,5 nas laterais da cabeça | mais baixas que os olhos |
| Rosto | "máscara" em coração, 40 de largura | pêssego; os olhos ficam dentro dela |
| Olhos | elipses 14 × 17, íris menta | grandes, levemente convergentes |
| Tronco | elipse 36 × 40 | barriga pêssego 23 × 28 |
| Braços | ~48 do ombro à ponta da mão (20 + 28) | muito compridos: em pé, as mãos ficam perto do chão; permitem pendurar-se com o topete abaixo dos punhos |
| Pernas | 22 do quadril à borda visual da sola | curtas; pés grandes, bons para agarrar; no perfil, coxa e pé são partes separadas para o pé ficar plano no chão |
| Cauda | ~70 de comprimento | espiral de 1,5 volta; ponta menta |

**Teste de silhueta:** preenchida de uma cor só, a figura deve ser reconhecível pelo topete de três
tufos, pelas duas orelhas redondas baixas e pela espiral da cauda. A prévia
`assets/identidade/previa/silhuetas.png` mostra esse teste para cada pose.

## 3. Paleta

| Nome no arquivo-fonte | Cor | Uso |
|---|---|---|
| `pelo` | `#574AA0` violeta-índigo | cabeça, corpo, membros, cauda |
| `pelo-sombra` | `#43397E` | membro de trás, lado de dentro das orelhas quando de perfil |
| `contorno` | `#211B42` índigo escuro | contorno de tudo, pupilas, traços do rosto |
| `rosto` | `#F6DFC6` pêssego | máscara do rosto, barriga, mãos, pés, focinho |
| `rosto-sombra` | `#E4C3A2` | dobras das mãos e pés |
| `orelha` | `#EFC0A8` pêssego-rosado | parte interna das orelhas |
| `menta` | `#3CCFA3` | íris e ponta da cauda (a cor de marca) |
| `menta-escura` | `#22A882` | anel da íris |
| `branco` | `#FFFFFF` | esclera e brilho dos olhos |
| `boca` | `#6B2344` | interior da boca |
| `lingua` | `#E8768F` | língua |

A paleta evita de propósito o vermelho, o azul-marinho e o amarelo-palha das referências e da
inspiração (seção 8). Contraste: o pêssego e a menta se destacam em papel de parede escuro; o
contorno índigo escuro e o violeta se destacam em papel de parede claro.

## 4. Estilo de traço e cor

- Cores chapadas, com no máximo um tom de sombra por material; sem degradê, textura ou sombra suave.
- Contorno contínuo de 2,2 DIP na cor `contorno`, com junções e pontas arredondadas.
- Pelo sugerido só pelo recorte dos tufos do topete; nada de pelos soltos na borda.
- Sem sombra projetada no sprite. Se um dia houver sombra, ela fica numa janela separada sem
  interação (ARCHITECTURE.md 2.13.7, item 7).

## 5. Regras técnicas dos assets

1. **Borda dura.** Só alfa exatamente 0 deixa o clique passar (P1). Fora de uma faixa de 2 px ao
   redor da silhueta, todo pixel é alfa 0 ou 255 (critério 3 da Fase 6). O renderizador pode
   suavizar só essa faixa ou forçar 0/255, como o placeholder faz.
2. **Vetorial e por DPI.** As fontes são vetoriais; cada quadro é rasterizado no DPI do monitor em que
   está a âncora, para ficar nítido em 100%, 150% e 200%.
3. **Espelhamento.** Poses de perfil são desenhadas olhando para a direita; a esquerda é o espelho
   horizontal. Nenhum detalhe assimétrico pode ficar errado no espelho.
4. **Âncora por pose.** Em pé, a âncora é o centro entre os pés. Pendurado, é o ponto de apoio das
   mãos; escalando, o ponto de contato com a parede. O manifesto da Fase 6 declara a âncora de cada
   clipe.
5. **Tamanho mínimo legível:** 64 DIP (menor passo de escala previsto para a Fase 8).
6. **Sem texto na arte.** O personagem não exibe texto (Q-12, DEC-003).

## 6. Expressões

Camadas do rosto, independentes do corpo (ARCHITECTURE.md 2.6: trocar expressão não muda estado nem
posição). Cada expressão combina olhos, sobrancelhas, boca e o estado do topete.

| Expressão | Olhos | Sobrancelhas | Boca | Topete |
|---|---|---|---|---|
| `neutro` | abertos | neutras | sorriso fechado | normal |
| `feliz` | felizes (arcos) | neutras | sorriso aberto | normal |
| `rindo` | felizes | erguidas | sorriso aberto grande | eriçado |
| `curioso` | abertos, olhando para o lado | uma erguida | boca de canto | normal |
| `surpreso` | arregalados | erguidas | "o" | eriçado |
| `assustado` | arregalados, pupila pequena | preocupadas | ondulada | eriçado |
| `sonolento` | meio fechados | caídas | reta | caído |
| `bocejando` | fechados | caídas | "o" grande | caído |
| `dormindo` | fechados | nenhuma | reta pequena | caído |
| `travesso` | piscada | uma erguida | língua de fora | normal |
| `entediado` | meio fechados, olhando para cima | retas | reta | caído |
| `pensativo` | olhando para cima | uma erguida | boca de canto | normal |
| `empolgado` | abertos com brilho em estrela | erguidas | sorriso aberto | eriçado |
| `determinado` | abertos | bravas | sorriso fechado firme | normal |

## 7. Poses e princípios de animação

Poses-chave desenhadas em `assets/identidade/buzzy-poses.json`, uma por estado ou gesto de
ARCHITECTURE.md 2.6. As animações completas (mais quadros e interpolação) são da Fase 6.

| Pose | Vista | Estado ou gesto |
|---|---|---|
| `parado` | frente | `IDLE` |
| `andando-1` a `andando-4` | perfil | `WALKING` (ciclo de quatro quadros) |
| `escalando-1`, `escalando-2` | perfil, de frente para a parede | `CLIMBING` |
| `pendurado` | frente | `HANGING` |
| `impulso`, `no-ar` | perfil | `JUMPING` (antecipação e extensão) |
| `caindo` | frente | `FALLING` |
| `pousando` | frente | `LANDING` (compressão) |
| `sentado`, `dormindo` | frente | `RESTING` |
| `segurado` | frente | `PRESSED`/`DRAGGING` (pendurado pelo topete, membros soltos) |
| `reagindo` | frente | `REACTING` (pulinho de braços abertos) |
| `olhando`, `cocando`, `espreguicando` | frente | gestos curtos (olhar ao redor, coçar-se, espreguiçar-se); espiar e brincar ganham poses com os clipes da Fase 6 |

Princípios, tirados do espírito da inspiração: **antecipação** antes de saltar (agachar e comprimir),
**comprimir e esticar** no salto e no pouso, **impacto** ao pousar (quadro curto de compressão),
**acompanhamento** da cauda e do topete depois do corpo, **exagero** moderado nas reações, e sorriso
aberto como expressão de repouso quando a energia está Alta.

## 8. Distinção e originalidade

Conferido nesta direção (critério de PRODUCT_SPEC.md):

- Sem chapéu, faixa, colete, bermuda, sandália, cicatriz, símbolo, bandeira, fala ou bordão; sem
  membros que esticam como poder.
- Paleta sem vermelho, sem azul-marinho e sem amarelo-palha: nada da combinação vermelho/azul/palha.
- Silhueta própria: topete de três tufos, orelhas redondas baixas e cauda em espiral com ponta menta.
  Nenhum desses elementos vem da inspiração nem das pranchas.
- Das pranchas de referência (fornecidas pelo usuário) ficam só ideias gerais: primata pequeno de
  cabeça grande, rosto claro, cauda enrolada e a lista de poses e expressões úteis. Nome "Pixel",
  chapéu, pelo azul-marinho e render 3D não foram usados.

## 9. Arquivos

| Arquivo | Conteúdo |
|---|---|
| `assets/identidade/buzzy-partes.svg` | Biblioteca de partes vetoriais (fonte editável). Cada parte é um `<g id="...">` desenhado em coordenadas locais com o pivô em (0, 0); as cores vêm das classes CSS da paleta. |
| `assets/identidade/buzzy-poses.json` | Poses e expressões: para cada pose, a lista ordenada de partes com posição, rotação e escala, e a expressão do rosto. |
| `assets/identidade/previa/` | Prévias renderizadas a partir das fontes: folha de modelo, expressões e silhuetas. Geradas por `tools/Buzzy.Identidade`; não são editadas à mão. |

**Como editar:** mude as partes no SVG (qualquer editor vetorial que preserve `id` e `class`) ou as
transformações no JSON, rode `dotnet run --project tools/Buzzy.Identidade -c Release` e confira as
prévias. O formato é um subconjunto de SVG (`g`, `path`, `circle`, `ellipse`, `transform`, `class`,
`fill`, `stroke`) lido pelo renderizador do Buzzy; recursos fora dele são ignorados.

## 10. Histórico

| Data | Mudança |
|---|---|
| 2026-09-29 | Direção original criada por Claude sob DEC-015: sagui-acrobata violeta-índigo, topete de três tufos, cauda em espiral com ponta menta, paleta violeta/pêssego/menta, estilo chapado de borda dura, boneco de recorte vetorial. |
| 2026-09-29 | Ajustes depois das prévias: braços alongados para ~48 DIP (com 38, pendurado na borda superior a cabeça passava da borda); pé de perfil separado da coxa (o calcanhar da perna de trás afundava no chão na caminhada); poses mantêm o corpo dentro do quadro, porque pulo, queda e reação movem a janela. As 19 poses cabem no quadro de 128 × 128 DIP (conferido pela ferramenta). |

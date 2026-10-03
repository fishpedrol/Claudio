> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de a crítica de integração do tamagotchi, que resolveu os conflitos entre os desenhos do núcleo, do app e da arte (conflitos C…, lacunas L…, fatos F…; seções 0 a 6) e deu a ordem T1–T9 e A1–A4 (o código a cita como "crítica, C11", "crítica, L13", "crítica, seção 4" e semelhantes), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a emoção dominante e o tamagotchi. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-027 e DEC-028) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

**Crítica de integração: DEC-027 (emoção dominante) e DEC-028 (tamagotchi adulto)**

Trabalhei só lendo arquivos. Não compilei nem executei nada. Li o núcleo (`Maquina`, `Tipos`, `Eventos`, `Efeitos`, `EstadoDoNucleo`, `Configuracao`, `Movimento`, `Nucleo`, `Aleatorio`, `Gravacao`, `Posicionador` e `Persistencia/*`) e o app (`Aplicacao`, `MenuNativo`, `Win32`, `Textos.*`, `JanelaPersonagem`, `PoseDoPersonagem`, `SpriteProvisorio`). Também li `Buzzy.Visual/Pixel/*`, `PreviaPixel`, `Regras.cs` e `Portao.cs` do portão, `tools/testar.ps1`, os testes citados, os documentos de `docs/`, o `CONTINUIDADE.md` e a crítica da Fase 5.

**Resumo.** Os três desenhos são compatíveis na intenção, mas divergem em quase todo nome e mecanismo. Proponho uma base só: o núcleo segue o projetista do núcleo, a arte segue o projetista da arte, e do projetista do app ficam só o menu nativo e as janelas. Achei cinco testes existentes que quebrariam sem ninguém ter previsto (F1–F5).

## 0. Fatos do código que mudam a leitura dos desenhos

- **F1. O sorteio de expressão depende do tamanho do enum.** `DecidirParado` sorteia com `Entre(0, Enum.GetValues<Expressao>().Length - 2)` (Maquina.cs:879). A referência 04 usa `# acoes: …TrocarExpressao`. Acrescentar valores a `Expressao` muda a 04, a menos que o sorteio passe a usar a lista fixa `Expressoes.DeHumor` (D11 do núcleo). O mesmo sorteio sobre o enum inteiro aparece em `InvariantesTestes.cs:880` e em `Entrada/ArbitroPropriedadesTestes.cs:305`, que nenhum desenho cita.
- **F2. O invariante 11 percorre todos os valores de `Estado`** (`InvariantesTestes.cs:152-170`). Por isso `Estado.Using` só pode entrar no enum no mesmo passo em que o gerador de testes cria entrada e saída dele.
- **F3. Todo tipo de `Evento` precisa de um exemplo** em `EsquemaDeConfiguracoesTestes.Politica_Imediata_SoSuspensaoFimDeSessaoSairEBloqueio` (linhas 549-562). O teste descobre os tipos por reflexão, então os 9 eventos novos o quebram se não entrarem na lista.
- **F4. O invariante 18 faz ida e volta pelo esquema.** `InvarianteDezoito.Violacao` (linhas 46-48) compara `Preferencias` por valor depois de gravar e ler. Se o núcleo aceitar a emoção antes de o esquema gravá-la, toda sequência com emoção escolhida seguida de `GravarPosicao` falha. Núcleo e esquema da DEC-027 precisam entrar juntos.
- **F5. Uma chave de arte ausente derruba o app.** `BonecoPixel.Desenhar` indexa `Rostos.Expressoes[...]` (BonecoPixel.cs:114), `SpriteProvisorio.Renderizar` lança exceção para pose desconhecida (linhas 55-56), e o `DispatcherUnhandledException` (Aplicacao.cs:134-139) não marca `Handled`.
- **F6. `Buzzy.Visual` não referencia o núcleo.** Os dois lados se ligam pelo nome em minúsculas (`NomeDaExpressao`), e já há dois desencontros:
  - `Eletrico` (núcleo) contra `acelerado` (arte);
  - `LancaPerfume`, que vira `"lancaperfume"`, contra a chave `"lanca"` da arte.
- **F7. `expressoes.png` é um recorte de 40×32 em (12, 0)** do quadro `parado` (PreviaPixel.cs:47). Não é 32×32 em (16, 1), como diz o app, nem em (16, 2), como diz a arte.
- **F8. No esconderijo, a boca fica fora do quadro.** Na pose `escondido` (QuadrilY 84), o centro da cabeça fica em y≈57,8 e o carimbo da boca cai em y=64-67. "Usar escondido" não tem boca visível.
- **F9. A cara passada pelo retrato só aparece em quatro situações:** `WALKING`, `HANGING`, `IDLE` sem gesto e `PEEKING` (`PoseDoPersonagem.Escolher`). Nas outras poses vale a cara própria da pose, então a emoção dominante e as caras de efeito só se veem nessas quatro.
- **F10. A legenda dos carimbos sobrescreve em silêncio.** `Carimbo.Legenda` usa inicializador por indexador (`['K'] = …`), então uma letra repetida substitui a anterior sem erro.
- **F11. A P7 da Fase 5 (persistência ligada) ainda não entrou.** `Iniciar` envia `Preferencias.Padrao` (Aplicacao.cs:176) e `GravarPreferencias` só gera o log `efeitoPendente` (linhas 680-687). Antes do bloco B, a emoção escolhida não sobrevive a reabrir o app.
- **F12. O log do menu é contrato da verificação.** `Verificacao.cs:653` espera `MENU|fechado=AlternarVisibilidade|Sair` (nomes de `ComandoDoMenu`), e `C10SairPeloMenu` usa a tecla S.
- **F13. Portão de APIs:**
  - o portão também lê `src/Buzzy.Visual` (`testar.ps1:97-99`), embora a SECURITY 8.1 ainda diga só App e Core;
  - `InsertMenuItemW`, `CreateDIBSection` e `DeleteObject` não estão em `Regras.cs`;
  - `WindowFromPoint` é proibido no produto, mas `tests/Buzzy.Verificacao/Nativo.cs:154` já o usa, e essa pasta não é lida pelo portão.
- **F14.** `Maquina`, `Maquina.Passo` e `Aplicacao` ainda não são `partial`. O cache de `SpriteProvisorio` não tem limite.
- **F15. Numeração da Fase 5:** invariantes 18 a 21 (crítica, C2) e só a referência 06 (passo P13a).

## 1. Conflitos entre os desenhos e resolução

| # | Tema | Divergência | Resolução |
|---|---|---|---|
| C1 | Modelo do efeito | `Onda` com frente, fundo, níveis e precedência (núcleo); `Alteracao` única, que substitui a anterior, com água e banana como restauradores (app); cara e duração por item (arte 4.11) | Fica a onda do núcleo (D7–D12, tabelas 4.1–4.5). Descartar app 4.3 e arte 4.11. |
| C2 | Temporizador | `ItemEffectTimer`, `AgendarOnda`, `CancelarOnda` contra `ItemEffectPhaseEnd`, `AgendarFimDaAlteracao` | Nomes do núcleo; log `ONDA`. |
| C3 | Caras de efeito | Mais 7 valores no fim de `Expressao` (núcleo e arte) contra caras só na apresentação (app D6) | Mais 7 no enum, com o sorteio pela lista fixa `DeHumor` (isso resolve o motivo do D6). Um nome só: `Eletrico`, com chave `"eletrico"` na arte. Descartar zonzo, ressaca, larica, relaxado e pegando do app. |
| C4 | Opções do menu de emoção | 14 + "Automática" (núcleo e app) contra 19, com caras de efeito e sem dormindo e bocejando (arte 4.10) | 14 + "Automática", na ordem de `expressoes.png` (o usuário pediu "use as expressões png"). `expressoes.png` continua com 14; as caras novas vão para `rostos-efeito.png`. |
| C5 | A dominante muda pesos da agenda? | Não, invariante 27 (núcleo); sim, tendência por emoção (app 4.4) | Não. A DEC-027 diz "cara de base e a mais frequente". Mudar pesos exige pedido novo do usuário. |
| C6 | Sorteio com a dominante | `[6,1,1,1,1]`, que pode repetir a cara atual (núcleo) contra 6/2 sem a atual (app) | Fica o núcleo: 60% do tempo com a dominante. Custo: 36% das trocas não mudam nada, o que precisa ficar escrito. Nos dois casos cada sorteio continua gastando um `Sortear()`. |
| C7 | Enum dos itens | `Item` na ordem do usuário, com `Md` (núcleo) contra `TipoDeItem` agrupado, com `MD` (app) | `Item` na ordem da resposta 2 do usuário. A chave da arte é `Item.ToString().ToLowerInvariant()`, então a arte usa `"lancaperfume"`. Separadores por grupo, se entrarem, ficam numa lista de exibição do menu, com teste de que ela cobre o enum inteiro. |
| C8 | Verbo de uso | Bala: Comer, Engolir, Engolir. Cogumelo: Comer, Comer, Engolir (núcleo, app, arte) | Uma tabela só, no núcleo (`TabelaDoTamagotchi`): bala e MD = Engolir; banana e cogumelo = Comer. |
| C9 | Duração do uso | 60–210 passos por item (núcleo), roteiros de 60–216 (app), 52–102 por verbo (arte) | A duração é a da sequência de quadros da arte, por verbo. O núcleo guarda o número, e um teste no App.Testes confere `PassosDoUso == soma dos quadros do verbo`. |
| C10 | Cara durante o uso | `CaraDurante` no retrato (núcleo) contra a cara de cada quadro (arte) | No chão manda a cara da pose (a apresentação passa `null`). `CaraDurante` vale só nos apoios sem pose de uso (C25). |
| C11 | Tamanho do item | 48 DIP, 24 px de arte (núcleo e arte) contra 40 DIP, 20 px (app) | 48 DIP, 24 px. |
| C12 | Máximo de itens | 6 contra 5 | 6. |
| C13 | Onde o item nasce | 140 DIP acima, caindo (núcleo) contra 24 DIP acima com impulso de 420 DIP/s (app) | Núcleo. |
| C14 | "Solto sobre ele" | O núcleo decide pelos retângulos com margem de 20%, sem regra escrita, contra a raiz decidir por limites opacos e mandar `ItemDrop(sobre)` | O núcleo decide, com a regra escrita: o retângulo do item, já preso na área útil, cruza o retângulo do personagem encolhido 20% de cada lado. Se a verificação V4 mostrar erros de alvo, a raiz passa a informar o fato, como já faz no `PRESS`. |
| C15 | Botão direito no item | Abre o menu (núcleo) contra remove o item, `ItemRemove` (app) | Abre o menu, como no personagem (Q-03). Remover é pelo "Recolher itens". |
| C16 | Uso interrompido | A onda vale desde o soltar e o uso só acaba (núcleo) contra ponto de consumo e item recriado com Id novo (app) | Núcleo. |
| C17 | Item solto com ele no ar | Recusado, o item cai (núcleo) contra ele pega e usa ao pousar (app) | Núcleo. |
| C18 | Quando ele fica atento | `ItemPress` (núcleo) contra `ItemDragStart` (app) | `ItemPress`. |
| C19 | Efeitos das janelas dos itens | Efeitos separados (`MostrarItem`, `MoverItem`, `EsconderItem`, `RemoverItem`) contra um instantâneo `AtualizarItens` | Núcleo. A raiz pula só um `MoverItem` que tenha outro do mesmo Id adiante no lote; mostrar, esconder e remover nunca são pulados. |
| C20 | Versão do esquema | v2 com o campo sempre escrito (núcleo) contra v1 condicionada à P7, com o campo só quando há valor (app) | v2, sempre com `"automatica"` ou o nome. A versão descreve o formato do arquivo, não quem o grava; `VersaoFutura` protege um build antigo. |
| C21 | `Retrato` | Propriedades `init` (núcleo) contra parâmetros opcionais no fim (app) | `init`. |
| C22 | Gestos novos | Soluço, dança, gargalhada, espirro, tosse, tremedeira (núcleo) contra cambalear, soluçar, dançar, tremer, viajar (app); a arte não desenhou nenhum | Os seis do núcleo (ver L11). |
| C23 | Chave `Tamagotchi` | Ligada só no fim (núcleo D15) contra `Itens = true` já de início (app) | D15. |
| C24 | Paleta, legenda, carimbos, partículas, ícones | 35 cores e uma legenda única (arte) contra 30 cores, `LegendaDosItens`, apêndices A e B, 12 partículas e ícone de 16×16 (app) | Arte. Descartar a seção 4.7 e os apêndices A e B do app. |
| C25 | Poses de uso por apoio | Só no chão, de frente (arte); quatro apoios sem desenho conferido (app); o núcleo aceita o uso nos quatro apoios | Na primeira entrega, o chão usa as poses da arte. Na parede, no cipó e no esconderijo aparece a pose do apoio com `CaraDurante` e a sobreposição, sem objeto na mão. Poses por apoio vêm depois (A5): a parede é de perfil e ainda não tem `Pontos` de perfil, e no esconderijo a cabeça precisa subir cerca de 5 px (F8). |
| C26 | Numeração | Invariantes 22–29 e referência 07 (núcleo) contra invariantes 21–26 e referências 10–11 (app) | Núcleo, porque o invariante 21 é da Fase 5. |
| C27 | Achatamento do item ao pousar | Nenhum (núcleo), 5 passos com relógio ligado (app), `Deformada` (arte) | Sem deformação na primeira entrega. Se entrar, o núcleo conta os passos e o invariante 29 inclui "item pousando"; sem isso, o quadro achatado fica congelado quando o relógio desliga. |
| C28 | Cache de quadros | LRU de 16 MB (app) contra 256 quadros (arte) | LRU por bytes, 16 MB; `QuadrosEmCache` continua no log. |
| C29 | Ícone do rosto no menu | 32×32 em (16, 1) (app) contra 32×32 em (16, 2) com contorno refeito (arte) | O recorte exato da prévia (12, 0, 40×32), numa função única de `Tela` usada pela prévia e pelo menu. |
| C30 | Orientação do DIB | Altura positiva, com inversão das linhas (app), contra de cima para baixo (arte) | Altura positiva, como o exemplo que o app cita; o teste `DeBaixoParaCima` cobre a inversão. |

## 2. Lacunas e erros factuais

**Lacunas**

- **L1.** Acrescentar à lista de `Politica_Imediata` exemplos dos 9 eventos novos, todos com gravação adiada (F3).
- **L2.** Implementar a emoção no núcleo e no esquema no mesmo passo (F4).
- **L3.** `Estado.Using` entra junto com os itens e com o gerador do `InvariantesTestes` (F2).
- **L4. O item na mão fica sempre visível.** Com a fórmula `ItemVisivel` do núcleo, o item arrastado para um monitor em tela cheia some no meio do gesto, contra o espírito do invariante 14.
- **L5. Um relógio só.** `Visivel && AlgumCaindo` liga o relógio com um item invisível caindo (num monitor ocupado), enquanto o invariante 29 fala em "item visível caindo". Resolver assentando no chão o item que fica invisível, como já acontece ao esconder.
- **L6.** `CmdClearItems` precisa emitir `LiberarCapturaDoItem` para o item que está na mão, ou pular esse item.
- **L7.** `FicarAtento` durante o foguete precisa limpar `Movimento.Foguete`. Na pose, o caso `Climbing when Foguete` vem antes de `Agarrado`, e sem isso o `impulso` esticado congela no meio da parede.
- **L8. O oráculo R-b também muda.** `InvariantesTestes.cs:318` e as linhas 328-330 exigem relógio desligado em `IDLE` sem gesto, `PRESSED` e `DRAGGING`. Com um item caindo, o relógio liga nesses estados. O núcleo só citou o invariante 29.
- **L9.** Em `ArbitroPropriedadesTestes.cs:305`, sortear com `rnd.Next(14)`, como no `InvariantesTestes`.
- **L10.** `SobreOPersonagem(..., margemPercentual)` precisa da regra escrita (C14).
- **L11. Os seis gestos novos não têm pose.** Mapeamento provisório:
  - Danca → `brincando`;
  - Gargalhada → `reagindo`;
  - Tremedeira → `parado`, deslocado ±1 px;
  - Soluco, Tosse e Espirro → `parado` com a sobreposição.
- **L12. Falta o mapeamento de onda para sobreposição:**
  - Bebado → Bolhas;
  - Chapado → Fumaça;
  - Eletrico → Brilhos;
  - Tonto → Estrelinhas;
  - Euforico → Corações;
  - Viajando → Cores;
  - as demais, nenhuma.
- **L13. Menu:**
  - esconder o submenu "Itens" com a chave desligada;
  - separar a lista de entradas (função pura, testável sem Win32) da montagem no HMENU;
  - anexar cada submenu logo depois de criá-lo, porque um submenu solto num erro vaza um objeto USER.
- **L14. Itens pequenos são difíceis de agarrar.** Só pixels opacos recebem clique: MD e cigarro ficam com cerca de 24×18 e 36×12 DIP a 100%. Medir na V3; se ficar ruim, altura mínima de 6 a 8 px de arte.
- **L15. Com a chave desligada, o evento não é ignorado por completo.** `Tratar` encerra o gesto por prioridade antes do `switch` (Maquina.cs:63-64), e isso reagenda a agenda. Conferir a chave antes, nos eventos de item, ou ajustar o teste `Invocar_…_EhIgnorado`.
- **L16. Escalada lenta com o mesmo tempo na parede.** Com a escalada a 50–60% e `TempoNaParede` sem mudar, o disparo da agenda corta a subida (é o problema da DEC-022, item 5). Aceitar como piada documentada ou escalar `TempoNaParede` por 100/Velocidade.
- **L17. Ordem Z durante o arraste do item.** Abaixo do personagem, o item some atrás dele justamente quando vai ser solto sobre ele. Usar `TrazerParaFrente` no gesto e `ColocarAbaixoDe` depois, sempre por evento, nunca por timer (SECURITY 2).
- **L18. `JanelaDoItem` herda problemas da Fase 5:** o defeito de DPI da janela (crítica, C16/L1) e a minimização pelo Windows ao desconectar um monitor (S8/P12). P12 e P14 passam a valer também para as janelas de item.
- **L19.** A persistência da emoção entre execuções depende da P7 (F11).
- **L20. Documentos a atualizar:**
  - SECURITY 8.1: as fontes que o portão lê;
  - SECURITY 3.1: captura do mouse também nas janelas de item e as APIs do menu;
  - SECURITY 5: o campo novo; onda, uso e itens ficam só em memória.

**Erros factuais**

- **E1. Arte:** "acrescentadas no fim, as referências não mudam" só vale se o sorteio passar para a lista fixa (F1).
- **E2. Arte e app:** "o mesmo recorte de `expressoes.png`" está errado; o recorte é (12, 0), 40×32 (F7).
- **E3. App:** o invariante 21 já é da Fase 5, e a Fase 5 só reserva a referência 06, não 06 a 09.
- **E4. App:** a versão condicionada à P7 não se sustenta. `VersaoAtual == 1` e "a versão 2 é futura" já estão afirmados em `EsquemaDeConfiguracoesTestes` (linhas 530-537).
- **E5. Arte:** 19 opções de emoção, sem dormindo e bocejando, contrariam o pedido do usuário e o núcleo, que ignora caras fora das 14.

## 3. Fase 5 em andamento: o que esperar e como evitar conflito

**Bloco A, o que está em andamento agora.** Pelos arquivos citados e pelo `git status`, o bloco A corresponde aos passos P1 a P5; a correspondência exata é inferência. Os arquivos em uso são:
- núcleo: `Eventos.cs`, `Gravacao.cs`, `Maquina.cs` (Carregar e Validar), `Posicionador.cs`, `Persistencia/*`;
- app: `Programa.cs`, `Aplicacao.cs`, `ArquivoDeConfiguracoes.cs`, `PastaDeDados.cs`, `InstanciaUnica.cs`;
- testes: `InvariantesTestes`, `SimuladorDeTempo`, `EsquemaDeConfiguracoesTestes`, `PropriedadesDaPersistenciaTestes`, `RestaurarTestes`, `InvarianteDezoito`, `BuzzyEmTeste`, `PerfilDeTeste`, `PlataformaTestes`, `IsolamentoTestes`, `ArquivoDeConfiguracoesTestes`;
- verificação e ferramentas: `tests/Buzzy.Verificacao/*` e `tools/{testar,medir-desempenho}.ps1`.

Regra: nada muda na árvore principal até o bloco A estar mesclado e verde, porque o outro workflow compila a árvore e roda os testes. A arte pode andar agora numa cópia isolada, como foi feito com a `f4`, porque só toca `src/Buzzy.Visual/Pixel/*`, `tools/Buzzy.Identidade/*` e testes novos.

**Blocos B a D, que vêm depois do tamagotchi, precisam preservar:**
- **P7:** passar as preferências lidas, com a emoção, no `Loaded`; ligar a verificação V16. Também mexe em `Iniciar`, `ExecutarEfeito` e `EncerrarAplicacao`, que o tamagotchi terá tocado.
- **P8:** tratar `Using` como `Reacting` nas classes A e T; `ReacomodarItens` segue a mesma regra de rebase.
- **P12:** cuidar da minimização das janelas de item (L18).
- **P13:**
  - o recuo do cambaleio não pode disparar a travessia pela lateral de trás;
  - o estado atento espera a travessia em curso terminar, como a pausa;
  - `PesoAtravessar` entra no `PerfilEfetivo`.
- **P14:** a correção de DPI vale também para `JanelaDoItem`. Nesse momento, extrair o gancho comum às duas janelas.
- **P15:** `--fase tamagotchi` convive com `--fase 5` no mesmo `Programa.cs` da verificação.
- **L8 da crítica da Fase 5** (item "Redefinir posição" no menu): não usar as teclas D e I.

## 4. Riscos às regras duras

- **O usuário prevalece:**
  - `PRESS` em `USING` leva a `PRESSED` no mesmo evento (`AceitaPressionar`);
  - o item na mão é sempre visível (L4);
  - esconder ou sair no meio do arraste do item solta a captura;
  - o estado atento só pausa a agenda e termina com `ItemRelease`, `DragCancel` ou "Recolher itens";
  - as janelas de item nunca são ativadas (`WS_EX_NOACTIVATE`, `MA_NOACTIVATE`, `ShowActivated = false`), e a verificação confere o primeiro plano.
- **Sem timer periódico:**
  - cada onda gera no máximo "2 + nível" disparos únicos de 1 s ou mais, e o episódio mais longo dura 530 s;
  - `CancelarOnda` em `EXITING`, e o temporizador para em `EncerrarAplicacao`;
  - o relógio só corre com item caindo, uso ou gesto; C27 e L5 são as duas formas de quebrar isso;
  - sobreposição sem relógio fica parada na fase 0.
- **Determinismo:**
  - cada sorteio de cara continua gastando exatamente um `Sortear()`;
  - os gestos da onda usam `Ponderado` e depois `Entre`, os mesmos dois sorteios de hoje;
  - o cambaleio usa só aritmética básica, e as posições arredondam `AwayFromZero`;
  - `ItensNoMundo` precisa de igualdade por valor, porque o teste de `EXITING` compara `depois == antes`;
  - as referências 01 a 05 devem ficar idênticas byte a byte a cada passo.
- **Segurança e portão de APIs:**
  - as três APIs novas ficam só em `Win32.cs`; o DIB é preenchido por `Marshal.Copy`, sem `BitBlt`, `StretchBlt` ou `CreateDC`;
  - nenhum `WindowFromPoint` ou `GetForegroundWindow` no produto;
  - `emocaoDominante` é lida de uma lista fechada, nunca com `Enum.Parse`;
  - código novo em `Buzzy.Visual` não pode conter palavras da lista proibida;
  - uma falha de `CreateDIBSection` vai para o log só com o código, porque o teste `ExcecoesDoSistema…` barra o uso de `.Message`.
- **GDI e USER sem vazamento:**
  - HBITMAP criado a cada abertura do menu e apagado com `DeleteObject` depois do `DestroyMenu`, que não apaga `hbmpItem`;
  - o `using` fica declarado antes do `try`, e o log mostra criados = apagados;
  - submenus anexados na hora (L13); alto contraste sem ícones;
  - janelas de item fechadas ao remover e ao sair;
  - cache de quadros limitado (C28).
- **Conteúdo:**
  - só nomes no `.resx`, sem descrição, dose, preparo ou obtenção;
  - carimbos sem texto nem marca;
  - se o repositório virar público (Q-10), os nomes ficam visíveis; a decisão é do usuário, como foi com o chapéu na DEC-019.

## 5. Ordem de implementação

Cada passo termina com `tools/testar.ps1` verde, as referências 01 a 05 idênticas e uma entrada no `CONTINUIDADE.md`.

- **P0.** Esperar o bloco A. A arte (A1–A4) segue numa cópia isolada.
- **T1. Emoção dominante no núcleo e no esquema, juntos.**
  - **Arquivos:**
    - `Tipos.cs`: classe `Expressoes` (`DeHumor`, `EhDeHumor`, `Companheiras`);
    - `Eventos.cs`: `CmdSetDominantEmotion` e a propriedade `init` `Preferencias.EmocaoDominante`;
    - `Maquina.cs`: `Tratar`, `EscolherEmocao`, `Sanear`, `Carregar` e `MudarPreferencias`; os sorteios de cara em `DecidirParado`, `Decidir` (esconderijo e acordar) e `DecidirPreso`; `VoltarACaraDeBase` no fim de `REACTING` e `LANDING`;
    - `EstadoDoNucleo.cs`: retrato com a emoção e `emocao=`;
    - `Gravacao.cs`;
    - `EsquemaDeConfiguracoes.cs`: versão 2.
  - **Testes:**
    - `EmocaoDominanteTestes`;
    - `EsquemaDeConfiguracoesTestes`: texto da v2, v1 lida sem aviso, emoção inválida, versão futura 3, `Politica_Imediata` com o evento novo;
    - `PropriedadesDaPersistenciaTestes`;
    - `ReproducaoTestes`: ida e volta e `emocao=` só quando há valor;
    - `InvariantesTestes` e `ArbitroPropriedadesTestes` com `rnd.Next(14)`, R1 da emoção e invariante 27;
    - `InvarianteDezoito`.
- **T2. Menu de emoção com os rostos.**
  - **Arquivos:**
    - Visual: `Tela.Recortada`, `IconesDoMenu.Rosto` e `Ampliar`, com `PreviaPixel` passando a usar o mesmo recorte;
    - App: bloco novo em `Win32.cs`, `BitmapsDoMenu.cs`, `MenuNativo.cs` (lista pura + montagem; `fechado` continua com o nome de `ComandoDoMenu`), `Textos.resx/.cs`, `ExibirMenuDoDesktop`.
  - **Testes:**
    - `PlataformaTestes`: tamanhos 80 e 40, textos, teclas de acesso do menu principal e das 15 opções de emoção;
    - `MenuNativoTestes`, `IconesDoMenuTestes` (o ícone é igual à célula da prévia) e `DeBaixoParaCima`;
    - integração: `Menu20Vezes_GdiEstavel` e `EmocaoPeloMenu`, este N/A se o laço do menu ignorar teclado postado.
  - **Documentos:** a DEC-027 fica fechada, exceto a persistência entre execuções, que espera a P7.
- **T3. Tipos e tabelas do tamagotchi, ainda sem comportamento.**
  - **Arquivos:** `Tamagotchi.cs`, `TabelaDoTamagotchi.cs`, `Tipos.cs` (mais 7 valores de `Expressao` e 6 de `Gesto`, no fim), `Configuracao.cs` (`Tamagotchi = false`), `Movimento.cs` (parâmetros do item).
  - **Testes:** `TabelaDoTamagotchiTestes`.
- **T4. Onda da frente, atrás da chave.**
  - **Arquivos:**
    - `Maquina.cs` vira `partial`, e o perfil passa a sair do `PerfilEfetivo`;
    - as velocidades passam a sair de `FisicaEfetiva`, em cinco lugares;
    - cambaleio em `PassoAndando`; caras da fase nos sorteios;
    - `Concluir` chama `EfeitosDaOnda`;
    - `Maquina.Onda.cs` (novo), `ItemEffectTimer`, `AgendarOnda`/`CancelarOnda`, campos em `EstadoDoNucleo`, `Gravacao.cs`, `SimuladorDeTempo`.
  - **Testes:** `OndaTestes` com o estado semeado pelo construtor `Nucleo(config, estado)`, cobrindo durações, disparo antigo, pausado só troca a cara, repouso sem relógio, `PerfilEfetivo`, `FisicaEfetiva` e cambaleio; `Politica_Imediata`; `EscreverELer`.
- **T5. Itens, estado `USING`, onda de fundo e combinação, atrás da chave.**
  - **Arquivos:**
    - `Tipos.cs`: `Using`, `Grupo`, `AceitaPressionar`;
    - `Eventos.cs` (7 eventos) e `Efeitos.cs` (5 efeitos);
    - `EstadoDoNucleo.cs` e `Maquina.Itens.cs`;
    - ganchos em `Maquina.cs`: `Tratar`, `Passar`, `Decidir`, `IrPara`, `MudarTopologia`, `Esconder`, `Sair`, `SairDoMonitorOcupado`, `Concluir`;
    - `AplicarNaOnda` e `Refrescar`.
    - Este passo já resolve L4–L7 e L15.
  - **Testes:**
    - `ItensTestes`, `UsoTestes` e o resto de `OndaTestes`;
    - `Cenario.Em(Using)` e `FilaEAleatorioTestes`;
    - `InvariantesTestes` no mesmo passo: gerador de itens com `Random` próprio, relógio, R-b, agenda, R11, invariantes 22–29 e os casos exigidos. Sem isso, o invariante 11 falha.
- **T6. Referência `07-tamagotchi.txt`.** Diretiva `# tamagotchi: sim` em `LerCabecalho`, com as linhas de arraste escritas por extenso.
- **A1–A4. Arte, mesclada depois de T1.**
  - **A1:** paleta; legenda com `Add` em vez de indexador; `Carimbo.Girado`; itens 24×24 com as chaves do núcleo; `ItensPixelTestes`; `itens-8x.png`.
  - **A2:** caras de efeito com os nomes do enum, `Topete.Torto` e rubor; `rostos-efeito.png`. Depois de T3, entra `EnumDoNucleoCobreOsRostos`.
  - **A3:** poses de uso no chão, `Pontos`, `UsosPixelTestes`, `usos.png` e uma rodada de retoque.
  - **A4:** sobreposições, poses provisórias dos gestos (L11) e ícones de item.
- **T7. Apresentação, sem janelas de item.**
  - **Arquivos:** `PoseDoPersonagem.cs` (uso por verbo, passo e apoio, com a alternativa de C25; L12; gestos novos), `QuadroDoSprite` e `SpriteProvisorio.cs` (LRU; item, efeito e fase no quadro).
  - **Testes:** `PoseDeUsoTestes`, `CacheDeQuadrosTestes` e `TodaPoseEscolhidaExisteNaPixelArt…`, estendido a `Using` e a todas as `Expressao` e `Gesto`; `PassosDoUso` igual à duração do verbo.
- **T8. Janelas dos itens, submenu "Itens" e temporizador da onda, com a chave ainda desligada.**
  - **Arquivos:** `JanelaDoItem.cs`, `SpriteDoItem.cs`, `GerenteDosItens.cs`, `Aplicacao.Itens.cs` (`Aplicacao` vira `partial`), ganchos em `Aplicacao.cs`, `MenuNativo.cs`, `Textos.*`.
  - **Testes:** sem janela, com a lista do menu, as teclas do submenu de itens e os ícones.
- **T9. Ligar a chave** (`DoAplicativo.Tamagotchi = true`).
  - **Testes:** integração `ItensIntegracaoTestes`, verificação de tela e repouso de 10 minutos com onda ativa.
  - **Documentos:** DEC-028; ARCHITECTURE 2.6, 2.7, 2.10, 2.13.1, 2.13.3 e 2.16 (nova); SECURITY 3.1, 5, 7 e 8.1; IDENTIDADE_VISUAL; PRODUCT_SPEC; seção "Interação" no TODO.

## 6. Verificações de tela e pendências

A verificação de tela roda com `Buzzy.Verificacao --injetar-input-na-tela --fase tamagotchi`, input sintético por `SendInput`, `--perfil-de-teste`, `--semente` e `--diagnostico`. O usuário é avisado antes, e o resultado não conta como evidência humana.

| Caso | O que confere |
|---|---|
| V1 | Menu de emoção por clique direito e teclas D + letra: a cara muda e a opção fica marcada. "Automática" também funciona. |
| V2 | 20 aberturas do menu: GDI e USER estáveis (±2), bitmaps criados = apagados. |
| V3 | Invocar a banana: pousa ao lado em até 1,5 s, depois fica 3 s parada. O relógio desliga. O ponto opaco do item cai na janela do Buzzy e o transparente no receptor. O foco fica no receptor. |
| V4 | Arrastar até ele: log `USING`, quadros de uso na ordem, a janela do item some, volta a `IDLE`. Arraste do item com p95 abaixo de 16,7 ms. |
| V5 | Soltar longe: o item cai de onde foi solto. |
| V6 | Pressionar no meio do uso: `PRESSED` em até um quadro, e a onda continua. |
| V7 | Sétimo item: o mais antigo sai. |
| V8 | Com a agenda ligada, segurar um item: `WALKING` vira `IDLE`, nenhuma agenda durante o gesto, e a próxima vem em 3 s ou mais. |
| V9 | Soltar um item nele preso no cipó, na parede e no esconderijo: ele volta preso ao mesmo lugar. |
| V10 | Esconder e mostrar: as janelas dos itens acompanham. |
| V11 | Cocaína contra baseado: velocidades de caminhada com tolerância de ±10%. |
| V12 | Nível 3 de bebedeira: recua na caminhada, sempre no chão. |
| V13 | Repouso de 10 minutos, pausado, com uma vodka ativa: CPU média de até 0,1%, relógio desligado. |
| V14 | Sair com itens na tela: código 0 e nenhuma janela viva. |
| V15 | Botão direito num item abre o menu. |
| V16 | Emoção gravada no perfil de teste e restaurada ao reabrir. Fica N/A até a P7. |
| X1 | Dois monitores: N/A com um monitor só. |

**Integração com mensagens postadas.** Segue o desenho do app, só para janelas do próprio processo, com o PID conferido. Se o laço do menu ignorar teclado postado, esses casos ficam N/A e a cobertura fica em V1 e V3.

**Pendências [MANUAL] ou [HW]:**
- a marca de rádio aparece junto do ícone nos temas claro, escuro e alto contraste (risco R1 do app);
- menu e janelas de item a 125, 150, 175 e 200%; o usuário consegue testar parte disso mudando a escala;
- arrastar um item entre monitores de escalas diferentes [HW];
- Narrador lendo os submenus;
- revisão visual das prévias e do tom pelo usuário, porque os carimbos foram desenhados sem renderizar;
- conforto para agarrar os itens pequenos (L14);
- desconectar o monitor com itens na tela (S8), depois da P12;
- gravação a 120 qps das animações de uso (critério 5 da Fase 4, que já estava pendente);
- Process Monitor confirmando que o Buzzy só grava em `%LOCALAPPDATA%\Buzzy`, depois da P7.
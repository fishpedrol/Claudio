# PRODUCT_SPEC.md — Visão e escopo do Buzzy

> Requisitos estáveis do produto (STATUS: PLANNED; o estado da implementação fica em [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md)). Atualize quando o usuário mudar a intenção ou o escopo.

## Visão

Buzzy é um mascote de companhia para o desktop Windows, no espírito lúdico dos mascotes da era do BonziBuddy, para ficar na tela enquanto o usuário programa ou joga (2026-09-28). Parece um personagem vivendo no desktop, não uma janela: engraçado, curioso, expressivo e ativo, com a personalidade só em movimento, poses, expressões, gestos e reações não verbais. Não é assistente nem chatbot e não executa tarefas no computador.

**Buzzy lembra o Luffy de propósito (DEC-019).** É um macaquinho com o jeito do Luffy, de *One Piece*: o chapéu de palha com a faixa vermelha e a personalidade dele — livre, impulsivo, otimista, aventureiro, espuleta, de sorriso largo. O nome continua Buzzy, e não se copia outro mascote de desktop.

- **Personagem:** pequeno primata arbóreo antropomórfico, amigável e ágil: cabeça grande, olhos expressivos, focinho curto, orelhas arredondadas, braços longos, mãos de escalar, pernas compactas e cauda longa. Pixel art fiel às pranchas (DEC-018, DEC-019).
- **Personalidade em cena:** risada solta, impulsividade, energia inesgotável, cochilos despreocupados; movimento exagerado e elástico (antecipar, comprimir e esticar, pousar com impacto). O chapéu reage às emoções. Outros elementos do Luffy (cicatriz, braço elástico) entram se o usuário pedir.
- **Limites:** sem fala, bordão escrito, chat, texto ou voz (DEC-003). Uso pessoal, sem distribuição (Q-10); antes de qualquer distribuição pública, revisar o uso de *One Piece*, marca de terceiros. A identidade não tem gate de aprovação (DEC-015).
- **Pranchas:** [buzzy-character-concept.png](../assets/references/buzzy-character-concept.png) e [buzzy-character-concept2.png](../assets/references/buzzy-character-concept2.png) são a base, não arte final; chapéu, cores, proporções, rosto e poses aprovados (DEC-019). O nome "Pixel" e o cursor desenhados nelas não valem, e as poses não ampliam as superfícies do MVP.

## Escopo do MVP

- **Janela:** transparente por pixel, sempre no topo por padrão (desligável), com ícone e menu na bandeja e sem botão na barra de tarefas; ocultar e restaurar pela bandeja; abrir de novo revela a instância existente; escala em passos fixos; sem opacidade.
- **Controle:** posicionamento livre e arraste pelo mouse ou touchpad. Fora do MVP: atalhos, toque, caneta e modo fantasma.
- **Movimento** local e determinístico — andar, escalar, pendurar-se por pouco tempo, saltar e cair — pelas bordas das áreas úteis dos monitores (chão, bordas horizontais alcançáveis, laterais), nunca sobre janelas de outros aplicativos.
- **Monitores:** qualquer arranjo do desktop virtual — acima, abaixo ou dos lados, com resolução, orientação e DPI diferentes, conexão e desconexão —, com travessia por caminho válido, ligada por padrão e desligável (Q-05): andando entre monitores de mesmo chão, num pulo para um degrau ao alcance ou subindo pela parede até o vizinho mais alto (DEC-032).
- **Interação:** clique dá uma reação não verbal; dois cliques o escondem atrás da barra de tarefas ou de uma lateral, só com a cabeça e as mãos para fora, e outros dois o tiram (DEC-025); solto no alto, agarra um cipó na borda de cima, e junto a uma lateral gruda na parede, e, posto lá pelo usuário, só sai quando o usuário o tira (DEC-024). O botão direito abre o menu, que abre o painel compacto de energia.
- **Personalidade** local e determinística, em ações curtas que não atrapalham os aplicativos — explorar bordas, espiar, olhar ao redor, se espreguiçar, se coçar, brincar, descansar, reagir a cliques —, sem observar pixels, títulos, conteúdo ou identidade de outros aplicativos. A expressão é separada do movimento.
- **Energia Baixa, Média (padrão) ou Alta** (DEC-014), no painel e nas configurações, com o mesmo valor persistido: Baixa mais tranquila, Média brincalhona e equilibrada, Alta com mais e mais longas brincadeiras. Física, segurança e prioridade do usuário não mudam.
- **Emoção dominante e tamagotchi adulto** (seção abaixo).
- **Configurações** locais, só em português do Brasil; painel e configurações acessíveis por teclado e leitor de tela (a janela do personagem não é alvo de leitor de tela). Iniciar com o Windows é opcional e vem desligado (Q-04).
- **Modo de tela cheia**, ligado por padrão e desligável (Q-09): com uma janela em tela cheia no primeiro plano, o Buzzy vai para um monitor livre ou, sem nenhum, se oculta até ela acabar, e depois volta à posição anterior, se existir. A ação manual (arrastar, ocultar, mostrar) prevalece e não é desfeita. A detecção usa a tela cheia como sinal, sem identificar jogos: vídeos e apresentações também a ativam; jogos em janela, não. O protótipo P7 valida custo e casos (exclusiva, sem borda) antes da Fase 8.
- **Plataforma e uso:** Windows 11, alvo inicial 24H2 ou posterior (Q-02). Pacote pessoal em ZIP portátil sem assinatura nem instalador; o código pode, no máximo, ficar no GitHub; nada de releases para outras pessoas sem nova decisão (Q-10).

## Prioridade do usuário

Ordem: (1) ação direta do usuário; (2) menu, painel e outras ações explícitas; (3) mudanças necessárias do sistema, como o modo de tela cheia; (4) comportamento autônomo.

No arraste, o mouse down inicia DRAGGING e para a autonomia incompatível; o personagem acompanha o cursor sem andar, pular, fugir ou escalar; o mouse up valida a posição no desktop virtual, e, identificados monitor e superfície, ele retoma o comportamento permitido. O mascote não disputa o cursor. Arrastar um item do tamagotchi também é ação direta: com o item na mão, o personagem espera, e pressioná-lo no meio de um uso o segura na hora. O input distingue clique de arraste; teclas só valem nos controles próprios em foco, sem captura global.

O movimento é uma máquina de estados determinística, sem IA, e a expressão é uma dimensão visual à parte. Trocar a arte e as animações preserva o núcleo de estado, input, movimento, desktop e segurança.

## Emoção dominante e tamagotchi adulto

Pedidos do usuário de 2026-09-30 e 2026-10-01; regras e palavras do usuário em DEC-027 e DEC-028.

- **Emoção dominante:** o menu "Emoção dominante" lista as 14 expressões de `expressoes.png`, com o rosto ao lado do nome, e "Automática". A escolhida vira a cara de base e a mais frequente, sem mudar comportamento, física ou prioridade, e continua valendo ao reabrir.
- **Tamagotchi adulto, cartunesco e cômico, para uso privado de um adulto.** "Itens" lista 13 — banana, água, vodka, cerveja, baseado, cigarro, cocaína, MD, lança-perfume, café, energético, cogumelo e bala —, que só aparecem pelo menu: o item cai no chão ao lado dele, com física de desenho, e espera; até 6 na tela; "Recolher itens" tira todos; nada é lembrado ao fechar.
- **Uso:** só ao arrastar o item e soltá-lo sobre o personagem, com uma animação por verbo (comer, beber, fumar, cheirar, engolir, inalar) no lugar em que ele está. Cada uso muda por um tempo o jeito, as caras e as animações dele, com efeitos de desenho animado que passam sozinhos.
- **Alívio:** banana, café e energético acalmam um passo o efeito de uma substância, sem trazer o deles; a água acalma assim qualquer efeito. Sem efeito em curso, ou com um leve, fazem o de sempre.
- **Paranoia de desenho animado:** só ao misturar uma droga sintética do jogo (bala, MD, cocaína, lança-perfume) com outra substância — nunca com álcool e maconha sozinhos, nem com uma sintética repetida. Chance de 1 em 8, sorteada uma vez por mistura; só uma leva nova, depois que os efeitos passam, tem outra chance. Paranoico, sua e treme; no auge, olha e aponta pro teto, agacha segurando o chapéu e não escala, não pula nem descansa.
- **Baseado por conta própria:** de vez em quando, parado no chão, ele fuma um baseado tirado do chapéu, sem item na tela, com o mesmo efeito do baseado do menu; na Média, cerca de um a cada 4 minutos do tempo elegível, ficando chapado cerca de 39% do tempo só com a autonomia — frequência confirmada pelo usuário em 2026-10-02 (DEC-028, item 41). Não fuma já chapado nem paranoico, escondido, no ar, pausado ou com o usuário segurando um item; um clique o interrompe. Conta na mistura da paranoia, com a mesma chance por leva (item 39).
- **Conteúdo adulto** (pedido do usuário de 2026-10-02; DEC-033): uma chave no menu, ligada por padrão e lembrada ao reabrir. Desligada, somem do menu e da tela os itens adultos — fica só banana, água, café e energético —, acabam os efeitos de substância e a paranoia, e ele não fuma sozinho.
- **Sem necessidades com o tempo:** nada de fome, sede ou sono que decaem.
- **O usuário prevalece:** pressionar ou arrastar o personagem interrompe o uso; o botão direito num item abre o mesmo menu.
- **Tom:** só nomes de itens e efeitos de desenho animado; nenhuma informação real sobre drogas (dose, obtenção, preparo) no app, nos textos ou na documentação. A divisão em comida e bebida sem álcool, sintéticas e outras substâncias é regra do jogo, pedida pelo usuário.

## Limites de capacidade e segurança

Sem conversa, chat, campo de texto, respostas escritas, voz ou reconhecimento de fala; sem IA integrada, LLM, RAG, embeddings, APIs de IA, backend, atualização automática, nuvem, telemetria ou analytics, agora ou no plano futuro, a menos que o usuário reabra a decisão. Claude é ferramenta de desenvolvimento, não capacidade do produto.

O Buzzy é legítimo, previsível e auditável: sem execução arbitrária, shell, PowerShell, CMD, keylogging, captura silenciosa de tela, processos ocultos, persistência escondida, coleta remota, download ou execução de código desconhecido, nem controle genérico de mouse e teclado. Capacidade nova do sistema segue intenção explícita → capacidade específica → permissão limitada → ação auditável, definida antes de implementar ([SECURITY.md](SECURITY.md)).

## Configurações e dados locais

Ficam locais as configurações, a última posição escolhida pelo usuário, o monitor, o tamanho, a energia, a emoção dominante e as preferências aprovadas. Não são gravados os itens e efeitos do tamagotchi nem a posição temporária do modo de tela cheia, que fica em memória. Sem memória de IA nem nuvem. Esquema e lugar: ARCHITECTURE.md 2.12 e SECURITY.md 5.

Ao reabrir, o Buzzy volta onde estava, no mesmo monitor e com a mesma emoção; escondido, volta escondido no mesmo lado; preso pelo usuário, continua preso (DEC-029). Sem aquele monitor, aparece na mesma posição relativa de um monitor no mesmo lugar e do mesmo tamanho ou, sem nenhum, do principal (DEC-030). A preferência de atravessar monitores já está no arquivo, ligada, e passa a valer com a travessia (Fase 5).

## Desempenho

Aberto por horas: quase nenhuma atividade em repouso, renderização eficiente, eventos em vez de polling e dependências justificadas. Metas Q-08 aceitas em DEC-011; a Fase 11 as mede.

## Decisões abertas

Modelo detalhado de física, superfícies, animações e assets (nas fases); detalhes finais da persistência e das permissões; o empacotamento do runtime .NET (plano de build); visibilidade do repositório no GitHub (reabrir Q-10 antes de preparar binários para outras pessoas). Não as invente no plano: registre alternativas, trade-offs e recomendação antes de codificar.

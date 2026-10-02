# PRODUCT_SPEC.md — Visão e escopo do Buzzy

> Referência estável do produto. Atualize quando o usuário mudar intenção ou escopo; o estado de implementação fica em PROJECT_CONTEXT.md.
>
> STATUS: PLANNED. Esta especificação descreve o produto desejado, não funcionalidades implementadas.

## Visão

Buzzy é um mascote digital interativo para desktop Windows: um companheiro moderno inspirado na sensação lúdica dos mascotes de desktop da era do BonziBuddy. Deve parecer um personagem vivendo no desktop, não uma janela convencional.

**Intenção central do usuário (2026-09-28):** Buzzy é um mascote interativo de companhia para ficar no desktop enquanto o usuário programa ou joga. Deve ser engraçado, curioso, expressivo e ativo no desktop; não é um assistente de tarefas nem um chatbot. Sua personalidade aparece por movimento, expressões, gestos e reações não verbais. Não haverá chat, campo de texto, conversa digitada ou respostas em texto. Não incluir IA no aplicativo, agora ou no plano futuro, a menos que o usuário reabra essa decisão explicitamente. Buzzy não executa tarefas gerais no computador.

**Buzzy lembra o Luffy de propósito (decisão do usuário, 2026-09-29, DEC-019).** A ideia central do projeto é um macaquinho com o jeito do Luffy, de *One Piece*: a personalidade dele e o chapéu de palha com a faixa vermelha, como nas pranchas de referência. O nome continua Buzzy. Não se copia outro mascote de desktop.

O personagem conceitual é um pequeno primata arbóreo antropomórfico, amigável, curioso, brincalhão, ágil e expressivo: cabeça relativamente grande, olhos expressivos, focinho curto, orelhas arredondadas, braços longos, mãos adequadas à escalada, pernas compactas e cauda longa e expressiva. A identidade visual está em pixel art, fiel às pranchas (DEC-018 e DEC-019); o software mostra o quadro parado dela e integra as animações na Fase 6.

O usuário quer que o Buzzy **tenha a personalidade do Luffy**: livre, impulsivo, otimista, aventureiro, espuleta, de sorriso largo e sempre pronto para a próxima brincadeira. A semelhança é desejada. Como o produto não tem fala nem texto (DEC-003), a personalidade aparece em poses, gestos, expressões e no ritmo das ações.

**Tradução da inspiração (direção de design, STATUS: PLANNED):**

- **Personalidade do Luffy:** sorriso largo e fácil, risada solta, postura confiante, impulsividade (vai antes de pensar), otimismo, curiosidade aventureira, energia inesgotável, cochilos despreocupados e apetite por brincadeira. Aparece em poses, tempo das ações e expressões; o movimento é exagerado e elástico (antecipar, comprimir e esticar, pousar com impacto).
- **Visual:** macaquinho das pranchas com o **chapéu de palha e a faixa vermelha**, que também reage às emoções (salta no susto e na risada, desce no sono). Outros elementos do Luffy (por exemplo, a cicatriz sob o olho ou gags de braço elástico) podem entrar se o usuário pedir; não são proibidos.
- **Limites que continuam:** sem fala, bordão escrito, chat, texto ou voz (DEC-003); o nome é Buzzy; o aplicativo é de uso pessoal e não é distribuído (Q-10). Antes de qualquer distribuição pública, revisar o uso de elementos de *One Piece*, que são marca e obra de terceiros.
- Não há gate de aprovação rotineira do usuário para criar ou integrar a identidade (DEC-015).

### Referências visuais fornecidas pelo usuário

As duas pranchas em `assets/references/buzzy-character-concept.png` e `assets/references/buzzy-character-concept2.png` são a base da identidade do Buzzy: a pixel art segue o macaquinho delas, com o chapéu de palha (DEC-018 e DEC-019). Não são arte final. Claude está autorizado a produzir a identidade sem aguardar aprovação.

![Primeira prancha conceitual de referência](../assets/references/buzzy-character-concept.png)

![Segunda prancha conceitual de referência](../assets/references/buzzy-character-concept2.png)

Das pranchas, o chapéu de palha, as cores, as proporções, o rosto e as poses estão aprovados (DEC-019). O nome "Pixel" não: o produto continua Buzzy, e qualquer mudança de nome depende de decisão explícita do usuário. As poses não ampliam o escopo atual de superfícies do MVP.

O cursor de mouse mostrado em uma prancha é apenas ilustrativo e não pertence ao personagem nem ao produto. As poses e animações devem corresponder às superfícies e ações previstas no MVP.

## Escopo do MVP

O MVP deve incluir:

- Um personagem visível no desktop Windows.
- Integração com desktop e janela transparente.
- Posicionamento livre na tela.
- Arraste direto pelo mouse.
- Movimento local determinístico: caminhada, escalada, salto e queda.
- Movimento autônomo pelas superfícies de borda aprovadas, escalada das laterais, pequenas pausas pendurado e travessia entre monitores com caminho válido.
- Suporte a múltiplos monitores, com travessia ligada por padrão e opção para desligar.
- Expressões separadas da lógica de movimento.
- Interação por clique e arraste. Um clique provoca uma reação não verbal; dois cliques escondem o mascote atrás da barra de tarefas ou de uma lateral, só com a cabeça e as mãos para fora, e outros dois cliques o tiram de lá (DEC-025). Solto no alto, ele agarra um cipó na borda de cima; solto junto a uma lateral, gruda na parede. Posto lá pelo usuário, só sai quando o usuário o tira (DEC-024). O painel compacto com o controle de energia em três posições abre pelo menu. Não há conversa nem chat.
- Ações visuais curtas de mascote, como se espreguiçar, se coçar, espiar, brincar e descansar; não bloqueiam o uso dos aplicativos.
- A emoção dominante escolhida pelo menu e o tamagotchi adulto, com itens invocados pelo menu e usados ao arrastá-los até ele, pedidos pelo usuário em 2026-09-30 (seção [Emoção dominante e tamagotchi adulto](#emoção-dominante-e-tamagotchi-adulto)).
- Configurações e persistência local.
- Segurança, previsibilidade e desempenho adequados para uso prolongado.

O MVP começa com uma personalidade local e determinística. Curiosidade, humor e energia aparecem em ações, expressões faciais e gestos: explorar bordas, espiar, olhar ao redor, reagir a cliques e fazer pequenas travessuras. Isso não envolve observar pixels, títulos, conteúdo ou identidade de outros aplicativos. Um controle de três posições oferece **Baixa**, **Média** (padrão) e **Alta** — equivalentes a Low/Mid/High. Baixa deixa o Buzzy mais tranquilo, com pausas maiores e menos ações; Média mantém um ritmo brincalhão equilibrado; Alta aumenta a frequência e a duração das brincadeiras e reações. Os três níveis preservam as mesmas regras de movimento, segurança e prioridade do usuário. Dois cliques mostram o seletor de energia; as configurações oferecem o mesmo valor persistido.

## Decisões de produto confirmadas

As decisões abaixo foram respondidas pelo usuário entre 2026-09-26 e 2026-09-28 e detalhadas em [DECISIONS.md](DECISIONS.md). São escopo planejado, ainda não implementado:

- **Plataforma:** Windows 11, com Windows 11 24H2 ou posterior como alvo inicial de teste.
- **Janela e acesso:** sempre no topo por padrão, opção para desligar, ícone e menu na bandeja, sem botão na barra de tarefas, ocultar/restaurar pela bandeja, escala em passos fixos, sem controle de opacidade no MVP e segunda abertura revelando a instância existente. O botão direito abre o menu.
- **Controle e movimento:** clique reage com animação ou gesto; clique duplo alterna o esconderijo (DEC-025); botão direito abre menu, e o menu abre o painel compacto com o seletor de energia. O painel se reposiciona para permanecer visível e funciona por teclado e leitor de tela. No MVP, o personagem usa mouse e touchpad e percorre as superfícies das bordas das áreas úteis dos monitores: anda pelo chão e pelas bordas horizontais alcançáveis, escala paredes laterais, pode ficar pendurado por pouco tempo e atravessa monitores quando existe caminho válido. Travessia fica ligada por padrão e pode ser desligada. Ele não anda sobre janelas de outros aplicativos. Atalhos de controle, toque e caneta ficam fora do MVP.
- **Interface e configurações:** somente português do Brasil. O painel rápido de energia e a janela de configurações permitem navegação por teclado e leitor de tela; a janela do personagem não é alvo de leitor de tela. Não há interface de conversa, chat, campo de texto nem respostas escritas.
- **Preferências:** iniciar com o Windows será opcional, desligado por padrão. A energia da personalidade oferece Baixa/Média/Alta, começando em Média. O modo de tela cheia fica ligado por padrão: ao detectar uma janela em tela cheia em primeiro plano, o Buzzy vai para um monitor livre; se não houver nenhum, fica oculto até a tela cheia terminar. Ao sair desse modo, volta à posição anterior, se ela ainda existir. Se o usuário arrastar, ocultar ou mostrar o Buzzy durante o modo, a ação manual prevalece e o retorno automático não a desfaz. O usuário pode desligar o modo nas configurações. A detecção usa tela cheia como sinal prático, sem identificar jogos: apresentações e vídeos em tela cheia também podem ativá-la; jogos em janela comum não são detectados automaticamente. P7 valida o custo e os casos de tela cheia exclusiva e sem borda antes da Fase 8. Modo fantasma não entra no MVP.
- **Uso e distribuição:** uso pessoal; não há plano de divulgar ou distribuir o aplicativo pronto. O código poderá, no máximo, ficar no GitHub. O primeiro pacote para uso próprio e testes será ZIP portátil sem assinatura nem instalador. Não preparar releases binários para outras pessoas sem uma nova decisão. O modo de empacotar o runtime .NET será definido no plano de build.

## Interação e prioridade do usuário

Prioridade de controle:

1. Ação direta do usuário.
2. Menu, painel de energia e outras ações explícitas do usuário.
3. Mudanças necessárias do sistema, incluindo o modo de tela cheia aprovado em Q-09.
4. Comportamento autônomo e determinístico do mascote.

Ciclo obrigatório de arraste:

1. Mouse down inicia DRAGGING.
2. Comportamento autônomo incompatível é interrompido.
3. O personagem acompanha o cursor sem andar, pular, fugir ou iniciar escalada.
4. Mouse up valida a posição no desktop virtual.
5. O sistema identifica monitor e superfície e só então retoma comportamento permitido.

O mascote não disputa o cursor com o usuário. Arrastar um item do tamagotchi também é ação direta do usuário: enquanto o item está na mão, o personagem para e espera, e pressionar o personagem no meio do uso de um item o segura na hora.

O desenho de input deve distinguir clique de arraste. Teclas só afetam os controles próprios enquanto o painel de energia ou as configurações estão explicitamente em foco; não há captura global de teclado.

## Movimento, expressão e apresentação

Movimento usa máquina de estados, eventos e regras determinísticas; não depende de IA. Expressão é uma dimensão visual separada e pode mudar sem alterar a máquina de movimento.

O asset provisório não determina o formato da arquitetura. A troca futura de arte e animações deve preservar o núcleo de estado, input, movimento, desktop e segurança.

## Emoção dominante e tamagotchi adulto

Pedidos do usuário em 2026-09-30, e em 2026-10-01 o alívio, a paranoia e o baseado que ele fuma por conta própria, detalhados em DEC-027 e DEC-028. O estado de implementação fica em PROJECT_CONTEXT.md.

**Emoção dominante.** No menu do botão direito, "Emoção dominante" lista as 14 expressões de `expressoes.png`, cada uma com o rosto dela ao lado do nome, e "Automática", o jeito de sempre. A escolhida vira a cara de base e a mais frequente; o comportamento, a física e a prioridade do usuário não mudam. É uma preferência: continua valendo ao reabrir o app, guardada com as configurações locais (seção [Configurações e dados locais](#configurações-e-dados-locais)).

**Tamagotchi adulto.** Um "tamagotchi virtual adulto", cartunesco e cômico, para o uso privado de um adulto:

- **Itens só pelo menu:** "Itens" lista 13: banana, água, vodka, cerveja, baseado, cigarro, cocaína, MD, lança-perfume, café, energético, cogumelo e bala. Nada aparece sozinho na tela, nem quando ele fuma por conta própria.
- **Onde aparecem:** o item cai no chão ao lado do personagem, com física de desenho, e fica esperando. Cabem até 6 na tela, e "Recolher itens" tira todos. Os itens não são lembrados ao fechar o app.
- **Uso:** o item só é usado quando o usuário o arrasta e solta sobre o personagem. Clicar no item ou soltá-lo longe não o usa. Cada uso tem uma animação própria (comer, beber, fumar, cheirar, engolir ou inalar), no lugar em que ele está.
- **Efeitos de desenho animado:** cada uso muda por um tempo o jeito, as caras e as animações dele ("mais animado", "meio chapado de erva", "bêbado" e assim por diante, nas palavras do usuário), e o efeito passa sozinho.
- **Comida e bebida sem álcool acalmam aos poucos** (2026-10-01): "sobre o tabaco, cerveja e vodka, mantém como está, só quero que alimentos ou bebidas sem ser alcoólicas diminuam aos poucos o efeito da onda". A banana, o café e o energético acalmam um pouco o efeito de uma substância, um passo por item, sem trazer o efeito deles; a água acalma do mesmo jeito qualquer efeito. Sem efeito em curso, ou com um efeito leve, como o da própria banana ou o do café, a banana, o café e o energético fazem o de sempre. O tabaco, a cerveja, a vodka e as outras substâncias continuam como estavam.
- **Paranoia, de desenho animado** (2026-10-01): "caso o macaco use muitas coisas ele fica paranoico, como o meme 'os cara tá no teto' mas de forma engraçada". Ele só pode ficar paranoico quando mistura uma droga sintética do jogo (bala, MD, cocaína ou lança-perfume) com outra substância; com álcool e maconha, não. A chance é de 1 em 8, sorteada uma vez por mistura: usar mais coisas na mesma leva não aumenta a chance, e só uma leva nova, depois que os efeitos das substâncias passam por completo, tem a sua própria chance. Uma sintética sozinha, mesmo repetida, não o deixa paranoico. Paranoico, ele sua e treme; no auge, olha e aponta pro teto, se agacha segurando o chapéu e não escala, não pula nem descansa. Comida e bebida sem álcool também o acalmam aos poucos.
- **Baseado por conta própria** (2026-10-01, 19:10): "uma funcionalidade que o macaco fume maconha à vontade quando ele quiser". De vez em quando, parado no chão, ele fuma um baseado sozinho, tirado do chapéu: nenhum item aparece na tela. É o mesmo fumar e o mesmo efeito do baseado que o usuário dá a ele. Na energia Média, sai cerca de um a cada 4 minutos do tempo em que ele fica parado no chão sem estar chapado nem paranoico, o que dá cerca de um a cada 14 ou 15 minutos no total. Sem ninguém dar nada a ele, ele fica chapado cerca de 39% do tempo na Média (20% na Baixa e 55% na Alta); o usuário confirmou essa frequência em 2026-10-02. Ele não acende outro já chapado nem paranoico, e não fuma escondido, no ar, com o movimento pausado ou enquanto o usuário segura um item. Só conta o efeito que está aparecendo: bêbado por cima de um chapado, ele pode fumar de novo, e o chapado de baixo fica mais forte. Um clique o interrompe, como em todo uso. O baseado não é droga sintética, mas conta na mistura: uma droga sintética dada com ele chapado do baseado dele, ou ele fumando depois dela, pode deixá-lo paranoico, com a mesma chance de 1 em 8 por leva; o usuário também confirmou isso em 2026-10-02.
- **Sem necessidades com o tempo:** nada de fome, sede ou sono que decaem; só os itens e as interações mudam o humor e o comportamento.
- **O usuário prevalece:** pressionar ou arrastar o personagem interrompe o uso; o botão direito num item abre o mesmo menu.
- **Tom:** só os nomes dos itens e efeitos de desenho animado. Nenhuma informação real sobre drogas, como dose, obtenção ou preparo, no app, nos textos ou na documentação. A divisão dos itens em comida e bebida sem álcool, drogas sintéticas e outras substâncias é regra do jogo, pedida pelo usuário, e não diz nada sobre o mundo real.

## Desktop e múltiplos monitores

O projeto deve suportar o desktop virtual real: monitores podem estar acima, abaixo, à esquerda ou à direita, com resolução, orientação e DPI diferentes. Considerar monitor primário, área útil, conexão/desconexão, mudanças de resolução/DPI e persistência de posição.

Não assumir que os monitores estão lado a lado. A Fase 0 define a abstração e os cenários verificáveis; a Fase 5 completa o tratamento de topologia e casos de borda.

## Personalidade não verbal e limites de capacidade

Buzzy expressa curiosidade e humor por movimento, poses, expressões e reações visuais locais e determinísticas. O produto não terá conversa, chat, entrada de texto, respostas escritas, voz ou reconhecimento de fala. Também não terá IA integrada, LLM, RAG, embeddings, APIs de IA, backend, atualização automática, sincronização em nuvem, telemetria ou analytics. IA no produto só volta ao plano se o usuário pedir explicitamente.

Claude pode ser usado como assistente de desenvolvimento nos modelos que o usuário escolher, incluindo Fable e Opus. Isso não é uma capacidade do produto em execução.

## Configurações e dados locais

Persistência local pode guardar configurações, última posição escolhida pelo usuário, monitor preferido, tamanho, nível de energia, a emoção dominante e preferências de comportamento aprovadas. Os itens e os efeitos do tamagotchi não são gravados. A posição temporária usada pelo modo de tela cheia fica só em memória e não substitui essa posição persistida. Opacidade não é uma configuração do MVP. Não há memória de IA nem sincronização em nuvem. O esquema e a localização planejados estão em ARCHITECTURE.md e SECURITY.md.

Ao reabrir, o Buzzy volta onde estava, no mesmo monitor, com a mesma emoção dominante. Se estava escondido na borda, volta escondido no mesmo lado; se o usuário o deixou preso na parede ou no cipó, continua preso lá. O usuário deixou essa escolha com Claude em 2026-09-30 (DEC-029). Se aquele monitor não estiver mais lá, ele aparece na mesma posição relativa de um monitor no mesmo lugar e com o mesmo tamanho ou, sem nenhum, do principal (DEC-030). A preferência de atravessar monitores (Q-05) já tem lugar no arquivo, ligada por padrão, mas só terá efeito quando a travessia entrar, ainda na Fase 5. O estado de implementação fica em PROJECT_CONTEXT.md.

## Segurança

Buzzy deve ser legítimo, previsível e auditável. O MVP não terá execução arbitrária, shell, PowerShell, CMD, keylogging, captura silenciosa de tela, processos ocultos, persistência escondida, coleta remota, downloads/execução de código desconhecido ou controle genérico de mouse/teclado.

Qualquer capacidade futura do sistema operacional deve seguir: intenção explícita → capacidade específica → permissão limitada → ação auditável. A necessidade real de permissões é definida antes da implementação.

## Desempenho

O aplicativo pode permanecer aberto por horas. Priorizar baixa atividade em idle, renderização eficiente, eventos em vez de polling desnecessário e dependências justificadas. As metas mensuráveis Q-08 foram aceitas e estão em DEC-011; continuam planejadas, ainda não verificadas no aplicativo. A Fase 11 mede os resultados.

## Decisões abertas

- Modelo detalhado de física, superfícies, animações e assets, conforme as fases correspondentes.
- Detalhes finais do formato de persistência e permissões específicas, seguindo ARCHITECTURE.md e SECURITY.md.
- Visibilidade do repositório GitHub, se o código for hospedado ali. Nenhuma distribuição de aplicativo pronto está planejada; reabrir Q-10 antes de preparar binários ou releases para outras pessoas.

Essas decisões não devem ser inventadas pelo plano de implementação. Registrar alternativas, trade-offs e recomendação antes de codificar a parte correspondente.

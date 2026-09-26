# Decisões do usuário — Buzzy

Este resumo registra suas respostas. A fonte oficial, com os motivos e os detalhes técnicos, é [DECISIONS.md](DECISIONS.md). As escolhas abaixo são planos de produto; não querem dizer que já estejam implementadas.

## Respostas registradas

- **Q-02 — Windows:** Windows 11; o alvo inicial de teste é Windows 11 24H2 ou posterior.
- **Q-03 — Aparência e acesso:** aceito o conjunto recomendado: sempre no topo por padrão, com opção para desligar; bandeja com menu; sem botão na barra de tarefas; minimizar esconde e a bandeja restaura; tamanho em passos fixos; sem opacidade no MVP; botão direito abre o mesmo menu; uma segunda abertura mostra a instância existente.
- **Q-04 — Iniciar com o Windows:** oferecer a opção, desligada por padrão e ativada somente pelo usuário.
- **Q-05 — Movimento:** usar apenas as bordas das áreas úteis dos monitores no MVP; janelas de outros aplicativos ficam para depois. Atravessar monitores fica ligado por padrão, com opção para desligar.
- **Q-06 — Atalhos:** não haverá atalhos para controlar o personagem no MVP.
- **Q-07 — Gestos:** clique reage, clique duplo abre a conversa e botão direito abre o menu.
- **Q-09 — Tela cheia:** esconder ou deixar o Buzzy quieto durante outro aplicativo em tela cheia, se o protótipo P7 confirmar custo desprezível.
- **Q-10 — Build pessoal:** começar com um ZIP portátil sem assinatura nem instalador, para uso pessoal e testes. Se decidir publicar para outras pessoas, possivelmente pelo Git, formato e assinatura serão revistos antes. Como empacotar o runtime .NET ainda será definido no plano do build.
- **Q-11 — Ordem final:** primeiro verificar o MVP na Fase 10; depois otimizar na Fase 11 e repetir a regressão.
- **Q-12 — Idioma:** somente português do Brasil no MVP.
- **Q-14 — Toque e caneta:** ficam para depois do MVP; o MVP mira mouse e touchpad.
- **Q-20 — Acessibilidade:** conversa e configurações navegáveis por teclado e utilizáveis por leitor de tela; a janela do personagem não será alvo de leitor de tela.
- **Q-21 — Modo fantasma:** não incluir click-through total no MVP.

## Escolhas que ficam para depois

- **Q-08 — Metas de desempenho:** definir depois da medição do protótipo P2. Não é necessário escolher números antes de ver os resultados.
- **Q-10 — Publicação pública:** decidir formato e assinatura somente se o projeto for distribuído para outras pessoas.

## O que ainda impede começar a Fase 1

As escolhas de produto necessárias para a Fase 1 estão respondidas. Ainda é preciso concluir os protótipos técnicos P1, P3 e P2, revisar os resultados e fechar a Fase 0. A Fase 1 só começa depois disso e da sua autorização explícita.

As decisões anteriores sobre a stack, os protótipos e as imagens de referência estão registradas em [DECISIONS.md](DECISIONS.md); elas não precisam ser respondidas novamente.

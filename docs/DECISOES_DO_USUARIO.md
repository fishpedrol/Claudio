# Decisões do usuário — Buzzy

Resumo de decisões vigentes; os detalhes, alternativas e histórico ficam em [DECISIONS.md](DECISIONS.md). STATUS: decisões de produto PLANNED até serem implementadas e verificadas.

## Produto

- **2026-09-29:** o Buzzy é um macaquinho com o jeito do Luffy — **chapéu de palha com faixa vermelha e personalidade do Luffy**, semelhança intencional; é a ideia central do projeto (DEC-019). A identidade é pixel art fiel às pranchas (DEC-018).
- Mascote para desktop Windows 11, de uso pessoal; personalidade curiosa e brincalhona por movimento, expressões e gestos não verbais. Sem chat, texto, voz, IA integrada, tarefas gerais, rede ou coleta remota.
- Conceito visual das pranchas de referência, com o chapéu de palha (DEC-019). Claude está autorizado a criar e ajustar a identidade sem aprovação rotineira. Consulte PRODUCT_SPEC.md para limites contra cópia.
- Mouse e touchpad no MVP. Clique gera reação não verbal; duplo clique abre na Fase 8 um painel compacto só com o seletor de energia; botão direito abre o menu. Arraste manual tem prioridade sobre comportamento autônomo.
- O personagem circula por superfícies aprovadas do desktop, escala bordas, pode ficar pendurado brevemente e atravessa caminhos válidos entre monitores. Janelas de outros aplicativos não são superfícies.
- Energia Baixa/Média/Alta, Média padrão, altera frequência e duração das ações; não muda física, segurança nem prioridade do usuário.
- Modo de tela cheia ligado por padrão: mover para monitor livre ou ocultar; restaurar posição e respeitar ações manuais. P7 é gate antes da Fase 8.
- Windows 11 24H2 ou posterior é o alvo inicial. Iniciar com o Windows é opcional e desligado por padrão; configurações acessíveis por teclado e leitor de tela. Sem modo fantasma, atalhos globais, toque ou caneta no MVP.
- Uso pessoal; não há plano de distribuir o aplicativo pronto. O formato inicial de teste é ZIP portátil sem assinatura/instalador. Visibilidade do repositório e eventual release para terceiros ficam para decisão futura.

## Estado dos protótipos

- **P1:** aceite como PASS somente no ambiente medido (Windows 11, 96 DPI); parte dos cliques foi sintética.
- **P2:** aceite como medição de viabilidade; animação não atingiu 60 qps e houve crescimento de memória. Investigar antes da Fase 6.
- **P3:** o receptor confirmou os cenários centrais em três rodadas com input sintético; o relatório agregado ainda falha numa tentativa adicional B4b abortada pela salvaguarda, corrigida depois sem nova execução registrada. Os dez movimentos anteriores foram exploração informal da namorada do usuário, não teste formal. Veja TODO.md.
- P1/P2 aceitos não significam que o aplicativo foi validado nem encerram a Fase 0.

## Autorização de execução — 2026-09-29

O usuário autorizou Claude a criar a identidade visual, fechar os gates técnicos, implementar, testar, corrigir e documentar as fases 1–11 em sequência, sem pedir aprovação rotineira de plano, conceito ou avanço. A Fase 1 já está autorizada após a conclusão técnica da Fase 0. Claude deve continuar tarefas independentes quando um teste depender de hardware indisponível, mantendo o item pendente e sem declarar fase VERIFIED antes da evidência exigida. Veja DEC-015 e [prompt_usuario.md](../prompt_usuario.md).

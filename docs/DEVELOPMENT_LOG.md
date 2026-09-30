# DEVELOPMENT_LOG.md — Histórico essencial

> Registro resumido de marcos e correções. Critérios e números técnicos ficam em [TODO.md](TODO.md); decisões permanentes ficam em [DECISIONS.md](DECISIONS.md); evidências brutas, quando existirem, ficam em spikes/resultados/.
>
> Atualizado em 2026-09-29.

## Marcos

- **2026-09-25 a 2026-09-26 — Estrutura inicial:** definidos o mascote original, o escopo local sem IA integrada, limites de segurança, documentação canônica e roadmap. WPF com C#/.NET 10 foi escolhido em DEC-006; protótipos descartáveis P1, P2 e P3 foram criados em spikes/.
- **2026-09-26 — Protótipos:** P1, P2 e P3 foram medidos nos limites descritos em TODO.md. P2 registrou repouso sem atividade periódica evitável, desempenho de animação abaixo de 60 qps e crescimento de memória em dez minutos. Os detalhes e limites permanecem em TODO.md.
- **2026-09-27 — Aceites limitados:** o usuário aceitou P1 apenas no ambiente medido (Windows 11, 96 DPI), P2 como medição de viabilidade e as metas Q-08. Esses aceites não validam o aplicativo nem fecham a Etapa 0B.
- **2026-09-28 — Direção do produto:** clarificado mascote não verbal, curioso e brincalhão, com energia Baixa/Média/Alta e modo de tela cheia sujeito a P7. P4 foi aposentado; detalhes em DEC-013/014, PRODUCT_SPEC.md e TODO.md.
- **2026-09-29 — Correção e autorização:** os dez movimentos de mouse anteriormente chamados de “reais” foram feitos pela namorada do usuário enquanto explorava o protótipo. Foi uma observação informal, não um teste formal do usuário nem evidência humana aprovada de P3. Input por SendInput continua classificado como sintético. O usuário autorizou Claude a criar a identidade visual original e implementar/testar as fases 1–11 em sequência, sem aprovações rotineiras; a Fase 1 começa após os gates técnicos da Fase 0 (DEC-015).

## Estado desta atualização documental

- P3: três rodadas do harness registram os cenários centrais como OK, incluindo entrega ao receptor e foco. O relatório completo marca 0/3 por B4b adicional ter sido abortado pela salvaguarda ao detectar outra janela no ponto de reativação; nenhum clique foi enviado. Claude informou ter corrigido o harness, mas ainda não há relatório posterior à correção. Estado: aguardando nova saída ou registro explícito de que B4b não é critério de gate.
- As referências visuais permanecem em assets/references/. Claude está autorizado a criar identidade própria sem aprovação rotineira; Fase 1 usa placeholder e a integração animada está na Fase 6.
- Claude já criou arquivos iniciais do app e testes em src/ e tests/. Este Codex revisou e enxugou os Markdown, sem alterar código, executar build, testes ou scripts.
- STATUS do aplicativo: PLANNED; não há código de produto implementado. Os protótipos em spikes/ são descartáveis.

# Continuação da Fase 0 — completar a proposta técnica do Buzzy

Continue o trabalho existente neste repositório. O pedido inicial da Fase 0 já foi enviado a você pelo terminal; o usuário limpou `prompt_usuario.md` depois de copiar aquele texto. Isso não significa que o trabalho foi perdido nem que deva ser refeito do zero. Este arquivo contém agora a mensagem de continuação e pode ser limpo novamente depois de ser copiado.

**Workflow em andamento:** existe um workflow dinâmico chamado `buzzy-stack-research`, pausado pelo limite de uso. A captura do terminal indica que a fase de Pesquisa chegou a 14/14 e a de Verificação está em 68/84. Antes de pesquisar ou comparar tecnologias novamente, tente retomar ou recuperar os resultados deste mesmo workflow. Não crie outro workflow concorrente nem descarte o atual. Se não conseguir acessá-lo, diga isso claramente e aguarde orientação em vez de repetir a pesquisa.

## Primeiro, leia e confirme o estado

Leia `AGENTS.md`, `CLAUDE.md`, `docs/PROJECT_CONTEXT.md`, `docs/PRODUCT_SPEC.md`, `docs/TODO.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md`, `docs/SECURITY.md`, `docs/DEVELOPMENT_LOG.md`, `docs/PLAN_REVIEW.md` e `context codex/PROMPT_MESTRE_BUZZY.md`. Inspecione também o Git e os arquivos reais. Preserve o trabalho útil já feito e corrija apenas as lacunas comprovadas.

O repositório já tem Git local, sem remoto. Não reinicialize o Git. Ainda não há código de produto, build ou testes. A Fase atual continua sendo **Fase 0 — Descoberta e arquitetura** e não está concluída.

## O que falta nesta continuação

1. `docs/DECISIONS.md`, seção DEC-006, ainda contém só um espaço reservado. Complete a comparação de stack e registre critérios ligados ao produto, alternativas consideradas, fontes primárias consultáveis, trade-offs, riscos, recomendação e incertezas. Não marque Q-01 como aprovada: a recomendação continua pendente da decisão do usuário.
2. `docs/ARCHITECTURE.md`, seção 2.13, ainda é um espaço reservado. Explique como a arquitetura proposta se encaixa na stack recomendada; deixe claros limites da stack e pontos que dependem de protótipos.
3. `docs/SECURITY.md`, seção 4, ainda é um espaço reservado. Registre processos auxiliares, permissões, superfície de rede, arquivos, distribuição e riscos específicos da stack, sem prometer isolamento que a tecnologia não oferece.
4. Confira os itens marcados como concluídos em `docs/TODO.md`. A comparação de stack e permissões por stack não estão documentadas, portanto continuam pendentes até que os entregáveis acima existam. O TODO afirma que uma revisão adversarial anterior foi recuperada, mas `docs/DEVELOPMENT_LOG.md` não contém esse resultado: confirme se a evidência existe nos arquivos/histórico disponíveis; se não existir, registre que não foi possível recuperá-la e deixe a tarefa pendente.
5. Depois de preencher os documentos, sincronize os estados e o resumo do projeto conforme `AGENTS.md`. Atualize `docs/DEVELOPMENT_LOG.md` com o que de fato mudou e com as fontes consultadas. Não marque como `VERIFIED` algo que não foi implementado e testado.

## Limites deste trabalho

- Trabalhe apenas na conclusão documental da Fase 0. Não implemente o aplicativo, não inicie a Fase 1 e não crie dependências ou protótipos de viabilidade agora.
- Q-01 a Q-14 continuam escolhas do usuário. Q-13 trata de fazer P1 a P3 antes ou no começo da Fase 1; não execute P1 a P3 nem escolha essa opção por conta própria.
- Preserve a visão e os limites de `docs/PRODUCT_SPEC.md`; não aumente o escopo do MVP por inferência.
- Preserve as partes úteis já escritas em `ARCHITECTURE.md`, `SECURITY.md` e `TODO.md`. Não reescreva esses documentos inteiros para preencher três lacunas.
- A imagem `assets/references/buzzy-character-concept.png` é referência conceitual do personagem. Ela não é uma folha de sprites pronta; o cursor desenhado nela não pertence ao personagem.
- Use o modelo Claude escolhido pelo usuário (Fable ou Opus). Não fixe uma versão nem dependa de um modelo específico.

## Ao terminar

Apresente um resumo curto do que completou e do que ainda falta, a recomendação de stack e seus principais motivos e trade-offs, as fontes consultadas, e se conseguiu recuperar a revisão adversarial anterior. Pare depois da entrega documental e aguarde a revisão independente do Codex e as decisões do usuário. Não declare a Fase 0 concluída.

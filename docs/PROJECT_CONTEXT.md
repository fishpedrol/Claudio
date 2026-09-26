# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Leia este resumo antes de qualquer trabalho. A especificação do produto define o que o Buzzy deve ser; este arquivo registra o estado e a próxima fase.
>
> Última sincronização documental: 2026-09-26

## Estado do projeto

STATUS: PLANNED. O produto está em planejamento. A inspeção atual encontrou documentação, sem código de aplicação, repositório Git, stack escolhida, build ou testes.

## Produto

O Buzzy é um companheiro de desktop para Windows, inspirado na experiência lúdica dos mascotes de desktop da era do BonziBuddy, mas com personagem, identidade visual, conteúdo e implementação originais. O produto deve ser moderno, seguro, leve, interativo e previsível.

A especificação de referência está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md). Ela define o MVP, os limites de segurança, a prioridade da interação do usuário e as decisões que não podem ser inferidas apenas do código.

## Estado atual das decisões

- Identidade e intenção do produto: STATUS: PLANNED.
- Escopo do MVP e exclusões: STATUS: PLANNED; detalhes em PRODUCT_SPEC.md.
- Stack e tecnologia de desktop: STATUS: UNCERTAIN; comparar alternativas na Fase 0.
- Arquitetura de software: STATUS: PLANNED; princípios e decisões pendentes em ARCHITECTURE.md.
- Segurança: princípios do produto definidos; detalhes da implementação aguardam a Fase 0, em SECURITY.md.
- Implementação, build e testes: inexistentes nesta inspeção.

## Inventário resumido do projeto

STATUS: PLANNED. A árvore atual contém README.md, AGENTS.md, CLAUDE.md, prompt_usuario.md, os documentos em docs/ e os índices/prompts em context codex/. A lista real de arquivos é a fonte exata da estrutura; não há módulos de software.

| Área | Estado atual |
|---|---|
| Módulos e funcionalidades implementados | Nenhum código ou funcionalidade de produto encontrado. |
| Funcionalidades parcialmente implementadas | Nenhuma identificada. |
| Funcionalidades pendentes | MVP descrito em PRODUCT_SPEC.md e dividido por fase em TODO.md. STATUS: PLANNED. |
| Máquina de estados e eventos | Conceito determinístico definido; modelo e transições detalhados pendentes da Fase 0. STATUS: PLANNED. |
| Input e drag | Prioridade do usuário e ciclo esperado definidos em PRODUCT_SPEC.md; ainda não implementados. STATUS: PLANNED. |
| Movimento, superfícies e multi-monitor | Requisitos descritos; modelo técnico e implementação pendentes. STATUS: PLANNED. |
| Rendering, animações e expressões | Asset provisório e separação entre movimento e visual previstos; stack e detalhes pendentes. STATUS: PLANNED. |
| Configurações e persistência | Persistência local prevista; formato, campos finais e localização pendentes. STATUS: PLANNED. |
| Permissões e segurança | Limites de produto definidos em SECURITY.md; APIs necessárias ainda não identificadas. STATUS: PLANNED. |
| Testes | Nenhum teste ou infraestrutura de build encontrada; estratégia e critérios pendentes da Fase 0. STATUS: PLANNED. |
| Limitações conhecidas | Não há app, stack, build, testes ou repositório Git; comandos de execução ainda não existem. |
| Bugs conhecidos | Nenhum bug de implementação identificado; ainda não há comportamento executável para inspecionar. |
| Decisões importantes | Registradas em DECISIONS.md; stack permanece STATUS: UNCERTAIN. |
| Instruções para executar e testar | Ainda não aplicáveis; serão documentadas depois da escolha da stack e criação do build. STATUS: PLANNED. |

## Fase atual

Fase 0 — Descoberta e arquitetura. STATUS: PLANNED.

O objetivo é reconciliar os documentos, comparar opções tecnológicas com evidências, recomendar uma stack e definir as fronteiras mínimas da arquitetura, o comportamento, a estratégia de testes, os critérios de aceitação e os limites mensuráveis de desempenho. A Fase 0 não implementa o produto.

## Próxima ação

Concluir a Fase 0 conforme a lista em [TODO.md](TODO.md). A stack não deve ser escolhida por preferência ou popularidade: a recomendação precisa comparar Windows integration, transparência, tray, input, monitores/DPI, consumo, estabilidade, distribuição e manutenção.

## Fluxo de planejamento e revisão

Claude prepara o plano da fase atual. Codex atua como revisor independente: compara o plano com PRODUCT_SPEC.md, o estado real do projeto, ARCHITECTURE.md, DECISIONS.md e TODO.md; aponta lacunas, riscos, premissas e divergências; e apresenta uma versão corrigida. O usuário decide quando iniciar a implementação.

O prompt operativo variável de Claude/Fable fica em [context codex/PROMPT_MESTRE_BUZZY.md](../context%20codex/PROMPT_MESTRE_BUZZY.md) e é atualizado para a fase vigente. HANDOFF.md é somente uma ponte de compatibilidade. [PLAN_REVIEW.md](PLAN_REVIEW.md) define o procedimento estável de revisão. Nenhum desses documentos substitui PRODUCT_SPEC.md como referência do produto.

## Mapa da documentação

| Documento | Fonte de verdade para | Atualizar quando |
|---|---|---|
| PRODUCT_SPEC.md | Visão do Buzzy, escopo do MVP e restrições | O usuário alterar uma decisão de produto |
| PROJECT_CONTEXT.md | Estado atual, fase e próximos passos | Ao fim de toda fase |
| PLAN_REVIEW.md | Critérios para revisar planos de Claude | O processo de revisão mudar |
| ARCHITECTURE.md | Arquitetura implementada e desenho proposto identificado como planejado | Quando o desenho ou a arquitetura real mudar |
| DECISIONS.md | Decisões e alternativas descartadas | Quando uma decisão relevante for aceita ou substituída |
| SECURITY.md | Regras de segurança, dados e permissões | Quando o modelo de segurança mudar |
| TODO.md | Fases, tarefas, critérios e bloqueios | Ao fim de toda fase |
| DEVELOPMENT_LOG.md | Histórico cronológico das fases | Ao fim de toda fase |
| context codex/PROMPT_MESTRE_BUZZY.md | Prompt operativo e seção variável da fase atual de Claude/Fable | Ao iniciar ou concluir uma fase |

## Regra de sincronização

Ao concluir uma fase, sincronizar nesta ordem:

1. PROJECT_CONTEXT.md
2. DEVELOPMENT_LOG.md
3. ARCHITECTURE.md, se o desenho ou a arquitetura mudou
4. DECISIONS.md, se uma decisão foi aceita ou substituída
5. SECURITY.md, se o modelo de segurança mudou
6. TODO.md
7. PROMPT_MESTRE_BUZZY.md para a próxima fase
8. Conferir cada afirmação contra os arquivos e resultados reais

STATUS: PLANNED não significa concluído. STATUS: VERIFIED só pode ser usado quando a implementação existe e os testes correspondentes foram executados. Escolhas ainda sem evidência permanecem STATUS: UNCERTAIN.

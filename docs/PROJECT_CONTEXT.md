# PROJECT_CONTEXT.md — Estado atual do Buzzy

> Leia este resumo antes de qualquer trabalho. Ele é a fonte resumida de verdade do projeto. A especificação do produto está em [PRODUCT_SPEC.md](PRODUCT_SPEC.md); os demais documentos guardam detalhes especializados e histórico e estão listados no mapa ao final.
>
> Última sincronização documental: 2026-09-26

**Legenda de status**, definida em AGENTS.md:

- `STATUS: VERIFIED`: implementado e testado.
- `STATUS: PLANNED`: planejado, ainda não verificado.
- `STATUS: UNCERTAIN`: escolha ou estado indefinido. Uma proposta que aguarda aprovação do usuário está nesse caso.

## Estado do projeto

Inspeção dos arquivos em 2026-09-26:

- Há documentação, duas pranchas conceituais em `assets/references/` e um repositório Git com dois commits. O remoto `origin` aponta para `https://github.com/fishpedrol/Claudio.git` e o branch `main` o rastreia; se o envio chegou ao servidor não foi verificado. O primeiro commit guarda a documentação como estava antes do planejamento técnico da Fase 0.
- Não há código de aplicação, dependência, build nem teste.
- O pedido inicial da Fase 0 já foi enviado ao Claude pelo terminal; `prompt_usuario.md` é atualizado como instrução operacional para os próximos protótipos. Limpar o arquivo depois de copiar não significa que o trabalho anterior se perdeu.

**Fase atual:** Fase 0 — Descoberta e arquitetura, Etapa 0B. STATUS: PLANNED. A escolha WPF/C#/.NET 10 foi aceita por delegação do usuário e está registrada em DEC-006. A revisão do plano pelo Codex foi concluída. As respostas do usuário para Q-02 a Q-07, Q-09, Q-11, Q-12, Q-14, Q-20 e Q-21 foram registradas; Q-10 escolhe um ZIP portátil sem assinatura para uso pessoal e testes. Permanecem os protótipos descartáveis P1, P3 e P2, a revisão do Codex e Q-08 (metas após P2). Distribuição pública, se houver, será decidida depois. Q-01 e Q-13 estão resolvidas; Q-15 foi resolvida como questão operacional; Q-16 a Q-19 foram esclarecidas como referências visuais; Q-22 não se aplica. A Fase 0 **não** está concluída e a stack não foi validada no aplicativo.

## 1. Objetivo do projeto

O Buzzy é um mascote digital original e interativo para desktop Windows, inspirado na experiência lúdica dos mascotes de desktop da era do BonziBuddy, com personagem, identidade visual, conteúdo e implementação próprios. Ele deve ser moderno, seguro, leve, previsível e parecer um personagem vivendo no desktop, não uma janela convencional. Detalhes em PRODUCT_SPEC.md e DEC-002.

## 2. Escopo atual (MVP)

STATUS: PLANNED. Personagem visível; janela transparente e posicionável; arraste; caminhada, escalada, salto e queda; múltiplos monitores; expressões separadas do movimento; clique e caixa de texto com respostas locais; configurações e persistência local; segurança e desempenho para uso prolongado. Lista completa em PRODUCT_SPEC.md.

## 3. Fora do escopo

LLM e qualquer IA integrada, RAG, embeddings, APIs de IA, voz, backend, nuvem, telemetria, analytics, atualização automática, comandos arbitrários, shell, keylogging, captura silenciosa de tela e controle genérico do computador (DEC-003, DEC-005). Itens adiados para depois do MVP estão em TODO.md, seções POST-MVP e FUTURO.

## 4. Stack atual

**WPF com C# e .NET 10 LTS**, escolhida por delegação explícita do usuário. STATUS: PLANNED. P1, P2 e P3 ainda precisam demonstrar transparência por pixel, repouso adequado e arraste sem roubo de foco antes da Fase 1. A pesquisa que colocou Win32 nativo em primeiro continua como comparação histórica; Win32 fica como alternativa de retorno se um requisito central falhar.

O comparativo cobriu nove alternativas e registrou três avaliações independentes; a pesquisa técnica original favoreceu Win32 nativo por controle e custo potencial, mas a decisão final considera também a facilidade de evoluir e manter o produto por uma pessoa com apoio de agentes. WPF oferece a janela layered documentada e controles de interface prontos; seus riscos de runtime, DPI e repouso serão verificados por P1–P3. P9 e Q-22 foram encerrados como não aplicáveis.

## 5. Arquitetura atual

Nenhuma arquitetura implementada.

Arquitetura planejada (STATUS: PLANNED, detalhes em [ARCHITECTURE.md](ARCHITECTURE.md)):

- **Processo:** um só processo, com núcleo puro e determinístico, um único adaptador que fala com o Windows e uma raiz de composição (DEC-007).
- **Coordenadas:** pixels físicos do desktop virtual, com processo Per-Monitor V2 (DEC-008).
- **Janelas:** a janela do personagem tem o tamanho do sprite e não rouba foco. A caixa de texto fica em outra janela (DEC-009).
- **Persistência:** JSON local versionado com gravação atômica (DEC-010).
- **Tempo:** nada roda em repouso; o relógio só funciona com movimento, animação ou arraste (DEC-011).

## 6. Estrutura de diretórios

```
claudio/                       # nome herdado da pasta local; o projeto é o Buzzy
├── .git/                      # remoto origin: github.com/fishpedrol/Claudio.git; main rastreia origin/main
├── .gitignore                 # mínimo; revisar com os protótipos WPF e o primeiro build
├── assets/
│   └── references/
│       ├── buzzy-character-concept.png  # referência visual para proposta original; incluída em PRODUCT_SPEC.md
│       └── buzzy-character-concept2.png # segunda referência visual; incluída em PRODUCT_SPEC.md
├── AGENTS.md                  # instruções comuns dos agentes: fontes, papéis, sincronização, veracidade
├── CLAUDE.md                  # entrada curta para Claude; aponta para AGENTS.md
├── README.md                  # visão geral e índice da documentação
├── prompt_usuario.md          # próximo prompt de trabalho para o terminal do Claude
├── context codex/
│   ├── PROMPT_MESTRE_BUZZY.md # prompt operativo; atualizar a seção "Fase atual" quando fase ou decisão material mudar
│   ├── context.md             # índice para as fontes canônicas
│   └── HANDOFF.md             # ponte de compatibilidade para o prompt mestre
└── docs/
    ├── PROJECT_CONTEXT.md     # este arquivo
    ├── PRODUCT_SPEC.md        # visão estável, MVP e limites
    ├── DECISOES_DO_USUARIO.md # resumo simples das decisões respondidas e pendências futuras
    ├── ARCHITECTURE.md        # arquitetura implementada (nenhuma) e planejada
    ├── DECISIONS.md           # decisões DEC-nnn e respostas/pendências Q-nn
    ├── SECURITY.md            # segurança, dados e permissões
    ├── TODO.md                # roadmap único de fases, critérios e testes
    ├── DEVELOPMENT_LOG.md     # histórico cronológico
    └── PLAN_REVIEW.md         # critérios do Codex para revisar planos
```

## 7. Módulos existentes

Nenhum. Os módulos planejados (adaptador de plataforma, mundo do desktop, núcleo do personagem, arbitragem de input, movimento, apresentação, conversa local, configurações e raiz de composição) estão em ARCHITECTURE.md, seção 2.2. STATUS: PLANNED.

## 8. Funcionalidades

| Situação | Conteúdo |
|---|---|
| Implementadas | Nenhuma. |
| Parcialmente implementadas | Nenhuma. |
| Pendentes | Todo o MVP, dividido nas Fases 1 a 11 de TODO.md. STATUS: PLANNED. |

## 9. Áreas técnicas

Nenhuma área tem código. Cada linha resume a proposta e aponta para o detalhe.

| Área | Status | Resumo | Detalhe |
|---|---|---|---|
| Máquina de estados | PLANNED | Estados de comportamento (`IDLE`, `WALKING`, `CLIMBING`, `JUMPING`, `FALLING`, `PRESSED`, `DRAGGING`, `SETTLING`, `CONVERSING` e outros) separados da expressão; tabela de transições e invariantes testáveis. | ARCHITECTURE.md 2.6 |
| Eventos | PLANNED | Ponteiro, gestos, caixa de texto, sistema, bandeja, relógio e configurações, com prioridade: usuário, input local, sistema, autonomia. | ARCHITECTURE.md 2.3 e 2.6 |
| Input | PLANNED | Só o input das próprias janelas; clique atravessa pixels transparentes; janela do personagem não rouba foco; nenhum hook ou atalho global. | ARCHITECTURE.md 2.7, DEC-009 |
| Drag | PLANNED | Pressionar congela a autonomia; passar do limiar do sistema inicia `DRAGGING`, que segue o cursor sem física; soltar valida monitor, área útil e apoio antes de retomar. Arraste manual, sem o loop modal do Windows. Depende do protótipo P3. | ARCHITECTURE.md 2.7 |
| Clique e caixa de texto | PLANNED | Soltar dentro do limiar é clique. A caixa de texto fica em janela própria e recebe todo o teclado quando focada. Nenhuma tecla move o personagem. | ARCHITECTURE.md 2.7 |
| Multi-monitor | PLANNED | Topologia arbitrária, coordenadas negativas, escalas e orientações diferentes, conexão e desconexão, restauração em cascata. Fundamentos nas Fases 1 e 3; conclusão na Fase 5. | ARCHITECTURE.md 2.4, 2.5 e 2.8, DEC-008 |
| Movimento | PLANNED | Passo fixo, física em DIPs, superfícies derivadas das áreas úteis (escolha Q-05). | ARCHITECTURE.md 2.5 e 2.9 |
| Rendering | PLANNED | Janela WPF do tamanho do sprite com `AllowsTransparency`; redesenho só quando o quadro muda. P1 verifica clique atravessando alfa 0; P2 mede repouso. | ARCHITECTURE.md 2.10 e 2.13 |
| Animações | PLANNED | Clipes definidos em manifesto de assets, com taxa de quadros própria; asset provisório original e substituível. | ARCHITECTURE.md 2.10 |
| Expressões | PLANNED | Dimensão separada do comportamento; trocar expressão não muda estado nem posição. | ARCHITECTURE.md 2.6 e 2.10 |
| Conversa | PLANNED | Tabela local de intenções, determinística, sem rede e sem gravar o que foi digitado. | ARCHITECTURE.md 2.11 |
| Configurações | PLANNED | Escala, sempre no topo, iniciar com o Windows, autonomia, atravessar monitores, idioma; várias dependem das escolhas Q-03 a Q-12. | ARCHITECTURE.md 2.12, DECISIONS.md |
| Persistência | PLANNED | `settings.json` versionado na pasta local do usuário, com gravação atômica; posição a partir da Fase 5. | DEC-010, SECURITY.md 5 |
| Permissões | PLANNED | Nenhuma elevação, nenhuma rede, nenhuma leitura de outros aplicativos; lista de APIs proibidas verificada no build. | SECURITY.md 2 a 4 |
| Desempenho | PLANNED | Métricas M1 a M7 e protocolo de medição; metas numéricas dependem de medição no protótipo P2 (escolha Q-08). | DEC-011 |
| Testes | PLANNED | Toda fase tem critérios [AUTO], [MANUAL] e [HW]; núcleo testável sem janela; a Fase 10 é regressão integrada, não o primeiro teste. | TODO.md |

## 10. Limitações conhecidas

- Não há aplicativo, build ou teste; nada pode ser executado.
- Várias afirmações técnicas só se confirmam com protótipo: clique através de pixels transparentes, custo em repouso, arraste sem roubar foco, foco da caixa de texto, mensagens de topologia, estabilidade da chave do monitor e esforço da interface nativa (P1 a P10 em TODO.md).
- As verificações [HW] exigem dois ou mais monitores com escalas ou orientações diferentes. A disponibilidade desse hardware não foi confirmada.
- Só 84 das 300 alegações decisivas da pesquisa de stack passaram por verificação adversarial, e dois terços dessas precisaram de correção. As 216 restantes não foram verificadas.
- Nenhuma das nove stacks comparadas tem número oficial de consumo de memória, CPU ou GPU no Windows. Todos os números encontrados são de terceiros e vários foram corrigidos na verificação adversarial por exagerar o que a fonte sustenta. A comparação de consumo em DEC-006 é de mecanismo, não de medição.
- A viabilidade real de janela layered WPF, repouso e arraste precisa dos protótipos P1–P3. O custo comparativo de controles nativos (P9) não é mais relevante para a escolha atual.
- A pesquisa de stack esgotou o limite de buscas na web da sessão. Parte da verificação adversarial trabalhou abrindo as URLs já citadas, sem novas buscas.
- A revisão adversarial documental de fundo terminou, mas **não é recuperável a partir deste repositório**: nenhum achado ou relatório dela existe nos arquivos versionados nem no histórico. A tarefa correspondente continua pendente em TODO.md. Além disso, ela avaliou a versão dos documentos de 2026-09-25, anterior à reestruturação do Buzzy, então não valeria como revisão do plano atual.

## 11. Bugs conhecidos

Nenhum bug de implementação, porque não há código.

## 12. Decisões importantes

| ID | Assunto | Estado |
|---|---|---|
| DEC-001 | Documentação viva | ACCEPTED |
| DEC-002 | Mascote original | ACCEPTED |
| DEC-003 | MVP local sem IA | ACCEPTED |
| DEC-004 | Prioridade do usuário | ACCEPTED |
| DEC-005 | Segurança explícita | ACCEPTED |
| DEC-006 | Stack escolhida: WPF/C#/.NET 10; pesquisa técnica comparativa preservada | ACCEPTED; P1–P3 ainda pendentes |
| DEC-007 | Um processo, núcleo puro, adaptador único | UNCERTAIN, proposta |
| DEC-008 | Coordenadas e monitores | UNCERTAIN, proposta |
| DEC-009 | Arbitragem de input e foco | UNCERTAIN, proposta |
| DEC-010 | Persistência local | UNCERTAIN, proposta |
| DEC-011 | Ociosidade e medição de desempenho | UNCERTAIN, proposta |
| DEC-012 | Revisão do roadmap | UNCERTAIN, proposta |

As decisões de produto atualmente respondidas e as pendências estão discriminadas em [DECISIONS.md](DECISIONS.md). Q-08 depende da medição P2; formato e assinatura de uma eventual distribuição pública continuam para decisão futura em Q-10. Q-01 e Q-13 foram resolvidas por delegação do usuário; Q-15 foi resolvida como questão operacional; Q-16 a Q-19 foram esclarecidas como referências visuais, sem decisões implícitas; Q-22 não se aplica.

As referências visuais devem orientar Claude a criar uma proposta original para o Buzzy. Nome, acessórios, poses e estilo vistos nelas não são requisitos aprovados; a nova proposta deve ser apresentada ao usuário para aprovação.

## 13. Próxima fase

1. Claude executa P1, P3 e P2 como protótipos descartáveis WPF conforme `prompt_usuario.md`; não inicia o produto.
2. Codex revisa o código e os resultados dos protótipos, propõe Q-08 a partir das medidas e reabre DEC-006 se P1 ou P3 falharem.
3. Sincronizar os documentos e concluir a revisão da Fase 0. As escolhas atuais de plataforma, janela e ZIP de teste já foram respondidas.
4. Só depois da Fase 0 fechada e da autorização do usuário começa a **Fase 1 — Shell do desktop** (TODO.md).

## 14. Instruções para executar

Não aplicável: não há aplicativo. As instruções entram aqui na Fase 1, depois da escolha da stack.

## 15. Instruções para testar

Não há testes de código. Para conferir a documentação, verifique se os links relativos entre os arquivos de `docs/`, o README e `context codex/` resolvem, e se cada afirmação sobre o estado confere com a árvore real de arquivos.

## Fluxo de planejamento e revisão

Claude/Fable prepara o plano da fase atual. Codex atua como revisor independente: compara o plano com PRODUCT_SPEC.md, o estado real do projeto, ARCHITECTURE.md, DECISIONS.md e TODO.md, aponta lacunas, riscos, premissas e divergências, e apresenta uma versão corrigida. O usuário decide quando iniciar a implementação.

O prompt operativo variável fica em [context codex/PROMPT_MESTRE_BUZZY.md](../context%20codex/PROMPT_MESTRE_BUZZY.md). HANDOFF.md é só uma ponte de compatibilidade. [PLAN_REVIEW.md](PLAN_REVIEW.md) define o procedimento estável de revisão. Nenhum desses documentos substitui PRODUCT_SPEC.md como referência do produto.

A regra de sincronização ao fim de cada fase e a regra de veracidade estão em [AGENTS.md](../AGENTS.md).

## Mapa da documentação

| Documento | Fonte de verdade para | Atualizar quando |
|---|---|---|
| AGENTS.md | Instruções comuns dos agentes, papéis, sincronização e veracidade | O processo de trabalho mudar |
| CLAUDE.md | Entrada curta de Claude, que aponta para AGENTS.md | Raramente |
| README.md | Visão geral e índice | Um documento for criado ou renomeado |
| docs/PRODUCT_SPEC.md | Visão do Buzzy, escopo do MVP e restrições | O usuário alterar uma decisão de produto |
| docs/DECISOES_DO_USUARIO.md | Resumo legível das escolhas respondidas e das pendências futuras; DECISIONS.md continua sendo a fonte oficial | Uma escolha for resolvida ou mudar |
| docs/PROJECT_CONTEXT.md | Estado atual, fase e próximos passos | Ao fim de toda fase |
| docs/ARCHITECTURE.md | Arquitetura implementada e desenho planejado, separados por status | O desenho ou a arquitetura real mudar |
| docs/DECISIONS.md | Decisões, alternativas, respostas do usuário e pendências | Uma decisão for proposta, aceita ou substituída |
| docs/SECURITY.md | Segurança, dados e permissões | O modelo de segurança mudar |
| docs/TODO.md | Fases, tarefas, critérios, testes e bloqueios | Ao fim de toda fase |
| docs/DEVELOPMENT_LOG.md | Histórico cronológico | Ao fim de toda fase |
| docs/PLAN_REVIEW.md | Critérios do Codex para revisar planos | O processo de revisão mudar |
| context codex/PROMPT_MESTRE_BUZZY.md | Prompt operativo e seção "Fase atual" | A fase atual ou uma decisão material mudar |
| context codex/context.md | Índice para as fontes canônicas | Uma fonte canônica mudar de lugar |
| context codex/HANDOFF.md | Ponte de compatibilidade | Raramente |
| prompt_usuario.md | Rascunho temporário do próximo prompt que o usuário copia para o terminal do Fable | Antes do envio; pode ser limpo após copiar |

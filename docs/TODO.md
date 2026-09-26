# TODO.md — Fases e pendências do Buzzy

> Esta lista é o roadmap operativo. Cada fase termina com build, testes e verificação do seu próprio escopo; a Fase 10 é a regressão integrada final, não o primeiro momento de testar.
>
> Última atualização: 2026-09-26

## Fase atual

**Fase 0 — Descoberta e arquitetura. STATUS: PLANNED.**

Nenhum código de produto deve ser iniciado nesta fase. O objetivo é converter PRODUCT_SPEC.md em uma arquitetura e um plano viáveis, mantendo stack e detalhes técnicos como decisões abertas até haver comparação.

### Pendências da Fase 0

- [x] Consolidar a visão do Buzzy e os limites do MVP em uma fonte de produto.
- [x] Separar a especificação estável do produto do prompt mestre variável por fase.
- [x] Definir o ciclo Claude/Fable → revisão independente do Codex → decisão do usuário.
- [x] Criar README com links para as fontes canônicas.
- [x] Preservar o trabalho documental anterior e indicar como recuperar a revisão de fundo iniciada por Claude/Fable.
- [x] Manter em PROJECT_CONTEXT.md um inventário resumido de todas as áreas do projeto, marcando o que ainda é planejado ou incerto.
- [ ] Comparar alternativas de stack para Windows com critérios, fontes e trade-offs; recomendar uma sem escolher por preferência.
- [ ] Definir arquitetura mínima: shell, desktop world, núcleo determinístico, input, movimento, apresentação, configurações e fronteiras do sistema operacional.
- [ ] Especificar máquina de estados, eventos, arbitragem de input, distinção entre clique e drag, foco da caixa de texto e ciclo de drag.
- [ ] Definir coordenadas do desktop virtual, superfícies, DPI, reconexão e critérios multi-monitor antes de implementar arraste e movimento.
- [ ] Especificar persistência local mínima e permissões necessárias para a stack escolhida.
- [ ] Definir cenários de aceitação e estratégia de testes por fase.
- [ ] Definir métricas mensuráveis de RAM, CPU em idle, estabilidade prolongada e responsividade.
- [ ] Inicializar Git e .gitignore; criar instruções de build/teste quando a stack for escolhida.

### Critério para concluir a Fase 0

- Comparação técnica e recomendação de stack estão registradas em DECISIONS.md, com evidências, riscos e alternativas.
- ARCHITECTURE.md descreve o desenho proposto como PLANNED e não o apresenta como implementado.
- Máquina de estados, fluxo de drag, modelo do desktop virtual, segurança, persistência, testes e métricas têm critérios verificáveis.
- A documentação e a seção Fase atual do prompt mestre descrevem a mesma próxima fase.
- O usuário decide iniciar a Fase 1.

## Roadmap do MVP

Cada fase abaixo tem STATUS: PLANNED. Testes e critérios de conclusão são definidos na Fase 0 e executados continuamente; nenhum marco é considerado concluído só porque compila.

### Fase 1 — Desktop shell

- Janela transparente, posicionamento e ciclo de vida.
- Tray, restauração e comportamento always-on-top se aprovados na Fase 0.
- Base de coordenadas do desktop virtual e descoberta inicial de monitores.
- Critério: build e comportamento básico verificados no ambiente Windows definido.

### Fase 2 — Character engine

- Núcleo determinístico, eventos e máquina de estados mínima.
- Separação entre estado de movimento e expressão.
- Critério: transições e eventos definidos podem ser verificados sem depender de IA.

### Fase 3 — Input e drag

- Seleção, início, acompanhamento e término do arraste.
- Interação direta cancela movimento autônomo incompatível; release valida posição, monitor e superfície.
- Critério: o mascote nunca disputa o cursor durante DRAGGING.

### Fase 4 — Movimento e superfícies

- Caminhada, escalada, salto e queda, conforme escopo confirmado do MVP.
- Critério: movimentos locais determinísticos sobre superfícies especificadas.

### Fase 5 — Multi-monitor completo

- Posições relativas arbitrárias, resoluções, orientação, DPI, monitor principal, desconexão e mudança de configuração.
- Critério: drag e posição persistida continuam corretos nos cenários definidos na Fase 0.

### Fase 6 — Rendering e expressões

- Asset provisório substituível, animações e expressões separadas da lógica de movimento.
- Critério: estado e expressão visuais não alteram a máquina de movimento.

### Fase 7 — Interação e caixa de texto

- Clique/interação, entrada de texto e respostas locais.
- Sem LLM, API de IA, RAG ou backend no MVP.

### Fase 8 — Configurações e persistência local

- Preferências e posição armazenadas localmente, conforme escopo e formato definidos.
- Critério: persistência previsível, validada e recuperável nos cenários definidos.

### Fase 9 — Segurança e hardening

- Revisar permissões, armazenamento, processos, dependências e superfície de rede.
- Critério: confirmar as restrições de SECURITY.md na implementação.

### Fase 10 — Verificação integrada

- Executar regressão do MVP, cenários de aceitação e testes integrados.
- Corrigir problemas encontrados e documentar resultados.

### Fase 11 — Performance e acabamento

- Medir consumo de RAM/CPU, atividade em idle, responsividade e estabilidade prolongada.
- Critério: comparar os resultados com as metas definidas na Fase 0.

## Fora do MVP

LLM, RAG, embeddings, vector database, APIs de IA, voz, reconhecimento de voz, backend, sincronização em nuvem, telemetria, analytics, atualização automática, execução arbitrária de comandos, shell genérico, keylogging, captura silenciosa de tela e controle genérico do computador.

## Bugs e bloqueios

Nenhum bug de implementação foi avaliado porque não há código. Stack e arquitetura continuam pendentes da Fase 0.

# ARCHITECTURE.md — Arquitetura do Buzzy

> Esta página separa fatos implementados de desenho planejado. Não descreva proposta como arquitetura existente.
>
> Última atualização: 2026-09-26

## Arquitetura implementada

STATUS: UNCERTAIN. A inspeção atual não encontrou código de aplicação, framework, dependências, build ou testes. Portanto, ainda não existe arquitetura implementada.

## Direção arquitetural planejada

STATUS: PLANNED. As fronteiras abaixo são hipóteses para a Fase 0, não uma escolha de stack ou uma lista final de módulos. A proposta precisa ser validada contra a tecnologia escolhida e registrada em DECISIONS.md.

### Fronteiras candidatas

- **Desktop shell:** janela, ciclo de vida, tray e integração específica com Windows.
- **Desktop world:** coordenadas do desktop virtual, monitores, DPI, superfícies e mudanças de topologia.
- **Character core:** estado determinístico, transições, eventos e comportamento local.
- **Input arbitration:** seleção e arraste do personagem; interação direta do usuário interrompe comportamentos autônomos incompatíveis.
- **Movement:** caminhada, escalada, salto e queda sobre as superfícies disponíveis.
- **Presentation:** renderização, asset, animação e expressão, separados da lógica de movimento.
- **Local settings:** preferências e posição persistidas localmente, apenas no formato mínimo necessário.
- **Permission boundary:** operações específicas e explícitas do sistema operacional, sem comandos genéricos.

Essas fronteiras não implicam processos separados, microserviços, plugins ou abstrações genéricas. A Fase 0 deve escolher os limites mais simples que suportem o MVP.

### Fluxo comportamental a preservar

1. Eventos locais e de desktop chegam ao núcleo.
2. A arbitragem de input dá prioridade máxima à interação direta do usuário.
3. O estado e o movimento determinísticos definem a ação atual.
4. A camada visual representa estado e expressão sem controlar a lógica do personagem.
5. Configurações e posição são persistidas localmente de forma explícita.

Durante o arraste, o estado DRAGGING cancela movimentos incompatíveis, acompanha o cursor e, ao soltar, valida a posição no desktop virtual, identifica monitor/superfície e retoma um comportamento permitido.

### Multi-monitor

STATUS: PLANNED. A fundação deve representar a geometria real do desktop virtual, incluindo posições relativas arbitrárias, resoluções, orientação, escala/DPI e monitor principal. A implementação deve reagir a conexão, desconexão e alteração de configuração. Não assumir monitores lado a lado.

O modelo de coordenadas precisa ser definido na Fase 0 e usado pelo shell e pelo arraste antes da fase de conclusão multi-monitor. A fase multi-monitor completa cobre os casos de borda e a reconexão.

### IA futura

STATUS: PLANNED. O MVP funciona sem LLM, RAG, API, memória de IA ou rede. Uma integração futura, se aprovada, deve ser um adaptador opcional fora do núcleo determinístico; desligá-la ou ficar indisponível não pode interromper o personagem. Não construir agora uma interface genérica de provider sem necessidade do MVP.

## Decisões ainda pendentes da Fase 0

- Stack, framework e APIs de janela.
- Modelo concreto de estado, eventos e transições.
- Representação de superfícies e colisões.
- Estratégia de rendering e animações.
- Formato e localização da persistência local.
- Permissões realmente necessárias.
- Critérios de aceitação e cenários de teste.
- Metas mensuráveis de CPU, RAM e atividade em idle.

## Histórico de mudanças arquiteturais

| Data | Mudança | Decisão relacionada |
|---|---|---|
| 2026-09-25 | Não havia arquitetura de software; foi criada a documentação inicial. | DEC-001 |
| 2026-09-26 | Registradas fronteiras candidatas e requisitos arquiteturais planejados, sem escolher stack. | DEC-002 a DEC-005 |

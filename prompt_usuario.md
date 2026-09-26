# Etapa 0B — validar a stack WPF do Buzzy

Estou retomando o trabalho porque o terminal/conversa anterior foi encerrado. Use este prompt e os arquivos do repositório como contexto canônico; não dependa de lembrar a conversa anterior. Leia primeiro `AGENTS.md`, `CLAUDE.md`, `docs/PROJECT_CONTEXT.md`, `docs/PRODUCT_SPEC.md`, `docs/TODO.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md`, `docs/SECURITY.md`, `docs/DEVELOPMENT_LOG.md`, `docs/PLAN_REVIEW.md` e `context codex/PROMPT_MESTRE_BUZZY.md`. Inspecione o estado real do Git e dos arquivos antes de alterar qualquer coisa. Se houver protótipos, resultados ou documentação parcialmente atualizada, preserve-os e continue do ponto real; não sobrescreva nem refaça trabalho sem necessidade.

## Decisão vigente

O usuário delegou ao Codex a escolha da stack e dos próximos passos. A escolha registrada é **WPF com C# e .NET 10 LTS** (DEC-006). Não volte a comparar as nove stacks nem trate Win32 como recomendação atual. Win32 nativo é uma alternativa caso os requisitos decisivos falhem nos protótipos.

As duas imagens em `assets/references/` são referências para uma futura proposta visual original melhor. Não são arte final nem especificação literal. Não copie o nome, acessórios, poses ou estilo como requisitos. O escopo de implementação deste ciclo é técnico; a proposta visual ao final será apenas conceitual, sem criar ou alterar arte/assets.

As respostas do usuário já estão registradas em `docs/DECISOES_DO_USUARIO.md` e na tabela canônica de `docs/DECISIONS.md`. Não pergunte de novo sobre Windows 11, comportamento da janela, superfícies, atalhos, gestos, início com o Windows, tela cheia, ZIP pessoal, ordem das fases, idioma, toque/caneta, acessibilidade ou modo fantasma. Q-08 será decidida depois da medição P2. Formato e assinatura de uma eventual publicação pública ficam para depois; o primeiro ZIP sem assinatura é somente para uso pessoal e testes.

## Escopo autorizado deste ciclo

Implemente somente protótipos descartáveis de viabilidade WPF para **P1, P3 e P2, nesta ordem**, conforme `docs/TODO.md`. Coloque-os sob `spikes/`, isolados do aplicativo futuro. Ainda não existe autorização para iniciar a Fase 1 ou transformar o protótipo em produto.

Antes de criar arquivos, confira se o SDK do .NET 10 está instalado e registre a versão do Windows e o estado do repositório. Não instale ferramentas nem altere configurações do sistema silenciosamente. Se faltar o SDK ou algum recurso indispensável, pare e informe ao usuário o passo necessário.

### P1 — clique através dos pixels transparentes

Crie uma janela WPF `AllowsTransparency` do tamanho de uma figura simples de teste, com pixels alfa 0, alfa 1 e pixels visíveis. Prepare instruções simples para colocar a janela sobre o Bloco de Notas e clicar em cada área. Se não puder fazer essa interação física, pare com P1 como pendente e entregue os passos exatos para o usuário; não declare que passou. Registre versão do Windows, DPI, método de desenho, imagem usada e resultado. Não declare o resultado com base apenas na documentação.

### P3 — arraste sem roubar foco

Teste uma janela que não ativa, com captura do mouse ao pressionar. Arraste rapidamente para fora da janela, solte fora, use Alt+Tab durante o gesto e teste ClickLock. Se a verificação exigir interação física e você não puder realizá-la, pare com P3 pendente e forneça passos claros ao usuário; não declare que passou. Confirme que o Bloco de Notas continua com foco e recebe texto antes e depois. Faça uma repetição entre dois monitores se houver hardware disponível; caso contrário, registre essa parte como pendente de hardware, nunca como aprovada. Se a abordagem falhar, teste somente a margem temporária de captura alfa 1 descrita em TODO.md. Qualquer alternativa que roube foco é reprovada. Não introduza hook global, leitura periódica do cursor ou capacidades proibidas. Se continuar falhando, pare e reporte o bloqueio; não escolha outra stack por conta própria.

### P2 — repouso e animação

Meça o protótipo parado por uma hora sem movimento, animações nem timers periódicos; em seguida, meça uma animação simples a 10 e a 60 quadros por segundo por dez minutos cada. Registre CPU, memória privada, GPU e acordadas por segundo, com ferramenta, máquina, resolução, duração e condições reproduzíveis. Se uma métrica não puder ser obtida, marque-a como não medida; não estime nem invente valores. Os dados informam Q-08. Se houver atividade periódica evitável em repouso, ajuste o protótipo e repita a medição antes de reportar.

## Regras de execução

- Use só dependências necessárias e disponíveis no .NET/WPF; fixe versões se adicionar alguma dependência.
- Não use hooks globais, polling de cursor, captura de tela, rede, telemetria, execução de comandos ou acesso a dados de outros aplicativos.
- Mantenha o código descartável separado dos módulos planejados do produto. Não comece a janela, bandeja, configurações ou núcleo da Fase 1.
- Distingua claramente no relatório: o que foi executado, o que foi apenas escrito, o que passou, falhou ou ficou sem hardware/dados.
- Atualize `docs/DEVELOPMENT_LOG.md`, `docs/TODO.md` e `docs/PROJECT_CONTEXT.md` conforme `AGENTS.md`, conferindo cada afirmação. Não marque `VERIFIED` sem a execução da verificação correspondente.
- Se P1 ou P3 falhar, não esconda o resultado nem avance. Registre o problema e pare para revisão do Codex. P2 informa a linha de base; resultado ruim exige corrigir e medir de novo.
- Não altere PRODUCT_SPEC.md nem as decisões de produto durante estes protótipos, exceto para registrar uma lacuna comprovada e necessária.

## Ao terminar

Depois de concluir e registrar os protótipos técnicos, inclua no relatório uma **proposta conceitual preliminar para o personagem** baseada nas referências, separada dos resultados técnicos. Sugira uma direção original (silhueta, paleta, expressão e como funcionaria como mascote de desktop), explique brevemente como ela evita copiar as referências ou personagens existentes e indique o que o usuário precisaria aprovar. Não crie nem altere imagens, sprites ou outros assets; não mude o nome nem as escolhas registradas; não deixe esta proposta atrasar os testes. Ela não aprova arte final nem autoriza a Fase 1.

Entregue um resumo objetivo dos protótipos executados, evidências e medições, resultados de P1/P3/P2, pendências de hardware, dependências adicionadas e arquivos documentais alterados. Indique se WPF segue tecnicamente viável ou se precisa de revisão. Separe com clareza a proposta visual das evidências técnicas. Pare ao final da Etapa 0B; não comece a Fase 1.

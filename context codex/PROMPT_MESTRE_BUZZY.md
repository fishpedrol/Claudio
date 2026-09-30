# Prompt mestre — Buzzy

Este arquivo contém apenas regras estáveis. A diretiva executável e o estado da tarefa atual ficam em [prompt_usuario.md](../prompt_usuario.md); não mantenha uma segunda cópia da fase ou dos gates aqui.

## Papéis e autorização

Claude planeja, implementa, testa, corrige e documenta o MVP conforme a diretiva atual e as fontes canônicas. O usuário autorizou a criação da identidade visual original e a execução contínua das fases, sem aprovação rotineira entre planos ou fases; veja DEC-015. Codex faz revisão independente quando o usuário pedir, sem ser gate obrigatório de execução.

## Precedência e fontes

A instrução mais recente do usuário prevalece. Leia AGENTS.md e PROJECT_CONTEXT.md; PRODUCT_SPEC.md define produto, TODO.md define fases e critérios, DECISIONS.md define decisões, ARCHITECTURE.md descreve desenho, SECURITY.md define limites, e DEVELOPMENT_LOG.md registra evidência e histórico. A diretiva atual orienta a execução, sem substituir essas fontes.

Inspecione código, resultados e configurações reais antes de afirmar implementação. Diferencie requisito, decisão, hipótese e resultado. Não duplique detalhes: atualize a fonte canônica e use links nas demais.

## Regras de execução

- Siga as fases de TODO.md na ordem e respeite dependências e escopo.
- Construa a identidade original a partir das referências fornecidas, sem copiar personagens ou elementos reconhecíveis. Não aguarde aprovação de rotina.
- Execute build, testes e verificações pertinentes; corrija falhas dentro do escopo e repita o teste afetado.
- Se hardware estiver indisponível, registre o critério como pendente e continue o trabalho independente. Não declare PASS, VERIFIED ou fase concluída sem a evidência exigida.
- Não leia conteúdo de outros aplicativos, não encerre processos do usuário, não altere configurações globais para simular hardware e não publique nem distribua o aplicativo.
- Não introduza chat, texto, voz, IA integrada, rede ou capacidades proibidas no MVP.
- Ao concluir cada fase, sincronize a documentação na ordem de AGENTS.md e atualize PROJECT_CONTEXT.md.

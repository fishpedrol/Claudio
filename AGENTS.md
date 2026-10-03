# Instruções para agentes — Buzzy

Buzzy é um mascote de desktop para Windows 11 em C#/.NET 10/WPF, local, não verbal e de uso pessoal. Estas regras valem para Claude e Codex. Fale com o usuário e escreva os documentos em português do Brasil.

## O que ler

Ao abrir a sessão: `CONTINUIDADE.md` ("Em andamento", próximas ações, log recente), `docs/PROJECT_CONTEXT.md` (fases, última validação, comandos), `prompt_usuario.md` (a diretiva do seu papel) e `docs/PRODUCT_SPEC.md` (requisitos).

Depois, só as partes que a tarefa toca:

- `docs/TODO.md`: a seção da fase — critérios, passos, pendências, evidências.
- `docs/DECISIONS.md`: as DEC citadas pela tarefa ou pelo código que vai mudar.
- `docs/ARCHITECTURE.md`: 2.3 fluxo, 2.4 coordenadas e topologia, 2.6 estados e invariantes, 2.7 input e arraste, 2.8 monitores, 2.9 movimento, 2.10 apresentação, 2.12 persistência e tempo, 2.13 APIs do Windows, 2.16 tamagotchi.
- `docs/SECURITY.md`: antes de mexer em permissões, dados, processos, rede ou APIs do Windows.
- `docs/IDENTIDADE_VISUAL.md` (arte, poses, caras); `docs/PLAN_REVIEW.md` (revisão pedida pelo usuário); `docs/DEVELOPMENT_LOG.md` (marcos passados). `docs/arquivo/` guarda o histórico detalhado, só para consulta.

A instrução mais recente do usuário prevalece; nenhum plano de agente substitui uma decisão dele. Código, configuração e resultados executados mostram o que existe: inspecione-os antes de afirmar o estado da implementação.

## Papéis (DEC-015)

- **Claude é o desenvolvedor principal**, com autorização contínua do usuário para criar a identidade visual e executar o projeto fase a fase: planeja, implementa, testa, corrige, documenta e avança sem pedir aprovação de plano ou de fase. Decide o que é técnico dentro dos limites registrados e registra as escolhas materiais em DECISIONS.md.
- **O Codex é o segundo desenvolvedor e o revisor, quando o usuário pedir.** Como desenvolvedor, segue estas regras; como revisor, segue PLAN_REVIEW.md e deixa a árvore como a encontrou. A revisão não é gate para Claude.
- Peça ajuda ao usuário só diante de uma ação exclusivamente humana, de uma permissão externa não concedida ou de uma decisão de produto fora da especificação; antes, procure uma alternativa segura, agrupe os pedidos e continue o trabalho independente.

## Trabalho a dois na mesma árvore

Claude e Codex usam o mesmo checkout, às vezes ao mesmo tempo; o quadro "Em andamento" do CONTINUIDADE.md diz quem mexe em quê.

1. Antes da primeira edição, leia "Em andamento" e o `git status`. Mudanças que você não fez são do outro agente ou do usuário: preserve-as.
2. Registre a sua tarefa em "Em andamento" (agente, tarefa, arquivos ou áreas, início) e edite fora das áreas do outro; se precisar de uma delas, combine pelo usuário.
3. Um Buzzy por vez: antes do `-Integracao` ou de uma verificação de tela, confirme que nenhum Buzzy está aberto; se houver, espere ou avise o usuário.
4. Ao terminar, tire a linha de "Em andamento" e registre o marco no log do CONTINUIDADE.md.
5. Os commits são do usuário.

**Regra do usuário — handoff compartilhado:** `CONTINUIDADE.md`, na raiz, é o único backup e registro de comunicação entre Claude e Codex. Atualize-o a cada marco (teste, módulo, decisão ou falha) e, em trabalho longo, ao menos a cada cerca de 30 minutos, com estado, evidência, limitações e próximo passo; decisões estáveis ficam nas fontes canônicas, com link.

## Execução

- Siga as fases e dependências de TODO.md e feche os critérios de uma fase antes de declarar o status dela. Um teste [HW] sem o equipamento fica pendente: continue o que não depende dele e volte na integração final.
- Todo teste, verificação ou ferramenta que abra o `Buzzy.exe` passa `--perfil-de-teste NOME`, nessa grafia, e nunca toca as configurações reais do usuário (DEC-029).
- Avise o usuário antes de qualquer verificação que abra janelas ou mova o cursor.
- Não leia conteúdo de outros aplicativos, não encerre processos do usuário, não altere configurações globais do Windows para simular testes e não publique nem distribua o aplicativo.
- O produto é local e não verbal (PRODUCT_SPEC.md): sem chat, texto, voz, IA integrada, rede ou capacidades vetadas por SECURITY.md.
- Identidade (DEC-018, DEC-019): pixel art fiel às pranchas, com o chapéu de palha e a personalidade do Luffy; a semelhança é intencional, sem regra de distância visual.

## Veracidade

Toda funcionalidade, teste ou decisão tem status: **VERIFIED** (implementado e com os testes executados), **PLANNED** (não verificado) ou **UNCERTAIN** (indefinido). Diferencie teste automatizado, input sintético, observação informal, verificação manual e [HW]; input por `SendInput` é sintético, nunca evidência humana. VERIFIED e fase concluída exigem evidência executada — build, testes, erros corrigidos, comportamento verificado e documentação sincronizada; compilar não basta.

## Documentação

Ao fechar um passo ou uma fase, atualize o que mudou — PROJECT_CONTEXT, DEVELOPMENT_LOG, ARCHITECTURE (desenho), DECISIONS (decisão), SECURITY (segurança) e TODO — e confira cada afirmação contra os arquivos e os resultados. Cada documento abre com o formato das suas entradas.

Escreva para dar contexto com poucos tokens: cada fato numa fonte só, com link nas outras; resultado e evidência (comando, números, arquivo em `resultados/`), não narrativa (sessões, horários, tentativas); só a contagem de testes mais recente e as que fecharam um gate; o histórico que não orienta mais vai para `docs/arquivo/`. Não renumere seções, DEC, itens, invariantes nem passos: o código os cita.

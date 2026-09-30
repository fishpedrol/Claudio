# DEVELOPMENT_LOG.md — Histórico essencial

> Registro resumido de marcos e correções. Critérios e números técnicos ficam em [TODO.md](TODO.md); decisões permanentes ficam em [DECISIONS.md](DECISIONS.md); evidências brutas ficam em `spikes/resultados/` e `resultados/`.
>
> Atualizado em 2026-09-30.

## Marcos

- **2026-09-25 a 2026-09-26 — Estrutura inicial:** definidos o mascote original, o escopo local sem IA integrada, limites de segurança, documentação canônica e roadmap. WPF com C#/.NET 10 foi escolhido em DEC-006; protótipos descartáveis P1, P2 e P3 foram criados em spikes/.
- **2026-09-26 — Protótipos:** P1, P2 e P3 foram medidos nos limites descritos em TODO.md. P2 registrou repouso sem atividade periódica evitável, desempenho de animação abaixo de 60 qps e crescimento de memória em dez minutos. Os detalhes e limites permanecem em TODO.md.
- **2026-09-27 — Aceites limitados:** o usuário aceitou P1 apenas no ambiente medido (Windows 11, 96 DPI), P2 como medição de viabilidade e as metas Q-08. Esses aceites não validam o aplicativo nem fecham a Etapa 0B.
- **2026-09-28 — Direção do produto:** clarificado mascote não verbal, curioso e brincalhão, com energia Baixa/Média/Alta e modo de tela cheia sujeito a P7. P4 foi aposentado; detalhes em DEC-013/014, PRODUCT_SPEC.md e TODO.md.
- **2026-09-29 — Correção e autorização:** os dez movimentos de mouse anteriormente chamados de “reais” foram feitos pela namorada do usuário enquanto explorava o protótipo. Foi uma observação informal, não um teste formal do usuário nem evidência humana aprovada de P3. Input por SendInput continua classificado como sintético. O usuário autorizou Claude a criar a identidade visual original e implementar/testar as fases 1–11 em sequência, sem aprovações rotineiras; a Fase 1 começa após os gates técnicos da Fase 0 (DEC-015).
- **2026-09-29 — P3 e fechamento técnico da Fase 0:** a SondaP3 usou um receptor controlado pelo spike e completou 3/3 rodadas, com 28/28 cenários OK em cada rodada, incluindo B4b. O input foi todo sintético; cursor, ClickLock e processos de teste foram restaurados/encerrados. M5 ficou entre 0,238 e 0,270 ms de média e 0,533 a 0,705 ms de p95. O resultado vale para Windows 11 build 26200, .NET 10.0.12 e dois monitores 1920×1080 a 96 DPI; escala mista não foi testada. Evidência: `spikes/resultados/p3-receptor.log` e `spikes/resultados/p3.log`. A Etapa 0B fechou tecnicamente, sem atribuir evidência humana.
- **2026-09-29 — Fase 1 e identidade visual:** o shell está implementado. `resultados/verificacao-fase1.log` registra 25 OK, 4 SIMULADO e 0 falhas com input sintético; o menu do personagem passou, enquanto as ações pela bandeja ficaram simuladas. Os critérios de escala 150/200% e de mudança real de resolução/escala/barra seguem pendentes. `resultados/desempenho-20260929-215550.txt` registra dez minutos de repouso: CPU de um núcleo 0,000%, memória privada 56,67 → 56,51 MB, nenhuma conexão de rede em 58 verificações e nenhum processo filho em 629 verificações. A identidade original e seus arquivos editáveis estão registrados em `docs/IDENTIDADE_VISUAL.md` e `assets/identidade/`.
- **2026-09-29 — Fase 2:** máquina de estados, fila, agenda, efeitos e gravação/reprodução implementados no Core. A revisão manual das referências 02–05 foi feita; a execução local passou 147/147 testes do Core. A integração à raiz de composição ainda estava pendente nesta atualização.
- **2026-09-29 — Identidade refeita em pixel art:** o usuário não gostou da direção vetorial (sagui violeta) e pediu algo mais fiel às pranchas, em pixel art. Nova direção (DEC-018): macaquinho azul-marinho com rosto creme, olhos castanhos, orelhas pêssego e cauda com ponta creme, em quadro de 64 × 64 pixels mostrado em 128 DIP; paleta lida da prancha; sem o chapéu de palha (PRODUCT_SPEC.md). O gerador `src/Buzzy.Visual/Pixel/` produziu 19 poses e 14 expressões, e `tools/Buzzy.Identidade` conferiu que nenhuma encosta na borda do quadro. A direção vetorial foi arquivada em `assets/identidade/arquivo-vetorial/`.
- **2026-09-29 — Chapéu de palha e personalidade do Luffy (DEC-019):** o usuário esclareceu que quer o Buzzy parecido com o Luffy — chapéu de palha com faixa vermelha e personalidade dele —, e que essa é a ideia central do projeto; as regras de distância visual da especificação foram retiradas. A pixel art ganhou o chapéu (reage às emoções) e o ícone da bandeja também; o sprite parado do app passou a ser o quadro da pixel art. `Buzzy.App.Testes`: 18/18 sem janela, incluindo os 8 testes do sprite; portão de APIs aprovado com `Buzzy.Visual` incluído.
- **2026-09-30 — Fase 2 fechada (VERIFIED):**
  - O núcleo foi ligado à raiz de composição e validado:
    - integração 22/22 com o log real `NUCLEO` (Booting→Settling→Idle, esconder por minimização, segunda instância, WM_CLOSE);
    - verificação de tela com o núcleo: 25 OK, 4 SIMULADO, 0 falhas;
    - repouso de 10 min com CPU média de 0,010% de um núcleo (`resultados/desempenho-20260930-112125.txt`).
  - Uma auditoria adversarial (quatro auditores e verificação cética) achou lacunas da tabela no modo de tela cheia, na carga, na energia inválida, no CLICK depois de mudança de topologia e no intervalo de acomodação. Foram corrigidas (DEC-020).
  - Um workflow de cobertura escreveu os testes pedidos pela auditoria: `TransicoesComplementaresTestes`, `TelaCheiaTestes`, o teste de propriedade ampliado e as reproduções com lista fixa. Ele conferiu os testes com 124 mutantes; só os 4 equivalentes sobreviveram. Achou um defeito real: desligar o modo de tela cheia com o retorno guardado tornava definitiva a posição temporária. Foi corrigido (linha nova da tabela; DEC-020).
- **2026-09-30 — Fase 3 implementada (PLANNED por pendências manuais):**
  - Árbitro de gestos puro e captura do mouse na janela do personagem (DEC-021).
  - Verificação de tela com input SINTÉTICO: 33 OK, 2 N/A, 0 falhas, com M5 p95 abaixo de 1 ms.
  - Pendentes: UAC, escalas mistas [HW] e ClickLock ligado (a configuração global não é alterada).
- **2026-09-30 — Commit durante a pausa:** às 13:38, durante uma pausa de Claude por limite de uso, o estado intermediário do trabalho entrou no commit `828c1f7`, feito com a identidade git do usuário. Claude não faz commit automaticamente.
- **2026-09-30 — Fase 4 implementada (PLANNED pelo critério 5):**
  - Física de passo fixo no núcleo (DEC-022), relógio pelos quadros do compositor e poses provisórias por estado, desenvolvidas numa cópia isolada e depois mescladas.
  - No meio da fase, o usuário pediu: "quero que o bixinho suba pelas laterais do monitor também, tenha tipo toon force". Isso virou DEC-023:
    - toda lateral é escalável, inclusive a encostada em outro monitor;
    - quique de borracha;
    - foguete de borracha;
    - achatar e esticar nas poses.
  - Contagens depois da mescla: Core 255/255, portão 73/73, App 34/34 com a integração.
  - Verificação de tela com input SINTÉTICO:
    - Fase 4: 22 OK, 0 falhas, inclusive o quique e a subida pela lateral interna, entre os dois monitores do usuário;
    - regressão da Fase 1: 25 OK, 4 SIMULADO;
    - regressão da Fase 3: 33 OK, 2 N/A.
  - Pendente: gravação de tela a 120 qps (critério 5).

## Estado desta atualização documental

- **Fase 0:** P3/Etapa 0B VERIFIED como gate técnico no ambiente medido, com evidência sintética e limite de DPI descrito acima.
- **Fase 1:** STATUS PLANNED até a verificação manual real da bandeja, das escalas 150/200% e de mudanças reais de resolução/escala/barra. Os testes sintéticos não substituem esses critérios.
- **Fase 2:** VERIFIED em 2026-09-30.
- **Fase 3:** PLANNED até UAC [MANUAL], escalas mistas [HW] e ClickLock [MANUAL].
- **Fase 4:** PLANNED até a gravação de tela a 120 qps [MANUAL].
- **Identidade visual:** está em pixel art (DEC-018/019). As poses provisórias por estado entraram na Fase 4; as animações completas continuam previstas para a Fase 6.
- O código em `spikes/` é descartável.

> **Arquivo morto — desenho anterior à implementação.** Cópia sem cortes de a crítica de integração da Fase 5, que resolveu os conflitos entre os quatro desenhos de área (seções 0 a 5; conflitos C…, lacunas L…) e deu a ordem dos passos P1–P16 (o código a cita como "crítica da Fase 5"), escrito em 2026-09-30 por um agente de desenho, só lendo o código, antes de implementar a Fase 5. As decisões canônicas estão em [DECISIONS.md](../../DECISIONS.md) (DEC-029 a DEC-031) e o estado em [TODO.md](../../TODO.md); onde o desenho e elas divergem, valem elas. Guardado porque o código cita trechos pelos números. Não é leitura obrigatória.

**Raiz:** `C:\Users\Cliente\Documents\claudio`. Os caminhos abaixo são relativos a ela; a cópia da Fase 4 está em `C:\Users\Cliente\AppData\Local\Temp\claude\C--Users-Cliente-Documents-claudio\3b0e750b-9776-418f-aa13-3a4fa1644cac\scratchpad\f4`. Só li arquivos; não compilei nem executei testes. O único cálculo foi conferir os hashes de R1 com `sha256sum`.

# Crítica de integração da Fase 5

## 0. Fatos do código que mudam a leitura dos desenhos

1. **DEC-023 já está ocupada** pela toon force (`docs/DECISIONS.md`, linha 468). Os quatro desenhos numeram as próprias decisões como DEC-023.
2. **O `src/` da árvore principal já é igual ao da f4.** O `cmp` dá idênticos `Maquina.cs`, `Movimento.cs`, `Configuracao.cs`, `EstadoDoNucleo.cs`, `Posicionador.cs`, `Aplicacao.cs` e `Win32.cs`, copiados às 15:53.
   - Os testes da árvore principal estão mais novos que os da f4: `Cenario.TresEmLinha` e o caso "carga com posição salva conferida" em `InvariantesTestes`.
   - A premissa "a árvore principal ainda tem o núcleo sem física" está vencida.
3. **Os desenhos de topologia e de travessia leram o `Movimento.cs` de antes da DEC-023.**
   - Não existem `Superficies.ParedeEsquerda/ParedeDireita` nem `NaParede`. O código atual tem `PassagemEsquerda/PassagemDireita`, que só informam, e `NaLateral` (`Movimento.cs`, linhas 89–119).
   - Com a toon force, `PassoAndando` trata toda lateral como parede: chama `Sinalizar(Parede)`, e `Escolher` sorteia entre Idle, Climbing e Walking com `Virar` (`Maquina.cs`, linhas 854–866). Não há meia-volta fixa.
   - `Sinalizar(Hanging, FimDaBorda)` desce por qualquer lateral (linhas 656–665).
   - O teste `MovimentoTestes.Passagem_NaoEhAtravessadaNaFase4MasSeEscaladaComToonForce` (linha 82) exige subir pela lateral encostada em outro monitor.
   - A DEC-023, item 1, diz: "Na Fase 5, uma passagem poderá ser atravessada ou escalada; a escolha fica com a agenda."
4. **Numeração livre na ARCHITECTURE:** os invariantes da 2.6 vão até o 17, e a seção 2.13.6 não existe (de 2.13.5 pula para 2.13.7).
5. **Hashes de R1 conferidos:** `mon:6852e0b1cd2318a2` e `mon:0994facb2eabe85c` estão corretos.

## 1. Conflitos e resolução

**C1 — Número da DEC.** Resolução: quatro decisões, numeradas na ordem de mesclagem.
- DEC-024: persistência.
- DEC-025: chave estável e topologia em execução.
- DEC-026: sessão, energia e minimização.
- DEC-027: travessia e escala.
- Acrescentar nota nas DEC-008 (o caminho do dispositivo vira resumo), DEC-010 (`.bak`) e DEC-016, item 10.

**C2 — Invariantes.** A persistência propõe o 18, a topologia o 18 e o 19, a sessão do 18 ao 22. Resolução:
- 18: todo `GravarPosicao` é gravável e só sai de evento do usuário ou do sistema, nunca de relógio ou autonomia;
- 19: um `TOPOLOGY_CHANGED` que não muda a geometria do monitor do personagem não troca o estado, salvo travessia em curso;
- 20: depois de `TOPOLOGY_CHANGED`, visível e fora de `PRESSED`/`DRAGGING`, `Lugar.Monitor` pertence à topologia;
- 21 (travessia): fora de `PRESSED`, `DRAGGING`, `REACTING` e de travessia em curso, o sprite fica inteiro na área útil do monitor da âncora;
- os cinco invariantes da sessão viram A1–A5 na 2.13.6 nova, porque são do árbitro, não da máquina.

**C3 — `PosicaoDoPersonagem.TelaDoMonitor`.** A persistência usa `RetanguloPx`, com vazio significando "desconhecido"; a topologia usa `RetanguloPx?`. Resolução:
- `RetanguloPx?`, `init` e fora do construtor posicional;
- semântica única: "tela do monitor da chave na última vez em que a posição foi descrita nele";
- por isso `Rebasear` (passo 2) **não** deve fazer `TelaDoMonitor?.Deslocado(dx,dy)`. Senão o disco guarda um retângulo que nunca existiu, que pode casar por engano no passo "mesmo retângulo" da partida seguinte.

**C4 — `Posicionador.Restaurar` desenhado duas vezes.** Há dois enums (`PassoDaRestauracao{PelaChave,PeloRetangulo,NoPrincipal}` e `OrigemDaRestauracao{MesmaChave,MesmoRetangulo,Principal}`), o passo 3 difere (`salva with{…}` contra `Descrever(r)`) e o texto da regra também. Resolução:
- uma função e um enum;
- manter as frações salvas, normalizadas, nos três passos: é idempotente e coerente com o passo 1;
- um texto de regra só, e os testes das duas áreas afirmam o mesmo texto;
- sem posição salva, o texto atual fica igual (referências 01–05).

**C5 — `Gravacao.cs`.** A persistência diz "sem mudança"; a topologia muda `LerPosicao` para 5 ou 9 campos e cria `DescreverPosicaoCompleta` só para `Escrever(Loaded)`; a travessia escreve `travessia=nao` só quando falso. Resolução: aceitar as duas extensões.
- Nenhuma referência tem `Loaded … posicao=` nem travessia desligada.
- O `GravarPosicao` continua com 5 campos.

**C6 — `Preferencias.AtravessarMonitores`.** Nome e padrão `true` coincidem. A persistência e a travessia usam parâmetro posicional com padrão; a topologia, propriedade `init`. Não há desconstrução de `Preferencias` no código. Resolução:
- posicional com padrão, e `Padrao = new(Media, true, true)`;
- o gerador de `InvariantesTestes` (linhas 751, 770 e 813) passa a sortear esse campo.

**C7 — Gravação urgente.** A persistência grava na hora em `Suspending`, `SessionEnding` e `CmdExit`; a sessão inclui também `SessionLocked`. Resolução:
- incluir `SessionLocked` em `PoliticaDeGravacao.Imediata`, porque `SUSPENDING` com o personagem já em `HIDDEN(POR_SESSAO)` não emite `GravarPosicao` (`Esconder`, linhas 306–321);
- manter o `Descarregar("SUSPENDING")` incondicional da persistência;
- ajustar `S12_BloquearESuspender_…` e `Politica_Imediata_…`.

**C8 — Minimização pelo Windows desenhada duas vezes.**
- Topologia (D13/R16): janela de 3 s depois de `WM_DISPLAYCHANGE`, espera de 750 ms e `RestaurarEstadoNormal()` antes de `CmdHide`.
- Sessão (D9, R7–R11): `ArbitroDoSistema` com J de 2 s, leitura divergente da topologia e trava de três restaurações em 10 s.
- Resolução: ficar com o árbitro da sessão, que é puro e testável sem janela. Descartar `_confirmarMinimizacao` e `RestaurarEstadoNormal`, que mostraria a janela por um quadro antes de escondê-la.

**C9 — `JanelaPersonagem`.** As duas áreas propõem APIs diferentes. Resolução:
- usar a da sessão: `Minimizada(long)`, `RestauradaPorFora`, `EstaMinimizada`, `RestaurarSemAtivar(): bool` com `_restaurandoPorNos`, e `EsconderNoWin32`;
- as duas tiram o `WindowState = Normal` de `AoMudarEstado` (linhas 252–257).

**C10 — `Win32.cs`.** A topologia e a sessão declaram as duas `ShowWindow` e `SW_SHOWNOACTIVATE`, e a mesclagem não compila com membro duplicado. `IsIconic` também aparece duas vezes em `NativoTeste`. Resolução: um só bloco "Janela própria" com `ShowWindow`, `IsIconic`, `SW_HIDE` e `SW_SHOWNOACTIVATE`.

**C11 — Agrupador de mensagens.** A topologia põe um teto de 1 s desde a primeira mensagem (R17); a sessão exige "não antes de" (`naoAntesDe`, E = 1,5 s depois da retomada). Resolução:
- um só `AgendarReleitura(motivo, naoAntesDe = 0)`, que dispara em `max(naoAntesDe, min(ultima + 300 ms, inicioDaRajada + 1 s))`;
- `AoPossivelMudancaDeTopologia` faz, nesta ordem: registra `MENSAGEM`, avisa o árbitro (`SinalDeTopologia`) e chama `AgendarReleitura`.

**C12 — Duplicatas iguais.** Implementar cada uma uma vez só.
- `AoRecriarBarra` → `AoPossivelMudancaDeTopologia("TaskbarCreated")`: topologia e sessão.
- `EventoDoLog.Instante`: persistência e sessão.
- Auxiliares de topologia de teste: `Rebaseada` e `SemOPrincipal` (topologia) contra `SemMonitorComoOWindows` (sessão). O `GeradorDeTopologias.Rebasear`, privado (linha 226), já faz esse rebase; basta um auxiliar que o reaproveite.
- Topologias novas ficam fora de `Todas`, porque entrar ali muda as contagens das propriedades e as evidências do TODO.

**C13 — Topologia contra a DEC-023.** Contradizem a toon force: os passos 2–3 de R12 ("a lateral virou passagem → `SETTLING`"; "a caminhada perde a parede-alvo"), a linha nova da 2.6 "se o apoio deixou de existir…" e os testes `Escalando_ParedeViraPassagem_Cai` e `AndandoParaEscalar_ParedeAlvoViraPassagem_…`. A lateral da área útil continua existindo. Resolução:
- nas classes A e T, o estado continua sempre, salvo travessia em curso (C15);
- em `ContinuarNoMonitor`, calcular `Posicao` por `Descrever(Lugar novo)`, para não divergir 1 px por arredondamento;
- R15d precisa prever os quiques: a queda de 904 px bate a 600 DIP/s ou mais.

**C14 — Travessia contra a DEC-023.** A D10 ("não escala uma passagem") e a D11 ("desligada, a passagem vira beirada com meia-volta") revertem um pedido explícito do usuário e quebram o teste da linha 82 de `MovimentoTestes`. Resolução:
- portas por área útil (D1, que é boa) em `Passagens.Portas`, sem renomear campos de `Superficies`;
- numa porta compatível, com `_cfg.Travessia` ligada, `AtravessarMonitores` e sem calma, a agenda sorteia (com a semente) entre atravessar e o caminho atual da parede. O peso novo `PerfilDeEnergia.PesoAtravessar` muda frequência, não física (invariante 12);
- travessia desligada, calma ou porta incompatível: exatamente o código da f4;
- transbordo como opção sorteada quando a escalada cruza a altura do chão do vizinho entre dois passos, inclusive no foguete; não como parada obrigatória no "topo do trecho";
- pendurado: "atravessa" vira mais uma opção de `Escolher("HANGING: …")` quando os tetos coincidem.

**C15 — Travessia contra R11.** Na classe A/T o estado "continua", mas o plano `EstadoDoMovimento.Travessia` guarda chaves e coordenadas absolutas que `ContinuarNoMonitor` não translada. O monitor que "só mudou" pode ser justamente o destino. Resolução:
- com `Travessia` não nula, todo `TOPOLOGY_CHANGED` de configuração diferente vai para `SETTLING`;
- `IrPara` limpa `Travessia` ao sair dos estados de movimento.

**C16 — Histerese de escala no arraste (travessia D9b) contra o DPI da janela.** O Windows troca o DPI da janela pela maior área, que coincide com a coluna da âncora.
- `SpriteProvisorio` cria `WriteableBitmap(largura, altura, dpi, dpi, …)` (linha 144), e a imagem usa `Stretch.None`.
- Na faixa de histerese, um bitmap de 192 px a 144 DPI numa janela já a 96 DPI é desenhado com 128 px no canto superior esquerdo: o sprite fica deslocado e com os pés 64 px acima do chão.
- Ainda pior: `WM_GETDPISCALEDSIZE` encolhe a janela, e o núcleo a reaumenta no passo seguinte.
- Resolução: não implementar D9(b) antes da correção de P6 no app (L1).

**C17 — Isolamento dos testes.** Três propostas: `--perfil-de-teste` (persistência), `--dados`/`--ignorar-posicao-salva` (topologia) e "pasta de teste" (sessão). Resolução: a da persistência, que fica sob `%LOCALAPPDATA%\Buzzy\testes` e falha fechada. Entra antes de o app ler ou gravar qualquer arquivo.

**C18 — `Aplicacao.cs`, tocado pelas quatro áreas.** Ordem combinada:
- **`Iniciar`:**
  1. ler as configurações;
  2. ler a topologia com `LerDetalhado`;
  3. criar a janela de serviço e fazer os registros de sessão e energia;
  4. criar o personagem com `Minimizada`;
  5. criar o `Nucleo`;
  6. enviar `Loaded(topologia, posicaoSalva, preferencias)`.
- **`EncerrarAplicacao`:**
  1. `_encerrando = true`;
  2. `Descarregar`, depois `Parar`;
  3. parar os temporizadores novos;
  4. o que já existe: menu, captura, relógio e agenda;
  5. bandeja;
  6. `_servico.Dispose()`, adiado se estiver dentro do gancho;
  7. `Close` e `Shutdown`.
- `_leitura` é atualizada em todo caminho de leitura da topologia.

## 2. Lacunas e erros factuais

**Lacunas:**
- **L1 — P6 no app não tem dono.** Proposta mínima:
  - dimensionar a imagem em DIP como pixels do bitmap × 96 / DPI atual da janela, para que o bitmap saia sempre 1:1;
  - em `DpiChanged`, recalcular isso e reaplicar na hora o `_posicionamento`;
  - manter `WM_GETDPISCALEDSIZE` como está e não mexer no `WM_DPICHANGED`. A ideia (a) da travessia contradiz a ARCHITECTURE 2.13.3.
- **L2 — Configuração do app duplicada.** `MovimentoIntegracaoTestes.ConfiguracaoDoApp` (linha 19) e `VerificacaoFase4.ConfiguracaoDoApp` (linha 20) simulam sementes com uma cópia da configuração. Com `Travessia = true`, as simulações divergem do app na topologia do usuário. Resolução: uma fábrica única, ou um teste que compare as três configurações.
- **L3 — `LeitorDeTopologia.Ler` devolve topologia parcial.** Uma falha de `GetMonitorInfo` pula o monitor e ainda devolve a topologia (linhas 24–28 e 53–56); uma falha de `GetDpiForMonitor` vira 96 sem aviso. Com a regra "monitor sumido não volta", o personagem migra de monitor para sempre. As duas falhas devem contar como leitura incoerente.
- **L4 — Falta trava em `Decidir`.** `DecideNoEstado(Hanging)` mantém o `AUTONOMY_TIMER` armado, e `Decidir` pode interromper a travessia pendurado.
- **L5 — Rearme de 2 s durante gesto.** Com ClickLock, a persistência rearma a gravação a cada 2 s enquanto o botão estiver pressionado. Trocar por gravar o pendente ao sair de `PRESSED`/`DRAGGING`.
- **L6 — Tela apagada sem suspender.** Monitores DisplayPort podem sumir da topologia e virar classe C (migração permanente). Ninguém cobre. Incluir em P5; se confirmado, adiar a revalidação enquanto a tela estiver apagada.
- **L7 — Repouso não remedido.** Falta repetir a medição de 10 min (DEC-011, Q-08) e um teste de que nenhuma agenda nova fica armada em repouso.
- **L8 — `CmdResetPosition` fora do menu.** O núcleo já o trata (`RedefinirPosicao`); falta o item em `MenuNativo`/`Textos.resx`. Com posição persistida, é a saída do usuário para uma restauração ruim.
- **L9 — Sobreposição de monitores.** `NaUniaoDasAreasUteis` supõe áreas sem sobreposição, mas o construtor de `Topologia` não valida isso.
- **L10 — Critério de outra fase.** A cascata principal → `.bak` → padrões muda o critério 3 da Fase 8 e a ARCHITECTURE 2.12. É defensável, mas precisa ser registrada de forma explícita.
- **L11 — `JsonSerializerIsReflectionEnabledByDefault=false` é opcional.** Ganha pouco e quebra `Buzzy.Visual/Boneco.cs` se ele voltar a rodar.
- **Dúvida de produto a registrar (sem bloquear):** que posição persistir (persistência D4) e se monitores empilhados (S3) ficam sem travessia autônoma (travessia D4).

**Erros factuais:**
- **Travessia:** "meia-volta como hoje" e as APIs `ParedeX`/`NaParede` não existem no código atual. O teste existente se chama `Passagem_NaoEhAtravessadaNaFase4MasSeEscaladaComToonForce`.
- **Topologia, referências de linha:** em `PropriedadesTestes`, a afirmação é a linha 107 e o ramo do monitor sumido as linhas 112–116. Em `TelaCheiaTestes`, o teste citado está na linha 195. Em `VerificacaoFase3`, a comparação é a linha 265. `Cenario.TresEmLinha` já existe.
- **Sessão:** segundo a documentação da Microsoft, `PBT_APMRESUMECRITICAL` deixou de existir no Vista. O caso "suspensão sem aviso" cai em "retomada sem suspensão".
- **Enunciado:** o núcleo da árvore principal já tem física (fato 2).
- **Hardware disponível:**
  - S2 e S7 são a própria máquina do usuário;
  - S5 é parcialmente possível: o usuário pode pôr um monitor em 125/150/175 %;
  - S1, S3, S4 e S6 só exigem rearranjar ou girar monitores, o que o usuário pode fazer.

## 3. Riscos às regras duras

- **O usuário prevalece:**
  - C14 é o maior risco, porque reverteria o que o usuário pediu na DEC-023;
  - a heurística de minimização pode desfazer uma minimização do usuário dentro de J. É raro (não há botão de minimizar); registrar como leitura de Q-03 e nunca mostrar com `_visivel` falso;
  - pausa e painel esperam a travessia terminar (até cerca de 1,5 s); PRESS e `CmdHide` continuam imediatos.
- **Sem timer periódico em repouso:** só L5 repete. Os demais disparam uma vez depois de um evento (2/10/60 s, J/E/T, 300 ms com teto de 1 s, 1,5 s, 2/5/15/30 s) e todos param em `EncerrarAplicacao`.
- **Determinismo:**
  - o árbitro recebe o tempo pelo sinal;
  - a escolha entre atravessar e subir usa o `Aleatorio` do estado;
  - o desempate por chave na D12 é impossível (duas portas do mesmo lado com o mesmo chão se sobreporiam), então a chave segue opaca e `VerificacaoFase4.TopologiaLida` continua válida;
  - o risco real de divergência é L2.
- **Portão e dados:**
  - nenhuma API nova está em `tools/Buzzy.PortaoApis/Regras.cs`; `CreateDC` e `GetForegroundWindow` continuam proibidos no produto;
  - `ShowWindow` precisa ser restrito, no código e por um teste de fonte, à janela do Buzzy e a `SW_SHOWNOACTIVATE`/`SW_HIDE`;
  - acrescentar ao portão as regras de sessão (`WTSEnumerateSessions*`, `WTSQuerySessionInformation`);
  - a chave é um resumo truncado do caminho, sem nome amigável; registrar em SECURITY 5 e 6, junto com `.tmp`, `settings.corrupt.json` e a pasta `testes\`.
- **Não ler outros aplicativos nem mudar configuração global:** nenhum desenho viola. As mudanças reais de vídeo, sessão e energia ficam com o usuário, e os testes só postam mensagens às janelas do próprio Buzzy.

## 4. Ordem de implementação

| Passo | Arquivos | Testes que devem passar ao fim |
|---|---|---|
| **P0.** Base: Fase 4 mesclada | Esperar a compilação em curso; conciliar os testes f4 × árvore principal | `tools/testar.ps1` e `-Integracao` (avisar o usuário) verdes; referências 01–05 idênticas |
| **P1.** `TelaDoMonitor` e pixel dos pés | `Posicionador.cs`: `TelaDoMonitor`, `Descrever`, `Reacomodar`, `PixelDosPes`. `Maquina.cs`: `Validar`, `MonitorDaAncora` | `PropriedadesTestes` (linhas 107 e 112–116 ajustadas), Core inteiro, referências |
| **P2.** Restauração com alternativas | `Restaurar` (C4), `Maquina.Carregar`, `Gravacao.LerPosicao` e `Escrever(Loaded)` | `RestaurarTestes` (S1–S7, S9, NaN); `ConferirCarga` com a cascata |
| **P3.** Esquema de configurações | `src/Buzzy.Core/Persistencia/*` com C7; `Preferencias.AtravessarMonitores` (C6); `Gravacao` | `EsquemaDeConfiguracoesTestes`, propriedades, amostra v1, invariante 18 |
| **P4.** Arquivo de configurações, ainda desligado | `PastaDeDados.cs`, `ArquivoDeConfiguracoes.cs`; `Diagnostico.PastaDeDados` delega (também corrige o caminho relativo "Buzzy" quando a pasta vem vazia) | `ArquivoDeConfiguracoesTestes` com `GravadorSemParar` e `MatarDuranteGravacoes`; portão |
| **P5.** Isolamento dos testes | `Programa.LerOpcoes`, `OpcoesDaAplicacao`, `BuzzyEmTeste` (perfil, limpar, `Instante`), lançadores do `Buzzy.Verificacao`, `medir-desempenho.ps1` | Integração inteira, sem mudança de comportamento |
| **P6.** Chave estável, antes de gravar no disco real | `Win32` (vídeo), `ConfiguracaoDeVideo`, `ChavesDeMonitor`, `LerDetalhado` com L3; logs `TOPOLOGIA` e `POSICAO gdi`; `VerificacaoFase3` linha 265 | `ChavesDeMonitorTestes`; tamanhos 8/20/48/72/64/20/84/420; leitura real; `Fumaca` |
| **P7.** Persistência ligada | `AgendaDeGravacao` com L5; `Iniciar`, `ExecutarEfeito`, `EncerrarAplicacao`, `DispatcherUnhandledException` | `AgendaDeGravacaoTestes`; `PersistenciaIntegracaoTestes` (S2/S7, ilegível, arquivo real intocado). Docs: DEC-024, ARCHITECTURE 2.12, SECURITY 5 |
| **P8.** Topologia em execução (núcleo) | `SoTranslacao`, `MonitorCorrespondente`, `Rebasear` (sem transladar o retângulo); `MudarTopologia` sem os passos 2–3 de R12; `ContinuarNoMonitor`; `Clicar` | `RebasearTestes`, `MudancaDeTopologiaTestes`, `TelaCheiaTestes` linha 195 (`Idle → Idle`), invariantes 19 e 20, referências 01–05 (a 02 está escondida; a 05 é classe B) |
| **P9.** Releitura robusta no app | `AoRecriarBarra`, `AgendarReleitura` (C11), reafirmação tardia, log `MENSAGEM` | `Bandeja_…` com `TaskbarCreated`, agrupamento, `ReafirmacaoTardia_…` |
| **P10.** `ArbitroDoSistema` (núcleo) | `src/Buzzy.Core/Sistema/ArbitroDoSistema.cs` | Testes unitários, simulador, propriedades A1–A5, cenários nas topologias |
| **P11.** Sessão, energia e fim de sessão | `Win32` (bloco de C10), `JanelaDeServico`, `Aplicacao` (`ReceberDoSistema`, `RelerTopologiaAgora`, `Descarregar` na suspensão), `Regras.cs` do portão | `PlataformaTestes`, `SistemaTestes`, `TestesDaListaProibida` |
| **P12.** Minimização pelo sistema | `JanelaPersonagem` (C9); `AoMinimizar`; guardas em `AplicarNaJanela` e `ReafirmarLugarDaJanela` | Testes de minimização; `InstanciaUnica_…` com espera de 5 s e `IsIconic`. Docs: DEC-026, ARCHITECTURE 2.13.6 |
| **P13a.** Travessia plana, reconciliada (C14, C15, L4) | `Passagens.cs`, `Movimento.cs`, `Configuracao.cs`, `Tipos.cs` (valores novos no fim do enum), `Maquina.cs`, `Travessia = true` no app, L2 | `PassagensTestes`; `TravessiaTestes` (S1/S2/S7, vão e quina, desligada igual à f4); `ConferirApoio` pela união; referência 06 |
| **P13b.** Salto de degrau | `Passagens`, `Maquina.PassoNoAr` | S4 e S6 descendo; `EscalasMistas` (±24 px) |
| **P13c.** Transbordo | `Maquina.PassoEscalando` | S4 e S6 subindo; `TresMonitores` |
| **P14.** P6 no app | `JanelaPersonagem` (L1); D9(b) só se P6 mostrar oscilação | Testes de pose e sprite; o resto é [HW] |
| **P15.** Verificação de tela | `Buzzy.Verificacao --fase 5`, compartilhada; `TravessiaIntegracaoTestes` | Input sintético, com aviso ao usuário |
| **P16.** Gate | ARCHITECTURE 1, 2.4, 2.5, 2.6, 2.8, 2.12, 2.13.3, 2.13.6; DEC-024 a DEC-027; SECURITY; TODO (mapa S1–S12); PROJECT_CONTEXT; DEVELOPMENT_LOG; COMO_INICIAR; CONTINUIDADE | Repouso de 10 min com perfil de teste |

## 5. Pendências [MANUAL]/[HW] para o TODO

1. **P5 — mensagens e tempos.** Conectar, desconectar, rearranjar, trocar o principal, girar e mudar a escala, com "Minimizar janelas quando um monitor for desconectado" e "Lembrar locais das janelas" ligados e desligados.
   - Calibrar: agrupamento de 300 ms, teto de 1 s, J = 2 s, E = 1,5 s, T = 8 s e reafirmação de 1,5 s.
   - Confirmar se o Windows minimiza uma janela de ferramenta sempre no topo.
   - Verificar a tela apagada por inatividade (L6).
   - Quem faz: o usuário, com `--diagnostico`; possível com o equipamento atual.
2. **P5 — estabilidade da chave `mon:`.** Reiniciar, trocar porta ou cabo, atualizar driver; modo clone (Win+P "Duplicar"); consulta com a sessão bloqueada. Feito pelo usuário. RDP depende de outra máquina.
3. **P6 e S5 — escala mista, andando e arrastando.** Parcial: o usuário põe um monitor em 125/150/175 %. Os 200 % e um monitor 4K são [HW].
4. **S1, S3, S4 e S6.** Rearranjar os monitores ou girar um deles (configuração global). Conferir a restauração "pela chave" e a travessia nos dois sentidos. Feito pelo usuário.
5. **S2 e S7 (a máquina atual).** Travessia plana cruzando x = 0 e persistência "soltar, fechar e reabrir". O agente pode fazer com input sintético, avisando antes; não conta como evidência humana.
6. **S8.** Desconectar o monitor do Buzzy, com e sem a opção de minimizar, e reconectar: não some, não fica minimizado, não rouba foco e não volta sozinho. Ação física do usuário.
7. **S9.** Iniciar com o monitor salvo ausente (Win+P "Somente tela do PC") e depois "Estender". Feito pelo usuário.
8. **S10.** Resolução, escala e orientação com o app aberto. Isso fecha também o critério 9 da Fase 1. Feito pelo usuário.
9. **S11.** Ligar a ocultação automática da barra e reiniciar o Explorer. Outras bordas não existem no Windows 11: N/A, cobertas só por [AUTO].
10. **S12.** Todos feitos pelo usuário:
    - bloquear com Win+L;
    - suspender com e sem senha ao despertar;
    - hibernar;
    - esconder pela bandeja antes de bloquear ou suspender;
    - suspender com o personagem andando ou caindo;
    - trocar de usuário;
    - sair da conta ou reiniciar, conferindo a gravação;
    - desligamento cancelado por outro aplicativo (limitação conhecida, DEC-016);
    - `powercfg /a` para ver se há Modern Standby.
11. **Travessia [MANUAL][HW] em S1/S3/S4/S6** e critério 5 da Fase 4 (gravação a 120 qps) com travessia.
12. **Process Monitor (SECURITY 8.4):** gravação só em `%LOCALAPPDATA%\Buzzy` e `testes\`.
13. **Repouso de 10 min depois da Fase 5.** Instrumentado; o agente pode fazer, avisando antes.
14. **Queda de energia no meio da gravação.** Não dá para testar; fica como limitação documentada.
15. **Evidência humana.** Nenhum input sintético conta como evidência humana; nada vira VERIFIED sem a execução real.
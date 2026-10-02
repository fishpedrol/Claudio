# README — Buzzy

Mascote original e não verbal para desktop Windows. A especificação e as fases descrevem o produto desejado; o estado atual e o que já foi verificado estão em PROJECT_CONTEXT.md.

**Para compilar e abrir o mascote:** [COMO_INICIAR.md](COMO_INICIAR.md).

**Status (2026-10-02):** o aplicativo existe em `src/`, com testes em `tests/`. O mascote aparece, pode ser clicado e arrastado e circula sozinho num monitor; pelo menu, ganha uma emoção dominante e itens para usar; e, ao abrir de novo, volta onde estava. Situação das fases:

- Fase 2: VERIFIED.
- Fases 1, 3 e 4: implementadas, mas ainda PLANNED, por verificações manuais e de hardware pendentes.
- Fase 5: em andamento. O app lembra entre execuções a posição, o esconderijo, o "preso" e a emoção dominante, usa uma chave estável para cada monitor e acompanha a troca de monitores com ele aberto. Isso foi verificado por testes automatizados e de integração, mas ainda não na tela nem pelo usuário; a travessia entre monitores e outros passos continuam pela frente.
- Pedidos novos, a emoção dominante e o tamagotchi adulto (DEC-027 e DEC-028): já aparecem no app, pelo menu (veja a seção 5 de [COMO_INICIAR.md](COMO_INICIAR.md)), e foram verificados por testes automatizados, de integração e, em 2026-10-01, na tela, com input sintético, e num repouso de 10 minutos com uma onda ativa. Nada disso é teste humano: faltam a revisão visual e de tom pelo usuário e outras conferências manuais e de hardware. Desde o passo P7 da Fase 5, a emoção escolhida é lembrada ao reabrir o app.
- Pedidos de 2026-10-01, de desenho animado: a banana, a água, o café e o energético acalmam o efeito aos poucos; e misturar uma droga sintética do jogo (bala, MD, cocaína ou lança-perfume) com outra substância tem chance de 1 em 8, sorteada uma vez por mistura, de deixar o Buzzy paranoico, olhando pro teto. Álcool e maconha não deixam. E, de vez em quando, parado no chão, ele fuma um baseado sozinho, sem item na tela; um clique o interrompe. Verificado por testes automatizados e de integração; a verificação na tela e a revisão do usuário, inclusive de quantas vezes ele fuma sozinho, ainda faltam. A cópia do Buzzy na Área de Trabalho é anterior a isso e ainda tem a regra antiga (seção 5 de [COMO_INICIAR.md](COMO_INICIAR.md)).

O Buzzy guarda as configurações em `%LOCALAPPDATA%\Buzzy\settings.json`, com a versão anterior em `settings.json.bak`. Para voltar à posição inicial, feche o Buzzy e só então apague os dois arquivos (seção 6 de [COMO_INICIAR.md](COMO_INICIAR.md)).

Testes e ferramentas abrem o Buzzy com `--perfil-de-teste NOME`, nessa grafia exata, para nunca tocar nas configurações reais; veja a seção 7 de [COMO_INICIAR.md](COMO_INICIAR.md).

O estado exato e as evidências estão em [PROJECT_CONTEXT.md](docs/PROJECT_CONTEXT.md). O usuário autorizou Claude a avançar as fases sem aprovações rotineiras (DEC-015).

## Fontes

- [Instruções para agentes](AGENTS.md)
- [Estado atual](docs/PROJECT_CONTEXT.md)
- [Produto](docs/PRODUCT_SPEC.md)
- [Roadmap e critérios](docs/TODO.md)
- [Arquitetura](docs/ARCHITECTURE.md)
- [Decisões](docs/DECISIONS.md)
- [Segurança](docs/SECURITY.md)
- [Evidências e histórico](docs/DEVELOPMENT_LOG.md)
- [Diretiva atual para Claude](prompt_usuario.md)
- [Protótipos e instruções de execução](spikes/README.md)

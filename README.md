# README — Buzzy

Mascote de companhia, original e não verbal, para desktop Windows 11: um macaquinho em pixel art com o chapéu de palha e o jeito do Luffy, que circula pelas bordas da tela, reage a cliques e, pelo menu, ganha uma emoção dominante e itens de um "tamagotchi adulto" de desenho animado. Local, sem rede e sem IA; uso pessoal.

**Para compilar e abrir o mascote:** [COMO_INICIAR.md](COMO_INICIAR.md).

**Status (2026-10-02):** o aplicativo existe em `src/`, com testes em `tests/`. A Fase 2 está VERIFIED; as Fases 1, 3 e 4 estão implementadas, com verificações manuais e de hardware pendentes; a Fase 5 (multi-monitor, travessia entre monitores e posição lembrada entre execuções) está em andamento; e a emoção dominante e o tamagotchi já aparecem no app, verificados por testes e na tela com input sintético, faltando a revisão visual e de tom pelo usuário. O estado exato e as evidências estão em [PROJECT_CONTEXT.md](docs/PROJECT_CONTEXT.md).

O Buzzy guarda as configurações em `%LOCALAPPDATA%\Buzzy\settings.json`, com a versão anterior em `settings.json.bak` (seção 6 de [COMO_INICIAR.md](COMO_INICIAR.md) explica como voltar à posição inicial). Testes e ferramentas abrem o Buzzy com `--perfil-de-teste NOME` para nunca tocar nas configurações reais (seção 7).

## Fontes

- [Instruções para agentes](AGENTS.md) e [diretiva atual](prompt_usuario.md)
- [Estado atual](docs/PROJECT_CONTEXT.md) e [continuidade entre Claude e Codex](CONTINUIDADE.md)
- [Produto](docs/PRODUCT_SPEC.md)
- [Roadmap e critérios](docs/TODO.md)
- [Arquitetura](docs/ARCHITECTURE.md)
- [Decisões](docs/DECISIONS.md)
- [Segurança](docs/SECURITY.md)
- [Identidade visual](docs/IDENTIDADE_VISUAL.md)
- [Histórico essencial](docs/DEVELOPMENT_LOG.md); o histórico detalhado fica em [docs/arquivo/](docs/arquivo/)
- [Protótipos e instruções de execução](spikes/README.md)

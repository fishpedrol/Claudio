# Como iniciar o Buzzy

> **Estado atual:** versão inicial. O Buzzy aparece como o macaquinho em pixel art com o chapéu de
> palha (`docs/IDENTIDADE_VISUAL.md`), parado no canto da tela. Ele ainda não anda nem pode ser
> arrastado; isso chega nas Fases 3 e 4, e as animações na Fase 6.

## 1. Antes da primeira vez

- **Windows 11.**
- **SDK do .NET 10.** O projeto fixa a versão 10.0.401 (arquivo `global.json`). Para conferir, abra o
  PowerShell e rode:

  ```powershell
  dotnet --list-sdks
  ```

  A lista precisa ter uma linha começando com `10.0.`. Se não tiver, instale o SDK do .NET 10 pelo site
  oficial da Microsoft (dotnet.microsoft.com).

## 2. Compilar

No PowerShell, dentro da pasta do projeto:

```powershell
cd C:\Users\Cliente\Documents\claudio
dotnet build src\Buzzy.App\Buzzy.App.csproj -c Release
```

Deu certo quando o fim da saída mostra `0 Erro(s)` e `Resumo: APROVADO`. Esse resumo vem do portão
de segurança, que roda sozinho depois de cada build.

Só é preciso compilar de novo quando o código mudar. Feche o Buzzy antes de recompilar; com ele aberto,
o build falha porque o arquivo está em uso.

## 3. Abrir o mascote

```powershell
.\src\Buzzy.App\bin\Release\net10.0-windows\Buzzy.exe
```

Outro jeito é dar dois cliques em `Buzzy.exe` nessa pasta pelo Explorador de Arquivos. Para ter um
atalho na área de trabalho, clique com o botão direito em `Buzzy.exe` → **Mostrar mais opções** →
**Enviar para** → **Área de trabalho (criar atalho)**.

Não use "Executar como administrador": o Buzzy recusa rodar assim e mostra um aviso.

## 4. O que você vai ver

- O personagem aparece no canto inferior direito do monitor principal, com os pés logo acima da barra
  de tarefas.
- Ele fica sempre por cima das outras janelas e não aparece na barra de tarefas nem no Alt+Tab.
- Clicar nele não tira o foco do programa que você está usando. Clicar nas partes transparentes em
  volta dele atinge o que está embaixo.
- Um ícone do Buzzy vai para a bandeja, perto do relógio. No Windows 11, ícones novos ficam escondidos
  na setinha **^**. Para deixá-lo sempre visível: **Configurações** → **Personalização** → **Barra de
  tarefas** → **Outros ícones da bandeja do sistema** → ative **Buzzy**.

## 5. Como usar

| Quero... | Como fazer |
|---|---|
| Abrir o menu | Botão direito no personagem, ou botão direito no ícone da bandeja |
| Esconder o Buzzy | Menu → **Esconder Buzzy** |
| Mostrar de novo | Clique no ícone da bandeja, ou menu da bandeja → **Mostrar Buzzy**, ou abra o `Buzzy.exe` outra vez |
| Fechar | Menu → **Sair** |

Abrir o `Buzzy.exe` com ele já aberto não cria um segundo Buzzy: só traz de volta o que já está
rodando.

O Buzzy não usa a internet e não lê outros programas. Sem a opção `--diagnostico` (abaixo), ele não
grava nenhum arquivo.

## 6. Se algo der errado

| Situação | O que fazer |
|---|---|
| Aviso "O Buzzy não roda como administrador" | Abra de novo normalmente, sem "Executar como administrador" |
| O Windows pede para baixar o .NET | Falta o .NET 10: instale o SDK (item 1) |
| O personagem sumiu | Clique no ícone da bandeja ou abra o `Buzzy.exe` de novo |
| Nada aparece, ou para relatar um problema | Abra com `Buzzy.exe --diagnostico`. Ele registra só eventos do próprio Buzzy em `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB) |
| O build falha com o arquivo em uso | Feche o Buzzy (menu → **Sair**) e compile de novo |

## 7. Para quem desenvolve

- Testes sem abrir janelas: `powershell -NoProfile -File tools\testar.ps1`.
- Testes que abrem janelas e movem o cursor, e a medição de desempenho: os comandos estão em
  `BACKUP_CLAUDE.md` (seção 3) até entrarem em `docs/PROJECT_CONTEXT.md`. Avise quem estiver usando o
  computador antes de rodá-los.

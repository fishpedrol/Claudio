# Como iniciar o Buzzy

> **Estado atual:** o Buzzy aparece como o macaquinho em pixel art com o chapéu de palha
> (`docs/IDENTIDADE_VISUAL.md`). Pode ser arrastado com o mouse e, sozinho, anda pelo chão, escala as
> laterais da tela (inclusive a que encosta no outro monitor), pendura-se num cipó na borda de cima,
> pula, quica como borracha e descansa. Por enquanto fica no monitor em que está: atravessar para
> outro monitor e lembrar a posição chegam na Fase 5. As poses ainda são quadros fixos por estado; as
> animações completas chegam na Fase 6.

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
| Mudar o Buzzy de lugar | Clique nele e arraste; solto no meio do ar, ele cai até o chão (e quica, se cair de alto) |
| Pendurar no cipó | Arraste até perto da borda de cima e solte: ele agarra um cipó e fica lá até você tirá-lo |
| Grudar numa parede | Arraste até perto de uma lateral da tela e solte: ele gruda na parede e fica lá até você tirá-lo |
| Esconder o Buzzy na borda | Dois cliques nele: ele se esconde atrás da barra de tarefas (ou da lateral, se estiver numa parede), só com a cabeça e as mãos para fora. Dois cliques de novo tiram ele de lá |
| Ver uma reação | Clique nele uma vez |
| Fazer ele ficar quieto | Menu → **Pausar movimento**. Para voltar a circular: menu → **Retomar movimento** |
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
- Testes que abrem janelas ou movem o cursor, e a medição de desempenho: consulte os comandos e limites em
  `docs/PROJECT_CONTEXT.md`. Avise quem estiver usando o computador antes de rodá-los.

# Como iniciar o Buzzy

> **Estado atual:** o Buzzy aparece como o macaquinho em pixel art com o chapéu de palha
> (`docs/IDENTIDADE_VISUAL.md`). Pode ser arrastado com o mouse e, sozinho, anda pelo chão, escala as
> laterais da tela (inclusive a que encosta no outro monitor), pendura-se num cipó na borda de cima,
> pula, quica como borracha e descansa. Pelo menu, você escolhe a emoção dominante dele e invoca itens
> do "tamagotchi adulto", que ele usa quando você os arrasta até ele (seção 5). Essa parte já passou
> por testes automáticos e por uma verificação na tela, com cliques e teclas injetados por uma
> ferramenta de teste, não por uma pessoa; falta você conferir os desenhos, as animações e o tom.
> Ao abrir de novo, ele volta onde estava quando você o fechou, escondido ou preso do mesmo jeito e
> com a mesma emoção dominante (seção 5). Isso passou por testes automáticos, que abrem o Buzzy de
> verdade com um perfil de teste, mas ainda não pela verificação na tela nem por você. Sozinho, ele
> ainda fica no monitor em que está: atravessar para o outro monitor chega com a Fase 5, em
> andamento. As poses ainda são quadros fixos por estado; as animações completas chegam na Fase 6.

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

- Na primeira vez, o personagem aparece no canto inferior direito do monitor principal, com os pés
  logo acima da barra de tarefas. Nas outras, aparece onde estava quando você o fechou (seção 5).
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
| Abrir o menu | Botão direito no personagem, num item ou no ícone da bandeja |
| Escolher a emoção dominante | Menu → **Emoção dominante** → uma das caras, com o rosto ao lado do nome. Ela vira a cara de base e a mais frequente. **Automática** volta ao jeito de sempre. A opção atual fica marcada |
| Invocar um item | Menu → **Itens** → o item. Ele aparece ao lado do Buzzy, cai no chão e fica esperando. Cabem até 6; o sétimo tira o mais antigo |
| Dar um item a ele | Arraste o item com o mouse até o Buzzy e solte em cima dele: ele usa o item, com a animação do item e um efeito de desenho animado que passa sozinho. Clicar no item ou soltá-lo longe não faz ele usar: o item fica onde está ou cai de onde foi solto |
| Interromper o uso | Clique no Buzzy ou arraste-o. O efeito do item continua até passar sozinho |
| Tirar os itens da tela | Menu → **Itens** → **Recolher itens** |
| Esconder o Buzzy | Menu → **Esconder Buzzy** |
| Mostrar de novo | Clique no ícone da bandeja, ou menu da bandeja → **Mostrar Buzzy**, ou abra o `Buzzy.exe` outra vez |
| Fechar | Menu → **Sair** |

Com o menu aberto, dá para usar o teclado: cada opção tem uma letra, a sublinhada quando o Windows
mostra os sublinhados. Por exemplo, **D** e depois **F** escolhem a emoção **Feliz**, e **I** e
depois **B** invocam a **Banana**. Com o Buzzy
escondido, **Itens** fica indisponível; a emoção pode ser escolhida e aparece quando ele voltar.

Ao abrir de novo, o Buzzy lembra:

- o lugar onde estava, no mesmo monitor. Se aquele monitor não estiver ligado, ele aparece na mesma
  posição relativa de um monitor no mesmo lugar e com o mesmo tamanho ou, sem nenhum, do principal;
- se estava escondido na borda, volta escondido no mesmo lado; se você o deixou preso na parede ou no
  cipó, continua preso lá;
- a emoção dominante escolhida.

Os itens somem quando você fecha o Buzzy, de propósito. Escondido pelo menu (**Esconder Buzzy**), ele
reaparece ao abrir de novo.

Ele guarda isso sozinho, pouco depois de você soltá-lo, escondê-lo ou escolher uma emoção, e na hora
ao sair ou quando o Windows encerra a sessão. Se o programa for fechado à força, vale o que já estava
guardado.

Abrir o `Buzzy.exe` com ele já aberto não cria um segundo Buzzy: só traz de volta o que já está
rodando.

O Buzzy não usa a internet e não lê outros programas. Ele guarda a posição, o esconderijo, o "preso"
e as preferências em `%LOCALAPPDATA%\Buzzy\settings.json`, com a versão anterior do arquivo em
`settings.json.bak`. Para abrir essa pasta, digite `%LOCALAPPDATA%\Buzzy` na barra de endereços do
Explorador de Arquivos. Se o `settings.json` estiver estragado, ele usa a cópia `.bak` ou, sem ela,
começa do jeito padrão; na próxima vez que gravar, guarda o arquivo estragado como
`settings.corrupt.json`. Fora isso, só grava o log de diagnóstico, com a opção `--diagnostico`
(abaixo).

## 6. Se algo der errado

| Situação | O que fazer |
|---|---|
| Aviso "O Buzzy não roda como administrador" | Abra de novo normalmente, sem "Executar como administrador" |
| O Windows pede para baixar o .NET | Falta o .NET 10: instale o SDK (item 1) |
| O personagem sumiu | Clique no ícone da bandeja ou abra o `Buzzy.exe` de novo |
| Nada aparece, ou para relatar um problema | Abra com `Buzzy.exe --diagnostico`. Ele registra só eventos do próprio Buzzy em `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB) |
| O build falha com o arquivo em uso | Feche o Buzzy (menu → **Sair**) e compile de novo |
| Ele volta sempre a um lugar ruim e você quer a posição inicial | Feche o Buzzy (menu → **Sair**) e só então apague `settings.json` e `settings.json.bak` em `%LOCALAPPDATA%\Buzzy`. Apague os dois: sem o primeiro, ele usaria a cópia `.bak`. Com o Buzzy aberto não adianta, porque ele grava de novo ao sair. Ele volta ao canto inferior direito do principal e à emoção automática |

## 7. Para quem desenvolve

- Testes sem abrir janelas: `powershell -NoProfile -File tools\testar.ps1`.
- Testes que abrem janelas ou movem o cursor, e a medição de desempenho: consulte os comandos e limites em
  `docs/PROJECT_CONTEXT.md`. Avise quem estiver usando o computador antes de rodá-los.
- Sem opção nenhuma, o Buzzy lê e grava as suas configurações reais, em `%LOCALAPPDATA%\Buzzy`.
- Para testar sem mexer nas suas configurações, abra o Buzzy com `--perfil-de-teste NOME`, por exemplo
  `Buzzy.exe --perfil-de-teste meu-teste`. Os dados dele ficam em `%LOCALAPPDATA%\Buzzy\testes\NOME`.
  O nome tem até 32 caracteres: letras minúsculas sem acento, algarismos e hífen, que não pode vir no
  começo. Escreva a opção exatamente assim, separada do nome por um espaço: com outra grafia
  (`--perfil-de-teste=NOME`, maiúsculas, `/perfil-de-teste`) ou com um nome inválido, o Buzzy não lê
  nem guarda configuração nenhuma naquela vez. Os testes e as ferramentas do projeto já fazem isso
  sozinhos e conferem, antes e depois, que os seus arquivos reais não mudaram, olhando só a existência,
  o tamanho e as datas, nunca o conteúdo. Se você abrir o seu Buzzy enquanto eles rodam, essa
  conferência pode falhar.

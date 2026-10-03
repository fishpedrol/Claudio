# Como iniciar o Buzzy

> **Estado atual:** o Buzzy é o macaquinho em pixel art com o chapéu de palha. Dá para arrastá-lo com o
> mouse, e sozinho ele anda pelo chão, escala as laterais da tela (inclusive a que encosta no outro
> monitor), pendura-se num cipó na borda de cima, pula, quica como borracha e descansa. Pelo menu, você
> escolhe a emoção dominante dele e invoca itens do "tamagotchi adulto", que ele usa quando você os
> arrasta até ele (seção 5). Ao abrir de novo, ele volta onde estava. Tudo isso passou por testes
> automáticos e por verificações na tela feitas por uma ferramenta, com cliques e teclas injetados, não
> por uma pessoa: falta você conferir os desenhos, as animações e o tom. Ele também passa sozinho de
> um monitor para o outro: andando, num pulo ou subindo pela parede. As poses ainda são quadros fixos (as
> animações chegam na Fase 6). O estado detalhado está em `docs/PROJECT_CONTEXT.md`.

## 1. Antes da primeira vez

- **Windows 11.**
- **SDK do .NET 10.** O projeto fixa a versão 10.0.401 (arquivo `global.json`). Para conferir, rode no
  PowerShell `dotnet --list-sdks`: a lista precisa ter uma linha começando com `10.0.`. Se não tiver,
  instale o SDK do .NET 10 pelo site oficial da Microsoft (dotnet.microsoft.com).

## 2. Compilar

No PowerShell, dentro da pasta do projeto:

```powershell
cd C:\Users\Cliente\Documents\claudio
dotnet build src\Buzzy.App\Buzzy.App.csproj -c Release
```

Deu certo quando o fim da saída mostra `0 Erro(s)` e `Resumo: APROVADO` (o portão de segurança, que
roda sozinho depois de cada build). Só é preciso compilar de novo quando o código mudar. Feche o Buzzy
antes: com ele aberto, o build falha porque o arquivo está em uso.

## 3. Abrir o mascote

```powershell
.\src\Buzzy.App\bin\Release\net10.0-windows\Buzzy.exe
```

Ou dê dois cliques em `Buzzy.exe` nessa pasta. Para um atalho na área de trabalho: botão direito em
`Buzzy.exe` → **Mostrar mais opções** → **Enviar para** → **Área de trabalho (criar atalho)**. Não use
"Executar como administrador": o Buzzy recusa rodar assim e mostra um aviso.

A cópia da pasta `net10.0-windows` na sua Área de Trabalho foi atualizada em 2026-10-02.

## 4. O que você vai ver

- Na primeira vez, o personagem aparece no canto inferior direito do monitor principal, com os pés
  logo acima da barra de tarefas; nas outras, onde estava quando você o fechou.
- Ele fica sempre por cima das outras janelas e não aparece na barra de tarefas nem no Alt+Tab.
- Clicar nele não tira o foco do programa que você está usando, e clicar nas partes transparentes em
  volta dele atinge o que está embaixo.
- Um ícone do Buzzy vai para a bandeja, perto do relógio. No Windows 11, ícones novos ficam escondidos
  na setinha **^**; para deixá-lo sempre visível: **Configurações** → **Personalização** → **Barra de
  tarefas** → **Outros ícones da bandeja do sistema** → ative **Buzzy**.

## 5. Como usar

| Quero... | Como fazer |
|---|---|
| Mudar o Buzzy de lugar | Clique nele e arraste; solto no meio do ar, ele cai até o chão (e quica, se cair de alto) |
| Pendurar no cipó | Arraste até perto da borda de cima e solte: ele agarra um cipó e fica lá até você tirá-lo |
| Grudar numa parede | Arraste até perto de uma lateral da tela e solte: ele gruda na parede e fica lá até você tirá-lo |
| Esconder o Buzzy na borda | Dois cliques nele: ele se esconde atrás da barra de tarefas (ou da lateral, se estiver numa parede), só com a cabeça e as mãos para fora. Dois cliques de novo o tiram de lá |
| Ver uma reação | Clique nele uma vez |
| Fazer ele ficar quieto | Menu → **Pausar movimento**; para voltar a circular, **Retomar movimento** |
| Abrir o menu | Botão direito no personagem, num item ou no ícone da bandeja |
| Escolher a emoção dominante | Menu → **Emoção dominante** → uma das caras, com o rosto ao lado do nome: ela vira a cara de base e a mais frequente. **Automática** volta ao jeito de sempre |
| Invocar um item | Menu → **Itens** → o item. Ele aparece ao lado do Buzzy, cai no chão e espera. Cabem até 6; o sétimo tira o mais antigo |
| Dar um item a ele | Arraste o item até o Buzzy e solte em cima dele: ele usa o item, com a animação dele e um efeito de desenho animado que passa sozinho. Clicar no item ou soltá-lo longe não faz ele usar |
| Interromper o uso | Clique no Buzzy ou arraste-o, também quando ele fuma sozinho. O efeito continua até passar |
| Acalmar o efeito aos poucos | Dê comida ou bebida sem álcool: **banana**, **água**, **café** ou **energético**. Cada um acalma um passo o efeito de uma substância; a água acalma qualquer efeito |
| Tirar os itens da tela | Menu → **Itens** → **Recolher itens** |
| Jogar ou ver vídeo em tela cheia | Nada a fazer: com uma janela em tela cheia (um jogo, um vídeo) em primeiro plano, ele vai para o outro monitor e volta para onde estava quando ela sai do primeiro plano; com um monitor só, ou com ela cobrindo todos, ele some até lá. Arrastar o Buzzy ou mostrá-lo pela bandeja nesse meio-tempo vale como escolha sua. Para desligar, menu → **Desviar da tela cheia** (marcado quando ligado, o padrão); a escolha é lembrada ao reabrir |
| Ligar ou desligar o conteúdo adulto | Menu → **Conteúdo adulto** (marcado quando ligado, o padrão). Desligado, **Itens** só tem banana, água, café e energético; os outros itens saem da tela, os efeitos de substância acabam e ele não fuma sozinho. A escolha é lembrada ao reabrir |
| Esconder o Buzzy | Menu → **Esconder Buzzy** |
| Mostrar de novo | Clique no ícone da bandeja, ou menu da bandeja → **Mostrar Buzzy**, ou abra o `Buzzy.exe` outra vez |
| Fechar | Menu → **Sair** |

Com o menu aberto, cada opção tem uma letra (a sublinhada): por exemplo, **D** e depois **F** escolhem
a emoção **Feliz**, **I** e depois **B** invocam a **Banana**, **A** liga ou desliga o conteúdo adulto e **T**, o desvio da tela cheia. Com o Buzzy escondido, **Itens** fica
indisponível; a emoção pode ser escolhida e aparece quando ele voltar.

**Os efeitos são de desenho animado** e passam sozinhos:

- **Paranoia:** se ele misturar uma droga sintética do jogo (bala, MD, cocaína ou lança-perfume) com
  outra substância, pode ficar paranoico, "como o meme 'os cara tá no teto'": sua, treme, olha e aponta
  pro teto e se agacha segurando o chapéu. A chance é de 1 em 8, sorteada uma vez por mistura; só uma
  leva nova, depois que os efeitos passam, tem outra chance. Álcool e maconha nunca o deixam paranoico,
  nem uma sintética sozinha, repetida. Comida e bebida sem álcool também a acalmam aos poucos.
- **A bala** é droga sintética nessa regra do jogo: dá o efeito eufórico, com corações em volta da
  cabeça, e pode entrar numa mistura. O desenho dela continua o de um doce.
- **Baseado por conta própria:** de vez em quando, parado no chão, ele fuma um baseado tirado do
  chapéu, sem item na tela, com o mesmo efeito do baseado do menu — na energia Média, mais ou menos um a
  cada 14 ou 15 minutos, e chapado cerca de 39% do tempo se ninguém der nada a ele. Não acende outro já
  chapado ou paranoico, nem fuma escondido, no ar, pausado ou enquanto você segura um item; um clique o
  interrompe. Enquanto ele fuma (3,5 s), um item solto nele cai. O baseado conta na mistura: uma
  sintética dada com ele chapado do baseado dele pode deixá-lo paranoico (1 em 8 por leva).

A divisão dos itens é só regra do jogo, de desenho animado, e não diz nada sobre o mundo real.

**Ao abrir de novo**, o Buzzy lembra o lugar onde estava, no mesmo monitor (se aquele monitor não
estiver ligado, a mesma posição relativa de um monitor no mesmo lugar e do mesmo tamanho ou, sem
nenhum, do principal); se estava escondido na borda, volta escondido no mesmo lado; se você o deixou
preso na parede ou no cipó, continua preso; e a emoção dominante. Os itens somem quando você o fecha,
de propósito, e escondido pelo menu ele reaparece ao abrir. Ele guarda isso sozinho, pouco depois de
você soltá-lo, escondê-lo ou escolher uma emoção, e na hora ao sair ou quando o Windows encerra a
sessão; fechado à força, vale o que já estava guardado. Abrir o `Buzzy.exe` com ele aberto só traz de
volta o que já está rodando.

O Buzzy não usa a internet e não lê outros programas. Guarda a posição, o esconderijo, o "preso" e as
preferências em `%LOCALAPPDATA%\Buzzy\settings.json` (digite `%LOCALAPPDATA%\Buzzy` na barra de
endereços do Explorador de Arquivos), com a versão anterior em `settings.json.bak`. Se o
`settings.json` estiver estragado, ele usa a cópia `.bak` ou, sem ela, começa do jeito padrão, e na
próxima gravação guarda o estragado como `settings.corrupt.json`. Fora isso, só grava o log de
diagnóstico, com a opção `--diagnostico`.

## 6. Se algo der errado

| Situação | O que fazer |
|---|---|
| Aviso "O Buzzy não roda como administrador" | Abra de novo normalmente, sem "Executar como administrador" |
| O Windows pede para baixar o .NET | Falta o .NET 10: instale o SDK (item 1) |
| O personagem sumiu | Clique no ícone da bandeja ou abra o `Buzzy.exe` de novo. Com um jogo ou vídeo em tela cheia, ele pode estar no outro monitor, ou escondido até a tela cheia sair do primeiro plano |
| Ele fica por cima de um jogo em tela cheia | Confira se **Desviar da tela cheia** está marcado no menu. Se estiver e ele continuar por cima, abra com `Buzzy.exe --diagnostico`, jogue um pouco e mande as linhas `TELA_CHEIA` do log: elas dizem o que o Windows informou, sem nada do jogo |
| Nada aparece, ou para relatar um problema | Abra com `Buzzy.exe --diagnostico`: ele registra só eventos do próprio Buzzy em `%LOCALAPPDATA%\Buzzy\diagnostico.log` (até 1 MB) |
| O build falha com o arquivo em uso | Feche o Buzzy (menu → **Sair**) e compile de novo |
| Ele volta sempre a um lugar ruim e você quer a posição inicial | Feche o Buzzy (menu → **Sair**) e só então apague `settings.json` e `settings.json.bak` em `%LOCALAPPDATA%\Buzzy` — os dois, porque sem o primeiro ele usaria a cópia `.bak`; com ele aberto não adianta, porque grava de novo ao sair. Ele volta ao canto inferior direito do principal e à emoção automática |

## 7. Para quem desenvolve

- Testes sem abrir janelas: `powershell -NoProfile -File tools\testar.ps1`. Testes que abrem janelas ou
  movem o cursor e a medição de desempenho: comandos e limites em `docs/PROJECT_CONTEXT.md`. Avise quem
  estiver usando o computador antes de rodá-los.
- Sem opção nenhuma, o Buzzy lê e grava as suas configurações reais. Para testar sem mexer nelas, abra
  com `--perfil-de-teste NOME` (por exemplo, `Buzzy.exe --perfil-de-teste meu-teste`): os dados ficam
  em `%LOCALAPPDATA%\Buzzy\testes\NOME`. O nome tem até 32 caracteres — letras minúsculas sem acento,
  algarismos e hífen, que não pode vir no começo —, e a opção precisa ser escrita exatamente assim,
  separada do nome por um espaço; com outra grafia (`--perfil-de-teste=NOME`, maiúsculas,
  `/perfil-de-teste`) ou um nome inválido, o Buzzy não lê nem guarda configuração nenhuma naquela vez.
  Os testes e as ferramentas do projeto já fazem isso e conferem, antes e depois, que os seus arquivos
  reais não mudaram (só existência, tamanho e datas, nunca o conteúdo); com o seu Buzzy aberto enquanto
  eles rodam, essa conferência pode falhar.
- Com `--diagnostico`, cada sorteio da paranoia, saindo ou não, vira uma linha `PARANOIA` no log (por
  exemplo `PARANOIA|item=Bala|chance=1 em 8|saiu=sim|substancias=2|distintas=Vodka,Bala`), e o baseado
  por conta própria aparece na linha `NUCLEO` da transição, com a regra
  `IDLE + AUTONOMY_TIMER: Fumar Baseado por conta própria`, sem linha `ITEM`. O contrato das linhas
  está em `docs/ARCHITECTURE.md`, seção 2.13.4.

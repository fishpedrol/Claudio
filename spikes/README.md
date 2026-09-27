# spikes/ — protótipos descartáveis da Etapa 0B

> **Este código é descartável e não vai para a Fase 1.** Ele existe só para responder às
> perguntas P1, P2 e P3 de [../docs/TODO.md](../docs/TODO.md), Etapa 0B, antes de o Buzzy
> ganhar código de produto. Nada aqui é módulo planejado do aplicativo: não há janela do
> personagem, bandeja, configurações nem núcleo de estado.
>
> Última atualização: 2026-09-26

## O que tem aqui

```
spikes/
├── global.json              # fixa o SDK do .NET em 10.0.401
├── BuzzySpike/              # um único projeto WPF, com modos de operação
│   ├── BuzzySpike.csproj    # net10.0-windows, x64, zero pacotes NuGet
│   ├── app.manifest         # Per-Monitor V2 e asInvoker
│   ├── Programa.cs          # linha de comando e escolha do modo
│   ├── JanelaSpike.cs       # a janela transparente e o comportamento de cada modo
│   ├── FiguraTeste.cs       # gera a figura de teste com alfa exato, em código
│   ├── Interop.cs           # chamadas ao Windows, isoladas num só arquivo
│   └── Diagnostico.cs       # registro de evidência em resultados/
├── ferramentas/             # scripts de execução e leitura (PowerShell 5.1)
└── resultados/              # logs e relatórios gerados pelas execuções
```

Um projeto só, com modos, em vez de três projetos: assim a medição de P2 vale para
exatamente a mesma janela que P1 e P3 exercitam.

## Por que um único projeto e nenhuma dependência

- `dotnet list package` não retorna nada: o protótipo usa só o que vem no SDK e no WPF.
- A versão do SDK está fixada em `global.json`, então o resultado é reproduzível.
- Todas as chamadas ao Windows ficam em `Interop.cs`, espelhando o papel que o adaptador
  de plataforma terá no produto ([ARCHITECTURE.md 2.2](../docs/ARCHITECTURE.md)).

## Limites respeitados de propósito

O protótipo e as ferramentas **não** usam: hook global, injeção de input (`SendInput`),
captura de tela, leitura periódica do cursor, rede, telemetria, execução de comandos, nem
leitura de título ou conteúdo de janela de outro processo. A verificação de foco compara
apenas identificadores de janela. Ver [SECURITY.md 3.2](../docs/SECURITY.md).

Consequência prática: **o veredito de P1 e de P3 depende de cliques e arrastes feitos por
uma pessoa.** Os scripts produzem toda a evidência que dá para produzir sem injetar input,
e param aí. Onde falta o gesto humano, o resultado fica registrado como pendente, nunca
como aprovado.

## Como compilar

Precisa do SDK do .NET 10. Confira com `dotnet --list-sdks`.

```powershell
cd spikes\BuzzySpike
dotnet build -c Release
```

## Como rodar cada protótipo

Todos os scripts ficam em `ferramentas/` e escrevem em `resultados/`.

### P1 — o clique atravessa os pixels transparentes?

```powershell
cd spikes\ferramentas
.\sonda-p1.ps1 -ManterAberto     # abre Bloco de Notas + protótipo e sonda o teste de acerto
# clique no centro das quatro faixas
.\ler-p1.ps1 -CliqueiTodasAsQuatro
```

A figura de teste tem quatro faixas, cada uma com uma moldura opaca colorida e um rótulo
com o valor de alfa do seu interior:

| Faixa | Alfa do interior | Aparência | Comportamento esperado do clique |
|---|---|---|---|
| 1 | 0 | moldura vermelha, interior invisível | atravessa e chega ao aplicativo de baixo |
| 2 | 1 | moldura laranja, interior quase invisível | fica na janela do Buzzy |
| 3 | 255 | preenchimento verde | fica na janela do Buzzy |
| 4 | 128 | moldura azul, interior meio transparente | fica na janela do Buzzy |

A coluna escura da esquerda é opaca e serve de controle: um clique nela sempre pertence à
janela. A faixa alfa 128 não estava no pedido original de P1; ela foi acrescentada para
testar a regra de [ARCHITECTURE.md 2.13.7 item 7](../docs/ARCHITECTURE.md), que afirma que
só alfa exatamente 0 é transparente ao clique.

### P3 — o arraste funciona sem roubar foco?

```powershell
cd spikes\ferramentas
.\sonda-p3.ps1        # Parte A automatizada, depois imprime os passos B1 a B7
# faça os passos B1 a B7
.\ler-p3.ps1
```

A Parte A verifica sozinha o que não exige gesto: que a janela aparece sem tirar o foco,
que os estilos `WS_EX_NOACTIVATE` e `WS_EX_LAYERED` estão presentes, e que a janela
atravessa os dois monitores por `SetWindowPos` sem trocar o foco. A Parte B é o arraste
com o mouse, que só uma pessoa faz.

Se a captura simples falhar, existe o recuo previsto em TODO.md:

```powershell
.\sonda-p3.ps1 -Margem
.\ler-p3.ps1 -Modo p3-margem
```

### P2 — quanto custa ficar parado e animar?

```powershell
cd spikes\ferramentas
.\medir-p2-tudo.ps1              # sequência completa, cerca de 92 minutos
```

Ou uma etapa por vez:

```powershell
.\medir-p2.ps1 -Modo p2-repouso    -Minutos 60 -IntervaloSegundos 5
.\medir-p2.ps1 -Modo p2-anim10     -Minutos 10 -IntervaloSegundos 1
.\medir-p2.ps1 -Modo p2-anim60     -Minutos 10 -IntervaloSegundos 1
.\medir-p2.ps1 -Modo p2-anim60comp -Minutos 10 -IntervaloSegundos 1
```

As métricas seguem M1 a M4 de [DEC-011](../docs/DECISIONS.md), medidas **por PID** e não
por nome de processo, porque pode haver mais de uma instância aberta ao mesmo tempo.

O modo `p2-anim60comp` não estava no pedido. Ele existe porque a primeira medição mostrou
que `DispatcherTimer` não entrega 60 quadros por segundo sem elevar a resolução global do
timer, o que DEC-011 proíbe. Medir os dois caminhos separa "60 pedidos" de "60 entregues".

## Detalhes que afetam a leitura dos resultados

- **Codificação dos scripts.** Os `.ps1` estão em UTF-8 **com BOM**. Sem o BOM, o Windows
  PowerShell 5.1 lê o arquivo como ANSI e os acentos viram tokens inválidos, que quebram o
  script. Ao editar, preserve o BOM.
- **Resolução do timer global.** Nesta máquina ela já está em 1 ms por causa de outro
  processo, antes de o Buzzy abrir. O critério de DEC-011 é sobre *quem muda* a resolução,
  então os relatórios medem antes, durante e depois, para permitir atribuição.
- **Contagem interna de desenho.** `SuperficieContada.OnRender` conta as passagens de
  desenho. Em repouso ela deve ficar em 1 ou 2: é a prova interna, de custo zero, de que
  nada está sendo redesenhado. Um assinante de `CompositionTarget.Rendering` não serviria
  para isso, porque assinar já força quadros contínuos.
- **Janela de medição.** O modo P2 abre a janela no canto superior esquerdo do monitor
  secundário, em (-1900,40), para ficar longe de onde se está trabalhando. Se algo passar
  por cima dela durante a medição, a contagem de `OnRender` sobe e o relatório revela.

## Como jogar isto fora

Apagar a pasta `spikes/` inteira. Nada no produto vai depender dela. O que precisa
sobreviver são os resultados registrados em
[../docs/DEVELOPMENT_LOG.md](../docs/DEVELOPMENT_LOG.md).

# spikes/ — protótipos descartáveis da Etapa 0B

Tudo aqui é código de experimento, não parte do aplicativo. Nenhum arquivo deve ser copiado para a Fase 1 sem uma decisão técnica explícita. P1, P2 e P3 e seus resultados ficam descritos em [docs/TODO.md](../docs/TODO.md).

## Conteúdo

- BuzzySpike: janela WPF usada nas medições P1/P2/P3; sem dependências NuGet.
- SondaP3: harness isolado de P3, atualmente em desenvolvimento.
- ferramentas/: scripts PowerShell legados e medições.
- resultados/: logs das execuções existentes.

## Build do protótipo

Requer o SDK .NET 10 definido em global.json.

```powershell
cd spikes\BuzzySpike
dotnet build -c Release
```

## P1 e P2

P1 foi aceito nos limites descritos em TODO.md; P2 também foi aceito como medição de viabilidade. Só os repita se houver razão técnica. Os comandos antigos estão em ferramentas/sonda-p1.ps1, ler-p1.ps1, medir-p2-tudo.ps1 e medir-p2.ps1. Não trate input sintético como gesto humano.

## P3 — instrução atual

O harness isolado já registrou três rodadas dos cenários centrais com resultado OK. O resultado agregado marca 0/3 porque B4b adicional abortou pela salvaguarda ao detectar outra janela no ponto de reativação; nenhum clique foi feito. O Claude informou uma correção, ainda sem execução registrada. Confira spikes/resultados/p3-receptor.log antes de declarar o gate concluído.

Não use o fluxo antigo de ferramentas/auto-p3.ps1 para fechar o gate: ele abre o Bloco de Notas, injeta texto, depende de confirmação visual e move o cursor real. Não abra, leia ou inspecione conteúdo de outro aplicativo. Não feche processos do usuário nem altere configurações globais. Identifique input por SendInput como sintético; os dez movimentos anteriores foram uma exploração informal da namorada do usuário, não teste formal nem evidência humana aprovada.

## Salvaguardas

As ferramentas de input são externas ao produto e usam SendInput apenas para teste. Antes de executar qualquer harness que mova o cursor ou altere ClickLock, verifique se preserva e restaura o estado original mesmo em caso de falha, e se encerra apenas processos criados pelo próprio teste. Não execute enquanto houver interação do usuário com o computador.

P2 já registrou a configuração do timer e o método de medição em TODO.md. Resultados detalhados das execuções existentes ficam em resultados/.

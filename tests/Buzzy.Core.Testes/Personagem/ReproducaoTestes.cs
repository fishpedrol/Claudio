using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.Core.Testes.Personagem;

/// <summary>
/// Reproduções gravadas comparadas com um resultado de referência (TODO.md, Fase 2). Cada
/// arquivo de <c>Referencias/</c> traz, no cabeçalho, a semente e a configuração
/// (<c># semente: 42</c>, <c># queda-fisica: sim</c>, <c># painel: sim</c>,
/// <c># acoes: Andar,Descansar</c>), depois as linhas de evento (<c>&gt;</c>) e a saída esperada.
///
/// Para regravar as referências depois de uma mudança intencional, rode com a variável de
/// ambiente <c>BUZZY_ATUALIZAR_REFERENCIAS=1</c> e revise a diferença no Git antes de aceitar.
/// </summary>
internal static class ReproducaoTestes
{
    [Teste]
    public static void ReproducoesGravadasBatemComAReferencia()
    {
        string pasta = Path.Combine(AppContext.BaseDirectory, "Referencias");
        string[] arquivos = [.. Directory.GetFiles(pasta, "*.txt").Where(f => !f.EndsWith(".obtido.txt", StringComparison.Ordinal))];
        Afirmar.Verdadeiro(arquivos.Length >= 4, $"referências encontradas em {pasta}: {arquivos.Length}");
        bool atualizar = Environment.GetEnvironmentVariable("BUZZY_ATUALIZAR_REFERENCIAS") == "1";

        var falhas = new List<string>();
        foreach (string arquivo in arquivos.Order(StringComparer.Ordinal))
        {
            string[] linhas = File.ReadAllLines(arquivo, Encoding.UTF8);
            string[] cabecalho = [.. linhas.TakeWhile(l => l.StartsWith('#') || l.Length == 0)];
            (ConfiguracaoDoNucleo cfg, ulong semente) = LerCabecalho(cabecalho, arquivo);

            IReadOnlyList<string> obtido = Gravacao.Reproduzir(cfg, semente, linhas, PorNome);
            string[] esperado = [.. linhas.Skip(cabecalho.Length).Where(l => l.Length > 0 && !l.StartsWith('#'))];

            if (atualizar)
            {
                string fonte = Path.Combine(PastaDasFontes(), "Referencias", Path.GetFileName(arquivo));
                File.WriteAllText(fonte, Gravacao.Juntar(cabecalho.Concat(obtido)), new UTF8Encoding(false));
                Console.WriteLine($"         referência regravada: {fonte}");
                continue;
            }

            int diferente = PrimeiraDiferenca(esperado, obtido);
            if (diferente >= 0)
            {
                string obtidoEm = Path.ChangeExtension(arquivo, ".obtido.txt");
                File.WriteAllText(obtidoEm, Gravacao.Juntar(cabecalho.Concat(obtido)), new UTF8Encoding(false));
                falhas.Add($"{Path.GetFileName(arquivo)}, linha {diferente + 1} da saída: esperado <{Linha(esperado, diferente)}>, obtido <{Linha(obtido, diferente)}> (saída completa em {obtidoEm})");
            }
        }
        if (falhas.Count > 0) Afirmar.Falhar(string.Join(Environment.NewLine + "         ", falhas));
    }

    [Teste]
    public static void EscreverELerDevolvemOMesmoEvento()
    {
        Topologia umMonitor = TopologiasDeExemplo.UmMonitor;
        Evento[] eventos =
        [
            new Press(new PontoPx(-5, 7)), new Click(), new DoubleClick(), new DragStart(), new DragMove(new PontoPx(1, -2)),
            new DragEnd(new PontoPx(3, 4)), new DragCancel(), new ContextMenu(new PontoPx(9, 9)), new EnergyPanelOpen(),
            new EnergySelected(NivelDeEnergia.Alta), new EnergyPanelClose(), new CmdHide(), new CmdShow(), new CmdPauseAutonomy(),
            new CmdResumeAutonomy(), new CmdOpenSettings(), new CmdResetPosition(), new CmdExit(), new SessionLocked(),
            new SessionUnlocked(), new Suspending(), new Resumed(), new SessionEnding(), new Tick(),
            new FullscreenTargetsChanged(new MonitoresOcupados([TopologiasDeExemplo.Display2, TopologiasDeExemplo.Display1])),
            new FullscreenTargetsChanged(MonitoresOcupados.Nenhum),
            new SettingsChanged(new Preferencias(NivelDeEnergia.Baixa, false)), new MovementSignal(SinalDeMovimento.BordaSuperior),
            new AutonomyTimer(12), new ExpressionChange(Expressao.Travesso),
            new Loaded(umMonitor, new PosicaoDoPersonagem(TopologiasDeExemplo.Display1, 0.5, 1, new PontoPx(960, 1032)), Preferencias.Padrao),
            new TopologyChanged(umMonitor),
        ];
        EstadoDoNucleo estado = EstadoDoNucleo.Inicial(1);
        foreach (Evento e in eventos)
        {
            string linha = Gravacao.Escrever(e, _ => "UmMonitor");
            IReadOnlyList<Evento> lidos = Gravacao.Ler(linha, _ => umMonitor, estado);
            Afirmar.Igual(1, lidos.Count, linha);
            Afirmar.Igual(linha, Gravacao.Escrever(lidos[0], _ => "UmMonitor"), "ida e volta");
            if (e is not (Loaded or TopologyChanged)) Afirmar.Igual(e, lidos[0], $"mesmo evento para {linha}");
        }
        Afirmar.Igual(36, Gravacao.Ler("Tick vezes=36", _ => umMonitor, estado).Count, "Tick vezes=N");
        Afirmar.Lanca<FormatException>(() => Gravacao.Ler("Voar alto=sim", _ => umMonitor, estado));
    }

    private static Topologia PorNome(string nome)
        => TopologiasDeExemplo.Todas.FirstOrDefault(t => t.Nome == nome).Topologia
           ?? throw new FormatException($"Topologia de exemplo desconhecida: {nome}");

    private static (ConfiguracaoDoNucleo, ulong) LerCabecalho(string[] cabecalho, string arquivo)
    {
        var cfg = new ConfiguracaoDoNucleo();
        ulong semente = 1;
        foreach (string linha in cabecalho)
        {
            int doisPontos = linha.IndexOf(':', StringComparison.Ordinal);
            if (!linha.StartsWith("# ", StringComparison.Ordinal) || doisPontos < 0) continue;
            string chave = linha[2..doisPontos].Trim();
            string valor = linha[(doisPontos + 1)..].Trim();
            switch (chave)
            {
                case "semente":
                    semente = ulong.Parse(valor, NumberStyles.None, CultureInfo.InvariantCulture);
                    break;
                case "queda-fisica":
                    cfg = cfg with { QuedaFisica = valor == "sim" };
                    break;
                case "painel":
                    cfg = cfg with { PainelDeEnergiaDisponivel = valor == "sim" };
                    break;
                case "acoes":
                    cfg = cfg with
                    {
                        Acoes = valor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Aggregate(AcoesAutonomas.Nenhuma, (total, a) => total | Enum.Parse<AcoesAutonomas>(a)),
                    };
                    break;
                case "descricao":
                    break;
                default:
                    throw new FormatException($"{Path.GetFileName(arquivo)}: diretiva de cabeçalho desconhecida: {chave}");
            }
        }
        return (cfg, semente);
    }

    private static int PrimeiraDiferenca(IReadOnlyList<string> esperado, IReadOnlyList<string> obtido)
    {
        int n = Math.Max(esperado.Count, obtido.Count);
        for (int i = 0; i < n; i++)
            if (i >= esperado.Count || i >= obtido.Count || esperado[i] != obtido[i]) return i;
        return -1;
    }

    private static string Linha(IReadOnlyList<string> linhas, int i) => i < linhas.Count ? linhas[i] : "(fim)";

    private static string PastaDasFontes([CallerFilePath] string caminho = "") => Path.GetDirectoryName(Path.GetDirectoryName(caminho)!)!;
}

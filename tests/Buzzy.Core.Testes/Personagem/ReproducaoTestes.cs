using System.Globalization;
using System.Reflection;
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
/// As referências são lidas da pasta-fonte (não da cópia do build, que pode estar velha), a lista
/// é fixa (<see cref="Referencias"/>) e o conteúdo inteiro do arquivo precisa ser igual ao que a
/// regravação escreveria, normalizando só o fim de linha (o Git pode trocar LF por CRLF).
///
/// Para regravar as referências depois de uma mudança intencional, rode só este teste, fora de
/// integração contínua, com a variável de ambiente <c>BUZZY_ATUALIZAR_REFERENCIAS=1</c> (por
/// exemplo, <c>Buzzy.Core.Testes.exe --filtro ReproducaoTestes</c>) e revise a diferença no Git
/// antes de aceitar. Com a variável ligada em qualquer outra execução, o teste falha em vez de
/// regravar e passar em silêncio.
/// </summary>
internal static class ReproducaoTestes
{
    /// <summary>As reproduções exigidas, nem mais nem menos: um arquivo apagado ou acrescentado sem entrar aqui falha.</summary>
    private static readonly string[] Referencias =
    [
        "01-clique-e-arraste.txt",
        "02-ocultacao-e-sessao.txt",
        "03-tela-cheia.txt",
        "04-agenda-e-energia.txt",
        "05-movimento-e-fisica.txt",
    ];

    private const string VariavelDeAtualizacao = "BUZZY_ATUALIZAR_REFERENCIAS";

    /// <summary>Variáveis que os serviços de integração contínua comuns definem.</summary>
    private static readonly string[] VariaveisDeIntegracaoContinua = ["CI", "TF_BUILD", "GITHUB_ACTIONS"];

    // Item S da revisão do gate da Fase 2.
    [Teste]
    public static void ReproducoesGravadasBatemComAReferencia()
    {
        string fontes = Path.Combine(PastaDasFontes(), "Referencias");
        Afirmar.Verdadeiro(Directory.Exists(fontes), $"pasta-fonte das referências não encontrada: {fontes}");
        Afirmar.Sequencia(Referencias, ListarReferencias(fontes), $"referências em {fontes}");

        bool atualizar = Environment.GetEnvironmentVariable(VariavelDeAtualizacao) == "1";
        if (atualizar && RecusaDeAtualizacao(Environment.GetEnvironmentVariable, Environment.GetCommandLineArgs(), NomesDosTestes()) is { } recusa)
        {
            Afirmar.Falhar($"{VariavelDeAtualizacao}=1 fora de uso local explícito ({recusa}): nada foi regravado. "
                + $"Para regravar, rode só as reproduções (--filtro {nameof(ReproducaoTestes)}) fora de integração contínua e revise a diferença no Git; senão, desligue a variável.");
        }

        var falhas = new List<string>();
        foreach (string nome in Referencias)
        {
            string arquivo = Path.Combine(fontes, nome);
            string conteudo = NormalizarFimDeLinha(File.ReadAllText(arquivo, Encoding.UTF8));
            string[] linhas = conteudo.Split('\n');
            string[] cabecalho = [.. linhas.TakeWhile(l => l.StartsWith('#') || l.Length == 0)];
            (ConfiguracaoDoNucleo cfg, ulong semente) = LerCabecalho(cabecalho, arquivo);

            IReadOnlyList<string> obtido = Gravacao.Reproduzir(cfg, semente, linhas, PorNome);
            string esperado = Gravacao.Juntar(cabecalho.Concat(obtido));

            if (atualizar)
            {
                File.WriteAllText(arquivo, esperado, new UTF8Encoding(false));
                Console.WriteLine($"         referência regravada ({VariavelDeAtualizacao}=1): {arquivo}");
                continue;
            }

            if (!string.Equals(conteudo, esperado, StringComparison.Ordinal))
            {
                string obtidoEm = Path.Combine(AppContext.BaseDirectory, "Referencias", Path.ChangeExtension(nome, ".obtido.txt"));
                Directory.CreateDirectory(Path.GetDirectoryName(obtidoEm)!);
                File.WriteAllText(obtidoEm, esperado, new UTF8Encoding(false));
                string[] noArquivo = conteudo.Split('\n'), reproduzido = esperado.Split('\n');
                int diferente = PrimeiraDiferenca(noArquivo, reproduzido);
                falhas.Add($"{nome}, linha {diferente + 1}: no arquivo <{Linha(noArquivo, diferente)}>, reproduzido <{Linha(reproduzido, diferente)}> (saída completa em {obtidoEm})");
            }
        }
        if (falhas.Count > 0) Afirmar.Falhar(string.Join(Environment.NewLine + "         ", falhas));
    }

    // Item S: a regravação só vale como uso local explícito.
    [Teste]
    public static void RegravarReferencias_SoEmUsoLocalExplicito()
    {
        string[] testes = [.. NomesDosTestes()];
        Afirmar.Verdadeiro(testes.Contains($"{nameof(ReproducaoTestes)}.{nameof(ReproducoesGravadasBatemComAReferencia)}"), "os nomes seguem os do executor");
        string? SemVariaveis(string _) => null;
        string? EmIntegracao(string nome) => nome == "GITHUB_ACTIONS" ? "true" : null;
        string[] exe = ["Buzzy.Core.Testes.exe"];

        Afirmar.NaoNulo(RecusaDeAtualizacao(SemVariaveis, exe, testes), "suíte completa, sem --filtro: recusa");
        Afirmar.NaoNulo(RecusaDeAtualizacao(SemVariaveis, [.. exe, "--filtro", "Testes"], testes), "filtro que também escolhe outros testes: recusa");
        Afirmar.NaoNulo(RecusaDeAtualizacao(SemVariaveis, [.. exe, "--filtro", "EscreverELer"], testes), "filtro que não escolhe as reproduções gravadas: recusa");
        Afirmar.NaoNulo(RecusaDeAtualizacao(SemVariaveis, [.. exe, "--filtro"], testes), "--filtro sem texto: recusa");
        Afirmar.NaoNulo(RecusaDeAtualizacao(EmIntegracao, [.. exe, "--filtro", nameof(ReproducaoTestes)], testes), "integração contínua: recusa");
        Afirmar.Nulo(RecusaDeAtualizacao(SemVariaveis, [.. exe, "--filtro", nameof(ReproducaoTestes)], testes), "só as reproduções, localmente: aceita");
        Afirmar.Nulo(RecusaDeAtualizacao(SemVariaveis, [.. exe, "--filtro", "reproducoesgravadas"], testes), "o filtro ignora maiúsculas, como o executor: aceita");
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

    /// <summary>
    /// Por que recusar a regravação, ou nulo se ela é uso local explícito: fora de integração
    /// contínua e com o executor restrito, pelo <c>--filtro</c>, a testes das reproduções que
    /// incluem o de comparação. Uma variável esquecida no ambiente, numa execução completa ou em
    /// CI, aceitaria em silêncio qualquer mudança de comportamento.
    /// </summary>
    private static string? RecusaDeAtualizacao(Func<string, string?> ambiente, IReadOnlyList<string> argumentos, IEnumerable<string> testes)
    {
        foreach (string variavel in VariaveisDeIntegracaoContinua)
        {
            if (!string.IsNullOrEmpty(ambiente(variavel))) return $"a variável {variavel} indica integração contínua";
        }

        int indice = -1;
        for (int i = 0; i < argumentos.Count; i++)
        {
            if (argumentos[i] == "--filtro") indice = i;
        }
        if (indice < 0 || indice + 1 >= argumentos.Count) return "a execução não restringiu os testes com --filtro";

        string filtro = argumentos[indice + 1];
        string prefixo = nameof(ReproducaoTestes) + ".";
        string[] escolhidos = [.. testes.Where(t => t.Contains(filtro, StringComparison.OrdinalIgnoreCase))];
        if (!escolhidos.Contains(prefixo + nameof(ReproducoesGravadasBatemComAReferencia), StringComparer.Ordinal))
            return $"o filtro \"{filtro}\" não escolhe {nameof(ReproducoesGravadasBatemComAReferencia)}";
        int outros = escolhidos.Count(t => !t.StartsWith(prefixo, StringComparison.Ordinal));
        return outros > 0 ? $"o filtro \"{filtro}\" também escolhe {outros} teste(s) fora das reproduções" : null;
    }

    /// <summary>Nomes dos testes deste assembly, no formato do executor (<c>Classe.Metodo</c>).</summary>
    private static IEnumerable<string> NomesDosTestes()
        => typeof(ReproducaoTestes).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<TesteAttribute>() is not null)
                .Select(m => $"{t.Name}.{m.Name}"));

    /// <summary>Arquivos de referência da pasta, em ordem, sem as saídas obtidas de execuções que falharam.</summary>
    private static string[] ListarReferencias(string pasta)
        => [.. Directory.GetFiles(pasta, "*.txt")
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(n => !n.EndsWith(".obtido.txt", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)];

    private static string NormalizarFimDeLinha(string texto) => texto.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

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

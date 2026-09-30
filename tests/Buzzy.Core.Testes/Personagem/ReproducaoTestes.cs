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
/// regravação escreveria, byte a byte em UTF-8 sem BOM, normalizando só o fim de linha (o Git pode
/// trocar LF por CRLF). Um BOM, um byte inválido ou qualquer outra diferença falha.
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

    /// <summary>UTF-8 estrito, como a regravação escreve: sem BOM e lançando em byte inválido.</summary>
    private static readonly UTF8Encoding Utf8Estrito = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

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
            byte[] bytes = File.ReadAllBytes(arquivo);
            if (!atualizar && ProblemaDeCodificacao(bytes) is { } problema)
            {
                falhas.Add($"{nome}: {problema}");
                continue;
            }
            string conteudo = NormalizarFimDeLinha(atualizar ? Encoding.UTF8.GetString(bytes).TrimStart('﻿') : Utf8Estrito.GetString(bytes));
            string[] linhas = conteudo.Split('\n');
            string[] cabecalho = [.. linhas.TakeWhile(l => l.StartsWith('#') || l.Length == 0)];
            (ConfiguracaoDoNucleo cfg, ulong semente) = LerCabecalho(cabecalho, arquivo);

            IReadOnlyList<string> obtido = Gravacao.Reproduzir(cfg, semente, linhas, PorNome);
            string esperado = Gravacao.Juntar(cabecalho.Concat(obtido));

            if (atualizar)
            {
                File.WriteAllText(arquivo, esperado, Utf8Estrito);
                Console.WriteLine($"         referência regravada ({VariavelDeAtualizacao}=1): {arquivo}");
                continue;
            }

            if (Diferenca(bytes, esperado) is { } diferenca)
            {
                string obtidoEm = Path.Combine(AppContext.BaseDirectory, "Referencias", Path.ChangeExtension(nome, ".obtido.txt"));
                Directory.CreateDirectory(Path.GetDirectoryName(obtidoEm)!);
                File.WriteAllText(obtidoEm, esperado, Utf8Estrito);
                falhas.Add($"{nome}, {diferenca} (saída completa em {obtidoEm})");
            }
        }
        if (falhas.Count > 0) Afirmar.Falhar(string.Join(Environment.NewLine + "         ", falhas));
    }

    // Item S: a comparação normaliza só o fim de linha; BOM, byte inválido, espaço no fim da linha, linha a mais ou a menos e a falta da quebra final falham.
    [Teste]
    public static void ComparacaoDasReferencias_NormalizaSoOFimDeLinha()
    {
        const string Esperado = "# semente: 42\n> Tick\n= Idle ancora=(1,2)\n";
        byte[] Bytes(string texto) => Utf8Estrito.GetBytes(texto);

        Afirmar.Nulo(Diferenca(Bytes(Esperado), Esperado), "LF, igual à regravação");
        Afirmar.Nulo(Diferenca(Bytes(Esperado.Replace("\n", "\r\n", StringComparison.Ordinal)), Esperado), "CRLF do Git no Windows");
        Afirmar.Nulo(Diferenca(Bytes(Esperado.Replace('\n', '\r')), Esperado), "CR sozinho");

        Afirmar.Contem("BOM", Diferenca([0xEF, 0xBB, 0xBF, .. Bytes(Esperado)], Esperado), "BOM UTF-8 no início");
        Afirmar.Contem("UTF-8", Diferenca([.. Bytes("# semente: 42\n> Tick\n= Idle ancora=(1,2)"), 0xFF, 0x0A], Esperado), "byte inválido");
        Afirmar.Contem("linha 2", Diferenca(Bytes("# semente: 42\n> Tick \n= Idle ancora=(1,2)\n"), Esperado), "espaço no fim da linha");
        Afirmar.Contem("linha 3", Diferenca(Bytes("# semente: 42\n> Tick\n# comentário\n= Idle ancora=(1,2)\n"), Esperado), "linha a mais no meio");
        Afirmar.Contem("linha 5", Diferenca(Bytes(Esperado + "\n"), Esperado), "linha em branco a mais no fim");
        Afirmar.NaoNulo(Diferenca(Bytes(Esperado.TrimEnd('\n')), Esperado), "sem a quebra de linha final");
        Afirmar.NaoNulo(Diferenca(Bytes(Esperado.Replace("(1,2)", "(1,3)", StringComparison.Ordinal)), Esperado), "valor diferente");
    }

    // Item S: as referências vêm da pasta-fonte mesmo quando o compilador mapeia os caminhos (PathMap ou ContinuousIntegrationBuild gravam "/_/..." no [CallerFilePath]).
    [Teste]
    public static void PastaDasFontes_ComCaminhoMapeado_AchaPelaSaidaDoBuild()
    {
        string real = PastaDasFontes();
        Afirmar.Verdadeiro(Directory.Exists(Path.Combine(real, "Referencias")), $"a pasta-fonte tem as referências: {real}");
        string mapeado = "/_/tests/Buzzy.Core.Testes/Personagem/ReproducaoTestes.cs";
        Afirmar.Igual(Path.GetFullPath(real), Path.GetFullPath(Afirmar.NaoNulo(AcharPastaDasFontes(mapeado, AppContext.BaseDirectory), "caminho mapeado")), "pela saída do build");
        Afirmar.Nulo(AcharPastaDasFontes(mapeado, Path.GetPathRoot(AppContext.BaseDirectory)!), "sem a fonte e sem o projeto acima da saída: nenhuma pasta");
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

    /// <summary>
    /// Nulo se o arquivo é exatamente o texto esperado em UTF-8 sem BOM, normalizando só o fim de
    /// linha; senão, o problema de codificação ou a primeira linha diferente.
    /// </summary>
    private static string? Diferenca(byte[] arquivo, string esperado)
    {
        if (ProblemaDeCodificacao(arquivo) is { } problema) return problema;
        string conteudo = NormalizarFimDeLinha(Utf8Estrito.GetString(arquivo));
        if (string.Equals(conteudo, esperado, StringComparison.Ordinal)) return null;
        string[] noArquivo = conteudo.Split('\n'), reproduzido = esperado.Split('\n');
        int diferente = PrimeiraDiferenca(noArquivo, reproduzido);
        return $"linha {diferente + 1}: no arquivo <{Linha(noArquivo, diferente)}>, reproduzido <{Linha(reproduzido, diferente)}>";
    }

    /// <summary>BOM ou UTF-8 inválido, que a leitura comum esconderia: a regravação escreve UTF-8 sem BOM.</summary>
    private static string? ProblemaDeCodificacao(byte[] arquivo)
    {
        if (arquivo.AsSpan().StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF])) return "BOM UTF-8 no início (a regravação escreve sem BOM)";
        try
        {
            _ = Utf8Estrito.GetString(arquivo);
            return null;
        }
        catch (DecoderFallbackException e)
        {
            return $"UTF-8 inválido: {e.Message}";
        }
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
                case "movimento":
                    cfg = cfg with { Movimento = valor == "sim" };
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

    /// <summary>Pasta-fonte deste projeto de testes (a que tem o .csproj e <c>Referencias/</c>).</summary>
    private static string PastaDasFontes([CallerFilePath] string caminho = "")
        => AcharPastaDasFontes(caminho, AppContext.BaseDirectory)
           ?? throw new FalhaDeAfirmacao($"pasta-fonte do projeto não encontrada: nem acima de {caminho} nem acima de {AppContext.BaseDirectory}");

    /// <summary>
    /// A pasta do arquivo-fonte, se ela existe e tem o .csproj do projeto (o caso normal); senão, a
    /// primeira pasta acima da saída do build que o tenha, para quando o compilador mapeou os
    /// caminhos (PathMap ou ContinuousIntegrationBuild gravam "/_/..." no [CallerFilePath]). Nulo se
    /// nenhuma das duas achar.
    /// </summary>
    private static string? AcharPastaDasFontes(string arquivoFonte, string saidaDoBuild)
    {
        string projeto = typeof(ReproducaoTestes).Assembly.GetName().Name + ".csproj";
        string? pelaFonte = Path.GetDirectoryName(Path.GetDirectoryName(arquivoFonte));
        if (!string.IsNullOrEmpty(pelaFonte) && File.Exists(Path.Combine(pelaFonte, projeto))) return pelaFonte;
        for (DirectoryInfo? pasta = new(saidaDoBuild); pasta is not null; pasta = pasta.Parent)
        {
            if (File.Exists(Path.Combine(pasta.FullName, projeto))) return pasta.FullName;
        }
        return null;
    }
}

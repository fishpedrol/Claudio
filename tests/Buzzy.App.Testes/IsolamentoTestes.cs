using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Buzzy.App.Composicao;
using Buzzy.App.Plataforma;
using Buzzy.App.Testes.Integracao;
using Buzzy.Testes;
using Microsoft.Win32.SafeHandles;
using ProgramaDoBuzzy = Buzzy.App.Programa;

namespace Buzzy.App.Testes;

/// <summary>
/// Isolamento dos testes (Fase 5): todo Buzzy aberto por um teste ou por uma ferramenta usa um perfil de teste
/// (<c>--perfil-de-teste</c>), nunca as configurações reais do usuário. Tudo sem abrir o Buzzy: a leitura das
/// opções (também escritas de outro jeito), os lançadores (os argumentos que montam e os únicos lugares que abrem o
/// Buzzy), a limpeza da pasta do perfil (numa pasta temporária), a regra do aplicativo que escolhe a pasta das
/// configurações e o instante das linhas do log, usado para medir durações.
/// </summary>
internal sealed class IsolamentoTestes : IDisposable
{
    private readonly string _raiz = Directory.CreateTempSubdirectory("buzzy-isolamento-").FullName;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_raiz, recursive: true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"         limpeza: a pasta temporária {_raiz} ficou para o sistema apagar ({e.GetType().Name})");
        }
    }

    // ------------------------------------------------------------------ linha de comando do Buzzy

    [Teste]
    public void LerOpcoes_SemPerfil_ComoAntes()
    {
        Afirmar.Igual(OpcoesDaAplicacao.Padrao, ProgramaDoBuzzy.LerOpcoes([]));
        Afirmar.Igual(new OpcoesDaAplicacao(true, 42UL), ProgramaDoBuzzy.LerOpcoes(["--diagnostico", "--pausado", "--semente", "42"]));
        OpcoesDaAplicacao opcoes = ProgramaDoBuzzy.LerOpcoes(["--pausado"]);
        Afirmar.Nulo(opcoes.PerfilDeTeste);
        Afirmar.Falso(opcoes.PersistenciaDesligada);
    }

    [Teste]
    public void LerOpcoes_PerfilValido_IsolaSemDesligar()
    {
        // "perfil-de-teste" também é um nome válido: sem traço ou barra na frente, não é outra grafia da opção.
        foreach (string nome in new[] { "integracao", "verificacao", "desempenho", "a-1", "perfil-de-teste", new string('z', 32) })
        {
            OpcoesDaAplicacao opcoes = ProgramaDoBuzzy.LerOpcoes(["--diagnostico", "--perfil-de-teste", nome, "--pausado", "--semente", "7"]);
            Afirmar.Igual(nome, opcoes.PerfilDeTeste);
            Afirmar.Falso(opcoes.PersistenciaDesligada, nome);
            Afirmar.Verdadeiro(opcoes.MovimentoPausado, nome);
            Afirmar.Igual<ulong?>(7, opcoes.Semente, nome);
        }
    }

    [Teste]
    public void LerOpcoes_PerfilInvalidoOuSemNome_DesligaAPersistencia()
    {
        // Falha fechada: sem nome ou com um nome que sairia da pasta de testes, nada de pasta real.
        string[][] casos =
        [
            ["--perfil-de-teste"],
            ["--perfil-de-teste", ""],
            ["--perfil-de-teste", ".."],
            ["--perfil-de-teste", @"..\..\Buzzy"],
            ["--perfil-de-teste", "../x"],
            ["--perfil-de-teste", @"C:\Users"],
            ["--perfil-de-teste", "Integracao"],
            ["--perfil-de-teste", "con"],
            ["--perfil-de-teste", "a b"],
            ["--perfil-de-teste", new string('z', 33)],
            ["--perfil-de-teste", "--pausado"],
        ];
        foreach (string[] argumentos in casos)
        {
            string rotulo = string.Join(" ", argumentos.Select(a => $"[{a}]"));
            OpcoesDaAplicacao opcoes = ProgramaDoBuzzy.LerOpcoes(argumentos);
            Afirmar.Nulo(opcoes.PerfilDeTeste, rotulo);
            Afirmar.Verdadeiro(opcoes.PersistenciaDesligada, rotulo);
        }

        // O resto da linha de comando continua valendo.
        OpcoesDaAplicacao comOutras = ProgramaDoBuzzy.LerOpcoes(["--perfil-de-teste", "--pausado", "--semente", "7"]);
        Afirmar.Verdadeiro(comOutras.PersistenciaDesligada);
        Afirmar.Verdadeiro(comOutras.MovimentoPausado);
        Afirmar.Igual<ulong?>(7, comOutras.Semente);
    }

    [Teste]
    public void LerOpcoes_OutraGrafiaDaOpcao_DesligaAPersistencia()
    {
        // Quem escreveu a opção de outro jeito quis isolar o Buzzy: em vez de ignorá-la e cair na pasta real do
        // usuário, a persistência fica desligada. Também quando a grafia exata vem junto, com um nome válido.
        string[][] casos =
        [
            ["--perfil-de-teste=integracao"],
            ["--perfil-de-teste:integracao"],
            ["--Perfil-De-Teste", "integracao"],
            ["--PERFIL-DE-TESTE", "integracao"],
            ["/perfil-de-teste", "integracao"],
            ["-perfil-de-teste", "integracao"],
            ["---perfil-de-teste", "integracao"],
            ["--perfil_de_teste", "integracao"],
            ["--perfildeteste", "integracao"],
            ["—perfil-de-teste", "integracao"],
            ["--diagnostico", "--perfil-de-teste", "integracao", "--perfil-de-teste=outro"],
            ["/Perfil-de-teste=integracao", "--pausado"],
        ];
        foreach (string[] argumentos in casos)
        {
            string rotulo = string.Join(" ", argumentos.Select(a => $"[{a}]"));
            OpcoesDaAplicacao opcoes = ProgramaDoBuzzy.LerOpcoes(argumentos);
            Afirmar.Nulo(opcoes.PerfilDeTeste, rotulo);
            Afirmar.Verdadeiro(opcoes.PersistenciaDesligada, rotulo);
        }
        Afirmar.Verdadeiro(ProgramaDoBuzzy.LerOpcoes(["/perfil-de-teste", "x", "--pausado"]).MovimentoPausado, "o resto continua valendo");

        // Não são a opção: a grafia exata, um nome sem traço nem barra na frente e outras opções.
        foreach (string argumento in new[] { "--perfil-de-teste", "perfil-de-teste", "integracao", "--pausado", "--semente", "--perfil", "--perfil-de-energia", "-", "/", "" })
            Afirmar.Falso(ProgramaDoBuzzy.ParecidoComAOpcaoDoPerfil(argumento), $"[{argumento}]");
    }

    // ------------------------------------------------------------------ lançadores

    [Teste]
    public void BuzzyEmTeste_AbreComOPerfilDeIntegracao()
    {
        List<string> argumentos = [.. BuzzyEmTeste.DescreverProcesso().ArgumentList];
        Afirmar.Sequencia(["--diagnostico", "--perfil-de-teste", PerfilDeTeste.Integracao, "--pausado"], argumentos);

        argumentos = [.. BuzzyEmTeste.DescreverProcesso(pausado: false, semente: 5, perfil: "persistencia").ArgumentList];
        Afirmar.Sequencia(["--diagnostico", "--perfil-de-teste", "persistencia", "--semente", "5"], argumentos);

        // A pasta que o teste limpa é a mesma que o Buzzy usa com esse perfil.
        string buzzy = Afirmar.NaoNulo(PastaDeDados.DoBuzzy());
        Afirmar.Igual(PastaDeDados.DoPerfilDeTeste(PerfilDeTeste.Integracao), PerfilDeTeste.Pasta(PerfilDeTeste.Integracao, buzzy));
    }

    [Teste]
    public void Verificacao_AbreOBuzzySoComOPerfilDeTeste()
    {
        string pasta = Path.Combine(Caminhos.Raiz, "tests", "Buzzy.Verificacao");
        string fontes = string.Join("\n", FontesCs(pasta).Select(File.ReadAllText));

        // Todo processo que a verificação inicia é Process.Start(psi), e todo psi é o do receptor (o próprio
        // executável) ou o do descritor com o perfil; nenhum ProcessStartInfo montado de outro jeito.
        Afirmar.Sequencia(["psi"], Regex.Matches(fontes, @"Process\.Start\(\s*([^)]*?)\s*\)").Select(m => m.Groups[1].Value).Distinct(), "o que Process.Start recebe");
        string[] origens = [.. Regex.Matches(fontes, @"\bpsi\s*=\s*(PerfilDaVerificacao\.Descrever\(\w+|new ProcessStartInfo\(\w+\))").Select(m => m.Groups[1].Value)];
        Afirmar.Igual(Regex.Matches(fontes, @"\bpsi\s*=").Count, origens.Length, "todo psi vem de um dos dois descritores");
        Afirmar.Sequencia(["PerfilDaVerificacao.Descrever(_exeBuzzy", "new ProcessStartInfo(_exeProprio)", "new ProcessStartInfo(exeBuzzy)"],
            origens.Distinct().Order(StringComparer.Ordinal), "de onde vem cada psi");
        Afirmar.Igual(0, Regex.Matches(fontes, @"new\s+ProcessStartInfo\s*(\(\s*\)|\{)|\.StartInfo\b|UseShellExecute\s*=\s*true").Count, "ProcessStartInfo sem o executável, StartInfo ou shell");
        string[] alvos = [.. Regex.Matches(fontes, @"new ProcessStartInfo\((\w+)\)").Select(m => m.Groups[1].Value).Distinct().Order(StringComparer.Ordinal)];
        Afirmar.Sequencia(["_exeProprio", "exeBuzzy"], alvos, "processos que a verificação descreve");
        Afirmar.Verdadeiro(Regex.Matches(fontes, @"PerfilDaVerificacao\.Descrever\(_exeBuzzy").Count >= 3, "abrir, segunda instância e abrir com a autonomia");

        string apoio = File.ReadAllText(Path.Combine(pasta, "Apoio.cs"));
        Afirmar.Contem("psi.ArgumentList.Add(\"--perfil-de-teste\");", apoio);
        Afirmar.Contem("internal const string Nome = \"verificacao\";", apoio);
        Afirmar.Verdadeiro(PastaDeDados.NomeDePerfilValido("verificacao"), "o nome do perfil da verificação é válido");

        // A pasta do perfil e a limpeza dela não são uma cópia: são as do Buzzy e as destes testes, compiladas junto.
        string projeto = File.ReadAllText(Path.Combine(pasta, "Buzzy.Verificacao.csproj"));
        Afirmar.Contem(@"<Compile Include=""..\..\src\Buzzy.App\Plataforma\PastaDeDados.cs""", projeto);
        Afirmar.Contem(@"<Compile Include=""..\Buzzy.App.Testes\Integracao\PerfilDeTeste.cs""", projeto);
        Afirmar.Contem("PerfilDeTeste.Limpar(Nome);", apoio);
        Afirmar.Igual(0, Regex.Matches(fontes, @"Directory\.Delete\(|""testes""").Count, "a verificação não monta o caminho nem apaga pastas por conta própria");
    }

    [Teste]
    public void TestesDoAplicativo_AbremOBuzzySoPeloBuzzyEmTeste()
    {
        // Nos testes do aplicativo, o Buzzy.exe só é iniciado por BuzzyEmTeste, sempre com um perfil de teste
        // (BuzzyEmTeste_AbreComOPerfilDeIntegracao); o outro processo que eles iniciam é o próprio executável de
        // testes, como gravador (ArquivoDeConfiguracoesTestes.MatarDuranteGravacoes_NuncaDeixaIlegivel).
        string pasta = Path.Combine(Caminhos.Raiz, "tests", "Buzzy.App.Testes");
        Dictionary<string, string> fontes = FontesCs(pasta)
            .Where(f => Path.GetFileName(f) != "IsolamentoTestes.cs")
            .ToDictionary(f => Path.GetRelativePath(pasta, f), File.ReadAllText, StringComparer.Ordinal);
        string[] Onde(string padrao) => [.. fontes.Where(f => Regex.IsMatch(f.Value, padrao)).Select(f => f.Key).Order(StringComparer.Ordinal)];

        Afirmar.Sequencia(["ArquivoDeConfiguracoesTestes.cs", @"Integracao\BuzzyEmTeste.cs"], Onde(@"Process\.Start\("), "quem inicia processos");
        Afirmar.Sequencia(["Caminhos.cs", "CaminhosTestes.cs", @"Integracao\BuzzyEmTeste.cs"], Onde(@"\bExeDoBuzzy\("), "quem usa o caminho do Buzzy.exe");
        Afirmar.Sequencia([], Onde(@"new\s+ProcessStartInfo\s*(\(\s*\)|\{)|\.StartInfo\b|UseShellExecute\s*=\s*true"), "ProcessStartInfo sem o executável, StartInfo ou shell");

        string buzzyEmTeste = fontes[@"Integracao\BuzzyEmTeste.cs"];
        Afirmar.Igual(1, Regex.Matches(buzzyEmTeste, @"Process\.Start\(").Count, "BuzzyEmTeste inicia o Buzzy num lugar só");
        Afirmar.Contem("Process.Start(DescreverProcesso(pausado, semente, perfil))", buzzyEmTeste);
        Afirmar.Igual(1, Regex.Matches(buzzyEmTeste, @"new ProcessStartInfo\(").Count, "e o descreve num lugar só");
        Afirmar.Contem("new ProcessStartInfo(Caminhos.ExeDoBuzzy())", buzzyEmTeste);

        string arquivo = fontes["ArquivoDeConfiguracoesTestes.cs"];
        Afirmar.Igual(1, Regex.Matches(arquivo, @"Process\.Start\(").Count, "um processo filho só");
        Afirmar.Contem("new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, \"Buzzy.App.Testes.exe\"))", arquivo);
    }

    [Teste]
    public void MedirDesempenho_AbreOBuzzyComOPerfilDeTesteLimpo()
    {
        string[] linhas = File.ReadAllLines(Path.Combine(Caminhos.Raiz, "tools", "medir-desempenho.ps1"));
        int[] Linhas(string trecho) => [.. linhas.Select((l, i) => (l, i)).Where(x => x.l.Contains(trecho, StringComparison.Ordinal)).Select(x => x.i)];
        string[] Atribuicoes(string variavel) => [.. linhas.Select(l => l.Trim()).Where(l => l.StartsWith(variavel + " = ", StringComparison.Ordinal))];

        Afirmar.Sequencia(["$perfilDeTeste = 'desempenho'"], Atribuicoes("$perfilDeTeste"), "o perfil, numa atribuição só");
        Afirmar.Verdadeiro(PastaDeDados.NomeDePerfilValido("desempenho"), "nome de perfil válido");
        Afirmar.Sequencia(["$argumentosDoBuzzy = '--diagnostico --perfil-de-teste ' + $perfilDeTeste"], Atribuicoes("$argumentosDoBuzzy"), "uma só atribuição; o resto só acrescenta");
        int[] aberturas = Linhas("[Diagnostics.Process]::Start($infoPartida)");
        Afirmar.Igual(1, aberturas.Length, "o Buzzy é aberto num lugar só");
        int abrir = aberturas[0];
        Afirmar.Verdadeiro(Linhas("$infoPartida.Arguments = $argumentosDoBuzzy").Any(i => i < abrir), "o Buzzy medido recebe esses argumentos");

        // Antes de abrir, a pasta do perfil é apagada, entre duas conferências de que nenhum Buzzy está aberto: a
        // medição parte sempre da posição inicial, sem a posição gravada pela anterior (mesma semente, mesmo começo).
        int[] limpezas = Linhas("LimparPerfilDeTeste $");
        Afirmar.Igual(1, limpezas.Length, "uma limpeza");
        int limpar = limpezas[0];
        int[] conferencias = Linhas("@(ProcessosBuzzyAbertos)");
        Afirmar.Verdadeiro(limpar < abrir && conferencias.Any(i => i < limpar) && conferencias.Any(i => i > limpar && i < abrir),
            "limpa antes de abrir, com nenhum Buzzy aberto antes e depois");

        // A limpeza é a dos testes (PerfilDeTeste.Limpar): só a pasta testes\<perfil> e sem seguir junção nem link.
        int[] definicoes = Linhas("function LimparPerfilDeTeste(");
        Afirmar.Igual(1, definicoes.Length, "a função de limpeza");
        string funcao = string.Join("\n", linhas.Skip(definicoes[0]).TakeWhile(l => l != "}"));
        foreach (string trecho in new[] { "'Buzzy'", "'testes'", "ReparsePoint", "Abortar", "[IO.Directory]::Delete($pasta, $true)" })
            Afirmar.Contem(trecho, funcao, "LimparPerfilDeTeste");
    }

    [Teste]
    public void Aplicativo_SoCriaOArquivoDeConfiguracoesPelaRegraDaPasta()
    {
        // Em src\, o arquivo de configurações só nasce de ArquivoDeConfiguracoes.DaExecucao, com a pasta de
        // PastaDeDados.DasConfiguracoes (a falha fechada, numa regra só, testada em ArquivoDeConfiguracoesTestes); e a
        // pasta real do Buzzy só é pedida por essa regra e pelo log de diagnóstico. Um "DoPerfilDeTeste(p) ?? DoBuzzy()"
        // em outro lugar faria o isolamento dos testes falhar aberto.
        string src = Path.Combine(Caminhos.Raiz, "src");
        foreach (string arquivo in FontesCs(src))
        {
            string nome = Path.GetRelativePath(src, arquivo);
            string texto = File.ReadAllText(arquivo);
            int criacoes = Regex.Matches(texto, @"new\s+ArquivoDeConfiguracoes\s*\(").Count
                + Regex.Matches(texto, @"ArquivoDeConfiguracoes\??\s+\w+\s*=\s*new\s*\(").Count;
            Afirmar.Igual(nome == @"Buzzy.App\Plataforma\ArquivoDeConfiguracoes.cs" ? 1 : 0, criacoes, $"{nome}: onde o arquivo de configurações é criado");
            if (nome is not (@"Buzzy.App\Plataforma\PastaDeDados.cs" or @"Buzzy.App\Plataforma\Diagnostico.cs"))
                Afirmar.Igual(0, Regex.Matches(texto, @"\bDoBuzzy\s*\(").Count, $"{nome}: pede a pasta real do Buzzy");
        }
    }

    // ------------------------------------------------------------------ limpeza da pasta do perfil

    [Teste]
    public void PerfilDeTeste_LimparApagaSoAPastaDoPerfil()
    {
        string buzzy = Path.Combine(_raiz, "Buzzy");
        string perfil = Path.Combine(buzzy, "testes", "integracao");
        string[] vizinhos =
        [
            Path.Combine(buzzy, "settings.json"),
            Path.Combine(buzzy, "diagnostico.log"),
            Path.Combine(buzzy, "testes", "verificacao", "settings.json"),
            Path.Combine(buzzy, "testes", "solto.txt"),
            Path.Combine(_raiz, "integracao", "settings.json"),
        ];
        foreach (string arquivo in vizinhos) Escrever(arquivo, "vizinho");
        Escrever(Path.Combine(perfil, "settings.json"), "do perfil");
        Escrever(Path.Combine(perfil, "sub", "outro.txt"), "do perfil");

        Afirmar.Verdadeiro(PerfilDeTeste.Limpar("integracao", buzzy), "apagou");
        Afirmar.Falso(Directory.Exists(perfil), "a pasta do perfil foi apagada inteira");
        foreach (string arquivo in vizinhos)
            Afirmar.Verdadeiro(File.Exists(arquivo), $"intacto: {Path.GetRelativePath(_raiz, arquivo)}");

        Afirmar.Falso(PerfilDeTeste.Limpar("integracao", buzzy), "sem a pasta, nada a apagar");
        Afirmar.Falso(PerfilDeTeste.Limpar("persistencia", buzzy), "perfil que nunca existiu");
    }

    [Teste]
    public void PerfilDeTeste_LimparRecusaNomeInvalidoSemApagarNada()
    {
        string buzzy = Path.Combine(_raiz, "Buzzy");
        string[] arquivos =
        [
            Path.Combine(buzzy, "settings.json"),
            Path.Combine(buzzy, "testes", "integracao", "settings.json"),
            Path.Combine(_raiz, "fora.txt"),
        ];
        foreach (string arquivo in arquivos) Escrever(arquivo, "x");

        foreach (string nome in new[] { "", ".", "..", @"..\..", "../..", @"integracao\..\..", "a/b", "Integracao", "con", @"C:\" })
            Afirmar.Lanca<ArgumentException>(() => PerfilDeTeste.Limpar(nome, buzzy), $"\"{nome}\"");
        foreach (string arquivo in arquivos)
            Afirmar.Verdadeiro(File.Exists(arquivo), $"intacto: {Path.GetRelativePath(_raiz, arquivo)}");
    }

    [Teste]
    public void PerfilDeTeste_LimparRecusaJuncaoNoCaminhoSemApagarNadaDeFora()
    {
        // Uma junção, que qualquer usuário cria sem privilégio, na pasta do Buzzy, em testes ou na própria pasta do
        // perfil levaria a limpeza para fora: a pasta do perfil, resolvida através dela, é uma pasta comum, e apagá-la
        // apagaria o que está do outro lado. A limpeza recusa sem apagar nada. É a mesma limpeza da verificação de tela.
        foreach (string ondeFicaAJuncao in new[] { "Buzzy", @"Buzzy\testes", @"Buzzy\testes\integracao" })
        {
            string caso = Path.Combine(_raiz, $"juncao-{ondeFicaAJuncao.Count(c => c == '\\')}");
            string buzzy = Path.Combine(caso, "Buzzy");
            string juncao = Path.Combine(caso, ondeFicaAJuncao);
            string fora = Path.Combine(caso, "fora");

            // Do outro lado da junção, o que estaria no lugar dela: a pasta do perfil fica fora da pasta do Buzzy.
            string perfilDoOutroLado = Path.GetFullPath(Path.Combine(fora, Path.GetRelativePath(juncao, Path.Combine(buzzy, "testes", "integracao"))));
            string importante = Path.Combine(perfilDoOutroLado, "importante.txt");
            Escrever(importante, "fora da pasta do Buzzy");
            Directory.CreateDirectory(Path.GetDirectoryName(juncao)!);
            CriarJuncao(juncao, fora);
            try
            {
                Afirmar.Verdadeiro(File.Exists(Path.Combine(buzzy, "testes", "integracao", "importante.txt")), $"{ondeFicaAJuncao}: a pasta do perfil chega ao outro lado");
                InvalidOperationException recusa = Afirmar.Lanca<InvalidOperationException>(() => PerfilDeTeste.Limpar("integracao", buzzy), ondeFicaAJuncao);
                Afirmar.Contem("junção", recusa.Message, ondeFicaAJuncao);
                Afirmar.Verdadeiro(File.Exists(importante), $"{ondeFicaAJuncao}: nada do outro lado foi apagado");
            }
            finally
            {
                // Apagar a junção (sem recursão) remove só ela, nunca o outro lado.
                if (Directory.Exists(juncao)) Directory.Delete(juncao);
            }
            Afirmar.Verdadeiro(File.Exists(importante), $"{ondeFicaAJuncao}: desfeita a junção, o outro lado continua");
        }
    }

    // ------------------------------------------------------------------ log

    [Teste]
    public void EventoDoLog_InstanteLidoDoPrefixo()
    {
        EventoDoLog lido = Afirmar.NaoNulo(EventoDoLog.Ler("[01:02:03.456] BUZZY|CONFIG|lido=principal"));
        Afirmar.Igual(new TimeSpan(0, 1, 2, 3, 456), lido.Instante);
        EventoDoLog gravado = Afirmar.NaoNulo(EventoDoLog.Ler("[01:02:05.006] BUZZY|CONFIG|gravado=sim"));
        Afirmar.Igual(TimeSpan.FromMilliseconds(1550), gravado.Instante - lido.Instante);

        foreach (string sem in new[] { "BUZZY|X|a=1", "[1:2] BUZZY|X|a=1", "[01:02:03] BUZZY|X|a=1", "[] BUZZY|X|a=1", "x[01:02:03.456] BUZZY|X|a=1" })
        {
            EventoDoLog evento = Afirmar.NaoNulo(EventoDoLog.Ler(sem));
            Afirmar.Lanca<FormatException>(() => _ = evento.Instante, sem);
        }
    }

    private static void Escrever(string arquivo, string texto)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(arquivo)!);
        File.WriteAllText(arquivo, texto);
    }

    /// <summary>
    /// Cria <paramref name="juncao"/>, que não pode existir, como junção (ponto de montagem) para a pasta
    /// <paramref name="alvo"/>, pelo FSCTL_SET_REPARSE_POINT, como o <c>mklink /J</c>: sem privilégio e sem o modo de
    /// desenvolvedor, ao contrário do link simbólico. Só neste teste, fora do produto.
    /// </summary>
    private static void CriarJuncao(string juncao, string alvo)
    {
        const uint EtiquetaDePontoDeMontagem = 0xA0000003;   // IO_REPARSE_TAG_MOUNT_POINT
        const uint DefinirPontoDeReanalise = 0x000900A4;     // FSCTL_SET_REPARSE_POINT
        const uint EscritaGenerica = 0x40000000;             // GENERIC_WRITE
        const uint AbrirExistente = 3;                       // OPEN_EXISTING
        const uint SemanticaDeBackupEPontoDeReanalise = 0x02000000 | 0x00200000; // FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT

        string exibido = Path.GetFullPath(alvo);
        string substituto = @"\??\" + exibido;
        byte[] nomes = Encoding.Unicode.GetBytes(substituto + "\0" + exibido + "\0");

        // REPARSE_DATA_BUFFER do ponto de montagem: etiqueta, tamanho dos dados e reservado; depois os deslocamentos e
        // tamanhos (em bytes, sem o nulo) do nome substituto e do exibido, e os dois nomes.
        byte[] buffer = new byte[16 + nomes.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0), EtiquetaDePontoDeMontagem);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(4), checked((ushort)(8 + nomes.Length)));
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(8), 0);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(10), checked((ushort)(substituto.Length * 2)));
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(12), checked((ushort)(substituto.Length * 2 + 2)));
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(14), checked((ushort)(exibido.Length * 2)));
        nomes.CopyTo(buffer, 16);

        Directory.CreateDirectory(juncao);
        using SafeFileHandle pasta = AbrirParaReanalise(juncao, EscritaGenerica, 0, 0, AbrirExistente, SemanticaDeBackupEPontoDeReanalise, 0);
        if (pasta.IsInvalid)
            throw new IOException($"CreateFileW falhou (erro {Marshal.GetLastPInvokeError()}).");
        if (!ControlarDispositivo(pasta, DefinirPontoDeReanalise, buffer, buffer.Length, 0, 0, out _, 0))
            throw new IOException($"FSCTL_SET_REPARSE_POINT falhou (erro {Marshal.GetLastPInvokeError()}).");
        Afirmar.Verdadeiro((File.GetAttributes(juncao) & FileAttributes.ReparsePoint) != 0, "a junção foi criada");
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle AbrirParaReanalise(string nome, uint acesso, uint compartilhamento, nint seguranca, uint disposicao, uint atributos, nint modelo);

    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ControlarDispositivo(SafeFileHandle dispositivo, uint codigo, byte[] entrada, int tamanhoDaEntrada, nint saida, int tamanhoDaSaida, out int devolvidos, nint sobreposto);

    /// <summary>Os fontes C# da pasta e das subpastas, sem os gerados pelo build (bin e obj), em ordem ordinal.</summary>
    private static string[] FontesCs(string pasta)
        => [.. Directory.GetFiles(pasta, "*.cs", SearchOption.AllDirectories)
            .Where(f => !Path.GetRelativePath(pasta, f).Split(Path.DirectorySeparatorChar).Any(parte => parte is "bin" or "obj"))
            .Order(StringComparer.Ordinal)];
}

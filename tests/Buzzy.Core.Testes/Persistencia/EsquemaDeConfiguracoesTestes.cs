using System.Globalization;
using System.Text;
using System.Text.Json;
using Buzzy.Core.Persistencia;
using Buzzy.Core.Personagem;
using Buzzy.Core.Testes.Personagem;
using Buzzy.Testes;
using static Buzzy.Core.Testes.TopologiasDeExemplo;

namespace Buzzy.Core.Testes.Persistencia;

/// <summary>
/// Esquema v1 do settings.json (Fase 5, desenho de persistência, seções 4.1 a 4.3): o formato escrito,
/// byte a byte, a leitura tolerante campo a campo e os casos ilegíveis. A amostra de referência
/// (<c>Amostras/settings-v1.json</c>) é lida da pasta-fonte, como as reproduções gravadas.
/// </summary>
internal static class EsquemaDeConfiguracoesTestes
{
    private static readonly TamanhoDip Sprite = new(128, 128);

    // ---------------------------------------------------------------- a amostra v1

    // A posição S2 (secundário à esquerda, 25% da área útil, no chão) com as preferências padrão
    // escreve exatamente a amostra: um formato alterado por acidente falha aqui. Como nas
    // referências, só o fim de linha é normalizado (o Git pode trocar LF por CRLF na amostra).
    [Teste]
    public static void Escrever_ExemploS2_IgualAAmostraV1()
    {
        byte[] escrito = EsquemaDeConfiguracoes.Escrever(new ConfiguracoesSalvas(PosicaoS2(), Preferencias.Padrao));

        Afirmar.Igual(Amostra(), Encoding.UTF8.GetString(escrito), "texto escrito");
        Afirmar.Falso(escrito.AsSpan().StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF]), "sem BOM");
        Afirmar.Falso(escrito.Contains((byte)'\r'), "fim de linha \\n, sem \\r");
        Afirmar.Igual((byte)'\n', escrito[^1], "termina com \\n");
    }

    // A amostra, lida do disco como está (com CRLF, se o Git a converteu), volta com os valores exatos.
    [Teste]
    public static void Ler_AmostraV1_DevolveOsValores()
    {
        LeituraDasConfiguracoes lida = EsquemaDeConfiguracoes.Ler(File.ReadAllBytes(CaminhoDaAmostra()));

        Afirmar.Igual(SituacaoDaLeitura.Valida, lida.Situacao, "situação");
        Afirmar.Igual(1, lida.Versao, "versão");
        Afirmar.Nulo(lida.MotivoIlegivel, "motivo");
        Afirmar.Sequencia([], lida.Avisos, "avisos");
        PosicaoDoPersonagem p = Afirmar.NaoNulo(lida.Configuracoes.Posicao, "posição");
        Afirmar.Igual(@"\\.\DISPLAY2", p.ChaveMonitor, "chave");
        Afirmar.Igual(new RetanguloPx(-1920, 0, 0, 1080), p.TelaDoMonitor, "tela do monitor");
        Afirmar.Igual(0.25, p.FracaoX, "fração x");
        Afirmar.Igual(1.0, p.FracaoY, "fração y");
        Afirmar.Igual(new PontoPx(-1440, 1032), p.AncoraAbsoluta, "âncora");
        Afirmar.Igual(new Preferencias(NivelDeEnergia.Media, true, true), lida.Configuracoes.Preferencias, "preferências");
    }

    // Os bytes não dependem da cultura atual: pt-BR (vírgula decimal), uma cultura com o sinal de menos
    // tipográfico (U+2212, como sv-SE com ICU) e tr-TR escrevem o mesmo arquivo que a invariante; e a
    // leitura, nessas culturas, devolve os mesmos valores.
    [Teste]
    public static void Escrever_IndependeDaCultura()
    {
        var c = new ConfiguracoesSalvas(PosicaoS2() with { FracaoY = 0.123456789012345 }, new Preferencias(NivelDeEnergia.Alta, true, false));
        byte[] invariante = Cultura.Com(CultureInfo.InvariantCulture, () => EsquemaDeConfiguracoes.Escrever(c));
        string texto = Encoding.UTF8.GetString(invariante);
        Afirmar.Contem("\"x\": -1440,", texto, "âncora negativa com hífen");
        Afirmar.Contem("\"fracaoX\": 0.25,", texto, "fração com ponto");
        Afirmar.Contem("\"fracaoY\": 0.123456789012345,", texto, "fração no formato mais curto que a reproduz");

        foreach (CultureInfo cultura in new[] { new CultureInfo("pt-BR"), Cultura.ComMenosTipografico(), new CultureInfo("tr-TR") })
        {
            Afirmar.Sequencia(invariante, Cultura.Com(cultura, () => EsquemaDeConfiguracoes.Escrever(c)), $"escrita em {cultura.Name}");
            Afirmar.Igual(c, Cultura.Com(cultura, () => EsquemaDeConfiguracoes.Ler(invariante).Configuracoes), $"leitura em {cultura.Name}");
            // Em tr-TR, "I" minúsculo é "ı": os nomes da energia continuam reconhecidos sem diferenciar maiúsculas.
            Afirmar.Igual(NivelDeEnergia.Baixa, Cultura.Com(cultura, () => Ler(ComEnergia("BAIXA")).Configuracoes.Preferencias.Energia), $"BAIXA em {cultura.Name}");
        }
    }

    // Escrever normaliza antes e nunca lança por causa do conteúdo: frações NaN, infinitas ou -0, chave
    // vazia, nula, longa demais, com controle ou com surrogate solto, coordenadas extremas, tela invertida,
    // energia fora do enum e preferências nulas saem num arquivo válido, sem aviso, igual ao das
    // configurações normalizadas (valores esperados escritos à mão).
    [Teste]
    public static void Escrever_NuncaLanca()
    {
        PosicaoDoPersonagem boa = PosicaoS2();
        ConfiguracoesSalvas Com(PosicaoDoPersonagem? p) => new(p, Preferencias.Padrao);
        (string Caso, ConfiguracoesSalvas Configuracoes, ConfiguracoesSalvas Normalizadas)[] casos =
        [
            ("fração NaN", Com(boa with { FracaoX = double.NaN }), Com(boa with { FracaoX = 0.5 })),
            ("frações infinitas", Com(boa with { FracaoX = double.PositiveInfinity, FracaoY = double.NegativeInfinity }), Com(boa with { FracaoX = 1, FracaoY = 0 })),
            ("frações fora de [0, 1]", Com(boa with { FracaoX = 7, FracaoY = -1 }), Com(boa with { FracaoX = 1, FracaoY = 0 })),
            ("fração -0", Com(boa with { FracaoX = -0.0 }), Com(boa with { FracaoX = 0 })),
            ("chave vazia", Com(boa with { ChaveMonitor = "" }), Com(null)),
            ("chave nula", Com(boa with { ChaveMonitor = null! }), Com(null)),
            ("chave de 1025 caracteres", Com(boa with { ChaveMonitor = new string('a', 1025) }), Com(null)),
            ("chave com \\u0001", Com(boa with { ChaveMonitor = "a\u0001" }), Com(null)),
            ("chave com surrogate alto solto", Com(boa with { ChaveMonitor = "a\uD800b" }), Com(null)),
            ("chave com surrogate baixo solto", Com(boa with { ChaveMonitor = "\uDC00" }), Com(null)),
            ("chave com surrogate alto no fim", Com(boa with { ChaveMonitor = "a\uD83D" }), Com(null)),
            ("chave de 1024 caracteres com aspas e barras", Com(boa with { ChaveMonitor = string.Concat(Enumerable.Repeat("\"\\", 512)) }), Com(boa with { ChaveMonitor = string.Concat(Enumerable.Repeat("\"\\", 512)) })),
            ("coordenadas extremas", Com(boa with { AncoraAbsoluta = new PontoPx(int.MinValue, int.MaxValue), TelaDoMonitor = new RetanguloPx(int.MinValue, -5, int.MaxValue, 5) }),
                Com(boa with { AncoraAbsoluta = new PontoPx(-32768, 32767), TelaDoMonitor = new RetanguloPx(-32768, -5, 32767, 5) })),
            ("tela invertida", Com(boa with { TelaDoMonitor = new RetanguloPx(10, 0, 0, 10) }), Com(boa with { TelaDoMonitor = null })),
            ("tela vazia depois de presa", Com(boa with { TelaDoMonitor = new RetanguloPx(40000, 0, 50000, 10) }), Com(boa with { TelaDoMonitor = null })),
            ("energia 7", new(boa, new Preferencias((NivelDeEnergia)7, false, false)), new(boa, new Preferencias(NivelDeEnergia.Media, false, false))),
            ("energia -1", new(null, new Preferencias((NivelDeEnergia)(-1), true, false)), new(null, new Preferencias(NivelDeEnergia.Media, true, false))),
            ("preferências nulas", new(boa, null!), new(boa, Preferencias.Padrao)),
        ];
        foreach ((string caso, ConfiguracoesSalvas configuracoes, ConfiguracoesSalvas normalizadas) in casos)
        {
            Afirmar.Igual(normalizadas, EsquemaDeConfiguracoes.Normalizar(configuracoes), $"{caso}: normalizadas");
            byte[] bytes = EsquemaDeConfiguracoes.Escrever(configuracoes);
            LeituraDasConfiguracoes lida = EsquemaDeConfiguracoes.Ler(bytes);
            Afirmar.Igual(SituacaoDaLeitura.Valida, lida.Situacao, $"{caso}: situação");
            Afirmar.Sequencia([], lida.Avisos, $"{caso}: sem aviso");
            Afirmar.Igual(normalizadas, lida.Configuracoes, $"{caso}: lida");
            Afirmar.Sequencia(EsquemaDeConfiguracoes.Escrever(normalizadas), bytes, $"{caso}: Escrever(Normalizar(c)) = Escrever(c)");
        }
    }

    // Ida e volta com 5.000 configurações aleatórias, muitas inválidas (semente fixa, como os testes de
    // propriedade): o arquivo cabe no limite, é UTF-8 sem BOM terminado em \n, é lido como válido e sem
    // aviso, e devolve exatamente as configurações normalizadas; escrever é idempotente, e a normalização
    // só produz valores graváveis.
    [Teste]
    public static void IdaEVolta_5000Aleatorias_VoltamNormalizadas()
    {
        const int Semente = 20260930;
        var mestre = new Random(Semente);
        int comPosicao = 0, posicaoDescartada = 0, comTela = 0;
        for (int caso = 0; caso < 5000; caso++)
        {
            int sementeDoCaso = mestre.Next();
            ConfiguracoesSalvas c = ConfiguracoesAleatorias(new Random(sementeDoCaso));
            string Onde() => $"semente {Semente}, caso {caso} (semente do caso {sementeDoCaso}): {Descrever(c)}";

            ConfiguracoesSalvas n = EsquemaDeConfiguracoes.Normalizar(c);
            byte[] bytes = EsquemaDeConfiguracoes.Escrever(c);
            Verificar(bytes.Length <= EsquemaDeConfiguracoes.TamanhoMaximoEmBytes, () => $"{Onde()}: {bytes.Length} bytes");
            Verificar(bytes[^1] == '\n' && !bytes.Contains((byte)'\r') && System.Text.Unicode.Utf8.IsValid(bytes) && bytes[0] == '{', () => $"{Onde()}: formato do arquivo");

            LeituraDasConfiguracoes lida = EsquemaDeConfiguracoes.Ler(bytes);
            Verificar(lida.Situacao == SituacaoDaLeitura.Valida && lida.Versao == 1 && lida.MotivoIlegivel is null && lida.Avisos.Count == 0,
                () => $"{Onde()}: lida como {lida.Situacao} ({lida.MotivoIlegivel}), avisos: {string.Join(" | ", lida.Avisos)}");
            Verificar(lida.Configuracoes == n, () => $"{Onde()}: lida {Descrever(lida.Configuracoes)}, normalizada {Descrever(n)}");
            Verificar(EsquemaDeConfiguracoes.Escrever(n).AsSpan().SequenceEqual(bytes), () => $"{Onde()}: Escrever(Normalizar(c)) difere de Escrever(c)");
            Verificar(EsquemaDeConfiguracoes.Normalizar(n) == n, () => $"{Onde()}: normalizar de novo mudou");

            // Só valores graváveis.
            Verificar(Enum.IsDefined(n.Preferencias.Energia), () => $"{Onde()}: energia {n.Preferencias.Energia}");
            if (c.Posicao is not null) comPosicao++;
            if (n.Posicao is not { } p)
            {
                if (c.Posicao is not null) posicaoDescartada++;
                continue;
            }
            if (p.TelaDoMonitor is not null) comTela++;
            Verificar(p.ChaveMonitor.Length is >= 1 and <= EsquemaDeConfiguracoes.ComprimentoMaximoDaChave && !p.ChaveMonitor.Any(char.IsControl) && Utf16Valido(p.ChaveMonitor),
                () => $"{Onde()}: chave");
            Verificar(p.FracaoX is >= 0 and <= 1 && p.FracaoY is >= 0 and <= 1 && !double.IsNegative(p.FracaoX) && !double.IsNegative(p.FracaoY), () => $"{Onde()}: frações");
            Verificar(NaFaixa(p.AncoraAbsoluta.X) && NaFaixa(p.AncoraAbsoluta.Y), () => $"{Onde()}: âncora");
            Verificar(p.TelaDoMonitor is not { } t || (!t.Vazio && NaFaixa(t.Esquerda) && NaFaixa(t.Topo) && NaFaixa(t.Direita) && NaFaixa(t.Base)), () => $"{Onde()}: tela");
        }
        Console.WriteLine($"         {comPosicao} com posição ({posicaoDescartada} descartadas pela chave), {comTela} com a tela do monitor gravada");
        Afirmar.Verdadeiro(posicaoDescartada > 100 && comPosicao - posicaoDescartada > 1000 && comTela > 500, "o gerador exercita posições válidas, descartadas e com tela");
    }

    // S1 a S7 pelo arquivo: em cada monitor das sete topologias, a posição descrita como a execução a grava
    // passa por Escrever e Ler, volta igual (com a tela do monitor) e é restaurada pela chave no mesmo lugar,
    // com o mesmo tamanho aparente (128 DIP no DPI do monitor); pela carga da máquina, com a configuração do
    // aplicativo, também. Posição no ar (0,5; 0,5) inclusive: longe do teto e das laterais, ela só cai depois.
    [Teste]
    public static void S1aS7_GravarLerRestaurar_MesmoLugarEmCadaMonitor()
    {
        (string Id, Topologia Topologia)[] topologias =
        [
            ("S1", LadoALado), ("S2", SecundarioAEsquerda), ("S3", EmpilhadoSecundarioAcima), ("S4", DegrauDesalinhado),
            ("S5", EscalasMistas), ("S6", TopologiasDeExemplo.Retrato), ("S7", PrincipalADireita),
        ];
        (double Fx, double Fy)[] fracoes = [(0, 1), (0.25, 1), (0.5, 1), (0.85, 1), (1, 1), (0.5, 0.5)];
        ConfiguracaoDoNucleo doAplicativo = ConfiguracaoDoNucleo.DoAplicativo(Sprite);
        int conferidos = 0;
        foreach ((string id, Topologia t) in topologias)
        {
            foreach (MonitorDoDesktop m in t.Monitores)
            {
                foreach ((double fx, double fy) in fracoes)
                {
                    string caso = $"{id}, {m.Chave}, frações ({fx}; {fy})";
                    Posicionamento antes = Posicionador.NoMonitor(m, fx, fy, Sprite);
                    PosicaoDoPersonagem gravada = Posicionador.Descrever(antes);

                    LeituraDasConfiguracoes lida = EsquemaDeConfiguracoes.Ler(EsquemaDeConfiguracoes.Escrever(new ConfiguracoesSalvas(gravada, Preferencias.Padrao)));
                    Afirmar.Igual(SituacaoDaLeitura.Valida, lida.Situacao, $"{caso}: arquivo válido");
                    PosicaoDoPersonagem salva = Afirmar.NaoNulo(lida.Configuracoes.Posicao, $"{caso}: posição no arquivo");
                    Afirmar.Igual(gravada, salva, $"{caso}: o arquivo devolve a posição gravada, com a tela do monitor");

                    (Posicionamento depois, _, OrigemDaRestauracao origem) = Posicionador.Restaurar(t, salva, Sprite);
                    Afirmar.Igual(OrigemDaRestauracao.PelaChave, origem, $"{caso}: pela chave");
                    Afirmar.Igual(antes, depois, $"{caso}: mesmo lugar");
                    Afirmar.Igual(Sprite.ParaPixels(m.Dpi), depois.Tamanho, $"{caso}: tamanho aparente de 128 DIP");

                    Cenario c = new Cenario(doAplicativo).Aplicar(new Loaded(t, salva, Preferencias.Padrao));
                    Afirmar.Igual("BOOTING: configurações e topologia carregadas; posição salva restaurada pela chave", c.Transicoes[0].Regra, $"{caso}: regra da carga");
                    Afirmar.Igual(antes.Ancora, c.Ancora, $"{caso}: a carga põe no mesmo lugar");
                    Afirmar.Igual(m.Chave, c.Retrato.ChaveMonitor, $"{caso}: no mesmo monitor");
                    Afirmar.Igual(Sprite.ParaPixels(m.Dpi), c.Retrato.Tamanho, $"{caso}: com o mesmo tamanho aparente");
                    conferidos++;
                }
            }
        }
        Afirmar.Igual(16 * 6, conferidos, "16 monitores nas sete topologias (três em S4 e em S5), 6 frações em cada");
    }

    // ---------------------------------------------------------------- ilegível

    // Estrutura inválida: o arquivo inteiro é ilegível, com o motivo da primeira checagem que falhou e as
    // configurações padrão. A leitura nunca lança, nem com um escape que não vira UTF-16 válido.
    [Teste]
    public static void Ler_IlegivelEstrutural()
    {
        (string Caso, byte[] Bytes, string Motivo)[] casos =
        [
            ("vazio", [], "json"),
            ("só espaços", Utf8("  \n "), "json"),
            ("null", Utf8("null"), "raiz"),
            ("lista", Utf8("[]"), "raiz"),
            ("texto", Utf8("\"schemaVersion\""), "raiz"),
            ("número", Utf8("1"), "raiz"),
            ("só a chave de abertura", Utf8("{"), "json"),
            ("truncado", Utf8("{\"schemaVersion\":1"), "json"),
            ("dois documentos", Utf8("{\"schemaVersion\":1}{}"), "json"),
            ("aspas simples", Utf8("{'schemaVersion':1}"), "json"),
            ("NaN", Utf8("{\"schemaVersion\":1,\"posicao\":{\"chaveMonitor\":\"a\",\"fracaoX\":NaN,\"fracaoY\":1}}"), "json"),
            ("bytes C3 28", [0xC3, 0x28], "utf8"),
            ("UTF-8 inválido dentro de um texto", [.. Utf8("{\"schemaVersion\":1,\"x\":\""), 0xFF, .. Utf8("\"}")], "utf8"),
            ("surrogate codificado em UTF-8", [.. Utf8("{\"schemaVersion\":1,\"x\":\""), 0xED, 0xA0, 0x80, .. Utf8("\"}")], "utf8"),
            ("profundidade 9", Utf8(Aninhado(9)), "json"),
            ("sem schemaVersion", Utf8("{}"), "schemaVersion"),
            ("schemaVersion nulo", Utf8("{\"schemaVersion\":null}"), "schemaVersion"),
            ("schemaVersion \"1\"", Utf8("{\"schemaVersion\":\"1\"}"), "schemaVersion"),
            ("schemaVersion 1.5", Utf8("{\"schemaVersion\":1.5}"), "schemaVersion"),
            ("schemaVersion 1.0", Utf8("{\"schemaVersion\":1.0}"), "schemaVersion"),
            ("schemaVersion 0", Utf8("{\"schemaVersion\":0}"), "schemaVersion"),
            ("schemaVersion -3", Utf8("{\"schemaVersion\":-3}"), "schemaVersion"),
            ("schemaVersion 2147483648", Utf8("{\"schemaVersion\":2147483648}"), "schemaVersion"),
            ("chave com surrogate solto (\\uD800)", Utf8("{\"schemaVersion\":1,\"posicao\":{\"chaveMonitor\":\"\\uD800\",\"fracaoX\":0.5,\"fracaoY\":1}}"), "json"),
        ];
        foreach ((string caso, byte[] bytes, string motivo) in casos)
            AfirmarIlegivel(EsquemaDeConfiguracoes.Ler(bytes), motivo, caso);

        // O limite é inclusivo: profundidade 8 ainda é lida (o campo aninhado é desconhecido e só gera aviso).
        LeituraDasConfiguracoes oito = EsquemaDeConfiguracoes.Ler(Utf8(Aninhado(8)));
        Afirmar.Igual(SituacaoDaLeitura.Valida, oito.Situacao, "profundidade 8");
        Afirmar.Igual(ConfiguracoesSalvas.Padrao, oito.Configuracoes, "profundidade 8: nada conhecido além da versão");
    }

    // Até 65.536 bytes, contando um BOM, o arquivo é lido; com um byte a mais, é ilegível por tamanho,
    // antes de qualquer outra checagem (nem o UTF-8 é conferido).
    [Teste]
    public static void Ler_LimiteDeTamanho()
    {
        byte[] ComTamanho(int total, byte[] inicio) => [.. inicio, .. Enumerable.Repeat((byte)' ', total - inicio.Length)];
        byte[] minimo = Utf8("{\"schemaVersion\":1,\"preferencias\":{\"energia\":\"alta\"}}");

        LeituraDasConfiguracoes noLimite = EsquemaDeConfiguracoes.Ler(ComTamanho(65_536, minimo));
        Afirmar.Igual(SituacaoDaLeitura.Valida, noLimite.Situacao, "65.536 bytes");
        Afirmar.Igual(NivelDeEnergia.Alta, noLimite.Configuracoes.Preferencias.Energia, "65.536 bytes: lido");
        Afirmar.Igual(SituacaoDaLeitura.Valida, EsquemaDeConfiguracoes.Ler(ComTamanho(65_536, [0xEF, 0xBB, 0xBF, .. minimo])).Situacao, "65.536 bytes com o BOM");

        AfirmarIlegivel(EsquemaDeConfiguracoes.Ler(ComTamanho(65_537, minimo)), "tamanho", "65.537 bytes");
        AfirmarIlegivel(EsquemaDeConfiguracoes.Ler(ComTamanho(65_537, [0xEF, 0xBB, 0xBF, .. minimo])), "tamanho", "65.537 bytes contando o BOM");
        AfirmarIlegivel(EsquemaDeConfiguracoes.Ler(ComTamanho(70_000, [0xC3, 0x28])), "tamanho", "70.000 bytes de UTF-8 inválido: o tamanho vem antes");
        Afirmar.Igual(65_536, EsquemaDeConfiguracoes.TamanhoMaximoEmBytes, "constante");
    }

    // O arquivo pode ter sido editado à mão: BOM, comentários e vírgula final são aceitos.
    [Teste]
    public static void Ler_BomComentarioEVirgulaFinal_Aceitos()
    {
        ConfiguracoesSalvas amostra = new(PosicaoS2(), Preferencias.Padrao);
        byte[] amostraComBom = [0xEF, 0xBB, 0xBF, .. File.ReadAllBytes(CaminhoDaAmostra())];
        LeituraDasConfiguracoes comBom = EsquemaDeConfiguracoes.Ler(amostraComBom);
        Afirmar.Igual(SituacaoDaLeitura.Valida, comBom.Situacao, "BOM");
        Afirmar.Igual(amostra, comBom.Configuracoes, "BOM: os mesmos valores");

        LeituraDasConfiguracoes comentada = EsquemaDeConfiguracoes.Ler(Utf8(
            "// editado à mão\n{ \"schemaVersion\": 1, /* nível */ \"preferencias\": { \"energia\": \"baixa\" // menos agitado\n } }\n// fim"));
        Afirmar.Igual(SituacaoDaLeitura.Valida, comentada.Situacao, "comentários");
        Afirmar.Igual(new Preferencias(NivelDeEnergia.Baixa, true, true), comentada.Configuracoes.Preferencias, "comentários: lido");

        LeituraDasConfiguracoes virgula = EsquemaDeConfiguracoes.Ler(Utf8(
            "{\"schemaVersion\":1,\"preferencias\":{\"energia\":\"alta\",\"modoTelaCheia\":false,},}"));
        Afirmar.Igual(SituacaoDaLeitura.Valida, virgula.Situacao, "vírgula final");
        Afirmar.Igual(new Preferencias(NivelDeEnergia.Alta, false, true), virgula.Configuracoes.Preferencias, "vírgula final: lido");
    }

    // ---------------------------------------------------------------- leitura campo a campo

    // Campos desconhecidos, na raiz, na posição e nas preferências, inclusive um nome do esquema com outra
    // caixa, são ignorados: os conhecidos continuam valendo, e os avisos não citam nome nem valor do arquivo.
    [Teste]
    public static void Ler_CamposDesconhecidos_Ignorados()
    {
        LeituraDasConfiguracoes lida = Ler("""
            {"schemaVersion": 1, "extra": {"a": [1, 2, {"b": null}]},
             "posicao": {"ChaveMonitor": "outra", "chaveMonitor": "mon:1", "fracaoX": 0.5, "fracaoY": 1, "novo": true},
             "preferencias": {"Energia": "baixa", "energia": "alta", "corDoChapeu": "vermelho"}}
            """);

        Afirmar.Igual(SituacaoDaLeitura.Valida, lida.Situacao, "situação");
        Afirmar.Igual(new PosicaoDoPersonagem("mon:1", 0.5, 1, default), lida.Configuracoes.Posicao, "posição");
        Afirmar.Igual(new Preferencias(NivelDeEnergia.Alta, true, true), lida.Configuracoes.Preferencias, "preferências");
        Afirmar.Igual(5, lida.Avisos.Count, $"um aviso por campo desconhecido: {string.Join(" | ", lida.Avisos)}");
        foreach (string aviso in lida.Avisos)
        {
            foreach (string doArquivo in new[] { "extra", "novo", "ChaveMonitor", "outra", "Energia", "corDoChapeu", "vermelho", "mon:1" })
                Afirmar.Falso(aviso.Contains(doArquivo, StringComparison.Ordinal), $"o aviso \"{aviso}\" cita \"{doArquivo}\", do arquivo");
        }

        // Um nome do esquema com outra caixa não é o campo, nem sozinho.
        AfirmarIlegivel(Ler("""{"SchemaVersion": 1}"""), "schemaVersion", "SchemaVersion");
        Afirmar.Igual(ConfiguracoesSalvas.Padrao, Ler("""{"schemaVersion": 1, "Preferencias": {"energia": "alta"}, "Posicao": {"chaveMonitor": "a", "fracaoX": 0, "fracaoY": 1}}""").Configuracoes,
            "Preferencias e Posicao com maiúscula: ignoradas");
    }

    // Campo repetido: vale a primeira ocorrência, com um aviso que cita o nome do esquema.
    [Teste]
    public static void Ler_CampoRepetido_ValeOPrimeiro()
    {
        LeituraDasConfiguracoes energia = Ler("""{"schemaVersion": 1, "preferencias": {"energia": "baixa", "energia": "alta"}}""");
        Afirmar.Igual(NivelDeEnergia.Baixa, energia.Configuracoes.Preferencias.Energia, "o primeiro vale");
        Afirmar.Sequencia(["campo repetido: preferencias.energia; vale o primeiro"], energia.Avisos, "aviso");

        // Também na raiz: o primeiro schemaVersion e a primeira posição, mesmo que os seguintes sejam inválidos.
        LeituraDasConfiguracoes raiz = Ler("""
            {"schemaVersion": 1, "schemaVersion": "x",
             "posicao": {"chaveMonitor": "a", "fracaoX": 0, "fracaoY": 1, "fracaoX": 0.75}, "posicao": null}
            """);
        Afirmar.Igual(SituacaoDaLeitura.Valida, raiz.Situacao, "o primeiro schemaVersion vale");
        PosicaoDoPersonagem p = Afirmar.NaoNulo(raiz.Configuracoes.Posicao, "a primeira posição vale");
        Afirmar.Igual(0.0, p.FracaoX, "a primeira fração vale");
        Afirmar.Sequencia(
            ["campo repetido: schemaVersion; vale o primeiro", "campo repetido: posicao; vale o primeiro", "campo repetido: posicao.fracaoX; vale o primeiro"],
            raiz.Avisos, "avisos");
    }

    // Número fora da faixa é preso ao limite, com aviso: frações em [0, 1], coordenadas em [-32768, 32767].
    [Teste]
    public static void Ler_ForaDaFaixa_PresoAoLimite()
    {
        LeituraDasConfiguracoes lida = Ler("""
            {"schemaVersion": 1, "posicao": {"chaveMonitor": "a", "fracaoX": 1.5, "fracaoY": -0.25,
             "telaDoMonitor": {"esquerda": -99999, "topo": 0, "direita": 99999, "base": 1080},
             "ancoraAbsoluta": {"x": -40000, "y": 40000}}}
            """);
        PosicaoDoPersonagem p = Afirmar.NaoNulo(lida.Configuracoes.Posicao, "posição");
        Afirmar.Igual(1.0, p.FracaoX, "fracaoX 1,5");
        Afirmar.Igual(0.0, p.FracaoY, "fracaoY -0,25");
        Afirmar.Igual(new RetanguloPx(-32768, 0, 32767, 1080), p.TelaDoMonitor, "tela presa");
        Afirmar.Igual(new PontoPx(-32768, 32767), p.AncoraAbsoluta, "âncora presa");
        Afirmar.Igual(6, lida.Avisos.Count, $"um aviso por valor preso: {string.Join(" | ", lida.Avisos)}");

        (string Texto, double Esperada)[] fracoes = [("1e308", 1), ("-1e308", 0), ("1", 1), ("0", 0), ("-0", 0), ("0.5e-323", 5e-324)];
        foreach ((string texto, double esperada) in fracoes)
        {
            PosicaoDoPersonagem q = Afirmar.NaoNulo(Ler($$$"""{"schemaVersion": 1, "posicao": {"chaveMonitor": "a", "fracaoX": {{{texto}}}, "fracaoY": 1}}""").Configuracoes.Posicao, texto);
            Afirmar.Igual(esperada, q.FracaoX, $"fracaoX {texto}");
            Afirmar.Falso(double.IsNegative(q.FracaoX), $"fracaoX {texto}: sem zero negativo");
        }
    }

    // Tipo errado vale o padrão do campo, com aviso; o resto do arquivo é preservado.
    [Teste]
    public static void Ler_TipoErrado_PadraoDoCampo()
    {
        const string PosicaoValida = "\"posicao\": {\"chaveMonitor\": \"a\", \"fracaoX\": 0.5, \"fracaoY\": 1}";
        var posicao = new PosicaoDoPersonagem("a", 0.5, 1, default);

        LeituraDasConfiguracoes campos = Ler($$$"""
            {"schemaVersion": 1, {{{PosicaoValida}}},
             "preferencias": {"energia": 2, "modoTelaCheia": "false", "atravessarMonitores": null}}
            """);
        Afirmar.Igual(new ConfiguracoesSalvas(posicao, Preferencias.Padrao), campos.Configuracoes, "energia 2, modoTelaCheia \"false\", atravessarMonitores nulo: padrões");
        Afirmar.Igual(3, campos.Avisos.Count, "um aviso por campo");

        LeituraDasConfiguracoes lista = Ler($$$"""{"schemaVersion": 1, {{{PosicaoValida}}}, "preferencias": []}""");
        Afirmar.Igual(new ConfiguracoesSalvas(posicao, Preferencias.Padrao), lista.Configuracoes, "preferências em lista: padrões, posição preservada");
        Afirmar.Igual(1, lista.Avisos.Count, "preferências em lista: aviso");

        LeituraDasConfiguracoes texto = Ler("""
            {"schemaVersion": 1, "posicao": "x",
             "preferencias": {"energia": "alta", "modoTelaCheia": false, "atravessarMonitores": false}}
            """);
        Afirmar.Igual(new ConfiguracoesSalvas(null, new Preferencias(NivelDeEnergia.Alta, false, false)), texto.Configuracoes, "posição em texto: nula, preferências preservadas");
        Afirmar.Igual(1, texto.Avisos.Count, "posição em texto: aviso");

        LeituraDasConfiguracoes ancora = Ler("""
            {"schemaVersion": 1, "posicao": {"chaveMonitor": "a", "fracaoX": 0.5, "fracaoY": 1, "ancoraAbsoluta": {"x": 1.5, "y": 2}}}
            """);
        Afirmar.Igual(posicao, ancora.Configuracoes.Posicao, "âncora com x fracionário: (0, 0), posição preservada");
        Afirmar.Igual(1, ancora.Avisos.Count, "âncora inválida: aviso");

        // Ausentes ou nulos, as seções e os campos opcionais valem o padrão sem aviso.
        LeituraDasConfiguracoes vazias = Ler("""{"schemaVersion": 1, "posicao": null, "preferencias": {}}""");
        Afirmar.Igual(ConfiguracoesSalvas.Padrao, vazias.Configuracoes, "seções nulas ou vazias");
        Afirmar.Sequencia([], vazias.Avisos, "seções nulas ou vazias: sem aviso");
        LeituraDasConfiguracoes semOpcionais = Ler($$$"""{"schemaVersion": 1, {{{PosicaoValida}}}}""");
        Afirmar.Igual(new ConfiguracoesSalvas(posicao, Preferencias.Padrao), semOpcionais.Configuracoes, "sem tela, âncora e preferências");
        Afirmar.Sequencia([], semOpcionais.Avisos, "sem os opcionais: sem aviso");
    }

    // Energia só pelos três nomes, sem diferenciar maiúsculas; nunca como número, lista de flags ou
    // variante do nome (SECURITY.md 7). Recusada, vale Média, com aviso.
    [Teste]
    public static void Ler_Energia_SoOsTresNomes()
    {
        (string Texto, NivelDeEnergia Nivel)[] aceitos = [("baixa", NivelDeEnergia.Baixa), ("MEDIA", NivelDeEnergia.Media), ("Alta", NivelDeEnergia.Alta), ("aLtA", NivelDeEnergia.Alta)];
        foreach ((string texto, NivelDeEnergia nivel) in aceitos)
        {
            Afirmar.Verdadeiro(EsquemaDeConfiguracoes.TentarLerEnergia(texto, out NivelDeEnergia lido), $"\"{texto}\" aceito");
            Afirmar.Igual(nivel, lido, $"\"{texto}\"");
            LeituraDasConfiguracoes lida = Ler(ComEnergia(texto));
            Afirmar.Igual(nivel, lida.Configuracoes.Preferencias.Energia, $"\"{texto}\" no arquivo");
            Afirmar.Sequencia([], lida.Avisos, $"\"{texto}\": sem aviso");
        }

        string[] recusados = ["1", "0", "2", "Baixa,Alta", "baixa,alta", " media", "alta ", "média", "turbo", "", "Media\u0000", "Alta\n"];
        foreach (string texto in recusados)
        {
            string caso = $"\"{JsonEncodedText.Encode(texto)}\"";
            Afirmar.Falso(EsquemaDeConfiguracoes.TentarLerEnergia(texto, out NivelDeEnergia lido), $"{caso} recusado");
            Afirmar.Igual(NivelDeEnergia.Media, lido, $"{caso}: Média");
            LeituraDasConfiguracoes lida = Ler(ComEnergia(texto));
            Afirmar.Igual(NivelDeEnergia.Media, lida.Configuracoes.Preferencias.Energia, $"{caso} no arquivo: Média");
            Afirmar.Sequencia(["preferencias.energia: não é baixa, media nem alta; vale media"], lida.Avisos, $"{caso}: aviso");
        }

        foreach (string numero in new[] { "0", "2", "true", "null", "[\"alta\"]" })
        {
            LeituraDasConfiguracoes lida = Ler($$$"""{"schemaVersion": 1, "preferencias": {"energia": {{{numero}}}}}""");
            Afirmar.Igual(NivelDeEnergia.Media, lida.Configuracoes.Preferencias.Energia, $"energia {numero}: Média");
            Afirmar.Igual(1, lida.Avisos.Count, $"energia {numero}: aviso");
        }

        Afirmar.Sequencia(["baixa", "media", "alta", "media"],
            new[] { NivelDeEnergia.Baixa, NivelDeEnergia.Media, NivelDeEnergia.Alta, (NivelDeEnergia)7 }.Select(EsquemaDeConfiguracoes.NomeDaEnergia), "nomes escritos; fora do enum, media");
    }

    // A posição é tudo ou nada nos campos obrigatórios: sem chave válida ou sem uma das frações como número
    // finito, não há posição, e as preferências continuam valendo.
    [Teste]
    public static void Ler_PosicaoTudoOuNada()
    {
        (string Caso, string Posicao)[] casos =
        [
            ("sem chave", """{"fracaoX": 0.5, "fracaoY": 1}"""),
            ("chave nula", """{"chaveMonitor": null, "fracaoX": 0.5, "fracaoY": 1}"""),
            ("chave numérica", """{"chaveMonitor": 2, "fracaoX": 0.5, "fracaoY": 1}"""),
            ("chave vazia", """{"chaveMonitor": "", "fracaoX": 0.5, "fracaoY": 1}"""),
            ("chave de 1025 caracteres", $$$"""{"chaveMonitor": "{{{new string('a', 1025)}}}", "fracaoX": 0.5, "fracaoY": 1}"""),
            ("chave com \\u0001", """{"chaveMonitor": "a\u0001b", "fracaoX": 0.5, "fracaoY": 1}"""),
            ("chave com \\u007F", """{"chaveMonitor": "a\u007Fb", "fracaoX": 0.5, "fracaoY": 1}"""),
            ("chave com \\n", """{"chaveMonitor": "a\nb", "fracaoX": 0.5, "fracaoY": 1}"""),
            ("fracaoX em texto", """{"chaveMonitor": "a", "fracaoX": "0.5", "fracaoY": 1}"""),
            ("fracaoX nula", """{"chaveMonitor": "a", "fracaoX": null, "fracaoY": 1}"""),
            ("fracaoX booleana", """{"chaveMonitor": "a", "fracaoX": true, "fracaoY": 1}"""),
            ("fracaoX 1e400, que não cabe num double", """{"chaveMonitor": "a", "fracaoX": 1e400, "fracaoY": 1}"""),
            ("fracaoY ausente", """{"chaveMonitor": "a", "fracaoX": 0.5}"""),
            ("fracaoY em lista", """{"chaveMonitor": "a", "fracaoX": 0.5, "fracaoY": [1]}"""),
        ];
        var preferencias = new Preferencias(NivelDeEnergia.Alta, false, true);
        foreach ((string caso, string posicao) in casos)
        {
            LeituraDasConfiguracoes lida = Ler($$$"""{"schemaVersion": 1, "posicao": {{{posicao}}}, "preferencias": {"energia": "alta", "modoTelaCheia": false}}""");
            Afirmar.Igual(SituacaoDaLeitura.Valida, lida.Situacao, $"{caso}: situação");
            Afirmar.Nulo(lida.Configuracoes.Posicao, $"{caso}: sem posição");
            Afirmar.Igual(preferencias, lida.Configuracoes.Preferencias, $"{caso}: preferências intactas");
            Afirmar.Verdadeiro(lida.Avisos.Count > 0, $"{caso}: aviso");
        }

        // No limite: 1024 caracteres, e caracteres fora do ASCII, inclusive um par surrogate, são aceitos.
        foreach (string chave in new[] { new string('a', 1024), "mon:ção-😀", @"\\?\DISPLAY#GSM5B09#4&1a2b3c&0&UID4352#{e6f07b5f}" })
        {
            string json = $$$"""{"schemaVersion": 1, "posicao": {"chaveMonitor": "{{{JsonEncodedText.Encode(chave)}}}", "fracaoX": 0.5, "fracaoY": 1}}""";
            Afirmar.Igual(chave, Afirmar.NaoNulo(Ler(json).Configuracoes.Posicao, chave).ChaveMonitor, $"chave \"{chave}\"");
        }
    }

    // Tela do monitor inválida vale desconhecida, com aviso, e a posição continua (a partida ainda restaura
    // pela chave ou no principal). Nula ou ausente, desconhecida sem aviso.
    [Teste]
    public static void Ler_RetanguloInvalido_ViraDesconhecido()
    {
        (string Caso, string Tela)[] casos =
        [
            ("esquerda igual à direita", """{"esquerda": 0, "topo": 0, "direita": 0, "base": 1080}"""),
            ("topo depois da base", """{"esquerda": 0, "topo": 1080, "direita": 1920, "base": 0}"""),
            ("sem a base", """{"esquerda": 0, "topo": 0, "direita": 1920}"""),
            ("lado em texto", """{"esquerda": "0", "topo": 0, "direita": 1920, "base": 1080}"""),
            ("lado fracionário", """{"esquerda": 0.5, "topo": 0, "direita": 1920, "base": 1080}"""),
            ("lado maior que um inteiro de 64 bits", """{"esquerda": 0, "topo": 0, "direita": 99999999999999999999, "base": 1080}"""),
            ("vazia depois de presa", """{"esquerda": 40000, "topo": 0, "direita": 50000, "base": 1080}"""),
            ("lista", "[0, 0, 1920, 1080]"),
            ("texto", "\"(0,0)-(1920,1080)\""),
        ];
        foreach ((string caso, string tela) in casos)
        {
            LeituraDasConfiguracoes lida = Ler($$$"""{"schemaVersion": 1, "posicao": {"chaveMonitor": "a", "telaDoMonitor": {{{tela}}}, "fracaoX": 0.5, "fracaoY": 1}}""");
            PosicaoDoPersonagem p = Afirmar.NaoNulo(lida.Configuracoes.Posicao, $"{caso}: a posição continua");
            Afirmar.Igual(new PosicaoDoPersonagem("a", 0.5, 1, default), p, $"{caso}: tela desconhecida, o resto igual");
            Afirmar.Verdadeiro(lida.Avisos.Contains("posicao.telaDoMonitor: inválida; tela desconhecida"), $"{caso}: aviso ({string.Join(" | ", lida.Avisos)})");
        }

        LeituraDasConfiguracoes nula = Ler("""{"schemaVersion": 1, "posicao": {"chaveMonitor": "a", "telaDoMonitor": null, "fracaoX": 0.5, "fracaoY": 1}}""");
        Afirmar.Nulo(Afirmar.NaoNulo(nula.Configuracoes.Posicao, "tela nula").TelaDoMonitor, "tela nula: desconhecida");
        Afirmar.Sequencia([], nula.Avisos, "tela nula: sem aviso");
    }

    // Versão futura (a Fase 8 amplia o esquema e incrementa a versão): os campos conhecidos são lidos pelas
    // regras da v1, os novos são ignorados, e a situação avisa a raiz para não gravar por cima.
    [Teste]
    public static void Ler_VersaoFutura_LeOQueConhece()
    {
        LeituraDasConfiguracoes lida = Ler("""
            {"schemaVersion": 2, "posicao": {"chaveMonitor": "a", "fracaoX": 0.25, "fracaoY": 1, "monitorPreferido": "b"},
             "preferencias": {"energia": "baixa", "volume": 7}, "janelaDeConfiguracoes": {"largura": 400}}
            """);
        Afirmar.Igual(SituacaoDaLeitura.VersaoFutura, lida.Situacao, "situação");
        Afirmar.Igual(2, lida.Versao, "versão");
        Afirmar.Nulo(lida.MotivoIlegivel, "motivo");
        Afirmar.Igual(new ConfiguracoesSalvas(new PosicaoDoPersonagem("a", 0.25, 1, default), new Preferencias(NivelDeEnergia.Baixa, true, true)),
            lida.Configuracoes, "valores da v1");

        Afirmar.Igual(SituacaoDaLeitura.VersaoFutura, Ler("""{"schemaVersion": 2147483647}""").Situacao, "a maior versão possível");
        Afirmar.Igual(SituacaoDaLeitura.Valida, Ler("""{"schemaVersion": 1}""").Situacao, "a versão atual");
        Afirmar.Igual(1, EsquemaDeConfiguracoes.VersaoAtual, "versão atual");
    }

    // ---------------------------------------------------------------- política de gravação

    // Gravação na hora só depois de um evento após o qual o processo pode não ter outra chance (suspensão,
    // fim de sessão, sair) e no bloqueio de sessão (C7); todos os outros, com atraso. A lista de exemplos
    // cobre cada tipo concreto de evento do núcleo: um evento novo sem decisão aqui falha.
    [Teste]
    public static void Politica_Imediata_SoSuspensaoFimDeSessaoSairEBloqueio()
    {
        Evento[] exemplos =
        [
            new Press(default), new Click(), new DoubleClick(), new DragStart(), new DragMove(default), new DragEnd(default), new DragCancel(),
            new ContextMenu(default), new EnergyPanelOpen(), new EnergySelected(NivelDeEnergia.Alta), new EnergyPanelClose(),
            new CmdHide(), new CmdShow(), new CmdPauseAutonomy(), new CmdResumeAutonomy(), new CmdOpenSettings(), new CmdResetPosition(), new CmdExit(),
            new Loaded(UmMonitor, null, Preferencias.Padrao), new TopologyChanged(UmMonitor), new SessionLocked(), new SessionUnlocked(),
            new Suspending(), new Resumed(), new SessionEnding(), new FullscreenTargetsChanged(MonitoresOcupados.Nenhum),
            new SettingsChanged(Preferencias.Padrao), new Tick(), new MovementSignal(default), new AutonomyTimer(1), new ExpressionChange(default),
        ];
        string[] todos = [.. typeof(Evento).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Evento))).Select(t => t.Name).Order(StringComparer.Ordinal)];
        Afirmar.Sequencia(todos, exemplos.Select(e => e.GetType().Name).Order(StringComparer.Ordinal), "um exemplo de cada tipo de evento");

        string[] imediatos = [.. exemplos.Where(PoliticaDeGravacao.Imediata).Select(e => e.GetType().Name).Order(StringComparer.Ordinal)];
        Afirmar.Sequencia(["CmdExit", "SessionEnding", "SessionLocked", "Suspending"], imediatos, "imediatos");
    }

    // Tempos da agenda de gravação (desenho de persistência, D11): o atraso de 2 s fica abaixo do intervalo
    // de acomodação do aplicativo (3 s), para a gravação cair com o personagem parado; depois de uma falha,
    // novas tentativas únicas em 2, 10 e 60 s; no caminho imediato, 3 tentativas com 50 ms entre elas.
    [Teste]
    public static void Politica_TemposDoDesenho()
    {
        Afirmar.Igual(TimeSpan.FromSeconds(2), PoliticaDeGravacao.Atraso, "atraso");
        Afirmar.Verdadeiro(PoliticaDeGravacao.Atraso < ConfiguracaoDoNucleo.DoAplicativo(Sprite).IntervaloDeAcomodacao, "atraso menor que o intervalo de acomodação do aplicativo");
        Afirmar.Sequencia([TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60)], PoliticaDeGravacao.EsperasDeNovaTentativa, "novas tentativas");
        Afirmar.Igual(3, PoliticaDeGravacao.TentativasImediatas, "tentativas no caminho imediato");
        Afirmar.Igual(TimeSpan.FromMilliseconds(50), PoliticaDeGravacao.PausaEntreTentativasImediatas, "pausa entre elas");
    }

    // S12 (C7): bloquear a sessão ou suspender com o personagem à vista grava a posição, e a raiz grava na
    // hora. Bloqueado e depois suspenso, a suspensão não grava de novo (o personagem já está escondido pela
    // sessão): por isso o bloqueio também é imediato. Escondido pelo usuário, suspender não grava; esconder
    // pela bandeja grava com atraso.
    [Teste]
    public static void S12_BloquearESuspender_EmitemGravarPosicaoImediata()
    {
        Cenario bloqueado = Cenario.Parado().Aplicar(new SessionLocked()).EstaEscondido(MotivoDoOcultamento.PorSessao);
        Afirmar.Igual(Cenario.AncoraInicial, bloqueado.Efeito<GravarPosicao>().Posicao.AncoraAbsoluta, "bloquear grava onde ele estava");
        Afirmar.Verdadeiro(PoliticaDeGravacao.Imediata(new SessionLocked()), "e grava na hora");
        bloqueado.Aplicar(new Suspending()).EstaEscondido(MotivoDoOcultamento.PorSessao).SemEfeito<GravarPosicao>();

        Cenario suspenso = Cenario.Parado().Aplicar(new Suspending()).EstaEscondido(MotivoDoOcultamento.PorSuspensao);
        Afirmar.Igual(Cenario.AncoraInicial, suspenso.Efeito<GravarPosicao>().Posicao.AncoraAbsoluta, "suspender grava onde ele estava");
        Afirmar.Verdadeiro(PoliticaDeGravacao.Imediata(new Suspending()), "e grava na hora");

        Cenario.Parado().Aplicar(new CmdHide(), new Suspending()).EstaEscondido(MotivoDoOcultamento.PorUsuario).SemEfeito<GravarPosicao>();

        Cenario pelaBandeja = Cenario.Parado().Aplicar(new CmdHide());
        Afirmar.Verdadeiro(pelaBandeja.Tem<GravarPosicao>() && !PoliticaDeGravacao.Imediata(new CmdHide()), "esconder pela bandeja grava com atraso");
        foreach (Evento saida in new Evento[] { new CmdExit(), new SessionEnding() })
        {
            Cenario c = Cenario.Parado().Aplicar(saida).Esta(Estado.Exiting);
            Afirmar.Verdadeiro(c.Tem<GravarPosicao>() && PoliticaDeGravacao.Imediata(saida), $"{saida.GetType().Name} grava na hora");
        }
    }

    // ---------------------------------------------------------------- auxiliares

    private static LeituraDasConfiguracoes Ler(string json) => EsquemaDeConfiguracoes.Ler(Utf8(json));

    /// <summary>
    /// Configurações aleatórias, muitas inválidas: posição ausente, chave vazia, nula, longa, com controle,
    /// surrogate solto, aspas, barras e caracteres fora do ASCII; frações NaN, infinitas, -0, subnormais,
    /// enormes ou com bits quaisquer; coordenadas em qualquer ponto de int; tela vazia, invertida ou fora da
    /// faixa; energia fora do enum; às vezes preferências nulas.
    /// </summary>
    private static ConfiguracoesSalvas ConfiguracoesAleatorias(Random rnd)
    {
        PosicaoDoPersonagem? posicao = rnd.Next(10) == 0 ? null
            : new PosicaoDoPersonagem(ChaveAleatoria(rnd), FracaoAleatoria(rnd), FracaoAleatoria(rnd), new PontoPx(CoordenadaAleatoria(rnd), CoordenadaAleatoria(rnd)))
            {
                TelaDoMonitor = rnd.Next(4) == 0 ? null : TelaAleatoria(rnd),
            };
        Preferencias preferencias = rnd.Next(50) == 0 ? null! : new Preferencias((NivelDeEnergia)rnd.Next(-2, 6), rnd.Next(2) == 0, rnd.Next(2) == 0);
        return new ConfiguracoesSalvas(posicao, preferencias);
    }

    private static string ChaveAleatoria(Random rnd) => rnd.Next(12) switch
    {
        0 => "",
        1 => null!,
        2 => new string('k', rnd.Next(1020, 1030)),
        3 or 4 => $@"\\.\DISPLAY{rnd.Next(1, 10)}",
        5 => "mon:" + rnd.NextInt64().ToString("x16", CultureInfo.InvariantCulture),
        6 or 7 or 8 => TextoAleatorio(rnd, rnd.Next(1, 40), soGravaveis: true),
        _ => TextoAleatorio(rnd, rnd.Next(1, 40), soGravaveis: false),
    };

    /// <summary>
    /// Texto com os caracteres que o JSON ou o codificador escapam, fora do ASCII e pares surrogate; sem
    /// <paramref name="soGravaveis"/>, também controles e surrogates soltos, que invalidam a chave.
    /// </summary>
    private static string TextoAleatorio(Random rnd, int caracteres, bool soGravaveis)
    {
        var sb = new StringBuilder(caracteres);
        while (sb.Length < caracteres)
        {
            switch (rnd.Next(10))
            {
                case 0 when !soGravaveis: sb.Append((char)rnd.Next(0, 0x20)); break;             // controle C0
                case 1: sb.Append("\"\\/'<>&+`"[rnd.Next(9)]); break;                            // escapados pelo JSON ou pelo codificador
                case 2: sb.Append((char)rnd.Next(soGravaveis ? 0xA0 : 0x7F, 0xD800)); break;    // fora do ASCII (sem gravável: DEL e controle C1)
                case 3 when !soGravaveis: sb.Append((char)rnd.Next(0xD800, 0xE000)); break;      // surrogate solto (alto ou baixo)
                case 4: sb.Append(char.ConvertFromUtf32(rnd.Next(0x10000, 0x110000))); break;   // par surrogate
                case 5: sb.Append((char)rnd.Next(0xE000, 0x10000)); break;                       // uso privado, U+FFFE, U+FFFF
                default: sb.Append((char)rnd.Next(0x20, 0x7F)); break;                           // ASCII visível
            }
        }
        return sb.ToString();
    }

    private static double FracaoAleatoria(Random rnd) => rnd.Next(14) switch
    {
        0 => double.NaN,
        1 => rnd.Next(2) == 0 ? double.PositiveInfinity : double.NegativeInfinity,
        2 => -0.0,
        3 => double.Epsilon * rnd.Next(1, 1000),
        4 => (rnd.Next(2) == 0 ? 1 : -1) * double.MaxValue / rnd.Next(1, 1000),
        5 => BitConverter.Int64BitsToDouble(rnd.NextInt64(long.MinValue, long.MaxValue)),
        6 => rnd.NextDouble() * 4 - 2,
        7 => 0,
        8 => 1,
        9 => Math.BitDecrement(1.0),
        _ => rnd.NextDouble(),
    };

    private static int CoordenadaAleatoria(Random rnd) => rnd.Next(4) switch
    {
        0 => rnd.Next(int.MinValue, int.MaxValue),
        1 => rnd.Next(-32770, -32765),
        2 => rnd.Next(32765, 32770),
        _ => rnd.Next(-20000, 20000),
    };

    private static RetanguloPx TelaAleatoria(Random rnd)
    {
        if (rnd.Next(3) != 0)
        {
            int esquerda = rnd.Next(-10000, 10000), topo = rnd.Next(-10000, 10000);
            return new RetanguloPx(esquerda, topo, esquerda + rnd.Next(1, 8000), topo + rnd.Next(1, 5000));
        }
        return new RetanguloPx(CoordenadaAleatoria(rnd), CoordenadaAleatoria(rnd), CoordenadaAleatoria(rnd), CoordenadaAleatoria(rnd));
    }

    private static bool NaFaixa(int coordenada) => coordenada is >= EsquemaDeConfiguracoes.CoordenadaMinima and <= EsquemaDeConfiguracoes.CoordenadaMaxima;

    /// <summary>Se o texto é UTF-16 válido (sem surrogate solto): o codificador estrito de UTF-8 aceita.</summary>
    private static bool Utf16Valido(string texto)
    {
        try
        {
            _ = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetByteCount(texto);
            return true;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    /// <summary>Descrição para mensagens de falha, com a chave escapada (pode ter controle ou surrogate solto).</summary>
    private static string Descrever(ConfiguracoesSalvas c)
    {
        string posicao = c.Posicao is not { } p ? "sem posição"
            : string.Create(CultureInfo.InvariantCulture, $"chave \"{(p.ChaveMonitor is null ? "(nula)" : EscaparParaMensagem(p.ChaveMonitor))}\" ({p.ChaveMonitor?.Length}) frações ({p.FracaoX:R}; {p.FracaoY:R}) âncora {p.AncoraAbsoluta} tela {p.TelaDoMonitor?.ToString() ?? "desconhecida"}");
        return $"{posicao}; preferências {c.Preferencias?.ToString() ?? "(nulas)"}";
    }

    private static string EscaparParaMensagem(string texto)
        => string.Concat(texto.Select(ch => ch is >= ' ' and < '\u007F' ? ch.ToString() : $"\\u{(int)ch:X4}"));

    private static void Verificar(bool condicao, Func<string> mensagem)
    {
        if (!condicao) Afirmar.Falhar(mensagem());
    }

    /// <summary>Documento só com a energia, dada como texto (escapado para JSON).</summary>
    private static string ComEnergia(string texto) => $$$"""{"schemaVersion": 1, "preferencias": {"energia": "{{{JsonEncodedText.Encode(texto)}}}"}}""";

    private static byte[] Utf8(string texto) => Encoding.UTF8.GetBytes(texto);

    /// <summary>
    /// Documento válido com <paramref name="profundidade"/> objetos aninhados, contando a raiz: a raiz traz
    /// <c>schemaVersion</c> 1 e um campo desconhecido <c>x</c> com o resto da pilha.
    /// </summary>
    private static string Aninhado(int profundidade)
        => "{\"schemaVersion\":1,\"x\":" + string.Concat(Enumerable.Repeat("{\"x\":", profundidade - 2)) + "{}" + new string('}', profundidade - 2) + "}";

    private static void AfirmarIlegivel(LeituraDasConfiguracoes lida, string motivo, string caso)
    {
        Afirmar.Igual(SituacaoDaLeitura.Ilegivel, lida.Situacao, $"{caso}: situação");
        Afirmar.Igual(motivo, lida.MotivoIlegivel, $"{caso}: motivo");
        Afirmar.Nulo(lida.Versao, $"{caso}: versão");
        Afirmar.Igual(ConfiguracoesSalvas.Padrao, lida.Configuracoes, $"{caso}: configurações padrão");
    }

    /// <summary>A posição S2: DISPLAY2 de SecundarioAEsquerda, a 25% da área útil, no chão, como a execução a descreve.</summary>
    private static PosicaoDoPersonagem PosicaoS2()
        => Posicionador.Descrever(Posicionador.NoMonitor(Afirmar.NaoNulo(SecundarioAEsquerda.PorChave(Display2)), 0.25, 1, Sprite));

    /// <summary>Caminho da amostra v1 na pasta-fonte.</summary>
    private static string CaminhoDaAmostra() => Path.Combine(ReproducaoTestes.PastaDasFontes(), "Persistencia", "Amostras", "settings-v1.json");

    /// <summary>A amostra v1 como texto, em UTF-8 estrito sem BOM e com o fim de linha normalizado para \n.</summary>
    private static string Amostra()
    {
        byte[] bytes = File.ReadAllBytes(CaminhoDaAmostra());
        Afirmar.Falso(bytes.AsSpan().StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF]), "a amostra não tem BOM");
        string texto = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes);
        return texto.Replace("\r\n", "\n", StringComparison.Ordinal);
    }
}

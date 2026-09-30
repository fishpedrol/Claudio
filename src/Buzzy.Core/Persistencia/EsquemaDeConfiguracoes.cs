using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Unicode;
using Buzzy.Core.Personagem;

namespace Buzzy.Core.Persistencia;

/// <summary>
/// Esquema v1 do settings.json (Fase 5; ARCHITECTURE.md 2.12; SECURITY.md 7): converte bytes em
/// <see cref="ConfiguracoesSalvas"/> e de volta, sem E/S.
///
/// A leitura é tolerante campo a campo e nunca lança. Só é ilegível o arquivo grande demais, fora de
/// UTF-8, que não é JSON (comentários e vírgula final são aceitos), fundo demais, sem objeto na raiz ou
/// sem um <c>schemaVersion</c> inteiro positivo. No resto, um campo desconhecido é ignorado, um repetido
/// vale na primeira ocorrência, um número fora da faixa é preso ao limite e um tipo errado vale o padrão
/// do campo; a posição é tudo ou nada nos campos obrigatórios. Cada caso gera um aviso.
///
/// A escrita produz sempre o mesmo texto para o mesmo conteúdo: UTF-8 sem BOM, indentação de 2
/// espaços, fim de linha <c>\n</c> (também no fim do arquivo), campos numa ordem fixa e números no
/// formato mais curto que reproduz o valor, sem depender da cultura.
///
/// Sem JsonSerializer e sem reflexão: a leitura extrai campo a campo de um <see cref="JsonDocument"/>, e
/// a escrita usa <see cref="Utf8JsonWriter"/>.
/// </summary>
public static class EsquemaDeConfiguracoes
{
    /// <summary>Versão escrita no campo <c>schemaVersion</c>. Toda ampliação do esquema a incrementa.</summary>
    public const int VersaoAtual = 1;

    /// <summary>Tamanho máximo do arquivo, contando um BOM; maior, é ilegível sem ser interpretado.</summary>
    public const int TamanhoMaximoEmBytes = 65_536;

    /// <summary>Profundidade máxima de objetos e listas aninhados, contando a raiz; mais funda, o arquivo é ilegível.</summary>
    public const int ProfundidadeMaxima = 8;

    /// <summary>Comprimento máximo da chave do monitor, em caracteres UTF-16.</summary>
    public const int ComprimentoMaximoDaChave = 1_024;

    /// <summary>Faixa das coordenadas gravadas (âncora e tela do monitor), em pixels físicos.</summary>
    public const int CoordenadaMinima = -32_768, CoordenadaMaxima = 32_767;

    // Campos do esquema v1, na ordem em que são escritos.
    private static readonly string[] CamposDaRaiz = ["schemaVersion", "posicao", "preferencias"];
    private static readonly string[] CamposDaPosicao = ["chaveMonitor", "telaDoMonitor", "fracaoX", "fracaoY", "ancoraAbsoluta"];
    private static readonly string[] CamposDaTela = ["esquerda", "topo", "direita", "base"];
    private static readonly string[] CamposDaAncora = ["x", "y"];
    private static readonly string[] CamposDasPreferencias = ["energia", "modoTelaCheia", "atravessarMonitores"];

    private static readonly NivelDeEnergia[] NiveisDeEnergia = [NivelDeEnergia.Baixa, NivelDeEnergia.Media, NivelDeEnergia.Alta];

    private static readonly JsonDocumentOptions OpcoesDeLeitura = new()
    {
        MaxDepth = ProfundidadeMaxima,
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly JsonWriterOptions OpcoesDeEscrita = new()
    {
        Indented = true,
        IndentCharacter = ' ',
        IndentSize = 2,
        NewLine = "\n",
    };

    private static ReadOnlySpan<byte> Bom => [0xEF, 0xBB, 0xBF];

    /// <summary>
    /// Lê o settings.json. Nunca lança: um arquivo que não se consegue ler é
    /// <see cref="SituacaoDaLeitura.Ilegivel"/>, com as configurações padrão.
    /// </summary>
    public static LeituraDasConfiguracoes Ler(ReadOnlyMemory<byte> conteudo)
    {
        if (conteudo.Length > TamanhoMaximoEmBytes) return Ilegivel("tamanho");
        if (conteudo.Span.StartsWith(Bom)) conteudo = conteudo[Bom.Length..];
        if (!Utf8.IsValid(conteudo.Span)) return Ilegivel("utf8");

        JsonDocument documento;
        try
        {
            documento = JsonDocument.Parse(conteudo, OpcoesDeLeitura);
        }
        catch (JsonException)
        {
            return Ilegivel("json");
        }

        using (documento)
        {
            try
            {
                return Extrair(documento.RootElement);
            }
            catch (Exception)
            {
                // Qualquer outra falha ao extrair, como um escape que não vira UTF-16 válido: a leitura
                // nunca lança, e o arquivo conta como JSON ilegível.
                return Ilegivel("json");
            }
        }
    }

    /// <summary>Bytes do settings.json.</summary>
    public static byte[] Escrever(ConfiguracoesSalvas configuracoes)
    {
        ArgumentNullException.ThrowIfNull(configuracoes);
        var saida = new ArrayBufferWriter<byte>(512);
        using (var json = new Utf8JsonWriter(saida, OpcoesDeEscrita))
        {
            json.WriteStartObject();
            json.WriteNumber("schemaVersion", VersaoAtual);
            if (configuracoes.Posicao is { } p)
            {
                json.WriteStartObject("posicao");
                json.WriteString("chaveMonitor", p.ChaveMonitor);
                if (p.TelaDoMonitor is { } tela)
                {
                    json.WriteStartObject("telaDoMonitor");
                    json.WriteNumber("esquerda", tela.Esquerda);
                    json.WriteNumber("topo", tela.Topo);
                    json.WriteNumber("direita", tela.Direita);
                    json.WriteNumber("base", tela.Base);
                    json.WriteEndObject();
                }
                json.WriteNumber("fracaoX", p.FracaoX);
                json.WriteNumber("fracaoY", p.FracaoY);
                json.WriteStartObject("ancoraAbsoluta");
                json.WriteNumber("x", p.AncoraAbsoluta.X);
                json.WriteNumber("y", p.AncoraAbsoluta.Y);
                json.WriteEndObject();
                json.WriteEndObject();
            }
            json.WriteStartObject("preferencias");
            json.WriteString("energia", NomeDaEnergia(configuracoes.Preferencias.Energia));
            json.WriteBoolean("modoTelaCheia", configuracoes.Preferencias.ModoTelaCheia);
            json.WriteBoolean("atravessarMonitores", configuracoes.Preferencias.AtravessarMonitores);
            json.WriteEndObject();
            json.WriteEndObject();
        }
        return [.. saida.WrittenSpan, (byte)'\n'];
    }

    /// <summary>Nome do nível no arquivo: <c>"baixa"</c>, <c>"media"</c> ou <c>"alta"</c>; fora dos três, <c>"media"</c>.</summary>
    public static string NomeDaEnergia(NivelDeEnergia nivel) => nivel switch
    {
        NivelDeEnergia.Baixa => "baixa",
        NivelDeEnergia.Alta => "alta",
        _ => "media",
    };

    /// <summary>
    /// Nível pelos três nomes do arquivo (<see cref="NomeDaEnergia"/>), sem diferenciar maiúsculas, e nunca
    /// pelo Enum.Parse, que aceitaria <c>"1"</c> e <c>"Baixa,Alta"</c>. Falso, <paramref name="nivel"/> é
    /// Média, o padrão seguro (SECURITY.md 7).
    /// </summary>
    public static bool TentarLerEnergia(string texto, out NivelDeEnergia nivel)
    {
        ArgumentNullException.ThrowIfNull(texto);
        foreach (NivelDeEnergia candidato in NiveisDeEnergia)
        {
            if (string.Equals(texto, NomeDaEnergia(candidato), StringComparison.OrdinalIgnoreCase))
            {
                nivel = candidato;
                return true;
            }
        }
        nivel = Preferencias.Padrao.Energia;
        return false;
    }

    // ---------------------------------------------------------------- leitura campo a campo

    private static LeituraDasConfiguracoes Extrair(JsonElement raiz)
    {
        if (raiz.ValueKind != JsonValueKind.Object) return Ilegivel("raiz");

        var avisos = new List<string>();
        JsonElement?[] campos = Campos(raiz, CamposDaRaiz, "", avisos);
        if (campos[0] is not { ValueKind: JsonValueKind.Number } versaoLida || !versaoLida.TryGetInt32(out int versao) || versao < 1)
            return Ilegivel("schemaVersion");

        var configuracoes = new ConfiguracoesSalvas(LerPosicao(campos[1], avisos), LerPreferencias(campos[2], avisos));
        SituacaoDaLeitura situacao = versao > VersaoAtual ? SituacaoDaLeitura.VersaoFutura : SituacaoDaLeitura.Valida;
        return new LeituraDasConfiguracoes(situacao, versao, configuracoes, avisos.AsReadOnly(), null);
    }

    private static LeituraDasConfiguracoes Ilegivel(string motivo)
        => new(SituacaoDaLeitura.Ilegivel, null, ConfiguracoesSalvas.Padrao, [], motivo);

    /// <summary>
    /// A primeira ocorrência de cada campo do esquema no objeto, na ordem de <paramref name="nomes"/>; nula
    /// quando ausente. O nome precisa ser igual, diferenciando maiúsculas. Uma repetição e um campo
    /// desconhecido só geram aviso, com o nome do esquema e nunca um nome ou valor do arquivo.
    /// </summary>
    private static JsonElement?[] Campos(JsonElement objeto, string[] nomes, string onde, List<string> avisos)
    {
        var achados = new JsonElement?[nomes.Length];
        foreach (JsonProperty campo in objeto.EnumerateObject())
        {
            int i = 0;
            while (i < nomes.Length && !campo.NameEquals(nomes[i])) i++;
            if (i == nomes.Length)
                avisos.Add(onde.Length == 0 ? "campo desconhecido na raiz: ignorado" : $"campo desconhecido em {onde}: ignorado");
            else if (achados[i] is not null)
                avisos.Add($"campo repetido: {(onde.Length == 0 ? nomes[i] : $"{onde}.{nomes[i]}")}; vale o primeiro");
            else
                achados[i] = campo.Value;
        }
        return achados;
    }

    /// <summary>
    /// A posição, tudo ou nada nos campos obrigatórios: sem a chave ou uma das frações, ou com uma delas
    /// inválida, não há posição. A tela do monitor e a âncora são opcionais: inválidas, valem desconhecida
    /// e (0, 0), porque a partida recalcula a âncora de qualquer forma (Posicionador.Restaurar).
    /// </summary>
    private static PosicaoDoPersonagem? LerPosicao(JsonElement? valor, List<string> avisos)
    {
        if (valor is not { } posicao || posicao.ValueKind == JsonValueKind.Null) return null;
        if (posicao.ValueKind != JsonValueKind.Object)
        {
            avisos.Add("posicao: não é um objeto; sem posição");
            return null;
        }

        JsonElement?[] campos = Campos(posicao, CamposDaPosicao, "posicao", avisos);
        string? chave = campos[0] is { ValueKind: JsonValueKind.String } texto ? texto.GetString() : null;
        if (!ChaveValida(chave))
        {
            avisos.Add("posicao.chaveMonitor: ausente ou inválida; sem posição");
            return null;
        }
        if (!LerFracao(campos[2], "posicao.fracaoX", avisos, out double fracaoX) || !LerFracao(campos[3], "posicao.fracaoY", avisos, out double fracaoY))
            return null;
        return new PosicaoDoPersonagem(chave, fracaoX, fracaoY, LerAncora(campos[4], avisos)) { TelaDoMonitor = LerTela(campos[1], avisos) };
    }

    /// <summary>Fração obrigatória: um número finito, preso em [0, 1]. Falso se ausente ou de outro tipo.</summary>
    private static bool LerFracao(JsonElement? valor, string nome, List<string> avisos, out double fracao)
    {
        if (valor is { ValueKind: JsonValueKind.Number } numero && numero.TryGetDouble(out double lida) && double.IsFinite(lida))
        {
            fracao = Fracao(lida);
            if (fracao != lida) avisos.Add($"{nome}: fora de [0, 1]; presa ao limite");
            return true;
        }
        avisos.Add($"{nome}: ausente ou não é um número finito; sem posição");
        fracao = 0;
        return false;
    }

    /// <summary>
    /// Tela do monitor da época, opcional: os quatro lados inteiros, presos na faixa das coordenadas, com
    /// esquerda menor que direita e topo menor que base. Qualquer outra coisa vale desconhecida.
    /// </summary>
    private static RetanguloPx? LerTela(JsonElement? valor, List<string> avisos)
    {
        if (valor is not { } tela || tela.ValueKind == JsonValueKind.Null) return null;
        if (tela.ValueKind == JsonValueKind.Object)
        {
            JsonElement?[] lados = Campos(tela, CamposDaTela, "posicao.telaDoMonitor", avisos);
            if (LerCoordenada(lados[0], "posicao.telaDoMonitor.esquerda", avisos, out int esquerda)
                && LerCoordenada(lados[1], "posicao.telaDoMonitor.topo", avisos, out int topo)
                && LerCoordenada(lados[2], "posicao.telaDoMonitor.direita", avisos, out int direita)
                && LerCoordenada(lados[3], "posicao.telaDoMonitor.base", avisos, out int baseY)
                && new RetanguloPx(esquerda, topo, direita, baseY) is { Vazio: false } lida)
                return lida;
        }
        avisos.Add("posicao.telaDoMonitor: inválida; tela desconhecida");
        return null;
    }

    /// <summary>Âncora absoluta, opcional: x e y inteiros, presos na faixa das coordenadas; senão, (0, 0).</summary>
    private static PontoPx LerAncora(JsonElement? valor, List<string> avisos)
    {
        if (valor is not { } ancora || ancora.ValueKind == JsonValueKind.Null) return default;
        if (ancora.ValueKind == JsonValueKind.Object)
        {
            JsonElement?[] eixos = Campos(ancora, CamposDaAncora, "posicao.ancoraAbsoluta", avisos);
            if (LerCoordenada(eixos[0], "posicao.ancoraAbsoluta.x", avisos, out int x) && LerCoordenada(eixos[1], "posicao.ancoraAbsoluta.y", avisos, out int y))
                return new PontoPx(x, y);
        }
        avisos.Add("posicao.ancoraAbsoluta: inválida; vale (0, 0)");
        return default;
    }

    /// <summary>Coordenada: um número inteiro, preso na faixa das coordenadas. Falso se ausente ou de outro tipo.</summary>
    private static bool LerCoordenada(JsonElement? valor, string nome, List<string> avisos, out int coordenada)
    {
        coordenada = 0;
        if (valor is not { ValueKind: JsonValueKind.Number } numero || !numero.TryGetInt64(out long lida)) return false;
        coordenada = Coordenada(lida);
        if (coordenada != lida) avisos.Add($"{nome}: fora da faixa; presa ao limite");
        return true;
    }

    /// <summary>Preferências campo a campo: ausente vale o padrão, sem aviso; inválido vale o padrão, com aviso.</summary>
    private static Preferencias LerPreferencias(JsonElement? valor, List<string> avisos)
    {
        Preferencias padrao = Preferencias.Padrao;
        if (valor is not { } preferencias || preferencias.ValueKind == JsonValueKind.Null) return padrao;
        if (preferencias.ValueKind != JsonValueKind.Object)
        {
            avisos.Add("preferencias: não é um objeto; valem os padrões");
            return padrao;
        }

        JsonElement?[] campos = Campos(preferencias, CamposDasPreferencias, "preferencias", avisos);
        NivelDeEnergia energia = padrao.Energia;
        if (campos[0] is { } nivel && (nivel.ValueKind != JsonValueKind.String || !TentarLerEnergia(nivel.GetString()!, out energia)))
            avisos.Add("preferencias.energia: não é baixa, media nem alta; vale media");
        return new Preferencias(
            energia,
            LerBooleano(campos[1], "preferencias.modoTelaCheia", padrao.ModoTelaCheia, avisos),
            LerBooleano(campos[2], "preferencias.atravessarMonitores", padrao.AtravessarMonitores, avisos));
    }

    private static bool LerBooleano(JsonElement? valor, string nome, bool padrao, List<string> avisos)
    {
        switch (valor?.ValueKind)
        {
            case null:
                return padrao;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            default:
                avisos.Add($"{nome}: não é true nem false; vale o padrão");
                return padrao;
        }
    }

    // ---------------------------------------------------------------- regras de valor

    /// <summary>
    /// Chave gravável: de 1 a <see cref="ComprimentoMaximoDaChave"/> caracteres, sem caractere de controle e
    /// em UTF-16 válido (um surrogate solto não tem representação em JSON).
    /// </summary>
    private static bool ChaveValida([NotNullWhen(true)] string? chave)
    {
        if (string.IsNullOrEmpty(chave) || chave.Length > ComprimentoMaximoDaChave) return false;
        for (int i = 0; i < chave.Length; i++)
        {
            if (char.IsControl(chave[i])) return false;
            if (char.IsHighSurrogate(chave[i]) && i + 1 < chave.Length && char.IsLowSurrogate(chave[i + 1])) i++;
            else if (char.IsSurrogate(chave[i])) return false;
        }
        return true;
    }

    /// <summary>
    /// Fração saneada como na restauração (<see cref="Posicionador.SanearFracao"/>), sem o zero negativo, que
    /// o arquivo escreveria como <c>-0</c>.
    /// </summary>
    private static double Fracao(double fracao)
    {
        double saneada = Posicionador.SanearFracao(fracao);
        return saneada == 0 ? 0 : saneada;
    }

    private static int Coordenada(long valor) => (int)Math.Clamp(valor, CoordenadaMinima, CoordenadaMaxima);
}

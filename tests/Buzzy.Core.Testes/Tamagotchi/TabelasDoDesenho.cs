using System.Globalization;
using Buzzy.Core.Personagem;

namespace Buzzy.Core.Testes.Tamagotchi;

/// <summary>
/// As tabelas do desenho do núcleo para o tamagotchi adulto (DEC-028; tabelas 4.1 a 4.4), transcritas aqui como texto,
/// no formato do desenho e à parte do código do núcleo: é a fonte independente dos testes. Os passos do uso são os do
/// coordenador, por verbo, para a arte e o núcleo concordarem. Tudo é de desenho animado: os números são de jogo.
/// </summary>
internal static class TabelasDoDesenho
{
    /// <summary>Passos do uso por verbo (60 por segundo), fixados pelo coordenador: a soma dos quadros de cada verbo na arte.</summary>
    public static readonly IReadOnlyDictionary<VerboDeUso, int> PassosPorVerbo = new Dictionary<VerboDeUso, int>
    {
        [VerboDeUso.Comer] = 150,
        [VerboDeUso.Beber] = 120,
        [VerboDeUso.Fumar] = 210,
        [VerboDeUso.Cheirar] = 120,
        [VerboDeUso.Engolir] = 90,
        [VerboDeUso.Inalar] = 120,
    };

    // 4.1, com o verbo do coordenador: item | verbo | cara durante | onda | intensidade | duração sozinho (s).
    private static readonly string[] Itens =
    [
        "Banana | Comer | Feliz | Satisfeito | 1 | 63",
        "Agua | Beber | Feliz | - | 0 | -",
        "Vodka | Beber | Determinado | Bebado | 2 | 320.5",
        "Cerveja | Beber | Feliz | Bebado | 1 | 198",
        "Baseado | Fumar | Pensativo | Chapado | 2 | 342.5",
        "Cigarro | Fumar | Pensativo | Relaxado | 1 | 63",
        "Cocaina | Cheirar | Surpreso | Eletrico | 2 | 265.5",
        "Md | Engolir | Travesso | Euforico | 2 | 385",
        "LancaPerfume | Inalar | Surpreso | Tonto | 2 | 43.5",
        "Cafe | Beber | Determinado | Ligado | 1 | 125",
        "Energetico | Beber | Empolgado | Ligado | 2 | 211.25",
        "Cogumelo | Comer | Curioso | Viajando | 2 | 375",
        "Bala | Engolir | Feliz | Alegre | 1 | 62",
    ];

    // 4.2: onda | precedência | subida (s) | cada nível do pico (s) | queda base (s) | cara na subida | cara no pico | cara na queda.
    private static readonly string[] Ondas =
    [
        "Satisfeito | 1 | 3 | 60 | - | Feliz | Feliz | -",
        "Alegre | 1 | 2 | 40 | 20 | Empolgado | Empolgado | Entediado",
        "Relaxado | 1 | 3 | 60 | - | Pensativo | Pensativo | -",
        "Ligado | 2 | 5 | 75 | 45 | Surpreso | Determinado | Sonolento",
        "Bebado | 3 | 8 | 100 | 90 | Feliz | Bebado | Enjoado",
        "Chapado | 3 | 10 | 110 | 90 | Pensativo | Chapado | Sonolento",
        "Eletrico | 3 | 3 | 75 | 90 | Surpreso | Eletrico | Entediado",
        "Euforico | 3 | 15 | 110 | 120 | Feliz | Apaixonado | Entediado",
        "Tonto | 3 | 1 | 15 | 10 | Surpreso | Tonto | Sonolento",
        "Viajando | 3 | 20 | 140 | 60 | Curioso | Viajando | Pensativo",
    ];

    // 4.3, pico ("a/b/c" = níveis 1, 2 e 3; a subida usa o nível 1): onda | intervalo | descanso | andar | escalar | pular |
    // descansar | gesto | troca de cara | velocidade | cambaleio | foguete | altura do pulo.
    private static readonly string[] PerfisDoPico =
    [
        "Satisfeito | 100 | 100 | 100 | 100 | 100 | 150 | 150 | 150 | 100 | 0 | perfil | 100",
        "Alegre | 60/50/40 | 60 | 130 | 130 | 200 | 30 | 150 | 100 | 120/125/130 | 0 | perfil | 120",
        "Relaxado | 130 | 120 | 70 | 50 | 30 | 150 | 120 | 100 | 90 | 0 | perfil | 100",
        "Ligado | 70/55/40 | 60/45/30 | 150 | 150 | 150/200/250 | 30/15/5 | 120 | 100 | 115/130/145 | 0 | 40/50/60 | 110/125/140",
        "Bebado | 100 | 120 | 130 | 40 | 40 | 120 | 200 | 150 | 80/70/60 | 60/90/120 | 5 | 80",
        "Chapado | 160 | 150 | 60 | 30 | 20 | 200 | 150 | 120 | 60/55/50 | 0 | 0 | 80",
        "Eletrico | 35/28/20 | 30/20/10 | 200 | 200 | 180 | 10/5/5 | 150 | 200 | 170/185/200 | 0 | 60/70/80 | 120/130/140",
        "Euforico | 60 | 50 | 120 | 100 | 150 | 30 | 250 | 150 | 120 | 0 | perfil | 120",
        "Tonto | 50 | 100 | 50 | 0 | 0 | 100 | 300 | 200 | 60 | 100 | 0 | 100",
        "Viajando | 130 | 120 | 80 | 80 | 60 | 100 | 200 | 250 | 70 | 0 | 20 | 100",
    ];

    // 4.3, queda (não depende do nível; altura do pulo 100 e foguete do perfil, salvo onde indicado), mesmas colunas.
    private static readonly string[] PerfisDaQueda =
    [
        "Alegre | 120 | 150 | 80 | 60 | 50 | 200 | 80 | 100 | 90 | 0 | perfil | 100",
        "Ligado | 130 | 150 | 70 | 50 | 40 | 200 | 80 | 100 | 85 | 0 | perfil | 100",
        "Bebado | 150 | 200 | 60 | 20 | 10 | 250 | 80 | 80 | 75 | 30 | 5 | 100",
        "Chapado | 150 | 200 | 60 | 30 | 20 | 300 | 80 | 100 | 70 | 0 | 0 | 100",
        "Eletrico | 150 | 180 | 60 | 40 | 30 | 250 | 80 | 100 | 80 | 0 | perfil | 100",
        "Euforico | 140 | 150 | 70 | 60 | 50 | 200 | 80 | 100 | 85 | 0 | perfil | 100",
        "Tonto | 100 | 100 | 80 | 50 | 50 | 120 | 80 | 100 | 80 | 0 | perfil | 100",
        "Viajando | 120 | 120 | 80 | 70 | 60 | 150 | 100 | 150 | 85 | 0 | perfil | 100",
    ];

    // 4.4: onda | gestos no pico | caras no pico | gestos na queda | caras na queda. Na subida, os gestos são os do pico e
    // a cara é só a da subida.
    private static readonly string[] GestosECaras =
    [
        "Satisfeito | Cocar 2, Espreguicar 2, Brincar 1 | Feliz 4, Rindo 1, Travesso 1, Sonolento 1 | - | -",
        "Alegre | Brincar 2, Danca 2, Gargalhada 1 | Empolgado 3, Rindo 2, Feliz 2 | Espreguicar 1 | Entediado 2, Sonolento 2",
        "Relaxado | Espreguicar 2, Tosse 1, OlharAoRedor 1 | Pensativo 2, Neutro 2, Sonolento 1, Feliz 1 | - | -",
        "Ligado | Tremedeira 2, OlharAoRedor 2, Brincar 1 | Determinado 2, Empolgado 2, Surpreso 1 | Espreguicar 2, Cocar 1 | Sonolento 3, Bocejando 1",
        "Bebado | Soluco 3, Danca 1, Gargalhada 1 | Bebado 4, Rindo 2, Feliz 1, Sonolento 1 | Soluco 1, Espreguicar 1 | Enjoado 3, Sonolento 2, Entediado 1",
        "Chapado | Gargalhada 3, OlharAoRedor 1, Cocar 1 | Chapado 4, Rindo 2, Pensativo 1, Sonolento 1 | Espreguicar 2 | Sonolento 3, Bocejando 2, Pensativo 1",
        "Eletrico | Tremedeira 3, Espirro 1, OlharAoRedor 1 | Eletrico 4, Determinado 1, Surpreso 1, Empolgado 1 | Espreguicar 1, Cocar 1 | Entediado 3, Sonolento 2, Pensativo 1",
        "Euforico | Danca 4, Brincar 1 | Apaixonado 3, Empolgado 2, Feliz 1, Rindo 1 | Espreguicar 1, OlharAoRedor 1 | Entediado 2, Pensativo 2, Sonolento 1",
        "Tonto | Gargalhada 2, OlharAoRedor 1 | Tonto 4, Rindo 2 | OlharAoRedor 1 | Sonolento 1, Surpreso 1, Neutro 1",
        "Viajando | OlharAoRedor 2, Danca 1, Espiar 1 | Viajando 4, Surpreso 1, Pensativo 1, Curioso 1, Rindo 1 | OlharAoRedor 2, Espiar 1 | Pensativo 3, Curioso 1, Sonolento 1",
    ];

    /// <summary>Uma linha da tabela 4.1.</summary>
    public sealed record ItemEsperado(Item Item, VerboDeUso Verbo, Expressao CaraDurante, Onda? Onda, int Intensidade, TimeSpan? DuracaoSozinho);

    /// <summary>
    /// Uma onda das tabelas 4.2 a 4.4: tempos, caras e os perfis esperados já na linha canônica de
    /// <see cref="Descrever(PerfilDaOnda)"/> (um por nível do pico e o da queda, nulo sem queda).
    /// </summary>
    public sealed record OndaEsperada(
        Onda Onda, int Precedencia, TimeSpan Subida, TimeSpan NivelDoPico, TimeSpan? QuedaBase,
        Expressao CaraDaSubida, Expressao CaraDoPico, Expressao? CaraDaQueda, IReadOnlyList<string> PicoPorNivel, string? Queda)
    {
        /// <summary>A linha canônica do perfil da subida: o do nível 1 do pico, com a cara da subida só.</summary>
        public string Subir => PicoPorNivel[0][..PicoPorNivel[0].IndexOf(" caras=", StringComparison.Ordinal)] + $" caras=[{CaraDaSubida} 1]";

        /// <summary>
        /// Os atrasos do temporizador da onda num episódio sozinho, começando na subida no nível dado: a subida, um por
        /// nível do pico e a queda, base × 100, 125 ou 150% pelo pior nível (o próprio nível, sem item novo).
        /// </summary>
        public IReadOnlyList<TimeSpan> Atrasos(int nivel)
        {
            var atrasos = new List<TimeSpan> { Subida };
            for (int i = 0; i < nivel; i++) atrasos.Add(NivelDoPico);
            if (QuedaBase is { } q) atrasos.Add(TimeSpan.FromTicks(q.Ticks * (100 + 25 * (nivel - 1)) / 100));
            return atrasos;
        }
    }

    /// <summary>A tabela 4.1, na ordem do desenho.</summary>
    public static IReadOnlyList<ItemEsperado> ItensEsperados() =>
    [
        .. Itens.Select(Celulas).Select(c => new ItemEsperado(
            Enum.Parse<Item>(c[0]), Enum.Parse<VerboDeUso>(c[1]), Enum.Parse<Expressao>(c[2]),
            c[3] == "-" ? null : Enum.Parse<Onda>(c[3]), int.Parse(c[4], CultureInfo.InvariantCulture),
            c[5] == "-" ? null : TimeSpan.FromMilliseconds(double.Parse(c[5], CultureInfo.InvariantCulture) * 1000))),
    ];

    /// <summary>As ondas das tabelas 4.2 a 4.4, na ordem do desenho.</summary>
    public static IReadOnlyList<OndaEsperada> OndasEsperadas() => [.. Ondas.Select(Celulas).Select(MontarOnda)];

    /// <summary>A onda esperada pelo nome.</summary>
    public static OndaEsperada Esperada(Onda onda) => OndasEsperadas().Single(o => o.Onda == onda);

    /// <summary>
    /// Linha canônica de um perfil de fase, com os percentuais na ordem das colunas da tabela 4.3 e os conjuntos da 4.4,
    /// para comparar o do núcleo com o transcrito.
    /// </summary>
    public static string Descrever(PerfilDaOnda p)
        => Linha(p.Intervalo, p.Descanso, p.Andar, p.Escalar, p.Pular, p.Descansar, p.Gesticular, p.TrocarCara, p.Velocidade, p.Cambaleio,
            p.ChanceDoFoguete?.ToString(CultureInfo.InvariantCulture) ?? "perfil", p.AlturaDoPulo,
            string.Join(", ", p.Gestos.Select(g => $"{g.Gesto} {g.Peso}")), string.Join(", ", p.Caras.Select(c => $"{c.Cara} {c.Peso}")));

    private static OndaEsperada MontarOnda(string[] c)
    {
        Onda onda = Enum.Parse<Onda>(c[0]);
        string[] pico = Celulas(PerfisDoPico.Single(l => l.StartsWith(c[0] + " ", StringComparison.Ordinal)));
        string[]? queda = PerfisDaQueda.Where(l => l.StartsWith(c[0] + " ", StringComparison.Ordinal)).Select(Celulas).SingleOrDefault();
        string[] conjuntos = Celulas(GestosECaras.Single(l => l.StartsWith(c[0] + " ", StringComparison.Ordinal)));
        // Uma célula "a/b/c" vale a, b ou c conforme o nível; uma célula simples vale para os três.
        string Nivel(string celula, int nivel) => celula.Split('/') is { Length: 3 } niveis ? niveis[nivel - 1] : celula;
        string PerfilNoNivel(string[] colunas, int nivel, string gestos, string caras)
        {
            int N(int coluna) => int.Parse(Nivel(colunas[coluna], nivel), CultureInfo.InvariantCulture);
            return Linha(N(1), N(2), N(3), N(4), N(5), N(6), N(7), N(8), N(9), N(10), Nivel(colunas[11], nivel), N(12), gestos, caras);
        }

        string[] porNivel = [.. Enumerable.Range(1, 3).Select(n => PerfilNoNivel(pico, n, conjuntos[1], conjuntos[2]))];
        if ((queda is null) != (c[4] == "-") || (queda is null) != (conjuntos[3] == "-"))
            throw new InvalidOperationException($"Transcrição inconsistente da queda de {onda}.");
        return new OndaEsperada(
            onda, int.Parse(c[1], CultureInfo.InvariantCulture), Segundos(c[2]), Segundos(c[3]), c[4] == "-" ? null : Segundos(c[4]),
            Enum.Parse<Expressao>(c[5]), Enum.Parse<Expressao>(c[6]), c[7] == "-" ? null : Enum.Parse<Expressao>(c[7]),
            porNivel, queda is null ? null : PerfilNoNivel(queda, 1, conjuntos[3], conjuntos[4]));
    }

    private static string Linha(int intervalo, int descanso, int andar, int escalar, int pular, int descansar, int gesto, int troca,
        int velocidade, int cambaleio, string foguete, int pulo, string gestos, string caras)
        => $"intervalo={intervalo} descanso={descanso} andar={andar} escalar={escalar} pular={pular} descansar={descansar} gesto={gesto} "
            + $"troca={troca} velocidade={velocidade} cambaleio={cambaleio} foguete={foguete} pulo={pulo} gestos=[{gestos}] caras=[{caras}]";

    private static TimeSpan Segundos(string celula) => TimeSpan.FromSeconds(int.Parse(celula, CultureInfo.InvariantCulture));

    private static string[] Celulas(string linha) => [.. linha.Split('|').Select(c => c.Trim())];
}

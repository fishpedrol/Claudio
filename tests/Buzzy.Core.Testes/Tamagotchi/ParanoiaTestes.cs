using Buzzy.Core.Personagem;
using Buzzy.Core.Testes.Movimento;
using Buzzy.Core.Testes.Personagem;
using Buzzy.Testes;
using static Buzzy.Core.Testes.Tamagotchi.ApoioDosItens;

namespace Buzzy.Core.Testes.Tamagotchi;

/// <summary>
/// A paranoia (pedido do usuário de 2026-10-01, "caso o macaco use muitas coisas ele fica paranoico, como o meme 'os cara
/// tá no teto' mas de forma engraçada"; decisão do coordenador para a DEC-028). De desenho animado: ele acha que tem alguém
/// no teto. Cada item de substância soma 1 à carga do episódio, depois da combinação de sempre; a comida e a bebida sem
/// álcool não somam. Na 4ª substância, sem a paranoia na frente, ela vai para a frente, na subida do nível 1, e a frente
/// anterior vai para o fundo; com ela na frente, mais uma substância sobe um nível e recomeça o pico. A carga volta a 0 no
/// fim de todo evento sem onda de substância na frente nem no fundo. A onda tem subida de 1 s (assustado), pico de 40 s por
/// nível (paranoico) e queda de 15 s (sonolento); no pico, ele nunca escala, pula nem descansa. O uso que começa a paranoia,
/// se termina com ele em IDLE sem gesto, acaba com ele olhando pro teto, por 90 passos e sem sorteio. O esperado vem da
/// decisão, escrito aqui à parte do núcleo, com os números da tabela.
/// </summary>
internal static class ParanoiaTestes
{
    // ---------------------------------------------------------------- apoio

    /// <summary>Invoca o item, solta sobre ele e deixa o uso acabar; devolve o resultado do soltar.</summary>
    private static Resultado Usar(Cenario c, Item item)
    {
        ItemNoMundo it = InvocarEAssentar(c, item);
        c.SoltarSobreEle(it.Id).Esta(Estado.Using, $"usando {item}");
        Resultado soltar = Afirmar.NaoNulo(c.Ultimo, "o resultado do soltar");
        c.Passos(c.Atual.PassosRestantes).Esta(Estado.Idle, $"fim do uso de {item}");
        return soltar;
    }

    /// <summary>O disparo da onda na geração agendada; devolve o resultado.</summary>
    private static Resultado Disparar(Cenario c) => Afirmar.NaoNulo(c.Aplicar(new ItemEffectTimer(c.Atual.GeracaoDaOnda)).Ultimo, "o resultado do disparo");

    /// <summary>Os atrasos agendados para a onda no resultado.</summary>
    private static TimeSpan[] Agendados(Resultado r) => [.. r.Efeitos.OfType<AgendarOnda>().Select(a => a.Atraso)];

    /// <summary>Parado no chão, sem a física e com a autonomia pausada: só o que os testes mandam acontece.</summary>
    private static Cenario Pausado() => Cenario.Parado(SemFisica()).Aplicar(new CmdPauseAutonomy());

    /// <summary>
    /// O caminho mais curto até a paranoia: três cervejas (o bêbado na subida do nível 3, carga 3) e um cigarro, absorvido
    /// pelo bêbado (precedência 1 contra 3): a 4ª substância. Devolve o resultado do soltar do cigarro.
    /// </summary>
    private static Resultado AteAParanoia(Cenario c)
    {
        Usar(c, Item.Cerveja);
        Usar(c, Item.Cerveja);
        Usar(c, Item.Cerveja);
        Afirmar.Igual((new EstadoDaOnda(Onda.Bebado, FaseDaOnda.Subida, 3, 3), 3), (c.Atual.Onda, c.Atual.Carga), "três cervejas: o bêbado no nível 3, carga 3");
        return Usar(c, Item.Cigarro);
    }

    private static readonly EstadoDaOnda BebadoNaSubida3 = new(Onda.Bebado, FaseDaOnda.Subida, 3, 3);

    private static readonly EstadoDaOnda ParanoiaComecando = new(Onda.Paranoico, FaseDaOnda.Subida, 1, 1);

    // ---------------------------------------------------------------- o gatilho

    // O gatilho exato: a vodka, a cerveja e o cigarro somam a carga 1, 2 e 3 sem paranoia nenhuma (a regra do soltar é a de
    // sempre, e a onda é a da combinação); o baseado, a 4ª substância, primeiro combina como sempre (o chapado vai para a
    // frente, o bêbado para o fundo) e depois a paranoia começa na frente, na subida do nível 1, com o chapado no fundo (o
    // bêbado sai: só cabem duas). O temporizador é o da subida dela (1 s), o retrato leva a carga, e nada é sorteado.
    [Teste]
    public static void Gatilho_NaQuartaSubstanciaENaoAntes()
    {
        Cenario c = Pausado();
        (Item Item, EstadoDaOnda Frente, EstadoDaOnda? Fundo)[] antes =
        [
            (Item.Vodka, new(Onda.Bebado, FaseDaOnda.Subida, 2, 2), null),
            (Item.Cerveja, BebadoNaSubida3, null),
            (Item.Cigarro, BebadoNaSubida3, null),
        ];
        int carga = 0;
        foreach ((Item item, EstadoDaOnda frente, EstadoDaOnda? fundo) in antes)
        {
            Resultado r = Usar(c, item);
            carga++;
            Afirmar.Igual((frente, fundo, carga), (r.Estado.Onda, r.Estado.OndaDeFundo, r.Estado.Carga), $"{item}: a combinação de sempre, carga {carga}, sem paranoia");
            Afirmar.Falso(r.Transicoes.Single().Regra.Contains("paranoia", StringComparison.Ordinal), $"{item}: a regra de sempre ({r.Transicoes.Single().Regra})");
            Afirmar.Falso(r.Estado.Uso!.ComecouAParanoia, $"{item}: o uso não começou a paranoia");
            Afirmar.Igual(carga, c.Retrato.Carga, $"{item}: o retrato leva a carga");
        }

        ItemNoMundo baseado = InvocarEAssentar(c, Item.Baseado);
        Aleatorio gerador = c.Atual.Aleatorio;
        c.SoltarSobreEle(baseado.Id).Esta(Estado.Using, "usando o baseado");
        Resultado quarta = Afirmar.NaoNulo(c.Ultimo, "o soltar");
        Afirmar.Igual((ParanoiaComecando, new EstadoDaOnda(Onda.Chapado, FaseDaOnda.Subida, 2, 2), 4), (quarta.Estado.Onda, quarta.Estado.OndaDeFundo, quarta.Estado.Carga),
            "a 4ª substância: a paranoia na frente, na subida do nível 1, e a frente da combinação (o chapado) no fundo; o bêbado sai");
        Afirmar.Sequencia([new Transicao(Estado.Idle, Estado.Using, "ITEM_DRAG_END sobre o personagem: Fumar Baseado; a paranoia começa: Paranoico/Subida/1")], quarta.Transicoes, "a regra diz que a paranoia começou");
        Afirmar.Sequencia([TimeSpan.FromSeconds(1)], Agendados(quarta), "a subida da paranoia: 1 s");
        Afirmar.Verdadeiro(quarta.Estado.Uso!.ComecouAParanoia, "este uso começou a paranoia");
        Afirmar.Igual(Expressao.Pensativo, quarta.Estado.Expressao, "durante o uso, a cara de quem fuma");
        Afirmar.Igual(gerador, quarta.Estado.Aleatorio, "a paranoia não sorteia nada");
        Afirmar.Contem(" fundo=Chapado/Subida/2 carga=4 uso=Baseado/Fumar/0de210/Chao", c.Retrato.Descrever(), "a linha do retrato");
    }

    // Qualquer uma das oito substâncias é a 4ª: depois de três cervejas (o bêbado na subida do nível 3), cada uma combina
    // como sempre e a paranoia vai para a frente, com a frente da combinação no fundo: o bêbado (somado ou absorvendo o
    // cigarro) ou a onda de precedência igual que tomou a frente dele.
    [Teste]
    public static void Gatilho_QualquerSubstanciaEhAQuarta()
    {
        (Item Item, EstadoDaOnda Fundo)[] casos =
        [
            (Item.Vodka, BebadoNaSubida3),
            (Item.Cerveja, BebadoNaSubida3),
            (Item.Cigarro, BebadoNaSubida3),
            (Item.Baseado, new(Onda.Chapado, FaseDaOnda.Subida, 2, 2)),
            (Item.Cocaina, new(Onda.Eletrico, FaseDaOnda.Subida, 2, 2)),
            (Item.Md, new(Onda.Euforico, FaseDaOnda.Subida, 2, 2)),
            (Item.LancaPerfume, new(Onda.Tonto, FaseDaOnda.Subida, 2, 2)),
            (Item.Cogumelo, new(Onda.Viajando, FaseDaOnda.Subida, 2, 2)),
        ];
        Afirmar.Sequencia(TabelasDoDesenho.ItensEsperados().Where(i => !i.Alivio).Select(i => i.Item).Order(), casos.Select(x => x.Item).Order(), "as oito substâncias");
        foreach ((Item item, EstadoDaOnda fundo) in casos)
        {
            Cenario c = Pausado();
            Usar(c, Item.Cerveja);
            Usar(c, Item.Cerveja);
            Usar(c, Item.Cerveja);
            Resultado r = Usar(c, item);
            Afirmar.Igual((ParanoiaComecando, fundo, 4), (r.Estado.Onda, r.Estado.OndaDeFundo, r.Estado.Carga), $"{item} como a 4ª");
            Afirmar.Igual($"ITEM_DRAG_END sobre o personagem: {TabelasDoDesenho.ItensEsperados().Single(i => i.Item == item).Verbo} {item}; a paranoia começa: Paranoico/Subida/1",
                r.Transicoes.Single().Regra, $"{item}: a regra");
        }
    }

    // A comida e a bebida sem álcool não somam: com a carga em 3 (três cervejas), a banana, a bala, a água, o café e o
    // energético aliviam o bêbado um passo (o alívio de sempre) e a carga continua 3, sem paranoia; a substância seguinte
    // é a 4ª. Sem onda, também não somam: a banana começa o satisfeito, e a carga fica em 0.
    [Teste]
    public static void ComidaEAgua_NaoSomam()
    {
        foreach (Item alivio in new[] { Item.Banana, Item.Bala, Item.Agua, Item.Cafe, Item.Energetico })
        {
            Cenario c = Pausado();
            Usar(c, Item.Cerveja);
            Usar(c, Item.Cerveja);
            Usar(c, Item.Cerveja);
            Resultado r = Usar(c, alivio);
            Afirmar.Igual((new EstadoDaOnda(Onda.Bebado, FaseDaOnda.Subida, 2, 3), 3), (r.Estado.Onda, r.Estado.Carga), $"{alivio}: alivia um passo e não soma");
            Afirmar.Falso(r.Transicoes.Single().Regra.Contains("paranoia", StringComparison.Ordinal), $"{alivio}: sem paranoia");
            Resultado quarta = Usar(c, Item.Cigarro);
            Afirmar.Igual((ParanoiaComecando, new EstadoDaOnda(Onda.Bebado, FaseDaOnda.Subida, 2, 3), 4), (quarta.Estado.Onda, quarta.Estado.OndaDeFundo, quarta.Estado.Carga),
                $"{alivio}: o cigarro seguinte é a 4ª substância");
        }

        Cenario semOnda = Pausado();
        Usar(semOnda, Item.Banana);
        Usar(semOnda, Item.Agua);
        Afirmar.Igual(0, semOnda.Atual.Carga, "sem onda de substância, a comida e a água não deixam carga");
        Afirmar.Falso(semOnda.Retrato.Descrever().Contains("carga=", StringComparison.Ordinal), $"nem na linha: {semOnda.Retrato.Descrever()}");
    }

    // A carga volta a 0 no fim de todo evento em que nem a onda da frente nem a de fundo é de substância: quando o disparo
    // acaba o relaxado do cigarro (sem queda), quando a água acaba a queda do bêbado e quando um cigarro é absorvido pelo
    // ligado do café (a onda leve na frente: o relaxado nem entra, e a carga dele não sobra). Enquanto sobra uma onda de
    // substância, no fundo inclusive, ela fica: a paranoia acaba, o bêbado volta e a carga continua 4, até ele acabar.
    [Teste]
    public static void Carga_VoltaAZeroQuandoNaoSobraOndaDeSubstancia()
    {
        Cenario cigarro = Pausado();
        Usar(cigarro, Item.Cigarro);
        Afirmar.Igual(1, cigarro.Atual.Carga, "o cigarro: carga 1");
        Disparar(cigarro);
        Afirmar.Igual((new EstadoDaOnda(Onda.Relaxado, FaseDaOnda.Pico, 1, 1), 1), (cigarro.Atual.Onda, cigarro.Atual.Carga), "no pico do relaxado, a carga fica");
        Disparar(cigarro);
        Afirmar.Igual(((EstadoDaOnda?)null, 0), (cigarro.Atual.Onda, cigarro.Atual.Carga), "o relaxado acabou: a carga volta a 0 no mesmo disparo");

        Cenario agua = Pausado();
        Usar(agua, Item.Vodka);
        Usar(agua, Item.Agua);
        Usar(agua, Item.Agua);
        Afirmar.Igual((new EstadoDaOnda(Onda.Bebado, FaseDaOnda.Queda, 1, 2), 1), (agua.Atual.Onda, agua.Atual.Carga), "a vodka e duas águas: a queda do bêbado, carga 1");
        Usar(agua, Item.Agua);
        Afirmar.Igual(((EstadoDaOnda?)null, 0), (agua.Atual.Onda, agua.Atual.Carga), "a água acabou a queda: carga 0");

        Cenario absorvido = Pausado();
        Usar(absorvido, Item.Cafe);
        Resultado r = Usar(absorvido, Item.Cigarro);
        Afirmar.Igual((new EstadoDaOnda(Onda.Ligado, FaseDaOnda.Subida, 1, 1), (EstadoDaOnda?)null, 0), (r.Estado.Onda, r.Estado.OndaDeFundo, r.Estado.Carga),
            "o cigarro absorvido pelo ligado (leve): sem onda de substância, a carga volta a 0 no próprio soltar");

        // Semeada (os itens não chegam a esse estado: a comida e a bebida aliviam uma onda de substância em vez de ir para a
        // frente dela), uma onda leve na frente com uma de substância no fundo: a carga fica, porque a regra olha as duas.
        // Quando a leve acaba e o bêbado volta, ela continua, e a substância seguinte é a 4ª.
        var semeado = new SimuladorDeTempo(SemFisica(), 7, TopologiasDeExemplo.UmMonitor);
        semeado.Aplicar(new CmdPauseAutonomy());
        semeado = semeado.Semeado(s => s with { Onda = new EstadoDaOnda(Onda.Alegre, FaseDaOnda.Queda, 1, 1), OndaDeFundo = new EstadoDaOnda(Onda.Bebado, FaseDaOnda.Pico, 2, 2), Carga = 3 });
        semeado.Aplicar(new ItemEffectTimer(semeado.Estado.GeracaoDaOnda));
        Afirmar.Igual(3, semeado.Estado.Carga, "a leve na frente, o bêbado no fundo: a carga fica");
        semeado.Aplicar(new ItemEffectTimer(semeado.Estado.GeracaoDaOnda));
        Afirmar.Igual((new EstadoDaOnda(Onda.Bebado, FaseDaOnda.Pico, 2, 2), 3), (semeado.Estado.Onda, semeado.Estado.Carga), "a leve acabou e o bêbado voltou: a carga continua");
        SoltarSobreEle(semeado, InvocarEAssentar(semeado, Item.Cigarro).Id);
        Afirmar.Igual((ParanoiaComecando, 4), (semeado.Estado.Onda, semeado.Estado.Carga), "e o cigarro é a 4ª substância");

        Cenario fundo = Pausado();
        AteAParanoia(fundo);
        while (fundo.Atual.Onda!.Tipo == Onda.Paranoico) Disparar(fundo);
        Afirmar.Igual((BebadoNaSubida3, 4), (fundo.Atual.Onda, fundo.Atual.Carga), "a paranoia acabou e o bêbado voltou: a carga fica");
        int disparos = 0;
        while (fundo.Atual.Onda is not null)
        {
            Afirmar.Verdadeiro(disparos++ < 10, "o bêbado acaba");
            Afirmar.Igual(4, fundo.Atual.Carga, "com o bêbado, a carga fica");
            Disparar(fundo);
        }
        Afirmar.Igual(0, fundo.Atual.Carga, "sem onda nenhuma, a carga volta a 0");
    }

    // ---------------------------------------------------------------- com a paranoia na frente

    // Com a paranoia na frente, mais uma substância sobe um nível (até 3), o pior acompanha e a fase recomeça: na subida,
    // continua subida (1 s); no pico, recomeça o pico (40 s); na queda, volta ao pico, no nível 2. No nível 3, fica no 3, e
    // o pico recomeça assim mesmo. A substância combina antes com o fundo, como sempre (a cerveja soma no bêbado; o
    // cigarro é absorvido), e a carga continua subindo.
    [Teste]
    public static void ComAParanoiaNaFrente_MaisUmaSubstanciaSobeONivelERecomecaOPico()
    {
        Cenario c = Pausado();
        AteAParanoia(c);
        Resultado naSubida = Usar(c, Item.Cerveja);
        Afirmar.Igual((new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Subida, 2, 2), BebadoNaSubida3, 5), (naSubida.Estado.Onda, naSubida.Estado.OndaDeFundo, naSubida.Estado.Carga),
            "na subida: o nível 2, ainda na subida; a cerveja somou no bêbado do fundo, que já estava no 3");
        Afirmar.Sequencia([TimeSpan.FromSeconds(1)], Agendados(naSubida), "a subida recomeça");
        Afirmar.Igual("ITEM_DRAG_END sobre o personagem: Beber Cerveja; a paranoia sobe: Paranoico/Subida/1 -> Paranoico/Subida/2", naSubida.Transicoes.Single().Regra, "a regra");
        Afirmar.Falso(naSubida.Estado.Uso!.ComecouAParanoia, "subir não é começar");

        Disparar(c);
        Afirmar.Igual(new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, 2, 2), c.Atual.Onda, "o pico do nível 2");
        Resultado noPico = Usar(c, Item.Cigarro);
        Afirmar.Igual((new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, 3, 3), BebadoNaSubida3, 6), (noPico.Estado.Onda, noPico.Estado.OndaDeFundo, noPico.Estado.Carga),
            "no pico: o nível 3, com o pior; o cigarro foi absorvido");
        Afirmar.Sequencia([TimeSpan.FromSeconds(40)], Agendados(noPico), "o pico recomeça: 40 s");
        Resultado noTeto = Usar(c, Item.Vodka);
        Afirmar.Igual((new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, 3, 3), 7), (noTeto.Estado.Onda, noTeto.Estado.Carga), "no nível 3, fica no 3");
        Afirmar.Sequencia([TimeSpan.FromSeconds(40)], Agendados(noTeto), "e o pico recomeça assim mesmo");
        Afirmar.Igual(noPico.Estado.GeracaoDaOnda + 1, noTeto.Estado.GeracaoDaOnda, "com um disparo novo");

        while (c.Atual.Onda!.Fase != FaseDaOnda.Queda) Disparar(c);
        Afirmar.Igual(new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Queda, 1, 3), c.Atual.Onda, "a queda, com o pior 3");
        Resultado naQueda = Usar(c, Item.LancaPerfume);
        Afirmar.Igual((new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, 2, 3), new EstadoDaOnda(Onda.Bebado, FaseDaOnda.Subida, 3, 3)), (naQueda.Estado.Onda, naQueda.Estado.OndaDeFundo),
            "na queda: de volta ao pico, no nível 2 (o tonto, de precedência 3, é absorvido pela paranoia)");
        Afirmar.Sequencia([TimeSpan.FromSeconds(40)], Agendados(naQueda), "o pico inteiro");
        Afirmar.Igual("ITEM_DRAG_END sobre o personagem: Inalar LancaPerfume; a paranoia sobe: Paranoico/Queda/1 -> Paranoico/Pico/2", naQueda.Transicoes.Single().Regra, "a regra");
    }

    // A paranoia acabada com a carga em 4 ou mais (o bêbado de volta à frente): a substância seguinte a começa de novo, na
    // subida do nível 1, com o bêbado no fundo.
    [Teste]
    public static void ParanoiaAcabadaComCarga_ASubstanciaSeguinteAComecaDeNovo()
    {
        Cenario c = Pausado();
        AteAParanoia(c);
        while (c.Atual.Onda!.Tipo == Onda.Paranoico) Disparar(c);
        Afirmar.Igual((BebadoNaSubida3, 4), (c.Atual.Onda, c.Atual.Carga), "o bêbado voltou, com a carga");
        Resultado deNovo = Usar(c, Item.Cigarro);
        Afirmar.Igual((ParanoiaComecando, BebadoNaSubida3, 5), (deNovo.Estado.Onda, deNovo.Estado.OndaDeFundo, deNovo.Estado.Carga), "a 5ª substância: a paranoia começa de novo");
        Afirmar.Verdadeiro(deNovo.Estado.Uso!.ComecouAParanoia, "e este uso a começou");
        Afirmar.Igual(Gesto.OlharProTeto, c.Atual.Gesto, "e ele olha pro teto de novo no fim do uso");
    }

    // ---------------------------------------------------------------- a onda: fases, durações, caras e perfis

    // As fases e as durações, pelos itens: a subida de 1 s; um pico de 40 s por nível; a queda de 15 s × 100, 125 ou 150%,
    // pelo pior nível (15, 18,75 e 22,5 s); e o fim, com a frente anterior de volta. Nos níveis 2 e 3, a paranoia sobe na
    // subida (a 5ª e a 6ª substâncias, sem disparo no meio).
    [Teste]
    public static void FasesEDuracoes_PelosItens()
    {
        foreach (int nivel in new[] { 1, 2, 3 })
        {
            Cenario c = Pausado();
            Resultado inicio = AteAParanoia(c);
            var atrasos = new List<TimeSpan>(Agendados(inicio));
            for (int n = 2; n <= nivel; n++) atrasos = [.. Agendados(Usar(c, Item.Cerveja))];
            Afirmar.Igual(new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Subida, nivel, nivel), c.Atual.Onda, $"nível {nivel}: na subida");
            var fases = new List<EstadoDaOnda?>();
            while (c.Atual.Onda!.Tipo == Onda.Paranoico)
            {
                Afirmar.Verdadeiro(fases.Count < 10, $"nível {nivel}: a paranoia acaba");
                Resultado r = Disparar(c);
                fases.Add(r.Estado.Onda);
                atrasos.AddRange(Agendados(r));
            }
            TimeSpan queda = TimeSpan.FromMilliseconds(15000 * (100 + 25 * (nivel - 1)) / 100);
            TimeSpan[] esperados = [TimeSpan.FromSeconds(1), .. Enumerable.Repeat(TimeSpan.FromSeconds(40), nivel), queda, TimeSpan.FromSeconds(8)];
            Afirmar.Sequencia(esperados, atrasos, $"nível {nivel}: subida, {nivel} níveis do pico, a queda e a subida do bêbado que voltou");
            EstadoDaOnda?[] fasesEsperadas =
            [
                .. Enumerable.Range(0, nivel).Select(k => (EstadoDaOnda?)new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, nivel - k, nivel)),
                new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Queda, 1, nivel),
                BebadoNaSubida3,
            ];
            Afirmar.Sequencia(fasesEsperadas, fases, $"nível {nivel}: as fases");
            Afirmar.Igual(2 + nivel, fases.Count, $"nível {nivel}: 2 + nível disparos (invariante 25)");
        }
        DadosDaOnda d = TabelaDoTamagotchi.DaOnda(Onda.Paranoico);
        Afirmar.Igual(TimeSpan.FromSeconds(143.5), d.Duracao(FaseDaOnda.Subida, 3) + 3 * d.Duracao(FaseDaOnda.Pico, 3) + d.Duracao(FaseDaOnda.Queda, 3), "o episódio mais longo: 1 + 3 × 40 + 22,5 s");
    }

    // As caras: assustado na subida, paranoico no pico, sonolento na queda, cada uma na hora em que a fase começa, com a
    // cara livre; no fim, a de base (aqui, a do bêbado que voltou). Durante o uso, a cara de quem usa espera o fim.
    [Teste]
    public static void Caras_DeCadaFase()
    {
        DadosDaOnda d = TabelaDoTamagotchi.DaOnda(Onda.Paranoico);
        Afirmar.Igual((Expressao.Assustado, Expressao.Paranoico, Expressao.Sonolento), (d.Cara(FaseDaOnda.Subida), d.Cara(FaseDaOnda.Pico), d.Cara(FaseDaOnda.Queda)), "as caras da tabela");

        Cenario c = Pausado();
        AteAParanoia(c);
        Afirmar.Igual(Expressao.Assustado, c.Atual.Expressao, "no fim do uso, ainda na subida: assustado");
        Afirmar.Igual(Expressao.Paranoico, Disparar(c).Estado.Expressao, "no pico: paranoico");
        Afirmar.Igual(Expressao.Sonolento, Disparar(c).Estado.Expressao, "na queda: sonolento");
        Afirmar.Igual(Expressao.Feliz, Disparar(c).Estado.Expressao, "no fim, a da subida do bêbado que voltou");

        Cenario usando = Pausado();
        Usar(usando, Item.Cerveja);
        Usar(usando, Item.Cerveja);
        Usar(usando, Item.Cerveja);
        ItemNoMundo cigarro = InvocarEAssentar(usando, Item.Cigarro);
        usando.SoltarSobreEle(cigarro.Id);
        Disparar(usando);
        Afirmar.Igual((FaseDaOnda.Pico, Expressao.Pensativo), (usando.Atual.Onda!.Fase, usando.Atual.Expressao), "o pico chega durante o uso: a cara de quem fuma continua");
        usando.Passos(usando.Atual.PassosRestantes);
        Afirmar.Igual(Expressao.Paranoico, usando.Atual.Expressao, "no fim do uso, a do pico");
    }

    // O perfil no pico e na queda (tabela da decisão), pelo PerfilEfetivo e pela FisicaEfetiva, contado à mão na energia
    // média (decisão 8–20 s, descanso 30–90 s, pesos 4/2/1/3/3/2, pulo 50–100, foguete 30, parede 15–35 s, pendurado 3–8 s;
    // andar 90, escalar 110 e pendurar 80 DIP/s). No pico, igual nos três níveis e na subida: decide mais vezes, descansa
    // pouco, anda devagar (70%), gesticula muito, e escalar, pular, descansar e o foguete vão a zero. Na queda, o cansaço.
    [Teste]
    public static void Perfil_NoPicoENaQueda()
    {
        ConfiguracaoDoNucleo cfg = SemFisica();
        EstadoDoNucleo inicial = EstadoDoNucleo.Inicial(1);
        foreach (EstadoDaOnda pico in new EstadoDaOnda[] { new(Onda.Paranoico, FaseDaOnda.Subida, 1, 1), new(Onda.Paranoico, FaseDaOnda.Pico, 1, 1), new(Onda.Paranoico, FaseDaOnda.Pico, 2, 2), new(Onda.Paranoico, FaseDaOnda.Pico, 3, 3) })
        {
            string caso = $"{pico.Fase} {pico.Nivel}";
            EstadoDoNucleo s = inicial with { Onda = pico };
            PerfilDeEnergia p = Maquina.PerfilEfetivo(s, cfg);
            Afirmar.Igual((TimeSpan.FromMilliseconds(4800), TimeSpan.FromSeconds(12)), (p.DecisaoMinima, p.DecisaoMaxima), $"{caso}: decisão a 60%");
            Afirmar.Igual((TimeSpan.FromSeconds(9), TimeSpan.FromSeconds(27)), (p.DescansoMinimo, p.DescansoMaximo), $"{caso}: descanso a 30%");
            Afirmar.Igual((2, 0, 0, 0, 9, 3), (p.PesoAndar, p.PesoEscalar, p.PesoPular, p.PesoDescansar, p.PesoGesto, p.PesoTrocarExpressao), $"{caso}: andar 50%, escalar, pular e descansar 0, gesto 300%, troca 150%");
            Afirmar.Igual((50, 100, 0), (p.AlturaDoPuloMinima, p.AlturaDoPuloMaxima, p.ChanceDoFoguete), $"{caso}: pulo a 100% e foguete 0");
            Afirmar.Igual((TimeSpan.FromMilliseconds(21428), TimeSpan.FromSeconds(50)), (p.TempoNaParedeMinimo, p.TempoNaParedeMaximo), $"{caso}: parede × 100/70");
            Afirmar.Igual((TimeSpan.FromMilliseconds(4285), TimeSpan.FromMilliseconds(11428)), (p.TempoPenduradoMinimo, p.TempoPenduradoMaximo), $"{caso}: pendurado × 100/70");
            ParametrosDeMovimento f = Maquina.FisicaEfetiva(s, cfg);
            Afirmar.Igual(cfg.Fisica with { VelocidadeAndando = 63, VelocidadeEscalando = 77, VelocidadePendurado = 56 }, f, $"{caso}: só as três velocidades, a 70%");
        }

        EstadoDoNucleo naQueda = inicial with { Onda = new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Queda, 1, 2) };
        PerfilDeEnergia q = Maquina.PerfilEfetivo(naQueda, cfg);
        Afirmar.Igual((TimeSpan.FromMilliseconds(9600), TimeSpan.FromSeconds(24)), (q.DecisaoMinima, q.DecisaoMaxima), "queda: decisão a 120%");
        Afirmar.Igual((TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(135)), (q.DescansoMinimo, q.DescansoMaximo), "queda: descanso a 150%");
        Afirmar.Igual((3, 1, 1, 5, 3, 2), (q.PesoAndar, q.PesoEscalar, q.PesoPular, q.PesoDescansar, q.PesoGesto, q.PesoTrocarExpressao), "queda: andar 80%, escalar e pular 50%, descansar 150%, gesto e troca 100%");
        Afirmar.Igual((50, 100, 30), (q.AlturaDoPuloMinima, q.AlturaDoPuloMaxima, q.ChanceDoFoguete), "queda: pulo a 100% e o foguete do perfil");
        Afirmar.Igual((TimeSpan.FromMilliseconds(17647), TimeSpan.FromMilliseconds(41176)), (q.TempoNaParedeMinimo, q.TempoNaParedeMaximo), "queda: parede × 100/85");
        Afirmar.Igual((TimeSpan.FromMilliseconds(3529), TimeSpan.FromMilliseconds(9411)), (q.TempoPenduradoMinimo, q.TempoPenduradoMaximo), "queda: pendurado × 100/85");
        Afirmar.Igual(cfg.Fisica with { VelocidadeAndando = 76.5, VelocidadeEscalando = 93.5, VelocidadePendurado = 68 }, Maquina.FisicaEfetiva(naQueda, cfg), "queda: as três velocidades a 85%, sem cambaleio");
        Afirmar.Igual(1.0, Maquina.FatorDoCambaleio(24, TabelaDoTamagotchi.DaOnda(Onda.Paranoico).Perfil(FaseDaOnda.Pico, 3).Cambaleio), "anda reto: sem cambaleio");
    }

    /// <summary>Uma tabela com a paranoia no pico por duas horas: ela fica no pico durante toda a simulação.</summary>
    private static DadosDaOnda PicoLongo(Onda onda) => TabelaDoTamagotchi.DaOnda(onda) with { NivelDoPico = TimeSpan.FromHours(2) };

    // No pico, com a física do aplicativo e a agenda livre, com todas as ações, por meia hora em cada uma de 6 sementes: ele
    // nunca escala, pula nem descansa por conta própria (os pesos vão a zero, até no fim da caminhada junto a uma parede),
    // mas anda; as trocas de cara só sorteiam as caras do pico (a paranoica, a mais pesada, é a mais sorteada); e os gestos
    // só os do pico, todos, com o olhar pro teto como o mais sorteado.
    [Teste]
    public static void NoPico_NuncaEscalaPulaNemDescansa_ECarasEGestosDaFase()
    {
        ConfiguracaoDoNucleo cfg = ComFisica() with { TabelaDeOndas = PicoLongo, Acoes = AcoesAutonomas.Todas };
        // As caras e os gestos do pico, pela decisão (à parte da tabela do núcleo).
        Expressao[] carasDoPico = [Expressao.Paranoico, Expressao.Assustado, Expressao.Surpreso];
        Gesto[] gestosDoPico = [Gesto.OlharProTeto, Gesto.Agachar, Gesto.Tremedeira, Gesto.OlharAoRedor, Gesto.Espiar];
        var caras = new Dictionary<Expressao, int>();
        var gestos = new Dictionary<Gesto, int>();
        long passosAndando = 0, decisoes = 0;
        for (ulong semente = 1; semente <= 6; semente++)
        {
            var sim = new SimuladorDeTempo(cfg, semente, TopologiasDeExemplo.UmMonitor);
            sim = sim.Semeado(s => s with { Onda = new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, 1, 1), Expressao = Expressao.Paranoico, Carga = 4 });
            sim.Aplicar(new ItemEffectTimer(sim.Estado.GeracaoDaOnda));
            sim.AoResultado = (antes, e, r) =>
            {
                string onde = $"semente {semente}, {e} em {antes.Estado}";
                Afirmar.Igual(Onda.Paranoico, r.Estado.Onda?.Tipo, $"{onde}: no pico da paranoia");
                foreach (Transicao t in r.Transicoes)
                    Afirmar.Falso(t.Para is Estado.Climbing or Estado.Jumping or Estado.Resting or Estado.Hanging, $"{onde}: {t}");
                if (e is Tick && r.Estado.Estado == Estado.Walking) passosAndando++;
                if (e is not AutonomyTimer || antes.Estado != Estado.Idle) return;
                // Parado, a agenda escolhe andar (uma transição), um gesto (uma transição para o mesmo estado, com o gesto) ou
                // trocar a cara (nenhuma transição; a cara pode repetir).
                decisoes++;
                if (r.Estado.Gesto != Gesto.Nenhum) gestos[r.Estado.Gesto] = gestos.GetValueOrDefault(r.Estado.Gesto) + 1;
                else if (r.Transicoes.Count == 0) caras[r.Estado.Expressao] = caras.GetValueOrDefault(r.Estado.Expressao) + 1;
            };
            sim.Avancar(TimeSpan.FromMinutes(30));
        }
        string Contagem<T>(Dictionary<T, int> vezes) where T : notnull => string.Join(", ", vezes.OrderByDescending(v => v.Value).Select(v => $"{v.Key}={v.Value}"));
        Console.WriteLine($"         {decisoes} decisões em 3 horas no pico; {passosAndando} passos andando; caras: {Contagem(caras)}; gestos: {Contagem(gestos)}");
        Afirmar.Verdadeiro(passosAndando > 1000, $"ele anda: {passosAndando} passos");
        Afirmar.Sequencia(carasDoPico.Order(), caras.Keys.Order(), "as caras do pico, e só elas");
        Afirmar.Igual(Expressao.Paranoico, caras.MaxBy(c => c.Value).Key, "a paranoica é a mais sorteada");
        Afirmar.Sequencia(gestosDoPico.Order(), gestos.Keys.Order(), "os gestos do pico, todos, e só eles");
        Afirmar.Igual(Gesto.OlharProTeto, gestos.MaxBy(g => g.Value).Key, "olhar pro teto é o mais sorteado");
    }

    // ---------------------------------------------------------------- o gesto do começo e o usuário

    // O uso que começa a paranoia acaba, no chão, com ele em IDLE: na hora, olha pro teto, por 90 passos, sem sorteio (o
    // gerador fica o mesmo, e a agenda espera o fim do gesto), com uma transição que diz por quê. O relógio corre no gesto;
    // no 90º passo ele acaba e a agenda volta. Vale também com a autonomia pausada (não é decisão da agenda). O uso
    // seguinte, que só sobe a paranoia, não traz gesto.
    [Teste]
    public static void GestoImediato_OlhaProTetoNoFimDoUsoQueAComecou()
    {
        foreach (bool pausado in new[] { false, true })
        {
            string caso = pausado ? "pausado" : "com a agenda livre";
            Cenario c = Cenario.Parado(SemFisica() with { Acoes = AcoesAutonomas.Nenhuma });
            if (pausado) c.Aplicar(new CmdPauseAutonomy());
            Usar(c, Item.Cerveja);
            Usar(c, Item.Cerveja);
            Usar(c, Item.Cerveja);
            ItemNoMundo cigarro = InvocarEAssentar(c, Item.Cigarro);
            c.SoltarSobreEle(cigarro.Id);
            c.Passos(c.Atual.PassosRestantes - 1);
            EstadoDoNucleo antes = c.Atual;
            c.Aplicar(new Tick());
            Afirmar.Sequencia(
                [new Transicao(Estado.Using, Estado.Settling, "USING: fim do uso de Cigarro"), new Transicao(Estado.Settling, Estado.Idle, "SETTLING com apoio"),
                 new Transicao(Estado.Idle, Estado.Idle, "IDLE: a paranoia começou, gesto OlharProTeto")],
                c.Transicoes, $"{caso}: o fim do uso e o gesto");
            Afirmar.Igual((Gesto.OlharProTeto, 90), (c.Atual.Gesto, c.Atual.PassosDoGesto), $"{caso}: olha pro teto por 90 passos");
            Afirmar.Igual(antes.Aleatorio, c.Atual.Aleatorio, $"{caso}: sem sorteio");
            Afirmar.Verdadeiro(c.Atual.RelogioAtivo && !c.Tem<DesligarRelogio>() && !c.Tem<AgendarDecisao>(), $"{caso}: o relógio continua e a agenda espera o gesto");
            for (int passo = 1; passo < 90; passo++)
            {
                c.Aplicar(new Tick()).SemTransicao();
                Afirmar.Igual(Gesto.OlharProTeto, c.Atual.Gesto, $"{caso}: passo {passo} do gesto");
            }
            c.Aplicar(new Tick());
            Afirmar.Igual(Gesto.Nenhum, c.Atual.Gesto, $"{caso}: no 90º passo, o gesto acaba");
            Afirmar.Verdadeiro(c.Tem<DesligarRelogio>() && c.Tem<AgendarDecisao>() != pausado, $"{caso}: o relógio desliga e a agenda volta (pausado, não)");

            Usar(c, Item.Vodka);
            Afirmar.Igual(Gesto.Nenhum, c.Atual.Gesto, $"{caso}: o uso que só sobe a paranoia não traz gesto");
        }
    }

    // O usuário prevalece: um PRESS no meio do uso que começa a paranoia o segura no mesmo evento (o uso acaba, a paranoia
    // continua, sem mexer no temporizador dela), e o clique depois não traz o olhar pro teto (o uso não foi até o fim). Um
    // PRESS no meio do olhar pro teto acaba o gesto na hora.
    [Teste]
    public static void PressNoMeio_ContinuaPrioritario()
    {
        Cenario c = Pausado();
        Usar(c, Item.Cerveja);
        Usar(c, Item.Cerveja);
        Usar(c, Item.Cerveja);
        ItemNoMundo cigarro = InvocarEAssentar(c, Item.Cigarro);
        c.SoltarSobreEle(cigarro.Id).Passos(30);
        Afirmar.Igual(ParanoiaComecando, c.Atual.Onda, "a paranoia começou no soltar");
        long geracao = c.Atual.GeracaoDaOnda;
        c.Aplicar(new Press(Cenario.PontoOpaco)).Percorreu(Estado.Using, Estado.Pressed);
        Afirmar.Nulo(c.Atual.Uso, "o uso acabou");
        Afirmar.Igual((ParanoiaComecando, BebadoNaSubida3, 4, geracao), (c.Atual.Onda, c.Atual.OndaDeFundo, c.Atual.Carga, c.Atual.GeracaoDaOnda), "a paranoia continua, com o mesmo disparo");
        Afirmar.Falso(c.Efeitos.Any(e => e is AgendarOnda or CancelarOnda), "sem mexer no temporizador dela");
        c.Aplicar(new Click()).Esta(Estado.Reacting);
        c.Passos(c.Atual.PassosRestantes).Esta(Estado.Idle, "fim da reação");
        Afirmar.Igual(Gesto.Nenhum, c.Atual.Gesto, "sem o olhar pro teto: o uso não foi até o fim");

        Cenario olhando = Pausado();
        AteAParanoia(olhando);
        Afirmar.Igual(Gesto.OlharProTeto, olhando.Atual.Gesto, "olhando pro teto");
        olhando.Passos(10);
        olhando.Aplicar(new Press(Cenario.PontoOpaco)).Percorreu(Estado.Idle, Estado.Pressed);
        Afirmar.Igual(Gesto.Nenhum, olhando.Atual.Gesto, "o PRESS acaba o gesto na hora");
    }

    // DEC-024: preso pelo usuário na parede ou no cipó, ele usa as quatro substâncias ali; a paranoia começa, e ele volta
    // agarrado e preso, sem o olhar pro teto (só em IDLE). Com a agenda livre e a paranoia correndo (subida, pico, queda e
    // o fundo de volta), por cinco minutos, ele continua preso no mesmo apoio: nunca cai, nunca vai ao chão.
    [Teste]
    public static void Preso_ContinuaPreso()
    {
        foreach (bool noCipo in new[] { false, true })
        {
            string caso = noCipo ? "no cipó" : "na parede";
            Estado apoio = noCipo ? Estado.Hanging : Estado.Climbing;
            var sim = new SimuladorDeTempo(ComFisica(), 6, TopologiasDeExemplo.UmMonitor);
            Superficies sup = Sup(sim.Estado);
            SoltarPersonagemEm(sim, noCipo ? new PontoPx(960, sup.Teto + 12) : new PontoPx(sup.Direita - 20, 600));
            sim.Esta(apoio, $"{caso}: posto lá");
            Afirmar.Verdadeiro(sim.Estado.PresoPeloUsuario, $"{caso}: preso");
            foreach (Item item in new[] { Item.Cerveja, Item.Cerveja, Item.Cerveja, Item.Cigarro })
            {
                SoltarSobreEle(sim, InvocarEAssentar(sim, item).Id);
                sim.Esta(Estado.Using, $"{caso}: usando {item}");
                sim.Passos(sim.Estado.PassosRestantes);
                sim.Esta(apoio, $"{caso}: de volta depois de {item}");
            }
            Afirmar.Igual(Onda.Paranoico, sim.Estado.Onda?.Tipo, $"{caso}: a paranoia começou");
            Afirmar.Verdadeiro(sim.Estado.PresoPeloUsuario && sim.Estado.Movimento.Agarrado, $"{caso}: agarrado e preso");
            Afirmar.Igual(Gesto.Nenhum, sim.Estado.Gesto, $"{caso}: sem o olhar pro teto fora de IDLE");
            sim.AoResultado = (_, e, r) =>
            {
                Afirmar.Igual(apoio, r.Estado.Estado, $"{caso}: {e} o tirou do apoio");
                Afirmar.Verdadeiro(r.Estado.PresoPeloUsuario, $"{caso}: {e} o soltou");
            };
            sim.Avancar(TimeSpan.FromMinutes(5));
            Afirmar.Igual(Onda.Bebado, sim.Estado.Onda?.Tipo, $"{caso}: a paranoia acabou, e o bêbado voltou");
        }
    }

    // DEC-025: escondido atrás da borda de baixo, ele usa as quatro substâncias ali; a paranoia começa e ele continua
    // escondido na mesma borda, sem o olhar pro teto. Com a agenda livre, por três minutos, ele só espia, com as caras da
    // fase da paranoia enquanto ela dura.
    [Teste]
    public static void Escondido_ContinuaEscondido()
    {
        var sim = new SimuladorDeTempo(ComFisica(), 4, TopologiasDeExemplo.UmMonitor);
        Posicionamento l = sim.Estado.Lugar!;
        var corpo = new PontoPx(l.Ancora.X, l.Ancora.Y - 20);
        foreach (Evento e in new Evento[] { new Press(corpo), new Click(), new Press(corpo), new DoubleClick() }) sim.Aplicar(e);
        sim.Esta(Estado.Peeking, "escondido");
        LadoDoEsconderijo lado = sim.Estado.Esconderijo;
        PontoPx escondido = sim.Estado.Lugar!.Ancora;
        foreach (Item item in new[] { Item.Cerveja, Item.Cerveja, Item.Cerveja, Item.Cigarro })
        {
            SoltarSobreEle(sim, InvocarEAssentar(sim, item).Id);
            sim.Esta(Estado.Using, $"usando {item} escondido");
            sim.Passos(sim.Estado.PassosRestantes);
            sim.Esta(Estado.Peeking, $"de volta ao esconderijo depois de {item}");
        }
        Afirmar.Igual(Onda.Paranoico, sim.Estado.Onda?.Tipo, "a paranoia começou");
        Afirmar.Igual((lado, escondido, Gesto.Nenhum), (sim.Estado.Esconderijo, sim.Estado.Lugar!.Ancora, sim.Estado.Gesto), "na mesma borda, no mesmo lugar, sem gesto");
        int espiadas = 0;
        sim.AoResultado = (_, e, r) =>
        {
            Afirmar.Igual(Estado.Peeking, r.Estado.Estado, $"{e} o tirou do esconderijo");
            if (e is AutonomyTimer && r.Estado.Onda is { Tipo: Onda.Paranoico, Fase: FaseDaOnda.Pico })
            {
                espiadas++;
                Afirmar.Verdadeiro(r.Estado.Expressao is Expressao.Paranoico or Expressao.Assustado or Expressao.Surpreso, $"espiando com a cara {r.Estado.Expressao}, que não é do pico");
            }
        };
        sim.Avancar(TimeSpan.FromMinutes(3));
        Afirmar.Verdadeiro(espiadas > 0, "espiou no pico da paranoia");
    }

    // ---------------------------------------------------------------- o alívio e o fim

    // O alívio acalma a paranoia um passo, como as outras ondas de substância: a banana baixa o pico do nível 2 ao 1 sem
    // mexer no temporizador; a água leva o pico do nível 1 à queda (15 s × 125%, pelo pior 2); a bala acaba a queda, e a
    // frente anterior (o bêbado) volta do fundo, com a fase recomeçada. A carga não muda: a comida e a água não somam, e o
    // bêbado continua de substância.
    [Teste]
    public static void Alivio_AcalmaUmPasso()
    {
        Cenario c = Pausado();
        AteAParanoia(c);
        Usar(c, Item.Vodka);
        Disparar(c);
        Afirmar.Igual((new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, 2, 2), 5), (c.Atual.Onda, c.Atual.Carga), "o pico do nível 2");
        long geracao = c.Atual.GeracaoDaOnda;

        Resultado banana = Usar(c, Item.Banana);
        Afirmar.Igual((new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Pico, 1, 2), 5), (banana.Estado.Onda, banana.Estado.Carga), "a banana: nível 1");
        Afirmar.Igual(geracao, banana.Estado.GeracaoDaOnda, "sem mexer no temporizador");
        Afirmar.Igual("ITEM_DRAG_END sobre o personagem: Comer Banana; alivia Paranoico/Pico/2 -> Paranoico/Pico/1", banana.Transicoes.Single().Regra, "a regra");

        Resultado agua = Usar(c, Item.Agua);
        Afirmar.Igual(new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Queda, 1, 2), agua.Estado.Onda, "a água: a queda");
        Afirmar.Sequencia([TimeSpan.FromSeconds(18.75)], Agendados(agua), "15 s × 125%");
        Afirmar.Igual(Expressao.Sonolento, c.Atual.Expressao, "no fim do uso, a cara da queda");

        Resultado bala = Usar(c, Item.Bala);
        Afirmar.Igual((BebadoNaSubida3, (EstadoDaOnda?)null, 5), (bala.Estado.Onda, bala.Estado.OndaDeFundo, bala.Estado.Carga), "a bala acaba a paranoia; o bêbado volta, e a carga fica");
        Afirmar.Sequencia([TimeSpan.FromSeconds(8)], Agendados(bala), "a subida do bêbado de novo");
        Afirmar.Igual("ITEM_DRAG_END sobre o personagem: Engolir Bala; alivia Paranoico/Queda/1 -> fim; a de fundo volta: Bebado/Subida/3", bala.Transicoes.Single().Regra, "a regra");
    }

    // Quando a paranoia acaba pelo temporizador, a frente anterior volta do fundo, na fase em que estava, com a duração
    // cheia e a cara dela; a regra do disparo diz isso.
    [Teste]
    public static void FimDaParanoia_AFrenteAnteriorVoltaDoFundo()
    {
        Cenario c = Pausado();
        AteAParanoia(c);
        Disparar(c);
        Disparar(c);
        Afirmar.Igual((new EstadoDaOnda(Onda.Paranoico, FaseDaOnda.Queda, 1, 1), BebadoNaSubida3), (c.Atual.Onda, c.Atual.OndaDeFundo), "a queda, com o bêbado no fundo");
        Resultado fim = Disparar(c);
        Afirmar.Igual((BebadoNaSubida3, (EstadoDaOnda?)null), (fim.Estado.Onda, fim.Estado.OndaDeFundo), "o bêbado volta à frente");
        Afirmar.Igual("ITEM_EFFECT_TIMER: onda Paranoico/Queda/1 -> fim; a de fundo volta: Bebado/Subida/3", fim.Transicoes.Single().Regra, "a regra");
        Afirmar.Sequencia([TimeSpan.FromSeconds(8)], Agendados(fim), "com a subida inteira");
        Afirmar.Igual(Expressao.Feliz, fim.Estado.Expressao, "e a cara da subida do bêbado");
    }

    // ---------------------------------------------------------------- o retrato e a chave

    // O retrato leva a carga, e a linha canônica só a mostra acima de 0, logo depois da onda de fundo: sem ela, a linha é a
    // de antes. Com o tamagotchi desligado, os itens nem chegam, e a carga fica em 0.
    [Teste]
    public static void Retrato_CargaSoApareceAcimaDeZero()
    {
        Cenario c = Pausado();
        string semCarga = c.Retrato.Descrever();
        Afirmar.Falso(semCarga.Contains("carga=", StringComparison.Ordinal), semCarga);
        Usar(c, Item.Vodka);
        Afirmar.Igual(1, c.Retrato.Carga, "o retrato leva a carga");
        Afirmar.Verdadeiro(c.Retrato.Descrever().EndsWith(" sinal=Nenhum onda=Bebado/Subida/2 carga=1", StringComparison.Ordinal), c.Retrato.Descrever());
        EstadoDoNucleo comFundo = c.Atual with { OndaDeFundo = new EstadoDaOnda(Onda.Tonto, FaseDaOnda.Pico, 1, 1) };
        Afirmar.Contem(" onda=Bebado/Subida/2 fundo=Tonto/Pico/1 carga=1", comFundo.Retrato().Descrever(), "logo depois do fundo");
        Afirmar.Igual(semCarga, (c.Atual with { Onda = null, Carga = 0, Expressao = Expressao.Neutro }).Retrato().Descrever(), "sem a onda e sem a carga, a linha de antes");

        Cenario desligado = Cenario.Parado(new ConfiguracaoDoNucleo()).Aplicar(new CmdSummonItem(Item.Vodka));
        Afirmar.Igual((0, 0), (desligado.Atual.Itens.Quantidade, desligado.Atual.Carga), "com a chave desligada, nem item nem carga");
    }
}

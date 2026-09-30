using System.Globalization;
using Buzzy.Core.Persistencia;
using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.Core.Testes.Persistencia;

/// <summary>
/// Propriedades da restauração na partida e do settings.json (desenho de persistência, seção 5), no estilo
/// de <see cref="PropriedadesTestes"/>: 5.000 casos com topologias aleatórias (<see cref="GeradorDeTopologias"/>),
/// cada um com a própria semente, tirada de uma semente mestra fixa. Toda falha informa as sementes e as
/// entradas, para o caso virar um teste de exemplo.
/// </summary>
internal static class PropriedadesDaPersistenciaTestes
{
    private const int Semente = 20260930;
    private const int Casos = 5000;

    // A partida restaura pela cascata (ARCHITECTURE.md 2.8): o monitor da chave; sem ele, o primeiro com a
    // tela salva; sem nenhum dos dois, o principal. Termina na área útil desse monitor, na posição relativa
    // salva, saneada, e a posição passa a ser dele, com a tela dele. É determinística; restaurar ou
    // reacomodar de novo, na mesma topologia, não move o personagem.
    [Teste]
    public static void RestaurarSegueACascataETerminaNaAreaUtil()
    {
        var porOrigem = new Dictionary<OrigemDaRestauracao, int>();
        ParaCadaCaso((rnd, gerador, contexto) =>
        {
            Topologia t = gerador.NovaTopologia();
            TamanhoDip tamanho = gerador.Sprite();
            PosicaoDoPersonagem salva = PosicaoSalvaAleatoria(rnd, gerador, t);
            string Onde() => $"{contexto()}; topologia: {t}; salva: {Descrever(salva)}; sprite {tamanho}";

            MonitorDoDesktop? daChave = t.PorChave(salva.ChaveMonitor);
            MonitorDoDesktop? daTela = daChave is null && salva.TelaDoMonitor is { } tela ? t.Monitores.FirstOrDefault(m => m.Tela == tela) : null;
            (MonitorDoDesktop esperado, OrigemDaRestauracao origemEsperada) =
                daChave is not null ? (daChave, OrigemDaRestauracao.PelaChave)
                : daTela is not null ? (daTela, OrigemDaRestauracao.PeloRetangulo)
                : (t.Principal, OrigemDaRestauracao.NoPrincipal);

            (Posicionamento r, PosicaoDoPersonagem nova, OrigemDaRestauracao origem) = Posicionador.Restaurar(t, salva, tamanho);
            porOrigem[origem] = porOrigem.GetValueOrDefault(origem) + 1;
            Verificar(origem == origemEsperada && ReferenceEquals(r.Monitor, esperado),
                () => $"{Onde()}: restaurou {origem} em {r.Monitor.Chave}; esperado {origemEsperada} em {esperado.Chave}");
            PropriedadesTestes.VerificarPosicionamento(r, tamanho, Onde);
            Verificar(ReferenceEquals(Maquina.MonitorDaAncora(t, r.Ancora), r.Monitor), () => $"{Onde()}: a validação atribui a âncora {r.Ancora} a outro monitor");

            double fx = Saneada(salva.FracaoX), fy = Saneada(salva.FracaoY);
            Verificar(r == Posicionador.NoMonitor(esperado, fx, fy, tamanho), () => $"{Onde()}: não está nas frações salvas, saneadas ({fx}; {fy})");
            Verificar(nova.ChaveMonitor == esperado.Chave && nova.TelaDoMonitor == esperado.Tela && nova.AncoraAbsoluta == r.Ancora
                    && nova.FracaoX.Equals(fx) && nova.FracaoY.Equals(fy),
                () => $"{Onde()}: posição nova {Descrever(nova)}");

            // Determinística: cópias iguais das entradas dão o mesmo resultado.
            var copia = new Topologia(t.Monitores.Select(m => m with { }));
            Verificar((r, nova, origem) == Posicionador.Restaurar(copia, salva with { }, tamanho), () => $"{Onde()}: resultado mudou com cópias das entradas");

            // Restaurar de novo (agora pela chave) e a reacomodação da execução não movem o personagem.
            (Posicionamento deNovo, PosicaoDoPersonagem novaDeNovo, OrigemDaRestauracao origemDeNovo) = Posicionador.Restaurar(t, nova, tamanho);
            Verificar(origemDeNovo == OrigemDaRestauracao.PelaChave && deNovo == r && novaDeNovo == nova, () => $"{Onde()}: restaurar de novo moveu para {deNovo.Ancora} ({origemDeNovo})");
            Verificar((r, nova) == Posicionador.Reacomodar(t, nova, tamanho), () => $"{Onde()}: reacomodar na mesma topologia moveu o personagem");
        });

        Console.WriteLine("         " + string.Join(", ", porOrigem.OrderBy(o => o.Key).Select(o => $"{o.Key}={o.Value}")));
        foreach (OrigemDaRestauracao origem in Enum.GetValues<OrigemDaRestauracao>())
            Afirmar.Verdadeiro(porOrigem.GetValueOrDefault(origem) > 500, $"o gerador exercita a restauração {origem} ({porOrigem.GetValueOrDefault(origem)} casos)");
    }

    // A posição que a execução grava (Descrever) passa pelo arquivo, volta igual e, restaurada na mesma
    // topologia, dá o mesmo resultado que a reacomodação da execução (Reacomodar). Numa topologia mudada em
    // que o monitor continua, também; sem ele, a partida segue a cascata (pela tela ou no principal), e
    // nunca a âncora absoluta, como a reacomodação faria.
    [Teste]
    public static void ArquivoIdaEVolta_EquivaleAReacomodarNaMesmaTopologia()
    {
        int mesmaChave = 0, pelaTela = 0, noPrincipal = 0;
        ParaCadaCaso((rnd, gerador, contexto) =>
        {
            Topologia t = gerador.NovaTopologia();
            TamanhoDip tamanho = gerador.Sprite();
            MonitorDoDesktop m = t.Monitores[rnd.Next(t.Monitores.Count)];
            PosicaoDoPersonagem gravada = Posicionador.Descrever(Posicionador.NoMonitor(m, gerador.Fracao(), gerador.Fracao(), tamanho));
            var preferencias = new Preferencias((NivelDeEnergia)rnd.Next(3), rnd.Next(2) == 0, rnd.Next(2) == 0);
            var configuracoes = new ConfiguracoesSalvas(gravada, preferencias);
            string Onde() => $"{contexto()}; topologia: {t}; gravada: {Descrever(gravada)}; sprite {tamanho}";

            LeituraDasConfiguracoes lida = EsquemaDeConfiguracoes.Ler(EsquemaDeConfiguracoes.Escrever(configuracoes));
            Verificar(lida.Situacao == SituacaoDaLeitura.Valida && lida.Avisos.Count == 0 && lida.Configuracoes == configuracoes,
                () => $"{Onde()}: o arquivo devolveu {lida.Situacao}, {Descrever(lida.Configuracoes.Posicao)}, avisos: {string.Join(" | ", lida.Avisos)}");
            PosicaoDoPersonagem salva = lida.Configuracoes.Posicao!;

            (Posicionamento restaurado, PosicaoDoPersonagem restaurada, OrigemDaRestauracao origem) = Posicionador.Restaurar(t, salva, tamanho);
            (Posicionamento reacomodado, PosicaoDoPersonagem reacomodada) = Posicionador.Reacomodar(t, gravada, tamanho);
            Verificar(origem == OrigemDaRestauracao.PelaChave && restaurado == reacomodado && restaurada == reacomodada,
                () => $"{Onde()}: restaurada em {restaurado.Monitor.Chave} {restaurado.Ancora} ({origem}), reacomodada em {reacomodado.Monitor.Chave} {reacomodado.Ancora}");

            // Na partida seguinte, noutra topologia.
            Topologia outra = gerador.Mudar(t);
            (Posicionamento naOutra, _, OrigemDaRestauracao origemNaOutra) = Posicionador.Restaurar(outra, salva, tamanho);
            if (outra.PorChave(gravada.ChaveMonitor) is not null)
            {
                mesmaChave++;
                Posicionamento esperado = Posicionador.Reacomodar(outra, gravada, tamanho).Resultado;
                Verificar(origemNaOutra == OrigemDaRestauracao.PelaChave && naOutra == esperado,
                    () => $"{Onde()}; outra: {outra}: com o monitor presente, restaurou em {naOutra.Monitor.Chave} {naOutra.Ancora} ({origemNaOutra}), reacomodaria em {esperado.Monitor.Chave} {esperado.Ancora}");
                return;
            }
            MonitorDoDesktop? daTela = outra.Monitores.FirstOrDefault(x => x.Tela == gravada.TelaDoMonitor);
            if (daTela is not null) pelaTela++;
            else noPrincipal++;
            MonitorDoDesktop monitorEsperado = daTela ?? outra.Principal;
            OrigemDaRestauracao origemEsperada = daTela is not null ? OrigemDaRestauracao.PeloRetangulo : OrigemDaRestauracao.NoPrincipal;
            Verificar(origemNaOutra == origemEsperada && ReferenceEquals(naOutra.Monitor, monitorEsperado),
                () => $"{Onde()}; outra: {outra}: sem o monitor, restaurou {origemNaOutra} em {naOutra.Monitor.Chave}; esperado {origemEsperada} em {monitorEsperado.Chave}");
        });

        Console.WriteLine($"         na topologia mudada: {mesmaChave} pela chave, {pelaTela} pela tela, {noPrincipal} no principal");
        Afirmar.Verdadeiro(mesmaChave > 1000 && pelaTela > 0 && noPrincipal > 100, "o gerador exercita os três passos na topologia mudada");
    }

    // ---------------------------------------------------------------- auxiliares

    /// <summary>
    /// Posição salva por outra sessão: chave de um monitor da topologia, uma do gerador (que pode ou não
    /// existir) ou uma que nenhuma topologia gerada tem; tela desconhecida, a de um monitor da topologia (às
    /// vezes com outra chave) ou um retângulo qualquer; frações quaisquer, inclusive NaN e infinitas; âncora
    /// em qualquer lugar (a partida não a usa).
    /// </summary>
    private static PosicaoDoPersonagem PosicaoSalvaAleatoria(Random rnd, GeradorDeTopologias gerador, Topologia t)
    {
        string chave = rnd.Next(3) switch
        {
            0 => t.Monitores[rnd.Next(t.Monitores.Count)].Chave,
            1 => GeradorDeTopologias.Chave(rnd.Next(1, 12)),
            _ => "mon:" + rnd.Next().ToString("x8", CultureInfo.InvariantCulture),
        };
        RetanguloPx? tela = rnd.Next(3) switch
        {
            0 => null,
            1 => t.Monitores[rnd.Next(t.Monitores.Count)].Tela,
            _ => RetanguloPx.DePosicaoETamanho(gerador.Ponto(t), gerador.Resolucao()),
        };
        return new PosicaoDoPersonagem(chave, gerador.Fracao(), gerador.Fracao(), gerador.Ponto(t)) { TelaDoMonitor = tela };
    }

    /// <summary>A regra das frações salvas, escrita aqui à parte: NaN vira 0,5; o resto é preso em [0, 1].</summary>
    private static double Saneada(double f) => double.IsNaN(f) ? 0.5 : Math.Min(Math.Max(f, 0), 1);

    private static string Descrever(PosicaoDoPersonagem? p)
        => p is null ? "nenhuma" : $"{Gravacao.DescreverPosicaoCompleta(p)}";

    /// <summary>Roda <see cref="Casos"/> casos, cada um com a própria semente tirada da semente mestra.</summary>
    private static void ParaCadaCaso(Action<Random, GeradorDeTopologias, Func<string>> verificar)
    {
        var mestre = new Random(Semente);
        for (int caso = 0; caso < Casos; caso++)
        {
            int sementeDoCaso = mestre.Next();
            var rnd = new Random(sementeDoCaso);
            int numero = caso;
            verificar(rnd, new GeradorDeTopologias(rnd), () => $"semente {Semente}, caso {numero} (semente do caso {sementeDoCaso})");
        }
    }

    private static void Verificar(bool condicao, Func<string> mensagem)
    {
        if (!condicao) Afirmar.Falhar(mensagem());
    }
}

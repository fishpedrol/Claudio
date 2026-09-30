using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.Core.Testes.Personagem;

/// <summary>
/// Testes de propriedade da máquina de estados (critérios 2 e 3 da Fase 2): milhares de
/// sequências aleatórias de eventos, sobre topologias aleatórias e configurações variadas,
/// conferindo a cada passo os invariantes de ARCHITECTURE.md 2.6 e a regra do relógio. A
/// semente é fixa; toda falha informa a sequência, o passo e o evento para virar teste.
/// </summary>
internal static class InvariantesTestes
{
    private const int Semente = 20260929;
    private const int Sequencias = 2000;
    private const int EventosPorSequencia = 200;

    [Teste]
    public static void InvariantesValemEmMilharesDeSequenciasAleatorias()
    {
        var mestre = new Random(Semente);
        var transicoesVistas = new HashSet<(Estado, Estado)>();
        long passos = 0;

        for (int n = 0; n < Sequencias; n++)
        {
            int sementeDaSequencia = mestre.Next();
            (ConfiguracaoDoNucleo cfg, ulong sementeDoNucleo, List<Evento> eventos) = GerarSequencia(sementeDaSequencia);
            string Contexto(int i, Evento e) => $"sequência {n} (semente {sementeDaSequencia}), passo {i}, evento {e}";

            var nucleo = new Nucleo(cfg, sementeDoNucleo);
            var retratos = new List<string>(eventos.Count);
            for (int i = 0; i < eventos.Count; i++)
            {
                Evento evento = eventos[i];
                EstadoDoNucleo antes = nucleo.Estado;
                var resultados = new List<Resultado>();
                nucleo.Enfileirar(evento);
                IReadOnlyList<Efeito> efeitos = nucleo.Processar((_, r) => resultados.Add(r));
                EstadoDoNucleo depois = nucleo.Estado;
                retratos.Add(depois.Retrato().Descrever());
                foreach (Resultado r in resultados)
                    foreach (Transicao t in r.Transicoes) transicoesVistas.Add((t.De, t.Para));

                Conferir(antes, depois, evento, resultados, efeitos, () => Contexto(i, evento));
                passos++;
            }

            // Invariante 7: mesma semente e mesma sequência dão a mesma sequência de retratos.
            var outro = new Nucleo(cfg, sementeDoNucleo);
            for (int i = 0; i < eventos.Count; i++)
            {
                outro.Enfileirar(eventos[i]);
                outro.Processar();
                string retrato = outro.Retrato.Descrever();
                if (retrato != retratos[i])
                    Afirmar.Falhar($"invariante 7: {Contexto(i, eventos[i])}: primeira execução {retratos[i]}, segunda {retrato}");
            }
        }

        Console.WriteLine($"         {Sequencias} sequências, {passos} passos, {transicoesVistas.Count} pares de transição distintos");

        // Invariante 11: todo estado tem entrada e saída; BOOTING só saída, EXITING só entrada.
        foreach (Estado e in Enum.GetValues<Estado>())
        {
            bool entra = transicoesVistas.Any(t => t.Item2 == e && t.Item1 != e);
            bool sai = transicoesVistas.Any(t => t.Item1 == e && t.Item2 != e);
            if (e == Estado.Booting)
            {
                Afirmar.Falso(entra, "BOOTING não tem entrada");
                Afirmar.Verdadeiro(sai, "BOOTING tem saída");
            }
            else if (e == Estado.Exiting)
            {
                Afirmar.Verdadeiro(entra, "EXITING tem entrada");
                Afirmar.Falso(sai, "EXITING não tem saída");
            }
            else
            {
                Afirmar.Verdadeiro(entra && sai, $"{e}: entrada={entra}, saída={sai}");
            }
        }
    }

    private static void Conferir(
        EstadoDoNucleo antes, EstadoDoNucleo depois, Evento evento, List<Resultado> resultados, IReadOnlyList<Efeito> efeitos, Func<string> onde)
    {
        PontoPx? ancoraAntes = antes.Lugar?.Ancora;
        PontoPx? ancoraDepois = depois.Lugar?.Ancora;
        bool mesmoEstadoEPosicao = antes.Estado == depois.Estado && ancoraAntes == ancoraDepois;

        // Invariante 1: em PRESSED, DRAGGING e SETTLING nada autônomo muda estado ou posição.
        if (antes.Estado is Estado.Pressed or Estado.Dragging or Estado.Settling && evento.Origem is Origem.Autonomo or Origem.Relogio)
            Verificar(mesmoEstadoEPosicao, () => $"invariante 1: {onde()}: {antes.Estado}→{depois.Estado}, {ancoraAntes}→{ancoraDepois}");

        // Invariante 2: em DRAGGING, posição = cursor − pegada.
        if (depois.Estado == Estado.Dragging && evento is DragMove m)
            Verificar(ancoraDepois == new PontoPx(m.Cursor.X - depois.Pegada.X, m.Cursor.Y - depois.Pegada.Y), () => $"invariante 2: {onde()}: âncora {ancoraDepois}");

        // Invariante 5: depois de SETTLING, a âncora está na área útil de um monitor presente.
        if (resultados.SelectMany(r => r.Transicoes).Any(t => t.De == Estado.Settling) && depois.Topologia is { } topologia && ancoraDepois is { } a)
        {
            bool dentro = topologia.Monitores.Any(mon => a.X >= mon.AreaUtil.Esquerda && a.X < mon.AreaUtil.Direita && a.Y > mon.AreaUtil.Topo && a.Y <= mon.AreaUtil.Base);
            Verificar(dentro, () => $"invariante 5: {onde()}: âncora {a} fora de toda área útil de {topologia}");
        }

        // Invariante 6: trocar expressão não muda estado nem posição.
        if (evento is ExpressionChange x)
        {
            Verificar(mesmoEstadoEPosicao, () => $"invariante 6: {onde()}: {antes.Estado}→{depois.Estado}, {ancoraAntes}→{ancoraDepois}");
            Verificar(antes.Estado == Estado.Exiting || depois.Expressao == x.Expressao, () => $"invariante 6: {onde()}: expressão {depois.Expressao}");
        }

        // Invariante 8: abrir ou fechar o painel nunca muda a posição.
        if (evento is EnergyPanelOpen or EnergyPanelClose)
            Verificar(mesmoEstadoEPosicao, () => $"invariante 8: {onde()}: {antes.Estado}→{depois.Estado}, {ancoraAntes}→{ancoraDepois}");

        // Invariante 9: iniciar o arraste fecha o painel; soltar não muda a energia.
        if (evento is DragStart && depois.Estado == Estado.Dragging)
            Verificar(!depois.PainelAberto, () => $"invariante 9: {onde()}: painel aberto no arraste");
        if (evento is DragStart or DragMove or DragEnd or DragCancel)
            Verificar(antes.Preferencias.Energia == depois.Preferencias.Energia, () => $"invariante 9: {onde()}: energia mudou");

        // Invariante 10: desbloquear ou retomar nunca mostra o que o usuário escondeu.
        if (evento is SessionUnlocked or Resumed && antes.Estado == Estado.Hidden && antes.Motivo == MotivoDoOcultamento.PorUsuario)
            Verificar(depois.Estado == Estado.Hidden && depois.Motivo == MotivoDoOcultamento.PorUsuario, () => $"invariante 10: {onde()}: {depois.Estado}/{depois.Motivo}");

        // Invariante 14: a tela cheia não interrompe o gesto do usuário; soltar descarta o retorno.
        if (evento is FullscreenTargetsChanged && antes.Estado is Estado.Pressed or Estado.Dragging)
            Verificar(mesmoEstadoEPosicao, () => $"invariante 14: {onde()}: {antes.Estado}→{depois.Estado}");
        if (evento is DragEnd && antes.Estado == Estado.Dragging)
            Verificar(depois.RetornoDaTelaCheia is null, () => $"invariante 14: {onde()}: retorno temporário sobreviveu ao arraste");

        // Invariante 15: gesto curto não muda estado nem posição e termina com prioridade maior.
        if (antes.Gesto != Gesto.Nenhum && depois.Gesto == Gesto.Nenhum && evento is Tick)
            Verificar(mesmoEstadoEPosicao, () => $"invariante 15: {onde()}: o fim do gesto mudou {antes.Estado}→{depois.Estado}");
        if (antes.Gesto != Gesto.Nenhum && evento.Origem >= Origem.Sistema)
            Verificar(depois.Gesto == Gesto.Nenhum, () => $"invariante 15: {onde()}: gesto {depois.Gesto} sobreviveu");
        if (depois.Gesto != Gesto.Nenhum)
            Verificar(depois.Estado == Estado.Idle, () => $"invariante 15: {onde()}: gesto fora de IDLE ({depois.Estado})");

        // Critério 3: sem movimento nem animação, nenhum TICK agendado.
        bool precisa = depois.Estado.EmMovimento() || depois.Estado == Estado.Reacting || (depois.Estado == Estado.Idle && depois.Gesto != Gesto.Nenhum);
        Verificar(depois.RelogioAtivo == precisa, () => $"critério 3: {onde()}: relógio={depois.RelogioAtivo} em {depois.Estado} com gesto {depois.Gesto}");
        Efeito? ultimoDoRelogio = efeitos.LastOrDefault(e => e is LigarRelogio or DesligarRelogio);
        if (antes.RelogioAtivo != depois.RelogioAtivo)
            Verificar(ultimoDoRelogio is not null && ultimoDoRelogio is LigarRelogio == depois.RelogioAtivo, () => $"critério 3: {onde()}: o relógio mudou sem o efeito certo");
        else
            Verificar(ultimoDoRelogio is null, () => $"critério 3: {onde()}: efeito do relógio sem mudança");

        // Agenda: um temporizador só nos estados que decidem, com a autonomia livre.
        if (depois.DecisaoAgendada)
            Verificar(Maquina.DecideNoEstado(depois.Estado) && !depois.AutonomiaPausada && !depois.PainelAberto && depois.Gesto == Gesto.Nenhum,
                () => $"agenda: {onde()}: temporizador pendente em {depois.Estado} (pausada {depois.AutonomiaPausada}, painel {depois.PainelAberto}, gesto {depois.Gesto})");

        // Janela: mostrar e esconder acompanham a visibilidade.
        if (depois.Estado != Estado.Exiting)
        {
            bool mostrou = efeitos.Any(e => e is MostrarJanela), escondeu = efeitos.Any(e => e is EsconderJanela);
            Verificar(mostrou == (!antes.Estado.Visivel() && depois.Estado.Visivel()), () => $"janela: {onde()}: mostrar={mostrou} de {antes.Estado} para {depois.Estado}");
            Verificar(escondeu == (antes.Estado.Visivel() && !depois.Estado.Visivel()), () => $"janela: {onde()}: esconder={escondeu} de {antes.Estado} para {depois.Estado}");
        }
    }

    /// <summary>Uma sequência começando pela carga, com eventos de todas as origens.</summary>
    private static (ConfiguracaoDoNucleo, ulong, List<Evento>) GerarSequencia(int semente)
    {
        var rnd = new Random(semente);
        var gerador = new GeradorDeTopologias(rnd);
        var cfg = new ConfiguracaoDoNucleo
        {
            QuedaFisica = rnd.Next(2) == 0,
            PainelDeEnergiaDisponivel = rnd.Next(2) == 0,
            ConfiguracoesDisponiveis = rnd.Next(2) == 0,
            Acoes = (AcoesAutonomas)rnd.Next((int)AcoesAutonomas.Todas + 1),
        };
        Topologia topologia = gerador.NovaTopologia();
        var eventos = new List<Evento>
        {
            new Loaded(topologia, null, new Preferencias((NivelDeEnergia)rnd.Next(3), rnd.Next(4) != 0)),
        };

        // Um núcleo-sombra acompanha a sequência para os eventos fazerem sentido (PRESS no
        // personagem, AUTONOMY_TIMER da geração agendada); a conferência roda depois, noutro núcleo.
        var sombra = new Nucleo(cfg, (ulong)semente);
        sombra.Enfileirar(eventos[0]);
        sombra.Processar();

        for (int i = 1; i < EventosPorSequencia; i++)
        {
            Evento e = Sortear(rnd, gerador, sombra.Estado, ref topologia);
            eventos.Add(e);
            sombra.Enfileirar(e);
            sombra.Processar();
        }
        return (cfg, (ulong)semente, eventos);
    }

    private static Evento Sortear(Random rnd, GeradorDeTopologias gerador, EstadoDoNucleo s, ref Topologia topologia)
    {
        PontoPx ancora = s.Lugar?.Ancora ?? new PontoPx(0, 0);
        Topologia atual = topologia;
        PontoPx NoCorpo() => new(ancora.X + rnd.Next(-20, 21), ancora.Y - rnd.Next(5, 60));
        PontoPx Qualquer() => gerador.Ponto(atual);
        string[] chaves = [.. atual.Monitores.Select(m => m.Chave)];

        int sorteio = rnd.Next(100);
        return sorteio switch
        {
            < 18 => new Tick(),
            < 24 => new Press(NoCorpo()),
            < 27 => new Click(),
            < 29 => new DoubleClick(),
            < 32 => new DragStart(),
            < 40 => new DragMove(Qualquer()),
            < 43 => new DragEnd(Qualquer()),
            < 44 => new DragCancel(),
            < 45 => new ContextMenu(NoCorpo()),
            < 47 => new EnergyPanelOpen(),
            < 48 => new EnergySelected((NivelDeEnergia)rnd.Next(3)),
            < 49 => new EnergyPanelClose(),
            < 51 => new CmdHide(),
            < 53 => new CmdShow(),
            < 54 => new CmdPauseAutonomy(),
            < 55 => new CmdResumeAutonomy(),
            < 56 => new CmdOpenSettings(),
            < 57 => new CmdResetPosition(),
            < 58 => rnd.Next(20) == 0 ? new CmdExit() : new Tick(),
            < 61 => NovaTopologia(rnd, gerador, ref topologia),
            < 62 => new SessionLocked(),
            < 63 => new SessionUnlocked(),
            < 64 => new Suspending(),
            < 65 => new Resumed(),
            < 66 => rnd.Next(20) == 0 ? new SessionEnding() : new Tick(),
            < 70 => new FullscreenTargetsChanged(new MonitoresOcupados(chaves.Where(_ => rnd.Next(2) == 0))),
            < 71 => new SettingsChanged(new Preferencias((NivelDeEnergia)rnd.Next(3), rnd.Next(3) != 0)),
            < 78 => new MovementSignal((SinalDeMovimento)rnd.Next(Enum.GetValues<SinalDeMovimento>().Length)),
            < 90 => new AutonomyTimer(rnd.Next(10) == 0 ? s.Geracao - 1 : s.Geracao),
            _ => new ExpressionChange((Expressao)rnd.Next(Enum.GetValues<Expressao>().Length)),
        };
    }

    private static TopologyChanged NovaTopologia(Random rnd, GeradorDeTopologias gerador, ref Topologia topologia)
    {
        topologia = rnd.Next(3) == 0 ? gerador.NovaTopologia() : gerador.Mudar(topologia);
        return new TopologyChanged(topologia);
    }

    private static void Verificar(bool condicao, Func<string> mensagem)
    {
        if (!condicao) Afirmar.Falhar(mensagem());
    }
}

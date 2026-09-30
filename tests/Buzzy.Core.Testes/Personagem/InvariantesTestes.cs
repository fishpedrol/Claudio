using System.Text;
using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.Core.Testes.Personagem;

/// <summary>
/// Testes de propriedade da máquina de estados (critérios 2 e 3 da Fase 2): milhares de
/// sequências aleatórias de eventos, sobre topologias aleatórias e configurações variadas,
/// conferindo a cada evento aplicado os invariantes de ARCHITECTURE.md 2.6, a regra do relógio e
/// as regras do núcleo R1, R6, R8, R9, R11 e R12 (Maquina.cs). Parte das sequências entrega os
/// eventos em lotes de 1 a 4 por <see cref="Nucleo.Processar"/>, como a raiz faz com rajadas;
/// parte começa com pedidos anteriores à carga; parte usa perfis de decisão curtos, para o piso
/// do intervalo de acomodação importar. A semente é fixa; toda falha informa a sequência, o lote,
/// o evento e as opções da sequência, para virar teste.
/// </summary>
internal static class InvariantesTestes
{
    private const int Semente = 20260929;
    private const int Sequencias = 2000;
    private const int EventosPorSequencia = 200;

    /// <summary>Chave que nenhuma topologia gerada tem (o gerador usa DISPLAY1 a DISPLAY9).</summary>
    private const string ChaveDesconhecida = @"\\.\DISPLAY42";

    /// <summary>Situações que o gerador precisa produzir pelo menos uma vez, para nenhuma conferência nova ficar vazia.</summary>
    private static readonly string[] CasosExigidos =
    [
        "evento aplicado num lote de 2 a 4", "evento antes da carga", "carga depois de pedidos de esconder", "carga repetida",
        "carga com posição salva em monitor inexistente", "carga fora do enum", "SETTINGS_CHANGED fora do enum",
        "ENERGY_SELECTED fora do enum com o painel aberto", "tela cheia com chave desconhecida", "expressão trocada sem transição",
        "DRAG_CANCEL do arraste com retorno", "CMD_SHOW com retorno", "fim da tela cheia sem retorno, visível",
        "agendamento no piso de um sorteio menor", "AUTONOMY_TIMER com o painel aberto", "painel abriu ou fechou sem troca de estado",
        "DRAG_START com o painel aberto", "esconder ou sair com retorno guardado",
    ];

    /// <summary>Uma sequência gerada: configuração, semente do núcleo e os eventos, em lotes.</summary>
    private sealed record Sequencia(
        ConfiguracaoDoNucleo Config, ulong SementeDoNucleo, List<List<Evento>> Lotes, bool EmLotes, bool AntesDaCarga, bool PerfilCurto)
    {
        public string Opcoes => $"lotes {(EmLotes ? "de 1 a 4" : "de 1")}, {(AntesDaCarga ? "com" : "sem")} eventos antes da carga, perfil {(PerfilCurto ? "curto" : "padrão")}";
    }

    [Teste]
    public static void InvariantesValemEmMilharesDeSequenciasAleatorias()
    {
        var mestre = new Random(Semente);
        var transicoesVistas = new HashSet<(Estado, Estado)>();
        long passos = 0, sequenciasEmLotes = 0, sequenciasAntesDaCarga = 0, sequenciasComPerfilCurto = 0;
        // Quantas vezes cada situação das regras novas apareceu: nenhuma conferência pode ficar vazia.
        var casos = new SortedDictionary<string, long>(StringComparer.Ordinal);
        void Contar(string caso) => casos[caso] = casos.GetValueOrDefault(caso) + 1;

        for (int n = 0; n < Sequencias; n++)
        {
            int sementeDaSequencia = mestre.Next();
            Sequencia seq = GerarSequencia(sementeDaSequencia);
            if (seq.EmLotes) sequenciasEmLotes++;
            if (seq.AntesDaCarga) sequenciasAntesDaCarga++;
            if (seq.PerfilCurto) sequenciasComPerfilCurto++;

            // R-a: o estado antes de cada Aplicar vem do callback, também dentro de um lote.
            var nucleo = new Nucleo(seq.Config, seq.SementeDoNucleo);
            var registro = new List<string>();
            for (int l = 0; l < seq.Lotes.Count; l++)
            {
                int lote = l, aplicado = 0, tamanho = seq.Lotes[l].Count;
                foreach (Evento e in seq.Lotes[l]) nucleo.Enfileirar(e);
                EstadoDoNucleo anterior = nucleo.Estado;
                nucleo.Processar((evento, resultado) =>
                {
                    int i = aplicado++;
                    string Onde() => $"sequência {n} (semente {sementeDaSequencia}; {seq.Opcoes}), lote {lote}, {i + 1}º evento aplicado do lote, evento {evento}";
                    Conferir(seq.Config, anterior, evento, resultado, Onde, Contar);
                    if (tamanho > 1) Contar("evento aplicado num lote de 2 a 4");
                    foreach (Transicao t in resultado.Transicoes) transicoesVistas.Add((t.De, t.Para));
                    registro.Add(Registrar(evento, resultado));
                    anterior = resultado.Estado;
                    passos++;
                });
            }

            // Invariante 7 (R-d): mesma semente e mesma sequência dão, evento a evento, o mesmo
            // retrato, as mesmas transições e os mesmos efeitos.
            var outro = new Nucleo(seq.Config, seq.SementeDoNucleo);
            int k = 0;
            for (int l = 0; l < seq.Lotes.Count; l++)
            {
                int lote = l;
                foreach (Evento e in seq.Lotes[l]) outro.Enfileirar(e);
                outro.Processar((evento, resultado) =>
                {
                    string linha = Registrar(evento, resultado);
                    string primeira = k < registro.Count ? registro[k] : "(nada)";
                    if (linha != primeira)
                        Afirmar.Falhar($"invariante 7: sequência {n} (semente {sementeDaSequencia}; {seq.Opcoes}), lote {lote}, evento aplicado {k}: primeira execução <{primeira}>, segunda <{linha}>");
                    k++;
                });
            }
            Afirmar.Igual(registro.Count, k, $"invariante 7: sequência {n}: mesmo número de eventos aplicados");
            Afirmar.Igual(nucleo.Descartados, outro.Descartados, $"invariante 7: sequência {n}: mesmos descartes");
        }

        Console.WriteLine($"         {Sequencias} sequências ({sequenciasEmLotes} em lotes, {sequenciasAntesDaCarga} com eventos antes da carga, {sequenciasComPerfilCurto} com perfil curto), {passos} eventos aplicados, {transicoesVistas.Count} pares de transição distintos");
        Console.WriteLine("         casos: " + string.Join(", ", casos.Select(c => $"{c.Key}={c.Value}")));
        foreach (string caso in CasosExigidos)
            Afirmar.Verdadeiro(casos.GetValueOrDefault(caso) > 0, $"o gerador não exercitou \"{caso}\": a conferência correspondente ficou vazia");

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

    /// <summary>Confere um evento aplicado: <paramref name="antes"/> é o estado logo antes do Aplicar.</summary>
    private static void Conferir(ConfiguracaoDoNucleo cfg, EstadoDoNucleo antes, Evento evento, Resultado r, Func<string> onde, Action<string> contar)
    {
        EstadoDoNucleo depois = r.Estado;
        IReadOnlyList<Efeito> efeitos = r.Efeitos;
        IReadOnlyList<Transicao> transicoes = r.Transicoes;
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
        if (transicoes.Any(t => t.De == Estado.Settling) && depois.Topologia is { } topologia && ancoraDepois is { } a)
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

        // Invariante 6 generalizado (R-c): toda troca de expressão sem transição, venha do evento que
        // vier (clique duplo, decisão autônoma, troca pedida), mantém estado e âncora.
        if (antes.Expressao != depois.Expressao && transicoes.Count == 0)
        {
            contar("expressão trocada sem transição");
            Verificar(mesmoEstadoEPosicao, () => $"invariante 6: {onde()}: expressão {antes.Expressao}→{depois.Expressao} com {antes.Estado}→{depois.Estado}, {ancoraAntes}→{ancoraDepois}");
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
        if (evento is FullscreenTargetsChanged f && depois.Topologia is { } atual && f.Ocupados.Chaves.Any(ch => atual.PorChave(ch) is null))
            contar("tela cheia com chave desconhecida");
        if (evento is FullscreenTargetsChanged && antes.Estado is Estado.Pressed or Estado.Dragging)
            Verificar(mesmoEstadoEPosicao, () => $"invariante 14: {onde()}: {antes.Estado}→{depois.Estado}");
        if (evento is DragEnd && antes.Estado == Estado.Dragging)
            Verificar(depois.RetornoDaTelaCheia is null, () => $"invariante 14: {onde()}: retorno temporário sobreviveu ao arraste");

        // Invariante 14 estendido (R-g, regras R7 e R9): cancelar o arraste e mostrar manualmente
        // também são escolha do usuário e descartam o retorno; sem retorno, o fim da tela cheia não
        // move o personagem visível.
        if (evento is DragCancel && antes.Estado == Estado.Dragging)
        {
            if (antes.RetornoDaTelaCheia is not null) contar("DRAG_CANCEL do arraste com retorno");
            Verificar(depois.RetornoDaTelaCheia is null, () => $"invariante 14: {onde()}: retorno temporário sobreviveu ao DRAG_CANCEL do arraste");
        }
        if (evento is CmdShow && antes.Estado is not (Estado.Booting or Estado.Exiting))
        {
            if (antes.RetornoDaTelaCheia is not null) contar("CMD_SHOW com retorno");
            Verificar(depois.RetornoDaTelaCheia is null, () => $"invariante 14: {onde()}: retorno temporário sobreviveu ao CMD_SHOW em {antes.Estado}");
        }
        if (evento is FullscreenTargetsChanged { Ocupados.Vazio: true } && antes.RetornoDaTelaCheia is null && antes.Estado.Visivel())
        {
            contar("fim da tela cheia sem retorno, visível");
            Verificar(transicoes.Count == 0 && ancoraAntes == ancoraDepois, () => $"invariante 14: {onde()}: fim da tela cheia sem retorno moveu {antes.Estado} {ancoraAntes}→{ancoraDepois}");
        }

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

        // Critério 3 afirmado diretamente (R-b): sem relógio em IDLE sem gesto, RESTING, HIDDEN,
        // PRESSED, DRAGGING, BOOTING e EXITING.
        if ((depois.Estado == Estado.Idle && depois.Gesto == Gesto.Nenhum)
            || depois.Estado is Estado.Resting or Estado.Hidden or Estado.Pressed or Estado.Dragging or Estado.Booting or Estado.Exiting)
            Verificar(!depois.RelogioAtivo, () => $"critério 3: {onde()}: relógio ligado em {depois.Estado} (gesto {depois.Gesto})");

        // Agenda: um temporizador só nos estados que decidem, com a autonomia livre.
        if (depois.DecisaoAgendada)
            Verificar(Maquina.DecideNoEstado(depois.Estado) && !depois.AutonomiaPausada && !depois.PainelAberto && depois.Gesto == Gesto.Nenhum,
                () => $"agenda: {onde()}: temporizador pendente em {depois.Estado} (pausada {depois.AutonomiaPausada}, painel {depois.PainelAberto}, gesto {depois.Gesto})");

        // R11 (R-h): todo agendamento respeita o piso do intervalo de acomodação e a faixa do perfil
        // do nível de energia em vigor (descanso em RESTING, decisão nos demais).
        AgendarDecisao[] agendas = [.. efeitos.OfType<AgendarDecisao>()];
        Verificar(agendas.Length <= 1, () => $"agenda: {onde()}: {agendas.Length} agendamentos num só evento");
        foreach (AgendarDecisao agenda in agendas)
        {
            TimeSpan piso = cfg.IntervaloDeAcomodacao;
            PerfilDeEnergia perfil = cfg.Perfil(depois.Preferencias.Energia);
            (TimeSpan minimo, TimeSpan maximo) = depois.Estado == Estado.Resting
                ? (perfil.DescansoMinimo, perfil.DescansoMaximo)
                : (perfil.DecisaoMinima, perfil.DecisaoMaxima);
            if (agenda.Atraso == piso && minimo < piso) contar("agendamento no piso de um sorteio menor");
            Verificar(agenda.Atraso >= piso, () => $"R11: {onde()}: atraso {agenda.Atraso} abaixo do intervalo de acomodação {piso}");
            Verificar(agenda.Atraso >= Maior(minimo, piso) && agenda.Atraso <= Maior(maximo, piso),
                () => $"R11: {onde()}: atraso {agenda.Atraso} fora da faixa {minimo}–{maximo} (piso {piso}) do perfil {perfil.Nivel} em {depois.Estado}");
            Verificar(depois.DecisaoAgendada && agenda.Geracao == depois.Geracao, () => $"agenda: {onde()}: geração {agenda.Geracao} agendada, estado com {depois.Geracao} (pendente {depois.DecisaoAgendada})");
        }

        // Janela: mostrar e esconder acompanham a visibilidade.
        if (depois.Estado != Estado.Exiting)
        {
            bool mostrou = efeitos.Any(e => e is MostrarJanela), escondeu = efeitos.Any(e => e is EsconderJanela);
            Verificar(mostrou == (!antes.Estado.Visivel() && depois.Estado.Visivel()), () => $"janela: {onde()}: mostrar={mostrou} de {antes.Estado} para {depois.Estado}");
            Verificar(escondeu == (antes.Estado.Visivel() && !depois.Estado.Visivel()), () => $"janela: {onde()}: esconder={escondeu} de {antes.Estado} para {depois.Estado}");
        }

        // R6 (R-e): antes da carga o personagem nunca aparece; a primeira carga vale, com a topologia
        // e as preferências dela (energia saneada); as seguintes são ignoradas.
        if (!depois.Carregado)
        {
            contar("evento antes da carga");
            Verificar(!depois.Estado.Visivel(), () => $"R6: {onde()}: visível ({depois.Estado}) antes da carga");
        }
        if (evento is Loaded carga)
        {
            if (antes.Carregado || antes.Estado == Estado.Exiting)
            {
                contar("carga repetida");
                Verificar(transicoes.Count == 0 && antes.Estado == depois.Estado && antes.Motivo == depois.Motivo
                        && Equals(antes.Lugar, depois.Lugar) && Equals(antes.Posicao, depois.Posicao)
                        && antes.Preferencias == depois.Preferencias && ReferenceEquals(antes.Topologia, depois.Topologia),
                    () => $"R6: {onde()}: carga repetida não foi ignorada ({antes.Estado}→{depois.Estado}, {Descrever(transicoes)})");
            }
            else
            {
                if (antes.Estado == Estado.Hidden) contar("carga depois de pedidos de esconder");
                if (carga.PosicaoSalva is { } salva && carga.Topologia.PorChave(salva.ChaveMonitor) is null) contar("carga com posição salva em monitor inexistente");
                if (!Enum.IsDefined(carga.Preferencias.Energia)) contar("carga fora do enum");
                Verificar(depois.Carregado && ReferenceEquals(depois.Topologia, carga.Topologia) && depois.Preferencias == Saneadas(carga.Preferencias),
                    () => $"R6: {onde()}: a primeira carga não valeu (carregado {depois.Carregado}, preferências {depois.Preferencias})");
            }
        }

        // R1 (R-e): a energia é sempre um dos três níveis; ENERGY_SELECTED fora deles é ignorado, sem gravar.
        Verificar(Enum.IsDefined(depois.Preferencias.Energia), () => $"R1: {onde()}: energia fora do enum: {(int)depois.Preferencias.Energia}");
        if (evento is SettingsChanged { Preferencias.Energia: var configurado } && !Enum.IsDefined(configurado))
            contar("SETTINGS_CHANGED fora do enum");
        if (evento is EnergySelected { Nivel: var nivel } && !Enum.IsDefined(nivel))
        {
            if (antes.PainelAberto) contar("ENERGY_SELECTED fora do enum com o painel aberto");
            Verificar(antes.Preferencias == depois.Preferencias && !efeitos.Any(e => e is GravarPreferencias), () => $"R1: {onde()}: ENERGY_SELECTED({(int)nivel}) não foi ignorado");
        }

        // Painel (R-f). Invariante 4: com o painel aberto, AUTONOMY_TIMER não produz transição,
        // gesto nem expressão, e nenhuma transição entra num estado autônomo em movimento ou em
        // RESTING, exceto pelo próprio movimento (MovementSignal: a física em curso termina; o
        // comportamento calmo de CLIMBING/HANGING com o painel aberto é da Fase 4).
        if (evento is AutonomyTimer && antes.PainelAberto && depois.PainelAberto)
        {
            contar("AUTONOMY_TIMER com o painel aberto");
            Verificar(transicoes.Count == 0 && depois.Gesto == antes.Gesto && depois.Expressao == antes.Expressao,
                () => $"invariante 4: {onde()}: decisão autônoma com o painel aberto ({Descrever(transicoes)}, gesto {depois.Gesto}, expressão {depois.Expressao})");
        }
        if (antes.PainelAberto && depois.PainelAberto && evento is not MovementSignal)
            Verificar(!transicoes.Any(t => t.Para is Estado.Walking or Estado.Climbing or Estado.Hanging or Estado.Jumping or Estado.Resting),
                () => $"invariante 4: {onde()}: comportamento autônomo começou com o painel aberto ({Descrever(transicoes)})");
        // Invariante 8 em todo passo em que o painel abre ou fecha sem troca de estado.
        if (antes.PainelAberto != depois.PainelAberto && antes.Estado == depois.Estado)
        {
            contar("painel abriu ou fechou sem troca de estado");
            Verificar(ancoraAntes == ancoraDepois, () => $"invariante 8: {onde()}: o painel {(depois.PainelAberto ? "abriu" : "fechou")} e a âncora foi de {ancoraAntes} a {ancoraDepois}");
        }
        // Invariante 9: iniciar o arraste com o painel aberto pede para fechá-lo.
        if (evento is DragStart && antes.PainelAberto && depois.Estado == Estado.Dragging)
        {
            contar("DRAG_START com o painel aberto");
            Verificar(efeitos.Any(e => e is FecharPainelDeEnergia), () => $"invariante 9: {onde()}: arraste começou sem FecharPainelDeEnergia");
        }
        // Painel só existe com o personagem visível.
        if (!depois.Estado.Visivel())
            Verificar(!depois.PainelAberto, () => $"painel: {onde()}: aberto em {depois.Estado}");

        // R8 (R-i): esconder ou sair grava a posição de antes da tela cheia quando havia retorno.
        // De DRAGGING, o arraste interrompido vale como DRAG_CANCEL (ARCHITECTURE.md 2.7), que
        // descarta o retorno (R9): grava o ponto validado do arraste.
        if (evento is CmdHide or SessionLocked or Suspending or CmdExit or SessionEnding
            && antes.RetornoDaTelaCheia is { } retorno && antes.Estado != Estado.Dragging)
        {
            if (efeitos.Any(e => e is GravarPosicao)) contar("esconder ou sair com retorno guardado");
            foreach (GravarPosicao g in efeitos.OfType<GravarPosicao>())
                Verificar(Equals(g.Posicao, retorno), () => $"R8: {onde()}: gravou {Gravacao.DescreverPosicao(g.Posicao)} em vez do retorno {Gravacao.DescreverPosicao(retorno)}");
        }
    }

    /// <summary>
    /// Uma sequência: às vezes com pedidos anteriores à carga, depois a carga e eventos de todas as
    /// origens, em lotes de 1 (ou de 1 a 4) eventos.
    /// </summary>
    private static Sequencia GerarSequencia(int semente)
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
        // R-h: perfil com decisões e descansos curtos e piso variável, para o piso do intervalo de
        // acomodação importar (com o perfil padrão, todo sorteio já passa de 3 s).
        bool perfilCurto = rnd.Next(3) == 0;
        if (perfilCurto)
            cfg = cfg with { Perfil = PerfilCurto, IntervaloDeAcomodacao = TimeSpan.FromMilliseconds(rnd.Next(300, 5001)) };
        bool emLotes = rnd.Next(3) == 0;
        bool antesDaCarga = rnd.Next(4) == 0;
        Topologia topologia = gerador.NovaTopologia();

        // Um núcleo-sombra acompanha a sequência, lote a lote como a execução conferida, para os
        // eventos fazerem sentido (PRESS no personagem, AUTONOMY_TIMER da geração agendada).
        var sombra = new Nucleo(cfg, (ulong)semente);
        var lotes = new List<List<Evento>>();
        int total = 0;
        void Entregar(List<Evento> lote)
        {
            lotes.Add(lote);
            foreach (Evento e in lote) sombra.Enfileirar(e);
            sombra.Processar();
            total += lote.Count;
        }

        if (antesDaCarga)
        {
            for (int i = rnd.Next(1, 7); i > 0; i--)
                Entregar([SortearAntesDaCarga(rnd, gerador, sombra.Estado, ref topologia)]);
        }
        Entregar([NovaCarga(rnd, gerador, topologia)]);

        while (total < EventosPorSequencia)
        {
            int tamanho = emLotes ? rnd.Next(1, 5) : 1;
            var lote = new List<Evento>(tamanho);
            for (int j = 0; j < tamanho; j++) lote.Add(Sortear(rnd, gerador, sombra.Estado, ref topologia));
            Entregar(lote);
        }
        return new Sequencia(cfg, (ulong)semente, lotes, emLotes, antesDaCarga, perfilCurto);
    }

    /// <summary>
    /// Pedidos que podem chegar antes da carga (R6): esconder, mostrar, sessão, suspensão,
    /// topologia, tela cheia, configurações, e às vezes qualquer outro evento.
    /// </summary>
    private static Evento SortearAntesDaCarga(Random rnd, GeradorDeTopologias gerador, EstadoDoNucleo s, ref Topologia topologia) => rnd.Next(12) switch
    {
        0 => new CmdHide(),
        1 => new CmdShow(),
        2 => new SessionLocked(),
        3 => new SessionUnlocked(),
        4 => new Suspending(),
        5 => new Resumed(),
        6 => NovaTopologia(rnd, gerador, ref topologia),
        7 => TelaCheia(rnd, topologia),
        8 => new SettingsChanged(new Preferencias(Nivel(rnd), rnd.Next(3) != 0)),
        9 => new CmdResetPosition(),
        10 => rnd.Next(2) == 0 ? new CmdPauseAutonomy() : new CmdResumeAutonomy(),
        _ => Sortear(rnd, gerador, s, ref topologia),
    };

    /// <summary>
    /// Carga com preferências às vezes inválidas e, metade das vezes, uma posição salva: num monitor
    /// da topologia ou numa chave que ela não tem, com frações às vezes fora de [0, 1] ou NaN.
    /// </summary>
    private static Loaded NovaCarga(Random rnd, GeradorDeTopologias gerador, Topologia topologia)
    {
        PosicaoDoPersonagem? salva = null;
        if (rnd.Next(2) == 0)
        {
            string chave = rnd.Next(4) == 0 ? ChaveDesconhecida : topologia.Monitores[rnd.Next(topologia.Monitores.Count)].Chave;
            double Fracao() => rnd.Next(5) == 0 ? gerador.Fracao() : rnd.NextDouble();
            salva = new PosicaoDoPersonagem(chave, Fracao(), Fracao(), gerador.Ponto(topologia));
        }
        return new Loaded(topologia, salva, new Preferencias(Nivel(rnd), rnd.Next(4) != 0));
    }

    private static Evento Sortear(Random rnd, GeradorDeTopologias gerador, EstadoDoNucleo s, ref Topologia topologia)
    {
        PontoPx ancora = s.Lugar?.Ancora ?? new PontoPx(0, 0);
        Topologia atual = topologia;
        PontoPx NoCorpo() => new(ancora.X + rnd.Next(-20, 21), ancora.Y - rnd.Next(5, 60));
        PontoPx Qualquer() => gerador.Ponto(atual);

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
            < 48 => new EnergySelected(Nivel(rnd)),
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
            < 70 => TelaCheia(rnd, atual),
            < 71 => new SettingsChanged(new Preferencias(Nivel(rnd), rnd.Next(3) != 0)),
            < 78 => new MovementSignal(SinalCoerente(rnd, s.Estado)),
            < 90 => new AutonomyTimer(rnd.Next(10) == 0 ? s.Geracao - 1 : s.Geracao),
            // R-e: carga repetida no meio da sequência, com outra topologia; deve ser ignorada.
            < 91 => NovaCarga(rnd, gerador, rnd.Next(2) == 0 ? atual : gerador.NovaTopologia()),
            _ => new ExpressionChange((Expressao)rnd.Next(Enum.GetValues<Expressao>().Length)),
        };
    }

    /// <summary>
    /// Sinal do movimento: dois terços das vezes um que o estado atual trata (como o passo físico da
    /// Fase 4 emitirá), para a sequência chegar a HANGING, pousar e sair da parede; no resto, qualquer um.
    /// </summary>
    private static SinalDeMovimento SinalCoerente(Random rnd, Estado estado)
    {
        SinalDeMovimento[] coerentes = estado switch
        {
            Estado.Walking => [SinalDeMovimento.Parede, SinalDeMovimento.Passagem, SinalDeMovimento.FimDoChao],
            Estado.Climbing => [SinalDeMovimento.TopoDaParede, SinalDeMovimento.FimDaParede, SinalDeMovimento.BordaSuperior],
            Estado.Hanging => [SinalDeMovimento.FimDaBorda],
            Estado.Jumping or Estado.Falling => [SinalDeMovimento.ContatoComOChao],
            _ => [],
        };
        return coerentes.Length > 0 && rnd.Next(3) != 0
            ? coerentes[rnd.Next(coerentes.Length)]
            : (SinalDeMovimento)rnd.Next(Enum.GetValues<SinalDeMovimento>().Length);
    }

    /// <summary>Monitores ocupados: qualquer subconjunto dos presentes e, às vezes, uma chave que a topologia não tem.</summary>
    private static FullscreenTargetsChanged TelaCheia(Random rnd, Topologia topologia)
    {
        List<string> chaves = [.. topologia.Monitores.Select(m => m.Chave).Where(_ => rnd.Next(2) == 0)];
        if (rnd.Next(5) == 0) chaves.Add(rnd.Next(2) == 0 ? ChaveDesconhecida : GeradorDeTopologias.Chave(rnd.Next(1, 10)));
        return new FullscreenTargetsChanged(new MonitoresOcupados(chaves));
    }

    /// <summary>Nível de energia; uma vez em dez, fora do enum (3 a 99), como um arquivo adulterado (SECURITY.md 7).</summary>
    private static NivelDeEnergia Nivel(Random rnd) => rnd.Next(10) == 0 ? (NivelDeEnergia)rnd.Next(3, 100) : (NivelDeEnergia)rnd.Next(3);

    private static TopologyChanged NovaTopologia(Random rnd, GeradorDeTopologias gerador, ref Topologia topologia)
    {
        topologia = rnd.Next(3) == 0 ? gerador.NovaTopologia() : gerador.Mudar(topologia);
        return new TopologyChanged(topologia);
    }

    /// <summary>Perfis com decisões de 100 a 900 ms e descansos de 200 a 1500 ms.</summary>
    private static PerfilDeEnergia PerfilCurto(NivelDeEnergia nivel) => PerfilDeEnergia.Padrao(nivel) with
    {
        DecisaoMinima = TimeSpan.FromMilliseconds(100),
        DecisaoMaxima = TimeSpan.FromMilliseconds(900),
        DescansoMinimo = TimeSpan.FromMilliseconds(200),
        DescansoMaximo = TimeSpan.FromMilliseconds(1500),
    };

    /// <summary>As preferências como a carga deve guardá-las: nível fora do enum vira Média (R1).</summary>
    private static Preferencias Saneadas(Preferencias p) => Enum.IsDefined(p.Energia) ? p : p with { Energia = NivelDeEnergia.Media };

    private static TimeSpan Maior(TimeSpan a, TimeSpan b) => a > b ? a : b;

    /// <summary>Linha de um evento aplicado: o evento, as transições, os efeitos e o retrato, no formato das reproduções.</summary>
    private static string Registrar(Evento evento, Resultado r)
    {
        var sb = new StringBuilder(Gravacao.Escrever(evento, t => t.ImpressaoDigital));
        foreach (Transicao t in r.Transicoes) sb.Append(" ~ ").Append(t);
        foreach (Efeito e in r.Efeitos) sb.Append(" ! ").Append(Gravacao.DescreverEfeito(e));
        sb.Append(" = ").Append(r.Estado.Retrato().Descrever());
        return sb.ToString();
    }

    private static string Descrever(IEnumerable<Transicao> transicoes) => string.Join(", ", transicoes.Select(t => $"{t.De}->{t.Para}"));

    private static void Verificar(bool condicao, Func<string> mensagem)
    {
        if (!condicao) Afirmar.Falhar(mensagem());
    }
}

using System.Text;
using Buzzy.Core.Persistencia;
using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.Core.Testes.Personagem;

/// <summary>
/// Testes de propriedade da máquina de estados (critérios 2 e 3 da Fase 2): milhares de
/// sequências aleatórias de eventos, sobre topologias aleatórias e configurações variadas,
/// conferindo a cada evento aplicado os invariantes de ARCHITECTURE.md 2.6 (inclusive o 18, da Fase 5:
/// toda posição gravada é gravável, só sai de evento do usuário ou do sistema e volta do settings.json),
/// a regra do relógio, as linhas de FULLSCREEN_TARGETS_CHANGED e as regras do núcleo R1, R6, R8, R9, R11
/// e R12 (Maquina.cs). Parte das sequências entrega os eventos em lotes de 1 a 4 por
/// <see cref="Nucleo.Processar"/>, como a raiz faz com rajadas; parte começa com pedidos
/// anteriores à carga; parte usa perfis de decisão e gestos curtos, para o piso do intervalo de
/// acomodação e o fim do gesto pelo relógio acontecerem. A semente é fixa; toda falha informa a
/// sequência, o lote, o evento e as opções da sequência, para virar teste.
///
/// Cada conferência nova conta quantas vezes a situação dela apareceu (<see cref="CasosExigidos"/>):
/// uma conferência que o gerador nunca exercita não protege nada. Duas conferências ficam fora da
/// lista por serem inalcançáveis por construção: um AUTONOMY_TIMER da geração pendente com o painel
/// aberto (abrir o painel já cancela a agenda) e um evento autônomo em PRESSED ou DRAGGING (o
/// <see cref="Nucleo"/> o descarta antes da máquina); para elas, o que se confere é a causa: a agenda
/// sem temporizador com o painel aberto e o descarte pelo núcleo.
/// </summary>
internal static class InvariantesTestes
{
    private const int Semente = 20260929;
    private const int Sequencias = 2000;
    private const int EventosPorSequencia = 200;

    /// <summary>Chave que nenhuma topologia gerada tem (o gerador usa DISPLAY1 a DISPLAY9).</summary>
    private const string ChaveDesconhecida = @"\\.\DISPLAY42";

    /// <summary>Situações que o gerador precisa produzir pelo menos uma vez, para nenhuma conferência ficar vazia.</summary>
    private static readonly string[] CasosExigidos =
    [
        "evento aplicado num lote de 2 a 4", "evento antes da carga", "carga depois de pedidos de esconder", "carga repetida",
        "carga que mantém um pedido de esconder", "carga que mostra o personagem", "carga com posição salva conferida",
        "carga com posição salva em monitor inexistente", "carga restaurada pelo retângulo do monitor", "carga restaurada no monitor principal",
        "carga fora do enum", "SETTINGS_CHANGED fora do enum",
        "ENERGY_SELECTED fora do enum com o painel aberto", "expressão trocada sem transição",
        "DRAG_CANCEL do arraste com retorno", "CMD_SHOW com retorno", "fim da tela cheia sem retorno, visível",
        "agendamento no piso de um sorteio menor", "painel abriu ou fechou sem troca de estado", "DRAG_START com o painel aberto",
        "esconder ou sair com retorno guardado", "esconder ou sair no meio do arraste com retorno",
        "tela cheia com chave desconhecida", "tela cheia repetida", "tela cheia com o modo desligado", "tela cheia durante o gesto",
        "tela cheia transfere com mais de um monitor livre", "tela cheia esconde sem monitor livre",
        "tela cheia vazia restaura a posição anterior", "HIDDEN(POR_TELA_CHEIA) reaparece no livre mais próximo do retorno",
        "fim da tela cheia escondido por outro motivo, com retorno", "tela cheia não vazia escondido por outro motivo, com retorno",
        "fim do clique com retorno e tela cheia mudada", "fim do clique com retorno, sem mudança", "PRESS com a marca de um gesto interrompido",
        "desligar o modo com retorno, visível ou escondido pela tela cheia", "gesto terminou por TICK",
        "desligar o modo com retorno no meio do gesto", "fim do clique com retorno e o modo desligado no gesto",
        "CMD_SHOW escondido pela tela cheia, com retorno", "CMD_SHOW escondido por outro motivo no meio de um episódio",
        "TICK ou sinal de movimento com o usuário no controle", "AUTONOMY_TIMER descartado com o usuário no controle",
        "GRAVAR_POSICAO conferida (invariante 18)", "carga com a travessia desligada", "SETTINGS_CHANGED com a travessia desligada",
    ];

    /// <summary>Mínimo de atrasos distintos acima do piso: um atraso fixo (no mínimo do perfil, por exemplo) passaria na conferência de faixa.</summary>
    private const int AtrasosDistintosMinimos = 100;

    /// <summary>Uma sequência gerada: configuração, semente do núcleo e os eventos, em lotes.</summary>
    private sealed record Sequencia(
        ConfiguracaoDoNucleo Config, ulong SementeDoNucleo, List<List<Evento>> Lotes, bool EmLotes, bool AntesDaCarga, bool PerfilCurto)
    {
        public string Opcoes => $"lotes {(EmLotes ? "de 1 a 4" : "de 1")}, {(AntesDaCarga ? "com" : "sem")} eventos antes da carga, perfil {(PerfilCurto ? "curto" : "padrão")}";
    }

    /// <summary>O que as conferências acumulam entre eventos: quantas vezes cada situação apareceu e os atrasos sorteados.</summary>
    private sealed class Contagens
    {
        public SortedDictionary<string, long> Casos { get; } = new(StringComparer.Ordinal);

        public HashSet<TimeSpan> AtrasosAcimaDoPiso { get; } = [];

        public void Contar(string caso, long vezes = 1) => Casos[caso] = Casos.GetValueOrDefault(caso) + vezes;
    }

    [Teste]
    public static void InvariantesValemEmMilharesDeSequenciasAleatorias()
    {
        var mestre = new Random(Semente);
        var transicoesVistas = new HashSet<(Estado, Estado)>();
        long passos = 0, sequenciasEmLotes = 0, sequenciasAntesDaCarga = 0, sequenciasComPerfilCurto = 0;
        var contagens = new Contagens();

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
                    Conferir(seq.Config, anterior, evento, resultado, Onde, contagens);
                    if (tamanho > 1) contagens.Contar("evento aplicado num lote de 2 a 4");
                    foreach (Transicao t in resultado.Transicoes) transicoesVistas.Add((t.De, t.Para));
                    registro.Add(Registrar(evento, resultado));
                    anterior = resultado.Estado;
                    passos++;
                });
            }
            // Invariante 1 e ARCHITECTURE.md 2.3: o núcleo descarta o evento autônomo que chega com
            // o usuário no controle (a conferência de cada evento aplicado confirma que nenhum passou).
            contagens.Contar("AUTONOMY_TIMER descartado com o usuário no controle", nucleo.Descartados);

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

        Console.WriteLine($"         {Sequencias} sequências ({sequenciasEmLotes} em lotes, {sequenciasAntesDaCarga} com eventos antes da carga, {sequenciasComPerfilCurto} com perfil curto), {passos} eventos aplicados, {transicoesVistas.Count} pares de transição distintos, {contagens.AtrasosAcimaDoPiso.Count} atrasos distintos acima do piso");
        Console.WriteLine("         casos: " + string.Join(", ", contagens.Casos.Select(c => $"{c.Key}={c.Value}")));
        foreach (string caso in CasosExigidos)
            Afirmar.Verdadeiro(contagens.Casos.GetValueOrDefault(caso) > 0, $"o gerador não exercitou \"{caso}\": a conferência correspondente ficou vazia");

        // R11: o atraso é sorteado na faixa do perfil, não fixado nela.
        Afirmar.Verdadeiro(contagens.AtrasosAcimaDoPiso.Count >= AtrasosDistintosMinimos,
            $"R11: só {contagens.AtrasosAcimaDoPiso.Count} atrasos distintos acima do piso; esperado ao menos {AtrasosDistintosMinimos}");

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
    private static void Conferir(ConfiguracaoDoNucleo cfg, EstadoDoNucleo antes, Evento evento, Resultado r, Func<string> onde, Contagens contagens)
    {
        void Contar(string caso) => contagens.Contar(caso);
        EstadoDoNucleo depois = r.Estado;
        IReadOnlyList<Efeito> efeitos = r.Efeitos;
        IReadOnlyList<Transicao> transicoes = r.Transicoes;
        PontoPx? ancoraAntes = antes.Lugar?.Ancora;
        PontoPx? ancoraDepois = depois.Lugar?.Ancora;
        bool mesmoEstadoEPosicao = antes.Estado == depois.Estado && ancoraAntes == ancoraDepois;

        // EXITING é final: nenhum evento muda o estado nem produz efeito.
        if (antes.Estado == Estado.Exiting)
            Verificar(transicoes.Count == 0 && efeitos.Count == 0 && depois == antes, () => $"EXITING: {onde()}: o evento teve efeito ({Descrever(transicoes)}; {efeitos.Count} efeitos)");

        // Invariante 1: em PRESSED e DRAGGING (SETTLING nunca sobra entre dois eventos), o relógio
        // e o movimento não mudam estado nem posição; um evento autônomo nem chega à máquina.
        if (antes.Estado is Estado.Pressed or Estado.Dragging && evento.Origem == Origem.Relogio)
        {
            Contar("TICK ou sinal de movimento com o usuário no controle");
            Verificar(mesmoEstadoEPosicao, () => $"invariante 1: {onde()}: {antes.Estado}→{depois.Estado}, {ancoraAntes}→{ancoraDepois}");
        }
        if (evento.Origem == Origem.Autonomo)
            Verificar(!antes.Estado.ControladoPeloUsuario(), () => $"invariante 1: {onde()}: evento autônomo chegou à máquina em {antes.Estado}; o núcleo devia descartá-lo");

        // Invariante 2: em DRAGGING, posição = cursor − pegada.
        if (depois.Estado == Estado.Dragging && evento is DragMove m)
            Verificar(ancoraDepois == new PontoPx(m.Cursor.X - depois.Pegada.X, m.Cursor.Y - depois.Pegada.Y), () => $"invariante 2: {onde()}: âncora {ancoraDepois}");

        // Invariante 5: depois de SETTLING, a âncora está na área útil de um monitor presente.
        if (transicoes.Any(t => t.De == Estado.Settling) && depois.Topologia is { } topologia && ancoraDepois is { } a)
            Verificar(NaAreaUtil(topologia, a), () => $"invariante 5: {onde()}: âncora {a} fora de toda área útil de {topologia}");

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
            Contar("expressão trocada sem transição");
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

        // Linhas de FULLSCREEN_TARGETS_CHANGED, com R12 e a marca de R9 (invariante 14 incluído).
        if (evento is FullscreenTargetsChanged f)
            ConferirTelaCheia(antes, f, r, onde, contagens);
        if (evento is DragEnd && antes.Estado == Estado.Dragging)
            Verificar(depois.RetornoDaTelaCheia is null, () => $"invariante 14: {onde()}: retorno temporário sobreviveu ao arraste");

        // Invariante 14 estendido (R-g, regras R7 e R9): cancelar o arraste e mostrar manualmente
        // também são escolha do usuário e descartam o retorno.
        if (evento is DragCancel && antes.Estado == Estado.Dragging)
        {
            if (antes.RetornoDaTelaCheia is not null) Contar("DRAG_CANCEL do arraste com retorno");
            Verificar(depois.RetornoDaTelaCheia is null, () => $"invariante 14: {onde()}: retorno temporário sobreviveu ao DRAG_CANCEL do arraste");
        }
        if (evento is CmdShow && antes.Estado is not (Estado.Booting or Estado.Exiting))
        {
            if (antes.RetornoDaTelaCheia is not null) Contar("CMD_SHOW com retorno");
            Verificar(depois.RetornoDaTelaCheia is null, () => $"invariante 14: {onde()}: retorno temporário sobreviveu ao CMD_SHOW em {antes.Estado}");
        }
        // Linhas HIDDEN(...) | CMD_SHOW: depois da carga, aparece na posição anterior validada — a de
        // antes da tela cheia, se foi ela que o escondeu; onde estava, se foi outro motivo — sem
        // reaplicar o modo (é escolha do usuário, DEC-020).
        if (evento is CmdShow && antes.Estado == Estado.Hidden && antes.Carregado && antes.Topologia is { } topologiaAoMostrar)
        {
            PosicaoDoPersonagem? anterior = antes.Motivo == MotivoDoOcultamento.PorTelaCheia ? antes.RetornoDaTelaCheia ?? antes.Posicao : antes.Posicao;
            if (antes.Motivo == MotivoDoOcultamento.PorTelaCheia && antes.RetornoDaTelaCheia is not null) Contar("CMD_SHOW escondido pela tela cheia, com retorno");
            if (antes.Motivo != MotivoDoOcultamento.PorTelaCheia && antes.RetornoDaTelaCheia is not null) Contar("CMD_SHOW escondido por outro motivo no meio de um episódio");
            Posicionamento esperado = anterior is null ? Posicionador.Inicial(topologiaAoMostrar, cfg.Tamanho) : Posicionador.Reacomodar(topologiaAoMostrar, anterior, cfg.Tamanho).Resultado;
            if (antes.Esconderijo != LadoDoEsconderijo.Nenhum)
            {
                // Escondido na borda (DEC-025): reaparece no esconderijo, na mesma borda do mesmo monitor.
                Contar("CMD_SHOW de quem estava escondido na borda");
                Verificar(depois.Estado == Estado.Peeking && depois.Esconderijo == antes.Esconderijo && depois.Lugar is { } escondido && escondido.Monitor.Chave == esperado.Monitor.Chave,
                    () => $"CMD_SHOW: {onde()}: escondido na borda {antes.Esconderijo} devia voltar ao esconderijo em {esperado.Monitor.Chave}; obtido {depois.Estado} ({depois.Esconderijo}) em {depois.Lugar?.Monitor.Chave}");
            }
            else
            {
                Verificar(depois.Estado.Visivel() && depois.Lugar is { } mostrado && mostrado.Monitor.Chave == esperado.Monitor.Chave && mostrado.Ancora.X == esperado.Ancora.X,
                    () => $"CMD_SHOW: {onde()}: de HIDDEN({antes.Motivo}) devia aparecer em {esperado.Monitor.Chave} {esperado.Ancora}; obtido {depois.Estado} em {depois.Lugar?.Monitor.Chave} {depois.Lugar?.Ancora}");
            }
        }

        // R9: um clique, clique duplo ou cancelamento em PRESSED só descarta o retorno se a tela cheia
        // mudou durante o gesto (o ponto do usuário vale); sem mudança, o retorno fica, ou, se o modo
        // foi desligado no meio do gesto, o personagem volta à posição anterior (linha SETTINGS_CHANGED
        // que desliga o modo). E um gesto novo começa sem a marca do anterior.
        if (antes.Estado == Estado.Pressed && evento is Click or DoubleClick or DragCancel && antes.RetornoDaTelaCheia is { } retornoNoGesto)
        {
            if (antes.TelaCheiaMudouNoGesto)
            {
                Contar("fim do clique com retorno e tela cheia mudada");
                Verificar(depois.RetornoDaTelaCheia is null, () => $"R9: {onde()}: a tela cheia mudou no gesto e o retorno sobreviveu");
            }
            else if (antes.Preferencias.ModoTelaCheia)
            {
                Contar("fim do clique com retorno, sem mudança");
                Verificar(Equals(depois.RetornoDaTelaCheia, retornoNoGesto), () => $"R9: {onde()}: sem mudança de tela cheia no gesto, o retorno sumiu");
            }
            else if (antes.Topologia is { } topologiaDoGesto)
            {
                Contar("fim do clique com retorno e o modo desligado no gesto");
                (Posicionamento anterior, _) = Posicionador.Reacomodar(topologiaDoGesto, retornoNoGesto, cfg.Tamanho);
                Verificar(depois.RetornoDaTelaCheia is null && depois.Lugar is { } lugar && lugar.Monitor.Chave == anterior.Monitor.Chave && lugar.Ancora.X == anterior.Ancora.X,
                    () => $"SETTINGS_CHANGED desligou o modo no gesto: {onde()}: o fim do clique devia levar à posição anterior {anterior.Monitor.Chave} {anterior.Ancora} sem retorno; obtido {depois.Lugar?.Monitor.Chave} {depois.Lugar?.Ancora}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
            }
        }
        if (evento is Press && transicoes.Any(t => t.Para == Estado.Pressed))
        {
            if (antes.TelaCheiaMudouNoGesto) Contar("PRESS com a marca de um gesto interrompido");
            Verificar(!depois.TelaCheiaMudouNoGesto, () => $"R9: {onde()}: o gesto novo começou com a marca de mudança de tela cheia de outro gesto");
        }

        // Invariante 15: gesto curto não muda estado nem posição e termina com prioridade maior.
        if (antes.Gesto != Gesto.Nenhum && depois.Gesto == Gesto.Nenhum && evento is Tick)
        {
            Contar("gesto terminou por TICK");
            Verificar(mesmoEstadoEPosicao, () => $"invariante 15: {onde()}: o fim do gesto mudou {antes.Estado}→{depois.Estado}");
        }
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

        // Agenda: um temporizador só nos estados que decidem, com a autonomia livre; e, nessas
        // condições, sempre um (a agenda não para sozinha, nem depois de um gesto curto).
        bool autonomiaLivre = Maquina.DecideNoEstado(depois.Estado) && !depois.AutonomiaPausada && !depois.PainelAberto && depois.Gesto == Gesto.Nenhum;
        Verificar(depois.DecisaoAgendada == autonomiaLivre,
            () => $"agenda: {onde()}: temporizador pendente={depois.DecisaoAgendada} em {depois.Estado} (pausada {depois.AutonomiaPausada}, painel {depois.PainelAberto}, gesto {depois.Gesto})");

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
            if (agenda.Atraso == piso && minimo < piso) Contar("agendamento no piso de um sorteio menor");
            if (agenda.Atraso > piso) contagens.AtrasosAcimaDoPiso.Add(agenda.Atraso);
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

        // R6 (R-e): antes da carga o personagem nunca aparece; a primeira carga vale, com a topologia,
        // as preferências e a posição dela; as seguintes são ignoradas.
        if (!depois.Carregado)
        {
            Contar("evento antes da carga");
            Verificar(!depois.Estado.Visivel(), () => $"R6: {onde()}: visível ({depois.Estado}) antes da carga");
        }
        if (evento is Loaded carga)
            ConferirCarga(cfg, antes, carga, r, onde, contagens);

        // R1 (R-e): a energia é sempre um dos três níveis; ENERGY_SELECTED fora deles é ignorado, sem
        // gravar; SETTINGS_CHANGED fora deles vira Média.
        Verificar(Enum.IsDefined(depois.Preferencias.Energia), () => $"R1: {onde()}: energia fora do enum: {(int)depois.Preferencias.Energia}");
        if (evento is EnergySelected { Nivel: var nivel } && !Enum.IsDefined(nivel))
        {
            if (antes.PainelAberto) Contar("ENERGY_SELECTED fora do enum com o painel aberto");
            Verificar(antes.Preferencias == depois.Preferencias && !efeitos.Any(e => e is GravarPreferencias), () => $"R1: {onde()}: ENERGY_SELECTED({(int)nivel}) não foi ignorado");
        }
        if (evento is SettingsChanged configuracao && antes.Estado != Estado.Exiting)
            ConferirConfiguracoes(cfg, antes, configuracao, r, onde, contagens);
        // DEC-020: com o modo desligado, o retorno temporário não sobra fora de um gesto.
        if (!depois.Preferencias.ModoTelaCheia && depois.Estado is not (Estado.Pressed or Estado.Dragging))
            Verificar(depois.RetornoDaTelaCheia is null, () => $"modo de tela cheia desligado: {onde()}: retorno {Gravacao.DescreverPosicao(depois.RetornoDaTelaCheia!)} sobrou em {depois.Estado}");

        // Painel (R-f). Invariante 4: com o painel aberto, AUTONOMY_TIMER não produz transição,
        // gesto nem expressão, e nenhuma transição entra num estado autônomo em movimento ou em
        // RESTING, exceto pelo próprio movimento (MovementSignal: a física em curso termina; o
        // comportamento calmo de CLIMBING/HANGING com o painel aberto é da Fase 4). O primeiro caso
        // nunca acha uma decisão pendente (abrir o painel cancela a agenda, conferido acima em
        // "agenda"): fica como rede de segurança, sem contar como cobertura.
        if (evento is AutonomyTimer && antes.PainelAberto && depois.PainelAberto)
        {
            Verificar(transicoes.Count == 0 && depois.Gesto == antes.Gesto && depois.Expressao == antes.Expressao,
                () => $"invariante 4: {onde()}: decisão autônoma com o painel aberto ({Descrever(transicoes)}, gesto {depois.Gesto}, expressão {depois.Expressao})");
        }
        if (antes.PainelAberto && depois.PainelAberto && evento is not MovementSignal)
            Verificar(!transicoes.Any(t => t.Para is Estado.Walking or Estado.Climbing or Estado.Hanging or Estado.Jumping or Estado.Resting),
                () => $"invariante 4: {onde()}: comportamento autônomo começou com o painel aberto ({Descrever(transicoes)})");
        // Invariante 8 em todo passo em que o painel abre ou fecha sem troca de estado.
        if (antes.PainelAberto != depois.PainelAberto && antes.Estado == depois.Estado)
        {
            Contar("painel abriu ou fechou sem troca de estado");
            Verificar(ancoraAntes == ancoraDepois, () => $"invariante 8: {onde()}: o painel {(depois.PainelAberto ? "abriu" : "fechou")} e a âncora foi de {ancoraAntes} a {ancoraDepois}");
        }
        // Invariante 9: iniciar o arraste com o painel aberto pede para fechá-lo.
        if (evento is DragStart && antes.PainelAberto && depois.Estado == Estado.Dragging)
        {
            Contar("DRAG_START com o painel aberto");
            Verificar(efeitos.Any(e => e is FecharPainelDeEnergia), () => $"invariante 9: {onde()}: arraste começou sem FecharPainelDeEnergia");
        }
        // Painel só existe com o personagem visível.
        if (!depois.Estado.Visivel())
            Verificar(!depois.PainelAberto, () => $"painel: {onde()}: aberto em {depois.Estado}");

        ConferirGravacaoAoEsconderOuSair(antes, evento, r, onde, contagens);
        ConferirGravacaoDaPosicao(evento, r, onde, contagens);
    }

    /// <summary>
    /// Invariante 18 (Fase 5): todo GravarPosicao traz uma posição gravável (chave não vazia, frações finitas
    /// e a tela do monitor da época, que toda posição descrita ou validada pela máquina tem) e só sai de
    /// evento do usuário ou do sistema, nunca do relógio, do movimento, da agenda autônoma nem da troca de
    /// expressão: não há gravação periódica (DEC-011). E a posição sobrevive ao settings.json: escrita e lida
    /// pelo esquema, volta normalizada, sem aviso e ainda com posição.
    /// </summary>
    private static void ConferirGravacaoDaPosicao(Evento evento, Resultado r, Func<string> onde, Contagens contagens)
    {
        foreach (GravarPosicao g in r.Efeitos.OfType<GravarPosicao>())
        {
            contagens.Contar("GRAVAR_POSICAO conferida (invariante 18)");
            PosicaoDoPersonagem p = g.Posicao;
            Verificar(evento.Origem >= Origem.Sistema,
                () => $"invariante 18: {onde()}: GravarPosicao saiu de {evento.GetType().Name}, de origem {evento.Origem}");
            Verificar(!string.IsNullOrEmpty(p.ChaveMonitor) && double.IsFinite(p.FracaoX) && double.IsFinite(p.FracaoY) && p.TelaDoMonitor is { Vazio: false },
                () => $"invariante 18: {onde()}: posição não gravável {Gravacao.DescreverPosicaoCompleta(p)}");

            var salvas = new ConfiguracoesSalvas(p, r.Estado.Preferencias);
            LeituraDasConfiguracoes lida = EsquemaDeConfiguracoes.Ler(EsquemaDeConfiguracoes.Escrever(salvas));
            Verificar(lida.Situacao == SituacaoDaLeitura.Valida && lida.Avisos.Count == 0 && lida.Configuracoes.Posicao is not null
                    && lida.Configuracoes == EsquemaDeConfiguracoes.Normalizar(salvas),
                () => $"invariante 18: {onde()}: {Gravacao.DescreverPosicaoCompleta(p)} não voltou do settings.json ({lida.Situacao}, {Descrever(lida.Configuracoes.Posicao)}, avisos: {string.Join(" | ", lida.Avisos)})");
        }
    }

    /// <summary>
    /// Linhas de FULLSCREEN_TARGETS_CHANGED (ARCHITECTURE.md 2.6, DEC-013 e DEC-020), com o modo e o
    /// cache de antes do evento: "uma vez por mudança" (R12) e, com o modo desligado, só o cache;
    /// em PRESSED e DRAGGING, só o cache e a marca de R9 (invariante 14); visível, age só se a âncora
    /// estiver num monitor ocupado e transfere para o livre mais próximo dela, ou esconde se não houver
    /// livre, guardando a posição anterior se ainda não houver uma; HIDDEN(POR_TELA_CHEIA) reaparece no
    /// livre mais próximo do retorno; escondido por outro motivo, só o conjunto vazio age (R4). Uma
    /// chave desconhecida conta no conjunto, mas nunca como monitor.
    /// </summary>
    private static void ConferirTelaCheia(EstadoDoNucleo antes, FullscreenTargetsChanged f, Resultado r, Func<string> onde, Contagens contagens)
    {
        if (antes.Estado == Estado.Exiting) return;
        EstadoDoNucleo depois = r.Estado;
        MonitoresOcupados ocupados = f.Ocupados;
        Topologia? topologia = antes.Topologia;
        PosicaoDoPersonagem? retorno = antes.RetornoDaTelaCheia;
        Verificar(depois.Ocupados.Equals(ocupados), () => $"tela cheia: {onde()}: cache {depois.Ocupados}, esperado {ocupados}");
        if (topologia is not null && ocupados.Chaves.Any(ch => topologia.PorChave(ch) is null)) contagens.Contar("tela cheia com chave desconhecida");

        // Nada além do cache (e do fim de um gesto curto, invariante 15) muda.
        void SoOCache(string regra, bool marca)
        {
            bool efeitosDoGesto = antes.Gesto != Gesto.Nenhum
                ? r.Efeitos.All(e => e is DesligarRelogio or AgendarDecisao)
                : r.Efeitos.Count == 0;
            Verificar(r.Transicoes.Count == 0 && depois.Estado == antes.Estado && depois.Motivo == antes.Motivo
                    && Equals(depois.Lugar, antes.Lugar) && Equals(depois.Posicao, antes.Posicao) && Equals(depois.RetornoDaTelaCheia, retorno)
                    && depois.PainelAberto == antes.PainelAberto && depois.TelaCheiaMudouNoGesto == marca && efeitosDoGesto,
                () => $"tela cheia ({regra}): {onde()}: devia só atualizar o cache, mas {antes.Estado}/{antes.Motivo}→{depois.Estado}/{depois.Motivo}, "
                    + $"{antes.Lugar?.Ancora}→{depois.Lugar?.Ancora}, retorno {Descrever(retorno)}→{Descrever(depois.RetornoDaTelaCheia)}, "
                    + $"marca {antes.TelaCheiaMudouNoGesto}→{depois.TelaCheiaMudouNoGesto}, {Descrever(r.Transicoes)}, efeitos [{string.Join(", ", r.Efeitos.Select(e => e.GetType().Name))}]");
        }

        if (ocupados.Equals(antes.Ocupados))
        {
            contagens.Contar("tela cheia repetida");
            SoOCache("R12: o mesmo conjunto não age", antes.TelaCheiaMudouNoGesto);
            return;
        }
        if (!antes.Preferencias.ModoTelaCheia)
        {
            contagens.Contar("tela cheia com o modo desligado");
            SoOCache("modo desligado", antes.TelaCheiaMudouNoGesto);
            return;
        }
        if (topologia is null || antes.Estado == Estado.Booting)
        {
            SoOCache("antes da carga", antes.TelaCheiaMudouNoGesto);
            return;
        }
        if (antes.Estado is Estado.Pressed or Estado.Dragging)
        {
            contagens.Contar("tela cheia durante o gesto");
            SoOCache("PRESSED, DRAGGING: o gesto não é interrompido", marca: true);
            return;
        }

        MonitorDoDesktop[] livres = [.. topologia.Monitores.Where(mon => !ocupados.Contem(mon.Chave))];
        // O livre escolhido é o mais próximo da referência (empates valem qualquer um deles).
        void NoLivreMaisProximo(string regra, PontoPx referencia)
        {
            long Distancia(MonitorDoDesktop mon) => mon.Tela.DistanciaAoQuadrado(referencia);
            MonitorDoDesktop? destino = depois.Lugar is { } lugar ? livres.FirstOrDefault(mon => mon.Chave == lugar.Monitor.Chave) : null;
            Verificar(depois.Estado.Visivel() && destino is not null && Distancia(destino) == livres.Min(Distancia),
                () => $"tela cheia ({regra}): {onde()}: foi para {depois.Estado} em {depois.Lugar?.Monitor.Chave}; livres {string.Join(",", livres.Select(l => $"{l.Chave}@{Distancia(l)}"))} a partir de {referencia}");
        }

        if (antes.Estado == Estado.Hidden)
        {
            if (antes.Motivo != MotivoDoOcultamento.PorTelaCheia)
            {
                if (ocupados.Vazio && retorno is not null)
                {
                    // R4: o episódio acabou com o personagem escondido por outro motivo.
                    contagens.Contar("fim da tela cheia escondido por outro motivo, com retorno");
                    Verificar(r.Transicoes.Count == 0 && r.Efeitos.Count == 0 && depois.Estado == Estado.Hidden && depois.Motivo == antes.Motivo
                            && Equals(depois.Posicao, retorno) && depois.RetornoDaTelaCheia is null,
                        () => $"R4: {onde()}: esperado continuar {antes.Motivo} com a posição de antes e sem retorno; obtido {depois.Estado}/{depois.Motivo}, posição {Descrever(depois.Posicao)}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
                    return;
                }
                if (retorno is not null) contagens.Contar("tela cheia não vazia escondido por outro motivo, com retorno");
                SoOCache("escondido por outro motivo", antes.TelaCheiaMudouNoGesto);
                return;
            }
            if (ocupados.Vazio)
            {
                contagens.Contar("tela cheia vazia restaura a posição anterior");
                Verificar(depois.Estado.Visivel() && depois.RetornoDaTelaCheia is null, () => $"tela cheia vazia: {onde()}: HIDDEN(POR_TELA_CHEIA) devia reaparecer sem retorno; obtido {depois.Estado}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
                return;
            }
            if (livres.Length == 0)
            {
                SoOCache("HIDDEN(POR_TELA_CHEIA) sem monitor livre", antes.TelaCheiaMudouNoGesto);
                return;
            }
            PosicaoDoPersonagem referencia = retorno ?? antes.Posicao!;
            if (retorno is not null && livres.Length > 1) contagens.Contar("HIDDEN(POR_TELA_CHEIA) reaparece no livre mais próximo do retorno");
            NoLivreMaisProximo("HIDDEN(POR_TELA_CHEIA) com monitor livre: reaparece nele", referencia.AncoraAbsoluta);
            Verificar(Equals(depois.RetornoDaTelaCheia, retorno), () => $"tela cheia: {onde()}: HIDDEN(POR_TELA_CHEIA) reapareceu sem manter o retorno");
            return;
        }

        // Visível, fora de um gesto.
        if (ocupados.Vazio)
        {
            if (retorno is null)
            {
                contagens.Contar("fim da tela cheia sem retorno, visível");
                SoOCache("invariante 14: fim da tela cheia sem retorno", antes.TelaCheiaMudouNoGesto);
                return;
            }
            contagens.Contar("tela cheia vazia restaura a posição anterior");
            Verificar(depois.Estado.Visivel() && depois.RetornoDaTelaCheia is null && r.Transicoes.Any(t => t.Para == Estado.Settling),
                () => $"tela cheia vazia: {onde()}: devia restaurar a posição anterior e limpar o retorno; obtido {depois.Estado}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
            return;
        }
        PontoPx ancora = antes.Lugar!.Ancora;
        if (!ocupados.Contem(antes.Lugar.Monitor.Chave))
        {
            SoOCache("a âncora está num monitor livre", antes.TelaCheiaMudouNoGesto);
            return;
        }
        PosicaoDoPersonagem guardada = retorno ?? antes.Posicao!;
        if (livres.Length == 0)
        {
            contagens.Contar("tela cheia esconde sem monitor livre");
            Verificar(depois.Estado == Estado.Hidden && depois.Motivo == MotivoDoOcultamento.PorTelaCheia && !depois.PainelAberto && Equals(depois.RetornoDaTelaCheia, guardada),
                () => $"tela cheia sem monitor livre: {onde()}: esperado HIDDEN(POR_TELA_CHEIA), painel fechado e retorno {Descrever(guardada)}; obtido {depois.Estado}/{depois.Motivo}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
            return;
        }
        if (livres.Length > 1) contagens.Contar("tela cheia transfere com mais de um monitor livre");
        NoLivreMaisProximo("transfere para o monitor livre", ancora);
        Verificar(Equals(depois.RetornoDaTelaCheia, guardada), () => $"tela cheia: {onde()}: transferido com retorno {Descrever(depois.RetornoDaTelaCheia)}, esperado {Descrever(guardada)} (\"se ainda não houver uma guardada\")");
    }

    /// <summary>
    /// Linha BOOTING | configurações e topologia carregadas (R6): a primeira carga vale, com a topologia
    /// e as preferências dela (energia saneada, R1); um pedido de esconder anterior continua valendo,
    /// sem mostrar nem agendar; sem ele, o personagem aparece (ou a tela cheia em cache o esconde). A
    /// posição salva é restaurada pela cascata da partida (ARCHITECTURE.md 2.8, Posicionador.Restaurar):
    /// o monitor da chave; sem ele, o primeiro com a tela salva; sem nenhum dos dois, o principal. A
    /// posição relativa salva é aplicada à área útil desse monitor, e a posição do núcleo passa a ser
    /// dele, com a tela dele e frações válidas. Sem posição salva, começa no principal. As cargas
    /// seguintes são ignoradas.
    /// </summary>
    private static void ConferirCarga(ConfiguracaoDoNucleo cfg, EstadoDoNucleo antes, Loaded carga, Resultado r, Func<string> onde, Contagens contagens)
    {
        EstadoDoNucleo depois = r.Estado;
        if (antes.Carregado || antes.Estado == Estado.Exiting)
        {
            contagens.Contar("carga repetida");
            Verificar(r.Transicoes.Count == 0 && antes.Estado == depois.Estado && antes.Motivo == depois.Motivo
                    && Equals(antes.Lugar, depois.Lugar) && Equals(antes.Posicao, depois.Posicao)
                    && antes.Preferencias == depois.Preferencias && ReferenceEquals(antes.Topologia, depois.Topologia),
                () => $"R6: {onde()}: carga repetida não foi ignorada ({antes.Estado}→{depois.Estado}, {Descrever(r.Transicoes)})");
            return;
        }

        if (antes.Estado == Estado.Hidden) contagens.Contar("carga depois de pedidos de esconder");
        if (!carga.Preferencias.AtravessarMonitores) contagens.Contar("carga com a travessia desligada");
        if (carga.PosicaoSalva is { } salvaDesconhecida && carga.Topologia.PorChave(salvaDesconhecida.ChaveMonitor) is null) contagens.Contar("carga com posição salva em monitor inexistente");
        if (!Enum.IsDefined(carga.Preferencias.Energia)) contagens.Contar("carga fora do enum");
        Verificar(depois.Carregado && ReferenceEquals(depois.Topologia, carga.Topologia) && depois.Preferencias == Saneadas(carga.Preferencias),
            () => $"R6: {onde()}: a primeira carga não valeu (carregado {depois.Carregado}, preferências {depois.Preferencias})");

        if (antes.Estado == Estado.Hidden && antes.Motivo != MotivoDoOcultamento.Nenhum)
        {
            contagens.Contar("carga que mantém um pedido de esconder");
            Verificar(r.Transicoes.Count == 0 && r.Efeitos.Count == 0 && depois.Estado == Estado.Hidden && depois.Motivo == antes.Motivo,
                () => $"R6: {onde()}: o pedido de esconder anterior à carga ({antes.Motivo}) não continuou valendo: {depois.Estado}/{depois.Motivo}, {Descrever(r.Transicoes)}, {r.Efeitos.Count} efeitos");
        }
        else
        {
            contagens.Contar("carga que mostra o personagem");
            Verificar(r.Transicoes.Count > 0 && r.Transicoes[0].Para == Estado.Settling
                    && (depois.Estado.Visivel() || (depois.Estado == Estado.Hidden && depois.Motivo == MotivoDoOcultamento.PorTelaCheia)),
                () => $"R6: {onde()}: a carga de {antes.Estado}/{antes.Motivo} não mostrou o personagem: {depois.Estado}/{depois.Motivo}, {Descrever(r.Transicoes)}");
        }

        // A tela cheia em cache não agiu (ela sempre guarda um retorno quando age): a posição é a da carga.
        if (depois.RetornoDaTelaCheia is not null) return;
        Posicionamento lugar = Afirmar.NaoNulo(depois.Lugar, $"R6: {onde()}: lugar depois da carga");
        if (carga.PosicaoSalva is { } salva)
        {
            contagens.Contar("carga com posição salva conferida");
            MonitorDoDesktop? daChave = carga.Topologia.PorChave(salva.ChaveMonitor);
            MonitorDoDesktop? daTela = daChave is null ? carga.Topologia.Monitores.FirstOrDefault(m => m.Tela == salva.TelaDoMonitor) : null;
            if (daTela is not null) contagens.Contar("carga restaurada pelo retângulo do monitor");
            else if (daChave is null) contagens.Contar("carga restaurada no monitor principal");
            MonitorDoDesktop esperado = daChave ?? daTela ?? carga.Topologia.Principal;
            Verificar(lugar.Monitor.Chave == esperado.Chave,
                () => $"R6: {onde()}: posição salva em {salva.ChaveMonitor} (tela {salva.TelaDoMonitor?.ToString() ?? "desconhecida"}) restaurada em {lugar.Monitor.Chave}, esperado {esperado.Chave}");
            int x = Posicionador.NoMonitor(esperado, salva.FracaoX, salva.FracaoY, cfg.Tamanho).Ancora.X;
            Verificar(lugar.Ancora.X == x, () => $"R6: {onde()}: âncora {lugar.Ancora} em {lugar.Monitor.Chave} não é a fração salva {salva.FracaoX} (x {x})");
            // A posição do núcleo passa a ser do monitor escolhido: a chave e a tela dele, e frações
            // válidas mesmo quando as salvas eram NaN, infinitas ou fora de [0, 1].
            Verificar(depois.Posicao is { } p && p.ChaveMonitor == esperado.Chave && p.TelaDoMonitor == esperado.Tela
                    && p.FracaoX is >= 0 and <= 1 && p.FracaoY is >= 0 and <= 1,
                () => $"R6: {onde()}: posição do núcleo {Descrever(depois.Posicao)} (tela {depois.Posicao?.TelaDoMonitor}) depois de restaurar em {esperado.Chave} (tela {esperado.Tela})");
        }
        else
        {
            Posicionamento inicial = Posicionador.Inicial(carga.Topologia, cfg.Tamanho);
            Verificar(lugar.Monitor.Chave == inicial.Monitor.Chave && lugar.Ancora.X == inicial.Ancora.X, () => $"R6: {onde()}: sem posição salva, começou em {lugar.Monitor.Chave} {lugar.Ancora}, não na posição inicial {inicial.Ancora}");
        }
    }

    /// <summary>
    /// SETTINGS_CHANGED: as preferências passam a ser as recebidas, com o nível fora do enum trocado
    /// por Média (R1). Desligar o modo com o retorno guardado desfaz o efeito temporário (linha
    /// "qualquer, com retorno temporário guardado | SETTINGS_CHANGED que desliga o modo"): visível ou
    /// escondido pela tela cheia, volta à posição anterior validada; escondido por outro motivo, a
    /// posição anterior volta a valer sem reaparecer; em PRESSED e DRAGGING o gesto não é
    /// interrompido e o retorno fica para o fim dele (conferido na regra R9 acima).
    /// </summary>
    private static void ConferirConfiguracoes(ConfiguracaoDoNucleo cfg, EstadoDoNucleo antes, SettingsChanged configuracao, Resultado r, Func<string> onde, Contagens contagens)
    {
        EstadoDoNucleo depois = r.Estado;
        if (!Enum.IsDefined(configuracao.Preferencias.Energia)) contagens.Contar("SETTINGS_CHANGED fora do enum");
        if (!configuracao.Preferencias.AtravessarMonitores) contagens.Contar("SETTINGS_CHANGED com a travessia desligada");
        Verificar(depois.Preferencias == Saneadas(configuracao.Preferencias),
            () => $"R1: {onde()}: preferências {depois.Preferencias}, esperado {Saneadas(configuracao.Preferencias)}");

        bool desliga = antes.Preferencias.ModoTelaCheia && !configuracao.Preferencias.ModoTelaCheia;
        if (!desliga || antes.RetornoDaTelaCheia is not { } retorno || antes.Topologia is null) return;
        if (antes.Estado is Estado.Pressed or Estado.Dragging)
        {
            contagens.Contar("desligar o modo com retorno no meio do gesto");
            Verificar(depois.Estado == antes.Estado && Equals(depois.RetornoDaTelaCheia, retorno) && Equals(depois.Lugar, antes.Lugar),
                () => $"SETTINGS_CHANGED desliga o modo no gesto: {onde()}: o gesto devia seguir intacto com o retorno {Descrever(retorno)}; obtido {depois.Estado}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
            return;
        }
        if (antes.Estado == Estado.Hidden && antes.Motivo != MotivoDoOcultamento.PorTelaCheia)
        {
            contagens.Contar("desligar o modo com retorno, escondido por outro motivo");
            Verificar(depois.Estado == Estado.Hidden && depois.Motivo == antes.Motivo && depois.RetornoDaTelaCheia is null && Equals(depois.Posicao, retorno),
                () => $"SETTINGS_CHANGED desliga o modo escondido por {antes.Motivo}: {onde()}: esperado continuar escondido com a posição {Descrever(retorno)} e sem retorno; obtido {depois.Estado}({depois.Motivo}), posição {Descrever(depois.Posicao)}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
            return;
        }
        bool voltaAPosicaoAnterior = antes.Estado.Visivel() || (antes.Estado == Estado.Hidden && antes.Motivo == MotivoDoOcultamento.PorTelaCheia);
        if (!voltaAPosicaoAnterior) return;

        contagens.Contar("desligar o modo com retorno, visível ou escondido pela tela cheia");
        (Posicionamento anterior, _) = Posicionador.Reacomodar(antes.Topologia, retorno, cfg.Tamanho);
        Verificar(depois.Estado.Visivel() && depois.RetornoDaTelaCheia is null && depois.Lugar is { } lugar
                && lugar.Monitor.Chave == anterior.Monitor.Chave && lugar.Ancora.X == anterior.Ancora.X,
            () => $"SETTINGS_CHANGED desliga o modo: {onde()}: esperado voltar a {anterior.Monitor.Chave} {anterior.Ancora} sem retorno; obtido {depois.Estado} em {depois.Lugar?.Monitor.Chave} {depois.Lugar?.Ancora}, retorno {Descrever(depois.RetornoDaTelaCheia)}");
    }

    /// <summary>
    /// R8 (R-i) e linhas de CMD_HIDE, SESSION_LOCKED, SUSPENDING, CMD_EXIT e SESSION_ENDING: esconder
    /// um personagem ainda não escondido, ou sair, grava exatamente uma vez a posição escolhida pelo
    /// usuário (o retorno, se houver; nenhuma, se ainda não há posição). No meio do arraste, o gesto
    /// termina onde está, como um DRAG_CANCEL (ARCHITECTURE.md 2.7), e "um arraste sempre descarta o
    /// retorno" (R9, DEC-020): grava o ponto validado. Esconder fora do arraste guarda o retorno para o
    /// fim da tela cheia (R4); já escondido, nada é gravado.
    /// </summary>
    private static void ConferirGravacaoAoEsconderOuSair(EstadoDoNucleo antes, Evento evento, Resultado r, Func<string> onde, Contagens contagens)
    {
        bool esconde = evento is CmdHide or SessionLocked or Suspending;
        bool sai = evento is CmdExit or SessionEnding;
        if (antes.Estado == Estado.Exiting || !(esconde || sai)) return;
        EstadoDoNucleo depois = r.Estado;
        GravarPosicao[] gravadas = [.. r.Efeitos.OfType<GravarPosicao>()];
        if (esconde && antes.Estado == Estado.Hidden)
        {
            Verificar(gravadas.Length == 0, () => $"R8: {onde()}: já escondido, gravou {gravadas.Length} posição(ões)");
            return;
        }

        bool doArraste = antes.Estado == Estado.Dragging;
        PosicaoDoPersonagem? esperada = doArraste ? depois.Posicao : antes.RetornoDaTelaCheia ?? antes.Posicao;
        if (antes.RetornoDaTelaCheia is not null)
            contagens.Contar(doArraste ? "esconder ou sair no meio do arraste com retorno" : "esconder ou sair com retorno guardado");
        Verificar(gravadas.Length == (esperada is null ? 0 : 1) && (esperada is null || Equals(gravadas[0].Posicao, esperada)),
            () => $"R8: {onde()}: esperado gravar {Descrever(esperada)} uma vez; gravou [{string.Join("; ", gravadas.Select(g => Gravacao.DescreverPosicao(g.Posicao)))}] (retorno antes {Descrever(antes.RetornoDaTelaCheia)}, de {antes.Estado})");
        if (doArraste)
        {
            Verificar(depois.RetornoDaTelaCheia is null, () => $"R9: {onde()}: o arraste interrompido não descartou o retorno {Descrever(depois.RetornoDaTelaCheia)}");
            Verificar(depois.Topologia is { } topologia && depois.Lugar is { } lugar && NaAreaUtil(topologia, lugar.Ancora),
                () => $"R8: {onde()}: o arraste interrompido não foi validado: âncora {depois.Lugar?.Ancora}");
        }
        else if (esconde && antes.Preferencias.ModoTelaCheia)
        {
            // Com o modo desligado, o retorno não sobra fora do gesto (conferido em todo passo).
            Verificar(Equals(depois.RetornoDaTelaCheia, antes.RetornoDaTelaCheia), () => $"R4: {onde()}: esconder de {antes.Estado} trocou o retorno {Descrever(antes.RetornoDaTelaCheia)} por {Descrever(depois.RetornoDaTelaCheia)}");
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
            // Esconderijo pelo clique duplo (DEC-025) em um terço das sequências, escolhido pela
            // semente da sequência para não mudar os outros sorteios do gerador.
            EsconderijoNoCliqueDuplo = (uint)semente % 3 == 0,
        };
        // R-h: perfil com decisões, descansos e gestos curtos e piso variável, para o piso do
        // intervalo de acomodação importar (com o perfil padrão, todo sorteio já passa de 3 s) e o
        // gesto terminar pelo relógio antes de outro evento o interromper.
        bool perfilCurto = rnd.Next(3) == 0;
        if (perfilCurto)
            cfg = cfg with { Perfil = PerfilCurto, IntervaloDeAcomodacao = TimeSpan.FromMilliseconds(rnd.Next(300, 5001)) };
        bool emLotes = rnd.Next(3) == 0;
        bool antesDaCarga = rnd.Next(4) == 0;
        Topologia topologia = gerador.NovaTopologia();
        // A travessia das preferências (Preferencias.AtravessarMonitores) sai de um gerador próprio, com
        // semente derivada da semente da sequência: variar o campo não consome sorteios do gerador principal,
        // e os outros sorteios (e as contagens dos casos) não mudam.
        var travessia = new Random(unchecked(semente * 31 + 7));

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
                Entregar([SortearAntesDaCarga(rnd, travessia, gerador, sombra.Estado, ref topologia)]);
        }
        Entregar([NovaCarga(rnd, travessia, gerador, topologia)]);

        while (total < EventosPorSequencia)
        {
            int tamanho = emLotes ? rnd.Next(1, 5) : 1;
            var lote = new List<Evento>(tamanho);
            for (int j = 0; j < tamanho; j++) lote.Add(Sortear(rnd, travessia, gerador, sombra.Estado, ref topologia));
            Entregar(lote);
        }
        return new Sequencia(cfg, (ulong)semente, lotes, emLotes, antesDaCarga, perfilCurto);
    }

    /// <summary>
    /// Pedidos que podem chegar antes da carga (R6): esconder, mostrar, sessão, suspensão,
    /// topologia, tela cheia, configurações, e às vezes qualquer outro evento.
    /// </summary>
    private static Evento SortearAntesDaCarga(Random rnd, Random travessia, GeradorDeTopologias gerador, EstadoDoNucleo s, ref Topologia topologia) => rnd.Next(12) switch
    {
        0 => new CmdHide(),
        1 => new CmdShow(),
        2 => new SessionLocked(),
        3 => new SessionUnlocked(),
        4 => new Suspending(),
        5 => new Resumed(),
        6 => NovaTopologia(rnd, gerador, ref topologia),
        7 => TelaCheia(rnd, topologia),
        8 => new SettingsChanged(new Preferencias(Nivel(rnd), rnd.Next(3) != 0, Atravessar(travessia))),
        9 => new CmdResetPosition(),
        10 => rnd.Next(2) == 0 ? new CmdPauseAutonomy() : new CmdResumeAutonomy(),
        _ => Sortear(rnd, travessia, gerador, s, ref topologia),
    };

    /// <summary>
    /// Carga com preferências às vezes inválidas e, metade das vezes, uma posição salva: num monitor
    /// da topologia ou numa chave que ela não tem, com frações às vezes fora de [0, 1] ou NaN. A tela
    /// salva é a do monitor em que a âncora salva está, se ela está em algum; senão, desconhecida. Ela
    /// sai da âncora já sorteada, sem sorteio novo, para não mudar os outros sorteios do gerador: com a
    /// chave desconhecida, leva a restauração pelo retângulo ou, sem ela, ao principal.
    /// </summary>
    private static Loaded NovaCarga(Random rnd, Random travessia, GeradorDeTopologias gerador, Topologia topologia)
    {
        PosicaoDoPersonagem? salva = null;
        if (rnd.Next(2) == 0)
        {
            string chave = rnd.Next(4) == 0 ? ChaveDesconhecida : topologia.Monitores[rnd.Next(topologia.Monitores.Count)].Chave;
            double Fracao() => rnd.Next(5) == 0 ? gerador.Fracao() : rnd.NextDouble();
            double fx = Fracao(), fy = Fracao();
            PontoPx ancora = gerador.Ponto(topologia);
            salva = new PosicaoDoPersonagem(chave, fx, fy, ancora) { TelaDoMonitor = topologia.MonitorQueContem(Posicionador.PixelDosPes(ancora))?.Tela };
        }
        return new Loaded(topologia, salva, new Preferencias(Nivel(rnd), rnd.Next(4) != 0, Atravessar(travessia)));
    }

    private static Evento Sortear(Random rnd, Random travessia, GeradorDeTopologias gerador, EstadoDoNucleo s, ref Topologia topologia)
    {
        PontoPx ancora = s.Lugar?.Ancora ?? new PontoPx(0, 0);
        Topologia atual = topologia;
        PontoPx NoCorpo() => new(ancora.X + rnd.Next(-20, 21), ancora.Y - rnd.Next(5, 60));
        PontoPx Qualquer() => gerador.Ponto(atual);

        // Durante um gesto curto o relógio corre a 60 Hz: metade das vezes, o próximo evento é um
        // TICK, para o gesto também terminar pelo relógio, e não só interrompido.
        if (s.Gesto != Gesto.Nenhum && rnd.Next(2) == 0) return new Tick();

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
            < 71 => new SettingsChanged(new Preferencias(Nivel(rnd), rnd.Next(3) != 0, Atravessar(travessia))),
            < 78 => new MovementSignal(SinalCoerente(rnd, s.Estado)),
            < 90 => new AutonomyTimer(rnd.Next(10) == 0 ? s.Geracao - 1 : s.Geracao),
            // R-e: carga repetida no meio da sequência, com outra topologia; deve ser ignorada.
            < 91 => NovaCarga(rnd, travessia, gerador, rnd.Next(2) == 0 ? atual : gerador.NovaTopologia()),
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

    /// <summary>Travessia entre monitores nas preferências: desligada uma vez em quatro, pelo gerador próprio dela.</summary>
    private static bool Atravessar(Random travessia) => travessia.Next(4) != 0;

    /// <summary>Nível de energia; uma vez em dez, fora do enum (3 a 99), como um arquivo adulterado (SECURITY.md 7).</summary>
    private static NivelDeEnergia Nivel(Random rnd) => rnd.Next(10) == 0 ? (NivelDeEnergia)rnd.Next(3, 100) : (NivelDeEnergia)rnd.Next(3);

    private static TopologyChanged NovaTopologia(Random rnd, GeradorDeTopologias gerador, ref Topologia topologia)
    {
        topologia = rnd.Next(3) == 0 ? gerador.NovaTopologia() : gerador.Mudar(topologia);
        return new TopologyChanged(topologia);
    }

    /// <summary>Perfis com decisões de 100 a 900 ms, descansos de 200 a 1500 ms e gestos de 1 a 5 passos.</summary>
    private static PerfilDeEnergia PerfilCurto(NivelDeEnergia nivel) => PerfilDeEnergia.Padrao(nivel) with
    {
        DecisaoMinima = TimeSpan.FromMilliseconds(100),
        DecisaoMaxima = TimeSpan.FromMilliseconds(900),
        DescansoMinimo = TimeSpan.FromMilliseconds(200),
        DescansoMaximo = TimeSpan.FromMilliseconds(1500),
        PassosDoGestoMinimo = 1,
        PassosDoGestoMaximo = 5,
    };

    /// <summary>As preferências como a carga deve guardá-las: nível fora do enum vira Média (R1).</summary>
    private static Preferencias Saneadas(Preferencias p) => Enum.IsDefined(p.Energia) ? p : p with { Energia = NivelDeEnergia.Media };

    private static TimeSpan Maior(TimeSpan a, TimeSpan b) => a > b ? a : b;

    /// <summary>Se a âncora está na área útil de algum monitor: dentro na horizontal, com os pés entre o topo (exclusivo) e o chão.</summary>
    private static bool NaAreaUtil(Topologia topologia, PontoPx a)
        => topologia.Monitores.Any(mon => a.X >= mon.AreaUtil.Esquerda && a.X < mon.AreaUtil.Direita && a.Y > mon.AreaUtil.Topo && a.Y <= mon.AreaUtil.Base);

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

    private static string Descrever(PosicaoDoPersonagem? p) => p is null ? "nenhuma" : Gravacao.DescreverPosicao(p);

    private static void Verificar(bool condicao, Func<string> mensagem)
    {
        if (!condicao) Afirmar.Falhar(mensagem());
    }
}

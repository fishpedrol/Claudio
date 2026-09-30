namespace Buzzy.Core.Personagem;

/// <summary>Resultado de aplicar um evento: o novo estado, os efeitos em ordem e as transições percorridas.</summary>
public sealed record Resultado(EstadoDoNucleo Estado, IReadOnlyList<Efeito> Efeitos, IReadOnlyList<Transicao> Transicoes);

/// <summary>
/// Máquina de estados do personagem (ARCHITECTURE.md 2.6): função pura de (estado, evento) para
/// (estado novo, efeitos). Não acessa relógio, arquivo nem Windows; o tempo só avança por
/// <see cref="Tick"/> e pelo disparo do <see cref="AutonomyTimer"/> que ela mesma agendou.
/// </summary>
public static class Maquina
{
    public static Resultado Aplicar(EstadoDoNucleo estado, Evento evento, ConfiguracaoDoNucleo config)
    {
        ArgumentNullException.ThrowIfNull(estado);
        ArgumentNullException.ThrowIfNull(evento);
        ArgumentNullException.ThrowIfNull(config);
        var passo = new Passo(estado, config);
        passo.Tratar(evento);
        return passo.Concluir();
    }

    /// <summary>
    /// Monitor em que está a âncora: o que contém o pixel dos pés, logo acima dela
    /// (<see cref="Posicionador.PixelDosPes"/>). A âncora fica na borda inferior exclusiva do sprite,
    /// que numa pilha de monitores já é o primeiro pixel do monitor de baixo.
    /// </summary>
    public static MonitorDoDesktop MonitorDaAncora(Topologia topologia, PontoPx ancora)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        return topologia.MonitorMaisProximo(Posicionador.PixelDosPes(ancora));
    }

    /// <summary>Estados em que a agenda autônoma mantém um temporizador pendente.</summary>
    public static bool DecideNoEstado(Estado estado)
        => estado is Estado.Idle or Estado.Resting or Estado.Climbing or Estado.Hanging or Estado.Peeking;

    private sealed class Passo
    {
        private readonly EstadoDoNucleo _inicio;
        private readonly ConfiguracaoDoNucleo _cfg;
        private readonly List<Efeito> _antes = [];
        private readonly List<Efeito> _depois = [];
        private readonly List<Transicao> _transicoes = [];
        private EstadoDoNucleo _s;
        private bool _reagendar;

        internal Passo(EstadoDoNucleo estado, ConfiguracaoDoNucleo config)
        {
            _inicio = estado;
            _cfg = config;
            _s = estado with { Sinal = Sinal.Nenhum };
        }

        private PerfilDeEnergia Perfil => _cfg.Perfil(_s.Preferencias.Energia);

        internal void Tratar(Evento evento)
        {
            if (_s.Estado == Estado.Exiting) return;

            // Invariante 15: um gesto curto termina ao chegar qualquer evento de prioridade maior
            // que a do relógio (PRESS, CMD_*, painel, sistema).
            if (evento.Origem >= Origem.Sistema && _s.Gesto != Gesto.Nenhum)
                EncerrarGesto();

            switch (evento)
            {
                case Loaded e: Carregar(e); break;
                case Press e: Pressionar(e.Cursor); break;
                case Click: Clicar(); break;
                case DoubleClick: CliqueDuplo(); break;
                case DragStart: IniciarArraste(); break;
                case DragMove e: Arrastar(e.Cursor); break;
                case DragEnd e: Soltar(e.Cursor); break;
                case DragCancel: CancelarArraste(); break;
                case ContextMenu e: MenuDeContexto(e.Cursor); break;
                case EnergyPanelOpen: AbrirPainel(); break;
                case EnergySelected e: EscolherEnergia(e.Nivel); break;
                case EnergyPanelClose: FecharPainel(); break;
                case CmdHide: Esconder(MotivoDoOcultamento.PorUsuario, "CMD_HIDE"); break;
                case CmdShow: MostrarPorComando(); break;
                case CmdPauseAutonomy: Pausar(true); break;
                case CmdResumeAutonomy: Pausar(false); break;
                case CmdOpenSettings: AbrirConfiguracoes(); break;
                case CmdResetPosition: RedefinirPosicao(); break;
                case CmdExit: Sair("CMD_EXIT"); break;
                case TopologyChanged e: MudarTopologia(e.Topologia); break;
                case SessionLocked: Esconder(MotivoDoOcultamento.PorSessao, "SESSION_LOCKED"); break;
                case SessionUnlocked: Reaparecer(MotivoDoOcultamento.PorSessao, "SESSION_UNLOCKED"); break;
                case Suspending: Esconder(MotivoDoOcultamento.PorSuspensao, "SUSPENDING"); break;
                case Resumed: Reaparecer(MotivoDoOcultamento.PorSuspensao, "RESUMED"); break;
                case SessionEnding: Sair("SESSION_ENDING"); break;
                case FullscreenTargetsChanged e: MudarTelaCheia(e.Ocupados); break;
                case SettingsChanged e: MudarPreferencias(e.Preferencias); break;
                case Tick: Passar(); break;
                case MovementSignal e: Sinalizar(e.Sinal); break;
                case AutonomyTimer e: Decidir(e.Geracao); break;
                case ExpressionChange e: _s = _s with { Expressao = e.Expressao }; break;
                default: throw new ArgumentException($"Evento desconhecido: {evento}.", nameof(evento));
            }
        }

        // ---------------------------------------------------------------- carga e topologia

        private void Carregar(Loaded e)
        {
            // Só a primeira carga vale. Antes dela o estado só pode ser BOOTING, HIDDEN (pedido de
            // esconder, bloqueio ou suspensão anteriores à carga) ou EXITING.
            if (_s.Carregado || _s.Estado is not (Estado.Booting or Estado.Hidden)) return;

            // A posição salva é restaurada pela cascata da partida (ARCHITECTURE.md 2.8): chave, tela do
            // monitor da época, principal. Sem ela, a posição inicial; o texto da regra fica o de sempre.
            Posicionamento lugar;
            PosicaoDoPersonagem posicao;
            string restaurada = "";
            if (e.PosicaoSalva is { } salva)
            {
                (lugar, posicao, OrigemDaRestauracao origem) = Posicionador.Restaurar(e.Topologia, salva, _cfg.Tamanho);
                restaurada = origem switch
                {
                    OrigemDaRestauracao.PelaChave => "; posição salva restaurada pela chave",
                    OrigemDaRestauracao.PeloRetangulo => "; posição salva restaurada pelo retângulo do monitor",
                    _ => "; posição salva restaurada no monitor principal",
                };
            }
            else
            {
                lugar = Posicionador.Inicial(e.Topologia, _cfg.Tamanho);
                posicao = Posicionador.Descrever(lugar);
            }
            _s = _s with { Carregado = true, Topologia = e.Topologia, Preferencias = Sanear(e.Preferencias), Lugar = lugar, Posicao = posicao };

            if (_s.Estado == Estado.Booting)
                Acomodar(lugar.Ancora, "BOOTING: configurações e topologia carregadas" + restaurada, posicao);
            else if (_s.Motivo == MotivoDoOcultamento.Nenhum)
                Acomodar(lugar.Ancora, "HIDDEN: pedido de mostrar anterior à carga" + restaurada, posicao);

            // O modo de tela cheia vale desde a partida: monitores ocupados avisados antes da carga
            // estão em cache.
            if (_s.Estado.Visivel() && _s.Preferencias.ModoTelaCheia && _s.Lugar is { } l && _s.Ocupados.Contem(l.Monitor.Chave))
                SairDoMonitorOcupado("BOOTING: carga sobre um monitor ocupado pela tela cheia");
        }

        private void MudarTopologia(Topologia nova)
        {
            bool mesma = _s.Topologia is not null && _s.Topologia.MesmaConfiguracao(nova);
            _s = _s with { Topologia = nova };
            if (mesma) return;

            // PRESSED, DRAGGING, BOOTING, HIDDEN e EXITING só atualizam o cache: a validação
            // acontece ao soltar, ao reaparecer ou ao terminar de carregar.
            GrupoDoEstado grupo = _s.Estado.Grupo();
            bool revalida = grupo is GrupoDoEstado.Autonomo or GrupoDoEstado.Fisico || _s.Estado == Estado.Reacting;
            if (!revalida || _s.Posicao is null) return;

            (Posicionamento r, PosicaoDoPersonagem p) = Posicionador.Reacomodar(nova, _s.Posicao, _cfg.Tamanho);
            Acomodar(r.Ancora, "TOPOLOGY_CHANGED", p);
        }

        // ---------------------------------------------------------------- ação direta

        private void Pressionar(PontoPx cursor)
        {
            if (!_s.Estado.AceitaPressionar() || _s.Lugar is null) return;
            PontoPx ancora = _s.Lugar.Ancora;
            _s = _s with { Pegada = new PontoPx(cursor.X - ancora.X, cursor.Y - ancora.Y), PassosRestantes = 0, TelaCheiaMudouNoGesto = false };
            IrPara(Estado.Pressed, "PRESS sobre pixel opaco");
        }

        /// <summary>
        /// Fim de um gesto do usuário (linha PRESSED, DRAGGING | FULLSCREEN_TARGETS_CHANGED): um
        /// arraste é sempre escolha de posição; um clique ou um cancelamento só descartam o retorno
        /// temporário se a tela cheia mudou durante o gesto (o ponto do usuário vale). Se o modo foi
        /// desligado durante o gesto, que não escolheu posição, o efeito temporário se desfaz agora:
        /// o personagem volta à posição de antes da tela cheia (linha SETTINGS_CHANGED).
        /// </summary>
        private void FimDoGestoDoUsuario(bool escolheuPosicao)
        {
            if (escolheuPosicao || _s.TelaCheiaMudouNoGesto)
            {
                _s = _s with { RetornoDaTelaCheia = null };
            }
            else if (!_s.Preferencias.ModoTelaCheia && _s.RetornoDaTelaCheia is { } retorno && _s.Topologia is { } topologia)
            {
                (Posicionamento lugar, PosicaoDoPersonagem posicao) = Posicionador.Reacomodar(topologia, retorno, _cfg.Tamanho);
                _s = _s with { Lugar = lugar, Posicao = posicao, RetornoDaTelaCheia = null };
            }
            _s = _s with { TelaCheiaMudouNoGesto = false };
        }

        private void Clicar()
        {
            if (_s.Estado != Estado.Pressed) return;
            // Com o botão pressionado, TOPOLOGY_CHANGED só atualiza o cache; "a validação acontece
            // ao soltar". Se o monitor do personagem mudou ou sumiu, valida já no CLICK, sem sair
            // da reação; senão, a reação começa no mesmo lugar.
            if (_s.Lugar is { } lugar && _s.Topologia is { } topologia && !Equals(topologia.PorChave(lugar.Monitor.Chave), lugar.Monitor))
            {
                (Posicionamento validado, PosicaoDoPersonagem posicao, _) = Validar(lugar.Ancora, _s.Posicao);
                _s = _s with { Lugar = validado, Posicao = posicao };
            }
            FimDoGestoDoUsuario(escolheuPosicao: false);
            _s = _s with { PassosRestantes = _cfg.PassosDaReacao, Expressao = Expressao.Feliz, Sinal = Sinal.FoiClicado };
            IrPara(Estado.Reacting, "CLICK");
        }

        private void CliqueDuplo()
        {
            Estado de = _s.Estado;
            if (_cfg.EsconderijoNoCliqueDuplo)
            {
                if (de is Estado.Pressed or Estado.Idle or Estado.Reacting or Estado.Peeking) AlternarEsconderijo(de);
                return;
            }
            if (de is not (Estado.Pressed or Estado.Idle or Estado.Reacting)) return;

            if (_cfg.PainelDeEnergiaDisponivel)
                AbrirPainelInterno();
            else
                _s = _s with { Sinal = Sinal.FoiClicadoDuasVezes, Expressao = Expressao.Rindo };

            if (de == Estado.Pressed && _s.Lugar is not null)
            {
                FimDoGestoDoUsuario(escolheuPosicao: false);
                Acomodar(_s.Lugar.Ancora, "DOUBLE_CLICK a partir de PRESSED");
            }
        }

        /// <summary>
        /// Clique duplo com o esconderijo ligado (DEC-025): escondido, sai de lá; senão, esconde-se
        /// atrás da borda mais próxima, só com a cabeça e as mãos para fora.
        /// </summary>
        private void AlternarEsconderijo(Estado de)
        {
            if (_s.Topologia is null) return;
            // O fim do gesto vem antes: com o modo de tela cheia desligado no meio dele, ele devolve
            // o personagem à posição de antes da tela cheia (DEC-020), e é de lá que ele se esconde.
            if (de == Estado.Pressed) FimDoGestoDoUsuario(escolheuPosicao: false);
            if (_s.Lugar is not { } lugar) return;
            if (_s.Esconderijo != LadoDoEsconderijo.Nenhum)
            {
                // Sai do esconderijo pela mão do usuário: na borda de baixo, fica de pé no chão; numa
                // lateral, fica grudado na parede (DEC-024).
                _s = _s with { Esconderijo = LadoDoEsconderijo.Nenhum, Expressao = Expressao.Feliz, Sinal = Sinal.FoiClicadoDuasVezes };
                Acomodar(lugar.Ancora, "DOUBLE_CLICK: sai do esconderijo", pelaMaoDoUsuario: true);
                return;
            }
            LadoDoEsconderijo lado = LadoMaisProximo(lugar);
            _s = _s with { Esconderijo = lado, Expressao = Expressao.Curioso, Sinal = Sinal.FoiClicadoDuasVezes };
            Acomodar(lugar.Ancora, $"DOUBLE_CLICK: esconde-se atrás da borda ({lado})");
        }

        /// <summary>
        /// Borda do esconderijo (DEC-025): a lateral mais próxima, se o personagem está no alto e junto
        /// dela (na parede, por exemplo); senão, a de baixo.
        /// </summary>
        private LadoDoEsconderijo LadoMaisProximo(Posicionamento lugar)
        {
            Superficies sup = Superficies.Do(_s.Topologia!, lugar.Monitor, lugar.Tamanho);
            double escala = lugar.Monitor.Dpi / 96.0;
            PontoPx a = lugar.Ancora;
            bool noAlto = sup.Chao - a.Y >= _cfg.Fisica.AlturaMinimaParaAgarrar * escala;
            double aEsquerda = a.X - sup.Esquerda, aDireita = sup.Direita - a.X;
            bool juntoDeUmaLateral = Math.Min(aEsquerda, aDireita) <= _cfg.Fisica.DistanciaParaAParede * escala;
            if (!noAlto || !juntoDeUmaLateral) return LadoDoEsconderijo.Baixo;
            return aDireita <= aEsquerda ? LadoDoEsconderijo.Direita : LadoDoEsconderijo.Esquerda;
        }

        /// <summary>
        /// Onde fica o esconderijo na borda dada (DEC-025), perto do lugar atual: na de baixo, os pés
        /// no chão (o quadro inteiro fica acima da barra e a pose só mostra a cabeça e as mãos); numa
        /// lateral, encostado nela, na mesma altura. O sprite continua inteiro na área útil.
        /// </summary>
        private Posicionamento EsconderijoPara(Posicionamento lugar, LadoDoEsconderijo lado)
        {
            Superficies sup = Superficies.Do(_s.Topologia!, lugar.Monitor, lugar.Tamanho);
            PontoPx a = lugar.Ancora;
            PontoPx ancora = lado switch
            {
                LadoDoEsconderijo.Direita => new PontoPx(sup.Direita, Math.Clamp(a.Y, sup.Teto, sup.Chao)),
                LadoDoEsconderijo.Esquerda => new PontoPx(sup.Esquerda, Math.Clamp(a.Y, sup.Teto, sup.Chao)),
                _ => new PontoPx(Math.Clamp(a.X, sup.Esquerda, sup.Direita), sup.Chao),
            };
            return NoLugar(lugar.Monitor, ancora);
        }

        /// <summary>Caras de quem está escondido, espiando o que acontece (DEC-025).</summary>
        private static readonly Expressao[] ExpressoesDoEscondido = [Expressao.Curioso, Expressao.Travesso, Expressao.Feliz, Expressao.Surpreso, Expressao.Pensativo, Expressao.Rindo];

        private void IniciarArraste()
        {
            if (_s.Estado != Estado.Pressed) return;
            // Invariante 9: iniciar um arraste fecha o painel de energia.
            FecharPainelSeAberto();
            // Arrastar tira o personagem do esconderijo (DEC-025).
            _s = _s with { Esconderijo = LadoDoEsconderijo.Nenhum };
            IrPara(Estado.Dragging, "DRAG_START");
        }

        private void Arrastar(PontoPx cursor)
        {
            if (_s.Estado != Estado.Dragging || _s.Topologia is null) return;
            // Invariante 2: posição = cursor menos o deslocamento da pegada, sem física nem limite.
            var ancora = new PontoPx(cursor.X - _s.Pegada.X, cursor.Y - _s.Pegada.Y);
            _s = _s with { Lugar = LugarLivre(_s.Topologia, ancora) };
        }

        private void Soltar(PontoPx cursor)
        {
            if (_s.Estado != Estado.Dragging) return;
            var ancora = new PontoPx(cursor.X - _s.Pegada.X, cursor.Y - _s.Pegada.Y);
            // Soltar é escolha manual: descarta o retorno temporário da tela cheia (DEC-013).
            FimDoGestoDoUsuario(escolheuPosicao: true);
            Acomodar(ancora, "DRAG_END", pelaMaoDoUsuario: true);
            if (_s.Posicao is not null) _depois.Add(new GravarPosicao(_s.Posicao));
        }

        private void CancelarArraste()
        {
            if (_s.Lugar is null) return;
            if (_s.Estado == Estado.Dragging)
            {
                // O personagem fica onde estava; não volta ao ponto de origem (ARCHITECTURE.md 2.7).
                FimDoGestoDoUsuario(escolheuPosicao: true);
                Acomodar(_s.Lugar.Ancora, "DRAG_CANCEL", pelaMaoDoUsuario: true);
                if (_s.Posicao is not null) _depois.Add(new GravarPosicao(_s.Posicao));
            }
            else if (_s.Estado == Estado.Pressed)
            {
                FimDoGestoDoUsuario(escolheuPosicao: false);
                Acomodar(_s.Lugar.Ancora, "DRAG_CANCEL em PRESSED (captura perdida antes do limiar)");
            }
        }

        private void MenuDeContexto(PontoPx cursor)
        {
            if (!_s.Estado.Visivel() || _s.Estado is Estado.Pressed or Estado.Dragging) return;
            _depois.Add(new AbrirMenu(cursor));
        }

        // ---------------------------------------------------------------- painel de energia

        private void AbrirPainel()
        {
            if (!_cfg.PainelDeEnergiaDisponivel || !_s.Estado.Visivel() || _s.Estado is Estado.Pressed or Estado.Dragging) return;
            AbrirPainelInterno();
        }

        private void AbrirPainelInterno()
        {
            if (_s.PainelAberto) return;
            _s = _s with { PainelAberto = true };
            _depois.Add(new AbrirPainelDeEnergia());
        }

        private void EscolherEnergia(NivelDeEnergia nivel)
        {
            // SECURITY.md 7: só os três níveis permitidos; um valor fora deles é ignorado.
            if (!_s.PainelAberto || !Enum.IsDefined(nivel)) return;
            if (_s.Preferencias.Energia == nivel) return;
            _s = _s with { Preferencias = _s.Preferencias with { Energia = nivel } };
            _depois.Add(new GravarPreferencias(_s.Preferencias));
        }

        private void FecharPainel()
        {
            if (!_s.PainelAberto) return;
            _s = _s with { PainelAberto = false };
            _reagendar = true;
        }

        private void FecharPainelSeAberto()
        {
            if (!_s.PainelAberto) return;
            _s = _s with { PainelAberto = false };
            _antes.Add(new FecharPainelDeEnergia());
        }

        // ---------------------------------------------------------------- bandeja, menu e sessão

        private void Esconder(MotivoDoOcultamento motivo, string regra)
        {
            if (_s.Estado == Estado.Hidden)
            {
                // Um evento do sistema não troca uma ocultação do usuário; entre os motivos do
                // sistema, a sessão bloqueada prevalece sobre a suspensão e a tela cheia
                // (precedência em ARCHITECTURE.md 2.6).
                if (Precedencia(motivo) > Precedencia(_s.Motivo))
                {
                    _transicoes.Add(new Transicao(Estado.Hidden, Estado.Hidden, $"{regra}: motivo {_s.Motivo} -> {motivo}"));
                    // A ocultação deixa de ser da tela cheia: o retorno temporário volta a ser a
                    // posição do personagem, sem reaparecer, e não sobra para o próximo episódio.
                    if (_s.Motivo == MotivoDoOcultamento.PorTelaCheia)
                        _s = _s with { Posicao = _s.RetornoDaTelaCheia ?? _s.Posicao, RetornoDaTelaCheia = null };
                    _s = _s with { Motivo = motivo };
                }
                return;
            }

            LiberarGestoDoUsuario();
            FixarArrasteInterrompido();
            FecharPainelSeAberto();
            _s = _s with { Motivo = motivo, PassosRestantes = 0 };
            IrPara(Estado.Hidden, regra);
            // Com o modo desligado, o retorno só sobra de um PRESSED interrompido: a posição de
            // antes da tela cheia volta a valer, como no fim do gesto.
            if (!_s.Preferencias.ModoTelaCheia && _s.RetornoDaTelaCheia is { } retorno)
                _s = _s with { Posicao = retorno, RetornoDaTelaCheia = null };
            GravarPosicaoDoUsuario();
        }

        /// <summary>
        /// Grava a posição escolhida pelo usuário: durante a tela cheia, a de antes da transferência
        /// automática, nunca a temporária (SECURITY.md 5: a posição temporária fica só em memória).
        /// </summary>
        private void GravarPosicaoDoUsuario()
        {
            if ((_s.RetornoDaTelaCheia ?? _s.Posicao) is { } posicao) _depois.Add(new GravarPosicao(posicao));
        }

        private static int Precedencia(MotivoDoOcultamento motivo) => motivo switch
        {
            MotivoDoOcultamento.PorUsuario => 4,
            MotivoDoOcultamento.PorSessao => 3,
            MotivoDoOcultamento.PorSuspensao => 2,
            MotivoDoOcultamento.PorTelaCheia => 1,
            _ => 0,
        };

        private void Reaparecer(MotivoDoOcultamento motivoQueSeDesfaz, string regra)
        {
            // Invariante 10: SESSION_UNLOCKED e RESUMED nunca mostram o que o usuário escondeu.
            if (_s.Estado != Estado.Hidden || _s.Motivo != motivoQueSeDesfaz) return;
            Mostrar(regra);
            // Um evento do sistema também não desfaz o modo de tela cheia: se o personagem
            // reapareceu num monitor ainda ocupado (monitores em cache), o modo age de novo.
            if (_s.Preferencias.ModoTelaCheia && _s.Lugar is { } lugar && _s.Ocupados.Contem(lugar.Monitor.Chave))
                SairDoMonitorOcupado($"{regra}: reapareceu num monitor ocupado pela tela cheia");
        }

        private void MostrarPorComando()
        {
            if (_s.Estado == Estado.Hidden)
            {
                if (_s.Motivo == MotivoDoOcultamento.PorTelaCheia)
                {
                    // O usuário pediu: aparece na posição anterior e descarta o retorno temporário.
                    _s = _s with { Posicao = _s.RetornoDaTelaCheia ?? _s.Posicao, RetornoDaTelaCheia = null };
                }
                else
                {
                    // Escondido por outro motivo no meio de um episódio de tela cheia: mostrar é
                    // escolha manual, então reaparece onde estava e o fim da tela cheia não o move
                    // mais (invariante 14).
                    _s = _s with { RetornoDaTelaCheia = null };
                }
                Mostrar("CMD_SHOW");
                return;
            }
            // Visível: mostrar manualmente durante a tela cheia vale como escolha do usuário
            // (invariante 14).
            if (_s.Estado.Visivel()) _s = _s with { RetornoDaTelaCheia = null };
        }

        private void Mostrar(string regra)
        {
            if (!_s.Carregado || _s.Topologia is null)
            {
                // Ainda não carregou: aparece quando a carga chegar.
                _s = _s with { Motivo = MotivoDoOcultamento.Nenhum };
                return;
            }
            if (_s.Posicao is null)
            {
                Posicionamento inicial = Posicionador.Inicial(_s.Topologia, _cfg.Tamanho);
                Acomodar(inicial.Ancora, regra, Posicionador.Descrever(inicial));
                return;
            }
            (Posicionamento r, PosicaoDoPersonagem p) = Posicionador.Reacomodar(_s.Topologia, _s.Posicao, _cfg.Tamanho);
            Acomodar(r.Ancora, regra, p);
        }

        private void Pausar(bool pausar)
        {
            if (_s.AutonomiaPausada == pausar) return;
            _s = _s with { AutonomiaPausada = pausar };
            if (!pausar) _reagendar = true;
            // Fase 4 (DEC-022): pausar para a caminhada na hora; na parede ele desce e pendurado se
            // solta nos passos seguintes (PassoEscalando, PassoPendurado).
            if (pausar && _cfg.Movimento && _s.Estado == Estado.Walking) IrPara(Estado.Idle, "CMD_PAUSE_AUTONOMY: para de andar");
        }

        private void AbrirConfiguracoes()
        {
            if (_cfg.ConfiguracoesDisponiveis) _depois.Add(new AbrirConfiguracoes());
        }

        private void RedefinirPosicao()
        {
            if (!_s.Carregado || _s.Topologia is null || _s.Estado is Estado.Booting or Estado.Pressed or Estado.Dragging) return;
            Posicionamento inicial = Posicionador.Inicial(_s.Topologia, _cfg.Tamanho);
            _s = _s with { RetornoDaTelaCheia = null };
            if (_s.Estado == Estado.Hidden)
                _s = _s with { Lugar = inicial, Posicao = Posicionador.Descrever(inicial) };
            else
                Acomodar(inicial.Ancora, "CMD_RESET_POSITION", Posicionador.Descrever(inicial));
            if (_s.Posicao is not null) _depois.Add(new GravarPosicao(_s.Posicao));
        }

        private void Sair(string regra)
        {
            LiberarGestoDoUsuario();
            FixarArrasteInterrompido();
            FecharPainelSeAberto();
            IrPara(Estado.Exiting, regra);
            GravarPosicaoDoUsuario();
            _depois.Add(new Encerrar());
        }

        private void LiberarGestoDoUsuario()
        {
            if (_s.Estado is Estado.Pressed or Estado.Dragging) _antes.Add(new LiberarCaptura());
        }

        // ---------------------------------------------------------------- tela cheia (DEC-013)

        private void MudarTelaCheia(MonitoresOcupados ocupados)
        {
            bool mudou = !ocupados.Equals(_s.Ocupados);
            _s = _s with { Ocupados = ocupados };
            // Uma vez por mudança; com o modo desligado, só o cache.
            if (!mudou || !_s.Preferencias.ModoTelaCheia || _s.Topologia is null) return;

            // Invariante 14: PRESSED e DRAGGING nunca são interrompidos pelo modo; ao soltar, o ponto
            // do usuário vale e o retorno temporário é descartado (FimDoGestoDoUsuario).
            if (_s.Estado is Estado.Pressed or Estado.Dragging)
            {
                _s = _s with { TelaCheiaMudouNoGesto = true };
                return;
            }
            if (_s.Estado is Estado.Booting or Estado.Exiting) return;

            if (_s.Estado == Estado.Hidden)
            {
                if (_s.Motivo != MotivoDoOcultamento.PorTelaCheia)
                {
                    // O episódio terminou com o personagem escondido por outro motivo (usuário,
                    // sessão, suspensão): ele não reaparece, mas a posição de antes da tela cheia
                    // volta a valer para quando reaparecer, e o retorno não sobra.
                    if (ocupados.Vazio && _s.RetornoDaTelaCheia is { } retorno)
                        _s = _s with { Posicao = retorno, RetornoDaTelaCheia = null };
                    return;
                }
                if (ocupados.Vazio)
                {
                    RestaurarRetorno("FULLSCREEN_TARGETS_CHANGED(vazio): restaura a posição anterior");
                    return;
                }
                PosicaoDoPersonagem? referencia = _s.RetornoDaTelaCheia ?? _s.Posicao;
                if (referencia is null) return;
                MonitorDoDesktop? livre = MonitorLivreMaisProximo(_s.Topologia, ocupados, referencia.AncoraAbsoluta);
                if (livre is not null)
                {
                    Posicionamento destino = Posicionador.NoMonitor(livre, referencia.FracaoX, referencia.FracaoY, _cfg.Tamanho);
                    Acomodar(destino.Ancora, "FULLSCREEN_TARGETS_CHANGED: reaparece no monitor livre");
                }
                return;
            }

            if (ocupados.Vazio)
            {
                if (_s.RetornoDaTelaCheia is not null)
                    RestaurarRetorno("FULLSCREEN_TARGETS_CHANGED(vazio): restaura a posição anterior");
                return;
            }

            if (_s.Lugar is null || _s.Posicao is null || !ocupados.Contem(_s.Lugar.Monitor.Chave)) return;
            SairDoMonitorOcupado("FULLSCREEN_TARGETS_CHANGED");
        }

        /// <summary>
        /// Personagem visível num monitor ocupado pela tela cheia (monitores em cache): guarda a
        /// posição anterior só em memória, se ainda não houver uma, e transfere para o monitor livre
        /// mais próximo, sem ativar; se nenhum estiver livre, fecha o painel e esconde.
        /// </summary>
        private void SairDoMonitorOcupado(string regra)
        {
            if (_s.Topologia is null || _s.Lugar is null || _s.Posicao is null) return;
            PosicaoDoPersonagem anterior = _s.RetornoDaTelaCheia ?? _s.Posicao;
            MonitorDoDesktop? monitorLivre = MonitorLivreMaisProximo(_s.Topologia, _s.Ocupados, _s.Lugar.Ancora);
            _s = _s with { RetornoDaTelaCheia = anterior };
            if (monitorLivre is null)
            {
                FecharPainelSeAberto();
                _s = _s with { Motivo = MotivoDoOcultamento.PorTelaCheia, PassosRestantes = 0 };
                IrPara(Estado.Hidden, $"{regra}: nenhum monitor livre");
                return;
            }
            Posicionamento noLivre = Posicionador.NoMonitor(monitorLivre, _s.Posicao.FracaoX, _s.Posicao.FracaoY, _cfg.Tamanho);
            Acomodar(noLivre.Ancora, $"{regra}: transfere para o monitor livre");
        }

        private void RestaurarRetorno(string regra)
        {
            PosicaoDoPersonagem? retorno = _s.RetornoDaTelaCheia;
            _s = _s with { RetornoDaTelaCheia = null };
            if (retorno is not null) _s = _s with { Posicao = retorno };
            Mostrar(regra);
        }

        private static MonitorDoDesktop? MonitorLivreMaisProximo(Topologia topologia, MonitoresOcupados ocupados, PontoPx referencia)
        {
            MonitorDoDesktop? melhor = null;
            long melhorDistancia = long.MaxValue;
            foreach (MonitorDoDesktop m in topologia.Monitores)
            {
                if (ocupados.Contem(m.Chave)) continue;
                long d = m.Tela.DistanciaAoQuadrado(referencia);
                if (d < melhorDistancia)
                {
                    melhor = m;
                    melhorDistancia = d;
                }
            }
            return melhor;
        }

        /// <summary>SECURITY.md 7: nível de energia fora de BAIXA/MEDIA/ALTA vira o padrão seguro, Média.</summary>
        private static Preferencias Sanear(Preferencias preferencias)
            => Enum.IsDefined(preferencias.Energia) ? preferencias : preferencias with { Energia = Preferencias.Padrao.Energia };

        private void MudarPreferencias(Preferencias novas)
        {
            Preferencias antes = _s.Preferencias;
            novas = Sanear(novas);
            _s = _s with { Preferencias = novas };
            if (!antes.ModoTelaCheia || novas.ModoTelaCheia) return;

            // Desligar o modo desfaz o efeito temporário dele (Q-09): a posição temporária nunca vira
            // a do usuário (invariante 16).
            if (_s.Estado == Estado.Hidden && _s.Motivo == MotivoDoOcultamento.PorTelaCheia)
            {
                RestaurarRetorno("SETTINGS_CHANGED: modo de tela cheia desligado");
            }
            else if (_s.Estado == Estado.Hidden)
            {
                // Escondido por outro motivo: não reaparece, mas a posição de antes volta a valer.
                if (_s.RetornoDaTelaCheia is { } retorno) _s = _s with { Posicao = retorno, RetornoDaTelaCheia = null };
            }
            else if (_s.Estado is Estado.Pressed or Estado.Dragging)
            {
                // Invariante 14: o gesto não é interrompido; o fim dele decide (FimDoGestoDoUsuario).
            }
            else if (_s.Estado.Visivel() && _s.RetornoDaTelaCheia is not null)
            {
                RestaurarRetorno("SETTINGS_CHANGED: modo de tela cheia desligado");
            }
            else
            {
                _s = _s with { RetornoDaTelaCheia = null };
            }
        }

        // ---------------------------------------------------------------- relógio e movimento

        private void Passar()
        {
            _s = _s with { Passos = _s.Passos + 1 };
            switch (_s.Estado)
            {
                case Estado.Reacting:
                    _s = _s with { PassosRestantes = _s.PassosRestantes - 1 };
                    if (_s.PassosRestantes <= 0 && _s.Lugar is not null)
                        Acomodar(_s.Lugar.Ancora, "REACTING: fim da reação");
                    break;
                case Estado.Landing:
                    _s = _s with { PassosRestantes = _s.PassosRestantes - 1 };
                    if (_s.PassosRestantes <= 0)
                        IrPara(Estado.Idle, "LANDING: fim do pouso");
                    break;
                case Estado.Idle when _s.Gesto != Gesto.Nenhum:
                    _s = _s with { PassosDoGesto = _s.PassosDoGesto - 1 };
                    if (_s.PassosDoGesto <= 0) EncerrarGesto();
                    break;
                // Fase 4 (DEC-022): o passo físico move o personagem pelas superfícies.
                case Estado.Walking when _cfg.Movimento:
                    PassoAndando();
                    break;
                case Estado.Climbing when _cfg.Movimento:
                    PassoEscalando();
                    break;
                case Estado.Hanging when _cfg.Movimento:
                    PassoPendurado();
                    break;
                case Estado.Jumping or Estado.Falling when _cfg.Movimento:
                    PassoNoAr();
                    break;
            }
        }

        private void Sinalizar(SinalDeMovimento sinal)
        {
            bool calmo = _s.AutonomiaPausada || _s.PainelAberto;
            switch (_s.Estado, sinal)
            {
                case (Estado.Walking, SinalDeMovimento.Parede):
                    Escolher("WALKING: parede", calmo,
                        (Estado.Idle, 3),
                        (Estado.Climbing, Permite(AcoesAutonomas.Escalar) ? Perfil.PesoEscalar : 0),
                        (Estado.Walking, 2));
                    if (_s.Estado == Estado.Walking) Virar();
                    else if (_s.Estado == Estado.Climbing) TalvezFoguete();
                    break;
                case (Estado.Walking, SinalDeMovimento.Passagem):
                    IrPara(Estado.Walking, "WALKING: passagem (atravessa)");
                    break;
                case (Estado.Walking, SinalDeMovimento.FimDoChao):
                    IrPara(Estado.Falling, "WALKING: fim do chão");
                    break;
                case (Estado.Climbing, SinalDeMovimento.TopoDaParede):
                    Escolher("CLIMBING: topo da área útil", calmo,
                        (Estado.Idle, 2),
                        (Estado.Walking, Permite(AcoesAutonomas.Andar) ? 2 : 0),
                        (Estado.Jumping, Permite(AcoesAutonomas.Pular) ? Perfil.PesoPular : 0),
                        (Estado.Falling, 1));
                    break;
                case (Estado.Climbing, SinalDeMovimento.FimDaParede):
                    IrPara(Estado.Idle, "CLIMBING: fim da parede");
                    break;
                case (Estado.Climbing, SinalDeMovimento.BordaSuperior):
                    IrPara(Estado.Hanging, "CLIMBING: alcança borda superior apoiável");
                    break;
                case (Estado.Hanging, SinalDeMovimento.FimDaBorda):
                    // Com a toon force (DEC-023), toda lateral dá para descer, mesmo a passagem.
                    Escolher("HANGING: passagem compatível ou fim da borda", calmo,
                        (Estado.Climbing, 2),
                        (Estado.Hanging, 1),
                        (Estado.Falling, 1));
                    if (_s.Estado == Estado.Hanging) Virar();
                    // Desce pela parede em que chegou (a direção continua virada para ela).
                    else if (_s.Estado == Estado.Climbing) _s = _s with { Movimento = _s.Movimento with { SentidoVertical = 1 } };
                    break;
                case (Estado.Jumping or Estado.Falling, SinalDeMovimento.ContatoComOChao):
                    _s = _s with { PassosRestantes = _cfg.PassosDoPouso, Sinal = Sinal.Pousou };
                    IrPara(Estado.Landing, $"{_s.Estado.ToString().ToUpperInvariant()}: contato com o chão");
                    break;
            }
        }

        private void Decidir(long geracao)
        {
            // Disparo de um agendamento substituído ou cancelado: ignorado.
            if (!_s.DecisaoAgendada || geracao != _s.Geracao) return;
            // Invariante 1 e ARCHITECTURE.md 2.3: nada autônomo com o usuário no controle, com
            // a autonomia pausada ou com o painel aberto.
            if (_s.Estado.ControladoPeloUsuario() || _s.AutonomiaPausada || _s.PainelAberto || !_s.Estado.Visivel()) return;

            _s = _s with { DecisaoAgendada = false };
            _reagendar = true;
            switch (_s.Estado)
            {
                case Estado.Idle:
                    DecidirParado();
                    break;
                case Estado.Resting:
                    _s = _s with { Sinal = Sinal.Acordou, Expressao = Expressao.Neutro };
                    IrPara(Estado.Idle, "RESTING + AUTONOMY_TIMER: acorda");
                    break;
                case Estado.Peeking:
                    // Escondido (DEC-025): nada o tira de lá; só troca a cara, espiando.
                    (int cara, Aleatorio aDaCara) = _s.Aleatorio.Entre(0, ExpressoesDoEscondido.Length - 2);
                    Expressao atual = _s.Expressao;
                    Expressao[] outras = [.. ExpressoesDoEscondido.Where(e => e != atual)];
                    _s = _s with { Aleatorio = aDaCara, Expressao = outras[Math.Min(cara, outras.Length - 1)] };
                    _transicoes.Add(new Transicao(Estado.Peeking, Estado.Peeking, "PEEKING + AUTONOMY_TIMER: espia com outra cara"));
                    break;
                case Estado.Climbing or Estado.Hanging when _cfg.Movimento && _s.PresoPeloUsuario:
                    DecidirPreso();
                    break;
                case Estado.Climbing when _cfg.Movimento && _s.Movimento.Agarrado:
                    // Agarrado sem ter sido posto pelo usuário (numa revalidação): volta a escalar,
                    // para cima ou para baixo, salta ou se solta.
                    Escolher("CLIMBING agarrado + AUTONOMY_TIMER", calmo: false,
                        (Estado.Climbing, Permite(AcoesAutonomas.Escalar) ? 4 : 0),
                        (Estado.Jumping, Permite(AcoesAutonomas.Pular) ? Perfil.PesoPular : 0),
                        (Estado.Falling, 1));
                    if (_s.Estado == Estado.Climbing)
                    {
                        (int sobe, Aleatorio a) = _s.Aleatorio.Entre(0, 1);
                        _s = _s with { Aleatorio = a, Movimento = _s.Movimento with { Agarrado = false, SentidoVertical = sobe == 0 ? -1 : 1 } };
                        if (sobe == 0) TalvezFoguete();
                    }
                    else if (_s.Estado == Estado.Jumping)
                    {
                        SaltarDaParede();
                    }
                    break;
                case Estado.Hanging when _cfg.Movimento && _s.Movimento.Agarrado:
                    bool naQuinaAgarrado = Mundo(out _, out Superficies supAgarrado, out _) && supAgarrado.NaLateral(_s.Movimento.X, out _);
                    Escolher("HANGING agarrado + AUTONOMY_TIMER", calmo: false,
                        (Estado.Hanging, 3),
                        (Estado.Climbing, naQuinaAgarrado ? 2 : 0),
                        (Estado.Jumping, Permite(AcoesAutonomas.Pular) ? Perfil.PesoPular : 0),
                        (Estado.Falling, 1));
                    if (_s.Estado == Estado.Hanging)
                        _s = _s with { Movimento = _s.Movimento with { Agarrado = false } };
                    else
                        SairDoTeto();
                    break;
                case Estado.Climbing:
                    Escolher("CLIMBING + AUTONOMY_TIMER", calmo: false,
                        (Estado.Jumping, Permite(AcoesAutonomas.Pular) ? Perfil.PesoPular : 0),
                        (Estado.Falling, 1));
                    if (_cfg.Movimento && _s.Estado == Estado.Jumping) SaltarDaParede();
                    break;
                case Estado.Hanging:
                    // Pendurado no meio da borda não há parede para descer; só na quina.
                    bool naQuina = !_cfg.Movimento || (Mundo(out _, out Superficies sup, out _) && sup.NaLateral(_s.Movimento.X, out _));
                    Escolher("HANGING + AUTONOMY_TIMER", calmo: false,
                        (Estado.Hanging, 2),
                        (Estado.Climbing, naQuina ? 2 : 0),
                        (Estado.Jumping, Permite(AcoesAutonomas.Pular) ? Perfil.PesoPular : 0),
                        (Estado.Falling, 1));
                    if (_cfg.Movimento) SairDoTeto();
                    break;
            }
        }

        private void DecidirParado()
        {
            PerfilDeEnergia perfil = Perfil;
            var opcoes = new List<(AcoesAutonomas Acao, int Peso)>();
            void Opcao(AcoesAutonomas acao, int peso)
            {
                if (Permite(acao) && peso > 0) opcoes.Add((acao, peso));
            }
            // Com a física, escalar exige saber onde estão as laterais do monitor. Com a toon force
            // (DEC-023), as duas são escaláveis, mesmo a que encosta em outro monitor.
            bool temLateral = !_cfg.Movimento || Mundo(out _, out _, out _);
            Opcao(AcoesAutonomas.Andar, perfil.PesoAndar);
            Opcao(AcoesAutonomas.Escalar, temLateral ? perfil.PesoEscalar : 0);
            Opcao(AcoesAutonomas.Pular, perfil.PesoPular);
            Opcao(AcoesAutonomas.Descansar, perfil.PesoDescansar);
            Opcao(AcoesAutonomas.Gesto, perfil.PesoGesto);
            Opcao(AcoesAutonomas.TrocarExpressao, perfil.PesoTrocarExpressao);
            if (opcoes.Count == 0) return;

            (int indice, Aleatorio a) = _s.Aleatorio.Ponderado([.. opcoes.Select(o => o.Peso)]);
            _s = _s with { Aleatorio = a };
            switch (opcoes[indice].Acao)
            {
                case AcoesAutonomas.Andar:
                    (int lado, Aleatorio a2) = _s.Aleatorio.Entre(0, 1);
                    _s = _s with { Aleatorio = a2, Direcao = lado == 0 ? Direcao.Direita : Direcao.Esquerda };
                    IrPara(Estado.Walking, "IDLE + AUTONOMY_TIMER: andar");
                    if (_cfg.Movimento) PlanejarCaminhada(perfil);
                    break;
                case AcoesAutonomas.Escalar when _cfg.Movimento:
                    PlanejarEscalada();
                    break;
                case AcoesAutonomas.Escalar:
                    IrPara(Estado.Climbing, "IDLE + AUTONOMY_TIMER: escalar");
                    break;
                case AcoesAutonomas.Pular:
                    IrPara(Estado.Jumping, "IDLE + AUTONOMY_TIMER: pular");
                    if (_cfg.Movimento) PlanejarPulo(perfil);
                    break;
                case AcoesAutonomas.Descansar:
                    _s = _s with { Expressao = Expressao.Sonolento };
                    IrPara(Estado.Resting, "IDLE + AUTONOMY_TIMER: descansar");
                    break;
                case AcoesAutonomas.Gesto:
                    (int g, Aleatorio a3) = _s.Aleatorio.Entre((int)Gesto.Espiar, (int)Gesto.Brincar);
                    (int passos, Aleatorio a4) = a3.Entre(perfil.PassosDoGestoMinimo, perfil.PassosDoGestoMaximo);
                    _s = _s with { Aleatorio = a4, Gesto = (Gesto)g, PassosDoGesto = passos };
                    _transicoes.Add(new Transicao(Estado.Idle, Estado.Idle, $"IDLE + AUTONOMY_TIMER: gesto {(Gesto)g}"));
                    break;
                case AcoesAutonomas.TrocarExpressao:
                    (int e, Aleatorio a5) = _s.Aleatorio.Entre(0, Enum.GetValues<Expressao>().Length - 2);
                    // Sorteia entre as outras expressões: pula a atual.
                    var nova = (Expressao)(e >= (int)_s.Expressao ? e + 1 : e);
                    _s = _s with { Aleatorio = a5, Expressao = nova };
                    break;
            }
        }

        private bool Permite(AcoesAutonomas acao) => (_cfg.Acoes & acao) == acao;

        /// <summary>
        /// Escolha ponderada e reproduzível entre destinos permitidos. Com a autonomia pausada ou o
        /// painel aberto, fica com o primeiro destino, o mais calmo.
        /// </summary>
        private void Escolher(string regra, bool calmo, params (Estado Destino, int Peso)[] opcoes)
        {
            (Estado Destino, int Peso)[] validas = [.. opcoes.Where(o => o.Peso > 0)];
            if (validas.Length == 0) return;
            Estado destino;
            if (calmo || validas.Length == 1)
            {
                destino = validas[0].Destino;
            }
            else
            {
                (int i, Aleatorio a) = _s.Aleatorio.Ponderado([.. validas.Select(o => o.Peso)]);
                _s = _s with { Aleatorio = a };
                destino = validas[i].Destino;
            }
            IrPara(destino, regra);
        }

        private void Virar() => _s = _s with { Direcao = _s.Direcao == Direcao.Direita ? Direcao.Esquerda : Direcao.Direita };

        private void EncerrarGesto()
        {
            _s = _s with { Gesto = Gesto.Nenhum, PassosDoGesto = 0 };
            _reagendar = true;
        }

        // ---------------------------------------------------------------- movimento (Fase 4, DEC-022)

        /// <summary>Com a autonomia pausada ou o painel aberto, o movimento em curso termina num lugar estável.</summary>
        private bool Calmo => _s.AutonomiaPausada || _s.PainelAberto;

        /// <summary>+1 para a direita, −1 para a esquerda.</summary>
        private int Sentido => _s.Direcao == Direcao.Direita ? 1 : -1;

        /// <summary>Monitor atual (da topologia em cache), superfícies para o sprite e escala; falso sem lugar ou topologia.</summary>
        private bool Mundo(out MonitorDoDesktop monitor, out Superficies superficies, out double escala)
        {
            monitor = null!;
            superficies = default;
            escala = 1;
            if (_s.Lugar is not { } lugar || _s.Topologia is not { } topologia) return false;
            monitor = topologia.PorChave(lugar.Monitor.Chave) ?? lugar.Monitor;
            superficies = Superficies.Do(topologia, monitor, _cfg.Tamanho.ParaPixels(monitor.Dpi));
            escala = monitor.Dpi / 96.0;
            return true;
        }

        /// <summary>Pixels físicos por passo fixo, a partir de DIPs por segundo.</summary>
        private double PorPasso(double dipPorSegundo, double escala) => dipPorSegundo * escala / _cfg.PassosPorSegundo;

        /// <summary>
        /// Leva a âncora fina a (x, y): a janela usa a posição arredondada, e a posição relativa
        /// acompanha, para sobreviver a uma mudança de topologia no meio do movimento.
        /// </summary>
        private void MoverPara(MonitorDoDesktop monitor, double x, double y)
        {
            var ancora = new PontoPx((int)Math.Round(x, MidpointRounding.AwayFromZero), (int)Math.Round(y, MidpointRounding.AwayFromZero));
            TamanhoPx tamanho = _cfg.Tamanho.ParaPixels(monitor.Dpi);
            var lugar = new Posicionamento(monitor, ancora, tamanho, Posicionador.RetanguloDoSprite(ancora, tamanho));
            _s = _s with { Lugar = lugar, Posicao = Posicionador.Descrever(lugar), Movimento = _s.Movimento with { X = x, Y = y } };
        }

        private void PassoAndando()
        {
            if (!Mundo(out MonitorDoDesktop m, out Superficies sup, out double escala)) return;
            if (Calmo)
            {
                IrPara(Estado.Idle, "WALKING: autonomia pausada ou painel aberto (para)");
                return;
            }
            EstadoDoMovimento mv = _s.Movimento;
            double passo = PorPasso(_cfg.Fisica.VelocidadeAndando, escala);
            double x = mv.X + Sentido * passo;
            int limite = Sentido > 0 ? sup.Direita : sup.Esquerda;
            bool naBorda = Sentido > 0 ? x >= limite : x <= limite;
            if (naBorda) x = limite;
            _s = _s with { Movimento = mv with { Restante = mv.Restante - passo } };
            MoverPara(m, x, sup.Chao);

            if (naBorda)
            {
                // Toon force (DEC-023): a lateral é parede para ele mesmo quando outro monitor
                // encosta nela; a travessia pelas passagens é da Fase 5.
                if (mv.QuerEscalar)
                {
                    IrPara(Estado.Climbing, "WALKING: parede (andava até ela para escalar)");
                    TalvezFoguete();
                    return;
                }
                Sinalizar(SinalDeMovimento.Parede);
                return;
            }
            if (!mv.QuerEscalar && _s.Movimento.Restante <= 0) IrPara(Estado.Idle, "WALKING: fim do percurso");
        }

        private void PassoEscalando()
        {
            if (!Mundo(out MonitorDoDesktop m, out Superficies sup, out double escala)) return;
            EstadoDoMovimento mv = _s.Movimento;
            if (mv.Agarrado) return;
            if (_s.PresoPeloUsuario)
            {
                PassoPresoNaParede(m, sup, escala);
                return;
            }
            // Pausado ou com o painel aberto, desce até o chão em vez de subir (e o foguete apaga).
            int sentido = Calmo ? 1 : mv.SentidoVertical;
            bool foguete = mv.Foguete && sentido < 0;
            double velocidade = foguete ? _cfg.Fisica.VelocidadeDoFoguete : _cfg.Fisica.VelocidadeEscalando;
            double y = mv.Y + sentido * PorPasso(velocidade, escala);
            double x = Sentido > 0 ? sup.Direita : sup.Esquerda;
            _s = _s with { Movimento = mv with { SentidoVertical = sentido, Foguete = foguete } };
            if (sentido < 0 && y <= sup.Teto)
            {
                MoverPara(m, x, sup.Teto);
                Sinalizar(SinalDeMovimento.BordaSuperior);
                // Pendurado, segue pela borda para dentro, de costas para a parede.
                if (_s.Estado == Estado.Hanging) Virar();
                return;
            }
            if (sentido > 0 && y >= sup.Chao)
            {
                MoverPara(m, x, sup.Chao);
                Sinalizar(SinalDeMovimento.FimDaParede);
                return;
            }
            MoverPara(m, x, y);
        }

        /// <summary>
        /// Preso pelo usuário na parede (DEC-024): percorre o passeio sorteado pela mesma lateral, sem
        /// chegar ao chão nem passar para o cipó, e para agarrado no fim, num limite ou com calma.
        /// </summary>
        private void PassoPresoNaParede(MonitorDoDesktop m, Superficies sup, double escala)
        {
            EstadoDoMovimento mv = _s.Movimento;
            double passo = PorPasso(_cfg.Fisica.VelocidadeEscalando, escala);
            double baixo = Math.Max(sup.Teto, sup.Chao - _cfg.Fisica.AlturaMinimaParaAgarrar * escala);
            double y = mv.Y + mv.SentidoVertical * passo;
            bool noLimite = y <= sup.Teto || y >= baixo;
            y = Math.Clamp(y, sup.Teto, baixo);
            double restante = mv.Restante - passo;
            _s = _s with { Movimento = mv with { Restante = restante } };
            MoverPara(m, Sentido > 0 ? sup.Direita : sup.Esquerda, y);
            if (Calmo || noLimite || restante <= 0) _s = _s with { Movimento = _s.Movimento with { Agarrado = true } };
        }

        /// <summary>
        /// Preso pelo usuário no cipó (DEC-024): percorre o passeio sorteado pela borda de cima; numa
        /// quina dá meia-volta em vez de descer; para agarrado no fim ou com calma.
        /// </summary>
        private void PassoPresoNoCipo(MonitorDoDesktop m, Superficies sup, double escala)
        {
            EstadoDoMovimento mv = _s.Movimento;
            double passo = PorPasso(_cfg.Fisica.VelocidadePendurado, escala);
            double x = mv.X + Sentido * passo;
            int limite = Sentido > 0 ? sup.Direita : sup.Esquerda;
            if (Sentido > 0 ? x >= limite : x <= limite)
            {
                x = limite;
                Virar();
            }
            double restante = mv.Restante - passo;
            _s = _s with { Movimento = mv with { Restante = restante } };
            MoverPara(m, x, sup.Teto);
            if (Calmo || restante <= 0) _s = _s with { Movimento = _s.Movimento with { Agarrado = true } };
        }

        private void PassoPendurado()
        {
            if (!Mundo(out MonitorDoDesktop m, out Superficies sup, out double escala)) return;
            if (_s.Movimento.Agarrado) return;
            if (_s.PresoPeloUsuario)
            {
                PassoPresoNoCipo(m, sup, escala);
                return;
            }
            if (Calmo)
            {
                IrPara(Estado.Falling, "HANGING: autonomia pausada ou painel aberto (solta-se)");
                return;
            }
            EstadoDoMovimento mv = _s.Movimento;
            double x = mv.X + Sentido * PorPasso(_cfg.Fisica.VelocidadePendurado, escala);
            int limite = Sentido > 0 ? sup.Direita : sup.Esquerda;
            bool naBorda = Sentido > 0 ? x >= limite : x <= limite;
            if (naBorda) x = limite;
            MoverPara(m, x, sup.Teto);
            if (naBorda) Sinalizar(SinalDeMovimento.FimDaBorda);
        }

        /// <summary>Pulo ou queda: gravidade com velocidade máxima, integração semi-implícita, até o chão.</summary>
        private void PassoNoAr()
        {
            if (!Mundo(out MonitorDoDesktop m, out Superficies sup, out double escala)) return;
            EstadoDoMovimento mv = _s.Movimento;
            double dt = 1.0 / _cfg.PassosPorSegundo;
            double vy = Math.Min(mv.VY + _cfg.Fisica.Gravidade * escala * dt, _cfg.Fisica.VelocidadeMaximaDeQueda * escala);
            double vx = mv.VX;
            double x = mv.X + vx * dt;
            double y = mv.Y + vy * dt;
            // O sprite nunca sai da área útil: as laterais e a borda de cima param o voo.
            if (x < sup.Esquerda) { x = sup.Esquerda; vx = 0; }
            else if (x > sup.Direita) { x = sup.Direita; vx = 0; }
            if (y < sup.Teto) { y = sup.Teto; if (vy < 0) vy = 0; }
            if (y >= sup.Chao)
            {
                if (Quicar(m, x, sup.Chao, vx, vy, escala)) return;
                _s = _s with { Movimento = mv with { VX = 0, VY = 0, Quiques = 0 } };
                MoverPara(m, x, sup.Chao);
                Sinalizar(SinalDeMovimento.ContatoComOChao);
                return;
            }
            _s = _s with { Movimento = mv with { VX = vx, VY = vy } };
            MoverPara(m, x, y);
        }

        /// <summary>
        /// Toon force (DEC-023): um impacto forte no chão quica como borracha, rindo, em vez de
        /// pousar. A cada quique sobra uma fração da velocidade; depois de
        /// <see cref="ParametrosDeMovimento.QuiquesMaximos"/>, ou com a autonomia pausada ou o
        /// painel aberto, ele pousa. Devolve se quicou.
        /// </summary>
        private bool Quicar(MonitorDoDesktop m, double x, int chao, double vx, double vy, double escala)
        {
            ParametrosDeMovimento f = _cfg.Fisica;
            int quiques = _s.Movimento.Quiques;
            if (Calmo || f.RestituicaoDoQuique <= 0 || quiques >= f.QuiquesMaximos || vy < f.ImpactoMinimoDoQuique * escala) return false;

            string de = _s.Estado.ToString().ToUpperInvariant();
            _s = _s with { Expressao = Expressao.Rindo };
            IrPara(Estado.Jumping, $"{de}: contato com o chão, quique de borracha (toon force)");
            // IrPara recomeça o movimento parado quando o estado muda: a velocidade vem depois.
            _s = _s with { Movimento = _s.Movimento with { VX = vx * f.AtritoDoQuique, VY = -vy * f.RestituicaoDoQuique, Quiques = quiques + 1 } };
            MoverPara(m, x, chao);
            return true;
        }

        /// <summary>
        /// Toon force (DEC-023): ao começar a subir uma parede a partir do chão, às vezes dispara
        /// parede acima num foguete de borracha, até a borda superior. A chance é do perfil de
        /// energia (frequência de uma ação); a velocidade é a mesma em todo nível.
        /// </summary>
        private void TalvezFoguete()
        {
            if (!_cfg.Movimento || _s.Estado != Estado.Climbing || Calmo || _cfg.Fisica.VelocidadeDoFoguete <= 0) return;
            (int sorteio, Aleatorio a) = _s.Aleatorio.Entre(1, 100);
            _s = _s with { Aleatorio = a };
            if (sorteio <= Perfil.ChanceDoFoguete)
                _s = _s with { Movimento = _s.Movimento with { Foguete = true } };
        }

        /// <summary>Caminhada autônoma: distância do perfil de energia; sem espaço à frente, vira.</summary>
        private void PlanejarCaminhada(PerfilDeEnergia perfil)
        {
            if (!Mundo(out _, out Superficies sup, out double escala)) return;
            (int dip, Aleatorio a) = _s.Aleatorio.Entre(perfil.DistanciaAndandoMinima, perfil.DistanciaAndandoMaxima);
            _s = _s with { Aleatorio = a };
            double x = _s.Movimento.X;
            double livre = Sentido > 0 ? sup.Direita - x : x - sup.Esquerda;
            double atras = Sentido > 0 ? x - sup.Esquerda : sup.Direita - x;
            if (livre < _cfg.Fisica.EspacoMinimo * escala && atras > livre) Virar();
            _s = _s with { Movimento = _s.Movimento with { Restante = dip * escala } };
        }

        /// <summary>
        /// Escalar a partir de IDLE: já encostado numa lateral, sobe; senão, anda até a lateral
        /// mais próxima e sobe quando chegar. Com a toon force (DEC-023), as duas laterais servem,
        /// mesmo a que encosta em outro monitor.
        /// </summary>
        private void PlanejarEscalada()
        {
            if (!Mundo(out _, out Superficies sup, out _) || _s.Lugar is null) return;
            double x = _s.Lugar.Ancora.X;
            if (sup.NaLateral(x, out int lado))
            {
                _s = _s with { Direcao = lado > 0 ? Direcao.Direita : Direcao.Esquerda };
                IrPara(Estado.Climbing, "IDLE + AUTONOMY_TIMER: escalar");
                TalvezFoguete();
                return;
            }
            double ateDireita = sup.Direita - x;
            double ateEsquerda = x - sup.Esquerda;
            _s = _s with { Direcao = ateDireita <= ateEsquerda ? Direcao.Direita : Direcao.Esquerda };
            IrPara(Estado.Walking, "IDLE + AUTONOMY_TIMER: escalar (anda até a parede)");
            _s = _s with { Movimento = _s.Movimento with { QuerEscalar = true } };
        }

        /// <summary>
        /// Pulo autônomo: arco balístico com distância e altura do perfil de energia, calculado
        /// para pousar no chão; sem espaço à frente, pula para o outro lado.
        /// </summary>
        private void PlanejarPulo(PerfilDeEnergia perfil)
        {
            if (!Mundo(out _, out Superficies sup, out double escala)) return;
            (int lado, Aleatorio a1) = _s.Aleatorio.Entre(0, 1);
            (int distanciaDip, Aleatorio a2) = a1.Entre(perfil.DistanciaDoPuloMinima, perfil.DistanciaDoPuloMaxima);
            (int alturaDip, Aleatorio a3) = a2.Entre(perfil.AlturaDoPuloMinima, perfil.AlturaDoPuloMaxima);
            _s = _s with { Aleatorio = a3, Direcao = lado == 0 ? Direcao.Direita : Direcao.Esquerda };
            double x = _s.Movimento.X;
            double livre = Sentido > 0 ? sup.Direita - x : x - sup.Esquerda;
            double atras = Sentido > 0 ? x - sup.Esquerda : sup.Direita - x;
            if (livre < distanciaDip * escala && atras > livre)
            {
                Virar();
                livre = atras;
            }
            double distancia = Math.Min(distanciaDip * escala, livre);
            double g = _cfg.Fisica.Gravidade * escala;
            double vy0 = -Math.Sqrt(2 * g * alturaDip * escala);
            double voo = 2 * -vy0 / g;
            double vx = voo > 0 ? Sentido * distancia / voo : 0;
            _s = _s with { Movimento = _s.Movimento with { VX = vx, VY = vy0 } };
        }

        /// <summary>Expressões de quem está preso pelo usuário e só olha em volta (DEC-024).</summary>
        private static readonly Expressao[] ExpressoesDoPreso = [Expressao.Feliz, Expressao.Curioso, Expressao.Travesso, Expressao.Rindo, Expressao.Pensativo];

        /// <summary>
        /// Preso pelo usuário (DEC-024): a agenda nunca o tira de lá. Ou ele fica, trocando de cara,
        /// ou passeia um pouco pela mesma superfície: sobe ou desce pela parede, vai para um lado ou
        /// outro pelo cipó. O passeio para agarrado, e o relógio desliga.
        /// </summary>
        private void DecidirPreso()
        {
            bool naParede = _s.Estado == Estado.Climbing;
            (int escolha, Aleatorio a) = _s.Aleatorio.Ponderado([2, 2, 2]);
            (int dip, Aleatorio a2) = a.Entre(_cfg.Fisica.PasseioPresoMinimo, _cfg.Fisica.PasseioPresoMaximo);
            (int cara, Aleatorio a3) = a2.Entre(0, ExpressoesDoPreso.Length - 1);
            _s = _s with { Aleatorio = a3 };
            string onde = naParede ? "CLIMBING preso pelo usuário" : "HANGING preso pelo usuário no cipó";
            if (escolha == 0)
            {
                _s = _s with { Expressao = ExpressoesDoPreso[cara] };
                _transicoes.Add(new Transicao(_s.Estado, _s.Estado, $"{onde} + AUTONOMY_TIMER: fica e olha em volta"));
                return;
            }
            double escala = Mundo(out _, out _, out double e) ? e : 1;
            EstadoDoMovimento passeio = _s.Movimento with { Agarrado = false, Restante = dip * escala };
            if (naParede)
                _s = _s with { Movimento = passeio with { SentidoVertical = escolha == 1 ? -1 : 1 } };
            else
                _s = _s with { Movimento = passeio, Direcao = escolha == 1 ? Direcao.Direita : Direcao.Esquerda };
            _transicoes.Add(new Transicao(_s.Estado, _s.Estado, naParede
                ? $"{onde} + AUTONOMY_TIMER: passeia pela parede, para {(escolha == 1 ? "cima" : "baixo")}"
                : $"{onde} + AUTONOMY_TIMER: passeia pela borda, para a {(escolha == 1 ? "direita" : "esquerda")}"));
        }

        /// <summary>A agenda decidiu pular da parede: salta para longe dela, de costas para a parede.</summary>
        private void SaltarDaParede()
        {
            if (!Mundo(out _, out _, out double escala)) return;
            Virar();
            double g = _cfg.Fisica.Gravidade * escala;
            _s = _s with
            {
                Movimento = _s.Movimento with
                {
                    VX = Sentido * _cfg.Fisica.ImpulsoDaParede * escala,
                    VY = -Math.Sqrt(2 * g * _cfg.Fisica.AlturaDoPuloDaParede * escala),
                },
            };
        }

        /// <summary>A agenda decidiu pendurado: continua pela borda, desce pela parede da quina, salta ou solta.</summary>
        private void SairDoTeto()
        {
            if (!Mundo(out _, out Superficies sup, out double escala)) return;
            switch (_s.Estado)
            {
                case Estado.Climbing when sup.NaLateral(_s.Movimento.X, out int lado):
                    _s = _s with { Direcao = lado > 0 ? Direcao.Direita : Direcao.Esquerda, Movimento = _s.Movimento with { SentidoVertical = 1 } };
                    break;
                case Estado.Jumping:
                    _s = _s with { Movimento = _s.Movimento with { VX = Sentido * _cfg.Fisica.ImpulsoDaParede * escala, VY = 0 } };
                    break;
            }
        }

        // ---------------------------------------------------------------- validação (SETTLING)

        /// <summary>
        /// <c>SETTLING</c> (ARCHITECTURE.md 2.7, passos 4 e 5): escolhe o monitor da âncora (ou o mais
        /// próximo, num vão), prende o sprite na área útil dele e decide pelo apoio. Sem apoio, cai
        /// (<see cref="ConfiguracaoDoNucleo.QuedaFisica"/>) ou, antes da Fase 4, vai direto ao chão.
        /// </summary>
        /// <param name="pelaMaoDoUsuario">
        /// O usuário acabou de soltar o personagem (DRAG_END, DRAG_CANCEL do arraste). Se ele agarrar
        /// uma lateral ou o cipó, fica preso lá até o usuário tirá-lo (DEC-024).
        /// </param>
        private void Acomodar(PontoPx desejada, string regra, PosicaoDoPersonagem? preferida = null, bool pelaMaoDoUsuario = false)
        {
            IrPara(Estado.Settling, regra);
            (Posicionamento lugar, PosicaoDoPersonagem posicao, bool comApoio) = Validar(desejada, preferida);
            _s = _s with { Lugar = lugar, Posicao = posicao };
            _reagendar = true;

            // Escondido (DEC-025): toda acomodação o devolve ao esconderijo, na mesma borda.
            if (_s.Esconderijo != LadoDoEsconderijo.Nenhum)
            {
                Posicionamento escondido = EsconderijoPara(lugar, _s.Esconderijo);
                _s = _s with { Lugar = escondido, Posicao = Posicionador.Descrever(escondido) };
                IrPara(Estado.Peeking, $"SETTLING: escondido atrás da borda ({_s.Esconderijo})");
                return;
            }

            // Solto no alto ou junto a uma lateral, agarra ali em vez de cair (DEC-024). Quem já
            // estava preso pelo usuário continua preso: um clique ou uma revalidação não o tiram.
            if (!comApoio && _cfg.Movimento && OndeAgarrar(lugar) is { } agarre)
            {
                bool preso = pelaMaoDoUsuario || _s.PresoPeloUsuario;
                _s = _s with { Lugar = agarre.Lugar, Posicao = Posicionador.Descrever(agarre.Lugar), Direcao = agarre.Direcao, PresoPeloUsuario = preso };
                IrPara(agarre.Estado, agarre.Estado == Estado.Hanging
                    ? "SETTLING: solto perto da borda de cima, agarra o cipó"
                    : "SETTLING: solto junto a uma lateral, fica grudado na parede");
                // IrPara recomeça o movimento quando o estado muda: parado, agarrado, sem relógio.
                _s = _s with { Movimento = _s.Movimento with { Agarrado = true } };
                return;
            }

            _s = _s with { PresoPeloUsuario = false };
            if (comApoio)
                IrPara(Estado.Idle, "SETTLING com apoio");
            else if (_cfg.QuedaFisica)
                IrPara(Estado.Falling, "SETTLING sem apoio");
            else
                IrPara(Estado.Idle, "SETTLING sem apoio: preso no chão (a queda animada é da Fase 4)");
        }

        /// <summary>
        /// Onde agarrar um personagem sem apoio (DEC-024): o cipó da borda de cima, se o topo do sprite
        /// está perto dela; a lateral, se a âncora está perto dela; a mais próxima das duas, em
        /// proporção ao alcance de cada uma. Perto do chão, nenhuma: ele cai.
        /// </summary>
        private (Estado Estado, Posicionamento Lugar, Direcao Direcao)? OndeAgarrar(Posicionamento lugar)
        {
            if (_s.Topologia is not { } topologia) return null;
            MonitorDoDesktop m = lugar.Monitor;
            Superficies sup = Superficies.Do(topologia, m, lugar.Tamanho);
            double escala = m.Dpi / 96.0;
            ParametrosDeMovimento f = _cfg.Fisica;
            PontoPx a = lugar.Ancora;
            if (sup.Chao - a.Y < f.AlturaMinimaParaAgarrar * escala) return null;

            double paraOCipo = (a.Y - sup.Teto) / (f.DistanciaParaOCipo * escala);
            double paraAParede = Math.Min(a.X - sup.Esquerda, sup.Direita - a.X) / (f.DistanciaParaAParede * escala);
            bool cipo = paraOCipo <= 1, parede = paraAParede <= 1;
            if (!cipo && !parede) return null;

            if (cipo && (!parede || paraOCipo <= paraAParede))
            {
                var ancora = new PontoPx(Math.Clamp(a.X, sup.Esquerda, sup.Direita), sup.Teto);
                return (Estado.Hanging, NoLugar(m, ancora), _s.Direcao);
            }
            bool direita = sup.Direita - a.X <= a.X - sup.Esquerda;
            var naParede = new PontoPx(direita ? sup.Direita : sup.Esquerda, Math.Clamp(a.Y, sup.Teto, sup.Chao));
            return (Estado.Climbing, NoLugar(m, naParede), direita ? Direcao.Direita : Direcao.Esquerda);
        }

        private Posicionamento NoLugar(MonitorDoDesktop monitor, PontoPx ancora)
        {
            TamanhoPx tamanho = _cfg.Tamanho.ParaPixels(monitor.Dpi);
            return new Posicionamento(monitor, ancora, tamanho, Posicionador.RetanguloDoSprite(ancora, tamanho));
        }

        /// <summary>
        /// A validação de SETTLING sem transição: monitor da âncora (ou o mais próximo), sprite
        /// preso na área útil, apoio no chão e posição relativa resultante.
        /// </summary>
        private (Posicionamento Lugar, PosicaoDoPersonagem Posicao, bool ComApoio) Validar(PontoPx desejada, PosicaoDoPersonagem? preferida)
        {
            Topologia topologia = _s.Topologia ?? throw new InvalidOperationException("Validação sem topologia.");
            MonitorDoDesktop monitor = MonitorDaAncora(topologia, desejada);
            TamanhoPx tamanho = _cfg.Tamanho.ParaPixels(monitor.Dpi);
            PontoPx presa = Posicionador.PrenderNaAreaUtil(desejada, tamanho, monitor.AreaUtil);
            bool comApoio = presa.Y == monitor.AreaUtil.Base;
            if (!comApoio && !_cfg.QuedaFisica) presa = presa with { Y = monitor.AreaUtil.Base };

            var lugar = new Posicionamento(monitor, presa, tamanho, Posicionador.RetanguloDoSprite(presa, tamanho));
            PosicaoDoPersonagem posicao = preferida is not null && presa == desejada && preferida.ChaveMonitor == monitor.Chave
                ? preferida with { AncoraAbsoluta = presa, TelaDoMonitor = monitor.Tela }
                : Posicionador.Descrever(lugar);
            return (lugar, posicao, comApoio);
        }

        /// <summary>
        /// Esconder ou sair no meio do arraste encerra o gesto onde ele está, como um cancelamento
        /// (ARCHITECTURE.md 2.7): a posição do cursor é validada e passa a ser a escolha do usuário.
        /// </summary>
        private void FixarArrasteInterrompido()
        {
            if (_s.Estado != Estado.Dragging || _s.Lugar is null || _s.Topologia is null) return;
            (Posicionamento lugar, PosicaoDoPersonagem posicao, _) = Validar(_s.Lugar.Ancora, null);
            _s = _s with { Lugar = lugar, Posicao = posicao, RetornoDaTelaCheia = null };
        }

        /// <summary>Posicionamento durante o arraste: segue a âncora sem prender, com o tamanho do DPI do monitor dela.</summary>
        private Posicionamento LugarLivre(Topologia topologia, PontoPx ancora)
        {
            MonitorDoDesktop monitor = MonitorDaAncora(topologia, ancora);
            TamanhoPx tamanho = _cfg.Tamanho.ParaPixels(monitor.Dpi);
            return new Posicionamento(monitor, ancora, tamanho, Posicionador.RetanguloDoSprite(ancora, tamanho));
        }

        private void IrPara(Estado novo, string regra)
        {
            Estado de = _s.Estado;
            _transicoes.Add(new Transicao(de, novo, regra));
            _s = _s with { Estado = novo, Motivo = novo == Estado.Hidden ? _s.Motivo : MotivoDoOcultamento.Nenhum };
            if (novo != de && DecideNoEstado(novo)) _reagendar = true;
            // Ao entrar num estado de movimento, a física parte da âncora atual, parada; quem
            // chamou ajusta velocidade e plano depois (DEC-022).
            if (novo != de && novo.EmMovimento() && _s.Lugar is { } lugar)
                _s = _s with { Movimento = new EstadoDoMovimento(lugar.Ancora.X, lugar.Ancora.Y, 0, 0, double.PositiveInfinity, -1, false) };
        }

        // ---------------------------------------------------------------- efeitos

        internal Resultado Concluir()
        {
            var janela = new List<Efeito>();
            var tempo = new List<Efeito>();

            bool visivelAntes = _inicio.Estado.Visivel();
            bool visivelDepois = _s.Estado.Visivel();
            if (_s.Estado != Estado.Exiting)
            {
                if (visivelDepois && _s.Lugar is not null && (!visivelAntes || !Equals(_inicio.Lugar, _s.Lugar)))
                    janela.Add(new MoverJanela(_s.Lugar));
                if (!visivelAntes && visivelDepois) janela.Add(new MostrarJanela());
                if (visivelAntes && !visivelDepois) janela.Add(new EsconderJanela());
            }

            // Relógio: só com movimento, reação, pouso ou gesto (DEC-011; critério 3 da Fase 2).
            // Agarrado à parede ou ao cipó (DEC-024), nada se move: o relógio fica desligado.
            bool agarrado = _s.Estado is Estado.Climbing or Estado.Hanging && _s.Movimento.Agarrado;
            bool relogio = (_s.Estado.EmMovimento() && !agarrado) || _s.Estado == Estado.Reacting || (_s.Estado == Estado.Idle && _s.Gesto != Gesto.Nenhum);
            if (relogio != _s.RelogioAtivo)
            {
                tempo.Add(relogio ? new LigarRelogio() : new DesligarRelogio());
                _s = _s with { RelogioAtivo = relogio };
            }

            // Agenda autônoma: um temporizador único até a próxima decisão.
            bool querDecisao = DecideNoEstado(_s.Estado) && _s.Gesto == Gesto.Nenhum && !_s.AutonomiaPausada && !_s.PainelAberto;
            if (querDecisao && (_reagendar || !_s.DecisaoAgendada))
            {
                TimeSpan atraso = SortearAtraso();
                long geracao = _s.Geracao + 1;
                tempo.Add(new AgendarDecisao(atraso, geracao));
                _s = _s with { Geracao = geracao, DecisaoAgendada = true };
            }
            else if (!querDecisao && _s.DecisaoAgendada)
            {
                tempo.Add(new CancelarDecisao());
                _s = _s with { DecisaoAgendada = false };
            }

            var efeitos = new List<Efeito>(_antes.Count + janela.Count + tempo.Count + _depois.Count);
            efeitos.AddRange(_antes);
            efeitos.AddRange(janela);
            efeitos.AddRange(tempo);
            efeitos.AddRange(_depois);
            return new Resultado(_s, efeitos, _transicoes);
        }

        private TimeSpan SortearAtraso()
        {
            PerfilDeEnergia perfil = Perfil;
            (TimeSpan minimo, TimeSpan maximo) = _s.Estado switch
            {
                Estado.Resting => (perfil.DescansoMinimo, perfil.DescansoMaximo),
                // Fase 4: tempo na parede e tempo pendurado ("por pouco tempo") do perfil de energia.
                Estado.Climbing when _cfg.Movimento => (perfil.TempoNaParedeMinimo, perfil.TempoNaParedeMaximo),
                Estado.Hanging when _cfg.Movimento => (perfil.TempoPenduradoMinimo, perfil.TempoPenduradoMaximo),
                _ => (perfil.DecisaoMinima, perfil.DecisaoMaxima),
            };
            (TimeSpan atraso, Aleatorio a) = _s.Aleatorio.Duracao(minimo, maximo);
            _s = _s with { Aleatorio = a };
            // Depois de uma interação (soltar, fechar o painel, retomar a autonomia, em qualquer
            // estado que decide), a autonomia só volta após o intervalo de acomodação; é também o
            // menor intervalo entre duas decisões autônomas.
            return atraso < _cfg.IntervaloDeAcomodacao ? _cfg.IntervaloDeAcomodacao : atraso;
        }
    }
}

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
    /// Monitor em que está a âncora: o que contém o pixel logo acima dela. A âncora fica na borda
    /// inferior exclusiva do sprite (Posicionador), que numa pilha de monitores já é o primeiro
    /// pixel do monitor de baixo.
    /// </summary>
    public static MonitorDoDesktop MonitorDaAncora(Topologia topologia, PontoPx ancora)
    {
        ArgumentNullException.ThrowIfNull(topologia);
        return topologia.MonitorMaisProximo(new PontoPx(ancora.X, ancora.Y - 1));
    }

    /// <summary>Estados em que a agenda autônoma mantém um temporizador pendente.</summary>
    public static bool DecideNoEstado(Estado estado)
        => estado is Estado.Idle or Estado.Resting or Estado.Climbing or Estado.Hanging;

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

            Posicionamento lugar;
            PosicaoDoPersonagem posicao;
            if (e.PosicaoSalva is { } salva)
            {
                (lugar, posicao) = Posicionador.Reacomodar(e.Topologia, salva, _cfg.Tamanho);
            }
            else
            {
                lugar = Posicionador.Inicial(e.Topologia, _cfg.Tamanho);
                posicao = Posicionador.Descrever(lugar);
            }
            _s = _s with { Carregado = true, Topologia = e.Topologia, Preferencias = Sanear(e.Preferencias), Lugar = lugar, Posicao = posicao };

            if (_s.Estado == Estado.Booting)
                Acomodar(lugar.Ancora, "BOOTING: configurações e topologia carregadas", posicao);
            else if (_s.Motivo == MotivoDoOcultamento.Nenhum)
                Acomodar(lugar.Ancora, "HIDDEN: pedido de mostrar anterior à carga", posicao);

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
        /// temporário se a tela cheia mudou durante o gesto (o ponto do usuário vale).
        /// </summary>
        private void FimDoGestoDoUsuario(bool escolheuPosicao)
        {
            if (escolheuPosicao || _s.TelaCheiaMudouNoGesto) _s = _s with { RetornoDaTelaCheia = null };
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

        private void IniciarArraste()
        {
            if (_s.Estado != Estado.Pressed) return;
            // Invariante 9: iniciar um arraste fecha o painel de energia.
            FecharPainelSeAberto();
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
            Acomodar(ancora, "DRAG_END");
            if (_s.Posicao is not null) _depois.Add(new GravarPosicao(_s.Posicao));
        }

        private void CancelarArraste()
        {
            if (_s.Lugar is null) return;
            if (_s.Estado == Estado.Dragging)
            {
                // O personagem fica onde estava; não volta ao ponto de origem (ARCHITECTURE.md 2.7).
                FimDoGestoDoUsuario(escolheuPosicao: true);
                Acomodar(_s.Lugar.Ancora, "DRAG_CANCEL");
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

            // Desligar o modo desfaz o efeito temporário dele (Q-09).
            if (_s.Estado == Estado.Hidden && _s.Motivo == MotivoDoOcultamento.PorTelaCheia)
                RestaurarRetorno("SETTINGS_CHANGED: modo de tela cheia desligado");
            else if (_s.Estado.Visivel() && _s.Estado is not (Estado.Pressed or Estado.Dragging) && _s.RetornoDaTelaCheia is not null)
                RestaurarRetorno("SETTINGS_CHANGED: modo de tela cheia desligado");
            else
                _s = _s with { RetornoDaTelaCheia = null };
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
                // Walking, Climbing, Hanging, Jumping e Falling: o passo físico é da Fase 4.
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
                    Escolher("HANGING: passagem compatível ou fim da borda", calmo,
                        (Estado.Climbing, 2),
                        (Estado.Hanging, 1),
                        (Estado.Falling, 1));
                    if (_s.Estado == Estado.Hanging) Virar();
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
                case Estado.Climbing:
                    Escolher("CLIMBING + AUTONOMY_TIMER", calmo: false,
                        (Estado.Jumping, Permite(AcoesAutonomas.Pular) ? Perfil.PesoPular : 0),
                        (Estado.Falling, 1));
                    break;
                case Estado.Hanging:
                    Escolher("HANGING + AUTONOMY_TIMER", calmo: false,
                        (Estado.Hanging, 2),
                        (Estado.Climbing, 2),
                        (Estado.Jumping, Permite(AcoesAutonomas.Pular) ? Perfil.PesoPular : 0),
                        (Estado.Falling, 1));
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
            Opcao(AcoesAutonomas.Andar, perfil.PesoAndar);
            Opcao(AcoesAutonomas.Escalar, perfil.PesoEscalar);
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
                    break;
                case AcoesAutonomas.Escalar:
                    IrPara(Estado.Climbing, "IDLE + AUTONOMY_TIMER: escalar");
                    break;
                case AcoesAutonomas.Pular:
                    IrPara(Estado.Jumping, "IDLE + AUTONOMY_TIMER: pular");
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

        // ---------------------------------------------------------------- validação (SETTLING)

        /// <summary>
        /// <c>SETTLING</c> (ARCHITECTURE.md 2.7, passos 4 e 5): escolhe o monitor da âncora (ou o mais
        /// próximo, num vão), prende o sprite na área útil dele e decide pelo apoio. Sem apoio, cai
        /// (<see cref="ConfiguracaoDoNucleo.QuedaFisica"/>) ou, antes da Fase 4, vai direto ao chão.
        /// </summary>
        private void Acomodar(PontoPx desejada, string regra, PosicaoDoPersonagem? preferida = null)
        {
            IrPara(Estado.Settling, regra);
            (Posicionamento lugar, PosicaoDoPersonagem posicao, bool comApoio) = Validar(desejada, preferida);
            _s = _s with { Lugar = lugar, Posicao = posicao };
            _reagendar = true;

            if (comApoio)
                IrPara(Estado.Idle, "SETTLING com apoio");
            else if (_cfg.QuedaFisica)
                IrPara(Estado.Falling, "SETTLING sem apoio");
            else
                IrPara(Estado.Idle, "SETTLING sem apoio: preso no chão (a queda animada é da Fase 4)");
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
                ? preferida with { AncoraAbsoluta = presa }
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
            bool relogio = _s.Estado.EmMovimento() || _s.Estado == Estado.Reacting || (_s.Estado == Estado.Idle && _s.Gesto != Gesto.Nenhum);
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
            (TimeSpan atraso, Aleatorio a) = _s.Estado == Estado.Resting
                ? _s.Aleatorio.Duracao(perfil.DescansoMinimo, perfil.DescansoMaximo)
                : _s.Aleatorio.Duracao(perfil.DecisaoMinima, perfil.DecisaoMaxima);
            _s = _s with { Aleatorio = a };
            // Depois de uma interação (soltar, fechar o painel, retomar a autonomia, em qualquer
            // estado que decide), a autonomia só volta após o intervalo de acomodação; é também o
            // menor intervalo entre duas decisões autônomas.
            return atraso < _cfg.IntervaloDeAcomodacao ? _cfg.IntervaloDeAcomodacao : atraso;
        }
    }
}

using System.Globalization;
using System.IO;
using Buzzy.App.Plataforma;
using Buzzy.Core;
using Buzzy.Core.Persistencia;
using Buzzy.Core.Personagem;
using Buzzy.Testes;

namespace Buzzy.App.Testes.Integracao;

/// <summary>
/// A persistência ligada no Buzzy.exe de verdade (Fase 5, passo P7; DEC-029): soltar, fechar e reabrir no mesmo lugar
/// (S2 e S7 nesta máquina, com o secundário à esquerda, em x negativo, e o principal à direita dele), a postura (preso na
/// parede, escondido na lateral) e a emoção dominante voltando ao reabrir, e a partida com um arquivo ilegível. Os gestos
/// são mensagens POSTADAS às janelas do próprio Buzzy, com o PID conferido antes de cada uma: nada passa pela fila de
/// input do Windows nem por outro aplicativo. Tudo no perfil de teste <c>persistencia</c>
/// (<c>%LOCALAPPDATA%\Buzzy\testes\persistencia</c>), lido e escrito pelo teste só lá; os arquivos reais do usuário, na
/// pasta do Buzzy, são conferidos só por fora (existência, tamanho e datas), antes e depois de cada teste, e nunca lidos.
/// Só rodam com --integracao, depois de avisar o usuário: o Buzzy aparece e some na tela.
/// </summary>
[Integracao]
internal sealed class PersistenciaIntegracaoTestes
{
    private const string Perfil = "persistencia";

    private static readonly TamanhoDip Sprite = new(128, 128);

    // ------------------------------------------------------------------ apoio

    private static RetanguloPx SpriteEm(PontoPx ancora, TamanhoPx tamanho)
    {
        int esquerda = ancora.X - tamanho.Largura / 2;
        return new RetanguloPx(esquerda, ancora.Y - tamanho.Altura, esquerda + tamanho.Largura, ancora.Y);
    }

    private static PontoPx Ancora(RetanguloPx janela) => new(janela.Esquerda + janela.Largura / 2, janela.Base);

    private static EventoDoLog EsperarDesde(BuzzyEmTeste b, long marca, Func<EventoDoLog, bool> condicao, int limiteMs, string oQue)
    {
        var fim = DateTime.UtcNow.AddMilliseconds(limiteMs);
        while (true)
        {
            EventoDoLog? achado = BuzzyEmTeste.EventosDesde(marca).FirstOrDefault(condicao);
            if (achado is not null) return achado;
            if (b.Processo.HasExited) throw new InvalidOperationException($"O Buzzy encerrou antes de registrar: {oQue}.");
            if (DateTime.UtcNow > fim) throw new TimeoutException($"Tempo esgotado esperando no log: {oQue}.");
            Thread.Sleep(50);
        }
    }

    private static EventoDoLog Nucleo(BuzzyEmTeste b, long marca, string evento, string de, string para, int limiteMs, string oQue)
        => EsperarDesde(b, marca, e => e.Chave == "NUCLEO" && e["evento"] == evento && (de.Length == 0 || e["de"] == de) && e["para"] == para, limiteMs, oQue);

    /// <summary>A pasta do perfil de teste destes testes, em <c>%LOCALAPPDATA%\Buzzy\testes</c>.</summary>
    private static string PastaDoPerfil() => Afirmar.NaoNulo(PastaDeDados.DoPerfilDeTeste(Perfil), "pasta do perfil de teste");

    /// <summary>O settings.json do perfil de teste, lido pelo esquema.</summary>
    private static LeituraDasConfiguracoes LerDoPerfil()
    {
        string principal = Path.Combine(PastaDoPerfil(), ArquivoDeConfiguracoes.NomePrincipal);
        Afirmar.Verdadeiro(File.Exists(principal), "o Buzzy gravou o settings.json do perfil de teste");
        return EsquemaDeConfiguracoes.Ler(File.ReadAllBytes(principal));
    }

    /// <summary>
    /// O que se vê de fora dos arquivos REAIS do usuário, na pasta do Buzzy: o principal, a reserva, o temporário e a cópia
    /// de diagnóstico, cada um ausente ou com o tamanho e as datas de criação e de escrita. Só metadados da pasta: o
    /// conteúdo nunca é aberto nem lido.
    /// </summary>
    private static string FotoDosArquivosReais()
    {
        string buzzy = Afirmar.NaoNulo(PastaDeDados.DoBuzzy(), "pasta do Buzzy");
        return string.Join("; ", ArquivoDeConfiguracoes.Nomes.Select(nome =>
        {
            var info = new FileInfo(Path.Combine(buzzy, nome));
            return info.Exists
                ? string.Create(CultureInfo.InvariantCulture, $"{nome}: {info.Length} bytes, criado {info.CreationTimeUtc:O}, escrito {info.LastWriteTimeUtc:O}")
                : $"{nome}: ausente";
        }));
    }

    /// <summary>Roda o cenário e confere que os arquivos reais do usuário ficaram como estavam, por fora.</summary>
    private static void SemTocarNosArquivosReais(Action cenario)
    {
        string antes = FotoDosArquivosReais();
        cenario();
        Afirmar.Igual(antes, FotoDosArquivosReais(), "os arquivos reais do usuário, em %LOCALAPPDATA%\\Buzzy, continuam como estavam");
    }

    /// <summary>
    /// Arrasta o personagem por mensagens postadas à janela dele: pressiona no corpo, passa do limiar e leva a âncora a
    /// <paramref name="ancora"/>, conferindo a janela a cada movimento, e solta lá. Devolve a marca do log de antes do
    /// arraste e o DRAG_END que entrou no núcleo.
    /// </summary>
    private static (long Marca, EventoDoLog Soltou) Arrastar(BuzzyEmTeste b, PontoPx ancora, TamanhoPx tamanhoNoDestino)
    {
        RetanguloPx inicio = b.RetanguloDaJanela();
        PontoPx ancoraInicial = Ancora(inicio);
        var pegar = new PontoPx(ancoraInicial.X, ancoraInicial.Y - inicio.Altura / 3);
        var pegada = new PontoPx(pegar.X - ancoraInicial.X, pegar.Y - ancoraInicial.Y);
        long marca = BuzzyEmTeste.MarcaDoLog();

        b.PostarMouse(NativoTeste.WM_LBUTTONDOWN, NativoTeste.MK_LBUTTON, pegar);
        Nucleo(b, marca, "Press", "", "Pressed", 3000, "PRESS no corpo");
        var meio = new PontoPx(pegar.X - 60, pegar.Y - 60);
        b.PostarMouse(NativoTeste.WM_MOUSEMOVE, NativoTeste.MK_LBUTTON, meio);
        Nucleo(b, marca, "DragStart", "Pressed", "Dragging", 3000, "DRAG_START");
        b.EsperarRetangulo(SpriteEm(new PontoPx(meio.X - pegada.X, meio.Y - pegada.Y), inicio.Tamanho), 3000, "a janela no cursor menos a pegada");

        var cursor = new PontoPx(ancora.X + pegada.X, ancora.Y + pegada.Y);
        b.PostarMouse(NativoTeste.WM_MOUSEMOVE, NativoTeste.MK_LBUTTON, cursor);
        b.EsperarRetangulo(SpriteEm(ancora, tamanhoNoDestino), 3000, "a janela no destino do arraste");
        b.PostarMouse(NativoTeste.WM_LBUTTONUP, 0, cursor);
        EventoDoLog soltou = Nucleo(b, marca, "DragEnd", "Dragging", "Settling", 3000, "DRAG_END");
        return (marca, soltou);
    }

    /// <summary>O clique duplo por mensagens postadas no ponto dado (botão pressionado, solto, pressionado, solto).</summary>
    private static void CliqueDuplo(BuzzyEmTeste b, PontoPx ponto)
    {
        foreach (int msg in new[] { NativoTeste.WM_LBUTTONDOWN, NativoTeste.WM_LBUTTONUP, NativoTeste.WM_LBUTTONDOWN, NativoTeste.WM_LBUTTONUP })
            b.PostarMouse(msg, msg == NativoTeste.WM_LBUTTONDOWN ? NativoTeste.MK_LBUTTON : 0, ponto);
    }

    /// <summary>Fecha pelo WM_CLOSE (CMD_EXIT no núcleo) e devolve as linhas CONFIG de gravação da saída.</summary>
    private static List<EventoDoLog> FecharEGravar(BuzzyEmTeste b)
    {
        long marca = BuzzyEmTeste.MarcaDoLog();
        Afirmar.Igual(0, b.FecharPorWmClose(), "código de saída");
        return [.. BuzzyEmTeste.EventosDesde(marca).Where(e => e.Chave == "CONFIG" && e.Campos.ContainsKey("gravado"))];
    }

    /// <summary>Espera a janela ficar no lugar e não sair dele por <paramref name="duracaoMs"/>, sem o relógio ligar.</summary>
    private static void FicaParado(BuzzyEmTeste b, RetanguloPx lugar, int duracaoMs, string oQue)
    {
        b.EsperarRetangulo(lugar, 3000, oQue);
        long marca = BuzzyEmTeste.MarcaDoLog();
        var fim = DateTime.UtcNow.AddMilliseconds(duracaoMs);
        while (DateTime.UtcNow < fim)
        {
            Afirmar.Igual(lugar, b.RetanguloDaJanela(), $"{oQue}: não sai do lugar");
            Thread.Sleep(50);
        }
        Afirmar.Falso(BuzzyEmTeste.EventosDesde(marca).Any(e => e.Chave == "RELOGIO" && e["ligado"] == "sim"), $"{oQue}: o relógio não ligou");
    }

    // ------------------------------------------------------------------ cenários

    // S2 e S7 (TODO.md, Fase 5): o personagem solto a 25% da área útil do monitor em x negativo (o secundário à esquerda;
    // numa máquina com um monitor só, o principal), no chão. A gravação com atraso cai de 1,9 a 4 s depois do soltar; a
    // saída não grava de novo o que já está no disco; o arquivo do perfil é a v3, com a chave estável do monitor e a tela
    // dele; e o Buzzy reaberto lê o principal e volta ao mesmo retângulo, restaurado pela chave.
    [Teste]
    public void SoltarNoMonitorEmXNegativo_FecharEReabrir_VoltaAoMesmoLugar()
    {
        SemTocarNosArquivosReais(() =>
        {
            LeituraDaTopologia leitura = Afirmar.NaoNulo(LeitorDeTopologia.LerDetalhado(out string? erro), erro);
            Topologia t = leitura.Topologia;
            MonitorDoDesktop alvo = t.Monitores.FirstOrDefault(m => m.Tela.Esquerda < 0) ?? t.Principal;
            Console.WriteLine($"         destino: {(alvo.Principal ? "o principal (nenhum monitor em x negativo)" : $"{leitura.NomeGdi(alvo.Chave)}, em x negativo, com o principal à direita (S2 e S7)")}");
            TamanhoPx tamanho = Sprite.ParaPixels(alvo.Dpi);
            var ancora = new PontoPx(alvo.AreaUtil.Esquerda + alvo.AreaUtil.Largura / 4, alvo.AreaUtil.Base);
            RetanguloPx esperado = SpriteEm(ancora, tamanho);

            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil))
            {
                EventoDoLog partida = b.Esperar(e => e.Chave == "CONFIG" && e.Campos.ContainsKey("lido"), 5000, "a leitura das configurações na partida");
                Afirmar.Igual(("padroes", "Ausente", "perfil"), (partida["lido"], partida["principal"], partida["pasta"]), "perfil limpo: os padrões, sem arquivo");
                b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Idle", 5000, "núcleo carregado");

                (long marca, EventoDoLog soltou) = Arrastar(b, ancora, tamanho);
                Nucleo(b, marca, "DragEnd", "Settling", "Idle", 3000, "no chão do destino");
                b.EsperarRetangulo(esperado, 3000, "no chão, a 25% da área útil do destino");
                EventoDoLog gravado = EsperarDesde(b, marca, e => e.Chave == "CONFIG" && e["gravado"] == "sim" && e["motivo"] == "atraso", 8000, "a gravação com atraso");
                TimeSpan atraso = gravado.Instante - soltou.Instante;
                Console.WriteLine($"         gravação {atraso.TotalMilliseconds:0} ms depois do soltar, em {gravado["ms"]} ms, {gravado["bytes"]} bytes");
                Afirmar.Verdadeiro(atraso >= TimeSpan.FromSeconds(1.9) && atraso <= TimeSpan.FromSeconds(4), $"a gravação cai de 1,9 a 4 s depois do soltar: {atraso.TotalMilliseconds:0} ms");
                Afirmar.Igual("Ausente", gravado["principalAntes"], "a primeira gravação cria o principal");
                EventoDoLog posicao = BuzzyEmTeste.EventosDesde(marca).Last(e => e.Chave == "POSICAO");
                Afirmar.Igual(alvo.Chave, posicao["monitor"], "solto no monitor de destino, pela chave estável");

                List<EventoDoLog> saida = FecharEGravar(b);
                Afirmar.Sequencia(["sem mudanca/CmdExit"], saida.Select(e => $"{e["gravado"]}/{e["motivo"]}"), "a saída não regrava o que já está no disco");
            }

            LeituraDasConfiguracoes lida = LerDoPerfil();
            Afirmar.Igual((SituacaoDaLeitura.Valida, (int?)3, 0), (lida.Situacao, lida.Versao, lida.Avisos.Count), "o arquivo do perfil é a v3, sem aviso");
            PosicaoDoPersonagem salva = Afirmar.NaoNulo(lida.Configuracoes.Posicao, "posição salva");
            Afirmar.Igual((alvo.Chave, (RetanguloPx?)alvo.Tela), (salva.ChaveMonitor, salva.TelaDoMonitor), "a chave estável e a tela do monitor");
            Afirmar.Igual((LadoDoEsconderijo.Nenhum, false), (lida.Configuracoes.Esconderijo, lida.Configuracoes.PresoPeloUsuario), "no chão, sem esconderijo e solto");

            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil, limpar: false))
            {
                EventoDoLog partida = b.Esperar(e => e.Chave == "CONFIG" && e.Campos.ContainsKey("lido"), 5000, "a leitura das configurações na partida");
                Afirmar.Igual(("principal", "Valido", "3"), (partida["lido"], partida["principal"], partida["versao"]), "reaberto, lê o principal");
                EventoDoLog carga = b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["de"] == "Booting", 5000, "a carga");
                Afirmar.Igual("BOOTING: configurações e topologia carregadas; posição salva restaurada pela chave", carga["regra"], "restaurada pela chave");
                b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["de"] == "Settling" && e["para"] == "Idle", 5000, "de pé no chão");
                b.EsperarRetangulo(esperado, 3000, "no mesmo retângulo em que foi solto");
                Afirmar.Sequencia(["sem mudanca/CmdExit"], FecharEGravar(b).Select(e => $"{e["gravado"]}/{e["motivo"]}"), "nada mudou: nada regravado");
            }
        });
    }

    // A postura gravada com a posição (esquema v3; DEC-029, item 11): solto junto à lateral direita do principal, ele fica
    // preso na parede; fechado e reaberto, volta grudado no mesmo lugar e, pausado, não desce (quem não estivesse preso
    // desceria, pela regra da calma). Escondido atrás dessa lateral pelo clique duplo, fechado e reaberto, volta escondido
    // na mesma borda, no mesmo lugar.
    [Teste]
    public void PresoNaParedeEEscondidoNaLateral_FecharEReabrir_VoltamComoEstavam()
    {
        SemTocarNosArquivosReais(() =>
        {
            Topologia t = Afirmar.NaoNulo(LeitorDeTopologia.Ler(out string? erro), erro);
            MonitorDoDesktop principal = t.Principal;
            TamanhoPx tamanho = Sprite.ParaPixels(principal.Dpi);
            Superficies sup = Superficies.Do(t, principal, tamanho);
            int meio = principal.AreaUtil.Topo + principal.AreaUtil.Altura / 2;
            RetanguloPx naParede = SpriteEm(new PontoPx(sup.Direita, meio), tamanho);

            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil))
            {
                b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Idle", 5000, "núcleo carregado");
                (long marca, _) = Arrastar(b, new PontoPx(sup.Direita - 30, meio), tamanho);
                EventoDoLog agarrou = Nucleo(b, marca, "DragEnd", "Settling", "Climbing", 3000, "grudado na parede");
                Afirmar.Igual("SETTLING: solto junto a uma lateral, fica grudado na parede", agarrou["regra"]);
                b.EsperarRetangulo(naParede, 3000, "encostado na lateral direita");
                // A saída grava, ou encontra no disco o que a gravação com atraso já pôs lá, se ela veio antes de fechar.
                List<EventoDoLog> saida = FecharEGravar(b);
                Afirmar.Verdadeiro(saida.Count == 1 && saida[0]["gravado"] is "sim" or "sem mudanca" && saida[0]["motivo"] == "CmdExit",
                    $"a saída grava na hora ({string.Join("; ", saida.Select(e => e.Linha))})");
            }
            LeituraDasConfiguracoes preso = LerDoPerfil();
            Afirmar.Igual((LadoDoEsconderijo.Nenhum, true), (preso.Configuracoes.Esconderijo, preso.Configuracoes.PresoPeloUsuario), "o arquivo guarda a marca de preso");

            RetanguloPx escondido;
            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil, limpar: false))
            {
                EventoDoLog voltou = b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["de"] == "Settling", 5000, "a acomodação da carga");
                Afirmar.Igual(("Climbing", "SETTLING: solto junto a uma lateral, fica grudado na parede"), (voltou["para"], voltou["regra"]), "reaberto, grudado na parede");
                FicaParado(b, naParede, 1500, "preso e pausado, fica na parede");

                long marca = BuzzyEmTeste.MarcaDoLog();
                RetanguloPx agora = b.RetanguloDaJanela();
                CliqueDuplo(b, new PontoPx((agora.Esquerda + agora.Direita) / 2, agora.Topo + agora.Altura / 2));
                EventoDoLog escondeu = Nucleo(b, marca, "DoubleClick", "Settling", "Peeking", 3000, "escondido pelo clique duplo");
                Afirmar.Igual("SETTLING: escondido atrás da borda (Direita)", escondeu["regra"], "atrás da lateral direita");
                Thread.Sleep(300);
                escondido = b.RetanguloDaJanela();
                List<EventoDoLog> saida = FecharEGravar(b);
                Afirmar.Verdadeiro(saida.Any(e => e["gravado"] == "sim"), $"a saída grava a borda ({string.Join("; ", saida.Select(e => e.Linha))})");
            }
            LeituraDasConfiguracoes naBorda = LerDoPerfil();
            Afirmar.Igual((LadoDoEsconderijo.Direita, true), (naBorda.Configuracoes.Esconderijo, naBorda.Configuracoes.PresoPeloUsuario),
                "o arquivo guarda a borda, e a marca de preso continua guardada para quando ele sair do esconderijo");

            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil, limpar: false))
            {
                EventoDoLog voltou = b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["de"] == "Settling", 5000, "a acomodação da carga");
                Afirmar.Igual(("Peeking", "SETTLING: escondido atrás da borda (Direita)"), (voltou["para"], voltou["regra"]), "reaberto, escondido na mesma borda");
                FicaParado(b, escondido, 1000, "escondido, no mesmo lugar");
                Afirmar.Igual(0, b.FecharPorWmClose());
            }
        });
    }

    // A emoção dominante escolhida pelo menu (DEC-027) vai para o settings.json com atraso e volta ao reabrir: a carga já
    // começa com a cara dela, e o menu a mostra marcada. As teclas são WM_CHAR postadas ao dono do menu, deste Buzzy.
    [Teste]
    public void EmocaoEscolhidaPeloMenu_FecharEReabrir_VoltaComACaraEAMarca()
    {
        SemTocarNosArquivosReais(() =>
        {
            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil))
            {
                b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["para"] == "Idle", 5000, "núcleo carregado");
                PontoPx opaco = EventoDoLog.Ponto(b.Esperar(e => e.Chave == "POSICAO", 5000, "posição")["pontoOpaco"]);
                long marca = BuzzyEmTeste.MarcaDoLog();
                EventoDoLog fechado = EscolherNoMenu(b, opaco, 'd', 'f');
                Afirmar.Igual(("Emocao", "Feliz"), (fechado["fechado"], fechado["argumento"]), "o menu escolheu Feliz");
                EventoDoLog pedido = EsperarDesde(b, marca, e => e.Chave == "CONFIG" && e["pedido"] == "preferencias", 3000, "o pedido de gravação das preferências");
                Afirmar.Igual(("CmdSetDominantEmotion", "nao"), (pedido["evento"], pedido["imediata"]), "com atraso");
                EsperarDesde(b, marca, e => e.Chave == "CONFIG" && e["gravado"] == "sim" && e["motivo"] == "atraso", 8000, "a emoção gravada com atraso");
                Afirmar.Igual(0, b.FecharPorWmClose());
            }
            Afirmar.Igual<Expressao?>(Expressao.Feliz, LerDoPerfil().Configuracoes.Preferencias.EmocaoDominante, "o arquivo guarda a emoção");

            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil, limpar: false))
            {
                b.Esperar(e => e.Chave == "SPRITE" && e["pose"] == "parado" && e["expressao"] == "feliz", 5000, "reaberto, parado com a cara feliz");
                PontoPx opaco = EventoDoLog.Ponto(b.Esperar(e => e.Chave == "POSICAO", 5000, "posição")["pontoOpaco"]);
                long marca = BuzzyEmTeste.MarcaDoLog();
                b.PostarMouse(NativoTeste.WM_RBUTTONDOWN, 0, opaco);
                b.PostarMouse(NativoTeste.WM_RBUTTONUP, 0, opaco);
                EventoDoLog exibindo = EsperarDesde(b, marca, e => e.Chave == "MENU" && e["exibindo"] == "sim", 3000, "menu exibido");
                Afirmar.Igual("Feliz", exibindo["emocaoMarcada"], "o menu mostra a emoção restaurada");
                var dono = (nint)long.Parse(exibindo["dono"], CultureInfo.InvariantCulture);
                Afirmar.Igual((uint)b.Processo.Id, NativoTeste.PidDe(dono), "o dono do menu é deste Buzzy");
                Afirmar.Verdadeiro(NativoTeste.PostMessage(dono, NativoTeste.WM_CANCELMODE, 0, 0), "WM_CANCELMODE ao dono do menu");
                EsperarDesde(b, marca, e => e.Chave == "MENU" && e.Campos.ContainsKey("fechado"), 3000, "menu fechado");
                Afirmar.Igual(0, b.FecharPorWmClose());
            }
        });
    }

    // Um settings.json ilegível no perfil: a partida usa os padrões (a posição inicial), e a primeira gravação, a da saída,
    // guarda o ilegível como a cópia de diagnóstico (settings.corrupt.json) e recria o principal, válido.
    [Teste]
    public void PartidaComArquivoIlegivel_UsaOsPadroes_EASaidaGuardaUmaCopia()
    {
        SemTocarNosArquivosReais(() =>
        {
            Topologia t = Afirmar.NaoNulo(LeitorDeTopologia.Ler(out string? erro), erro);
            BuzzyEmTeste.ExigirNenhumBuzzyAberto();
            PerfilDeTeste.Limpar(Perfil);
            string pasta = PastaDoPerfil();
            Directory.CreateDirectory(pasta);
            File.WriteAllText(Path.Combine(pasta, ArquivoDeConfiguracoes.NomePrincipal), "{ lixo");

            using (BuzzyEmTeste b = BuzzyEmTeste.Iniciar(perfil: Perfil, limpar: false))
            {
                EventoDoLog partida = b.Esperar(e => e.Chave == "CONFIG" && e.Campos.ContainsKey("lido"), 5000, "a leitura das configurações na partida");
                Afirmar.Igual(("padroes", "Ilegivel", "Ausente"), (partida["lido"], partida["principal"], partida["reserva"]), "ilegível e sem reserva: os padrões");
                EventoDoLog carga = b.Esperar(e => e.Chave == "NUCLEO" && e["evento"] == "Loaded" && e["de"] == "Booting", 5000, "a carga");
                Afirmar.Igual("BOOTING: configurações e topologia carregadas", carga["regra"], "sem posição salva");
                b.EsperarRetangulo(Posicionador.Inicial(t, Sprite).Retangulo, 3000, "na posição inicial");

                List<EventoDoLog> saida = FecharEGravar(b);
                EventoDoLog gravado = Afirmar.NaoNulo(saida.SingleOrDefault(e => e["gravado"] == "sim"), $"a saída grava ({string.Join("; ", saida.Select(e => e.Linha))})");
                Afirmar.Igual(("CmdExit", "Ilegivel", "sim"), (gravado["motivo"], gravado["principalAntes"], gravado["copiaDeDiagnostico"]), "o ilegível vira a cópia de diagnóstico");
            }
            Afirmar.Igual("{ lixo", File.ReadAllText(Path.Combine(pasta, ArquivoDeConfiguracoes.NomeIlegivel)), "a cópia de diagnóstico é o ilegível");
            LeituraDasConfiguracoes lida = LerDoPerfil();
            Afirmar.Igual(SituacaoDaLeitura.Valida, lida.Situacao, "o principal recriado é válido");
            Afirmar.Sequencia(new[] { ArquivoDeConfiguracoes.NomePrincipal, ArquivoDeConfiguracoes.NomeIlegivel }.Order(StringComparer.Ordinal),
                Directory.GetFiles(pasta).Select(Path.GetFileName).Order(StringComparer.Ordinal), "só o principal e a cópia de diagnóstico, sem temporário nem reserva");
        });
    }

    /// <summary>Abre o menu pelo botão direito postado e escolhe pelas teclas, postadas como WM_CHAR ao dono do menu.</summary>
    private static EventoDoLog EscolherNoMenu(BuzzyEmTeste b, PontoPx opaco, char submenu, char opcao)
    {
        long marca = BuzzyEmTeste.MarcaDoLog();
        b.PostarMouse(NativoTeste.WM_RBUTTONDOWN, 0, opaco);
        b.PostarMouse(NativoTeste.WM_RBUTTONUP, 0, opaco);
        EventoDoLog exibindo = EsperarDesde(b, marca, e => e.Chave == "MENU" && e["exibindo"] == "sim", 3000, "menu exibido");
        var dono = (nint)long.Parse(exibindo["dono"], CultureInfo.InvariantCulture);
        b.PostarChar(dono, submenu);
        b.PostarChar(dono, opcao);
        return EsperarDesde(b, marca, e => e.Chave == "MENU" && e.Campos.ContainsKey("fechado"), 3000, $"menu fechado pelas teclas {submenu} e {opcao}");
    }
}

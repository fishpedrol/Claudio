namespace Buzzy.Visual.Pixel;

/// <summary>Vista do boneco. O perfil olha para a direita; a esquerda é o espelho.</summary>
public enum Vista
{
    Frente,
    Perfil,
}

public enum Mao
{
    Aberta,
    Fechada,
}

public enum Cauda
{
    Espiral,
    Alta,
    Caida,
    Perfil,
    PerfilAlta,
    PerfilCaida,
}

/// <summary>
/// Ângulos de um membro, em graus, na tela: 0 aponta para baixo, 90 para a direita, −90 para a
/// esquerda e 180 para cima. <see cref="Superior"/> é o do braço ou da coxa; <see cref="Inferior"/>,
/// o do antebraço ou da canela, também absoluto.
/// </summary>
public readonly record struct Membro(double Superior, double Inferior);

/// <summary>
/// Uma pose do boneco em pixel art, na grade nativa de 64 × 64 (1 pixel = 2 DIP a 100%). O
/// quadril é a raiz; o resto sai do esqueleto de <see cref="BonecoPixel"/>. Na vista de frente, A é o
/// lado esquerdo da tela e B o direito; no perfil, A é o lado de trás (mais escuro) e B o da frente.
/// </summary>
public sealed record PosePixel
{
    public required string Nome { get; init; }

    public string Estado { get; init; } = "";

    public Vista Vista { get; init; } = Vista.Frente;

    public double QuadrilX { get; init; } = 32;

    public double QuadrilY { get; init; } = 48.5;

    /// <summary>Inclinação do tronco em graus: positiva leva o pescoço para a direita.</summary>
    public double Tronco { get; init; }

    /// <summary>Inclinação extra da cabeça em relação ao tronco.</summary>
    public double Cabeca { get; init; }

    /// <summary>Quanto a cabeça desce em direção aos ombros (encolher, pendurar-se), em pixels.</summary>
    public double CabecaDescida { get; init; }

    public Membro BracoA { get; init; } = new(-10, -4);

    public Membro BracoB { get; init; } = new(10, 4);

    public Membro PernaA { get; init; } = new(-5, -2);

    public Membro PernaB { get; init; } = new(5, 2);

    public Mao MaoA { get; init; } = Mao.Aberta;

    public Mao MaoB { get; init; } = Mao.Aberta;

    public Cauda Cauda { get; init; } = Cauda.Espiral;

    public string Expressao { get; init; } = "neutro";

    /// <summary>Pés: na frente, virados para fora; no perfil, para a frente. Sem pé, a canela termina redonda.</summary>
    public bool PesNoChao { get; init; } = true;

    /// <summary>
    /// Linha de uma borda (em pixels do quadro) atrás da qual o corpo se esconde, como ao espiar:
    /// tudo abaixo dela some, e as mãos que a agarram ficam na frente da cabeça.
    /// </summary>
    public double? Borda { get; init; }
}

/// <summary>
/// Monta o Buzzy em pixel art a partir de uma <see cref="PosePixel"/>: formas simples rasterizadas
/// sem meio-tom, sombra de um pixel na borda de baixo e da direita, linhas internas entre partes
/// sobrepostas, contorno externo escuro e carimbos desenhados à mão para o rosto.
/// </summary>
public static class BonecoPixel
{
    public const int Lado = 64;

    // Esqueleto, em pixels nativos.
    private const double TroncoAltura = 18;
    private const double PescocoACabeca = 8.2;
    private const double RaioCabeca = 11.2;
    private const double BracoSuperior = 10.8;
    private const double Antebraco = 9.6;
    private const double Coxa = 5.6;
    private const double Canela = 5.2;

    public static Tela Desenhar(PosePixel pose, string? expressao = null)
    {
        ArgumentNullException.ThrowIfNull(pose);
        Rosto rosto = Rostos.Expressoes[expressao ?? pose.Expressao];
        var tela = new Tela(Lado, Lado);
        var e = new Esqueleto(pose);

        if (pose.Vista == Vista.Frente)
        {
            DesenharCauda(tela, e, pose.Cauda, longe: false);
            DesenharPerna(tela, e.QuadrilA, pose.PernaA, pose, ladoDaTela: -1, longe: false);
            DesenharPerna(tela, e.QuadrilB, pose.PernaB, pose, ladoDaTela: 1, longe: false);
            DesenharTronco(tela, e, pose.Vista);
            if (pose.Borda is null)
            {
                DesenharBraco(tela, e.OmbroA, pose.BracoA, pose.MaoA, longe: false);
                DesenharBraco(tela, e.OmbroB, pose.BracoB, pose.MaoB, longe: false);
                DesenharCabecaDeFrente(tela, e, rosto);
            }
            else
            {
                DesenharCabecaDeFrente(tela, e, rosto);
                DesenharBraco(tela, e.OmbroA, pose.BracoA, pose.MaoA, longe: false);
                DesenharBraco(tela, e.OmbroB, pose.BracoB, pose.MaoB, longe: false);
            }
        }
        else
        {
            DesenharBraco(tela, e.OmbroA, pose.BracoA, pose.MaoA, longe: true);
            DesenharPerna(tela, e.QuadrilA, pose.PernaA, pose, ladoDaTela: 1, longe: true);
            DesenharCauda(tela, e, pose.Cauda, longe: false);
            DesenharTronco(tela, e, pose.Vista);
            DesenharPerna(tela, e.QuadrilB, pose.PernaB, pose, ladoDaTela: 1, longe: false);
            DesenharBraco(tela, e.OmbroB, pose.BracoB, pose.MaoB, longe: false);
            DesenharCabecaDePerfil(tela, e, rosto);
        }

        if (pose.Borda is { } borda)
        {
            // O que fica atrás da borda some; o contorno depois fecha a linha da borda.
            for (int y = (int)Math.Ceiling(borda); y < Lado; y++)
                for (int x = 0; x < Lado; x++)
                    tela[x, y] = Cor.Nada;
        }

        tela.Contornar(Cor.Contorno);
        return tela;
    }

    // ------------------------------------------------------------------ esqueleto

    private sealed class Esqueleto
    {
        internal Esqueleto(PosePixel pose)
        {
            double t = pose.Tronco * Math.PI / 180;
            Cima = (Math.Sin(t), -Math.Cos(t));
            Direita = (Math.Cos(t), Math.Sin(t));
            Quadril = (pose.QuadrilX, pose.QuadrilY);
            Pescoco = Somar(Quadril, Escalar(Cima, TroncoAltura));
            double c = (pose.Tronco + pose.Cabeca) * Math.PI / 180;
            (double X, double Y) cimaDaCabeca = (Math.Sin(c), -Math.Cos(c));
            Cabeca = Somar(Pescoco, Escalar(cimaDaCabeca, PescocoACabeca - pose.CabecaDescida));
            bool frente = pose.Vista == Vista.Frente;
            double ombro = frente ? 5.5 : 1.2, quadril = frente ? 3.6 : 0.8;
            (double X, double Y) baseDoOmbro = Somar(Pescoco, Escalar(Cima, -3.2));
            OmbroA = Somar(baseDoOmbro, Escalar(Direita, -ombro));
            OmbroB = Somar(baseDoOmbro, Escalar(Direita, frente ? ombro : ombro + 0.6));
            QuadrilA = Somar(Quadril, Escalar(Direita, -quadril));
            QuadrilB = Somar(Quadril, Escalar(Direita, quadril));
        }

        internal (double X, double Y) Cima { get; }
        internal (double X, double Y) Direita { get; }
        internal (double X, double Y) Quadril { get; }
        internal (double X, double Y) Pescoco { get; }
        internal (double X, double Y) Cabeca { get; }
        internal (double X, double Y) OmbroA { get; }
        internal (double X, double Y) OmbroB { get; }
        internal (double X, double Y) QuadrilA { get; }
        internal (double X, double Y) QuadrilB { get; }

        /// <summary>Ponto no referencial do tronco: x para a direita do tronco, y para cima.</summary>
        internal (double X, double Y) NoTronco(double x, double y) => Somar(Somar(Quadril, Escalar(Direita, x)), Escalar(Cima, y));
    }

    // ------------------------------------------------------------------ partes

    private static Mascara Nova() => new(Lado, Lado);

    private static void DesenharTronco(Tela tela, Esqueleto e, Vista vista)
    {
        bool frente = vista == Vista.Frente;
        double graus = Math.Atan2(e.Direita.Y, e.Direita.X) * 180 / Math.PI;
        // Tronco em forma de pera, como nas pranchas: peito estreito e barriga mais larga.
        (double X, double Y) peito = e.NoTronco(frente ? 0 : 0.3, 13.2), ventre = e.NoTronco(0, 6.8);
        Mascara corpo = Nova()
            .Elipse(peito.X, peito.Y, frente ? 5.0 : 4.6, 7.2, graus)
            .Elipse(ventre.X, ventre.Y, frente ? 6.2 : 5.7, 7.6, graus);
        tela.Pintar(corpo, Cor.Pelo, Cor.PeloEscuro, Cor.PeloClaro, Cor.PeloEscuro);

        (double X, double Y) centroDaBarriga = frente ? e.NoTronco(0, 8.2) : e.NoTronco(2.4, 8.2);
        Mascara barriga = Nova().Elipse(centroDaBarriga.X, centroDaBarriga.Y, frente ? 3.9 : 3.0, 7.6, graus).Intersectar(corpo);
        tela.Pintar(barriga, Cor.Creme, Cor.CremeSombra);
    }

    private static void DesenharBraco(Tela tela, (double X, double Y) ombro, Membro membro, Mao mao, bool longe)
    {
        (double X, double Y) cotovelo = Somar(ombro, Escalar(Direcao(membro.Superior), BracoSuperior));
        (double X, double Y) pulso = Somar(cotovelo, Escalar(Direcao(membro.Inferior), Antebraco));
        Mascara braco = Nova()
            .Capsula(ombro.X, ombro.Y, cotovelo.X, cotovelo.Y, 1.9, 1.7)
            .Capsula(cotovelo.X, cotovelo.Y, pulso.X, pulso.Y, 1.7, 1.5);
        Cor pelo = longe ? Cor.PeloEscuro : Cor.Pelo, sombra = longe ? Cor.Contorno : Cor.PeloEscuro;
        tela.Pintar(braco, pelo, sombra, null, Cor.Contorno);

        (double X, double Y) dir = Direcao(membro.Inferior);
        (double X, double Y) centroDaMao = Somar(pulso, Escalar(dir, mao == Mao.Aberta ? 1.9 : 1.2));
        Mascara palma = mao == Mao.Aberta
            ? Nova().Elipse(centroDaMao.X, centroDaMao.Y, 2.2, 2.9, -membro.Inferior)
            : Nova().Circulo(centroDaMao.X, centroDaMao.Y, 2.2);
        tela.Pintar(palma, longe ? Cor.CremeSombra : Cor.Creme, longe ? Cor.PessegoEscuro : Cor.CremeSombra, null, Cor.Contorno);
        if (mao == Mao.Aberta && !longe)
        {
            // Dedos: dois riscos de sombra na ponta da mão, na direção do antebraço.
            (double X, double Y) ponta = Somar(centroDaMao, Escalar(dir, 1.6));
            (double X, double Y) lado = (-dir.Y, dir.X);
            foreach (double k in new[] { -0.8, 0.8 })
            {
                (double X, double Y) q = Somar(ponta, Escalar(lado, k));
                int qx = (int)Math.Floor(q.X), qy = (int)Math.Floor(q.Y);
                if (palma[qx, qy]) tela[qx, qy] = Cor.CremeSombra;
            }
        }
    }

    private static void DesenharPerna(Tela tela, (double X, double Y) quadril, Membro membro, PosePixel pose, int ladoDaTela, bool longe)
    {
        (double X, double Y) joelho = Somar(quadril, Escalar(Direcao(membro.Superior), Coxa));
        (double X, double Y) tornozelo = Somar(joelho, Escalar(Direcao(membro.Inferior), Canela));
        Mascara perna = Nova()
            .Capsula(quadril.X, quadril.Y, joelho.X, joelho.Y, 2.6, 2.4)
            .Capsula(joelho.X, joelho.Y, tornozelo.X, tornozelo.Y, 2.4, 2.1);
        Cor pelo = longe ? Cor.PeloEscuro : Cor.Pelo, sombra = longe ? Cor.Contorno : Cor.PeloEscuro;
        tela.Pintar(perna, pelo, sombra, null, longe ? Cor.Contorno : Cor.PeloEscuro);

        if (!pose.PesNoChao) return;
        (double X, double Y) centroDoPe = pose.Vista == Vista.Frente
            ? (tornozelo.X + 1.3 * ladoDaTela, tornozelo.Y + 1.7)
            : (tornozelo.X + 2.3, tornozelo.Y + 1.6);
        Mascara pe = Nova().Elipse(centroDoPe.X, centroDoPe.Y, pose.Vista == Vista.Frente ? 3.7 : 4.0, 1.8);
        tela.Pintar(pe, longe ? Cor.CremeSombra : Cor.Creme, longe ? Cor.PessegoEscuro : Cor.CremeSombra, null, Cor.Contorno);
        if (!longe)
        {
            // Um risco de dedo na ponta do pé.
            int dedoX = (int)Math.Floor(pose.Vista == Vista.Frente ? centroDoPe.X + 2.2 * ladoDaTela : centroDoPe.X + 2.4);
            int dedoY = (int)Math.Floor(centroDoPe.Y);
            if (pe[dedoX, dedoY]) tela[dedoX, dedoY] = Cor.CremeSombra;
        }
    }

    private static void DesenharCauda(Tela tela, Esqueleto e, Cauda forma, bool longe)
    {
        // Caminhos no referencial do tronco (x para a direita, y para cima), a partir da base.
        (double X, double Y)[] local = forma switch
        {
            Cauda.Espiral => [(3, 2), (10, -1.5), (17, -0.5), (19, 5.5), (21, 11.5), (17, 15), (15.3, 11.5), (14.4, 9), (17.4, 8), (18.2, 10.6)],
            Cauda.Alta => [(2.5, 1.5), (9, 1), (12, 8), (11, 16), (10, 22), (14.5, 25), (16, 21), (17, 18.5), (14, 17.5), (13.5, 20)],
            Cauda.Caida => [(2.5, 1.5), (8, 0), (11, -5), (11, -10), (11, -14), (7, -15), (6.5, -12), (6.2, -10), (8.5, -9.5), (9, -11.5)],
            Cauda.Perfil => [(-4.5, 2), (-11, -1), (-17, 0.5), (-18.5, 6), (-20, 11.5), (-15.5, 14.5), (-13.8, 11), (-12.8, 8.6), (-15.6, 7.8), (-16.4, 10.2)],
            Cauda.PerfilAlta => [(-4.5, 2), (-11, 3), (-15, 9), (-14, 15), (-13, 20), (-17.5, 22.5), (-18.5, 19), (-19, 16.5), (-16, 16), (-15.5, 18.2)],
            Cauda.PerfilCaida => [(-4.5, 2), (-10, 1), (-14, -4), (-14.5, -9), (-15, -13), (-11, -14.5), (-10.5, -11.5), (-10.2, -9.5), (-12.5, -9), (-13, -11)],
            _ => throw new ArgumentOutOfRangeException(nameof(forma), forma, "Cauda desconhecida."),
        };
        List<(double X, double Y)> caminho = [.. local.Select(p => e.NoTronco(p.X, p.Y))];
        Mascara pelo = Nova().Traco(caminho, 1.4, 1.5, 0, 0.72);
        Mascara ponta = Nova().Traco(caminho, 1.6, 1.9, 0.66, 1);
        pelo.Subtrair(ponta);
        tela.Pintar(pelo, longe ? Cor.PeloEscuro : Cor.Pelo, longe ? Cor.Contorno : Cor.PeloEscuro, null, Cor.Contorno);
        tela.Pintar(ponta, Cor.Creme, Cor.CremeSombra, null, Cor.PeloEscuro);
    }

    private static void DesenharCabecaDeFrente(Tela tela, Esqueleto e, Rosto rosto)
    {
        (double cx, double cy) = e.Cabeca;

        // Orelhas redondas, cor de pêssego com a borda de pelo, atrás da cabeça.
        foreach (int lado in new[] { -1, 1 })
        {
            Mascara orelha = Nova().Circulo(cx + 11.4 * lado, cy + 1.2, 4.7);
            tela.Pintar(orelha, Cor.Pelo, Cor.PeloEscuro, null, Cor.Contorno);
            Mascara dentro = Nova().Circulo(cx + 11.8 * lado, cy + 1.3, 3.5);
            tela.Pintar(dentro, Cor.Pessego, Cor.PessegoEscuro);
        }

        Mascara cabeca = Nova().Circulo(cx, cy, RaioCabeca).Unir(Tufo(cx, cy, rosto.Topete, perfil: false))
            .Poligono((cx - 8.6, cy + 5.4), (cx - 12.6, cy + 8.8), (cx - 8.0, cy + 8.4))
            .Poligono((cx + 8.6, cy + 5.4), (cx + 12.6, cy + 8.8), (cx + 8.0, cy + 8.4));
        tela.Pintar(cabeca, Cor.Pelo, Cor.PeloEscuro, Cor.PeloClaro, Cor.PeloEscuro);

        // Máscara do rosto: dois lobos sobre os olhos (o bico de pelo desce entre eles), bochechas
        // largas e focinho, como nas pranchas.
        Mascara mascara = Nova()
            .Elipse(cx - 5.0, cy - 1.3, 5.2, 6.0)
            .Elipse(cx + 5.0, cy - 1.3, 5.2, 6.0)
            .Elipse(cx, cy + 2.6, 9.7, 4.6)
            .Elipse(cx, cy + 5, 7.4, 5.1)
            .Intersectar(Nova().Circulo(cx, cy, RaioCabeca - 0.6));
        tela.Pintar(mascara, Cor.Creme, Cor.CremeSombra);

        int ex = (int)Math.Round(cx), ey = (int)Math.Round(cy);
        tela.Carimbar(Rostos.Olhos[rosto.OlhoE], ex - 9, ey - 4);
        tela.Carimbar(Rostos.Olhos[rosto.OlhoD], ex + 2, ey - 4);
        tela.Carimbar(Rostos.Sobrancelhas[rosto.Sobrancelhas], ex - 8, ey - 7);
        if (rosto.Corado)
        {
            tela[ex - 9, ey + 5] = Cor.Bochecha;
            tela[ex - 8, ey + 5] = Cor.Bochecha;
            tela[ex + 7, ey + 5] = Cor.Bochecha;
            tela[ex + 8, ey + 5] = Cor.Bochecha;
        }
        // Nariz pequeno cor de pêssego, como nas pranchas.
        tela[ex - 1, ey + 4] = Cor.Pessego;
        tela[ex, ey + 4] = Cor.Pessego;
        tela[ex - 1, ey + 5] = Cor.PessegoEscuro;
        tela[ex, ey + 5] = Cor.PessegoEscuro;
        tela.Carimbar(Rostos.Bocas[rosto.Boca], ex - 5, ey + 6);
        DesenharChapeu(tela, cx, cy, rosto.Topete, perfil: false);
    }

    private static void DesenharCabecaDePerfil(Tela tela, Esqueleto e, Rosto rosto)
    {
        (double cx, double cy) = e.Cabeca;
        Mascara cabeca = Nova().Circulo(cx, cy, RaioCabeca - 0.3).Unir(Tufo(cx, cy, rosto.Topete, perfil: true))
            .Poligono((cx - 3.5, cy + 8.0), (cx - 7.8, cy + 11.4), (cx - 2.2, cy + 10.4));
        tela.Pintar(cabeca, Cor.Pelo, Cor.PeloEscuro, Cor.PeloClaro, Cor.PeloEscuro);

        Mascara mascara = Nova()
            .Circulo(cx + 4.4, cy - 0.9, 6.1)
            .Elipse(cx + 6.6, cy + 4.3, 5.9, 4.3);
        tela.Pintar(mascara, Cor.Creme, Cor.CremeSombra, null, Cor.PeloEscuro);

        // Orelha grande na parte de trás da cabeça, por cima dela.
        Mascara orelha = Nova().Circulo(cx - 5.6, cy + 0.6, 4.5);
        tela.Pintar(orelha, Cor.Pelo, Cor.PeloEscuro, null, Cor.PeloEscuro);
        Mascara dentro = Nova().Circulo(cx - 5.6, cy + 1.0, 2.6);
        tela.Pintar(dentro, Cor.Pessego, Cor.PessegoEscuro);

        int ex = (int)Math.Round(cx), ey = (int)Math.Round(cy);
        tela.Carimbar(Rostos.OlhosPerfil[rosto.OlhoPerfil], ex + 3, ey - 5);
        tela[ex + 3, ey - 7] = Cor.Sobrancelha;
        tela[ex + 4, ey - 8] = Cor.Sobrancelha;
        tela[ex + 5, ey - 8] = Cor.Sobrancelha;
        tela[ex + 11, ey + 2] = Cor.Pessego;
        tela[ex + 12, ey + 2] = Cor.Pessego;
        tela[ex + 11, ey + 3] = Cor.PessegoEscuro;
        tela.Carimbar(Rostos.BocasPerfil[rosto.BocaPerfil], ex + 7, ey + 5);
        DesenharChapeu(tela, cx, cy, rosto.Topete, perfil: true);
    }

    /// <summary>
    /// Chapéu de palha com faixa vermelha, como nas pranchas (DEC-019). Reage à emoção junto com o
    /// tufo: salta com o susto e a risada, desce com o sono.
    /// </summary>
    private static void DesenharChapeu(Tela tela, double cx, double cy, Topete topete, bool perfil)
    {
        double dy = topete switch { Topete.Ericado => -2.2, Topete.Caido => 1.0, _ => 0 };
        double abaY = cy - 9.3 + dy, copaY = cy - 12.6 + dy;
        double abaX = perfil ? cx + 0.8 : cx, copaX = perfil ? cx - 0.8 : cx;
        double abaRx = perfil ? 15.4 : 17.0, abaRy = perfil ? 2.2 : 2.8;
        double copaRx = perfil ? 8.8 : 9.4, copaRy = 5.5;

        Mascara aba = Nova().Elipse(abaX, abaY, abaRx, abaRy);
        // Sombra da aba na testa e no pelo logo abaixo dela.
        for (int y = 0; y < Lado; y++)
            for (int x = 0; x < Lado; x++)
                if (!aba[x, y] && aba[x, y - 1])
                    tela[x, y] = tela[x, y] switch
                    {
                        Cor.Creme or Cor.CremeClaro => Cor.CremeSombra,
                        Cor.Pelo or Cor.PeloClaro => Cor.PeloEscuro,
                        var c => c,
                    };
        tela.Pintar(aba, Cor.Palha, Cor.PalhaEscura, Cor.PalhaClara, Cor.Contorno);

        Mascara acimaDaAba = Nova().Poligono((0, 0), (Lado, 0), (Lado, abaY + 0.4), (0, abaY + 0.4));
        Mascara copa = Nova().Elipse(copaX, copaY, copaRx, copaRy).Intersectar(acimaDaAba);
        tela.Pintar(copa, Cor.Palha, Cor.PalhaEscura, Cor.PalhaClara, Cor.PalhaEscura);
        // Trama da palha: pontos escuros em diagonal, fora das bordas.
        for (int y = 0; y < Lado; y++)
            for (int x = 0; x < Lado; x++)
                if (copa[x, y] && copa[x - 1, y] && copa[x + 1, y] && copa[x, y - 1] && tela[x, y] == Cor.Palha && (x + 2 * y) % 5 == 0)
                    tela[x, y] = Cor.PalhaEscura;

        Mascara faixa = Nova().Elipse(copaX, copaY, copaRx, copaRy).Intersectar(Nova().Poligono((0, abaY - 3.0), (Lado, abaY - 3.0), (Lado, abaY + 0.4), (0, abaY + 0.4)));
        tela.Pintar(faixa, Cor.Faixa, Cor.FaixaEscura);

        // A borda da frente da aba passa por cima da base da copa.
        Mascara labio = Nova().Elipse(abaX, abaY, abaRx, abaRy).Subtrair(Nova().Poligono((0, 0), (Lado, 0), (Lado, abaY + 0.2), (0, abaY + 0.2)));
        tela.Pintar(labio, Cor.Palha, Cor.PalhaEscura, null, Cor.PalhaEscura);
    }

    /// <summary>Tufo de pelo bagunçado no alto da cabeça (substitui o chapéu das pranchas como marca da silhueta).</summary>
    private static Mascara Tufo(double cx, double cy, Topete topete, bool perfil)
    {
        double s = perfil ? -1 : 1;
        (double X, double Y)[] pontos = topete switch
        {
            Topete.Ericado => [(-6.5, -8.5), (-7.5, -14.5), (-3.8, -11.5), (-2.2, -16.5), (0.6, -12), (3.6, -16), (4.4, -11.2), (8, -13.2), (6.4, -8.2)],
            Topete.Caido => [(-7, -8.5), (-9.5, -11.5), (-4.5, -11.2), (-4.4, -13.2), (-0.5, -11.8), (1.5, -13.6), (3.2, -11.2), (6.8, -11.2), (6.6, -8)],
            _ => [(-6.5, -8.5), (-8, -12.8), (-3.8, -11), (-2.5, -14.5), (0.4, -11.4), (3.2, -14), (4, -10.8), (7.4, -11.6), (6.4, -8)],
        };
        return Nova().Poligono([.. pontos.Select(p => (cx + p.X * s, cy + p.Y))]);
    }

    // ------------------------------------------------------------------ apoio

    private static (double X, double Y) Direcao(double graus)
    {
        double a = graus * Math.PI / 180;
        return (Math.Sin(a), Math.Cos(a));
    }

    private static (double X, double Y) Somar((double X, double Y) a, (double X, double Y) b) => (a.X + b.X, a.Y + b.Y);

    private static (double X, double Y) Escalar((double X, double Y) a, double k) => (a.X * k, a.Y * k);
}

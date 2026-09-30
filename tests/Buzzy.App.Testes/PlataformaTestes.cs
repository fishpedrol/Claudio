using System.IO;
using System.Runtime.InteropServices;
using Buzzy.App.Plataforma;
using Buzzy.App.Testes.Integracao;
using Buzzy.Testes;

namespace Buzzy.App.Testes;

/// <summary>
/// Partes do adaptador que dá para verificar sem abrir janelas: tamanhos das estruturas
/// nativas (um campo errado faz o Windows recusar a chamada em silêncio, como aconteceu
/// no protótipo P1), textos da interface e o manifesto.
/// </summary>
internal sealed class PlataformaTestes
{
    [Teste]
    public void EstruturasNativasTemOTamanhoQueOWindowsEspera()
    {
        Afirmar.Verdadeiro(Environment.Is64BitProcess, "os testes rodam em x64, como o aplicativo");
        Afirmar.Igual(104, Marshal.SizeOf<Win32.MONITORINFOEX>(), "MONITORINFOEXW");
        Afirmar.Igual(976, Marshal.SizeOf<Win32.NOTIFYICONDATA>(), "NOTIFYICONDATAW completa, com hBalloonIcon");
        Afirmar.Igual(40, Marshal.SizeOf<Win32.NOTIFYICONIDENTIFIER>(), "NOTIFYICONIDENTIFIER");
        Afirmar.Igual(16, Marshal.SizeOf<Win32.RECT>(), "RECT");
        Afirmar.Igual(8, Marshal.SizeOf<Win32.POINT>(), "POINT");
    }

    [Teste]
    public void CoordenadasComSinalSaoPreservadas()
    {
        // Monitor à esquerda do principal: x negativo (ARCHITECTURE.md 2.13.3).
        nint lParam = unchecked((nint)((uint)(ushort)(short)-1500 | ((uint)(ushort)(short)-20 << 16)));
        Afirmar.Igual(-1500, Win32.XComSinal(lParam));
        Afirmar.Igual(-20, Win32.YComSinal(lParam));
        nint positivo = (nint)(300 | (200 << 16));
        Afirmar.Igual(300, Win32.XComSinal(positivo));
        Afirmar.Igual(200, Win32.YComSinal(positivo));
        Afirmar.Igual(0x0402, Win32.LoWord(unchecked((nint)0x0001_0402)));
        Afirmar.Igual(0x0001, Win32.HiWord(unchecked((nint)0x0001_0402)));
    }

    [Teste]
    public void TodosOsTextosExistemEmPortugues()
    {
        foreach (string chave in Textos.Chaves)
            Afirmar.Verdadeiro(!string.IsNullOrWhiteSpace(Textos.Obter(chave)), $"texto {chave}");

        Afirmar.Igual("&Esconder Buzzy", Textos.MenuEsconder);
        Afirmar.Igual("&Mostrar Buzzy", Textos.MenuMostrar);
        Afirmar.Igual("&Pausar movimento", Textos.MenuPausar);
        Afirmar.Igual("&Retomar movimento", Textos.MenuRetomar);
        Afirmar.Igual("&Sair", Textos.MenuSair);
        Afirmar.Igual("Buzzy", Textos.DicaDaBandeja);
    }

    [Teste]
    public void TeclasDeAcessoDoMenuNaoSeRepetem()
    {
        // O menu mostra um item de cada par (Esconder/Mostrar, Pausar/Retomar) mais Sair: as teclas
        // de todas as combinações precisam ser diferentes.
        char Tecla(string t) => char.ToUpperInvariant(t[t.IndexOf('&', StringComparison.Ordinal) + 1]);
        foreach (string visibilidade in new[] { Textos.MenuEsconder, Textos.MenuMostrar })
        {
            foreach (string movimento in new[] { Textos.MenuPausar, Textos.MenuRetomar })
            {
                char[] teclas = [Tecla(visibilidade), Tecla(movimento), Tecla(Textos.MenuSair)];
                Afirmar.Igual(3, teclas.Distinct().Count(), $"teclas de acesso distintas em [{string.Join(", ", teclas)}]");
            }
        }
    }

    [Teste]
    public void ManifestoDeclaraAsInvokerEPerMonitorV2()
    {
        string manifesto = File.ReadAllText(Path.Combine(Caminhos.Raiz, "src", "Buzzy.App", "app.manifest"));
        Afirmar.Contem("level=\"asInvoker\"", manifesto);
        Afirmar.Contem("uiAccess=\"false\"", manifesto);
        Afirmar.Contem(">PerMonitorV2<", manifesto);
    }

    [Teste]
    public void PastaDeDadosFicaNoLocalAppDataDoUsuario()
    {
        string esperado = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Buzzy");
        Afirmar.Igual(esperado, Diagnostico.PastaDeDados());
    }

    [Teste]
    public void OProcessoDeTestesRodaEmPerMonitorV2PeloProprioManifesto()
    {
        // Sem janela: só consulta o contexto de DPI da thread. Prova que o app.manifest dos
        // testes foi embutido e vale, e que as coordenadas lidas pelos testes de integração
        // são físicas, como as do log do Buzzy.
        Afirmar.Verdadeiro(NativoTeste.ThreadEmPerMonitorV2(), "a thread de testes está em Per-Monitor V2");
    }

    [Teste]
    public void ManifestosDasFerramentasDeTesteDeclaramAsInvokerEPerMonitorV2()
    {
        foreach (string projeto in new[] { "Buzzy.App.Testes", "Buzzy.Verificacao" })
        {
            string pasta = Path.Combine(Caminhos.Raiz, "tests", projeto);
            string manifesto = File.ReadAllText(Path.Combine(pasta, "app.manifest"));
            Afirmar.Contem("level=\"asInvoker\"", manifesto, projeto);
            Afirmar.Contem("uiAccess=\"false\"", manifesto, projeto);
            Afirmar.Contem(">PerMonitorV2<", manifesto, projeto);
            Afirmar.Contem(">true/pm<", manifesto, projeto);
            Afirmar.Contem("<ApplicationManifest>app.manifest</ApplicationManifest>", File.ReadAllText(Path.Combine(pasta, projeto + ".csproj")), projeto);
        }
    }
}

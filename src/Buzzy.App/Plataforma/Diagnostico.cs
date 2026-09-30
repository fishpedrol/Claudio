using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Buzzy.App.Plataforma;

/// <summary>
/// Log de diagnóstico, DESLIGADO por padrão. Só grava com <c>--diagnostico</c> na linha de
/// comando, e só em <c>%LOCALAPPDATA%\Buzzy\diagnostico.log</c> (SECURITY.md 5: logs ficam na
/// pasta do Buzzy e têm tamanho limitado). Ao passar de 1 MB, o arquivo vira
/// <c>diagnostico.1.log</c> (uma cópia só) e recomeça.
///
/// Registra apenas fatos do próprio Buzzy: suas janelas, sua posição, os monitores, o ícone
/// da bandeja, os cliques que chegaram às suas janelas e os comandos do seu menu. Nunca
/// registra nada de outro aplicativo.
///
/// Linhas legíveis por máquina: <c>[hh:mm:ss.fff] BUZZY|CHAVE|campo=valor|campo=valor</c>,
/// usadas pelos testes de integração e pelo script de medição.
/// </summary>
internal static class Diagnostico
{
    private const long LimiteBytes = 1024 * 1024;
    private static readonly object Trava = new();
    private static readonly Stopwatch Relogio = Stopwatch.StartNew();
    private static string? _arquivo;

    internal static bool Ligado => _arquivo is not null;

    internal static string? Arquivo => _arquivo;

    /// <summary>Pasta de dados do Buzzy, pela consulta de pasta conhecida do Windows.</summary>
    internal static string PastaDeDados()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify), "Buzzy");

    internal static void Ligar()
    {
        try
        {
            string pasta = PastaDeDados();
            Directory.CreateDirectory(pasta);
            _arquivo = Path.Combine(pasta, "diagnostico.log");
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _arquivo = null;
        }
    }

    /// <summary>Registra um evento: <c>Evento("JANELA", ("hwnd", 1234))</c> vira <c>BUZZY|JANELA|hwnd=1234</c>.</summary>
    internal static void Evento(string chave, params (string Campo, object? Valor)[] campos)
    {
        if (_arquivo is null) return;
        var sb = new StringBuilder("BUZZY|").Append(chave);
        foreach ((string campo, object? valor) in campos)
        {
            sb.Append('|').Append(campo).Append('=');
            sb.Append(Convert.ToString(valor, CultureInfo.InvariantCulture)?.Replace('|', '/').Replace('\n', ' ').Replace('\r', ' '));
        }
        Escrever(sb.ToString());
    }

    private static void Escrever(string texto)
    {
        string? arquivo = _arquivo;
        if (arquivo is null) return;

        string carimbo = Relogio.Elapsed.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
        byte[] bytes = new UTF8Encoding(false).GetBytes($"[{carimbo}] {texto}{Environment.NewLine}");
        lock (Trava)
        {
            // Rotação à parte: se não der para renomear agora (arquivo aberto por outro
            // processo), a linha é gravada mesmo assim e a rotação fica para a próxima vez.
            try
            {
                var info = new FileInfo(arquivo);
                if (info.Exists && info.Length > LimiteBytes)
                    File.Move(arquivo, Path.Combine(info.DirectoryName!, "diagnostico.1.log"), overwrite: true);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
            }

            // Outra instância (a segunda abertura, que só pede para esta aparecer) pode estar
            // gravando no mesmo arquivo: poucas tentativas curtas antes de desistir da linha.
            for (int tentativa = 0; tentativa < 4; tentativa++)
            {
                try
                {
                    using var fluxo = new FileStream(arquivo, FileMode.Append, FileAccess.Write, FileShare.Read);
                    fluxo.Write(bytes);
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(2);
                }
                catch (UnauthorizedAccessException)
                {
                    return; // Diagnóstico nunca derruba o aplicativo.
                }
            }
        }
    }
}

using System.IO;
using Buzzy.App.Plataforma;

namespace Buzzy.App.Testes.Integracao;

/// <summary>
/// Perfil de teste do Buzzy aberto pelos testes (<c>--perfil-de-teste NOME</c>, Fase 5): os dados dele ficam em
/// <c>%LOCALAPPDATA%\Buzzy\testes\NOME</c> (<see cref="PastaDeDados.DoPerfilDeTeste"/>), nunca nas configurações reais
/// do usuário. Antes de abrir o Buzzy, o teste apaga a pasta do perfil, para ele partir sem posição salva; a limpeza
/// nunca apaga nada fora dela. A verificação de tela (Buzzy.Verificacao) compila este arquivo e usa a mesma limpeza.
/// </summary>
internal static class PerfilDeTeste
{
    /// <summary>Perfil dos testes de integração.</summary>
    internal const string Integracao = "integracao";

    /// <summary>A pasta do perfil dentro de <paramref name="pastaDoBuzzy"/>: <c>pastaDoBuzzy\testes\perfil</c>.</summary>
    internal static string Pasta(string perfil, string pastaDoBuzzy)
    {
        if (!PastaDeDados.NomeDePerfilValido(perfil))
            throw new ArgumentException($"Perfil de teste inválido: \"{perfil}\".", nameof(perfil));
        return Path.Combine(pastaDoBuzzy, PastaDeDados.NomeDaPastaDeTestes, perfil);
    }

    /// <summary>Apaga a pasta do perfil em <c>%LOCALAPPDATA%\Buzzy\testes</c>, se existir; devolve se apagou.</summary>
    internal static bool Limpar(string perfil)
        => Limpar(perfil, PastaDeDados.DoBuzzy() ?? throw new InvalidOperationException("O Windows não informou a pasta local do usuário."));

    /// <summary>
    /// Apaga <c>pastaDoBuzzy\testes\perfil</c>, se existir, com tudo o que houver dentro; devolve se apagou. Recusa um
    /// nome de perfil inválido (<see cref="PastaDeDados.NomeDePerfilValido"/>) e um caminho com junção ou link simbólico
    /// até a pasta do perfil: a limpeza nunca segue para fora da pasta do Buzzy.
    /// </summary>
    internal static bool Limpar(string perfil, string pastaDoBuzzy)
    {
        string pasta = Pasta(perfil, pastaDoBuzzy);
        if (!Directory.Exists(pasta)) return false;

        string testes = Path.GetDirectoryName(pasta)!;
        foreach (string trecho in new[] { pastaDoBuzzy, testes, pasta })
        {
            if ((File.GetAttributes(trecho) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException($"{trecho} é uma junção ou um link: a limpeza do perfil de teste não segue para fora da pasta do Buzzy.");
        }
        Directory.Delete(pasta, recursive: true);
        return true;
    }
}

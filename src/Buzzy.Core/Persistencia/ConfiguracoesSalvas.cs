using Buzzy.Core.Personagem;

namespace Buzzy.Core.Persistencia;

/// <summary>
/// O que o Buzzy guarda entre execuções no settings.json (Fase 5, ARCHITECTURE.md 2.12): a posição do
/// personagem, quando há uma, e as preferências que o núcleo conhece. É o conteúdo, sem o formato: quem
/// converte bytes é <see cref="EsquemaDeConfiguracoes"/>, e quem lê e grava o arquivo é o adaptador.
/// </summary>
/// <param name="Posicao">
/// A posição do último efeito <see cref="GravarPosicao"/>, com a tela do monitor da época; nula na primeira
/// execução ou quando a gravada era inválida.
/// </param>
/// <param name="Preferencias">Energia, modo de tela cheia e travessia entre monitores.</param>
public sealed record ConfiguracoesSalvas(PosicaoDoPersonagem? Posicao, Preferencias Preferencias)
{
    /// <summary>Sem posição e com as preferências padrão: o que vale sem arquivo ou com um ilegível.</summary>
    public static readonly ConfiguracoesSalvas Padrao = new(null, Preferencias.Padrao);
}

/// <summary>Como terminou a leitura de um settings.json (<see cref="EsquemaDeConfiguracoes.Ler"/>).</summary>
public enum SituacaoDaLeitura
{
    /// <summary>Versão atual do esquema, lida campo a campo; um campo ruim vale o padrão dele, com aviso.</summary>
    Valida,

    /// <summary>
    /// <c>schemaVersion</c> maior que a atual: os campos conhecidos são lidos pelas regras da versão atual,
    /// e a raiz não grava por cima nesta execução, para não apagar os campos que ela não conhece.
    /// </summary>
    VersaoFutura,

    /// <summary>Estrutura inválida (tamanho, UTF-8, JSON, raiz ou <c>schemaVersion</c>): valem as configurações padrão.</summary>
    Ilegivel,
}

/// <summary>Resultado de <see cref="EsquemaDeConfiguracoes.Ler"/>.</summary>
/// <param name="Situacao">Válida, de versão futura ou ilegível.</param>
/// <param name="Versao">O <c>schemaVersion</c> lido; nulo quando o arquivo é ilegível.</param>
/// <param name="Configuracoes">O que vale: o lido, campo a campo, ou <see cref="ConfiguracoesSalvas.Padrao"/> se ilegível.</param>
/// <param name="Avisos">
/// Um por campo ignorado, repetido, fora da faixa ou do tipo errado. Só leva nomes de campo do esquema e o
/// motivo, nunca um valor do arquivo nem o nome de um campo desconhecido (SECURITY.md 6).
/// </param>
/// <param name="MotivoIlegivel">
/// Só quando ilegível, a primeira checagem que falhou: <c>"tamanho"</c>, <c>"utf8"</c>, <c>"json"</c>,
/// <c>"raiz"</c> ou <c>"schemaVersion"</c>.
/// </param>
public sealed record LeituraDasConfiguracoes(
    SituacaoDaLeitura Situacao,
    int? Versao,
    ConfiguracoesSalvas Configuracoes,
    IReadOnlyList<string> Avisos,
    string? MotivoIlegivel);

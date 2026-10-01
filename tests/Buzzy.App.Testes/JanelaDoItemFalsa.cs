using System.Windows.Media.Imaging;
using Buzzy.App.Composicao;
using Buzzy.Core;
using Buzzy.Core.Entrada;

namespace Buzzy.App.Testes;

/// <summary>
/// Janela de item falsa para os testes sem janela (<see cref="GerenteDosItensTestes"/> e <see cref="LigacaoDosItensTestes"/>):
/// guarda as chamadas na ordem e dispara eventos de ponteiro como a de verdade.
/// </summary>
internal sealed class JanelaDoItemFalsa(int id) : IJanelaDoItem
{
    internal int Id { get; } = id;

    public nint Hwnd { get; } = 1000 + id;

    public bool Capturando { get; private set; }

    public event Action<EventoDePonteiro>? Ponteiro;

    internal List<string> Chamadas { get; } = [];

    internal BitmapSource? Sprite { get; private set; }

    internal int Ouvintes => Ponteiro?.GetInvocationList().Length ?? 0;

    public void DefinirSprite(BitmapSource sprite)
    {
        Sprite = sprite;
        Chamadas.Add($"sprite {sprite.PixelWidth}x{sprite.PixelHeight}");
    }

    public void AplicarRetangulo(RetanguloPx retangulo) => Chamadas.Add($"retangulo {retangulo}");

    public void Mostrar() => Chamadas.Add("mostrar");

    public void Esconder() => Chamadas.Add("esconder");

    public void ColocarAbaixoDe(nint hwnd) => Chamadas.Add($"abaixo de {hwnd}");

    public void TrazerParaFrente() => Chamadas.Add("frente");

    public void Capturar()
    {
        Capturando = true;
        Chamadas.Add("capturar");
    }

    public void SoltarCaptura()
    {
        Capturando = false;
        Chamadas.Add("soltar");
    }

    public void Fechar() => Chamadas.Add("fechar");

    internal void Disparar(EventoDePonteiro evento) => Ponteiro?.Invoke(evento);
}

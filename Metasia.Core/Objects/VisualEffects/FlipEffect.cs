using Metasia.Core.Attributes;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

/// <summary>
/// フリップエフェクト - 水平方向と垂直方向の反転をつけることができる
/// </summary>
[VisualEffectIdentifier("FlipEffect")]
public class FlipEffect : VisualEffectBase
{
    [EditableProperty("FlipHorizontal")]
    public bool FlipHorizontal { get; set; } = false;

    [EditableProperty("FlipVertical")]
    public bool FlipVertical { get; set; } = false;

    public override SKImage Apply(SKImage input, VisualEffectContext context)
    {
        if (input is null) return input;

        int width = input.Width;
        int height = input.Height;

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        float scaleX = FlipHorizontal ? -1 : 1;
        float scaleY = FlipVertical ? -1 : 1;
        if (FlipHorizontal)
        {
            canvas.Translate(width, 0);
        }
        if (FlipVertical)
        {
            canvas.Translate(0, height);
        }
        canvas.Scale(scaleX, scaleY);
        canvas.DrawImage(input, 0, 0, paint);

        return surface.Snapshot();
    }
}
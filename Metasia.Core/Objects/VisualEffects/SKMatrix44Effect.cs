using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

/// <summary>
/// SKMatrix44を用いた疑似3Dエフェクト
/// </summary>
[VisualEffectIdentifier("SKMatrix44Effect", DisplayKey = "effect.visual.skmatrix44.name", FallbackText = "SKMatrix44")]
public class SKMatrix44Effect : VisualEffectBase
{
    [EditableProperty("X", DisplayKey = "property.effect.skmatrix44.x", FallbackText = "X")]
    [ValueRange(-99999, 99999, -360, 360)]
    public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);
    
    [EditableProperty("Y", DisplayKey = "property.effect.skmatrix44.y", FallbackText = "Y")]
    [ValueRange(-99999, 99999, -360, 360)]
    public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);
    
    [EditableProperty("Z", DisplayKey = "property.effect.skmatrix44.z", FallbackText = "Z")]
    [ValueRange(0, 99999, 0, 2000)]
    public MetaNumberParam<double> Z { get; set; } = new MetaNumberParam<double>(1000);
    
    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        double cameraZ = Z.Get(context.Frame, context.ClipLength);

        // Z = 1000, focalLength = 1000のとき,対象の描画サイズは1倍になる
        double focalLength = 1000.0;

        int width = input.Width;
        int height = input.Height;

        double angleX = X.Get(context.Frame, context.ClipLength);
        double angleY = Y.Get(context.Frame, context.ClipLength);

        var src3 = new[]
        {
            new SKPoint3(-width * 0.5f, -height * 0.5f, 0),
            new SKPoint3(width * 0.5f, -height * 0.5f, 0),
            new SKPoint3(width * 0.5f, height * 0.5f, 0),
            new SKPoint3(-width * 0.5f, height * 0.5f, 0),
        };

        var m = SKMatrix44.CreateIdentity();
        m = m.PreConcat(SKMatrix44.CreateRotationDegrees(1, 0, 0, (float)angleX));
        m = m.PreConcat(SKMatrix44.CreateRotationDegrees(0, 1, 0, (float)angleY));

        var positions = new SKPoint[4];
        for (int i = 0; i < 4; i++)
        {
            SKPoint3 p3 = m.MapPoint(src3[i]);
            positions[i] = ProjectPoint(p3, width * 0.5f, height * 0.5f, (float)cameraZ, (float)focalLength);
        }

        var texs = new[]
        {
            new SKPoint(0, 0),
            new SKPoint(width, 0),
            new SKPoint(width, height),
            new SKPoint(0, height),
        };

        var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

        using var vertices = SKVertices.CreateCopy(
            SKVertexMode.Triangles,
            positions,
            texs,
            null,
            indices
        );

        using var shader = input.ToShader(
            SKShaderTileMode.Clamp,
            SKShaderTileMode.Clamp
        );

        using var paint = new SKPaint { IsAntialias = true, Shader = shader };

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = context.SurfaceFactory.CreateSurface(info);
        var drawImage = context.SurfaceFactory.GetDrawImage(input);

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.DrawVertices(vertices, SKBlendMode.Modulate, paint);

        var result = context.SurfaceFactory.Snapshot(surface, context.PreferRasterOutput);
        
        return new VisualEffectResult(result, IRenderImageCache.NO_CACHE_KEY, context.LogicalSize);
    }

    static SKPoint ProjectPoint(SKPoint3 p, float cx, float cy, float cameraZ, float focalLength)
    {
        float z = p.Z + cameraZ;
        z = Math.Max(z, 1f);

        float scale = focalLength / z;
        return new SKPoint(cx + p.X * scale, cy + p.Y * scale);
    }
}

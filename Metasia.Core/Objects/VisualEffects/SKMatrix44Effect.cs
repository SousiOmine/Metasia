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
    [EditableProperty("SKMatrix44 X", DisplayKey = "property.effect.skmatrix44.x", FallbackText = "X")]
    [ValueRange(-99999, 99999, -360, 360)]
    public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);
    
    [EditableProperty("SKMatrix44 Y", DisplayKey = "property.effect.skmatrix44.y", FallbackText = "Y")]
    [ValueRange(-99999, 99999, -360, 360)]
    public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);
    
    [EditableProperty("SKMatrix44 Z", DisplayKey = "property.effect.skmatrix44.z", FallbackText = "Z")]
    [ValueRange(1, 99999, 1, 2000)]
    public MetaNumberParam<double> Z { get; set; } = new MetaNumberParam<double>(1000);

    // グリッド分割数: 画像を細かいメッシュに分割して、
    // 透視投影とテクスチャ線形補間のミスマッチを緩和する
    private const int GridW = 10;
    private const int GridH = 10;
    
    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        double cameraZ = Z.Get(context.Frame, context.ClipLength);
        double focalLength = 1000.0;

        int width = input.Width;
        int height = input.Height;

        double angleX = X.Get(context.Frame, context.ClipLength);
        double angleY = Y.Get(context.Frame, context.ClipLength);

        var m = SKMatrix44.CreateIdentity();
        m = m.PreConcat(SKMatrix44.CreateRotationDegrees(1, 0, 0, (float)angleX));
        m = m.PreConcat(SKMatrix44.CreateRotationDegrees(0, 1, 0, (float)angleY));

        // グリッドの頂点ごとに3D変換・投影
        var projectedPoints = new SKPoint[GridW + 1, GridH + 1];
        for (int gy = 0; gy <= GridH; gy++)
        {
            float v = (float)gy / GridH; // 0..1
            float ySrc = v * height - height * 0.5f;
            for (int gx = 0; gx <= GridW; gx++)
            {
                float u = (float)gx / GridW; // 0..1
                float xSrc = u * width - width * 0.5f;

                var srcPoint = new SKPoint3(xSrc, ySrc, 0);
                SKPoint3 transformed = m.MapPoint(srcPoint);
                projectedPoints[gx, gy] = ProjectPoint(transformed, width * 0.5f, height * 0.5f, (float)cameraZ, (float)focalLength);
            }
        }

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        for (int gy = 0; gy <= GridH; gy++)
        {
            for (int gx = 0; gx <= GridW; gx++)
            {
                SKPoint p = projectedPoints[gx, gy];
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
        }

        int newWidth = Math.Max(1, (int)Math.Ceiling(maxX - minX));
        int newHeight = Math.Max(1, (int)Math.Ceiling(maxY - minY));

        // 頂点バッファ構築
        int vertCount = (GridW + 1) * (GridH + 1);
        var positions = new SKPoint[vertCount];
        var texs = new SKPoint[vertCount];

        for (int gy = 0; gy <= GridH; gy++)
        {
            for (int gx = 0; gx <= GridW; gx++)
            {
                int idx = gy * (GridW + 1) + gx;
                positions[idx] = new SKPoint(
                    projectedPoints[gx, gy].X - minX,
                    projectedPoints[gx, gy].Y - minY
                );
                texs[idx] = new SKPoint(
                    (float)gx / GridW * width,
                    (float)gy / GridH * height
                );
            }
        }

        // インデックスバッファ構築（各セルを2つの三角形に分割）
        var indices = new List<ushort>();
        for (int gy = 0; gy < GridH; gy++)
        {
            for (int gx = 0; gx < GridW; gx++)
            {
                int topLeft = gy * (GridW + 1) + gx;
                int topRight = topLeft + 1;
                int bottomLeft = (gy + 1) * (GridW + 1) + gx;
                int bottomRight = bottomLeft + 1;

                // 三角形1: 左上, 右上, 右下
                indices.Add((ushort)topLeft);
                indices.Add((ushort)topRight);
                indices.Add((ushort)bottomRight);

                // 三角形2: 左上, 右下, 左下
                indices.Add((ushort)topLeft);
                indices.Add((ushort)bottomRight);
                indices.Add((ushort)bottomLeft);
            }
        }

        using var vertices = SKVertices.CreateCopy(
            SKVertexMode.Triangles,
            positions,
            texs,
            null,
            indices.ToArray()
        );

        using var shader = input.ToShader(
            SKShaderTileMode.Clamp,
            SKShaderTileMode.Clamp
        );

        using var paint = new SKPaint { IsAntialias = true, Shader = shader };

        var info = new SKImageInfo(newWidth, newHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = context.SurfaceFactory.CreateSurface(info);

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.DrawVertices(vertices, SKBlendMode.Modulate, paint);

        var result = context.SurfaceFactory.Snapshot(surface, context.PreferRasterOutput);

        float logicalScaleX = context.LogicalSize.Width / width;
        float logicalScaleY = context.LogicalSize.Height / height;

        var newLogicalSize = new SKSize(newWidth / logicalScaleX, newHeight / logicalScaleY);

        return new VisualEffectResult(result, IRenderImageCache.NO_CACHE_KEY, newLogicalSize);
    }

    static SKPoint ProjectPoint(SKPoint3 p, float cx, float cy, float cameraZ, float focalLength)
    {
        float z = p.Z + cameraZ;
        z = Math.Max(z, 1f);

        float scale = focalLength / z;
        return new SKPoint(cx + p.X * scale, cy + p.Y * scale);
    }
}

using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("ChromaKeyEffect", DisplayKey = "effect.visual.chroma_key.name", FallbackText = "クロマキー")]
public class ChromaKeyEffect : VisualEffectBase
{
    [EditableProperty("KeyColor", DisplayKey = "property.effect.chroma_key.key_color", FallbackText = "キー色")]
    public ColorRgb8 KeyColor { get; set; } = new ColorRgb8(0, 255, 0);

    [EditableProperty("Similarity", DisplayKey = "property.effect.chroma_key.similarity", FallbackText = "類似度")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Similarity { get; set; } = new MetaNumberParam<double>(50);

    [EditableProperty("Smoothness", DisplayKey = "property.effect.chroma_key.smoothness", FallbackText = "滑らかさ")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Smoothness { get; set; } = new MetaNumberParam<double>(10);

    private static readonly SKColorType ColorType = SKColorType.Rgba8888;
    private static readonly SKAlphaType AlphaType = SKAlphaType.Premul;

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        double similarity = Similarity.Get(relativeFrame, clipLength);
        if (similarity <= 0)
        {
            return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);
        }

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            var cachedImage = context.ImageCache?.TryGet(cacheKey);
            if (cachedImage is not null)
            {
                return new VisualEffectResult(cachedImage, cacheKey, context.LogicalSize);
            }
        }

        int width = input.Width;
        int height = input.Height;
        var info = new SKImageInfo(width, height, ColorType, AlphaType);

        using var surface = context.SurfaceFactory.CreateSurface(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var drawImage = context.SurfaceFactory.GetDrawImage(input);
        try
        {
            canvas.DrawImage(drawImage, 0, 0);
        }
        finally
        {
            if (!ReferenceEquals(drawImage, input))
            {
                drawImage.Dispose();
            }
        }

        int stride = width * 4;
        byte[] pixels = new byte[stride * height];

        var readPixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            surface.ReadPixels(info, readPixelsHandle.AddrOfPinnedObject(), stride, 0, 0);
        }
        finally
        {
            readPixelsHandle.Free();
        }

        double smoothness = Smoothness.Get(relativeFrame, clipLength);
        ProcessChromaKey(pixels, width, height, KeyColor, similarity / 100.0, smoothness / 100.0);

        using var outputSurface = context.SurfaceFactory.CreateSurface(info);
        var outputCanvas = outputSurface.Canvas;
        outputCanvas.Clear(SKColors.Transparent);

        var pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            using var outputBitmap = new SKBitmap();
            outputBitmap.InstallPixels(info, pixelsHandle.AddrOfPinnedObject(), stride);
            outputCanvas.DrawBitmap(outputBitmap, 0, 0);
        }
        finally
        {
            pixelsHandle.Free();
        }

        var resultImage = context.SurfaceFactory.Snapshot(outputSurface, context.PreferRasterOutput);

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            context.ImageCache?.Set(cacheKey, resultImage);
            return new VisualEffectResult(resultImage, cacheKey, context.LogicalSize);
        }
        else
        {
            return new VisualEffectResult(resultImage, IRenderImageCache.NO_CACHE_KEY, context.LogicalSize);
        }
    }

    private static void ProcessChromaKey(byte[] pixels, int width, int height, ColorRgb8 keyColor, double similarity, double smoothness)
    {
        float keyR = keyColor.R / 255f;
        float keyG = keyColor.G / 255f;
        float keyB = keyColor.B / 255f;
        float similarityF = (float)similarity;
        float smoothnessF = Math.Max((float)smoothness, 0.001f);

        float threshold = 1f - similarityF;
        float outerThreshold = threshold + smoothnessF;
        float maxDist = MathF.Sqrt(3f);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            float a = pixels[i + 3] / 255f;
            if (a <= 0f) continue;

            float r = Math.Clamp((pixels[i + 0] / 255f) / a, 0f, 1f);
            float g = Math.Clamp((pixels[i + 1] / 255f) / a, 0f, 1f);
            float b = Math.Clamp((pixels[i + 2] / 255f) / a, 0f, 1f);

            float dr = r - keyR;
            float dg = g - keyG;
            float db = b - keyB;
            float dist = MathF.Sqrt(dr * dr + dg * dg + db * db);
            float diff = dist / maxDist;

            float alphaFactor;
            if (diff <= threshold)
            {
                alphaFactor = 0f;
            }
            else if (diff >= outerThreshold)
            {
                alphaFactor = 1f;
            }
            else
            {
                alphaFactor = (diff - threshold) / (outerThreshold - threshold);
            }

            float newAlpha = a * alphaFactor;
            pixels[i + 0] = (byte)(Math.Clamp(r * newAlpha, 0f, 1f) * 255f + 0.5f);
            pixels[i + 1] = (byte)(Math.Clamp(g * newAlpha, 0f, 1f) * 255f + 0.5f);
            pixels[i + 2] = (byte)(Math.Clamp(b * newAlpha, 0f, 1f) * 255f + 0.5f);
            pixels[i + 3] = (byte)(Math.Clamp(newAlpha, 0f, 1f) * 255f + 0.5f);
        }
    }

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(ChromaKeyEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(Similarity.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Smoothness.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(KeyColor.R);
        hash.Add(KeyColor.G);
        hash.Add(KeyColor.B);
        return hash.ToHashCode();
    }
}

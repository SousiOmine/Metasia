using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("MosaicEffect", DisplayKey = "effect.visual.mosaic.name", FallbackText = "モザイク")]
public class MosaicEffect : VisualEffectBase
{
    [EditableProperty("BlockSize", DisplayKey = "property.effect.mosaic.block_size", FallbackText = "ブロックサイズ")]
    [ValueRange(2, 100, 2, 20)]
    public MetaNumberParam<double> BlockSize { get; set; } = new MetaNumberParam<double>(10);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        int width = input.Width;
        int height = input.Height;

        float logicalScale = context.LogicalSize.Width > 0 ? width / context.LogicalSize.Width : 1f;
        int blockSize = Math.Max(2, (int)(BlockSize.Get(relativeFrame, clipLength) * logicalScale));

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            var cachedImage = context.ImageCache?.TryGet(cacheKey);
            if (cachedImage != null)
            {
                return new VisualEffectResult(cachedImage, cacheKey, context.LogicalSize);
            }
        }

        int smallWidth = Math.Max(1, width / blockSize);
        int smallHeight = Math.Max(1, height / blockSize);

        var smallInfo = new SKImageInfo(smallWidth, smallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var smallSurface = context.SurfaceFactory.CreateSurface(smallInfo);
        var smallCanvas = smallSurface.Canvas;
        smallCanvas.Clear(SKColors.Transparent);

        var drawImage = context.SurfaceFactory.GetDrawImage(input);
        try
        {
            var nearestSampling = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None);
            using var paint = new SKPaint { IsAntialias = false };
            smallCanvas.DrawImage(drawImage, new SKRect(0, 0, smallWidth, smallHeight), nearestSampling, paint);
        }
        finally
        {
            if (!ReferenceEquals(drawImage, input))
            {
                drawImage.Dispose();
            }
        }

        var smallImage = context.SurfaceFactory.Snapshot(smallSurface, context.PreferRasterOutput);

        var fullInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var fullSurface = context.SurfaceFactory.CreateSurface(fullInfo);
        var fullCanvas = fullSurface.Canvas;
        fullCanvas.Clear(SKColors.Transparent);

        var nearestSampling2 = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None);
        using var paint2 = new SKPaint { IsAntialias = false };
        fullCanvas.DrawImage(smallImage, new SKRect(0, 0, width, height), nearestSampling2, paint2);
        smallImage.Dispose();

        var result = context.SurfaceFactory.Snapshot(fullSurface, context.PreferRasterOutput);

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            context.ImageCache?.Set(cacheKey, result);
            return new VisualEffectResult(result, cacheKey, context.LogicalSize);
        }
        else
        {
            return new VisualEffectResult(result, IRenderImageCache.NO_CACHE_KEY, context.LogicalSize);
        }
    }

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(MosaicEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(BlockSize.Get(context.RelativeFrame, context.ClipLength));
        return hash.ToHashCode();
    }
}
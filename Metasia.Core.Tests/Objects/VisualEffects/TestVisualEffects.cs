using System.Xml.Serialization;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    /// <summary>
    /// テスト用の共通ビジュアルエフェクト実装群
    /// XMLシリアライザが全IMetasiaObject実装型をスキャンするため、
    /// publicかつパラメータレスコンストラクタ付きで、名前が一意である必要がある
    /// </summary>

    /// <summary>
    /// 入力画像をそのまま返すパススルーエフェクト
    /// </summary>
    public class TestPassThroughEffect : VisualEffectBase
    {
        [XmlIgnore]
        public int ApplyCallCount { get; private set; }
        [XmlIgnore]
        public VisualEffectContext? LastContext { get; private set; }

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            ApplyCallCount++;
            LastContext = context;
            return new VisualEffectResult(input, context.TargetImageCacheKey);
        }
    }

    /// <summary>
    /// 入力画像を赤一色の画像に置き換えるエフェクト
    /// </summary>
    public class TestRedFillEffect : VisualEffectBase
    {
        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            var info = new SKImageInfo(input.Width, input.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(SKColors.Red);
            return new VisualEffectResult(surface.Snapshot(), context.TargetImageCacheKey);
        }
    }

    /// <summary>
    /// 入力画像を緑一色の画像に置き換えるエフェクト
    /// </summary>
    public class TestGreenFillEffect : VisualEffectBase
    {
        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            var info = new SKImageInfo(input.Width, input.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(SKColors.Green);
            return new VisualEffectResult(surface.Snapshot(), context.TargetImageCacheKey);
        }
    }

    /// <summary>
    /// 入力画像を黄色一色の画像に置き換えるエフェクト
    /// </summary>
    public class TestYellowFillEffect : VisualEffectBase
    {
        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            var info = new SKImageInfo(input.Width, input.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(SKColors.Yellow);
            return new VisualEffectResult(surface.Snapshot(), context.TargetImageCacheKey);
        }
    }

    /// <summary>
    /// 指定したキャッシュキーを返すテスト用エフェクト
    /// </summary>
    public class TestCacheKeyEffect : VisualEffectBase
    {
        [XmlIgnore]
        public long LastReceivedCacheKey { get; private set; } = IRenderImageCache.NO_CACHE_KEY;

        [XmlIgnore]
        public long ReturnedCacheKey { get; set; } = IRenderImageCache.NO_CACHE_KEY;

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            LastReceivedCacheKey = context.TargetImageCacheKey;
            return new VisualEffectResult(input, ReturnedCacheKey);
        }
    }
}

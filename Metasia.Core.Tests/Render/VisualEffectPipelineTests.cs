using Metasia.Core.Media;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using Metasia.Core.Tests.Objects.VisualEffects;
using SkiaSharp;

namespace Metasia.Core.Tests.Render
{
    /// <summary>
    /// VisualEffectPipelineクラスのテスト
    /// </summary>
    [TestFixture]
    public class VisualEffectPipelineTests
    {
        private RenderContext _renderContext = null!;

        [SetUp]
        public void Setup()
        {
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);
            _renderContext = new RenderContext(
                frame: 10,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(1920, 1080),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: projectInfo,
                projectPath: string.Empty);
        }

        [Test]
        public void ApplyEffects_NullEffectsList_ReturnsInputImage()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue);

            // Act
            var result = VisualEffectPipeline.ApplyEffects(input, null!, _renderContext, 0, 100, new SKSize(100, 100));

            // Assert
            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void ApplyEffects_EmptyEffectsList_ReturnsInputImage()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue);
            var effects = new List<VisualEffectBase>();

            // Act
            var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(100, 100));

            // Assert
            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void ApplyEffects_EmptyEffectsList_PreservesLogicalSize()
        {
            using var input = CreateTestImage(SKColors.Blue, 960, 540);
            var effects = new List<VisualEffectBase>();

            var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(1920, 1080));

            Assert.That(result.LogicalSize.Width, Is.EqualTo(1920));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(1080));
        }

        [Test]
        public void ApplyEffects_ContextOverloadWithEmptyEffects_PreservesLogicalSize()
        {
            using var input = CreateTestImage(SKColors.Blue, 960, 540);
            var context = new VisualEffectContext(
                frame: 10,
                relativeFrame: 0,
                clipLength: 100,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                logicalSize: new SKSize(1920, 1080));

            var result = VisualEffectPipeline.ApplyEffects(input, [], context);

            Assert.That(result.LogicalSize.Width, Is.EqualTo(1920));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(1080));
        }

        [Test]
        public void ApplyEffects_SingleEffect_AppliesEffect()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue, 50, 50);
            var effects = new List<VisualEffectBase>
            {
                new TestRedFillEffect()
            };

            // Act
            using var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(50, 50)).Image;

            // Assert
            using var bitmap = SKBitmap.FromImage(result);
            Assert.That(bitmap.GetPixel(25, 25), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void ApplyEffects_MultipleEffects_AppliesInOrder()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue, 50, 50);
            var effects = new List<VisualEffectBase>
            {
                new TestRedFillEffect(),
                new TestGreenFillEffect(),
            };

            // Act - 最後のエフェクト(Green)が最終結果になるはず
            using var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(50, 50)).Image;

            // Assert
            using var bitmap = SKBitmap.FromImage(result);
            Assert.That(bitmap.GetPixel(25, 25), Is.EqualTo(SKColors.Green));
        }

        [Test]
        public void ApplyEffects_InactiveEffect_IsSkipped()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue, 50, 50);
            var inactiveEffect = new TestRedFillEffect { IsActive = false };
            var effects = new List<VisualEffectBase> { inactiveEffect };

            // Act
            var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(50, 50));

            // Assert - 非アクティブなので入力画像がそのまま返る
            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void ApplyEffects_MixedActiveInactive_OnlyAppliesActiveEffects()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue, 50, 50);
            var effects = new List<VisualEffectBase>
            {
                new TestRedFillEffect { IsActive = false },      // スキップ
                new TestGreenFillEffect { IsActive = true },     // 適用
                new TestYellowFillEffect { IsActive = false },   // スキップ
            };

            // Act
            using var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(50, 50)).Image;

            // Assert - Greenのみ適用される
            using var bitmap = SKBitmap.FromImage(result);
            Assert.That(bitmap.GetPixel(25, 25), Is.EqualTo(SKColors.Green));
        }

        [Test]
        public void ApplyEffects_AllInactive_ReturnsInputImage()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue, 50, 50);
            var passThroughEffect = new TestPassThroughEffect { IsActive = false };
            var effects = new List<VisualEffectBase> { passThroughEffect };

            // Act
            var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(50, 50));

            // Assert
            Assert.That(result.Image, Is.SameAs(input));
            Assert.That(passThroughEffect.ApplyCallCount, Is.EqualTo(0));
        }

        [Test]
        public void ApplyEffects_EffectsReceiveCorrectContext()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue);
            var effect = new TestPassThroughEffect();
            var effects = new List<VisualEffectBase> { effect };

            // Act
            VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 5, 50, new SKSize(100, 100));

            // Assert
            Assert.That(effect.ApplyCallCount, Is.EqualTo(1));
        }

        [Test]
        public void ApplyEffects_ChainedEffects_EachReceivesPreviousOutput()
        {
            // Arrange
            using var input = CreateTestImage(SKColors.Blue, 50, 50);

            // 1つ目: 青→赤に変換, 2つ目: パススルー（入力をそのまま返す）
            var passThroughEffect = new TestPassThroughEffect();
            var effects = new List<VisualEffectBase>
            {
                new TestRedFillEffect(),
                passThroughEffect,
            };

            // Act
            using var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(50, 50)).Image;

            // Assert - パススルーが受け取った結果（赤）がそのまま返される
            Assert.That(passThroughEffect.ApplyCallCount, Is.EqualTo(1));
            using var bitmap = SKBitmap.FromImage(result);
            Assert.That(bitmap.GetPixel(25, 25), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void ApplyEffects_NoEffects_PreservesNoCacheKeyByDefault()
        {
            using var input = CreateTestImage(SKColors.Blue);
            var effects = new List<VisualEffectBase>();

            var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(100, 100));

            Assert.That(result.Image, Is.SameAs(input));
            Assert.That(result.ImageCacheKey, Is.EqualTo(IRenderImageCache.NO_CACHE_KEY));
        }

        [Test]
        public void ApplyEffects_UsesFinalEffectCacheKey()
        {
            using var input = CreateTestImage(SKColors.Blue);
            var first = new TestCacheKeyEffect { ReturnedCacheKey = 111 };
            var second = new TestCacheKeyEffect { ReturnedCacheKey = 222 };
            var effects = new List<VisualEffectBase> { first, second };

            var result = VisualEffectPipeline.ApplyEffects(input, effects, _renderContext, 0, 100, new SKSize(100, 100), imageCacheKey: 10);

            Assert.That(first.LastReceivedCacheKey, Is.EqualTo(10));
            Assert.That(second.LastReceivedCacheKey, Is.EqualTo(111));
            Assert.That(result.ImageCacheKey, Is.EqualTo(222));
        }

        #region ヘルパーメソッド

        private static SKImage CreateTestImage(SKColor color, int width = 100, int height = 100)
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(color);
            return surface.Snapshot();
        }

        #endregion
    }
}

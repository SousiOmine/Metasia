using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class BorderEffectTests
    {
        private static VisualEffectContext CreateContext(int relativeFrame = 0, int clipLength = 100)
        {
            return new VisualEffectContext(relativeFrame, relativeFrame, clipLength,
                new SKSize(1920, 1080), new SKSize(1920, 1080), new SKSize(100, 100));
        }

        private static SKImage CreateTestImage(SKColor color, int width = 50, int height = 50)
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(color);
            return surface.Snapshot();
        }

        [Test]
        public void Apply_ZeroSize_ReturnsSameImage()
        {
            var effect = new BorderEffect();
            effect.Size = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_WithBorder_OutputIsLarger()
        {
            var effect = new BorderEffect();
            effect.Size = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            // 論理サイズ100に対して入力画像50なので、描画側では約半分のピクセル拡張になる
            Assert.That(result.Image.Width, Is.EqualTo(input.Width + 6));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height + 6));
            Assert.That(result.LogicalSize.Width, Is.EqualTo(110));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(110));
        }

        [Test]
        public void Apply_WithBorder_CenterPreservesOriginal()
        {
            var effect = new BorderEffect();
            effect.Size = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5);
            effect.Color = new ColorRgb8(0, 255, 0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var bitmap = SKBitmap.FromImage(result.Image);
            // 中央はオリジナルの赤が保持されている
            Assert.That(bitmap.GetPixel(result.Image.Width / 2, result.Image.Height / 2), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void Apply_WithScaledRenderSize_PreservesLogicalExpansion()
        {
            var effect = new BorderEffect();
            effect.Size = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5);
            using var input = CreateTestImage(SKColors.Red, 960, 540);
            var context = new VisualEffectContext(
                frame: 0,
                relativeFrame: 0,
                clipLength: 100,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                logicalSize: new SKSize(1920, 1080));

            var result = effect.Apply(input, context);

            Assert.That(result.LogicalSize.Width, Is.EqualTo(1930));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(1090));
        }

        [Test]
        public void DefaultColor_IsBlack()
        {
            var effect = new BorderEffect();
            Assert.That(effect.Color.R, Is.EqualTo(0));
            Assert.That(effect.Color.G, Is.EqualTo(0));
            Assert.That(effect.Color.B, Is.EqualTo(0));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(BorderEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("BorderEffect"));
        }
    }
}

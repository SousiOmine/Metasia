using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class ClippingEffectTests
    {
        private static VisualEffectContext CreateContext(int relativeFrame = 0, int clipLength = 100)
        {
            return new VisualEffectContext(relativeFrame, relativeFrame, clipLength,
                new SKSize(1920, 1080), new SKSize(1920, 1080), new SKSize(100, 100));
        }

        private static SKImage CreateTestImage(SKColor color, int width = 100, int height = 100)
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(color);
            return surface.Snapshot();
        }

        [Test]
        public void Apply_ZeroClipping_ReturnsSameImage()
        {
            var effect = new ClippingEffect();
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.SameAs(input));
        }

        [Test]
        public void Apply_ClipTop_MakesTopTransparent()
        {
            var effect = new ClippingEffect();
            effect.Top = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(20);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            using var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            using var bitmap = SKBitmap.FromImage(result);
            // 上部20pxはクリッピングされて透明になる
            Assert.That(bitmap.GetPixel(50, 5).Alpha, Is.EqualTo(0));
            // 下部はそのまま残る
            Assert.That(bitmap.GetPixel(50, 50), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void Apply_ClipLeft_MakesLeftTransparent()
        {
            var effect = new ClippingEffect();
            effect.Left = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(30);
            using var input = CreateTestImage(SKColors.Blue);
            var context = CreateContext();

            using var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            using var bitmap = SKBitmap.FromImage(result);
            Assert.That(bitmap.GetPixel(5, 50).Alpha, Is.EqualTo(0));
            Assert.That(bitmap.GetPixel(60, 50), Is.EqualTo(SKColors.Blue));
        }

        [Test]
        public void Apply_AllSidesClipped_OutputIsSameSize()
        {
            var effect = new ClippingEffect();
            effect.Top = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            effect.Bottom = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            effect.Left = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            effect.Right = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            using var input = CreateTestImage(SKColors.Green);
            var context = CreateContext();

            using var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Width, Is.EqualTo(input.Width));
            Assert.That(result.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(ClippingEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("ClippingEffect"));
        }
    }
}

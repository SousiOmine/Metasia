using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class AreaExpansionEffectTests
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
        public void Apply_ZeroExpansion_ReturnsSameImage()
        {
            var effect = new AreaExpansionEffect();
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_WithExpansion_OutputIsLarger()
        {
            var effect = new AreaExpansionEffect
            {
                Top = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10),
                Bottom = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10),
                Left = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10),
                Right = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10),
            };
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.EqualTo(60));
            Assert.That(result.Image.Height, Is.EqualTo(60));
            Assert.That(result.LogicalSize.Width, Is.EqualTo(120));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(120));
        }

        [Test]
        public void Apply_WithExpansion_CenterPreservesOriginal()
        {
            var effect = new AreaExpansionEffect
            {
                Top = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Bottom = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Left = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Right = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
            };
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var bitmap = SKBitmap.FromImage(result.Image);
            Assert.That(bitmap.GetPixel(result.Image.Width / 2, result.Image.Height / 2), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void Apply_ExpandedArea_IsTransparent()
        {
            var effect = new AreaExpansionEffect
            {
                Top = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Bottom = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Left = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Right = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
            };
            using var input = CreateTestImage(SKColors.Blue);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var bitmap = SKBitmap.FromImage(result.Image);
            // 拡張領域は透明
            Assert.That(bitmap.GetPixel(0, 0).Alpha, Is.EqualTo(0));
            Assert.That(bitmap.GetPixel(result.Image.Width - 1, result.Image.Height - 1).Alpha, Is.EqualTo(0));
        }

        [Test]
        public void Apply_WithScaledRenderSize_PreservesLogicalExpansion()
        {
            var effect = new AreaExpansionEffect
            {
                Top = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Bottom = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5),
                Left = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10),
                Right = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10),
            };
            using var input = CreateTestImage(SKColors.Red, 960, 540);
            var context = new VisualEffectContext(
                frame: 0,
                relativeFrame: 0,
                clipLength: 100,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                logicalSize: new SKSize(1920, 1080));

            var result = effect.Apply(input, context);

            Assert.That(result.LogicalSize.Width, Is.EqualTo(1940));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(1090));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(AreaExpansionEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("AreaExpansionEffect"));
        }
    }
}

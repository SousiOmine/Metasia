using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class MonochromeEffectTests
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
        public void Apply_ZeroIntensity_ReturnsSameImage()
        {
            var effect = new MonochromeEffect();
            effect.Intensity = new MetaNumberParam<double>(0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_FullIntensity_ProducesOutput()
        {
            var effect = new MonochromeEffect();
            effect.Intensity = new MetaNumberParam<double>(100);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_FullIntensity_GrayedImage()
        {
            var effect = new MonochromeEffect();
            effect.Intensity = new MetaNumberParam<double>(100);
            effect.TintColor = new ColorRgb8(255, 255, 255);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Red, Is.EqualTo(pixel.Green).Within(1));
            Assert.That(pixel.Red, Is.EqualTo(pixel.Blue).Within(1));
        }

        [Test]
        public void Apply_WithTintColor_ColoredOutput()
        {
            var effect = new MonochromeEffect();
            effect.Intensity = new MetaNumberParam<double>(100);
            effect.TintColor = new ColorRgb8(0, 0, 255);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Blue, Is.GreaterThan(pixel.Red));
        }

        [Test]
        public void Apply_PartialIntensity_PreservesSomeColor()
        {
            var effect = new MonochromeEffect();
            effect.Intensity = new MetaNumberParam<double>(50);
            effect.TintColor = new ColorRgb8(255, 255, 255);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            using var inputBitmap = SKBitmap.FromImage(input);
            var resultPixel = resultBitmap.GetPixel(50, 50);
            var inputPixel = inputBitmap.GetPixel(50, 50);
            Assert.That(resultPixel.Red, Is.GreaterThan(0));
        }

        [Test]
        public void DefaultValues()
        {
            var effect = new MonochromeEffect();
            Assert.That(effect.Intensity.StartPoint.Value, Is.EqualTo(100));
            Assert.That(effect.TintColor.R, Is.EqualTo(255));
            Assert.That(effect.TintColor.G, Is.EqualTo(255));
            Assert.That(effect.TintColor.B, Is.EqualTo(255));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(MonochromeEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("MonochromeEffect"));
        }
    }
}
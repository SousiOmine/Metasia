using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class ColorCorrectionEffectTests
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
        public void Apply_DefaultValues_ReturnsSameImage()
        {
            var effect = new ColorCorrectionEffect();
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_BrightnessChange_OutputNotSameImage()
        {
            var effect = new ColorCorrectionEffect();
            effect.Brightness = new MetaNumberParam<double>(50);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image, Is.Not.SameAs(input));
        }

        [Test]
        public void Apply_BrightnessChange_OutputSameSize()
        {
            var effect = new ColorCorrectionEffect();
            effect.Brightness = new MetaNumberParam<double>(50);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_BrightnessIncrease_LightensPixels()
        {
            var effect = new ColorCorrectionEffect();
            effect.Brightness = new MetaNumberParam<double>(150);
            using var input = CreateTestImage(new SKColor(128, 0, 0, 255));
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Red, Is.GreaterThan(128));
        }

        [Test]
        public void Apply_BrightnessZero_BlackPixels()
        {
            var effect = new ColorCorrectionEffect();
            effect.Brightness = new MetaNumberParam<double>(0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Red, Is.EqualTo(0));
            Assert.That(pixel.Green, Is.EqualTo(0));
            Assert.That(pixel.Blue, Is.EqualTo(0));
        }

        [Test]
        public void Apply_ContrastChange_OutputSameSize()
        {
            var effect = new ColorCorrectionEffect();
            effect.Contrast = new MetaNumberParam<double>(50);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_SaturationChange_OutputSameSize()
        {
            var effect = new ColorCorrectionEffect();
            effect.Saturation = new MetaNumberParam<double>(50);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_GammaChange_OutputSameSize()
        {
            var effect = new ColorCorrectionEffect();
            effect.Gamma = new MetaNumberParam<double>(2.0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_HueShift_OutputSameSize()
        {
            var effect = new ColorCorrectionEffect();
            effect.HueShift = new MetaNumberParam<double>(90);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_HueShift_ChangesColor()
        {
            var effect = new ColorCorrectionEffect();
            effect.HueShift = new MetaNumberParam<double>(120);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            using var inputBitmap = SKBitmap.FromImage(input);
            var resultPixel = resultBitmap.GetPixel(50, 50);
            var inputPixel = inputBitmap.GetPixel(50, 50);
            Assert.That(resultPixel.Red, Is.Not.EqualTo(inputPixel.Red));
        }

        [Test]
        public void DefaultValues()
        {
            var effect = new ColorCorrectionEffect();
            Assert.That(effect.Brightness.StartPoint.Value, Is.EqualTo(100));
            Assert.That(effect.Contrast.StartPoint.Value, Is.EqualTo(0));
            Assert.That(effect.Saturation.StartPoint.Value, Is.EqualTo(0));
            Assert.That(effect.HueShift.StartPoint.Value, Is.EqualTo(0));
            Assert.That(effect.Gamma.StartPoint.Value, Is.EqualTo(1));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(ColorCorrectionEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("ColorCorrectionEffect"));
        }
    }
}
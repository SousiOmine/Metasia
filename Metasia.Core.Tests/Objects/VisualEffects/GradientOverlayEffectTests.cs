using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class GradientOverlayEffectTests
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
        public void Apply_ZeroOpacity_ReturnsSameImage()
        {
            var effect = new GradientOverlayEffect();
            effect.Opacity = new MetaNumberParam<double>(0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_WithGradient_OutputSameSize()
        {
            var effect = new GradientOverlayEffect();
            effect.Opacity = new MetaNumberParam<double>(50);
            using var input = CreateTestImage(SKColors.White);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_WithGradient_ModifiesPixels()
        {
            var effect = new GradientOverlayEffect();
            effect.Opacity = new MetaNumberParam<double>(100);
            effect.StartColor = new ColorRgb8(0, 0, 0);
            effect.EndColor = new ColorRgb8(255, 255, 255);
            effect.Angle = new MetaNumberParam<double>(0);
            using var input = CreateTestImage(SKColors.Transparent);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var inputBitmap = SKBitmap.FromImage(input);
            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var resultPixel = resultBitmap.GetPixel(50, 50);
            var inputPixel = inputBitmap.GetPixel(50, 50);
            Assert.That(resultPixel.Alpha, Is.Not.EqualTo(inputPixel.Alpha));
        }

        [Test]
        public void Apply_WithGradient_PreservesLogicalSize()
        {
            var effect = new GradientOverlayEffect();
            effect.Opacity = new MetaNumberParam<double>(50);
            using var input = CreateTestImage(SKColors.White);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.LogicalSize.Width, Is.EqualTo(context.LogicalSize.Width));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(context.LogicalSize.Height));
        }

        [Test]
        public void Apply_FullOpacityGradientOverWhite()
        {
            var effect = new GradientOverlayEffect();
            effect.Opacity = new MetaNumberParam<double>(100);
            effect.StartColor = new ColorRgb8(255, 0, 0);
            effect.EndColor = new ColorRgb8(0, 0, 255);
            effect.Angle = new MetaNumberParam<double>(90);
            using var input = CreateTestImage(SKColors.White);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
        }

        [Test]
        public void DefaultValues()
        {
            var effect = new GradientOverlayEffect();
            Assert.That(effect.StartColor.R, Is.EqualTo(0));
            Assert.That(effect.StartColor.G, Is.EqualTo(0));
            Assert.That(effect.StartColor.B, Is.EqualTo(0));
            Assert.That(effect.EndColor.R, Is.EqualTo(255));
            Assert.That(effect.EndColor.G, Is.EqualTo(255));
            Assert.That(effect.EndColor.B, Is.EqualTo(255));
            Assert.That(effect.Angle.StartPoint.Value, Is.EqualTo(0));
            Assert.That(effect.Opacity.StartPoint.Value, Is.EqualTo(50));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(GradientOverlayEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("GradientOverlayEffect"));
        }
    }
}
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class DropShadowEffectTests
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
        public void Apply_ZeroOpacity_ReturnsSameImage()
        {
            var effect = new DropShadowEffect();
            effect.Opacity = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_WithShadow_OutputIsLarger()
        {
            var effect = new DropShadowEffect();
            effect.OffsetX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5);
            effect.OffsetY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5);
            effect.BlurSize = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5);
            effect.Opacity = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(100);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.GreaterThan(input.Width));
            Assert.That(result.Image.Height, Is.GreaterThan(input.Height));
            Assert.That(result.LogicalSize.Width, Is.GreaterThan(context.LogicalSize.Width));
            Assert.That(result.LogicalSize.Height, Is.GreaterThan(context.LogicalSize.Height));
        }

        [Test]
        public void Apply_WithOffset_ShadowMovesCorrectly()
        {
            var effect = new DropShadowEffect();
            effect.OffsetX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            effect.OffsetY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(0);
            effect.BlurSize = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(0);
            effect.Opacity = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(100);
            effect.Color = new ColorRgb8(0, 0, 0);
            var input = CreateTestImage(SKColors.Red, 20, 20);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var bitmap = SKBitmap.FromImage(result.Image);
            int centerY = result.Image.Height / 2;

            int shadowRightEdge = 10 + 20 + 2;
            Assert.That(bitmap.GetPixel(shadowRightEdge + 5, centerY), Is.EqualTo(SKColors.Red));

            int shadowCenterX = 10 + 10 + 1;
            Assert.That(bitmap.GetPixel(shadowCenterX, centerY).Alpha, Is.GreaterThan(0));
        }

        [Test]
        public void Apply_ShadowDoesNotAffectOriginalArea()
        {
            var effect = new DropShadowEffect();
            effect.OffsetX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            effect.OffsetY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            effect.BlurSize = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(5);
            effect.Opacity = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(100);
            effect.Color = new ColorRgb8(0, 255, 0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var bitmap = SKBitmap.FromImage(result.Image);
            Assert.That(bitmap.GetPixel(result.Image.Width / 2, result.Image.Height / 2), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void DefaultColor_IsBlack()
        {
            var effect = new DropShadowEffect();
            Assert.That(effect.Color.R, Is.EqualTo(0));
            Assert.That(effect.Color.G, Is.EqualTo(0));
            Assert.That(effect.Color.B, Is.EqualTo(0));
        }

        [Test]
        public void DefaultOpacity_Is50()
        {
            var effect = new DropShadowEffect();
            Assert.That(effect.Opacity.StartPoint.Value, Is.EqualTo(50));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(DropShadowEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("DropShadowEffect"));
        }
    }
}

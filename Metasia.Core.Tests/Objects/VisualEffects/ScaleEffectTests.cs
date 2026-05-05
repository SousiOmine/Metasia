using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class ScaleEffectTests
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
        public void Apply_IdentityScale_ReturnsSameImage()
        {
            var effect = new ScaleEffect();
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_ScaleX200_WidthDoubled()
        {
            var effect = new ScaleEffect
            {
                ScaleX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(200),
                ScaleY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(100),
            };
            using var input = CreateTestImage(SKColors.Red, 50, 50);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.EqualTo(100));
            Assert.That(result.Image.Height, Is.EqualTo(50));
            Assert.That(result.LogicalSize.Width, Is.EqualTo(200));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(100));
        }

        [Test]
        public void Apply_ScaleY200_HeightDoubled()
        {
            var effect = new ScaleEffect
            {
                ScaleX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(100),
                ScaleY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(200),
            };
            using var input = CreateTestImage(SKColors.Red, 50, 50);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(50));
            Assert.That(result.Image.Height, Is.EqualTo(100));
        }

        [Test]
        public void Apply_ScaleXY200_BothDimensionsDoubled()
        {
            var effect = new ScaleEffect
            {
                ScaleX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(200),
                ScaleY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(200),
            };
            using var input = CreateTestImage(SKColors.Red, 50, 50);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(100));
            Assert.That(result.Image.Height, Is.EqualTo(100));
            Assert.That(result.LogicalSize.Width, Is.EqualTo(200));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(200));
        }

        [Test]
        public void Apply_ScaleDown50_WidthAndHeightHalved()
        {
            var effect = new ScaleEffect
            {
                ScaleX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50),
                ScaleY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50),
            };
            using var input = CreateTestImage(SKColors.Red, 100, 100);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(50));
            Assert.That(result.Image.Height, Is.EqualTo(50));
            Assert.That(result.LogicalSize.Width, Is.EqualTo(50));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(50));
        }

        [Test]
        public void Apply_IndependentXY_WidthAndHeightScaledDifferently()
        {
            var effect = new ScaleEffect
            {
                ScaleX = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(200),
                ScaleY = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50),
            };
            using var input = CreateTestImage(SKColors.Red, 100, 100);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image.Width, Is.EqualTo(200));
            Assert.That(result.Image.Height, Is.EqualTo(50));
            Assert.That(result.LogicalSize.Width, Is.EqualTo(200));
            Assert.That(result.LogicalSize.Height, Is.EqualTo(50));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(ScaleEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("ScaleEffect"));
        }
    }
}

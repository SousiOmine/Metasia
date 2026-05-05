using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class ChromaKeyEffectTests
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

        private static SKImage CreateCheckerPattern(int width = 100, int height = 100)
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var color = (x / 10 + y / 10) % 2 == 0
                        ? new SKColor(0, 255, 0, 255)
                        : new SKColor(255, 0, 0, 255);
                    using var paint = new SKPaint { Color = color };
                    canvas.DrawPoint(x, y, color);
                }
            }

            return surface.Snapshot();
        }

        [Test]
        public void Apply_ZeroSimilarity_ReturnsSameImage()
        {
            var effect = new ChromaKeyEffect();
            effect.Similarity = new MetaNumberParam<double>(0);
            using var input = CreateTestImage(SKColors.Green);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_DefaultSimilarity_ProducesOutput()
        {
            var effect = new ChromaKeyEffect();
            using var input = CreateTestImage(SKColors.Green);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.EqualTo(input.Width));
            Assert.That(result.Image.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_LimeImage_FullyKeyedOut()
        {
            var effect = new ChromaKeyEffect();
            effect.Similarity = new MetaNumberParam<double>(100);
            effect.Smoothness = new MetaNumberParam<double>(0);
            effect.KeyColor = new ColorRgb8(0, 255, 0);
            using var input = CreateTestImage(SKColors.Lime);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Alpha, Is.LessThan(5));
        }

        [Test]
        public void Apply_Green128_StaysPartiallyOpaque()
        {
            var effect = new ChromaKeyEffect();
            effect.Similarity = new MetaNumberParam<double>(100);
            effect.KeyColor = new ColorRgb8(0, 128, 0);
            using var input = CreateTestImage(SKColors.Green);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Alpha, Is.LessThan(5));
        }

        [Test]
        public void Apply_RedPixel_StaysOpaque()
        {
            var effect = new ChromaKeyEffect();
            effect.Similarity = new MetaNumberParam<double>(100);
            effect.KeyColor = new ColorRgb8(0, 255, 0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Alpha, Is.EqualTo(255));
            Assert.That(pixel.Red, Is.GreaterThan(200));
        }

        [Test]
        public void Apply_WithCustomKeyColor_KeysOutCustomColor()
        {
            var effect = new ChromaKeyEffect();
            effect.Similarity = new MetaNumberParam<double>(100);
            effect.Smoothness = new MetaNumberParam<double>(0);
            effect.KeyColor = new ColorRgb8(255, 0, 0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Alpha, Is.LessThan(5));
        }

        [Test]
        public void Apply_PartialSimilarity_PreservesNonKeyColor()
        {
            var effect = new ChromaKeyEffect();
            effect.Similarity = new MetaNumberParam<double>(50);
            effect.Smoothness = new MetaNumberParam<double>(0);
            effect.KeyColor = new ColorRgb8(0, 255, 0);
            using var input = CreateTestImage(SKColors.Red);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Alpha, Is.EqualTo(255));
            Assert.That(pixel.Red, Is.GreaterThan(200));
        }

        [Test]
        public void Apply_CyanWithRedKey_StaysOpaque()
        {
            var effect = new ChromaKeyEffect();
            effect.Similarity = new MetaNumberParam<double>(100);
            effect.KeyColor = new ColorRgb8(255, 0, 0);
            using var input = CreateTestImage(SKColors.Cyan);
            var context = CreateContext();

            var result = effect.Apply(input, context);

            using var resultBitmap = SKBitmap.FromImage(result.Image);
            var pixel = resultBitmap.GetPixel(50, 50);
            Assert.That(pixel.Alpha, Is.EqualTo(255));
        }

        [Test]
        public void DefaultValues()
        {
            var effect = new ChromaKeyEffect();
            Assert.That(effect.Similarity.StartPoint.Value, Is.EqualTo(50));
            Assert.That(effect.Smoothness.StartPoint.Value, Is.EqualTo(10));
            Assert.That(effect.KeyColor.R, Is.EqualTo(0));
            Assert.That(effect.KeyColor.G, Is.EqualTo(255));
            Assert.That(effect.KeyColor.B, Is.EqualTo(0));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(ChromaKeyEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("ChromaKeyEffect"));
        }
    }
}

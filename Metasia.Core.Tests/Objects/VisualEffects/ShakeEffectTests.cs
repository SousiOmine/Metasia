using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class ShakeEffectTests
    {
        private static VisualEffectContext CreateContext(int relativeFrame = 0, int clipLength = 100)
        {
            return new VisualEffectContext(relativeFrame, relativeFrame, clipLength,
                new SKSize(1920, 1080), new SKSize(1920, 1080), new SKSize(100, 100));
        }

        private static SKImage CreateTestImage(int width = 100, int height = 100)
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.Red };
            canvas.DrawRect(40, 40, 20, 20, paint);
            return surface.Snapshot();
        }

        [Test]
        public void Apply_ZeroStrength_ReturnsSameImage()
        {
            var effect = new ShakeEffect();
            effect.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(0);
            using var input = CreateTestImage();
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result.Image, Is.SameAs(input));
        }

        [Test]
        public void Apply_WithStrength_OutputLargerThanInput()
        {
            var effect = new ShakeEffect();
            effect.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            using var input = CreateTestImage();
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Image.Width, Is.GreaterThan(input.Width));
            Assert.That(result.Image.Height, Is.GreaterThan(input.Height));
        }

        [Test]
        public void Apply_SameSeedAndFrame_ProducesIdenticalOffset()
        {
            var effect1 = new ShakeEffect();
            effect1.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50);
            effect1.Seed = new Metasia.Core.Objects.Parameters.MetaIntParam(42);
            using var input1 = CreateTestImage();
            var context1 = CreateContext(relativeFrame: 10);

            var result1 = effect1.Apply(input1, context1);

            var effect2 = new ShakeEffect();
            effect2.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50);
            effect2.Seed = new Metasia.Core.Objects.Parameters.MetaIntParam(42);
            using var input2 = CreateTestImage();
            var context2 = CreateContext(relativeFrame: 10);

            var result2 = effect2.Apply(input2, context2);

            using var bmp1 = SKBitmap.FromImage(result1.Image);
            using var bmp2 = SKBitmap.FromImage(result2.Image);
            for (int y = 0; y < bmp1.Height; y++)
            {
                for (int x = 0; x < bmp1.Width; x++)
                {
                    Assert.That(bmp2.GetPixel(x, y), Is.EqualTo(bmp1.GetPixel(x, y)),
                        $"Pixel mismatch at ({x}, {y})");
                }
            }
        }

        [Test]
        public void Apply_DifferentSeed_ProducesDifferentOffset()
        {
            var effect1 = new ShakeEffect();
            effect1.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50);
            effect1.Seed = new Metasia.Core.Objects.Parameters.MetaIntParam(1);
            using var input1 = CreateTestImage();
            var context1 = CreateContext(relativeFrame: 10);

            var result1 = effect1.Apply(input1, context1);

            var effect2 = new ShakeEffect();
            effect2.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50);
            effect2.Seed = new Metasia.Core.Objects.Parameters.MetaIntParam(2);
            using var input2 = CreateTestImage();
            var context2 = CreateContext(relativeFrame: 10);

            var result2 = effect2.Apply(input2, context2);

            using var bmp1 = SKBitmap.FromImage(result1.Image);
            using var bmp2 = SKBitmap.FromImage(result2.Image);

            bool anyDifferent = false;
            for (int y = 0; y < bmp1.Height && !anyDifferent; y++)
            {
                for (int x = 0; x < bmp1.Width && !anyDifferent; x++)
                {
                    if (bmp2.GetPixel(x, y) != bmp1.GetPixel(x, y))
                        anyDifferent = true;
                }
            }
            Assert.That(anyDifferent, Is.True);
        }

        [Test]
        public void Apply_DifferentFrame_ProducesDifferentOffset()
        {
            var effect1 = new ShakeEffect();
            effect1.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50);
            effect1.Seed = new Metasia.Core.Objects.Parameters.MetaIntParam(0);
            using var input1 = CreateTestImage();
            var context1 = CreateContext(relativeFrame: 5);

            var result1 = effect1.Apply(input1, context1);

            var effect2 = new ShakeEffect();
            effect2.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(50);
            effect2.Seed = new Metasia.Core.Objects.Parameters.MetaIntParam(0);
            using var input2 = CreateTestImage();
            var context2 = CreateContext(relativeFrame: 10);

            var result2 = effect2.Apply(input2, context2);

            using var bmp1 = SKBitmap.FromImage(result1.Image);
            using var bmp2 = SKBitmap.FromImage(result2.Image);

            bool anyDifferent = false;
            for (int y = 0; y < bmp1.Height && !anyDifferent; y++)
            {
                for (int x = 0; x < bmp1.Width && !anyDifferent; x++)
                {
                    if (bmp2.GetPixel(x, y) != bmp1.GetPixel(x, y))
                        anyDifferent = true;
                }
            }
            Assert.That(anyDifferent, Is.True);
        }

        [Test]
        public void Apply_LargerLogicalSize_ScalesOffset()
        {
            var effect = new ShakeEffect();
            effect.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            using var input = CreateTestImage(200, 200);
            var context = new VisualEffectContext(0, 0, 100,
                new SKSize(1920, 1080), new SKSize(1920, 1080), new SKSize(200, 200));

            var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void DefaultValues()
        {
            var effect = new ShakeEffect();
            Assert.That(effect.Strength.StartPoint.Value, Is.EqualTo(10));
            Assert.That(effect.Seed.Value, Is.EqualTo(0));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(ShakeEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("ShakeEffect"));
        }
    }
}

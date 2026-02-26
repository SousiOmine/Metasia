using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    [TestFixture]
    public class MotionBlurEffectTests
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
            // 中央に赤い四角を描画（ブラーの効果を確認しやすい）
            using var paint = new SKPaint { Color = SKColors.Red };
            canvas.DrawRect(40, 40, 20, 20, paint);
            return surface.Snapshot();
        }

        [Test]
        public void Apply_ZeroStrength_ReturnsSameImage()
        {
            var effect = new MotionBlurEffect();
            effect.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(0);
            using var input = CreateTestImage();
            var context = CreateContext();

            var result = effect.Apply(input, context);

            Assert.That(result, Is.SameAs(input));
        }

        [Test]
        public void Apply_WithStrength_OutputSameSize()
        {
            var effect = new MotionBlurEffect();
            effect.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(10);
            using var input = CreateTestImage();
            var context = CreateContext();

            using var result = effect.Apply(input, context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Width, Is.EqualTo(input.Width));
            Assert.That(result.Height, Is.EqualTo(input.Height));
        }

        [Test]
        public void Apply_WithBlur_BlursContent()
        {
            var effect = new MotionBlurEffect();
            effect.Strength = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(15);
            effect.Angle = new Metasia.Core.Objects.Parameters.MetaNumberParam<double>(0);
            using var input = CreateTestImage();
            var context = CreateContext();

            using var result = effect.Apply(input, context);
            using var inputBitmap = SKBitmap.FromImage(input);
            using var resultBitmap = SKBitmap.FromImage(result);

            // ブラーが適用されると、元は透明だった領域にも色がにじむ
            // 赤い四角の右隣（元は透明）に色が広がっているはず
            var nearEdgePixel = resultBitmap.GetPixel(65, 50);
            Assert.That(nearEdgePixel.Alpha, Is.GreaterThan(0));
        }

        [Test]
        public void DefaultValues()
        {
            var effect = new MotionBlurEffect();
            Assert.That(effect.Angle.StartPoint.Value, Is.EqualTo(0));
            Assert.That(effect.Strength.StartPoint.Value, Is.EqualTo(10));
        }

        [Test]
        public void HasVisualEffectIdentifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(typeof(MotionBlurEffect),
                typeof(Metasia.Core.Attributes.VisualEffectIdentifierAttribute))
                as Metasia.Core.Attributes.VisualEffectIdentifierAttribute;

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Identifier, Is.EqualTo("MotionBlurEffect"));
        }
    }
}

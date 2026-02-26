using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    /// <summary>
    /// VisualEffectBaseクラスのテスト
    /// </summary>
    [TestFixture]
    public class VisualEffectBaseTests
    {
        [Test]
        public void DefaultId_IsEmptyString()
        {
            var effect = new TestPassThroughEffect();
            Assert.That(effect.Id, Is.EqualTo(string.Empty));
        }

        [Test]
        public void DefaultIsActive_IsTrue()
        {
            var effect = new TestPassThroughEffect();
            Assert.That(effect.IsActive, Is.True);
        }

        [Test]
        public void Id_CanBeSetAndRetrieved()
        {
            var effect = new TestPassThroughEffect { Id = "test-effect-1" };
            Assert.That(effect.Id, Is.EqualTo("test-effect-1"));
        }

        [Test]
        public void IsActive_CanBeSetToFalse()
        {
            var effect = new TestPassThroughEffect { IsActive = false };
            Assert.That(effect.IsActive, Is.False);
        }

        [Test]
        public void Apply_ReturnsImage()
        {
            // Arrange
            var effect = new TestPassThroughEffect();
            using var inputImage = CreateTestImage(SKColors.Blue);
            var context = CreateTestContext();

            // Act
            using var result = effect.Apply(inputImage, context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(inputImage));
        }

        [Test]
        public void Apply_ReceivesCorrectContext()
        {
            // Arrange
            var effect = new TestPassThroughEffect();
            using var inputImage = CreateTestImage(SKColors.Blue);
            var context = new VisualEffectContext(10, 5, 100, new SKSize(1920, 1080), new SKSize(960, 540), new SKSize(200, 200));

            // Act
            effect.Apply(inputImage, context);

            // Assert
            Assert.That(effect.LastContext, Is.Not.Null);
            Assert.That(effect.LastContext!.Frame, Is.EqualTo(10));
            Assert.That(effect.LastContext.RelativeFrame, Is.EqualTo(5));
            Assert.That(effect.LastContext.ClipLength, Is.EqualTo(100));
        }

        [Test]
        public void Apply_TransformsImage()
        {
            // Arrange
            var effect = new TestRedFillEffect();
            using var inputImage = CreateTestImage(SKColors.Blue, 50, 50);
            var context = CreateTestContext();

            // Act
            using var result = effect.Apply(inputImage, context);

            // Assert
            Assert.That(result, Is.Not.Null);
            using var bitmap = SKBitmap.FromImage(result);
            Assert.That(bitmap.GetPixel(25, 25), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void ImplementsIVisualEffect()
        {
            var effect = new TestPassThroughEffect();
            Assert.That(effect, Is.InstanceOf<IVisualEffect>());
        }

        [Test]
        public void ImplementsIMetasiaObject()
        {
            var effect = new TestPassThroughEffect();
            Assert.That(effect, Is.InstanceOf<Metasia.Core.Objects.IMetasiaObject>());
        }

        #region ヘルパーメソッド

        private static SKImage CreateTestImage(SKColor color, int width = 100, int height = 100)
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(color);
            return surface.Snapshot();
        }

        private static VisualEffectContext CreateTestContext()
        {
            return new VisualEffectContext(0, 0, 100, new SKSize(1920, 1080), new SKSize(1920, 1080), new SKSize(100, 100));
        }

        #endregion
    }
}

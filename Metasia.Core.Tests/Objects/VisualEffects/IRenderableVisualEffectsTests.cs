using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;

namespace Metasia.Core.Tests.Objects.VisualEffects
{
    /// <summary>
    /// IRenderable実装クラスがVisualEffectsプロパティを正しく持つことを確認するテスト
    /// </summary>
    [TestFixture]
    public class IRenderableVisualEffectsTests
    {
        [Test]
        public void Text_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new Text();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void ShapeObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new ShapeObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void ImageObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new ImageObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void VideoObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new VideoObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void LayerObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new LayerObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void TimelineObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new TimelineObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void GroupControlObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new GroupControlObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void CameraControlObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new CameraControlObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void kariHelloObject_HasVisualEffectsProperty_InitializedEmpty()
        {
            var obj = new kariHelloObject();
            Assert.That(obj.VisualEffects, Is.Not.Null);
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void VisualEffects_CanAddEffect()
        {
            // Arrange
            var obj = new Text();
            var effect = new TestPassThroughEffect { Id = "test-1" };

            // Act
            obj.VisualEffects.Add(effect);

            // Assert
            Assert.That(obj.VisualEffects, Has.Count.EqualTo(1));
            Assert.That(obj.VisualEffects[0].Id, Is.EqualTo("test-1"));
        }

        [Test]
        public void VisualEffects_CanRemoveEffect()
        {
            // Arrange
            var obj = new Text();
            var effect = new TestPassThroughEffect { Id = "test-1" };
            obj.VisualEffects.Add(effect);

            // Act
            obj.VisualEffects.Remove(effect);

            // Assert
            Assert.That(obj.VisualEffects, Is.Empty);
        }

        [Test]
        public void VisualEffects_CanAddMultipleEffects()
        {
            // Arrange
            var obj = new ShapeObject();
            var effect1 = new TestPassThroughEffect { Id = "effect-1" };
            var effect2 = new TestPassThroughEffect { Id = "effect-2" };
            var effect3 = new TestPassThroughEffect { Id = "effect-3" };

            // Act
            obj.VisualEffects.Add(effect1);
            obj.VisualEffects.Add(effect2);
            obj.VisualEffects.Add(effect3);

            // Assert
            Assert.That(obj.VisualEffects, Has.Count.EqualTo(3));
            Assert.That(obj.VisualEffects[0].Id, Is.EqualTo("effect-1"));
            Assert.That(obj.VisualEffects[1].Id, Is.EqualTo("effect-2"));
            Assert.That(obj.VisualEffects[2].Id, Is.EqualTo("effect-3"));
        }
    }
}

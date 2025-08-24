using NUnit.Framework;
using Metasia.Core.Objects;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class TimelineObjectTests
    {
        private TimelineObject _timelineObject;

        [SetUp]
        public void Setup()
        {
            _timelineObject = new TimelineObject("timeline-id");
        }

        [Test]
        public void Constructor_WithId_InitializesCorrectly()
        {
            // Arrange & Act
            var timeline = new TimelineObject("test-id");

            // Assert
            Assert.That(timeline.Id, Is.EqualTo("test-id"));
            Assert.That(timeline.Volume, Is.EqualTo(100));
            Assert.That(timeline.Layers, Is.Not.Null);
            Assert.That(timeline.Layers, Is.InstanceOf<List<LayerObject>>());
            Assert.That(timeline.Layers.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithoutParameters_InitializesWithDefaults()
        {
            // Arrange & Act
            var timeline = new TimelineObject();

            // Assert
            Assert.That(timeline.Volume, Is.EqualTo(100));
            Assert.That(timeline.Layers, Is.Not.Null);
            Assert.That(timeline.Layers.Count, Is.EqualTo(0));
        }

        [Test]
        public void Volume_CanBeModified()
        {
            // Arrange
            Assert.That(_timelineObject.Volume, Is.EqualTo(100)); // デフォルト値確認

            // Act
            _timelineObject.Volume = 75;

            // Assert
            Assert.That(_timelineObject.Volume, Is.EqualTo(75));
        }

        [Test]
        public void Layers_CanAddAndRemove()
        {
            // Arrange
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");

            // Act - Add
            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer2);

            // Assert - Add
            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(2));
            Assert.That(_timelineObject.Layers[0].Id, Is.EqualTo("layer1"));
            Assert.That(_timelineObject.Layers[1].Id, Is.EqualTo("layer2"));

            // Act - Remove
            _timelineObject.Layers.Remove(layer1);

            // Assert - Remove
            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(1));
            Assert.That(_timelineObject.Layers[0].Id, Is.EqualTo("layer2"));
        }

        [Test]
        public void Layers_MaintainOrder()
        {
            // Arrange
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");
            var layer3 = new LayerObject("layer3", "Layer 3");

            // Act
            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer2);
            _timelineObject.Layers.Add(layer3);

            // Assert - 順序が保持されることを確認
            Assert.That(_timelineObject.Layers[0].Name, Is.EqualTo("Layer 1"));
            Assert.That(_timelineObject.Layers[1].Name, Is.EqualTo("Layer 2"));
            Assert.That(_timelineObject.Layers[2].Name, Is.EqualTo("Layer 3"));
        }

        [Test]
        public void Layers_CanInsertAtSpecificIndex()
        {
            // Arrange
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");
            var layer3 = new LayerObject("layer3", "Layer 3");

            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer3);

            // Act
            _timelineObject.Layers.Insert(1, layer2);

            // Assert
            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(3));
            Assert.That(_timelineObject.Layers[0].Name, Is.EqualTo("Layer 1"));
            Assert.That(_timelineObject.Layers[1].Name, Is.EqualTo("Layer 2"));
            Assert.That(_timelineObject.Layers[2].Name, Is.EqualTo("Layer 3"));
        }

        [Test]
        public void Layers_CanClear()
        {
            // Arrange
            _timelineObject.Layers.Add(new LayerObject("layer1", "Layer 1"));
            _timelineObject.Layers.Add(new LayerObject("layer2", "Layer 2"));
            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(2));

            // Act
            _timelineObject.Layers.Clear();

            // Assert
            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(0));
        }

        [Test]
        public void InheritedProperties_WorkCorrectly()
        {
            // TimelineObjectはClipObjectを継承しているので、基本プロパティも確認
            Assert.That(_timelineObject.StartFrame, Is.EqualTo(0));
            Assert.That(_timelineObject.EndFrame, Is.EqualTo(100));
            Assert.That(_timelineObject.IsActive, Is.True);

            // 変更も可能
            _timelineObject.StartFrame = 10;
            _timelineObject.EndFrame = 200;
            _timelineObject.IsActive = false;

            Assert.That(_timelineObject.StartFrame, Is.EqualTo(10));
            Assert.That(_timelineObject.EndFrame, Is.EqualTo(200));
            Assert.That(_timelineObject.IsActive, Is.False);
        }
    }
} 
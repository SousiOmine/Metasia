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
            Assert.That(timeline.Volume.Value, Is.EqualTo(100.0));
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
            Assert.That(timeline.Volume.Value, Is.EqualTo(100.0));
            Assert.That(timeline.Layers, Is.Not.Null);
            Assert.That(timeline.Layers.Count, Is.EqualTo(0));
        }

        [Test]
        public void Volume_CanBeModified()
        {
            // Arrange
            Assert.That(_timelineObject.Volume.Value, Is.EqualTo(100.0)); // デフォルト値確認

            // Act
            _timelineObject.Volume = 75;

            // Assert
            Assert.That(_timelineObject.Volume.Value, Is.EqualTo(75.0));
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
        public void IMetasiaObject_Properties_WorkCorrectly()
        {
            // TimelineObjectはIMetasiaObjectを実装しているので、基本プロパティを確認
            Assert.That(_timelineObject.Id, Is.EqualTo("timeline-id"));
            Assert.That(_timelineObject.IsActive, Is.True);

            // 変更も可能
            _timelineObject.Id = "modified-id";
            _timelineObject.IsActive = false;

            Assert.That(_timelineObject.Id, Is.EqualTo("modified-id"));
            Assert.That(_timelineObject.IsActive, Is.False);
        }

        [Test]
        public void SelectionRange_DefaultValues()
        {
            // Arrange & Act
            var timeline = new TimelineObject();

            // Assert
            Assert.That(timeline.SelectionStart, Is.EqualTo(0));
            Assert.That(timeline.SelectionEnd, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void SelectionRange_CanBeModified()
        {
            // Arrange
            var timeline = new TimelineObject();

            // Act
            timeline.SelectionStart = 50;
            timeline.SelectionEnd = 200;

            // Assert
            Assert.That(timeline.SelectionStart, Is.EqualTo(50));
            Assert.That(timeline.SelectionEnd, Is.EqualTo(200));
        }

        [Test]
        public void GetLastFrameOfClips_EmptyTimeline_ReturnsZero()
        {
            // Arrange
            var timeline = new TimelineObject();

            // Act
            int lastFrame = timeline.GetLastFrameOfClips();

            // Assert
            Assert.That(lastFrame, Is.EqualTo(0));
        }

        [Test]
        public void GetLastFrameOfClips_WithClips_ReturnsMaximumEndFrame()
        {
            // Arrange
            var timeline = new TimelineObject();
            var layer = new LayerObject("layer1", "Layer 1");
            timeline.Layers.Add(layer);

            var clip1 = new ClipObject("clip1") { StartFrame = 0, EndFrame = 50 };
            var clip2 = new ClipObject("clip2") { StartFrame = 60, EndFrame = 150 };
            var clip3 = new ClipObject("clip3") { StartFrame = 200, EndFrame = 300 };

            layer.Objects.Add(clip1);
            layer.Objects.Add(clip2);
            layer.Objects.Add(clip3);

            // Act
            int lastFrame = timeline.GetLastFrameOfClips();

            // Assert
            Assert.That(lastFrame, Is.EqualTo(300));
        }

        [Test]
        public void GetLastFrameOfClips_MultipleLayers_ReturnsMaximumEndFrame()
        {
            // Arrange
            var timeline = new TimelineObject();
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");

            timeline.Layers.Add(layer1);
            timeline.Layers.Add(layer2);

            layer1.Objects.Add(new ClipObject("clip1") { StartFrame = 0, EndFrame = 100 });
            layer2.Objects.Add(new ClipObject("clip2") { StartFrame = 50, EndFrame = 200 });

            // Act
            int lastFrame = timeline.GetLastFrameOfClips();

            // Assert
            Assert.That(lastFrame, Is.EqualTo(200));
        }
    }
}

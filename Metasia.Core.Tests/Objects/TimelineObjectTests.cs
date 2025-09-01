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

        /// <summary>
        /// タイムラインオブジェクトを正常に分割できることを確認するテスト
        /// 意図: タイムラインの分割機能が正しく動作し、基本プロパティとレイヤーが維持されることを検証
        /// 想定結果: 2つのTimelineObjectが返され、ID、フレーム範囲、音量、レイヤー数が正しく設定される
        /// </summary>
        [Test]
        public void SplitAtFrame_ValidSplitFrame_ReturnsTwoTimelineObjectsWithCorrectProperties()
        {
            // Arrange
            _timelineObject.StartFrame = 10;
            _timelineObject.EndFrame = 100;
            _timelineObject.Volume = 80;
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");
            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer2);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _timelineObject.SplitAtFrame(splitFrame);
            var firstTimeline = firstClip as TimelineObject;
            var secondTimeline = secondClip as TimelineObject;

            // Assert
            Assert.That(firstTimeline, Is.Not.Null);
            Assert.That(secondTimeline, Is.Not.Null);
            Assert.That(firstTimeline.Id, Is.EqualTo("timeline-id_part1"));
            Assert.That(secondTimeline.Id, Is.EqualTo("timeline-id_part2"));
            Assert.That(firstTimeline.StartFrame, Is.EqualTo(10));
            Assert.That(firstTimeline.EndFrame, Is.EqualTo(49));
            Assert.That(secondTimeline.StartFrame, Is.EqualTo(50));
            Assert.That(secondTimeline.EndFrame, Is.EqualTo(100));
            Assert.That(firstTimeline.Volume, Is.EqualTo(80));
            Assert.That(secondTimeline.Volume, Is.EqualTo(80));
            Assert.That(firstTimeline.Layers.Count, Is.EqualTo(2));
            Assert.That(secondTimeline.Layers.Count, Is.EqualTo(2));
        }

        /// <summary>
        /// 分割時にレイヤーのフレーム範囲が正しく調整されることを確認するテスト
        /// 意図: タイムライン分割後、各レイヤーのフレーム範囲が対応するタイムライン範囲に調整されることを検証
        /// 想定結果: 前半タイムラインのレイヤーは開始フレーム10、後半タイムラインのレイヤーは開始フレーム50になる
        /// </summary>
        [Test]
        public void SplitAtFrame_LayersHaveAdjustedFrameRanges()
        {
            // Arrange
            _timelineObject.StartFrame = 10;
            _timelineObject.EndFrame = 100;
            var layer = new LayerObject("layer1", "Layer 1");
            _timelineObject.Layers.Add(layer);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _timelineObject.SplitAtFrame(splitFrame);
            var firstTimeline = firstClip as TimelineObject;
            var secondTimeline = secondClip as TimelineObject;

            // Assert
            Assert.That(firstTimeline.Layers[0].StartFrame, Is.EqualTo(10));
            Assert.That(firstTimeline.Layers[0].EndFrame, Is.EqualTo(49));
            Assert.That(secondTimeline.Layers[0].StartFrame, Is.EqualTo(50));
            Assert.That(secondTimeline.Layers[0].EndFrame, Is.EqualTo(100));
        }

        /// <summary>
        /// 分割時に音響効果が正しく維持されることを確認するテスト
        /// 意図: タイムライン分割後、音響効果が両方のタイムラインにコピーされることを検証
        /// 想定結果: 分割された両方のタイムラインで音響効果リストに1つの効果が保持される
        /// </summary>
        [Test]
        public void SplitAtFrame_PreservesAudioEffects()
        {
            // Arrange
            _timelineObject.StartFrame = 10;
            _timelineObject.EndFrame = 100;
            var effect = new Metasia.Core.Objects.AudioEffects.VolumeFadeEffect();
            _timelineObject.AudioEffects.Add(effect);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _timelineObject.SplitAtFrame(splitFrame);
            var firstTimeline = firstClip as TimelineObject;
            var secondTimeline = secondClip as TimelineObject;

            // Assert
            Assert.That(firstTimeline.AudioEffects.Count, Is.EqualTo(1));
            Assert.That(secondTimeline.AudioEffects.Count, Is.EqualTo(1));
            Assert.That(firstTimeline.AudioEffects[0], Is.InstanceOf<Metasia.Core.Objects.AudioEffects.VolumeFadeEffect>());
            Assert.That(secondTimeline.AudioEffects[0], Is.InstanceOf<Metasia.Core.Objects.AudioEffects.VolumeFadeEffect>());
        }

        /// <summary>
        /// 分割されたタイムラインオブジェクトが元オブジェクトから独立していることを確認するテスト
        /// 意図: ディープコピーが正しく行われ、元タイムラインの変更が分割オブジェクトに影響しないことを検証
        /// 想定結果: 元タイムラインを変更しても、分割されたタイムラインの音量とレイヤーは変更されない
        /// </summary>
        [Test]
        public void SplitAtFrame_TimelineObjectsAreIndependent_ModifyingOriginalDoesNotAffectSplits()
        {
            // Arrange
            _timelineObject.StartFrame = 10;
            _timelineObject.EndFrame = 100;
            _timelineObject.Volume = 80;
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");
            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer2);
            var (firstClip, secondClip) = _timelineObject.SplitAtFrame(50);
            var firstTimeline = firstClip as TimelineObject;
            var secondTimeline = secondClip as TimelineObject;

            // Act
            _timelineObject.Volume = 50;
            _timelineObject.Layers.Clear();

            // Assert
            Assert.That(firstTimeline.Volume, Is.EqualTo(80));
            Assert.That(secondTimeline.Volume, Is.EqualTo(80));
            Assert.That(firstTimeline.Layers.Count, Is.GreaterThan(0));
            Assert.That(secondTimeline.Layers.Count, Is.GreaterThan(0));
        }
    }
} 
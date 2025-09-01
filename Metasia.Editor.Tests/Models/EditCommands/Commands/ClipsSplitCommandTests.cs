using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands
{
    [TestFixture]
    public class ClipsSplitCommandTests
    {
        private List<ClipObject> _targetClips;
        private List<LayerObject> _ownerLayers;
        private ClipsSplitCommand? _command;

        [SetUp]
        public void Setup()
        {
            _targetClips = new List<ClipObject>();
            _ownerLayers = new List<LayerObject>();

            // テスト用のレイヤーを作成
            var layer1 = new LayerObject();
            var layer2 = new LayerObject();
            
            // テスト用のクリップを作成
            var clip1 = new ClipObject("test-clip-1")
            {
                StartFrame = 10,
                EndFrame = 50
            };
            
            var clip2 = new ClipObject("test-clip-2")
            {
                StartFrame = 60,
                EndFrame = 100
            };

            layer1.Objects.Add(clip1);
            layer2.Objects.Add(clip2);
            
            _targetClips.Add(clip1);
            _targetClips.Add(clip2);
            _ownerLayers.Add(layer1);
            _ownerLayers.Add(layer2);
        }

        [Test]
        public void Execute_SplitsClipsAtSpecifiedFrame()
        {
            // Arrange - 分割可能なクリップのみを渡す
            var splittableClips = new List<ClipObject> { _targetClips[0] }; // clip1のみ（10-50）
            var splittableLayers = new List<LayerObject> { _ownerLayers[0] };
            int splitFrame = 30;
            _command = new ClipsSplitCommand(splittableClips, splittableLayers, splitFrame);

            // Act
            _command.Execute();

            // Assert
            // clip1は分割されているはず
            var layer1Clips = _ownerLayers[0].Objects.OrderBy(c => c.StartFrame).ToList();
            Assert.That(layer1Clips.Count, Is.EqualTo(2));
            
            // 前半クリップ (10-29)
            Assert.That(layer1Clips[0].StartFrame, Is.EqualTo(10));
            Assert.That(layer1Clips[0].EndFrame, Is.EqualTo(29));
            
            // 後半クリップ (30-50)
            Assert.That(layer1Clips[1].StartFrame, Is.EqualTo(30));
            Assert.That(layer1Clips[1].EndFrame, Is.EqualTo(50));

            // clip2は変更されていないはず
            var layer2Clips = _ownerLayers[1].Objects.ToList();
            Assert.That(layer2Clips.Count, Is.EqualTo(1));
            Assert.That(layer2Clips[0].StartFrame, Is.EqualTo(60));
            Assert.That(layer2Clips[0].EndFrame, Is.EqualTo(100));
        }

        [Test]
        public void Undo_RestoresOriginalClips()
        {
            // Arrange - 分割可能なクリップのみを渡す
            var splittableClips = new List<ClipObject> { _targetClips[0] }; // clip1のみ（10-50）
            var splittableLayers = new List<LayerObject> { _ownerLayers[0] };
            int splitFrame = 30;
            var originalClip1Start = _targetClips[0].StartFrame;
            var originalClip1End = _targetClips[0].EndFrame;
            var originalClip2Start = _targetClips[1].StartFrame;
            var originalClip2End = _targetClips[1].EndFrame;
            
            _command = new ClipsSplitCommand(splittableClips, splittableLayers, splitFrame);
            _command.Execute();

            // Act
            _command.Undo();

            // Assert
            // 元の状態に戻っているはず
            var layer1Clips = _ownerLayers[0].Objects.ToList();
            var layer2Clips = _ownerLayers[1].Objects.ToList();
            
            Assert.That(layer1Clips.Count, Is.EqualTo(1));
            Assert.That(layer1Clips[0].StartFrame, Is.EqualTo(originalClip1Start));
            Assert.That(layer1Clips[0].EndFrame, Is.EqualTo(originalClip1End));
            
            Assert.That(layer2Clips.Count, Is.EqualTo(1));
            Assert.That(layer2Clips[0].StartFrame, Is.EqualTo(originalClip2Start));
            Assert.That(layer2Clips[0].EndFrame, Is.EqualTo(originalClip2End));
        }

        [Test]
        public void Execute_WithBoundaryFrames()
        {
            // Arrange - 境界値テスト
            var layer = new LayerObject();
            var clip = new ClipObject("boundary-test") { StartFrame = 0, EndFrame = 100 };
            layer.Objects.Add(clip);
            
            var clips = new List<ClipObject> { clip };
            var layers = new List<LayerObject> { layer };
            
            int splitFrame = 1; // 最小限の分割
            _command = new ClipsSplitCommand(clips, layers, splitFrame);

            // Act
            _command.Execute();

            // Assert
            var layerClips = layer.Objects.OrderBy(c => c.StartFrame).ToList();
            Assert.That(layerClips.Count, Is.EqualTo(2));
            
            // 前半クリップ (0-0)
            Assert.That(layerClips[0].StartFrame, Is.EqualTo(0));
            Assert.That(layerClips[0].EndFrame, Is.EqualTo(0));
            
            // 後半クリップ (1-100)
            Assert.That(layerClips[1].StartFrame, Is.EqualTo(1));
            Assert.That(layerClips[1].EndFrame, Is.EqualTo(100));
        }

        [Test]
        public void Constructor_AcceptsIEnumerable()
        {
            // Arrange & Act - 分割可能なクリップのみを渡す
            var splittableClips = new List<ClipObject> { _targetClips[0] }; // clip1のみ（10-50）
            var splittableLayers = new List<LayerObject> { _ownerLayers[0] };
            int splitFrame = 30;
            _command = new ClipsSplitCommand(
                (IEnumerable<ClipObject>)splittableClips, 
                (IEnumerable<LayerObject>)splittableLayers, 
                splitFrame
            );

            // Execute to verify it works with IEnumerable
            _command.Execute();

            // Assert
            var layer1Clips = _ownerLayers[0].Objects.OrderBy(c => c.StartFrame).ToList();
            Assert.That(layer1Clips.Count, Is.EqualTo(2));
        }

        [Test]
        public void Execute_SplitsMultipleClipsCorrectly()
        {
            // Arrange - 両方のクリップが分割されるように設定
            var clip1 = new ClipObject("clip1") { StartFrame = 10, EndFrame = 50 };
            var clip2 = new ClipObject("clip2") { StartFrame = 20, EndFrame = 60 };
            
            var layer1 = new LayerObject();
            var layer2 = new LayerObject();
            
            layer1.Objects.Add(clip1);
            layer2.Objects.Add(clip2);
            
            var clips = new List<ClipObject> { clip1, clip2 };
            var layers = new List<LayerObject> { layer1, layer2 };
            
            int splitFrame = 30;
            _command = new ClipsSplitCommand(clips, layers, splitFrame);

            // Act
            _command.Execute();

            // Assert
            // clip1が分割されている
            var layer1Clips = layer1.Objects.OrderBy(c => c.StartFrame).ToList();
            Assert.That(layer1Clips.Count, Is.EqualTo(2));
            Assert.That(layer1Clips[0].EndFrame, Is.EqualTo(29));
            Assert.That(layer1Clips[1].StartFrame, Is.EqualTo(30));
            
            // clip2も分割されている
            var layer2Clips = layer2.Objects.OrderBy(c => c.StartFrame).ToList();
            Assert.That(layer2Clips.Count, Is.EqualTo(2));
            Assert.That(layer2Clips[0].EndFrame, Is.EqualTo(29));
            Assert.That(layer2Clips[1].StartFrame, Is.EqualTo(30));
        }

        [Test]
        public void Execute_SplitsAtCorrectPosition()
        {
            // Arrange - 分割位置を正確にテスト
            var layer = new LayerObject();
            var clip = new ClipObject("test-clip") { StartFrame = 20, EndFrame = 80 };
            
            layer.Objects.Add(clip);
            
            var clips = new List<ClipObject> { clip };
            var layers = new List<LayerObject> { layer };
            
            int splitFrame = 50;
            _command = new ClipsSplitCommand(clips, layers, splitFrame);

            // Act
            _command.Execute();

            // Assert
            var layerClips = layer.Objects.OrderBy(c => c.StartFrame).ToList();
            Assert.That(layerClips.Count, Is.EqualTo(2));
            
            // 前半クリップ (20-49)
            Assert.That(layerClips[0].StartFrame, Is.EqualTo(20));
            Assert.That(layerClips[0].EndFrame, Is.EqualTo(49));
            
            // 後半クリップ (50-80)
            Assert.That(layerClips[1].StartFrame, Is.EqualTo(50));
            Assert.That(layerClips[1].EndFrame, Is.EqualTo(80));
        }

        [Test]
        public void Description_ReturnsCorrectValue()
        {
            // Arrange & Act
            _command = new ClipsSplitCommand(_targetClips, _ownerLayers, 30);

            // Assert
            Assert.That(_command.Description, Is.EqualTo("クリップの分割"));
        }

        [Test]
        public void Execute_WithEmptyClips_ThrowsArgumentException()
        {
            // Arrange
            var emptyClips = new List<ClipObject>();
            var emptyLayers = new List<LayerObject>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new ClipsSplitCommand(emptyClips, emptyLayers, 30));
            Assert.That(ex.Message, Is.EqualTo("分割対象のクリップがありません。 (Parameter 'targetClips')"));
        }

        [Test]
        public void Execute_WithMismatchedCounts_ThrowsArgumentException()
        {
            // Arrange
            var clips = new List<ClipObject> { _targetClips[0] }; // 1 clip
            var layers = new List<LayerObject> { _ownerLayers[0], _ownerLayers[1] }; // 2 layers

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new ClipsSplitCommand(clips, layers, 30));
            Assert.That(ex.Message, Is.EqualTo("クリップとレイヤーの数が一致しません。 (Parameter 'ownerLayers')"));
        }

        [Test]
        public void Execute_WithNonSplittableClip_ThrowsArgumentException()
        {
            // Arrange - 分割不可能なクリップを渡す
            var nonSplittableClip = new ClipObject("non-splittable")
            {
                StartFrame = 10,
                EndFrame = 50
            };
            
            var layer = new LayerObject();
            layer.Objects.Add(nonSplittableClip);
            
            var clips = new List<ClipObject> { nonSplittableClip };
            var layers = new List<LayerObject> { layer };
            
            int splitFrame = 5; // クリップの開始フレームより前
            _command = new ClipsSplitCommand(clips, layers, splitFrame);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _command.Execute());
            Assert.That(ex.Message, Is.EqualTo("クリップ 'non-splittable' はフレーム 5 で分割できません。"));
        }

        [Test]
        public void Execute_WithBoundaryFrameSplit_ThrowsArgumentException()
        {
            // Arrange - 境界値で分割しようとする
            var boundaryClip = new ClipObject("boundary-clip")
            {
                StartFrame = 10,
                EndFrame = 50
            };
            
            var layer = new LayerObject();
            layer.Objects.Add(boundaryClip);
            
            var clips = new List<ClipObject> { boundaryClip };
            var layers = new List<LayerObject> { layer };
            
            int splitFrame = 10; // クリップの開始フレームと同じ
            _command = new ClipsSplitCommand(clips, layers, splitFrame);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _command.Execute());
            Assert.That(ex.Message, Is.EqualTo("クリップ 'boundary-clip' はフレーム 10 で分割できません。"));
        }

        [Test]
        public void Execute_WithEndFrameSplit_ThrowsArgumentException()
        {
            // Arrange - 終端フレームで分割しようとする
            var endFrameClip = new ClipObject("end-frame-clip")
            {
                StartFrame = 10,
                EndFrame = 50
            };
            
            var layer = new LayerObject();
            layer.Objects.Add(endFrameClip);
            
            var clips = new List<ClipObject> { endFrameClip };
            var layers = new List<LayerObject> { layer };
            
            int splitFrame = 50; // クリップの終了フレームと同じ
            _command = new ClipsSplitCommand(clips, layers, splitFrame);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _command.Execute());
            Assert.That(ex.Message, Is.EqualTo("クリップ 'end-frame-clip' はフレーム 50 で分割できません。"));
        }
    }
}
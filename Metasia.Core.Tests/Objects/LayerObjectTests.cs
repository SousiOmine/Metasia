using NUnit.Framework;
using Metasia.Core.Objects;
using System.Collections.ObjectModel;
using System.Linq;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class LayerObjectTests
    {
        private LayerObject _layerObject;

        [SetUp]
        public void Setup()
        {
            _layerObject = new LayerObject("layer-id", "Test Layer");
        }

        [Test]
        public void Constructor_WithIdAndName_InitializesCorrectly()
        {
            // Arrange & Act
            var layer = new LayerObject("test-id", "Layer Name");

            // Assert
            Assert.That(layer.Id, Is.EqualTo("test-id"));
            Assert.That(layer.Name, Is.EqualTo("Layer Name"));
            Assert.That(layer.StartFrame, Is.EqualTo(0));
            Assert.That(layer.EndFrame, Is.EqualTo(int.MaxValue));
            Assert.That(layer.Volume, Is.EqualTo(100));
            Assert.That(layer.Objects, Is.Not.Null);
            Assert.That(layer.Objects, Is.InstanceOf<ObservableCollection<ClipObject>>());
            Assert.That(layer.Objects.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithoutParameters_InitializesWithDefaults()
        {
            // Arrange & Act
            var layer = new LayerObject();

            // Assert
            Assert.That(layer.Name, Is.EqualTo(string.Empty));
            Assert.That(layer.StartFrame, Is.EqualTo(0));
            Assert.That(layer.EndFrame, Is.EqualTo(int.MaxValue));
            Assert.That(layer.Volume, Is.EqualTo(100));
            Assert.That(layer.Objects, Is.Not.Null);
            Assert.That(layer.Objects.Count, Is.EqualTo(0));
        }

        [Test]
        public void CanPlaceObjectAt_WithNoOverlap_ReturnsTrue()
        {
            // Arrange
            var existingObject = new ClipObject("existing-id") { StartFrame = 0, EndFrame = 50 };
            _layerObject.Objects.Add(existingObject);

            var newObject = new ClipObject("new-id");

            // Act & Assert
            // 既存オブジェクトの後
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 51, 100), Is.True);
            // 既存オブジェクトの前
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, -50, -1), Is.True);
        }

        [Test]
        public void CanPlaceObjectAt_WithOverlap_ReturnsFalse()
        {
            // Arrange
            var existingObject = new ClipObject("existing-id") { StartFrame = 50, EndFrame = 100 };
            _layerObject.Objects.Add(existingObject);

            var newObject = new ClipObject("new-id");

            // Act & Assert
            // 完全に重なる
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 50, 100), Is.False);
            // 部分的に重なる（前側）
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 40, 60), Is.False);
            // 部分的に重なる（後側）
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 90, 110), Is.False);
            // 内包される
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 60, 80), Is.False);
            // 外包する
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 40, 110), Is.False);
        }

        [Test]
        public void CanPlaceObjectAt_WithSameObject_ReturnsTrue()
        {
            // Arrange
            var existingObject = new ClipObject("same-id") { StartFrame = 50, EndFrame = 100 };
            _layerObject.Objects.Add(existingObject);

            // Act & Assert
            // 同じオブジェクトの場合は重なっても配置可能
            Assert.That(_layerObject.CanPlaceObjectAt(existingObject, 50, 100), Is.True);
            Assert.That(_layerObject.CanPlaceObjectAt(existingObject, 0, 200), Is.True);
        }

        [Test]
        public void CanPlaceObjectAt_WithInvalidRange_ReturnsFalse()
        {
            // Arrange
            var newObject = new ClipObject("new-id");

            // Act & Assert
            // 開始フレームが終了フレームより大きい
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 100, 50), Is.False);
        }

        [Test]
        public void CanPlaceObjectAt_WithEdgeFrames_HandlesCorrectly()
        {
            // Arrange
            var existingObject = new ClipObject("existing-id") { StartFrame = 50, EndFrame = 100 };
            _layerObject.Objects.Add(existingObject);

            var newObject = new ClipObject("new-id");

            // Act & Assert
            // 終了フレームが既存の開始フレームと同じ（重なる）
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 0, 50), Is.False);
            // 開始フレームが既存の終了フレームと同じ（重なる）
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 100, 150), Is.False);
            // 終了フレームが既存の開始フレームの1つ前（重ならない）
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 0, 49), Is.True);
            // 開始フレームが既存の終了フレームの1つ後（重ならない）
            Assert.That(_layerObject.CanPlaceObjectAt(newObject, 101, 150), Is.True);
        }

        [Test]
        public void Volume_CanBeModified()
        {
            // Arrange
            Assert.That(_layerObject.Volume, Is.EqualTo(100)); // デフォルト値確認

            // Act
            _layerObject.Volume = 50;

            // Assert
            Assert.That(_layerObject.Volume, Is.EqualTo(50));
        }

        [Test]
        public void Name_CanBeModified()
        {
            // Arrange
            Assert.That(_layerObject.Name, Is.EqualTo("Test Layer")); // 初期値確認

            // Act
            _layerObject.Name = "Modified Layer";

            // Assert
            Assert.That(_layerObject.Name, Is.EqualTo("Modified Layer"));
        }

        [Test]
        public void Objects_CanAddAndRemove()
        {
            // Arrange
            var obj1 = new ClipObject("obj1");
            var obj2 = new ClipObject("obj2");

            // Act - Add
            _layerObject.Objects.Add(obj1);
            _layerObject.Objects.Add(obj2);

            // Assert - Add
            Assert.That(_layerObject.Objects.Count, Is.EqualTo(2));
            Assert.That(_layerObject.Objects[0].Id, Is.EqualTo("obj1"));
            Assert.That(_layerObject.Objects[1].Id, Is.EqualTo("obj2"));

            // Act - Remove
            _layerObject.Objects.Remove(obj1);

            // Assert - Remove
            Assert.That(_layerObject.Objects.Count, Is.EqualTo(1));
            Assert.That(_layerObject.Objects[0].Id, Is.EqualTo("obj2"));
        }

        /// <summary>
        /// レイヤーオブジェクトを正常に分割できることを確認するテスト
        /// 意図: レイヤーの分割機能が正しく動作し、基本プロパティが維持されることを検証
        /// 想定結果: 2つのLayerObjectが返され、ID、フレーム範囲、音量、名前が正しく設定される
        /// </summary>
        [Test]
        public void SplitAtFrame_ValidSplitFrame_ReturnsTwoLayerObjectsWithCorrectProperties()
        {
            // Arrange
            _layerObject.StartFrame = 10;
            _layerObject.EndFrame = 100;
            _layerObject.Volume = 75;
            var obj1 = new ClipObject("obj1") { StartFrame = 20, EndFrame = 40 };
            var obj2 = new ClipObject("obj2") { StartFrame = 60, EndFrame = 80 };
            _layerObject.Objects.Add(obj1);
            _layerObject.Objects.Add(obj2);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _layerObject.SplitAtFrame(splitFrame);
            var firstLayer = firstClip as LayerObject;
            var secondLayer = secondClip as LayerObject;

            // Assert
            Assert.That(firstLayer, Is.Not.Null);
            Assert.That(secondLayer, Is.Not.Null);
            Assert.That(firstLayer.Id, Is.EqualTo("layer-id_part1"));
            Assert.That(secondLayer.Id, Is.EqualTo("layer-id_part2"));
            Assert.That(firstLayer.StartFrame, Is.EqualTo(10));
            Assert.That(firstLayer.EndFrame, Is.EqualTo(49));
            Assert.That(secondLayer.StartFrame, Is.EqualTo(50));
            Assert.That(secondLayer.EndFrame, Is.EqualTo(100));
            Assert.That(firstLayer.Volume, Is.EqualTo(75));
            Assert.That(secondLayer.Volume, Is.EqualTo(75));
            Assert.That(firstLayer.Name, Is.EqualTo("Test Layer"));
            Assert.That(secondLayer.Name, Is.EqualTo("Test Layer"));
        }

        /// <summary>
        /// 分割時に子オブジェクトが正しく配布されることを確認するテスト
        /// 意図: 分割フレームをまたぐオブジェクトが両方のレイヤーに分割され、位置関係が維持されることを検証
        /// 想定結果: 分割前のオブジェクトは前半レイヤーに、分割後のオブジェクトは後半レイヤーに、分割をまたぐオブジェクトは両方に分割される
        /// </summary>
        [Test]
        public void SplitAtFrame_ObjectsAreDistributedCorrectly()
        {
            // Arrange
            _layerObject.StartFrame = 10;
            _layerObject.EndFrame = 100;
            var objBeforeSplit = new ClipObject("obj1") { StartFrame = 20, EndFrame = 40 };
            var objAfterSplit = new ClipObject("obj2") { StartFrame = 60, EndFrame = 80 };
            var objSpanningSplit = new ClipObject("obj3") { StartFrame = 45, EndFrame = 55 };
            _layerObject.Objects.Add(objBeforeSplit);
            _layerObject.Objects.Add(objAfterSplit);
            _layerObject.Objects.Add(objSpanningSplit);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _layerObject.SplitAtFrame(splitFrame);
            var firstLayer = firstClip as LayerObject;
            var secondLayer = secondClip as LayerObject;

            // Assert
            Assert.That(firstLayer.Objects.Count, Is.EqualTo(2)); // objBeforeSplit + objSpanningSplitの前半
            Assert.That(secondLayer.Objects.Count, Is.EqualTo(2)); // objAfterSplit + objSpanningSplitの後半

            // 分割前のオブジェクトは最初のレイヤーに
            Assert.That(firstLayer.Objects.Any(o => o.Id == "obj1"), Is.True);
            // 分割後のオブジェクトは2番目のレイヤーに
            Assert.That(secondLayer.Objects.Any(o => o.Id == "obj2"), Is.True);
            // 分割にまたがるオブジェクトは両方のレイヤーに分割されて存在
            Assert.That(firstLayer.Objects.Any(o => o.Id.StartsWith("obj3_copy")), Is.True);
            Assert.That(secondLayer.Objects.Any(o => o.Id.StartsWith("obj3_copy")), Is.True);
        }

        /// <summary>
        /// 分割時に音響効果が正しく維持されることを確認するテスト
        /// 意図: レイヤー分割後、音響効果が両方のレイヤーにコピーされることを検証
        /// 想定結果: 分割された両方のレイヤーで音響効果リストに1つの効果が保持される
        /// </summary>
        [Test]
        public void SplitAtFrame_PreservesAudioEffects()
        {
            // Arrange
            _layerObject.StartFrame = 10;
            _layerObject.EndFrame = 100;
            var effect = new Metasia.Core.Objects.AudioEffects.VolumeFadeEffect();
            _layerObject.AudioEffects.Add(effect);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _layerObject.SplitAtFrame(splitFrame);
            var firstLayer = firstClip as LayerObject;
            var secondLayer = secondClip as LayerObject;

            // Assert
            Assert.That(firstLayer.AudioEffects.Count, Is.EqualTo(1));
            Assert.That(secondLayer.AudioEffects.Count, Is.EqualTo(1));
            Assert.That(firstLayer.AudioEffects[0], Is.InstanceOf<Metasia.Core.Objects.AudioEffects.VolumeFadeEffect>());
            Assert.That(secondLayer.AudioEffects[0], Is.InstanceOf<Metasia.Core.Objects.AudioEffects.VolumeFadeEffect>());
        }

        /// <summary>
        /// 空のレイヤーを分割した場合の挙動を確認するテスト
        /// 意図: 子オブジェクトがないレイヤーを分割しても正しく動作することを検証
        /// 想定結果: 2つの空のLayerObjectが返され、フレーム範囲のみが正しく設定される
        /// </summary>
        [Test]
        public void SplitAtFrame_EmptyLayer_ReturnsTwoEmptyLayers()
        {
            // Arrange
            _layerObject.StartFrame = 10;
            _layerObject.EndFrame = 100;
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _layerObject.SplitAtFrame(splitFrame);
            var firstLayer = firstClip as LayerObject;
            var secondLayer = secondClip as LayerObject;

            // Assert
            Assert.That(firstLayer.Objects.Count, Is.EqualTo(0));
            Assert.That(secondLayer.Objects.Count, Is.EqualTo(0));
            Assert.That(firstLayer.StartFrame, Is.EqualTo(10));
            Assert.That(firstLayer.EndFrame, Is.EqualTo(49));
            Assert.That(secondLayer.StartFrame, Is.EqualTo(50));
            Assert.That(secondLayer.EndFrame, Is.EqualTo(100));
        }

        /// <summary>
        /// 分割されたレイヤーオブジェクトが元オブジェクトから独立していることを確認するテスト
        /// 意図: ディープコピーが正しく行われ、元レイヤーの変更が分割オブジェクトに影響しないことを検証
        /// 想定結果: 元レイヤーを変更しても、分割されたレイヤーの音量と子オブジェクトは変更されない
        /// </summary>
        [Test]
        public void SplitAtFrame_LayerObjectsAreIndependent_ModifyingOriginalDoesNotAffectSplits()
        {
            // Arrange
            _layerObject.StartFrame = 10;
            _layerObject.EndFrame = 100;
            _layerObject.Volume = 75;
            var obj = new ClipObject("obj1") { StartFrame = 20, EndFrame = 40 };
            _layerObject.Objects.Add(obj);
            var (firstClip, secondClip) = _layerObject.SplitAtFrame(50);
            var firstLayer = firstClip as LayerObject;
            var secondLayer = secondClip as LayerObject;

            // Act
            _layerObject.Volume = 25;
            _layerObject.Objects.Clear();

            // Assert
            Assert.That(firstLayer.Volume, Is.EqualTo(75));
            Assert.That(secondLayer.Volume, Is.EqualTo(75));
            Assert.That(firstLayer.Objects.Count, Is.EqualTo(1));
            Assert.That(secondLayer.Objects.Count, Is.EqualTo(0));
        }
    }
}
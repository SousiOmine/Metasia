using NUnit.Framework;
using Metasia.Core.Objects;
using System.Collections.ObjectModel;

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
    }
} 
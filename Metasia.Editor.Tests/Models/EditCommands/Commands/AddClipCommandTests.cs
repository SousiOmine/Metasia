using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands.Commands;
using System;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands
{
    [TestFixture]
    public class AddClipCommandTests
    {
        private LayerObject _ownerLayer;
        private ClipObject _targetObject;
        private AddClipCommand? _command;

        [SetUp]
        public void Setup()
        {
            _ownerLayer = new LayerObject("test-layer", "Test Layer");
            _targetObject = new ClipObject("test-clip")
            {
                StartFrame = 10,
                EndFrame = 50
            };
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // 意図：正しいパラメータでコンストラクタが正常に初期化されることを確認
            // Arrange & Act
            _command = new AddClipCommand(_ownerLayer, _targetObject);

            // Assert
            Assert.That(_command.Description, Is.EqualTo("クリップの追加"));
        }

        [Test]
        public void Constructor_WithNullOwnerLayer_ThrowsArgumentNullException()
        {
            // 意図：ownerLayerがnullの場合にArgumentNullExceptionがスローされることを確認（命令網羅）
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddClipCommand(null!, _targetObject));
        }

        [Test]
        public void Constructor_WithNullTargetObject_ThrowsArgumentNullException()
        {
            // 意図：targetObjectがnullの場合にArgumentNullExceptionがスローされることを確認（命令網羅）
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddClipCommand(_ownerLayer, null!));
        }

        [Test]
        public void Execute_WhenObjectNotInLayer_AddsObjectToLayer()
        {
            // 意図：レイヤーにオブジェクトが存在しない場合、Executeでオブジェクトが追加されることを確認
            // Arrange
            _command = new AddClipCommand(_ownerLayer, _targetObject);
            int initialCount = _ownerLayer.Objects.Count;

            // Act
            _command.Execute();

            // Assert
            Assert.That(_ownerLayer.Objects.Count, Is.EqualTo(initialCount + 1));
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.True);
        }

        [Test]
        public void Execute_WhenObjectAlreadyInLayer_DoesNotAddDuplicate()
        {
            // 意図：レイヤーにオブジェクトが既に存在する場合、重複して追加されないことを確認（分岐網羅）
            // Arrange
            _ownerLayer.Objects.Add(_targetObject);
            _command = new AddClipCommand(_ownerLayer, _targetObject);
            int initialCount = _ownerLayer.Objects.Count;

            // Act
            _command.Execute();

            // Assert
            Assert.That(_ownerLayer.Objects.Count, Is.EqualTo(initialCount));
            Assert.That(_ownerLayer.Objects.Count(x => x == _targetObject), Is.EqualTo(1));
        }

        [Test]
        public void Undo_WhenObjectInLayer_RemovesObjectFromLayer()
        {
            // 意図：Undo実行時にオブジェクトがレイヤーから削除されることを確認
            // Arrange
            _ownerLayer.Objects.Add(_targetObject);
            _command = new AddClipCommand(_ownerLayer, _targetObject);

            // Act
            _command.Undo();

            // Assert
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.False);
        }

        [Test]
        public void Undo_WhenObjectNotInLayer_DoesNothing()
        {
            // 意図：レイヤーにオブジェクトが存在しない場合、Undoで何も起きないことを確認（分岐網羅）
            // Arrange
            _command = new AddClipCommand(_ownerLayer, _targetObject);
            int initialCount = _ownerLayer.Objects.Count;

            // Act
            _command.Undo();

            // Assert
            Assert.That(_ownerLayer.Objects.Count, Is.EqualTo(initialCount));
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.False);
        }

        [Test]
        public void ExecuteUndo_ExecuteAgain_AddsAndRemovesCorrectly()
        {
            // 意図：ExecuteとUndoを繰り返しても正しく動作することを確認（回復性テスト）
            // Arrange
            _command = new AddClipCommand(_ownerLayer, _targetObject);

            // Act & Assert - 1回目のExecute
            _command.Execute();
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.True);

            // Undo
            _command.Undo();
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.False);

            // 2回目のExecute
            _command.Execute();
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.True);

            // 2回目のUndo
            _command.Undo();
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.False);
        }

        [Test]
        public void Execute_WithMultipleObjects_OnlyAddsTargetObject()
        {
            // 意図：複数のオブジェクトがあるレイヤーで、ターゲットオブジェクトのみが追加されることを確認
            // Arrange
            var otherObject = new ClipObject("other-clip");
            _ownerLayer.Objects.Add(otherObject);
            _command = new AddClipCommand(_ownerLayer, _targetObject);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_ownerLayer.Objects.Count, Is.EqualTo(2));
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.True);
            Assert.That(_ownerLayer.Objects.Contains(otherObject), Is.True);
        }

        [Test]
        public void Undo_WithMultipleObjects_OnlyRemovesTargetObject()
        {
            // 意図：複数のオブジェクトがあるレイヤーで、ターゲットオブジェクトのみが削除されることを確認
            // Arrange
            var otherObject = new ClipObject("other-clip");
            _ownerLayer.Objects.Add(otherObject);
            _ownerLayer.Objects.Add(_targetObject);
            _command = new AddClipCommand(_ownerLayer, _targetObject);

            // Act
            _command.Undo();

            // Assert
            Assert.That(_ownerLayer.Objects.Count, Is.EqualTo(1));
            Assert.That(_ownerLayer.Objects.Contains(_targetObject), Is.False);
            Assert.That(_ownerLayer.Objects.Contains(otherObject), Is.True);
        }

        [Test]
        public void Constructor_NullOwnerLayer_UsesCorrectParameterName()
        {
            // 意図：ownerLayerがnullの場合の例外メッセージに正しいパラメータ名が含まれることを確認
            // Arrange, Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new AddClipCommand(null!, _targetObject));
            Assert.That(ex.ParamName, Is.EqualTo("ownerLayer"));
        }

        [Test]
        public void Constructor_NullTargetObject_UsesCorrectParameterName()
        {
            // 意図：targetObjectがnullの場合の例外メッセージに正しいパラメータ名が含まれることを確認
            // Arrange, Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new AddClipCommand(_ownerLayer, null!));
            Assert.That(ex.ParamName, Is.EqualTo("targetObject"));
        }

        [Test]
        public void Execute_ThenUndo_RestoresOriginalState()
        {
            // 意図：Executeの後にUndoを実行すると、元の状態に完全に戻ることを確認
            // Arrange
            var originalState = new LayerObject("original", "Original");
            var originalObjects = new System.Collections.ObjectModel.ObservableCollection<ClipObject>();
            foreach (var obj in _ownerLayer.Objects)
            {
                originalObjects.Add(obj);
            }

            _command = new AddClipCommand(_ownerLayer, _targetObject);

            // Act
            _command.Execute();
            _command.Undo();

            // Assert
            Assert.That(_ownerLayer.Objects.Count, Is.EqualTo(originalObjects.Count));
            for (int i = 0; i < originalObjects.Count; i++)
            {
                Assert.That(_ownerLayer.Objects[i], Is.EqualTo(originalObjects[i]));
            }
        }
    }
}
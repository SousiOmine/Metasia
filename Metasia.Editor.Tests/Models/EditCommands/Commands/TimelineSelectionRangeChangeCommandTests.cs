using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands
{
    [TestFixture]
    public class TimelineSelectionRangeChangeCommandTests
    {
        private TimelineObject _timeline;

        [SetUp]
        public void Setup()
        {
            _timeline = new TimelineObject("test-timeline")
            {
                SelectionStart = 100,
                SelectionEnd = 500
            };
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // 意図: 正しいパラメータでコンストラクタが正常に初期化されることを確認
            // Arrange & Act
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 200, 600);

            // Assert
            Assert.That(command.Description, Is.EqualTo("選択範囲を変更"));
        }

        [Test]
        public void Constructor_WithNullTimeline_ThrowsArgumentNullException()
        {
            // 意図: timelineがnullの場合にArgumentNullExceptionがスローされることを確認
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TimelineSelectionRangeChangeCommand(null!, 200, 600));
        }

        [Test]
        public void Execute_ChangesBothStartAndEnd()
        {
            // 意図: ExecuteでSelectionStartとSelectionEndの両方が変更されることを確認
            // Arrange
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 200, 600);

            // Act
            command.Execute();

            // Assert
            Assert.That(_timeline.SelectionStart, Is.EqualTo(200));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(600));
        }

        [Test]
        public void Execute_ChangesOnlyStart()
        {
            // 意図: Startのみを変更する場合、Endは変更されないことを確認
            // Arrange
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 200, 500);

            // Act
            command.Execute();

            // Assert
            Assert.That(_timeline.SelectionStart, Is.EqualTo(200));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(500));
        }

        [Test]
        public void Execute_ChangesOnlyEnd()
        {
            // 意図: Endのみを変更する場合、Startは変更されないことを確認
            // Arrange
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 100, 600);

            // Act
            command.Execute();

            // Assert
            Assert.That(_timeline.SelectionStart, Is.EqualTo(100));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(600));
        }

        [Test]
        public void Undo_RestoresOriginalValues()
        {
            // 意図: Undo実行時に元の値に復元されることを確認
            // Arrange
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 200, 600);

            // Act
            command.Execute();
            command.Undo();

            // Assert
            Assert.That(_timeline.SelectionStart, Is.EqualTo(100));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(500));
        }

        [Test]
        public void ExecuteUndo_ExecuteAgain_ChangesCorrectly()
        {
            // 意図: ExecuteとUndoを繰り返しても正しく動作することを確認（回復性テスト）
            // Arrange
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 200, 600);

            // Act & Assert - 1回目のExecute
            command.Execute();
            Assert.That(_timeline.SelectionStart, Is.EqualTo(200));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(600));

            // Undo
            command.Undo();
            Assert.That(_timeline.SelectionStart, Is.EqualTo(100));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(500));

            // 2回目のExecute
            command.Execute();
            Assert.That(_timeline.SelectionStart, Is.EqualTo(200));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(600));

            // 2回目のUndo
            command.Undo();
            Assert.That(_timeline.SelectionStart, Is.EqualTo(100));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(500));
        }

        [Test]
        public void Execute_ThenUndo_RestoresOriginalState()
        {
            // 意図: Executeの後にUndoを実行すると、元の状態に完全に戻ることを確認
            // Arrange
            int originalStart = _timeline.SelectionStart;
            int originalEnd = _timeline.SelectionEnd;

            var command = new TimelineSelectionRangeChangeCommand(_timeline, 200, 600);

            // Act
            command.Execute();
            command.Undo();

            // Assert
            Assert.That(_timeline.SelectionStart, Is.EqualTo(originalStart));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(originalEnd));
        }

        [Test]
        public void Execute_CanSetToMaxValue()
        {
            // 意図: SelectionEndをint.MaxValueに設定できることを確認
            // Arrange
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 0, int.MaxValue);

            // Act
            command.Execute();

            // Assert
            Assert.That(_timeline.SelectionStart, Is.EqualTo(0));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void Execute_CanSetToZero()
        {
            // 意図: 選択範囲を0に設定できることを確認
            // Arrange
            var command = new TimelineSelectionRangeChangeCommand(_timeline, 0, 0);

            // Act
            command.Execute();

            // Assert
            Assert.That(_timeline.SelectionStart, Is.EqualTo(0));
            Assert.That(_timeline.SelectionEnd, Is.EqualTo(0));
        }
    }
}

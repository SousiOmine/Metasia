using NUnit.Framework;
using Metasia.Editor.Models.EditCommands;
using Moq;

namespace Metasia.Editor.Tests.Models.EditCommands
{
    [TestFixture]
    public class EditCommandManagerTests
    {
        private EditCommandManager _manager;
        private Mock<IEditCommand> _mockCommand;

        [SetUp]
        public void Setup()
        {
            _manager = new EditCommandManager();
            _mockCommand = new Mock<IEditCommand>();
            _mockCommand.Setup(c => c.Description).Returns("Test Command");
        }

        [Test]
        public void InitialState_CannotUndoOrRedo()
        {
            // Assert
            Assert.That(_manager.CanUndo, Is.False);
            Assert.That(_manager.CanRedo, Is.False);
        }

        [Test]
        public void Execute_CallsCommandExecute()
        {
            // Act
            _manager.Execute(_mockCommand.Object);

            // Assert
            _mockCommand.Verify(c => c.Execute(), Times.Once);
        }

        [Test]
        public void Execute_EnablesUndo()
        {
            // Act
            _manager.Execute(_mockCommand.Object);

            // Assert
            Assert.That(_manager.CanUndo, Is.True);
            Assert.That(_manager.CanRedo, Is.False);
        }

        [Test]
        public void Execute_ClearsRedoStack()
        {
            // Arrange
            var secondCommand = new Mock<IEditCommand>();
            _manager.Execute(_mockCommand.Object);
            _manager.Undo();
            Assert.That(_manager.CanRedo, Is.True);

            // Act
            _manager.Execute(secondCommand.Object);

            // Assert
            Assert.That(_manager.CanRedo, Is.False);
        }

        [Test]
        public void Execute_FiresCommandExecutedEvent()
        {
            // Arrange
            IEditCommand? executedCommand = null;
            _manager.CommandExecuted += (sender, command) => executedCommand = command;

            // Act
            _manager.Execute(_mockCommand.Object);

            // Assert
            Assert.That(executedCommand, Is.EqualTo(_mockCommand.Object));
        }

        [Test]
        public void Undo_CallsCommandUndo()
        {
            // Arrange
            _manager.Execute(_mockCommand.Object);

            // Act
            _manager.Undo();

            // Assert
            _mockCommand.Verify(c => c.Undo(), Times.Once);
        }

        [Test]
        public void Undo_MovesCommandToRedoStack()
        {
            // Arrange
            _manager.Execute(_mockCommand.Object);

            // Act
            _manager.Undo();

            // Assert
            Assert.That(_manager.CanUndo, Is.False);
            Assert.That(_manager.CanRedo, Is.True);
        }

        [Test]
        public void Undo_FiresCommandUndoneEvent()
        {
            // Arrange
            _manager.Execute(_mockCommand.Object);
            IEditCommand? undoneCommand = null;
            _manager.CommandUndone += (sender, command) => undoneCommand = command;

            // Act
            _manager.Undo();

            // Assert
            Assert.That(undoneCommand, Is.EqualTo(_mockCommand.Object));
        }

        [Test]
        public void Undo_WhenNoCommands_DoesNothing()
        {
            // Arrange
            bool eventFired = false;
            _manager.CommandUndone += (sender, command) => eventFired = true;

            // Act
            _manager.Undo();

            // Assert
            Assert.That(eventFired, Is.False);
        }

        [Test]
        public void Redo_CallsCommandExecute()
        {
            // Arrange
            _manager.Execute(_mockCommand.Object);
            _manager.Undo();

            // Act
            _manager.Redo();

            // Assert
            _mockCommand.Verify(c => c.Execute(), Times.Exactly(2)); // 初回Execute + Redo
        }

        [Test]
        public void Redo_MovesCommandBackToUndoStack()
        {
            // Arrange
            _manager.Execute(_mockCommand.Object);
            _manager.Undo();

            // Act
            _manager.Redo();

            // Assert
            Assert.That(_manager.CanUndo, Is.True);
            Assert.That(_manager.CanRedo, Is.False);
        }

        [Test]
        public void Redo_FiresCommandRedoneEvent()
        {
            // Arrange
            _manager.Execute(_mockCommand.Object);
            _manager.Undo();
            IEditCommand? redoneCommand = null;
            _manager.CommandRedone += (sender, command) => redoneCommand = command;

            // Act
            _manager.Redo();

            // Assert
            Assert.That(redoneCommand, Is.EqualTo(_mockCommand.Object));
        }

        [Test]
        public void Redo_WhenNoCommands_DoesNothing()
        {
            // Arrange
            bool eventFired = false;
            _manager.CommandRedone += (sender, command) => eventFired = true;

            // Act
            _manager.Redo();

            // Assert
            Assert.That(eventFired, Is.False);
        }

        [Test]
        public void Clear_RemovesAllCommands()
        {
            // Arrange
            _manager.Execute(_mockCommand.Object);
            _manager.Execute(new Mock<IEditCommand>().Object);
            _manager.Undo();
            Assert.That(_manager.CanUndo, Is.True);
            Assert.That(_manager.CanRedo, Is.True);

            // Act
            _manager.Clear();

            // Assert
            Assert.That(_manager.CanUndo, Is.False);
            Assert.That(_manager.CanRedo, Is.False);
        }

        [Test]
        public void ComplexScenario_MultipleCommandsUndoRedo()
        {
            // Arrange
            var command1 = new Mock<IEditCommand>();
            var command2 = new Mock<IEditCommand>();
            var command3 = new Mock<IEditCommand>();

            // Act & Assert - Execute 3 commands
            _manager.Execute(command1.Object);
            _manager.Execute(command2.Object);
            _manager.Execute(command3.Object);
            Assert.That(_manager.CanUndo, Is.True);
            Assert.That(_manager.CanRedo, Is.False);

            // Undo 2 commands
            _manager.Undo();
            _manager.Undo();
            Assert.That(_manager.CanUndo, Is.True);
            Assert.That(_manager.CanRedo, Is.True);

            // Redo 1 command
            _manager.Redo();
            Assert.That(_manager.CanUndo, Is.True);
            Assert.That(_manager.CanRedo, Is.True);

            // Execute new command (should clear redo stack)
            var command4 = new Mock<IEditCommand>();
            _manager.Execute(command4.Object);
            Assert.That(_manager.CanUndo, Is.True);
            Assert.That(_manager.CanRedo, Is.False);
        }
    }
}
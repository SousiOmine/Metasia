using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands
{
    [TestFixture]
    public class LayerIsActiveChangeCommandTests
    {
        private LayerObject _targetLayer;
        private LayerIsActiveChangeCommand? _command;

        [SetUp]
        public void Setup()
        {
            _targetLayer = new LayerObject("test-layer", "Test Layer")
            {
                IsActive = true
            };
        }

        [Test]
        public void Execute_ChangesIsActiveToFalse()
        {
            // Arrange
            _command = new LayerIsActiveChangeCommand(_targetLayer, false);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetLayer.IsActive, Is.False);
        }

        [Test]
        public void Execute_ChangesIsActiveToTrue()
        {
            // Arrange
            _targetLayer.IsActive = false;
            _command = new LayerIsActiveChangeCommand(_targetLayer, true);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetLayer.IsActive, Is.True);
        }

        [Test]
        public void Undo_RestoresOriginalValue_FromTrueToFalse()
        {
            // Arrange
            _targetLayer.IsActive = true;
            _command = new LayerIsActiveChangeCommand(_targetLayer, false);
            _command.Execute();

            // Act
            _command.Undo();

            // Assert
            Assert.That(_targetLayer.IsActive, Is.True);
        }

        [Test]
        public void Undo_RestoresOriginalValue_FromFalseToTrue()
        {
            // Arrange
            _targetLayer.IsActive = false;
            _command = new LayerIsActiveChangeCommand(_targetLayer, true);
            _command.Execute();

            // Act
            _command.Undo();

            // Assert
            Assert.That(_targetLayer.IsActive, Is.False);
        }

        [Test]
        public void ExecuteUndo_CanBeRepeated()
        {
            // Arrange
            bool originalState = _targetLayer.IsActive;
            _command = new LayerIsActiveChangeCommand(_targetLayer, !originalState);

            // Act & Assert - Execute
            _command.Execute();
            Assert.That(_targetLayer.IsActive, Is.EqualTo(!originalState));

            // Undo
            _command.Undo();
            Assert.That(_targetLayer.IsActive, Is.EqualTo(originalState));

            // Execute again
            _command.Execute();
            Assert.That(_targetLayer.IsActive, Is.EqualTo(!originalState));

            // Undo again
            _command.Undo();
            Assert.That(_targetLayer.IsActive, Is.EqualTo(originalState));
        }

        [Test]
        public void Description_IsEmptyString()
        {
            // Arrange & Act
            _command = new LayerIsActiveChangeCommand(_targetLayer, false);

            // Assert
            Assert.That(_command.Description, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Execute_WhenSettingSameValue_StillWorks()
        {
            // Arrange
            _targetLayer.IsActive = true;
            _command = new LayerIsActiveChangeCommand(_targetLayer, true);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetLayer.IsActive, Is.True);

            // Undo should still restore the original state
            _command.Undo();
            Assert.That(_targetLayer.IsActive, Is.True);
        }
    }
} 
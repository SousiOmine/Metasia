using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands
{
    [TestFixture]
    public class ClipResizeCommandTests
    {
        private MetasiaObject _targetObject;
        private ClipResizeCommand? _command;

        [SetUp]
        public void Setup()
        {
            _targetObject = new MetasiaObject("test-object")
            {
                StartFrame = 10,
                EndFrame = 50
            };
        }

        [Test]
        public void Execute_UpdatesStartAndEndFrames()
        {
            // Arrange
            int newStartFrame = 20;
            int newEndFrame = 80;
            _command = new ClipResizeCommand(_targetObject, 10, newStartFrame, 50, newEndFrame);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetObject.StartFrame, Is.EqualTo(newStartFrame));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(newEndFrame));
        }

        [Test]
        public void Undo_RestoresOriginalFrames()
        {
            // Arrange
            int originalStart = _targetObject.StartFrame;
            int originalEnd = _targetObject.EndFrame;
            _command = new ClipResizeCommand(_targetObject, originalStart, 30, originalEnd, 70);
            _command.Execute();

            // Act
            _command.Undo();

            // Assert
            Assert.That(_targetObject.StartFrame, Is.EqualTo(originalStart));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(originalEnd));
        }

        [Test]
        public void ExecuteUndo_CanBeRepeated()
        {
            // Arrange
            int originalStart = _targetObject.StartFrame;
            int originalEnd = _targetObject.EndFrame;
            int newStart = 5;
            int newEnd = 100;
            _command = new ClipResizeCommand(_targetObject, originalStart, newStart, originalEnd, newEnd);

            // Act & Assert - First Execute
            _command.Execute();
            Assert.That(_targetObject.StartFrame, Is.EqualTo(newStart));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(newEnd));

            // Undo
            _command.Undo();
            Assert.That(_targetObject.StartFrame, Is.EqualTo(originalStart));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(originalEnd));

            // Execute again
            _command.Execute();
            Assert.That(_targetObject.StartFrame, Is.EqualTo(newStart));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(newEnd));

            // Undo again
            _command.Undo();
            Assert.That(_targetObject.StartFrame, Is.EqualTo(originalStart));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(originalEnd));
        }

        [Test]
        public void Constructor_PreservesAllValues()
        {
            // Arrange & Act
            _command = new ClipResizeCommand(_targetObject, 1, 2, 3, 4);

            // Execute to verify values are stored correctly
            _command.Execute();
            Assert.That(_targetObject.StartFrame, Is.EqualTo(2));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(4));

            _command.Undo();
            Assert.That(_targetObject.StartFrame, Is.EqualTo(1));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(3));
        }

        [Test]
        public void Execute_WithNegativeFrames_StillWorks()
        {
            // Arrange
            _command = new ClipResizeCommand(_targetObject, 10, -5, 50, 30);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetObject.StartFrame, Is.EqualTo(-5));
            Assert.That(_targetObject.EndFrame, Is.EqualTo(30));
        }
    }
} 
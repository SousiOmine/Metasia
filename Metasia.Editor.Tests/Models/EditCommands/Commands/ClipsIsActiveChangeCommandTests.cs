using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands
{
    [TestFixture]
    public class ClipsIsActiveChangeCommandTests
    {
        private List<ClipObject> _targetClips;
        private ClipsIsActiveChangeCommand? _command;

        [SetUp]
        public void Setup()
        {
            _targetClips = new List<ClipObject>
            {
                new ClipObject("clip-1") { IsActive = true },
                new ClipObject("clip-2") { IsActive = false },
                new ClipObject("clip-3") { IsActive = true }
            };
        }

        [Test]
        public void Execute_ChangesAllClipsToFalse()
        {
            // Arrange
            _command = new ClipsIsActiveChangeCommand(_targetClips, false);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetClips.All(c => c.IsActive == false), Is.True);
        }

        [Test]
        public void Execute_ChangesAllClipsToTrue()
        {
            // Arrange
            _command = new ClipsIsActiveChangeCommand(_targetClips, true);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetClips.All(c => c.IsActive == true), Is.True);
        }

        [Test]
        public void Undo_RestoresOriginalStates()
        {
            // Arrange
            var originalStates = _targetClips.Select(c => c.IsActive).ToList();
            _command = new ClipsIsActiveChangeCommand(_targetClips, false);
            _command.Execute();

            // Act
            _command.Undo();

            // Assert
            for (int i = 0; i < _targetClips.Count; i++)
            {
                Assert.That(_targetClips[i].IsActive, Is.EqualTo(originalStates[i]));
            }
        }

        [Test]
        public void ExecuteUndo_CanBeRepeated()
        {
            // Arrange
            var originalStates = _targetClips.Select(c => c.IsActive).ToList();
            _command = new ClipsIsActiveChangeCommand(_targetClips, !originalStates.First());

            // Act & Assert - Execute
            _command.Execute();
            Assert.That(_targetClips.All(c => c.IsActive == !originalStates.First()), Is.True);

            // Undo
            _command.Undo();
            for (int i = 0; i < _targetClips.Count; i++)
            {
                Assert.That(_targetClips[i].IsActive, Is.EqualTo(originalStates[i]));
            }

            // Execute again
            _command.Execute();
            Assert.That(_targetClips.All(c => c.IsActive == !originalStates.First()), Is.True);

            // Undo again
            _command.Undo();
            for (int i = 0; i < _targetClips.Count; i++)
            {
                Assert.That(_targetClips[i].IsActive, Is.EqualTo(originalStates[i]));
            }
        }

        [Test]
        public void Description_IsCorrect()
        {
            // Arrange & Act
            _command = new ClipsIsActiveChangeCommand(_targetClips, false);

            // Assert
            Assert.That(_command.Description, Is.EqualTo("クリップの選択状態変更"));
        }

        [Test]
        public void Execute_WithEmptyCollection_DoesNothing()
        {
            // Arrange
            var emptyClips = new List<ClipObject>();
            _command = new ClipsIsActiveChangeCommand(emptyClips, true);

            // Act & Assert
            Assert.DoesNotThrow(() => _command.Execute());
            Assert.DoesNotThrow(() => _command.Undo());
        }

        [Test]
        public void Execute_WithSingleClip_WorksCorrectly()
        {
            // Arrange
            var singleClip = new List<ClipObject> { new ClipObject("single-clip") { IsActive = true } };
            _command = new ClipsIsActiveChangeCommand(singleClip, false);

            // Act
            _command.Execute();

            // Assert
            Assert.That(singleClip[0].IsActive, Is.False);

            // Undo
            _command.Undo();
            Assert.That(singleClip[0].IsActive, Is.True);
        }

        [Test]
        public void Execute_WhenSettingSameValue_StillWorks()
        {
            // Arrange
            foreach (var clip in _targetClips)
            {
                clip.IsActive = true;
            }
            _command = new ClipsIsActiveChangeCommand(_targetClips, true);

            // Act
            _command.Execute();

            // Assert
            Assert.That(_targetClips.All(c => c.IsActive == true), Is.True);

            // Undo should still restore the original state
            _command.Undo();
            Assert.That(_targetClips.All(c => c.IsActive == true), Is.True);
        }
    }
}
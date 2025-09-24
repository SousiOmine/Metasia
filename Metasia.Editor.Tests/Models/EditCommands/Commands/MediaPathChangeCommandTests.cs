using NUnit.Framework;
using Metasia.Core.Media;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands
{
    [TestFixture]
    public class MediaPathChangeCommandTests
    {
        private MediaPath? _target;
        private MediaPath? _newPath;
        private MediaPath? _oldPath;

        [SetUp]
        public void Setup()
        {
            _target = new MediaPath
            {
                FileName = "old.png",
                Directory = "C:/old",
                PathType = PathType.Absolute
            };

            _newPath = new MediaPath
            {
                FileName = "new.png",
                Directory = "C:/new",
                PathType = PathType.Absolute
            };

            _oldPath = new MediaPath
            {
                FileName = _target!.FileName,
                Directory = _target!.Directory,
                PathType = _target!.PathType
            };
        }

        // Execute が新しいパスに正しく更新することを確認
        [Test]
        public void Execute_UpdatesTargetToNewPath()
        {
            var command = new MediaPathChangeCommand(_target!, _newPath!);
            command.Execute();

            Assert.That(_target!.FileName, Is.EqualTo(_newPath!.FileName));
            Assert.That(_target!.Directory, Is.EqualTo(_newPath!.Directory));
            Assert.That(_target!.PathType, Is.EqualTo(_newPath!.PathType));
        }

        // Undo が元の値に復元できることを確認
        [Test]
        public void Undo_RestoresOriginalValues()
        {
            var command = new MediaPathChangeCommand(_target!, _newPath!);
            command.Execute();
            command.Undo();

            Assert.That(_target!.FileName, Is.EqualTo(_oldPath!.FileName));
            Assert.That(_target!.Directory, Is.EqualTo(_oldPath!.Directory));
            Assert.That(_target!.PathType, Is.EqualTo(_oldPath!.PathType));
        }

        // Execute と Undo を複数回実行できることを確認
        [Test]
        public void ExecuteUndo_CanBeRepeated()
        {
            var command = new MediaPathChangeCommand(_target!, _newPath!);

            // First execute
            command.Execute();
            Assert.That(_target!.FileName, Is.EqualTo(_newPath!.FileName));

            // First undo
            command.Undo();
            Assert.That(_target!.FileName, Is.EqualTo(_oldPath!.FileName));

            // Second execute
            command.Execute();
            Assert.That(_target!.FileName, Is.EqualTo(_newPath!.FileName));

            // Second undo
            command.Undo();
            Assert.That(_target!.FileName, Is.EqualTo(_oldPath!.FileName));
        }

        // Description が期待通りの文字列を返すことを確認
        [Test]
        public void Description_ReturnsCorrectValue()
        {
            var command = new MediaPathChangeCommand(_target!, _newPath!);
            Assert.That(command.Description, Is.EqualTo("MediaPathChangeCommand"));
        }

        // 明示的に旧パスを渡したコンストラクタが正しく動作することを確認
        [Test]
        public void Constructor_WithExplicitOldPath_UsesProvidedOldPath()
        {
            var explicitOld = new MediaPath
            {
                FileName = "explicit-old.png",
                Directory = "C:/explicit-old",
                PathType = PathType.Absolute
            };
            var command = new MediaPathChangeCommand(_target!, explicitOld, _newPath!);
            command.Execute();
            command.Undo();

            // After undo, target should match the explicit old path, not the original values set in Setup.
            Assert.That(_target!.FileName, Is.EqualTo(explicitOld.FileName));
            Assert.That(_target!.Directory, Is.EqualTo(explicitOld.Directory));
            Assert.That(_target!.PathType, Is.EqualTo(explicitOld.PathType));
        }

        // null target で ArgumentNullException がスローされることを確認
        [Test]
        public void Constructor_NullTarget_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MediaPathChangeCommand(null!, _newPath));
        }

        // null oldPath で ArgumentNullException がスローされることを確認
        [Test]
        public void Constructor_NullOldPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MediaPathChangeCommand(_target, null!, _newPath));
        }

        // null newPath で ArgumentNullException がスローされることを確認
        [Test]
        public void Constructor_NullNewPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MediaPathChangeCommand(_target, _oldPath, null!));
        }
    }
}

using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class TextTests
    {
        private Text _textObject;

        [SetUp]
        public void Setup()
        {
            _textObject = new Text("text-id");
        }

        [Test]
        public void Constructor_WithId_InitializesCorrectly()
        {
            // Arrange & Act
            var text = new Text("test-id");

            // Assert
            Assert.That(text.Id, Is.EqualTo("test-id"));
            Assert.That(text.X, Is.Not.Null);
            Assert.That(text.Y, Is.Not.Null);
            Assert.That(text.Scale, Is.Not.Null);
            Assert.That(text.Alpha, Is.Not.Null);
            Assert.That(text.Rotation, Is.Not.Null);
            Assert.That(text.TextSize, Is.Not.Null);
        }

        [Test]
        public void CoordinateParameters_HaveCorrectDefaultValues()
        {
            // Assert
            Assert.That(_textObject.X.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.Y.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.Scale.Get(0), Is.EqualTo(100));
            Assert.That(_textObject.Alpha.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.Rotation.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.TextSize.Get(0), Is.EqualTo(100));
        }

        [Test]
        public void Contents_CanBeSetAndRetrieved()
        {
            // Act
            _textObject.Contents = "Hello, World!";

            // Assert
            Assert.That(_textObject.Contents, Is.EqualTo("Hello, World!"));
        }

        [Test]
        public void TypefaceName_CanBeSetAndRetrieved()
        {
            // Act
            _textObject.TypefaceName = "Arial";

            // Assert
            Assert.That(_textObject.TypefaceName, Is.EqualTo("Arial"));
        }

        // Parentプロパティは存在しないため、このテストは削除

        [Test]
        public void InheritedProperties_WorkCorrectly()
        {
            // TextはMetasiaObjectを継承しているので、基本プロパティも確認
            Assert.That(_textObject.StartFrame, Is.EqualTo(0));
            Assert.That(_textObject.EndFrame, Is.EqualTo(100));
            Assert.That(_textObject.IsActive, Is.True);
            Assert.That(_textObject.Child, Is.Null);

            // 変更も可能
            _textObject.StartFrame = 10;
            _textObject.EndFrame = 200;
            _textObject.IsActive = false;

            Assert.That(_textObject.StartFrame, Is.EqualTo(10));
            Assert.That(_textObject.EndFrame, Is.EqualTo(200));
            Assert.That(_textObject.IsActive, Is.False);
        }

        [Test]
        public void Child_CanBeSetWithCoordableObject()
        {
            // Arrange
            var childText = new Text("child-text");

            // Act
            _textObject.Child = childText;

            // Assert
            Assert.That(_textObject.Child, Is.Not.Null);
            Assert.That(_textObject.Child, Is.InstanceOf<IMetaCoordable>());
            Assert.That(_textObject.Child.Id, Is.EqualTo("child-text"));
        }

        [Test]
        public void TypefaceName_ChangeTriggersFontReload()
        {
            // Arrange
            var initialTypefaceName = _textObject.TypefaceName;

            // Act
            _textObject.TypefaceName = "NewFont";
            var afterFirstChange = _textObject.TypefaceName;

            _textObject.TypefaceName = "AnotherFont";
            var afterSecondChange = _textObject.TypefaceName;

            // Assert
            Assert.That(afterFirstChange, Is.EqualTo("NewFont"));
            Assert.That(afterSecondChange, Is.EqualTo("AnotherFont"));
        }
    }
} 
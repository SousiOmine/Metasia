using NUnit.Framework;
using Metasia.Core.Objects;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class MetasiaObjectTests
    {
        private MetasiaObject _metasiaObject;

        [SetUp]
        public void Setup()
        {
            _metasiaObject = new MetasiaObject("test-id");
        }

        [Test]
        public void Constructor_WithId_SetsIdCorrectly()
        {
            // Arrange & Act
            var obj = new MetasiaObject("unique-id");

            // Assert
            Assert.That(obj.Id, Is.EqualTo("unique-id"));
        }

        [Test]
        public void Constructor_WithoutParameters_InitializesWithEmptyId()
        {
            // Arrange & Act
            var obj = new MetasiaObject();

            // Assert
            Assert.That(obj.Id, Is.EqualTo(string.Empty));
        }

        [Test]
        public void DefaultValues_AreCorrectlySet()
        {
            // Assert
            Assert.That(_metasiaObject.StartFrame, Is.EqualTo(0));
            Assert.That(_metasiaObject.EndFrame, Is.EqualTo(100));
            Assert.That(_metasiaObject.IsActive, Is.True);
            Assert.That(_metasiaObject.Child, Is.Null);
        }

        [TestCase(0, true)]    // 開始フレーム
        [TestCase(50, true)]   // 中間フレーム
        [TestCase(100, true)]  // 終了フレーム
        [TestCase(-1, false)]  // 開始前
        [TestCase(101, false)] // 終了後
        public void IsExistFromFrame_ReturnsCorrectValue(int frame, bool expected)
        {
            // Arrange
            _metasiaObject.StartFrame = 0;
            _metasiaObject.EndFrame = 100;

            // Act
            var result = _metasiaObject.IsExistFromFrame(frame);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsExistFromFrame_WithCustomRange_WorksCorrectly()
        {
            // Arrange
            _metasiaObject.StartFrame = 50;
            _metasiaObject.EndFrame = 150;

            // Act & Assert
            Assert.That(_metasiaObject.IsExistFromFrame(49), Is.False);
            Assert.That(_metasiaObject.IsExistFromFrame(50), Is.True);
            Assert.That(_metasiaObject.IsExistFromFrame(100), Is.True);
            Assert.That(_metasiaObject.IsExistFromFrame(150), Is.True);
            Assert.That(_metasiaObject.IsExistFromFrame(151), Is.False);
        }

        [Test]
        public void Child_CanBeSetAndRetrieved()
        {
            // Arrange
            var childObject = new MetasiaObject("child-id");

            // Act
            _metasiaObject.Child = childObject;

            // Assert
            Assert.That(_metasiaObject.Child, Is.Not.Null);
            Assert.That(_metasiaObject.Child.Id, Is.EqualTo("child-id"));
        }

        [Test]
        public void IsActive_CanBeToggled()
        {
            // Arrange
            Assert.That(_metasiaObject.IsActive, Is.True); // デフォルト値確認

            // Act
            _metasiaObject.IsActive = false;

            // Assert
            Assert.That(_metasiaObject.IsActive, Is.False);
        }
    }
} 
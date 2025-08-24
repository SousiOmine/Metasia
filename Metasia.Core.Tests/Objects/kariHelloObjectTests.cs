using NUnit.Framework;
using Metasia.Core.Objects;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class kariHelloObjectTests
    {
        private kariHelloObject _kariHelloObject;

        [SetUp]
        public void Setup()
        {
            _kariHelloObject = new kariHelloObject("kari-id");
        }

        [Test]
        public void Constructor_WithId_InitializesCorrectly()
        {
            // Arrange & Act
            var obj = new kariHelloObject("test-id");

            // Assert
            Assert.That(obj.Id, Is.EqualTo("test-id"));
            Assert.That(obj.X, Is.Not.Null);
            Assert.That(obj.Y, Is.Not.Null);
            Assert.That(obj.Scale, Is.Not.Null);
            Assert.That(obj.Alpha, Is.Not.Null);
            Assert.That(obj.Rotation, Is.Not.Null);
            Assert.That(obj.Volume, Is.EqualTo(100));
        }

        [Test]
        public void Constructor_WithoutParameters_InitializesWithDefaults()
        {
            // Arrange & Act
            var obj = new kariHelloObject();

            // Assert
            Assert.That(obj.X, Is.Null); // パラメータなしコンストラクタでは座標パラメータは初期化されない
            Assert.That(obj.Y, Is.Null);
            Assert.That(obj.Scale, Is.Null);
            Assert.That(obj.Alpha, Is.Null);
            Assert.That(obj.Rotation, Is.Null);
            Assert.That(obj.Volume, Is.EqualTo(100));
        }

        [Test]
        public void CoordinateParameters_HaveCorrectDefaultValues()
        {
            // Assert
            Assert.That(_kariHelloObject.X.Get(0), Is.EqualTo(0));
            Assert.That(_kariHelloObject.Y.Get(0), Is.EqualTo(0));
            Assert.That(_kariHelloObject.Scale.Get(0), Is.EqualTo(100));
            Assert.That(_kariHelloObject.Alpha.Get(0), Is.EqualTo(0));
            Assert.That(_kariHelloObject.Rotation.Get(0), Is.EqualTo(0));
        }

        [Test]
        public void Volume_CanBeModified()
        {
            // Arrange
            Assert.That(_kariHelloObject.Volume, Is.EqualTo(100)); // デフォルト値確認

            // Act
            _kariHelloObject.Volume = 50;

            // Assert
            Assert.That(_kariHelloObject.Volume, Is.EqualTo(50));
        }

        [Test]
        public void InheritedProperties_WorkCorrectly()
        {
            // kariHelloObjectはMetasiaObjectを継承しているので、基本プロパティも確認
            Assert.That(_kariHelloObject.StartFrame, Is.EqualTo(0));
            Assert.That(_kariHelloObject.EndFrame, Is.EqualTo(100));
            Assert.That(_kariHelloObject.IsActive, Is.True);
            Assert.That(_kariHelloObject.Child, Is.Null);

            // 変更も可能
            _kariHelloObject.StartFrame = 10;
            _kariHelloObject.EndFrame = 200;
            _kariHelloObject.IsActive = false;

            Assert.That(_kariHelloObject.StartFrame, Is.EqualTo(10));
            Assert.That(_kariHelloObject.EndFrame, Is.EqualTo(200));
            Assert.That(_kariHelloObject.IsActive, Is.False);
        }

        [Test]
        public void ImplementsRequiredInterfaces()
        {
            // kariHelloObjectが必要なインターフェースを実装していることを確認
            Assert.That(_kariHelloObject, Is.InstanceOf<IAudible>());
            Assert.That(_kariHelloObject, Is.InstanceOf<IRenderable>());
        }
    }
} 
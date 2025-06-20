using NUnit.Framework;
using Metasia.Core.Coordinate;

namespace Metasia.Core.Tests.Coordinate
{
    [TestFixture]
    public class CoordPointTests
    {
        private CoordPoint _coordPoint;

        [SetUp]
        public void Setup()
        {
            _coordPoint = new CoordPoint();
        }

        [Test]
        public void Constructor_DefaultConstructor_InitializesWithDefaults()
        {
            // Act
            var point = new CoordPoint();

            // Assert
            Assert.That(point.Frame, Is.EqualTo(0));
            Assert.That(point.Value, Is.EqualTo(0f));
            Assert.That(point.JSLogic, Is.Not.Null);
            Assert.That(point.JSLogic, Is.Not.Empty);
        }

        [Test]
        public void Frame_CanBeSetAndRetrieved()
        {
            // Arrange
            const int expectedFrame = 100;

            // Act
            _coordPoint.Frame = expectedFrame;

            // Assert
            Assert.That(_coordPoint.Frame, Is.EqualTo(expectedFrame));
        }

        [Test]
        public void Value_CanBeSetAndRetrieved()
        {
            // Arrange
            const double expectedValue = 123.456;

            // Act
            _coordPoint.Value = expectedValue;

            // Assert
            Assert.That(_coordPoint.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void JSLogic_CanBeSetAndRetrieved()
        {
            // Arrange
            const string customLogic = "return StartValue + 10;";

            // Act
            _coordPoint.JSLogic = customLogic;

            // Assert
            Assert.That(_coordPoint.JSLogic, Is.EqualTo(customLogic));
        }

        [Test]
        public void JSLogic_DefaultValue_ContainsLinearInterpolation()
        {
            // Act
            var defaultLogic = _coordPoint.JSLogic;

            // Assert
            Assert.That(defaultLogic, Does.Contain("StartValue"));
            Assert.That(defaultLogic, Does.Contain("EndValue"));
            Assert.That(defaultLogic, Does.Contain("NowFrame"));
            Assert.That(defaultLogic, Does.Contain("StartFrame"));
            Assert.That(defaultLogic, Does.Contain("EndFrame"));
        }

        [TestCase(-100)]
        [TestCase(0)]
        [TestCase(50)]
        [TestCase(100)]
        [TestCase(999)]
        public void Frame_AcceptsVariousValues(int frame)
        {
            // Act
            _coordPoint.Frame = frame;

            // Assert
            Assert.That(_coordPoint.Frame, Is.EqualTo(frame));
        }

        [TestCase(-999.999)]
        [TestCase(0.0)]
        [TestCase(123.456)]
        [TestCase(999.999)]
        public void Value_AcceptsVariousDoubleValues(double value)
        {
            // Act
            _coordPoint.Value = value;

            // Assert
            Assert.That(_coordPoint.Value, Is.EqualTo(value));
        }
    }
} 
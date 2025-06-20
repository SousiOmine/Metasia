using NUnit.Framework;
using SkiaSharp;
using Metasia.Core.Project;

namespace Metasia.Core.Tests.Project
{
    [TestFixture]
    public class ProjectInfoTests
    {
        private ProjectInfo _projectInfo;

        [SetUp]
        public void SetUp()
        {
            _projectInfo = new ProjectInfo();
        }

        [Test]
        public void Framerate_GetSet_WorksCorrectly()
        {
            // Arrange
            int expectedFramerate = 60;

            // Act
            _projectInfo.Framerate = expectedFramerate;
            int actualFramerate = _projectInfo.Framerate;

            // Assert
            Assert.That(actualFramerate, Is.EqualTo(expectedFramerate));
        }

        [Test]
        public void Framerate_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange & Act
            _projectInfo.Framerate = 30;
            _projectInfo.Framerate = 60;
            _projectInfo.Framerate = 120;

            // Assert
            Assert.That(_projectInfo.Framerate, Is.EqualTo(120));
        }

        [Test]
        public void Size_GetSet_WorksCorrectly()
        {
            // Arrange
            var expectedSize = new SKSize(1920, 1080);

            // Act
            _projectInfo.Size = expectedSize;
            var actualSize = _projectInfo.Size;

            // Assert
            Assert.That(actualSize, Is.EqualTo(expectedSize));
            Assert.That(actualSize.Width, Is.EqualTo(1920));
            Assert.That(actualSize.Height, Is.EqualTo(1080));
        }

        [Test]
        public void Size_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange & Act
            _projectInfo.Size = new SKSize(1920, 1080);
            _projectInfo.Size = new SKSize(3840, 2160);
            _projectInfo.Size = new SKSize(1280, 720);

            // Assert
            Assert.That(_projectInfo.Size.Width, Is.EqualTo(1280));
            Assert.That(_projectInfo.Size.Height, Is.EqualTo(720));
        }

        [Test]
        public void Size_SetWithDifferentAspectRatios_WorksCorrectly()
        {
            // Test 16:9 aspect ratio
            _projectInfo.Size = new SKSize(1920, 1080);
            Assert.That(_projectInfo.Size.Width / _projectInfo.Size.Height, Is.EqualTo(16.0f / 9.0f).Within(0.01f));

            // Test 4:3 aspect ratio
            _projectInfo.Size = new SKSize(1024, 768);
            Assert.That(_projectInfo.Size.Width / _projectInfo.Size.Height, Is.EqualTo(4.0f / 3.0f).Within(0.01f));

            // Test 21:9 aspect ratio (ultrawide)
            _projectInfo.Size = new SKSize(2560, 1080);
            Assert.That(_projectInfo.Size.Width / _projectInfo.Size.Height, Is.EqualTo(2560.0f / 1080.0f).Within(0.01f));
        }

        [Test]
        public void DefaultValues_AreNotSet_ReturnsDefault()
        {
            // Arrange
            var newProjectInfo = new ProjectInfo();

            // Assert
            Assert.That(newProjectInfo.Framerate, Is.EqualTo(0));
            Assert.That(newProjectInfo.Size, Is.EqualTo(default(SKSize)));
        }
    }
} 
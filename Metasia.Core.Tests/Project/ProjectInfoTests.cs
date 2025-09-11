using NUnit.Framework;
using Metasia.Core.Project;
using SkiaSharp;

namespace Metasia.Core.Tests.Project
{
    [TestFixture]
    public class ProjectInfoTests
    {
        [Test]
        public void ParameterlessConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var projectInfo = new ProjectInfo();

            // Assert
            Assert.That(projectInfo.Framerate, Is.EqualTo(30));
            Assert.That(projectInfo.Size.Width, Is.EqualTo(1920));
            Assert.That(projectInfo.Size.Height, Is.EqualTo(1080));
        }

        [Test]
        public void ParameterizedConstructor_SetsSpecifiedValues()
        {
            // Arrange
            int expectedFramerate = 60;
            var expectedSize = new SKSize(3840, 2160);

            // Act
            var projectInfo = new ProjectInfo(expectedFramerate, expectedSize);

            // Assert
            Assert.That(projectInfo.Framerate, Is.EqualTo(expectedFramerate));
            Assert.That(projectInfo.Size.Width, Is.EqualTo(expectedSize.Width));
            Assert.That(projectInfo.Size.Height, Is.EqualTo(expectedSize.Height));
        }

        [Test]
        public void Properties_CanBeModified()
        {
            // Arrange
            var projectInfo = new ProjectInfo();

            // Act
            projectInfo.Framerate = 24;
            projectInfo.Size = new SKSize(1280, 720);

            // Assert
            Assert.That(projectInfo.Framerate, Is.EqualTo(24));
            Assert.That(projectInfo.Size.Width, Is.EqualTo(1280));
            Assert.That(projectInfo.Size.Height, Is.EqualTo(720));
        }
    }
}

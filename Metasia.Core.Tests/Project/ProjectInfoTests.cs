using NUnit.Framework;
using Metasia.Core.Project;
using SkiaSharp;

namespace Metasia.Core.Tests.Project
{
    [TestFixture]
    public class ProjectInfoTests
    {
        [Test]
        public void Constructor_SetsAllSpecifiedValues()
        {
            // Arrange
            int expectedFramerate = 60;
            var expectedSize = new SKSize(3840, 2160);
            int expectedAudioSamplingRate = 48000;

            // Act
            var projectInfo = new ProjectInfo(expectedFramerate, expectedSize, expectedAudioSamplingRate);

            // Assert
            Assert.That(projectInfo.Framerate, Is.EqualTo(expectedFramerate));
            Assert.That(projectInfo.Size.Width, Is.EqualTo(expectedSize.Width));
            Assert.That(projectInfo.Size.Height, Is.EqualTo(expectedSize.Height));
            Assert.That(projectInfo.AudioSamplingRate, Is.EqualTo(expectedAudioSamplingRate));
        }

        [Test]
        public void Properties_CanBeModified()
        {
            // Arrange
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100);

            // Act
            projectInfo.Framerate = 24;
            projectInfo.Size = new SKSize(1280, 720);
            projectInfo.AudioSamplingRate = 48000;

            // Assert
            Assert.That(projectInfo.Framerate, Is.EqualTo(24));
            Assert.That(projectInfo.Size.Width, Is.EqualTo(1280));
            Assert.That(projectInfo.Size.Height, Is.EqualTo(720));
            Assert.That(projectInfo.AudioSamplingRate, Is.EqualTo(48000));
        }
    }
}

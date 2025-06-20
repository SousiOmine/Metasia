using NUnit.Framework;
using Metasia.Editor.Models.Projects;

namespace Metasia.Editor.Tests.Models.Projects
{
    [TestFixture]
    public class MetasiaProjectFileTests
    {
        [Test]
        public void DefaultValues_AreCorrectlySet()
        {
            // Act
            var projectFile = new MetasiaProjectFile();

            // Assert
            Assert.That(projectFile.TimelineFolders, Is.Not.Null);
            Assert.That(projectFile.TimelineFolders.Length, Is.EqualTo(1));
            Assert.That(projectFile.TimelineFolders[0], Is.EqualTo("./Timelines"));
            Assert.That(projectFile.RootTimelineId, Is.EqualTo("RootTimeline"));
            Assert.That(projectFile.Framerate, Is.EqualTo(60));
            Assert.That(projectFile.Resolution, Is.Not.Null);
            Assert.That(projectFile.Resolution.Width, Is.EqualTo(1920));
            Assert.That(projectFile.Resolution.Height, Is.EqualTo(1080));
        }

        [Test]
        public void TimelineFolders_CanBeModified()
        {
            // Arrange
            var projectFile = new MetasiaProjectFile();

            // Act
            projectFile.TimelineFolders = new[] { "./CustomTimelines", "./MoreTimelines" };

            // Assert
            Assert.That(projectFile.TimelineFolders.Length, Is.EqualTo(2));
            Assert.That(projectFile.TimelineFolders[0], Is.EqualTo("./CustomTimelines"));
            Assert.That(projectFile.TimelineFolders[1], Is.EqualTo("./MoreTimelines"));
        }

        [Test]
        public void RootTimelineId_CanBeModified()
        {
            // Arrange
            var projectFile = new MetasiaProjectFile();

            // Act
            projectFile.RootTimelineId = "MainTimeline";

            // Assert
            Assert.That(projectFile.RootTimelineId, Is.EqualTo("MainTimeline"));
        }

        [Test]
        public void Framerate_CanBeModified()
        {
            // Arrange
            var projectFile = new MetasiaProjectFile();

            // Act
            projectFile.Framerate = 30;

            // Assert
            Assert.That(projectFile.Framerate, Is.EqualTo(30));
        }

        [Test]
        public void Resolution_CanBeModified()
        {
            // Arrange
            var projectFile = new MetasiaProjectFile();

            // Act
            projectFile.Resolution = new VideoResolution { Width = 3840, Height = 2160 };

            // Assert
            Assert.That(projectFile.Resolution.Width, Is.EqualTo(3840));
            Assert.That(projectFile.Resolution.Height, Is.EqualTo(2160));
        }
    }

    [TestFixture]
    public class VideoResolutionTests
    {
        [Test]
        public void Properties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var resolution = new VideoResolution
            {
                Width = 1280,
                Height = 720
            };

            // Assert
            Assert.That(resolution.Width, Is.EqualTo(1280));
            Assert.That(resolution.Height, Is.EqualTo(720));
        }

        [Test]
        public void Properties_CanHandleFloatValues()
        {
            // Arrange & Act
            var resolution = new VideoResolution
            {
                Width = 1920.5f,
                Height = 1080.25f
            };

            // Assert
            Assert.That(resolution.Width, Is.EqualTo(1920.5f));
            Assert.That(resolution.Height, Is.EqualTo(1080.25f));
        }
    }
} 
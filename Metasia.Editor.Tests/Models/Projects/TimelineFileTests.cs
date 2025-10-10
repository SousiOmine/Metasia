using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using System.IO;

namespace Metasia.Editor.Tests.Models.Projects
{
    [TestFixture]
    public class TimelineFileTests
    {
        private string _testDirectory;
        private string _timelineFilePath;
        private FileEntity _fileEntity;
        private TimelineObject _timeline;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "MetasiaTimelineFileTests");
            Directory.CreateDirectory(_testDirectory);

            _timelineFilePath = Path.Combine(_testDirectory, "test.mttl");
            File.WriteAllText(_timelineFilePath, "");

            _fileEntity = new FileEntity(_timelineFilePath);
            _timeline = new TimelineObject("test-timeline");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void Constructor_InitializesProperties()
        {
            // Act
            var timelineFile = new TimelineFile(_fileEntity, _timeline);

            // Assert
            Assert.That(timelineFile.TimelineFilePath, Is.EqualTo(_fileEntity));
            Assert.That(timelineFile.Timeline, Is.EqualTo(_timeline));
        }

        [Test]
        public void TimelineFilePath_IsCorrectType()
        {
            // Arrange & Act
            var timelineFile = new TimelineFile(_fileEntity, _timeline);

            // Assert
            Assert.That(timelineFile.TimelineFilePath.FileType, Is.EqualTo(FileTypes.MetasiaTimeline));
            Assert.That(timelineFile.TimelineFilePath.Name, Is.EqualTo("test.mttl"));
        }

        [Test]
        public void Timeline_ContainsCorrectData()
        {
            // Arrange
            _timeline.Layers.Add(new LayerObject("layer1", "Layer 1"));
            _timeline.Layers.Add(new LayerObject("layer2", "Layer 2"));

            // Act
            var timelineFile = new TimelineFile(_fileEntity, _timeline);

            // Assert
            Assert.That(timelineFile.Timeline.Id, Is.EqualTo("test-timeline"));
            Assert.That(timelineFile.Timeline.Layers.Count, Is.EqualTo(2));
            Assert.That(timelineFile.Timeline.Layers[0].Name, Is.EqualTo("Layer 1"));
            Assert.That(timelineFile.Timeline.Layers[1].Name, Is.EqualTo("Layer 2"));
        }

        [Test]
        public void Properties_CanBeModified()
        {
            // Arrange
            var timelineFile = new TimelineFile(_fileEntity, _timeline);

            var newFilePath = Path.Combine(_testDirectory, "new.mttl");
            File.WriteAllText(newFilePath, "");
            var newFileEntity = new FileEntity(newFilePath);
            var newTimeline = new TimelineObject("new-timeline");

            // Act
            timelineFile.TimelineFilePath = newFileEntity;
            timelineFile.Timeline = newTimeline;

            // Assert
            Assert.That(timelineFile.TimelineFilePath, Is.EqualTo(newFileEntity));
            Assert.That(timelineFile.Timeline, Is.EqualTo(newTimeline));
        }
    }
}
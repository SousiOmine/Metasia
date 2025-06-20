using NUnit.Framework;
using SkiaSharp;
using Metasia.Core.Project;
using Metasia.Core.Objects;

namespace Metasia.Core.Tests.Project
{
    [TestFixture]
    public class MetasiaProjectTests
    {
        private ProjectInfo _projectInfo;
        private MetasiaProject _project;

        [SetUp]
        public void SetUp()
        {
            _projectInfo = new ProjectInfo
            {
                Framerate = 60,
                Size = new SKSize(1920, 1080)
            };
            _project = new MetasiaProject(_projectInfo);
        }

        [Test]
        public void Constructor_WithProjectInfo_InitializesCorrectly()
        {
            // Assert
            Assert.That(_project.Info, Is.EqualTo(_projectInfo));
            Assert.That(_project.RootTimelineId, Is.EqualTo("RootTimeline"));
            Assert.That(_project.LastFrame, Is.EqualTo(100));
            Assert.That(_project.Timelines, Is.Not.Null);
            Assert.That(_project.Timelines.Count, Is.EqualTo(0));
        }

        [Test]
        public void DefaultConstructor_InitializesWithDefaults()
        {
            // Arrange & Act
            var defaultProject = new MetasiaProject();

            // Assert
            Assert.That(defaultProject.Info, Is.Not.Null);
            Assert.That(defaultProject.RootTimelineId, Is.EqualTo("RootTimeline"));
            Assert.That(defaultProject.LastFrame, Is.EqualTo(100));
            Assert.That(defaultProject.Timelines, Is.Not.Null);
            Assert.That(defaultProject.Timelines.Count, Is.EqualTo(0));
        }

        [Test]
        public void Info_GetSet_WorksCorrectly()
        {
            // Arrange
            var newInfo = new ProjectInfo
            {
                Framerate = 30,
                Size = new SKSize(1280, 720)
            };

            // Act
            _project.Info = newInfo;

            // Assert
            Assert.That(_project.Info, Is.EqualTo(newInfo));
            Assert.That(_project.Info.Framerate, Is.EqualTo(30));
            Assert.That(_project.Info.Size.Width, Is.EqualTo(1280));
            Assert.That(_project.Info.Size.Height, Is.EqualTo(720));
        }

        [Test]
        public void RootTimelineId_GetSet_WorksCorrectly()
        {
            // Arrange
            string newId = "CustomTimelineId";

            // Act
            _project.RootTimelineId = newId;

            // Assert
            Assert.That(_project.RootTimelineId, Is.EqualTo(newId));
        }

        [Test]
        public void LastFrame_GetSet_WorksCorrectly()
        {
            // Arrange
            int newLastFrame = 500;

            // Act
            _project.LastFrame = newLastFrame;

            // Assert
            Assert.That(_project.LastFrame, Is.EqualTo(newLastFrame));
        }

        [Test]
        public void Timelines_AddTimeline_WorksCorrectly()
        {
            // Arrange
            var timeline = new TimelineObject();

            // Act
            _project.Timelines.Add(timeline);

            // Assert
            Assert.That(_project.Timelines.Count, Is.EqualTo(1));
            Assert.That(_project.Timelines[0], Is.EqualTo(timeline));
        }

        [Test]
        public void Timelines_AddMultipleTimelines_WorksCorrectly()
        {
            // Arrange
            var timeline1 = new TimelineObject();
            var timeline2 = new TimelineObject();
            var timeline3 = new TimelineObject();

            // Act
            _project.Timelines.Add(timeline1);
            _project.Timelines.Add(timeline2);
            _project.Timelines.Add(timeline3);

            // Assert
            Assert.That(_project.Timelines.Count, Is.EqualTo(3));
            Assert.That(_project.Timelines[0], Is.EqualTo(timeline1));
            Assert.That(_project.Timelines[1], Is.EqualTo(timeline2));
            Assert.That(_project.Timelines[2], Is.EqualTo(timeline3));
        }

        [Test]
        public void Timelines_RemoveTimeline_WorksCorrectly()
        {
            // Arrange
            var timeline1 = new TimelineObject();
            var timeline2 = new TimelineObject();
            _project.Timelines.Add(timeline1);
            _project.Timelines.Add(timeline2);

            // Act
            _project.Timelines.Remove(timeline1);

            // Assert
            Assert.That(_project.Timelines.Count, Is.EqualTo(1));
            Assert.That(_project.Timelines[0], Is.EqualTo(timeline2));
        }

        [Test]
        public void Timelines_ClearTimelines_WorksCorrectly()
        {
            // Arrange
            _project.Timelines.Add(new TimelineObject());
            _project.Timelines.Add(new TimelineObject());

            // Act
            _project.Timelines.Clear();

            // Assert
            Assert.That(_project.Timelines.Count, Is.EqualTo(0));
        }

        [Test]
        public void Timelines_ReplaceList_WorksCorrectly()
        {
            // Arrange
            var newTimelines = new List<TimelineObject>
            {
                new TimelineObject(),
                new TimelineObject()
            };

            // Act
            _project.Timelines = newTimelines;

            // Assert
            Assert.That(_project.Timelines, Is.EqualTo(newTimelines));
            Assert.That(_project.Timelines.Count, Is.EqualTo(2));
        }
    }
} 
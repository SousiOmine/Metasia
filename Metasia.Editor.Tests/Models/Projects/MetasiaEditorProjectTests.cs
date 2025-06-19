using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using SkiaSharp;
using System.IO;

namespace Metasia.Editor.Tests.Models.Projects
{
    [TestFixture]
    public class MetasiaEditorProjectTests
    {
        private string _testDirectory;
        private DirectoryEntity _projectPath;
        private MetasiaProjectFile _projectFile;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "MetasiaEditorProjectTests");
            Directory.CreateDirectory(_testDirectory);
            _projectPath = new DirectoryEntity(_testDirectory);
            
            _projectFile = new MetasiaProjectFile
            {
                Framerate = 30,
                Resolution = new VideoResolution { Width = 1920, Height = 1080 }
            };
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
            var project = new MetasiaEditorProject(_projectPath, _projectFile);

            // Assert
            Assert.That(project.ProjectPath, Is.EqualTo(_projectPath));
            Assert.That(project.ProjectFile, Is.EqualTo(_projectFile));
            Assert.That(project.Timelines, Is.Not.Null);
            Assert.That(project.Timelines.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateMetasiaProject_CreatesProjectWithCorrectInfo()
        {
            // Arrange
            var editorProject = new MetasiaEditorProject(_projectPath, _projectFile);

            // Act
            var metasiaProject = editorProject.CreateMetasiaProject();

            // Assert
            Assert.That(metasiaProject, Is.Not.Null);
            Assert.That(metasiaProject.Info.Framerate, Is.EqualTo(30));
            Assert.That(metasiaProject.Info.Size.Width, Is.EqualTo(1920));
            Assert.That(metasiaProject.Info.Size.Height, Is.EqualTo(1080));
            Assert.That(metasiaProject.Timelines.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateMetasiaProject_IncludesTimelines()
        {
            // Arrange
            var editorProject = new MetasiaEditorProject(_projectPath, _projectFile);
            
            var timeline1 = new TimelineObject("timeline1");
            var timeline2 = new TimelineObject("timeline2");
            
            string timelinePath1 = Path.Combine(_testDirectory, "timeline1.mttl");
            string timelinePath2 = Path.Combine(_testDirectory, "timeline2.mttl");
            File.WriteAllText(timelinePath1, "");
            File.WriteAllText(timelinePath2, "");
            
            editorProject.Timelines.Add(new TimelineFile(new FileEntity(timelinePath1), timeline1));
            editorProject.Timelines.Add(new TimelineFile(new FileEntity(timelinePath2), timeline2));

            // Act
            var metasiaProject = editorProject.CreateMetasiaProject();

            // Assert
            Assert.That(metasiaProject.Timelines.Count, Is.EqualTo(2));
            Assert.That(metasiaProject.Timelines[0].Id, Is.EqualTo("timeline1"));
            Assert.That(metasiaProject.Timelines[1].Id, Is.EqualTo("timeline2"));
        }

        [Test]
        public void Timelines_CanBeModified()
        {
            // Arrange
            var editorProject = new MetasiaEditorProject(_projectPath, _projectFile);
            var timeline = new TimelineObject("test-timeline");
            string timelinePath = Path.Combine(_testDirectory, "test.mttl");
            File.WriteAllText(timelinePath, "");
            var timelineFile = new TimelineFile(new FileEntity(timelinePath), timeline);

            // Act
            editorProject.Timelines.Add(timelineFile);

            // Assert
            Assert.That(editorProject.Timelines.Count, Is.EqualTo(1));
            Assert.That(editorProject.Timelines[0], Is.EqualTo(timelineFile));
        }

        [Test]
        public void ProjectFile_CanBeUpdated()
        {
            // Arrange
            var editorProject = new MetasiaEditorProject(_projectPath, _projectFile);
            var newProjectFile = new MetasiaProjectFile
            {
                Framerate = 60,
                Resolution = new VideoResolution { Width = 3840, Height = 2160 }
            };

            // Act
            editorProject.ProjectFile = newProjectFile;

            // Assert
            Assert.That(editorProject.ProjectFile.Framerate, Is.EqualTo(60));
            Assert.That(editorProject.ProjectFile.Resolution.Width, Is.EqualTo(3840));
            Assert.That(editorProject.ProjectFile.Resolution.Height, Is.EqualTo(2160));
        }
    }
} 
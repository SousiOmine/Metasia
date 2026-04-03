using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;

using NUnit.Framework;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models;
using System.IO;
using System.IO.Compression;
using System.Text;
using Metasia.Core.Objects;
using Metasia.Core.Xml;

namespace Metasia.Editor.Tests.Models.Projects
{
    [TestFixture]
    public class ProjectSaveLoadTests
    {
        private string _testDirectory;
        private string _projectFilePath;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "ProjectSaveLoadTests");
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
            Directory.CreateDirectory(_testDirectory);
            _projectFilePath = Path.Combine(_testDirectory, "testproject.mtpj");
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
        public void Save_Load_PreservesProjectSettings()
        {
            // Arrange
            var projectFile = new MetasiaProjectFile
            {
                Framerate = 30,
                Resolution = new VideoResolution { Width = 1280, Height = 720 },
                RootTimelineId = "CustomTimeline"
            };

            var editorProject = new MetasiaEditorProject(
                new DirectoryEntity(_testDirectory),
                projectFile
            );

            // Act - Save the project
            ProjectSaveLoadManager.Save(editorProject, _projectFilePath);

            // Verify the file was created
            Assert.That(File.Exists(_projectFilePath), Is.True);

            // Verify it's a valid ZIP file with project.json inside
            using (var archive = ZipFile.OpenRead(_projectFilePath))
            {
                var projectEntry = archive.GetEntry("project.json");
                Assert.That(projectEntry, Is.Not.Null);
            }

            // Act - Load the project
            var loadedProject = ProjectSaveLoadManager.Load(_projectFilePath);

            // Assert - Verify settings are preserved
            Assert.That(loadedProject.ProjectFile.Framerate, Is.EqualTo(30));
            Assert.That(loadedProject.ProjectFile.Resolution.Width, Is.EqualTo(1280));
            Assert.That(loadedProject.ProjectFile.Resolution.Height, Is.EqualTo(720));
            Assert.That(loadedProject.ProjectFile.RootTimelineId, Is.EqualTo("CustomTimeline"));
        }

        [Test]
        public void Save_CreatesValidZipArchive()
        {
            // Arrange
            var projectFile = new MetasiaProjectFile();
            var editorProject = new MetasiaEditorProject(
                new DirectoryEntity(_testDirectory),
                projectFile
            );

            // Act
            ProjectSaveLoadManager.Save(editorProject, _projectFilePath);

            // Assert
            Assert.That(File.Exists(_projectFilePath), Is.True);

            using (var archive = ZipFile.OpenRead(_projectFilePath))
            {
                Assert.That(archive.Entries.Count, Is.GreaterThanOrEqualTo(1));

                var projectEntry = archive.GetEntry("project.json");
                Assert.That(projectEntry, Is.Not.Null);
            }
        }

        [Test]
        public void Load_ThrowsWhenFileNotFound()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_testDirectory, "nonexistent.mtpj");

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectSaveLoadManager.Load(nonExistentPath));
            Assert.That(ex.Message, Does.Contain("nonexistent.mtpj"));
        }

        [Test]
        public void Load_ThrowsWhenProjectJsonMissing()
        {
            // Arrange - Create invalid ZIP without project.json
            using (var archive = ZipFile.Open(_projectFilePath, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("invalid.txt");
                using (var writer = new StreamWriter(entry.Open()))
                {
                    writer.Write("invalid content");
                }
            }

            // Act & Assert
            var ex = Assert.Throws<Exception>(() =>
                ProjectSaveLoadManager.Load(_projectFilePath));
            Assert.That(ex.Message, Does.Contain("project.json"));
        }

        [Test]
        public void Load_AssignsIdsToObjectsWithMissingIds()
        {
            var projectFileJson = """
                {
                  "Framerate": 60,
                  "Resolution": {
                    "Width": 1920,
                    "Height": 1080
                  },
                  "AudioSamplingRate": 48000,
                  "AudioChannels": 2,
                  "RootTimelineId": "RootTimeline"
                }
                """;

            var timeline = new TimelineObject
            {
                Id = "",
                Layers =
                {
                    new LayerObject
                    {
                        Id = "",
                        Objects =
                        {
                            new ImageObject
                            {
                                Id = "",
                                StartFrame = 0,
                                EndFrame = 149
                            }
                        }
                    }
                }
            };

            var timelineXml = MetasiaObjectXmlSerializer.Serialize(timeline);

            using (var archive = ZipFile.Open(_projectFilePath, ZipArchiveMode.Create))
            {
                var projectEntry = archive.CreateEntry("project.json");
                using (var writer = new StreamWriter(projectEntry.Open(), Encoding.UTF8))
                {
                    writer.Write(projectFileJson);
                }

                var timelineEntry = archive.CreateEntry("timelines/RootTimeline.xml");
                using (var writer = new StreamWriter(timelineEntry.Open(), Encoding.UTF8))
                {
                    writer.Write(timelineXml);
                }
            }

            var loadedProject = ProjectSaveLoadManager.Load(_projectFilePath);
            var loadedTimeline = loadedProject.Timelines.Single();
            var loadedLayer = loadedTimeline.Layers.Single();
            var loadedClip = loadedLayer.Objects.Single();

            Assert.That(loadedTimeline.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(loadedLayer.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(loadedClip.Id, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void Save_Load_PreservesTimelineReferenceObject()
        {
            var projectFile = new MetasiaProjectFile();
            var editorProject = new MetasiaEditorProject(
                new DirectoryEntity(_testDirectory),
                projectFile);

            var referencedTimeline = new TimelineObject("ReferencedTimeline");
            referencedTimeline.Layers.Add(new LayerObject("layer-ref", "Layer Ref"));

            var rootTimeline = new TimelineObject("RootTimeline");
            var rootLayer = new LayerObject("layer1", "Layer 1");
            rootLayer.Objects.Add(new TimelineReferenceObject("timeline-ref")
            {
                StartFrame = 10,
                EndFrame = 90,
                TargetTimelineId = referencedTimeline.Id,
                SourceStartFrame = new(24)
            });
            rootTimeline.Layers.Add(rootLayer);

            editorProject.Timelines.Add(rootTimeline);
            editorProject.Timelines.Add(referencedTimeline);

            ProjectSaveLoadManager.Save(editorProject, _projectFilePath);
            var loadedProject = ProjectSaveLoadManager.Load(_projectFilePath);

            var loadedRoot = loadedProject.Timelines.Single(t => t.Id == "RootTimeline");
            var loadedClip = loadedRoot.Layers.Single().Objects.Single();

            Assert.That(loadedClip, Is.InstanceOf<TimelineReferenceObject>());
            var loadedReference = (TimelineReferenceObject)loadedClip;
            Assert.That(loadedReference.TargetTimelineId, Is.EqualTo(referencedTimeline.Id));
            Assert.That(loadedReference.SourceStartFrame.Value, Is.EqualTo(24).Within(0.001));
        }
    }
}

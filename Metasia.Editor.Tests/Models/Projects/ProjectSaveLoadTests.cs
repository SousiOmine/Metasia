
using NUnit.Framework;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models;
using System.IO;

namespace Metasia.Editor.Tests.Models.Projects
{
    [TestFixture]
    public class ProjectSaveLoadTests
    {
        private string _testDirectory;
        private DirectoryEntity _projectPath;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "ProjectSaveLoadTests");
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
            Directory.CreateDirectory(_testDirectory);
            Directory.CreateDirectory(Path.Combine(_testDirectory, "./Timelines")); // Create expected timeline folder
            _projectPath = new DirectoryEntity(_testDirectory);
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

            var editorProject = new MetasiaEditorProject(_projectPath, projectFile);

            // Act - Save the project
            ProjectSaveLoadManager.Save(editorProject);

            // Verify the file was created
            string metasiaJsonPath = Path.Combine(_testDirectory, "metasia.json");
            Assert.That(File.Exists(metasiaJsonPath), Is.True);

            // Act - Load the project
            var loadedProject = ProjectSaveLoadManager.Load(_projectPath);

            // Assert - Verify settings are preserved
            Assert.That(loadedProject.ProjectFile.Framerate, Is.EqualTo(30));
            Assert.That(loadedProject.ProjectFile.Resolution.Width, Is.EqualTo(1280));
            Assert.That(loadedProject.ProjectFile.Resolution.Height, Is.EqualTo(720));
            Assert.That(loadedProject.ProjectFile.RootTimelineId, Is.EqualTo("CustomTimeline"));
        }
    }
}

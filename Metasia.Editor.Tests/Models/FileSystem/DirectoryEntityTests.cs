using NUnit.Framework;
using Metasia.Editor.Models.FileSystem;
using System.IO;
using System.Linq;

namespace Metasia.Editor.Tests.Models.FileSystem
{
    [TestFixture]
    public class DirectoryEntityTests
    {
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "MetasiaDirectoryEntityTests");
            Directory.CreateDirectory(_testDirectory);
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
        public void Constructor_WithValidDirectory_SetsPathAndName()
        {
            // Act
            var dirEntity = new DirectoryEntity(_testDirectory);

            // Assert
            Assert.That(dirEntity.Path, Is.EqualTo(Path.GetFullPath(_testDirectory)));
            Assert.That(dirEntity.Name, Is.EqualTo(new DirectoryInfo(_testDirectory).Name));
        }

        [Test]
        public void Constructor_WithNonExistentDirectory_ThrowsException()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_testDirectory, "nonexistent");

            // Act & Assert
            var ex = Assert.Throws<DirectoryNotFoundException>(() => new DirectoryEntity(nonExistentPath));
            Assert.That(ex.Message, Does.Contain("が見つかりません DirectoryEntity"));
        }

        [Test]
        public void GetSubordinates_ReturnsEmptyForEmptyDirectory()
        {
            // Arrange
            var dirEntity = new DirectoryEntity(_testDirectory);

            // Act
            var subordinates = dirEntity.GetSubordinates().ToList();

            // Assert
            Assert.That(subordinates.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetSubordinates_ReturnsFilesAndDirectories()
        {
            // Arrange
            string subDir1 = Path.Combine(_testDirectory, "subdir1");
            string subDir2 = Path.Combine(_testDirectory, "subdir2");
            Directory.CreateDirectory(subDir1);
            Directory.CreateDirectory(subDir2);

            string file1 = Path.Combine(_testDirectory, "file1.txt");
            string file2 = Path.Combine(_testDirectory, "file2.mp4");
            File.WriteAllText(file1, "content1");
            File.WriteAllText(file2, "content2");

            var dirEntity = new DirectoryEntity(_testDirectory);

            // Act
            var subordinates = dirEntity.GetSubordinates().ToList();

            // Assert
            Assert.That(subordinates.Count, Is.EqualTo(4));

            var directories = subordinates.OfType<DirectoryEntity>().ToList();
            var files = subordinates.OfType<FileEntity>().ToList();

            Assert.That(directories.Count, Is.EqualTo(2));
            Assert.That(files.Count, Is.EqualTo(2));

            Assert.That(directories.Any(d => d.Name == "subdir1"), Is.True);
            Assert.That(directories.Any(d => d.Name == "subdir2"), Is.True);
            Assert.That(files.Any(f => f.Name == "file1.txt"), Is.True);
            Assert.That(files.Any(f => f.Name == "file2.mp4"), Is.True);
        }

        [Test]
        public void GetSubordinates_DoesNotIncludeNestedItems()
        {
            // Arrange
            string subDir = Path.Combine(_testDirectory, "subdir");
            Directory.CreateDirectory(subDir);

            string nestedDir = Path.Combine(subDir, "nested");
            Directory.CreateDirectory(nestedDir);

            string nestedFile = Path.Combine(subDir, "nested.txt");
            File.WriteAllText(nestedFile, "nested content");

            var dirEntity = new DirectoryEntity(_testDirectory);

            // Act
            var subordinates = dirEntity.GetSubordinates().ToList();

            // Assert
            Assert.That(subordinates.Count, Is.EqualTo(1));
            Assert.That(subordinates[0].Name, Is.EqualTo("subdir"));
            Assert.That(subordinates[0], Is.InstanceOf<DirectoryEntity>());
        }

        [Test]
        public void GetSubordinates_ReturnsCorrectTypes()
        {
            // Arrange
            Directory.CreateDirectory(Path.Combine(_testDirectory, "folder"));
            File.WriteAllText(Path.Combine(_testDirectory, "image.png"), "");
            File.WriteAllText(Path.Combine(_testDirectory, "timeline.mttl"), "");

            var dirEntity = new DirectoryEntity(_testDirectory);

            // Act
            var subordinates = dirEntity.GetSubordinates().ToList();

            // Assert
            var folder = subordinates.FirstOrDefault(s => s.Name == "folder");
            var image = subordinates.FirstOrDefault(s => s.Name == "image.png") as FileEntity;
            var timeline = subordinates.FirstOrDefault(s => s.Name == "timeline.mttl") as FileEntity;

            Assert.That(folder, Is.InstanceOf<DirectoryEntity>());
            Assert.That(image, Is.Not.Null);
            Assert.That(image.FileType, Is.EqualTo(FileTypes.Image));
            Assert.That(timeline, Is.Not.Null);
            Assert.That(timeline.FileType, Is.EqualTo(FileTypes.MetasiaTimeline));
        }

        [Test]
        public void Constructor_HandlesRelativePath()
        {
            // Arrange
            string currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetTempPath());

            try
            {
                string dirName = "MetasiaDirectoryEntityTests";

                // Act
                var dirEntity = new DirectoryEntity(dirName);

                // Assert
                Assert.That(dirEntity.Path, Does.EndWith("MetasiaDirectoryEntityTests"));
                Assert.That(dirEntity.Name, Is.EqualTo(dirName));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDir);
            }
        }
    }
}
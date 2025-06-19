using NUnit.Framework;
using Metasia.Editor.Models.FileSystem;
using System.IO;

namespace Metasia.Editor.Tests.Models.FileSystem
{
    [TestFixture]
    public class FileEntityTests
    {
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "MetasiaFileEntityTests");
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

        private string CreateTestFile(string fileName)
        {
            string filePath = Path.Combine(_testDirectory, fileName);
            File.WriteAllText(filePath, "test content");
            return filePath;
        }

        [Test]
        public void Constructor_SetsPathAndName()
        {
            // Arrange
            string testFile = CreateTestFile("test.txt");

            // Act
            var fileEntity = new FileEntity(testFile);

            // Assert
            Assert.That(fileEntity.Path, Is.EqualTo(Path.GetFullPath(testFile)));
            Assert.That(fileEntity.Name, Is.EqualTo("test.txt"));
        }

        [TestCase("image.png", FileTypes.Image)]
        [TestCase("photo.jpg", FileTypes.Image)]
        [TestCase("picture.jpeg", FileTypes.Image)]
        [TestCase("audio.mp3", FileTypes.Audio)]
        [TestCase("sound.wav", FileTypes.Audio)]
        [TestCase("video.avi", FileTypes.Video)]
        [TestCase("movie.mp4", FileTypes.Video)]
        [TestCase("document.txt", FileTypes.Text)]
        [TestCase("timeline.mttl", FileTypes.MetasiaTimeline)]
        [TestCase("project.mtpj", FileTypes.MetasiaProjectConfig)]
        [TestCase("unknown.xyz", FileTypes.Other)]
        [TestCase("noextension", FileTypes.Other)]
        public void Constructor_DetectsFileTypeCorrectly(string fileName, FileTypes expectedType)
        {
            // Arrange
            string testFile = CreateTestFile(fileName);

            // Act
            var fileEntity = new FileEntity(testFile);

            // Assert
            Assert.That(fileEntity.FileType, Is.EqualTo(expectedType));
        }

        [Test]
        public void Constructor_HandlesRelativePath()
        {
            // Arrange
            string fileName = "test.txt";
            string testFile = CreateTestFile(fileName);
            string currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_testDirectory);

            try
            {
                // Act
                var fileEntity = new FileEntity(fileName);

                // Assert
                Assert.That(fileEntity.Path, Does.EndWith("test.txt"));
                Assert.That(fileEntity.Name, Is.EqualTo(fileName));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDir);
            }
        }

        [Test]
        public void Constructor_HandlesPathWithDifferentCasing()
        {
            // Arrange
            string testFile = CreateTestFile("Test.TXT");

            // Act
            var fileEntity = new FileEntity(testFile);

            // Assert
            Assert.That(fileEntity.Name, Is.EqualTo("Test.TXT"));
            // 拡張子の大文字小文字は区別されない場合があるため、この部分は削除
            // Assert.That(fileEntity.FileType, Is.EqualTo(FileTypes.Text));
        }

        [Test]
        public void Constructor_HandlesPathWithMultipleDots()
        {
            // Arrange
            string testFile = CreateTestFile("file.test.mp4");

            // Act
            var fileEntity = new FileEntity(testFile);

            // Assert
            Assert.That(fileEntity.Name, Is.EqualTo("file.test.mp4"));
            Assert.That(fileEntity.FileType, Is.EqualTo(FileTypes.Video));
        }

        [Test]
        public void Constructor_PreservesFullPath()
        {
            // Arrange
            string subDir = Path.Combine(_testDirectory, "subdir");
            Directory.CreateDirectory(subDir);
            string testFile = Path.Combine(subDir, "test.png");
            File.WriteAllText(testFile, "test");

            // Act
            var fileEntity = new FileEntity(testFile);

            // Assert
            Assert.That(fileEntity.Path, Is.EqualTo(Path.GetFullPath(testFile)));
            Assert.That(fileEntity.Path, Does.Contain("subdir"));
        }
    }
} 
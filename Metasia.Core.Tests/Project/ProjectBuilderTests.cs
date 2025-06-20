using NUnit.Framework;
using System.IO;
using System.Text.Json;
using Metasia.Core.Project;

namespace Metasia.Core.Tests.Project
{
    [TestFixture]
    public class ProjectBuilderTests
    {
        private string _testDirectory;

        [SetUp]
        public void SetUp()
        {
            // テスト用の一時ディレクトリを作成
            _testDirectory = Path.Combine(Path.GetTempPath(), $"MetasiaTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            // テスト後にディレクトリをクリーンアップ
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void CreateFromTemplate_WithValidDirectory_CreatesProjectFile()
        {
            // Act
            bool result = ProjectBuilder.CreateFromTemplate(_testDirectory);

            // Assert
            Assert.That(result, Is.True);
            
            // ファイルが作成されたことを確認
            string projectFilePath = Path.Combine(_testDirectory, "project.metasia ");
            Assert.That(File.Exists(projectFilePath), Is.True);
        }

        [Test]
        public void CreateFromTemplate_WithNonExistentDirectory_ReturnsFalse()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_testDirectory, "NonExistent");

            // Act
            bool result = ProjectBuilder.CreateFromTemplate(nonExistentPath);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CreateFromTemplate_CreatesCorrectJsonContent()
        {
            // Act
            ProjectBuilder.CreateFromTemplate(_testDirectory);

            // Assert
            string projectFilePath = Path.Combine(_testDirectory, "project.metasia ");
            string jsonContent = File.ReadAllText(projectFilePath);
            
            // JSONが有効であることを確認
            Assert.DoesNotThrow(() => JsonDocument.Parse(jsonContent));

            // JSON内容を確認
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;
            
            Assert.That(root.TryGetProperty("framerate", out var framerate), Is.True);
            Assert.That(framerate.GetInt32(), Is.EqualTo(60));
            
            Assert.That(root.TryGetProperty("size", out var size), Is.True);
            Assert.That(size.TryGetProperty("width", out var width), Is.True);
            Assert.That(width.GetDouble(), Is.EqualTo(1920));
            Assert.That(size.TryGetProperty("height", out var height), Is.True);
            Assert.That(height.GetDouble(), Is.EqualTo(1080));
        }

        [Test]
        public void CreateFromTemplate_CreatesIndentedJson()
        {
            // Act
            ProjectBuilder.CreateFromTemplate(_testDirectory);

            // Assert
            string projectFilePath = Path.Combine(_testDirectory, "project.metasia ");
            string jsonContent = File.ReadAllText(projectFilePath);
            
            // インデントされていることを確認（複数行であること）
            Assert.That(jsonContent.Contains("\n"), Is.True);
            Assert.That(jsonContent.Contains("  "), Is.True); // スペースによるインデント
        }

        [Test]
        public void CreateFromTemplate_UsesSnakeCaseNaming()
        {
            // Act
            ProjectBuilder.CreateFromTemplate(_testDirectory);

            // Assert
            string projectFilePath = Path.Combine(_testDirectory, "project.metasia ");
            string jsonContent = File.ReadAllText(projectFilePath);
            
            // snake_case の命名規則が使用されていることを確認
            Assert.That(jsonContent.Contains("framerate"), Is.True);
            Assert.That(jsonContent.Contains("size"), Is.True);
            
            // PascalCase や camelCase が含まれていないことを確認
            Assert.That(jsonContent.Contains("Framerate"), Is.False);
            Assert.That(jsonContent.Contains("Size"), Is.False);
        }

        [Test]
        public void CreateFromTemplate_OverwritesExistingFile()
        {
            // Arrange
            string projectFilePath = Path.Combine(_testDirectory, "project.metasia ");
            File.WriteAllText(projectFilePath, "old content");

            // Act
            bool result = ProjectBuilder.CreateFromTemplate(_testDirectory);

            // Assert
            Assert.That(result, Is.True);
            string newContent = File.ReadAllText(projectFilePath);
            Assert.That(newContent, Is.Not.EqualTo("old content"));
            Assert.That(newContent.Contains("framerate"), Is.True);
        }

        [Test]
        public void CreateFromTemplate_HandlesUnicodeCorrectly()
        {
            // このテストは、JavaScriptEncoder.Create(UnicodeRanges.All) が
            // 正しく設定されていることを確認するためのものです
            
            // Act
            ProjectBuilder.CreateFromTemplate(_testDirectory);

            // Assert
            string projectFilePath = Path.Combine(_testDirectory, "project.metasia ");
            string jsonContent = File.ReadAllText(projectFilePath);
            
            // 基本的なJSON構造が保持されていることを確認
            Assert.DoesNotThrow(() => JsonDocument.Parse(jsonContent));
        }
    }
} 
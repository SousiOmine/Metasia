using NUnit.Framework;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Tools.ProjectTool;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Moq;

namespace Metasia.Editor.Tests.Models.Tools.ProjectTool
{
    [TestFixture]
    public class FileTreeNodeTests
    {
        [Test]
        public void Constructor_WithTitleOnly_CreatesNodeWithoutSubNodes()
        {
            // Act
            var node = new FileTreeNode("Test Node");

            // Assert
            Assert.That(node.Title, Is.EqualTo("Test Node"));
            Assert.That(node.ResourceEntity, Is.Null);
            Assert.That(node.SubNodes, Is.Null);
        }

        [Test]
        public void Constructor_WithTitleAndSubNodes_CreatesNodeWithSubNodes()
        {
            // Arrange
            var subNodes = new ObservableCollection<FileTreeNode>
            {
                new FileTreeNode("Child 1"),
                new FileTreeNode("Child 2")
            };

            // Act
            var node = new FileTreeNode("Parent Node", subNodes);

            // Assert
            Assert.That(node.Title, Is.EqualTo("Parent Node"));
            Assert.That(node.ResourceEntity, Is.Null);
            Assert.That(node.SubNodes, Is.EqualTo(subNodes));
            Assert.That(node.SubNodes.Count, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_WithFileEntity_CreatesNodeWithFileInfo()
        {
            // Arrange
            var mockFileEntity = new Mock<IFileEntity>();
            mockFileEntity.Setup(f => f.Name).Returns("test.txt");
            mockFileEntity.Setup(f => f.Path).Returns("/path/to/test.txt");
            mockFileEntity.Setup(f => f.FileType).Returns(FileTypes.Text);

            // Act
            var node = new FileTreeNode(mockFileEntity.Object);

            // Assert
            Assert.That(node.Title, Is.EqualTo("test.txt"));
            Assert.That(node.ResourceEntity, Is.EqualTo(mockFileEntity.Object));
            Assert.That(node.SubNodes, Is.Null);
        }

        [Test]
        public void Constructor_WithDirectoryEntity_CreatesNodeWithSubordinates()
        {
            // Arrange
            var mockFile1 = new Mock<IFileEntity>();
            mockFile1.Setup(f => f.Name).Returns("file1.txt");
            
            var mockFile2 = new Mock<IFileEntity>();
            mockFile2.Setup(f => f.Name).Returns("file2.txt");
            
            var mockSubDir = new Mock<IDirectoryEntity>();
            mockSubDir.Setup(d => d.Name).Returns("subdir");
            mockSubDir.Setup(d => d.GetSubordinates()).Returns(new IResourceEntity[] { });

            var mockDirEntity = new Mock<IDirectoryEntity>();
            mockDirEntity.Setup(d => d.Name).Returns("TestDirectory");
            mockDirEntity.Setup(d => d.Path).Returns("/path/to/TestDirectory");
            mockDirEntity.Setup(d => d.GetSubordinates()).Returns(new IResourceEntity[] 
            { 
                mockFile1.Object, 
                mockFile2.Object,
                mockSubDir.Object 
            });

            // Act
            var node = new FileTreeNode(mockDirEntity.Object);

            // Assert
            Assert.That(node.Title, Is.EqualTo("TestDirectory"));
            Assert.That(node.ResourceEntity, Is.EqualTo(mockDirEntity.Object));
            Assert.That(node.SubNodes, Is.Not.Null);
            Assert.That(node.SubNodes.Count, Is.EqualTo(3));
            
            var titles = node.SubNodes.Select(n => n.Title).ToList();
            Assert.That(titles, Does.Contain("file1.txt"));
            Assert.That(titles, Does.Contain("file2.txt"));
            Assert.That(titles, Does.Contain("subdir"));
        }

        [Test]
        public void Constructor_WithEmptyDirectory_CreatesNodeWithEmptySubNodes()
        {
            // Arrange
            var mockDirEntity = new Mock<IDirectoryEntity>();
            mockDirEntity.Setup(d => d.Name).Returns("EmptyDir");
            mockDirEntity.Setup(d => d.GetSubordinates()).Returns(new IResourceEntity[] { });

            // Act
            var node = new FileTreeNode(mockDirEntity.Object);

            // Assert
            Assert.That(node.Title, Is.EqualTo("EmptyDir"));
            Assert.That(node.SubNodes, Is.Not.Null);
            Assert.That(node.SubNodes.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithNullTitle_CreatesNodeWithNullTitle()
        {
            // Act
            var node = new FileTreeNode((string?)null);

            // Assert
            Assert.That(node.Title, Is.Null);
            Assert.That(node.ResourceEntity, Is.Null);
            Assert.That(node.SubNodes, Is.Null);
        }

        [Test]
        public void SubNodes_IsObservableCollection()
        {
            // Arrange
            var mockDirEntity = new Mock<IDirectoryEntity>();
            mockDirEntity.Setup(d => d.Name).Returns("TestDir");
            mockDirEntity.Setup(d => d.GetSubordinates()).Returns(new IResourceEntity[] { });

            // Act
            var node = new FileTreeNode(mockDirEntity.Object);

            // Assert
            Assert.That(node.SubNodes, Is.InstanceOf<ObservableCollection<FileTreeNode>>());
        }

        [Test]
        public void NestedDirectories_AreHandledRecursively()
        {
            // Arrange
            var mockNestedFile = new Mock<IFileEntity>();
            mockNestedFile.Setup(f => f.Name).Returns("nested.txt");

            var mockNestedDir = new Mock<IDirectoryEntity>();
            mockNestedDir.Setup(d => d.Name).Returns("NestedDir");
            mockNestedDir.Setup(d => d.GetSubordinates()).Returns(new IResourceEntity[] { mockNestedFile.Object });

            var mockRootDir = new Mock<IDirectoryEntity>();
            mockRootDir.Setup(d => d.Name).Returns("RootDir");
            mockRootDir.Setup(d => d.GetSubordinates()).Returns(new IResourceEntity[] { mockNestedDir.Object });

            // Act
            var rootNode = new FileTreeNode(mockRootDir.Object);

            // Assert
            Assert.That(rootNode.SubNodes.Count, Is.EqualTo(1));
            
            var nestedDirNode = rootNode.SubNodes[0];
            Assert.That(nestedDirNode.Title, Is.EqualTo("NestedDir"));
            Assert.That(nestedDirNode.SubNodes, Is.Not.Null);
            Assert.That(nestedDirNode.SubNodes.Count, Is.EqualTo(1));
            
            var nestedFileNode = nestedDirNode.SubNodes![0];
            Assert.That(nestedFileNode.Title, Is.EqualTo("nested.txt"));
            Assert.That(nestedFileNode.SubNodes, Is.Null);
        }
    }
} 
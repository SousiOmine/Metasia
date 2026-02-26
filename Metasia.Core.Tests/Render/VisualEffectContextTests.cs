using Metasia.Core.Media;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Moq;
using SkiaSharp;

namespace Metasia.Core.Tests.Render
{
    /// <summary>
    /// VisualEffectContextクラスのテスト
    /// </summary>
    [TestFixture]
    public class VisualEffectContextTests
    {
        [Test]
        public void Constructor_SetsAllProperties()
        {
            // Arrange & Act
            var context = new VisualEffectContext(
                frame: 15,
                relativeFrame: 5,
                clipLength: 100,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                logicalSize: new SKSize(200, 150));

            // Assert
            Assert.That(context.Frame, Is.EqualTo(15));
            Assert.That(context.RelativeFrame, Is.EqualTo(5));
            Assert.That(context.ClipLength, Is.EqualTo(100));
            Assert.That(context.ProjectResolution, Is.EqualTo(new SKSize(1920, 1080)));
            Assert.That(context.RenderResolution, Is.EqualTo(new SKSize(960, 540)));
            Assert.That(context.LogicalSize, Is.EqualTo(new SKSize(200, 150)));
        }

        [Test]
        public void FromRenderContext_CalculatesRelativeFrameCorrectly()
        {
            // Arrange
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);
            var renderContext = new RenderContext(
                frame: 20,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: projectInfo,
                projectPath: string.Empty);

            int startFrame = 10;
            int endFrame = 50;
            var logicalSize = new SKSize(300, 200);

            // Act
            var context = VisualEffectContext.FromRenderContext(renderContext, startFrame, endFrame, logicalSize);

            // Assert
            Assert.That(context.Frame, Is.EqualTo(20));
            Assert.That(context.RelativeFrame, Is.EqualTo(10)); // 20 - 10
            Assert.That(context.ClipLength, Is.EqualTo(41));    // 50 - 10 + 1
        }

        [Test]
        public void FromRenderContext_CopiesResolutions()
        {
            // Arrange
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);
            var renderContext = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: projectInfo,
                projectPath: string.Empty);

            // Act
            var context = VisualEffectContext.FromRenderContext(renderContext, 0, 100, new SKSize(100, 100));

            // Assert
            Assert.That(context.ProjectResolution, Is.EqualTo(new SKSize(1920, 1080)));
            Assert.That(context.RenderResolution, Is.EqualTo(new SKSize(960, 540)));
        }

        [Test]
        public void FromRenderContext_CopiesLogicalSize()
        {
            // Arrange
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);
            var renderContext = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(1920, 1080),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: projectInfo,
                projectPath: string.Empty);
            var logicalSize = new SKSize(512, 256);

            // Act
            var context = VisualEffectContext.FromRenderContext(renderContext, 0, 100, logicalSize);

            // Assert
            Assert.That(context.LogicalSize, Is.EqualTo(logicalSize));
        }

        [Test]
        public void FromRenderContext_StartFrameEqualsFrame_RelativeFrameIsZero()
        {
            // Arrange
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);
            var renderContext = new RenderContext(
                frame: 30,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(1920, 1080),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: projectInfo,
                projectPath: string.Empty);

            // Act
            var context = VisualEffectContext.FromRenderContext(renderContext, 30, 60, new SKSize(100, 100));

            // Assert
            Assert.That(context.RelativeFrame, Is.EqualTo(0));
            Assert.That(context.ClipLength, Is.EqualTo(31)); // 60 - 30 + 1
        }
    }
}

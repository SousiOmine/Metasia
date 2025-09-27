using System.Collections.Generic;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Moq;
using NUnit.Framework;
using SkiaSharp;

namespace Metasia.Core.Tests.Render
{
    public class CompositorTests
    {
        private Compositor _compositor = null!;
        private EmptyImageFileAccessor _imageAccessor = null!;
        private EmptyVideoFileAccessor _videoAccessor = null!;

        [SetUp]
        public void Setup()
        {
            _compositor = new Compositor();
            _imageAccessor = new EmptyImageFileAccessor();
            _videoAccessor = new EmptyVideoFileAccessor();
        }

        [Test]
        public void RenderFrame_SingleNode_DrawBitmap()
        {
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Red),
                LogicalSize = new SKSize(100, 100),
            };

            var mockRenderable = new Mock<IRenderable>();
            mockRenderable.Setup(x => x.Render(It.IsAny<RenderContext>())).Returns(node);

            var projectInfo = CreateProjectInfo(new SKSize(3840, 2160));
            var renderResolution = new SKSize(192, 108);

            using var resultBitmap = _compositor.RenderFrame(
                mockRenderable.Object,
                0,
                renderResolution,
                projectInfo.Size,
                _imageAccessor,
                _videoAccessor,
                projectInfo);

            Assert.That(resultBitmap, Is.Not.Null);
            Assert.That(resultBitmap.Width, Is.EqualTo((int)renderResolution.Width));
            Assert.That(resultBitmap.Height, Is.EqualTo((int)renderResolution.Height));
            Assert.That(resultBitmap.GetPixel(0, 0), Is.EqualTo(SKColors.Black));
            Assert.That(resultBitmap.GetPixel((int)renderResolution.Width - 1, (int)renderResolution.Height - 1), Is.EqualTo(SKColors.Black));
            Assert.That(resultBitmap.GetPixel((int)renderResolution.Width / 2, (int)renderResolution.Height / 2), Is.EqualTo(SKColors.Red));
        }

        [Test]
        public void RenderFrame_NodeWithScale_RenderedCorrectly()
        {
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Blue),
                LogicalSize = new SKSize(50, 50),
                Transform = new Transform { Scale = 2.0f, Position = new SKPoint(0, 0) }
            };

            var mockRenderable = new Mock<IRenderable>();
            mockRenderable.Setup(x => x.Render(It.IsAny<RenderContext>())).Returns(node);

            var projectInfo = CreateProjectInfo(new SKSize(200, 200));
            var renderResolution = new SKSize(200, 200);

            using var resultBitmap = _compositor.RenderFrame(
                mockRenderable.Object,
                0,
                renderResolution,
                projectInfo.Size,
                _imageAccessor,
                _videoAccessor,
                projectInfo);

            Assert.That(resultBitmap.GetPixel((int)renderResolution.Width / 2, (int)renderResolution.Height / 2), Is.EqualTo(SKColors.Blue));
        }

        [Test]
        public void RenderFrame_NodeWithRotation_RotatesBitmap()
        {
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Green, 20, 10),
                LogicalSize = new SKSize(20, 10),
                Transform = new Transform { Rotation = 90 }
            };

            var mockRenderable = new Mock<IRenderable>();
            mockRenderable.Setup(x => x.Render(It.IsAny<RenderContext>())).Returns(node);

            var projectInfo = CreateProjectInfo(new SKSize(100, 100));
            var renderResolution = new SKSize(100, 100);

            using var resultBitmap = _compositor.RenderFrame(
                mockRenderable.Object,
                0,
                renderResolution,
                projectInfo.Size,
                _imageAccessor,
                _videoAccessor,
                projectInfo);

            Assert.That(resultBitmap.GetPixel((int)renderResolution.Width / 2, (int)renderResolution.Height / 2), Is.EqualTo(SKColors.Green));
        }

        [Test]
        public void RenderFrame_NodeWithAlpha_AlphaApplied()
        {
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Yellow),
                LogicalSize = new SKSize(100, 100),
                Transform = new Transform { Alpha = 0.5f }
            };

            var mockRenderable = new Mock<IRenderable>();
            mockRenderable.Setup(x => x.Render(It.IsAny<RenderContext>())).Returns(node);

            var projectInfo = CreateProjectInfo(new SKSize(200, 200));
            var renderResolution = new SKSize(200, 200);

            using var resultBitmap = _compositor.RenderFrame(
                mockRenderable.Object,
                0,
                renderResolution,
                projectInfo.Size,
                _imageAccessor,
                _videoAccessor,
                projectInfo);

            var pixel = resultBitmap.GetPixel((int)renderResolution.Width / 2, (int)renderResolution.Height / 2);
            Assert.That(pixel.Alpha, Is.EqualTo((byte)(0.5f * 255)));
            Assert.That(pixel.Red, Is.GreaterThan(0));
            Assert.That(pixel.Green, Is.GreaterThan(0));
        }

        [Test]
        public void RenderFrame_WithChildNodes_RendersAll()
        {
            var child = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Magenta),
                LogicalSize = new SKSize(30, 30),
                Transform = new Transform { Position = new SKPoint(50, 0) }
            };

            var parent = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Cyan),
                LogicalSize = new SKSize(30, 30),
                Children = new List<RenderNode> { child },
                Transform = new Transform { Position = new SKPoint(-50, 0) }
            };

            var mockRenderable = new Mock<IRenderable>();
            mockRenderable.Setup(x => x.Render(It.IsAny<RenderContext>())).Returns(parent);

            var projectInfo = CreateProjectInfo(new SKSize(200, 200));
            var renderResolution = new SKSize(200, 200);

            using var resultBitmap = _compositor.RenderFrame(
                mockRenderable.Object,
                0,
                renderResolution,
                projectInfo.Size,
                _imageAccessor,
                _videoAccessor,
                projectInfo);

            Assert.That(resultBitmap.GetPixel(50, (int)renderResolution.Height / 2), Is.EqualTo(SKColors.Cyan));
            Assert.That(resultBitmap.GetPixel(150, (int)renderResolution.Height / 2), Is.EqualTo(SKColors.Magenta));
        }

        private static ProjectInfo CreateProjectInfo(SKSize size) => new ProjectInfo(30, size, 44100, 2);

        private static SKBitmap CreateTestBitmap(SKColor color, int width = 100, int height = 100)
        {
            var bitmap = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(color);
            }

            return bitmap;
        }
    }
}

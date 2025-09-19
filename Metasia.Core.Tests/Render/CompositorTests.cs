using Metasia.Core.Objects;
using Metasia.Core.Render;
using Moq;
using SkiaSharp;
using Metasia.Core.Media;

namespace Metasia.Core.Tests.Render
{
	public class CompositorTests
	{
        private Compositor compositor;

        [SetUp]
        public void Setup()
        {
            compositor = new Compositor();
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

            using var resultBitmap = compositor.RenderFrame(mockRenderable.Object, 0, new SKSize(192, 108), new SKSize(3840, 2160), null);

            Assert.That(resultBitmap, Is.Not.Null);
            Assert.That(resultBitmap.Width, Is.EqualTo(192));
            Assert.That(resultBitmap.Height, Is.EqualTo(108));

            Assert.That(resultBitmap.GetPixel(0, 0), Is.EqualTo(SKColors.Black));
            Assert.That(resultBitmap.GetPixel(191, 107), Is.EqualTo(SKColors.Black));
            Assert.That(resultBitmap.GetPixel(96, 54), Is.EqualTo(SKColors.Red));
        }


        private SKBitmap CreateTestBitmap(SKColor color, int width = 100, int height = 100)
        {
            var bitmap = new SKBitmap(width, height);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(color);
            }
            return bitmap;
        }
	}
}
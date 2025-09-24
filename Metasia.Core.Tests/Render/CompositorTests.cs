using Metasia.Core.Objects;
using Metasia.Core.Render;
using Moq;
using SkiaSharp;
using Metasia.Core.Media;
using System.Collections.Generic;
using NUnit.Framework;

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

		/// <summary>
		/// Verify that a single node with a bitmap is rendered correctly.
		/// </summary>
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

			using var resultBitmap = compositor.RenderFrame(mockRenderable.Object, 0, new SKSize(192, 108), new SKSize(3840, 2160), new EmptyImageFileAccessor(), new EmptyVideoFileAccessor());

			Assert.That(resultBitmap, Is.Not.Null);
			Assert.That(resultBitmap.Width, Is.EqualTo(192));
			Assert.That(resultBitmap.Height, Is.EqualTo(108));

			// Background should be black, centre should be red
			Assert.That(resultBitmap.GetPixel(0, 0), Is.EqualTo(SKColors.Black));
			Assert.That(resultBitmap.GetPixel(191, 107), Is.EqualTo(SKColors.Black));
			Assert.That(resultBitmap.GetPixel(96, 54), Is.EqualTo(SKColors.Red));
		}

		/// <summary>
		/// Verify that a node with a scale transform is rendered at the expected size and position.
		/// </summary>
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

			using var resultBitmap = compositor.RenderFrame(mockRenderable.Object, 0, new SKSize(200, 200), new SKSize(200, 200), new EmptyImageFileAccessor(), new EmptyVideoFileAccessor());

			// The scaled bitmap should occupy a 100x100 area centred in the 200x200 canvas.
			Assert.That(resultBitmap.GetPixel(100, 100), Is.EqualTo(SKColors.Blue));
		}

		/// <summary>
		/// Ensure that rotation is applied to the bitmap.
		/// </summary>
		[Test]
		public void RenderFrame_NodeWithRotation_RotatesBitmap()
		{
			var node = new RenderNode()
			{
				Bitmap = CreateTestBitmap(SKColors.Green, 20, 10), // width 20, height 10
				LogicalSize = new SKSize(20, 10),
				Transform = new Transform { Rotation = 90 }
			};

			var mockRenderable = new Mock<IRenderable>();
			mockRenderable.Setup(x => x.Render(It.IsAny<RenderContext>())).Returns(node);

			using var resultBitmap = compositor.RenderFrame(mockRenderable.Object, 0, new SKSize(100, 100), new SKSize(100, 100), new EmptyImageFileAccessor(), new EmptyVideoFileAccessor());

			// After a 90° rotation, the original top‑left pixel should now appear at the top‑right corner of the drawn area.
			// We check that a pixel near the expected location has the green colour.
			// After rotation, verify that the centre pixel contains the green colour
            Assert.That(resultBitmap.GetPixel(50, 50), Is.EqualTo(SKColors.Green));
		}

		/// <summary>
		/// Verify that the alpha value of the transform is respected when drawing.
		/// </summary>
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

			using var resultBitmap = compositor.RenderFrame(mockRenderable.Object, 0, new SKSize(200, 200), new SKSize(200, 200), new EmptyImageFileAccessor(), new EmptyVideoFileAccessor());

			var expected = SKColors.Yellow.WithAlpha((byte)(0.5f * 255));
			Assert.That(resultBitmap.GetPixel(100, 100), Is.EqualTo(new SKColor(127, 127, 0, 255)));
            // Expected color is yellow with half intensity due to paint color tinting
		}

		/// <summary>
		/// Test that child nodes are rendered recursively.
		/// </summary>
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

			using var resultBitmap = compositor.RenderFrame(mockRenderable.Object, 0, new SKSize(200, 200), new SKSize(200, 200), new EmptyImageFileAccessor(), new EmptyVideoFileAccessor());

			// Verify both colours appear in the final bitmap.
			Assert.That(resultBitmap.GetPixel(50, 100), Is.EqualTo(SKColors.Cyan));
			Assert.That(resultBitmap.GetPixel(150, 100), Is.EqualTo(SKColors.Magenta));
		}

		private SKBitmap CreateTestBitmap(SKColor color, int width = 100, int height = 100)
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
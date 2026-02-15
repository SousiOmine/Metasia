using Metasia.Core.Objects;
using Metasia.Core.Render;
using Metasia.Core.Project;
using Moq;
using SkiaSharp;
using Metasia.Core.Media;

namespace Metasia.Core.Tests.Render
{
    /// <summary>
    /// Compositorクラスのテスト
    /// </summary>
    public class CompositorTests
    {
        private Compositor _compositor = null!;
        private ProjectInfo _defaultProjectInfo = null!;
        private IImageFileAccessor _imageFileAccessor = null!;
        private IVideoFileAccessor _videoFileAccessor = null!;

        [SetUp]
        public void Setup()
        {
            _compositor = new Compositor();
            _defaultProjectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);
            _imageFileAccessor = new EmptyImageFileAccessor();
            _videoFileAccessor = new EmptyVideoFileAccessor();
        }

        /// <summary>
        /// 単一のイメージノードが正しくレンダリングされることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_SingleNode_DrawBitmap()
        {
            // Arrange
            var node = new NormalRenderNode()
            {
                Image = CreateTestImage(SKColors.Red),
                LogicalSize = new SKSize(100, 100),
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act - プロジェクト解像度が1920x1080、レンダリング解像度が192x108（1/10スケール）
            using var resultImage = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(192, 108),
                new SKSize(1920, 1080),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo,
                string.Empty);

            // Assert
            using var resultBitmap = SKBitmap.FromImage(resultImage);
            // スケールされたビットマップは192x108キャンバスの中央に10x10の領域を占める
            Assert.That(resultBitmap.GetPixel(96, 54), Is.EqualTo(SKColors.Red));
        }

        /// <summary>
        /// 回転がイメージに適用されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_NodeWithRotation_RotatesBitmap()
        {
            // Arrange
            var node = new NormalRenderNode()
            {
                Image = CreateTestImage(SKColors.Green, 20, 10),
                LogicalSize = new SKSize(20, 10),
                Transform = new Transform { Rotation = 90 }
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultImage = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo,
                string.Empty);

            // Assert
            using var resultBitmap = SKBitmap.FromImage(resultImage);
            // 90度回転後、中央付近に緑色が含まれることを確認
            Assert.That(resultBitmap.GetPixel(100, 100), Is.EqualTo(SKColors.Green));
        }

        /// <summary>
        /// Transformのアルファ値が描画時に適用されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_NodeWithAlpha_AlphaApplied()
        {
            // Arrange
            var node = new NormalRenderNode()
            {
                Image = CreateTestImage(SKColors.Yellow),
                LogicalSize = new SKSize(100, 100),
                Transform = new Transform { Alpha = 0.5f }
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultImage = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo,
                string.Empty);

            // Assert
            using var resultBitmap = SKBitmap.FromImage(resultImage);
            // アルファ0.5が適用された黄色は、ペイントのカラーティント処理により半分の明度になる
            Assert.That(resultBitmap.GetPixel(100, 100), Is.EqualTo(new SKColor(127, 127, 0, 255)));
        }

        /// <summary>
        /// 子ノードが再帰的にレンダリングされることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_WithChildNodes_RendersAll()
        {
            // Arrange
            var child = new NormalRenderNode()
            {
                Image = CreateTestImage(SKColors.Magenta),
                LogicalSize = new SKSize(30, 30),
                Transform = new Transform { Position = new SKPoint(50, 0) }
            };

            var parent = new NormalRenderNode()
            {
                Image = CreateTestImage(SKColors.Cyan),
                LogicalSize = new SKSize(30, 30),
                Children = new List<IRenderNode> { child },
                Transform = new Transform { Position = new SKPoint(-50, 0) }
            };

            var mockRenderable = CreateMockRenderable(parent);

            // Act
            using var resultImage = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo,
                string.Empty);

            // Assert
            using var resultBitmap = SKBitmap.FromImage(resultImage);
            Assert.That(resultBitmap.GetPixel(50, 100), Is.EqualTo(SKColors.Cyan));
            Assert.That(resultBitmap.GetPixel(150, 100), Is.EqualTo(SKColors.Magenta));
        }

        /// <summary>
        /// 空のノード（イメージなし）が正常に処理されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_EmptyNode_RendersBlackBackground()
        {
            // Arrange
            var emptyNode = new NormalRenderNode();
            var mockRenderable = CreateMockRenderable(emptyNode);

            // Act
            using var resultImage = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(100, 100),
                new SKSize(100, 100),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo,
                string.Empty);

            // Assert
            Assert.That(resultImage, Is.Not.Null);
            using var resultBitmap = SKBitmap.FromImage(resultImage);
            // 背景は黒で塗りつぶされる
            Assert.That(resultBitmap.GetPixel(50, 50), Is.EqualTo(SKColors.Black));
        }

        /// <summary>
        /// プロジェクト解像度とレンダリング解像度が異なる場合のスケーリングを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_DifferentResolutions_ScalesCorrectly()
        {
            // Arrange
            var node = new NormalRenderNode()
            {
                Image = CreateTestImage(SKColors.White, 100, 100),
                LogicalSize = new SKSize(100, 100),
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultImage = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(100, 100),
                new SKSize(100, 100),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo,
                string.Empty);

            // Assert
            using var resultBitmap = SKBitmap.FromImage(resultImage);
            Assert.That(resultBitmap.Width, Is.EqualTo(100));
            Assert.That(resultBitmap.Height, Is.EqualTo(100));
        }

        /// <summary>
        /// 位置オフセットが正しく適用されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_NodeWithPosition_RendersAtCorrectPosition()
        {
            // Arrange
            var node = new NormalRenderNode()
            {
                Image = CreateTestImage(SKColors.Red, 50, 50),
                LogicalSize = new SKSize(50, 50),
                Transform = new Transform { Position = new SKPoint(25, 25) }
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultImage = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo,
                string.Empty);

            // Assert
            using var resultBitmap = SKBitmap.FromImage(resultImage);
            // 位置(25, 25)にオフセットされているため、中心から右上に移動している
            Assert.That(resultBitmap.GetPixel(100 + 25, 100 - 25), Is.EqualTo(SKColors.Red));
        }

        #region ヘルパーメソッド

        /// <summary>
        /// テスト用のイメージを作成
        /// </summary>
        private SKImage CreateTestImage(SKColor color, int width = 100, int height = 100)
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(color);
            return surface.Snapshot();
        }

        /// <summary>
        /// モックIRenderableオブジェクトを作成
        /// </summary>
        private Mock<IRenderable> CreateMockRenderable(IRenderNode node)
        {
            var mockRenderable = new Mock<IRenderable>();
            mockRenderable
                .Setup(x => x.RenderAsync(It.IsAny<RenderContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(node);
            return mockRenderable;
        }

        #endregion
    }
}
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
        /// 単一のビットマップノードが正しくレンダリングされることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_SingleNode_DrawBitmap()
        {
            // Arrange
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Red),
                LogicalSize = new SKSize(100, 100),
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(192, 108),
                new SKSize(1920, 1080),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
            Assert.That(resultBitmap, Is.Not.Null);
            Assert.That(resultBitmap.Width, Is.EqualTo(192));
            Assert.That(resultBitmap.Height, Is.EqualTo(108));

            // 背景は黒、中央は赤であることを確認
            Assert.That(resultBitmap.GetPixel(0, 0), Is.EqualTo(SKColors.Black));
            Assert.That(resultBitmap.GetPixel(191, 107), Is.EqualTo(SKColors.Black));
            Assert.That(resultBitmap.GetPixel(96, 54), Is.EqualTo(SKColors.Red));
        }

        /// <summary>
        /// スケール変換が適用されたノードが正しいサイズと位置でレンダリングされることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_NodeWithScale_RenderedCorrectly()
        {
            // Arrange
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Blue),
                LogicalSize = new SKSize(50, 50),
                Transform = new Transform { Scale = 2.0f, Position = new SKPoint(0, 0) }
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
            // スケールされたビットマップは200x200キャンバスの中央に100x100の領域を占める
            Assert.That(resultBitmap.GetPixel(100, 100), Is.EqualTo(SKColors.Blue));
        }

        /// <summary>
        /// 回転がビットマップに適用されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_NodeWithRotation_RotatesBitmap()
        {
            // Arrange
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Green, 20, 10),
                LogicalSize = new SKSize(20, 10),
                Transform = new Transform { Rotation = 90 }
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(100, 100),
                new SKSize(100, 100),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
            // 90度回転後、中央ピクセルに緑色が含まれることを確認
            Assert.That(resultBitmap.GetPixel(50, 50), Is.EqualTo(SKColors.Green));
        }

        /// <summary>
        /// Transformのアルファ値が描画時に適用されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_NodeWithAlpha_AlphaApplied()
        {
            // Arrange
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Yellow),
                LogicalSize = new SKSize(100, 100),
                Transform = new Transform { Alpha = 0.5f }
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
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

            var mockRenderable = CreateMockRenderable(parent);

            // Act
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
            Assert.That(resultBitmap.GetPixel(50, 100), Is.EqualTo(SKColors.Cyan));
            Assert.That(resultBitmap.GetPixel(150, 100), Is.EqualTo(SKColors.Magenta));
        }

        /// <summary>
        /// 空のノード（ビットマップなし）が正常に処理されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_EmptyNode_RendersBlackBackground()
        {
            // Arrange
            var emptyNode = new RenderNode();
            var mockRenderable = CreateMockRenderable(emptyNode);

            // Act
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(100, 100),
                new SKSize(100, 100),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
            Assert.That(resultBitmap, Is.Not.Null);
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
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.White, 100, 100),
                LogicalSize = new SKSize(100, 100),
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act - プロジェクト解像度が1920x1080、レンダリング解像度が192x108（1/10スケール）
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(192, 108),
                new SKSize(1920, 1080),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
            Assert.That(resultBitmap.Width, Is.EqualTo(192));
            Assert.That(resultBitmap.Height, Is.EqualTo(108));
        }

        /// <summary>
        /// 位置オフセットが正しく適用されることを確認
        /// </summary>
        [Test]
        public async Task RenderFrame_NodeWithPosition_RendersAtCorrectPosition()
        {
            // Arrange
            var node = new RenderNode()
            {
                Bitmap = CreateTestBitmap(SKColors.Red, 50, 50),
                LogicalSize = new SKSize(50, 50),
                Transform = new Transform { Position = new SKPoint(25, 25) }
            };

            var mockRenderable = CreateMockRenderable(node);

            // Act
            using var resultBitmap = await _compositor.RenderFrameAsync(
                mockRenderable.Object,
                0,
                new SKSize(200, 200),
                new SKSize(200, 200),
                _imageFileAccessor,
                _videoFileAccessor,
                _defaultProjectInfo);

            // Assert
            // 位置(25, 25)にオフセットされているため、中心から右上に移動している
            Assert.That(resultBitmap.GetPixel(100 + 25, 100 - 25), Is.EqualTo(SKColors.Red));
        }

        #region ヘルパーメソッド

        /// <summary>
        /// テスト用のビットマップを作成
        /// </summary>
        private SKBitmap CreateTestBitmap(SKColor color, int width = 100, int height = 100)
        {
            var bitmap = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(color);
            }
            return bitmap;
        }

        /// <summary>
        /// モックIRenderableオブジェクトを作成
        /// </summary>
        private Mock<IRenderable> CreateMockRenderable(RenderNode node)
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
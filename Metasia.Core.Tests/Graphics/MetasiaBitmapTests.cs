using NUnit.Framework;
using Metasia.Core.Graphics;
using SkiaSharp;
using System;

namespace Metasia.Core.Tests.Graphics
{
    [TestFixture]
    public class MetasiaBitmapTests
    {
        private MetasiaBitmap _testBitmap;
        private const int TestWidth = 100;
        private const int TestHeight = 100;

        [SetUp]
        public void Setup()
        {
            _testBitmap = new MetasiaBitmap(TestWidth, TestHeight);
        }

        [TearDown]
        public void TearDown()
        {
            _testBitmap?.Dispose();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Default_CreatesValidBitmap()
        {
            // Arrange & Act
            using var bitmap = new MetasiaBitmap();

            // Assert
            Assert.That(bitmap, Is.Not.Null);
            Assert.That(bitmap, Is.InstanceOf<SKBitmap>());
        }

        [Test]
        public void Constructor_WithWidthAndHeight_CreatesCorrectSize()
        {
            // Arrange & Act
            using var bitmap = new MetasiaBitmap(200, 150);

            // Assert
            Assert.That(bitmap.Width, Is.EqualTo(200));
            Assert.That(bitmap.Height, Is.EqualTo(150));
        }

        [Test]
        public void Constructor_WithSKImageInfo_CreatesCorrectSize()
        {
            // Arrange
            var info = new SKImageInfo(300, 250);

            // Act
            using var bitmap = new MetasiaBitmap(info);

            // Assert
            Assert.That(bitmap.Width, Is.EqualTo(300));
            Assert.That(bitmap.Height, Is.EqualTo(250));
        }

        [Test]
        public void Constructor_WithSKImageInfoAndRowBytes_CreatesValidBitmap()
        {
            // Arrange
            var info = new SKImageInfo(100, 100, SKColorType.Rgba8888);
            int rowBytes = info.Width * info.BytesPerPixel;

            // Act
            using var bitmap = new MetasiaBitmap(info, rowBytes);

            // Assert
            Assert.That(bitmap.Width, Is.EqualTo(100));
            Assert.That(bitmap.Height, Is.EqualTo(100));
        }

        [Test]
        public void Constructor_WithColorTypeAndAlphaType_CreatesValidBitmap()
        {
            // Arrange & Act
            using var bitmap = new MetasiaBitmap(150, 120, SKColorType.Rgba8888, SKAlphaType.Premul);

            // Assert
            Assert.That(bitmap.Width, Is.EqualTo(150));
            Assert.That(bitmap.Height, Is.EqualTo(120));
            Assert.That(bitmap.ColorType, Is.EqualTo(SKColorType.Rgba8888));
            Assert.That(bitmap.AlphaType, Is.EqualTo(SKAlphaType.Premul));
        }

        #endregion

        #region Rotate Method Tests

        [Test]
        public void Rotate_With90Degrees_RotatesCorrectly()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var rotatedBitmap = MetasiaBitmap.Rotate(_testBitmap, 90);

            // Assert
            Assert.That(rotatedBitmap, Is.Not.Null);
            Assert.That(rotatedBitmap.Width, Is.EqualTo(_testBitmap.Height));
            Assert.That(rotatedBitmap.Height, Is.EqualTo(_testBitmap.Width));
        }

        [Test]
        public void Rotate_With180Degrees_RotatesCorrectly()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var rotatedBitmap = MetasiaBitmap.Rotate(_testBitmap, 180);

            // Assert
            Assert.That(rotatedBitmap, Is.Not.Null);
            // 180度回転では幅と高さは変わらない
            Assert.That(rotatedBitmap.Width, Is.EqualTo(_testBitmap.Width));
            Assert.That(rotatedBitmap.Height, Is.EqualTo(_testBitmap.Height));
        }

        [Test]
        public void Rotate_With360Degrees_ReturnsSimilarSize()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var rotatedBitmap = MetasiaBitmap.Rotate(_testBitmap, 360);

            // Assert
            Assert.That(rotatedBitmap, Is.Not.Null);
            // 360度回転では元のサイズに近い値になる
            Assert.That(rotatedBitmap.Width, Is.GreaterThan(0));
            Assert.That(rotatedBitmap.Height, Is.GreaterThan(0));
        }

        [Test]
        public void Rotate_With45Degrees_IncreasesSize()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var rotatedBitmap = MetasiaBitmap.Rotate(_testBitmap, 45);

            // Assert
            Assert.That(rotatedBitmap, Is.Not.Null);
            // 45度回転では画像サイズが大きくなる
            Assert.That(rotatedBitmap.Width, Is.GreaterThan(_testBitmap.Width));
            Assert.That(rotatedBitmap.Height, Is.GreaterThan(_testBitmap.Height));
        }

        [Test]
        public void Rotate_WithNegativeAngle_RotatesCorrectly()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var rotatedBitmap = MetasiaBitmap.Rotate(_testBitmap, -90);

            // Assert
            Assert.That(rotatedBitmap, Is.Not.Null);
            Assert.That(rotatedBitmap.Width, Is.EqualTo(_testBitmap.Height));
            Assert.That(rotatedBitmap.Height, Is.EqualTo(_testBitmap.Width));
        }

        [Test]
        public void Rotate_WithZeroDegrees_ReturnsSameSize()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var rotatedBitmap = MetasiaBitmap.Rotate(_testBitmap, 0);

            // Assert
            Assert.That(rotatedBitmap, Is.Not.Null);
            Assert.That(rotatedBitmap.Width, Is.EqualTo(_testBitmap.Width));
            Assert.That(rotatedBitmap.Height, Is.EqualTo(_testBitmap.Height));
        }

        #endregion

        #region Transparency Method Tests

        [Test]
        public void Transparency_WithFullOpacity_ReturnsSameSize()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var transparentBitmap = MetasiaBitmap.Transparency(_testBitmap, 1.0);

            // Assert
            Assert.That(transparentBitmap, Is.Not.Null);
            Assert.That(transparentBitmap.Width, Is.EqualTo(_testBitmap.Width));
            Assert.That(transparentBitmap.Height, Is.EqualTo(_testBitmap.Height));
        }

        [Test]
        public void Transparency_WithHalfOpacity_ReturnsSameSize()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var transparentBitmap = MetasiaBitmap.Transparency(_testBitmap, 0.5);

            // Assert
            Assert.That(transparentBitmap, Is.Not.Null);
            Assert.That(transparentBitmap.Width, Is.EqualTo(_testBitmap.Width));
            Assert.That(transparentBitmap.Height, Is.EqualTo(_testBitmap.Height));
        }

        [Test]
        public void Transparency_WithZeroOpacity_ReturnsSameSize()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var transparentBitmap = MetasiaBitmap.Transparency(_testBitmap, 0.0);

            // Assert
            Assert.That(transparentBitmap, Is.Not.Null);
            Assert.That(transparentBitmap.Width, Is.EqualTo(_testBitmap.Width));
            Assert.That(transparentBitmap.Height, Is.EqualTo(_testBitmap.Height));
        }

        [TestCase(0.0)]
        [TestCase(0.25)]
        [TestCase(0.5)]
        [TestCase(0.75)]
        [TestCase(1.0)]
        public void Transparency_WithVariousAlphaValues_WorksCorrectly(double alpha)
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act
            using var transparentBitmap = MetasiaBitmap.Transparency(_testBitmap, alpha);

            // Assert
            Assert.That(transparentBitmap, Is.Not.Null);
            Assert.That(transparentBitmap.Width, Is.EqualTo(_testBitmap.Width));
            Assert.That(transparentBitmap.Height, Is.EqualTo(_testBitmap.Height));
        }

        [Test]
        public void Transparency_WithNegativeAlpha_WorksWithoutException()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                using var transparentBitmap = MetasiaBitmap.Transparency(_testBitmap, -0.5);
                Assert.That(transparentBitmap, Is.Not.Null);
            });
        }

        [Test]
        public void Transparency_WithAlphaGreaterThanOne_WorksWithoutException()
        {
            // Arrange
            CreateTestPattern(_testBitmap);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                using var transparentBitmap = MetasiaBitmap.Transparency(_testBitmap, 1.5);
                Assert.That(transparentBitmap, Is.Not.Null);
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// テスト用の簡単なパターンを描画
        /// </summary>
        /// <param name="bitmap">描画対象のビットマップ</param>
        private void CreateTestPattern(SKBitmap bitmap)
        {
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);
            
            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Fill
            };
            
            // 左上に赤い矩形を描画
            canvas.DrawRect(0, 0, bitmap.Width / 2, bitmap.Height / 2, paint);
            
            paint.Color = SKColors.Blue;
            // 右下に青い円を描画
            canvas.DrawCircle(bitmap.Width * 3 / 4, bitmap.Height * 3 / 4, bitmap.Width / 8, paint);
        }

        #endregion
    }
} 
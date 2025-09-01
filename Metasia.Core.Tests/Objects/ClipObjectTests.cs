using NUnit.Framework;
using Metasia.Core.Objects;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class ClipObjectTests
    {
        private ClipObject _clipObject;

        [SetUp]
        public void Setup()
        {
            _clipObject = new ClipObject("test-clip");
            _clipObject.StartFrame = 10;
            _clipObject.EndFrame = 100;
        }

        [Test]
        public void Constructor_WithId_InitializesCorrectly()
        {
            // Arrange & Act
            var clip = new ClipObject("test-id");

            // Assert
            Assert.That(clip.Id, Is.EqualTo("test-id"));
            Assert.That(clip.StartFrame, Is.EqualTo(0));
            Assert.That(clip.EndFrame, Is.EqualTo(100));
            Assert.That(clip.IsActive, Is.True);
        }

        [Test]
        public void Constructor_WithoutParameters_InitializesWithDefaults()
        {
            // Arrange & Act
            var clip = new ClipObject();

            // Assert
            Assert.That(clip.Id, Is.EqualTo(string.Empty));
            Assert.That(clip.StartFrame, Is.EqualTo(0));
            Assert.That(clip.EndFrame, Is.EqualTo(100));
            Assert.That(clip.IsActive, Is.True);
        }

        [Test]
        public void IsExistFromFrame_WithinRange_ReturnsTrue()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };

            // Act & Assert
            Assert.That(clip.IsExistFromFrame(10), Is.True);  // 開始フレーム
            Assert.That(clip.IsExistFromFrame(50), Is.True);  // 中間フレーム
            Assert.That(clip.IsExistFromFrame(100), Is.True); // 終了フレーム
        }

        [Test]
        public void IsExistFromFrame_OutsideRange_ReturnsFalse()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };

            // Act & Assert
            Assert.That(clip.IsExistFromFrame(9), Is.False);   // 開始前
            Assert.That(clip.IsExistFromFrame(101), Is.False); // 終了後
        }

        /// <summary>
        /// 有効な分割フレームでクリップを分割するテスト
        /// 想定結果: 2つのクリップが返され、フレーム範囲が正しく分割される
        /// </summary>
        [Test]
        public void SplitAtFrame_ValidSplitFrame_ReturnsTwoClips()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = clip.SplitAtFrame(splitFrame);

            // Assert
            Assert.That(firstClip, Is.Not.Null);
            Assert.That(secondClip, Is.Not.Null);
            Assert.That(firstClip.Id, Is.EqualTo("test_copy"));
            Assert.That(secondClip.Id, Is.EqualTo("test_copy"));
            Assert.That(firstClip.StartFrame, Is.EqualTo(10));
            Assert.That(firstClip.EndFrame, Is.EqualTo(49));
            Assert.That(secondClip.StartFrame, Is.EqualTo(50));
            Assert.That(secondClip.EndFrame, Is.EqualTo(100));
        }

        /// <summary>
        /// 開始フレームで分割しようとした場合のテスト
        /// 想定結果: ArgumentExceptionがスローされる
        /// </summary>
        [Test]
        public void SplitAtFrame_SplitFrameAtStart_ThrowsException()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => clip.SplitAtFrame(10));
            Assert.That(ex.Message, Does.Contain("分割フレームはクリップの開始フレームより大きく、終了フレームより小さい必要があります。"));
        }

        /// <summary>
        /// 終了フレームで分割しようとした場合のテスト
        /// 想定結果: ArgumentExceptionがスローされる
        /// </summary>
        [Test]
        public void SplitAtFrame_SplitFrameAtEnd_ThrowsException()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => clip.SplitAtFrame(100));
            Assert.That(ex.Message, Does.Contain("分割フレームはクリップの開始フレームより大きく、終了フレームより小さい必要があります。"));
        }

        /// <summary>
        /// 開始フレームより前で分割しようとした場合のテスト
        /// 想定結果: ArgumentExceptionがスローされる
        /// </summary>
        [Test]
        public void SplitAtFrame_SplitFrameBeforeStart_ThrowsException()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => clip.SplitAtFrame(5));
            Assert.That(ex.Message, Does.Contain("分割フレームはクリップの開始フレームより大きく、終了フレームより小さい必要があります。"));
        }

        /// <summary>
        /// 終了フレームより後で分割しようとした場合のテスト
        /// 想定結果: ArgumentExceptionがスローされる
        /// </summary>
        [Test]
        public void SplitAtFrame_SplitFrameAfterEnd_ThrowsException()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => clip.SplitAtFrame(150));
            Assert.That(ex.Message, Does.Contain("分割フレームはクリップの開始フレームより大きく、終了フレームより小さい必要があります。"));
        }

        /// <summary>
        /// 分割されたクリップが独立していることのテスト
        /// 想定結果: 元のクリップを変更しても分割されたクリップは影響を受けない
        /// </summary>
        [Test]
        public void SplitAtFrame_ClipsAreIndependent_ModifyingOriginalDoesNotAffectSplits()
        {
            // Arrange
            var originalClip = new ClipObject("original") { StartFrame = 10, EndFrame = 100 };
            var (firstClip, secondClip) = originalClip.SplitAtFrame(50);

            // Act
            originalClip.StartFrame = 20;
            originalClip.EndFrame = 80;

            // Assert
            Assert.That(firstClip.StartFrame, Is.EqualTo(10));
            Assert.That(firstClip.EndFrame, Is.EqualTo(49));
            Assert.That(secondClip.StartFrame, Is.EqualTo(50));
            Assert.That(secondClip.EndFrame, Is.EqualTo(100));
        }

        /// <summary>
        /// 分割されたクリップが異なるインスタンスであることのテスト
        /// 想定結果: 3つのクリップ（元、前半、後半）がすべて異なるインスタンス
        /// </summary>
        [Test]
        public void SplitAtFrame_ReturnedClipsAreDifferentInstances()
        {
            // Arrange
            var clip = new ClipObject("test") { StartFrame = 10, EndFrame = 100 };

            // Act
            var (firstClip, secondClip) = clip.SplitAtFrame(50);

            // Assert
            Assert.That(firstClip, Is.Not.SameAs(secondClip));
            Assert.That(firstClip, Is.Not.SameAs(clip));
            Assert.That(secondClip, Is.Not.SameAs(clip));
        }

        [Test]
        public void IsActive_CanBeModified()
        {
            // Arrange
            Assert.That(_clipObject.IsActive, Is.True);

            // Act
            _clipObject.IsActive = false;

            // Assert
            Assert.That(_clipObject.IsActive, Is.False);
        }
    }
}
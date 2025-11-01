using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class TextTests
    {
        private Text _textObject;

        [SetUp]
        public void Setup()
        {
            _textObject = new Text("text-id");
        }

        [Test]
        public void Constructor_WithId_InitializesCorrectly()
        {
            // Arrange & Act
            var text = new Text("test-id");

            // Assert
            Assert.That(text.Id, Is.EqualTo("test-id"));
            Assert.That(text.X, Is.Not.Null);
            Assert.That(text.Y, Is.Not.Null);
            Assert.That(text.Scale, Is.Not.Null);
            Assert.That(text.Alpha, Is.Not.Null);
            Assert.That(text.Rotation, Is.Not.Null);
            Assert.That(text.TextSize, Is.Not.Null);
        }

        [Test]
        public void CoordinateParameters_HaveCorrectDefaultValues()
        {
            // Assert
            Assert.That(_textObject.X.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.Y.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.Scale.Get(0), Is.EqualTo(100));
            Assert.That(_textObject.Alpha.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.Rotation.Get(0), Is.EqualTo(0));
            Assert.That(_textObject.TextSize.Get(0), Is.EqualTo(100));
        }

        [Test]
        public void Contents_CanBeSetAndRetrieved()
        {
            // Act
            _textObject.Contents = "Hello, World!";

            // Assert
            Assert.That(_textObject.Contents, Is.EqualTo("Hello, World!"));
        }

        [Test]
        public void FontParam_CanBeSetAndRetrieved()
        {
            // Act
            var font = new MetaFontParam("Arial", true, true);
            _textObject.Font = font;

            // Assert
            Assert.That(_textObject.Font.FamilyName, Is.EqualTo("Arial"));
            Assert.That(_textObject.Font.IsBold, Is.True);
            Assert.That(_textObject.Font.IsItalic, Is.True);
        }

        // Parentプロパティは存在しないため、このテストは削除

        [Test]
        public void InheritedProperties_WorkCorrectly()
        {
            // TextはClipObjectを継承しているので、基本プロパティも確認
            Assert.That(_textObject.StartFrame, Is.EqualTo(0));
            Assert.That(_textObject.EndFrame, Is.EqualTo(100));
            Assert.That(_textObject.IsActive, Is.True);

            // 変更も可能
            _textObject.StartFrame = 10;
            _textObject.EndFrame = 200;
            _textObject.IsActive = false;

            Assert.That(_textObject.StartFrame, Is.EqualTo(10));
            Assert.That(_textObject.EndFrame, Is.EqualTo(200));
            Assert.That(_textObject.IsActive, Is.False);
        }

        [Test]
        public void FontParam_SetterCreatesClone()
        {
            // Arrange
            var font = new MetaFontParam("Arial");

            // Act
            _textObject.Font = font;
            font.FamilyName = "Changed";

            // Assert
            Assert.That(_textObject.Font.FamilyName, Is.EqualTo("Arial"));
        }

        /// <summary>
        /// テキストオブジェクトを正常に分割できることを確認するテスト
        /// 意図: 分割機能がTextクラスで正しく動作し、基本プロパティが維持されることを検証
        /// 想定結果: 2つのTextオブジェクトが返され、ID、フレーム範囲、コンテンツ、フォントが正しく設定される
        /// </summary>
        [Test]
        public void SplitAtFrame_ValidSplitFrame_ReturnsTwoTextClipsWithCorrectProperties()
        {
            // Arrange
            _textObject.StartFrame = 10;
            _textObject.EndFrame = 100;
            _textObject.Contents = "Test Text";
            _textObject.Font = new MetaFontParam("Arial", true, false);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _textObject.SplitAtFrame(splitFrame);
            var firstText = firstClip as Text;
            var secondText = secondClip as Text;

            // Assert
            Assert.That(firstText, Is.Not.Null);
            Assert.That(secondText, Is.Not.Null);
            Assert.That(firstText.Id, Is.EqualTo("text-id_part1"));
            Assert.That(secondText.Id, Is.EqualTo("text-id_part2"));
            Assert.That(firstText.StartFrame, Is.EqualTo(10));
            Assert.That(firstText.EndFrame, Is.EqualTo(49));
            Assert.That(secondText.StartFrame, Is.EqualTo(50));
            Assert.That(secondText.EndFrame, Is.EqualTo(100));
            Assert.That(firstText.Contents, Is.EqualTo("Test Text"));
            Assert.That(secondText.Contents, Is.EqualTo("Test Text"));
            Assert.That(firstText.Font.FamilyName, Is.EqualTo("Arial"));
            Assert.That(firstText.Font.IsBold, Is.True);
            Assert.That(secondText.Font.FamilyName, Is.EqualTo("Arial"));
            Assert.That(secondText.Font.IsBold, Is.True);
        }

        /// <summary>
        /// 分割時に座標パラメータが正しく維持されることを確認するテスト
        /// 意図: MetaNumberParamプロパティが分割後も値を保持していることを検証
        /// 想定結果: 分割された両方のオブジェクトでX、Y、Scale座標パラメータが元の値を維持
        /// </summary>
        [Test]
        public void SplitAtFrame_PreservesCoordinateParameters()
        {
            // Arrange
            _textObject.StartFrame = 10;
            _textObject.EndFrame = 100;
            _textObject.X = new MetaNumberParam<double>(100);
            _textObject.Y = new MetaNumberParam<double>(200);
            _textObject.Scale = new MetaNumberParam<double>(150);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _textObject.SplitAtFrame(splitFrame);
            var firstText = firstClip as Text;
            var secondText = secondClip as Text;

            // Assert
            Assert.That(firstText.X.Get(0), Is.EqualTo(100));
            Assert.That(firstText.Y.Get(0), Is.EqualTo(200));
            Assert.That(firstText.Scale.Get(0), Is.EqualTo(150));
            Assert.That(secondText.X.Get(0), Is.EqualTo(100));
            Assert.That(secondText.Y.Get(0), Is.EqualTo(200));
            Assert.That(secondText.Scale.Get(0), Is.EqualTo(150));
        }

        /// <summary>
        /// 分割されたテキストオブジェクトが元オブジェクトから独立していることを確認するテスト
        /// 意図: ディープコピーが正しく行われ、元オブジェクトの変更が分割オブジェクトに影響しないことを検証
        /// 想定結果: 元オブジェクトを変更しても、分割されたオブジェクトのプロパティは変更されない
        /// </summary>
        [Test]
        public void SplitAtFrame_TextClipsAreIndependent_ModifyingOriginalDoesNotAffectSplits()
        {
            // Arrange
            _textObject.StartFrame = 10;
            _textObject.EndFrame = 100;
            _textObject.Contents = "Original Text";
            var (firstClip, secondClip) = _textObject.SplitAtFrame(50);
            var firstText = firstClip as Text;
            var secondText = secondClip as Text;

            // Act
            _textObject.Contents = "Modified Text";
            _textObject.Font = new MetaFontParam("New Font");

            // Assert
            Assert.That(firstText.Contents, Is.EqualTo("Original Text"));
            Assert.That(secondText.Contents, Is.EqualTo("Original Text"));
            Assert.That(firstText.Font.FamilyName, Is.Not.EqualTo("New Font"));
            Assert.That(secondText.Font.FamilyName, Is.Not.EqualTo("New Font"));
        }
    }
}

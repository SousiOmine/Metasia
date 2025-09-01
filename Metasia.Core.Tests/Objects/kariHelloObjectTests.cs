using NUnit.Framework;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class kariHelloObjectTests
    {
        private kariHelloObject _kariHelloObject;

        [SetUp]
        public void Setup()
        {
            _kariHelloObject = new kariHelloObject("kari-id");
        }

        [Test]
        public void Constructor_WithId_InitializesCorrectly()
        {
            // Arrange & Act
            var obj = new kariHelloObject("test-id");

            // Assert
            Assert.That(obj.Id, Is.EqualTo("test-id"));
            Assert.That(obj.X, Is.Not.Null);
            Assert.That(obj.Y, Is.Not.Null);
            Assert.That(obj.Scale, Is.Not.Null);
            Assert.That(obj.Alpha, Is.Not.Null);
            Assert.That(obj.Rotation, Is.Not.Null);
            Assert.That(obj.Volume, Is.EqualTo(100));
        }

        [Test]
        public void Constructor_WithoutParameters_InitializesWithDefaults()
        {
            // Arrange & Act
            var obj = new kariHelloObject();

            // Assert
            Assert.That(obj.X, Is.Null); // パラメータなしコンストラクタでは座標パラメータは初期化されない
            Assert.That(obj.Y, Is.Null);
            Assert.That(obj.Scale, Is.Null);
            Assert.That(obj.Alpha, Is.Null);
            Assert.That(obj.Rotation, Is.Null);
            Assert.That(obj.Volume, Is.EqualTo(100));
        }

        [Test]
        public void CoordinateParameters_HaveCorrectDefaultValues()
        {
            // Assert
            Assert.That(_kariHelloObject.X.Get(0), Is.EqualTo(0));
            Assert.That(_kariHelloObject.Y.Get(0), Is.EqualTo(0));
            Assert.That(_kariHelloObject.Scale.Get(0), Is.EqualTo(100));
            Assert.That(_kariHelloObject.Alpha.Get(0), Is.EqualTo(0));
            Assert.That(_kariHelloObject.Rotation.Get(0), Is.EqualTo(0));
        }

        [Test]
        public void Volume_CanBeModified()
        {
            // Arrange
            Assert.That(_kariHelloObject.Volume, Is.EqualTo(100)); // デフォルト値確認

            // Act
            _kariHelloObject.Volume = 50;

            // Assert
            Assert.That(_kariHelloObject.Volume, Is.EqualTo(50));
        }

        [Test]
        public void InheritedProperties_WorkCorrectly()
        {
            // kariHelloObjectはClipObjectを継承しているので、基本プロパティも確認
            Assert.That(_kariHelloObject.StartFrame, Is.EqualTo(0));
            Assert.That(_kariHelloObject.EndFrame, Is.EqualTo(100));
            Assert.That(_kariHelloObject.IsActive, Is.True);

            // 変更も可能
            _kariHelloObject.StartFrame = 10;
            _kariHelloObject.EndFrame = 200;
            _kariHelloObject.IsActive = false;

            Assert.That(_kariHelloObject.StartFrame, Is.EqualTo(10));
            Assert.That(_kariHelloObject.EndFrame, Is.EqualTo(200));
            Assert.That(_kariHelloObject.IsActive, Is.False);
        }

        [Test]
        public void ImplementsRequiredInterfaces()
        {
            // kariHelloObjectが必要なインターフェースを実装していることを確認
            Assert.That(_kariHelloObject, Is.InstanceOf<IAudible>());
            Assert.That(_kariHelloObject, Is.InstanceOf<IRenderable>());
        }

        /// <summary>
        /// kariHelloObjectを正常に分割できることを確認するテスト
        /// 意図: kariHelloObjectの分割機能が正しく動作し、基本プロパティが維持されることを検証
        /// 想定結果: 2つのkariHelloObjectが返され、ID、フレーム範囲、音量が正しく設定される
        /// </summary>
        [Test]
        public void SplitAtFrame_ValidSplitFrame_ReturnsTwoKariHelloObjectsWithCorrectProperties()
        {
            // Arrange
            _kariHelloObject.StartFrame = 10;
            _kariHelloObject.EndFrame = 100;
            _kariHelloObject.Volume = 60;
            _kariHelloObject.X = new MetaNumberParam<double>(_kariHelloObject, 150);
            _kariHelloObject.Y = new MetaNumberParam<double>(_kariHelloObject, 250);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _kariHelloObject.SplitAtFrame(splitFrame);
            var firstHello = firstClip as kariHelloObject;
            var secondHello = secondClip as kariHelloObject;

            // Assert
            Assert.That(firstHello, Is.Not.Null);
            Assert.That(secondHello, Is.Not.Null);
            Assert.That(firstHello.Id, Is.EqualTo("kari-id_part1"));
            Assert.That(secondHello.Id, Is.EqualTo("kari-id_part2"));
            Assert.That(firstHello.StartFrame, Is.EqualTo(10));
            Assert.That(firstHello.EndFrame, Is.EqualTo(49));
            Assert.That(secondHello.StartFrame, Is.EqualTo(50));
            Assert.That(secondHello.EndFrame, Is.EqualTo(100));
            Assert.That(firstHello.Volume, Is.EqualTo(60));
            Assert.That(secondHello.Volume, Is.EqualTo(60));
        }

        /// <summary>
        /// 分割時に座標パラメータが正しく維持されることを確認するテスト
        /// 意図: kariHelloObjectのMetaNumberParamプロパティが分割後も値を保持していることを検証
        /// 想定結果: 分割された両方のオブジェクトでX、Y、Scale、Alpha、Rotation座標パラメータが元の値を維持
        /// </summary>
        [Test]
        public void SplitAtFrame_PreservesCoordinateParameters()
        {
            // Arrange
            _kariHelloObject.StartFrame = 10;
            _kariHelloObject.EndFrame = 100;
            _kariHelloObject.X = new MetaNumberParam<double>(_kariHelloObject, 100);
            _kariHelloObject.Y = new MetaNumberParam<double>(_kariHelloObject, 200);
            _kariHelloObject.Scale = new MetaNumberParam<double>(_kariHelloObject, 150);
            _kariHelloObject.Alpha = new MetaNumberParam<double>(_kariHelloObject, 25);
            _kariHelloObject.Rotation = new MetaNumberParam<double>(_kariHelloObject, 45);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _kariHelloObject.SplitAtFrame(splitFrame);
            var firstHello = firstClip as kariHelloObject;
            var secondHello = secondClip as kariHelloObject;

            // Assert
            Assert.That(firstHello.X.Get(0), Is.EqualTo(100));
            Assert.That(firstHello.Y.Get(0), Is.EqualTo(200));
            Assert.That(firstHello.Scale.Get(0), Is.EqualTo(150));
            Assert.That(firstHello.Alpha.Get(0), Is.EqualTo(25));
            Assert.That(firstHello.Rotation.Get(0), Is.EqualTo(45));
            
            Assert.That(secondHello.X.Get(0), Is.EqualTo(100));
            Assert.That(secondHello.Y.Get(0), Is.EqualTo(200));
            Assert.That(secondHello.Scale.Get(0), Is.EqualTo(150));
            Assert.That(secondHello.Alpha.Get(0), Is.EqualTo(25));
            Assert.That(secondHello.Rotation.Get(0), Is.EqualTo(45));
        }

        /// <summary>
        /// 分割時に音響効果が正しく維持されることを確認するテスト
        /// 意図: kariHelloObject分割後、音響効果が両方のオブジェクトにコピーされることを検証
        /// 想定結果: 分割された両方のオブジェクトで音響効果リストに1つの効果が保持される
        /// </summary>
        [Test]
        public void SplitAtFrame_PreservesAudioEffects()
        {
            // Arrange
            _kariHelloObject.StartFrame = 10;
            _kariHelloObject.EndFrame = 100;
            var effect = new Metasia.Core.Objects.AudioEffects.VolumeFadeEffect();
            _kariHelloObject.AudioEffects.Add(effect);
            var splitFrame = 50;

            // Act
            var (firstClip, secondClip) = _kariHelloObject.SplitAtFrame(splitFrame);
            var firstHello = firstClip as kariHelloObject;
            var secondHello = secondClip as kariHelloObject;

            // Assert
            Assert.That(firstHello.AudioEffects.Count, Is.EqualTo(1));
            Assert.That(secondHello.AudioEffects.Count, Is.EqualTo(1));
            Assert.That(firstHello.AudioEffects[0], Is.InstanceOf<Metasia.Core.Objects.AudioEffects.VolumeFadeEffect>());
            Assert.That(secondHello.AudioEffects[0], Is.InstanceOf<Metasia.Core.Objects.AudioEffects.VolumeFadeEffect>());
        }

        /// <summary>
        /// 分割されたkariHelloObjectが元オブジェクトから独立していることを確認するテスト
        /// 意図: ディープコピーが正しく行われ、元オブジェクトの変更が分割オブジェクトに影響しないことを検証
        /// 想定結果: 元オブジェクトを変更しても、分割されたオブジェクトの音量と音響効果は変更されない
        /// </summary>
        [Test]
        public void SplitAtFrame_KariHelloObjectsAreIndependent_ModifyingOriginalDoesNotAffectSplits()
        {
            // Arrange
            _kariHelloObject.StartFrame = 10;
            _kariHelloObject.EndFrame = 100;
            _kariHelloObject.Volume = 60;
            var effect = new Metasia.Core.Objects.AudioEffects.VolumeFadeEffect();
            _kariHelloObject.AudioEffects.Add(effect);
            var (firstClip, secondClip) = _kariHelloObject.SplitAtFrame(50);
            var firstHello = firstClip as kariHelloObject;
            var secondHello = secondClip as kariHelloObject;

            // Act
            _kariHelloObject.Volume = 30;
            _kariHelloObject.AudioEffects.Clear();

            // Assert
            Assert.That(firstHello.Volume, Is.EqualTo(60));
            Assert.That(secondHello.Volume, Is.EqualTo(60));
            Assert.That(firstHello.AudioEffects.Count, Is.GreaterThan(0));
            Assert.That(secondHello.AudioEffects.Count, Is.GreaterThan(0));
        }
    }
} 
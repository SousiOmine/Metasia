using NUnit.Framework;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Moq;

namespace Metasia.Core.Tests.Coordinate
{
    [TestFixture]
    public class MetaFloatParamTests
    {
        private MetaFloatParam _metaFloatParam;
        private Mock<MetasiaObject> _mockOwner;

        [SetUp]
        public void Setup()
        {
            _mockOwner = new Mock<MetasiaObject>("test-id");
            _mockOwner.Object.StartFrame = 0;
        }

        [Test]
        public void Constructor_DefaultConstructor_InitializesEmptyParams()
        {
            // Act
            var param = new MetaFloatParam();

            // Assert
            Assert.That(param.Params, Is.Not.Null);
            Assert.That(param.Params, Is.Empty);
        }

        [Test]
        public void Constructor_WithOwnerAndInitialValue_InitializesCorrectly()
        {
            // Arrange
            const float initialValue = 123.456f;

            // Act
            var param = new MetaFloatParam(_mockOwner.Object, initialValue);

            // Assert
            Assert.That(param.Params, Is.Not.Null);
            Assert.That(param.Params.Count, Is.EqualTo(1));
            Assert.That(param.Params[0].Value, Is.EqualTo(initialValue).Within(0.001));
            Assert.That(param.Params[0].Frame, Is.EqualTo(0));
        }

        [Test]
        public void Get_WithSinglePoint_ReturnsPointValue()
        {
            // Arrange
            const float expectedValue = 100.0f;
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, expectedValue);

            // Act
            var result = _metaFloatParam.Get(50);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue).Within(0.001f));
        }

        [Test]
        public void Get_WithMultiplePoints_CalculatesLinearInterpolation()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 0.0f);
            _metaFloatParam.Params.Add(new CoordPoint { Frame = 100, Value = 200.0 });

            // Act - フレーム50では線形補間で100.0になるはず
            var result = _metaFloatParam.Get(50);

            // Assert
            Assert.That(result, Is.EqualTo(100.0f).Within(0.001f));
        }

        [Test]
        public void Get_BeforeFirstPoint_ReturnsFirstPointValue()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 50.0f);
            _metaFloatParam.Params[0].Frame = 10; // 最初のポイントをフレーム10に設定

            // Act - フレーム5（最初のポイントより前）
            var result = _metaFloatParam.Get(5);

            // Assert
            Assert.That(result, Is.EqualTo(50.0f).Within(0.001f));
        }

        [Test]
        public void Get_AfterLastPoint_ReturnsLastPointValue()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 10.0f);
            _metaFloatParam.Params.Add(new CoordPoint { Frame = 50, Value = 100.0 });

            // Act - フレーム200（最後のポイントより後）
            var result = _metaFloatParam.Get(200);

            // Assert
            Assert.That(result, Is.EqualTo(100.0f).Within(0.001f));
        }

        [Test]
        public void Get_WithOwnerStartFrame_AdjustsFrameCorrectly()
        {
            // Arrange
            _mockOwner.Object.StartFrame = 100;
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 0.0f);
            _metaFloatParam.Params.Add(new CoordPoint { Frame = 50, Value = 150.0 });

            // Act - 絶対フレーム150は、オブジェクトの相対フレーム50
            var result = _metaFloatParam.Get(150);

            // Assert
            Assert.That(result, Is.EqualTo(150.0f).Within(0.001f));
        }

        [Test]
        public void Get_WithInvalidJSLogic_ReturnsFallbackValue()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 42.0f);
            _metaFloatParam.Params[0].JSLogic = "invalid javascript code {{{";

            // Act
            var result = _metaFloatParam.Get(10);

            // Assert - 無効なJSの場合、startPointの値を返すはず
            Assert.That(result, Is.EqualTo(42.0f).Within(0.001f));
        }

        [Test]
        public void Get_WithCustomJSLogic_ExecutesCorrectly()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 10.0f);
            _metaFloatParam.Params[0].JSLogic = "StartValue * 2";
            
            // Act
            var result = _metaFloatParam.Get(0);

            // Assert
            Assert.That(result, Is.EqualTo(20.0f).Within(0.001f));
        }

        [Test]
        public void Get_WithNullOwner_HandlesGracefully()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(null, 123.0f);

            // Act & Assert - 例外が発生しないことを確認
            Assert.DoesNotThrow(() => _metaFloatParam.Get(50));
        }

        [Test]
        public void Params_CanBeModifiedDirectly()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 0.0f);
            var newPoint = new CoordPoint { Frame = 75, Value = 175.0 };

            // Act
            _metaFloatParam.Params.Add(newPoint);

            // Assert
            Assert.That(_metaFloatParam.Params.Count, Is.EqualTo(2));
            Assert.That(_metaFloatParam.Params.Contains(newPoint), Is.True);
        }

        [Test]
        public void Get_WithUnsortedParams_SortsAutomatically()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 0.0f);
            _metaFloatParam.Params.Add(new CoordPoint { Frame = 100, Value = 200.0 });
            _metaFloatParam.Params.Add(new CoordPoint { Frame = 50, Value = 100.0 });

            // Act - フレーム75では、50と100の間の補間値になるはず
            var result = _metaFloatParam.Get(75);

            // Assert - 線形補間で150.0になる
            Assert.That(result, Is.EqualTo(150.0f).Within(0.001f));
        }

        [Test]
        public void Get_WithDoubleToFloatCasting_HandlesCorrectly()
        {
            // Arrange
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, 0.0f);
            // doubleの値を設定
            _metaFloatParam.Params.Add(new CoordPoint { Frame = 50, Value = 999.999 });

            // Act
            var result = _metaFloatParam.Get(100);

            // Assert - doubleからfloatへの変換が正しく行われることを確認
            Assert.That(result, Is.EqualTo(999.999f).Within(0.001f));
        }

        [TestCase(0.0f)]
        [TestCase(123.456f)]
        [TestCase(-999.999f)]
        [TestCase(float.MaxValue)]
        [TestCase(float.MinValue)]
        public void Constructor_WithVariousFloatValues_WorksCorrectly(float value)
        {
            // Act
            var param = new MetaFloatParam(_mockOwner.Object, value);

            // Assert
            Assert.That(param.Params[0].Value, Is.EqualTo(value).Within(0.001));
        }

        [Test]
        public void Get_WithEqualStartAndEndValues_ReturnsConstantValue()
        {
            // Arrange
            const float constantValue = 42.5f;
            _metaFloatParam = new MetaFloatParam(_mockOwner.Object, constantValue);
            _metaFloatParam.Params.Add(new CoordPoint { Frame = 100, Value = constantValue });

            // Act & Assert - 任意のフレームで同じ値が返されることを確認
            Assert.That(_metaFloatParam.Get(25), Is.EqualTo(constantValue).Within(0.001f));
            Assert.That(_metaFloatParam.Get(50), Is.EqualTo(constantValue).Within(0.001f));
            Assert.That(_metaFloatParam.Get(75), Is.EqualTo(constantValue).Within(0.001f));
        }
    }
} 
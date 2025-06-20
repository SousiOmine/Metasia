using NUnit.Framework;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;

namespace Metasia.Core.Tests.Coordinate
{
    [TestFixture]
    public class MetaDoubleParamTests
    {
        private MetaDoubleParam _metaDoubleParam;
        private MetasiaObject _owner;

        [SetUp]
        public void Setup()
        {
            _owner = new MetasiaObject("test-id");
            _owner.StartFrame = 0;
        }

        [Test]
        public void Constructor_DefaultConstructor_InitializesEmptyParams()
        {
            // Act
            var param = new MetaDoubleParam();

            // Assert
            Assert.That(param.Params, Is.Not.Null);
            Assert.That(param.Params, Is.Empty);
        }

        [Test]
        public void Constructor_WithOwnerAndInitialValue_InitializesCorrectly()
        {
            // Arrange
            const double initialValue = 123.456;

            // Act
            var param = new MetaDoubleParam(_owner, initialValue);

            // Assert
            Assert.That(param.Params, Is.Not.Null);
            Assert.That(param.Params.Count, Is.EqualTo(1));
            Assert.That(param.Params[0].Value, Is.EqualTo(initialValue));
            Assert.That(param.Params[0].Frame, Is.EqualTo(0));
        }

        [Test]
        public void Get_WithSinglePoint_ReturnsPointValue()
        {
            // Arrange
            const double expectedValue = 100.0;
            _metaDoubleParam = new MetaDoubleParam(_owner, expectedValue);

            // Act
            var result = _metaDoubleParam.Get(50);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Get_WithMultiplePoints_CalculatesLinearInterpolation()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 0.0);
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 100, Value = 200.0 });

            // Act - フレーム50では線形補間で100.0になるはず
            var result = _metaDoubleParam.Get(50);

            // Assert
            Assert.That(result, Is.EqualTo(100.0).Within(0.001));
        }

        [Test]
        public void Get_BeforeFirstPoint_ReturnsFirstPointValue()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 50.0);
            _metaDoubleParam.Params[0].Frame = 10; // 最初のポイントをフレーム10に設定

            // Act - フレーム5（最初のポイントより前）
            var result = _metaDoubleParam.Get(5);

            // Assert
            Assert.That(result, Is.EqualTo(50.0));
        }

        [Test]
        public void Get_AfterLastPoint_ReturnsLastPointValue()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 10.0);
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 50, Value = 100.0 });

            // Act - フレーム200（最後のポイントより後）
            var result = _metaDoubleParam.Get(200);

            // Assert
            Assert.That(result, Is.EqualTo(100.0));
        }

        [Test]
        public void Get_WithOwnerStartFrame_AdjustsFrameCorrectly()
        {
            // Arrange
            _owner.StartFrame = 100;
            _metaDoubleParam = new MetaDoubleParam(_owner, 0.0);
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 50, Value = 150.0 });

            // Act - 絶対フレーム150は、オブジェクトの相対フレーム50
            var result = _metaDoubleParam.Get(150);

            // Assert
            Assert.That(result, Is.EqualTo(150.0));
        }

        [Test]
        public void Get_WithInvalidJSLogic_ReturnsFallbackValue()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 42.0);
            _metaDoubleParam.Params[0].JSLogic = "invalid javascript code {{{";

            // Act
            var result = _metaDoubleParam.Get(10);

            // Assert - 無効なJSの場合、startPointの値を返すはず
            Assert.That(result, Is.EqualTo(42.0));
        }

        [Test]
        public void Get_WithCustomJSLogic_ExecutesCorrectly()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 10.0);
            _metaDoubleParam.Params[0].JSLogic = "StartValue * 2";
            
            // Act
            var result = _metaDoubleParam.Get(0);

            // Assert
            Assert.That(result, Is.EqualTo(20.0));
        }

        [Test]
        public void Get_WithNullOwner_HandlesGracefully()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(null!, 123.0);

            // Act & Assert - 例外が発生しないことを確認
            Assert.DoesNotThrow(() => _metaDoubleParam.Get(50));
        }

        [Test]
        public void Params_CanBeModifiedDirectly()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 0.0);
            var newPoint = new CoordPoint { Frame = 75, Value = 175.0 };

            // Act
            _metaDoubleParam.Params.Add(newPoint);

            // Assert
            Assert.That(_metaDoubleParam.Params.Count, Is.EqualTo(2));
            Assert.That(_metaDoubleParam.Params.Contains(newPoint), Is.True);
        }

        [Test]
        public void Get_WithUnsortedParams_SortsAutomatically()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 0.0);
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 100, Value = 200.0 });
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 50, Value = 100.0 });

            // Act - フレーム75では、50と100の間の補間値になるはず
            var result = _metaDoubleParam.Get(75);

            // Assert - 線形補間で150.0になる
            Assert.That(result, Is.EqualTo(150.0).Within(0.001));
        }

        [Test]
        public void Get_WithComplexJSLogic_CalculatesCorrectly()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 0.0);
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 100, Value = 100.0 });
            // カスタム補間式（二次補間）
            _metaDoubleParam.Params[0].JSLogic = 
                "var t = (NowFrame - StartFrame) / (EndFrame - StartFrame);" +
                "StartValue + (EndValue - StartValue) * t * t";

            // Act - フレーム50では二次補間で25.0になるはず
            var result = _metaDoubleParam.Get(50);

            // Assert
            Assert.That(result, Is.EqualTo(25.0).Within(0.001));
        }

        [Test]
        public void Get_WithExactFrameMatch_ReturnsExactValue()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 10.0);
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 50, Value = 100.0 });
            _metaDoubleParam.Params.Add(new CoordPoint { Frame = 100, Value = 200.0 });

            // Act - 正確にフレーム50
            var result = _metaDoubleParam.Get(50);

            // Assert
            Assert.That(result, Is.EqualTo(100.0));
        }

        [Test]
        public void Get_WithNegativeFrame_HandlesCorrectly()
        {
            // Arrange
            _owner.StartFrame = 50;
            _metaDoubleParam = new MetaDoubleParam(_owner, 42.0);

            // Act - 絶対フレーム30は、相対フレーム-20
            var result = _metaDoubleParam.Get(30);

            // Assert - 最初のポイントの値を返す
            Assert.That(result, Is.EqualTo(42.0));
        }

        [Test]
        public void Get_WithEmptyJSLogic_UsesFallback()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 50.0);
            _metaDoubleParam.Params[0].JSLogic = "";

            // Act
            var result = _metaDoubleParam.Get(0);

            // Assert - 空のJSロジックの場合、startPointの値を返す
            Assert.That(result, Is.EqualTo(50.0));
        }

        [Test]
        public void Get_WithJSExceptionDuringExecution_ReturnsFallback()
        {
            // Arrange
            _metaDoubleParam = new MetaDoubleParam(_owner, 75.0);
            // 実行時に例外を発生させるJSコード
            _metaDoubleParam.Params[0].JSLogic = "throw new Error('Test error');";

            // Act
            var result = _metaDoubleParam.Get(0);

            // Assert
            Assert.That(result, Is.EqualTo(75.0));
        }

        [TestCase(double.MinValue)]
        [TestCase(-999999.999)]
        [TestCase(0.0)]
        [TestCase(0.0000001)]
        [TestCase(999999.999)]
        [TestCase(double.MaxValue)]
        public void Constructor_WithExtremeValues_HandlesCorrectly(double value)
        {
            // Act
            var param = new MetaDoubleParam(_owner, value);

            // Assert
            Assert.That(param.Params[0].Value, Is.EqualTo(value));
            Assert.DoesNotThrow(() => param.Get(0));
        }
    }
} 
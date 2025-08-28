using NUnit.Framework;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using System;

namespace Metasia.Core.Tests.Coordinate
{
    public class MetaNumberParamTests
    {
        #region コンストラクタのテスト

        // デフォルトコンストラクタが、空のParamsリストを持つインスタンスを正しく生成することを確認する
        [Test]
        public void Constructor_Default_ShouldInitializeParamsAsEmptyList()
        {
            // Act
            var param = new MetaNumberParam<double>();

            // Assert
            Assert.NotNull(param);
            Assert.NotNull(param.Params);
            Assert.That(param.Params, Is.Empty);
        }

        // パラメータ付きコンストラクタが、所有者と初期値を正しく設定することを確認する
        [Test]
        public void Constructor_WithOwnerAndInitialValue_ShouldInitializeCorrectly()
        {
            // Arrange
            var owner = new ClipObject("test_owner");
            double initialValue = 10.0;

            // Act
            var param = new MetaNumberParam<double>(owner, initialValue);

            // Assert
            Assert.NotNull(param);
            Assert.NotNull(param.Params);
            Assert.That(param.Params, Has.Count.EqualTo(1));
            Assert.That(param.Params[0].Value, Is.EqualTo(initialValue));
            Assert.That(param.Params[0].Frame, Is.EqualTo(0)); // CoordPointのデフォルトFrame値
            // ownerObjectはprotectedなので直接アクセスできないが、後続のテストで間接的に確認
        }

        #endregion

        #region Getメソッドのテスト

        // Paramsが空の場合にInvalidOperationExceptionがスローされることを確認する
        [Test]
        public void Get_WithEmptyParams_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var param = new MetaNumberParam<double>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => param.Get(0));
        }

        // Paramsに1つのポイントのみ存在する場合、その値が常に返されることを確認する (ownerObjectがnullの場合)
        [Test]
        public void Get_WithSinglePointAndNullOwner_ShouldReturnThePointValue()
        {
            // Arrange
            double initialValue = 10.0;
            var param = new MetaNumberParam<double>(null, initialValue);

            // Act & Assert
            Assert.That(param.Get(0), Is.EqualTo(10.0));
            Assert.That(param.Get(10), Is.EqualTo(10.0));
            Assert.That(param.Get(100), Is.EqualTo(10.0));
        }

        // Paramsに1つのポイントのみ存在する場合、その値が常に返されることを確認する (ownerObjectが指定されている場合)
        [Test]
        public void Get_WithSinglePointAndOwner_ShouldReturnThePointValue()
        {
            // Arrange
            var owner = new ClipObject("test_owner") { StartFrame = 100 };
            double initialValue = 10.0;
            var param = new MetaNumberParam<double>(owner, initialValue);

            // Act & Assert
            // owner.StartFrame=100なので、frame=100が実際のframe=0に相当
            Assert.That(param.Get(100), Is.EqualTo(10.0));
            Assert.That(param.Get(110), Is.EqualTo(10.0));
            Assert.That(param.Get(200), Is.EqualTo(10.0));
        }

        // 指定フレームがすべてのキーフレームより前にある場合、最初のキーフレームの値が返されることを確認する
        [Test]
        public void Get_FrameBeforeAllPoints_ShouldReturnFirstPointValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(); // 初期ポイントを追加
            param.Params.Add(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.Params.Add(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            double result = param.Get(5);

            // Assert
            Assert.That(result, Is.EqualTo(10.0)); // startPointとendPointが最初のポイントになる
        }

        // 指定フレームがすべてのキーフレームより後にある場合、最後のキーフレームの値が返されることを確認する
        [Test]
        public void Get_FrameAfterAllPoints_ShouldReturnLastPointValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0); // 初期ポイントを追加
            param.Params.Add(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.Params.Add(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            double result = param.Get(25);

            // Assert
            Assert.That(result, Is.EqualTo(20.0)); // startPointとendPointが最後のポイントになる
        }

        // 指定フレームが2つのキーフレーム間にある場合、デフォルトのJSロジック（線形補間）で正しく値が計算されることを確認する
        [Test]
        public void Get_FrameBetweenPoints_ShouldReturnInterpolatedValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0); // 初期ポイントを追加
            param.Params.Add(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.Params.Add(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            double result = param.Get(15); // 中間フレーム

            // Assert
            Assert.That(result, Is.EqualTo(15.0)); // 線形補間: 10.0 + (20.0-10.0) * (15-10)/(20-10) = 15.0
        }

        // ownerObject.StartFrameによるフレーム調整が正しく機能することを確認する
        [Test]
        public void Get_WithOwnerStartFrame_ShouldAdjustFrameCorrectly()
        {
            // Arrange
            var owner = new ClipObject("test_owner") { StartFrame = 100 };
            var param = new MetaNumberParam<double>(owner, 10.0); // owner基準Frame=0に相当
            param.Params.Add(new CoordPoint() { Frame = 10, Value = 110.0 }); // owner基準Frame=10に相当

            // Act
            double result1 = param.Get(100); // owner基準Frame=0
            double result2 = param.Get(110); // owner基準Frame=10

            // Assert
            Assert.That(result1, Is.EqualTo(10.0)); // 初期値
            Assert.That(result2, Is.EqualTo(110.0)); // 追加したポイントの値
        }

        // Paramsがフレーム順にソートされていない状態でも、正しくソートされ正しい値が計算されることを確認する
        [Test]
        public void Get_WithUnsortedParams_ShouldSortAndReturnCorrectValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0); // 初期ポイントを追加
            // 意図的に逆順に追加
            param.Params.Add(new CoordPoint() { Frame = 20, Value = 20.0 });
            param.Params.Add(new CoordPoint() { Frame = 10, Value = 10.0 });

            // Act
            double result = param.Get(15);

            // Assert
            Assert.That(result, Is.EqualTo(15.0)); // ソートが正しく機能し、線形補間が行われる
        }

        // JavaScriptロジック実行時の例外がキャッチされ、startPoint.Valueがフォールバック値として返されることを確認する
        [Test]
        public void Get_WithInvalidJSLogic_ShouldReturnStartPointValueAsFallback()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0); // 初期ポイントを追加
            var startPoint = new CoordPoint() { Frame = 10, Value = 10.0, JSLogic = "invalid script syntax" };
            var endPoint = new CoordPoint() { Frame = 20, Value = 20.0 };
            param.Params.Add(startPoint);
            param.Params.Add(endPoint);

            // Act
            double result = param.Get(15);

            // Assert
            Assert.That(result, Is.EqualTo(10.0)); // JS実行エラー時はstartPoint.Valueを返す
        }

        // ジェネリック型Tがintの場合にも、型変換が正しく行われることを確認する
        [Test]
        public void Get_WithIntGenericType_ShouldReturnCorrectIntValue()
        {
            // Arrange
            var param = new MetaNumberParam<int>(null, 0); // int型のパラメータ
            param.Params.Add(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.Params.Add(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            int result = param.Get(15); // 中間フレーム

            // Assert
            Assert.That(result, Is.EqualTo(15)); // doubleの15.0がintの15に変換される
        }

        #endregion
    }
}

using NUnit.Framework;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate.InterpolationLogic;
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

        // パラメータ付きコンストラクタが、初期値を正しく設定することを確認する
        [Test]
        public void Constructor_WithInitialValue_ShouldInitializeCorrectly()
        {
            // Arrange
            double initialValue = 10.0;

            // Act
            var param = new MetaNumberParam<double>(initialValue);

            // Assert
            Assert.NotNull(param);
            Assert.NotNull(param.Params);
            Assert.That(param.Params, Has.Count.EqualTo(1));
            Assert.That(param.Params[0].Value, Is.EqualTo(initialValue));
            Assert.That(param.Params[0].Frame, Is.EqualTo(0)); // CoordPointのデフォルトFrame値
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

        // Paramsに1つのポイントのみ存在する場合、その値が常に返されることを確認する
        [Test]
        public void Get_WithSinglePoint_ShouldReturnThePointValue()
        {
            // Arrange
            double initialValue = 10.0;
            var param = new MetaNumberParam<double>(initialValue);

            // Act & Assert
            Assert.That(param.Get(0), Is.EqualTo(10.0));
            Assert.That(param.Get(10), Is.EqualTo(10.0));
            Assert.That(param.Get(100), Is.EqualTo(10.0));
        }

        // Paramsに1つのポイントのみ存在する場合、その値が常に返されることを確認する
        [Test]
        public void Get_WithSinglePointAtDifferentFrame_ShouldReturnThePointValue()
        {
            // Arrange
            double initialValue = 10.0;
            var param = new MetaNumberParam<double>(initialValue);
            param.AddPoint(new CoordPoint { Frame = 50, Value = 20.0 });

            // Act & Assert
            Assert.That(param.Get(0), Is.EqualTo(10.0));
            Assert.That(param.Get(25), Is.EqualTo(15.0)); // 線形補間
            Assert.That(param.Get(50), Is.EqualTo(20.0));
            Assert.That(param.Get(75), Is.EqualTo(20.0)); // 最後のポイントの値
        }

        // 指定フレームがすべてのキーフレームより前にある場合、線形補間された値が返されることを確認する
        [Test]
        public void Get_FrameBeforeAllPoints_ShouldReturnInterpolatedValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(5.0); // 初期ポイントを追加
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            double result = param.Get(5);

            // Assert
            Assert.That(result, Is.EqualTo(7.5)); // 線形補間された値: 5.0 + (10.0-5.0) * (5-0)/(10-0) = 7.5
        }

        // 指定フレームがすべてのキーフレームより後にある場合、最後のキーフレームの値が返されることを確認する
        [Test]
        public void Get_FrameAfterAllPoints_ShouldReturnLastPointValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(0.0); // 初期ポイントを追加
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            double result = param.Get(25);

            // Assert
            Assert.That(result, Is.EqualTo(20.0)); // 最後のポイントの値
        }

        // 指定フレームが2つのキーフレーム間にある場合、デフォルトの線形補間で正しく値が計算されることを確認する
        [Test]
        public void Get_FrameBetweenPoints_ShouldReturnInterpolatedValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(0.0); // 初期ポイントを追加
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            double result = param.Get(15); // 中間フレーム

            // Assert
            Assert.That(result, Is.EqualTo(15.0)); // 線形補間: 10.0 + (20.0-10.0) * (15-10)/(20-10) = 15.0
        }

        // 異なるフレーム位置のポイント間で正しく補間されることを確認する
        [Test]
        public void Get_WithMultiplePoints_ShouldInterpolateCorrectly()
        {
            // Arrange
            var param = new MetaNumberParam<double>(0.0); // 初期ポイントを追加
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 100.0 });
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 200.0 });

            // Act
            double result1 = param.Get(0);   // 初期値
            double result2 = param.Get(5);   // 中間補間
            double result3 = param.Get(10);  // 最初の追加ポイント
            double result4 = param.Get(15);  // 2番目の補間
            double result5 = param.Get(20);  // 2番目の追加ポイント

            // Assert
            Assert.That(result1, Is.EqualTo(0.0));   // 初期値
            Assert.That(result2, Is.EqualTo(50.0));  // 0.0 + (100.0-0.0) * (5-0)/(10-0) = 50.0
            Assert.That(result3, Is.EqualTo(100.0)); // 最初の追加ポイント
            Assert.That(result4, Is.EqualTo(150.0)); // 100.0 + (200.0-100.0) * (15-10)/(20-10) = 150.0
            Assert.That(result5, Is.EqualTo(200.0)); // 2番目の追加ポイント
        }

        // Paramsがフレーム順にソートされていない状態でも、正しくソートされ正しい値が計算されることを確認する
        [Test]
        public void Get_WithUnsortedParams_ShouldSortAndReturnCorrectValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(0.0); // 初期ポイントを追加
            // 意図的に逆順に追加
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 20.0 });
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 10.0 });

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
            var param = new MetaNumberParam<double>(0.0); // 初期ポイントを追加
            var startPoint = new CoordPoint() { Frame = 10, Value = 10.0 };
            startPoint.InterpolationLogic = new JavaScriptLogic() { JSLogic = "invalid script syntax" };
            var endPoint = new CoordPoint() { Frame = 20, Value = 20.0 };
            param.AddPoint(startPoint);
            param.AddPoint(endPoint);

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
            var param = new MetaNumberParam<int>(0); // int型のパラメータ
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 20.0 });

            // Act
            int result = param.Get(15);

            // Assert
            Assert.That(result, Is.EqualTo(15)); // 線形補間された値
        }

        // 同じフレームに複数のポイントがある場合、最初のポイントの値が返されることを確認する
        [Test]
        public void Get_WithMultiplePointsSameFrame_ShouldReturnFirstPointValue()
        {
            // Arrange
            var param = new MetaNumberParam<double>(0.0);
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.AddPoint(new CoordPoint() { Frame = 10, Value = 20.0 }); // 同じフレームの後から追加されたポイント

            // Act
            double result = param.Get(10);

            // Assert
            Assert.That(result, Is.EqualTo(10.0)); // 最初のポイントの値
        }

        #endregion

        #region Splitメソッドのテスト

        // 分割フレームに既存のポイントがある場合、そのポイントの補間ロジックが正しくコピーされることを確認する
        [Test]
        public void Split_WithExistingPointAtSplitFrame_ShouldCopyInterpolationLogic()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            var jsLogic = new JavaScriptLogic() { JSLogic = "return StartValue + (EndValue - StartValue) * 0.5;" };
            param.AddPoint(new CoordPoint() { Frame = 50, Value = 20.0, InterpolationLogic = jsLogic });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            // 前半のポイント数を確認
            Assert.That(firstHalf.Params.Count, Is.GreaterThan(0));
            // 後半のポイント数を確認
            Assert.That(secondHalf.Params.Count, Is.GreaterThan(0));
            // 分割フレーム位置の補間ロジックがコピーされていることを確認
            Assert.That(firstHalf.Params.Last().InterpolationLogic, Is.Not.Null);
            Assert.That(secondHalf.Params.First().InterpolationLogic, Is.Not.Null);
        }

        // 分割フレームにポイントがない場合、最も近い前方のポイントの補間ロジックが正しくコピーされることを確認する
        [Test]
        public void Split_WithoutPointAtSplitFrame_ShouldCopyNearestInterpolationLogic()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            var jsLogic = new JavaScriptLogic() { JSLogic = "return StartValue + (EndValue - StartValue) * 0.5;" };
            param.AddPoint(new CoordPoint() { Frame = 40, Value = 15.0, InterpolationLogic = jsLogic });
            param.AddPoint(new CoordPoint() { Frame = 60, Value = 25.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            // 前半のポイント数を確認
            Assert.That(firstHalf.Params.Count, Is.GreaterThan(0));
            // 後半のポイント数を確認
            Assert.That(secondHalf.Params.Count, Is.GreaterThan(0));
            // 最も近い前方のポイントの補間ロジックがコピーされていることを確認
            Assert.That(firstHalf.Params.Last().InterpolationLogic, Is.Not.Null);
            Assert.That(secondHalf.Params.First().InterpolationLogic, Is.Not.Null);
        }

        // 前半部分が正しく分割されることを確認する
        [Test]
        public void Split_FirstHalf_ShouldContainCorrectPoints()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 15.0 });
            param.AddPoint(new CoordPoint() { Frame = 40, Value = 20.0 });
            param.AddPoint(new CoordPoint() { Frame = 60, Value = 25.0 });
            param.AddPoint(new CoordPoint() { Frame = 80, Value = 30.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            // 前半部分には分割フレームより前のポイントが含まれることを確認
            Assert.That(firstHalf.Params.Count, Is.EqualTo(4)); // 初期ポイント + 20, 40, 50のポイント
            Assert.That(firstHalf.Params[0].Value, Is.EqualTo(10.0)); // 初期値
            Assert.That(firstHalf.Params[1].Value, Is.EqualTo(15.0)); // Frame=20
            Assert.That(firstHalf.Params[2].Value, Is.EqualTo(20.0)); // Frame=40
            Assert.That(firstHalf.Params[3].Frame, Is.EqualTo(49)); // 境界ポイント (50-1)
        }

        // 後半部分が正しく分割されることを確認する
        [Test]
        public void Split_SecondHalf_ShouldContainCorrectPoints()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            param.AddPoint(new CoordPoint() { Frame = 20, Value = 15.0 });
            param.AddPoint(new CoordPoint() { Frame = 40, Value = 20.0 });
            param.AddPoint(new CoordPoint() { Frame = 60, Value = 25.0 });
            param.AddPoint(new CoordPoint() { Frame = 80, Value = 30.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            // 後半部分には分割フレーム以降のポイントが含まれることを確認
            Assert.That(secondHalf.Params.Count, Is.EqualTo(3)); // 境界ポイント(0) + 60, 80のポイント
            Assert.That(secondHalf.Params[0].Frame, Is.EqualTo(0)); // 境界ポイント
            Assert.That(secondHalf.Params[0].Value, Is.EqualTo(22.5)); // 分割フレームでの補間値 (40:20.0 と 60:25.0 の間)
            Assert.That(secondHalf.Params[1].Frame, Is.EqualTo(10)); // 60 - 50
            Assert.That(secondHalf.Params[1].Value, Is.EqualTo(25.0));
            Assert.That(secondHalf.Params[2].Frame, Is.EqualTo(30)); // 80 - 50
            Assert.That(secondHalf.Params[2].Value, Is.EqualTo(30.0));
        }

        // int型のパラメータを分割した場合、型変換が正しく行われることを確認する
        [Test]
        public void Split_WithIntGenericType_ShouldConvertTypeCorrectly()
        {
            // Arrange
            var param = new MetaNumberParam<int>(10);
            param.AddPoint(new CoordPoint() { Frame = 50, Value = 20.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            Assert.That(firstHalf, Is.InstanceOf<MetaNumberParam<int>>());
            Assert.That(secondHalf, Is.InstanceOf<MetaNumberParam<int>>());
            // 値が正しくint型に変換されていることを確認
            Assert.That(firstHalf.Get(0), Is.EqualTo(10));
            Assert.That(secondHalf.Get(0), Is.EqualTo(20)); // 初期値10と20.0の補間値
        }

        #endregion

        #region 新しいメソッドのテスト

        // AddPointメソッドが正しくポイントを追加することを確認する
        [Test]
        public void AddPoint_ShouldAddPointCorrectly()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            var point = new CoordPoint() { Frame = 50, Value = 20.0 };

            // Act
            param.AddPoint(point);

            // Assert
            Assert.That(param.Params.Count, Is.EqualTo(2));
            Assert.That(param.Params[1].Frame, Is.EqualTo(50));
            Assert.That(param.Params[1].Value, Is.EqualTo(20.0));
        }

        // RemovePointメソッドが正しくポイントを削除することを確認する
        [Test]
        public void RemovePoint_ShouldRemovePointCorrectly()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            var point = new CoordPoint() { Frame = 50, Value = 20.0 };
            param.AddPoint(point);

            // Act
            bool result = param.RemovePoint(point);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(param.Params.Count, Is.EqualTo(1)); // 初期ポイントのみ残る
            Assert.That(param.Params[0].Value, Is.EqualTo(10.0));
        }

        // ポイントが1つしかない場合、RemovePointがfalseを返すことを確認する
        [Test]
        public void RemovePoint_WithSinglePoint_ShouldReturnFalse()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);

            // Act
            bool result = param.RemovePoint(param.Params[0]);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(param.Params.Count, Is.EqualTo(1));
        }

        // UpdatePointメソッドが正しくポイントを更新することを確認する
        [Test]
        public void UpdatePoint_ShouldUpdatePointCorrectly()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            var originalPoint = new CoordPoint() { Frame = 50, Value = 20.0 };
            param.AddPoint(originalPoint);

            var updatedPoint = new CoordPoint() { Id = originalPoint.Id, Frame = 50, Value = 30.0 };

            // Act
            bool result = param.UpdatePoint(updatedPoint);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(param.Params[1].Value, Is.EqualTo(30.0));
            Assert.That(param.Params[1].Frame, Is.EqualTo(50));
        }

        // 存在しないポイントを更新しようとするとfalseを返すことを確認する
        [Test]
        public void UpdatePoint_WithNonExistentPoint_ShouldReturnFalse()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            var nonExistentPoint = new CoordPoint() { Id = "non-existent-id", Frame = 50, Value = 20.0 };

            // Act
            bool result = param.UpdatePoint(nonExistentPoint);

            // Assert
            Assert.That(result, Is.False);
        }

        // SetSinglePointメソッドが正しく単一のポイントを設定することを確認する
        [Test]
        public void SetSinglePoint_ShouldSetSinglePointCorrectly()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);
            param.AddPoint(new CoordPoint() { Frame = 50, Value = 20.0 });
            param.AddPoint(new CoordPoint() { Frame = 100, Value = 30.0 });

            // Act
            param.SetSinglePoint(25.0);

            // Assert
            Assert.That(param.Params.Count, Is.EqualTo(1));
            Assert.That(param.Params[0].Value, Is.EqualTo(25.0));
            Assert.That(param.Params[0].Frame, Is.EqualTo(0));
        }

        // AddPointが自動的にソートされることを確認する
        [Test]
        public void AddPoint_ShouldSortPointsAutomatically()
        {
            // Arrange
            var param = new MetaNumberParam<double>(10.0);

            // Act - 逆順で追加
            param.AddPoint(new CoordPoint() { Frame = 100, Value = 30.0 });
            param.AddPoint(new CoordPoint() { Frame = 50, Value = 20.0 });
            param.AddPoint(new CoordPoint() { Frame = 25, Value = 15.0 });

            // Assert - フレーム順にソートされていることを確認
            Assert.That(param.Params.Count, Is.EqualTo(4));
            Assert.That(param.Params[0].Frame, Is.EqualTo(0));   // 初期ポイント
            Assert.That(param.Params[1].Frame, Is.EqualTo(25));
            Assert.That(param.Params[2].Frame, Is.EqualTo(50));
            Assert.That(param.Params[3].Frame, Is.EqualTo(100));
        }

        #endregion
    }
}

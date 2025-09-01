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

        #region Splitメソッドのテスト

        // 要件1: 基本機能 - 指定フレームで2つのパラメータに分割できること
        [Test]
        public void Split_BasicFunctionality_ShouldCreateTwoParams()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 10.0);

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            Assert.NotNull(firstHalf);
            Assert.NotNull(secondHalf);
            Assert.That(firstHalf.Params, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(secondHalf.Params, Has.Count.GreaterThanOrEqualTo(1));
        }

        // 要件2: ポイントの分配ルール - 前半は分割フレーム未満、後半は分割フレーム以上を含むこと
        [Test]
        public void Split_PointDistribution_ShouldDistributePointsCorrectly()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0);
            param.Params.Add(new CoordPoint() { Frame = 10, Value = 10.0 });
            param.Params.Add(new CoordPoint() { Frame = 30, Value = 30.0 });
            param.Params.Add(new CoordPoint() { Frame = 60, Value = 60.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(40);

            // Assert - 前半は分割フレーム(40)未満のポイントを含む
            Assert.That(firstHalf.Params.All(p => p.Frame < 40), Is.True);
            Assert.That(firstHalf.Params.Any(p => p.Frame == 0), Is.True); // 初期値
            Assert.That(firstHalf.Params.Any(p => p.Frame == 10), Is.True);
            Assert.That(firstHalf.Params.Any(p => p.Frame == 30), Is.True);
            Assert.That(firstHalf.Params.Any(p => p.Frame == 39), Is.True); // 境界ポイント
            
            // Assert - 後半は調整されたフレームで分割フレーム以上のポイントを含む
            Assert.That(secondHalf.Params.All(p => p.Frame >= 0), Is.True);
            Assert.That(secondHalf.Params.Any(p => p.Frame == 0), Is.True); // 境界ポイント
            Assert.That(secondHalf.Params.Any(p => p.Frame == 20), Is.True); // 60-40=20
        }

        // 要件3: 境界ポイントの追加条件 - 必要な場合のみ境界ポイントが追加されること
        [Test]
        public void Split_BoundaryPointAddition_ShouldAddBoundaryPointsOnlyWhenNeeded()
        {
            // Arrange - 分割フレームに既にポイントが存在する場合
            var param1 = new MetaNumberParam<double>(null, 0.0);
            param1.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param1.Params.Add(new CoordPoint() { Frame = 50, Value = 50.0 }); // 分割フレームにポイントあり
            param1.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf1, secondHalf1) = param1.Split(50);

            // Assert - 前半は分割フレーム未満なので境界ポイントが追加される
            Assert.That(firstHalf1.Params.Count(p => p.Frame == 49), Is.EqualTo(1)); // 境界ポイント
            Assert.That(secondHalf1.Params.Count(p => p.Frame == 0), Is.EqualTo(1)); // 境界ポイント

            // Arrange - 分割フレームにポイントが存在しない場合
            var param2 = new MetaNumberParam<double>(null, 0.0);
            param2.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param2.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf2, secondHalf2) = param2.Split(50);

            // Assert - 境界ポイントが追加される
            Assert.That(firstHalf2.Params.Any(p => p.Frame == 49), Is.True); // 境界ポイント
            Assert.That(secondHalf2.Params.Any(p => p.Frame == 0), Is.True);
        }

        // 要件4: JSロジックの保持 - 既存ポイントのJSロジックが保持されること
        [Test]
        public void Split_JSLogicPreservation_ShouldPreserveExistingJSLogic()
        {
            // Arrange
            var param = new MetaNumberParam<double>();
            var customLogic = "custom unique logic string";
            
            // 最初のポイントにカスタムロジックを設定
            param.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0, JSLogic = customLogic });
            // 2番目のポイントにはデフォルトロジックを使用
            param.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert - 最初のポイントのJSロジックが保持されている
            Assert.That(firstHalf.Params[0].JSLogic, Does.Contain("custom unique logic"));
        }

        // 要件4: JSロジックの保持 - 境界ポイントのJSロジックが適切に設定されること
        [Test]
        public void Split_BoundaryPointJSLogic_ShouldUseNearestPointLogic()
        {
            // Arrange - 分割フレーム位置にポイントがある場合
            var param1 = new MetaNumberParam<double>(null, 0.0);
            var customLogic = "custom logic";
            param1.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param1.Params.Add(new CoordPoint() { Frame = 50, Value = 50.0, JSLogic = customLogic });

            // Act
            var (firstHalf1, secondHalf1) = param1.Split(50);

            // Assert - 分割フレーム位置のポイントのJSロジックを境界ポイントに使用
            Assert.That(firstHalf1.Params.Last(p => p.Frame == 49).JSLogic, Is.EqualTo(customLogic));
            Assert.That(secondHalf1.Params.First(p => p.Frame == 0).JSLogic, Is.EqualTo(customLogic));

            // Arrange - 分割フレーム位置にポイントがない場合
            var param2 = new MetaNumberParam<double>(null, 0.0);
            param2.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0, JSLogic = customLogic });
            param2.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf2, secondHalf2) = param2.Split(50);

            // Assert - 最も近い前方ポイントのJSロジックを使用
            Assert.That(firstHalf2.Params.Last(p => p.Frame == 49).JSLogic, Is.EqualTo(customLogic));
            Assert.That(secondHalf2.Params.First(p => p.Frame == 0).JSLogic, Is.EqualTo(customLogic));
        }

        // 要件5: 値の連続性 - 分割位置での値が連続していること
        [Test]
        public void Split_ValueContinuity_ShouldMaintainContinuityAtSplitPoint()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0);
            param.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            double firstHalfEndValue = firstHalf.Get(49); // 前半の最終フレーム
            double secondHalfStartValue = secondHalf.Get(0);
            
            Assert.That(firstHalfEndValue, Is.EqualTo(49.0)); // 0-100の線形補間でframe 49
            Assert.That(secondHalfStartValue, Is.EqualTo(50.0)); // 境界ポイントの値
            // 値は連続している（前半の最終値と後半の開始値が近い）
        }

        // 要件6: 不変性 - 元のパラメータが変更されないこと
        [Test]
        public void Split_Immutability_ShouldNotModifyOriginalParam()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0);
            param.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });
            int originalCount = param.Params.Count;
            double originalValue = param.Get(25);

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert - 元のパラメータは変更されていない
            Assert.That(param.Params, Has.Count.EqualTo(originalCount));
            Assert.That(param.Get(25), Is.EqualTo(originalValue));
        }

        // 要件7: エッジケース - 空のParamsの場合
        [Test]
        public void Split_EmptyParams_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var param = new MetaNumberParam<double>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => param.Split(50));
        }

        // 要件7: エッジケース - 分割フレームがすべてのポイントの前
        [Test]
        public void Split_SplitFrameBeforeAllPoints_ShouldCreateEmptyFirstHalf()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 10.0);
            param.Params.Add(new CoordPoint() { Frame = 20, Value = 20.0 });
            param.Params.Add(new CoordPoint() { Frame = 40, Value = 40.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(10);

            // Assert
            Assert.That(firstHalf.Params.Count, Is.EqualTo(2)); // 0と10の境界ポイント
            Assert.That(secondHalf.Params.Count, Is.EqualTo(3)); // 0(境界), 10(20-10), 30(40-10)
        }

        // 要件7: エッジケース - 分割フレームがすべてのポイントの後
        [Test]
        public void Split_SplitFrameAfterAllPoints_ShouldCreateEmptySecondHalf()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 10.0);
            param.Params.Add(new CoordPoint() { Frame = 20, Value = 20.0 });
            param.Params.Add(new CoordPoint() { Frame = 40, Value = 40.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            Assert.That(firstHalf.Params.Count, Is.EqualTo(4)); // 0, 20, 40, 50(境界)
            Assert.That(secondHalf.Params.Count, Is.EqualTo(1)); // 0の境界ポイントのみ
        }

        // 要件7: エッジケース - 分割フレームが既存ポイントと一致
        [Test]
        public void Split_SplitFrameAtExistingPoint_ShouldIncludePointOnlyInSecondHalf()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0);
            param.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param.Params.Add(new CoordPoint() { Frame = 50, Value = 50.0 }); // 分割位置
            param.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert - 前半は分割フレーム未満、後半は分割フレーム以上
            Assert.That(firstHalf.Params.All(p => p.Frame < 50), Is.True);
            Assert.That(firstHalf.Params.Any(p => p.Frame == 49), Is.True); // 境界ポイント
            Assert.That(secondHalf.Params.Any(p => p.Frame == 0), Is.True); // 境界ポイント（50-50=0）
        }

        // 追加: ジェネリック型対応
        [Test]
        public void Split_GenericTypeSupport_ShouldWorkWithDifferentTypes()
        {
            // Arrange
            var param = new MetaNumberParam<int>(null, 0);
            param.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert
            int firstHalfEndValue = firstHalf.Get(49); // 前半の最終フレーム
            int secondHalfStartValue = secondHalf.Get(0);
            
            Assert.That(firstHalfEndValue, Is.EqualTo(49)); // 0-100の線形補間でframe 49
            Assert.That(secondHalfStartValue, Is.EqualTo(50)); // 境界ポイントの値
        }

        // 追加: 補間計算の正確性
        [Test]
        public void Split_InterpolationAccuracy_ShouldMaintainCorrectInterpolation()
        {
            // Arrange
            var param = new MetaNumberParam<double>(null, 0.0);
            param.Params.Add(new CoordPoint() { Frame = 0, Value = 0.0 });
            param.Params.Add(new CoordPoint() { Frame = 100, Value = 100.0 });

            // Act
            var (firstHalf, secondHalf) = param.Split(50);

            // Assert - 分割後も補間が正しく機能する
            Assert.That(firstHalf.Get(25), Is.EqualTo(25.0)); // 0-49の範囲で25
            Assert.That(secondHalf.Get(25), Is.EqualTo(75.0)); // 50-100の範囲で25 = 全体で75
            Assert.That(firstHalf.Get(10), Is.EqualTo(10.0));
            Assert.That(secondHalf.Get(40), Is.EqualTo(90.0)); // 50+40=90
        }

        #endregion
    }
}

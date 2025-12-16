using NUnit.Framework;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Core.Tests.Objects.Parameters;

[TestFixture]
public class MetaDoubleParamTests
{
    #region コンストラクタのテスト

    [Test]
    public void Constructor_Default_ShouldInitializeWithZero()
    {
        // Act
        var param = new MetaDoubleParam();

        // Assert
        Assert.That(param.Value, Is.EqualTo(0.0));
    }

    [Test]
    public void Constructor_WithValue_ShouldInitializeCorrectly()
    {
        // Arrange
        double expectedValue = 42.5;

        // Act
        var param = new MetaDoubleParam(expectedValue);

        // Assert
        Assert.That(param.Value, Is.EqualTo(expectedValue));
    }

    #endregion

    #region 値の取得と設定のテスト

    [Test]
    public void Value_CanBeSet_AndRetrieved()
    {
        // Arrange
        var param = new MetaDoubleParam();
        double expectedValue = 123.456;

        // Act
        param.Value = expectedValue;

        // Assert
        Assert.That(param.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public void Value_CanBeModified_ThroughReference()
    {
        // Arrange
        var param = new MetaDoubleParam(10.0);

        // Act - 参照を通じて値を変更
        ModifyValue(param);

        // Assert
        Assert.That(param.Value, Is.EqualTo(20.0));
    }

    private void ModifyValue(MetaDoubleParam param)
    {
        param.Value = 20.0;
    }

    #endregion

    #region Splitメソッドのテスト

    [Test]
    public void Split_ShouldReturnTwoParamsWithSameValue()
    {
        // Arrange
        var param = new MetaDoubleParam(100.0);

        // Act
        var (firstHalf, secondHalf) = param.Split(50);

        // Assert
        Assert.That(firstHalf, Is.Not.Null);
        Assert.That(secondHalf, Is.Not.Null);
        Assert.That(firstHalf.Value, Is.EqualTo(100.0));
        Assert.That(secondHalf.Value, Is.EqualTo(100.0));
    }

    [Test]
    public void Split_ShouldCreateNewInstances()
    {
        // Arrange
        var param = new MetaDoubleParam(50.0);

        // Act
        var (firstHalf, secondHalf) = param.Split(25);

        // Assert - 異なるインスタンスであることを確認
        Assert.That(firstHalf, Is.Not.SameAs(param));
        Assert.That(secondHalf, Is.Not.SameAs(param));
        Assert.That(firstHalf, Is.Not.SameAs(secondHalf));
    }

    [Test]
    public void Split_ModifyingSplitResults_ShouldNotAffectOriginal()
    {
        // Arrange
        var param = new MetaDoubleParam(100.0);

        // Act
        var (firstHalf, secondHalf) = param.Split(50);
        firstHalf.Value = 200.0;
        secondHalf.Value = 300.0;

        // Assert
        Assert.That(param.Value, Is.EqualTo(100.0)); // 元の値は変更されていない
        Assert.That(firstHalf.Value, Is.EqualTo(200.0));
        Assert.That(secondHalf.Value, Is.EqualTo(300.0));
    }

    #endregion

    #region 暗黙的な型変換のテスト

    [Test]
    public void ImplicitConversion_FromMetaDoubleParamToDouble_ShouldWork()
    {
        // Arrange
        var param = new MetaDoubleParam(42.0);

        // Act
        double value = param; // 暗黙的な型変換

        // Assert
        Assert.That(value, Is.EqualTo(42.0));
    }

    [Test]
    public void ImplicitConversion_FromDoubleToMetaDoubleParam_ShouldWork()
    {
        // Arrange
        double value = 123.45;

        // Act
        MetaDoubleParam param = value; // 暗黙的な型変換

        // Assert
        Assert.That(param.Value, Is.EqualTo(123.45));
    }

    [Test]
    public void ImplicitConversion_WithNull_ShouldReturnZero()
    {
        // Arrange
        MetaDoubleParam? param = null;

        // Act
        double value = param!; // 暗黙的な型変換（null許容性を明示的に処理）

        // Assert
        Assert.That(value, Is.EqualTo(0.0));
    }

    [Test]
    public void ImplicitConversion_AllowsDoubleOperations()
    {
        // Arrange
        var param = new MetaDoubleParam(10.0);

        // Act
        double result = param + 5.0; // doubleとして使用

        // Assert
        Assert.That(result, Is.EqualTo(15.0));
    }

    #endregion

    #region ToString/Equals/GetHashCodeのテスト

    [Test]
    public void ToString_ShouldReturnValueAsString()
    {
        // Arrange
        var param = new MetaDoubleParam(42.5);

        // Act
        string result = param.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("42.5"));
    }

    [Test]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var param1 = new MetaDoubleParam(100.0);
        var param2 = new MetaDoubleParam(100.0);

        // Act & Assert
        Assert.That(param1.Equals(param2), Is.True);
    }

    [Test]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var param1 = new MetaDoubleParam(100.0);
        var param2 = new MetaDoubleParam(200.0);

        // Act & Assert
        Assert.That(param1.Equals(param2), Is.False);
    }

    [Test]
    public void Equals_WithDoubleValue_ShouldReturnTrue()
    {
        // Arrange
        var param = new MetaDoubleParam(100.0);
        double value = 100.0;

        // Act & Assert
        Assert.That(param.Equals(value), Is.True);
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var param = new MetaDoubleParam(100.0);

        // Act & Assert
        Assert.That(param.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_SamValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var param1 = new MetaDoubleParam(100.0);
        var param2 = new MetaDoubleParam(100.0);

        // Act
        int hash1 = param1.GetHashCode();
        int hash2 = param2.GetHashCode();

        // Assert
        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var param1 = new MetaDoubleParam(100.0);
        var param2 = new MetaDoubleParam(200.0);

        // Act
        int hash1 = param1.GetHashCode();
        int hash2 = param2.GetHashCode();

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    #endregion

    #region 実用的なシナリオのテスト

    [Test]
    public void UseCase_EditCommand_CanModifyThroughReference()
    {
        // Arrange - EditCommandで編集するシナリオをシミュレート
        var param = new MetaDoubleParam(50.0);
        var originalValue = param.Value;

        // Act - EditCommandによる変更
        ExecuteEditCommand(param, 75.0);

        // Assert
        Assert.That(param.Value, Is.Not.EqualTo(originalValue));
        Assert.That(param.Value, Is.EqualTo(75.0));
    }

    private void ExecuteEditCommand(MetaDoubleParam param, double newValue)
    {
        // EditCommandのExecuteメソッドをシミュレート
        param.Value = newValue;
    }

    [Test]
    public void UseCase_UndoRedo_CanRestoreValue()
    {
        // Arrange
        var param = new MetaDoubleParam(100.0);
        double oldValue = param.Value;

        // Act - Do
        param.Value = 200.0;
        double newValue = param.Value;

        // Act - Undo
        param.Value = oldValue;

        // Assert
        Assert.That(param.Value, Is.EqualTo(100.0));

        // Act - Redo
        param.Value = newValue;

        // Assert
        Assert.That(param.Value, Is.EqualTo(200.0));
    }

    #endregion
}

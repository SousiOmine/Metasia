using NUnit.Framework;
using SkiaSharp;
using Metasia.Core.Render;

namespace Metasia.Core.Tests.Render;

[TestFixture]
public class BlendModeParamTests
{
    #region コンストラクタのテスト

    [Test]
    public void Constructor_Default_ShouldInitializeWithSrcOver()
    {
        var param = new BlendModeParam();

        Assert.That(param.Value, Is.EqualTo(BlendModeKind.SrcOver));
    }

    [Test]
    public void Constructor_WithValue_ShouldInitializeCorrectly()
    {
        var param = new BlendModeParam(BlendModeKind.Multiply);

        Assert.That(param.Value, Is.EqualTo(BlendModeKind.Multiply));
    }

    #endregion

    #region 値の取得と設定のテスト

    [Test]
    public void Value_CanBeSet_AndRetrieved()
    {
        var param = new BlendModeParam();
        param.Value = BlendModeKind.Screen;

        Assert.That(param.Value, Is.EqualTo(BlendModeKind.Screen));
    }

    [Test]
    public void Value_CanBeSetToAllBlendModes()
    {
        var param = new BlendModeParam();

        foreach (var mode in BlendModeParam.AllOptions)
        {
            param.Value = mode;
            Assert.That(param.Value, Is.EqualTo(mode), $"Failed for mode: {mode}");
        }
    }

    #endregion

    #region AllOptionsのテスト

    [Test]
    public void AllOptions_ShouldContainWorkingBlendModeKinds()
    {
        Assert.That(BlendModeParam.AllOptions.Count, Is.EqualTo(18));
    }

    [Test]
    public void AllOptions_ShouldContainSrcOver()
    {
        Assert.That(BlendModeParam.AllOptions, Contains.Item(BlendModeKind.SrcOver));
    }

    [Test]
    public void AllOptions_ShouldContainMultiply()
    {
        Assert.That(BlendModeParam.AllOptions, Contains.Item(BlendModeKind.Multiply));
    }

    [Test]
    public void AllOptions_ShouldContainScreen()
    {
        Assert.That(BlendModeParam.AllOptions, Contains.Item(BlendModeKind.Screen));
    }

    [Test]
    public void AllOptions_ShouldContainOverlay()
    {
        Assert.That(BlendModeParam.AllOptions, Contains.Item(BlendModeKind.Overlay));
    }

    #endregion

    #region ToSkBlendModeのテスト

    [Test]
    public void ToSkBlendMode_SrcOver_ShouldReturnSkSrcOver()
    {
        var param = new BlendModeParam(BlendModeKind.SrcOver);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.SrcOver));
    }

    [Test]
    public void ToSkBlendMode_Multiply_ShouldReturnSkMultiply()
    {
        var param = new BlendModeParam(BlendModeKind.Multiply);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.Multiply));
    }

    [Test]
    public void ToSkBlendMode_Screen_ShouldReturnSkScreen()
    {
        var param = new BlendModeParam(BlendModeKind.Screen);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.Screen));
    }

    [Test]
    public void ToSkBlendMode_Overlay_ShouldReturnSkOverlay()
    {
        var param = new BlendModeParam(BlendModeKind.Overlay);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.Overlay));
    }

    [Test]
    public void ToSkBlendMode_Darken_ShouldReturnSkDarken()
    {
        var param = new BlendModeParam(BlendModeKind.Darken);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.Darken));
    }

    [Test]
    public void ToSkBlendMode_Lighten_ShouldReturnSkLighten()
    {
        var param = new BlendModeParam(BlendModeKind.Lighten);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.Lighten));
    }

    [Test]
    public void ToSkBlendMode_ColorDodge_ShouldReturnSkColorDodge()
    {
        var param = new BlendModeParam(BlendModeKind.ColorDodge);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.ColorDodge));
    }

    [Test]
    public void ToSkBlendMode_Plus_ShouldReturnSkPlus()
    {
        var param = new BlendModeParam(BlendModeKind.Plus);

        Assert.That(param.ToSkBlendMode(), Is.EqualTo(SKBlendMode.Plus));
    }

    [Test]
    public void ToSkBlendMode_AllModes_ShouldMapCorrectly()
    {
        foreach (var mode in BlendModeParam.AllOptions)
        {
            var param = new BlendModeParam(mode);
            var skMode = param.ToSkBlendMode();

            Assert.That(skMode.ToString(), Is.EqualTo(mode.ToString()), $"Mismatch for mode: {mode}");
        }
    }

    #endregion

    #region Splitメソッドのテスト

    [Test]
    public void Split_ShouldReturnTwoParamsWithSameValue()
    {
        var param = new BlendModeParam(BlendModeKind.Multiply);

        var (firstHalf, secondHalf) = param.Split();

        Assert.That(firstHalf, Is.Not.Null);
        Assert.That(secondHalf, Is.Not.Null);
        Assert.That(firstHalf.Value, Is.EqualTo(BlendModeKind.Multiply));
        Assert.That(secondHalf.Value, Is.EqualTo(BlendModeKind.Multiply));
    }

    [Test]
    public void Split_ShouldCreateNewInstances()
    {
        var param = new BlendModeParam(BlendModeKind.Screen);

        var (firstHalf, secondHalf) = param.Split();

        Assert.That(firstHalf, Is.Not.SameAs(param));
        Assert.That(secondHalf, Is.Not.SameAs(param));
        Assert.That(firstHalf, Is.Not.SameAs(secondHalf));
    }

    [Test]
    public void Split_ModifyingSplitResults_ShouldNotAffectOriginal()
    {
        var param = new BlendModeParam(BlendModeKind.Overlay);

        var (firstHalf, secondHalf) = param.Split();
        firstHalf.Value = BlendModeKind.Darken;
        secondHalf.Value = BlendModeKind.Lighten;

        Assert.That(param.Value, Is.EqualTo(BlendModeKind.Overlay));
        Assert.That(firstHalf.Value, Is.EqualTo(BlendModeKind.Darken));
        Assert.That(secondHalf.Value, Is.EqualTo(BlendModeKind.Lighten));
    }

    #endregion

    #region 暗黙的な型変換のテスト

    [Test]
    public void ImplicitConversion_FromBlendModeParamToBlendModeKind_ShouldWork()
    {
        var param = new BlendModeParam(BlendModeKind.Screen);

        BlendModeKind value = param;

        Assert.That(value, Is.EqualTo(BlendModeKind.Screen));
    }

    [Test]
    public void ImplicitConversion_FromBlendModeKindToBlendModeParam_ShouldWork()
    {
        BlendModeParam param = BlendModeKind.Overlay;

        Assert.That(param.Value, Is.EqualTo(BlendModeKind.Overlay));
    }

    [Test]
    public void ImplicitConversion_WithNull_ShouldReturnSrcOver()
    {
        BlendModeParam? param = null;

        BlendModeKind value = param!;

        Assert.That(value, Is.EqualTo(BlendModeKind.SrcOver));
    }

    #endregion

    #region ToString/Equals/GetHashCodeのテスト

    [Test]
    public void ToString_ShouldReturnValueAsString()
    {
        var param = new BlendModeParam(BlendModeKind.Multiply);

        Assert.That(param.ToString(), Is.EqualTo("Multiply"));
    }

    [Test]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var param1 = new BlendModeParam(BlendModeKind.Screen);
        var param2 = new BlendModeParam(BlendModeKind.Screen);

        Assert.That(param1.Equals(param2), Is.True);
    }

    [Test]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var param1 = new BlendModeParam(BlendModeKind.Screen);
        var param2 = new BlendModeParam(BlendModeKind.Overlay);

        Assert.That(param1.Equals(param2), Is.False);
    }

    [Test]
    public void Equals_WithBlendModeKind_ShouldReturnTrue()
    {
        var param = new BlendModeParam(BlendModeKind.Multiply);

        Assert.That(param.Equals(BlendModeKind.Multiply), Is.True);
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var param = new BlendModeParam(BlendModeKind.SrcOver);

        Assert.That(param.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_SameValues_ShouldReturnSameHashCode()
    {
        var param1 = new BlendModeParam(BlendModeKind.Screen);
        var param2 = new BlendModeParam(BlendModeKind.Screen);

        Assert.That(param1.GetHashCode(), Is.EqualTo(param2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHashCode()
    {
        var param1 = new BlendModeParam(BlendModeKind.Screen);
        var param2 = new BlendModeParam(BlendModeKind.Overlay);

        Assert.That(param1.GetHashCode(), Is.Not.EqualTo(param2.GetHashCode()));
    }

    #endregion

    #region SerializableValueのテスト

    [Test]
    public void SerializableValue_Get_ShouldReturnStringRepresentation()
    {
        var param = new BlendModeParam(BlendModeKind.Multiply);

        Assert.That(param.SerializableValue, Is.EqualTo("Multiply"));
    }

    [Test]
    public void SerializableValue_Set_ShouldUpdateValue()
    {
        var param = new BlendModeParam();
        param.SerializableValue = "Screen";

        Assert.That(param.Value, Is.EqualTo(BlendModeKind.Screen));
    }

    [Test]
    public void SerializableValue_SetWithInvalidValue_ShouldAssignDefaultValue()
    {
        var param = new BlendModeParam(BlendModeKind.SrcOver);
        param.SerializableValue = "InvalidMode";

        Assert.That(param.Value, Is.EqualTo(BlendModeKind.SrcOver));
    }

    #endregion
}

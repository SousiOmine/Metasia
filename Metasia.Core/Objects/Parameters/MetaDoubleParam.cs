using System.Xml.Serialization;

namespace Metasia.Core.Objects.Parameters;

/// <summary>
/// double型のパラメータを表すクラス
/// アニメーション機能はなく、参照型として値を保持する
/// EditCommandによる編集に適した構造
/// </summary>
public class MetaDoubleParam
{
    /// <summary>
    /// 値
    /// </summary>
    [XmlAttribute("Value")]
    public double Value { get; set; }

    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public MetaDoubleParam()
    {
        Value = 0.0;
    }

    /// <summary>
    /// 値を指定するコンストラクタ
    /// </summary>
    /// <param name="value">初期値</param>
    public MetaDoubleParam(double value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定されたフレームでパラメータを2つに分割する
    /// アニメーション機能がないため、同じ値を持つ2つのパラメータを返す
    /// </summary>
    /// <param name="splitFrame">分割フレーム位置（使用されない）</param>
    /// <returns>前半部分と後半部分の2つのMetaDoubleParam</returns>
    public (MetaDoubleParam FirstHalf, MetaDoubleParam SecondHalf) Split(int splitFrame)
    {
        var firstHalf = new MetaDoubleParam(Value);
        var secondHalf = new MetaDoubleParam(Value);
        return (firstHalf, secondHalf);
    }

    /// <summary>
    /// MetaDoubleParamからdoubleへの暗黙的な型変換
    /// </summary>
    public static implicit operator double(MetaDoubleParam param)
    {
        return param?.Value ?? 0.0;
    }

    /// <summary>
    /// doubleからMetaDoubleParamへの暗黙的な型変換
    /// </summary>
    public static implicit operator MetaDoubleParam(double value)
    {
        return new MetaDoubleParam(value);
    }

    /// <summary>
    /// 文字列表現
    /// </summary>
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    /// 等値比較
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is MetaDoubleParam other)
        {
            return Value.Equals(other.Value);
        }
        if (obj is double doubleValue)
        {
            return Value.Equals(doubleValue);
        }
        return false;
    }

    /// <summary>
    /// ハッシュコード取得
    /// </summary>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

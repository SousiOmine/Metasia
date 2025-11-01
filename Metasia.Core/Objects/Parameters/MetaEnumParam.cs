using System.Xml.Serialization;
using System.ComponentModel;

namespace Metasia.Core.Objects.Parameters;

/// <summary>
/// 列挙型のパラメータを表すクラス
/// アニメーション機能はなし
/// </summary>
public class MetaEnumParam
{
    /// <summary>
    /// 選択可能な値のリスト
    /// </summary>
    [XmlIgnore]
    public IReadOnlyList<string> Options => _options.AsReadOnly();

    /// <summary>
    /// シリアライズ専用の選択可能な値のリスト
    /// </summary>
    [XmlArray("Options")]
    [XmlArrayItem("Option")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public List<string> SerializableOptions
    {
        get => _options;
        set => _options = value ?? new List<string>();
    }

    /// <summary>
    /// 現在選択されている値のインデックス
    /// </summary>
    public int SelectedIndex { get; set; } = 0;

    /// <summary>
    /// 現在選択されている値
    /// </summary>
    [XmlIgnore]
    public string SelectedValue
    {
        get => _options.Count > SelectedIndex && SelectedIndex >= 0 ? _options[SelectedIndex] : string.Empty;
        set
        {
            int index = _options.IndexOf(value);
            if (index >= 0)
            {
                SelectedIndex = index;
            }
        }
    }

    /// <summary>
    /// シリアライズ用の選択値
    /// </summary>
    [XmlElement("SelectedValue")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string SerializableSelectedValue
    {
        get => SelectedValue;
        set => SelectedValue = value;
    }

    private List<string> _options = new();

    public MetaEnumParam()
    {
        _options = new();
    }

    public MetaEnumParam(params string[] options)
    {
        _options = options.ToList();
        SelectedIndex = 0;
    }

    public MetaEnumParam(IEnumerable<string> options, int selectedIndex = 0)
    {
        _options = options.ToList();
        SelectedIndex = Math.Max(0, Math.Min(selectedIndex, _options.Count - 1));
    }

    /// <summary>
    /// 指定されたフレームでパラメータを2つに分割する
    /// アニメーション機能がないため、同じ値を持つ2つのパラメータを返す
    /// </summary>
    /// <param name="splitFrame">分割フレーム位置（使用されない）</param>
    /// <returns>前半部分と後半部分の2つのMetaEnumParam</returns>
    public (MetaEnumParam FirstHalf, MetaEnumParam SecondHalf) Split(int splitFrame)
    {
        var firstHalf = new MetaEnumParam(_options, SelectedIndex);
        var secondHalf = new MetaEnumParam(_options, SelectedIndex);
        return (firstHalf, secondHalf);
    }
}

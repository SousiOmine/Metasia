using System;
using System.Xml.Serialization;
using SkiaSharp;

namespace Metasia.Core.Typography;

[Serializable]
public class MetaFontParam : IEquatable<MetaFontParam>
{
    private static readonly string DefaultFamily = DetermineDefaultFamily();

    /// <summary>
    /// フォントファミリ名
    /// </summary>
    public string FamilyName { get; set; } = DefaultFamily;

    /// <summary>
    /// 太字フラグ
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// イタリックフラグ
    /// </summary>
    public bool IsItalic { get; set; }

    [XmlIgnore]
    public static MetaFontParam Default => new();

    public MetaFontParam()
    {
        FamilyName = DefaultFamily;
        IsBold = false;
        IsItalic = false;
    }

    public MetaFontParam(string familyName, bool isBold = false, bool isItalic = false)
    {
        FamilyName = string.IsNullOrWhiteSpace(familyName) ? DefaultFamily : familyName;
        IsBold = isBold;
        IsItalic = isItalic;
    }

    public MetaFontParam Clone()
    {
        return new MetaFontParam(FamilyName, IsBold, IsItalic);
    }

    private static string DetermineDefaultFamily()
    {
        var defaultFamily = SKTypeface.Default?.FamilyName;
        if (!string.IsNullOrWhiteSpace(defaultFamily))
        {
            return defaultFamily;
        }

        if (OperatingSystem.IsWindows())
        {
            return "Yu Gothic UI";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "Hiragino Sans";
        }

        if (OperatingSystem.IsLinux())
        {
            return "Noto Sans";
        }

        return "Arial";
    }

    public SKTypeface ResolveTypeface(Func<SKTypeface> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);

        var style = new SKFontStyle(
            IsBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            IsItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

        try
        {
            var typeface = SKTypeface.FromFamilyName(FamilyName, style);
            if (typeface != null)
            {
                return typeface;
            }
        }
        catch
        {
            // フォント取得に失敗した場合はフォールバックへ
        }

        return fallbackFactory();
    }

    public bool Equals(MetaFontParam? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(FamilyName, other.FamilyName, StringComparison.OrdinalIgnoreCase)
               && IsBold == other.IsBold
               && IsItalic == other.IsItalic;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MetaFontParam);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            FamilyName?.ToUpperInvariant(),
            IsBold,
            IsItalic);
    }
}

using System.ComponentModel;
using System.Xml.Serialization;
using SkiaSharp;

namespace Metasia.Core.Render
{
    public enum BlendModeKind
    {
        Clear,
        Src,
        Dst,
        SrcOver,
        DstOver,
        SrcIn,
        DstIn,
        SrcOut,
        DstOut,
        SrcATop,
        DstATop,
        Xor,
        Plus,
        Modulate,
        Screen,
        Overlay,
        Darken,
        Lighten,
        ColorDodge,
        ColorBurn,
        HardLight,
        SoftLight,
        Difference,
        Exclusion,
        Multiply,
        Hue,
        Saturation,
        Color,
        Luminosity
    }

    public class BlendModeParam
    {
        private static readonly IReadOnlyList<BlendModeKind> _allOptions = new List<BlendModeKind>
        {
            BlendModeKind.SrcOver,
            BlendModeKind.DstIn,
            BlendModeKind.DstOut,
            BlendModeKind.Plus,
            BlendModeKind.Screen,
            BlendModeKind.Overlay,
            BlendModeKind.Darken,
            BlendModeKind.Lighten,
            BlendModeKind.ColorDodge,
            BlendModeKind.HardLight,
            BlendModeKind.SoftLight,
            BlendModeKind.Difference,
            BlendModeKind.Exclusion,
            BlendModeKind.Multiply,
            BlendModeKind.Hue,
            BlendModeKind.Saturation,
            BlendModeKind.Color,
            BlendModeKind.Luminosity
        }.AsReadOnly();

        public static IReadOnlyList<BlendModeKind> AllOptions => _allOptions;

        public BlendModeKind Value { get; set; } = BlendModeKind.SrcOver;

        [XmlAttribute("Value")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SerializableValue
        {
            get => Value.ToString();
            set
            {
                if (Enum.TryParse<BlendModeKind>(value, out var result))
                {
                    Value = result;
                }
                else
                {
                    Value = BlendModeKind.SrcOver;
                }
            }
        }

        public BlendModeParam()
        {
            Value = BlendModeKind.SrcOver;
        }

        public BlendModeParam(BlendModeKind value)
        {
            Value = value;
        }

        public SKBlendMode ToSkBlendMode()
        {
            return Value switch
            {
                BlendModeKind.Clear => SKBlendMode.Clear,
                BlendModeKind.Src => SKBlendMode.Src,
                BlendModeKind.Dst => SKBlendMode.Dst,
                BlendModeKind.SrcOver => SKBlendMode.SrcOver,
                BlendModeKind.DstOver => SKBlendMode.DstOver,
                BlendModeKind.SrcIn => SKBlendMode.SrcIn,
                BlendModeKind.DstIn => SKBlendMode.DstIn,
                BlendModeKind.SrcOut => SKBlendMode.SrcOut,
                BlendModeKind.DstOut => SKBlendMode.DstOut,
                BlendModeKind.SrcATop => SKBlendMode.SrcATop,
                BlendModeKind.DstATop => SKBlendMode.DstATop,
                BlendModeKind.Xor => SKBlendMode.Xor,
                BlendModeKind.Plus => SKBlendMode.Plus,
                BlendModeKind.Modulate => SKBlendMode.Modulate,
                BlendModeKind.Screen => SKBlendMode.Screen,
                BlendModeKind.Overlay => SKBlendMode.Overlay,
                BlendModeKind.Darken => SKBlendMode.Darken,
                BlendModeKind.Lighten => SKBlendMode.Lighten,
                BlendModeKind.ColorDodge => SKBlendMode.ColorDodge,
                BlendModeKind.ColorBurn => SKBlendMode.ColorBurn,
                BlendModeKind.HardLight => SKBlendMode.HardLight,
                BlendModeKind.SoftLight => SKBlendMode.SoftLight,
                BlendModeKind.Difference => SKBlendMode.Difference,
                BlendModeKind.Exclusion => SKBlendMode.Exclusion,
                BlendModeKind.Multiply => SKBlendMode.Multiply,
                BlendModeKind.Hue => SKBlendMode.Hue,
                BlendModeKind.Saturation => SKBlendMode.Saturation,
                BlendModeKind.Color => SKBlendMode.Color,
                BlendModeKind.Luminosity => SKBlendMode.Luminosity,
                _ => SKBlendMode.SrcOver
            };
        }

        public (BlendModeParam FirstHalf, BlendModeParam SecondHalf) Split()
        {
            var firstHalf = new BlendModeParam(Value);
            var secondHalf = new BlendModeParam(Value);
            return (firstHalf, secondHalf);
        }

        public static implicit operator BlendModeKind(BlendModeParam? param)
        {
            return param?.Value ?? BlendModeKind.SrcOver;
        }

        public static implicit operator BlendModeParam(BlendModeKind value)
        {
            return new BlendModeParam(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj is BlendModeParam other)
            {
                return Value.Equals(other.Value);
            }
            if (obj is BlendModeKind kind)
            {
                return Value.Equals(kind);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}

using System.Xml.Serialization;

namespace Metasia.Core.Objects.Parameters;

public class MetaIntParam
{
    [XmlAttribute("Value")]
    public int Value { get; set; }

    public MetaIntParam()
    {
        Value = 0;
    }

    public MetaIntParam(int value)
    {
        Value = value;
    }

    public (MetaIntParam FirstHalf, MetaIntParam SecondHalf) Split(int splitFrame)
    {
        var firstHalf = new MetaIntParam(Value);
        var secondHalf = new MetaIntParam(Value);
        return (firstHalf, secondHalf);
    }

    public static implicit operator int(MetaIntParam param)
    {
        return param?.Value ?? 0;
    }

    public static implicit operator MetaIntParam(int value)
    {
        return new MetaIntParam(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override bool Equals(object? obj)
    {
        if (obj is MetaIntParam other)
        {
            return Value.Equals(other.Value);
        }
        if (obj is int intValue)
        {
            return Value.Equals(intValue);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

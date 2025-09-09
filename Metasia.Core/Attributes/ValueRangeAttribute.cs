namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ValueRangeAttribute : Attribute
{
    public double Min { get; }
    public double Max { get; }
    public double RecommendedMin { get; }
    public double RecommendedMax { get; }

    public ValueRangeAttribute(double min, double max, double recommendedMin, double recommendedMax)
    {
        Min = min;
        Max = max;
        RecommendedMin = recommendedMin;
        RecommendedMax = recommendedMax;
    }
}
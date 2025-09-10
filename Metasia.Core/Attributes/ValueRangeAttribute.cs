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
        if (min > max)
        {
            throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));
        }

        if (recommendedMin > recommendedMax)
        {
            throw new ArgumentException("Recommended minimum value cannot be greater than recommended maximum value.", nameof(recommendedMin));
        }

        if (recommendedMin < min)
        {
            throw new ArgumentOutOfRangeException(nameof(recommendedMin), recommendedMin,
                $"Recommended minimum value ({recommendedMin}) must be greater than or equal to minimum value ({min}).");
        }

        if (recommendedMax > max)
        {
            throw new ArgumentOutOfRangeException(nameof(recommendedMax), recommendedMax,
                $"Recommended maximum value ({recommendedMax}) must be less than or equal to maximum value ({max}).");
        }

        Min = min;
        Max = max;
        RecommendedMin = recommendedMin;
        RecommendedMax = recommendedMax;
    }
}
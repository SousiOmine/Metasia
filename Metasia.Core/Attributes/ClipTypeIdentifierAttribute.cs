namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ClipTypeIdentifierAttribute : Attribute
{
    public string Identifier { get; }
    public string? DisplayKey { get; init; }
    public string? FallbackText { get; init; }

    public ClipTypeIdentifierAttribute(string clipType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clipType);
        Identifier = clipType;
    }
}

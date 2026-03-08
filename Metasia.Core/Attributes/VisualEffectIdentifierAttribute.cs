namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class VisualEffectIdentifierAttribute : Attribute
{
    public string Identifier { get; }
    public string? DisplayKey { get; init; }
    public string? FallbackText { get; init; }

    public VisualEffectIdentifierAttribute(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        Identifier = identifier;
    }
}

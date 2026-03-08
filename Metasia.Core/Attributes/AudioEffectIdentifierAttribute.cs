namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AudioEffectIdentifierAttribute : Attribute
{
    public string Identifier { get; }
    public string? DisplayKey { get; init; }
    public string? FallbackText { get; init; }

    public AudioEffectIdentifierAttribute(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        Identifier = identifier;
    }
}

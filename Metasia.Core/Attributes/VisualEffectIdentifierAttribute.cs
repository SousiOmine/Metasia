namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class VisualEffectIdentifierAttribute : Attribute
{
    public string Identifier { get; }

    public VisualEffectIdentifierAttribute(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        Identifier = identifier;
    }
}

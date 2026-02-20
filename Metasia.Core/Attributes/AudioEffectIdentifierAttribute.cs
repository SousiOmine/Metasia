namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AudioEffectIdentifierAttribute : Attribute
{
    public string Identifier { get; }
    
    public AudioEffectIdentifierAttribute(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        Identifier = identifier;
    }
}
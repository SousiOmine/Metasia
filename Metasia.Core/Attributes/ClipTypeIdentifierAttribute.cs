namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ClipTypeIdentifierAttribute : Attribute
{
    public string Identifier { get; }

    public ClipTypeIdentifierAttribute(string clipType)
    {
        if (string.IsNullOrWhiteSpace(clipType))
        {
            throw new ArgumentException("Clip type cannot be null or whitespace", nameof(clipType));
        }
        Identifier = clipType;
    }
}
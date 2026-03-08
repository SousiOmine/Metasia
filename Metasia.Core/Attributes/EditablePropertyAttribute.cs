namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditablePropertyAttribute : Attribute
{
    public string PropertyIdentifier { get; }
    public string? DisplayKey { get; init; }
    public string? FallbackText { get; init; }

    public EditablePropertyAttribute(string propertyIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyIdentifier);
        PropertyIdentifier = propertyIdentifier;
    }
}

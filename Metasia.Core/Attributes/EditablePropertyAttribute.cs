namespace Metasia.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditablePropertyAttribute : Attribute
{
    public string PropertyIdentifier { get; }

    public EditablePropertyAttribute(string propertyIdentifier)
    {
        PropertyIdentifier = propertyIdentifier;
    }
}

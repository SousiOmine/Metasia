using System;
using System.Collections.Generic;
using Metasia.Core.Attributes;

namespace Metasia.Editor.Models;

public class ObjectPropertyFinder
{
	public record EditablePropertyInfo(
        Type Type,
        string Identifier,
        object? PropertyValue
    );

    public static List<EditablePropertyInfo> FindEditableProperties(object target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var properties = new List<EditablePropertyInfo>();

        var type = target.GetType();

        foreach (var prop in type.GetProperties())
        {
            var attr = Attribute.GetCustomAttribute(prop, typeof(EditablePropertyAttribute), false);
            if (attr == null)
                continue;

            if (!(attr is EditablePropertyAttribute editablePropertyAttribute))
                continue;

            properties.Add(new EditablePropertyInfo(
                prop.PropertyType,
                editablePropertyAttribute.PropertyIdentifier,
                prop.GetValue(target)
            ));
        }

        return properties;
    }
}

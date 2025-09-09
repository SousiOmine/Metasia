using System;
using System.Collections.Generic;
using Metasia.Core.Attributes;

namespace Metasia.Editor.Models;

public class ObjectPropertyFinder
{
	public record EditablePropertyInfo(
        Type Type,
        string Identifier,
        object? PropertyValue,
        double? Min,
        double? Max,
        double? RecommendedMin,
        double? RecommendedMax
    );

    public static List<EditablePropertyInfo> FindEditableProperties(object target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var properties = new List<EditablePropertyInfo>();

        var type = target.GetType();

        foreach (var prop in type.GetProperties())
        {
            if(Attribute.GetCustomAttribute(prop, typeof(EditablePropertyAttribute)) is not EditablePropertyAttribute editablePropertyAttribute)
                continue;

            var rangeAttr = Attribute.GetCustomAttribute(prop, typeof(ValueRangeAttribute)) as ValueRangeAttribute;

            double? min = rangeAttr?.Min ?? double.MinValue;
            double? max = rangeAttr?.Max ?? double.MaxValue;
            double? recommendedMin = rangeAttr?.RecommendedMin ?? min;
            double? recommendedMax = rangeAttr?.RecommendedMax ?? max;

            properties.Add(new EditablePropertyInfo(
                prop.PropertyType,
                editablePropertyAttribute.PropertyIdentifier,
                prop.GetValue(target),
                min,
                max,
                recommendedMin,
                recommendedMax
            ));
        }

        return properties;
    }
}

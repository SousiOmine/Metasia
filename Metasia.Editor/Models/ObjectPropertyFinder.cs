using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.Generic;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models;

public class ObjectPropertyFinder
{
    public record EditablePropertyInfo(
        Type OwnerType,
        Type Type,
        string Identifier,
        string? DisplayKey,
        string? FallbackText,
        object? PropertyValue,
        double? Min,
        double? Max,
        double? RecommendedMin,
        double? RecommendedMax,
        IMetasiaObject? OwnerObject = null
    );

    public static List<EditablePropertyInfo> FindEditableProperties(object target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var properties = new List<EditablePropertyInfo>();

        var type = target.GetType();
        var ownerObject = target as IMetasiaObject;

        foreach (var prop in type.GetProperties())
        {
            if (Attribute.GetCustomAttribute(prop, typeof(EditablePropertyAttribute)) is not EditablePropertyAttribute editablePropertyAttribute)
                continue;

            var rangeAttr = Attribute.GetCustomAttribute(prop, typeof(ValueRangeAttribute)) as ValueRangeAttribute;

            double? min = rangeAttr?.Min ?? double.MinValue;
            double? max = rangeAttr?.Max ?? double.MaxValue;
            double? recommendedMin = rangeAttr?.RecommendedMin ?? min;
            double? recommendedMax = rangeAttr?.RecommendedMax ?? max;

            properties.Add(new EditablePropertyInfo(
                type,
                prop.PropertyType,
                editablePropertyAttribute.PropertyIdentifier,
                editablePropertyAttribute.DisplayKey,
                editablePropertyAttribute.FallbackText,
                prop.GetValue(target),
                min,
                max,
                recommendedMin,
                recommendedMax,
                ownerObject
            ));
        }

        return properties;
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters.Color;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class ColorValueChangeCommand : IEditCommand
{
    public record ColorValueChangeInfo(IMetasiaObject TargetObject, string PropertyIdentifier, ColorRgb8 OldValue, ColorRgb8 NewValue);

    public string Description => "色プロパティを変更";

    private readonly IReadOnlyList<ColorValueChangeInfo> _changeInfos;
    private static readonly ConcurrentDictionary<(Type Type, string Identifier), PropertyInfo?> _propertyCache = new();

    public ColorValueChangeCommand(IEnumerable<ColorValueChangeInfo> changeInfos)
    {
        var list = changeInfos.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("changeInfos is empty");
        }
        _changeInfos = list;
    }

    public void Execute()
    {
        foreach (var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.TargetObject, changeInfo.PropertyIdentifier);
            if (property is not null)
            {
                property.SetValue(changeInfo.TargetObject, changeInfo.NewValue.Clone());
            }
        }
    }

    public void Undo()
    {
        foreach (var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.TargetObject, changeInfo.PropertyIdentifier);
            if (property is not null)
            {
                property.SetValue(changeInfo.TargetObject, changeInfo.OldValue.Clone());
            }
        }
    }

    private static PropertyInfo? ResolveProperty(IMetasiaObject target, string propertyIdentifier)
    {
        var key = (target.GetType(), propertyIdentifier);
        return _propertyCache.GetOrAdd(key, static tuple =>
        {
            var (type, identifier) = tuple;
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType != typeof(ColorRgb8))
                {
                    continue;
                }

                if (Attribute.GetCustomAttribute(property, typeof(EditablePropertyAttribute)) is not EditablePropertyAttribute attribute)
                {
                    continue;
                }

                if (attribute.PropertyIdentifier == identifier)
                {
                    return property;
                }
            }

            return null;
        });
    }
}

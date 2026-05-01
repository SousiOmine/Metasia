using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Abstractions.EditCommands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class IntValueChangeCommand : IEditCommand
{
    public record IntValueChangeInfo(IMetasiaObject targetObject, string propertyIdentifier, int valueDifference);

    public string Description => "整数プロパティを変更";

    private readonly IEnumerable<IntValueChangeInfo> _changeInfos;
    private static readonly ConcurrentDictionary<(Type Type, string Identifier), PropertyInfo?> _propertyCache = new();

    public IntValueChangeCommand(IEnumerable<IntValueChangeInfo> changeInfos)
    {
        if (!changeInfos.Any())
        {
            throw new ArgumentException("changeInfos is empty");
        }
        _changeInfos = changeInfos;
    }

    public void Execute()
    {
        foreach (var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.targetObject, changeInfo.propertyIdentifier);
            if (property is not null && property.PropertyType == typeof(MetaIntParam))
            {
                var param = (MetaIntParam)property.GetValue(changeInfo.targetObject)!;
                param.Value += changeInfo.valueDifference;
            }
        }
    }

    public void Undo()
    {
        foreach (var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.targetObject, changeInfo.propertyIdentifier);
            if (property is not null && property.PropertyType == typeof(MetaIntParam))
            {
                var param = (MetaIntParam)property.GetValue(changeInfo.targetObject)!;
                param.Value -= changeInfo.valueDifference;
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
                if (property.PropertyType != typeof(MetaIntParam))
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

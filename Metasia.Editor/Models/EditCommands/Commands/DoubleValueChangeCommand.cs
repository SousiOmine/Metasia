using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class DoubleValueChangeCommand : IEditCommand
{
    public record DoubleValueChangeInfo(ClipObject targetClip, string propertyIdentifier, double valueDifference);

    public string Description => "数値プロパティを変更";

    private readonly IEnumerable<DoubleValueChangeInfo> _changeInfos;
    private static readonly ConcurrentDictionary<(Type Type, string Identifier), PropertyInfo?> _propertyCache = new();

    public DoubleValueChangeCommand(IEnumerable<DoubleValueChangeInfo> changeInfos)
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
            var property = ResolveProperty(changeInfo.targetClip, changeInfo.propertyIdentifier);
            if (property is not null && property.PropertyType == typeof(MetaDoubleParam))
            {
                var param = (MetaDoubleParam)property.GetValue(changeInfo.targetClip)!;
                param.Value += changeInfo.valueDifference;
            }
        }
    }

    public void Undo()
    {
        foreach (var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.targetClip, changeInfo.propertyIdentifier);
            if (property is not null && property.PropertyType == typeof(MetaDoubleParam))
            {
                var param = (MetaDoubleParam)property.GetValue(changeInfo.targetClip)!;
                param.Value -= changeInfo.valueDifference;
            }
        }
    }

    private static PropertyInfo? ResolveProperty(ClipObject clip, string propertyIdentifier)
    {
        var key = (clip.GetType(), propertyIdentifier);
        return _propertyCache.GetOrAdd(key, static tuple =>
        {
            var (type, identifier) = tuple;
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType != typeof(MetaDoubleParam))
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
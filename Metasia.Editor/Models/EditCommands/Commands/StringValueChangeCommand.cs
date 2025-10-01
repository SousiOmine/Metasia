using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class StringValueChangeCommand : IEditCommand
{
    public record StringValueChangeInfo(ClipObject targetClip, string propertyIdentifier, string oldValue, string newValue);

    public string Description => "文字列プロパティを変更";

    private readonly IEnumerable<StringValueChangeInfo> _changeInfos;
    private static readonly ConcurrentDictionary<(Type Type, string Identifier), PropertyInfo?> _propertyCache = new();

    public StringValueChangeCommand(IEnumerable<StringValueChangeInfo> changeInfos)
    {
        if(!changeInfos.Any())
        {
            throw new ArgumentException("changeInfos is empty");
        }
        _changeInfos = changeInfos;
    }

    public void Execute()
    {
        foreach(var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.targetClip, changeInfo.propertyIdentifier);
            if (property is not null)
            {
                property.SetValue(changeInfo.targetClip, changeInfo.newValue);
            }
        }
    }

    public void Undo()
    {
        foreach(var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.targetClip, changeInfo.propertyIdentifier);
            if (property is not null)
            {
                property.SetValue(changeInfo.targetClip, changeInfo.oldValue);
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
                if (property.PropertyType != typeof(string))
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

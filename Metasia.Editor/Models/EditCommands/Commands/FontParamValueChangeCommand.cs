using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Typography;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class FontParamValueChangeCommand : IEditCommand
{
    public record FontParamValueChangeInfo(ClipObject TargetClip, string PropertyIdentifier, MetaFontParam BeforeValue, MetaFontParam AfterValue);

    public string Description => "フォント設定の変更";

    private readonly IEnumerable<FontParamValueChangeInfo> _changeInfos;

    private static readonly ConcurrentDictionary<(Type Type, string Identifier), PropertyInfo?> PropertyCache = new();

    public FontParamValueChangeCommand(IEnumerable<FontParamValueChangeInfo> changeInfos)
    {
        if (changeInfos is null || !changeInfos.Any())
        {
            throw new ArgumentException("changeInfos is empty", nameof(changeInfos));
        }
        _changeInfos = changeInfos;
    }

    public void Execute()
    {
        foreach (var info in _changeInfos)
        {
            var property = ResolveProperty(info.TargetClip, info.PropertyIdentifier);
            if (property is not null)
            {
                property.SetValue(info.TargetClip, info.AfterValue.Clone());
            }
        }
    }

    public void Undo()
    {
        foreach (var info in _changeInfos)
        {
            var property = ResolveProperty(info.TargetClip, info.PropertyIdentifier);
            if (property is not null)
            {
                property.SetValue(info.TargetClip, info.BeforeValue.Clone());
            }
        }
    }

    private static PropertyInfo? ResolveProperty(ClipObject clip, string propertyIdentifier)
    {
        var key = (clip.GetType(), propertyIdentifier);
        return PropertyCache.GetOrAdd(key, static tuple =>
        {
            var (type, identifier) = tuple;
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType != typeof(MetaFontParam))
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

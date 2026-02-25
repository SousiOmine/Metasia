using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class FontParamValueChangeCommand : IEditCommand
{
    public record FontParamValueChangeInfo(IMetasiaObject TargetObject, string PropertyIdentifier, MetaFontParam BeforeValue, MetaFontParam AfterValue);

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
            ApplyPropertyValue(info.TargetObject, info.PropertyIdentifier, info.AfterValue);
        }
    }

    public void Undo()
    {
        foreach (var info in _changeInfos)
        {
            ApplyPropertyValue(info.TargetObject, info.PropertyIdentifier, info.BeforeValue);
        }
    }

    private static void ApplyPropertyValue(IMetasiaObject target, string propertyIdentifier, MetaFontParam value)
    {
        var property = ResolveProperty(target, propertyIdentifier);
        if (property is null)
        {
            throw new InvalidOperationException(
                $"プロパティ '{propertyIdentifier}' をオブジェクト '{target.Id}' (型: {target.GetType().Name}) で解決できません。");
        }
        property.SetValue(target, value.Clone());
    }

    private static PropertyInfo? ResolveProperty(IMetasiaObject target, string propertyIdentifier)
    {
        var key = (target.GetType(), propertyIdentifier);
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

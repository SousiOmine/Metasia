using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Render;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class BlendModeValueChangeCommand : IEditCommand
{
    public record BlendModeValueChangeInfo(ClipObject TargetClip, string PropertyIdentifier, BlendModeKind OldValue, BlendModeKind NewValue);

    public string Description => "ブレンドモードの変更";

    private readonly IEnumerable<BlendModeValueChangeInfo> _changeInfos;

    private static readonly ConcurrentDictionary<(Type Type, string Identifier), PropertyInfo?> PropertyCache = new();

    public BlendModeValueChangeCommand(IEnumerable<BlendModeValueChangeInfo> changeInfos)
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
            ApplyPropertyValue(info.TargetClip, info.PropertyIdentifier, info.NewValue);
        }
    }

    public void Undo()
    {
        foreach (var info in _changeInfos)
        {
            ApplyPropertyValue(info.TargetClip, info.PropertyIdentifier, info.OldValue);
        }
    }

    public void Redo()
    {
        Execute();
    }

    private static void ApplyPropertyValue(ClipObject clip, string propertyIdentifier, BlendModeKind value)
    {
        var property = ResolveProperty(clip, propertyIdentifier);
        if (property is null)
        {
            throw new InvalidOperationException(
                $"プロパティ '{propertyIdentifier}' をクリップ '{clip.Id}' (型: {clip.GetType().Name}) で解決できません。");
        }
        var blendModeParam = (BlendModeParam?)property.GetValue(clip);
        if (blendModeParam is not null)
        {
            blendModeParam.Value = value;
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
                if (property.PropertyType != typeof(BlendModeParam))
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.Models.EditCommands.Commands;

/// <summary>
/// 対象レイヤー指定（LayerTarget）の変更を行うコマンド
/// </summary>
public class LayerTargetValueChangeCommand : IEditCommand
{
    public record LayerTargetValueChangeInfo(
        ClipObject TargetClip,
        string PropertyIdentifier,
        LayerTarget OldValue,
        LayerTarget NewValue);

    public string Description => "対象レイヤー指定を変更";

    private readonly IReadOnlyList<LayerTargetValueChangeInfo> _changeInfos;
    private static readonly ConcurrentDictionary<(Type Type, string Identifier), PropertyInfo?> _propertyCache = new();

    public LayerTargetValueChangeCommand(IEnumerable<LayerTargetValueChangeInfo> changeInfos)
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
            var property = ResolveProperty(changeInfo.TargetClip, changeInfo.PropertyIdentifier);
            if (property is null)
            {
                throw new InvalidOperationException($"Property '{changeInfo.PropertyIdentifier}' not found on clip type {changeInfo.TargetClip.GetType().Name}");
            }
            property.SetValue(changeInfo.TargetClip, changeInfo.NewValue.Clone());
        }
    }

    public void Undo()
    {
        foreach (var changeInfo in _changeInfos)
        {
            var property = ResolveProperty(changeInfo.TargetClip, changeInfo.PropertyIdentifier);
            if (property is null)
            {
                throw new InvalidOperationException($"Property '{changeInfo.PropertyIdentifier}' not found on clip type {changeInfo.TargetClip.GetType().Name}");
            }
            property.SetValue(changeInfo.TargetClip, changeInfo.OldValue.Clone());
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
                if (property.PropertyType != typeof(LayerTarget))
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

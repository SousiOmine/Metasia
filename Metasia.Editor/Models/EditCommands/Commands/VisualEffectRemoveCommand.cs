using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;
using System;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class VisualEffectRemoveCommand : IEditCommand
{
    public string Description => "ビジュアルエフェクトの削除";

    private readonly IRenderable _target;
    private readonly VisualEffectBase _effect;
    private readonly int _removedIndex;

    public VisualEffectRemoveCommand(IRenderable target, VisualEffectBase effect)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _removedIndex = target.VisualEffects.IndexOf(effect);
        if (_removedIndex == -1)
        {
            throw new ArgumentException("指定されたビジュアルエフェクトがターゲットのVisualEffectsリストに存在しません。", nameof(effect));
        }
    }

    public void Execute()
    {
        if (_target.VisualEffects.Contains(_effect))
        {
            _target.VisualEffects.Remove(_effect);
        }
    }

    public void Undo()
    {
        if (!_target.VisualEffects.Contains(_effect))
        {
            int insertIndex = _removedIndex >= 0 && _removedIndex < _target.VisualEffects.Count
                ? _removedIndex
                : _target.VisualEffects.Count;
            _target.VisualEffects.Insert(insertIndex, _effect);
        }
    }
}

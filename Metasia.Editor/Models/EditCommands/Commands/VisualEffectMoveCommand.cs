using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.EditCommands;
using System;
using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class VisualEffectMoveCommand : IEditCommand
{
    public string Description => "ビジュアルエフェクトの移動";

    private readonly IRenderable _target;
    private readonly VisualEffectBase _effect;
    private readonly int _oldIndex;
    private readonly int _newIndex;

    public VisualEffectMoveCommand(IRenderable target, VisualEffectBase effect, int newIndex)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _oldIndex = target.VisualEffects.IndexOf(effect);
        if (_oldIndex == -1)
        {
            throw new ArgumentException("指定されたビジュアルエフェクトがターゲットのVisualEffectsリストに存在しません。", nameof(effect));
        }

        _newIndex = newIndex;
    }

    public void Execute() => Move(_newIndex);

    public void Undo() => Move(_oldIndex);

    private void Move(int targetIndex)
    {
        int currentIndex = _target.VisualEffects.IndexOf(_effect);
        if (currentIndex == -1)
        {
            return;
        }

        int clampedIndex = Math.Clamp(targetIndex, 0, _target.VisualEffects.Count - 1);
        if (currentIndex == clampedIndex)
        {
            return;
        }

        _target.VisualEffects.RemoveAt(currentIndex);
        _target.VisualEffects.Insert(clampedIndex, _effect);
    }
}

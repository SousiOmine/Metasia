using System;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class AudioEffectMoveCommand : IEditCommand
{
    public string Description => "オーディオエフェクトの移動";

    private readonly IAudible _target;
    private readonly AudioEffectBase _effect;
    private readonly int _oldIndex;
    private readonly int _newIndex;

    public AudioEffectMoveCommand(IAudible target, AudioEffectBase effect, int newIndex)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _oldIndex = target.AudioEffects.IndexOf(effect);
        if (_oldIndex == -1)
        {
            throw new ArgumentException("指定されたオーディオエフェクトがターゲットのAudioEffectsリストに存在しません。", nameof(effect));
        }

        _newIndex = newIndex;
    }

    public void Execute() => Move(_newIndex);

    public void Undo() => Move(_oldIndex);

    private void Move(int targetIndex)
    {
        int currentIndex = _target.AudioEffects.IndexOf(_effect);
        if (currentIndex == -1)
        {
            return;
        }

        int clampedIndex = Math.Clamp(targetIndex, 0, _target.AudioEffects.Count - 1);
        if (currentIndex == clampedIndex)
        {
            return;
        }

        _target.AudioEffects.RemoveAt(currentIndex);
        _target.AudioEffects.Insert(clampedIndex, _effect);
    }
}

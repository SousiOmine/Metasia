using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using System;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class AudioEffectRemoveCommand : IEditCommand
{
    public string Description => "オーディオエフェクトの削除";

    private readonly IAudible _target;
    private readonly AudioEffectBase _effect;
    private readonly int _removedIndex;

    public AudioEffectRemoveCommand(IAudible target, AudioEffectBase effect)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _removedIndex = target.AudioEffects.IndexOf(effect);
    }

    public void Execute()
    {
        if (_target.AudioEffects.Contains(_effect))
        {
            _target.AudioEffects.Remove(_effect);
        }
    }

    public void Undo()
    {
        if (!_target.AudioEffects.Contains(_effect))
        {
            int insertIndex = _removedIndex >= 0 && _removedIndex < _target.AudioEffects.Count
                ? _removedIndex
                : _target.AudioEffects.Count;
            _target.AudioEffects.Insert(insertIndex, _effect);
        }
    }
}
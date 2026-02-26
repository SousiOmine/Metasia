using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using System;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class AudioEffectAddCommand : IEditCommand
{
    public string Description => "オーディオエフェクトの追加";

    private readonly IAudible _target;
    private readonly AudioEffectBase _effect;
    private int? _index;

    public AudioEffectAddCommand(IAudible target, AudioEffectBase effect)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
    }

    public void Execute()
    {
        if (!_target.AudioEffects.Contains(_effect))
        {
            if (_index.HasValue)
            {
                _target.AudioEffects.Insert(_index.Value, _effect);
            }
            else
            {
                _index = _target.AudioEffects.Count;
                _target.AudioEffects.Add(_effect);
            }
        }
        else
        {
            _index = _target.AudioEffects.IndexOf(_effect);
        }
    }

    public void Undo()
    {
        if (_target.AudioEffects.Contains(_effect))
        {
            _index = _target.AudioEffects.IndexOf(_effect);
            _target.AudioEffects.Remove(_effect);
        }
    }
}
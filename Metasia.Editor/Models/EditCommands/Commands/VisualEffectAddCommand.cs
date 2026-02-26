using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;
using System;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class VisualEffectAddCommand : IEditCommand
{
    public string Description => "ビジュアルエフェクトの追加";

    private readonly IRenderable _target;
    private readonly VisualEffectBase _effect;
    private int? _index;

    public VisualEffectAddCommand(IRenderable target, VisualEffectBase effect)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
    }

    public void Execute()
    {
        if (!_target.VisualEffects.Contains(_effect))
        {
            if (_index.HasValue)
            {
                _target.VisualEffects.Insert(_index.Value, _effect);
            }
            else
            {
                _index = _target.VisualEffects.Count;
                _target.VisualEffects.Add(_effect);
            }
        }
        else
        {
            _index = _target.VisualEffects.IndexOf(_effect);
        }
    }

    public void Undo()
    {
        if (_target.VisualEffects.Contains(_effect))
        {
            _index = _target.VisualEffects.IndexOf(_effect);
            _target.VisualEffects.Remove(_effect);
        }
    }
}

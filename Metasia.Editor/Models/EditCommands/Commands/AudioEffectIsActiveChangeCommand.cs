using System;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class AudioEffectIsActiveChangeCommand : IEditCommand
{
    public string Description => "オーディオエフェクトの有効状態変更";

    private readonly AudioEffectBase _effect;
    private readonly bool _newIsActive;
    private readonly bool _oldIsActive;

    public AudioEffectIsActiveChangeCommand(IAudible target, AudioEffectBase effect, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(target);
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _newIsActive = isActive;
        _oldIsActive = effect.IsActive;

        if (!target.AudioEffects.Contains(effect))
        {
            throw new ArgumentException("指定されたオーディオエフェクトがターゲットのAudioEffectsリストに存在しません。", nameof(effect));
        }
    }

    public void Execute()
    {
        _effect.IsActive = _newIsActive;
    }

    public void Undo()
    {
        _effect.IsActive = _oldIsActive;
    }
}

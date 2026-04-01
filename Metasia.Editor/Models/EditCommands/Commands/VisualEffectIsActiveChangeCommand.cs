using System;
using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class VisualEffectIsActiveChangeCommand : IEditCommand
{
    public string Description => "ビジュアルエフェクトの有効状態変更";

    private readonly VisualEffectBase _effect;
    private readonly bool _newIsActive;
    private readonly bool _oldIsActive;

    public VisualEffectIsActiveChangeCommand(IRenderable target, VisualEffectBase effect, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(target);
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _newIsActive = isActive;
        _oldIsActive = effect.IsActive;

        if (!target.VisualEffects.Contains(effect))
        {
            throw new ArgumentException("指定されたビジュアルエフェクトがターゲットのVisualEffectsリストに存在しません。", nameof(effect));
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

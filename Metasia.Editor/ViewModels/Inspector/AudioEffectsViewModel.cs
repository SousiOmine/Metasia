using System.Collections.ObjectModel;
using Metasia.Core.Objects;
using Metasia.Core.Sounds;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector;

public class AudioEffectsViewModel : ViewModelBase
{
    public ObservableCollection<AudioEffectItemViewModel> AudioEffectItems { get; } = new();
    
    private readonly IAudible _target;
    private readonly IProjectState _projectState;
    
    public AudioEffectsViewModel(
        IAudible target,
        IProjectState projectState
        )
    {
        _target = target;
        _projectState = projectState;

        foreach (IAudioEffect effect in _target.AudioEffects)
        {
            AudioEffectItems.Add(new AudioEffectItemViewModel(effect));
        }
    }
}

public sealed class AudioEffectItemViewModel : ViewModelBase
{
    public string EffectId { get; init; } = string.Empty;
    public string EffectName { get; init; } = string.Empty;

    public AudioEffectItemViewModel(IAudioEffect effect)
    {
        EffectId = effect.Id;
        EffectName = effect.GetType().Name;
    }
}
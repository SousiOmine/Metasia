using System;
using Metasia.Core.Objects;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector;

public class AudioEffectsViewModelFactory : IAudioEffectsViewModelFactory
{
    private readonly IProjectState _projectState;
    
    public AudioEffectsViewModelFactory(
        IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(projectState);
        _projectState = projectState;
    }
    public AudioEffectsViewModel Create(IAudible target)
    {
        ArgumentNullException.ThrowIfNull(target);
        
        return new AudioEffectsViewModel(target, _projectState);
    }
}


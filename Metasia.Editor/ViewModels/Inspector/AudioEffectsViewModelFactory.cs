using System;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector;

public class AudioEffectsViewModelFactory : IAudioEffectsViewModelFactory
{
    private readonly IProjectState _projectState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IPropertyRouterViewModelFactory _propertyRouterViewModelFactory;
    
    public AudioEffectsViewModelFactory(
        IProjectState projectState,
        IEditCommandManager editCommandManager,
        IPropertyRouterViewModelFactory propertyRouterViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(projectState);
        _projectState = projectState;
        _editCommandManager = editCommandManager;
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;
    }
    public AudioEffectsViewModel Create(IAudible target)
    {
        ArgumentNullException.ThrowIfNull(target);
        
        return new AudioEffectsViewModel(target, _projectState, _editCommandManager, _propertyRouterViewModelFactory);
    }
}


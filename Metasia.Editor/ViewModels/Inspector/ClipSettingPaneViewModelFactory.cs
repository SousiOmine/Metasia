using System;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.ViewModels.Inspector;

public class ClipSettingPaneViewModelFactory : IClipSettingPaneViewModelFactory
{
    private readonly IPropertyRouterViewModelFactory _propertyRouterViewModelFactory;
    private readonly IAudioEffectsViewModelFactory _audioEffectsViewModelFactory;
    private readonly IEditCommandManager _editCommandManager;
    public ClipSettingPaneViewModelFactory(
        IPropertyRouterViewModelFactory propertyRouterViewModelFactory,
        IAudioEffectsViewModelFactory audioEffectsViewModelFactory,
        IEditCommandManager editCommandManager)
    {
        ArgumentNullException.ThrowIfNull(propertyRouterViewModelFactory);
        ArgumentNullException.ThrowIfNull(audioEffectsViewModelFactory);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;
        _audioEffectsViewModelFactory = audioEffectsViewModelFactory;
        _editCommandManager = editCommandManager;
    }

    public ClipSettingPaneViewModel Create()
    {
        return new ClipSettingPaneViewModel(_propertyRouterViewModelFactory, _audioEffectsViewModelFactory, _editCommandManager);
    }
}
using System;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.ViewModels.Inspector;

public class ClipSettingPaneViewModelFactory : IClipSettingPaneViewModelFactory
{
    private readonly IPropertyRouterViewModelFactory _propertyRouterViewModelFactory;
    private readonly IEditCommandManager _editCommandManager;
    public ClipSettingPaneViewModelFactory(IPropertyRouterViewModelFactory propertyRouterViewModelFactory, IEditCommandManager editCommandManager)
    {
        ArgumentNullException.ThrowIfNull(propertyRouterViewModelFactory);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;
        _editCommandManager = editCommandManager;
    }

    public ClipSettingPaneViewModel Create()
    {
        return new ClipSettingPaneViewModel(_propertyRouterViewModelFactory, _editCommandManager);
    }
}
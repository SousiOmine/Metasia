using System;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector;

public class VisualEffectsViewModelFactory : IVisualEffectsViewModelFactory
{
    private readonly IProjectState _projectState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IPropertyRouterViewModelFactory _propertyRouterViewModelFactory;
    private readonly INewObjectSelectViewModelFactory _newObjectSelectViewModelFactory;

    public VisualEffectsViewModelFactory(
        IProjectState projectState,
        IEditCommandManager editCommandManager,
        IPropertyRouterViewModelFactory propertyRouterViewModelFactory,
        INewObjectSelectViewModelFactory newObjectSelectViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(projectState);
        _projectState = projectState;
        _editCommandManager = editCommandManager;
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;
        _newObjectSelectViewModelFactory = newObjectSelectViewModelFactory;
    }

    public VisualEffectsViewModel Create(IRenderable target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return new VisualEffectsViewModel(target, _projectState, _editCommandManager, _propertyRouterViewModelFactory, _newObjectSelectViewModelFactory);
    }
}

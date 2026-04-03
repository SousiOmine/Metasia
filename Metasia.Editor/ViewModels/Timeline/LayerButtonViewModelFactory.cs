using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using Metasia.Core.Objects;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.ViewModels.Timeline;

public class LayerButtonViewModelFactory : ILayerButtonViewModelFactory
{
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;
    private readonly ISelectionState _selectionState;
    public LayerButtonViewModelFactory(IEditCommandManager editCommandManager, IProjectState projectState, ISelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(selectionState);
        _editCommandManager = editCommandManager;
        _projectState = projectState;
        _selectionState = selectionState;
    }
    public LayerButtonViewModel Create(LayerObject targetLayerObject)
    {
        ArgumentNullException.ThrowIfNull(_editCommandManager);
        ArgumentNullException.ThrowIfNull(_projectState);
        ArgumentNullException.ThrowIfNull(_selectionState);
        ArgumentNullException.ThrowIfNull(targetLayerObject);
        return new LayerButtonViewModel(targetLayerObject, _editCommandManager, _projectState, _selectionState);
    }
}
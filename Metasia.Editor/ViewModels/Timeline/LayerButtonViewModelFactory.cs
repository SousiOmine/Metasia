using System;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Timeline;

public class LayerButtonViewModelFactory : ILayerButtonViewModelFactory
{
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;
    public LayerButtonViewModelFactory(IEditCommandManager editCommandManager, IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }
    public LayerButtonViewModel Create(LayerObject targetLayerObject)
    {
        ArgumentNullException.ThrowIfNull(_editCommandManager);
        ArgumentNullException.ThrowIfNull(_projectState);
        ArgumentNullException.ThrowIfNull(targetLayerObject);
        return new LayerButtonViewModel(targetLayerObject, _editCommandManager, _projectState);
    }
}
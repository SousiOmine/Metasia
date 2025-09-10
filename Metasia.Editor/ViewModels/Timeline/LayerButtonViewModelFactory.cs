using System;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.ViewModels.Timeline;

public class LayerButtonViewModelFactory : ILayerButtonViewModelFactory
{
    private readonly IEditCommandManager _editCommandManager;
    public LayerButtonViewModelFactory(IEditCommandManager editCommandManager)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        _editCommandManager = editCommandManager;
    }
    public LayerButtonViewModel Create(LayerObject targetLayerObject)
    {
        ArgumentNullException.ThrowIfNull(_editCommandManager);
        ArgumentNullException.ThrowIfNull(targetLayerObject);
        return new LayerButtonViewModel(targetLayerObject, _editCommandManager);
    }
}
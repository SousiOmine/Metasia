using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class LayerTargetPropertyViewModelFactory : ILayerTargetPropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public LayerTargetPropertyViewModelFactory(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState)
    {
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    public LayerTargetPropertyViewModel Create(string propertyIdentifier, LayerTarget target)
    {
        return new LayerTargetPropertyViewModel(
            _selectionState,
            propertyIdentifier,
            _editCommandManager,
            _projectState,
            target);
    }
}

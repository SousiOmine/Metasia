using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaEnumParamPropertyViewModelFactory : IMetaEnumParamPropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public MetaEnumParamPropertyViewModelFactory(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState)
    {
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    public MetaEnumParamPropertyViewModel Create(string propertyIdentifier, MetaEnumParam target)
    {
        return new MetaEnumParamPropertyViewModel(
            _selectionState,
            propertyIdentifier,
            _editCommandManager,
            _projectState,
            target);
    }
}

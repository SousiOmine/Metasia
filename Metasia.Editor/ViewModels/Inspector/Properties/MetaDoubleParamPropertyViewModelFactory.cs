using System;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaDoubleParamPropertyViewModelFactory : IMetaDoubleParamPropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public MetaDoubleParamPropertyViewModelFactory(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    public MetaDoubleParamPropertyViewModel Create(string propertyIdentifier, MetaDoubleParam target, double min = double.MinValue, double max = double.MaxValue, double recommendMin = double.MinValue, double recommendMax = double.MaxValue)
    {
        return new MetaDoubleParamPropertyViewModel(
            _selectionState,
            propertyIdentifier,
            _editCommandManager,
            _projectState,
            target,
            min,
            max,
            recommendMin,
            recommendMax);
    }
}
using System;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class DoublePropertyViewModelFactory : IDoublePropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public DoublePropertyViewModelFactory(
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

    public DoublePropertyViewModel Create(string propertyIdentifier, double target, double min = double.MinValue, double max = double.MaxValue, double recommendMin = double.MinValue, double recommendMax = double.MaxValue)
    {
        return new DoublePropertyViewModel(
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
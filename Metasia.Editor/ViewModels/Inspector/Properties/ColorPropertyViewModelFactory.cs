using System;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class ColorPropertyViewModelFactory : IColorPropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public ColorPropertyViewModelFactory(ISelectionState selectionState, IEditCommandManager editCommandManager, IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    public ColorPropertyViewModel Create(string propertyIdentifier, ColorRgb8 target)
    {
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        ArgumentNullException.ThrowIfNull(target);
        return new ColorPropertyViewModel(_selectionState, propertyIdentifier, _editCommandManager, _projectState, target);
    }
}

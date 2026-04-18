using Metasia.Core.Coordinate;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.States;
using System;

namespace Metasia.Editor.ViewModels.Inspector.Properties.Components;

public class InterpolationLogicMenuItemViewModelFactory : IInterpolationLogicMenuItemViewModelFactory
{
    private readonly IProjectState _projectState;
    private readonly IEditCommandManager _editCommandManager;

    public InterpolationLogicMenuItemViewModelFactory(
        IProjectState projectState,
        IEditCommandManager editCommandManager)
    {
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        _projectState = projectState;
        _editCommandManager = editCommandManager;
    }

    public InterpolationLogicMenuItemViewModel Create(CoordPoint coordPoint, Type interpolationLogicType)
    {
        ArgumentNullException.ThrowIfNull(coordPoint);
        ArgumentNullException.ThrowIfNull(interpolationLogicType);
        return new InterpolationLogicMenuItemViewModel(coordPoint, interpolationLogicType, _projectState, _editCommandManager);
    }
}
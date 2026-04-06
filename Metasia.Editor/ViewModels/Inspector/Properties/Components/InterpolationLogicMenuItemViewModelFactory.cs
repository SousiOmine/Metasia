using Metasia.Core.Coordinate;
using Metasia.Editor.Models.States;
using System;

namespace Metasia.Editor.ViewModels.Inspector.Properties.Components;

public class InterpolationLogicMenuItemViewModelFactory : IInterpolationLogicMenuItemViewModelFactory
{
    private readonly IProjectState _projectState;
    
    public InterpolationLogicMenuItemViewModelFactory(IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(projectState);
        _projectState = projectState;
    }
    
    public InterpolationLogicMenuItemViewModel Create(CoordPoint coordPoint, Type interpolationLogicType)
    {
        ArgumentNullException.ThrowIfNull(coordPoint);
        ArgumentNullException.ThrowIfNull(interpolationLogicType);
        return new InterpolationLogicMenuItemViewModel(coordPoint, interpolationLogicType, _projectState);
    }
}
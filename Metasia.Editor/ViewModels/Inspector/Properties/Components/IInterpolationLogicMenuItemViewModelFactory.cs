using Metasia.Core.Coordinate;
using System;

namespace Metasia.Editor.ViewModels.Inspector.Properties.Components;

public interface IInterpolationLogicMenuItemViewModelFactory
{
    InterpolationLogicMenuItemViewModel Create(CoordPoint coordPoint, Type interpolationLogicType);
}
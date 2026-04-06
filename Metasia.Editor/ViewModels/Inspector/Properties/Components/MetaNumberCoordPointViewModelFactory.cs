using Metasia.Core.Coordinate;

namespace Metasia.Editor.ViewModels.Inspector.Properties.Components;

public class MetaNumberCoordPointViewModelFactory : IMetaNumberCoordPointViewModelFactory
{
    private readonly IInterpolationLogicMenuItemViewModelFactory _interpolationLogicMenuItemFactory;

    public MetaNumberCoordPointViewModelFactory(IInterpolationLogicMenuItemViewModelFactory interpolationLogicMenuItemFactory)
    {
        _interpolationLogicMenuItemFactory = interpolationLogicMenuItemFactory;
    }

    public MetaNumberCoordPointViewModel Create(
        MetaNumberParamPropertyViewModel parentViewModel,
        CoordPoint target,
        MetaNumberCoordPointViewModel.PointType pointType = MetaNumberCoordPointViewModel.PointType.Start,
        double min = double.MinValue,
        double max = double.MaxValue,
        double recommendedMin = double.MinValue,
        double recommendedMax = double.MaxValue)
    {
        return new MetaNumberCoordPointViewModel(
            _interpolationLogicMenuItemFactory,
            parentViewModel,
            target,
            pointType,
            min,
            max,
            recommendedMin,
            recommendedMax);
    }
}
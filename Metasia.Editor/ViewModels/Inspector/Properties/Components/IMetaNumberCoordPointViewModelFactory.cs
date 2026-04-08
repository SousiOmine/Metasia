using Metasia.Core.Coordinate;

namespace Metasia.Editor.ViewModels.Inspector.Properties.Components;

public interface IMetaNumberCoordPointViewModelFactory
{
    MetaNumberCoordPointViewModel Create(
        MetaNumberParamPropertyViewModel parentViewModel,
        CoordPoint target,
        MetaNumberCoordPointViewModel.PointType pointType = MetaNumberCoordPointViewModel.PointType.Start,
        double min = double.MinValue,
        double max = double.MaxValue,
        double recommendedMin = double.MinValue,
        double recommendedMax = double.MaxValue);
}
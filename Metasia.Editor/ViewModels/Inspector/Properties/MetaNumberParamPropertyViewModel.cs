using System;
using System.Collections.ObjectModel;
using Metasia.Core.Coordinate;
using Metasia.Editor.Models;
using Metasia.Editor.ViewModels.Inspector.Properties.Components;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaNumberParamPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }
    
    public ObservableCollection<MetaNumberCoordPointViewModel> CoordPoints { get; } = new();

    public string PropertyValue
    {
        get => _propertyValue;
        set => this.RaiseAndSetIfChanged(ref _propertyValue, value);
    }
    private string _propertyDisplayName = string.Empty;
    private string _propertyValue = string.Empty;
    private double _min = double.MinValue;
    private double _max = double.MaxValue;
    private double _recommendedMin = double.MinValue;
    private double _recommendedMax = double.MaxValue;


    public MetaNumberParamPropertyViewModel(string propertyIdentifier, MetaNumberParam<double> target, double min = double.MinValue, double max = double.MaxValue, double recommendedMin = double.MinValue, double recommendedMax = double.MaxValue)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = "100(仮)";
        _min = min;
        _max = max;
        _recommendedMin = recommendedMin;
        _recommendedMax = recommendedMax;

        /*foreach (var coordPoint in target.Params)
        {
            MetaNumberCoordPointViewModel.PointType pointType = MetaNumberCoordPointViewModel.PointType.Single;
            if (target.Params.Count == 1)
            {
                pointType = MetaNumberCoordPointViewModel.PointType.Single;
            }
            else
            {
                
            }
            CoordPoints.Add(new MetaNumberCoordPointViewModel(coordPoint, pointType));
        }*/
        for (int i = 0; i < target.Params.Count; i++)
        {
            MetaNumberCoordPointViewModel.PointType pointType = MetaNumberCoordPointViewModel.PointType.Single;
            if (target.Params.Count == 1)
            {
                pointType = MetaNumberCoordPointViewModel.PointType.Single;
            }
            else if (i == 0)
            {
                pointType = MetaNumberCoordPointViewModel.PointType.Start;
            }
            else if (i == target.Params.Count - 1)
            {
                pointType = MetaNumberCoordPointViewModel.PointType.End;
            }
            else
            {
                pointType = MetaNumberCoordPointViewModel.PointType.Mid;
            }
            CoordPoints.Add(new MetaNumberCoordPointViewModel(target.Params[i], pointType, min, max, recommendedMin, recommendedMax));
        }
    }
    
}
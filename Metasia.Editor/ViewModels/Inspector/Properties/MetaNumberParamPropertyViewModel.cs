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

    public MetaNumberParamPropertyViewModel(string propertyIdentifier, MetaNumberParam<double> target)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = "100(仮)";
        
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
            else if(i == 0)
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
            CoordPoints.Add(new MetaNumberCoordPointViewModel(target.Params[i], pointType));
        }
    }
    
}
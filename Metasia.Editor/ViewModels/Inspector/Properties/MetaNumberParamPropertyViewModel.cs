using System;
using System.Collections.ObjectModel;
using System.Linq;
using Metasia.Core.Coordinate;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
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
    private MetaNumberParam<double> _target;
    private string _propertyDisplayName = string.Empty;
    private string _propertyValue = string.Empty;
    private double _min = double.MinValue;
    private double _max = double.MaxValue;
    private double _recommendedMin = double.MinValue;
    private double _recommendedMax = double.MaxValue;
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;
    public MetaNumberParamPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier, 
        IEditCommandManager editCommandManager,
        MetaNumberParam<double> target, 
        double min = double.MinValue, 
        double max = double.MaxValue, 
        double recommendedMin = double.MinValue, 
        double recommendedMax = double.MaxValue)
    {
        _target = target;
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = "100(仮)";
        _min = min;
        _max = max;
        _recommendedMin = recommendedMin;
        _recommendedMax = recommendedMax;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
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
            CoordPoints.Add(new MetaNumberCoordPointViewModel(this, target.Params[i], pointType, _min, _max, _recommendedMin, _recommendedMax));
        }
    }

    public void UpdatePointValue(CoordPoint targetCoordPoint, double value)
    {
        var targetPoint = _target.Params.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
        if (targetPoint is not null)
        {
            var command = TimelineInteractor.CreateCoordPointsValueChangeCommand(_propertyDisplayName, targetCoordPoint, value, _selectionState.SelectedClips);
            if (command is not null)
            {
                _editCommandManager.Execute(command);
            }
        }
    }
    
}
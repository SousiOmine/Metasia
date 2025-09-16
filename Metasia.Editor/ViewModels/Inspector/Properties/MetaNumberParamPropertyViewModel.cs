using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Metasia.Core.Coordinate;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
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

    public string PropertyValueText
    {
        get => _propertyValueText;
        set => this.RaiseAndSetIfChanged(ref _propertyValueText, value);
    }

    public MetaNumberParam<double> PropertyValue
    {
        get => _propertyValue;
        set => this.RaiseAndSetIfChanged(ref _propertyValue, value);
    }

    public string PropertyIdentifier
    {
        get => _propertyIdentifier;
        set => this.RaiseAndSetIfChanged(ref _propertyIdentifier, value);
    }

    private string _propertyDisplayName = string.Empty;
    private string _propertyValueText = string.Empty;
    private string _propertyIdentifier = string.Empty;
    private MetaNumberParam<double> _propertyValue;
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
        _propertyDisplayName = propertyIdentifier;
        _propertyValueText = "100(仮)";
        _propertyIdentifier = propertyIdentifier;
        _propertyValue = target;
        _min = min;
        _max = max;
        _recommendedMin = recommendedMin;
        _recommendedMax = recommendedMax;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        RestructureParams();
    }

    public void UpdatePointValue(CoordPoint targetCoordPoint, double beforeValue, double value)
    {
        var targetPoint = _propertyValue.Params.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
        if (targetPoint is not null)
        {
            var command = TimelineInteractor.CreateCoordPointsValueChangeCommand(_propertyDisplayName, targetCoordPoint, beforeValue, value, _selectionState.SelectedClips);
            if (command is not null)
            {
                _editCommandManager.Execute(command);
            }
        }
    }

    public void PreviewUpdatePointValue(CoordPoint targetCoordPoint, double beforeValue, double value)
    {
        var targetPoint = _propertyValue.Params.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
        if (targetPoint is not null)
        {
            var command = TimelineInteractor.CreateCoordPointsValueChangeCommand(_propertyDisplayName, targetCoordPoint, beforeValue, value, _selectionState.SelectedClips);
            if (command is not null)
            {
                _editCommandManager.PreviewExecute(command);
            }
        }
    }

    public void UpdatePointFrame(CoordPoint targetCoordPoint, int beforeFrame, int frame)
    {
        var targetPoint = _propertyValue.Params.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
        if (targetPoint is not null)
        {
            var command = new CoordPointFrameChangeCommand(_propertyValue, targetCoordPoint, beforeFrame, frame);
            if (command is not null)
            {
                _editCommandManager.Execute(command);
            }
        }
    }

    private void RestructureParams()
    {
        CoordPoints.Clear();
        for (int i = 0; i < _propertyValue.Params.Count; i++)
        {
            MetaNumberCoordPointViewModel.PointType pointType;
            if (_propertyValue.Params.Count == 1)
            {
                pointType = MetaNumberCoordPointViewModel.PointType.Single;
            }
            else if (i == 0)
            {
                pointType = MetaNumberCoordPointViewModel.PointType.Start;
            }
            else if (i == _propertyValue.Params.Count - 1)
            {
                pointType = MetaNumberCoordPointViewModel.PointType.End;
            }
            else
            {
                pointType = MetaNumberCoordPointViewModel.PointType.Mid;
            }
            CoordPoints.Add(new MetaNumberCoordPointViewModel(this, _propertyValue.Params[i], pointType, _min, _max, _recommendedMin, _recommendedMax));
        }
    }
    
}
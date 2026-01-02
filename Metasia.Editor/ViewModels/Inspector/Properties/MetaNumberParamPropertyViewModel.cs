using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
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

    public bool IsMovable
    {
        get => _isMovable;
        set => this.RaiseAndSetIfChanged(ref _isMovable, value);
    }

    public ICommand AddMoveCommand { get; }
    public ICommand RemoveMoveCommand { get; }

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
    private IProjectState _projectState;
    private bool _isMovable;
    public MetaNumberParamPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
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
        _projectState = projectState;
        _projectState.TimelineChanged += OnTimelineChanged;

        AddMoveCommand = ReactiveCommand.Create(AddMove);
        
        RemoveMoveCommand = ReactiveCommand.Create(RemoveMove);
        
        RestructureParams();
    }

    public void UpdatePointValue(CoordPoint targetCoordPoint, double beforeValue, double value)
    {
        var points = new List<CoordPoint> { _propertyValue.StartPoint, _propertyValue.EndPoint };
        points.AddRange(_propertyValue.Params);
        var targetPoint = points.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
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
        var points = new List<CoordPoint> { _propertyValue.StartPoint, _propertyValue.EndPoint };
        points.AddRange(_propertyValue.Params);
        var targetPoint = points.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
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
        var points = new List<CoordPoint> { _propertyValue.StartPoint, _propertyValue.EndPoint };
        points.AddRange(_propertyValue.Params);
        var targetPoint = points.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
        if (targetPoint is not null)
        {
            var command = new CoordPointFrameChangeCommand(_propertyValue, targetCoordPoint, beforeFrame, frame);
            if (command is not null)
            {
                _editCommandManager.Execute(command);
            }
        }
    }

    private void OnTimelineChanged()
    {
        RestructureParams();
    }

    private void RestructureParams()
    {
        IsMovable = _propertyValue.IsMovable;
        var desiredPoints = new List<(CoordPoint, MetaNumberCoordPointViewModel.PointType)>
        {

        };
        if (_propertyValue.IsMovable)
        {
            desiredPoints.Add((_propertyValue.StartPoint, MetaNumberCoordPointViewModel.PointType.Start));
            foreach (var param in _propertyValue.Params)
            {
                desiredPoints.Add((param, MetaNumberCoordPointViewModel.PointType.Mid));
            }
            desiredPoints.Add((_propertyValue.EndPoint, MetaNumberCoordPointViewModel.PointType.End));
        }
        else
        {
            desiredPoints.Add((_propertyValue.StartPoint, MetaNumberCoordPointViewModel.PointType.Single));
        }

        // 不要になったポイントは削除
        for (int i = CoordPoints.Count - 1; i >= 0; i--)
        {
            var existingPoint = CoordPoints[i];
            var isDesired = desiredPoints.Any(p => p.Item1.Id == existingPoint.TargetId);
            if (!isDesired)
            {
                CoordPoints.RemoveAt(i);
            }
        }

        for (int i = 0; i < desiredPoints.Count; i++)
        {
            var (point, type) = desiredPoints[i];
            
            bool existing = CoordPoints.Any(p => p.TargetId == point.Id);
            if (existing)
            {
                //すでに存在するポイントViewModelのインデックスを探索
                var existingPoint = CoordPoints.First(p => p.TargetId == point.Id);
                int existingIndex = CoordPoints.IndexOf(existingPoint);
                // 既存のポイントが正しい位置にない場合は移動
                if (existingIndex != i)
                {
                    var pointToMove = CoordPoints[existingIndex];
                    CoordPoints.Move(existingIndex, i);
                }
            }
            else
            {
                // 新しいポイントを追加
                var newPoint = new MetaNumberCoordPointViewModel(this, point, type, _min, _max, _recommendedMin, _recommendedMax);
                CoordPoints.Insert(i, newPoint);
            }
        }
        for (int i = 0; i < CoordPoints.Count; i++)
        {
            var (point, type) = desiredPoints[i];
            CoordPoints[i].RefreshFromTarget(point, type, _min, _max, _recommendedMin, _recommendedMax);
        }
    }


    private void AddMove()
    {
        if (!_propertyValue.IsMovable)
        {
            var command = new MetaNumberParamIsMovableChangeCommand<double>(_propertyValue, true);
            _editCommandManager.Execute(command);
        }
    }
    
    private void RemoveMove()
    {
        if (_propertyValue.IsMovable)
        {
            var command = new MetaNumberParamIsMovableChangeCommand<double>(_propertyValue, false);
            _editCommandManager.Execute(command);
        }
    }

}
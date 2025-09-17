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
    private IProjectState _projectState;
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

    private void OnTimelineChanged()
    {
        RestructureParams();
    }

    private void RestructureParams()
    {
        var desiredPoints = _propertyValue.Params.ToList();
        var desiredIds = desiredPoints.Select(p => p.Id).ToHashSet();

        // Remove VMs that no longer exist
        for (int idx = CoordPoints.Count - 1; idx >= 0; idx--)
        {
            var vm = CoordPoints[idx];
            if (!desiredIds.Contains(vm.TargetId))
            {
                CoordPoints.RemoveAt(idx);
            }
        }

        // Reorder existing VMs and insert missing ones according to desired order
        for (int i = 0; i < desiredPoints.Count; i++)
        {
            var point = desiredPoints[i];
            var desiredId = point.Id;

            MetaNumberCoordPointViewModel.PointType ComputePointType(int index, int count)
            {
                if (count == 1) return MetaNumberCoordPointViewModel.PointType.Single;
                if (index == 0) return MetaNumberCoordPointViewModel.PointType.Start;
                if (index == count - 1) return MetaNumberCoordPointViewModel.PointType.End;
                return MetaNumberCoordPointViewModel.PointType.Mid;
            }

            var desiredType = ComputePointType(i, desiredPoints.Count);

            if (i < CoordPoints.Count && CoordPoints[i].TargetId == desiredId)
            {
                // Update in place
                CoordPoints[i].RefreshFromTarget(point, desiredType, _min, _max, _recommendedMin, _recommendedMax);
            }
            else
            {
                // Find existing VM with the desired id
                int existingIndex = -1;
                for (int j = 0; j < CoordPoints.Count; j++)
                {
                    if (CoordPoints[j].TargetId == desiredId)
                    {
                        existingIndex = j;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    // Move to correct position and refresh
                    if (existingIndex != i)
                    {
                        CoordPoints.Move(existingIndex, i);
                    }
                    CoordPoints[i].RefreshFromTarget(point, desiredType, _min, _max, _recommendedMin, _recommendedMax);
                }
                else
                {
                    // Insert new VM at the correct position
                    var newVm = new MetaNumberCoordPointViewModel(this, point, desiredType, _min, _max, _recommendedMin, _recommendedMax);
                    if (i <= CoordPoints.Count)
                    {
                        CoordPoints.Insert(i, newVm);
                    }
                    else
                    {
                        CoordPoints.Add(newVm);
                    }
                }
            }
        }

        // Trim any trailing items if collection is longer than desired
        while (CoordPoints.Count > desiredPoints.Count)
        {
            CoordPoints.RemoveAt(CoordPoints.Count - 1);
        }
    }
    
}
using System;
using System.Windows.Input;
using Metasia.Core.Coordinate;
using Metasia.Core.Coordinate.InterpolationLogic;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties.Components;

public class InterpolationLogicMenuItemViewModel : ViewModelBase
{
    public string Header
    {
        get => _header;
        set => this.RaiseAndSetIfChanged(ref _header, value);
    }

    public ICommand Command { get; }
    private string _header = string.Empty;

    private CoordPoint _targetCoordPoint;
    private Type _interpolationLogicType;

    private readonly IProjectState _projectState;

    public InterpolationLogicMenuItemViewModel(
        CoordPoint coordPoint, 
        Type interpolationLogicType,
        IProjectState projectState
        )
    {
        if (!typeof(IInterpolationLogic).IsAssignableFrom(interpolationLogicType))
        {
            throw new ArgumentException("interpolationLogicType must implement IInterpolationLogic");
        }
        
        _targetCoordPoint = coordPoint;
        _interpolationLogicType = interpolationLogicType;
        _projectState = projectState;

        Header = interpolationLogicType.Name;

        Command = ReactiveCommand.Create(OnSelected);
        
        _projectState.TimelineChanged += OnTimelineChanged;

        OnTimelineChanged();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _projectState.TimelineChanged -= OnTimelineChanged;
        }
        base.Dispose(disposing);
    }

    private void OnSelected()
    {
        Console.WriteLine($"Selected: {_interpolationLogicType.Name}");
    }

    private void OnTimelineChanged()
    {
        if (_targetCoordPoint.InterpolationLogic?.GetType() == _interpolationLogicType)
        {
            Header = " ・ " + _interpolationLogicType.Name;
        }
        else
        {
            Header = "   " + _interpolationLogicType.Name;
        }
    }
}

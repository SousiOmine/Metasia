using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Metasia.Core.Coordinate;
using Metasia.Core.Coordinate.InterpolationLogic;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
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
    private readonly IEditCommandManager _editCommandManager;

    public InterpolationLogicMenuItemViewModel(
        CoordPoint coordPoint,
        Type interpolationLogicType,
        IProjectState projectState,
        IEditCommandManager editCommandManager
        )
    {
        if (!typeof(InterpolationLogicBase).IsAssignableFrom(interpolationLogicType))
        {
            throw new ArgumentException("interpolationLogicType must implement InterpolationLogicBase");
        }

        _targetCoordPoint = coordPoint;
        _interpolationLogicType = interpolationLogicType;
        _projectState = projectState;
        _editCommandManager = editCommandManager;

        Header = GetHeader(interpolationLogicType);

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

        var command = new InterpolationLogicChangeCommand(
            (InterpolationLogicBase)Activator.CreateInstance(_interpolationLogicType)!,
            _targetCoordPoint
        );

        _editCommandManager.Execute(command);
    }

    private void OnTimelineChanged()
    {
        if (_targetCoordPoint.InterpolationLogic?.GetType() == _interpolationLogicType)
        {
            Header = " ・ " + GetHeader(_interpolationLogicType);
        }
        else
        {
            Header = "   " + GetHeader(_interpolationLogicType);
        }
    }

    private string GetHeader(Type type)
    {
        return type.Name switch
        {
            nameof(LinearLogic) => "線形移動",
            nameof(EaseInLogic) => "EaseIn",
            nameof(EaseOutLogic) => "EaseOut",
            nameof(EaseInOutLogic) => "EaseInOut",
            nameof(EaseInStrongLogic) => "EaseIn(強)",
            nameof(EaseOutStrongLogic) => "EaseOut(強)",
            nameof(EaseInOutStrongLogic) => "EaseInOut(強)",
            nameof(TeleportLogic) => "瞬間移動",
            _ => type.Name
        };
    }
}

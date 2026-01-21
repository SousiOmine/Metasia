using System;
using System.Timers;
using Avalonia.Media;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class ColorPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor.Equals(value))
            {
                return;
            }

            var previous = _selectedColor;
            _selectedColor = value;
            this.RaisePropertyChanged();

            TryValueEnter(previous);
        }
    }

    public string PropertyIdentifier
    {
        get => _propertyIdentifier;
        set => this.RaiseAndSetIfChanged(ref _propertyIdentifier, value);
    }

    private string _propertyDisplayName = string.Empty;
    private string _propertyIdentifier = string.Empty;
    private Color _selectedColor;
    private ColorRgb8 _propertyValue;
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;
    private const double _valueEnterThreshold = 0.2;

    private Timer? _valueEnterTimer;
    private bool _isValueEnteringFlag = false;
    private Color _beforeColor;
    private bool _suppressChangeEvents = false;

    public ColorPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        ColorRgb8 target)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyIdentifier = propertyIdentifier;
        _propertyValue = target;
        _selectedColor = ToAvaloniaColor(target);
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    private void TryValueEnter(Color previousValue)
    {
        if (_suppressChangeEvents)
        {
            return;
        }

        if (!_isValueEnteringFlag)
        {
            _beforeColor = previousValue;
            _isValueEnteringFlag = true;
        }

        if (SelectedColor.Equals(_beforeColor))
        {
            _isValueEnteringFlag = false;
            return;
        }

        EnsureValueEnterTimer();
        ValueChanging();

        if (_valueEnterTimer is not null)
        {
            _valueEnterTimer.Stop();
            _valueEnterTimer.Start();
        }
    }

    private void EnsureValueEnterTimer()
    {
        if (_valueEnterTimer is not null)
        {
            return;
        }

        _valueEnterTimer = new Timer(_valueEnterThreshold * 1000)
        {
            AutoReset = false
        };
        _valueEnterTimer.Elapsed += (_, _) =>
        {
            if (!SelectedColor.Equals(_beforeColor))
            {
                UpdateColorValue(_beforeColor, SelectedColor);
            }

            _isValueEnteringFlag = false;
        };
    }

    private void ValueChanging()
    {
        PreviewUpdateColorValue(_beforeColor, SelectedColor);
    }

    private void UpdateColorValue(Color beforeValue, Color value)
    {
        var beforeColor = ToColorRgb8(beforeValue);
        var afterColor = ToColorRgb8(value);
        _propertyValue = afterColor;

        var command = CreateColorValueChangeCommand(beforeColor, afterColor);
        if (command is not null)
        {
            _editCommandManager.Execute(command);
        }
    }

    private void PreviewUpdateColorValue(Color beforeValue, Color value)
    {
        var beforeColor = ToColorRgb8(beforeValue);
        var afterColor = ToColorRgb8(value);

        var command = CreateColorValueChangeCommand(beforeColor, afterColor);
        if (command is not null)
        {
            _editCommandManager.PreviewExecute(command);
        }
    }

    private IEditCommand? CreateColorValueChangeCommand(ColorRgb8 beforeValue, ColorRgb8 value)
    {
        return TimelineInteractor.CreateColorValueChangeCommand(
            _propertyIdentifier,
            beforeValue,
            value,
            _selectionState.SelectedClips);
    }

    private static Color ToAvaloniaColor(ColorRgb8 value)
    {
        return Color.FromRgb(value.R, value.G, value.B);
    }

    private static ColorRgb8 ToColorRgb8(Color value)
    {
        return new ColorRgb8(value.R, value.G, value.B);
    }
}

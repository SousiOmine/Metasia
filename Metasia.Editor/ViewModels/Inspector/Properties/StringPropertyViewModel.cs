using System;
using System.Timers;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class StringPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public string PropertyValue
    {
        get => _propertyValue;
        set
        {
            if (_propertyValue == value)
            {
                return;
            }

            var previous = _propertyValue;
            _propertyValue = value;
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
    private string _propertyValue = string.Empty;
    private string _propertyIdentifier = string.Empty;
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;
    private IProjectState _projectState;
    private const double _valueEnterThreshold = 0.2;

    private Timer? _valueEnterTimer;
    private bool _isValueEnteringFlag = false;
    private string _beforeValue = string.Empty;
    private bool _suppressChangeEvents = false;

    public StringPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        string target)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = target;
        _propertyIdentifier = propertyIdentifier;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    private void TryValueEnter(string previousValue)
    {
        if (_suppressChangeEvents)
        {
            return;
        }

        if (!_isValueEnteringFlag)
        {
            _beforeValue = previousValue;
            _isValueEnteringFlag = true;
        }

        if (PropertyValue == _beforeValue)
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
            if (PropertyValue != _beforeValue)
            {
                UpdateStringValue(_beforeValue, PropertyValue);
            }

            _isValueEnteringFlag = false;
        };
    }

    private void ValueChanging()
    {
        PreviewUpdateStringValue(_beforeValue, PropertyValue);
    }

    private void UpdateStringValue(string beforeValue, string value)
    {
        var command = CreateStringValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.Execute(command);
        }
    }

    private void PreviewUpdateStringValue(string beforeValue, string value)
    {
        var command = CreateStringValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.PreviewExecute(command);
        }
    }

    private IEditCommand? CreateStringValueChangeCommand(string beforeValue, string value)
    {
        return TimelineInteractor.CreateStringValueChangeCommand(
            _propertyIdentifier,
            beforeValue,
            value,
            _selectionState.SelectedClips);
    }
}
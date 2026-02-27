using System;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class BoolPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public bool PropertyValue
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

            UpdateBoolValue(previous, value);
        }
    }

    public string PropertyIdentifier
    {
        get => _propertyIdentifier;
        set => this.RaiseAndSetIfChanged(ref _propertyIdentifier, value);
    }

    private string _propertyDisplayName = string.Empty;
    private bool _propertyValue;
    private string _propertyIdentifier = string.Empty;
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;

    public BoolPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        bool target)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = target;
        _propertyIdentifier = propertyIdentifier;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
    }

    private void UpdateBoolValue(bool beforeValue, bool afterValue)
    {
        var command = CreateBoolValueChangeCommand(beforeValue, afterValue);
        if (command is not null)
        {
            _editCommandManager.Execute(command);
        }
    }

    private IEditCommand? CreateBoolValueChangeCommand(bool beforeValue, bool afterValue)
    {
        return TimelineInteractor.CreateBoolValueChangeCommand(
            _propertyIdentifier,
            beforeValue,
            afterValue,
            _selectionState.SelectedClips);
    }
}
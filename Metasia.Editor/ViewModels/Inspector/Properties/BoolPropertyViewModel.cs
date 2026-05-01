using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Abstractions.States;
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
    private bool _allowMultiClipApply;
    private IMetasiaObject? _owner;

    public BoolPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        bool target,
        bool allowMultiClipApply = true,
        IMetasiaObject? owner = null)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = target;
        _propertyIdentifier = propertyIdentifier;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _allowMultiClipApply = allowMultiClipApply;
        _owner = owner;
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
        if (_allowMultiClipApply)
        {
            return TimelineInteractor.CreateBoolValueChangeCommand(
                _propertyIdentifier,
                beforeValue,
                afterValue,
                _selectionState.SelectedClips);
        }

        var owner = _owner ?? _selectionState.CurrentSelectedClip ?? _selectionState.SelectedClips.FirstOrDefault();
        if (owner is null) return null;

        return new BoolValueChangeCommand([new BoolValueChangeCommand.BoolValueChangeInfo(owner, _propertyIdentifier, beforeValue, afterValue)]);
    }
}
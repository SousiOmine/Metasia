using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Abstractions.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaEnumParamPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public ObservableCollection<string> Options { get; } = new();

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var oldValue = _selectedIndex;
            this.RaiseAndSetIfChanged(ref _selectedIndex, value);
            if (oldValue != value && _propertyValue is not null)
            {
                UpdateSelectedIndex(oldValue, value);
            }
        }
    }

    public MetaEnumParam? PropertyValue
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
    private int _selectedIndex = 0;
    private string _propertyIdentifier = string.Empty;
    private MetaEnumParam? _propertyValue;
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;
    private IProjectState _projectState;
    private bool _allowMultiClipApply;
    private IMetasiaObject? _owner;

    public MetaEnumParamPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        MetaEnumParam target,
        bool allowMultiClipApply = true,
        IMetasiaObject? owner = null)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyIdentifier = propertyIdentifier;
        _propertyValue = target;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
        _allowMultiClipApply = allowMultiClipApply;
        _owner = owner;

        RefreshOptions();
    }

    private void UpdateSelectedIndex(int oldIndex, int newIndex)
    {
        if (_propertyValue is not null && oldIndex != newIndex)
        {
            var command = new EnumValueChangeCommand(_propertyIdentifier, _propertyValue, oldIndex, newIndex);
            if (command is not null)
            {
                _editCommandManager.Execute(command);
            }
        }
    }

    public void RefreshOptions()
    {
        if (_propertyValue is not null)
        {
            Options.Clear();
            foreach (var option in _propertyValue.Options)
            {
                Options.Add(option);
            }
            _selectedIndex = _propertyValue.SelectedIndex;
        }
    }
}

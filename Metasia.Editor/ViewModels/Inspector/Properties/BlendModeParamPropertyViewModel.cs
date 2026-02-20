using System;
using System.Collections.ObjectModel;
using System.Linq;
using Metasia.Core.Render;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class BlendModeParamPropertyViewModel : ViewModelBase
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

    public BlendModeParam? PropertyValue
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
    private BlendModeParam? _propertyValue;
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;
    private bool _optionsInitialized = false;
    private bool _disposed = false;

    public BlendModeParamPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        BlendModeParam target)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyIdentifier = propertyIdentifier;
        _propertyValue = target;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
        _projectState.TimelineChanged += OnTimelineChanged;

        InitializeOptions();
        UpdateSelectedIndexFromValue();
    }

    private void InitializeOptions()
    {
        if (!_optionsInitialized)
        {
            Options.Clear();
            foreach (var option in BlendModeParam.AllOptions)
            {
                Options.Add(option.ToString());
            }
            _optionsInitialized = true;
        }
    }

    private void UpdateSelectedIndex(int oldIndex, int newIndex)
    {
        if (_propertyValue is not null && oldIndex != newIndex && newIndex >= 0 && newIndex < BlendModeParam.AllOptions.Count)
        {
            var oldValue = BlendModeParam.AllOptions[oldIndex];
            var newValue = BlendModeParam.AllOptions[newIndex];
            var command = TimelineInteractor.CreateBlendModeValueChangeCommand(
                _propertyIdentifier,
                oldValue,
                newValue,
                _selectionState.SelectedClips);
            if (command is not null)
            {
                _editCommandManager.Execute(command);
            }
        }
    }

    private void UpdateSelectedIndexFromValue()
    {
        if (_propertyValue is not null)
        {
            var newIndex = BlendModeParam.AllOptions.ToList().IndexOf(_propertyValue.Value);
            if (newIndex >= 0)
            {
                _selectedIndex = newIndex;
                this.RaisePropertyChanged(nameof(SelectedIndex));
            }
        }
    }

    private void OnTimelineChanged()
    {
        UpdateSelectedIndexFromValue();
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected new virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _projectState.TimelineChanged -= OnTimelineChanged;
            }

            _disposed = true;
        }
    }
}
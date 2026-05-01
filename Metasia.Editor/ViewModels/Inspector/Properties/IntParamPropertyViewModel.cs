using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services.Notification;
using ReactiveUI;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class IntParamPropertyViewModel : ViewModelBase, IDisposable
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public string PropertyValueText
    {
        get => _propertyValueText;
        set
        {
            if (_propertyValueText == value)
            {
                return;
            }

            var previous = _propertyValueText;
            _propertyValueText = value;
            this.RaisePropertyChanged();

            TryValueEnter(previous);
        }
    }

    public int PropertyValue
    {
        get => _propertyValue;
        set
        {
            if (_suppressChangeEvents)
            {
                _propertyValue = value;
                _propertyValueText = value.ToString(CultureInfo.InvariantCulture);
                _sliderValue = value;
                return;
            }

            if (_propertyValue == value)
            {
                return;
            }

            var previous = _propertyValue;
            _propertyValue = value;
            _propertyValueText = value.ToString(CultureInfo.InvariantCulture);

            if (_sliderValue != value)
            {
                _sliderValue = value;
                this.RaisePropertyChanged(nameof(SliderValue));
            }

            this.RaisePropertyChanged();

            TryValueEnter(previous.ToString(CultureInfo.InvariantCulture));
        }
    }

    public string PropertyIdentifier
    {
        get => _propertyIdentifier;
        set => this.RaiseAndSetIfChanged(ref _propertyIdentifier, value);
    }

    public int Min
    {
        get => _min;
        set => this.RaiseAndSetIfChanged(ref _min, value);
    }

    public int Max
    {
        get => _max;
        set => this.RaiseAndSetIfChanged(ref _max, value);
    }

    public int RecommendMin
    {
        get => _recommendMin;
        set => this.RaiseAndSetIfChanged(ref _recommendMin, value);
    }

    public int RecommendMax
    {
        get => _recommendMax;
        set => this.RaiseAndSetIfChanged(ref _recommendMax, value);
    }

    public double SliderValue
    {
        get => _sliderValue;
        set
        {
            var clamped = Math.Max(_recommendMin, Math.Min(_recommendMax, value));
            if (Math.Abs(_sliderValue - clamped) < double.Epsilon)
            {
                return;
            }

            _sliderValue = clamped;
            this.RaisePropertyChanged();

            var intVal = (int)Math.Round(clamped);
            if (_propertyValue != intVal)
            {
                PropertyValue = intVal;
            }
        }
    }

    private string _propertyDisplayName = string.Empty;
    private string _propertyValueText = string.Empty;
    private string _propertyIdentifier = string.Empty;
    private int _propertyValue;
    private double _sliderValue;
    private int _min = int.MinValue;
    private int _max = int.MaxValue;
    private int _recommendMin = int.MinValue;
    private int _recommendMax = int.MaxValue;
    private MetaIntParam _targetParam;
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;
    private IProjectState _projectState;
    private bool _allowMultiClipApply;
    private IMetasiaObject? _owner;

    private bool _isInteracting = false;
    private int _beforeValue = 0;
    private bool _suppressChangeEvents = false;

    public IntParamPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        MetaIntParam target,
        int min = int.MinValue,
        int max = int.MaxValue,
        int recommendMin = int.MinValue,
        int recommendMax = int.MaxValue,
        bool allowMultiClipApply = true,
        IMetasiaObject? owner = null)
    {
        _propertyDisplayName = propertyIdentifier;
        _targetParam = target;
        _propertyValue = target.Value;
        _propertyValueText = target.Value.ToString(CultureInfo.InvariantCulture);
        _sliderValue = target.Value;
        _propertyIdentifier = propertyIdentifier;
        _min = min;
        _max = max;
        _recommendMin = recommendMin;
        _recommendMax = recommendMax;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
        _allowMultiClipApply = allowMultiClipApply;
        _owner = owner;

        _projectState.TimelineChanged += OnTimelineChanged;

        InteractionStartedCommand = ReactiveCommand.Create(StartInteraction);
        InteractionCompletedCommand = ReactiveCommand.Create(EndInteraction);
    }

    public ICommand InteractionStartedCommand { get; }
    public ICommand InteractionCompletedCommand { get; }

    private void OnTimelineChanged()
    {
        RefreshPropertyValue();
    }

    private void RefreshPropertyValue()
    {
        var clip = _selectionState.CurrentSelectedClip ?? _selectionState.SelectedClips.FirstOrDefault();
        if (clip is null) return;

        var properties = ObjectPropertyFinder.FindEditableProperties(clip);
        var property = properties.FirstOrDefault(x => x.Identifier == _propertyIdentifier && x.Type == typeof(MetaIntParam));
        if (property?.PropertyValue is MetaIntParam intParam)
        {
            _suppressChangeEvents = true;
            _propertyValue = intParam.Value;
            _propertyValueText = intParam.Value.ToString(CultureInfo.InvariantCulture);
            _sliderValue = intParam.Value;

            this.RaisePropertyChanged(nameof(PropertyValue));
            this.RaisePropertyChanged(nameof(PropertyValueText));
            this.RaisePropertyChanged(nameof(SliderValue));
            _suppressChangeEvents = false;
        }
    }

    private void TryValueEnter(string previousText)
    {
        if (_suppressChangeEvents)
        {
            return;
        }

        if (!_isInteracting)
        {
            return;
        }

        if (!int.TryParse(_propertyValueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int currentValue))
        {
            return;
        }

        currentValue = Math.Max(_min, Math.Min(_max, currentValue));
        if (_propertyValue != currentValue)
        {
            try
            {
                _suppressChangeEvents = true;
                _propertyValue = currentValue;
                _propertyValueText = currentValue.ToString(CultureInfo.InvariantCulture);

                if (Math.Abs(_sliderValue - currentValue) > double.Epsilon)
                {
                    _sliderValue = currentValue;
                    this.RaisePropertyChanged(nameof(SliderValue));
                }

                this.RaisePropertyChanged(nameof(PropertyValue));
                this.RaisePropertyChanged(nameof(PropertyValueText));
            }
            finally
            {
                _suppressChangeEvents = false;
            }
        }

        PreviewUpdateIntValue(_beforeValue, currentValue);
    }

    private void StartInteraction()
    {
        _isInteracting = true;
        _beforeValue = _propertyValue;
    }

    private void EndInteraction()
    {
        _isInteracting = false;
        var beforeValue = _beforeValue;

        if (int.TryParse(_propertyValueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int currentValue))
        {
            currentValue = Math.Max(_min, Math.Min(_max, currentValue));
            if (currentValue != beforeValue)
            {
                UpdateIntValue(beforeValue, currentValue);
            }
        }
    }

    private void UpdateIntValue(int beforeValue, int value)
    {
        var command = CreateIntValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.Execute(command);
        }
    }

    private void PreviewUpdateIntValue(int beforeValue, int value)
    {
        var command = CreateIntValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.PreviewExecute(command);
        }
    }

    private IEditCommand? CreateIntValueChangeCommand(int beforeValue, int value)
    {
        if (_allowMultiClipApply)
        {
            return TimelineInteractor.CreateIntValueChangeCommand(
                _propertyIdentifier,
                beforeValue,
                value,
                _selectionState.SelectedClips);
        }

        var owner = _owner ?? _selectionState.CurrentSelectedClip ?? _selectionState.SelectedClips.FirstOrDefault();
        if (owner is null) return null;

        var valueDifference = value - beforeValue;
        return new IntValueChangeCommand([new IntValueChangeCommand.IntValueChangeInfo(owner, _propertyIdentifier, valueDifference)]);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_projectState != null)
            {
                _projectState.TimelineChanged -= OnTimelineChanged;
            }
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Timers;
using Avalonia.Threading;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaDoubleParamPropertyViewModel : ViewModelBase, IDisposable
{
    private bool _disposed = false;

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

    public double PropertyValue
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

            if (Math.Abs(_propertyValue - value) < double.Epsilon)
            {
                return;
            }

            var previous = _propertyValue;
            _propertyValue = value;
            _propertyValueText = value.ToString(CultureInfo.InvariantCulture);

            if (Math.Abs(_sliderValue - value) > double.Epsilon)
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

    public double Min
    {
        get => _min;
        set => this.RaiseAndSetIfChanged(ref _min, value);
    }

    public double Max
    {
        get => _max;
        set => this.RaiseAndSetIfChanged(ref _max, value);
    }

    public double RecommendMin
    {
        get => _recommendMin;
        set => this.RaiseAndSetIfChanged(ref _recommendMin, value);
    }

    public double RecommendMax
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

            if (Math.Abs(_propertyValue - clamped) > double.Epsilon)
            {
                PropertyValue = clamped;
            }

            if (_isValueEnteringFlag)
            {
                PreviewUpdateDoubleValue(_beforeValue, _propertyValue);
            }
        }
    }

    private string _propertyDisplayName = string.Empty;
    private string _propertyValueText = string.Empty;
    private string _propertyIdentifier = string.Empty;
    private double _propertyValue;
    private double _sliderValue;
    private double _min = double.MinValue;
    private double _max = double.MaxValue;
    private double _recommendMin = double.MinValue;
    private double _recommendMax = double.MaxValue;
    private MetaDoubleParam _targetParam;
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;
    private IProjectState _projectState;
    private const double _valueEnterThreshold = 0.2;

    private Timer? _valueEnterTimer;
    private bool _isValueEnteringFlag = false;
    private double _beforeValue = 0;
    private bool _suppressChangeEvents = false;

    public MetaDoubleParamPropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        MetaDoubleParam target,
        double min = double.MinValue,
        double max = double.MaxValue,
        double recommendMin = double.MinValue,
        double recommendMax = double.MaxValue)
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

        _projectState.TimelineChanged += OnTimelineChanged;
    }

    private void OnTimelineChanged()
    {
        RefreshPropertyValue();
    }

    private void RefreshPropertyValue()
    {
        var clip = _selectionState.CurrentSelectedClip ?? _selectionState.SelectedClips.FirstOrDefault();
        if (clip is null) return;

        var properties = ObjectPropertyFinder.FindEditableProperties(clip);
        var property = properties.FirstOrDefault(x => x.Identifier == _propertyIdentifier && x.Type == typeof(MetaDoubleParam));
        if (property?.PropertyValue is MetaDoubleParam doubleParam)
        {
            _suppressChangeEvents = true;
            _propertyValue = doubleParam.Value;
            _propertyValueText = doubleParam.Value.ToString(CultureInfo.InvariantCulture);
            _sliderValue = doubleParam.Value;

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

        if (!_isValueEnteringFlag)
        {
            double.TryParse(previousText, NumberStyles.Float, CultureInfo.InvariantCulture, out _beforeValue);
            _isValueEnteringFlag = true;
        }

        if (!double.TryParse(_propertyValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
        {
            return;
        }

        if (Math.Abs(currentValue - _beforeValue) < double.Epsilon)
        {
            _isValueEnteringFlag = false;
            return;
        }

        currentValue = Math.Max(_min, Math.Min(_max, currentValue));
        if (Math.Abs(_propertyValue - currentValue) > double.Epsilon)
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
            Dispatcher.UIThread.Post(() =>
            {
                if (_disposed || !_isValueEnteringFlag)
                {
                    return;
                }

                if (double.TryParse(_propertyValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
                {
                    currentValue = Math.Max(_min, Math.Min(_max, currentValue));
                    if (Math.Abs(currentValue - _beforeValue) > double.Epsilon)
                    {
                        UpdateDoubleValue(_beforeValue, currentValue);
                    }
                }

                _isValueEnteringFlag = false;
            });
        };
    }

    private void ValueChanging()
    {
        if (double.TryParse(_propertyValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
        {
            currentValue = Math.Max(_min, Math.Min(_max, currentValue));
            PreviewUpdateDoubleValue(_beforeValue, currentValue);
        }
    }

    public void StartSliderPreview()
    {
        _valueEnterTimer?.Stop();

        if (!_isValueEnteringFlag)
        {
            _beforeValue = _propertyValue;
            _isValueEnteringFlag = true;
        }

        if (double.TryParse(_propertyValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
        {
            currentValue = Math.Max(_min, Math.Min(_max, currentValue));
            PreviewUpdateDoubleValue(_beforeValue, currentValue);
        }
    }

    public void EndSliderPreview()
    {
        _valueEnterTimer?.Stop();
        var beforeValue = _beforeValue;
        _isValueEnteringFlag = false;

        if (double.TryParse(_propertyValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
        {
            currentValue = Math.Max(_min, Math.Min(_max, currentValue));
            if (Math.Abs(currentValue - beforeValue) > double.Epsilon)
            {
                UpdateDoubleValue(beforeValue, currentValue);
            }
        }
    }

    private void UpdateDoubleValue(double beforeValue, double value)
    {
        var command = CreateDoubleValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.Execute(command);
        }
    }

    private void PreviewUpdateDoubleValue(double beforeValue, double value)
    {
        var command = CreateDoubleValueChangeCommand(beforeValue, value);
        if (command is not null)
        {
            _editCommandManager.PreviewExecute(command);
        }
    }

    private IEditCommand? CreateDoubleValueChangeCommand(double beforeValue, double value)
    {
        return TimelineInteractor.CreateDoubleValueChangeCommand(
            _propertyIdentifier,
            beforeValue,
            value,
            _selectionState.SelectedClips);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_projectState != null)
                {
                    _projectState.TimelineChanged -= OnTimelineChanged;
                }

                if (_valueEnterTimer != null)
                {
                    _valueEnterTimer.Stop();
                    _valueEnterTimer.Dispose();
                    _valueEnterTimer = null;
                }
            }

            _disposed = true;
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Timers;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class DoublePropertyViewModel : ViewModelBase, IDisposable
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
                // イベントを抑制している場合は内部状態のみ更新
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

            // SliderValueも更新
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
            // スライダーではRecommendMin/RecommendMaxの範囲を使用
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

            // スライダー操作中はプレビュー更新
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
    private ISelectionState _selectionState;
    private IEditCommandManager _editCommandManager;
    private IProjectState _projectState;
    private const double _valueEnterThreshold = 0.2;

    private Timer? _valueEnterTimer;
    private bool _isValueEnteringFlag = false;
    private double _beforeValue = 0;
    private bool _suppressChangeEvents = false;

    public DoublePropertyViewModel(
        ISelectionState selectionState,
        string propertyIdentifier,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        double target,
        double min = double.MinValue,
        double max = double.MaxValue,
        double recommendMin = double.MinValue,
        double recommendMax = double.MaxValue)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = target;
        _propertyValueText = target.ToString(CultureInfo.InvariantCulture);
        _sliderValue = target;
        _propertyIdentifier = propertyIdentifier;
        _min = min;
        _max = max;
        _recommendMin = recommendMin;
        _recommendMax = recommendMax;
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;

        // Undo/Redoイベントを購読してプロパティ値の変更を検知
        _editCommandManager.CommandExecuted += OnCommandChanged;
        _editCommandManager.CommandUndone += OnCommandChanged;
        _editCommandManager.CommandRedone += OnCommandChanged;
    }

    private void OnCommandChanged(object? sender, IEditCommand command)
    {
        // コマンドがこのプロパティに影響する場合のみ更新
        if (IsCommandAffectThisProperty(command))
        {
            RefreshPropertyValue();
        }
    }

    private bool IsCommandAffectThisProperty(IEditCommand command)
    {
        // DoubleValueChangeCommandかつ同じプロパティIDの場合のみtrueを返す
        if (command is not DoubleValueChangeCommand doubleCommand)
        {
            return false;
        }

        // リフレクションで_changeInfosの最初の要素を取得してPropertyIdentifierを比較
        var changeInfosField = typeof(DoubleValueChangeCommand).GetField("_changeInfos",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (changeInfosField?.GetValue(doubleCommand) is System.Collections.Generic.IEnumerable<DoubleValueChangeCommand.DoubleValueChangeInfo> changeInfos)
        {
            var firstInfo = changeInfos.FirstOrDefault();
            return firstInfo?.propertyIdentifier == _propertyIdentifier;
        }

        return false;
    }

    private void RefreshPropertyValue()
    {
        // 選択中のクリップから現在のプロパティ値を取得して表示を更新
        _suppressChangeEvents = true;

        if (_selectionState.SelectedClips.Count > 0)
        {
            // 最初の選択クリップの値を表示（複数選択の場合は最初のクリップに合わせる）
            var firstClip = _selectionState.SelectedClips[0];
            if (TimelineInteractor.TryGetDoubleProperty(_propertyIdentifier, firstClip, out double currentValue))
            {
                _propertyValue = currentValue;
                _propertyValueText = currentValue.ToString(CultureInfo.InvariantCulture);
                _sliderValue = currentValue;

                this.RaisePropertyChanged(nameof(PropertyValue));
                this.RaisePropertyChanged(nameof(PropertyValueText));
                this.RaisePropertyChanged(nameof(SliderValue));
            }
        }

        _suppressChangeEvents = false;
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

        // 値の範囲チェック
        currentValue = Math.Max(_min, Math.Min(_max, currentValue));
        if (Math.Abs(_propertyValue - currentValue) > double.Epsilon)
        {
            try
            {
                _suppressChangeEvents = true;
                _propertyValue = currentValue;
                _propertyValueText = currentValue.ToString(CultureInfo.InvariantCulture);

                // SliderValueも更新
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
            if (double.TryParse(_propertyValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
            {
                currentValue = Math.Max(_min, Math.Min(_max, currentValue));
                if (Math.Abs(currentValue - _beforeValue) > double.Epsilon)
                {
                    UpdateDoubleValue(_beforeValue, currentValue);
                }
            }

            _isValueEnteringFlag = false;
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
        if (double.TryParse(_propertyValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
        {
            currentValue = Math.Max(_min, Math.Min(_max, currentValue));
            if (Math.Abs(currentValue - _beforeValue) > double.Epsilon)
            {
                UpdateDoubleValue(_beforeValue, currentValue);
            }
        }

        _isValueEnteringFlag = false;
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
                // イベント購読を解除
                if (_editCommandManager != null)
                {
                    _editCommandManager.CommandExecuted -= OnCommandChanged;
                    _editCommandManager.CommandUndone -= OnCommandChanged;
                    _editCommandManager.CommandRedone -= OnCommandChanged;
                }

                // タイマーを破棄
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
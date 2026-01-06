using System;
using System.Timers;
using System.Windows.Input;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties.Components;

public class MetaNumberCoordPointViewModel : ViewModelBase
{
    public enum PointType
    {
        Start,
        Mid,
        End,
        Single
    }

    public double PointValue
    {
        get => _pointValue;
        set
        {
            var clamped = Math.Max(Min, Math.Min(Max, value));
            if (AreClose(_pointValue, clamped))
            {
                return;
            }

            var previous = _pointValue;
            _pointValue = clamped;
            this.RaisePropertyChanged();

            var sliderValue = Math.Min(RecommendedMax, Math.Max(RecommendedMin, _pointValue));
            if (!AreClose(_sliderPointValue, sliderValue))
            {
                _sliderPointValue = sliderValue;
                this.RaisePropertyChanged(nameof(SliderPointValue));
            }

            TryValueEnter(previous);
        }
    }

    public double SliderPointValue
    {
        get => _sliderPointValue;
        set
        {
            var clamped = Math.Max(RecommendedMin, Math.Min(RecommendedMax, value));
            if (AreClose(_sliderPointValue, clamped))
            {
                return;
            }

            _sliderPointValue = clamped;
            this.RaisePropertyChanged();

            if (!AreClose(_pointValue, clamped))
            {
                PointValue = clamped;
            }
        }
    }

    public int PointFrame
    {
        get => _pointFrame;
        set => this.RaiseAndSetIfChanged(ref _pointFrame, value);
    }
    public bool IsSingle
    {
        get => _isSingle;
        set => this.RaiseAndSetIfChanged(ref _isSingle, value);
    }

    public bool IsMidpoint
    {
        get => _isMidpoint;
        set => this.RaiseAndSetIfChanged(ref _isMidpoint, value);
    }

    public string Label
    {
        get => _label;
        set => this.RaiseAndSetIfChanged(ref _label, value);
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
    public double RecommendedMin
    {
        get => _recommendedMin;
        set => this.RaiseAndSetIfChanged(ref _recommendedMin, value);
    }
    public double RecommendedMax
    {
        get => _recommendedMax;
        set => this.RaiseAndSetIfChanged(ref _recommendedMax, value);
    }

    public string TargetId => _target.Id;

    public ICommand AddPointCommand { get; }
    public ICommand RemovePointCommand { get; }

    private double _pointValue;
    private double _sliderPointValue;
    private int _pointFrame;
    private bool _isSingle;
    private bool _isMidpoint;
    private string _label = string.Empty;
    private double _min = double.MinValue;
    private double _max = double.MaxValue;
    private double _recommendedMin = double.MinValue;
    private double _recommendedMax = double.MaxValue;
    private const double _valueEnterThreshold = 0.2;
    private const double _frameEnterThreshold = 0.2;
    private const double _doubleComparisonTolerance = 1e-6;

    private Timer? _valueEnterTimer;
    private Timer? _frameEnterTimer;
    private bool _isValueEnteringFlag = false;
    private bool _isFrameEnteringFlag = false;
    private double _beforeValue = 0;
    private int _beforeFrame = 0;
    private bool _suppressChangeEvents = false;
    private MetaNumberParamPropertyViewModel _parentViewModel;
    private CoordPoint _target;

    public MetaNumberCoordPointViewModel(
        MetaNumberParamPropertyViewModel parentViewModel,
        CoordPoint target,
        PointType pointType = PointType.Start,
        double min = double.MinValue,
        double max = double.MaxValue,
        double recommendedMin = double.MinValue,
        double recommendedMax = double.MaxValue)
    {
        _parentViewModel = parentViewModel;
        _target = target;
        RefreshFromTarget(target, pointType, min, max, recommendedMin, recommendedMax);
        this.WhenAnyValue(vm => vm.PointFrame).Subscribe(_ =>
        {
            TryFrameEnter();
        });
        AddPointCommand = ReactiveCommand.Create(AddPoint);
        RemovePointCommand = ReactiveCommand.Create(RemovePoint);
    }

    public void RefreshFromTarget(
        CoordPoint target,
        PointType pointType,
        double min,
        double max,
        double recommendedMin,
        double recommendedMax)
    {
        _suppressChangeEvents = true;

        _target = target;

        // Update type related flags/labels
        switch (pointType)
        {
            case PointType.Start:
                Label = "始点";
                IsMidpoint = false;
                IsSingle = false;
                break;
            case PointType.Mid:
                Label = "中点";
                IsMidpoint = true;
                IsSingle = false;
                break;
            case PointType.End:
                Label = "終点";
                IsMidpoint = false;
                IsSingle = false;
                break;
            case PointType.Single:
                Label = "移動なし";
                IsSingle = true;
                IsMidpoint = false;
                break;
        }

        Min = min;
        Max = max;
        RecommendedMin = recommendedMin;
        RecommendedMax = recommendedMax;

        // Refresh values
        PointValue = target.Value;
        SliderPointValue = Math.Min(RecommendedMax, Math.Max(RecommendedMin, PointValue));
        PointFrame = target.Frame;

        _suppressChangeEvents = false;
    }

    private void AddPoint()
    {
        _parentViewModel.AddPointRequest(_target);
    }

    private void RemovePoint()
    {
        _parentViewModel.RemovePointRequest(_target);
    }

    private void TryValueEnter(double previousValue)
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

        if (AreClose(PointValue, _beforeValue))
        {
            _isValueEnteringFlag = false;
            return;
        }

        EnsureValueEnterTimer();
        ValueSliderMoving();

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
            if (!AreClose(PointValue, _beforeValue))
            {
                _parentViewModel.UpdatePointValue(_target, _beforeValue, PointValue);
            }

            _isValueEnteringFlag = false;
        };
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) <= _doubleComparisonTolerance;
    }

    private void ValueSliderMoving()
    {
        _parentViewModel.PreviewUpdatePointValue(_target, _beforeValue, PointValue);
    }

    private void TryFrameEnter()
    {
        if (_suppressChangeEvents)
        {
            return;
        }
        if (_isFrameEnteringFlag)
        {
            if (_frameEnterTimer is null)
            {
                _frameEnterTimer = new Timer(_frameEnterThreshold * 1000)
                {
                    AutoReset = false
                };
                _frameEnterTimer.Elapsed += (sender, e) =>
                {
                    if (PointFrame != _beforeFrame)
                    {
                        _parentViewModel.UpdatePointFrame(_target, _beforeFrame, PointFrame);
                        _isFrameEnteringFlag = false;
                    }
                };
            }
            _frameEnterTimer.Stop();
            _frameEnterTimer.Start();
        }
        else
        {
            _beforeFrame = PointFrame;
        }

        _isFrameEnteringFlag = true;
    }
}

using System;
using System.Timers;
using Metasia.Core.Coordinate;
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
        set {
            if (_pointValue != value)
            {
                _pointValue = Math.Max(Min, Math.Min(Max, value));
                this.RaisePropertyChanged();
                // スライダーの値を更新（RecommendedMaxを超える場合はRecommendedMaxに制限）
                SliderPointValue = Math.Min(RecommendedMax, Math.Max(RecommendedMin, _pointValue));
            }
        }
    }

    public double SliderPointValue
    {
        get => _sliderPointValue;
        set {
            if (_sliderPointValue != value)
            {
                _sliderPointValue = Math.Max(RecommendedMin, Math.Min(RecommendedMax, value));
                this.RaisePropertyChanged();
                // スライダー操作時はPointValueを更新
                PointValue = _sliderPointValue;
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

    private Timer? _valueEnterTimer;
    private Timer? _frameEnterTimer;
    private bool _isValueEnteringFlag = false;
    private bool _isFrameEnteringFlag = false;
    private double _beforeValue = 0;
    private int _beforeFrame = 0;
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
                break;
        }
        _parentViewModel = parentViewModel;
        _target = target;
        Min = min;
        Max = max;
        RecommendedMin = recommendedMin;
        RecommendedMax = recommendedMax;
        _beforeValue = target.Value;
        PointValue = target.Value;
        SliderPointValue = Math.Min(RecommendedMax, Math.Max(RecommendedMin, PointValue));
        PointFrame = target.Frame;


        this.WhenAnyValue(vm => vm.PointValue).Subscribe(_ =>
        {
            TryValueEnter();
        });
        this.WhenAnyValue(vm => vm.PointFrame).Subscribe(_ =>
        {
            TryFrameEnter();
        });
    }

    private void TryValueEnter()
    {
        if(_isValueEnteringFlag)
        {
            if (_valueEnterTimer is null)
            {
                _valueEnterTimer = new Timer(_valueEnterThreshold * 1000)
                {
                    AutoReset = false
                };
                _valueEnterTimer.Elapsed += (sender, e) => {
                    if (PointValue != _beforeValue)
                    {
                        UpdateValue();
                        _isValueEnteringFlag = false;
                    }
                };
            }
            _valueEnterTimer.Stop();
            _valueEnterTimer.Start();
            ValueSliderMoving();
        }
        else
        {
            _beforeValue = PointValue;
        }

        _isValueEnteringFlag = true;
    }

    private void UpdateValue()
    {
        _parentViewModel.UpdatePointValue(_target, _beforeValue, PointValue);
    }

    private void ValueSliderMoving()
    {
        _parentViewModel.PreviewUpdatePointValue(_target, _beforeValue, PointValue);
    }

    private void TryFrameEnter()
    {
        if(_isFrameEnteringFlag)
        {
            if(_frameEnterTimer is null)
            {
                _frameEnterTimer = new Timer(_frameEnterThreshold * 1000)
                {
                    AutoReset = false
                };
                _frameEnterTimer.Elapsed += (sender, e) => {
                    if(PointFrame != _beforeFrame)
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

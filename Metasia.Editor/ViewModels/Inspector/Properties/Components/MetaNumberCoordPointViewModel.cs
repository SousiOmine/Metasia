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
        set => this.RaiseAndSetIfChanged(ref _pointValue, value);
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
    private int _pointFrame;
    private bool _isSingle;
    private bool _isMidpoint;
    private string _label = string.Empty;
    private double _min = double.MinValue;
    private double _max = double.MaxValue;
    private double _recommendedMin = double.MinValue;
    private double _recommendedMax = double.MaxValue;

    public MetaNumberCoordPointViewModel(CoordPoint target, PointType pointType = PointType.Start, double min = double.MinValue, double max = double.MaxValue, double recommendedMin = double.MinValue, double recommendedMax = double.MaxValue)
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
        
        PointValue = target.Value;
        PointFrame = target.Frame;
        Min = min;
        Max = max;
        RecommendedMin = recommendedMin;
        RecommendedMax = recommendedMax;
    }
}
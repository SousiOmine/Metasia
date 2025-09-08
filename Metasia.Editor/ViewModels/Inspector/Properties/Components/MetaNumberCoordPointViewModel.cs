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
    
    private bool _isSingle;
    private bool _isMidpoint;
    private string _label = string.Empty;

    public MetaNumberCoordPointViewModel(CoordPoint target, PointType pointType = PointType.Start)
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
    }
}
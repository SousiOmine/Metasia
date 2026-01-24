using System.Diagnostics;
using Jint;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Metasia.Core.Objects.Parameters;

public class MetaNumberParam<T> where T : struct, INumber<T>
{
    /// <summary>
    /// CoordPointパラメータのリスト
    /// </summary>
    [XmlIgnore]
    public IReadOnlyList<CoordPoint> Params => _params.AsReadOnly();

    /// <summary>
    /// シリアライズ専用のCoordPointパラメータのリスト
    /// </summary>
    [XmlArray("Params")]
    [XmlArrayItem("CoordPoint")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public List<CoordPoint> SerializableParams
    {
        get => _params;
        set => _params = value ?? [];
    }

    /// <summary>
    /// 移動を有効にするか
    /// </summary>
    public bool IsMovable { get; set; } = false;

    /// <summary>
    /// 開始地点における値 IsMovable=falseの場合は常にこの値を返す
    /// </summary>
    public CoordPoint StartPoint { get; set; } = new();

    /// <summary>
    /// 終了地点における値
    /// </summary>
    public CoordPoint EndPoint { get; set; } = new();

    private List<CoordPoint> _params = [];


    public MetaNumberParam()
    {
        _params = [];
    }

    public MetaNumberParam(T initialValue)
    {
        StartPoint.Value = double.CreateChecked(initialValue);
        EndPoint.Value = double.CreateChecked(initialValue);
    }

    public T Get(int frame, int clipLength)
    {
        return CalculateMidValue(frame, clipLength);
    }

    public void AddPoint(CoordPoint point)
    {
        _params.Add(point);
        Sort();
    }

    public bool RemovePoint(CoordPoint point)
    {
        _params.Remove(point);
        Sort();
        return true;
    }

    public bool UpdatePoint(CoordPoint point)
    {
        var points = new List<CoordPoint> { StartPoint, EndPoint };
        points.AddRange(_params);

        var targetPoint = points.FirstOrDefault(p => p.Id == point.Id);
        if (targetPoint is not null)
        {
            targetPoint.Value = point.Value;
            targetPoint.Frame = point.Frame;
            targetPoint.InterpolationLogic = point.InterpolationLogic;
            Sort();
            return true;
        }
        return false;
    }

    public void SetSinglePoint(T value)
    {
        IsMovable = false;
        StartPoint.Value = double.CreateChecked(value);
        EndPoint.Value = double.CreateChecked(value);
    }

    /// <summary>
    /// 指定されたフレームでパラメータを2つに分割する
    /// </summary>
    /// <param name="splitFrame">分割フレーム位置</param>
    /// <returns>前半部分と後半部分の2つのMetaNumberParam</returns>
    /// <remarks>
    /// 分割仕様:
    /// - 前半: splitFrame - 1 までのフレームを含む
    /// - 後半: splitFrame から開始（splitFrame を含む）
    /// - つまり splitFrame は後半クリップに属する
    /// </remarks>
    public (MetaNumberParam<T> FirstHalf, MetaNumberParam<T> SecondHalf) Split(int splitFrame, int oldClipLength)
    {
        var firstHalf = new MetaNumberParam<T>();
        var secondHalf = new MetaNumberParam<T>();

        if (!IsMovable)
        {
            firstHalf.SetSinglePoint(T.CreateChecked(StartPoint.Value));
            secondHalf.SetSinglePoint(T.CreateChecked(StartPoint.Value));
            return (firstHalf, secondHalf);
        }

        firstHalf.IsMovable = true;
        secondHalf.IsMovable = true;

        // 分割フレームの値を計算
        T splitValue = Get(splitFrame, oldClipLength);

        // 前半部分：分割フレームより前のポイントをコピー
        firstHalf.StartPoint.Value = double.CreateChecked(StartPoint.Value);
        firstHalf.StartPoint.InterpolationLogic = StartPoint.InterpolationLogic.HardCopy();
        firstHalf.EndPoint.Value = double.CreateChecked(splitValue);
        foreach (var point in _params.Where(p => p.Frame < splitFrame))
        {
            var newPoint = new CoordPoint
            {
                Frame = point.Frame,
                Value = point.Value,
                InterpolationLogic = point.InterpolationLogic.HardCopy()
            };
            firstHalf._params.Add(newPoint);
        }

        // 後半部分：分割フレーム以降のポイントをコピー（フレームを調整）
        secondHalf.StartPoint.Value = double.CreateChecked(splitValue);
        secondHalf.EndPoint.Value = double.CreateChecked(EndPoint.Value);
        secondHalf.EndPoint.InterpolationLogic = EndPoint.InterpolationLogic.HardCopy();
        foreach (var point in _params.Where(p => p.Frame >= splitFrame))
        {
            var newPoint = new CoordPoint
            {
                Frame = point.Frame - splitFrame,
                Value = point.Value,
                InterpolationLogic = point.InterpolationLogic.HardCopy()
            };
            secondHalf._params.Add(newPoint);
        }

        // 分割フレーム位置にあるポイントを探して、そのJSロジックを境界ポイントに設定
        var splitFramePoint = _params.FirstOrDefault(p => p.Frame == splitFrame);
        if (splitFramePoint is not null)
        {
            firstHalf.EndPoint.InterpolationLogic = splitFramePoint.InterpolationLogic.HardCopy();
            secondHalf.StartPoint.InterpolationLogic = splitFramePoint.InterpolationLogic.HardCopy();
        }
        else
        {
            // 分割フレーム位置にポイントがない場合、最も近い前方のポイントのJSロジックを使用
            var nearestPoint = _params.LastOrDefault(p => p.Frame < splitFrame);
            if (nearestPoint != null)
            {
                firstHalf.EndPoint.InterpolationLogic = nearestPoint.InterpolationLogic.HardCopy();
                secondHalf.StartPoint.InterpolationLogic = nearestPoint.InterpolationLogic.HardCopy();
            }
        }

        return (firstHalf, secondHalf);
    }


    protected T CalculateMidValue(int frame, int clipLength)
    {
        EndPoint.Frame = clipLength;

        if (IsMovable)
        {
            Sort();
            CoordPoint start = StartPoint;
            CoordPoint end = EndPoint;

            if (_params.Count != 0)
            {
                List<CoordPoint> points = [.. _params, StartPoint, EndPoint];
                points.Sort((a, b) => a.Frame - b.Frame);
                //frameを含む前後２つのポイントを取得
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].Frame >= frame)
                    {
                        end = points[i];
                        if (i >= 1) start = points[i - 1];
                        break;
                    }
                }
            }

            try
            {
                double midValue = start.InterpolationLogic.Calculate(start.Value, end.Value, frame, start.Frame, end.Frame);
                return T.CreateChecked(midValue);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return T.CreateChecked(start.Value);
            }
        }
        else
        {
            return T.CreateChecked(StartPoint.Value);
        }
    }

    private void Sort()
    {
        if (_params.Count == 0) return;
        _params.Sort((a, b) => a.Frame - b.Frame);
    }
}

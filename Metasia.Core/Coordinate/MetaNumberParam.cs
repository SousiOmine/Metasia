using System.Diagnostics;
using Jint;
using Metasia.Core.Objects;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Metasia.Core.Coordinate;

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
        set => _params = value ?? new List<CoordPoint>();
    }

    private List<CoordPoint> _params = new();


    public MetaNumberParam()
    {
        _params = new();
    }

    public MetaNumberParam(T initialValue)
    {
        _params = [new CoordPoint() { Value = double.CreateChecked(initialValue) }];
    }

    public T Get(int frame)
    {
        return CalculateMidValue(frame);
    }

    public void AddPoint(CoordPoint point)
    {
        _params.Add(point);
        Sort();
    }

    public bool RemovePoint(CoordPoint point)
    {
        if (_params.Count <= 1)
        {
            return false;
        }
        _params.Remove(point);
        Sort();
        return true;
    }

    public bool UpdatePoint(CoordPoint point)
    {
        var targetPoint = _params.FirstOrDefault(p => p.Id == point.Id);
        if (targetPoint is not null)
        {
            targetPoint.Value = point.Value;
            targetPoint.Frame = point.Frame;
            targetPoint.InterpolationLogic = point.InterpolationLogic;
            return true;
        }
        return false;
    }

    public void SetSinglePoint(T value)
    {
        _params = [new CoordPoint() { Value = double.CreateChecked(value) }];
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
    public (MetaNumberParam<T> FirstHalf, MetaNumberParam<T> SecondHalf) Split(int splitFrame)
    {
        var firstHalf = new MetaNumberParam<T>();
        var secondHalf = new MetaNumberParam<T>();

        // 分割フレームの値を計算
        T splitValue = Get(splitFrame);

        // 前半部分：分割フレームより前のポイントをコピー
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

        // 境界ポイントを追加（既に分割フレーム位置にポイントがある場合は追加しない）
        // 境界ポイントには、分割フレーム位置にあるポイントのJSロジックを使用する
        CoordPoint boundaryPointForFirstHalf = new CoordPoint
        {
            Frame = splitFrame,
            Value = double.CreateChecked(splitValue)
        };

        CoordPoint boundaryPointForSecondHalf = new CoordPoint
        {
            Frame = 0,
            Value = double.CreateChecked(splitValue)
        };

        // 分割フレーム位置にあるポイントを探して、そのJSロジックを境界ポイントに設定
        var splitFramePoint = _params.FirstOrDefault(p => p.Frame == splitFrame);
        if (splitFramePoint != null)
        {
            boundaryPointForFirstHalf.InterpolationLogic = splitFramePoint.InterpolationLogic.HardCopy();
            boundaryPointForSecondHalf.InterpolationLogic = splitFramePoint.InterpolationLogic.HardCopy();
        }
        else
        {
            // 分割フレーム位置にポイントがない場合、最も近い前方のポイントのJSロジックを使用
            var nearestPoint = _params.LastOrDefault(p => p.Frame < splitFrame);
            if (nearestPoint != null)
            {
                boundaryPointForFirstHalf.InterpolationLogic = nearestPoint.InterpolationLogic.HardCopy();
                boundaryPointForSecondHalf.InterpolationLogic = nearestPoint.InterpolationLogic;
            }
        }

        if (firstHalf._params.Count == 0 || firstHalf._params.Last().Frame < splitFrame - 1)
        {
            // 前半の最終フレームをsplitFrame-1に設定
            boundaryPointForFirstHalf.Frame = splitFrame - 1;
            // 前半の境界ポイントの値はsplitFrame-1での補間値
            boundaryPointForFirstHalf.Value = double.CreateChecked(Get(splitFrame - 1));
            firstHalf._params.Add(boundaryPointForFirstHalf);
        }

        if (secondHalf._params.Count == 0 || secondHalf._params[0].Frame > 0)
        {
            secondHalf._params.Insert(0, boundaryPointForSecondHalf);
        }

        return (firstHalf, secondHalf);
    }


    protected T CalculateMidValue(int frame)
    {
        Sort();
        if (_params.Count == 0)
        {
            throw new InvalidOperationException("Params is empty");
        }

        // 指定フレームがすべてのキーフレームより前にある場合、最初のキーフレームの値を返す
        if (frame < _params[0].Frame)
        {
            return T.CreateChecked(_params[0].Value);
        }

        CoordPoint startPoint = _params.Last();
        CoordPoint endPoint = startPoint;

        //frameを含む前後２つのポイントを取得
        for (int i = 0; i < _params.Count; i++)
        {
            if (_params[i].Frame >= frame)
            {
                endPoint = _params[i];
                if (i > 0) startPoint = _params[i - 1];
                else startPoint = endPoint;
                break;
            }
        }

        try
        {
            double midValue = startPoint.InterpolationLogic.Calculate(startPoint.Value, endPoint.Value, frame, startPoint.Frame, endPoint.Frame);
            return T.CreateChecked(midValue);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return T.CreateChecked(startPoint.Value);
        }
    }

    private void Sort()
    {
        _params.Sort((a, b) => a.Frame - b.Frame);
    }
}

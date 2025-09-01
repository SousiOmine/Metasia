using System.Diagnostics;
using Jint;
using Metasia.Core.Objects;
using System.Linq;

namespace Metasia.Core.Coordinate;

public class MetaNumberParam<T> where T : struct, IConvertible, IEquatable<T>
{
    public List<CoordPoint> Params { get; protected set; }

    private ClipObject ownerObject;

    private Engine jsEngine = new Engine(opts => opts
        .MaxStatements(10000)
        .LimitRecursion(10000)
        .TimeoutInterval(TimeSpan.FromMilliseconds(100))
    );

    public MetaNumberParam()
    {
        Params = new();
    }

    public MetaNumberParam(ClipObject owner, T initialValue)
    {
        ownerObject = owner;
        Params = [new CoordPoint(){Value = Convert.ToDouble(initialValue)}];
    }

    public T Get(int frame)
    {
        return CalculateMidValue(frame);
    }

    protected T CalculateMidValue(int frame)
    {
        //CoordPointのFrameはオブジェクトの始点基準なので合わせる
        frame -= ownerObject?.StartFrame ?? 0;
        //pointsをFrameの昇順に並べ替え
        Params.Sort((a, b) => a.Frame - b.Frame);
        if(Params.Count == 0)
        {
            throw new InvalidOperationException("Params is empty");
        }
        CoordPoint startPoint = Params.Last();
        CoordPoint endPoint = startPoint;

        //frameを含む前後２つのポイントを取得
        for(int i = 0; i < Params.Count; i++)
        {
            if (Params[i].Frame >= frame)
            {
                endPoint = Params[i];
                if(i > 0) startPoint = Params[i - 1];
                else startPoint = endPoint;
                break;
            }
        }

        jsEngine.SetValue("StartValue", startPoint.Value)
                .SetValue("EndValue", endPoint.Value)
                .SetValue("NowFrame", frame)
                .SetValue("StartFrame", startPoint.Frame)
                .SetValue("EndFrame", endPoint.Frame);

        try
        {
            double midValue = jsEngine.Evaluate(startPoint.JSLogic).AsNumber();
            return (T)Convert.ChangeType(midValue, typeof(T));
        }
        catch(Exception e)
        {
            Debug.WriteLine(e.Message);
            return (T)Convert.ChangeType(startPoint.Value, typeof(T));
        }
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
        firstHalf.ownerObject = ownerObject;
        secondHalf.ownerObject = ownerObject;
        
        // 分割フレームの値を計算
        T splitValue = Get(splitFrame);
        
        // 前半部分：分割フレームより前のポイントをコピー
        foreach (var point in Params.Where(p => p.Frame < splitFrame))
        {
            var newPoint = new CoordPoint
            {
                Frame = point.Frame,
                Value = point.Value,
                JSLogic = point.JSLogic
            };
            firstHalf.Params.Add(newPoint);
        }
        
        // 後半部分：分割フレーム以降のポイントをコピー（フレームを調整）
        foreach (var point in Params.Where(p => p.Frame >= splitFrame))
        {
            var newPoint = new CoordPoint
            {
                Frame = point.Frame - splitFrame,
                Value = point.Value,
                JSLogic = point.JSLogic
            };
            secondHalf.Params.Add(newPoint);
        }
        
        // 境界ポイントを追加（既に分割フレーム位置にポイントがある場合は追加しない）
        // 境界ポイントには、分割フレーム位置にあるポイントのJSロジックを使用する
        CoordPoint boundaryPointForFirstHalf = new CoordPoint
        {
            Frame = splitFrame,
            Value = Convert.ToDouble(splitValue)
        };
        
        CoordPoint boundaryPointForSecondHalf = new CoordPoint
        {
            Frame = 0,
            Value = Convert.ToDouble(splitValue)
        };
        
        // 分割フレーム位置にあるポイントを探して、そのJSロジックを境界ポイントに設定
        var splitFramePoint = Params.FirstOrDefault(p => p.Frame == splitFrame);
        if (splitFramePoint != null)
        {
            boundaryPointForFirstHalf.JSLogic = splitFramePoint.JSLogic;
            boundaryPointForSecondHalf.JSLogic = splitFramePoint.JSLogic;
        }
        else
        {
            // 分割フレーム位置にポイントがない場合、最も近い前方のポイントのJSロジックを使用
            var nearestPoint = Params.LastOrDefault(p => p.Frame < splitFrame);
            if (nearestPoint != null)
            {
                boundaryPointForFirstHalf.JSLogic = nearestPoint.JSLogic;
                boundaryPointForSecondHalf.JSLogic = nearestPoint.JSLogic;
            }
        }
        
        if (firstHalf.Params.Count == 0 || firstHalf.Params.Last().Frame < splitFrame - 1)
        {
            // 前半の最終フレームをsplitFrame-1に設定
            boundaryPointForFirstHalf.Frame = splitFrame - 1;
            // 前半の境界ポイントの値はsplitFrame-1での補間値
            boundaryPointForFirstHalf.Value = Convert.ToDouble(Get(splitFrame - 1));
            firstHalf.Params.Add(boundaryPointForFirstHalf);
        }
        
        if (secondHalf.Params.Count == 0 || secondHalf.Params[0].Frame > 0)
        {
            secondHalf.Params.Insert(0, boundaryPointForSecondHalf);
        }
        
        return (firstHalf, secondHalf);
    }
}


using System.Diagnostics;
using Jint;
using Metasia.Core.Objects;

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
}


using Jint;
using Metasia.Core.Objects;
using System.Diagnostics;

namespace Metasia.Core.Coordinate;

/// <summary>
/// double型の中間値を持つパラメータ
/// </summary>
public class MetaDoubleParam
{
    private MetasiaObject ownerObject;

    /// <summary>
    /// 中間値CoordPointを格納するリスト
    /// </summary>
    public List<CoordPoint> Params { get; protected set; }

    public MetaDoubleParam()
    {

    }

    public MetaDoubleParam(MetasiaObject owner, double initialValue)
    {
        ownerObject = owner;
        Params = new();
        Params.Add(new CoordPoint(){Value = initialValue});
    }

    /// <summary>
    /// 中間点の間の値を計算するためのJavaScriptエンジン
    /// </summary>
    private Engine jsEngine = new Engine();

    /// <summary>
    /// フレームから値を取得する
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public double Get(int frame)
    {
        return CalculateMidValue(frame);
    }
    
    protected double CalculateMidValue(int frame)
    {
        //CoordPointのFrameはオブジェクトの始点基準なので合わせる
        frame -= ownerObject.StartFrame;
        //pointsをFrameの昇順に並べ替え
        Params.Sort((a, b) => a.Frame - b.Frame);
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
            return midValue;
        }
        catch(Exception e)
        {
            return startPoint.Value;
        }
    }
}
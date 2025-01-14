using Jint;
using Metasia.Core.Objects;

namespace Metasia.Core.Coordinate;

public class MetaFloatParam
{
    private MetasiaObject ownerObject;
    public List<CoordPoint> Params { get; protected set; }

    /// <summary>
    /// 中間点の間の値を計算するためのJavaScriptエンジン
    /// </summary>
    private Engine jsEngine = new Engine();

    public MetaFloatParam(MetasiaObject owner, float initialValue)
    {
        ownerObject = owner;
        Params = new();
        Params.Add(new CoordPoint(){Value = initialValue});
    }
    
    public float Get(int frame)
    {
        return CalculateMidValue(frame);
    }
    
    protected float CalculateMidValue(int frame)
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
            float midValue = (float)jsEngine.Evaluate(startPoint.JSLogic).AsNumber();
            return midValue;
        }
        catch (Exception e)
        {
            return (float)startPoint.Value;
        }
    }
}
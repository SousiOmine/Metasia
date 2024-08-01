using Metasia.Core.Objects;

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

    public MetaDoubleParam(MetasiaObject owner, double initialValue)
    {
        ownerObject = owner;
        Params = new();
        Params.Add(new CoordPoint(){Value = initialValue});
    }
    
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
        double midValue = startPoint.PointLogic.GetBetweenPoint(startPoint.Value, endPoint.Value, frame, startPoint.Frame, endPoint.Frame);
			
        return midValue;
    }
}
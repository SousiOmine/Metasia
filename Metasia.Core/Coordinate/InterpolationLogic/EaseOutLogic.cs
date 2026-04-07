namespace Metasia.Core.Coordinate.InterpolationLogic;

public class EaseOutLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "EaseOutLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        double t = (double)(nowFrame - startFrame) / (endFrame - startFrame);
        double c = endValue - startValue;
        return -c * t * (t - 2) + startValue;
    }

    /// <summary>
    /// 自身をハードコピーします。
    /// </summary>
    /// <returns>現在のインスタンスのハードコピー</returns>
    public override InterpolationLogicBase HardCopy()
    {
        return new EaseOutLogic();
    }
}
namespace Metasia.Core.Coordinate.InterpolationLogic;

public class EaseOutLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "EaseOutLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        return startValue + (endValue - startValue) * Math.Pow((endFrame - nowFrame) / (endFrame - startFrame), 2);
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
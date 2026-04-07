namespace Metasia.Core.Coordinate.InterpolationLogic;

public class EaseInOutLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "EaseInOutLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        double t = (double)(nowFrame - startFrame) / (endFrame - startFrame);
        double c = endValue - startValue;

        if (t < 0.5)
        {
            return c * 8 * Math.Pow(t, 4) + startValue;
        }
        t -= 1;
        return -c * (8 * Math.Pow(t, 4) - 1) + startValue;
    }

    /// <summary>
    /// 自身をハードコピーします。
    /// </summary>
    /// <returns>現在のインスタンスのハードコピー</returns>
    public override InterpolationLogicBase HardCopy()
    {
        return new EaseInOutLogic();
    }
}
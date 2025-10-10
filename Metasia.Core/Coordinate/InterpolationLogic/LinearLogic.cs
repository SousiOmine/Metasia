namespace Metasia.Core.Coordinate.InterpolationLogic;

public class LinearLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "LinearLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        return startValue + (endValue - startValue) * (nowFrame - startFrame) / (endFrame - startFrame);
    }

    /// <summary>
    /// 自身をハードコピーします。
    /// </summary>
    /// <returns>現在のインスタンスのハードコピー</returns>
    public override InterpolationLogicBase HardCopy()
    {
        return new LinearLogic();
    }
}
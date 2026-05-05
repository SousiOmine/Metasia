namespace Metasia.Core.Coordinate.InterpolationLogic;

public class TeleportLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "TeleportLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        if (nowFrame >= endFrame) return endValue;
        return startValue;
    }

    public override InterpolationLogicBase HardCopy()
    {
        return new TeleportLogic();
    }
}

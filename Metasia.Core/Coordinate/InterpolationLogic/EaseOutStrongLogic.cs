namespace Metasia.Core.Coordinate.InterpolationLogic;

public class EaseOutStrongLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "EaseOutStrongLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        double t = (double)(nowFrame - startFrame) / (endFrame - startFrame);
        double c = endValue - startValue;
        return c * (1 - Math.Pow(1 - t, 4)) + startValue;
    }

    public override InterpolationLogicBase HardCopy()
    {
        return new EaseOutStrongLogic();
    }
}

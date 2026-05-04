namespace Metasia.Core.Coordinate.InterpolationLogic;

public class EaseInStrongLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "EaseInStrongLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        double t = (double)(nowFrame - startFrame) / (endFrame - startFrame);
        double c = endValue - startValue;
        return c * Math.Pow(t, 4) + startValue;
    }

    public override InterpolationLogicBase HardCopy()
    {
        return new EaseInStrongLogic();
    }
}

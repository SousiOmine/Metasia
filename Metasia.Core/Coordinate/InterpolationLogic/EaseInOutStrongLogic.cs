namespace Metasia.Core.Coordinate.InterpolationLogic;

public class EaseInOutStrongLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "EaseInOutStrongLogic";

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        if (startValue == endValue) return startValue;
        double t = (double)(nowFrame - startFrame) / (endFrame - startFrame);
        double c = endValue - startValue;

        if (t < 0.5)
        {
            return c * 32 * Math.Pow(t, 6) + startValue;
        }
        t -= 1;
        return -c * (32 * Math.Pow(t, 6) - 1) + startValue;
    }

    public override InterpolationLogicBase HardCopy()
    {
        return new EaseInOutStrongLogic();
    }
}

namespace Metasia.Core.Coordinate.InterpolationLogic;

public abstract class InterpolationLogicBase : IInterpolationLogic
{
    public abstract string Identify { get; }
    public abstract double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame);
    public abstract InterpolationLogicBase HardCopy();
}
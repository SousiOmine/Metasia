namespace Metasia.Core.Coordinate.InterpolationLogic;

public interface IInterpolationLogic
{
    string Identify { get; }
    double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame);
}
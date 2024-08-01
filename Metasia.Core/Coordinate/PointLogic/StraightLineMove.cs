

namespace Metasia.Core.Coordinate.PointLogic
{
    /// <summary>
    /// 直線移動をするPointLogic
    /// </summary>
    public class StraightLineMove : IPointLogic
    {
        public double GetBetweenPoint(double StartValue, double EndValue, double NowFrame, double StartFrame, double EndFrame)
        {
            if(StartValue == EndValue) return StartValue;
            return StartValue + (EndValue - StartValue) * (NowFrame - StartFrame) / (EndFrame - StartFrame);
        }
    }
}
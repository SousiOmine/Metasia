

namespace Metasia.Core.Coordinate.PointLogic
{
    /// <summary>
    /// 直線移動をするPointLogic
    /// </summary>
    public class StraightLineMove : IPointLogic
    {
        public float GetBetweenPoint(float StartValue, float EndValue, float NowFrame, float StartFrame, float EndFrame)
        {
            if(StartValue == EndValue) return StartValue;
            return StartValue + (EndValue - StartValue) * (NowFrame - StartFrame) / (EndFrame - StartFrame);
        }
    }
}
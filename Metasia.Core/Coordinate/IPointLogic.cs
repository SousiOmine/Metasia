
namespace Metasia.Core.Coordinate
{
    public interface IPointLogic
    {
        /// <summary>
        /// 開始点と終了点から、現在のフレームの値を返す
        /// </summary>
        /// <param name="StartValue">開始点</param>
        /// <param name="EndValue">終了点</param>
        /// <param name="NowFrame">現在の相対フレーム</param>
        /// <param name="StartFrame">開始点のある相対フレーム</param>
        /// <param name="EndFrame">終了点のある相対フレーム</param>
        /// <returns></returns>
        public float GetBetweenPoint(float StartValue, float EndValue, float NowFrame, float StartFrame, float EndFrame);
    }
}
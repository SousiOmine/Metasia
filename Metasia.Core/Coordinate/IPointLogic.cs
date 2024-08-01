
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
        /// <returns>現在フレームの計算結果</returns>
        public double GetBetweenPoint(double StartValue, double EndValue, double NowFrame, double StartFrame, double EndFrame);
    }
}
using Metasia.Core.Coordinate.InterpolationLogic;

namespace Metasia.Core.Coordinate
{
    /// <summary>
    /// AviUtlでいう中間点みたいなやつ。始点になる。
    /// </summary>
    public class CoordPoint
    {
        /// <summary>
        /// 一意なID 自動生成
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// ポイントが存在するフレームの位置 親オブジェクトの始点を基準にする
        /// </summary>
        public int Frame;

        /// <summary>
        /// 保持する値
        /// </summary>
        public double Value = 0f;

        public InterpolationLogicBase InterpolationLogic = new LinearLogic();

        /// <summary>
        /// PointLogicを指定しないコンストラクタ。PointLogicには直線移動が設定される。
        /// </summary>
        public CoordPoint()
        {

        }

    }
}
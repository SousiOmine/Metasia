

namespace Metasia.Core.Coordinate
{
    /// <summary>
    /// AviUtlでいう中間点みたいなやつ。終点になる。
    /// </summary>
    public class CoordPoint
    {
        /// <summary>
        /// ポイントが存在するフレームの位置 親オブジェクトの始点を基準にする
        /// </summary>
        public int Frame;

        /// <summary>
        /// 保持する値
        /// </summary>
        public float Value = 0f;

        /// <summary>
        /// ポイント間の移動方法
        /// </summary>
        private IPointLogic pointLogic;
        public IPointLogic PointLogic{
            get => pointLogic;
            set => pointLogic = value;
        }

        /// <summary>
        /// PointLogicを指定しないコンストラクタ。PointLogicには直線移動が設定される。
        /// </summary>
        public CoordPoint()
        {
            PointLogic = new PointLogic.StraightLineMove();
        }

        public CoordPoint(IPointLogic argPointLogic)
        {
            PointLogic = argPointLogic;
        }

    }
}
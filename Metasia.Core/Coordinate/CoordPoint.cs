

namespace Metasia.Core.Coordinate
{
    /// <summary>
    /// AviUtlでいう中間点みたいなやつ。
    /// </summary>
    public class CoordPoint
    {
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
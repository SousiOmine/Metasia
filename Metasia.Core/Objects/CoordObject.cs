using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    /// <summary>
    /// Coord(座標とか拡大率とか)を書き換え可能なオブジェクト
    /// </summary>
    public class CoordObject : MetasiaObject
	{
		public new Coordinate Coord { get; set; } = new();

		public CoordObject(string id) : base(id)
		{
		}
	}
}

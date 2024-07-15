using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Coordinate;
using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
    /// <summary>
    /// 座標とか拡大率とかを書き換え可能なオブジェクト
    /// </summary>
    public class CoordObject : MetasiaObject, IMetaCoordable
	{
		public MetaDoubleParam X { get; set; }
		public MetaDoubleParam Y { get; set; }
		public MetaDoubleParam Scale { get; set; }
		public MetaDoubleParam Alpha { get; set; }
		public MetaDoubleParam Rotation { get; set; }

		public CoordObject(string id) : base(id)
		{
			X = new MetaDoubleParam(this, 0);
			Y = new MetaDoubleParam(this, 0);
			Scale = new MetaDoubleParam(this, 100);
			Alpha = new MetaDoubleParam(this, 100);
			Rotation = new MetaDoubleParam(this, 0);
		}
		public virtual void DrawExpresser(ref DrawExpresserArgs e, int frame)
		{
			if (frame < StartFrame || frame > EndFrame) return;
			if (Child is not null && Child is IMetaDrawable)
			{
				IMetaDrawable drawChild = (IMetaDrawable)Child;
				Child.StartFrame = this.StartFrame;
				Child.EndFrame = this.EndFrame;
				drawChild.DrawExpresser(ref e, frame);
			}
		}
	}
}

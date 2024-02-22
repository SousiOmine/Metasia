using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core
{
	internal class ConvertSKPoint
	{
		public static SKPoint ToSKPoint(SKBitmap bmp, float x, float y)
		{
			SKPoint point = new SKPoint();
			point.X = (bmp.Width / 2) + x;
			point.Y = (bmp.Height / 2) - y;
			return point;
		}
	}
}

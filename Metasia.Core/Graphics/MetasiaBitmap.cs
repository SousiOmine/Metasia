using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Graphics
{
	/// <summary>
	/// Metasia内で使う画像型 SKBitmapを継承している
	/// </summary>
	public class MetasiaBitmap : SKBitmap
	{
		public MetasiaBitmap()
		{
		}

		public MetasiaBitmap(SKImageInfo info) : base(info)
		{
		}

		public MetasiaBitmap(SKImageInfo info, int rowBytes) : base(info, rowBytes)
		{
		}

		public MetasiaBitmap(SKImageInfo info, SKBitmapAllocFlags flags) : base(info, flags)
		{
		}


		public MetasiaBitmap(int width, int height, bool isOpaque = false) : base(width, height, isOpaque)
		{
		}

		public MetasiaBitmap(int width, int height, SKColorType colorType, SKAlphaType alphaType) : base(width, height, colorType, alphaType)
		{
		}

		public MetasiaBitmap(int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace) : base(width, height, colorType, alphaType, colorspace)
		{
		}

		/// <summary>
		/// 画像を任意の角度で回転させて返す
		/// </summary>
		/// <param name="bitmap">元画像</param>
		/// <param name="angle">回転角（度数法）</param>
		/// <returns>回転後の画像</returns>
		public static MetasiaBitmap Rotate(SKBitmap bitmap, double angle)
		{
			//リンク先のDatch氏の解答をそのまま利用 https://stackoverflow.com/questions/45077047/rotate-photo-with-skiasharp
			double radians = Math.PI * angle / 180;
			float sine = (float)Math.Abs(Math.Sin(radians));
			float cosine = (float)Math.Abs(Math.Cos(radians));
			int originalWidth = bitmap.Width;
			int originalHeight = bitmap.Height;
			int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
			int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

			var rotatedBitmap = new MetasiaBitmap(rotatedWidth, rotatedHeight);

			using (var surface = new SKCanvas(rotatedBitmap))
			{
				surface.Clear();
				surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
				surface.RotateDegrees((float)angle);
				surface.Translate(-originalWidth / 2, -originalHeight / 2);
				surface.DrawBitmap(bitmap, new SKPoint());
			}
			return rotatedBitmap;
		}

		/// <summary>
		/// 画像を透過させる
		/// </summary>
		/// <param name="bitmap">元画像</param>
		/// <param name="alpha">全ピクセルにこの値をかける。1.0で変更せず、0.0で完全に透明になる</param>
		/// <returns>透過後の画像</returns>
		public static SKBitmap Transparency(SKBitmap bitmap, double alpha)
		{
			SKBitmap blendedBitmap = new(bitmap.Width, bitmap.Height);
			using (var surface = new SKCanvas(blendedBitmap))
			{
				surface.Clear();	
				SKPaint paint = new();
				paint.Color = new SKColor(0, 0, 0, (byte)(255 * alpha));
				surface.DrawBitmap(bitmap, 0, 0, paint);
			}
			return blendedBitmap;
		}
	}
}

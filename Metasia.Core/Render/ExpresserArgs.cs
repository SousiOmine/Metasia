using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Render
{
    /// <summary>
    /// オブジェクトの表現時に渡す引数クラス
    /// </summary>
    public class ExpresserArgs : IDisposable
    {
        public SKBitmap? bitmap;

        /// <summary>
        /// レイアウトに使用するサイズ
        /// </summary>
        public SKSize targetSize;

		/// <summary>
		/// targetSizeの値を1としたとき、実際にレンダリングする解像度の比率
		/// たとえば、targetSizeが1920*1080の場合、ResolutionLevelが0.5であればレンダリング解像度は960*540になる
		/// </summary>
		public float ResolutionLevel;

        public ExpresserArgs()
        {
            //bitmap = new SKBitmap(1920, 1080);
        }

		public void Dispose()
		{
            bitmap.Dispose();
		}
	}
}

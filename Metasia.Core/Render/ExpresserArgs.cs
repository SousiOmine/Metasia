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
    public class ExpresserArgs
    {
        public SKBitmap bitmap;

        public ExpresserArgs()
        {
            bitmap = new SKBitmap(8192, 8192);
        }
    }
}

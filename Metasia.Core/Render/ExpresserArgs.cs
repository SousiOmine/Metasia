﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Sounds;

namespace Metasia.Core.Render
{
    /// <summary>
    /// オブジェクトの表現時に渡す引数クラス
    /// </summary>
    public class ExpresserArgs : IDisposable
    {
        /// <summary>
        /// 描写結果が格納されるSKBitmap
        /// </summary>
        public SKBitmap? bitmap;

        public MetasiaSound? sound;

        /// <summary>
        /// レイアウトに使用するサイズ
        /// </summary>
        public SKSize targetSize;

		/// <summary>
		/// targetSizeの値を1としたとき、実際にレンダリングする解像度の比率
		/// たとえば、targetSizeが1920*1080の場合、ResolutionLevelが0.5であればレンダリング解像度は960*540になる
		/// </summary>
		public float ResolutionLevel;
		
		public byte AudioChannel;
		
		public uint SoundSampleRate;

		public ushort FPS;

        public ExpresserArgs()
        {
            //bitmap = new SKBitmap(1920, 1080);
        }

		public void Dispose()
		{
            if (bitmap is not null) bitmap.Dispose();
            if (sound is not null) sound.Dispose();
		}
	}
}

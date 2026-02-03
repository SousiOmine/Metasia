using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Graphics
{
    /// <summary>
    /// Metasia内で使う画像処理ユーティリティクラス
    /// </summary>
    public static class MetasiaBitmap
    {
        /// <summary>
        /// 画像を任意の角度で回転させて返す
        /// </summary>
        /// <param name="bitmap">元画像</param>
        /// <param name="angle">回転角（度数法）</param>
        /// <returns>回転後の画像</returns>
        public static SKImage Rotate(SKBitmap bitmap, double angle)
        {
            //リンク先のDatch氏の解答をそのまま利用 https://stackoverflow.com/questions/45077047/rotate-photo-with-skiasharp
            double radians = Math.PI * angle / 180;
            float sine = (float)Math.Abs(Math.Sin(radians));
            float cosine = (float)Math.Abs(Math.Cos(radians));
            int originalWidth = bitmap.Width;
            int originalHeight = bitmap.Height;
            int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            var info = new SKImageInfo(rotatedWidth, rotatedHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info) ?? throw new InvalidOperationException("SKSurface.Create returned null in Rotate.");
            using var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);
            canvas.Translate(rotatedWidth / 2, rotatedHeight / 2);
            canvas.RotateDegrees((float)angle);
            canvas.Translate(-originalWidth / 2, -originalHeight / 2);
            canvas.DrawBitmap(bitmap, new SKPoint());

            return surface.Snapshot();
        }

        /// <summary>
        /// 画像を透過させる
        /// </summary>
        /// <param name="bitmap">元画像</param>
        /// <param name="alpha">全ピクセルにこの値をかける。1.0で変更せず、0.0で完全に透明になる</param>
        /// <returns>透過後の画像</returns>
        public static SKImage Transparency(SKBitmap bitmap, double alpha)
        {
            alpha = Math.Max(0.0, Math.Min(1.0, alpha));
            var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info) ?? throw new InvalidOperationException("SKSurface.Create returned null in Transparency.");
            using var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, (byte)(255 * alpha))
            };
            canvas.DrawBitmap(bitmap, 0, 0, paint);

            return surface.Snapshot();
        }
    }
}

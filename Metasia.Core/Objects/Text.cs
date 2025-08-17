using Metasia.Core.Coordinate;
using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    public class Text : MetasiaObject, IRenderable
    {
        public MetaDoubleParam X { get; set; }
        public MetaDoubleParam Y { get; set; }
        public MetaDoubleParam Scale { get; set; }
        public MetaDoubleParam Alpha { get; set; }
        public MetaDoubleParam Rotation { get; set; }

        public string TypefaceName
        {
            get { return typefaceName; }
            set 
            { 
                typefaceName = value;
                LoadTypeface();
            }
        }

        public string Contents { get; set; }

        public MetaFloatParam TextSize { get; set; }

        private string typefaceName;
        private SKTypeface? _typeface;

        public Text(string id) : base(id)
        {
            X = new MetaDoubleParam(this, 0);
            Y = new MetaDoubleParam(this, 0);
            Scale = new MetaDoubleParam(this, 100);
            Alpha = new MetaDoubleParam(this, 0);
            Rotation = new MetaDoubleParam(this, 0);
            TextSize = new MetaFloatParam(this, 100);
            LoadTypeface();
        }

        public RenderNode Render(RenderContext context)
        {
            SKPaint skPaint = new SKPaint()
            {
                IsAntialias = true,
                TextSize = TextSize.Get(context.Frame),
                Typeface = _typeface,
                Color = SKColors.White,
            };
            SKRect textBounds = new();
            skPaint.MeasureText(Contents, ref textBounds);
            
            int bitmapWidth = (int)textBounds.Width;
            int bitmapHeight = (int)textBounds.Height;

            if (bitmapWidth <= 0 || bitmapHeight <= 0 || string.IsNullOrEmpty(Contents))
            {
                return new RenderNode();
            }

            var logicalSize = new SKSize(bitmapWidth, bitmapHeight);
            var bitmap = new SKBitmap(bitmapWidth, bitmapHeight);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear();
                canvas.DrawText(Contents, -textBounds.Left, -textBounds.Top, skPaint);
            }

            //縦横のレンダリング倍率
            float renderScaleWidth = context.RenderResolution.Width / context.ProjectResolution.Width;
            float renderScaleHeight = context.RenderResolution.Height / context.ProjectResolution.Height;

            //レンダリング倍率に合わせて画像をリサイズ
            if (renderScaleWidth != 1.0f || renderScaleHeight != 1.0f)
            {
                var scaledInfo = new SKImageInfo((int)(bitmap.Width * renderScaleWidth), (int)(bitmap.Height * renderScaleHeight));
                bitmap = bitmap.Resize(scaledInfo, SKFilterQuality.High);
            }

            var transform = new Transform()
            {
                Position = new SKPoint((float)X.Get(context.Frame), (float)Y.Get(context.Frame)),
                Scale = (float)Scale.Get(context.Frame) / 100,
                Rotation = (float)Rotation.Get(context.Frame),
                Alpha = (100.0f - (float)Alpha.Get(context.Frame)) / 100,
            };


            if (Child is not IRenderable renderableChild)
            {
                return new RenderNode()
                {
                    Bitmap = bitmap,
                    LogicalSize = logicalSize,
                    Transform = transform,
                };
            }


            //もし子オブジェクトも描画対応していた場合
            var childNode = renderableChild.Render(context);
            return new RenderNode()
            {
                Bitmap = bitmap,
                LogicalSize = logicalSize,
                Transform = transform,
                Children = new List<RenderNode>() { childNode },
            };
        }

        private bool LoadTypeface()
        {
            _typeface = SKTypeface.FromFamilyName(TypefaceName);
            if(_typeface.FamilyName != TypefaceName)
            {
                using (var ms = new MemoryStream(Properties.Resources.LINESeedJP_TTF_Rg))
                {
                    _typeface = SKTypeface.FromStream(ms);
                }
                return false;

            }
            return true;
        }
    }
}

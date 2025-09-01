using Metasia.Core.Coordinate;
using Metasia.Core.Render;
using Metasia.Core.Xml;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    [Serializable]
    public class Text : ClipObject, IRenderable
    {
        public MetaNumberParam<double> X { get; set; }
        public MetaNumberParam<double> Y { get; set; }
        public MetaNumberParam<double> Scale { get; set; }
        public MetaNumberParam<double> Alpha { get; set; }
        public MetaNumberParam<double> Rotation { get; set; }

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

        public MetaNumberParam<double> TextSize { get; set; }

        private string typefaceName;
        private SKTypeface? _typeface;

        public Text(string id) : base(id)
        {
            X = new MetaNumberParam<double>(this, 0);
            Y = new MetaNumberParam<double>(this, 0);
            Scale = new MetaNumberParam<double>(this, 100);
            Alpha = new MetaNumberParam<double>(this, 0);
            Rotation = new MetaNumberParam<double>(this, 0);
            TextSize = new MetaNumberParam<double>(this, 100);
            LoadTypeface();
        }

        public Text()
        {
            X = new MetaNumberParam<double>(this, 0);
            Y = new MetaNumberParam<double>(this, 0);
            Scale = new MetaNumberParam<double>(this, 100);
            Alpha = new MetaNumberParam<double>(this, 0);
            Rotation = new MetaNumberParam<double>(this, 0);
            TextSize = new MetaNumberParam<double>(this, 100);
            LoadTypeface();
        }

        [Obsolete]
        public RenderNode Render(RenderContext context)
        {
            SKPaint skPaint = new SKPaint()
            {
                IsAntialias = true,
                TextSize = (float)TextSize.Get(context.Frame),
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


            return new RenderNode()
            {
                Bitmap = bitmap,
                LogicalSize = logicalSize,
                Transform = transform,
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

        /// <summary>
        /// 指定したフレームでテキストクリップを分割する
        /// </summary>
        /// <param name="splitFrame">分割フレーム</param>
        /// <returns>分割後の2つのテキストクリップ（前半と後半）</returns>
        public override (ClipObject firstClip, ClipObject secondClip) SplitAtFrame(int splitFrame)
        {
            var result = base.SplitAtFrame(splitFrame);
            
            var firstText = (Text)result.firstClip;
            var secondText = (Text)result.secondClip;
            
            firstText.Id = Id + "_part1";
            secondText.Id = Id + "_part2";
            
            return (firstText, secondText);
        }

        /// <summary>
        /// テキストクリップのコピーを作成する
        /// </summary>
        /// <returns>コピーされたテキストクリップ</returns>
        protected override ClipObject CreateCopy()
        {
            var xml = MetasiaObjectXmlSerializer.Serialize(this);
            var copy = MetasiaObjectXmlSerializer.Deserialize<Text>(xml);
            copy.Id = Id + "_copy";
            return copy;
        }
    }
}

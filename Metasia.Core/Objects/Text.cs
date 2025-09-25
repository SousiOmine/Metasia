using Metasia.Core.Coordinate;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Core.Xml;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Attributes;
namespace Metasia.Core.Objects
{
    [Serializable]
    [ClipTypeIdentifier("Text")]
    public class Text : ClipObject, IRenderable
    {
        [EditableProperty("X")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> X { get; set; }

        [EditableProperty("Y")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> Y { get; set; }

        [EditableProperty("Scale")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Scale { get; set; }

        [EditableProperty("Alpha")]
        [ValueRange(0, 100, 0, 100)]
        public MetaNumberParam<double> Alpha { get; set; }

        [EditableProperty("Rotation")]
        [ValueRange(-99999, 99999, 0, 360)]
        public MetaNumberParam<double> Rotation { get; set; }

        [EditableProperty("TypefaceName")]
        public string TypefaceName
        {
            get { return typefaceName; }
            set 
            { 
                typefaceName = value;
                LoadTypeface();
            }
        }

        [EditableProperty("TextContents")]
        public string Contents { get; set; }

        [EditableProperty("TextSize")]
        [ValueRange(0, 2000, 0, 500)]
        public MetaNumberParam<double> TextSize { get; set; }

        /// <summary>
        /// 描画エフェクトのリスト
        /// </summary>
        public List<VisualEffectBase> VisualEffects { get; } = new List<VisualEffectBase>();

        private string typefaceName;
        private SKTypeface? _typeface;

        public Text(string id) : base(id)
        {
            X = new MetaNumberParam<double>(0);
            Y = new MetaNumberParam<double>(0);
            Scale = new MetaNumberParam<double>(100);
            Alpha = new MetaNumberParam<double>(0);
            Rotation = new MetaNumberParam<double>(0);
            TextSize = new MetaNumberParam<double>(100);
            LoadTypeface();
        }

        public Text()
        {
            X = new MetaNumberParam<double>(0);
            Y = new MetaNumberParam<double>(0);
            Scale = new MetaNumberParam<double>(100);
            Alpha = new MetaNumberParam<double>(0);
            Rotation = new MetaNumberParam<double>(0);
            TextSize = new MetaNumberParam<double>(100);
            LoadTypeface();
        }

        [Obsolete]
        public RenderNode Render(RenderContext context)
        {
            //このオブジェクトのStartFrameを基準としたフレーム
            int relativeFrame = context.Frame - StartFrame;
            SKPaint skPaint = new SKPaint()
            {
                IsAntialias = true,
                TextSize = (float)TextSize.Get(relativeFrame),
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
                Position = new SKPoint((float)X.Get(relativeFrame), (float)Y.Get(relativeFrame)),
                Scale = (float)Scale.Get(relativeFrame) / 100,
                Rotation = (float)Rotation.Get(relativeFrame),
                Alpha = (100.0f - (float)Alpha.Get(relativeFrame)) / 100,
            };


            var renderNode = new RenderNode()
            {
                Bitmap = bitmap,
                LogicalSize = logicalSize,
                Transform = transform,
            };

            // 描画エフェクトを適用
            var effectContext = new VisualEffectContext
            {
                Time = relativeFrame / (double)context.ProjectResolution.Height,
                Project = null, // プロジェクト情報は現在のコンテキストから取得できない場合はnull
                OriginalPosition = new System.Numerics.Vector2((float)X.Get(relativeFrame), (float)Y.Get(relativeFrame)),
                OriginalSize = new System.Numerics.Vector2(logicalSize.Width, logicalSize.Height)
            };

            foreach (var effect in VisualEffects.Where(e => e.IsActive))
            {
                renderNode = effect.Apply(renderNode, effectContext);
            }

            return renderNode;
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

            // MetaNumberParamプロパティの分割
            // 相対フレーム位置で分割するため、オブジェクトの開始フレームを基準とした相対位置で分割
            int relativeSplitFrame = splitFrame - StartFrame;

            // Xプロパティの分割
            var (firstX, secondX) = X.Split(relativeSplitFrame);
            firstText.X = firstX;
            secondText.X = secondX;

            // Yプロパティの分割
            var (firstY, secondY) = Y.Split(relativeSplitFrame);
            firstText.Y = firstY;
            secondText.Y = secondY;

            // Scaleプロパティの分割
            var (firstScale, secondScale) = Scale.Split(relativeSplitFrame);
            firstText.Scale = firstScale;
            secondText.Scale = secondScale;

            // Alphaプロパティの分割
            var (firstAlpha, secondAlpha) = Alpha.Split(relativeSplitFrame);
            firstText.Alpha = firstAlpha;
            secondText.Alpha = secondAlpha;

            // Rotationプロパティの分割
            var (firstRotation, secondRotation) = Rotation.Split(relativeSplitFrame);
            firstText.Rotation = firstRotation;
            secondText.Rotation = secondRotation;

            // TextSizeプロパティの分割
            var (firstTextSize, secondTextSize) = TextSize.Split(relativeSplitFrame);
            firstText.TextSize = firstTextSize;
            secondText.TextSize = secondTextSize;

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

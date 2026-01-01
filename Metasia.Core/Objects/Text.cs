using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Xml;
using SkiaSharp;
using System;
using System.Threading.Tasks;
namespace Metasia.Core.Objects
{
    [Serializable]
    [ClipTypeIdentifier("Text")]
    public class Text : ClipObject, IRenderable
    {
        [EditableProperty("X")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Y")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Scale")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Scale { get; set; } = new MetaNumberParam<double>(100);

        [EditableProperty("Alpha")]
        [ValueRange(0, 100, 0, 100)]
        public MetaNumberParam<double> Alpha { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Rotation")]
        [ValueRange(-99999, 99999, 0, 360)]
        public MetaNumberParam<double> Rotation { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Font")]
        public MetaFontParam Font
        {
            get => _font;
            set
            {
                _font = value?.Clone() ?? MetaFontParam.Default;
                LoadTypeface();
            }
        }

        [Obsolete("Fontプロパティを使用してください。")]
        public string TypefaceName
        {
            get => Font.FamilyName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }
                Font = new MetaFontParam(value, Font.IsBold, Font.IsItalic);
            }
        }

        [EditableProperty("TextContents")]
        public string Contents { get; set; } = string.Empty;

        [EditableProperty("TextSize")]
        [ValueRange(0, 2000, 0, 500)]
        public MetaNumberParam<double> TextSize { get; set; } = new MetaNumberParam<double>(100);

        private SKTypeface? _typeface;
        private MetaFontParam _font = MetaFontParam.Default;

        public Text(string id) : base(id)
        {
            _font = MetaFontParam.Default;
            LoadTypeface();
        }

        public Text()
        {
            _font = MetaFontParam.Default;
            LoadTypeface();
        }

        public Task<RenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //このオブジェクトのStartFrameを基準としたフレーム
            int relativeFrame = context.Frame - StartFrame;
            int clipLength = EndFrame - StartFrame + 1;
            SKPaint skPaint = new SKPaint()
            {
                IsAntialias = true,
                TextSize = (float)TextSize.Get(relativeFrame, clipLength),
                Typeface = _typeface,
                Color = SKColors.White,
            };
            SKRect textBounds = new();
            skPaint.MeasureText(Contents, ref textBounds);

            int bitmapWidth = (int)textBounds.Width;
            int bitmapHeight = (int)textBounds.Height;

            if (bitmapWidth <= 0 || bitmapHeight <= 0 || string.IsNullOrEmpty(Contents))
            {
                return Task.FromResult(new RenderNode());
            }

            var logicalSize = new SKSize(bitmapWidth, bitmapHeight);
            var bitmap = new SKBitmap(bitmapWidth, bitmapHeight);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear();
                canvas.DrawText(Contents, -textBounds.Left, -textBounds.Top, skPaint);
            }

            cancellationToken.ThrowIfCancellationRequested();

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
                Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
                Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
                Rotation = (float)Rotation.Get(relativeFrame, clipLength),
                Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
            };


            return Task.FromResult(new RenderNode()
            {
                Bitmap = bitmap,
                LogicalSize = logicalSize,
                Transform = transform,
            });
        }

        private bool LoadTypeface()
        {
            bool usedFallback = false;

            SKTypeface CreateFallback()
            {
                usedFallback = true;
                var fallbackStyle = new SKFontStyle(
                    Font.IsBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal,
                    Font.IsItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

                try
                {
                    var fallback = SKTypeface.FromFamilyName(MetaFontParam.Default.FamilyName, fallbackStyle);
                    if (fallback is not null)
                    {
                        return fallback;
                    }
                }
                catch
                {
                    // フォールバックフォントの取得に失敗した場合はデフォルトフォントを使用
                }

                return SKTypeface.Default ?? SKTypeface.FromFamilyName("Arial") ?? throw new InvalidOperationException("No fallback typeface available.");
            }

            _typeface?.Dispose();
            _typeface = Font.ResolveTypeface(CreateFallback);
            return !usedFallback;
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

            int clipLength = EndFrame - StartFrame + 1;

            // MetaNumberParamプロパティの分割
            // 相対フレーム位置で分割するため、オブジェクトの開始フレームを基準とした相対位置で分割
            int relativeSplitFrame = splitFrame - StartFrame;

            // Xプロパティの分割
            var (firstX, secondX) = X.Split(relativeSplitFrame, clipLength);
            firstText.X = firstX;
            secondText.X = secondX;

            // Yプロパティの分割
            var (firstY, secondY) = Y.Split(relativeSplitFrame, clipLength);
            firstText.Y = firstY;
            secondText.Y = secondY;

            // Scaleプロパティの分割
            var (firstScale, secondScale) = Scale.Split(relativeSplitFrame, clipLength);
            firstText.Scale = firstScale;
            secondText.Scale = secondScale;

            // Alphaプロパティの分割
            var (firstAlpha, secondAlpha) = Alpha.Split(relativeSplitFrame, clipLength);
            firstText.Alpha = firstAlpha;
            secondText.Alpha = secondAlpha;

            // Rotationプロパティの分割
            var (firstRotation, secondRotation) = Rotation.Split(relativeSplitFrame, clipLength);
            firstText.Rotation = firstRotation;
            secondText.Rotation = secondRotation;

            // TextSizeプロパティの分割
            var (firstTextSize, secondTextSize) = TextSize.Split(relativeSplitFrame, clipLength);
            firstText.TextSize = firstTextSize;
            secondText.TextSize = secondTextSize;

            firstText.Font = Font.Clone();
            secondText.Font = Font.Clone();

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




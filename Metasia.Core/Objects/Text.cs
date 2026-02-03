using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
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

        [EditableProperty("Color")]
        public ColorRgb8 Color { get; set; } = new ColorRgb8(255, 255, 255);

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
            var skFont = new SKFont(_typeface, (float)TextSize.Get(relativeFrame, clipLength));
            skFont.Edging = SKFontEdging.Antialias;
            SKPaint skPaint = new SKPaint()
            {
                IsAntialias = true,
                Color = new SKColor(Color.R, Color.G, Color.B),
            };
            SKRect textBounds = new();
            skFont.MeasureText(Contents, out textBounds, skPaint);

            int bitmapWidth = (int)textBounds.Width;
            int bitmapHeight = (int)textBounds.Height;

            if (bitmapWidth <= 0 || bitmapHeight <= 0 || string.IsNullOrEmpty(Contents))
            {
                return Task.FromResult(new RenderNode());
            }

            var logicalSize = new SKSize(bitmapWidth, bitmapHeight);

            //縦横のレンダリング倍率
            float renderScaleWidth = context.RenderResolution.Width / context.ProjectResolution.Width;
            float renderScaleHeight = context.RenderResolution.Height / context.ProjectResolution.Height;

            //レンダリング倍率に合わせてサイズを計算
            int finalWidth = (int)(bitmapWidth * renderScaleWidth);
            int finalHeight = (int)(bitmapHeight * renderScaleHeight);
            if (finalWidth <= 0 || finalHeight <= 0)
            {
                return Task.FromResult(new RenderNode());
            }

            SKImage image;
            if (renderScaleWidth == 1.0f && renderScaleHeight == 1.0f)
            {
                // リサイズ不要な場合は直接描画
                var info = new SKImageInfo(bitmapWidth, bitmapHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                using var surface = SKSurface.Create(info);
                using var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);
                canvas.DrawText(Contents, new SKPoint(-textBounds.Left, -textBounds.Top), skFont, skPaint);
                cancellationToken.ThrowIfCancellationRequested();
                image = surface.Snapshot();
            }
            else
            {
                // リサイズが必要な場合は、一度大きなサイズで描画してからスケール
                var info = new SKImageInfo(finalWidth, finalHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                using var surface = SKSurface.Create(info);
                using var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);
                canvas.Scale(renderScaleWidth, renderScaleHeight);
                canvas.DrawText(Contents, new SKPoint(-textBounds.Left, -textBounds.Top), skFont, skPaint);
                cancellationToken.ThrowIfCancellationRequested();
                image = surface.Snapshot();
            }

            cancellationToken.ThrowIfCancellationRequested();

            var transform = new Transform()
            {
                Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
                Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
                Rotation = (float)Rotation.Get(relativeFrame, clipLength),
                Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
            };

            return Task.FromResult(new RenderNode()
            {
                Image = image,
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

            firstText.Color = Color.Clone();
            secondText.Color = Color.Clone();

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




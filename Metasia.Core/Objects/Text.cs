using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Core.Xml;
using SkiaSharp;
using System;
using System.Threading.Tasks;
namespace Metasia.Core.Objects
{
    [Serializable]
    [ClipTypeIdentifier("Text", DisplayKey = "clip.text.name", FallbackText = "テキスト")]
    public class Text : ClipObject, IRenderable
    {
        [EditableProperty("BlendMode", DisplayKey = "property.common.blend_mode", FallbackText = "合成モード")]
        public BlendModeParam BlendMode { get; set; } = new BlendModeParam();

        [EditableProperty("X", DisplayKey = "property.common.x", FallbackText = "X")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Y", DisplayKey = "property.common.y", FallbackText = "Y")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Scale", DisplayKey = "property.common.scale", FallbackText = "拡大率")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Scale { get; set; } = new MetaNumberParam<double>(100);

        [EditableProperty("Alpha", DisplayKey = "property.common.alpha", FallbackText = "透明度")]
        [ValueRange(0, 100, 0, 100)]
        public MetaNumberParam<double> Alpha { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Rotation", DisplayKey = "property.common.rotation", FallbackText = "回転")]
        [ValueRange(-99999, 99999, 0, 360)]
        public MetaNumberParam<double> Rotation { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Font", DisplayKey = "property.common.font", FallbackText = "フォント")]
        public MetaFontParam Font
        {
            get => _font;
            set
            {
                _font = value?.Clone() ?? MetaFontParam.Default;
                LoadTypeface();
            }
        }

        [EditableProperty("TextAlign", DisplayKey = "property.text.align", FallbackText = "テキスト配置")]
        public MetaEnumParam TextAlign { get; set; } = new MetaEnumParam("Left", "Center", "Right");

        [EditableProperty("TextContents", DisplayKey = "property.text.contents", FallbackText = "テキスト内容")]
        public string Contents { get; set; } = string.Empty;

        [EditableProperty("TextSize", DisplayKey = "property.text.size", FallbackText = "文字サイズ")]
        [ValueRange(0, 2000, 0, 500)]
        public MetaNumberParam<double> TextSize { get; set; } = new MetaNumberParam<double>(100);

        [EditableProperty("Color", DisplayKey = "property.common.color", FallbackText = "色")]
        public ColorRgb8 Color { get; set; } = new ColorRgb8(255, 255, 255);

        [EditableProperty("TextEffectType", DisplayKey = "property.text.effect_type", FallbackText = "テキスト効果")]
        public MetaEnumParam EffectType { get; set; } = new MetaEnumParam("None", "Stroke", "StrokeThin", "Shadow");

        [EditableProperty("EffectColor", DisplayKey = "property.text.effect_color", FallbackText = "効果色")]
        public ColorRgb8 EffectColor { get; set; } = new ColorRgb8(0, 0, 0);


        public List<VisualEffectBase> VisualEffects { get; set; } = new();

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

        public Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrEmpty(Contents))
            {
                return Task.FromResult<IRenderNode>(new NormalRenderNode());
            }

            // 改行で行を分割
            var lines = Contents.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            float lineSpacing = skFont.Spacing;

            // 効果に応じたマージンを計算
            float effectMarginX = 0;
            float effectMarginY = 0;
            float effectOffsetX = 0;
            float effectOffsetY = 0;
            string effectType = EffectType.SelectedValue;
            float fontSize = (float)TextSize.Get(relativeFrame, clipLength);

            switch (effectType)
            {
                case "Stroke":
                    effectMarginX = fontSize * 0.10f;
                    effectMarginY = fontSize * 0.10f;
                    break;
                case "StrokeThin":
                    effectMarginX = fontSize * 0.04f;
                    effectMarginY = fontSize * 0.04f;
                    break;
                case "Shadow":
                    effectMarginX = 15;
                    effectMarginY = 15;
                    effectOffsetX = 15;
                    effectOffsetY = 15;
                    break;
            }

            // 各行のバウンディング情報を計算
            float totalWidth = 0;
            float totalHeight = 0;
            var lineBounds = new SKRect[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                SKRect bounds = new();
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    skFont.MeasureText(lines[i], out bounds, skPaint);
                    if (bounds.Width > totalWidth)
                    {
                        totalWidth = bounds.Width;
                    }
                }
                lineBounds[i] = bounds;
                totalHeight += (i < lines.Length - 1) ? lineSpacing : bounds.Height;
            }

            float maxLineWidth = totalWidth;

            // 効果のマージンを考慮してサイズを拡張
            totalWidth += effectMarginX * 2;
            totalHeight += effectMarginY * 2;

            int bitmapWidth = (int)Math.Ceiling(totalWidth);
            int bitmapHeight = (int)Math.Ceiling(totalHeight);

            if (bitmapWidth <= 0 || bitmapHeight <= 0)
            {
                return Task.FromResult<IRenderNode>(new NormalRenderNode());
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
                return Task.FromResult<IRenderNode>(new NormalRenderNode());
            }

            SKImage? image = context.ImageCache?.TryGet(GetImageHashCode(relativeFrame, renderScaleWidth, renderScaleHeight));

            if (image is null)
            {
                if (renderScaleWidth == 1.0f && renderScaleHeight == 1.0f)
                {
                    // リサイズ不要な場合は直接描画
                    var info = new SKImageInfo(bitmapWidth, bitmapHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                    using var surface = SKSurface.Create(info);
                    using var canvas = surface.Canvas;
                    canvas.Clear(SKColors.Transparent);
                    DrawMultilineText(canvas, lines, lineBounds, skFont, skPaint, lineSpacing, effectType, EffectColor, effectMarginX, effectMarginY, effectOffsetX, effectOffsetY, TextAlign.SelectedValue, maxLineWidth);
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
                    DrawMultilineText(canvas, lines, lineBounds, skFont, skPaint, lineSpacing, effectType, EffectColor, effectMarginX, effectMarginY, effectOffsetX, effectOffsetY, TextAlign.SelectedValue, maxLineWidth);
                    cancellationToken.ThrowIfCancellationRequested();
                    image = surface.Snapshot();
                }
                context.ImageCache?.Set(GetImageHashCode(relativeFrame, renderScaleWidth, renderScaleHeight), image);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var transform = new Transform()
            {
                Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
                Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
                Rotation = (float)Rotation.Get(relativeFrame, clipLength),
                Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
            };

            long imageCacheKey = GetImageHashCode(relativeFrame, renderScaleWidth, renderScaleHeight);
            var finalResult = VisualEffectPipeline.ApplyEffects(image, VisualEffects, context, StartFrame, EndFrame, logicalSize, imageCacheKey);

            return Task.FromResult<IRenderNode>(new NormalRenderNode()
            {
                Image = finalResult.Image,
                LogicalSize = finalResult.LogicalSize,
                Transform = transform,
                BlendMode = BlendMode.Value,
                ImageCacheKey = finalResult.ImageCacheKey,
            });
        }

        private static void DrawMultilineText(
            SKCanvas canvas,
            string[] lines,
            SKRect[] lineBounds,
            SKFont skFont,
            SKPaint skPaint,
            float lineSpacing,
            string effectType,
            ColorRgb8 effectColor,
            float effectMarginX,
            float effectMarginY,
            float effectOffsetX,
            float effectOffsetY,
            string textAlign,
            float maxLineWidth)
        {
            SKColor effectSkColor = new SKColor(effectColor.R, effectColor.G, effectColor.B);
            float fontSize = skFont.Size;

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    float alignOffset = textAlign switch
                    {
                        "Center" => (maxLineWidth - lineBounds[i].Width) / 2,
                        "Right" => maxLineWidth - lineBounds[i].Width,
                        _ => 0
                    };
                    var position = new SKPoint(-lineBounds[i].Left + effectMarginX + alignOffset, -lineBounds[i].Top + effectMarginY);
                    float y = i * lineSpacing;

                    switch (effectType)
                    {
                        case "Stroke":
                            {
                                float strokeWidth = fontSize * 0.10f;
                                using var strokePaint = new SKPaint()
                                {
                                    IsAntialias = true,
                                    Color = effectSkColor,
                                    Style = SKPaintStyle.Stroke,
                                    StrokeWidth = strokeWidth,
                                    StrokeCap = SKStrokeCap.Round,
                                    StrokeJoin = SKStrokeJoin.Round,
                                };
                                canvas.DrawText(lines[i], new SKPoint(position.X, y + position.Y), skFont, strokePaint);
                                canvas.DrawText(lines[i], new SKPoint(position.X, y + position.Y), skFont, skPaint);
                            }
                            break;
                        case "StrokeThin":
                            {
                                float strokeWidth = fontSize * 0.04f;
                                using var strokePaint = new SKPaint()
                                {
                                    IsAntialias = true,
                                    Color = effectSkColor,
                                    Style = SKPaintStyle.Stroke,
                                    StrokeWidth = strokeWidth,
                                    StrokeCap = SKStrokeCap.Round,
                                    StrokeJoin = SKStrokeJoin.Round,
                                };
                                canvas.DrawText(lines[i], new SKPoint(position.X, y + position.Y), skFont, strokePaint);
                                canvas.DrawText(lines[i], new SKPoint(position.X, y + position.Y), skFont, skPaint);
                            }
                            break;
                        case "Shadow":
                            {
                                using var shadowPaint = new SKPaint()
                                {
                                    IsAntialias = true,
                                    Color = effectSkColor,
                                };
                                canvas.DrawText(lines[i], new SKPoint(position.X + effectOffsetX, y + position.Y + effectOffsetY), skFont, shadowPaint);
                                canvas.DrawText(lines[i], new SKPoint(position.X, y + position.Y), skFont, skPaint);
                            }
                            break;
                        default:
                            canvas.DrawText(lines[i], new SKPoint(position.X, y + position.Y), skFont, skPaint);
                            break;
                    }
                }
            }
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

            var (firstEffectType, secondEffectType) = EffectType.Split(0);
            firstText.EffectType = firstEffectType;
            secondText.EffectType = secondEffectType;

            firstText.EffectColor = EffectColor.Clone();
            secondText.EffectColor = EffectColor.Clone();

            var (firstTextAlign, secondTextAlign) = TextAlign.Split(0);
            firstText.TextAlign = firstTextAlign;
            secondText.TextAlign = secondTextAlign;

            firstText.Font = Font.Clone();
            secondText.Font = Font.Clone();

            var (firstBlendMode, secondBlendMode) = BlendMode.Split();
            firstText.BlendMode = firstBlendMode;
            secondText.BlendMode = secondBlendMode;

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

        private long GetImageHashCode(int relativeFrame, float renderScaleWidth, float renderScaleHeight)
        {
            var hash = new HashCode();
            hash.Add(nameof(Text));
            hash.Add(Contents);
            hash.Add(TextSize.Get(relativeFrame, EndFrame - StartFrame + 1));
            hash.Add(Color);
            hash.Add(Font.FamilyName);
            hash.Add(Font.IsBold);
            hash.Add(Font.IsItalic);
            hash.Add(EffectType.SelectedIndex);
            hash.Add(EffectColor);
            hash.Add(TextAlign.SelectedIndex);
            hash.Add(renderScaleWidth);
            hash.Add(renderScaleHeight);
            return hash.ToHashCode();
        }
    }
}




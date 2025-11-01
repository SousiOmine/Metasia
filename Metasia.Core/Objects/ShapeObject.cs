using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
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
    [ClipTypeIdentifier("Shape")]
    public class ShapeObject : ClipObject, IRenderable
    {
        [EditableProperty("X")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Y")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Size")]
        [ValueRange(1, 99999, 10, 2000)]
        public MetaNumberParam<double> Size { get; set; } = new MetaNumberParam<double>(100);

        [EditableProperty("AspectRatio")]
        [ValueRange(0.1, 10, 0.1, 3)]
        public MetaNumberParam<double> AspectRatio { get; set; } = new MetaNumberParam<double>(1.0);

        [EditableProperty("Scale")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Scale { get; set; } = new MetaNumberParam<double>(100);

        [EditableProperty("Alpha")]
        [ValueRange(0, 100, 0, 100)]
        public MetaNumberParam<double> Alpha { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Rotation")]
        [ValueRange(-99999, 99999, 0, 360)]
        public MetaNumberParam<double> Rotation { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("Shape")]
        public MetaEnumParam Shape { get; set; } = new MetaEnumParam("Circle", "Square", "Triangle", "Star");

        public ShapeObject(string id) : base(id)
        {
        }

        public ShapeObject()
        {
        }

        public Task<RenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //このオブジェクトのStartFrameを基準としたフレーム
            int relativeFrame = context.Frame - StartFrame;

            SKColor shapeColor = SKColors.White;
            int size = (int)Size.Get(relativeFrame);
            double aspectRatio = AspectRatio.Get(relativeFrame);

            int width = size;
            int height = size;

            if (aspectRatio > 1.0)
            {
                width = (int)(size * aspectRatio);
            }
            else if (aspectRatio < 1.0)
            {
                height = (int)(size / aspectRatio);
            }

            var bitmap = new SKBitmap(width, height);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Transparent);

                using (SKPaint paint = new SKPaint())
                {
                    paint.Color = shapeColor;
                    paint.IsAntialias = true;

                    switch (Shape.SelectedValue)
                    {
                        case "Circle":
                            float radiusX = width / 2f;
                            float radiusY = height / 2f;
                            canvas.DrawOval(new SKRect(width / 2f - radiusX, height / 2f - radiusY, width / 2f + radiusX, height / 2f + radiusY), paint);
                            break;
                        case "Square":
                            canvas.DrawRect(0, 0, width, height, paint);
                            break;
                        case "Triangle":
                            var path = new SKPath();
                            path.MoveTo(width / 2f, 0);
                            path.LineTo(0, height);
                            path.LineTo(width, height);
                            path.Close();
                            canvas.DrawPath(path, paint);
                            break;
                        case "Star":
                            float outerRadiusX = width / 2f;
                            float outerRadiusY = height / 2f;
                            float innerRadiusX = outerRadiusX / 2f;
                            float innerRadiusY = outerRadiusY / 2f;
                            DrawStarWithAspectRatio(canvas, paint, width / 2f, height / 2f, outerRadiusX, outerRadiusY, innerRadiusX, innerRadiusY, 5);
                            break;
                    }
                }
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
                Position = new SKPoint((float)X.Get(relativeFrame), (float)Y.Get(relativeFrame)),
                Scale = (float)Scale.Get(relativeFrame) / 100,
                Rotation = (float)Rotation.Get(relativeFrame),
                Alpha = (100.0f - (float)Alpha.Get(relativeFrame)) / 100,
            };

            return Task.FromResult(new RenderNode()
            {
                Bitmap = bitmap,
                LogicalSize = new SKSize(width, height),
                Transform = transform,
            });
        }

        private static void DrawStarWithAspectRatio(SKCanvas canvas, SKPaint paint, float cx, float cy, float outerRadiusX, float outerRadiusY, float innerRadiusX, float innerRadiusY, int points)
        {
            var path = new SKPath();
            double angle = Math.PI / points;

            for (int i = 0; i < 2 * points; i++)
            {
                bool isOuter = i % 2 == 0;
                float radiusX = isOuter ? outerRadiusX : innerRadiusX;
                float radiusY = isOuter ? outerRadiusY : innerRadiusY;

                double x = cx + radiusX * Math.Cos(i * angle - Math.PI / 2);
                double y = cy + radiusY * Math.Sin(i * angle - Math.PI / 2);

                if (i == 0)
                    path.MoveTo((float)x, (float)y);
                else
                    path.LineTo((float)x, (float)y);
            }

            path.Close();
            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// 指定したフレームでオブジェクトを分割する
        /// </summary>
        /// <param name="splitFrame">分割フレーム</param>
        /// <returns>分割後の2つのオブジェクト（前半と後半）</returns>
        public override (ClipObject firstClip, ClipObject secondClip) SplitAtFrame(int splitFrame)
        {
            var (firstClip, secondClip) = base.SplitAtFrame(splitFrame);

            var firstObject = (ShapeObject)firstClip;
            var secondObject = (ShapeObject)secondClip;

            firstObject.Id = Id + "_part1";
            secondObject.Id = Id + "_part2";

            // MetaNumberParamプロパティの分割
            int relativeSplitFrame = splitFrame - StartFrame;

            var (firstX, secondX) = X.Split(relativeSplitFrame);
            firstObject.X = firstX;
            secondObject.X = secondX;

            var (firstY, secondY) = Y.Split(relativeSplitFrame);
            firstObject.Y = firstY;
            secondObject.Y = secondY;

            // Sizeプロパティの分割
            var (firstSize, secondSize) = Size.Split(relativeSplitFrame);
            firstObject.Size = firstSize;
            secondObject.Size = secondSize;

            // AspectRatioプロパティの分割
            var (firstAspectRatio, secondAspectRatio) = AspectRatio.Split(relativeSplitFrame);
            firstObject.AspectRatio = firstAspectRatio;
            secondObject.AspectRatio = secondAspectRatio;

            // Scaleプロパティの分割
            var (firstScale, secondScale) = Scale.Split(relativeSplitFrame);
            firstObject.Scale = firstScale;
            secondObject.Scale = secondScale;

            // Alphaプロパティの分割
            var (firstAlpha, secondAlpha) = Alpha.Split(relativeSplitFrame);
            firstObject.Alpha = firstAlpha;
            secondObject.Alpha = secondAlpha;

            // Rotationプロパティの分割
            var (firstRotation, secondRotation) = Rotation.Split(relativeSplitFrame);
            firstObject.Rotation = firstRotation;
            secondObject.Rotation = secondRotation;

            // MetaEnumParamプロパティの分割
            var (firstShape, secondShape) = Shape.Split(relativeSplitFrame);
            firstObject.Shape = firstShape;
            secondObject.Shape = secondShape;

            return (firstObject, secondObject);
        }

        /// <summary>
        /// オブジェクトのコピーを作成する
        /// </summary>
        /// <returns>コピーされたオブジェクト</returns>
        protected override ClipObject CreateCopy()
        {
            var xml = MetasiaObjectXmlSerializer.Serialize(this);
            var copy = MetasiaObjectXmlSerializer.Deserialize<ShapeObject>(xml);
            copy.Id = Id + "_copy";
            return copy;
        }
    }
}

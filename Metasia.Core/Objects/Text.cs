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
    public class Text : MetasiaObject, IMetaCoordable
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

        public void DrawExpresser(ref DrawExpresserArgs e, int frame)
        {
            if (frame < StartFrame || frame > EndFrame) return;

            SKPaint skPaint = new SKPaint()
            {
                IsAntialias = true,
                TextSize = TextSize.Get(frame),
                Typeface = _typeface,
                Color = SKColors.White,
            };
            SKRect textRect= new SKRect();
            var width = skPaint.MeasureText(Contents, ref textRect);

            e.Bitmap = new SKBitmap((int)(width), (int)(textRect.Height * 1.2));

            using (SKCanvas canvas = new SKCanvas(e.Bitmap))
            {
                canvas.Clear();
                canvas.DrawText(Contents, 0, skPaint.TextSize, skPaint);
            }

            if (Child is not null && Child is IMetaCoordable)
            {
                IMetaCoordable drawChild = (IMetaCoordable)Child;
                Child.StartFrame = this.StartFrame;
                Child.EndFrame = this.EndFrame;
                drawChild.DrawExpresser(ref e, frame);
            }
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

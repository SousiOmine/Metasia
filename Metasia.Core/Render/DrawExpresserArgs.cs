using Metasia.Core.Graphics;
using SkiaSharp;

namespace Metasia.Core.Render;

public class DrawExpresserArgs : IDisposable
{
    public MetasiaBitmap? Bitmap;
    
    public SKSize TargetSize;
    
    public float ResolutionLevel;
    
    public ushort FPS;
    
    public void Dispose()
    {
        if(Bitmap is not null) Bitmap.Dispose();
    }
}
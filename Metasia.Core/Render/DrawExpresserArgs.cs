using Metasia.Core.Graphics;
using SkiaSharp;

namespace Metasia.Core.Render;

public class DrawExpresserArgs : IDisposable
{
    public SKBitmap? Bitmap;
    
    public SKSize TargetSize;
    
    public double ResolutionLevel;
    
    public int FPS;
    
    public void Dispose()
    {
        if(Bitmap is not null) Bitmap.Dispose();
    }
}
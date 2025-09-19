using SkiaSharp;

namespace Metasia.Core.Media;

public class VideoFileAccessorResult
{
    public bool IsSucceed { get; set; } = false;
    public SKBitmap? Bitmap { get; set; } = null;
}
using SkiaSharp;

namespace Metasia.Core.Media;

public class ImageFileAccessorResult
{
    public bool IsSucceed { get; set; } = false;
    public SKBitmap? Bitmap { get; set; } = null;
}

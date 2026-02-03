using SkiaSharp;

namespace Metasia.Core.Media;

public class ImageFileAccessorResult
{
    public bool IsSuccessful { get; set; } = false;
    public SKImage? Image { get; set; } = null;
}

using SkiaSharp;

namespace Metasia.Core.Media;

public class VideoFileAccessorResult
{
    public bool IsSuccessful { get; set; } = false;
    public SKImage? Image { get; set; } = null;
}
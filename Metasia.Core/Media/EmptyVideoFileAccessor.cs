using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyVideoFileAccessor : IVideoFileAccessor
{
    public VideoFileAccessorResult GetBitmap(MediaPath path, TimeSpan time, string? projectDir)
    {
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame, string? projectDir)
    {
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }
}

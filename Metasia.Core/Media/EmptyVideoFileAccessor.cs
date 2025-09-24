using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyVideoFileAccessor : IVideoFileAccessor
{
    public VideoFileAccessorResult GetBitmap(MediaPath path, DateTime time)
    {
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame)
    {
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }
}

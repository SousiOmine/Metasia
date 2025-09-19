using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyVideoFileAccessor : IVideoFileAccessor
{
    public VideoFileAccessorResult GetBitmap(MediaPath path, DateTime time, SKSize size)
    {
        return new VideoFileAccessorResult { IsSucceed = false, Bitmap = null };
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame, SKSize size)
    {
        return new VideoFileAccessorResult { IsSucceed = false, Bitmap = null };
    }
}

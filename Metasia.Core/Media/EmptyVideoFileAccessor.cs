using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyVideoFileAccessor : IVideoFileAccessor
{
    public Task<VideoFileAccessorResult> GetBitmapAsync(string path, TimeSpan time)
    {
        return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null });
    }

    public Task<VideoFileAccessorResult> GetBitmapAsync(string path, int frame)
    {
        return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null });
    }
}

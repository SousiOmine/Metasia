using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyVideoFileAccessor : IVideoFileAccessor
{
    public Task<VideoFileAccessorResult> GetBitmapAsync(MediaPath path, TimeSpan time, string? projectDir)
    {
        return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null });
    }

    public Task<VideoFileAccessorResult> GetBitmapAsync(MediaPath path, int frame, string? projectDir)
    {
        return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null });
    }
}

using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyVideoFileAccessor : IVideoFileAccessor
{
    public Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
    {
        return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Image = null });
    }

    public Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
    {
        return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Image = null });
    }
}

namespace Metasia.Core.Media;

public interface IVideoFileAccessor : IMediaAccessor
{
    public Task<VideoFileAccessorResult> GetBitmapAsync(MediaPath path, TimeSpan time, string? projectDir);

    public Task<VideoFileAccessorResult> GetBitmapAsync(MediaPath path, int frame, string? projectDir);
}
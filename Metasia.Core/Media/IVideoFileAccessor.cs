namespace Metasia.Core.Media;

public interface IVideoFileAccessor : IMediaAccessor
{
    public Task<VideoFileAccessorResult> GetBitmapAsync(string path, TimeSpan time);

    public Task<VideoFileAccessorResult> GetBitmapAsync(string path, int frame);
}
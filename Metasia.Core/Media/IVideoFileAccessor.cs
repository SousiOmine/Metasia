namespace Metasia.Core.Media;

public interface IVideoFileAccessor : IMediaAccessor
{
    public Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time);

    public Task<VideoFileAccessorResult> GetImageAsync(string path, int frame);
}
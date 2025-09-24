namespace Metasia.Core.Media;

public interface IVideoFileAccessor : IMediaAccessor
{
    public VideoFileAccessorResult GetBitmap(MediaPath path, TimeSpan time, string? projectDir);

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame, string? projectDir);
}
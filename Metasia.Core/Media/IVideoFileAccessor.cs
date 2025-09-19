namespace Metasia.Core.Media;

public interface IVideoFileAccessor : IMediaAccessor
{
    public VideoFileAccessorResult GetBitmap(MediaPath path, DateTime time);

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame);
}
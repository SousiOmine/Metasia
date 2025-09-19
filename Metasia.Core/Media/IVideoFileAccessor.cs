using Metasia.Core.Graphics;
using SkiaSharp;

namespace Metasia.Core.Media;

public interface IVideoFileAccessor : IMediaAccessor
{
    public VideoFileAccessorResult GetBitmap(MediaPath path, DateTime time, SKSize size);

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame, SKSize size);
}
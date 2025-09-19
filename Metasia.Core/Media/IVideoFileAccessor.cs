using Metasia.Core.Graphics;
using SkiaSharp;

namespace Metasia.Core.Media;

public interface IVideoFileAccessor : IMediaAccessor
{
    public void GetBitmap(MediaPath path, DateTime time, SKSize size);

    public void GetBitmap(MediaPath path, int frame, SKSize size);
}
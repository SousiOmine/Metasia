using SkiaSharp;

namespace Metasia.Core.Media;

public interface IImageFileAccessor : IMediaAccessor
{
    public ImageFileAccessorResult GetBitmap(MediaPath path, SKSize size);
}

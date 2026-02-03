using SkiaSharp;

namespace Metasia.Core.Media;

public interface IImageFileAccessor : IMediaAccessor
{
    public Task<ImageFileAccessorResult> GetImageAsync(string path);
}

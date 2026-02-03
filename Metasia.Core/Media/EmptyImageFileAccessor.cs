using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyImageFileAccessor : IImageFileAccessor
{
    public async Task<ImageFileAccessorResult> GetImageAsync(string path)
    {
        return await Task.FromResult(new ImageFileAccessorResult()
        {
            IsSuccessful = false,
            Image = null,
        });
    }
}

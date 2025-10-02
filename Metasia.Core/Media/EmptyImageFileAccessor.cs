using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyImageFileAccessor : IImageFileAccessor
{
    public async Task<ImageFileAccessorResult> GetBitmapAsync(MediaPath path)
    {
        return await Task.FromResult(new ImageFileAccessorResult(){
            IsSuccessful = false,
            Bitmap = null,
        });
    }
}

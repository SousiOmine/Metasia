using SkiaSharp;

namespace Metasia.Core.Media;

public class EmptyImageFileAccessor : IImageFileAccessor
{
    public ImageFileAccessorResult GetBitmap(MediaPath path)
    {
        return new ImageFileAccessorResult(){
            IsSuccessful = false,
            Bitmap = null,
        };
    }
}

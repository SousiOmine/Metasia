using System.IO;
using System.Threading.Tasks;
using Metasia.Core.Media;
using SkiaSharp;

namespace Metasia.Editor.Models.Media.StandardInput;

public class StdInput : IImageFileAccessor
{
    public async Task<ImageFileAccessorResult> GetBitmapAsync(MediaPath path)
    {
        var fullPath = MediaPath.GetFullPath(path, "");
        if (!File.Exists(fullPath))
        {
            return new ImageFileAccessorResult { IsSuccessful = false, Bitmap = null };
        }

        SKBitmap bitmap = SKBitmap.Decode(fullPath);
        return new ImageFileAccessorResult { IsSuccessful = true, Bitmap = bitmap };
    } 
}
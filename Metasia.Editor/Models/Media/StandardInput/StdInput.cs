using System.IO;
using Metasia.Core.Media;
using SkiaSharp;

namespace Metasia.Editor.Models.Media.StandardInput;

public class StdInput : IImageFileAccessor
{
    public ImageFileAccessorResult GetBitmap(MediaPath path)
    {
        SKBitmap bitmap = SKBitmap.Decode(MediaPath.GetFullPath(path, ""));
        return new ImageFileAccessorResult { IsSucceed = true, Bitmap = bitmap };
    }
}
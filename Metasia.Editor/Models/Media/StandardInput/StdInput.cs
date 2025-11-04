using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Metasia.Core.Media;
using SkiaSharp;

namespace Metasia.Editor.Models.Media.StandardInput;

public class StdInput : IImageFileAccessor
{
    private static readonly ConcurrentDictionary<string, SKBitmap> _imageCache = new();
    
    public async Task<ImageFileAccessorResult> GetBitmapAsync(MediaPath path)
    {
        var fullPath = MediaPath.GetFullPath(path, "");
        if (!File.Exists(fullPath))
        {
            return new ImageFileAccessorResult { IsSuccessful = false, Bitmap = null };
        }

        if (_imageCache.TryGetValue(fullPath, out var cachedBitmap))
        {
            return new ImageFileAccessorResult { IsSuccessful = true, Bitmap = cachedBitmap };
        }

        SKBitmap bitmap = SKBitmap.Decode(fullPath);
        if (bitmap != null)
        {
            _imageCache.TryAdd(fullPath, bitmap);
        }
        
        return new ImageFileAccessorResult { IsSuccessful = true, Bitmap = bitmap };
    }
}
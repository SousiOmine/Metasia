using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Metasia.Core.Media;
using SkiaSharp;

namespace Metasia.Editor.Models.Media.StandardInput;

public class StdInput : IImageFileAccessor
{
    private static readonly ConcurrentDictionary<string, SKBitmap> _imageCache = new();
    
    public async Task<ImageFileAccessorResult> GetBitmapAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!File.Exists(path))
        {
            return new ImageFileAccessorResult { IsSuccessful = false, Bitmap = null };
        }

        if (_imageCache.TryGetValue(path, out var cachedBitmap))
        {
            return new ImageFileAccessorResult { IsSuccessful = true, Bitmap = cachedBitmap };
        }

        SKBitmap bitmap = SKBitmap.Decode(path);
        if (bitmap != null)
        {
            _imageCache.TryAdd(path, bitmap);
        }
        
        return new ImageFileAccessorResult { IsSuccessful = true, Bitmap = bitmap };
    }
}
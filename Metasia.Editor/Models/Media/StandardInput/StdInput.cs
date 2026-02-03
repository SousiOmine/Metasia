using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Metasia.Core.Media;
using SkiaSharp;

namespace Metasia.Editor.Models.Media.StandardInput;

public class StdInput : IImageFileAccessor
{
    private static readonly ConcurrentDictionary<string, SKImage> _imageCache = new();

    public async Task<ImageFileAccessorResult> GetImageAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!File.Exists(path))
        {
            return new ImageFileAccessorResult { IsSuccessful = false, Image = null };
        }

        if (_imageCache.TryGetValue(path, out var cachedImage))
        {
            return new ImageFileAccessorResult { IsSuccessful = true, Image = cachedImage };
        }

        using SKBitmap? bitmap = SKBitmap.Decode(path);
        if (bitmap is not null)
        {
            var image = SKImage.FromBitmap(bitmap);
            _imageCache.TryAdd(path, image);
            return new ImageFileAccessorResult { IsSuccessful = true, Image = image };
        }

        return new ImageFileAccessorResult { IsSuccessful = false, Image = null };
    }
}
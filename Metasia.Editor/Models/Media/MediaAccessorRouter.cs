using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metasia.Core.Media;
using Metasia.Editor.Models.Media.StandardInput;
using SkiaSharp;

namespace Metasia.Editor.Models.Media;

public class MediaAccessorRouter : IImageFileAccessor, IVideoFileAccessor
{
    public List<IMediaAccessor> Accessors { get; } = [];

    public MediaAccessorRouter()
    {
        Accessors.Add(new StdInput());
    }

    public async Task<ImageFileAccessorResult> GetBitmapAsync(string path)
    {
        foreach (var accessor in Accessors)
        {
            if (accessor is IImageFileAccessor imageAccessor)
            {
                var result = await imageAccessor.GetBitmapAsync(path);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new ImageFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }

    public async Task<VideoFileAccessorResult> GetBitmapAsync(string path, TimeSpan time)
    {
        foreach (var accessor in Accessors)
        {
            if (accessor is IVideoFileAccessor videoAccessor)
            {
                var result = await videoAccessor.GetBitmapAsync(path, time);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }

    public async Task<VideoFileAccessorResult> GetBitmapAsync(string path, int frame)
    {
        foreach (var accessor in Accessors)
        {
            if (accessor is IVideoFileAccessor videoAccessor)
            {
                var result = await videoAccessor.GetBitmapAsync(path, frame);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }
}

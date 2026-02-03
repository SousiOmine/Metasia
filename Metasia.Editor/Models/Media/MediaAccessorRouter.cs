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

    public async Task<ImageFileAccessorResult> GetImageAsync(string path)
    {
        foreach (var accessor in Accessors)
        {
            if (accessor is IImageFileAccessor imageAccessor)
            {
                var result = await imageAccessor.GetImageAsync(path);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new ImageFileAccessorResult { IsSuccessful = false, Image = null };
    }

    public async Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
    {
        foreach (var accessor in Accessors)
        {
            if (accessor is IVideoFileAccessor videoAccessor)
            {
                var result = await videoAccessor.GetImageAsync(path, time);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Image = null };
    }

    public async Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
    {
        foreach (var accessor in Accessors)
        {
            if (accessor is IVideoFileAccessor videoAccessor)
            {
                var result = await videoAccessor.GetImageAsync(path, frame);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Image = null };
    }
}

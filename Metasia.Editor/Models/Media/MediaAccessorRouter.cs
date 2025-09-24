using System;
using System.Collections.Generic;
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
    
    public ImageFileAccessorResult GetBitmap(MediaPath path)
    {
        foreach(var accessor in Accessors)
        {
            if(accessor is IImageFileAccessor imageAccessor)
            {
                var result = imageAccessor.GetBitmap(path);
                if(result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new ImageFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, TimeSpan time, string? projectDir)
    {
        foreach(var accessor in Accessors)
        {
            if(accessor is IVideoFileAccessor videoAccessor)
            {
                var result = videoAccessor.GetBitmap(path, time, projectDir);
                if(result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame, string? projectDir)
    {
        foreach(var accessor in Accessors)
        {
            if(accessor is IVideoFileAccessor videoAccessor)
            {
                var result = videoAccessor.GetBitmap(path, frame, projectDir);
                if(result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Bitmap = null };
    }
}

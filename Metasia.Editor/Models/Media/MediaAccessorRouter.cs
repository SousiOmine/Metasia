using System;
using System.Collections.Generic;
using Metasia.Core.Media;
using SkiaSharp;

namespace Metasia.Editor.Models.Media;

public class MediaAccessorRouter : IImageFileAccessor, IVideoFileAccessor
{
    

    public IReadOnlyList<IMediaAccessor> Accessors => _accessors;

    private readonly List<IMediaAccessor> _accessors = new();

    public void AddAccessor(IMediaAccessor accessor)
    {
        _accessors.Add(accessor);
    }

    public void InsertAccessor(int index, IMediaAccessor accessor)
    {
        _accessors.Insert(index, accessor);
    }

    public void RemoveAccessor(IMediaAccessor accessor)
    {
        _accessors.Remove(accessor);
    }
    
    public ImageFileAccessorResult GetBitmap(MediaPath path, SKSize size)
    {
        return new ImageFileAccessorResult { IsSucceed = false, Bitmap = null };
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, DateTime time, SKSize size)
    {
        return new VideoFileAccessorResult { IsSucceed = false, Bitmap = null };
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame, SKSize size)
    {
        return new VideoFileAccessorResult { IsSucceed = false, Bitmap = null };
    }
}

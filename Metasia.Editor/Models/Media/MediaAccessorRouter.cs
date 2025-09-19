using System;
using Metasia.Core.Media;
using SkiaSharp;

namespace Metasia.Editor.Models.Media;

public class MediaAccessorRouter : IImageFileAccessor, IVideoFileAccessor
{
    public ImageFileAccessorResult GetBitmap(MediaPath path, SKSize size)
    {
        throw new System.NotImplementedException();
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, DateTime time, SKSize size)
    {
        throw new System.NotImplementedException();
    }

    public VideoFileAccessorResult GetBitmap(MediaPath path, int frame, SKSize size)
    {
        throw new System.NotImplementedException();
    }
}

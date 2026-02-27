using System;
using System.Collections.Generic;
using Avalonia.Media;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models;

public class DefaultClipColorProvider : IClipColorProvider
{
    private static readonly IBrush FallbackBrush = new SolidColorBrush(Color.FromRgb(158, 158, 158));

    private static readonly Dictionary<string, IBrush> TypeColorMap = new()
    {
        { "VideoObject", new SolidColorBrush(Color.FromRgb(66, 133, 244)) },
        { "AudioObject", new SolidColorBrush(Color.FromRgb(234, 67, 53)) },
        { "Text",        new SolidColorBrush(Color.FromRgb(251, 188, 4)) },
        { "Shape",       new SolidColorBrush(Color.FromRgb(154, 110, 219)) },
        { "ImageObject", new SolidColorBrush(Color.FromRgb(0, 172, 193)) },
        { "CameraControlObject", new SolidColorBrush(Color.FromRgb(255, 112, 67)) },
        { "GroupControlObject",  new SolidColorBrush(Color.FromRgb(255, 112, 67)) },
    };

    public IBrush GetBrush(ClipObject clip)
    {
        ArgumentNullException.ThrowIfNull(clip);

        var attr = Attribute.GetCustomAttribute(clip.GetType(), typeof(ClipTypeIdentifierAttribute))
            as ClipTypeIdentifierAttribute;

        if (attr is not null && TypeColorMap.TryGetValue(attr.Identifier, out var brush))
        {
            return brush;
        }

        return FallbackBrush;
    }
}

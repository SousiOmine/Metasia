using Avalonia.Media;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models;

public interface IClipColorProvider
{
    IBrush GetBrush(ClipObject clip);
}

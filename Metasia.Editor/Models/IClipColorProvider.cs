using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Media;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;

namespace Metasia.Editor.Models;

public interface IClipColorProvider
{
    IBrush GetBrush(ClipObject clip);
}

using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Editor.Abstractions.EditCommands;

namespace Metasia.Editor.ViewModels.Timeline;

public interface IClipViewModelFactory
{
    ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline);
}

using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.ViewModels.Timeline;

public interface IClipViewModelFactory
{
    ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline);
}

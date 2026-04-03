using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;

using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Timeline;

public interface ILayerCanvasViewModelFactory
{
    LayerCanvasViewModel Create(TimelineViewModel parentTimelineViewModel, LayerObject targetLayerObject);
}
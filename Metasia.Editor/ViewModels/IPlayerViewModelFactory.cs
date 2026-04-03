using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Project;

namespace Metasia.Editor.ViewModels;

public interface IPlayerViewModelFactory
{
    PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo);
}

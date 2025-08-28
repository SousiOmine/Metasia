using Metasia.Core.Objects;
using Metasia.Core.Project;

namespace Metasia.Editor.ViewModels;

public interface IPlayerViewModelFactory
{
    PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo);
}

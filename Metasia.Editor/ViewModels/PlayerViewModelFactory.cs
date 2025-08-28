using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Services.Audio;

namespace Metasia.Editor.ViewModels;

public class PlayerViewModelFactory : IPlayerViewModelFactory
{
    private IEditCommandManager editCommandManager;
    private IAudioPlaybackService audioPlaybackService;

    public PlayerViewModelFactory(IEditCommandManager editCommandManager, IAudioPlaybackService audioPlaybackService)
    {
        this.editCommandManager = editCommandManager;
        this.audioPlaybackService = audioPlaybackService;
    }
    public PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo)
    {
        return new PlayerViewModel(timeline, projectInfo, editCommandManager, audioPlaybackService);
    }
}

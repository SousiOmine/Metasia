using System;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Services.Audio;

namespace Metasia.Editor.ViewModels;

public class PlayerViewModelFactory : IPlayerViewModelFactory
{
    private readonly IEditCommandManager editCommandManager;
    private readonly IAudioPlaybackService audioPlaybackService;

    public PlayerViewModelFactory(IEditCommandManager editCommandManager, IAudioPlaybackService audioPlaybackService)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(audioPlaybackService);
        this.editCommandManager = editCommandManager;
        this.audioPlaybackService = audioPlaybackService;
    }
    public PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(projectInfo);
        return new PlayerViewModel(timeline, projectInfo, editCommandManager, audioPlaybackService);
    }
}

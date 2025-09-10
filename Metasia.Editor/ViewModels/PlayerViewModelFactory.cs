using System;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels;

public class PlayerViewModelFactory : IPlayerViewModelFactory
{
    private readonly ISelectionState selectionState;
    private readonly IPlaybackState playbackState;
    public PlayerViewModelFactory(ISelectionState selectionState, IPlaybackState playbackState)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(playbackState);
        this.selectionState = selectionState;
        this.playbackState = playbackState;
    }
    public PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(projectInfo);
        return new PlayerViewModel(timeline, projectInfo, selectionState, playbackState);
    }
}

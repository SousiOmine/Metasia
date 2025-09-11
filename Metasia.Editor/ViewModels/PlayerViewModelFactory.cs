using System;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels;

public class PlayerViewModelFactory : IPlayerViewModelFactory
{
    private readonly ISelectionState selectionState;
    private readonly IPlaybackState playbackState;
    private readonly IEditCommandManager editCommandManager;
    private readonly IProjectState projectState;
    public PlayerViewModelFactory(ISelectionState selectionState, IPlaybackState playbackState, IEditCommandManager editCommandManager, IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        this.selectionState = selectionState;
        this.playbackState = playbackState;
        this.editCommandManager = editCommandManager;
        this.projectState = projectState;
    }
    public PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(projectInfo);
        return new PlayerViewModel(timeline, projectInfo, selectionState, playbackState, projectState, editCommandManager);
    }
}

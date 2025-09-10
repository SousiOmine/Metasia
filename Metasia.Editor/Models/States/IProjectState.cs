using System;
using System.Threading.Tasks;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.Projects;


namespace Metasia.Editor.Models.States;

public interface IProjectState : IDisposable
{
    MetasiaEditorProject? CurrentProject { get; }
    ProjectInfo? CurrentProjectInfo { get; }

    TimelineObject? CurrentTimeline { get; }

    Task LoadProjectAsync(MetasiaEditorProject project);

    void CloseProject();

    void SetCurrentTimeline(TimelineObject timeline);

    event Action? ProjectLoaded;

    event Action? ProjectClosed;

    event Action? TimelineChanged;
}
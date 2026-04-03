using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Core.Objects;
using Metasia.Editor.Models.Projects;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class TimelineAddCommand : IEditCommand
    {
        public string Description => "タイムラインの追加";

        private readonly MetasiaEditorProject _project;
        private readonly TimelineObject _timeline;

        public TimelineObject AddedTimeline => _timeline;

        public TimelineAddCommand(MetasiaEditorProject project, TimelineObject timeline)
        {
            _project = project;
            _timeline = timeline;
        }

        public void Execute()
        {
            if (!_project.Timelines.Contains(_timeline))
            {
                _project.Timelines.Add(_timeline);
            }
        }

        public void Undo()
        {
            if (_project.Timelines.Contains(_timeline))
            {
                _project.Timelines.Remove(_timeline);
            }
        }
    }
}
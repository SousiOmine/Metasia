using Metasia.Core.Objects;
using Metasia.Editor.Models.Projects;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class TimelineRemoveCommand : IEditCommand
    {
        public string Description => "タイムラインの削除";

        private readonly MetasiaEditorProject _project;
        private readonly TimelineObject _timeline;
        private int _removedIndex = -1;

        public TimelineRemoveCommand(MetasiaEditorProject project, TimelineObject timeline)
        {
            _project = project;
            _timeline = timeline;
        }

        public void Execute()
        {
            var index = _project.Timelines.IndexOf(_timeline);
            if (index >= 0)
            {
                _removedIndex = index;
                _project.Timelines.RemoveAt(index);
            }
        }

        public void Undo()
        {
            if (_removedIndex >= 0)
            {
                _project.Timelines.Insert(_removedIndex, _timeline);
            }
        }
    }
}
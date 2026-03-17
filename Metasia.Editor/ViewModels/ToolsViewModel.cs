using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels.Tools;

namespace Metasia.Editor.ViewModels
{
    public class ToolsViewModel
    {
        public ProjectToolViewModel ProjectToolVM { get; }

        public ToolsViewModel(PlayerParentViewModel playerParentViewModel, IProjectState projectState, ISelectionState selectionState, IEditCommandManager editCommandManager)
        {
            ProjectToolVM = new ProjectToolViewModel(playerParentViewModel, projectState, selectionState, editCommandManager);
        }
    }
}


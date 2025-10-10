using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels.Tools;

namespace Metasia.Editor.ViewModels
{
    public class ToolsViewModel
    {
        public ProjectToolViewModel ProjectToolVM { get; }

        public ToolsViewModel(PlayerParentViewModel playerParentViewModel, IProjectState projectState)
        {
            ProjectToolVM = new ProjectToolViewModel(playerParentViewModel, projectState);
        }
    }
}


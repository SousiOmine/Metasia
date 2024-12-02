using Metasia.Editor.ViewModels.Tools;

namespace Metasia.Editor.ViewModels
{
    public class ToolsViewModel
    {
        public ProjectToolViewModel ProjectToolVM { get; }
        
        public ToolsViewModel()
        {
            ProjectToolVM = new ProjectToolViewModel(null);
        }
    }
}


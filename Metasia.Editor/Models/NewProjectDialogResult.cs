using Metasia.Core.Project;

namespace Metasia.Editor.Models
{
    public class NewProjectDialogResult
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public ProjectInfo ProjectInfo { get; set; } = new ProjectInfo();
        public MetasiaProject? SelectedTemplate { get; set; }
        public bool Success { get; set; }
    }
}
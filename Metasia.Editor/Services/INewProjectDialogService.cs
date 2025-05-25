using System.Threading.Tasks;
using Metasia.Editor.Models.ProjectGenerate;

namespace Metasia.Editor.Services
{
    public interface INewProjectDialogService
    {
        Task<NewProjectDialogResult?> ShowNewProjectDialogAsync();
    }

    public class NewProjectDialogResult
    {
        public bool Result { get; set; }
        public string ProjectPath { get; set; }
        public ProjectInfo ProjectInfo { get; set; }
        public ProjectTemplate SelectedTemplate { get; set; }
    }
}
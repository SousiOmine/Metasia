
using System.Threading.Tasks;
using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;

namespace Metasia.Editor.Services
{
    public interface INewProjectDialogService
    {
        Task<(bool, string, ProjectInfo, MetasiaProject?)> ShowDialogAsync();
    }
}

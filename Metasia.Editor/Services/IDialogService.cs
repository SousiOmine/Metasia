using System.Threading.Tasks;
using Metasia.Editor.ViewModels;

namespace Metasia.Editor.Services
{
    public interface IDialogService
    {
        Task<T?> ShowDialogAsync<T>(object viewModel) where T : class;
        Task<bool> ShowNewProjectDialogAsync(NewProjectDialogViewModel viewModel);
    }
}
using System.Threading.Tasks;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Models;

namespace Metasia.Editor.Services
{
    public interface IDialogService
    {
        Task<TResult?> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel) 
            where TViewModel : ViewModelBase;
        
        Task<NewProjectDialogResult?> ShowNewProjectDialogAsync();
    }
}
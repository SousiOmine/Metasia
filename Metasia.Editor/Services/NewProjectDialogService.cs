using System.Threading.Tasks;
using Avalonia.Controls;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Views;

namespace Metasia.Editor.Services
{
    public class NewProjectDialogService : INewProjectDialogService
    {
        public async Task<NewProjectDialogResult?> ShowNewProjectDialogAsync()
        {
            var window = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (window == null) return null;

            var dialog = new NewProjectDialog();
            var result = await dialog.ShowDialog<bool>(window);

            if (result)
            {
                return new NewProjectDialogResult
                {
                    Result = result,
                    ProjectPath = dialog.ProjectPath,
                    ProjectInfo = dialog.ProjectInfo,
                    SelectedTemplate = dialog.SelectedTemplate
                };
            }
            return null;
        }
    }
}
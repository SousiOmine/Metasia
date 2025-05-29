using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Models;
using Metasia.Editor.Views;

namespace Metasia.Editor.Services
{
    public class DialogService : IDialogService
    {
        public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel) 
            where TViewModel : ViewModelBase
        {
            // ViewLocatorを使用してViewを取得
            var viewLocator = new ViewLocator();
            var view = viewLocator.Build(viewModel);
            if (view is Window window)
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    return await window.ShowDialog<TResult>(mainWindow);
                }
            }
            return default;
        }

        public async Task<NewProjectDialogResult?> ShowNewProjectDialogAsync()
        {
            var viewModel = new NewProjectDialogViewModel();
            var dialog = new NewProjectDialog();
            dialog.DataContext = viewModel;

            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;

            var tcs = new TaskCompletionSource<NewProjectDialogResult?>();
            
            viewModel.OnDialogResult = (result) =>
            {
                tcs.SetResult(result);
                dialog.Close();
            };

            // ShowDialogは非同期で実行し、結果を待つ
            _ = dialog.ShowDialog(mainWindow);
            
            return await tcs.Task;
        }

        private Window? GetMainWindow()
        {
            return App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        }
    }
}
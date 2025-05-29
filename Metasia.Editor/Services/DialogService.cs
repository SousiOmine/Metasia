using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Views;

namespace Metasia.Editor.Services
{
    public class DialogService : IDialogService
    {
        private readonly Window _owner;

        public DialogService(Window owner)
        {
            _owner = owner;
        }

        public async Task<T?> ShowDialogAsync<T>(object viewModel) where T : class
        {
            // ViewLocatorを使用してViewModelに対応するViewを取得
            var viewType = Type.GetType(viewModel.GetType().FullName!.Replace("ViewModel", ""));
            if (viewType == null)
                return default;

            var dialog = (Window)Activator.CreateInstance(viewType)!;
            dialog.DataContext = viewModel;
            
            return await dialog.ShowDialog<T>(_owner);
        }

        public async Task<bool> ShowNewProjectDialogAsync(NewProjectDialogViewModel viewModel)
        {
            var dialog = new NewProjectDialog
            {
                DataContext = viewModel
            };
            
            return await dialog.ShowDialog<bool>(_owner);
        }
    }
}
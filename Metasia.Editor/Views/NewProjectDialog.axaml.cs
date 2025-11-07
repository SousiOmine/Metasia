using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views
{
    public partial class NewProjectDialog : Window
    {
        private NewProjectViewModel? _viewModel;
        private IDisposable? _okCommandSubscription;
        private IDisposable? _cancelCommandSubscription;
        private IDisposable? _browseFolderCommandSubscription;

        public NewProjectDialog()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs args)
        {
            // Dispose of any existing subscriptions
            _okCommandSubscription?.Dispose();
            _cancelCommandSubscription?.Dispose();
            _browseFolderCommandSubscription?.Dispose();
            _okCommandSubscription = null;
            _cancelCommandSubscription = null;
            _browseFolderCommandSubscription = null;

            if (this.DataContext is NewProjectViewModel vm)
            {
                _viewModel = vm;

                // Subscribe to the commands
                _okCommandSubscription = _viewModel.OkCommand
                    .Subscribe(result => Close(result));

                _cancelCommandSubscription = _viewModel.CancelCommand
                    .Subscribe(result => Close(result));

                _browseFolderCommandSubscription = _viewModel.BrowseFolderCommand
                    .Subscribe(async _ => await BrowseFolderAsync());
            }
            else
            {
                _viewModel = null;
            }
        }

        private async Task BrowseFolderAsync()
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "保存先フォルダを選択",
                AllowMultiple = false
            });

            if (folders.Count > 0 && _viewModel != null)
            {
                _viewModel.FolderPath = folders[0].Path.LocalPath;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _okCommandSubscription?.Dispose();
            _cancelCommandSubscription?.Dispose();
            _browseFolderCommandSubscription?.Dispose();
            
            this.DataContextChanged -= OnDataContextChanged;

            base.OnClosed(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}



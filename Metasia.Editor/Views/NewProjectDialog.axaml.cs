using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views
{
    public partial class NewProjectDialog : Window
    {
        private NewProjectViewModel? _viewModel;
        private IDisposable? _okCommandSubscription;
        private IDisposable? _cancelCommandSubscription;

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
            _okCommandSubscription = null;
            _cancelCommandSubscription = null;

            if (this.DataContext is NewProjectViewModel vm)
            {
                _viewModel = vm;

                // Subscribe to the commands
                _okCommandSubscription = _viewModel.OkCommand
                    .Subscribe(result => Close(result));

                _cancelCommandSubscription = _viewModel.CancelCommand
                    .Subscribe(result => Close(result));
            }
            else
            {
                _viewModel = null;
            }
        }



        protected override void OnClosed(EventArgs e)
        {
            _okCommandSubscription?.Dispose();
            _cancelCommandSubscription?.Dispose();

            this.DataContextChanged -= OnDataContextChanged;

            base.OnClosed(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}



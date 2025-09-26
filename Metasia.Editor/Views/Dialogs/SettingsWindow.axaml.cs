using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views.Dialogs
{
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel? _viewModel;
        private IDisposable? _okCommandSubscription;
        private IDisposable? _cancelCommandSubscription;

        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
            OnDataContextChanged(this, EventArgs.Empty);
        }
        private void OnDataContextChanged(object? sender, EventArgs args)
        {
            // Dispose of any existing subscriptions
            _okCommandSubscription?.Dispose();
            _cancelCommandSubscription?.Dispose();
            _okCommandSubscription = null;
            _cancelCommandSubscription = null;

            if (this.DataContext is SettingsViewModel vm)
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
            // Clean up subscriptions when window is closed
            _okCommandSubscription?.Dispose();
            _cancelCommandSubscription?.Dispose();

            base.OnClosed(e);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            
            // ウィンドウが開かれたときに設定を再読み込み
            if (_viewModel != null)
            {
                _ = _viewModel.ReloadSettingsAsync();
            }
        }
    }
}

using System;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Views;
using Metasia.Editor.Views.Settings;

namespace Metasia.Editor.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel => DataContext as MainWindowViewModel;
        private IDisposable? _newProjectHandlerDisposable;
        private IDisposable? _openSettingsHandlerDisposable;

        public MainWindow()
        {
            InitializeComponent();

            // DataContextが設定された後にキーバインディングを適用
            this.DataContextChanged += OnDataContextChanged;

            // ウィンドウがロードされた時にフォーカスを設定
            this.Loaded += (s, e) =>
            {
                this.Focus();
            };
        }

        private void OnDataContextChanged(object? sender, EventArgs args)
        {
            // Dispose previous handler if it exists
            _newProjectHandlerDisposable?.Dispose();
            _openSettingsHandlerDisposable?.Dispose();
            _newProjectHandlerDisposable = null;
            _openSettingsHandlerDisposable = null;

            if (_viewModel is not { } viewModel)
            {
                return;
            }

            // ViewModelが設定された後にキーバインディングを適用
            var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
            keyBindingService?.ApplyKeyBindings(this);

            // NewProjectInteractionのハンドラーを登録
            _newProjectHandlerDisposable = viewModel.NewProjectInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    var dialog = new NewProjectDialog()
                    {
                        DataContext = interaction.Input
                    };
                    var result = await dialog.ShowDialog<(bool, string, Metasia.Core.Project.ProjectInfo, Metasia.Core.Project.MetasiaProject?)>(this);
                    interaction.SetOutput(result);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in NewProjectInteraction handler: {ex.Message}");
                    interaction.SetOutput((false, ex.Message, null, null));
                }
            });

            // OpenSettingsInteractionのハンドラーを登録
            _openSettingsHandlerDisposable = viewModel.OpenSettingsInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    var serviceProvider = App.Current?.Services;
                    if (serviceProvider is not null)
                    {
                        var settingsWindow = new SettingsWindow()
                        {
                            DataContext = serviceProvider.GetRequiredService<SettingsWindowViewModel>()
                        };
                        await settingsWindow.ShowDialog(this);
                    }
                    interaction.SetOutput(Unit.Default);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OpenSettingsInteraction handler: {ex.Message}");
                    interaction.SetOutput(Unit.Default);
                }
            });
        }

        protected override void OnUnloaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Clean up handler when view is unloaded
            _newProjectHandlerDisposable?.Dispose();
            _openSettingsHandlerDisposable?.Dispose();
            _newProjectHandlerDisposable = null;
            _openSettingsHandlerDisposable = null;

            base.OnUnloaded(e);
        }
    }
}
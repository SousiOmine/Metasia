using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Views;
using Metasia.Editor.Views.Dialogs;

namespace Metasia.Editor.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel => DataContext as MainWindowViewModel;
        private IDisposable? _newProjectHandlerDisposable;
        private IDisposable? _outputHandlerDisposable;
        private OutputWindow? _outputWindow;

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
            _newProjectHandlerDisposable = null;
            _outputHandlerDisposable?.Dispose();
            _outputHandlerDisposable = null;

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

            // OutputInteractionのハンドラーを登録
            _outputHandlerDisposable = viewModel.OutputInteraction.RegisterHandler(interaction =>
            {
                try
                {
                    if (_outputWindow is null)
                    {
                        _outputWindow = new OutputWindow()
                        {
                            DataContext = interaction.Input
                        };
                        _outputWindow.Show(this);
                        _outputWindow.Closed += (s, e) =>
                        {
                            _outputWindow = null;
                        };
                    }
                    else
                    {
                        _outputWindow.Activate();
                    }
                    interaction.SetOutput(null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OutputInteraction handler: {ex.Message}");
                    interaction.SetOutput(null);
                }
            });
        }

        protected override void OnUnloaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Clean up the handler when the view is unloaded
            _newProjectHandlerDisposable?.Dispose();
            _newProjectHandlerDisposable = null;
            _outputHandlerDisposable?.Dispose();
            _outputHandlerDisposable = null;

            base.OnUnloaded(e);
        }
    }
}
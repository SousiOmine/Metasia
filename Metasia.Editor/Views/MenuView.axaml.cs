using System;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Views.Dialogs;
using Metasia.Editor.Views.Settings;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Views
{
    public partial class MenuView : UserControl
    {
        private MenuViewModel? _viewModel => DataContext as MenuViewModel;
        private IDisposable? _newProjectHandlerDisposable;
        private IDisposable? _outputHandlerDisposable;
        private OutputWindow? _outputWindow;
        private IDisposable? _openSettingsHandlerDisposable;
        private IDisposable? _pluginListHandlerDisposable;
        private IDisposable? _openPluginSettingsHandlerDisposable;

        public MenuView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs args)
        {
            _newProjectHandlerDisposable?.Dispose();
            _openSettingsHandlerDisposable?.Dispose();
            _outputHandlerDisposable?.Dispose();
            _pluginListHandlerDisposable?.Dispose();
            _openPluginSettingsHandlerDisposable?.Dispose();
            _newProjectHandlerDisposable = null;
            _outputHandlerDisposable = null;
            _outputWindow?.Close();
            _outputWindow = null;
            _openSettingsHandlerDisposable = null;
            _pluginListHandlerDisposable = null;
            _openPluginSettingsHandlerDisposable = null;

            if (_viewModel is not { } viewModel)
            {
                return;
            }

            _newProjectHandlerDisposable = viewModel.NewProjectInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    var dialog = new NewProjectDialog()
                    {
                        DataContext = interaction.Input
                    };
                    if (VisualRoot is Window window)
                    {
                        var result = await dialog.ShowDialog<(bool, string, Metasia.Core.Project.ProjectInfo, Metasia.Core.Project.MetasiaProject?)>(window);
                        interaction.SetOutput(result);
                    }
                    else
                    {
                        interaction.SetOutput((false, "Window not available", null, null));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in NewProjectInteraction handler: {ex.Message}");
                    interaction.SetOutput((false, ex.Message, null, null));
                }
            });

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
                        if (VisualRoot is Window window)
                        {
                            _outputWindow.Show(window);
                            _outputWindow.Closed += (s, e) =>
                            {
                                _outputWindow = null;
                            };
                        }
                        else
                        {
                            _outputWindow.Close();
                            _outputWindow = null;
                        }
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

            _openSettingsHandlerDisposable = viewModel.OpenSettingsInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    var serviceProvider = App.Current?.Services;
                    if (serviceProvider is not null && VisualRoot is Window window)
                    {
                        var settingsViewModel = serviceProvider.GetRequiredService<SettingsWindowViewModel>();
                        var settingsWindow = new SettingsWindow()
                        {
                            DataContext = settingsViewModel
                        };
                        await settingsWindow.ShowDialog(window);
                        settingsViewModel.NotifySettingsChangedIfNeeded();
                    }
                    interaction.SetOutput(Unit.Default);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OpenSettingsInteraction handler: {ex.Message}");
                    interaction.SetOutput(Unit.Default);
                }
            });

            _pluginListHandlerDisposable = viewModel.PluginListInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    if (VisualRoot is Window window)
                    {
                        var pluginListWindow = new PluginListWindow()
                        {
                            DataContext = interaction.Input
                        };
                        await pluginListWindow.ShowDialog(window);
                    }
                    interaction.SetOutput(Unit.Default);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in PluginListInteraction handler: {ex.Message}");
                    interaction.SetOutput(Unit.Default);
                }
            });

            _openPluginSettingsHandlerDisposable = viewModel.OpenPluginSettingsInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    if (VisualRoot is Window window)
                    {
                        var settingsWindow = interaction.Input.CreateSettingsWindow();
                        await settingsWindow.ShowDialog(window);
                    }
                    interaction.SetOutput(Unit.Default);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OpenPluginSettingsInteraction handler: {ex.Message}");
                    interaction.SetOutput(Unit.Default);
                }
            });
        }

        protected override void OnUnloaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            _newProjectHandlerDisposable?.Dispose();
            _openSettingsHandlerDisposable?.Dispose();
            _outputHandlerDisposable?.Dispose();
            _pluginListHandlerDisposable?.Dispose();
            _openPluginSettingsHandlerDisposable?.Dispose();
            _newProjectHandlerDisposable = null;
            _outputHandlerDisposable = null;
            _outputWindow?.Close();
            _outputWindow = null;
            _openSettingsHandlerDisposable = null;
            _pluginListHandlerDisposable = null;
            _openPluginSettingsHandlerDisposable = null;

            DataContextChanged -= OnDataContextChanged;

            base.OnUnloaded(e);
        }
    }
}
using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views.Dialogs;

public partial class OutputWindow : Window
{
    private OutputViewModel? _viewModel;
    private readonly ContentControl? _pluginSettingsHost;

    public OutputWindow()
    {
        InitializeComponent();
        _pluginSettingsHost = this.FindControl<ContentControl>("PluginSettingsHost");

        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs args)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = this.DataContext as OutputViewModel;

        if (_viewModel is not null)
        {
            _viewModel.CancelAction = () => this.Close();
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        UpdatePluginSettingsHost();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OutputViewModel.SelectedOutputSession))
        {
            UpdatePluginSettingsHost();
        }
    }

    private void UpdatePluginSettingsHost()
    {
        if (_pluginSettingsHost is null)
        {
            return;
        }

        _pluginSettingsHost.Content = _viewModel?.SelectedOutputSession?.SettingsView;
    }
}

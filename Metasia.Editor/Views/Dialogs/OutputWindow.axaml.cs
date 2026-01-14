using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views.Dialogs;

public partial class OutputWindow : Window
{
    private OutputViewModel? _viewModel;
    public OutputWindow()
    {
        InitializeComponent();

        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs args)
    {
        _viewModel = this.DataContext as OutputViewModel;

        if (_viewModel is not null)
        {
            _viewModel.CancelAction = () => this.Close();
        }
    }
}
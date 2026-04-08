using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views.Dialogs;

public partial class ChangeClipLengthDialog : Window
{
    private ChangeClipLengthViewModel? _viewModel;
    private IDisposable? _okCommandSubscription;
    private IDisposable? _cancelCommandSubscription;

    public ChangeClipLengthDialog()
    {
        InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs args)
    {
        _okCommandSubscription?.Dispose();
        _cancelCommandSubscription?.Dispose();
        _okCommandSubscription = null;
        _cancelCommandSubscription = null;

        if (this.DataContext is ChangeClipLengthViewModel vm)
        {
            _viewModel = vm;
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
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views.Dialogs;

public partial class NewObjectSelectWindow : Window
{
    private NewObjectSelectViewModel? _viewModel;
    private IDisposable? _okCommandSubscription;
    private IDisposable? _cancelCommandSubscription;

    public NewObjectSelectWindow()
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

        if (this.DataContext is NewObjectSelectViewModel vm)
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
}
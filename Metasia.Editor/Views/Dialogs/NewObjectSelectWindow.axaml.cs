using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Views.Dialogs;

public partial class NewObjectSelectWindow : Window
{
    private NewObjectSelectViewModel? _viewModel
    {
        get { return this.DataContext as NewObjectSelectViewModel; }
    }
    
    public NewObjectSelectWindow()
    {
        InitializeComponent();
        this.DataContextChanged += (sender, args) =>
        {
            _viewModel!.OkCommand
                .Subscribe(result => Close(result));
            
            _viewModel!.CancelCommand
                .Subscribe(result => Close(result));
        };
    }
}
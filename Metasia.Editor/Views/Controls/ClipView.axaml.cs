using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Metasia.Editor.ViewModels.Controls;

namespace Metasia.Editor.Views.Controls;

public partial class ClipView : UserControl
{
    private ClipViewModel? VM
    {
        get { return this.DataContext as ClipViewModel; }
    
    }
    public ClipView()
    {
        InitializeComponent();
        
        this.DataContextChanged += (s, e) =>
        {
            //ViewModelが置き換えられたときの処理をいつか書く
        };
    }

    private void Clip_OnTapped(object? sender, TappedEventArgs e)
    {
        VM.ClipClick();
    }
}
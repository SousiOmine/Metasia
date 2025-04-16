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

    private void Clip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {

    }

    private void Clip_PointerMoved(object? sender, PointerEventArgs e)
    {

    }

    private void Clip_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {

    }

    private void Handle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        //ViewModelがnullか、Borderからのイベントであれば何もしない
        if (VM is null || sender is not Border handle) return;
        if (handle.Name != "StartHandle" && handle.Name != "EndHandle")
        {
            return;
        }

        var parentCanvas = this.Parent as Control;
        var position = e.GetCurrentPoint(parentCanvas).Position;

        //ViewModelでドラッグ開始処理
        VM.StartDrag(handle.Name, position.X);

        e.Pointer.Capture(handle);

        e.Handled = true;
    }

    private void Handle_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (VM is null || sender is not Border handle) return;
        if (e.Pointer.Captured == handle)
        {
            var parentCanvas = this.Parent as Control;
            if (parentCanvas is null) return;

            var position = e.GetCurrentPoint(parentCanvas).Position;

            VM.UpdateDrag(position.X);
            e.Handled = true;
        }
    }

    private void Handle_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (VM is null || sender is not Border handle) return;
        if (e.Pointer.Captured == handle)
        {
            var parentCanvas = this.Parent as Control;
            if (parentCanvas is null) return;

            var position = e.GetCurrentPoint(parentCanvas).Position;
            VM.EndDrag(position.X);

            e.Handled = true;
        }
        e.Pointer.Capture(null);
    }
}
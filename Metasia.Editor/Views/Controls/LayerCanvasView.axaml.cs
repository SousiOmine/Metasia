using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Metasia.Editor.ViewModels.Controls;

namespace Metasia.Editor.Views.Controls;

public partial class LayerCanvasView : UserControl
{
    private LayerCanvasViewModel? VM => DataContext as LayerCanvasViewModel;
    public LayerCanvasView()
    {
        InitializeComponent();

        this.DataContextChanged += (s, e) =>
        {

        };
    }

    private void LayerCanvasView_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains("MetasiaEditorClipViewModel"))
        {
            e.DragEffects = DragDropEffects.Move;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void LayerCanvasView_DragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains("MetasiaEditorClipViewModel"))
        {
            //視覚フィードバックを表示するならここ
            e.DragEffects = DragDropEffects.Move;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void LayerCanvasView_DragLeave(object? sender, DragEventArgs e)
    {
        //視覚フィードバックをクリアするならここ
        e.Handled = true;
    }

    private void LayerCanvasView_Drop(object? sender, DragEventArgs e)
    {
        if (VM is null) return;

        if (e.Data.Get("MetasiaEditorClipViewModel") is ClipViewModel clipVM)
        {
            var position = e.GetPosition(this);
            int targetFrame = (int)(position.X / VM.Frame_Per_DIP);
            targetFrame = Math.Max(0, targetFrame);
            VM.ClipDropped(clipVM, targetFrame);
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
            Debug.WriteLine("Dropped data is not ClipViewModel");
        }
        e.Handled = true;
    }
}
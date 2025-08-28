using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Controls;

namespace Metasia.Editor.Views.Controls;

public partial class LayerCanvasView : UserControl
{
    private LayerCanvasViewModel? VM => DataContext as LayerCanvasViewModel;
    public LayerCanvasView()
    {
        InitializeComponent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (VM != null)
        {
            var position = e.GetPosition(this);
            var frame = (int)(position.X / VM.Frame_Per_DIP);
            VM.EmptyAreaClicked(frame);
        }
    }
}

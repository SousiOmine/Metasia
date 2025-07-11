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
    /// <summary>
    /// Initializes a new instance of the <see cref="LayerCanvasView"/> user control.
    /// </summary>
    public LayerCanvasView()
    {
        InitializeComponent();
    }
}
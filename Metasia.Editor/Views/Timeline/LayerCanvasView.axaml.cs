using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Metasia.Core.Objects;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Timeline;
using Metasia.Editor.Views.Dialogs;

namespace Metasia.Editor.Views.Timeline;

public partial class LayerCanvasView : UserControl
{
    private LayerCanvasViewModel? _viewModel => DataContext as LayerCanvasViewModel;
    public LayerCanvasView()
    {
        InitializeComponent();

        this.DataContextChanged += (sender, args) =>
        {
            if (_viewModel is not { } viewModel)
            {
                return;
            }

            viewModel.NewObjectSelectInteraction.RegisterHandler(async interaction =>
            {
                if (TopLevel.GetTopLevel(this) is not Window ownerWindow)
                {
                    Debug.WriteLine("LayerCanvasView: Owning window was not found when opening NewObjectSelectWindow.");
                    interaction.SetOutput(null);
                    return;
                }

                var dialog = new NewObjectSelectWindow()
                {
                    DataContext = interaction.Input
                };
                var result = await dialog.ShowDialog<IMetasiaObject?>(ownerWindow);
                interaction.SetOutput(result);
            });
        };
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (_viewModel != null)
        {
            var position = e.GetPosition(this);
            var frame = (int)(position.X / _viewModel.Frame_Per_DIP);
            _viewModel.EmptyAreaClicked(frame);
        }
    }
}

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Inspector;
using Metasia.Editor.Views.Dialogs;

namespace Metasia.Editor.Views.Inspector;

public partial class AudioEffectsView : UserControl
{
    private AudioEffectsViewModel? _viewModel;
    private IDisposable? _newObjectSelectHandlerDisposable;
    public AudioEffectsView()
    {
        InitializeComponent();
        
        this.DataContextChanged += (sender, args) =>
        {
            if (DataContext is AudioEffectsViewModel viewModel)
            {
                _viewModel = viewModel;
                _newObjectSelectHandlerDisposable?.Dispose();
                _newObjectSelectHandlerDisposable = _viewModel.NewObjectSelectInteraction.RegisterHandler(async interaction =>
                {
                    if (TopLevel.GetTopLevel(this) is not Window ownerWindow)
                    {
                        Debug.WriteLine("AudioEffectsView: Owning window was not found when opening NewObjectSelectWindow.");
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
            }
            else
            {
                _newObjectSelectHandlerDisposable?.Dispose();
                _newObjectSelectHandlerDisposable = null;
                _viewModel = null;
            }
        };
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _newObjectSelectHandlerDisposable?.Dispose();
        _newObjectSelectHandlerDisposable = null;
    }
}
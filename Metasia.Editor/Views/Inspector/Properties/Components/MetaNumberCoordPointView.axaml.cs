using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Metasia.Editor.ViewModels.Inspector.Properties.Components;

namespace Metasia.Editor.Views.Inspector.Properties.Components;

public partial class MetaNumberCoordPointView : UserControl
{
    private MetaNumberCoordPointViewModel? VM
    {
        get { return this.DataContext as MetaNumberCoordPointViewModel; }
    }
    public MetaNumberCoordPointView()
    {
        InitializeComponent();
    }
}
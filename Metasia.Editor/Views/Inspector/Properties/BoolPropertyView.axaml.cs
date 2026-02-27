using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Metasia.Editor.Views.Inspector.Properties;

public partial class BoolPropertyView : UserControl
{
    public BoolPropertyView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
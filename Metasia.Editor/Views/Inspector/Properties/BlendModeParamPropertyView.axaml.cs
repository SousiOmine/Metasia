using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Metasia.Editor.Views.Inspector.Properties;

public partial class BlendModeParamPropertyView : UserControl
{
    public BlendModeParamPropertyView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
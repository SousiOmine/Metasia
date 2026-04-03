using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Editor.ViewModels.Inspector.Properties;

namespace Metasia.Editor.Views.Inspector.Properties;

public partial class MetaEnumParamPropertyView : UserControl
{
    public MetaEnumParamPropertyView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

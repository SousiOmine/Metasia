using Avalonia.Controls;

namespace Metasia.Editor.Views.Settings
{
    public partial class GeneralSettingsView : UserControl
    {
        public GeneralSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
    }
}

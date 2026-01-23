using Avalonia.Controls;

namespace Metasia.Editor.Views.Settings
{
    public partial class EditorSettingsView : UserControl
    {
        public EditorSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
    }
}

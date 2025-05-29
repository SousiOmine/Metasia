using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Metasia.Editor.Views
{
    public partial class NewProjectDialog : Window
    {
        public NewProjectDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 
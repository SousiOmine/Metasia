using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;

namespace Metasia.Editor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DataContextが設定された後にキーバインディングを適用
            this.DataContextChanged += OnDataContextChanged;

            // ウィンドウがロードされた時にフォーカスを設定
            this.Loaded += (s, e) =>
            {
                this.Focus();
            };
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            // ViewModelが設定された後にキーバインディングを適用
            var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
            keyBindingService?.ApplyKeyBindings(this);
        }
    }
}
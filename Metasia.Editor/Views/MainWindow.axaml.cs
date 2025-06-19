

using Avalonia.Controls;
using Avalonia.Input;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Metasia.Editor.Views
{
    public partial class MainWindow : Window
    {
        private IKeyBindingService? _keyBindingService;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Get the key binding service
            _keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
            if (_keyBindingService == null)
            {
                Console.WriteLine("KeyBindingService not found");
                return;
            }

            // Get the ViewModel
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                RegisterKeyBindings(viewModel);
            }
        }

        private void RegisterKeyBindings(MainWindowViewModel viewModel)
        {
            // Clear any existing key bindings
            this.KeyBindings.Clear();

            // Register command key bindings
            foreach (var commandPair in viewModel.CommandMap)
            {
                if (commandPair.Key == null || commandPair.Value == null)
                    continue;

                var gesture = _keyBindingService.GetGesture(commandPair.Key);
                if (gesture != null)
                {
                    var keyBinding = new KeyBinding
                    {
                        Gesture = gesture,
                        Command = commandPair.Value
                    };
                    this.KeyBindings.Add(keyBinding);
                }
            }
        }
    }
}


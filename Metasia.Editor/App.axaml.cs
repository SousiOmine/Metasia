

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Metasia.Editor
{
    public partial class App : Application
    {
        public new static App? Current => Application.Current as App;

        /// <summary>
        /// DIコンテナのサービスプロバイダ
        /// </summary>
        public IServiceProvider? Services { get; private set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();
                services.AddSingleton<IFileDialogService>(new FileDialogService(desktop.MainWindow));
                services.AddSingleton<INewProjectDialogService, NewProjectDialogService>();
                services.AddSingleton<IKeyBindingService, KeyBindingService>();
                Services = services.BuildServiceProvider();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(Services),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}


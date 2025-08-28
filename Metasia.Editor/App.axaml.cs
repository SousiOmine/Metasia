

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Services;
using Metasia.Editor.Services.Audio;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Controls;
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
                // まずMainWindowを作成
                var mainWindow = new MainWindow();
                desktop.MainWindow = mainWindow;

                // DIコンテナを設定
                var services = new ServiceCollection();
                services.AddSingleton<IFileDialogService>(new FileDialogService(mainWindow));
                services.AddSingleton<INewProjectDialogService, NewProjectDialogService>();
                services.AddSingleton<IKeyBindingService, KeyBindingService>();

                services.AddSingleton<IEditCommandManager, EditCommandManager>();
                services.AddSingleton<IAudioService, SoundIOService>();
                services.AddTransient<IAudioPlaybackService, AudioPlaybackService>();


                services.AddTransient<IPlayerViewModelFactory, PlayerViewModelFactory>();
                services.AddTransient<ITimelineViewModelFactory, TimelineViewModelFactory>();
                services.AddTransient<ILayerButtonViewModelFactory, LayerButtonViewModelFactory>();
                services.AddTransient<ILayerCanvasViewModelFactory, LayerCanvasViewModelFactory>();
                services.AddTransient<IClipViewModelFactory, ClipViewModelFactory>();
                
                services.AddTransient<MainWindowViewModel>();
                services.AddSingleton<PlayerParentViewModel>();
                services.AddSingleton<TimelineParentViewModel>();
                services.AddSingleton<InspectorViewModel>();
                services.AddSingleton<ToolsViewModel>();
                Services = services.BuildServiceProvider();

                // DIコンテナが設定された後にViewModelを作成
                mainWindow.DataContext = Services.GetRequiredService<MainWindowViewModel>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

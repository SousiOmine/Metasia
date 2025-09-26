

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.Services.Audio;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.ViewModels.Inspector;
using Metasia.Editor.ViewModels.Inspector.Properties;
using Metasia.Editor.ViewModels.Timeline;
using Metasia.Editor.Views;
using Metasia.Editor.Views.Dialogs;
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

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // まずMainWindowを作成
                var mainWindow = new MainWindow();
                desktop.MainWindow = mainWindow;

                // アプリケーション終了時の処理を登録
                desktop.ShutdownRequested += OnShutdownRequested;

                // DIコンテナを設定
                var services = new ServiceCollection();
                services.AddSingleton<IFileDialogService>(new FileDialogService(mainWindow));
                services.AddSingleton<INewProjectDialogService, NewProjectDialogService>();
                services.AddSingleton<IKeyBindingService, KeyBindingService>();
                services.AddSingleton<ISettingsService, SettingsService>();

                services.AddSingleton<IEditCommandManager, EditCommandManager>();
                services.AddSingleton<IAudioService, SoundIOService>();
                services.AddTransient<IAudioPlaybackService, AudioPlaybackService>();

                services.AddSingleton<IProjectState, ProjectState>();
                services.AddSingleton<ISelectionState, SelectionState>();
                services.AddSingleton<IPlaybackState, PlaybackState>();
                services.AddSingleton<ITimelineViewState, TimelineViewState>();

                services.AddSingleton<MediaAccessorRouter>();

                services.AddTransient<IPlayerViewModelFactory, PlayerViewModelFactory>();
                services.AddTransient<ITimelineViewModelFactory, TimelineViewModelFactory>();
                services.AddTransient<ILayerButtonViewModelFactory, LayerButtonViewModelFactory>();
                services.AddTransient<ILayerCanvasViewModelFactory, LayerCanvasViewModelFactory>();
                services.AddTransient<IClipViewModelFactory, ClipViewModelFactory>();
                services.AddTransient<IPropertyRouterViewModelFactory, PropertyRouterViewModelFactory>();
                services.AddTransient<IClipSettingPaneViewModelFactory, ClipSettingPaneViewModelFactory>();
                services.AddTransient<IMetaNumberParamPropertyViewModelFactory, MetaNumberParamPropertyViewModelFactory>();
                services.AddTransient<IMediaPathPropertyViewModelFactory, MediaPathPropertyViewModelFactory>();
                

                services.AddTransient<MainWindowViewModel>();
                services.AddSingleton<PlayerParentViewModel>();
                services.AddSingleton<TimelineParentViewModel>();
                services.AddSingleton<InspectorViewModel>();
                services.AddSingleton<ToolsViewModel>();
                services.AddTransient<SettingsViewModel>();
                Services = services.BuildServiceProvider(new ServiceProviderOptions 
                { 
                    ValidateScopes = true, 
                    ValidateOnBuild = true 
                });

                // DIコンテナが設定された後にViewModelを作成
                mainWindow.DataContext = Services.GetRequiredService<MainWindowViewModel>();
                
                // 起動時に設定を読み込む
                var settingsService = Services.GetRequiredService<ISettingsService>();
                try
                {
                    await settingsService.LoadSettingsAsync();
                }
                catch (Exception ex)
                {
                    // 設定の読み込みに失敗した場合のエラーログ出力
                    // SettingsService内部でも例外処理が行われているが、
                    // 呼び出し元でも失敗を観測できるようにログ出力
                    Console.WriteLine($"アプリケーション起動時の設定読み込みに失敗しました: {ex.Message}");
                    Debug.WriteLine($"アプリケーション起動時の設定読み込みに失敗しました: {ex}");
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            // ServiceProviderを破棄
            if (Services is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

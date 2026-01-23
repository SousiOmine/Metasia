

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.Services.Audio;
using Metasia.Editor.Services.PluginService;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.ViewModels.Inspector;
using Metasia.Editor.ViewModels.Inspector.Properties;
using Metasia.Editor.ViewModels.Settings;
using Metasia.Editor.ViewModels.Timeline;
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

        private MainWindow? _mainWindow;
        private Window? _splashScreen;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _splashScreen = new SplashScreen();
                desktop.MainWindow = _splashScreen;
                _splashScreen.Show();

                // アプリケーション終了時の処理を登録
                desktop.ShutdownRequested += OnShutdownRequested;

                LoadBeforeApplicationStartAsync().ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() => CompleteApplicationStart());
                });

            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task LoadBeforeApplicationStartAsync()
        {
            _mainWindow = new MainWindow();
            var services = new ServiceCollection();
            services.AddSingleton<IFileDialogService>(new FileDialogService(_mainWindow));
            services.AddSingleton<IKeyBindingService, KeyBindingService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<SettingsWindowViewModel>();

            services.AddSingleton<IEditCommandManager, EditCommandManager>();
            services.AddSingleton<IAudioService>(_ =>
            {
                try
                {
                    return new MiniaudioService();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MiniaudioService 初期化に失敗: {ex.Message}");
                    return new SoundIOService();
                }
            });

            services.AddSingleton<IProjectState, ProjectState>();
            services.AddSingleton<ISelectionState, SelectionState>();
            services.AddSingleton<IPlaybackState, PlaybackState>();
            services.AddSingleton<ITimelineViewState, TimelineViewState>();

            services.AddSingleton<MediaAccessorRouter>();
            services.AddSingleton<IPluginService, PluginService>();
            services.AddSingleton<IFontCatalogService, FontCatalogService>();
            services.AddSingleton<IAudioPlaybackService, AudioPlaybackService>();
            services.AddSingleton<IEncodeService, EncodeService>();

            services.AddTransient<IPlayerViewModelFactory, PlayerViewModelFactory>();
            services.AddTransient<ITimelineViewModelFactory, TimelineViewModelFactory>();
            services.AddTransient<ILayerButtonViewModelFactory, LayerButtonViewModelFactory>();
            services.AddTransient<ILayerCanvasViewModelFactory, LayerCanvasViewModelFactory>();
            services.AddTransient<IClipViewModelFactory, ClipViewModelFactory>();
            services.AddTransient<IPropertyRouterViewModelFactory, PropertyRouterViewModelFactory>();
            services.AddTransient<IClipSettingPaneViewModelFactory, ClipSettingPaneViewModelFactory>();
            services.AddTransient<IMetaNumberParamPropertyViewModelFactory, MetaNumberParamPropertyViewModelFactory>();
            services.AddTransient<IMediaPathPropertyViewModelFactory, MediaPathPropertyViewModelFactory>();
            services.AddTransient<IStringPropertyViewModelFactory, StringPropertyViewModelFactory>();
            services.AddTransient<IDoublePropertyViewModelFactory, DoublePropertyViewModelFactory>();
            services.AddTransient<IMetaEnumParamPropertyViewModelFactory, MetaEnumParamPropertyViewModelFactory>();
            services.AddTransient<IMetaFontParamPropertyViewModelFactory, MetaFontParamPropertyViewModelFactory>();
            services.AddTransient<IColorPropertyViewModelFactory, ColorPropertyViewModelFactory>();
            services.AddTransient<INewProjectViewModelFactory, NewProjectViewModelFactory>();
            services.AddTransient<IOutputViewModelFactory, OutputViewModelFactory>();


            services.AddTransient<MainWindowViewModel>();
            services.AddSingleton<PlayerParentViewModel>();
            services.AddSingleton<TimelineParentViewModel>();
            services.AddSingleton<InspectorViewModel>();
            services.AddSingleton<ToolsViewModel>();
            Services = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

            try
            {
                await Services.GetRequiredService<ISettingsService>().LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"設定の読み込みに失敗しました。デフォルト設定を使用します: {ex.Message}");
                // 必要に応じてデフォルト設定で続行するか、ユーザーに通知
            }
            // プラグインを読み込み
            await Services.GetRequiredService<IPluginService>().LoadPluginsAsync();

            await Task.Delay(1000);

        }

        private void CompleteApplicationStart()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;


            _mainWindow!.DataContext = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = _mainWindow;

            _mainWindow!.Show();

            _splashScreen!.Close();
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

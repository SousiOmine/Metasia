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
				// 依存性注入の設定
				var services = new ServiceCollection();
				ConfigureServices(services);
				Services = services.BuildServiceProvider();

				// MainWindowを作成し、ViewModelを依存性注入で取得
				var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();
				desktop.MainWindow = new MainWindow
				{
					DataContext = mainWindowViewModel,
				};

				// FileDialogServiceにMainWindowを設定
				var fileDialogService = Services.GetRequiredService<FileDialogService>();
				fileDialogService.SetMainWindow(desktop.MainWindow);
			}

			base.OnFrameworkInitializationCompleted();
		}

		private void ConfigureServices(IServiceCollection services)
		{
			// サービスの登録
			services.AddSingleton<IFileDialogService, FileDialogService>();
			services.AddSingleton<IDialogService, DialogService>();
			
			// ViewModelの登録
			services.AddTransient<MainWindowViewModel>();
			services.AddTransient<NewProjectDialogViewModel>();
		}
	}
}
using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Views.Dialogs;

namespace Metasia.Editor.Views
{
	public partial class MainWindow : Window
	{
		private MainWindowViewModel? _viewModel;
		private IDisposable? _settingsInteractionHandlerDisposable;

		public MainWindow()
		{
			InitializeComponent();
			
			// DataContextが設定された後にキーバインディングを適用
			this.DataContextChanged += OnDataContextChanged;
			
			// ウィンドウがロードされた時にフォーカスを設定
			this.Loaded += (s, e) => {
				this.Focus();
			};
		}

		private void OnDataContextChanged(object? sender, EventArgs args)
		{
			// Dispose previous handler if it exists
			_settingsInteractionHandlerDisposable?.Dispose();
			_settingsInteractionHandlerDisposable = null;

			if (this.DataContext is MainWindowViewModel vm)
			{
				_viewModel = vm;

				// Register the interaction handler
				_settingsInteractionHandlerDisposable = vm.SettingsInteraction
					.RegisterHandler(async interaction =>
					{
						if (TopLevel.GetTopLevel(this) is not Window ownerWindow)
						{
							Console.WriteLine("MainWindow: Owning window was not found when opening SettingsWindow.");
							interaction.SetOutput(false);
							return;
						}

						var settingsWindow = new SettingsWindow();
						
						// DIコンテナからViewModelを取得して設定
						if (App.Current is App app && app.Services != null)
						{
							var viewModel = app.Services.GetService(typeof(SettingsViewModel)) as SettingsViewModel;
							if (viewModel != null)
							{
								settingsWindow.DataContext = viewModel;
							}
						}

						var result = await settingsWindow.ShowDialog<bool>(ownerWindow);
						interaction.SetOutput(result);
					});
			}
			else
			{
				_viewModel = null;
			}

			// ViewModelが設定された後にキーバインディングを適用
			var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
			keyBindingService?.ApplyKeyBindings(this);
		}

		protected override void OnClosed(EventArgs e)
		{
			// Clean up the handler when the window is closed
			_settingsInteractionHandlerDisposable?.Dispose();
			_settingsInteractionHandlerDisposable = null;

			base.OnClosed(e);
		}
	}
}

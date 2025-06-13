using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Metasia.Editor.Models.KeyBindings;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using System;
using System.Collections.Generic;

namespace Metasia.Editor.Views
{
	public partial class MainWindow : Window
	{
		private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;
		private IKeyBindingService _keyBindingService;

		public MainWindow()
		{
			InitializeComponent();
			
			// ウィンドウがロードされたときにキーバインディングを登録
			this.Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (ViewModel == null) return;
			
			// キーバインディングサービスを取得
			_keyBindingService = App.Current?.Services?.GetService(typeof(IKeyBindingService)) as IKeyBindingService;
			if (_keyBindingService == null) return;
			
			// キーバインディングを登録
			RegisterKeyBindings();
		}

		/// <summary>
		/// キーバインディングを登録する
		/// </summary>
		private void RegisterKeyBindings()
		{
			if (ViewModel?.CommandMap == null || _keyBindingService == null) return;

			// 既存のキーバインディングをクリア
			this.KeyBindings.Clear();

			// 各コマンドに対応するキーバインディングを登録
			foreach (var commandEntry in ViewModel.CommandMap)
			{
				var commandId = commandEntry.Key;
				var command = commandEntry.Value;
				
				// キーバインディングサービスからキージェスチャーを取得
				var gesture = _keyBindingService.GetGesture(commandId);
				
				// キーバインディングを作成して登録
				var keyBinding = new KeyBinding
				{
					Command = command,
					Gesture = gesture
				};
				
				this.KeyBindings.Add(keyBinding);
			}
		}
	}
}
using ReactiveUI;
using System;
using System.Collections.Generic;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels
{
	public class ViewModelBase : ReactiveObject, IDisposable
	{
		private readonly List<string> _registeredCommandIds = new List<string>();
		private IKeyBindingService? _keyBindingService;
		private bool _disposed;

		/// <summary>
		/// KeyBindingServiceを設定
		/// </summary>
		protected void SetKeyBindingService(IKeyBindingService keyBindingService)
		{
			_keyBindingService = keyBindingService;
		}

		/// <summary>
		/// コマンドを登録し、登録したコマンドIDを記録
		/// </summary>
		protected void RegisterCommand(string commandId, System.Windows.Input.ICommand command)
		{
			if (_keyBindingService is null)
			{
				throw new InvalidOperationException("KeyBindingService is not set. Call SetKeyBindingService first.");
			}

			_keyBindingService.RegisterCommand(commandId, command);
			_registeredCommandIds.Add(commandId);
		}

		/// <summary>
		/// リソースの解放
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// リソースの解放処理
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// 登録したすべてのコマンドを解除
					if (_keyBindingService is not null)
					{
						foreach (var commandId in _registeredCommandIds)
						{
							_keyBindingService.UnregisterCommand(commandId);
						}
						_registeredCommandIds.Clear();
					}
				}
				_disposed = true;
			}
		}
	}
}

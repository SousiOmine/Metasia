using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Metasia.Editor.Services
{
	public class FileDialogService : IFileDialogService
	{
        //Avalonia公式サンプルの https://github.com/AvaloniaUI/AvaloniaUI.QuickGuides/tree/main/IoCFileOps を参考にした
        private readonly Window _target;

        public FileDialogService(Window target)
        {
            _target = target;
        }

		public async Task<IStorageFile?> OpenFileDialogAsync()
		{
			var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
			{
                Title = "ファイルを開く",
				AllowMultiple = false,
			});

            // filesの要素数が1以上の場合は最初の要素を返し、
            // そうでない場合(空の場合)はnullを返す
            return files.Count >= 1 ? files[0] : null;
		}

        public async Task<IStorageFile?> SaveFileDialogAsync()
        {
            return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "ファイルを保存",
            });
        }
	}
}
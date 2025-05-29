using System;
using System.Reactive.Joins;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Metasia.Editor.Services
{
	public class FileDialogService : IFileDialogService
	{
        //Avalonia公式サンプルの https://github.com/AvaloniaUI/AvaloniaUI.QuickGuides/tree/main/IoCFileOps を参考にした
        private Window _target;

        public FileDialogService()
        {
            _target = null!; // 後でSetMainWindowで設定される
        }

        public FileDialogService(Window target)
        {
            _target = target;
        }

        public void SetMainWindow(Window window)
        {
            _target = window;
        }

		public async Task<IStorageFile?> OpenFileDialogAsync()
		{
			var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "ファイルを開く",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[]
                {
                    new("Metasia Project File")
                    {
                        Patterns = new string[] { "*.mtpj" }
                    }
                }
			});

            // filesの要素数が1以上の場合は最初の要素を返し、
            // そうでない場合(空の場合)はnullを返す
            return files.Count >= 1 ? files[0] : null;
		}

        public async Task<IStorageFolder?> OpenFolderDialogAsync()
        {
            var folders = await _target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "フォルダを開く",
                AllowMultiple = false,
            });

            return folders.Count >= 1 ? folders[0] : null;
        }

        public async Task<IStorageFile?> SaveFileDialogAsync()
        {
            return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "ファイルを保存",
                FileTypeChoices = new FilePickerFileType[]
                {
                    new("Metasia Project File")
                    {
                        Patterns = new string[] { "*.mtpj" }
                    }
                }
            });
        }
	}
}
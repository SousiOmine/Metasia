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

        public async Task<IStorageFile?> OpenFileDialogAsync(string title, string[] patterns)
        {
            ArgumentNullException.ThrowIfNull(title);
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty or whitespace.", nameof(title));

            ArgumentNullException.ThrowIfNull(patterns);
            if (patterns.Length == 0)
                throw new ArgumentException("Patterns array must contain at least one pattern.", nameof(patterns));
            foreach (var p in patterns)
            {
                ArgumentNullException.ThrowIfNull(p);
                if (string.IsNullOrWhiteSpace(p))
                    throw new ArgumentException("Pattern cannot be empty or whitespace.", nameof(patterns));
            }

            var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[]
                {
                    new("File")
                    {
                        Patterns = patterns
                    }
                }
            });

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
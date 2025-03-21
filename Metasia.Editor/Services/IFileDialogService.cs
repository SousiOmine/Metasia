using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Metasia.Editor.Services
{
	public interface IFileDialogService
	{
		public Task<IStorageFile?> OpenFileDialogAsync();
		public Task<IStorageFile?> SaveFileDialogAsync();
		
		public Task<IStorageFolder?> OpenFolderDialogAsync();
	}
}

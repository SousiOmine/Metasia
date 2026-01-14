using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Metasia.Editor.Services
{
    public interface IFileDialogService
    {
        public Task<IStorageFile?> OpenFileDialogAsync();
        public Task<IStorageFile?> OpenFileDialogAsync(string title, string[] patterns);
        public Task<IStorageFile?> SaveFileDialogAsync(string title, string[] extensions, string defaultExtension = "");
        public Task<IStorageFolder?> OpenFolderDialogAsync();
    }
}

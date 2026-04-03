using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
namespace Metasia.Editor.Models.FileSystem
{
    /// <summary>
    /// ファイルの位置と種類を格納するインターフェース
    /// </summary>
    public interface IFileEntity : IResourceEntity
    {
        public FileTypes FileType { get; }
    }
}
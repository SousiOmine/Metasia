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
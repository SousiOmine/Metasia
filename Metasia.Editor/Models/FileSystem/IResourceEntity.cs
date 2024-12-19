namespace Metasia.Editor.Models.FileSystem
{
    /// <summary>
    /// ファイルやディレクトリの位置を格納するインターフェース
    /// </summary>
    public interface IResourceEntity
    {
        public string Path { get; }
        
        public string? Name { get; }
    }
}


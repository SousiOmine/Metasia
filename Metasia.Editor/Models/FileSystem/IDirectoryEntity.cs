using System.Collections.Generic;

namespace Metasia.Editor.Models.FileSystem
{
    /// <summary>
    /// ディレクトリの位置と、配下のResourceEntityを格納するインターフェース
    /// </summary>
    public interface IDirectoryEntity : IResourceEntity
    {
        public IEnumerable<IResourceEntity> GetSubordinates();
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Metasia.Editor.Models.FileSystem
{
    public class DirectoryEntity : IDirectoryEntity
    {
        public string Path { get; }
        public string? Name { get; }

        public DirectoryEntity(string path)
        {
            if (Directory.Exists(path))
            {
                Path = System.IO.Path.GetFullPath(path);
                Name = new DirectoryInfo(path).Name;
            }
            else
            {
                throw new DirectoryNotFoundException(path + "が見つかりません DirectoryEntity");
            }
        }
        
        public IEnumerable<IResourceEntity> GetSubordinates()
        {
            string[] files = Directory.GetFiles(Path, "*");
            var filesCollection = new Collection<IResourceEntity>();
            foreach (string file in files)
            {
                filesCollection.Add(new FileEntity(file));
            }
            
            return filesCollection;
        }
    }
}


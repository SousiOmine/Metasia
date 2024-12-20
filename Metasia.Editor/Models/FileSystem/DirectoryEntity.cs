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
            string[] directories = Directory.GetDirectories(Path, "*");
            string[] files = Directory.GetFiles(Path, "*", SearchOption.TopDirectoryOnly);

            var filesCollection = new Collection<IResourceEntity>();

            foreach (string directory in directories)
            {
                filesCollection.Add(new DirectoryEntity(directory));
            }
            foreach (string file in files)
            {
                filesCollection.Add(new FileEntity(file));
            }
            
            return filesCollection;
        }
    }
}


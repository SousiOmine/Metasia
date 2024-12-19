using System.IO;
using System.Net;

namespace Metasia.Editor.Models.FileSystem
{
    public class FileEntity : IFileEntity
    {
        public string Path { get; private set; }
        public string? Name { get; private set; }
        public FileTypes FileType { get; private set; }

        public FileEntity(string path)
        {
            //ファイルが存在すれば
            if (File.Exists(path))
            {
                Path = System.IO.Path.GetFullPath(path);
                Name = System.IO.Path.GetFileName(path);

                string fileExtention = System.IO.Path.GetExtension(path);
                switch (fileExtention)
                {
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        FileType = FileTypes.Image;
                        break;
                    
                    case ".mp3":
                    case ".wav":
                        FileType = FileTypes.Audio;
                        break;
                    
                    case ".avi":
                    case ".mp4":
                        FileType = FileTypes.Video;
                        break;
                    
                    case ".txt":
                        FileType = FileTypes.Text;
                        break;
                    
                    case ".mtl":
                        FileType = FileTypes.MetasiaTimeline;
                        break;
                    
                    case ".mtpj":
                        FileType = FileTypes.MetasiaProjectConfig;
                        break;
                    
                    
                    default:
                        FileType = FileTypes.Other;
                        break;
                }
            }
            else
            {
                throw new FileNotFoundException(path + "が見つかりません FileEtity");
            }
        }
    }
}


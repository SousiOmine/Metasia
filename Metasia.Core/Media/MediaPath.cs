using System.IO;
using System.Xml.Serialization;

namespace Metasia.Core.Media
{
    public class MediaPath
    {
        [XmlElement("FileName")]
        public string FileName { get; set; } = string.Empty;
        [XmlElement("Directory")]
        public string Directory { get; set; } = string.Empty;

        [XmlArray("Types")]
        [XmlArrayItem("Type")]
        public MediaType[] Types { get; set; } = Array.Empty<MediaType>();

        public MediaPath()
        {
        }

        public MediaPath(MediaType[] types)
        {
            ArgumentNullException.ThrowIfNull(types);
            Types = types;
        }

        /// <summary>
        /// MediaPathを作成する
        /// </summary>
        /// <param name="directory">ディレクトリパス</param>
        /// <param name="fileName">ファイル名</param>
        /// <returns>MediaPath</returns>
        public static MediaPath CreateFromPath(string directory, string fileName)
        {
            // Validate inputs
            ArgumentNullException.ThrowIfNull(directory);
            ArgumentNullException.ThrowIfNull(fileName);

            if (directory.Length == 0) throw new ArgumentException("directory cannot be empty", nameof(directory));
            if (fileName.Length == 0) throw new ArgumentException("fileName cannot be empty", nameof(fileName));

            // fileName must not contain directory separator characters
            if (fileName.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0)
                throw new ArgumentException("fileName must not contain directory separator characters", nameof(fileName));

            // Ensure directory does not end with fileName (segment‑safe check)
            if (string.Equals(Path.GetFileName(directory), fileName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("directory must not contain fileName as its last segment", nameof(directory));

            string pathToSave = directory.Replace(Path.DirectorySeparatorChar, '/');

            return new MediaPath
            {
                FileName = fileName,
                Directory = pathToSave
            };
        }

        /// <summary>
        /// MediaPathから絶対パスを取得する
        /// </summary>
        /// <param name="mediaPath">MediaPath</param>
        /// <param name="projectDir">プロジェクトファイルのあるディレクトリ</param>
        /// <returns>絶対パス</returns>
        public static string GetFullPath(MediaPath mediaPath, string? projectDir)
        {
            string separatorApplied = mediaPath.Directory.Replace('/', Path.DirectorySeparatorChar);

            // パスが相対パスか絶対パスかを判定
            if (Path.IsPathRooted(separatorApplied))
            {
                // 絶対パスの場合
                return Path.GetFullPath(Path.Combine(separatorApplied, mediaPath.FileName));
            }
            else
            {
                // 相対パスの場合、projectDirがnullの場合は例外をスロー
                ArgumentNullException.ThrowIfNull(projectDir);

                return Path.GetFullPath(Path.Combine(projectDir, separatorApplied, mediaPath.FileName));
            }
        }
    }
}
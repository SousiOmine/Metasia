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

        [XmlElement("PathType")]
        public PathType PathType { get; set; } = PathType.Absolute;

        /// <summary>
        /// 絶対パスからMediaPathを作成する
        /// </summary>
        /// <param name="directory">ディレクトリ 絶対パス</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="projectDir">プロジェクトファイルのあるディレクトリ</param>
        /// <param name="pathType">パスをどのような形で扱うか(絶対パス、プロジェクト基準の相対パスなど)</param>
        /// <returns>MediaPath</returns>
        public static MediaPath CreateFromPath(string directory, string fileName, string? projectDir, PathType pathType)
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

            // Resolve directory based on path type
            if (pathType == PathType.ProjectRelative)
            {
                try
                {
                    directory = Path.GetRelativePath(projectDir ?? "", directory);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Failed to get relative path", ex);
                }
            }
            else if (pathType == PathType.Absolute)
            {
                try
                {
                    directory = Path.GetFullPath(directory);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Failed to get full path", ex);
                }
            }
            else
            {
                throw new ArgumentException("pathType must be PathType.ProjectRelative or PathType.Absolute", nameof(pathType));
            }

            string pathToSave = directory.Replace(Path.DirectorySeparatorChar, '/');

            return new MediaPath
            {
                FileName = fileName,
                Directory = pathToSave,
                PathType = pathType
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
            if (mediaPath.PathType == PathType.Absolute)
            {
                return Path.Combine(separatorApplied, mediaPath.FileName);
            }
            else if (mediaPath.PathType == PathType.ProjectRelative)
            {
                return Path.Combine(projectDir ?? "", separatorApplied, mediaPath.FileName);
            }
            else
            {
                throw new ArgumentException("pathType must be PathType.ProjectRelative or PathType.Absolute");
            }
        }
    }
}
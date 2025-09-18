using System.IO;

namespace Metasia.Core.Media
{
    public class MediaPath
    {
        public string FileName { get; init; } = string.Empty;
        public string Directory { get; init; } = string.Empty;

        public PathType PathType { get; init; } = PathType.Absolute;

        /// <summary>
        /// 絶対パスからMediaPathを作成する
        /// </summary>
        /// <param name="directory">ディレクトリ 絶対パス</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="projectDir">プロジェクトファイルのあるディレクトリ</param>
        /// <param name="pathType">パスをどのような形で扱うか(絶対パス、プロジェクト基準の相対パスなど)</param>
        /// <returns>MediaPath</returns>
        public static MediaPath CreateFromPath(string directory, string fileName, string projectDir, PathType pathType)
        {
            if(directory.Contains(fileName)) throw new ArgumentException("directory must not contain fileName");

            if(pathType == PathType.ProjectRelative)
            {
                directory = Path.GetRelativePath(projectDir, directory);
            }
            else if(pathType == PathType.Absolute)
            {
                directory = Path.GetFullPath(directory);
            }
            else
            {
                throw new ArgumentException("pathType must be PathType.ProjectRelative or PathType.Absolute");
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
        public static string GetFullPath(MediaPath mediaPath, string projectDir)
        {
            string separatorApplied = mediaPath.Directory.Replace('/', Path.DirectorySeparatorChar);
            if(mediaPath.PathType == PathType.Absolute)
            {
                return Path.Combine(separatorApplied, mediaPath.FileName);
            }
            else if(mediaPath.PathType == PathType.ProjectRelative)
            {
                return Path.Combine(projectDir, separatorApplied, mediaPath.FileName);
            }
            else
            {
                throw new ArgumentException("pathType must be PathType.ProjectRelative or PathType.Absolute");
            }
        }
    }
}
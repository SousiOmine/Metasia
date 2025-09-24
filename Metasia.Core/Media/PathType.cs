namespace Metasia.Core.Media
{
    public enum PathType
    {
        /// <summary>
        /// 絶対パス
        /// </summary>
        Absolute,
        /// <summary>
        /// プロジェクトファイル(metasia.jsonのあるディレクトリ)から見た相対パス
        /// </summary>
        ProjectRelative,
    }
}
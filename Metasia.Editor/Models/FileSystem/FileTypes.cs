namespace Metasia.Editor.Models.FileSystem
{
    /// <summary>
    /// ファイルの大まかな属性（動画、音楽、プロジェクトファイルなど）を表現する
    /// </summary>
    public enum FileTypes
    {
        Video,
        Audio,
        Image,
        Text,

        MetasiaTimeline,
        MetasiaProjectConfig,

        Other,
    }
}
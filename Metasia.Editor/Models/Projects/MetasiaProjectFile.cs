using SkiaSharp;

namespace Metasia.Editor.Models.Projects;

/// <summary>
/// metasia.jsonの中身
/// </summary>
public class MetasiaProjectFile
{
    /// <summary>
    /// タイムラインを探索するフォルダの相対パス（Nullなら.mtpjが存在するフォルダのみ）
    /// </summary>
    public string[] TimelineFolders { get; set; } = ["./Timelines"];

    /// <summary>
    /// レンダリング対象となるタイムラインのID
    /// </summary>
    public string RootTimelineId { get; set; } = "RootTimeline";

    /// <summary>
    /// フレームレート
    /// </summary>
    public int Framerate { get; set; } = 60;

    /// <summary>
    /// 動画の解像度
    /// </summary>
    public VideoResolution Resolution { get; set; } = new VideoResolution { Width = 1920, Height = 1080 };

    /// <summary>
    /// 音声のサンプリングレート（Hz）
    /// </summary>
    public int AudioSamplingRate { get; set; } = 44100;
}

public class VideoResolution
{
    /// <summary>
    /// 幅
    /// </summary>
    public float Width { get; set; }
    
    /// <summary>
    /// 高さ
    /// </summary>
    public float Height { get; set; }
}

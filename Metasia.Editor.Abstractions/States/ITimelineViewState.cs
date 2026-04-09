namespace Metasia.Editor.Abstractions.States;

public interface ITimelineViewState : IDisposable
{
    /// <summary>
    /// タイムライン描画における、1フレームあたりのディスプレイ表示幅
    /// </summary>
    double Frame_Per_DIP { get; set; }

    /// <summary>
    /// タイムライン描画における、1フレームあたりのディスプレイ表示幅が変更された時に発生するイベント
    /// </summary>
    event Action? Frame_Per_DIP_Changed;

    /// <summary>
    /// タイムラインの水平スクロール位置（フレーム単位）
    /// </summary>
    int HorizontalScrollPosition { get; set; }

    /// <summary>
    /// 水平スクロール位置が変更された時に発生するイベント
    /// </summary>
    event Action? HorizontalScrollPosition_Changed;

    /// <summary>
    /// 現在の再生フレーム位置
    /// </summary>
    int CurrentFrame { get; set; }

    /// <summary>
    /// 現在の再生フレーム位置が変更された時に発生するイベント
    /// </summary>
    event Action? CurrentFrame_Changed;
}

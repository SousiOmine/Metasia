using System;

namespace Metasia.Editor.Models.States;

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
}
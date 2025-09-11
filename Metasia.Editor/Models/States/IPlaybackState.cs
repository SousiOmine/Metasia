using System;


namespace Metasia.Editor.Models.States;

public interface IPlaybackState : IDisposable
{
    /// <summary>
    /// 現在のフレーム
    /// </summary>
    int CurrentFrame { get; }

    /// <summary>
    /// 再生中であるか否か
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// プレビュー再生時のサンプリングレート（Hz）
    /// </summary>
    int SamplingRate { get; }

    /// <summary>
    /// プレビュー再生時の音声チャンネル数
    /// </summary>
    int AudioChannels { get; }

    /// <summary>
    /// 再生を開始する
    /// </summary>
    
    void Play();

    /// <summary>
    /// 再生を停止する
    /// </summary>
    void Pause();

    /// <summary>
    /// 指定したフレームにシークする
    /// </summary>
    /// <param name="frame"></param>
    void Seek(int frame);

    /// <summary>
    /// 再生が開始された時に発生するイベント
    /// </summary>
    event Action? PlaybackStarted;

    /// <summary>
    /// 再生が停止された時に発生するイベント
    /// </summary>
    event Action? PlaybackPaused;

    /// <summary>
    /// シークされた時に発生するイベント
    /// </summary>
    event Action? PlaybackSeeked;

    /// <summary>
    /// 再生フレームが変更された時に発生するイベント
    /// </summary>
    event Action? PlaybackFrameChanged;

    /// <summary>
    /// 再描写が要求された時に発生するイベント
    /// </summary>
    event Action? ReRenderingRequested;

    /// <summary>
    /// 再描写を要求する
    /// </summary>
    void RequestReRendering();
}

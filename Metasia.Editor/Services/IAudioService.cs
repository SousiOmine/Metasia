using System;
using Metasia.Core.Sounds;

namespace Metasia.Editor.Services;

/// <summary>
/// 音声をデバイスのスピーカーから出力するやつ
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// 波形を格納した配列とチャンネル数を再生キューにぶち込む
    /// </summary>
    /// <param name="pulse"></param>
    /// <param name="channel"></param>
    public void InsertQueue(IAudioChunk chunk);

    /// <summary>
    /// 再生キューをリセットする
    /// </summary>
    public void ClearQueue();

    /// <summary>
    /// 再生キューに格納されている(1チャンネルあたりの)サンプル数を返す
    /// </summary>
    /// <returns></returns>
    public long GetQueuedSamplesCount();
}
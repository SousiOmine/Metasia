using System.Security.Cryptography.X509Certificates;
using Metasia.Core.Sounds;

namespace Metasia.Core.Render;

public class AudioExpresserArgs : IDisposable
{
    /// <summary>
    /// 出力結果の音声が格納される変数
    /// </summary>
    public MetasiaSound? Sound;

    /// <summary>
    /// 音声のチャンネル数
    /// </summary>
    public byte AudioChannel;
    
    /// <summary>
    /// サンプリングレート
    /// </summary>
    public uint SoundSampleRate;

    /// <summary>
    /// この値を1秒で割った時間分の音声を要求する
    /// </summary>
    public int FPS;
    
    public void Dispose()
    {
        if(Sound is not null) Sound.Dispose();
    }
}
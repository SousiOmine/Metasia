namespace Metasia.Core.Sounds;

public class MetasiaSound : IDisposable
{
    public double[] Pulse;
    private byte _channel;
    private uint _sampleRate;
    private ushort _fps;

    /// <summary>
    /// 音声のチャンネル数
    /// </summary>
    public byte Channel
    {
        get => _channel;
    }
    
    /// <summary>
    /// 音声のサンプリングレート
    /// </summary>
    public uint SampleRate
    {
        get => _sampleRate;
        set
        {
            _sampleRate = value;
            Pulse = new double[_channel * (_sampleRate / FPS)];
        }
    }
    
    /// <summary>
    /// 一秒間に何フレームで構成されるか
    /// </summary>
    public ushort FPS
    {
        get => _fps;
        set 
        {
            _fps = value;
            Pulse = new double[_channel * (_sampleRate / FPS)];
        }
    }

    public MetasiaSound(byte Channel, uint SampleRate, ushort FPS)
    {
        Pulse = new double[Channel * (SampleRate / FPS)];
        _channel = Channel;
        _sampleRate = SampleRate;
        _fps = FPS;
    }
    
    public void Dispose()
    {
        Pulse = null;
    }

    /// <summary>
    /// 波形を合成する
    /// </summary>
    /// <param name="channel">合成するMetasiaSoundのチャンネル数</param>
    /// <param name="sounds">合成したいMetasiaSound</param>
    /// <returns>合成後のMetasiaSound チャンネル数はchannelと同じになる</returns>
    public static MetasiaSound SynthesisPulse(byte channel, params MetasiaSound[] sounds)
    {
        MetasiaSound result = new MetasiaSound(channel, sounds[0].SampleRate, sounds[0].FPS);
        for(int i = 0; i < result.Pulse.Length; i++)
        {
            for(int j = 0; j < sounds.Length; j++)
            {
                result.Pulse[i] += sounds[j].Pulse[i];
            }
        }
        return result;
    }
    
    /// <summary>
    /// 音量を変える
    /// </summary>
    /// <param name="sound">元のMetasiaSound</param>
    /// <param name="volume">音量 1.0で100%、0.0で0%</param>
    /// <returns>音量変更後のMetasiaSound</returns>
    public static MetasiaSound VolumeChange(MetasiaSound sound, double volume)
    {
        for(int i = 0; i < sound.Pulse.Length; i++)
        {
            sound.Pulse[i] *= volume;
            Console.WriteLine(volume);
        }
        return sound;
    }
}

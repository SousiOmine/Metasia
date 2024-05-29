namespace Metasia.Core.Sounds;

public class MetasiaSound : IDisposable
{
    public double[] Pulse;
    private byte _channel;
    private uint _sampleRate;
    private ushort _fps;

    public byte Channel
    {
        get => _channel;
    }
    
    public uint SampleRate
    {
        get => _sampleRate;
        set
        {
            _sampleRate = value;
            Pulse = new double[_channel * (_sampleRate / FPS)];
        }
    }
    
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
    
    public static MetasiaSound VolumeChange(MetasiaSound sound, float volume)
    {
        for(int i = 0; i < sound.Pulse.Length; i++)
        {
            sound.Pulse[i] *= volume;
        }
        return null;
    }
}

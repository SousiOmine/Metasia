namespace Metasia.Core.Sounds;

public class MetasiaSound : IDisposable
{
    public short[] Pulse;
    public static MetasiaSound CreateMetasiaSound()
    {
        return CreateMetasiaSound(new MetasiaSoundOption());
    }

    public static MetasiaSound CreateMetasiaSound(MetasiaSoundOption option)
    {
        MetasiaSound metasiaSound = new();
        metasiaSound.Pulse = new short[option.SampleRate / option.Fps * option.Channel];
        return metasiaSound;
    }


    public void Dispose()
    {
        Pulse = null;
    }
}
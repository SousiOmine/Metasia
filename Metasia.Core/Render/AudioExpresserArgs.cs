using System.Security.Cryptography.X509Certificates;
using Metasia.Core.Sounds;

namespace Metasia.Core.Render;

public class AudioExpresserArgs : IDisposable
{
    public MetasiaSound? Sound;

    public byte AudioChannel;
    
    public uint SoundSampleRate;

    public int FPS;
    
    public void Dispose()
    {
        if(Sound is not null) Sound.Dispose();
    }
}
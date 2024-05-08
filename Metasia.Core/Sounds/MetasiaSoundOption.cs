namespace Metasia.Core.Sounds;

public struct MetasiaSoundOption
{
    public MetasiaSoundOption()
    {
    }

    public byte Channel { get; set; } = 2;
    
    public uint SampleRate { get; set; } = 44100;
    
    public ushort Fps { get; set; } = 60;
}
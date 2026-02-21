using Metasia.Core.Sounds;

namespace Metasia.Core.Media;

public class AudioSampleResult
{
    public bool IsSuccessful { get; set; } = false;
    public IAudioChunk? Chunk { get; set; } = null;
    public long ActualStartSample { get; set; } = 0;
    public int ActualSampleCount { get; set; } = 0;
}
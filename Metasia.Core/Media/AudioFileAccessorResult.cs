using Metasia.Core.Sounds;

namespace Metasia.Core.Media;

public class AudioFileAccessorResult
{
    public bool IsSuccessful { get; set; } = false;
    public IAudioChunk? Chunk { get; set; } = null;
}

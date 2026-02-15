using Metasia.Core.Sounds;

namespace Metasia.Core.Media;

public class EmptyAudioFileAccessor : IAudioFileAccessor
{
    public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
    {
        return Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });
    }
}

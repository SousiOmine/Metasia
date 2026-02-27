using Metasia.Core.Sounds;

namespace Metasia.Core.Media;

public class EmptyAudioFileAccessor : IAudioFileAccessor
{
    public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
    {
        return Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });
    }

    public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate)
    {
        return Task.FromResult(new AudioSampleResult { IsSuccessful = false, Chunk = null });
    }
}

namespace Metasia.Core.Media;

public interface IAudioFileAccessor : IMediaAccessor
{
    public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null);
    
    public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate);
}

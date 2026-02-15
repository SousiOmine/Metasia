namespace Metasia.Core.Media;

public interface IAudioFileAccessor : IMediaAccessor
{
    public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null);
}

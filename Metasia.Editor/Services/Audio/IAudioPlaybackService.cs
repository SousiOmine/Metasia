using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.Services.Audio
{
    public interface IAudioPlaybackService
    {
        bool IsPlaying { get; }
        long CurrentSample { get; }
        void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels);
        void Pause();
    }
}
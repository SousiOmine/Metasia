using Metasia.Core.Objects;
using Metasia.Core.Project;

namespace Metasia.Editor.Services.Audio
{
	public interface IAudioPlaybackService
	{
        bool IsPlaying { get; }
        long CurrentSample { get; }
		void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed = 1.0);
		void Pause();
	}
}
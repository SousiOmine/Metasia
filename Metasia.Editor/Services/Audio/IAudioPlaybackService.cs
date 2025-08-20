using Metasia.Core.Objects;
using Metasia.Core.Project;

namespace Metasia.Editor.Services.Audio
{
	public interface IAudioPlaybackService
	{
        public bool IsPlaying { get; }
        public long CurrentSample { get; }
		public void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed = 1.0);
		public void Pause();
	}
}
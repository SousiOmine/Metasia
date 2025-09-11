using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Sounds;

namespace Metasia.Editor.Services.Audio
{
	public class AudioPlaybackService : IAudioPlaybackService
	{
        public bool IsPlaying { get; private set; }

        public long CurrentSample { get; private set; }

        private readonly IAudioService audioService;
        private CancellationTokenSource? cancellationTokenSource;

        public AudioPlaybackService(IAudioService audioService)
        {
            this.audioService = audioService;
        }

		public void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels)
		{
            if (IsPlaying) return;
            
			IsPlaying = true;
            audioService.ClearQueue();
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => AudioGenerationLoopAsync(timeline, projectInfo, startSample, speed, samplingRate, audioChannels, cancellationTokenSource.Token));
		}
		public void Pause()
		{
            if (!IsPlaying) return;

			IsPlaying = false;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            //cancellationTokenSource = null;

            audioService.ClearQueue();
		}

        private async Task AudioGenerationLoopAsync(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels, CancellationToken cancelToken)
        {
            try
            {
                var audioFormat = new AudioFormat(samplingRate, audioChannels);

                long currentSamplePosition = startSample;

                long targetBufferingSize = audioFormat.SampleRate / 2; //とりあえず0.5秒

                CurrentSample = currentSamplePosition;

                // タイムライン全体の長さを計算
                double timelineDuration = (timeline.EndFrame - timeline.StartFrame) / projectInfo.Framerate;

                //再生開始直前にキューをある程度満たす
                while (audioService.GetQueuedSamplesCount() < targetBufferingSize && !cancelToken.IsCancellationRequested)
                {
                    IAudioChunk chunk = timeline.GetAudioChunk(new GetAudioContext(audioFormat, currentSamplePosition, targetBufferingSize, projectInfo.Framerate, timelineDuration));
                    audioService.InsertQueue(chunk);
                    currentSamplePosition += targetBufferingSize;
                    CurrentSample = currentSamplePosition;
                }

                while (!cancelToken.IsCancellationRequested)
                {
                    if (audioService.GetQueuedSamplesCount() < targetBufferingSize)
                    {
                        var chunk = timeline.GetAudioChunk(new GetAudioContext(audioFormat, currentSamplePosition, targetBufferingSize, projectInfo.Framerate, timelineDuration));
                        audioService.InsertQueue(chunk);
                        currentSamplePosition += targetBufferingSize;
                        CurrentSample = currentSamplePosition;
                    }
                    else
                    {
                        await Task.Delay(10, cancelToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常な停止
            }
            catch (Exception ex)
            {
                // 予期しない例外
                Debug.WriteLine($"予期しない例外: {ex.Message}");
            }
            finally
            {
                IsPlaying = false;
            }
        }
	}
}

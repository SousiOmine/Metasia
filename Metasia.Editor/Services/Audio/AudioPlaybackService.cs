using System;
using System.Diagnostics;
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

		public void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed)
		{
            if (IsPlaying) return;
            
			IsPlaying = true;
            audioService.ClearQueue();
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => AudioGenerationLoopAsync(timeline, projectInfo, startSample, speed, cancellationTokenSource.Token));
		}
		public void Pause()
		{
            if (!IsPlaying) return;

			IsPlaying = false;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;

            audioService.ClearQueue();
		}

        private async Task AudioGenerationLoopAsync(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, CancellationToken cancelToken)
        {
            try
            {
                var audioFormat = new AudioFormat(44100, 2);    //将来的にプロジェクト設定から取得

                long currentSamplePosition = startSample;

                long targetBufferingSize = audioFormat.SampleRate / 2; //とりあえず0.5秒

                //再生開始直前にキューをある程度満たす
                while (audioService.GetQueuedSamplesCount() < targetBufferingSize && !cancelToken.IsCancellationRequested)
                {
                    var chunk = timeline.GetAudioChunk(audioFormat, currentSamplePosition, targetBufferingSize);
                    audioService.InsertQueue(chunk);
                    currentSamplePosition += targetBufferingSize;
                }

                while (!cancelToken.IsCancellationRequested)
                {
                    if (audioService.GetQueuedSamplesCount() < targetBufferingSize)
                    {
                        var chunk = timeline.GetAudioChunk(audioFormat, currentSamplePosition, targetBufferingSize);
                        audioService.InsertQueue(chunk);
                        currentSamplePosition += targetBufferingSize;
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
        }
	}
}
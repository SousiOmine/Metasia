using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Sounds;

namespace Metasia.Editor.Services.Audio
{
    public class AudioPlaybackService : IAudioPlaybackService
    {
        private const double PrefillSeconds = 2.0;
        private const double RefillLowWatermarkSeconds = 1.0;
        private const double RequestChunkSeconds = 1.0;

        public bool IsPlaying { get; private set; }

        public long CurrentSample { get; private set; }

        private readonly IAudioService audioService;
        private CancellationTokenSource? cancellationTokenSource;

        public AudioPlaybackService(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        public void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels, IAudioFileAccessor audioFileAccessor, string projectPath)
        {
            if (IsPlaying) return;

            IsPlaying = true;
            audioService.ClearQueue();
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => AudioGenerationLoopAsync(timeline, projectInfo, startSample, speed, samplingRate, audioChannels, audioFileAccessor, projectPath, cancellationTokenSource.Token));
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

        private async Task AudioGenerationLoopAsync(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels, IAudioFileAccessor audioFileAccessor, string projectPath, CancellationToken cancelToken)
        {
            try
            {
                var audioFormat = new AudioFormat(samplingRate, audioChannels);

                long currentSamplePosition = startSample;
                long prefillBufferSize = SecondsToSamples(audioFormat.SampleRate, PrefillSeconds);
                long refillLowWatermarkSize = SecondsToSamples(audioFormat.SampleRate, RefillLowWatermarkSeconds);
                long requestChunkSize = SecondsToSamples(audioFormat.SampleRate, RequestChunkSeconds);

                CurrentSample = currentSamplePosition;

                // タイムライン全体の長さとして渡す値
                double timelineDuration = int.MaxValue / projectInfo.Framerate;

                //再生開始直前にキューをある程度満たす
                while (audioService.GetQueuedSamplesCount() < prefillBufferSize && !cancelToken.IsCancellationRequested)
                {
                    IAudioChunk chunk = await timeline.GetAudioChunkAsync(new GetAudioContext(audioFormat, currentSamplePosition, requestChunkSize, projectInfo.Framerate, timelineDuration, audioFileAccessor, projectPath));
                    audioService.InsertQueue(chunk);
                    currentSamplePosition += requestChunkSize;
                    CurrentSample = currentSamplePosition;
                }

                while (!cancelToken.IsCancellationRequested)
                {
                    if (audioService.GetQueuedSamplesCount() < refillLowWatermarkSize)
                    {
                        while (audioService.GetQueuedSamplesCount() < prefillBufferSize && !cancelToken.IsCancellationRequested)
                        {
                            var chunk = await timeline.GetAudioChunkAsync(new GetAudioContext(audioFormat, currentSamplePosition, requestChunkSize, projectInfo.Framerate, timelineDuration, audioFileAccessor, projectPath));
                            audioService.InsertQueue(chunk);
                            currentSamplePosition += requestChunkSize;
                            CurrentSample = currentSamplePosition;
                        }
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

        private static long SecondsToSamples(int sampleRate, double seconds)
        {
            return Math.Max(1L, (long)Math.Round(sampleRate * seconds));
        }
    }
}

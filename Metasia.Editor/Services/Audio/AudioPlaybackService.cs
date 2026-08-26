using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Sounds;

namespace Metasia.Editor.Services.Audio
{
    public class AudioPlaybackService : IAudioPlaybackService
    {
        private const double PrefillSeconds = 0.2;
        private const double RefillLowWatermarkSeconds = 0.1;
        private const double RequestChunkSeconds = 0.05;

        public bool IsPlaying { get; private set; }

        public long CurrentSample { get; private set; }

        private readonly IAudioService audioService;
        private CancellationTokenSource? cancellationTokenSource;

        public AudioPlaybackService(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        public void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels, IAudioFileAccessor audioFileAccessor, string projectPath, IReadOnlyDictionary<string, TimelineObject> availableTimelines)
        {
            if (IsPlaying) return;

            IsPlaying = true;
            audioService.ClearQueue();
            var cts = new CancellationTokenSource();
            cancellationTokenSource = cts;
            Task.Run(() => AudioGenerationLoopAsync(timeline, projectInfo, startSample, speed, samplingRate, audioChannels, audioFileAccessor, projectPath, availableTimelines, cts));
        }

        public void Pause()
        {
            var cts = cancellationTokenSource;
            cancellationTokenSource = null;
            if (cts is null) return;

            IsPlaying = false;
            cts.Cancel();
            cts.Dispose();

            audioService.ClearQueue();
        }

        private async Task AudioGenerationLoopAsync(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels, IAudioFileAccessor audioFileAccessor, string projectPath, IReadOnlyDictionary<string, TimelineObject> availableTimelines, CancellationTokenSource cts)
        {
            var cancelToken = cts.Token;
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
                    IAudioChunk chunk = await timeline.GetAudioChunkAsync(new GetAudioContext(
                        audioFormat,
                        currentSamplePosition,
                        requestChunkSize,
                        projectInfo.Framerate,
                        timelineDuration,
                        audioFileAccessor,
                        projectPath,
                        availableTimelines,
                        string.IsNullOrWhiteSpace(timeline.Id) ? Array.Empty<string>() : [timeline.Id]));
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
                            var chunk = await timeline.GetAudioChunkAsync(new GetAudioContext(
                                audioFormat,
                                currentSamplePosition,
                                requestChunkSize,
                                projectInfo.Framerate,
                                timelineDuration,
                                audioFileAccessor,
                                projectPath,
                                availableTimelines,
                                string.IsNullOrWhiteSpace(timeline.Id) ? Array.Empty<string>() : [timeline.Id]));
                            audioService.InsertQueue(chunk);
                            currentSamplePosition += requestChunkSize;
                            CurrentSample = currentSamplePosition;
                        }
                    }
                    else
                    {
                        await Task.Delay(1, cancelToken);
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
                // 自分が現在の再生タスクである場合のみIsPlayingを解除する。
                // Pause()直後にPlay()が呼ばれた場合、古いタスクの終了処理が新しい再生の状態を上書きしないようにする。
                if (ReferenceEquals(this.cancellationTokenSource, cts))
                {
                    IsPlaying = false;
                }

                // キャンセルによる停止はPause()側でDispose済みのため、ここでは自然終了時のみ破棄する
                if (!cancelToken.IsCancellationRequested)
                {
                    cts.Dispose();
                }
            }
        }

        private static long SecondsToSamples(int sampleRate, double seconds)
        {
            return Math.Max(1L, (long)Math.Round(sampleRate * seconds));
        }
    }
}

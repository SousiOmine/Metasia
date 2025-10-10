using System;
using System.Diagnostics;
using Avalonia.Threading;
using Metasia.Editor.Services.Audio;

namespace Metasia.Editor.Models.States;

public class PlaybackState : IPlaybackState
{
    public int CurrentFrame
    {
        get => _currentFrame;
        private set
        {
            if (_currentFrame != value)
            {
                _currentFrame = value;
                PlaybackFrameChanged?.Invoke();
            }
        }
    }

    public bool IsPlaying { get; private set; }

    public int SamplingRate { get; } = 44100;

    public int AudioChannels { get; } = 2;

    public event Action? PlaybackStarted;
    public event Action? PlaybackPaused;
    public event Action? PlaybackSeeked;

    public event Action? PlaybackFrameChanged;
    public event Action? ReRenderingRequested;
    private readonly IProjectState _projectState;
    private readonly IAudioPlaybackService _audioPlaybackService;

    private int _currentFrame;

    private DispatcherTimer? timer;
    private readonly Stopwatch _playbackStopwatch = new();
    private int _frameAtPlaybackStart;

    public PlaybackState(IProjectState projectState, IAudioPlaybackService audioPlaybackService)
    {
        CurrentFrame = 0;
        IsPlaying = false;
        _projectState = projectState ?? throw new ArgumentNullException(nameof(projectState));
        _audioPlaybackService = audioPlaybackService ?? throw new ArgumentNullException(nameof(audioPlaybackService));
    }

    public void Pause()
    {
        StopPlaybackTimer();
        IsPlaying = false;
        _audioPlaybackService.Pause();
        _playbackStopwatch.Reset();
        PlaybackPaused?.Invoke();
    }

    public void Play()
    {
        if (_projectState.CurrentProjectInfo is null || _projectState.CurrentTimeline is null)
        {
            Debug.WriteLine("PlaybackState.Play skipped: project or timeline is not ready.");
            return;
        }

        StopPlaybackTimer();

        try
        {
            _frameAtPlaybackStart = CurrentFrame;
            _playbackStopwatch.Restart();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(5)
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            IsPlaying = true;
            PlaybackStarted?.Invoke();

            long startSample = (long)(CurrentFrame / (double)_projectState.CurrentProjectInfo.Framerate * SamplingRate);
            _audioPlaybackService.Play(_projectState.CurrentTimeline, _projectState.CurrentProjectInfo, startSample, 1.0, SamplingRate, AudioChannels);
        }
        catch (Exception ex)
        {
            StopPlaybackTimer();
            IsPlaying = false;
            _playbackStopwatch.Reset();
            Debug.WriteLine($"PlaybackState.Play failed: {ex.Message}");
            throw;
        }
    }

    public void Seek(int frame)
    {
        if (_projectState.CurrentTimeline is not null)
        {
            frame = Math.Max(frame, _projectState.CurrentTimeline.StartFrame);
        }

        CurrentFrame = frame;
        _playbackStopwatch.Reset();
        _frameAtPlaybackStart = CurrentFrame;

        PlaybackSeeked?.Invoke();

        // シーク時は強制的に再生停止
        Pause();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_projectState.CurrentProjectInfo is null || _projectState.CurrentTimeline is null)
        {
            return;
        }

        var elapsedSeconds = _playbackStopwatch.Elapsed.TotalSeconds;
        var elapsedFrames = (int)Math.Floor(elapsedSeconds * _projectState.CurrentProjectInfo.Framerate);
        var newFrame = _frameAtPlaybackStart + elapsedFrames;

        newFrame = Math.Max(newFrame, _projectState.CurrentTimeline.StartFrame);

        if (newFrame != CurrentFrame)
        {
            CurrentFrame = newFrame;
        }
    }

    public void RequestReRendering()
    {
        ReRenderingRequested?.Invoke();
    }

    public void Dispose()
    {
        StopPlaybackTimer();
    }

    private void StopPlaybackTimer()
    {
        if (timer is not null)
        {
            timer.Stop();
            timer.Tick -= Timer_Tick;
            timer = null;
        }
        _playbackStopwatch.Stop();
    }
}
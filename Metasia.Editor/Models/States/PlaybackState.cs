using System;
using System.Diagnostics;
using System.Timers;
using Metasia.Editor.Services.Audio;

namespace Metasia.Editor.Models.States;

public class PlaybackState : IPlaybackState
{
    public int CurrentFrame { 
        get => _currentFrame;
        private set {
            _currentFrame = value;
            PlaybackFrameChanged?.Invoke();
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
    private IProjectState _projectState;
    private IAudioPlaybackService _audioPlaybackService;

    private int _currentFrame;

    private System.Timers.Timer? timer;

    public PlaybackState(IProjectState projectState, IAudioPlaybackService audioPlaybackService)
    {
        CurrentFrame = 0;
        IsPlaying = false;
        _projectState = projectState;
        _audioPlaybackService = audioPlaybackService;
    }

    public void Pause()
    {
        if(timer is not null) {
            timer.Stop();
            timer.Elapsed -= Timer_Elapsed;
            timer.Dispose();
            timer = null;
        }
		IsPlaying = false;
		_audioPlaybackService.Pause();
		PlaybackPaused?.Invoke();

    }

    public void Play()
    {
        if (timer is not null)
        {
            timer.Stop();
            timer.Elapsed -= Timer_Elapsed;
            timer.Dispose();
            timer = null;
        }
        try
        {
            timer = new System.Timers.Timer(1000 / _projectState.CurrentProjectInfo!.Framerate);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            IsPlaying = true;
            PlaybackStarted?.Invoke();

            long startSample = (long)(CurrentFrame / (double)_projectState.CurrentProjectInfo!.Framerate * SamplingRate);
            _audioPlaybackService.Play(_projectState.CurrentTimeline!, _projectState.CurrentProjectInfo!, startSample, 1.0, SamplingRate, AudioChannels);
        }
        catch (Exception ex)
        {
            if (timer is not null)
            {
                timer.Stop();
                timer.Elapsed -= Timer_Elapsed;
                timer.Dispose();
                timer = null;
            }
            IsPlaying = false;
            Debug.WriteLine($"PlaybackState.Play failed: {ex.Message}");
            throw;
        }

    }

    public void Seek(int frame)
    {
        CurrentFrame = frame;
        PlaybackSeeked?.Invoke();
        // シーク時は強制的に再生停止
        Pause();
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        CurrentFrame++;
    }

    public void RequestReRendering()
    {
        ReRenderingRequested?.Invoke();
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}
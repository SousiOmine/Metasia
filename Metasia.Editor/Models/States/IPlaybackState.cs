using System;


namespace Metasia.Editor.Models.States;

public interface IPlaybackState : IDisposable
{
    int CurrentFrame { get; }
    bool IsPlaying { get; }
    
    void Play();
    void Pause();
    void Seek(int frame);

    event Action? PlaybackStarted;
    event Action? PlaybackPaused;
    event Action? PlaybackSeeked;
    event Action? PlaybackFrameChanged;

    event Action? ReRenderingRequested;

    void RequestReRendering();
}

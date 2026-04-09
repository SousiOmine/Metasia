using Metasia.Editor.Abstractions.States;
using System;

namespace Metasia.Editor.Models.States;

public class TimelineViewState : ITimelineViewState
{
    private bool _isDisposed;

    public double Frame_Per_DIP
    {
        get => _frame_per_DIP;
        set
        {
            if (_isDisposed) return;
            if (_frame_per_DIP == value) return;
            _frame_per_DIP = value;
            Frame_Per_DIP_Changed?.Invoke();
        }
    }

    public event Action? Frame_Per_DIP_Changed;

    private double _frame_per_DIP;

    public int HorizontalScrollPosition
    {
        get => _horizontalScrollPosition;
        set
        {
            if (_isDisposed) return;
            if (_horizontalScrollPosition == value) return;
            _horizontalScrollPosition = value;
            HorizontalScrollPosition_Changed?.Invoke();
        }
    }

    public event Action? HorizontalScrollPosition_Changed;

    private int _horizontalScrollPosition;

    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            if (_isDisposed) return;
            if (_currentFrame == value) return;
            _currentFrame = value;
            CurrentFrame_Changed?.Invoke();
        }
    }

    public event Action? CurrentFrame_Changed;

    private int _currentFrame;

    public TimelineViewState()
    {
        _frame_per_DIP = 3.0;
        _horizontalScrollPosition = 0;
        _currentFrame = 0;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Frame_Per_DIP_Changed = null;
        HorizontalScrollPosition_Changed = null;
        CurrentFrame_Changed = null;
    }
}
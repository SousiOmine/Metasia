using System;

namespace Metasia.Editor.Models.States;

public class TimelineViewState : ITimelineViewState
{
    public double Frame_Per_DIP
    {
        get => _frame_per_DIP;
        set
        {
            _frame_per_DIP = value;
            Frame_Per_DIP_Changed?.Invoke();
        }
    }

    public event Action? Frame_Per_DIP_Changed;

    private double _frame_per_DIP;

    public TimelineViewState()
    {
        _frame_per_DIP = 3.0;
    }

    public void Dispose()
    {
        Frame_Per_DIP_Changed = null;
    }
}
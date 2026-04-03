using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Timeline;

public class ControlHighlightInfo : ReactiveObject
{
    public double Left
    {
        get => _left;
        set => this.RaiseAndSetIfChanged(ref _left, value);
    }

    public double Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }

    private double _left;
    private double _width;

    public int StartFrame { get; set; }
    public int EndFrame { get; set; }

    public void Recalculate(double framePerDip)
    {
        Left = StartFrame * framePerDip;
        Width = Math.Max(0, (EndFrame - StartFrame + 1) * framePerDip);
    }
}

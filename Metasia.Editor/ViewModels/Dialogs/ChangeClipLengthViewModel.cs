using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Reactive;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Dialogs;

public class ChangeClipLengthViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, bool> OkCommand { get; }
    public ReactiveCommand<Unit, bool> CancelCommand { get; }

    private double _lengthSeconds;
    public double LengthSeconds
    {
        get => _lengthSeconds;
        set => this.RaiseAndSetIfChanged(ref _lengthSeconds, value);
    }

    public int FrameRate { get; }

    public ChangeClipLengthViewModel(int currentLengthFrames, int frameRate)
    {
        FrameRate = frameRate;
        LengthSeconds = Math.Round((double)currentLengthFrames / frameRate, 6);

        var canExecuteOk = this.WhenAnyValue(
            x => x.LengthSeconds,
            length => length > 0);

        OkCommand = ReactiveCommand.Create(() => true, canExecuteOk);
        CancelCommand = ReactiveCommand.Create(() => false);
    }

    public int GetNewLengthFrames()
    {
        return Math.Max(1, (int)Math.Round(LengthSeconds * FrameRate));
    }
}
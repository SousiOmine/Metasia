using System;

namespace Metasia.Editor.Models.States;

public interface ITimelineViewState : IDisposable
{
    double Frame_Per_DIP { get; set; }

    event Action? Frame_Per_DIP_Changed;
}
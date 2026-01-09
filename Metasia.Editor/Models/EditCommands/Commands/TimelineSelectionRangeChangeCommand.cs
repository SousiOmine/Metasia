using System;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class TimelineSelectionRangeChangeCommand : IEditCommand
{
    public string Description => "選択範囲を変更";

    private readonly TimelineObject _timeline;
    private readonly int _oldStart;
    private readonly int _oldEnd;
    private readonly int _newStart;
    private readonly int _newEnd;

    public TimelineSelectionRangeChangeCommand(TimelineObject timeline, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        _timeline = timeline;
        _oldStart = timeline.SelectionStart;
        _oldEnd = timeline.SelectionEnd;
        _newStart = start;
        _newEnd = end;
    }

    public void Execute()
    {
        _timeline.SelectionStart = _newStart;
        _timeline.SelectionEnd = _newEnd;
    }

    public void Undo()
    {
        _timeline.SelectionStart = _oldStart;
        _timeline.SelectionEnd = _oldEnd;
    }
}

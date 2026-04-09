using Metasia.Editor.Abstractions.States;
using System.Collections.Concurrent;

namespace Metasia.Editor.Models.States;

public class TimelineViewStateStore : ITimelineViewStateStore
{
    private readonly ConcurrentDictionary<string, ITimelineViewState> _states = new();

    public ITimelineViewState GetViewState(string timelineId)
    {
        return _states.GetOrAdd(timelineId, _ => new TimelineViewState());
    }

    public bool Contains(string timelineId)
    {
        return _states.ContainsKey(timelineId);
    }

    public void Remove(string timelineId)
    {
        if (_states.TryRemove(timelineId, out var state))
        {
            state.Dispose();
        }
    }

    public void Clear()
    {
        foreach (var timelineId in _states.Keys)
        {
            Remove(timelineId);
        }
    }
}

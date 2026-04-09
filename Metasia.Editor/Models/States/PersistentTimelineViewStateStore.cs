using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metasia.Editor.Models.States;

/// <summary>
/// タイムライン表示状態のメモリ保持に加えて、プロジェクト単位の保存と復元を行うストアです。
/// </summary>
public sealed class PersistentTimelineViewStateStore : ITimelineViewStateStore, IDisposable
{
    private static readonly TimeSpan DefaultSaveDebounceDelay = TimeSpan.FromMilliseconds(300);

    private readonly ITimelineViewStateStore _innerStore;
    private readonly IProjectTimelineViewStateRepository _repository;
    private readonly IProjectState _projectState;
    private readonly TimeSpan _saveDebounceDelay;
    private readonly Timer _saveDebounceTimer;
    private readonly Lock _stateLock = new();
    private readonly Dictionary<string, ITimelineViewState> _trackedStates = new(StringComparer.Ordinal);
    private MetasiaEditorProject? _loadedProject;
    private bool _hasDirtyState;
    private bool _isApplyingSnapshot;
    private bool _isDisposed;

    public PersistentTimelineViewStateStore(
        TimelineViewStateStore innerStore,
        IProjectTimelineViewStateRepository repository,
        IProjectState projectState,
        TimeSpan? saveDebounceDelay = null)
    {
        ArgumentNullException.ThrowIfNull(innerStore);
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(projectState);

        _innerStore = innerStore;
        _repository = repository;
        _projectState = projectState;
        _saveDebounceDelay = saveDebounceDelay ?? DefaultSaveDebounceDelay;
        _saveDebounceTimer = new Timer(static state =>
        {
            ((PersistentTimelineViewStateStore)state!).FlushPendingSave();
        }, this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        _projectState.ProjectLoaded += OnProjectLoaded;
        _projectState.ProjectClosed += OnProjectClosed;
    }

    public ITimelineViewState GetViewState(string timelineId)
    {
        lock (_stateLock)
        {
            return GetViewStateCore(timelineId);
        }
    }

    public bool Contains(string timelineId)
    {
        lock (_stateLock)
        {
            return _innerStore.Contains(timelineId);
        }
    }

    public void Remove(string timelineId)
    {
        lock (_stateLock)
        {
            UntrackStateCore(timelineId);
            _innerStore.Remove(timelineId);
        }
    }

    public void Clear()
    {
        lock (_stateLock)
        {
            ClearCore();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _projectState.ProjectLoaded -= OnProjectLoaded;
        _projectState.ProjectClosed -= OnProjectClosed;
        FlushPendingSave();

        lock (_stateLock)
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _saveDebounceTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            ClearCore();
            _loadedProject = null;
            _hasDirtyState = false;
        }

        _saveDebounceTimer.Dispose();
    }

    private void OnProjectLoaded()
    {
        FlushPendingSave();

        ProjectTimelineViewStateSnapshot? snapshot = null;
        lock (_stateLock)
        {
            ClearCore();
            _loadedProject = _projectState.CurrentProject;
            _hasDirtyState = false;

            var project = _loadedProject;
            if (project is null)
            {
                return;
            }

            foreach (var timeline in project.Timelines)
            {
                GetViewStateCore(timeline.Id);
            }

            if (!string.IsNullOrWhiteSpace(project.ProjectFilePath))
            {
                snapshot = _repository.Load(project.ProjectFilePath);
            }
        }

        if (snapshot is null)
        {
            return;
        }

        ApplySnapshot(snapshot);
    }

    private void OnProjectClosed()
    {
        FlushPendingSave();

        lock (_stateLock)
        {
            _saveDebounceTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            ClearCore();
            _loadedProject = null;
            _hasDirtyState = false;
        }
    }

    private void ApplySnapshot(ProjectTimelineViewStateSnapshot snapshot)
    {
        lock (_stateLock)
        {
            _isApplyingSnapshot = true;
            try
            {
                foreach (var timelineSnapshot in snapshot.Timelines)
                {
                    var viewState = GetViewStateCore(timelineSnapshot.TimelineId);
                    viewState.Frame_Per_DIP = timelineSnapshot.FramePerDip;
                    viewState.LastPreviewFrame = timelineSnapshot.LastPreviewFrame;
                    viewState.HorizontalScrollPosition = timelineSnapshot.HorizontalScrollPosition;
                }
            }
            finally
            {
                _isApplyingSnapshot = false;
            }
        }
    }

    private ITimelineViewState GetViewStateCore(string timelineId)
    {
        var state = _innerStore.GetViewState(timelineId);
        TrackStateCore(timelineId, state);
        return state;
    }

    private void TrackStateCore(string timelineId, ITimelineViewState state)
    {
        if (_trackedStates.TryGetValue(timelineId, out var existingState))
        {
            if (ReferenceEquals(existingState, state))
            {
                return;
            }

            Unsubscribe(existingState);
        }

        Subscribe(state);
        _trackedStates[timelineId] = state;
    }

    private void UntrackStateCore(string timelineId)
    {
        if (_trackedStates.Remove(timelineId, out var state))
        {
            Unsubscribe(state);
        }
    }

    private void Subscribe(ITimelineViewState state)
    {
        state.Frame_Per_DIP_Changed += OnViewStateChanged;
        state.HorizontalScrollPosition_Changed += OnViewStateChanged;
        state.LastPreviewFrame_Changed += OnViewStateChanged;
    }

    private void Unsubscribe(ITimelineViewState state)
    {
        state.Frame_Per_DIP_Changed -= OnViewStateChanged;
        state.HorizontalScrollPosition_Changed -= OnViewStateChanged;
        state.LastPreviewFrame_Changed -= OnViewStateChanged;
    }

    private void OnViewStateChanged()
    {
        var flushImmediately = false;
        lock (_stateLock)
        {
            if (_isApplyingSnapshot || _isDisposed)
            {
                return;
            }

            _hasDirtyState = true;
            if (_saveDebounceDelay <= TimeSpan.Zero)
            {
                flushImmediately = true;
            }
            else
            {
                _saveDebounceTimer.Change(_saveDebounceDelay, Timeout.InfiniteTimeSpan);
            }
        }

        if (flushImmediately)
        {
            FlushPendingSave();
        }
    }

    private void FlushPendingSave()
    {
        ProjectTimelineViewStateSnapshot? snapshot;
        lock (_stateLock)
        {
            if (_isDisposed || !_hasDirtyState)
            {
                return;
            }

            _saveDebounceTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            snapshot = CreateSnapshotCore(_loadedProject);
            if (snapshot is null)
            {
                return;
            }

            _hasDirtyState = false;
        }

        _repository.Save(snapshot);
    }

    private ProjectTimelineViewStateSnapshot? CreateSnapshotCore(MetasiaEditorProject? project)
    {
        if (project is null || string.IsNullOrWhiteSpace(project.ProjectFilePath))
        {
            return null;
        }

        return new ProjectTimelineViewStateSnapshot
        {
            ProjectFilePath = project.ProjectFilePath,
            Timelines = project.Timelines
                .Select(timeline =>
                {
                    var viewState = GetViewStateCore(timeline.Id);
                    return new TimelineViewStateSnapshot
                    {
                        TimelineId = timeline.Id,
                        FramePerDip = viewState.Frame_Per_DIP,
                        LastPreviewFrame = viewState.LastPreviewFrame,
                        HorizontalScrollPosition = viewState.HorizontalScrollPosition
                    };
                })
                .ToList()
        };
    }

    private void ClearCore()
    {
        foreach (var timelineId in _trackedStates.Keys.ToArray())
        {
            UntrackStateCore(timelineId);
        }

        _innerStore.Clear();
    }
}

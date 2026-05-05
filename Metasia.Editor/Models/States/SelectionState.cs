using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.States;
using System;
using System.Collections.Generic;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;

namespace Metasia.Editor.Models.States;

public class SelectionState : ISelectionState, IDisposable
{
    public IReadOnlyList<ClipObject> SelectedClips { get; }

    public ClipObject? CurrentSelectedClip { get; private set; }

    public LayerObject? SelectedLayer { get; private set; }

    public event Action? SelectionChanged;
    public event Action? LayerSelectionChanged;

    private List<ClipObject> _selectedClips = new();

    public SelectionState()
    {
        SelectedClips = _selectedClips.AsReadOnly();
    }

    public void ClearSelectedClips()
    {
        _selectedClips.Clear();
        CurrentSelectedClip = null;
        SelectionChanged?.Invoke();
    }

    public void SelectClip(ClipObject clip)
    {
        _selectedClips.Add(clip);
        if (CurrentSelectedClip is null)
        {
            CurrentSelectedClip = clip;
        }
        SelectionChanged?.Invoke();
    }

    public void SelectClips(List<ClipObject> clips)
    {
        foreach (var clip in clips)
        {
            _selectedClips.Add(clip);
        }
        if (CurrentSelectedClip is null && clips.Count > 0)
        {
            CurrentSelectedClip = clips.First();
        }
        SelectionChanged?.Invoke();
    }

    public void UnselectClip(ClipObject clip)
    {
        _selectedClips.Remove(clip);
        if (CurrentSelectedClip?.Id == clip.Id)
        {
            CurrentSelectedClip = _selectedClips.FirstOrDefault();
        }
        SelectionChanged?.Invoke();
    }

    public void UnselectClips(IEnumerable<ClipObject> clips)
    {
        foreach (var clip in clips)
        {
            _selectedClips.Remove(clip);
            if (CurrentSelectedClip?.Id == clip.Id)
            {
                CurrentSelectedClip = _selectedClips.FirstOrDefault();
            }
        }
        SelectionChanged?.Invoke();
    }

    public void Dispose()
    {
        SelectionChanged = null;
        LayerSelectionChanged = null;
    }

    public void SelectLayer(LayerObject layer)
    {
        SelectedLayer = layer;
        LayerSelectionChanged?.Invoke();
    }

    public void ClearSelectedLayer()
    {
        SelectedLayer = null;
        LayerSelectionChanged?.Invoke();
    }
}
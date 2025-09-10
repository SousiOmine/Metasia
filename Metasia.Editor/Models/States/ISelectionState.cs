using System;
using System.Collections.Generic;
using Metasia.Core.Objects;


namespace Metasia.Editor.Models.States;

public interface ISelectionState
{
    IReadOnlyList<ClipObject> SelectedClips { get; }
    ClipObject? CurrentSelectedClip { get; }

    void SelectClip(ClipObject clip);
    void SelectClips(List<ClipObject> clips);
    void UnselectClip(ClipObject clip);
    void UnselectClips(IEnumerable<ClipObject> clips);
    void ClearSelectedClips();

    event Action? SelectionChanged;
}
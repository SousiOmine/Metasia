using System.Collections.Generic;

namespace Metasia.Editor.Models.States;

/// <summary>
/// 1 つのプロジェクトに紐づくタイムライン表示状態の保存データです。
/// </summary>
public sealed class ProjectTimelineViewStateSnapshot
{
    public string ProjectFilePath { get; set; } = string.Empty;

    public List<TimelineViewStateSnapshot> Timelines { get; set; } = [];
}

/// <summary>
/// 1 つのタイムラインに紐づく表示状態の保存データです。
/// </summary>
public sealed class TimelineViewStateSnapshot
{
    public string TimelineId { get; set; } = string.Empty;

    public double FramePerDip { get; set; } = 3.0;

    public int LastPreviewFrame { get; set; }

    public int HorizontalScrollPosition { get; set; }
}

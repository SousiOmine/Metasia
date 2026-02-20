using Avalonia;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.DragDrop;

/// <summary>
/// ドロップ対象の情報をまとめたコンテキスト
/// </summary>
public record DropTargetContext
{
    /// <summary>
    /// ドロップ先のレイヤー
    /// </summary>
    public LayerObject TargetLayer { get; init; }

    /// <summary>
    /// ドロップ位置のフレーム
    /// </summary>
    public int TargetFrame { get; init; }

    /// <summary>
    /// 対象のタイムライン
    /// </summary>
    public TimelineObject Timeline { get; init; }

    /// <summary>
    /// ドロップ位置（ピクセル座標）
    /// </summary>
    public Point DropPosition { get; init; }

    public DropTargetContext(LayerObject targetLayer, int targetFrame, TimelineObject timeline, Point dropPosition)
    {
        TargetLayer = targetLayer;
        TargetFrame = targetFrame;
        Timeline = timeline;
        DropPosition = dropPosition;
    }
}
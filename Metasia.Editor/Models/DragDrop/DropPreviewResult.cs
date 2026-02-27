using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.Models.DragDrop;

/// <summary>
/// ドラッグオーバー時のプレビュー結果
/// </summary>
public record DropPreviewResult
{
    /// <summary>
    /// ドロップ可能かどうか
    /// </summary>
    public bool CanDrop { get; init; }

    /// <summary>
    /// プレビュー用のコマンド（nullの場合はプレビューなし）
    /// </summary>
    public IEditCommand? PreviewCommand { get; init; }

    /// <summary>
    /// ドラッグエフェクトの種類（Copy, Move, None等）
    /// </summary>
    public DropEffect Effect { get; init; }

    public static DropPreviewResult None => new() { CanDrop = false, Effect = DropEffect.None };

    public static DropPreviewResult Move(IEditCommand? previewCommand = null) => new()
    {
        CanDrop = true,
        PreviewCommand = previewCommand,
        Effect = DropEffect.Move
    };

    public static DropPreviewResult Copy(IEditCommand? previewCommand = null) => new()
    {
        CanDrop = true,
        PreviewCommand = previewCommand,
        Effect = DropEffect.Copy
    };
}

/// <summary>
/// ドロップエフェクトの種類
/// </summary>
public enum DropEffect
{
    None,
    Copy,
    Move,
    Link
}
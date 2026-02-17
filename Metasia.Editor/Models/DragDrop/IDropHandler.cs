using Avalonia.Input;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.Models.DragDrop;

/// <summary>
/// ドラッグアンドドロップ処理を行うハンドラのインターフェース
/// </summary>
public interface IDropHandler
{
    /// <summary>
    /// ハンドラの優先度（数値が小さいほど優先度高）
    /// </summary>
    int Priority => 100;

    /// <summary>
    /// このハンドラが処理可能なデータかどうかを判定
    /// </summary>
    /// <param name="data">ドロップデータ</param>
    /// <param name="context">ドロップ対象のコンテキスト</param>
    /// <returns>処理可能な場合true</returns>
    bool CanHandle(IDataObject data, DropTargetContext context);

    /// <summary>
    /// ドラッグオーバー時の処理（プレビュー用）
    /// </summary>
    /// <param name="data">ドロップデータ</param>
    /// <param name="context">ドロップ対象のコンテキスト</param>
    /// <returns>プレビュー結果</returns>
    DropPreviewResult HandleDragOver(IDataObject data, DropTargetContext context);

    /// <summary>
    /// ドロップ時の処理
    /// </summary>
    /// <param name="data">ドロップデータ</param>
    /// <param name="context">ドロップ対象のコンテキスト</param>
    /// <returns>実行するコマンド（nullの場合は何もしない）</returns>
    IEditCommand? HandleDrop(IDataObject data, DropTargetContext context);
}
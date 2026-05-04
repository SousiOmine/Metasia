using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Input;
using Metasia.Editor.Abstractions.EditCommands;
using System.Threading.Tasks;

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
    bool CanHandle(IDataTransfer data, DropTargetContext context);

    /// <summary>
    /// ドラッグオーバー時の処理（プレビュー用）
    /// </summary>
    /// <param name="data">ドロップデータ</param>
    /// <param name="context">ドロップ対象のコンテキスト</param>
    /// <returns>プレビュー結果</returns>
    DropPreviewResult HandleDragOver(IDataTransfer data, DropTargetContext context);

    /// <summary>
    /// ドロップ時の処理
    /// </summary>
    /// <param name="data">ドロップデータ</param>
    /// <param name="context">ドロップ対象のコンテキスト</param>
    /// <returns>実行するコマンド（nullの場合は何もしない）</returns>
    Task<IEditCommand?> HandleDropAsync(IDataTransfer data, DropTargetContext context);
}
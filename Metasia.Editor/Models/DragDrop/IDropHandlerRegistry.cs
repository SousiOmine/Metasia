using System.Collections.Generic;
using Avalonia.Input;

namespace Metasia.Editor.Models.DragDrop;

/// <summary>
/// ドロップハンドラを管理・検索するインターフェース
/// </summary>
public interface IDropHandlerRegistry
{
    /// <summary>
    /// 登録されているすべてのハンドラ
    /// </summary>
    IEnumerable<IDropHandler> Handlers { get; }

    /// <summary>
    /// データに対応するハンドラを検索
    /// </summary>
    /// <param name="data">ドロップデータ</param>
    /// <param name="context">ドロップ対象のコンテキスト</param>
    /// <returns>適切なハンドラ（見つからない場合はnull）</returns>
    IDropHandler? FindHandler(IDataObject data, DropTargetContext context);
}
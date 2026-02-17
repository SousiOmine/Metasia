using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;

namespace Metasia.Editor.Models.DragDrop;

/// <summary>
/// ドロップハンドラを管理・検索する実装
/// </summary>
public class DropHandlerRegistry : IDropHandlerRegistry
{
    private readonly IEnumerable<IDropHandler> _handlers;

    public IEnumerable<IDropHandler> Handlers => _handlers;

    public DropHandlerRegistry(IEnumerable<IDropHandler> handlers)
    {
        _handlers = handlers.OrderBy(h => h.Priority).ToList();
    }

    public IDropHandler? FindHandler(IDataObject data, DropTargetContext context)
    {
        return _handlers.FirstOrDefault(h => h.CanHandle(data, context));
    }
}
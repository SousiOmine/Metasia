using System.Collections.Generic;
using System.Threading;
using Avalonia.Input;

namespace Metasia.Editor.Models.DragDropData;

public static class DragDropFormats
{
    public static readonly DataFormat<string> ClipsMove = DataFormat.CreateStringApplicationFormat("clipsmove");

    public static readonly DataFormat<string> ProjectFile = DataFormat.CreateStringApplicationFormat("projectfile");

    private static readonly Dictionary<string, object> _dataStore = new();
    private static int _nextId = 0;

    public static string StoreData<T>(T data) where T : class
    {
        var id = Interlocked.Increment(ref _nextId).ToString();
        _dataStore[id] = data;
        return id;
    }

    public static T? RetrieveData<T>(string? id) where T : class
    {
        if (id == null || !_dataStore.TryGetValue(id, out var obj))
            return null;
        var data = obj as T;
        _dataStore.Remove(id);
        return data;
    }

    public static T? PeekData<T>(string? id) where T : class
    {
        if (id == null || !_dataStore.TryGetValue(id, out var obj))
            return null;
        return obj as T;
    }

    public static void RemoveData(string? id)
    {
        if (id == null)
            return;

        _dataStore.Remove(id);
    }
}

using Jint.Native.Error;
using SkiaSharp;

namespace Metasia.Core.Render.Cache;

public class LruImageCache : IRenderImageCache
{
    LinkedList<(long key, SKImage image)> cacheList = new LinkedList<(long key, SKImage image)>();
    Dictionary<long, LinkedListNode<(long key, SKImage image)>> cacheMap = new Dictionary<long, LinkedListNode<(long key, SKImage image)>>();

    readonly long capacity;

    readonly object lockObj = new();

    public LruImageCache(long capacity)
    {
        this.capacity = capacity;
    }

    public SKImage? TryGet(long key)
    {
        lock (lockObj)
        {
            cacheMap.TryGetValue(key, out var node);
            if (node is not null)
            {
                cacheList.Remove(node);
                cacheList.AddFirst(node);
            }
            return node?.Value.image;
        }

    }

    public void Set(long key, SKImage image)
    {
        lock (lockObj)
        {
            if (cacheMap.TryGetValue(key, out var existingNode))
            {
                cacheList.Remove(existingNode);
                cacheMap.Remove(key);
            }
            var newNode = cacheList.AddFirst((key, image));
            cacheMap[key] = newNode;

            if (cacheList.Count > capacity)
            {
                var last = cacheList.Last;
                if (last is not null)
                {
                    cacheMap.Remove(last.Value.key);
                    cacheList.RemoveLast();
                }
            }
        }

    }

    public void Clear()
    {
        lock (lockObj)
        {
            cacheList.Clear();
            cacheMap.Clear();
        }


    }
}
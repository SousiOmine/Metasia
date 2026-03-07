using SkiaSharp;

namespace Metasia.Core.Render.Cache;

public interface IRenderImageCache
{
    /// <summary>
    /// 指定されたキーに対応するキャッシュされた画像を取得する
    /// </summary>
    /// <param name="key">画像を識別するためのキー。</param>
    /// <returns>キャッシュされた画像が存在する場合はその画像、存在しない場合はnull</returns>
    SKImage? TryGet(long key);

    /// <summary>
    /// 指定されたキーと画像をキャッシュに保存
    /// </summary>
    /// <param name="key">画像を識別するためのキー。</param>
    /// <param name="image">キャッシュする画像。</param>
    void Set(long key, SKImage image);

    /// <summary>
    /// キャッシュ内のすべての画像をクリア
    /// </summary>
    void Clear();

    const long NO_CACHE_KEY = -1;
}
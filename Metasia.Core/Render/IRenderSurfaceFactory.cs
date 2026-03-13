using SkiaSharp;

namespace Metasia.Core.Render;

public interface IRenderSurfaceFactory : IDisposable
{
    /// <summary>
    /// 描画サーフェスを作成
    /// </summary>
    SKSurface CreateSurface(SKImageInfo info, SKSizeI? viewportSize = null);

    /// <summary>
    /// 描画用の画像を取得
    /// </summary>
    SKImage GetDrawImage(SKImage input);

    /// <summary>
    /// SKSurfaceから画像を取得
    /// </summary>
    SKImage Snapshot(SKSurface surface, bool preferRasterOutput = false);

    /// <summary>
    /// GPUが使用可能か否か
    /// </summary>
    bool IsGpuAvailable { get; }
}
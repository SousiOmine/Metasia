using SkiaSharp;

namespace Metasia.Core.Render;

public sealed class NullRenderSurfaceFactory : IRenderSurfaceFactory
{
    public bool IsGpuAvailable => false;

    public SKSurface CreateSurface(SKImageInfo info, SKSizeI? viewportSize = null)
    {
        return SKSurface.Create(info)
            ?? throw new InvalidOperationException($"Failed to create SKSurface with dimensions {info.Width}x{info.Height}");
    }

    public SKImage GetDrawImage(SKImage input)
    {
        return input;
    }

    public SKImage Snapshot(SKSurface surface, bool preferRasterOutput = false)
    {
        return surface.Snapshot();
    }

    public void Dispose()
    {
    }
}
using System;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Editor.Services.Rendering;

public sealed class FallbackRenderSurfaceFactory : IRenderSurfaceFactory
{
    private readonly IRenderSurfaceFactory _activeFactory;
    private bool _disposed;

    public FallbackRenderSurfaceFactory(params IRenderSurfaceFactory[] factories)
    {
        if (factories.Length == 0)
        {
            throw new ArgumentException("At least one render surface factory is required.", nameof(factories));
        }

        _activeFactory = SelectFactory(factories);
    }

    public bool IsGpuAvailable => _activeFactory.IsGpuAvailable;

    public SKSurface CreateSurface(SKImageInfo info, SKSizeI? viewportSize = null)
    {
        return _activeFactory.CreateSurface(info, viewportSize);
    }

    public SKImage GetDrawImage(SKImage input)
    {
        return _activeFactory.GetDrawImage(input);
    }

    public SKImage Snapshot(SKSurface surface, bool preferRasterOutput = false)
    {
        return _activeFactory.Snapshot(surface, preferRasterOutput);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _activeFactory.Dispose();
    }

    private static IRenderSurfaceFactory SelectFactory(IRenderSurfaceFactory[] factories)
    {
        IRenderSurfaceFactory selected = factories[^1];

        foreach (IRenderSurfaceFactory factory in factories)
        {
            if (factory.IsGpuAvailable)
            {
                selected = factory;
                break;
            }
        }

        foreach (IRenderSurfaceFactory factory in factories)
        {
            if (!ReferenceEquals(factory, selected))
            {
                factory.Dispose();
            }
        }

        return selected;
    }
}

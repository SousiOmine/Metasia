using Metasia.Core.Render;
using Metasia.Editor.Services.Rendering;
using SkiaSharp;

namespace Metasia.Editor.Tests.Services.Rendering;

[TestFixture]
public class FallbackRenderSurfaceFactoryTests
{
    [Test]
    public void Constructor_SelectsFirstGpuAvailableFactory()
    {
        var first = new TestRenderSurfaceFactory(isGpuAvailable: false);
        var second = new TestRenderSurfaceFactory(isGpuAvailable: true);
        var third = new TestRenderSurfaceFactory(isGpuAvailable: true);

        using var factory = new FallbackRenderSurfaceFactory(first, second, third);

        Assert.That(factory.IsGpuAvailable, Is.True);
        Assert.That(first.DisposeCallCount, Is.EqualTo(1));
        Assert.That(second.DisposeCallCount, Is.EqualTo(0));
        Assert.That(third.DisposeCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_FallsBackToLastFactoryWhenGpuFactoriesAreUnavailable()
    {
        var first = new TestRenderSurfaceFactory(isGpuAvailable: false);
        var fallback = new TestRenderSurfaceFactory(isGpuAvailable: false);

        using var factory = new FallbackRenderSurfaceFactory(first, fallback);

        Assert.That(factory.IsGpuAvailable, Is.False);
        Assert.That(first.DisposeCallCount, Is.EqualTo(1));
        Assert.That(fallback.DisposeCallCount, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_DisposesSelectedFactory()
    {
        var selected = new TestRenderSurfaceFactory(isGpuAvailable: true);
        using var factory = new FallbackRenderSurfaceFactory(selected);

        factory.Dispose();

        Assert.That(selected.DisposeCallCount, Is.EqualTo(1));
    }

    private sealed class TestRenderSurfaceFactory(bool isGpuAvailable) : IRenderSurfaceFactory
    {
        public int DisposeCallCount { get; private set; }

        public bool IsGpuAvailable => isGpuAvailable;

        public SKSurface CreateSurface(SKImageInfo info, SKSizeI? viewportSize = null)
        {
            return SKSurface.Create(info)
                ?? throw new InvalidOperationException($"Failed to create test SKSurface with dimensions {info.Width}x{info.Height}");
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
            DisposeCallCount++;
        }
    }
}

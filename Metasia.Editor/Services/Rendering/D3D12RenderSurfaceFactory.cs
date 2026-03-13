using System;
using System.Diagnostics;
using Metasia.Core.Render;
using SkiaSharp;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using static Vortice.Direct3D12.D3D12;
using static Vortice.DXGI.DXGI;

namespace Metasia.Editor.Services.Rendering;

public sealed class D3D12RenderSurfaceFactory : IRenderSurfaceFactory
{
    private readonly IDXGIFactory6? _factory;
    private readonly IDXGIAdapter1? _adapter;
    private readonly ID3D12Device2? _device;
    private readonly ID3D12CommandQueue? _queue;
    private readonly GRContext? _grContext;
    private bool _disposed;

    public bool IsGpuAvailable => _grContext is not null;

    public D3D12RenderSurfaceFactory()
    {
        try
        {
            if (!OperatingSystem.IsWindows() || !D3D12.IsSupported(FeatureLevel.Level_11_0))
            {
                return;
            }

            _factory = CreateDXGIFactory2<IDXGIFactory6>(false);
            _adapter = SelectAdapter(_factory);
            if (_adapter is null)
            {
                return;
            }

            D3D12CreateDevice(_adapter, FeatureLevel.Level_11_0, out ID3D12Device2? device).CheckError();
            _device = device;
            _queue = _device.CreateCommandQueue(CommandListType.Direct);

            var backendContext = new GRVorticeD3DBackendContext
            {
                Adapter = _adapter,
                Device = _device,
                Queue = _queue,
                ProtectedContext = false
            };

            _grContext = GRContext.CreateDirect3D(backendContext);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"D3D12RenderSurfaceFactory initialization failed: {ex.Message}");
            Dispose();
        }
    }

    public SKSurface CreateSurface(SKImageInfo info, SKSizeI? viewportSize = null)
    {
        if (_grContext is not null)
        {
            var gpuSurface = SKSurface.Create(_grContext, false, info);
            if (gpuSurface is not null)
            {
                return gpuSurface;
            }
        }

        return SKSurface.Create(info)
            ?? throw new InvalidOperationException($"Failed to create SKSurface with dimensions {info.Width}x{info.Height}");
    }

    public SKImage GetDrawImage(SKImage input)
    {
        if (_grContext is null || input.IsTextureBacked)
        {
            return input;
        }

        return input.ToTextureImage(_grContext) ?? input;
    }

    public SKImage Snapshot(SKSurface surface, bool preferRasterOutput = false)
    {
        var image = surface.Snapshot();
        if (!preferRasterOutput || !image.IsTextureBacked)
        {
            return image;
        }

        _grContext?.Flush(submit: true, synchronous: true);
        var rasterImage = image.ToRasterImage();
        if (rasterImage is null)
        {
            return image;
        }

        image.Dispose();
        return rasterImage;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _grContext?.Dispose();
        _queue?.Dispose();
        _device?.Dispose();
        _adapter?.Dispose();
        _factory?.Dispose();
    }

    private static IDXGIAdapter1? SelectAdapter(IDXGIFactory6 factory)
    {
        for (int adapterIndex = 0; factory.EnumAdapterByGpuPreference(adapterIndex, GpuPreference.HighPerformance, out IDXGIAdapter1? adapter).Success; adapterIndex++)
        {
            if (adapter is null)
            {
                continue;
            }

            var description = adapter.Description1;
            if ((description.Flags & AdapterFlags.Software) != 0)
            {
                adapter.Dispose();
                continue;
            }

            if (D3D12CreateDevice(adapter, FeatureLevel.Level_11_0, out ID3D12Device2? device).Success)
            {
                device?.Dispose();
                return adapter;
            }

            adapter.Dispose();
        }

        return null;
    }
}

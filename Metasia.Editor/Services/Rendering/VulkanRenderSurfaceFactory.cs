using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Metasia.Core.Render;
using SharpVk;
using SkiaSharp;
using InteropNativeLibrary = SharpVk.Interop.NativeLibrary;

namespace Metasia.Editor.Services.Rendering;

public sealed class VulkanRenderSurfaceFactory : IRenderSurfaceFactory
{
    private readonly Instance? _instance;
    private readonly PhysicalDevice? _physicalDevice;
    private readonly Device? _device;
    private readonly Queue? _queue;
    private readonly InteropNativeLibrary? _nativeLibrary;
    private readonly CommandCache? _commandCache;
    private readonly GRSharpVkBackendContext? _backendContext;
    private readonly GRContext? _grContext;
    private bool _disposed;

    public bool IsGpuAvailable => _grContext is not null;

    public VulkanRenderSurfaceFactory()
    {
        try
        {
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsWindows())
            {
                Debug.WriteLine("VulkanRenderSurfaceFactory: unsupported OS.");
                return;
            }

            if (!TryLoadVulkanLoader())
            {
                Debug.WriteLine("VulkanRenderSurfaceFactory: Vulkan loader was not found.");
                return;
            }

            _nativeLibrary = new InteropNativeLibrary();
            _commandCache = new CommandCache(_nativeLibrary);
            _instance = CreateInstance(_commandCache);

            (PhysicalDevice? physicalDevice, uint graphicsQueueFamilyIndex) = SelectPhysicalDevice(_instance);
            if (physicalDevice is null)
            {
                Debug.WriteLine("VulkanRenderSurfaceFactory: no graphics-capable Vulkan physical device was found.");
                return;
            }

            _physicalDevice = physicalDevice;
            PhysicalDeviceProperties deviceProperties = _physicalDevice.GetProperties();
            _device = CreateDevice(_physicalDevice, graphicsQueueFamilyIndex);
            _queue = _device.GetQueue(graphicsQueueFamilyIndex, 0);

            GRSharpVkGetProcedureAddressDelegate getProcedureAddress = GetProcedureAddress;
            _backendContext = new GRSharpVkBackendContext
            {
                VkInstance = _instance,
                VkPhysicalDevice = _physicalDevice,
                VkDevice = _device,
                VkQueue = _queue,
                GraphicsQueueIndex = graphicsQueueFamilyIndex,
                MaxAPIVersion = (uint)deviceProperties.ApiVersion,
                GetProcedureAddress = getProcedureAddress,
                ProtectedContext = false
            };

            _grContext = GRContext.CreateVulkan(_backendContext);
            if (_grContext is null)
            {
                Debug.WriteLine("VulkanRenderSurfaceFactory: GRContext.CreateVulkan returned null.");
                Dispose();
            }
            else
            {
                Debug.WriteLine($"VulkanRenderSurfaceFactory: initialized Vulkan on '{deviceProperties.DeviceName}' (API {deviceProperties.ApiVersion}).");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"VulkanRenderSurfaceFactory initialization failed: {ex.Message}");
            Dispose();
        }
    }

    public SKSurface CreateSurface(SKImageInfo info, SKSizeI? viewportSize = null)
    {
        if (_grContext is not null)
        {
            SKSurface? gpuSurface = SKSurface.Create(_grContext, false, info);
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
        SKImage image = surface.Snapshot();
        if (!preferRasterOutput || !image.IsTextureBacked)
        {
            return image;
        }

        if (_disposed)
        {
            return image;
        }

        try
        {
            _grContext?.Flush(submit: true, synchronous: true);
        }
        catch
        {
            return image;
        }

        try
        {
            SKImage? rasterImage = image.ToRasterImage();
            if (rasterImage is not null)
            {
                image.Dispose();
                return rasterImage;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"VulkanRenderSurfaceFactory.Snapshot: ToRasterImage failed ({ex.Message}), returning texture-backed image.");
        }

        return image;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _grContext?.Dispose();
        _backendContext?.Dispose();
        _device?.Dispose();
        _instance?.Dispose();
    }

    private static Instance CreateInstance(CommandCache commandCache)
    {
        var appInfo = new ApplicationInfo
        {
            ApplicationName = "Metasia",
            ApplicationVersion = new SharpVk.Version(1, 0, 0),
            EngineName = "SkiaSharp",
            EngineVersion = new SharpVk.Version(1, 0, 0),
            ApiVersion = new SharpVk.Version(1, 0, 0)
        };

        return Instance.Create(
            commandCache,
            null,
            null,
            null,
            appInfo,
            null,
            null,
            null);
    }

    private static Device CreateDevice(PhysicalDevice physicalDevice, uint graphicsQueueFamilyIndex)
    {
        var queueCreateInfo = new DeviceQueueCreateInfo
        {
            QueueFamilyIndex = graphicsQueueFamilyIndex,
            QueuePriorities = [1.0f]
        };

        return physicalDevice.CreateDevice(
            new[] { queueCreateInfo },
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    private static (PhysicalDevice? Device, uint GraphicsQueueFamilyIndex) SelectPhysicalDevice(Instance instance)
    {
        PhysicalDevice? bestDevice = null;
        uint bestGraphicsQueueFamilyIndex = 0;
        int bestScore = int.MinValue;

        foreach (PhysicalDevice physicalDevice in instance.EnumeratePhysicalDevices())
        {
            QueueFamilyProperties[] queueFamilies = physicalDevice.GetQueueFamilyProperties();
            uint? graphicsQueueFamilyIndex = FindGraphicsQueueFamilyIndex(queueFamilies);
            if (graphicsQueueFamilyIndex is null)
            {
                continue;
            }

            int score = GetDeviceScore(physicalDevice.GetProperties().DeviceType);
            if (score <= bestScore)
            {
                continue;
            }

            bestDevice = physicalDevice;
            bestGraphicsQueueFamilyIndex = graphicsQueueFamilyIndex.Value;
            bestScore = score;
        }

        return (bestDevice, bestGraphicsQueueFamilyIndex);
    }

    private IntPtr GetProcedureAddress(string name, Instance instance, Device device)
    {
        if (device is not null)
        {
            return device.GetProcedureAddress(name);
        }

        if (instance is not null)
        {
            IntPtr instanceProc = instance.GetProcedureAddress(name);
            if (instanceProc != IntPtr.Zero)
            {
                return instanceProc;
            }
        }

        if (_nativeLibrary is not null)
        {
            return _nativeLibrary.GetProcedureAddress(name);
        }

        return IntPtr.Zero;
    }

    private static uint? FindGraphicsQueueFamilyIndex(QueueFamilyProperties[] queueFamilies)
    {
        for (uint index = 0; index < queueFamilies.Length; index++)
        {
            QueueFamilyProperties queueFamily = queueFamilies[index];
            if (queueFamily.QueueCount > 0 && (queueFamily.QueueFlags & QueueFlags.Graphics) != 0)
            {
                return index;
            }
        }

        return null;
    }

    private static int GetDeviceScore(PhysicalDeviceType deviceType)
    {
        return deviceType switch
        {
            PhysicalDeviceType.DiscreteGpu => 4,
            PhysicalDeviceType.IntegratedGpu => 3,
            PhysicalDeviceType.VirtualGpu => 2,
            PhysicalDeviceType.Cpu => 1,
            _ => 0
        };
    }

    private static bool TryLoadVulkanLoader()
    {
        if (OperatingSystem.IsWindows())
        {
            return System.Runtime.InteropServices.NativeLibrary.TryLoad("vulkan-1", out IntPtr handle) && FreeLibrary(handle);
        }

        if (OperatingSystem.IsLinux())
        {
            if (System.Runtime.InteropServices.NativeLibrary.TryLoad("libvulkan.so.1", out IntPtr sonameHandle))
            {
                return FreeLibrary(sonameHandle);
            }

            return System.Runtime.InteropServices.NativeLibrary.TryLoad("libvulkan.so", out IntPtr genericHandle) && FreeLibrary(genericHandle);
        }

        return false;
    }

    private static bool FreeLibrary(IntPtr handle)
    {
        System.Runtime.InteropServices.NativeLibrary.Free(handle);
        return true;
    }
}

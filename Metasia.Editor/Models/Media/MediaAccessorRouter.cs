using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Metasia.Core.Media;
using Metasia.Editor.Models.Media.StandardInput;
using Metasia.Editor.Services;
using SkiaSharp;

namespace Metasia.Editor.Models.Media;

public class MediaAccessorRouter : IImageFileAccessor, IVideoFileAccessor, IDisposable
{
    public const string StdInputAccessorId = "builtin.stdinput";
    public const string StdInputAccessorDisplayName = "Standard Input";

    private readonly List<RegisteredAccessor> _registeredAccessors = [];
    private readonly List<RegisteredAccessor> _orderedAccessors = [];
    private readonly List<string> _lastPriorityOrder = [];
    private readonly ISettingsService _settingsService;
    private bool _disposed;

    public IReadOnlyList<IMediaAccessor> Accessors => _orderedAccessors.Select(x => x.Accessor).ToList();

    public MediaAccessorRouter(ISettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(settingsService);

        _settingsService = settingsService;

        RegisterAccessor(StdInputAccessorId, StdInputAccessorDisplayName, new StdInput());
        ApplyPriorityOrder(settingsService.CurrentSettings.Editor.MediaAccessorPriorityOrder);
        settingsService.SettingsChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        ApplyPriorityOrder(_settingsService.CurrentSettings.Editor.MediaAccessorPriorityOrder);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _settingsService.SettingsChanged -= OnSettingsChanged;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async Task<ImageFileAccessorResult> GetImageAsync(string path)
    {
        foreach (var entry in _orderedAccessors)
        {
            if (entry.Accessor is IImageFileAccessor imageAccessor)
            {
                var result = await imageAccessor.GetImageAsync(path);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new ImageFileAccessorResult { IsSuccessful = false, Image = null };
    }

    public async Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
    {
        foreach (var entry in _orderedAccessors)
        {
            if (entry.Accessor is IVideoFileAccessor videoAccessor)
            {
                var result = await videoAccessor.GetImageAsync(path, time);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Image = null };
    }

    public async Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
    {
        foreach (var entry in _orderedAccessors)
        {
            if (entry.Accessor is IVideoFileAccessor videoAccessor)
            {
                var result = await videoAccessor.GetImageAsync(path, frame);
                if (result.IsSuccessful)
                {
                    return result;
                }
            }
        }
        return new VideoFileAccessorResult { IsSuccessful = false, Image = null };
    }

    public void RegisterAccessor(string id, string displayName, IMediaAccessor accessor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(accessor);

        var existingIndex = _registeredAccessors.FindIndex(x => string.Equals(x.Id, id, StringComparison.Ordinal));
        if (existingIndex >= 0)
        {
            _registeredAccessors[existingIndex] = new RegisteredAccessor(id, displayName, accessor);
        }
        else
        {
            _registeredAccessors.Add(new RegisteredAccessor(id, displayName, accessor));
        }

        RebuildOrderedAccessors();
    }

    public void ApplyPriorityOrder(IReadOnlyList<string>? orderedIds)
    {
        _lastPriorityOrder.Clear();
        if (orderedIds is not null)
        {
            _lastPriorityOrder.AddRange(orderedIds);
        }

        RebuildOrderedAccessors();
    }

    public IReadOnlyList<MediaAccessorInfo> GetRegisteredAccessorInfos()
    {
        return new ReadOnlyCollection<MediaAccessorInfo>(
            _registeredAccessors.Select(x => new MediaAccessorInfo(x.Id, x.DisplayName)).ToList());
    }

    private void RebuildOrderedAccessors()
    {
        _orderedAccessors.Clear();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var byId = _registeredAccessors.ToDictionary(x => x.Id, x => x, StringComparer.Ordinal);

        foreach (var id in _lastPriorityOrder)
        {
            if (!seen.Add(id))
            {
                continue;
            }

            if (byId.TryGetValue(id, out var accessor))
            {
                _orderedAccessors.Add(accessor);
            }
        }

        foreach (var accessor in _registeredAccessors)
        {
            if (seen.Add(accessor.Id))
            {
                _orderedAccessors.Add(accessor);
            }
        }
    }

    private sealed record RegisteredAccessor(string Id, string DisplayName, IMediaAccessor Accessor);
}

public sealed record MediaAccessorInfo(string Id, string DisplayName);

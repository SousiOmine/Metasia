using System;

namespace Metasia.Editor.Plugin;

public sealed class PluginTypeInfo
{
    public string TypeId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string PluginName { get; init; } = string.Empty;
    public string PluginIdentifier { get; init; } = string.Empty;
    public Type Type { get; init; } = null!;
}

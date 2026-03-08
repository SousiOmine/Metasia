using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia;
using Metasia.Core.Attributes;

namespace Metasia.Editor.Models;

public static partial class DisplayTextResolver
{
    public static string ResolveClipDisplayName(Type clipType)
    {
        ArgumentNullException.ThrowIfNull(clipType);
        var attribute = clipType.GetCustomAttribute<ClipTypeIdentifierAttribute>();
        return Resolve(attribute?.DisplayKey, attribute?.FallbackText, attribute?.Identifier, clipType.Name);
    }

    public static string ResolveVisualEffectDisplayName(Type effectType)
    {
        ArgumentNullException.ThrowIfNull(effectType);
        var attribute = effectType.GetCustomAttribute<VisualEffectIdentifierAttribute>();
        return Resolve(attribute?.DisplayKey, attribute?.FallbackText, attribute?.Identifier, effectType.Name);
    }

    public static string ResolveAudioEffectDisplayName(Type effectType)
    {
        ArgumentNullException.ThrowIfNull(effectType);
        var attribute = effectType.GetCustomAttribute<AudioEffectIdentifierAttribute>();
        return Resolve(attribute?.DisplayKey, attribute?.FallbackText, attribute?.Identifier, effectType.Name);
    }

    public static string ResolvePropertyDisplayName(ObjectPropertyFinder.EditablePropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        return Resolve(propertyInfo.DisplayKey, propertyInfo.FallbackText, propertyInfo.Identifier, propertyInfo.Identifier);
    }

    public static string Resolve(string? displayKey, string? fallbackText, string? identifier, string typeFallback)
    {
        if (!string.IsNullOrWhiteSpace(displayKey) && TryGetStringResource(displayKey!, out var localized))
        {
            return localized;
        }

        if (!string.IsNullOrWhiteSpace(fallbackText))
        {
            return fallbackText!;
        }

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            return Humanize(identifier!);
        }

        return Humanize(typeFallback);
    }

    private static bool TryGetStringResource(string key, out string value)
    {
        value = string.Empty;

        var application = Application.Current;
        if (application?.TryGetResource(key, application.ActualThemeVariant, out var resource) != true)
        {
            return false;
        }

        if (resource is not string text || string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        value = text;
        return true;
    }

    private static string Humanize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var replaced = value.Replace('.', ' ').Replace('_', ' ').Trim();
        return HumanizeRegex().Replace(replaced, "$1 $2");
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex HumanizeRegex();
}

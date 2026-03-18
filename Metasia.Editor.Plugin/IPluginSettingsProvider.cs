using Avalonia.Controls;

namespace Metasia.Editor.Plugin;

public interface IPluginSettingsProvider
{
    string SettingsDisplayName { get; }
    Window CreateSettingsWindow();
}
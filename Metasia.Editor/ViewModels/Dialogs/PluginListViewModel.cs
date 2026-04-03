using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Collections.ObjectModel;
using Metasia.Editor.Services.PluginService;

namespace Metasia.Editor.ViewModels.Dialogs;

public class PluginListViewModel : ViewModelBase
{
    public ObservableCollection<PluginInfo> Plugins { get; } = [];

    public PluginListViewModel(IPluginService pluginService)
    {
        foreach (var plugin in pluginService.EditorPlugins)
        {
            Plugins.Add(new PluginInfo(plugin.PluginName, plugin.PluginVersion, plugin.PluginIdentifier));
        }
    }
}

public class PluginInfo
{
    public string Name { get; }
    public string Version { get; }
    public string Identifier { get; }

    public PluginInfo(string name, string version, string identifier)
    {
        Name = name;
        Version = version;
        Identifier = identifier;
    }
}
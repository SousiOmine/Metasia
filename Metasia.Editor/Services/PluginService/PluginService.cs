using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Plugins;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Services.PluginService
{
    public class PluginService : IPluginService
    {
        public List<IEditorPlugin> EditorPlugins { get; private set; } = [];

        public List<IMediaInputPlugin> MediaInputPlugins { get; private set; } = [];

        public List<IMediaOutputPlugin> MediaOutputPlugins { get; private set; } = [];

        private readonly MediaAccessorRouter _mediaAccessorRouter;

        public PluginService(MediaAccessorRouter mediaAccessorRouter)
        {
            _mediaAccessorRouter = mediaAccessorRouter;
        }

        public async Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync()
        {
            Debug.WriteLine("Loading plugins...");
            try
            {
                EditorPlugins.Clear();
                MediaInputPlugins.Clear();
                MediaOutputPlugins.Clear();

                EditorPlugins = (await PluginLoader.LoadEditorPluginsAsync()).ToList();
                foreach (var plugin in EditorPlugins)
                {
                    plugin.Initialize();
                    if (plugin is IMediaInputPlugin mediaInputPlugin)
                    {
                        MediaInputPlugins.Add(mediaInputPlugin);
                    }
                    if (plugin is IMediaOutputPlugin mediaOutputPlugin)
                    {
                        MediaOutputPlugins.Add(mediaOutputPlugin);
                    }
                }
                RegisterMediaInputPlugins();
                RegisterMediaOutputPlugins();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Debug.WriteLine($"{EditorPlugins.Count} plugins loaded.");
            return EditorPlugins;
        }

        private void RegisterMediaInputPlugins()
        {
            foreach (var plugin in MediaInputPlugins)
            {
                _mediaAccessorRouter.RegisterAccessor(plugin.PluginIdentifier, plugin.PluginName, plugin);
            }
        }

        private void RegisterMediaOutputPlugins()
        {

        }

        public IEnumerable<IPluginSettingsProvider> GetSettingsProviders()
        {
            foreach (var plugin in EditorPlugins)
            {
                if (plugin is IPluginSettingsProvider settingsProvider)
                {
                    yield return settingsProvider;
                }
            }
        }

        public IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels()
        {
            foreach (var plugin in EditorPlugins)
            {
                if (plugin is not ILeftPanePanelProvider panelProvider)
                {
                    continue;
                }

                var safePanels = new List<LeftPanePanelDefinition>();
                try
                {
                    foreach (var panel in panelProvider.GetLeftPanePanels() ?? [])
                    {
                        if (panel is null)
                        {
                            continue;
                        }

                        safePanels.Add(new LeftPanePanelDefinition(
                            $"{plugin.PluginIdentifier}:{panel.Id}",
                            panel.Title,
                            WrapCreateView(plugin, panel),
                            panel.Tooltip,
                            panel.Icon));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to get left pane panels from {plugin.PluginIdentifier}: {ex}");
                }

                foreach (var panel in safePanels)
                {
                    yield return panel;
                }
            }
        }

        private static Func<Control> WrapCreateView(IEditorPlugin plugin, LeftPanePanelDefinition panel)
        {
            return () =>
            {
                try
                {
                    return panel.CreateView();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to create left pane view {panel.Id} from {plugin.PluginIdentifier}: {ex}");
                    throw;
                }
            };
        }
    }
}

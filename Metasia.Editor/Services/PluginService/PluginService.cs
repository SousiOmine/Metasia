using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Media.StandardInput;
using Metasia.Editor.Models.Plugins;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Services.PluginService
{
    public class PluginService : IPluginService
    {
        public List<IEditorPlugin> EditorPlugins { get; private set; } = [];

        public List<IMediaInputPlugin> MediaInputPlugins { get; private set; } = [];

        public List<IMediaOutputPlugin> MediaOutputPlugins { get; private set; } = [];

        private MediaAccessorRouter _mediaAccessorRouter;

        public PluginService(MediaAccessorRouter mediaAccessorRouter)
        {
            _mediaAccessorRouter = mediaAccessorRouter;
        }

        public async Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync()
        {
            Debug.WriteLine("Loading plugins...");
            try
            {
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
                _mediaAccessorRouter.Accessors.Add(plugin);
            }
            _mediaAccessorRouter.Accessors.Add(new StdInput());
        }
        
        private void RegisterMediaOutputPlugins()
        {

        }
    }
}
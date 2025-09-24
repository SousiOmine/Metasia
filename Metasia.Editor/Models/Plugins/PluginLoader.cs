using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Models.Plugins
{
    public static class PluginLoader
    {
        private const string EDITOR_PLUGINS_PATH = "Plugins";

        private static List<IEditorPlugin> editorPlugins = [];
        
        public static async Task<IEnumerable<IEditorPlugin>> LoadEditorPluginsAsync()
        {
            if (!Directory.Exists(EDITOR_PLUGINS_PATH))
            {
                Directory.CreateDirectory(EDITOR_PLUGINS_PATH);
            }
            await LoadPluginsAsync();
            return editorPlugins;
        }

        private static async Task LoadPluginsAsync()
        {
            try{
                if (Directory.Exists(EDITOR_PLUGINS_PATH))
                {
                    var pluginFiles = Directory.GetFiles(EDITOR_PLUGINS_PATH, "*.dll", SearchOption.AllDirectories);
                    foreach (var pluginFile in pluginFiles)
                    {
                        var assembly = Assembly.LoadFrom(pluginFile);
                        foreach (var type in assembly.GetTypes())
                        {
                            if (typeof(IEditorPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                            {
                                if (Activator.CreateInstance(type) is IEditorPlugin pluginInstance)
                                {
                                    editorPlugins.Add(pluginInstance);
                                }
                            }
                        }
                    }
                }
                // TODO: Coreプラグイン実装時にここでCoreプラグインを読み込む
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
    }
}
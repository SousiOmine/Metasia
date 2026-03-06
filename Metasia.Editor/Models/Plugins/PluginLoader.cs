using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Models.Plugins
{
    public static class PluginLoader
    {
        private const string EDITOR_PLUGINS_FOLDER_NAME = "Plugins";

        private static List<IEditorPlugin> editorPlugins = [];
        private static List<PluginLoadContext> loadContexts = [];

        public static async Task<IEnumerable<IEditorPlugin>> LoadEditorPluginsAsync()
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, EDITOR_PLUGINS_FOLDER_NAME)))
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, EDITOR_PLUGINS_FOLDER_NAME));
            }
            await LoadPluginsAsync();
            return editorPlugins;
        }

        private static async Task LoadPluginsAsync()
        {
            if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, EDITOR_PLUGINS_FOLDER_NAME)))
            {
                var pluginFiles = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, EDITOR_PLUGINS_FOLDER_NAME), "*.dll", SearchOption.AllDirectories);
                foreach (var pluginFile in pluginFiles)
                {
                    try
                    {
                        var loadContext = new PluginLoadContext(pluginFile);
                        loadContexts.Add(loadContext);

                        var assembly = loadContext.LoadFromAssemblyPath(pluginFile);
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
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
            // TODO: Coreプラグイン実装時にここでCoreプラグインを読み込む
            await Task.CompletedTask;
        }
    }
}
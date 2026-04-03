using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
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
        private static List<IEditorPlugin> editorPlugins = [];
        private static List<PluginLoadContext> loadContexts = [];
        private static readonly HashSet<string> loadedPluginPaths = new(StringComparer.OrdinalIgnoreCase);

        public static async Task<IEnumerable<IEditorPlugin>> LoadEditorPluginsAsync()
        {
            MetasiaPaths.EnsureDirectoriesExist();

            editorPlugins.Clear();
            loadContexts.Clear();
            loadedPluginPaths.Clear();

            var pluginDirectories = new[]
            {
                MetasiaPaths.AppBundledPluginsDirectory,
                MetasiaPaths.UserPluginsDirectory
            };

            foreach (var pluginDirectory in pluginDirectories)
            {
                await LoadPluginsFromDirectoryAsync(pluginDirectory);
            }

            return editorPlugins;
        }

        private static async Task LoadPluginsFromDirectoryAsync(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            var pluginFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);
            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    var fileName = Path.GetFileName(pluginFile);
                    if (!loadedPluginPaths.Contains(fileName))
                    {
                        loadedPluginPaths.Add(fileName);
                        LoadPluginFromFile(pluginFile);
                    }
                    else
                    {
                        Debug.WriteLine($"Plugin already loaded, skipping: {fileName}");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to load plugin {pluginFile}: {e.Message}");
                }
            }

            await Task.CompletedTask;
        }

        private static void LoadPluginFromFile(string pluginFile)
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
    }
}
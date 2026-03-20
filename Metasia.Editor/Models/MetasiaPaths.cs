using System;
using System.IO;

namespace Metasia.Editor.Models
{
    public static class MetasiaPaths
    {
        private const string APP_DATA_FOLDER_NAME = "Metasia";
        private const string PLUGINS_FOLDER_NAME = "Plugins";

        private static readonly Lazy<string> _appDataDirectory = new(() =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, APP_DATA_FOLDER_NAME);
        });

        public static string AppDataDirectory => _appDataDirectory.Value;

        public static string UserPluginsDirectory => Path.Combine(AppDataDirectory, PLUGINS_FOLDER_NAME);

        public static string AppBundledPluginsDirectory => Path.Combine(AppContext.BaseDirectory, PLUGINS_FOLDER_NAME);

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(AppDataDirectory);
            Directory.CreateDirectory(UserPluginsDirectory);
        }
    }
}
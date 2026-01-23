using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Metasia.Editor.Models.Settings;

namespace Metasia.Editor.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly string _settingsDirectory;
        private readonly object _lock = new();

        public EditorSettings CurrentSettings { get; private set; } = new();
        public event Action? SettingsChanged;

        private const string SETTINGS_FILE_NAME = "settings.json";

        public SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsDirectory = Path.Combine(appData, "Metasia");
            Directory.CreateDirectory(_settingsDirectory);
            _settingsFilePath = Path.Combine(_settingsDirectory, SETTINGS_FILE_NAME);
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (File.Exists(_settingsFilePath))
                    {
                        try
                        {
                            var json = File.ReadAllText(_settingsFilePath);
                            var settings = JsonSerializer.Deserialize<EditorSettings>(json);
                            if (settings is not null)
                            {
                                CurrentSettings = settings;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"設定ファイルの読み込みエラー: {ex.Message}");
                        }
                    }
                }
            });
        }

        public async Task SaveAsync()
        {
            bool success = false;
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        Directory.CreateDirectory(_settingsDirectory);
                        var json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        File.WriteAllText(_settingsFilePath, json);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"設定ファイルの保存エラー: {ex.Message}");
                    }
                }
            });

            if (success)
            {
                SettingsChanged?.Invoke();
            }
        }

        public async Task UpdateSettingsAsync(EditorSettings settings)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    CurrentSettings = settings;
                    Directory.CreateDirectory(_settingsDirectory);
                    var json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    File.WriteAllText(_settingsFilePath, json);
                }
            });

            SettingsChanged?.Invoke();
        }
    }
}
